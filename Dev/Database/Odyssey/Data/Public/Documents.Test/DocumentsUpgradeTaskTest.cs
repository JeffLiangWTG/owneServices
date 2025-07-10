using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DocumentsUpgradeTaskTest : TransactionedTestCase
	{
		public void TestColumnsToIgnore_IsRunningForSetup()
		{
			var task = new DocumentsUpgradeTaskForTest();

			task.ExposedIsRunningForSetup = false;
			Assert("Columns To Ignore count > 0", task.ExposedColumnsToIgnore.Count > 0);

			task.ExposedIsRunningForSetup = true;
			AssertEquals("Columns To Ignore count", 0, task.ExposedColumnsToIgnore.Count);
		}

		public void TestUpdate_SU_IsVisibleOnWeb()
		{
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			var stmMenuMenuPivotTable = task.ResourceFile.DataSet.Tables[StmMenuMenuPivotSchema.Constants.TableName];
			var stmMenuItemTable = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName];

			Assert("At least one row in table", stmMenuMenuPivotTable.Rows.Count > 0);
			var row = stmMenuMenuPivotTable.Rows[0];

			var stmMenuItemInwardsRow = stmMenuItemTable.Rows.Find(row[StmMenuMenuPivotSchema.Constants.SF_SU_Inward]);

			// insert dbo.StmMenuItem first otherwise get FK constraint problems
			var sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}, {6}) VALUES ('{7}', '{8}', '{9}', {10}, {11}, {12})",
				StmMenuItemSchema.Constants.TableName,
				StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_BusinessContext,
				StmMenuItemSchema.Constants.SU_IsPublished,
				StmMenuItemSchema.Constants.SU_IsSystemDefined,
				StmMenuItemSchema.Constants.SU_IsVisibleOnWeb,
				stmMenuItemInwardsRow[StmMenuItemSchema.Constants.PK].ToString(),
				stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_MenuName].ToString(),
				stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_BusinessContext].ToString(),
				(bool)stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_IsPublished] ? "1" : "0",
				(bool)stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_IsSystemDefined] ? "1" : "0",
				"1");
			Db.Connection.ExecuteNonQuery(sqlText);

			task.Run();

			string resultSql = string.Format(@"SELECT SU_IsVisibleOnWeb FROM dbo.StmMenuItem WHERE SU_PK = '{0}'", stmMenuItemInwardsRow[StmMenuItemSchema.Constants.PK].ToString());
			object result = Db.Connection.ExecuteScalar(resultSql);

			AssertEquals("SU_IsVisibleOnWeb should remain deactivated", false, result);
		}

		public void TestUpdateSU_MenuItem_FromDOCToWEB()
		{
			AssertUpdateSU_MenuItem("DOC", "WEB");
		}

		public void TestUpdateSU_MenuItem_FromWEBToDOC()
		{
			AssertUpdateSU_MenuItem("WEB", "DOC");
		}

		void AssertUpdateSU_MenuItem(string fromMenuType, string toMenuType)
		{
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			var stmMenuItemTable = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName];
			Assert("Pre-condition: at least one row in StmMenuItem", stmMenuItemTable.Rows.Count > 0);

			var row = stmMenuItemTable.Rows[0];
			var rowPK = row[StmMenuItemSchema.Constants.PK].ToString();

			var setupInitialDBValueSQL = $"UPDATE dbo.StmMenuItem SET SU_MenuType = '{fromMenuType}' WHERE SU_PK = '{rowPK}'";
			Db.Connection.ExecuteNonQuery(setupInitialDBValueSQL);

			row[StmMenuItemSchema.Constants.SU_MenuType] = toMenuType;

			var menuTypeFromDBSQL = $"SELECT SU_MenuType FROM dbo.StmMenuItem WHERE SU_PK = '{rowPK}'";
			Func<string> getMenuTypeFromDB = () => { return (string)Db.Connection.ExecuteScalar(menuTypeFromDBSQL); };
			AssertEquals($"Pre-condition: SU_MenuType should be set to '{fromMenuType}'", fromMenuType, getMenuTypeFromDB());

			task.Run();

			AssertEquals($"SU_MenuType should have been updated to '{toMenuType}'", toMenuType, getMenuTypeFromDB());
		}

		public void TestKeepSU_DefaultAttachmentTypeWhenUpdate()
		{
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			var stmMenuItemTable = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName];
			Assert("Pre-condition: at least one row in StmMenuItem", stmMenuItemTable.Rows.Count > 0);

			var row = stmMenuItemTable.Rows[0];
			var rowPK = row[StmMenuItemSchema.Constants.PK].ToString();
			var attachmentType = "PDFA";
			var setupInitialDBValueSQL = $"UPDATE dbo.StmMenuItem SET SU_DefaultAttachmentType = '{attachmentType}' WHERE SU_PK = '{rowPK}'";
			Db.Connection.ExecuteNonQuery(setupInitialDBValueSQL);

			var attachmentTypeFromDBSQL = $"SELECT SU_DefaultAttachmentType FROM dbo.StmMenuItem WHERE SU_PK = '{rowPK}'";
			Func<string> getAttachmentTypeFromDB = () => { return (string)Db.Connection.ExecuteScalar(attachmentTypeFromDBSQL); };
			AssertEquals($"Pre-condition: SU_DefaultAttachmentType should be set to '{attachmentType}'", attachmentType, getAttachmentTypeFromDB());

			task.Run();

			AssertEquals($"SU_DefaultAttachmentType should keep setting to '{attachmentType}'", attachmentType, getAttachmentTypeFromDB());
		}

		void RunTestColumnsToIgnore(List<ColumnToIgnoreForTest> columns, bool isFormBuilder)
		{
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			string messageExists = "There should be at least 1 {0} with {1} set to {2}";
			string messageNotExists = "There should be NO {0} with {1} different than {2}";
			string updateSQL = "UPDATE {0} SET {1} = {2} WHERE 1 = 1 {3}";
			string selectEqualsSQL = "SELECT COUNT(*) FROM {0} WHERE {1} = {2}";
			string selectNotEqualSQL = "SELECT COUNT(*) FROM {0} WHERE {1} <> {2} {3}";

			foreach (var column in columns)
			{
				Db.Connection.ExecuteNonQuery(string.Format(updateSQL, column.TableName, column.ColumnName, column.Value1, GetExtraWhereClauseForFormDocument(column, isFormBuilder)));
			}

			task.Run();

			foreach (var column in columns)
			{
				AssertNotEquals(string.Format(messageExists, column.TableName, column.ColumnName, column.Value1),
					0, Db.Connection.ExecuteScalar(string.Format(selectEqualsSQL, column.TableName, column.ColumnName, column.Value1)));
				AssertEquals(string.Format(messageNotExists, column.TableName, column.ColumnName, column.Value1),
					0, Db.Connection.ExecuteScalar(string.Format(selectNotEqualSQL, column.TableName, column.ColumnName, column.Value1, GetExtraWhereClauseForFormDocument(column, isFormBuilder))));

				Db.Connection.ExecuteNonQuery(string.Format(updateSQL, column.TableName, column.ColumnName, column.Value2, GetExtraWhereClauseForFormDocument(column, isFormBuilder)));
			}

			task.Run();

			foreach (var column in columns)
			{
				AssertNotEquals(string.Format(messageExists, column.TableName, column.ColumnName, column.Value2),
					0, Db.Connection.ExecuteScalar(string.Format(selectEqualsSQL, column.TableName, column.ColumnName, column.Value2)));
				if (column.ColumnName != StmMenuItemSchema.SU_IsModifiable.Name)
				{
					AssertEquals(string.Format(messageNotExists, column.TableName, column.ColumnName, column.Value2),
						0, Db.Connection.ExecuteScalar(string.Format(selectNotEqualSQL, column.TableName, column.ColumnName, column.Value2, GetExtraWhereClauseForFormDocument(column, isFormBuilder))));
				}
			}
		}

		public void TestColumnsToIgnore()
		{
			RunTestColumnsToIgnore(ColumnsToIgnoreForTest, false);
		}

		public void TestFormBuilderColumnsToIgnore()
		{
			RunTestColumnsToIgnore(FormBuilderColumnsToIgnoreForTest, true);
		}

		string GetExtraWhereClauseForFormDocument(ColumnToIgnoreForTest column, bool isFormBuilder)
		{
			if (column.ColumnName == StmMenuItemSchema.SU_IsModifiable.Name)
			{ return string.Format(CultureInfo.InvariantCulture, " AND {0} = 1", StmMenuItemSchema.Constants.SU_SupportsVisualisation); }

			if (column.ColumnName == StmMenuItemSchema.SU_PreventAutoDelivery.Name)
			{
				return string.Format(CultureInfo.InvariantCulture, " AND {0} <> 'NCT'", StmMenuItemSchema.Constants.SU_ContactType);
			}

			return column.TableName == StmMenuItemSchema.Constants.TableName && !isFormBuilder ? string.Format(CultureInfo.InvariantCulture, " AND {0} <> '{1}'", StmMenuItemSchema.Constants.SU_MenuType, "FRM") : string.Empty;
		}

		public void TestColumnsToIgnoreContainMenuIndex()
		{
			var task = new DocumentsUpgradeTaskForTest();
			Assert(task.ExposedColumnsToIgnore.Contains(StmMenuItemSchema.SU_MenuIndex.Name));
		}

		public void TestColumnsToIgnoreContainCompanyBranchDepartmentSpecific()
		{
			var task = new DocumentsUpgradeTaskForTest();
			Assert(task.ExposedColumnsToIgnore.Contains(RefDocTypeSchema.RT_IsCompanySpecific.Name));
			Assert(task.ExposedColumnsToIgnore.Contains(RefDocTypeSchema.RT_IsBranchSpecific.Name));
			Assert(task.ExposedColumnsToIgnore.Contains(RefDocTypeSchema.RT_IsDepartmentSpecific.Name));
		}

		public void TestDeliveryInstructionColumnsToIgnore()
		{
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			string pk = Db.Connection.ExecuteScalar("select top 1 SU_PK from dbo.StmMenuItem").ToString();

			string updateSQL = string.Format("UPDATE dbo.StmMenuItem SET SU_DeliveryRestrictionType = 'UDF', SU_DeliveryRestrictionMacro = '<Macro>', SU_DeliveryRestrictionDescription = 'Test' where SU_PK = '{0}'", pk);
			Db.Connection.ExecuteNonQuery(updateSQL);

			task.Run();

			AssertEquals("UDF", Db.Connection.ExecuteScalar(string.Format("select SU_DeliveryRestrictionType from dbo.StmMenuItem where SU_PK = '{0}'", pk)).ToString());
			AssertEquals("<Macro>", Db.Connection.ExecuteScalar(string.Format("select SU_DeliveryRestrictionMacro from dbo.StmMenuItem where SU_PK = '{0}'", pk)).ToString());
			AssertEquals("Test", Db.Connection.ExecuteScalar(string.Format("select SU_DeliveryRestrictionDescription from dbo.StmMenuItem where SU_PK = '{0}'", pk)).ToString());
		}

		public void TestResourceFileTablesMatchExpectedTables()
		{
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			var tableNamesFromFile = new HashSet<string>();
			foreach (DataTable table in task.ResourceFile.DataSet.Tables)
			{
				tableNamesFromFile.Add(table.TableName);
			}
			var expectedTableNames = new HashSet<string>(DocumentsDataFile.DataFileTables);

			Assert("Resource file tables do NOT match expected tables", tableNamesFromFile.SetEquals(expectedTableNames));
		}

		public void TestMixOfUpdateInsertAndDeletes()
		{
			const string PreAlertTemplatePK = "1F8D57CC-ED7A-433B-A9A4-2B162BFBAF8D";

			string insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'UserTemplate1', 'Consol', null, 0, 0, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'User Doc', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', 'User Menu Sys Template', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle) VALUES('4E7C38C2-EB57-4AC7-9497-EE66AD1CF66E', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'User Doc UserTemplate1');
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle) VALUES('A93DD396-D92C-4662-9805-4C05CB0A5E62', 'DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', '" + PreAlertTemplatePK + @"', 'User Doc SysTemplate');

				INSERT dbo.RefDocType (RT_PK, RT_DocType, RT_ReferenceType, RT_IsSystem, RT_IsActive) VALUES ('2596006D-717C-4BE1-83FF-C44B1DA6420E', 'ABC', 'CON', 0, 1);

				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined, SX_IsClientSupressed) VALUES (newid(), '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '2596006D-717C-4BE1-83FF-C44B1DA6420E', 0, 0);
			";

			Db.Connection.ExecuteNonQuery(insertSql);

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem", "StmMenuEDocs", "RefDocType");

			AssertEquals("Table Count", 5, data.Tables.Count);

			DataRow[] pivots = data.Tables["StmMenuTemplatePivot"].Select("SI_DocumentTitle = 'User Doc UserTemplate1'");
			AssertEquals("User Pivots", 1, pivots.Length);

			DataRow[] templates = data.Tables["StmTemplate"].Select("SO_Name = 'UserTemplate1'");
			AssertEquals("User Templates", 1, templates.Length);

			DataRow[] menus0 = data.Tables["StmMenuItem"].Select("SU_MenuName = 'User Doc'");
			AssertEquals("User Menus 0 ", 1, menus0.Length);

			DataRow[] menus1 = data.Tables["StmMenuItem"].Select("SU_MenuName = 'User Menu Sys Template'");
			AssertEquals("User Menus 1 ", 1, menus1.Length);

			DataRow[] docType = data.Tables["RefDocType"].Select("RT_DocType = 'ABC'");
			AssertEquals("UserDefined DocType is still available after the upgrade", 1, docType.Length);
			AssertEquals("UserDefined DocType is still available after the upgrade", "CON", docType[0]["RT_ReferenceType"]);

			DataRow[] menuEDocs = data.Tables["StmMenuEDocs"].Select("SX_RT_DocType = '2596006D-717C-4BE1-83FF-C44B1DA6420E'");
			AssertEquals("UserDefined MenuEDocs is still available after the upgrade", 1, menuEDocs.Length);
			AssertEquals("UserDefined MenuEDocs is still available after the upgrade", new Guid("3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0"), menuEDocs[0]["SX_SU"]);
		}

		public void TestDeleteMenuItemChildReferences()
		{
			string insertSql = @"
				DECLARE @PrinterPK uniqueidentifier = NEWID();
				DECLARE @ServerPK uniqueidentifier = NEWID();
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('1B934998-649C-48F5-B452-89D885C85021', 'User Doc', 'Consol', '', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.OrgDocument (OD_PK, OD_SU_MenuItem, OD_OC) SELECT TOP 1 NEWID(), '1B934998-649C-48F5-B452-89D885C85021', OC_PK FROM dbo.OrgContact
				INSERT dbo.ProcessTaskNotification (PQ_PK, PQ_SU_Document, PQ_P9) SELECT TOP 1 NEWID(), '1B934998-649C-48F5-B452-89D885C85021', P9_PK FROM dbo.ProcessTasks
				INSERT dbo.AccComplianceSequence (XD_PK, XD_SU_MenuItem, XD_GC_Company) SELECT TOP 1 NEWID(), '1B934998-649C-48F5-B452-89D885C85021', GC_PK FROM dbo.GlbCompany
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPK, 'SERVER')
				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName) VALUES (@PrinterPK, @ServerPK, 'PRINTER')
				INSERT dbo.StmDefaultPrinter (SDP_PK, SDP_SubjectID, SDP_SubjectTableCode, SDP_SQ_Printer, SDP_SU_Document) SELECT TOP 1 NEWID(), GS_PK, 'GS', @PrinterPK, '1B934998-649C-48F5-B452-89D885C85021' FROM dbo.GlbStaff
				INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_BinaryValue) VALUES (NEWID(), 'RPT_CFG$DAU$LPTest','1B934998-649C-48F5-B452-89D885C85021', convert(varbinary(max), N'{1}'))";
			Db.Connection.ExecuteNonQuery(insertSql);

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();
			string selectSql = @"SELECT count(*) FROM {0} WHERE {1} = '1B934998-649C-48F5-B452-89D885C85021'";
			string sqlText;
			foreach (var reference in DocumentsUpgradeTask.ExternalMenuReferences[StmMenuItemSchema.Constants.TableName])
			{
				sqlText = string.Format(selectSql, reference.Schema.TableName, reference.MenuItemColumn);
				int childReferenceCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
				string message = string.Format("Child {0} should have been deleted", reference.Schema.TableName);
				AssertEquals(message, 0, childReferenceCount);
			}

			sqlText = string.Format(selectSql, StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.PK);
			int stmMenuItemCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Menu Item should have been deleted", 0, stmMenuItemCount);
		}

		public void TestDeleteMenuItemRelatedGlbSecurity()
		{
			string insertSql = @"
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('1B934998-649C-48F5-B452-89D885C85021', 'User Doc', 'Consol', '', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.GlbSecurity (GU_PK, GU_IsValid, GU_SecurityItemIsAllowed, GU_ItemGUID) VALUES ('23E2ED9E-06EC-472E-9A6E-3724E3D2A0AE', 1, 1, '1B934998-649C-48F5-B452-89D885C85021')";

			Db.Connection.ExecuteNonQuery(insertSql);

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();
			string selectSql = @"SELECT count(*) FROM {0} WHERE {1} = '1B934998-649C-48F5-B452-89D885C85021'";
			string sqlText;
			sqlText = string.Format(selectSql, StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.PK);
			int stmMenuItemCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Menu Item should have been deleted", 0, stmMenuItemCount);
			sqlText = string.Format(selectSql, GlbSecuritySchema.Constants.TableName, GlbSecuritySchema.Constants.GU_ItemGUID);
			int glbSecurityCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("glbSecurity should have been deleted", 0, glbSecurityCount);
		}

		public void TestDeleteOrgDocumentCopyRecipientWithOrgDocument()
		{
			const string InitialDataSql = @"
INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES ('22222222-3333-4444-5555-666666666666', 'Menu1', 1);
INSERT dbo.OrgDocument (OD_PK, OD_SU_MenuItem, OD_OC) SELECT TOP 1 '12345678-1111-2222-3333-1234567890AB', '22222222-3333-4444-5555-666666666666', OC_PK FROM dbo.OrgContact;
INSERT dbo.OrgDocumentCopyRecipient (ODR_PK, ODR_OD, ODR_RecipientType, ODR_EmailAddress) VALUES (newid(), '12345678-1111-2222-3333-1234567890AB', 'CC', 'aaa@bbb.ccc');
";
			Db.Connection.ExecuteNonQuery(InitialDataSql);

			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.StmMenuItem WHERE SU_PK = '22222222-3333-4444-5555-666666666666'"));
			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgDocument WHERE OD_SU_MenuItem = '22222222-3333-4444-5555-666666666666'"));
			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgDocumentCopyRecipient WHERE ODR_OD = '12345678-1111-2222-3333-1234567890AB'"));
		}

		public void TestDeleteOrgSecurityContactsWithOrgSecurity()
		{
			const string InitialDataSql = @"
INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES ('22222222-3333-4444-5555-666666666666', 'Menu1', 1);
INSERT dbo.OrgSecurity (OX_PK, OX_Granted, OX_SecurityItemName, OX_OH, OX_SU) SELECT TOP 1 '12345678-1111-2222-3333-1234567890AB', 1, '', OH_PK, '22222222-3333-4444-5555-666666666666' FROM dbo.OrgHeader;
INSERT dbo.OrgSecurityContacts (OZ_PK, OZ_Granted, OZ_OC, OZ_OX) SELECT TOP 1 newid(), 0, OC_PK, '12345678-1111-2222-3333-1234567890AB' FROM dbo.OrgContact;
";
			Db.Connection.ExecuteNonQuery(InitialDataSql);

			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.StmMenuItem WHERE SU_PK = '22222222-3333-4444-5555-666666666666'"));
			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgSecurity WHERE OX_SU = '22222222-3333-4444-5555-666666666666'"));
			AssertEquals(0, Db.Connection.ExecuteScalar("SELECT count(*) FROM dbo.OrgSecurityContacts WHERE OZ_OX = '12345678-1111-2222-3333-1234567890AB'"));
		}

		public void TestDeleteChildReferences()
		{
			var sb = new StringBuilder();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.DeleteChildReferences(StmMenuItemSchema.Constants.TableName, "= @Abc", sb);

			const string expectedSql = @"
DELETE dbo.OrgDocumentCopyRecipient WHERE ODR_OD IN (SELECT OD_PK FROM dbo.OrgDocument WHERE OD_SU_MenuItem = @Abc);
DELETE dbo.OrgDocument WHERE OD_SU_MenuItem = @Abc;
DELETE dbo.ProcessTaskNotification WHERE PQ_SU_Document = @Abc;
DELETE dbo.AccComplianceSequence WHERE XD_SU_MenuItem = @Abc;
DELETE dbo.StmDefaultPrinter WHERE SDP_SU_Document = @Abc;
DELETE dbo.OrgSecurityContacts WHERE OZ_OX IN (SELECT OX_PK FROM dbo.OrgSecurity WHERE OX_SU = @Abc);
DELETE dbo.OrgSecurity WHERE OX_SU = @Abc;
DELETE dbo.StmDocDataOverride WHERE DD_SU = @Abc;
DELETE dbo.JobDocumentDelivery WHERE JDC_SU_MenuItem = @Abc;
DELETE dbo.StmDocumentDelivery WHERE SDL_SU = @Abc;
";

			AssertEquals(expectedSql.Trim(), sb.ToString().Trim());
		}

		public void TestNoNewFKsToStmMenuItemWereAddedInSchemaWithoutUpdatingExternalSubstanceReferences()
		{
			DataTable fkReferences = UpgradeTaskTestHelper.GetFkReferences(TestConnection, "StmMenuItem", "'StmMenuEDocs', 'StmMenuMenuPivot', 'StmMenuTemplatePivot', 'RateAttachmentSet', 'StmMenuDeliveryRestriction'");

			List<string> expected = new List<string>();
			var references = DocumentsUpgradeTask.ExternalMenuReferences[StmMenuItemSchema.Constants.TableName];
			foreach (var reference in references)
			{
				expected.Add(reference.Schema.TableName + "." + reference.MenuItemColumn);
			}

			List<string> actual = new List<string>();
			foreach (DataRow row in fkReferences.Rows)
			{
				actual.Add((string)row["FkTable"] + "." + (string)row["FkColumn"]);
			}

			expected.Sort();
			actual.Sort();

			AssertMultilineASCIIEquals("ExternalSubstanceReferences should cover all references to StmMenuItem so that changes we make to Document Menus don't cause upgrade failures."
				, string.Join("\r\n", expected.ToArray())
				, string.Join("\r\n", actual.ToArray()));
		}

		public void TestDeleteRateAttachmentSetChildRateAttachments()
		{
			string insertSql = @"
				INSERT dbo.RatingHeader (TH_PK, TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES ('21972EFB-6227-4504-BDF7-EA4945A69C69', 'COS', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_IsSystemDefined, SU_IsClientSpecific) VALUES('39DBAD5C-EBAF-4546-A8D6-116719520974', 'TestMenu1', 'Quotation', 1, 0);
				INSERT dbo.RateAttachmentSet (TS_PK, TS_AttachmentName, TS_Sequence, TS_IsDefault, TS_IsSystemDefined, TS_SU, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser, TS_SystemCreateTimeUtc, TS_SystemCreateUser) VALUES ('FE4CCE27-4CE5-48ea-A5BD-CCB8E7105673', 'test', 1, 1, 1, '39DBAD5C-EBAF-4546-A8D6-116719520974', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.RateAttachment (TA_PK, TA_TS, TA_TH, TA_SystemLastEditTimeUtc, TA_SystemLastEditUser, TA_SystemCreateTimeUtc, TA_SystemCreateUser) VALUES (NEWID(), 'FE4CCE27-4CE5-48ea-A5BD-CCB8E7105673', '21972EFB-6227-4504-BDF7-EA4945A69C69', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (DbCommand command = Db.Connection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			const string sqlText1 = @"SELECT count(*) FROM dbo.RateAttachment WHERE TA_TS = 'FE4CCE27-4CE5-48ea-A5BD-CCB8E7105673'";
			const string sqlText2 = @"SELECT count(*) FROM dbo.RateAttachmentSet WHERE TS_PK = 'FE4CCE27-4CE5-48ea-A5BD-CCB8E7105673'";

			using (DbCommand command = Db.Connection.Command(sqlText1))
			{
				AssertEquals("Attachment should have been deleted", 0, command.ExecuteScalar());
			}

			using (DbCommand command = Db.Connection.Command(sqlText2))
			{
				AssertEquals("Attachment set should have been deleted", 0, command.ExecuteScalar());
			}
		}

		[ExpectNoExceptions()]
		public void TestExistingStmMenuEDocsRowsAreDeletedBeforeUpgrade()
		{
			Guid newGuid = Guid.NewGuid();
			DocumentTablesCleaner.Clean();
			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			DataTable stmMenuEDocsTable = task.ResourceFile.DataSet.Tables[StmMenuEDocsSchema.Constants.TableName];
			DataTable stmMenuItemTable = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName];
			DataTable refDocTypeTable = task.ResourceFile.DataSet.Tables[RefDocTypeSchema.Constants.TableName];

			Assert("At least one row in table", stmMenuEDocsTable.Rows.Count > 0);
			DataRow row = stmMenuEDocsTable.Rows[0];
			DataRow stmMenuItemRow = stmMenuItemTable.Rows.Find(row[StmMenuEDocsSchema.Constants.SX_SU]);
			DataRow refDocTypeRow = refDocTypeTable.Rows.Find(row[StmMenuEDocsSchema.Constants.SX_RT_DocType]);

			// insert dbo.RefDocType first otherwisse get FK constraint problems
			string sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', '{9}', '{10}')",
				RefDocTypeSchema.Constants.TableName,
				RefDocTypeSchema.Constants.PK,
				RefDocTypeSchema.Constants.RT_DocType,
				RefDocTypeSchema.Constants.RT_ReferenceType,
				RefDocTypeSchema.Constants.RT_Desc,
				RefDocTypeSchema.Constants.RT_IsSystem,
				refDocTypeRow[RefDocTypeSchema.Constants.PK].ToString(),
				refDocTypeRow[RefDocTypeSchema.Constants.RT_DocType].ToString(),
				refDocTypeRow[RefDocTypeSchema.Constants.RT_ReferenceType].ToString(),
				refDocTypeRow[RefDocTypeSchema.Constants.RT_Desc].ToString(),
				refDocTypeRow[RefDocTypeSchema.Constants.RT_IsSystem].ToString());
			Db.Connection.ExecuteNonQuery(sqlText);

			// insert dbo.StmMenuItem first otherwise get FK constraint problems
			sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}, {6}) VALUES ('{7}', '{8}', '{9}', {10}, {11}, {12})",
				StmMenuItemSchema.Constants.TableName,
				StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_BusinessContext,
				StmMenuItemSchema.Constants.SU_IsPublished,
				StmMenuItemSchema.Constants.SU_IsSystemDefined,
				StmMenuItemSchema.Constants.SU_IsModifiable,
				stmMenuItemRow[StmMenuItemSchema.Constants.PK].ToString(),
				stmMenuItemRow[StmMenuItemSchema.Constants.SU_MenuName].ToString(),
				stmMenuItemRow[StmMenuItemSchema.Constants.SU_BusinessContext].ToString(),
				(bool)stmMenuItemRow[StmMenuItemSchema.Constants.SU_IsPublished] ? "1" : "0",
				(bool)stmMenuItemRow[StmMenuItemSchema.Constants.SU_IsSystemDefined] ? "1" : "0",
				"0");
			Db.Connection.ExecuteNonQuery(sqlText);

			// insert duplicate StmMenuEDocs
			sqlText = String.Format(
				"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', {9}, {10})",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.PK,
				StmMenuEDocsSchema.Constants.SX_SU,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined,
				StmMenuEDocsSchema.Constants.SX_IsClientSupressed,
				newGuid.ToString(),
				row[StmMenuEDocsSchema.Constants.SX_SU].ToString(),
				row[StmMenuEDocsSchema.Constants.SX_RT_DocType].ToString(),
				"0",
				"0");
			Db.Connection.ExecuteNonQuery(sqlText);

			task.Run();

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.PK);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", newGuid, StmMenuEDocsSchema.PK);
			int rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("User defined row should have been replaced by system defined row - no docs with user defined PK", 0, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @IsSystemDefined AND {2} = @DocTypePK AND {3} = @MenuPK",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_SU);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@IsSystemDefined", true, StmMenuEDocsSchema.SX_IsSystemDefined);
			cmd.AddParameterBasedOnDbColumn("@DocTypePK", row[StmMenuEDocsSchema.Constants.SX_RT_DocType], StmMenuEDocsSchema.SX_RT_DocType);
			cmd.AddParameterBasedOnDbColumn("@MenuPK", row[StmMenuEDocsSchema.Constants.SX_SU], StmMenuEDocsSchema.SX_SU);
			rowCount = (int)cmd.ExecuteScalar();

			AssertEquals("Should be one row in table", 1, rowCount);
		}

		[ExpectNoExceptions()]
		public void TestExistingStmMenuEDocsRowsAreDeletedBeforeUpgradeWhenNotInserting()
		{
			DocumentTablesCleaner.Clean();
			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			DataTable stmMenuEDocsTable = task.ResourceFile.DataSet.Tables[StmMenuEDocsSchema.Constants.TableName];

			Assert("At least one row in table", stmMenuEDocsTable.Rows.Count > 0);
			DataRow row = stmMenuEDocsTable.Rows[0];

			string querySqlText = "select top 1 RT_PK from dbo.RefDocType where RT_PK not in (SELECT SX_RT_DocType from dbo.StmMenuEDocs)";
			DbCommand cmd = Db.Connection.Command(querySqlText);
			Guid spareDocTypePK = (Guid)cmd.ExecuteScalar();
			Assert("Found a spare doc type PK not used by StmMenuEDoc yet", spareDocTypePK != Guid.Empty);

			string sqlText = String.Format(
				"UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				spareDocTypePK.ToString(),
				StmMenuEDocsSchema.Constants.PK,
				row[StmMenuEDocsSchema.Constants.PK].ToString());
			Db.Connection.ExecuteNonQuery(sqlText);

			Guid newGuid = Guid.NewGuid();
			sqlText = String.Format(
				"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', 0, 0)",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.PK,
				StmMenuEDocsSchema.Constants.SX_SU,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined,
				StmMenuEDocsSchema.Constants.SX_IsClientSupressed,
				newGuid.ToString(),
				row[StmMenuEDocsSchema.Constants.SX_SU].ToString(),
				row[StmMenuEDocsSchema.Constants.SX_RT_DocType].ToString());
			Db.Connection.ExecuteNonQuery(sqlText);

			task.Run();

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.PK);

			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", newGuid, StmMenuEDocsSchema.PK);
			int rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("User defined row should have been replaced by system defined row - no docs with user defined PK", 0, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @IsSystemDefined AND {2} = @DocTypePK AND {3} = @MenuPK",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_SU);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@IsSystemDefined", true, StmMenuEDocsSchema.SX_IsSystemDefined);
			cmd.AddParameterBasedOnDbColumn("@DocTypePK", row[StmMenuEDocsSchema.Constants.SX_RT_DocType], StmMenuEDocsSchema.SX_RT_DocType);
			cmd.AddParameterBasedOnDbColumn("@MenuPK", row[StmMenuEDocsSchema.Constants.SX_SU], StmMenuEDocsSchema.SX_SU);
			rowCount = (int)cmd.ExecuteScalar();

			AssertEquals("Should be one row in table", 1, rowCount);
		}

		[ExpectNoExceptions()]
		public void TestExistingUserDefinedStmMenuMenuPivotRowsAreDeletedBeforeUpgrade()
		{
			var newGuid = Guid.NewGuid();
			var newGuid2 = Guid.NewGuid();
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			var stmMenuMenuPivotTable = task.ResourceFile.DataSet.Tables[StmMenuMenuPivotSchema.Constants.TableName];
			var stmMenuItemTable = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName];

			Assert("At least one row in table", stmMenuMenuPivotTable.Rows.Count > 0);
			var row = stmMenuMenuPivotTable.Rows[0];

			var stmMenuItemInwardsRow = stmMenuItemTable.Rows.Find(row[StmMenuMenuPivotSchema.Constants.SF_SU_Inward]);
			var stmMenuItemOutwardsRow = stmMenuItemTable.Rows.Find(row[StmMenuMenuPivotSchema.Constants.SF_SU_Outward]);

			// insert dbo.StmMenuItem first otherwise get FK constraint problems
			var sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', {9}, {10})",
				StmMenuItemSchema.Constants.TableName,
				StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_BusinessContext,
				StmMenuItemSchema.Constants.SU_IsPublished,
				StmMenuItemSchema.Constants.SU_IsSystemDefined,
				stmMenuItemInwardsRow[StmMenuItemSchema.Constants.PK].ToString(),
				stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_MenuName].ToString(),
				stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_BusinessContext].ToString(),
				(bool)stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_IsPublished] ? "1" : "0",
				(bool)stmMenuItemInwardsRow[StmMenuItemSchema.Constants.SU_IsSystemDefined] ? "1" : "0");
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', {9}, {10})",
				StmMenuItemSchema.Constants.TableName,
				StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_BusinessContext,
				StmMenuItemSchema.Constants.SU_IsPublished,
				StmMenuItemSchema.Constants.SU_IsSystemDefined,
				stmMenuItemOutwardsRow[StmMenuItemSchema.Constants.PK].ToString(),
				stmMenuItemOutwardsRow[StmMenuItemSchema.Constants.SU_MenuName].ToString(),
				stmMenuItemOutwardsRow[StmMenuItemSchema.Constants.SU_BusinessContext].ToString(),
				(bool)stmMenuItemOutwardsRow[StmMenuItemSchema.Constants.SU_IsPublished] ? "1" : "0",
				(bool)stmMenuItemOutwardsRow[StmMenuItemSchema.Constants.SU_IsSystemDefined] ? "1" : "0");
			Db.Connection.ExecuteNonQuery(sqlText);

			// insert the duplicate menumenupivot
			sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES ('{8}', 5, 0, 0, '{9}', '{10}', '{11}')",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.PK,
				StmMenuMenuPivotSchema.Constants.SF_Index,
				StmMenuMenuPivotSchema.Constants.SF_IsClientSpecific,
				StmMenuMenuPivotSchema.Constants.SF_IsSystemDefined,
				StmMenuMenuPivotSchema.Constants.SF_SU_Inward,
				StmMenuMenuPivotSchema.Constants.SF_SU_Outward,
				StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext,
				newGuid,
				row[StmMenuMenuPivotSchema.Constants.SF_SU_Inward].ToString(),
				row[StmMenuMenuPivotSchema.Constants.SF_SU_Outward].ToString(),
				row[StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext].ToString());
			Db.Connection.ExecuteNonQuery(sqlText);

			// insert the duplicate menumenupivot with different SF_OverriddenBusinessContext
			sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES ('{8}', 5, 0, 0, '{9}', '{10}', '{11}')",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.PK,
				StmMenuMenuPivotSchema.Constants.SF_Index,
				StmMenuMenuPivotSchema.Constants.SF_IsClientSpecific,
				StmMenuMenuPivotSchema.Constants.SF_IsSystemDefined,
				StmMenuMenuPivotSchema.Constants.SF_SU_Inward,
				StmMenuMenuPivotSchema.Constants.SF_SU_Outward,
				StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext,
				newGuid2,
				row[StmMenuMenuPivotSchema.Constants.SF_SU_Inward].ToString(),
				row[StmMenuMenuPivotSchema.Constants.SF_SU_Outward].ToString(),
				row[StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext].ToString() + "different");
			Db.Connection.ExecuteNonQuery(sqlText);

			task.Run();

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.PK);
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", newGuid, StmMenuMenuPivotSchema.PK);
			var rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("User defined row should have been replaced by system defined row - no docs with user defined PK", 0, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @IsSystemDefined AND {2} = @Inwards AND {3} = @Outwards AND {4} = @OverriddenBusinessContext",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.SF_IsSystemDefined,
				StmMenuMenuPivotSchema.Constants.SF_SU_Inward,
				StmMenuMenuPivotSchema.Constants.SF_SU_Outward,
				StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@IsSystemDefined", true, StmMenuMenuPivotSchema.SF_IsSystemDefined);
			cmd.AddParameterBasedOnDbColumn("@Inwards", row[StmMenuMenuPivotSchema.Constants.SF_SU_Inward], StmMenuMenuPivotSchema.SF_SU_Inward);
			cmd.AddParameterBasedOnDbColumn("@Outwards", row[StmMenuMenuPivotSchema.Constants.SF_SU_Outward], StmMenuMenuPivotSchema.SF_SU_Outward);
			cmd.AddParameterBasedOnDbColumn("@OverriddenBusinessContext", row[StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext], StmMenuMenuPivotSchema.SF_OverriddenBusinessContext);
			rowCount = (int)cmd.ExecuteScalar();

			AssertEquals("Should be one row in table", 1, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.PK);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", newGuid2, StmMenuMenuPivotSchema.PK);
			rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("User defined row should not be replaced by system defined row", 1, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @IsSystemDefined AND {2} = @Inwards AND {3} = @Outwards AND {4} = @OverriddenBusinessContext",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.SF_IsSystemDefined,
				StmMenuMenuPivotSchema.Constants.SF_SU_Inward,
				StmMenuMenuPivotSchema.Constants.SF_SU_Outward,
				StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@IsSystemDefined", true, StmMenuMenuPivotSchema.SF_IsSystemDefined);
			cmd.AddParameterBasedOnDbColumn("@Inwards", row[StmMenuMenuPivotSchema.Constants.SF_SU_Inward], StmMenuMenuPivotSchema.SF_SU_Inward);
			cmd.AddParameterBasedOnDbColumn("@Outwards", row[StmMenuMenuPivotSchema.Constants.SF_SU_Outward], StmMenuMenuPivotSchema.SF_SU_Outward);
			cmd.AddParameterBasedOnDbColumn("@OverriddenBusinessContext", row[StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext] + "different", StmMenuMenuPivotSchema.SF_OverriddenBusinessContext);
			rowCount = (int)cmd.ExecuteScalar();

			AssertEquals("System defined row should not be in table", 0, rowCount);
		}

		public void TestDeleteUnmatchingSystemRowsAndIgnoreUserRows()
		{
			// Inserts 3 DocTypes - 2 system types and 1 user (non-system) type
			string insertSql = @"
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('D3FF0168-0883-4D59-AB18-28CCB15BE9EF', '111', 'SYS', 'System Type', 1, 1);
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('2FA94C77-47A3-452E-BA98-E87E14A4FC6F', '222', 'SYS', 'System Type', 1, 1);
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('1E607E4A-8E5A-4DCE-8D2A-114F2F3F793C', '333', 'USR', 'User Type'  , 1, 0);";
			Db.Connection.ExecuteNonQuery(insertSql);

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			// Doesn't use a RefDocTypeDataFile as it doesn't load non-system RefDocTypes
			DataFile tempFile = new EmbeddedDataFile("", "RefDocType");
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);

			DataRow[] unmatchingSystemRows = data.Tables["RefDocType"].Select("RT_DocType = 'SYS'");
			AssertEquals("Unmatching System Types should have been deleted", 0, unmatchingSystemRows.Length);

			DataRow[] userTypes = data.Tables["RefDocType"].Select("RT_DocType = 'USR'");
			AssertEquals("User Type should NOT have been deleted", 1, userTypes.Length);
		}

		public void TestDeleteRefDocTypeWillDeleteStmMenuEDocsReferencesAsWell()
		{
			// insert dbo.RefDocType that will be deleted when the upgrade task is run
			string sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', '{9}', {10})",
				RefDocTypeSchema.Constants.TableName,
				RefDocTypeSchema.Constants.PK,
				RefDocTypeSchema.Constants.RT_DocType,
				RefDocTypeSchema.Constants.RT_ReferenceType,
				RefDocTypeSchema.Constants.RT_Desc,
				RefDocTypeSchema.Constants.RT_IsSystem,
				"2FA94C77-47A3-452E-BA98-E87E14A4FC6F",
				"ABC",
				"SHP",
				"This is a test doc type",
				1);
			Db.Connection.ExecuteNonQuery(sqlText);

			// insert user StmMenuItem
			sqlText = String.Format(
				"INSERT {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', {9}, {10})",
				StmMenuItemSchema.Constants.TableName,
				StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_BusinessContext,
				StmMenuItemSchema.Constants.SU_IsPublished,
				StmMenuItemSchema.Constants.SU_IsSystemDefined,
				"1E607E4A-8E5A-4DCE-8D2A-114F2F3F793C",
				"TestMenuName",
				"Shipment",
				1,
				0);
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = String.Format(
				"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) VALUES ('{6}', '{7}', '{8}', {9}, {10})",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.PK,
				StmMenuEDocsSchema.Constants.SX_SU,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined,
				StmMenuEDocsSchema.Constants.SX_IsClientSupressed,
				"D45DA2FD-5104-43A5-A979-58A1F2E37F69",
				"1E607E4A-8E5A-4DCE-8D2A-114F2F3F793C",
				"2FA94C77-47A3-452E-BA98-E87E14A4FC6F",
				0,
				0);
			Db.Connection.ExecuteNonQuery(sqlText);

			DocumentsUpgradeTask upgradeTask = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			upgradeTask.Run();

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.PK);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", new Guid("D45DA2FD-5104-43A5-A979-58A1F2E37F69"), StmMenuEDocsSchema.PK);
			int rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("Count should be 0, this StmMenuEDocs row should have been deleted when the upgrade ran", 0, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				RefDocTypeSchema.Constants.TableName,
				RefDocTypeSchema.Constants.PK);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", new Guid("2FA94C77-47A3-452E-BA98-E87E14A4FC6F"), RefDocTypeSchema.PK);
			rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("Count should be 0, this RefDocType row should have been deleted when the upgrade ran", 0, rowCount);
		}

		public void TestDeleteRefDocTypeWillDeleteMenuTemplatePivotReferencesAsWell()
		{
			const string PreAlertTemplatePK = "1F8D57CC-ED7A-433B-A9A4-2B162BFBAF8D";
			string insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'UserTemplate1', 'Consol', null, 0, 0, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'User Doc', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', 'User Menu Sys Template', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle) VALUES('4E7C38C2-EB57-4AC7-9497-EE66AD1CF66E', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'User Doc UserTemplate1');
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle) VALUES('A93DD396-D92C-4662-9805-4C05CB0A5E62', 'DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', '" + PreAlertTemplatePK + @"', 'User Doc SysTemplate');

				INSERT dbo.RefDocType (RT_PK, RT_DocType, RT_ReferenceType, RT_IsSystem, RT_IsActive) VALUES ('2596006D-717C-4BE1-83FF-C44B1DA6420E', 'ABC', 'CON', 1, 1);

				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined, SX_IsClientSupressed) VALUES ('D45DA2FD-5104-43A5-A979-58A1F2E37F69', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '2596006D-717C-4BE1-83FF-C44B1DA6420E', 0, 0);
			";

			Db.Connection.ExecuteNonQuery(insertSql);

			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				RefDocTypeSchema.Constants.TableName,
				RefDocTypeSchema.Constants.PK);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", new Guid("2596006D-717C-4BE1-83FF-C44B1DA6420E"), RefDocTypeSchema.PK);
			int rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("Count should be 1, for rows that reference the RefDocType about to be deleted", 1, rowCount);

			DocumentsUpgradeTask upgradeTask = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			upgradeTask.Run();

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				StmMenuTemplatePivotSchema.Constants.TableName,
				StmMenuTemplatePivotSchema.Constants.SI_RT_DocType);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", new Guid("2596006D-717C-4BE1-83FF-C44B1DA6420E"), StmMenuTemplatePivotSchema.SI_RT_DocType);
			rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("Count should be 0, there should be no rows in StmMenuTemplatePivot referencing the deleted DocType", 0, rowCount);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0} WHERE {1} = @PK",
				RefDocTypeSchema.Constants.TableName,
				RefDocTypeSchema.Constants.PK);
			cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@PK", new Guid("2596006D-717C-4BE1-83FF-C44B1DA6420E"), RefDocTypeSchema.PK);
			rowCount = (int)cmd.ExecuteScalar();
			AssertEquals("Count should be 0, this RefDocType row should have been deleted when the upgrade ran", 0, rowCount);
		}

		public void TestRemoveDocTypeWithReferencedPivotsDoesNotFailOnUpgrade()
		{
			DocumentsUpgradeTask task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());

			Guid stmMenuTemplatePivotPKWithNoSI_RT_DocType = new Guid("3bb950bc-26d2-477c-af43-0074bb7e6315");
			DataRow stmMenuTemplatePivotRow = task.ResourceFile.DataSet.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Rows.Find(stmMenuTemplatePivotPKWithNoSI_RT_DocType);

			string refDocTypePK = "8E3E9DEC-0F39-43F0-908B-6EC55A018067";
			string stmMenuTemplatePivotPK = "DFB76687-1ED7-4109-BFE8-3796FFE9652D";
			string stmMenuEDocsPK = "B603B4A8-7CC0-443B-BC1E-8EB1CCA81947";

			string sqlText = String.Format(
				@"INSERT dbo.RefDocType (RT_PK, RT_IsSystem, RT_DocType, RT_ReferenceType, RT_Desc) VALUES ('{0}', 1, 'HHH', 'SHP', 'A test doctype');
				UPDATE dbo.StmMenuTemplatePivot SET SI_RT_DocType = '{0}' WHERE SI_PK = '{1}';
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SO, SI_SU, SI_RT_DocType, SI_IsSystemDefined, SI_DocumentTitle) VALUES ('{2}', '{3}', '{4}', '{0}', 0, 'Test');
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined) VALUES ('{5}', '{4}', '{0}', 0);",
			refDocTypePK,
			stmMenuTemplatePivotRow[StmMenuTemplatePivotSchema.PK.Name],
			stmMenuTemplatePivotPK,
			stmMenuTemplatePivotRow[StmMenuTemplatePivotSchema.SI_SO.Name],
			stmMenuTemplatePivotRow[StmMenuTemplatePivotSchema.SI_SU.Name],
			stmMenuEDocsPK);
			Db.Connection.ExecuteNonQuery(sqlText);

			task.Run();

			DataFile tempFile = new EmbeddedDataFile("", RefDocTypeSchema.Constants.TableName, StmMenuTemplatePivotSchema.Constants.TableName, StmMenuEDocsSchema.Constants.TableName);
			var data = tempFile.LoadDataFromDatabase();

			DataRow[] matchingDocTypes = data.Tables[RefDocTypeSchema.Constants.TableName].Select("RT_PK = '" + refDocTypePK + "'");
			AssertEquals("The db upgrade should have removed the 'system' document from the db because it is not in the documents.xml upgrade file", 0, matchingDocTypes.Length);

			DataRow[] matchingPivotRows = data.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Select("SI_PK = '" + stmMenuTemplatePivotPK + "'");
			AssertEquals("Should find one row", 1, matchingPivotRows.Length);
			AssertEquals("the user defined pivot should no longer have a doc type defined", DBNull.Value, matchingPivotRows[0]["SI_RT_DocType"]);

			matchingPivotRows = data.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Select("SI_PK = '" + stmMenuTemplatePivotRow[StmMenuTemplatePivotSchema.PK.Name] + "'");
			AssertEquals("Should find one row", 1, matchingPivotRows.Length);
			AssertEquals("the system defined pivot should no longer have a doc type defined", DBNull.Value, matchingPivotRows[0]["SI_RT_DocType"]);

			DataRow[] matchingMenuEDocsRows = data.Tables[StmMenuEDocsSchema.Constants.TableName].Select("SX_PK = '" + stmMenuEDocsPK + "'");
			AssertEquals("DbUpgrader should have removed the user added MenuEDocs row", 0, matchingMenuEDocsRows.Length);
		}

		public void TestEDocsProviderPlaceholderIsIgnored()
		{
			DocumentTablesCleaner.Clean();

			string sqlText = string.Format("INSERT {0} ({1}, {2}, {3}, {4}) VALUES (@pk, 'MENU_NAME_1', 1, @filterList)",
				StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName, StmMenuItemSchema.Constants.SU_IsSystemDefined,
				StmMenuItemSchema.Constants.SU_FilterList);

			Guid pk1 = Guid.NewGuid();
			Guid pk2 = Guid.NewGuid();
			Guid pk3 = Guid.NewGuid();

			using (DbCommand cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk1);
				cmd.AddParameter("@filterList", SqlDbType.VarChar, 1, "");
				cmd.ExecuteNonQuery();
			}

			DocumentsCompleteDataFile dataFile = new DocumentsCompleteDataFile();
			using (var dataSet = dataFile.LoadDataFromDatabase())
			{
				using (DbCommand cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk2);
					cmd.AddParameter("@filterList", SqlDbType.VarChar, DocumentsUpgradeTask.EDocsProviderPlaceholderTag.Length, DocumentsUpgradeTask.EDocsProviderPlaceholderTag);
					cmd.ExecuteNonQuery();
				}

				using (DbCommand cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk3);
					cmd.AddParameter("@filterList", SqlDbType.VarChar, 8, "DeleteMe");
					cmd.ExecuteNonQuery();
				}

				DocumentsUpgradeTask task = new DocumentsUpgradeTask(dataFile);
				task.Run(dataSet);
			}

			List<Guid> pks = new List<Guid>();

			using (DbCommand command = Db.Connection.Command("SELECT SU_PK FROM dbo.StmMenuItem"))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					pks.Add((Guid)reader[0]);
				}
			}

			AssertEquals("There should only be 2 StmMenuItems.", 2, pks.Count);
			AssertEquals("StmMenuItem with pk1 should exist.", true, pks.Contains(pk1));
			AssertEquals("StmMenuItem with pk2 should exist.", true, pks.Contains(pk2));
		}

		public void TestSectionRepositoryTemplates()
		{
			DocumentTablesCleaner.Clean();
			Guid systemPK = Guid.NewGuid();
			Guid userPK = Guid.NewGuid();
			using (DbCommand cmd = Db.Connection.Command("INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_IsSystemDefined, SO_Template) VALUES (@pk, @name, '.Context.', 1, @template)"))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, systemPK);
				cmd.AddParameter("@name", SqlDbType.VarChar, 8000, "System Document Elements");
				cmd.AddParameter("@template", SqlDbType.VarBinary, new byte[] { 1, 2 });
				cmd.ExecuteNonQuery();

				cmd.SetParameterValue("@pk", userPK);
				cmd.SetParameterValue("@name", "Customized Document Elements");
				cmd.SetParameterValue("@template", new byte[] { 3, 4 });
				cmd.ExecuteNonQuery();
			}

			DocumentsCompleteDataFile dataFile = new DocumentsCompleteDataFile();
			using (var dataSet = dataFile.LoadDataFromDatabase())
			{
				using (DbCommand cmd = Db.Connection.Command("DELETE FROM dbo.StmTemplate WHERE SO_PK = @systemPK OR SO_PK = @userPK"))
				{
					cmd.AddParameter("@systemPK", SqlDbType.UniqueIdentifier, systemPK);
					cmd.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK);
					cmd.ExecuteNonQuery();
				}

				DocumentsUpgradeTask task = new DocumentsUpgradeTask(dataFile);
				task.Run(dataSet);
				AssertEquals("systemTemplate", new byte[] { 1, 2 }, GetTemplateBlob(systemPK));
				AssertEquals("userPK", new byte[] { 3, 4 }, GetTemplateBlob(userPK));

				using (DbCommand cmd = Db.Connection.Command("UPDATE dbo.StmTemplate SET SO_Template = @template WHERE SO_PK = @pk"))
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, systemPK);
					cmd.AddParameter("@template", SqlDbType.VarBinary, new byte[] { 5, 6 });
					cmd.ExecuteNonQuery();

					cmd.SetParameterValue("@pk", userPK);
					cmd.SetParameterValue("@template", new byte[] { 7, 8 });
					cmd.ExecuteNonQuery();
				}

				task.Run(dataSet);
				AssertEquals("systemTemplate", new byte[] { 1, 2 }, GetTemplateBlob(systemPK));
				AssertEquals("userPK", new byte[] { 7, 8 }, GetTemplateBlob(userPK));
			}
		}

		public void TestIncludeDocInArchiveColumnHandling()
		{
			SetupConditionallyIgnoredColumnTest(false);

			CombineAssertions(() =>
			{
				AssertEquals("TestMenu1 SU_IncludeDocInArchive", "", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu1'"));
				AssertEquals("TestMenu2 SU_IncludeDocInArchive", "YES", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu2'"));

				AssertEquals("TestMenu3 SU_IncludeDocInArchive", "YES", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu3'"));
				AssertEquals("TestMenu4 SU_IncludeDocInArchive", "", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu4'"));
				AssertEquals("TestMenu5 SU_IncludeDocInArchive", "SYS", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu5'"));
				AssertEquals("TestMenu6 SU_IncludeDocInArchive", "YES", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu6'"));
				AssertEquals("TestMenu7 SU_IncludeDocInArchive", "SNO", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu7'"));
				AssertEquals("TestMenu8 SU_IncludeDocInArchive", "", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu8'"));
			});
		}

		public void TestIncludeDocInArchiveColumnHandlingDuringSetup()
		{
			SetupConditionallyIgnoredColumnTest(true);

			CombineAssertions(() =>
			{
				AssertEquals("TestMenu1 SU_IncludeDocInArchive", "", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu1'"));
				AssertEquals("TestMenu2 SU_IncludeDocInArchive", "YES", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu2'"));

				AssertEquals("TestMenu3 SU_IncludeDocInArchive", "SYS", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu3'"));
				AssertEquals("TestMenu4 SU_IncludeDocInArchive", "SNO", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu4'"));
				AssertEquals("TestMenu5 SU_IncludeDocInArchive", "YES", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu5'"));
				AssertEquals("TestMenu6 SU_IncludeDocInArchive", "", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu6'"));
				AssertEquals("TestMenu7 SU_IncludeDocInArchive", "", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu7'"));
				AssertEquals("TestMenu8 SU_IncludeDocInArchive", "YES", (string)Db.Connection.ExecuteScalar("SELECT SU_IncludeDocInArchive FROM dbo.StmMenuItem WHERE SU_MenuName = 'TestMenu8'"));
			});
		}

		public void TestPreventAutoDeliveryColumnHandling()
		{
			SetupConditionallyIgnoredColumnTest(false);

			CombineAssertions(() =>
			{
				var testCases = new[]
				{
					("Contact_NCT_NCT_1"   , false),
					("Contact_ALL_ANY_1"   , false),
					("Contact_ALL_ANY_0"   , true),
					("Contact_NCT_NCT_1_SD", true),
					("Contact_ALL_ANY_1_SD", false),
					("Contact_ALL_ANY_0_SD", true),
					("Contact_NCT_OTH_1"   , false),
					("Contact_ALL_NCT_1"   , false),
					("Contact_ALL_NCT_0"   , true),
					("Contact_NCT_OTH_1_SD", true),
					("Contact_ALL_NCT_1_SD", true),
					("Contact_ALL_NCT_0_SD", false),
				};
				foreach (var (menuName, expectedPreventAutoDelivery) in testCases)
				{
					AssertEquals(menuName, expectedPreventAutoDelivery, (bool)Db.Connection.ExecuteScalar($"SELECT SU_PreventAutoDelivery FROM dbo.StmMenuItem WHERE SU_MenuName = '{menuName}'"));
				}
			});
		}

		public void TestPreventAutoDeliveryColumnHandlingDuringSetup()
		{
			SetupConditionallyIgnoredColumnTest(true);

			CombineAssertions(() =>
			{
				var testCases = new[]
				{
					("Contact_NCT_NCT_1"   , false),
					("Contact_ALL_ANY_1"   , false),
					("Contact_ALL_ANY_0"   , true),
					("Contact_NCT_NCT_1_SD", true),
					("Contact_ALL_ANY_1_SD", true),
					("Contact_ALL_ANY_0_SD", false),
					("Contact_NCT_OTH_1"   , false),
					("Contact_ALL_NCT_1"   , false),
					("Contact_ALL_NCT_0"   , true),
					("Contact_NCT_OTH_1_SD", true),
					("Contact_ALL_NCT_1_SD", true),
					("Contact_ALL_NCT_0_SD", false),
				};
				foreach (var (menuName, expectedPreventAutoDelivery) in testCases)
				{
					AssertEquals(menuName, expectedPreventAutoDelivery, (bool)Db.Connection.ExecuteScalar($"SELECT SU_PreventAutoDelivery FROM dbo.StmMenuItem WHERE SU_MenuName = '{menuName}'"));
				}
			});
		}

		public void TestCheckHasNonSystemRelatedItems()
		{
			const string sql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined) VALUES('11111111-2222-3333-4444-555555555555', 'Template1', 'Consol', null, 1);

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES('22222222-3333-4444-5555-666666666666', 'Menu1', 1);
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES('33333333-4444-5555-6666-777777777777', 'Menu2', 1);

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('AAAAAAAA-0000-0000-0000-111111111111', '22222222-3333-4444-5555-666666666666', '11111111-2222-3333-4444-555555555555', 'Doc1', 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('AAAAAAAA-0000-0000-0000-222222222222', '33333333-4444-5555-6666-777777777777', '11111111-2222-3333-4444-555555555555', 'Doc2', 0);
				";
			Db.Connection.ExecuteNonQuery(sql);

			DocumentsUpgradeTaskForTest upgradeTask = new DocumentsUpgradeTaskForTest();

			AssertNoExceptionThrown(() => upgradeTask.ExposedCheckHasNonSystemRelatedItems(new Guid("22222222-3333-4444-5555-666666666666")));
			AssertExceptionThrown(typeof(ApplicationException), () => upgradeTask.ExposedCheckHasNonSystemRelatedItems(new Guid("33333333-4444-5555-6666-777777777777")));
		}

		public void TestDeleteClientSpecificRelatedItems()
		{
			const string sql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined) VALUES('11111111-2222-3333-4444-555555555555', 'Template1', 'Consol', null, 1);
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined) VALUES('11111112-2223-3334-4445-555555555551', 'Template2', 'Consol', null, 1);
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined) VALUES('11111122-2233-3344-4455-555555555511', 'Template3', 'Consol', null, 1);

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES('22222222-3333-4444-5555-666666666666', 'Menu1', 1);
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES('33333333-4444-5555-6666-777777777777', 'Menu2', 1);
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined) VALUES('44444444-5555-6666-7777-888888888888', 'Menu3', 1);

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('AAAAAAAA-0000-0000-0000-111111111111', '22222222-3333-4444-5555-666666666666', '11111111-2222-3333-4444-555555555555', 'Doc1', 1, 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('AAAAAAAA-0000-0000-0000-222222222222', '22222222-3333-4444-5555-666666666666', '11111112-2223-3334-4445-555555555551', 'Doc2', 1, 0);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('AAAAAAAA-0000-0000-0000-333333333333', '22222222-3333-4444-5555-666666666666', '11111122-2233-3344-4455-555555555511', 'Doc3', 0, 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific) VALUES('AAAAAAAA-0000-0000-0000-444444444444', '33333333-4444-5555-6666-777777777777', '11111111-2222-3333-4444-555555555555', 'Doc4', 1, 1);

				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('BBBBBBBB-0000-0000-0000-111111111111', '22222222-3333-4444-5555-666666666666', '33333333-4444-5555-6666-777777777777', 1, 1);
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('BBBBBBBB-0000-0000-0000-222222222222', '33333333-4444-5555-6666-777777777777', '22222222-3333-4444-5555-666666666666', 1, 0);
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('BBBBBBBB-0000-0000-0000-333333333333', '22222222-3333-4444-5555-666666666666', '44444444-5555-6666-7777-888888888888', 0, 1);
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined, SF_IsClientSpecific) VALUES('BBBBBBBB-0000-0000-0000-444444444444', '33333333-4444-5555-6666-777777777777', '44444444-5555-6666-7777-888888888888', 1, 1);
				";
			Db.Connection.ExecuteNonQuery(sql);

			DocumentsUpgradeTaskForTest upgradeTask = new DocumentsUpgradeTaskForTest();
			upgradeTask.ExposedDeleteClientSpecificRelatedItems(new Guid("22222222-3333-4444-5555-666666666666"));

			AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = 'AAAAAAAA-0000-0000-0000-111111111111'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = 'AAAAAAAA-0000-0000-0000-222222222222'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = 'AAAAAAAA-0000-0000-0000-333333333333'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = 'AAAAAAAA-0000-0000-0000-444444444444'"));

			AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK = 'BBBBBBBB-0000-0000-0000-111111111111'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK = 'BBBBBBBB-0000-0000-0000-222222222222'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK = 'BBBBBBBB-0000-0000-0000-333333333333'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK = 'BBBBBBBB-0000-0000-0000-444444444444'"));
		}

		public void TestRunUpgradeTask_ShouldPurgeTemplateCacheAndIncrementCustomisationVersion()
		{
			var templateCachePath = Path.Combine(Temp.TempPath, "TemplateCache");
			if (!Directory.Exists(templateCachePath))
			{
				Directory.CreateDirectory(templateCachePath);
			}

			Func<int> numberOfTemplatesInCache = () => Directory.GetFiles(templateCachePath).Length;
			var startingTemplateCacheCount = numberOfTemplatesInCache();
			var startingCustomisationVersion = DbRegistry.DocumentCustomisationVersionNumber.LoadValue(TestConnection);

			try
			{
				File.WriteAllText(Path.Combine(templateCachePath, "Something.xls"), "Blah blah blah");
				AssertEquals(startingTemplateCacheCount + 1, numberOfTemplatesInCache());

				var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
				task.Run();

				AssertEquals(0, numberOfTemplatesInCache());
				AssertEquals(startingCustomisationVersion + 1, DbRegistry.DocumentCustomisationVersionNumber.LoadValue(TestConnection));
			}
			finally
			{
				if (Directory.Exists(templateCachePath))
				{
					Directory.Delete(templateCachePath, true);
				}
			}
		}

		public void TestDoNotDeleteNonSystemClientConfig()
		{
			DocumentTablesCleaner.Clean();

			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			//Create some client data here. Should not be removed by the upgrade process.
			string clientDataSQL = @"
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection)
				VALUES('51E9FE6F-284A-4378-B71C-972C844E5DB8', 'ClientMenu', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific)
				VALUES('96ED7553-FC0D-4B8E-95E1-240CBFDF7F14', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '454d1f87-dbbc-4b0b-8b34-7c695d9fbead', 'Client Template Pivot1', 0, 0);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific)
				VALUES('184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '454d1f87-dbbc-4b0b-8b34-7c695d9fbead', 'Client Template Pivot1', 0, 0);

				INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI) VALUES ('549C36EA-F69D-46CA-BA47-ACEC7251178C', 0, 1, '96ED7553-FC0D-4B8E-95E1-240CBFDF7F14');
				INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI) VALUES ('D8ACAA9E-9678-436F-B6F6-9EDC9F2AFD3E', 0, 0, '184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91');

				INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('E9132BA6-AA46-49A3-8EE4-E960D784579F', '549C36EA-F69D-46CA-BA47-ACEC7251178C', 0, 1);
				INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('56E80516-8A61-4663-AE98-A78ED5FB06E0', 'D8ACAA9E-9678-436F-B6F6-9EDC9F2AFD3E', 0, 0);
			";
			Db.Connection.ExecuteNonQuery(clientDataSQL);

			task.Run();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK = '549C36EA-F69D-46CA-BA47-ACEC7251178C'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK = 'D8ACAA9E-9678-436F-B6F6-9EDC9F2AFD3E'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = 'E9132BA6-AA46-49A3-8EE4-E960D784579F'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = '56E80516-8A61-4663-AE98-A78ED5FB06E0'"));
		}

		public void TestFormBuilderDocumentColumnsWhichAreNotIgnoredAreAlwaysUpdated()
		{
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			string pk = Db.Connection.ExecuteScalar("SELECT TOP 1 SU_PK FROM dbo.StmMenuItem").ToString();

			var row = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName].Select(string.Format(CultureInfo.InvariantCulture, "SU_PK = '{0}'", pk));
			row[0]["SU_IsModifiable"] = 0;
			row[0]["SU_EmailSenderOverride"] = string.Empty;
			row[0]["SU_MenuType"] = "FRM";

			var updateSQL = string.Format(CultureInfo.InvariantCulture, @"UPDATE dbo.StmMenuItem
				SET SU_IsModifiable = 1,
					SU_EmailSenderOverride = 'a@b.cd'
				WHERE SU_PK = '{0}'", pk);
			Db.Connection.ExecuteNonQuery(updateSQL);

			var selectSQL = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.StmMenuItem WHERE SU_PK = '{0}'", pk);

			using (var command = Db.Connection.Command(selectSQL))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();

				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", true, (bool)reader["SU_IsModifiable"]);
					AssertEquals("Pre-condition", "a@b.cd", (string)reader["SU_EmailSenderOverride"]);
				});
			}

			task.Run();

			using (var command = Db.Connection.Command(selectSQL))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();

				CombineAssertions(() =>
				{
					AssertEquals("FormBuilder documents data should be match the XML.", false, (bool)reader["SU_IsModifiable"]);
					AssertEquals("FormBuilder documents data should be match the XML.", string.Empty, (string)reader["SU_EmailSenderOverride"]);
				});
			}
		}

		public void TestFormBuilderDocumentColumnsWhichAreIgnoredAreNotUpdated()
		{
			DocumentTablesCleaner.Clean();
			var task = new DocumentsUpgradeTask(new DocumentsCompleteDataFile());
			task.Run();

			string pk = Db.Connection.ExecuteScalar("SELECT TOP 1 SU_PK FROM dbo.StmMenuItem").ToString();

			var row = task.ResourceFile.DataSet.Tables[StmMenuItemSchema.Constants.TableName].Select(string.Format(CultureInfo.InvariantCulture, "SU_PK = '{0}'", pk));
			row[0]["SU_DeliveryRestrictionType"] = "NON";
			row[0]["SU_DeliveryRestrictionDescription"] = string.Empty;
			row[0]["SU_DeliveryRestrictionMacro"] = string.Empty;
			row[0]["SU_IsPublished"] = 0;
			row[0]["SU_PreventAutoDelivery"] = 0;
			row[0]["SU_ContactType"] = "ALL";
			row[0]["SU_MenuIndex"] = 0;
			row[0]["SU_MenuType"] = "FRM";
			row[0]["SU_EmailSubjectLine"] = string.Empty;

			var updateSQL = string.Format(CultureInfo.InvariantCulture, @"UPDATE dbo.StmMenuItem
				SET SU_DeliveryRestrictionType = 'UDF',
					SU_DeliveryRestrictionDescription = 'A Delivery Restriction Description',
					SU_DeliveryRestrictionMacro = '""Blah"" == ""Blah""',
					SU_IsPublished = 1,
					SU_PreventAutoDelivery = 1,
					SU_ContactType = 'ALL',
					SU_MenuIndex = 1,
					SU_EmailSubjectLine = 'An Email Subject'
				WHERE SU_PK = '{0}'", pk);
			Db.Connection.ExecuteNonQuery(updateSQL);

			var selectSQL = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.StmMenuItem WHERE SU_PK = '{0}'", pk);

			using (var command = Db.Connection.Command(selectSQL))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();

				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition", "UDF", (string)reader["SU_DeliveryRestrictionType"]);
					AssertEquals("Pre-condition", "A Delivery Restriction Description", (string)reader["SU_DeliveryRestrictionDescription"]);
					AssertEquals("Pre-condition", "\"Blah\" == \"Blah\"", (string)reader["SU_DeliveryRestrictionMacro"]);
					AssertEquals("Pre-condition", true, (bool)reader["SU_IsPublished"]);
					AssertEquals("Pre-condition", true, (bool)reader["SU_PreventAutoDelivery"]);
					AssertEquals("Pre-condition", (short)1, (short)reader["SU_MenuIndex"]);
					AssertEquals("Pre-condition", "An Email Subject", (string)reader["SU_EmailSubjectLine"]);
				});
			}

			task.Run();

			using (var command = Db.Connection.Command(selectSQL))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();

				CombineAssertions(() =>
				{
					AssertEquals("Should be same as the pre-condition", "UDF", (string)reader["SU_DeliveryRestrictionType"]);
					AssertEquals("Should be same as the pre-condition", "A Delivery Restriction Description", (string)reader["SU_DeliveryRestrictionDescription"]);
					AssertEquals("Should be same as the pre-condition", "\"Blah\" == \"Blah\"", (string)reader["SU_DeliveryRestrictionMacro"]);
					AssertEquals("Should be same as the pre-condition", true, (bool)reader["SU_IsPublished"]);
					AssertEquals("Should be same as the pre-condition", true, (bool)reader["SU_PreventAutoDelivery"]);
					AssertEquals("Should be same as the pre-condition", (short)1, (short)reader["SU_MenuIndex"]);
					AssertEquals("Should be same as the pre-condition", "An Email Subject", (string)reader["SU_EmailSubjectLine"]);
				});
			}
		}

		#region Implementation

		void SetupConditionallyIgnoredColumnTest(bool isRunningForSetup)
		{
			DocumentTablesCleaner.Clean();

			string sqlText = @"
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE93', 'TestMenu1', 0, '')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE94', 'TestMenu2', 0, 'YES')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE95', 'TestMenu3', 1, 'SYS')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE96', 'TestMenu4', 1, 'SNO')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE97', 'TestMenu5', 1, 'YES')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE98', 'TestMenu6', 1, '')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE99', 'TestMenu7', 1, '')
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_IncludeDocInArchive) VALUES ('5692FF9B-FE58-4805-AB7F-297700BABE03', 'TestMenu8', 1, 'YES')

			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('062fb261-1a1d-4e61-8b43-49dbb9b25b77', 'Contact_NCT_NCT_1', 0, 'NCT', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('ead95097-e7e4-4f87-9a9a-562d5c48e95c', 'Contact_ALL_ANY_1', 0, 'ALL', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('e0f1e535-606f-419b-926b-e9b85971a0fe', 'Contact_ALL_ANY_0', 0, 'ALL', 0)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('f82f9b6e-490e-406f-96b0-d9bc2fe6fcfb', 'Contact_NCT_NCT_1_SD', 1, 'NCT', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('8ad22733-79cc-4c15-a6fe-55387f092242', 'Contact_ALL_ANY_1_SD', 1, 'ALL', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('87828a3b-8836-4b51-95b6-9631c4582313', 'Contact_ALL_ANY_0_SD', 1, 'ALL', 0)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('bc450b5b-d387-4e41-80c7-68346f71e52a', 'Contact_NCT_OTH_1', 0, 'NCT', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('4a064b2e-3086-4c95-8806-2f0947de4f09', 'Contact_ALL_NCT_1', 0, 'ALL', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('8821d7c8-3c22-48e0-a211-e23f205043a9', 'Contact_ALL_NCT_0', 0, 'ALL', 0)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('424132ee-ec8f-417e-b8a3-1825ad015768', 'Contact_NCT_OTH_1_SD', 1, 'NCT', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('2314cbf8-8b60-485a-886c-33d8df0b0302', 'Contact_ALL_NCT_1_SD', 1, 'ALL', 1)
			insert into dbo.StmMenuItem (SU_PK, SU_MenuName, SU_IsSystemDefined, SU_ContactType, SU_PreventAutoDelivery) VALUES ('ddd1a807-219f-403a-83a8-ccd5c4714f34', 'Contact_ALL_NCT_0_SD', 1, 'ALL', 0)
			";
			Db.Connection.ExecuteNonQuery(sqlText);

			using (TempDirectory tempDir = new TempDirectory())
			{
				string tempFilePath = Path.Combine(tempDir.DirectoryName, "TestWriteXml.xml");

				TempDocumentsDataFile dataFile = new TempDocumentsDataFile();
				var data = dataFile.LoadDataFromDatabase();
				dataFile.TestDataset = data;

				string sqlText2 = @"
				UPDATE dbo.StmMenuItem SET SU_IncludeDocInArchive = 'YES' WHERE SU_MenuName = 'TestMenu3'
				UPDATE dbo.StmMenuItem SET SU_IncludeDocInArchive = '' WHERE SU_MenuName = 'TestMenu4'
				UPDATE dbo.StmMenuItem SET SU_IncludeDocInArchive = 'SYS' WHERE SU_MenuName = 'TestMenu5'
				UPDATE dbo.StmMenuItem SET SU_IncludeDocInArchive = 'YES' WHERE SU_MenuName = 'TestMenu6'
				UPDATE dbo.StmMenuItem SET SU_IncludeDocInArchive = 'SNO' WHERE SU_MenuName = 'TestMenu7'
				UPDATE dbo.StmMenuItem SET SU_IncludeDocInArchive = '' WHERE SU_MenuName = 'TestMenu8'

				UPDATE dbo.StmMenuItem SET SU_PreventAutoDelivery = IIF(SU_PreventAutoDelivery = 1, 0, 1) WHERE SU_MenuName LIKE 'Contact%'
				UPDATE dbo.StmMenuItem SET SU_ContactType = 'OTH' WHERE SU_MenuName LIKE '%NCT_OTH%' AND SU_ContactType = 'NCT'
				UPDATE dbo.StmMenuItem SET SU_ContactType = 'NCT' WHERE SU_MenuName LIKE '%ALL_NCT%' AND SU_ContactType = 'ALL'
				UPDATE dbo.StmMenuItem SET SU_ContactType = 'ANY' WHERE SU_MenuName LIKE '%ALL_ANY%' AND SU_ContactType = 'ALL'
				";

				Db.Connection.ExecuteNonQuery(sqlText2);

				DocumentsUpgradeTask task = new DocumentsUpgradeTask(dataFile);
				task.IsRunningForSetup = isRunningForSetup;
				task.Run();
			}
		}

		byte[] GetTemplateBlob(Guid pk) => (byte[])Db.Connection.ExecuteScalar(
			FormattableString.Invariant($"SELECT [{StmTemplateSchema.Constants.SO_Template}] FROM [{StmTemplateSchema.Constants.TableName}] WHERE [{StmTemplateSchema.Constants.PK}] = @pk"),
			cmd => cmd.AddParameterBasedOnDbColumn("@pk", pk, StmTemplateSchema.PK));

		DataSet RetrieveData(params string[] tableNames)
		{
			DataFile tempFile = new EmbeddedDataFile("", tableNames);
			return tempFile.LoadDataFromDatabase();
		}

		class DocumentsUpgradeTaskForTest : DocumentsUpgradeTask
		{
			public DocumentsUpgradeTaskForTest() { }

			public bool ExposedIsRunningForSetup
			{
				get { return IsRunningForSetup; }
				set { IsRunningForSetup = value; }
			}

			public StringCollection ExposedColumnsToIgnore
			{
				get { return ColumnsToIgnore; }
			}

			public void ExposedCheckHasNonSystemRelatedItems(Guid menuItemPk)
			{
				CheckHasNonSystemRelatedItems(menuItemPk);
			}

			public void ExposedDeleteClientSpecificRelatedItems(Guid menuItemPk)
			{
				DeleteClientSpecificRelatedItems(menuItemPk);
			}
		}

		class TempDocumentsDataFile : DocumentsDataFile
		{
			protected override DataSet LoadDataSet()
			{
				return TestDataset;
			}

			public DataSet TestDataset { get; set; }
		}

		struct ColumnToIgnoreForTest
		{
			public string TableName;
			public string ColumnName;
			public string Value1;
			public string Value2;

			public ColumnToIgnoreForTest(string tableName, string columnName, string value1, string value2)
			{
				TableName = tableName;
				ColumnName = columnName;
				Value1 = value1;
				Value2 = value2;
			}
		}

		readonly List<ColumnToIgnoreForTest> ColumnsToIgnoreForTest = new List<ColumnToIgnoreForTest>
		{
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_IsModifiable, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_DeliveryRestrictionType, "'CNH'", "'NON'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_DeliveryRestrictionDescription, "'CNH'", "'NON'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_DeliveryRestrictionMacro, "''", "''"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_IsPublished, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_IsLocalDocument, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_IsVisibleOnWeb, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_PreventAutoDelivery, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_EmailSubjectLine, "'Email Subject 1'", "'Email Subject 2'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_EmailSenderOverride, "'Email Sender 1'", "'Email Sender 2'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_SignBy, "'NON'", "'PFX'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_IncludeDocInArchive, "'YES'", "'NO'"),

			new ColumnToIgnoreForTest(StmMenuEDocsSchema.Constants.TableName, StmMenuEDocsSchema.Constants.SX_IsClientSupressed, "1", "0"),

			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_IsPublished, "1", "0"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_SaveVersions, "1", "0"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_IsPublishUpdatable, "1", "0"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_LogSystemCreatedDocsToEDocs, "1", "0"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_ForceUserToRead, "1", "0"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_IsActive, "1", "0"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_SE_NKDocumentReceivedEvent, "'AAA'", "'BBB'"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_LogMacro, "'AAA'", "'BBB'"),
			new ColumnToIgnoreForTest(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.RT_AllowMultiplePeriodicDocs, "1", "0"),

			new ColumnToIgnoreForTest(RateAttachmentSetSchema.Constants.TableName, RateAttachmentSetSchema.Constants.TS_IsDefault, "1", "0"),
			new ColumnToIgnoreForTest(RateAttachmentSetSchema.Constants.TableName, RateAttachmentSetSchema.Constants.TS_IsMandatory, "1", "0"),
			new ColumnToIgnoreForTest(RateAttachmentSetSchema.Constants.TableName, RateAttachmentSetSchema.Constants.TS_Sequence, "1", "2"),

			new ColumnToIgnoreForTest(StmMenuTemplatePivotSchema.Constants.TableName, StmMenuTemplatePivotSchema.Constants.SI_IsPasswordProtected, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuTemplatePivotSchema.Constants.TableName, StmMenuTemplatePivotSchema.Constants.SI_IsPasswordProtectedForOpening, "1", "0"),
			new ColumnToIgnoreForTest(StmMenuTemplatePivotSchema.Constants.TableName, StmMenuTemplatePivotSchema.Constants.SI_PrintByDefault, "1", "0"),

			new ColumnToIgnoreForTest(StmMenuDocumentConfigSchema.Constants.TableName, StmMenuDocumentConfigSchema.Constants.S3_ExcludedFromDocPack, "1", "0"),
		};

		readonly List<ColumnToIgnoreForTest> FormBuilderColumnsToIgnoreForTest = new List<ColumnToIgnoreForTest>
		{
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_DeliveryRestrictionType, "'CNH'", "'NON'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_DeliveryRestrictionDescription, "'CNH'", "'NON'"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_DeliveryRestrictionMacro, "''", "''"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_MenuIndex, "0", "1"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_IsPublished, "0", "1"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_PreventAutoDelivery, "0", "1"),
			new ColumnToIgnoreForTest(StmMenuItemSchema.Constants.TableName, StmMenuItemSchema.Constants.SU_EmailSubjectLine, "'Email Subject 1'", "'Email Subject 2'"),
		};
		#endregion
	}
}
