using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5TransportMeansUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusInBondEvent), control.BindingSource.DataSourceType);
		}

		public void TestTransportAtDepartureTypeDropEdit()
		{
			var transportAtDepartureTypeDropEdit = control.TransportAtDepartureTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", transportAtDepartureTypeDropEdit);
				AssertEquals("BindTo", nameof(CusInBondEvent.BN_TransportAtDepartureType), transportAtDepartureTypeDropEdit.BindTo);
			});
		}

		public void TestTransportAtDepartureIDTextBox()
		{
			var transportAtDepartureIDTextBox = control.TransportAtDepartureIDTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", transportAtDepartureIDTextBox);
				AssertEquals("BindTo", nameof(CusInBondEvent.BN_TransportAtDepartureID), transportAtDepartureIDTextBox.BindTo);
			});
		}

		public void TestTransportAtDepartureNationalityFindBox()
		{
			var transportAtDepartureNationalityCodeFindBox = control.TransportAtDepartureNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", transportAtDepartureNationalityCodeFindBox);
				AssertEquals("BindTo", nameof(CusInBondEvent.BN_RN_NKTransportAtDepartureIDNationality), transportAtDepartureNationalityCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5TransportMeansUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		Phase5TransportMeansUserControl control;
	}
}
