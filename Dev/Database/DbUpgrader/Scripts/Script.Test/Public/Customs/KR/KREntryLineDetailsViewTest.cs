using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{
	[TestedType(typeof(KREntryLineDetailsView))]
	class KREntryLineDetailsViewTest : DbCreateScriptTest
	{
		public void TestCusEntryLineColumns()
		{
			using (var command = TestConnection.Command("SELECT KEL_LineNumber, KEL_AdValoremTariff, KEL_CustomsValue, KEL_Description, KEL_ValueForVAT, KEL_ClusterKey FROM [dbo].[KREntryLineDetailsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("1", reader["KEL_LineNumber"].ToString());
					AssertEquals("9404210010", reader["KEL_AdValoremTariff"]);
					AssertEquals(1000000m, reader["KEL_CustomsValue"]);
					AssertEquals("Mattress", reader["KEL_Description"]);
					AssertEquals(1000m, reader["KEL_ValueForVAT"]);
					AssertEquals(10, reader["KEL_ClusterKey"]);
				}
			}
		}

		public void TestCusEntryNumColumns()
		{
			using (var command = TestConnection.Command("SELECT KEL_EntryNum, KEL_EntryNumIssueDate FROM [dbo].[KREntryLineDetailsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("6N00224000051U", reader["KEL_EntryNum"]);
					AssertEquals(new DateTime(2024, 01, 02), reader["KEL_EntryNumIssueDate"]);
				}
			}
		}

		public void TestPayerColumn()
		{
			var payerPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@payerPK, 'PAY1', 'Duty Payer 1')
UPDATE dbo.JobDeclaration
SET
	JE_OH_DutyPayer = @payerPK,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@payerPK", SqlDbType.UniqueIdentifier, payerPK);
				cmd.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				cmd.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command("SELECT KEL_OH_DutyPayer FROM [dbo].[KREntryLineDetailsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(payerPK, reader["KEL_OH_DutyPayer"]);
				}
			}
		}

		public void TestInvoiceLineCountColumn()
		{
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK, 10);
			CreateJobComInvoiceLine(invoiceHeaderPK1, 10, entryLinePK);

			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, 10);
			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, 10);
			CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "6N00224000052U", "IMP", "CUS", new DateTime(2024, 01, 03), new DateTime(2023, 03, 04));
			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber" }, new List<object> { 1 });
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK2, 10, entryLineItems2);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 10, entryLinePK2);
			CreateJobComInvoiceLine(invoiceHeaderPK2, 10, entryLinePK2);

			AssertFilter(1, "WHERE KEL_EntryNum = '6N00224000051U'");
			AssertFilter(2, "WHERE KEL_EntryNum = '6N00224000052U'");

			void AssertFilter(int count, string filter)
			{
				using (var command = TestConnection.Command(string.Format("SELECT KEL_InvoiceLineCount FROM [dbo].[KREntryLineDetailsView] {0}", filter)))
				{
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						AssertEquals(count, reader["KEL_InvoiceLineCount"]);
					}
				}
			}
		}

		public void TestUserEnteredFilter()
		{
			var entryLineItems = GetItemList(new List<string> { "CL_LineNumber" }, new List<object> { 2 });
			CreateCusEntryLine(entryHeaderPK, 10, entryLineItems);

			AssertFilter(2, string.Empty);
			AssertFilter(1, "WHERE KEL_LineNumber = 1");
			AssertFilter(1, "WHERE KEL_LineNumber = 2");

			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, 10);
			CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "6N00224000052U", "IMP", "CUS", new DateTime(2024, 01, 03), new DateTime(2023, 03, 04));
			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber" }, new List<object> { 1 });
			CreateCusEntryLine(entryHeaderPK2, 10, entryLineItems2);

			var declarationPK2 = CreateJobDeclaration(11, "EXP", branchPK, companyPK);
			var entryHeaderPK3 = CreateCusEntryHeader(declarationPK, 11);
			CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "6N00224000050U", "EXP", "CUS", new DateTime(2024, 01, 01), new DateTime(2023, 03, 04));
			var entryLineItems3 = GetItemList(new List<string> { "CL_LineNumber" }, new List<object> { 1 });
			CreateCusEntryLine(entryHeaderPK3, 11, entryLineItems3);

			AssertFilter(3, string.Empty);
			AssertFilter(0, "WHERE KEL_EntryNum = '6N00224000050U'");
			AssertFilter(2, "WHERE KEL_EntryNum = '6N00224000051U'");
			AssertFilter(1, "WHERE KEL_EntryNum = '6N00224000052U'");
			AssertFilter(0, "WHERE KEL_EntryNum is null");

			void AssertFilter(int rowCount, string filter)
			{
				using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KREntryLineDetailsView] {0}", filter)))
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
			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
			branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRSEL");

			declarationPK = CreateJobDeclaration(10, "IMP", branchPK, companyPK);
			entryHeaderPK = CreateCusEntryHeader(declarationPK, 10);
			var entryLineItems = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff", "CL_CustomsValue", "CL_Description", "CL_ValueForVAT" },
											 new List<object> { 1, "9404210010", 1000000, "Mattress", 1000 });
			entryLinePK = CreateCusEntryLine(entryHeaderPK, 10, entryLineItems);

			CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "6N00224000051U", "IMP", "CUS", new DateTime(2024, 01, 02), new DateTime(2023, 03, 04));
		}
		Guid companyPK;
		Guid branchPK;
		Guid declarationPK;
		Guid entryHeaderPK;
		Guid entryLinePK;
	}
}
