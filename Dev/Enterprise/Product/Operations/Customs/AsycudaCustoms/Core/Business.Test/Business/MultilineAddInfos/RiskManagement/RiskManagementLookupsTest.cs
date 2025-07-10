using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class RiskManagementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var riskManagement = Factory.New<RiskManagement>();
			var lookups = new RiskManagementLookups(riskManagement);
			var list = lookups.CodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "PMT, ENT", list.CodesAsString);
				AssertSame("Cached", list, lookups.CodeList);
			});
		}

		public void TestEntryHeaders_Type()
		{
			var riskManagement = Factory.New<RiskManagement>();
			var lookups = new RiskManagementLookups(riskManagement);
			AssertType<OutOfRegimeCusEntryHeaderCollection>(lookups.EntryHeaders);
		}

		public void TestEntryHeaders_HasDefaultFilter()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;
			riskManagement.CSI_ReferenceNumber = "Entry";

			var lookups = new RiskManagementLookups(riskManagement);
			var entryHeaders = lookups.EntryHeaders;

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition", true, riskManagement.HasEntryNumber);
				AssertEquals("Entry Number:Property", "Entry", entryHeaders.FilterBusinessObjectDefaults["Entry Number:Property"].Value);
			});
		}

		public void TestEntryHeaders_NoDefaultFilter()
		{
			var riskManagement = Factory.New<RiskManagement>();
			riskManagement.CSI_Code = EntryPermitTypeList.Codes.EntryDeclaration;

			var lookups = new RiskManagementLookups(riskManagement);
			var entryHeaders = lookups.EntryHeaders;

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition", false, riskManagement.HasEntryNumber);
				AssertEquals("Entry Number:Property", 0, entryHeaders.FilterBusinessObjectDefaults.Count);
			});
		}
	}
}
