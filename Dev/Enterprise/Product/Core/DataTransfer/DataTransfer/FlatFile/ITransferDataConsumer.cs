
namespace Enterprise.DataTransfer.Business
{
	public interface ITransferDataConsumer
	{
		ITransferDataCollector DataCollector { get; }
		void ResetDataCollector();
	}
}