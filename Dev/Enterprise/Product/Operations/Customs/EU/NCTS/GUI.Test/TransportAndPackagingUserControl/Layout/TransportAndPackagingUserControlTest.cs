using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class TransportAndPackagingUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestTransportMethodOfPaymentDropEdit()
		{
			var transportMethodOfPaymentDropEdit = control.TransportMethodOfPaymentDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", transportMethodOfPaymentDropEdit);
				AssertEquals("BindTo", "BM_MethodOfPayment", transportMethodOfPaymentDropEdit.BindTo);
			});
		}

		public void TestCarrierDocAddressControl()
		{
			var carrierDocAddressControl = control.CarrierDocAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZDocAddressControl>("Type", carrierDocAddressControl);
				AssertEquals("BindTo", "Carrier", carrierDocAddressControl.BindTo);
				AssertEquals("BindToOrganisations", "Lookups.Organisations", carrierDocAddressControl.BindToOrganisations);
				AssertEquals("DisplayMode", ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox, carrierDocAddressControl.DisplayMode);
				AssertEquals("Caption", "Carrier", carrierDocAddressControl.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportAndPackagingUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportAndPackagingUserControl control;
	}
}
