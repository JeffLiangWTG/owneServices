using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class GuaranteeLiabilityAmountResponseData
	{
		public ZString CalculatedLiability { get; set; }
		public ZString CurrencyCode { get; set; }
		public ZString GRN { get; set; }
		public ZString TypeCode { get; set; }
	}
}
