using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IDHSAmendmentDetails : IAmendmentDetails
	{
		ZString AmendmentTypeForInvoiceLine { get; }
	}
}
