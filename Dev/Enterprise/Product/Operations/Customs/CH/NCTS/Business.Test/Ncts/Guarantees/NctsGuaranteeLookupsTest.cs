using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class NctsGuaranteeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestBondTypeList()
	{
		new RefDataTestHelper(Factory).CreateNctsBondTypeList();
		var bondTypeList = Guarantee.Lookups.BondTypeList;

		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "0, 1, 2, 3", bondTypeList.CodesAsString);
			AssertSame("cached", bondTypeList, lookups.BondTypeList);
		});
	}

	public void TestReferenceNumbers()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateGuaranteeHeader(orgHeader, "CHTRA01", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, countryCode: Core.Constants.CountryCodes.Switzerland);
		CreateGuaranteeHeader(orgHeader, "CHTRA02", EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, countryCode: Core.Constants.CountryCodes.Switzerland);
		CreateGuaranteeHeader(orgHeader, "LVTRA01", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, countryCode: Core.Constants.CountryCodes.Latvia);
		CreateGuaranteeHeader(orgHeader, "DECOM01", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, EUGuaranteeTypeList.Codes.IMP, Core.Constants.CountryCodes.Switzerland);
		Factory.Save();

		Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
		var list = lookups.ReferenceNumbers;

		CombineAssertions(() =>
		{
			AssertSame("Cached", list, lookups.ReferenceNumbers);
			AssertArrayEqualsByElements("PW_BondType 1", new ZString[] { "CHTRA01" }, list.Select(x => x.CPH_Number).ToArray());

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
			AssertArrayEqualsByElements("PW_BondType 2", new ZString[] { "CHTRA02" }, lookups.ReferenceNumbers.Select(x => x.CPH_Number).ToArray());
		});
	}

	public void TestSingleReferenceNumber()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		CreateGuaranteeHeader(orgHeader, "CHTRA01", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee);
		CreateGuaranteeHeader(orgHeader, "CHTRA02", EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee);
		CreateGuaranteeHeader(orgHeader, "CHTRA03", EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor);
		Factory.Save();

		CombineAssertions(() =>
		{
			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee;
			AssertEquals("Multiple reference numbers", null, lookups.SingleReferenceNumber);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor;
			AssertEquals("Single reference numbers", "CHTRA03", lookups.SingleReferenceNumber?.CPH_Number);

			Guarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			AssertEquals("No reference numbers", null, lookups.SingleReferenceNumber);
		});
	}

	public void TestGetGuaranteeHeaderCollection() => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var detail = nctsHeader.MovementHeader.Guarantees.AddNew();

		AssertGuaranteeInCollection(true);
		AssertGuaranteeInCollection(false, type: "XXX");
		AssertGuaranteeInCollection(false, applicationCode: CusPermitHeaderApplicationCodeList.Codes.Permit);
		AssertGuaranteeInCollection(false, country: Core.Constants.CountryCodes.Belgium);

		void AssertGuaranteeInCollection(bool expected, string type = GuaranteeTypeList.Codes.TRA, string applicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee, string country = Core.Constants.CountryCodes.Switzerland)
		{
			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			guarantee.CPH_Type = type;
			guarantee.CPH_ApplicationCode = applicationCode;
			guarantee.CPH_RN_NKCountryCode = country;
			var message = $"Type={guarantee.CPH_Type} ApplicationCode={guarantee.CPH_ApplicationCode} Country={guarantee.CPH_RN_NKCountryCode}";
			AssertEquals(message, expected, detail.Lookups.ReferenceNumbers.Contains(guarantee));
		}
	});

	CusGuaranteeHeader CreateGuaranteeHeader(OrgHeader orgHeader, ZString number, ZString subType, string type = EUGuaranteeTypeList.Codes.TRA, string countryCode = Core.Constants.CountryCodes.Switzerland)
	{
		var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		result.CPH_Type = type;
		result.CPH_Number = number;
		result.CPH_OH_PermitHolder = orgHeader.PK;
		result.CPH_RN_NKCountryCode = countryCode;
		result.CPH_SubType = subType;
		return result;
	}

	NctsGuarantee CreateGuarantee()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var guarantee = header.MovementHeader.Guarantees.AddNew();
		lookups = new NctsGuaranteeLookups(guarantee);
		return guarantee;
	}

	NctsGuaranteeLookups lookups;

	NctsGuarantee Guarantee => guarantee ?? (guarantee = CreateGuarantee());
	NctsGuarantee guarantee;
}
