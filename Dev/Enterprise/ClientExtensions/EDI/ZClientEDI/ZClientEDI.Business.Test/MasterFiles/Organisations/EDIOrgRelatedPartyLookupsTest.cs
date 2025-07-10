using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EDIOrgRelatedPartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductTypeList()
		{
			var list = new SystemProductCollection();

			list.AddNew("APL", "Apollo", true);
			list.AddNew("OSR", "Osiris", true);
			list.AddNew("ANB", "Anubis", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			LicenceHeader licenceHeader = Factory.NewWithValidTestData<LicenceHeader>();
			AssertEquals(true, licenceHeader.Lookups.ProductType.ContainsCode(EDIOrgRelatedPartyLookups.EdiEnterpriseProductTypeCode));
			AssertEquals(true, licenceHeader.Lookups.ProductType.ContainsCode("APL"));
			AssertEquals(true, licenceHeader.Lookups.ProductType.ContainsCode("OSR"));
			AssertEquals(true, licenceHeader.Lookups.ProductType.ContainsCode("ANB"));
		}

		public void TestPartyTypeList()
		{
			var relatedParty = Factory.New<EDIOrgRelatedParty>();
			AssertEquals(true, relatedParty.Lookups.PartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ContractingPartyCode));
			AssertEquals(true, relatedParty.Lookups.PartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.WARPConstant));
			AssertEquals(true, relatedParty.Lookups.PartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode));
			AssertListIsSorted(nameof(relatedParty.Lookups.PartyTypeList), relatedParty.Lookups.PartyTypeList);
		}

		public void TestAllPartyTypeList()
		{
			var relatedParty = Factory.New<EDIOrgRelatedParty>();
			AssertEquals(true, relatedParty.Lookups.AllPartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ContractingPartyCode));
			AssertEquals(true, relatedParty.Lookups.AllPartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.WARPConstant));
			AssertEquals(true, relatedParty.Lookups.PartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode));
			AssertListIsSorted(nameof(relatedParty.Lookups.AllPartyTypeList), relatedParty.Lookups.AllPartyTypeList);
		}

		public void TestParentPartyTypeList()
		{
			var relatedParty = Factory.New<EDIOrgRelatedParty>();
			AssertEquals(true, relatedParty.Lookups.ParentPartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ContractingPartyCode));
			AssertEquals(true, relatedParty.Lookups.ParentPartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.WARPConstant));
			AssertEquals(true, relatedParty.Lookups.PartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode));
			AssertListIsSorted(nameof(relatedParty.Lookups.ParentPartyTypeList), relatedParty.Lookups.ParentPartyTypeList);
		}

		public void TestAllParentTypeList()
		{
			var relatedParty = Factory.New<EDIOrgRelatedParty>();
			AssertEquals(true, relatedParty.Lookups.AllParentTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ContractingPartyCode));
			AssertEquals(true, relatedParty.Lookups.AllParentTypeList.ContainsCode(EDIOrgRelatedPartyLookups.WARPConstant));
			AssertEquals(true, relatedParty.Lookups.PartyTypeList.ContainsCode(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode));
			AssertListIsSorted(nameof(relatedParty.Lookups.AllParentTypeList), relatedParty.Lookups.AllParentTypeList);
		}

		void AssertListIsSorted(string listName, CodeDescriptionPairList list)
		{
			var sortedList = new CodeDescriptionPairList();
			sortedList.AddRange(list);
			sortedList.Sort();

			AssertContainsExactElementsInExactOrder($"List {listName} is not sorted correctly", sortedList, list);
		}
	}
}
