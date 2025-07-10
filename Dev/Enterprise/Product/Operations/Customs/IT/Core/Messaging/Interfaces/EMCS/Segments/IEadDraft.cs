using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IEadDraft
{
	ZString LocalReferenceNumber { get; }

	ZString InvoiceNumber { get; }

	ZDate InvoiceDate { get; }

	ZInt OriginTypeCode { get; }

	ZDate DateOfDispatch { get; }

	ZDateTime TimeOfDispatch { get; }
}
