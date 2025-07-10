using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class TransportDetailsControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestBox18UserControl()
		{
			AssertType<InlandTransportDetailsUserControl>(control.InlandTransportDetailsUserControl);
		}

		public void TestFlightAndNationalityUserControl()
		{
			AssertType<FlightAndNationalityUserControl>(control.FlightAndNationalityUserControl);
		}

		public void TestTransportIDAndNationalityUserControl()
		{
			AssertType<TransportIDAndNationalityUserControl>(control.TransportIDAndNationalityUserControl);
		}

		public void TestTransportIDAndNationalityRailUserControl()
		{
			AssertType<TransportIDAndNationalityRailUserControl>(control.TransportIDAndNationalityRailUserControl);
		}

		public void TestTransportIDAndNationalityInlandWaterwayUserControl()
		{
			AssertType<TransportIDAndNationalityInlandWaterwayUserControl>(control.TransportIDAndNationalityInlandWaterwayUserControl);
		}

		public void TestTransportIDAndNationalityInlandWaterwayENIUserControl()
		{
			AssertType<TransportIDAndNationalityInlandWaterwayENIUserControl>(control.TransportIDAndNationalityInlandWaterwayENIUserControl);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestAdditionalWagonNumbersUserControl()
		{
			AssertType<AdditionalWagonNumbersUserControl>(control.AdditionalWagonNumbersUserControl);
		}

		public void TestVesselUserControl()
		{
			AssertType<VesselUserControl>(control.VesselUserControl);
		}

		public void TestTransportNationalityCodeFindBox()
		{
			var transportNationalityCodeFindBox = control.TransportNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Nationality", transportNationalityCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("Binding", "JE_RN_NKTransportNationality", transportNationalityCodeFindBox.BindTo);
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
