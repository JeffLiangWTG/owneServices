using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class UpgradeTaskTest : TransactionedTestCase
	{
		public void TestInsert()
		{
			var fResourceFile = new DataFileForTest();

			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");

			AssertEquals("Table Count", 3, data.Tables.Count);

			AssertEquals("StmMenuItem row count", 1, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("Menu Name", "Test Doc", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());

			AssertEquals("StmTemplate row count", 3, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("Template1 Name", "TestTemplate1", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());
			AssertEquals("Template2 Name", "TestTemplate2", data.Tables["StmTemplate"].Rows[1]["SO_Name"].ToString());
			AssertEquals("Template3 Name", "TestTemplate3", data.Tables["StmTemplate"].Rows[2]["SO_Name"].ToString());

			AssertEquals("StmMenuTemplatePivot row count", 1, data.Tables["StmMenuTemplatePivot"].Rows.Count);
			AssertEquals("Pivot Title", "Test Title", data.Tables["StmMenuTemplatePivot"].Rows[0]["SI_DocumentTitle"].ToString());
		}

		public void TestInsert_WithAuditColumns()
		{
			var fResourceFile = new DataFileForTest();
			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			AssertEquals("Table Count", 3, data.Tables.Count);

			var stmMenuItemTable = data.Tables["StmMenuItem"];
			AssertEquals("StmMenuItem row count", 1, stmMenuItemTable.Rows.Count);

			foreach (DataRow row in stmMenuItemTable.Rows)
			{
				AssertDataRowHasAuditDetails(row, "SU");
			}

			var stmTemplateTable = data.Tables["StmTemplate"];
			AssertEquals("StmTemplate row count", 3, stmTemplateTable.Rows.Count);

			foreach (DataRow row in stmTemplateTable.Rows)
			{
				AssertDataRowHasAuditDetails(row, "SO");
			}

			var stmMenuTemplatePivotTable = data.Tables["StmMenuTemplatePivot"];
			AssertEquals("StmMenuTemplatePivot row count", 1, stmMenuTemplatePivotTable.Rows.Count);

			foreach (DataRow row in stmMenuTemplatePivotTable.Rows)
			{
				AssertDataRowHasAuditDetails(row, "SI");
			}

			void AssertDataRowHasAuditDetails(DataRow row, string prefix)
			{
				CombineAssertions(() =>
				{
					Assert($"{prefix}_SystemCreateTimeUtc", row[$"{prefix}_SystemCreateTimeUtc"] is DateTime createTime && createTime > DateTime.UtcNow.AddDays(-1));
					AssertEquals($"{prefix}_SystemCreateUser", "~BP", row[$"{prefix}_SystemCreateUser"]);
					Assert($"{prefix}_SystemLastEditTimeUtc", row[$"{prefix}_SystemLastEditTimeUtc"] is DateTime lastEdit && lastEdit > DateTime.UtcNow.AddDays(-1));
					AssertEquals($"{prefix}_SystemLastEditUser", "~BP", row[$"{prefix}_SystemLastEditUser"]);
				});
			}
		}

		public void TestUpdate()
		{
			string insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate11', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate22', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate33', 'Consol', null, 1, 0, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Test Doc Changed', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('4DCC8904-8A65-415E-98FF-1F23B8DC9A00', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Test Title Changed', 1);";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new DataFileForTest();

			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			DataSet data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");

			AssertEquals("Table Count", 3, data.Tables.Count);

			AssertEquals("StmMenuItem row count", 1, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("Menu Name", "Test Doc", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());

			AssertEquals("StmTemplate row count", 3, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("Template1 Name", "TestTemplate1", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());
			AssertEquals("Template2 Name", "TestTemplate2", data.Tables["StmTemplate"].Rows[1]["SO_Name"].ToString());
			AssertEquals("Template3 Name", "TestTemplate3", data.Tables["StmTemplate"].Rows[2]["SO_Name"].ToString());

			AssertEquals("StmMenuTemplatePivot row count", 1, data.Tables["StmMenuTemplatePivot"].Rows.Count);
			AssertEquals("Pivot Title", "Test Title", data.Tables["StmMenuTemplatePivot"].Rows[0]["SI_DocumentTitle"].ToString());
		}

		public void TestUpdate_WithAuditColumns()
		{
			var insertSql = @"
				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_StmTemplate_AuditDetailsAreNotMissing_Insert')
				BEGIN
					DISABLE TRIGGER TG_StmTemplate_AuditDetailsAreNotMissing_Insert ON StmTemplate
				END

				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_StmMenuItem_AuditDetailsAreNotMissing_Insert')
				BEGIN
					DISABLE TRIGGER TG_StmMenuItem_AuditDetailsAreNotMissing_Insert ON StmMenuItem
				END

				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_StmMenuTemplatePivot_AuditDetailsAreNotMissing_Insert')
				BEGIN
					DISABLE TRIGGER TG_StmMenuTemplatePivot_AuditDetailsAreNotMissing_Insert ON StmMenuTemplatePivot
				END

				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate11', 'Consol', null, 1, 0, null, '', DATEADD(DAY, -200, GetUtcDate()), 'D', null, 'A');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate22', 'Consol', null, 1, 0, null, '', DATEADD(DAY, -200, GetUtcDate()), 'E', null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate33', 'Consol', null, 1, 0, null, '', DATEADD(DAY, -200, GetUtcDate()), 'F', DATEADD(DAY, -100, GetUtcDate()), 'C');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection, SU_SystemCreateTimeUtc, SU_SystemCreateUser, SU_SystemLastEditTimeUtc, SU_SystemLastEditUser) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Test Doc Changed', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '', DATEADD(DAY, -200, GetUtcDate()), 'D', null, 'A');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_SystemCreateTimeUtc, SI_SystemCreateUser, SI_SystemLastEditTimeUtc, SI_SystemLastEditUser) VALUES('4DCC8904-8A65-415E-98FF-1F23B8DC9A00', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Test Title Changed', 1, DATEADD(DAY, -200, GetUtcDate()), 'D', null, 'A');

				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_StmTemplate_AuditDetailsAreNotMissing_Insert')
				BEGIN
					ENABLE TRIGGER TG_StmTemplate_AuditDetailsAreNotMissing_Insert ON StmTemplate
				END

				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_StmMenuItem_AuditDetailsAreNotMissing_Insert')
				BEGIN
					ENABLE TRIGGER TG_StmMenuItem_AuditDetailsAreNotMissing_Insert ON StmMenuItem
				END

				IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_StmMenuTemplatePivot_AuditDetailsAreNotMissing_Insert')
				BEGIN
					ENABLE TRIGGER TG_StmMenuTemplatePivot_AuditDetailsAreNotMissing_Insert ON StmMenuTemplatePivot
				END";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new DataFileForTest();

			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			AssertEquals("Table Count", 3, data.Tables.Count);

			var stmMenuItemTable = data.Tables["StmMenuItem"];
			AssertEquals("StmMenuItem row count", 1, stmMenuItemTable.Rows.Count);

			foreach (DataRow row in stmMenuItemTable.Rows)
			{
				AssertDataRowHasAuditDetails(row, "SU");
			}

			var stmTemplateTable = data.Tables["StmTemplate"];
			AssertEquals("StmTemplate row count", 3, stmTemplateTable.Rows.Count);

			foreach (DataRow row in stmTemplateTable.Rows)
			{
				AssertDataRowHasAuditDetails(row, "SO");
			}

			var stmMenuTemplatePivotTable = data.Tables["StmMenuTemplatePivot"];
			AssertEquals("StmMenuTemplatePivot row count", 1, stmMenuTemplatePivotTable.Rows.Count);

			foreach (DataRow row in stmMenuTemplatePivotTable.Rows)
			{
				AssertDataRowHasAuditDetails(row, "SI");
			}

			void AssertDataRowHasAuditDetails(DataRow row, string prefix)
			{
				CombineAssertions(() =>
				{
					// create columns should not be updated
					Assert($"{prefix}_SystemCreateTimeUtc", row[$"{prefix}_SystemCreateTimeUtc"] is DateTime createTime && createTime < DateTime.UtcNow.AddDays(-1));
					AssertNotEquals($"{prefix}_SystemCreateUser", "~BP", row[$"{prefix}_SystemCreateUser"]);
					Assert($"{prefix}_SystemLastEditTimeUtc", row[$"{prefix}_SystemLastEditTimeUtc"] is DateTime lastEdit && lastEdit > DateTime.UtcNow.AddDays(-1));
					AssertEquals($"{prefix}_SystemLastEditUser", "~BP", row[$"{prefix}_SystemLastEditUser"]);
				});
			}
		}

		public void TestUpdate_WithAuditColumns_WhenRowHasByteArrayChanges()
		{
			// 0x511175 is the binary equivalent of URF1 in base64
			var insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate1', 'Consol', null, 1, 0, 0x, '', DATEADD(DAY, -200, GetUtcDate()), 'D', DATEADD(DAY, -100, GetUtcDate()), 'A');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate2', 'Consol', null, 1, 0, 0x511175, '', DATEADD(DAY, -200, GetUtcDate()), 'E', DATEADD(DAY, -100, GetUtcDate()), 'B');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate3', 'Consol', null, 1, 0, 0x517511, '', DATEADD(DAY, -200, GetUtcDate()), 'F', DATEADD(DAY, -100, GetUtcDate()), 'C');";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new BinaryDataFile();
			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmTemplate");
			var stmTemplateTable = data.Tables["StmTemplate"];
			AssertEquals("StmTemplate row count", 3, stmTemplateTable.Rows.Count);

			var rows = stmTemplateTable.Rows.Cast<DataRow>();
			var row1 = rows.Single(r => (Guid)r["SO_PK"] == new Guid("59F9EB02-4B8D-4A50-9A5B-18119D7C8380"));
			var row2 = rows.Single(r => (Guid)r["SO_PK"] == new Guid("7D6292B9-B4B3-4D51-A43C-88E66981F356"));
			var row3 = rows.Single(r => (Guid)r["SO_PK"] == new Guid("63221015-496D-4839-A42D-8A5ACB61422D"));
			AssertDataRowHasAuditDetails(row1, "SO");
			AssertDataRowDidNotUpdateAuditDetails(row2, "SO");
			AssertDataRowHasAuditDetails(row3, "SO");

			void AssertDataRowHasAuditDetails(DataRow row, string prefix)
			{
				CombineAssertions(() =>
				{
					// create columns should not be updated
					Assert($"{prefix}_SystemCreateTimeUtc", row[$"{prefix}_SystemCreateTimeUtc"] is DateTime createTime && createTime < DateTime.UtcNow.AddDays(-1));
					AssertNotEquals($"{prefix}_SystemCreateUser", "~BP", row[$"{prefix}_SystemCreateUser"]);
					Assert($"{prefix}_SystemLastEditTimeUtc", row[$"{prefix}_SystemLastEditTimeUtc"] is DateTime lastEdit && lastEdit > DateTime.UtcNow.AddDays(-1));
					AssertEquals($"{prefix}_SystemLastEditUser", "~BP", row[$"{prefix}_SystemLastEditUser"]);
				});
			}

			void AssertDataRowDidNotUpdateAuditDetails(DataRow row, string prefix)
			{
				CombineAssertions(() =>
				{
					// no columns should be updated
					Assert($"{prefix}_SystemCreateTimeUtc", row[$"{prefix}_SystemCreateTimeUtc"] is DateTime createTime && createTime < DateTime.UtcNow.AddDays(-1));
					AssertNotEquals($"{prefix}_SystemCreateUser", "~BP", row[$"{prefix}_SystemCreateUser"]);
					Assert($"{prefix}_SystemLastEditTimeUtc", row[$"{prefix}_SystemLastEditTimeUtc"] is DateTime lastEdit && lastEdit < DateTime.UtcNow.AddDays(-1));
					AssertNotEquals($"{prefix}_SystemLastEditUser", "~BP", row[$"{prefix}_SystemLastEditUser"]);
				});
			}
		}

		class BinaryDataFile : EmbeddedDataFile
		{
			public BinaryDataFile()
				: base(@"Shared.Test\TestFiles\TestDataFileWithBinaryData.xml", "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem")
			{
			}
		}

		public void TestUpdate_WithAuditColumns_WhenRowHasNoChanges()
		{
			var insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate1', 'Consol', null, 1, 0, 0x, '', DATEADD(DAY, -200, GetUtcDate()), 'D', DATEADD(DAY, -100, GetUtcDate()), 'A');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate2', 'Consol', null, 1, 0, 0x, '', DATEADD(DAY, -200, GetUtcDate()), 'E', DATEADD(DAY, -100, GetUtcDate()), 'B');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction, SO_SystemCreateTimeUtc, SO_SystemCreateUser, SO_SystemLastEditTimeUtc, SO_SystemLastEditUser) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate3', 'Consol', null, 1, 0, 0x, '', DATEADD(DAY, -200, GetUtcDate()), 'F', DATEADD(DAY, -100, GetUtcDate()), 'C');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection, SU_SystemCreateTimeUtc, SU_SystemCreateUser, SU_SystemLastEditTimeUtc, SU_SystemLastEditUser) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Test Doc', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '', DATEADD(DAY, -200, GetUtcDate()), 'D', DATEADD(DAY, -100, GetUtcDate()), 'A');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_SystemCreateTimeUtc, SI_SystemCreateUser, SI_SystemLastEditTimeUtc, SI_SystemLastEditUser) VALUES('4DCC8904-8A65-415E-98FF-1F23B8DC9A00', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Test Title', 1, DATEADD(DAY, -200, GetUtcDate()), 'D', DATEADD(DAY, -100, GetUtcDate()), 'A');";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new DataFileForTest();

			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			AssertEquals("Table Count", 3, data.Tables.Count);

			var stmMenuItemTable = data.Tables["StmMenuItem"];
			AssertEquals("StmMenuItem row count", 1, stmMenuItemTable.Rows.Count);

			foreach (DataRow row in stmMenuItemTable.Rows)
			{
				AssertDataRowDidNotUpdateAuditDetails(row, "SU");
			}

			var stmTemplateTable = data.Tables["StmTemplate"];
			AssertEquals("StmTemplate row count", 3, stmTemplateTable.Rows.Count);

			foreach (DataRow row in stmTemplateTable.Rows)
			{
				AssertDataRowDidNotUpdateAuditDetails(row, "SO");
			}

			var stmMenuTemplatePivotTable = data.Tables["StmMenuTemplatePivot"];
			AssertEquals("StmMenuTemplatePivot row count", 1, stmMenuTemplatePivotTable.Rows.Count);

			foreach (DataRow row in stmMenuTemplatePivotTable.Rows)
			{
				AssertDataRowDidNotUpdateAuditDetails(row, "SI");
			}

			void AssertDataRowDidNotUpdateAuditDetails(DataRow row, string prefix)
			{
				CombineAssertions(() =>
				{
					// no columns should be updated
					Assert($"{prefix}_SystemCreateTimeUtc", row[$"{prefix}_SystemCreateTimeUtc"] is DateTime createTime && createTime < DateTime.UtcNow.AddDays(-1));
					AssertNotEquals($"{prefix}_SystemCreateUser", "~BP", row[$"{prefix}_SystemCreateUser"]);
					Assert($"{prefix}_SystemLastEditTimeUtc", row[$"{prefix}_SystemLastEditTimeUtc"] is DateTime lastEdit && lastEdit < DateTime.UtcNow.AddDays(-1));
					AssertNotEquals($"{prefix}_SystemLastEditUser", "~BP", row[$"{prefix}_SystemLastEditUser"]);
				});
			}
		}

		public void TestDelete()
		{
			string insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate1', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate2', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate3', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('83952AAE-8696-4BE2-9AC8-47DBB4D9902A', 'TestTemplate4', 'Consol', null, 1, 0, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Test Doc', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', 'Test Doc2', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('A3DC2E3A-AD00-4F11-932B-4C5D1D8E6345', 'DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', '83952AAE-8696-4BE2-9AC8-47DBB4D9902A', 'Test Title2', 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('4DCC8904-8A65-415E-98FF-1F23B8DC9A00', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Test Title', 1);";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new DataFileForTest();

			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");

			AssertEquals("Table Count", 3, data.Tables.Count);

			AssertEquals("StmMenuItem row count", 1, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("Menu Name", "Test Doc", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());

			AssertEquals("StmTemplate row count", 3, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("Template1 Name", "TestTemplate1", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());
			AssertEquals("Template2 Name", "TestTemplate2", data.Tables["StmTemplate"].Rows[1]["SO_Name"].ToString());
			AssertEquals("Template3 Name", "TestTemplate3", data.Tables["StmTemplate"].Rows[2]["SO_Name"].ToString());

			AssertEquals("StmMenuTemplatePivot row count", 1, data.Tables["StmMenuTemplatePivot"].Rows.Count);
			AssertEquals("Pivot Title", "Test Title", data.Tables["StmMenuTemplatePivot"].Rows[0]["SI_DocumentTitle"].ToString());
		}

		public void TestNothingUpdatingNothing()
		{
			var fResourceFile = new DataFileForTest(TestEmptyDataFileRelativePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");

			AssertEquals("Table Count", 3, data.Tables.Count);

			AssertEquals("StmMenuItem row count", 0, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("StmTemplate row count", 0, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("StmMenuTemplatePivot row count", 0, data.Tables["StmMenuTemplatePivot"].Rows.Count);
		}

		public void TestNothingUpdatingSomething()
		{
			var insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate1', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate2', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate3', 'Consol', null, 1, 0, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Test Doc', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle) VALUES('4DCC8904-8A65-415E-98FF-1F23B8DC9A00', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Test Title');";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new DataFileForTest(TestEmptyDataFileRelativePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			AssertEquals("Table Count", 3, data.Tables.Count);

			AssertEquals("StmMenuItem row count", 0, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("StmTemplate row count", 0, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("StmMenuTemplatePivot row count", 0, data.Tables["StmMenuTemplatePivot"].Rows.Count);
		}

		public void TestMixOfUpdateInsertAndDeletes()
		{
			var insertSql = @"
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('59F9EB02-4B8D-4A50-9A5B-18119D7C8380', 'TestTemplate1', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('7D6292B9-B4B3-4D51-A43C-88E66981F356', 'TestTemplate2', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('63221015-496D-4839-A42D-8A5ACB61422D', 'TestTemplate3', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('83952AAE-8696-4BE2-9AC8-47DBB4D9902A', 'TestTemplate4', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('8B8547C8-879A-45AE-BEEF-23D1B12C09B6', 'TestTemplate5', 'Consol', null, 1, 0, null, '');
				INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction) VALUES('C51C66DC-B779-46F8-B4E5-F1B6076D2E32', 'TestTemplate6', 'Consol', null, 1, 0, null, '');

				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', 'Test Doc', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', 'Test Doc2', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('EC7B5C5D-8A6B-45F6-B965-0B08C117A0F5', 'Test Doc3', 'Consol', 'CNE', 1, 1, 0, '', 'None', 0, '', '', '');

				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('A3DC2E3A-AD00-4F11-932B-4C5D1D8E6345', 'DA9FDF5F-F0C9-4F56-9FA4-8E1DD3E8345E', '83952AAE-8696-4BE2-9AC8-47DBB4D9902A', 'Test Title2', 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('4E7C38C2-EB57-4AC7-9497-EE66AD1CF66E', 'EC7B5C5D-8A6B-45F6-B965-0B08C117A0F5', '8B8547C8-879A-45AE-BEEF-23D1B12C09B6', 'Test Title3', 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('A93DD396-D92C-4662-9805-4C05CB0A5E62', 'EC7B5C5D-8A6B-45F6-B965-0B08C117A0F5', '8B8547C8-879A-45AE-BEEF-23D1B12C09B6', 'Test Title4', 1);
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined) VALUES('4DCC8904-8A65-415E-98FF-1F23B8DC9A00', '3BDE60A3-011E-46E9-A82C-B94D4F6FE4E0', '7D6292B9-B4B3-4D51-A43C-88E66981F356', 'Test Title', 1);";

			Db.Connection.ExecuteNonQuery(insertSql);

			var fResourceFile = new DataFileForTest(TestMixedUpdateDataFileRelativePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");

			var task = new EmbeddedUpgradeTask(fResourceFile);
			task.Run();

			var data = RetrieveData("StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");

			AssertEquals("Table Count", 3, data.Tables.Count);

			AssertEquals("StmMenuItem row count", 3, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("Menu Name", "Test Doc2 changed", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());
			AssertEquals("Menu Name", "Test Doc2", data.Tables["StmMenuItem"].Rows[1]["SU_MenuName"].ToString());
			AssertEquals("Menu Name", "Test Doc", data.Tables["StmMenuItem"].Rows[2]["SU_MenuName"].ToString());

			AssertEquals("StmTemplate row count", 5, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("Template0 Name", "TestTemplate111", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());
			AssertEquals("Template1 Name", "TestTemplate41", data.Tables["StmTemplate"].Rows[1]["SO_Name"].ToString());
			AssertEquals("Template2 Name", "TestTemplate4", data.Tables["StmTemplate"].Rows[2]["SO_Name"].ToString());
			AssertEquals("Template3 Name", "TestTemplate2", data.Tables["StmTemplate"].Rows[3]["SO_Name"].ToString());
			AssertEquals("Template4 Name", "TestTemplate42", data.Tables["StmTemplate"].Rows[4]["SO_Name"].ToString());

			AssertEquals("StmMenuTemplatePivot row count", 4, data.Tables["StmMenuTemplatePivot"].Rows.Count);
			AssertEquals("Pivot 0", "Test Title", data.Tables["StmMenuTemplatePivot"].Rows[0]["SI_DocumentTitle"].ToString());
			AssertEquals("Pivot 1", "Test Title2", data.Tables["StmMenuTemplatePivot"].Rows[1]["SI_DocumentTitle"].ToString());
			AssertEquals("Pivot 2", "Test Title4 new", data.Tables["StmMenuTemplatePivot"].Rows[2]["SI_DocumentTitle"].ToString());
			AssertEquals("Pivot 3", "Test Title3", data.Tables["StmMenuTemplatePivot"].Rows[3]["SI_DocumentTitle"].ToString());
		}

		/// <summary>
		/// If versions differ, upgrade is required (either upgrade or downgrade)
		/// </summary>
		public void TestIsRequired()
		{
			var fResourceFile = new DataFileForTest();
			AssertEquals("VersionInAssembly", 0, fResourceFile.Version);
			AssertEquals("Version in Database", 0, fResourceFile.VersionInDatabase);

			var task = new EmbeddedUpgradeTask(fResourceFile);
			AssertEquals("IsRequired", false, task.IsRequired);

			try
			{
				fResourceFile.VersionInDatabase = -1;
				AssertEquals("IsRequired", true, task.IsRequired);

				fResourceFile.VersionInDatabase = 1;
				AssertEquals("IsRequired", true, task.IsRequired);
			}
			finally
			{
				fResourceFile.VersionInDatabase = 0;
			}
		}

		public void TestVersionInDatabaseUpdated()
		{
			var fResourceFile = new DataFileForTest();

			AssertEquals("Version in Database", 0, fResourceFile.VersionInDatabase);
			AssertEquals("VersionInAssembly", 0, fResourceFile.Version);

			try
			{
				fResourceFile.VersionInDatabase = -1;

				var task = new EmbeddedUpgradeTask(fResourceFile);
				task.Run();

				AssertEquals("VersionInDatabase", 0, fResourceFile.VersionInDatabase);
			}
			finally
			{
				fResourceFile.VersionInDatabase = 0;
			}
		}

		public void TestTaskName()
		{
			var fResourceFile = new DataFileForTest();
			var task = new EmbeddedUpgradeTask(fResourceFile);

			AssertEquals("TaskNameWhenUpgrading", "Updating " + task.TableNames + " from version 0 to 0.", task.TaskNameWhenUpgrading);
		}

		[ExpectNoExceptions()]
		public void TestCanStillWorkWhenAColumnIsDropped()
		{
			var fResourceFile = new DataFileForTest();
			var task = new EmbeddedUpgradeTask(fResourceFile);

			var sqlString = "alter table dbo.stmmenuitem drop constraint " + GetColumnDefaultConstraintName("stmmenuitem", "SU_DocumentDirection") + @";
								alter table dbo.stmmenuitem drop column SU_DocumentDirection";

			Db.Connection.ExecuteNonQuery(sqlString);
			task.Run();
		}

		string GetColumnDefaultConstraintName(string tableName, string columnName)
		{
			var sqlString = string.Format(@"SELECT
	constobj.name ConstName
FROM
	sys.columns col
	INNER JOIN sys.tables tab ON tab.object_id = col.object_id
	INNER JOIN sys.default_constraints constobj
		ON constobj.parent_object_id = tab.object_id AND constobj.parent_column_id = col.column_id
WHERE
	tab.name = '{0}'
	AND col.name = '{1}'
", tableName, columnName);

			return (string)Db.Connection.ExecuteScalar(sqlString);
		}

		[ExpectNoExceptions()]
		public void TestCanStillWorkWhenAColumnIsAdded()
		{
			var fResourceFile = new DataFileForTest();
			var task = new EmbeddedUpgradeTask(fResourceFile);

			var sqlString = @"ALTER TABLE dbo.StmMenuTemplatePivot ADD SI_NewIndex numeric(18, 0) NOT NULL CONSTRAINT DF_StmMenuTemplatePivot_SI_Index DEFAULT 0";
			Db.Connection.ExecuteNonQuery(sqlString);

			var originalServiceProvider = GlobalServiceProvider.Instance;
			var originalResolver = originalServiceProvider.GetRequiredService<IApplicationSchemaResolver>();
			var mockResolver = new Mock<IApplicationSchemaResolver>(MockBehavior.Strict);

			var serviceProvider = new Mock<IServiceProvider>();
			serviceProvider.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns<Type>(t => originalServiceProvider.GetService(t));
			serviceProvider.Setup(sp => sp.GetService(typeof(IApplicationSchemaResolver))).Returns(mockResolver.Object);

			Func<string, string, SchemaColumn> getSchemaColumn = delegate(string columnName, string tableName)
			{
				return originalResolver.GetSchemaColumnSafe(columnName, tableName);
			};

			mockResolver.Setup(x => x.GetSchemaColumnSafe(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			mockResolver.Setup(x => x.GetSchemaColumn(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			mockResolver.Setup(x => x.GetTableSchemaFromColumnNamePrefix(It.IsAny<string>())).Returns<string>(prefix => originalResolver.GetTableSchemaFromColumnNamePrefix(prefix));
			mockResolver.Setup(x => x.GetPkColumn(It.IsAny<string>())).Returns<string>(tableName => originalResolver.GetPkColumn(tableName));

			mockResolver.Setup(x => x.GetSchemaColumns(It.IsAny<string>())).Returns<string>(tableName =>
			{
				return originalResolver.GetSchemaColumns(tableName);
			});

			mockResolver.Setup(x => x.GetTableSchema(It.IsAny<string>())).Returns<string>(tableName =>
			{
				if (tableName == "StmMenuTemplatePivot")
				{
					return StmMenuTemplatePivotForTestSchema.Instance;
				}
				return originalResolver.GetTableSchema(tableName);
			});

			using (GlobalServiceProvider.Configure(serviceProvider.Object))
			{
				task.Run();
			}
		}

		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class StmMenuTemplatePivotForTestSchema : Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline")]
			static StmMenuTemplatePivotForTestSchema()
			{
				var column = 0;
				Instance = new StmMenuTemplatePivotForTestSchema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
				SI_DataStoreName = new SchemaStringColumn(Instance, Constants.SI_DataStoreName, column++, SqlDbType.VarChar, "", !IsNullable, 127, false, false, CargoWise.Database.Shared.TVPHelper.TVP_varchar);
				SI_DocumentTitle = new SchemaStringColumn(Instance, Constants.SI_DocumentTitle, column++, SqlDbType.VarChar, "", !IsNullable, 256, false, false, "");
				SI_Index = new SchemaByteColumn(Instance, Constants.SI_Index, column++, (byte)0, !IsNullable, false, "dbo.TVP_tinyint");
				SI_IsClientSpecific = new SchemaBoolColumn(Instance, Constants.SI_IsClientSpecific, column++, false, true, false, "");
				SI_IsPasswordProtected = new SchemaBoolColumn(Instance, Constants.SI_IsPasswordProtected, column++, false, true, false, "");
				SI_IsPasswordProtectedForOpening = new SchemaBoolColumn(Instance, Constants.SI_IsPasswordProtectedForOpening, column++, false, true, false, "");
				SI_IsSystemDefined = new SchemaBoolColumn(Instance, Constants.SI_IsSystemDefined, column++, false, true, false, "");
				SI_MenuTemplateFilter = new SchemaStringColumn(Instance, Constants.SI_MenuTemplateFilter, column++, SqlDbType.VarChar, "", !IsNullable, 1024, false, false, "");
				SI_PrintByDefault = new SchemaBoolColumn(Instance, Constants.SI_PrintByDefault, column++, true, true, false, "");
				SI_PrintCopyType = new SchemaStringColumn(Instance, Constants.SI_PrintCopyType, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false, CargoWise.Database.Shared.TVPHelper.TVP_varchar);
				SI_RT_DocType = new SchemaGuidColumn(Instance, Constants.SI_RT_DocType, column++, DBNull.Value, IsNullable, false, "dbo.TVP_uniqueidentifier", true);
				SI_SO = new SchemaGuidColumn(Instance, Constants.SI_SO, column++, Guid.Empty, !IsNullable, false, "dbo.TVP_uniqueidentifier", true);
				SI_SU = new SchemaGuidColumn(Instance, Constants.SI_SU, column++, Guid.Empty, !IsNullable, false, "dbo.TVP_uniqueidentifier", true);
				SI_SystemCreateTimeUtc = new SchemaDateTimeColumn(Instance, Constants.SI_SystemCreateTimeUtc, column++, SqlDbType.SmallDateTime, DBNull.Value, IsNullable, false, "dbo.TVP_smalldatetime");
				SI_SystemCreateUser = new SchemaStringColumn(Instance, Constants.SI_SystemCreateUser, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false, CargoWise.Database.Shared.TVPHelper.TVP_varchar);
				SI_SystemLastEditTimeUtc = new SchemaDateTimeColumn(Instance, Constants.SI_SystemLastEditTimeUtc, column++, SqlDbType.SmallDateTime, DBNull.Value, IsNullable, false, "dbo.TVP_smalldatetime");
				SI_SystemLastEditUser = new SchemaStringColumn(Instance, Constants.SI_SystemLastEditUser, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false, CargoWise.Database.Shared.TVPHelper.TVP_varchar);
				SI_TrailingLines = new SchemaShortColumn(Instance, Constants.SI_TrailingLines, column++, (short)0, !IsNullable, false, "dbo.TVP_smallint");
				SI_NewIndex = new SchemaIntColumn(Instance, Constants.SI_NewIndex, 100, 0, false);
			}

			StmMenuTemplatePivotForTestSchema()
			{
			}

			#region Constants

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string TableName = "StmMenuTemplatePivot";
				public const string Prefix = "SI";
				public const string PK = "SI_PK";

				public const string SI_DataStoreName = "SI_DataStoreName";
				public const string SI_DocumentTitle = "SI_DocumentTitle";
				public const string SI_Index = "SI_Index";
				public const string SI_IsClientSpecific = "SI_IsClientSpecific";
				public const string SI_IsPasswordProtected = "SI_IsPasswordProtected";
				public const string SI_IsPasswordProtectedForOpening = "SI_IsPasswordProtectedForOpening";
				public const string SI_IsSystemDefined = "SI_IsSystemDefined";
				public const string SI_MenuTemplateFilter = "SI_MenuTemplateFilter";
				public const string SI_PrintByDefault = "SI_PrintByDefault";
				public const string SI_PrintCopyType = "SI_PrintCopyType";
				public const string SI_RT_DocType = "SI_RT_DocType";
				public const string SI_SO = "SI_SO";
				public const string SI_SU = "SI_SU";
				public const string SI_SystemCreateTimeUtc = "SI_SystemCreateTimeUtc";
				public const string SI_SystemCreateUser = "SI_SystemCreateUser";
				public const string SI_SystemLastEditTimeUtc = "SI_SystemLastEditTimeUtc";
				public const string SI_SystemLastEditUser = "SI_SystemLastEditUser";
				public const string SI_TrailingLines = "SI_TrailingLines";
				public const string SI_NewIndex = "SI_NewIndex";

				#region Indexes

				public const string PkIndex = "PK_UX__SI_PK";

				#endregion // Indexes
			}

			#endregion

			#region All

			public static SchemaColumnCollection All
			{
				get { return AllHolder.all; }
			}

			class AllHolder
			{
				static AllHolder()
				{
					// Empty constructor to prevent initialisation until first member access.
				}

				public static readonly SchemaColumnCollection all = new SchemaColumnCollection(PK, new SchemaColumn[]
				{
					SI_DataStoreName,
					SI_DocumentTitle,
					SI_Index,
					SI_IsClientSpecific,
					SI_IsPasswordProtected,
					SI_IsPasswordProtectedForOpening,
					SI_IsSystemDefined,
					SI_MenuTemplateFilter,
					SI_PrintByDefault,
					SI_PrintCopyType,
					SI_RT_DocType,
					SI_SO,
					SI_SU,
					SI_SystemCreateTimeUtc,
					SI_SystemCreateUser,
					SI_SystemLastEditTimeUtc,
					SI_SystemLastEditUser,
					SI_TrailingLines,
					SI_NewIndex,
				});
			}

			#endregion

			#region PK

			public static readonly SchemaPKColumn PK;

			#endregion

			public static readonly SchemaStringColumn SI_DataStoreName;
			public static readonly SchemaStringColumn SI_DocumentTitle;
			public static readonly SchemaByteColumn SI_Index;
			public static readonly SchemaBoolColumn SI_IsClientSpecific;
			public static readonly SchemaBoolColumn SI_IsPasswordProtected;
			public static readonly SchemaBoolColumn SI_IsPasswordProtectedForOpening;
			public static readonly SchemaBoolColumn SI_IsSystemDefined;
			public static readonly SchemaStringColumn SI_MenuTemplateFilter;
			public static readonly SchemaBoolColumn SI_PrintByDefault;
			public static readonly SchemaStringColumn SI_PrintCopyType;
			public static readonly SchemaGuidColumn SI_RT_DocType;
			public static readonly SchemaGuidColumn SI_SO;
			public static readonly SchemaGuidColumn SI_SU;
			public static readonly SchemaDateTimeColumn SI_SystemCreateTimeUtc;
			public static readonly SchemaStringColumn SI_SystemCreateUser;
			public static readonly SchemaDateTimeColumn SI_SystemLastEditTimeUtc;
			public static readonly SchemaStringColumn SI_SystemLastEditUser;
			public static readonly SchemaShortColumn SI_TrailingLines;
			public static readonly SchemaIntColumn SI_NewIndex;

			#region GetSchemaColumn(ColumnName)

			internal static SchemaColumn GetSchemaColumn(string columnName)
			{
				switch (columnName)
				{
					case Constants.PK:
						return PK;

					case Constants.SI_DataStoreName:
						return SI_DataStoreName;
					case Constants.SI_DocumentTitle:
						return SI_DocumentTitle;
					case Constants.SI_Index:
						return SI_Index;
					case Constants.SI_IsClientSpecific:
						return SI_IsClientSpecific;
					case Constants.SI_IsPasswordProtected:
						return SI_IsPasswordProtected;
					case Constants.SI_IsPasswordProtectedForOpening:
						return SI_IsPasswordProtectedForOpening;
					case Constants.SI_IsSystemDefined:
						return SI_IsSystemDefined;
					case Constants.SI_MenuTemplateFilter:
						return SI_MenuTemplateFilter;
					case Constants.SI_PrintByDefault:
						return SI_PrintByDefault;
					case Constants.SI_PrintCopyType:
						return SI_PrintCopyType;
					case Constants.SI_RT_DocType:
						return SI_RT_DocType;
					case Constants.SI_SO:
						return SI_SO;
					case Constants.SI_SU:
						return SI_SU;
					case Constants.SI_SystemCreateTimeUtc:
						return SI_SystemCreateTimeUtc;
					case Constants.SI_SystemCreateUser:
						return SI_SystemCreateUser;
					case Constants.SI_SystemLastEditTimeUtc:
						return SI_SystemLastEditTimeUtc;
					case Constants.SI_SystemLastEditUser:
						return SI_SystemLastEditUser;
					case Constants.SI_TrailingLines:
						return SI_TrailingLines;
					case Constants.SI_NewIndex:
						return SI_NewIndex;

					default:
						return null;
				}
			}

			#endregion

			#region ITableSchema

			public static readonly StmMenuTemplatePivotForTestSchema Instance;

			string ITableSchema.SqlSchemaName => Constants.SqlSchemaName;

			string ITableSchema.TableName => Constants.TableName;

			SchemaPKColumn ITableSchema.PK => PK;

			string ITableSchema.PkIndexName => Constants.PkIndex;

			SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
			{
				return GetSchemaColumn(columnName);
			}

			SchemaColumnCollection ITableSchema.All => All;

			#endregion
		}

		public void TestComparisonIgnoresCharCase()
		{
			string sqlText = "INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext) VALUES (NEWID(), 'tEsTtEmPlAtE1', '~OldDataContext~')";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = "SELECT count(*) FROM dbo.StmTemplate WHERE SO_Name = 'TestTemplate1'";
			int rowCount = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("[PRE-CONDITION] Only 1 TestTemplate1 record", 1, rowCount);

			sqlText = "SELECT SO_DataContext FROM dbo.StmTemplate WHERE SO_Name = 'TestTemplate1'";
			string actualDataContext = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("[PRE-CONDITION] Old DataContext Value", "~OldDataContext~", actualDataContext);

			UpgradeTaskForCharCaseComparisonTest task = new UpgradeTaskForCharCaseComparisonTest();
			task.Run();

			sqlText = "SELECT count(*) FROM dbo.StmTemplate WHERE SO_DataContext = '~OldDataContext~'";
			rowCount = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("Data Context should have been updated. Old DataContext count = ", 0, rowCount);

			sqlText = "SELECT SO_DataContext FROM dbo.StmTemplate WHERE SO_Name = 'TestTemplate1' COLLATE SQL_Latin1_General_CP1_CS_AS";
			actualDataContext = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Data Context should have been updated. New DataContext value = ", UpgradeTaskForCharCaseComparisonTest.NewDataContextValue, actualDataContext);

			sqlText = "SELECT SO_PK FROM dbo.StmTemplate WHERE SO_Name = 'TestTemplate1' COLLATE SQL_Latin1_General_CP1_CS_AS";
			Guid actualPk = (Guid)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("PK should have been updated. New PK value = ", new Guid("59f9eb02-4b8d-4a50-9a5b-18119d7c8380"), actualPk);
		}

		#region Implementation

		const string TestEmptyDataFileRelativePath = @"Shared.Test\TestFiles\TestEmptyDataFile.xml";
		const string TestMixedUpdateDataFileRelativePath = @"Shared.Test\TestFiles\TestMixedUpdateDataFile.xml";

		DataSet RetrieveData(params string[] tableNames)
		{
			DataFile tempFile = new EmbeddedDataFile("", tableNames);
			return tempFile.LoadDataFromDatabase();
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
		}

		#endregion
	}
}
