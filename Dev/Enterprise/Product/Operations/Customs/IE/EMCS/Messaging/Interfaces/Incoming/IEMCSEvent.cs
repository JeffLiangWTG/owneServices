using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IEMCSEvent
	{
		ZString AdministrativeReferenceCode { get; }
		ZString SequenceNumber { get; }
	}
}
