using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IEMCSEvent
	{
		ZString AdministrativeReferenceCode { get; }
		ZString SequenceNumber { get; }
	}
}
