using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class TransportDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestFlightNumberVisibleForAir()
		{
			using (var transportDetailsUserControl = new TransportDetailsUserControl())
			{
				transportDetailsUserControl.SetDataBinding(header, string.Empty);
				var transportRegistrationTextBox = (ZTextBox)transportDetailsUserControl.Controls.Find("TransportRegistrationNumTextBox", true).Single();
				var vesselFindBox = transportDetailsUserControl.Controls.Find("VesselCodeFindBox", true).Single();
				header.SJH_TransportMode = TransportTypeList.Codes.Air;
				CombineAssertions(() =>
				{
					AssertEquals("Transport Registration visible", true, transportRegistrationTextBox.Visible);
					AssertEquals("Transport Registration caption", "Flight Number", transportRegistrationTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Vessel not visible", false, vesselFindBox.Visible);
				});
			}
		}

		public void TestVesselVisiableForSeaAndInlandWaterway()
		{
			using (var transportDetailsUserControl = new TransportDetailsUserControl())
			{
				transportDetailsUserControl.SetDataBinding(header, string.Empty);
				var transportRegistrationTextBox = (ZTextBox)transportDetailsUserControl.Controls.Find("TransportRegistrationNumTextBox", true).Single();
				var vesselFindBox = transportDetailsUserControl.Controls.Find("VesselCodeFindBox", true).Single();
				foreach (var transportMode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.InlandWaterwayTransport })
				{
					header.SJH_TransportMode = transportMode;
					CombineAssertions("Transport Mode " + transportMode, () =>
					{
						AssertEquals("Transport Registration not visible", false, transportRegistrationTextBox.Visible);
						AssertEquals("Vessel visible", true, vesselFindBox.Visible);
					});
				}
			}
		}

		public void TestTransportRegNoVisibleForRemainingModes()
		{
			using (var transportDetailsUserControl = new TransportDetailsUserControl())
			{
				transportDetailsUserControl.SetDataBinding(header, string.Empty);
				var transportRegistrationTextBox = (ZTextBox)transportDetailsUserControl.Controls.Find("TransportRegistrationNumTextBox", true).Single();
				var vesselFindBox = transportDetailsUserControl.Controls.Find("VesselCodeFindBox", true).Single();
				var codesToRemove = new HashSet<string>() { TransportTypeList.Codes.Sea, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Air };
				foreach (var transportMode in new TransportTypeList().GetAllCodes().Except(codesToRemove))
				{
					header.SJH_TransportMode = transportMode;
					CombineAssertions("Transport Mode " + transportMode, () =>
					{
						AssertEquals("Transport Registration vissible", true, transportRegistrationTextBox.Visible);
						AssertEquals("Transport Registration caption", "Transport Reg. No.", transportRegistrationTextBox.CaptionResourceString.Caption);
						AssertEquals("Vessel visible", false, vesselFindBox.Visible);
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageJobHeader>();
		}
		CusTempStorageJobHeader header;
	}
}
