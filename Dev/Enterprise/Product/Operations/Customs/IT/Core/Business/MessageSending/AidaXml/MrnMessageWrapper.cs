using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml;

public class MrnMessageWrapper : IMrnProvider
{
	public MrnMessageWrapper(IMovementReferenceNumberProvider provider)
	{
		Mrn = Argument.NotNull(provider, nameof(provider)).MovementReferenceNumber;
	}

	public string Mrn { get; }
}
