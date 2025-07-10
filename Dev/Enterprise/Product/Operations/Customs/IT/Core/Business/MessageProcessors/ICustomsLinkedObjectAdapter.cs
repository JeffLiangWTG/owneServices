using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsLinkedObjectAdapter
{
	ZGuid PK { get; }

	void AddMessage(EDIMessage message);

	EDIMessage GetLastSuccessfullySentMessage();

	void GenerateDocuments();

	ZString EntryReferenceNumber { get; }

	ZString JobReferenceNumber { get; }

	BusinessObjectFactory Factory { get; }

	ZString CustomsProfile { get; }
}
