using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class CreditCODDataObjectLookupsTest : TestCaseWithFactory
	{
		public void TestCreditMethodList()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			AssertEquals("CPE, CPL, CPP", item.Lookups.CreditMethodList.CodesAsString);
		}

		public void TestReleasingEntryNoList()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES090;
			entryHeader1.EntryNumber = "1";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader2.EntryNumber = "2";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES090;
			entryHeader3.EntryNumber = "3";

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader4 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader4.EntryNumber = "4";

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader5 = declaration5.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader5.CH_ExitedStatus = ExportControlStatusList.Codes.ESO;
			entryHeader5.EntryNumber = "5";

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader6 = declaration6.CustomsEntryHeaders.AddNew();
			entryHeader6.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader6.CH_ExitedStatus = ExportControlStatusList.Codes.SOR;
			entryHeader6.EntryNumber = "6";
			Factory.Save();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			AssertContainsExactElementsInAnyOrder(new CusEntryHeader[] { entryHeader2, entryHeader5, entryHeader6 }, item.Lookups.ReleasingEntryNoList);
		}

		public void TestPreviousEntryNoList()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES090;
			entryHeader1.EntryNumber = "1";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader2.EntryNumber = "2";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES090;
			entryHeader3.EntryNumber = "3";

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader4 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader4.EntryNumber = "4";

			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader5 = declaration5.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader5.CH_ExitedStatus = ExportControlStatusList.Codes.ESO;
			entryHeader5.EntryNumber = "5";

			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryHeader6 = declaration6.CustomsEntryHeaders.AddNew();

			entryHeader6.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader6.CH_ExitedStatus = ExportControlStatusList.Codes.SOR;
			entryHeader6.EntryNumber = "6";
			Factory.Save();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			AssertContainsExactElementsInAnyOrder(new CusEntryHeader[] { entryHeader2 }, item.Lookups.PreviousEntryNoList);
		}
	}
}
