using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryLineTaxAndConfirmedFeeUserControl : ZUserControl
	{
		public EntryLineTaxAndConfirmedFeeUserControl()
		{
			InitializeComponent();
			EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("143D9429-AC98-4BE6-B8B0-3CE262818E61", "Calculated Duties And Taxes");
		}
	}
}
