using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class VehicleCommonWrapperTest : WrapperHelperTest<VehicleCommonWrapper>
	{
		public void TestChassis()
		{
			AssertEquals(VehiclePackage.Vin, wrapper.Chassis);
		}

		public void TestBrand()
		{
			AssertEquals(VehiclePackage.Brand, wrapper.Brand);
		}

		public void TestModel()
		{
			AssertEquals(VehiclePackage.Model, wrapper.Model);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new VehicleCommonWrapper(VehiclePackage.Vin, VehiclePackage.Brand, VehiclePackage.Model);
		}
		VehicleCommonWrapper wrapper;

		protected override VehicleCommonWrapper GetProvider() => wrapper;
	}
}
