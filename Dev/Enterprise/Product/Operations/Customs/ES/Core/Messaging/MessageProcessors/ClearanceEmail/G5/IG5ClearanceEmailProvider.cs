using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors;

public interface IG5ClearanceEmailProvider
{
	ZString G4MRN { get; }
}
