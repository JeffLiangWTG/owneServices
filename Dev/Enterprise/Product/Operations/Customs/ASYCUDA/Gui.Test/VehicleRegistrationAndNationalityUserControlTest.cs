using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(VehicleRegistrationAndNationalityUserControl))]
	sealed class VehicleRegistrationAndNationalityUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using var control = new VehicleRegistrationAndNationalityUserControl();
			AssertEquals("VehicleRegistrationAndNationalityUserControl test data source", typeof(AsycudaManifestHeader), control.BindingSource.DataSourceType);
		}

		public void TestVehicleRegistrationTextBox()
		{
			var vehicleRegistrationTextBox = form.VehicleRegistrationTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(vehicleRegistrationTextBox);
				AssertEquals("BindingMember", nameof(AsycudaManifestHeader.AMA_VehicleRegistration), vehicleRegistrationTextBox.GetBindingMember());
			});
		}

		public void TestCountryForVehicleRegistrationCodeFindBox()
		{
			var vehicleNationalityCodeFindBox = form.VehicleNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>(vehicleNationalityCodeFindBox);
				AssertEquals("BindingMember", nameof(AsycudaManifestHeader.AMA_RN_NKConveyanceNationality), vehicleNationalityCodeFindBox.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			form = new VehicleRegistrationAndNationalityUserControl();
		}

		VehicleRegistrationAndNationalityUserControl form;

		protected override void TearDown()
		{
			base.TearDown();

			form.Dispose();
		}
	}
}
