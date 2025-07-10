namespace Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage
{
	public interface IEInvoicingPenaltyTaxInfoProvider
	{
		public void ShowPenaltyTaxInfoForm();
		public string PenaltyTaxInfoMenuName { get; }
	}
}
