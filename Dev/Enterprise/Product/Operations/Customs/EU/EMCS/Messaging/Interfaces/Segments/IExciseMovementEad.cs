using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IExciseMovementEad
	{
		ZString AdministrativeReferenceCode { get; set; }

		ZString SequenceNumber { get; set; }
	}
}
