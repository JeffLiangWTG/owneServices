using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnLookupsTest : TestCaseWithFactory
	{
		public void TestPackageTypes()
		{
			AssertEquals("precondition", ZString.Empty, outturn.C5_CargoType);
			AssertEquals("type", typeof(CMRPackageTypes), lookups.PackageTypes.GetType());

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.BreakBulk;
			AssertEquals("type", typeof(CMRPackageTypes), lookups.PackageTypes.GetType());

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.Bulk;
			AssertEquals("type", typeof(CMRQuantityUnits), lookups.PackageTypes.GetType());
			Assert("PackageTypes should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRQuantityUnits>(), lookups.PackageTypes));

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertEquals("type", typeof(CMRPackageTypes), lookups.PackageTypes.GetType());

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			AssertEquals("type", typeof(CMRPackageTypes), lookups.PackageTypes.GetType());

			outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			AssertEquals("type", typeof(CMRPackageTypes), lookups.PackageTypes.GetType());

			outturn.C5_CargoType = "FOO";
			AssertEquals("type", typeof(CMRPackageTypes), lookups.PackageTypes.GetType());
			Assert("PackageTypes should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRPackageTypes>(), lookups.PackageTypes));
		}

		public void TestCargoTypes()
		{
			AssertEquals("type", typeof(CMRImportCargoTypes), lookups.CargoTypes.GetType());
			Assert("CargoTypes should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRImportCargoTypes>(), lookups.CargoTypes));
		}

		public void TestOutturnResultTypeList()
		{
			AssertEquals("Count", 4, lookups.OutturnResultTypeList.Count);
			Assert("OutturnResultTypeList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMROutturnResultType>(), lookups.OutturnResultTypeList));
		}

		protected override void SetUp()
		{
			base.SetUp();

			outturn = Factory.New<CusOutturn>();
			lookups = outturn.Lookups;
		}

		CusOutturn outturn;
		CusOutturnLookups lookups;
	}
}
