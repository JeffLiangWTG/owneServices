using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICommercialChargesProvider
	{
		string ITOTIncoTerm { get; }
		Money InvoiceTotal { get; }
		Money Commission { get; }
		Money Discount { get; }
		Money OtherCharges1 { get; }
		Money OtherCharges2 { get; }
		Money LandingCharges { get; }
		Money PackingCosts { get; }
		Money ForeignInlandFreight { get; }
	}
}
