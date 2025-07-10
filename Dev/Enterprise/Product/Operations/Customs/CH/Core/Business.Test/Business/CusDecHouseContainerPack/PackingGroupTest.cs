using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PackingGroup))]
sealed class PackingGroupTest : Customs.Business.Testing.BasePackingGroupTest
{
	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = base.ImportJobDeclaration;
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}
	}

	public new void TestGetConvertedPackType() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TotalNoOfPacks = 10;
		declaration.JE_TotalNoOfPacksPackType = "BAG";
		declaration.JE_HouseBill = "HB1";
		var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
		var package = packingGroup.Packages[0];

		AssertEquals(10, package.CW_PackQty);
		AssertEquals("BG", package.CW_PackType);
	});
}
