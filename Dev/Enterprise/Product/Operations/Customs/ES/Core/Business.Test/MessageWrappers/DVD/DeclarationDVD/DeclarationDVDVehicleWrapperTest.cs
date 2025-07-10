using Enterprise.Customs.ES.Business.MessageWrappers;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDVehicleWrapperTest : WrapperHelperTest<DeclarationDVDVehicleWrapper>
	{
		public void TestType()
		{
			AssertEquals("Expected filled Type with FR", RefCusCodeList.PackageType.Frame, wrapper.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new DeclarationDVDVehicleWrapper(VehiclePackage.Vin, VehiclePackage.Brand, VehiclePackage.Model);
		}
		DeclarationDVDVehicleWrapper wrapper;

		protected override DeclarationDVDVehicleWrapper GetProvider() => wrapper;
	}
}
