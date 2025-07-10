using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IN.Business;

public sealed class XtErrorMessageProcessor : BaseMessageProcessor
{
	public XtErrorMessageProcessor(EDIMessage message, LoggingInformation logger) : base(message, logger)
	{
	}

	protected override ZString ProcessorName => nameof(XtErrorMessageProcessor);

	protected override BusinessObject GetLinkedObjectCore() => Message.EM_LinkedObject;

	protected override bool ProcessCore()
	{
		SetMessageStatus();
		return true;
	}

	void SetMessageStatus()
	{
		var linkObject = Message.EM_LinkedObject;
		if (linkObject is IMessageAttachee messageAttachee)
		{
			messageAttachee.MessageStatus = MessageStatusList.Codes.MessageDeliveryFailed;
		}
		else
		{
			ErrorReporter.ReportOnce(message: $"LinkObject is not IMessageAttachee, type is {linkObject?.GetType().FullName ?? "null"}");
		}
	}
}
