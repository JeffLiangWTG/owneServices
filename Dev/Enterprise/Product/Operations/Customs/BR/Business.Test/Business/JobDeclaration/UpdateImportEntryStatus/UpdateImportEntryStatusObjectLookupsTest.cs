using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class UpdateImportEntryStatusObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRiskChannelList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.ActiveEntryHeaders.AddNew();

			var updateImportEntryStatus = new UpdateImportEntryStatusObject(declaration);
			var riskChannelList1 = updateImportEntryStatus.Lookups.RiskChannelList;
			AssertEquals("1, 2, 3, 4, 5", riskChannelList1.CodesAsString);

			var riskChannelList2 = new UpdateImportEntryStatusObjectLookups(updateImportEntryStatus);
			AssertSame("List should be cached", riskChannelList1, riskChannelList2.RiskChannelList);
		}

		public void TestEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);
			ReferenceTestDataHelper.CreateEntryStatusForISWList(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.ActiveEntryHeaders.AddNew();

			var updateImportEntryStatus = new UpdateImportEntryStatusObject(declaration);
			var entryStatusList1 = updateImportEntryStatus.Lookups.EntryStatusList;
			AssertEquals("S01, S02, S03, S04, S05, S06", entryStatusList1.CodesAsString);

			var entryStatusList2 = new UpdateImportEntryStatusObjectLookups(updateImportEntryStatus);
			AssertSame("List should be cached", entryStatusList1, entryStatusList2.EntryStatusList);
		}
	}
}
