using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class EntryLineTaxAndConfirmedFeeUserControl : ZUserControl
	{
		public EntryLineTaxAndConfirmedFeeUserControl()
		{
			InitializeComponent();
			EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Res.GetData("511BF195-1B68-4D5E-AE2B-2F86A3918D81", "Calculated Duties And Taxes");
		}
	}
}
