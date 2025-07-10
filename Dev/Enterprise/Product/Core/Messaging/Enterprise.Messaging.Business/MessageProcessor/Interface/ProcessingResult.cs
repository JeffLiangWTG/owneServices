using Enterprise.ZArchitecture.Core;

namespace Enterprise.Messaging.Business.MessageProcessor;

public record ProcessingResult<TReturnValue>(TReturnValue ReturnValue, MultilingualString DiscardReason)
{
	public ProcessingResult(TReturnValue returnValue) : this(returnValue, (NoResString)string.Empty)
	{
	}

	public static implicit operator ProcessingResult<TReturnValue>(TReturnValue returnValue) => new(returnValue);

	public static explicit operator TReturnValue(ProcessingResult<TReturnValue> processingResult) => processingResult.ReturnValue;
}

public static class ProcessingResult
{
	public static ProcessingResult<TReturnValue> New<TReturnValue>(TReturnValue returnValue, MultilingualString discardReason) => new (returnValue, discardReason);
	public static ProcessingResult<TReturnValue> New<TReturnValue>(TReturnValue returnValue) => new(returnValue, (NoResString)string.Empty);
}
