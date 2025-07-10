using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class DocumentsCompleteDataFileNoTableCleanupTest : DocumentsDataFileNoTableCleanupTest
	{
		protected override DocumentsDataFile GetDocumentsDataFile()
		{
			return new DocumentsCompleteDataFile();
		}

		public void TestOrphanDocConfigsAreDeleted()
		{
			DocumentsDataFile dataFile = GetDocumentsDataFile();
			using (DataSet dataSet = dataFile.LoadDataFromDatabase())
			{
				Guid templatePK = Guid.NewGuid();
				Guid menuPK = Guid.NewGuid();
				Guid menuTemplatePivotPK = Guid.NewGuid();
				Guid docConfig1PK = Guid.NewGuid();
				Guid docConfig2PK = Guid.NewGuid();
				Guid configItem1PK = Guid.NewGuid();
				Guid configItem2PK = Guid.NewGuid();

				Dictionary<string, Guid> parameters = new Dictionary<string, Guid>();
				parameters.Add("@templatePK", templatePK);
				parameters.Add("@menuPK", menuPK);
				parameters.Add("@menuTemplatePivotPK", menuTemplatePivotPK);
				parameters.Add("@docConfig1PK", docConfig1PK);
				parameters.Add("@docConfig2PK", docConfig2PK);
				parameters.Add("@configItem1PK", configItem1PK);
				parameters.Add("@configItem2PK", configItem2PK);

				string sqlText = @"
					INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_IsSystemDefined) VALUES(@templatePK, '.Template.', '.Context.', 1);
					INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_IsSystemDefined) VALUES(@menuPK, '.Menu.', '.Context.', 1);
					INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES(@menuTemplatePivotPK, @menuPK, @templatePK, '.Pivot.', 1);
					INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_SI, S3_Description) VALUES (@docConfig1PK, 1, @menuTemplatePivotPK, 'Default');
					INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_SI, S3_Description) VALUES (@docConfig2PK, 0, @menuTemplatePivotPK, 'Default');
					INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3) VALUES (@configItem1PK, @docConfig1PK);
					INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3) VALUES (@configItem2PK, @docConfig2PK)";

				using (DbCommand command = Db.Connection.Command(sqlText))
				{
					foreach (KeyValuePair<string, Guid> parameter in parameters)
					{
						command.AddParameter(parameter.Key, SqlDbType.UniqueIdentifier, parameter.Value);
					}
					command.ExecuteNonQuery();
				}

				DocumentsUpgradeTask task = new DocumentsUpgradeTask(GetDocumentsDataFile());
				task.Run(dataSet);

				sqlText = @"
					SELECT 
					(SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = @templatePK) AS StmTemplateCount,
					(SELECT COUNT(*) FROM dbo.StmMenuItem WHERE SU_PK = @menuPK) AS StmMenuItemCount,
					(SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = @menuTemplatePivotPK) AS StmMenuTemplatePivotCount,
					(SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK IN (@docConfig1PK, @docConfig2PK)) AS StmMenuDocumentConfigCount,
					(SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK IN (@configItem1PK, @configItem2PK)) AS StmMenuDocumentConfigItemCount";

				using (DbCommand command = Db.Connection.Command(sqlText))
				{
					foreach (KeyValuePair<string, Guid> parameter in parameters)
					{
						command.AddParameter(parameter.Key, SqlDbType.UniqueIdentifier, parameter.Value);
					}
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						AssertEquals("StmTemplate records should be deleted.", 0, (int)reader["StmTemplateCount"]);
						AssertEquals("StmMenuItem records should be deleted.", 0, (int)reader["StmMenuItemCount"]);
						AssertEquals("StmMenuTemplatePivot records should be deleted.", 0, (int)reader["StmMenuTemplatePivotCount"]);
						AssertEquals("StmMenuDocumentConfig records should be deleted.", 0, (int)reader["StmMenuDocumentConfigCount"]);
						AssertEquals("StmMenuDocumentConfigItem records should be deleted.", 0, (int)reader["StmMenuDocumentConfigItemCount"]);
					}
				}
			}
		}

		public void TestOrphanUserDefinedMenuPivotsAreDelete()
		{
			string sqlText = "SELECT TOP 1 SU_PK FROM dbo.StmMenuItem WHERE SU_IsSystemDefined = 1";
			string existingSystemMenuPk = Db.Connection.ExecuteScalar(sqlText).ToString();
			sqlText = "SELECT TOP 1 SO_PK FROM dbo.StmTemplate WHERE SO_IsSystemDefined = 1";
			string existingSystemTemplatePk = Db.Connection.ExecuteScalar(sqlText).ToString();

			string inexistingSystemMenuPk = "1B934998-649C-48F5-B452-89D885C85021";
			string inexistingSystemTemplatePk = "59F9EB02-4B8D-4A50-9A5B-18119D7C8380";

			string userExistingMenuInexistingTemplatePivotPk = "57C80CBE-6F3E-4EB9-90B7-5600475369A7";
			string userInexistingMenuExistingTemplatePivotPk = "8AC76D4F-A558-44F5-B6CD-B718E2EF91FA";

			string userExistingOutwardMenuInexistingInwardMenuPivotPk = "BD4A8840-7BBC-4FD5-9FC0-4B3B2F65F9D6";
			string userInexistingOutwardMenuExistingInwardMenuPivotPk = "E810991E-5A5A-4F66-99A0-6EC0D4EF9D21";

			string userMenuTemplatePivotToBePreservedPk = "F4215D7E-C0A1-4F1E-B585-6595E2709818";
			string userMenuMenuPivotToBePreservedPk = "33D8A3DC-D746-4513-8B9C-9BC32AD35E92";

			string userInexistingMenuEDocsPivotPk = "95CD5892-17E4-46E2-8077-AFD590317404";
			string userEDocsPivotToBePreservedPk = "47267295-9D74-400A-9F68-DAC41A6110A4";

			sqlText = String.Format(@"
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES ('{0}', 'SysMenuToBeDeleted', 'Consol', '', 1, 1, 0, '', 'None', 0, '', '', '')
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES ('{1}', 'SysTemplateToBeDeleted', 'Consol', null, 1, 0, null, '')

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_IsSystemDefined) VALUES ('{4}', '{2}', '{1}', 0)
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_IsSystemDefined) VALUES ('{5}', '{0}', '{3}', 0)
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_IsSystemDefined) VALUES ('{6}', '{2}', '{3}', 0)

				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined) VALUES ('{7}', '{2}', '{0}', 0)
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined) VALUES ('{8}', '{0}', '{2}', 0)
				INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsSystemDefined) VALUES ('{9}', '{2}', '{2}', 0)

				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_IsSystemDefined) VALUES ('{10}', '{0}', 0)
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_IsSystemDefined) VALUES ('{11}', '{2}', 0)",

				inexistingSystemMenuPk, inexistingSystemTemplatePk, existingSystemMenuPk, existingSystemTemplatePk,
				userExistingMenuInexistingTemplatePivotPk, userInexistingMenuExistingTemplatePivotPk, userMenuTemplatePivotToBePreservedPk,
				userExistingOutwardMenuInexistingInwardMenuPivotPk, userInexistingOutwardMenuExistingInwardMenuPivotPk, userMenuMenuPivotToBePreservedPk,
				userInexistingMenuEDocsPivotPk, userEDocsPivotToBePreservedPk);

			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK in ('{0}', '{1}', '{2}')",
				userExistingMenuInexistingTemplatePivotPk, userInexistingMenuExistingTemplatePivotPk, userMenuTemplatePivotToBePreservedPk);
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("[PRE-CONDITION] Test Menu-Template pivots should have been inserted", 3, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK in ('{0}', '{1}', '{2}')",
				userExistingOutwardMenuInexistingInwardMenuPivotPk, userInexistingOutwardMenuExistingInwardMenuPivotPk, userMenuMenuPivotToBePreservedPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("[PRE-CONDITION] Test Menu-Menu pivots should have been inserted", 3, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuEDocs WHERE SX_PK in ('{0}', '{1}')",
				userInexistingMenuEDocsPivotPk, userEDocsPivotToBePreservedPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("[PRE-CONDITION] Test Menu-DocType pivots should have been inserted", 2, rowCount);

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(GetDocumentsDataFile());
			task.Run();

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK in ('{0}', '{1}')",
				userExistingMenuInexistingTemplatePivotPk, userInexistingMenuExistingTemplatePivotPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Orphan Menu-Template pivots should have been deleted", 0, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '{0}'",
				userMenuTemplatePivotToBePreservedPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Non-Orphan Menu-Template pivot should NOT have been deleted", 1, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK in ('{0}', '{1}')",
				userExistingOutwardMenuInexistingInwardMenuPivotPk, userInexistingOutwardMenuExistingInwardMenuPivotPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Orphan Menu-Menu pivots should have been deleted", 0, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK = '{0}'",
				userMenuMenuPivotToBePreservedPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Non-Orphan Menu-Menu pivot should NOT have been deleted", 1, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuEDocs WHERE SX_PK = '{0}'",
				userInexistingMenuEDocsPivotPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Orphan Menu-DocType pivots should have been deleted", 0, rowCount);

			sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuEDocs WHERE SX_PK = '{0}'",
				userEDocsPivotToBePreservedPk);
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Non-Orphan Menu-DocType pivot should NOT have been deleted", 1, rowCount);
		}

		public void TestDuplicateRefDocTypesAreRemoved()
		{
			Guid docTypePk_SCL_PKD_Original = GetRefDocTypePk("SCL", "PKD");
			Guid docTypePk_ALL_PRV_Original = GetRefDocTypePk("ALL", "PRV");

			Guid docTypePk_SCL_PKD_Test = Guid.NewGuid();
			Guid docTypePk_ALL_PRV_Test = Guid.NewGuid();

			Guid stmMenuItemPk = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 SU_PK FROM dbo.StmMenuItem");

			string sqlText = @"
				DELETE dbo.RefDocType WHERE RT_ReferenceType = 'SCL' AND RT_DocType = 'PKD'
				DELETE dbo.RefDocType WHERE RT_ReferenceType = 'ALL' AND RT_DocType = 'PRV'
				";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = String.Format(@"
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('{0}', 'SCL', 'PKD', 'Test_SCL_PKD', 1, 0)
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('{1}', 'ALL', 'PRV', 'Test_ALL_PRV', 1, 0)
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined) VALUES ('4841DE6D-3F30-4713-9368-58213BBAED30', '{2}', '{0}', 0)
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined) VALUES ('97A2CACB-8689-41BE-9B6B-D8FA2CAB7A80', '{2}', '{1}', 0)
				-- INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_RT_DocType, SI_IsSystemDefined) VALUES('C6967639-85C8-4F70-A7CE-CD780B75B5A4', '{2}', 'sopk', '{0}', 0);
				",
				docTypePk_SCL_PKD_Test.ToString(), docTypePk_ALL_PRV_Test.ToString(), stmMenuItemPk.ToString());
			Db.Connection.ExecuteNonQuery(sqlText);

			AssertEquals("[PRE-CONDITION] SCL-PKD PK", docTypePk_SCL_PKD_Test, GetRefDocTypePk("SCL", "PKD"));
			AssertEquals("[PRE-CONDITION] ALL-PRV PK", docTypePk_ALL_PRV_Test, GetRefDocTypePk("ALL", "PRV"));

			AssertEquals("[PRE-CONDITION] StmMenuEDocs FK to SCL-PKD", 1, GetMenuEDocsRowCount(docTypePk_SCL_PKD_Test));
			AssertEquals("[PRE-CONDITION] StmMenuEDocs FK to ALL-PRV", 1, GetMenuEDocsRowCount(docTypePk_ALL_PRV_Test));

			DocumentsUpgradeTask task = new DocumentsUpgradeTask(GetDocumentsDataFile());
			task.Run();

			AssertEquals("SCL-PKD PK (After Upgrade)", docTypePk_SCL_PKD_Original, GetRefDocTypePk("SCL", "PKD"));
			AssertEquals("ALL-PRV PK (After Upgrade)", docTypePk_ALL_PRV_Original, GetRefDocTypePk("ALL", "PRV"));

			AssertEquals("StmMenuEDocs FK to Test SCL-PKD (After Upgrade)", 0, GetMenuEDocsRowCount(docTypePk_SCL_PKD_Test));
			AssertEquals("StmMenuEDocs FK to Test ALL-PRV (After Upgrade)", 0, GetMenuEDocsRowCount(docTypePk_ALL_PRV_Test));

			AssertEquals("StmMenuEDocs FK to Original SCL-PKD (After Upgrade)", 1, GetMenuEDocsRowCount(docTypePk_SCL_PKD_Original));
			AssertEquals("StmMenuEDocs FK to Original ALL-PRV (After Upgrade)", 1, GetMenuEDocsRowCount(docTypePk_ALL_PRV_Original));
		}

		[ExpectNoExceptions]
		public void TestSetupFromFileDoesNotThrowException()
		{
			var upgradeTask = new EmbeddedUpgradeTask(new DocumentsCompleteDataFile());
			upgradeTask.RunForSetup();
		}
	}
}
