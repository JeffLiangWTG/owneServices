using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Data;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class DocumentsDataFileNoTableCleanupTest : TransactionedTestCase
	{
		protected virtual DocumentsDataFile GetDocumentsDataFile()
		{
			return new DocumentsDataFile();
		}

		public void TestRT_ReferenceTypeRT_DocTypeIsRefDocTypeNk()
		{
			// Attempts to insert 2 rows with same RT_ReferenceType - RT_DocType pair
			string sqlText = @"
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('2FA94C77-47A3-452E-BA98-E87E14A4FC6F', 'XXX', 'YYY', 'User Type 1', 1, 1);
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('1E607E4A-8E5A-4DCE-8D2A-114F2F3F793C', 'XXX', 'YYY', 'User Type 2', 0, 0);";

			try
			{
				Db.Connection.ExecuteNonQuery(sqlText);
				Fail("Should have thrown an SqlException");
			}
			catch (SqlException e)
			{
				AssertEquals("Wrong exception caught - " + e.Message, DbErrorType.CannotInsertDuplicateUniqueIndexKey, new DbErrorHandler(e, Db.Connection).ExceptionType);
				Assert("Message should contain RefDocType table name", e.Message.IndexOf("RefDocType") >= 0);
				Assert("Message should contain violated index NR_UC__RT_ReferenceType_RT_DocType", e.Message.IndexOf("NR_UC__RT_ReferenceType_RT_DocType") >= 0);
			}
			catch (Exception e)
			{
				Fail($"Should have thrown an SqlException, but was another exception instead: {e}");
			}
		}

		internal Guid GetRefDocTypePk(string referenceType, string docType)
		{
			string sqlText = String.Format(
				"SELECT RT_PK FROM dbo.RefDocType WHERE RT_ReferenceType = '{0}' AND RT_DocType = '{1}'",
				referenceType, docType);
			Guid result = (Guid)Db.Connection.ExecuteScalar(sqlText);
			return result;
		}

		internal int GetMenuEDocsRowCount(Guid docTypePk)
		{
			string sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuEDocs WHERE SX_RT_DocType = '{0}'", docTypePk.ToString());
			int result = (int)Db.Connection.ExecuteScalar(sqlText);
			return result;
		}

		public void TestWriteXml()
		{
			string sqlText = @"
INSERT INTO dbo.StmMenuItem
	(SU_PK, SU_BusinessContext, SU_MenuName, SU_IsSystemDefined, SU_IsPublished, SU_GS_NKStaffCode)
VALUES
	(newid(), 'Shipment', 'Denis Test Menu Item 123321', 1, 0, 'E'),
	(newid(), 'RepSalesMgrReports', 'Opportunity Pipeline', 1, 0, 'E')
";
			Db.Connection.ExecuteNonQuery(sqlText);

			DocumentsDataFile dataFile = new DocumentsDataFile();
			var data = dataFile.LoadDataFromDatabase();
			var xml = new XmlDocument();
			using (TempDirectory tempDir = new TempDirectory())
			{
				string tempFilePath = Path.Combine(tempDir.DirectoryName, "TestWriteXml.xml");
				dataFile.WriteXml(data, tempFilePath, XmlWriteMode.WriteSchema);
				xml.Load(tempFilePath);
			}

			AssertEquals("Xml file must contains inserted row", 1, xml.SelectNodes("//StmMenuItem[SU_MenuName='Denis Test Menu Item 123321']").Count);
			Assert("Xml file must contain only rows with SU_IsPublished=true", xml.SelectNodes("//StmMenuItem[SU_IsPublished='true']").Count > 0);

			var unpublishedMenuItems = xml.SelectNodes("//StmMenuItem[SU_IsPublished!='true']");
			var unpublishedMenuItemPks = unpublishedMenuItems.Cast<XmlNode>().Select(x => Guid.Parse(x["SU_PK"].InnerText));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Guid.Parse("7d08c248-6143-42b8-b6fc-232e76101da3"),
					Guid.Parse("d0aae50c-c0b9-40af-99b2-35850ee88c43"),
					Guid.Parse("a79a55e7-82ab-4263-97a1-3d8397d87bf8"),
					Guid.Parse("1dd32ff3-453f-4d2e-bd9d-583f73455847"),
					Guid.Parse("442e2ef3-f05d-49d4-890c-c81177714681")
				},
				unpublishedMenuItemPks);

			AssertEquals("Xml file must contain only rows empty SU_GS_NKStaffCode", 0, xml.SelectNodes("//StmMenuItem[SU_GS_NKStaffCode!='']").Count);
			Assert("Xml file must contain only rows empty SU_GS_NKStaffCode", xml.SelectNodes("//StmMenuItem[SU_GS_NKStaffCode='']").Count > 0);
		}
	}
}
