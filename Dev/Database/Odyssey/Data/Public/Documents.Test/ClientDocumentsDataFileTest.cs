using System;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ClientDocumentsDataFileTest : TransactionedTestCase
	{
		public void TestClientDocumentsDataFile()
		{
			Db.Connection.ExecuteNonQuery(DocumentsDataFileTest.InsertSql);

			ClientDocumentsDataFile docFile = new ClientDocumentsDataFile("");
			var data = docFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 9, data.Tables.Count);

			AssertEquals("RateAttachmentSet", 1, data.Tables["RateAttachmentSet"].Rows.Count);
			AssertEquals("Client Attachment", data.Tables["RateAttachmentSet"].Rows[0]["TS_AttachmentName"].ToString());

			AssertEquals("StmMenuItem row count", 4, data.Tables["StmMenuItem"].Rows.Count);
			DataRow[] menuRows = data.Tables["StmMenuItem"].Select("", "SU_MenuName");
			AssertEquals("Menu Name", "Client Doc", menuRows[0]["SU_MenuName"].ToString());
			AssertEquals("Menu Name", "Client Doc Pack", menuRows[1]["SU_MenuName"].ToString());
			AssertEquals("Menu Name", "Client Doc Pack With Sys Doc", menuRows[2]["SU_MenuName"].ToString());
			AssertEquals("Menu Name", "Client Doc With Sys Template", menuRows[3]["SU_MenuName"].ToString());

			AssertEquals("StmTemplate row count", 1, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("Template1 Name", "ClientTemplate", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());

			AssertEquals("StmMenuTemplatePivot row count", 4, data.Tables["StmMenuTemplatePivot"].Rows.Count);
			DataRow[] pivotRows = data.Tables["StmMenuTemplatePivot"].Select("", "SI_DocumentTitle");

			AssertEquals("Pivot Title", "Client Pivot", pivotRows[0]["SI_DocumentTitle"].ToString());
			AssertEquals("Pivot Title", "Client Pivot With ClientTemp & SysMenu", pivotRows[1]["SI_DocumentTitle"].ToString());
			AssertEquals("Pivot Title", "Client Pivot With Sys Template", pivotRows[2]["SI_DocumentTitle"].ToString());
			AssertEquals("Pivot Title", "Client Pivot With SysTemp & SysMenu", pivotRows[3]["SI_DocumentTitle"].ToString());

			AssertEquals("StmMenuMenuPivot row count", 2, data.Tables["StmMenuMenuPivot"].Rows.Count);
			AssertEquals("Menu Pivot Index Sql Server version: " + Db.Connection.ServerVersionNumber.ToString(), (Int16)2, data.Tables["StmMenuMenuPivot"].Rows[0]["SF_Index"]);
			AssertEquals("Menu Pivot Index Sql Server version: " + Db.Connection.ServerVersionNumber.ToString(), (Int16)3, data.Tables["StmMenuMenuPivot"].Rows[1]["SF_Index"]);

			AssertEquals("StmMenuDocumentConfig row count - should be one", 1, data.Tables["StmMenuDocumentConfig"].Rows.Count);
			AssertEquals("StmMenuDocumentConfigItem row count - should be one", 1, data.Tables["StmMenuDocumentConfigItem"].Rows.Count);

			AssertEquals("StmMenuEDocs row count - should be none, you never get client specific eDocs records", 0, data.Tables["StmMenuEDocs"].Rows.Count);
			AssertEquals("RefDocTypes row count - should be none, you never get client specific RefDocType records", 0, data.Tables["RefDocType"].Rows.Count);
		}

		public void TestClientDocumentVersion()
		{
			ClientDocumentsDataFile docFile = new ClientDocumentsDataFile("");
			docFile.VersionInDatabase = 100;
			AssertEquals("ClientDocumentVersion", 100, DbRegistry.ClientDocumentVersion.LoadValue(TestConnection));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
		}

		#endregion
	}
}
