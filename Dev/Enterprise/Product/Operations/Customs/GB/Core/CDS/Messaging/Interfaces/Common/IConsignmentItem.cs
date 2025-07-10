using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IConsignmentItem
	{
		ZString FreightPaymentMethodCode { get; }
		IOrganisation Consignor { get; }
	}
}
