using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class EntryLineTaxAndConfirmedFeeUserControl : ZUserControl
	{
		public EntryLineTaxAndConfirmedFeeUserControl()
		{
			InitializeComponent();
			EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("DA9BA427-C017-4B5F-8004-DD708A16C6C2", "Calculated Duty And Tax");
		}
	}
}
