using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface IProducedDocument
	{
		ZString DocumentType { get; }
		ZString DocumentReference { get; }
		ZString DocumentReferenceLNG { get; }
	}
}
