using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class RestrictionAdditionalInformationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAdditionalInformationCodeList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeRestrictionCodes: true);

		var restriction = AdditionalInformation.Parent;

		AssertEquals("Empty list", 0, Lookups.CY_CodeList.Count);

		restriction.CSI_Code = RefCusCodeTestHelper.AdditionalInformationRestrictionCode;
		AssertEquals("Code with Attribute - B1001", false, Lookups.CY_CodeList.ContainsCode(RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001));
		AssertEquals("Code with Attribute - N1004", true, Lookups.CY_CodeList.ContainsCode(RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004));
		AssertEquals("Code without Attribute", false, Lookups.CY_CodeList.ContainsCode(RefCusCodeTestHelper.AdditionalInformationWithoutRestrictionCode_B1004));

		restriction.CSI_ReferenceNumber = "123";
		AssertEquals("With reference number without Attribute - B1001", true, Lookups.CY_CodeList.ContainsCode(RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001));

		restriction.CSI_Code = "AAA";
		AssertEquals("Empty list with invalid Restriction Code", 0, Lookups.CY_CodeList.Count);
	});

	public void TestDataList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodeRestrictions(Factory, includeLinkedCodeTypes: true);

		AdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_N1004;
		Lookups.DataList.Load();
		AssertContainsExactElementsInAnyOrder("With LinkedCodeType", new[] { RefCusCodeTestHelper.AdditionalInformationLinkedCode_N5004_1, RefCusCodeTestHelper.AdditionalInformationLinkedCode_N5004_2 }, Lookups.DataList.Select(x => x.ZZD_Code));

		AdditionalInformation.CY_Code = RefCusCodeTestHelper.AdditionalInformationWithRestrictionCode_B1001;
		Lookups.DataList.Load();
		AssertEquals("No LinkedCodeType", 0, Lookups.DataList.Count);
	});

	RestrictionAdditionalInformation AdditionalInformation => additionalInformation ??= GetNewAdditionalInformation(Factory);
	RestrictionAdditionalInformation additionalInformation;

	RestrictionAdditionalInformation GetNewAdditionalInformation(BusinessObjectFactory factory)
	{
		var restriction = Factory.New<Restriction>();
		return restriction.AdditionalInformations.AddNew();
	}

	RestrictionAdditionalInformationLookups Lookups => AdditionalInformation.Lookups;
}
