using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSPackage))]
	class EMCSPackageTest : CusInvPackTest<EMCSJobDeclaration>
	{
		public void TestB5_MarksAndNumbers()
		{
			EMCSPackageTestHelper.SetupPackageTypeCodeList(Factory);
			CombineAssertions(() =>
			{
				package.B5_UnitType = EMCSPackageTestHelper.UncountableUnitType;
				AssertEquals("ReadOnly for uncountable type.", true, package.B5_MarksAndNumbersInfo.ReadOnly);

				package.B5_UnitType = EMCSPackageTestHelper.CountableUnitType;
				AssertEquals("ReadOnly for countable type.", false, package.B5_MarksAndNumbersInfo.ReadOnly);
			});
		}

		public void TestClearB5_MarksAndNumbersIfNecessary()
		{
			EMCSPackageTestHelper.SetupPackageTypeCodeList(Factory);
			package.B5_UnitType = EMCSPackageTestHelper.CountableUnitType;
			package.B5_MarksAndNumbers = "123";

			CombineAssertions(() =>
			{
				package.B5_UnitType = EMCSPackageTestHelper.CountableUnitType2;
				AssertEquals("Changing B5_UnitType to countable type doesn't clear B5_MarksAndNumbers", "123", package.B5_MarksAndNumbers);

				package.B5_UnitType = EMCSPackageTestHelper.UncountableUnitType;
				AssertEquals("Changing B5_UnitType to uncountable type clears B5_MarksAndNumbers", ZString.Empty, package.B5_MarksAndNumbers);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			return declaration.EMCSPackages.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			package = (EMCSPackage)GetNewBusinessObject();
		}
		EMCSPackage package;
	}
}
