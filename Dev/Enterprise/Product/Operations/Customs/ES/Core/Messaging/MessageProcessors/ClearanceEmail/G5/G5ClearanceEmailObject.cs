using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors;

public class G5ClearanceEmailObject : IG5ClearanceEmailProvider
{
	public G5ClearanceEmailObject(ZString g4MRN)
	{
		G4MRN = g4MRN;
	}

	public ZString G4MRN { get; }
}
