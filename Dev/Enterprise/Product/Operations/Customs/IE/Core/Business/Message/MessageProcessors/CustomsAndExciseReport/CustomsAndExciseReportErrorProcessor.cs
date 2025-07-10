using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business;

public sealed class CustomsAndExciseReportErrorProcessor : CustomsAndExciseReportInboundMessageProcessor<CustomsAndExciseReportErrorProvider>
{
	public CustomsAndExciseReportErrorProcessor(LoggingInformation logger, Type jsonObjectType) : base(logger, jsonObjectType)
	{
	}

	protected override string MessageFriendlyNameCore => CommonResStrings.RosErrorMessageFriendlyName;

	protected override bool GetIsFailure() => true;

	protected override Type MessageInterpreterType => typeof(CustomsAndExciseReportErrorInterpreter);
}
