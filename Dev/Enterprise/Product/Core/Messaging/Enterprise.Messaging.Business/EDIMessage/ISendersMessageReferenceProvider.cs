using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	public interface ISendersMessageReferenceProvider
	{
		void PopulateSendersReferenceIfNeeded();
		ZString SendersReference { get; }
	}
}
