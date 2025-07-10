using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;
public partial class EntryLineTaxAndConfirmedFeeUserControl : ZUserControl
{
	public EntryLineTaxAndConfirmedFeeUserControl()
	{
		InitializeComponent();
		EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("8C947315-36F7-44C2-AB25-BCA6B2DFD964", "Calculated Duty And Tax");
	}
}
