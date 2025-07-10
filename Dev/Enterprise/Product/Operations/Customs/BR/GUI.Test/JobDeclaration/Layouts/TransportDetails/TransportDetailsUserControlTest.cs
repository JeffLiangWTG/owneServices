using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class TransportDetailsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestObjectsTypesForComponentsOnUserControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("SpecialTransportDropEdit should be ZDropEdit", control.CargoArrivalDocUtilizationDropEdit);
				AssertType<VesselAndCountryUserControl>("OperationTypeDropEdit should be VesselAndCountryUserControl", control.VesselAndCountryUserControl);
				AssertType<ZTextBox>("PlateTextBox should be ZTextBox", control.PlateTextBox);
			});
		}

		TransportDetailsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
