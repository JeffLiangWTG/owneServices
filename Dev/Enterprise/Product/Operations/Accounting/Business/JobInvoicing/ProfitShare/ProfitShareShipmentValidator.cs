namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public interface IProfitShareShipmentValidator
	{
		bool IsChargeValidToBePosted(Charge charge);
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ProfitShareShipmentValidator : IProfitShareShipmentValidator
	{
		public bool IsChargeValidToBePosted(Charge charge) => !charge?.HasErrors ?? false;
	}
}
