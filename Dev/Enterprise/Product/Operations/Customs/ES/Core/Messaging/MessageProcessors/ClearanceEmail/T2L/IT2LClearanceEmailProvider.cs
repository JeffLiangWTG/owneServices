using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface IT2LClearanceEmailProvider
	{
		ZString CSVClearance { get; }
	}
}
