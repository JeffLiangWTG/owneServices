using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IEMCSEvent
	{
		ZString AdministrativeReferenceCode { get; }
		ZString SequenceNumber { get; }
	}
}
