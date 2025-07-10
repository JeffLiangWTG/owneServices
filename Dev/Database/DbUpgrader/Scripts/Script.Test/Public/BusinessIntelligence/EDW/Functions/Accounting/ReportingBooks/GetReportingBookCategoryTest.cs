using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetReportingBookCategory))]
	internal class GetReportingBookCategoryTest : BiCreateScriptTest
	{
		public void TestRun()
		{
			PrepareData();
			var companyPK = Guid.NewGuid();
			var resultsTable = Execute(companyPK, ReportingBookPK1);
			AssertEquals(1, resultsTable.Rows.Count);
			AssertEquals(1, resultsTable.Select("IncludePresentationJournals='OUT'").Length);
			resultsTable = Execute(companyPK, ReportingBookPK2);
			AssertEquals(1, resultsTable.Select("IncludePresentationJournals='EEY'").Length);
			resultsTable = Execute(companyPK, ReportingBookPK3);
			AssertEquals(2, resultsTable.Select("IncludePresentationJournals in ('OUT', 'EET')").Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareData()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, $@"
			DECLARE @registryRawValue nvarchar(MAX)
					Set @registryRawValue = '<?xml version=""1.0"" encoding=""utf-16""?><GLPresentationJournalCategoryCollection xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
						<GLPresentationJournalCategory>
							<CodeMaxLength>3</CodeMaxLength>
							<Code>EET</Code>
							<Description>Inter Company</Description>
							<Bool>Y</Bool>
							<Bool2>N</Bool2>
							<Bool3>Y</Bool3>
							<Bool4>Y</Bool4>
							<ParentCode>OUT</ParentCode>
						</GLPresentationJournalCategory>
					</GLPresentationJournalCategoryCollection>'

					INSERT INTO [{ScriptDbName}].Finance.BAS__AccountStmData
						(AccountStmDataID, AccountStmDataKey, BinaryValue, OwnerID, SDName)
					VALUES
						(NEWID(), 1, CONVERT(varbinary(MAX), CONVERT(XML, @registryRawValue)), null, 'GLJournalAdjustmentCategoriesList');
					UPDATE [{ScriptDbName}].Finance.BAS__AccReportingBook SET IncludeChildPresentation = 1
			");
			TestConnection.ExecuteNonQuery(sql);
			TestHelper.InsertALternateChart("UUU", isGlobal: 1);
			ReportingBookPK1 = TestHelper.InsertReportingBook(1, 1, presentationJournals: "OUT");
			ReportingBookPK2 = TestHelper.InsertReportingBook(2, 2, "YYY", currency: "USD", presentationJournals: "EEY");
			ReportingBookPK3 = TestHelper.InsertReportingBook(3, 3, "ZZZ", currency: "USD", presentationJournals: "OUT", includeChildPresentation: 1);
		}

		DataTable Execute(Guid companyPK, Guid reportingBookPK)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].dbo.GetReportingBookCategory('{companyPK}', '{reportingBookPK}')");
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;

		Guid ReportingBookPK1, ReportingBookPK2, ReportingBookPK3;
	}
}
