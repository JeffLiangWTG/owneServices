using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(HasValidGeneralLedgerData))]
	sealed class HasValidGeneralLedgerDataTest : ValueProviderWithLoadControlFactoryTest<HasValidGeneralLedgerData>
	{
		[TestDate(2023, 1, 4)]
		public override void TestReplacement()
		{
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(2023);
			var companyPK = GlbCompany.CurrentCompany.PK.ToString();

			AssertEquals("Y", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "202308", "2023-04-09"), Report));

			Report.IsEdwDataSource = true;
			AssertEquals("N", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "202308", "2023-04-09"), Report));

			AssertEquals("N", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro("companyPK", "202308", "2023-04-09"), Report));
			AssertContains("Could not parse Guid", Report.ErrorManager.ToString());

			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(Guid.Parse(companyPK), Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime());
			AssertEquals("Y", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "202308", "2022-01-09"), Report));
			AssertEquals("N", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, null, "2023-01-01"), Report));
			AssertEquals("Y", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "", "2023-06-09"), Report));
			AssertEquals("N", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "", "2022-06-09"), Report));
			AssertEquals("Y", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "202308", ""), Report));
			AssertEquals("N", ValueProviderToTest.GetReplacement(HasValidGeneralLedgerDataMacro(companyPK, "", ""), Report));
		}

		public override void TestIsResponsibleForReplacing()
		{
			CombineAssertions(() =>
			{
				Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("HasValidGeneralLedgerData", Passes.FirstPass));
				Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData meh>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09,202304,2023-09-08)>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09,202304,2023-09-08)>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< HasValidGeneralLedgerData(          05D8F313-314F-42C6-9790-83DFB5893D09, 202304 , 2023-09-08 )    >", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< HasValidGeneralLedgerData (,202304, 2023-09-09)>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09,Order,2023-09-09 )       >", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<  HasValidGeneralLedgerData( 05D8F313-314F-42C6-9790-83DFB5893D09,202304,   2023-09-09 )       >", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09, 202304 ,   2023-09-09)>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09, 202304 ,   123456789-0)>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09, ,   123456789-0)>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<HasValidGeneralLedgerData(05D8F313-314F-42C6-9790-83DFB5893D09, , )>", Passes.FirstPass));
			});
		}

		protected override ValueProvider GetNewValueProvider() => new HasValidGeneralLedgerData();

		string HasValidGeneralLedgerDataMacro(string companyPK, string startPeriod, string startDate) => $"<HasValidGeneralLedgerData({companyPK}, {startPeriod} , {startDate} )>";
	}
}
