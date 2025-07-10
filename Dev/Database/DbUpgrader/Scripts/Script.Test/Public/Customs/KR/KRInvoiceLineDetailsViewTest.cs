using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{
	[TestedType(typeof(KRInvoiceLineDetailsView))]
	class KRInvoiceLineDetailsViewTest : DbCreateScriptTest
	{
		public void TestJobComInvoiceLineColumns()
		{
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, clusterKey);
			var invoiceLineAddInfo = new Dictionary<string, string>();
			invoiceLineAddInfo.Add("SequenceNumber", "1");
			var invoiceLineItems = GetItemList(new List<string> { "JI_Tariff", "JI_Model", "JI_InvoiceQuantity", "JI_LinePrice", "JI_AddInfo" },
											 new List<object> { "9404210010", "Model Name", 100, 10, GenerateAddInfoData(invoiceLineAddInfo) });
			CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLinePK, invoiceLineItems);

			using (var command = TestConnection.Command("SELECT KIL_Tariff, KIL_Model, KIL_InvoiceQuantity, KIL_LinePrice, KIL_SequenceNumber FROM [dbo].[KRInvoiceLineDetailsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("9404210010", reader["KIL_Tariff"]);
					AssertEquals("Model Name", reader["KIL_Model"]);
					AssertEquals(100m, reader["KIL_InvoiceQuantity"]);
					AssertEquals(10m, reader["KIL_LinePrice"]);
					AssertEquals(1, reader["KIL_SequenceNumber"]);
				}
			}
		}

		public void TestFilter()
		{
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, clusterKey);
			CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine1PK);
			CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine1PK);
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLinePK: entryLine1PK, dataModel: "US");

			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, clusterKey);
			CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine2PK);
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLinePK: entryLine2PK, dataModel: "CA");

			AssertFilter(3, string.Empty);
			AssertFilter(2, string.Format("WHERE KIL_KEL = '{0}'", entryLine1PK));
			AssertFilter(1, string.Format("WHERE KIL_KEL = '{0}'", entryLine2PK));
			AssertFilter(0, string.Format("WHERE KIL_KEL = '{0}'", Guid.Empty));

			void AssertFilter(int rowCount, string filter)
			{
				using (var command = TestConnection.Command(string.Format("SELECT Count(KIL_PK) AS 'RowCount' FROM [dbo].[KRInvoiceLineDetailsView] JOIN KREntryLineDetailsView on KIL_KEL = KEL_PK {0}", filter)))
				{
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						AssertEquals(rowCount, reader["RowCount"]);
					}
				}
			}
		}

		protected override void SetUp()
		{
			TestDataCreator.CreateRefDatabaseRefDataGrouping("KR", "South Korea");
			var companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRSEL");

			clusterKey = 10;
			var declarationPK = CreateJobDeclaration(clusterKey, "IMP", branchPK, companyPK);
			entryHeaderPK = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "6N00224000052U", "IMP", "CUS", new DateTime(2024, 01, 03), new DateTime(2023, 03, 04));
			invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, clusterKey);
		}
		int clusterKey;
		Guid entryHeaderPK;
		Guid invoiceHeaderPK;
	}
}
