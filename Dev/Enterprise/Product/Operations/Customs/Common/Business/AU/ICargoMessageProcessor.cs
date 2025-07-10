
namespace Enterprise.Customs.Common.AU
{
	public interface ICargoMessageProcessor
	{
		void Process(ICargoMessageProcessorJob processorJob, bool saveFactory);
	}
}
