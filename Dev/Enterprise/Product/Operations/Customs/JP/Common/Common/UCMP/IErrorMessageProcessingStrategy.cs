namespace Enterprise.Customs.JP.Common
{
	public interface IErrorMessageProcessingStrategyParent
	{
		IErrorMessageProcessingStrategy ProcessingStrategy { get; }
	}

	public interface IErrorMessageProcessingStrategy
	{
		void ProcessMessage(EDIMessage message);
	}
}
