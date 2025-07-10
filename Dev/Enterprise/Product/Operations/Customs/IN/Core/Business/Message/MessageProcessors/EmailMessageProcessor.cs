using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.IN.Business;

public class EmailMessageProcessor : BaseMessageProcessor
{
	public EmailMessageProcessor(EDIMessage message, LoggingInformation logger) : base(message, logger)
	{
		EmailInfo = new EmailInfo(Utils.CreateMessageFromEml(message));
	}

	protected readonly EmailInfo EmailInfo;

	protected override ZString ProcessorName => GetMessageProcessor(false)?.GetType().Name ?? GetType().Name;

	protected override BusinessObject GetLinkedObjectCore()
	{
		return GetMessageProcessor()?.GetLinkedObject(Message, EmailInfo, Logger);
	}

	protected override bool ProcessCore()
	{
		return GetMessageProcessor()?.Process(Message, EmailInfo, Logger) ?? false;
	}

	IEmailMessageProcessor GetMessageProcessor(bool shouldLog = true)
	{
		messageProcessor ??= messageProcessors.Value.FirstOrDefault(x => x.CanProcess(EmailInfo));

		if (messageProcessor == null && shouldLog)
		{
			var messageIDLog = EmailInfo.MessageIdOnAttachment.IsNullOrEmpty() ? string.Empty : $" with Message ID: '{EmailInfo.MessageIdOnAttachment}'";
			Logger.Log($"Could not find message processor for the email with Subject: '{EmailInfo.Subject}'{messageIDLog}.", LogType.Error);
		}
		return messageProcessor;
	}
	IEmailMessageProcessor messageProcessor;

	static Lazy<IReadOnlyList<IEmailMessageProcessor>> messageProcessors => new(() =>
		[
			new ShippingBillQueryPendingMessageProcessor(),
			new FileProcessingErrorMessageProcessor(),
			.. ObjectFactory.Get<IEmailMessageProcessorsProvider>("INManifest.IEmailMessageProcessorsProvider").GetMessageProcessors(),
		], LazyThreadSafetyMode.ExecutionAndPublication);
}
