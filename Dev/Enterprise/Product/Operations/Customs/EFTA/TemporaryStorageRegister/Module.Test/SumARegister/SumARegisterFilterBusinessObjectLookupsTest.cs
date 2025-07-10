using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(SumARegisterFilterBusinessObjectLookups))]
sealed class SumARegisterFilterBusinessObjectLookupsTest : TestCaseWithFactory
{
	public void TestOwnerReferenceTypeList()
	{
		var ownerReferenceTypeList = lookups.OwnerReferenceTypeList;
		AssertType<CodeDescriptionPairList>("Type", ownerReferenceTypeList);
	}

	public void TestCustomsStatusList()
	{
		var list = lookups.CustomsStatusList;
		AssertType<CodeDescriptionPairList>("Type", list);
	}

	protected override void SetUp()
	{
		base.SetUp();

		lookups = new SumARegisterFilterBusinessObjectLookups(new SumARegisterFilterBusinessObject());
	}

	SumARegisterFilterBusinessObjectLookups lookups;
}
