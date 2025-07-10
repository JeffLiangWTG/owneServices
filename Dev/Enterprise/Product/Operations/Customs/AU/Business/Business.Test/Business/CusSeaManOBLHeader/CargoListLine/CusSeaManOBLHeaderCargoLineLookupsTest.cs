namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLHeaderCargoLineLookupsTest : BaseCusSeaManOBLHeaderLookupsTest
	{
		public void TestPackageTypes()
		{
			var packageTypes = lookups.PackageTypes;
			AssertEquals(typeof(CMRPackageTypes), packageTypes.GetType());
			AssertSame(packageTypes, Factory.GetCachedValue<CMRPackageTypes>("AUCusSeaManOBLHeaderCargoLineLookups.PackageTypes", () => null));
		}

		public override void TestCargoCodes()
		{
			var cargoCodes = lookups.CargoCodes;
			AssertEquals(typeof(CMRCargoCodes), cargoCodes.GetType());
			AssertSame(cargoCodes, Factory.GetCachedValue<CMRCargoCodes>("AUCusSeaManOBLHeaderCargoLineLookups.CargoCodes", () => null));
		}

		public void TestCargoTypes()
		{
			var cargoTypes = lookups.CargoTypes;
			AssertEquals(typeof(CMRCargoTypes), cargoTypes.GetType());
			AssertSame(cargoTypes, Factory.GetCachedValue<CMRCargoTypes>("AUCusSeaManOBLHeaderCargoLineLookups.CargoTypes", () => null));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			lookups = (CusSeaManOBLHeaderCargoLineLookups)GetNewLookups();
		}

		protected override Customs.Business.CusSeaManOBLHeaderLookups GetNewLookups()
		{
			CusSeaManOBLHeaderCargoLine line = Factory.New<CusSeaManOBLHeaderCargoLine>();
			return line.Lookups;
		}

		CusSeaManOBLHeaderCargoLineLookups lookups;

		#endregion
	}
}
