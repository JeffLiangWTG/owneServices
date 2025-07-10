using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.H7.GUI
{
	public class RequestedDocumentsUserControl : EU.GUI.RequestedDocumentsUserControl, IAdditionalTabPage
	{
		protected override void CustomizeLayoutCore()
		{
			var referenceNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			referenceNumberTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("1f7cd70f-d896-4a22-abdc-237ae00b7142", "Ref. No.", "Reference No.", "Reference Number", "Requested document reference number.");
			referenceNumberTextBoxColumnStyleInfo.ColumnName = CusSupportingInfoSchema.Constants.CSI_ReferenceNumber;
			referenceNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			RequestedDocumentsGrid.ColumnStyles.Add(referenceNumberTextBoxColumnStyleInfo);
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("4ea32ee1-cdfa-4748-9278-b1147b308e64", "Requested Documents");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(header => true, null);

		int IAdditionalTabPage.TabPageSequence => 60;

		#endregion
	}
}
