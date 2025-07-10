using System.IO;
using System.Xml;
using CargoWise.Data;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ClientDocumentsUpgradeTaskTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateVersionNumber()
		{
			using (var directory = CopyLocal(TestFileConstants.TestDataFileFullRelativePath, out var filePath))
			{
				var task = new ClientDocumentsUpgradeTask(filePath);
				task.Run();
				AssertEquals("ClientDocumentName", "TestDataFile", DbRegistry.ClientDocumentName.LoadValue(TestConnection));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteSystemClientMenuItemWithPivots()
		{
			using (var directory = CopyLocal(Path.Combine(TestFileConstants.DefaultTestDataFileBasePath, TestClientDocumentsRelativePath), out var filePath))
			{
				// Insert menu items and pivots that should be deleted by the upgrade process
				var clientDataSQL = @"
					INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_IsSystemDefined, SU_IsClientSpecific)
					VALUES('51E9FE6F-284A-4378-B71C-972C844E5DB8', 'ClientMenu', 'Consol', 1, 1);

					INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_IsSystemDefined, SO_IsClientSpecific)
					VALUES('6FAEB320-9FAA-449E-ACF1-47CF180D73E9', 'SysTemplate', 'Consol', 1, 1);

					INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific)
					VALUES('96ED7553-FC0D-4B8E-95E1-240CBFDF7F14', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '6FAEB320-9FAA-449E-ACF1-47CF180D73E9', 'Client Template Pivot 1', 1, 1);
					INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific)
					VALUES('184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '6FAEB320-9FAA-449E-ACF1-47CF180D73E9', 'Client Template Pivot 2', 1, 1);

					INSERT dbo.StmMenuMenuPivot (SF_PK, SF_SU_Outward, SF_SU_Inward, SF_IsClientSpecific, SF_IsSystemDefined)
					VALUES ('267315B4-6390-40D4-93D4-18BCD2A0BD49', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '51E9FE6F-284A-4378-B71C-972C844E5DB8', 1, 1)
				";
				Db.Connection.ExecuteNonQuery(clientDataSQL);

				var task = new ClientDocumentsUpgradeTask(filePath);
				task.Run();

				AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuItem WHERE SU_PK = '51E9FE6F-284A-4378-B71C-972C844E5DB8'"));
				AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = '6FAEB320-9FAA-449E-ACF1-47CF180D73E9'"));
				AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '96ED7553-FC0D-4B8E-95E1-240CBFDF7F14'"));
				AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91'"));
				AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuMenuPivot WHERE SF_PK = '267315B4-6390-40D4-93D4-18BCD2A0BD49'"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoNotDeleteNonSystemClientConfig()
		{
			using (var directory = CopyLocal(Path.Combine(TestFileConstants.DefaultTestDataFileBasePath, TestClientDocumentsRelativePath), out var filePath))
			{
				//Create some client data here. Should not be removed by the upgrade process.
				var clientDataSQL = @"
					INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection)
					VALUES('51E9FE6F-284A-4378-B71C-972C844E5DB8', 'ClientMenu', 'Consol', '', 1, 0, 0, '', 'None', 0, '', '', '');

					INSERT dbo.StmTemplate (SO_PK, SO_Name, SO_DataContext, SO_Template, SO_IsSystemDefined, SO_IsClientSpecific, SO_UDFFieldCache, SO_TemplateRestriction)
					VALUES('6FAEB320-9FAA-449E-ACF1-47CF180D73E9', 'SysTemplate', 'Consol', null, 1, 0, null, '');

					INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific)
					VALUES('96ED7553-FC0D-4B8E-95E1-240CBFDF7F14', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '6FAEB320-9FAA-449E-ACF1-47CF180D73E9', 'Client Template Pivot 1', 0, 0);
					INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_DocumentTitle, SI_IsSystemDefined, SI_IsClientSpecific)
					VALUES('184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91', '51E9FE6F-284A-4378-B71C-972C844E5DB8', '6FAEB320-9FAA-449E-ACF1-47CF180D73E9', 'Client Template Pivot 2', 0, 0);

					INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI, S3_Description) VALUES ('549C36EA-F69D-46CA-BA47-ACEC7251178C', 0, 1, '96ED7553-FC0D-4B8E-95E1-240CBFDF7F14', 'Default');
					INSERT dbo.StmMenuDocumentConfig (S3_PK, S3_IsSystem, S3_IsClientSpecific, S3_SI, S3_Description) VALUES ('D8ACAA9E-9678-436F-B6F6-9EDC9F2AFD3E', 0, 0, '184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91', 'Default');

					INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('E9132BA6-AA46-49A3-8EE4-E960D784579F', '549C36EA-F69D-46CA-BA47-ACEC7251178C', 0, 1);
					INSERT dbo.StmMenuDocumentConfigItem (S4_PK, S4_S3, S4_IsSystemDefined, S4_IsClientSpecific) VALUES ('56E80516-8A61-4663-AE98-A78ED5FB06E0', 'D8ACAA9E-9678-436F-B6F6-9EDC9F2AFD3E', 0, 0);
				";
				Db.Connection.ExecuteNonQuery(clientDataSQL);

				var task = new ClientDocumentsUpgradeTask(filePath);
				task.Run();

				AssertEquals("SystemDocuments template should be there", 1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = '454d1f87-dbbc-4b0b-8b34-7c695d9fbead'"));

				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuItem WHERE SU_PK = '51E9FE6F-284A-4378-B71C-972C844E5DB8'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = '6FAEB320-9FAA-449E-ACF1-47CF180D73E9'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '96ED7553-FC0D-4B8E-95E1-240CBFDF7F14'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '184DEBCF-C4C9-44E3-B8C7-6AF4D0313F91'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK = '549C36EA-F69D-46CA-BA47-ACEC7251178C'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK = 'D8ACAA9E-9678-436F-B6F6-9EDC9F2AFD3E'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = 'E9132BA6-AA46-49A3-8EE4-E960D784579F'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = '56E80516-8A61-4663-AE98-A78ED5FB06E0'"));

				//Check StmMenuDocumentConfig and StmMenuDocumentConfigItem data from TestClientDocuments.xml are still there
				AssertEquals(GetNodeCount(filePath, "//S3_PK"), (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK in ('fd2f53f9-60ab-4c23-88cc-29e64c0ec2ff', '6135a117-21d7-440e-8612-3b0c579af757', '73877c5d-263c-45bf-8c6d-7fea48de2fe7', '9bda3e63-5618-4f16-aee4-ecfd38376362')"));
				AssertEquals(GetNodeCount(filePath, "//S4_PK"), (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_S3 in ('fd2f53f9-60ab-4c23-88cc-29e64c0ec2ff', '6135a117-21d7-440e-8612-3b0c579af757', '73877c5d-263c-45bf-8c6d-7fea48de2fe7', '9bda3e63-5618-4f16-aee4-ecfd38376362')"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClientDocumentUpgradeCanBeRunTwiceInARow()
		{
			using (var directory = CopyLocal(Path.Combine(TestFileConstants.DefaultTestDataFileBasePath, TestClientDocumentsRelativePath), out var filePath))
			{
				var task = new ClientDocumentsUpgradeTask(filePath);
				task.Run();
				task.Run();

				AssertEquals("SystemDocuments template should be there", 1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmTemplate WHERE SO_PK = '454d1f87-dbbc-4b0b-8b34-7c695d9fbead'"));

				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuItem WHERE SU_PK = 'da1a261f-033c-480a-8ad7-0acca50633c1'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuItem WHERE SU_PK = '245e7617-1516-4f2e-bb56-ac20ff546c34'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '87b24c11-6631-4233-a8c9-fa6c9c78051f'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuTemplatePivot WHERE SI_PK = '1a184212-d7ec-49d4-b5a9-c2d4f3f30592'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK = 'fd2f53f9-60ab-4c23-88cc-29e64c0ec2ff'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK = '6135a117-21d7-440e-8612-3b0c579af757'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = '1a2579d4-eb6f-45f2-8cd5-025062c36e06'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = '1a9ab2cc-7f9d-419e-943c-03fb4d0669d5'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = '5726f218-2a2f-43f5-ab07-166b69b8354d'"));
				AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_PK = '012b5491-2dde-4a2e-9e13-167ab44a6795'"));

				//Check StmMenuDocumentConfig and StmMenuDocumentConfigItem data from TestClientDocuments.xml are still there
				AssertEquals(GetNodeCount(filePath, "//S3_PK"), (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfig WHERE S3_PK in ('fd2f53f9-60ab-4c23-88cc-29e64c0ec2ff', '6135a117-21d7-440e-8612-3b0c579af757', '73877c5d-263c-45bf-8c6d-7fea48de2fe7', '9bda3e63-5618-4f16-aee4-ecfd38376362')"));
				AssertEquals(GetNodeCount(filePath, "//S4_PK"), (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmMenuDocumentConfigItem WHERE S4_S3 in ('fd2f53f9-60ab-4c23-88cc-29e64c0ec2ff', '6135a117-21d7-440e-8612-3b0c579af757', '73877c5d-263c-45bf-8c6d-7fea48de2fe7', '9bda3e63-5618-4f16-aee4-ecfd38376362')"));
			}
		}

		int GetNodeCount(string documentPath, string nodePath)
		{
			var xml = new XmlDocument();
			xml.Load(documentPath);
			return xml.SelectNodes(nodePath).Count;
		}

		static TempDirectory CopyLocal(string relativeFilePath, out string tempFilePath)
		{
			var result = new TempDirectory();

			tempFilePath = Path.Combine(result, Path.GetFileName(relativeFilePath));
			var targetDirectory = Path.GetDirectoryName(tempFilePath);

			Directory.CreateDirectory(targetDirectory);
			File.Copy(Path.Combine(BaseSourcePath, relativeFilePath), tempFilePath);
			File.SetAttributes(tempFilePath, FileAttributes.Normal);
			return result;
		}

		const string TestClientDocumentsRelativePath = @"Shared.Test\TestFiles\TestClientDocuments.xml";
	}
}
