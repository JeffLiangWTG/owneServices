using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public class UCC6EntryLineTaxAndConfirmedFeeUserControl : EU.GUI.EntryLineTaxAndConfirmedFeeUserControl
	{
		public UCC6EntryLineTaxAndConfirmedFeeUserControl()
			: base()
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			var methodOfPaymentDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			methodOfPaymentDescriptionColumnStyleInfo.CaptionResourceString = Res.GetData("3673B657-ADA3-4508-9406-FB60A8498734", "MOP Desc.", "MOP Description", "Method Of Payment Description");
			methodOfPaymentDescriptionColumnStyleInfo.ColumnName = nameof(CusEntryLineFee.MethodOfPaymentDescription);
			methodOfPaymentDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			base.EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Add(methodOfPaymentDescriptionColumnStyleInfo);

			var methodOfPaymentDescriptionColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			methodOfPaymentDescriptionColumnStyleInfo2.CaptionResourceString = Res.GetData("3673B657-ADA3-4508-9406-FB60A8498734", "MOP Desc.", "MOP Description", "Method Of Payment Description");
			methodOfPaymentDescriptionColumnStyleInfo2.ColumnName = nameof(CusEntryLineFee.MethodOfPaymentDescription);
			methodOfPaymentDescriptionColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			base.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(methodOfPaymentDescriptionColumnStyleInfo2);

			var nationalTypeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			nationalTypeColumnStyleInfo.CaptionResourceString = Res.GetData("A55A683D-8575-4A40-A73F-51FA2E041BE5", "National Type");
			nationalTypeColumnStyleInfo.ColumnName = CusEntryLineFee.Schema.NationalFeeTypeCode;
			nationalTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			base.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(nationalTypeColumnStyleInfo);

			var nationalTypeColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			nationalTypeColumnStyleInfo2.CaptionResourceString = Res.GetData("A55A683D-8575-4A40-A73F-51FA2E041BE5", "National Type");
			nationalTypeColumnStyleInfo2.ColumnName = CusEntryLineFee.Schema.NationalFeeTypeCode;
			nationalTypeColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			base.EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGrid.ColumnStyles.Add(nationalTypeColumnStyleInfo2);
		}
	}
}
