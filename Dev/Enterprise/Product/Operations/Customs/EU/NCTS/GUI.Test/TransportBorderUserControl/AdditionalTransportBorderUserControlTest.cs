using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class AdditionalTransportBorderUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new AdditionalTransportBorderUserControl())
			{
				AssertEquals("DataSourceType", typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
			}
		}

		public void TestCaptionRenderingEnabled()
		{
			using (var control = new AdditionalTransportBorderUserControl())
			{
				AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
			}
		}

		public void TestMoreButton()
		{
			using (var control = new AdditionalTransportBorderUserControl())
			{
				var moreButton = control.MoreButton;
				CombineAssertions(() =>
				{
					AssertType<ZButton>("Type", moreButton);
					AssertEquals("Caption", "More...", moreButton.CaptionResourceString.Caption);
				});
			}
		}

		public void TestMoreButton_Click()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			using (var form = new ZForm(nctsHeader))
			{
				var userControl = new Phase5TransportAndPackagingTabUserControl();
				userControl.SetDataBinding(nctsHeader, "");
				form.Controls.Add(userControl);
				form.Show();
				var transportBorderDynamicLayoutPanel = userControl.TransportBorderDynamicLayoutPanel;
				var button = transportBorderDynamicLayoutPanel.FindSingle<ZButton>("MoreButton");
				button.PerformClick();

				AssertType<AdditionalTransportBorderForm>(ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestAdditionalTransportBorderMeansCountCalcEdit()
		{
			using (var control = new AdditionalTransportBorderUserControl())
			{
				var additionalTransportBorderMeansCountCalcEdit = control.AdditionalTransportBorderMeansCountCalcEdit;
				CombineAssertions(() =>
				{
					AssertType<ZCalcEdit>("Type", additionalTransportBorderMeansCountCalcEdit);
					AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.AdditionalTransportAtBorderListCount), additionalTransportBorderMeansCountCalcEdit.BindTo);
					AssertEquals("ReadOnly", true, additionalTransportBorderMeansCountCalcEdit.ReadOnly);
				});
			}
		}
	}
}
