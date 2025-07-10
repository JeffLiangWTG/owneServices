using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetByDataGroupingCode() => CombineAssertions(() =>
	{
		var provider = EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Switzerland);
		AssertType<CusSupplyChainActorReferenceProvider>("Type", provider);
		AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Switzerland, provider.DataGroupingCode);
	});

	public void TestGetReferenceFromOwner() => CombineAssertions(() =>
	{
		var orgWithBIDandUIDandDUNN = CreateOrganizationWithCustomsCodes("1", OrgCusCode.SwissCodeTypes.BID, OrgCusCode.SwissCodeTypes.UID, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithUIDandDUN = CreateOrganizationWithCustomsCodes("2", OrgCusCode.SwissCodeTypes.UID, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithDUNonly = CreateOrganizationWithCustomsCodes("3", OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgMultipleDUN = CreateOrganizationWithCustomsCodes("4", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithNoID = CreateOrganizationWithCustomsCodes("5");

		CusReference.CFR_Reference = ZString.Empty;

		CusReference.OwnerOrgPK = orgWithBIDandUIDandDUNN.PK;
		AssertEquals("Org with BID, UID and DUN", "BID1", CusReference.CFR_Reference);

		CusReference.OwnerOrgPK = orgWithUIDandDUN.PK;
		AssertEquals("Org with UID and DUN", "UID2", CusReference.CFR_Reference);

		CusReference.OwnerOrgPK = orgWithDUNonly.PK;
		AssertEquals("Org with DUN only", "DUN3", CusReference.CFR_Reference);

		CusReference.OwnerOrgPK = orgMultipleDUN.PK;
		AssertEquals("Org with multiple DUN", ZString.Empty, CusReference.CFR_Reference);

		CusReference.OwnerOrgPK = orgWithNoID.PK;
		AssertEquals("Org with no ID", ZString.Empty, CusReference.CFR_Reference);
	});

	OrgHeader CreateOrganizationWithCustomsCodes(string index, params string[] customCodes)
	{
		var organization = Factory.New<OrgHeader>();
		customCodes.ForEach(c => organization.CustomsCodes.AddNew(c, $"{c}{index}"));
		return organization;
	}

	CusSupplyChainActorReference CusReference => cusReference ?? (cusReference = Factory.New<CusSupplyChainActorReference>());
	CusSupplyChainActorReference cusReference;
}
