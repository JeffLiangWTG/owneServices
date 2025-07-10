using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CustomsAndExciseReportInboundMessageProcessor<TDataProvider> : BranchCustomsApplicationTypeMessageProcessor
	{
		public CustomsAndExciseReportInboundMessageProcessor(LoggingInformation logger, Type jsonObjectType) : base(logger)
		{
			this.jsonObjectType = jsonObjectType;
		}
		protected readonly Type jsonObjectType;

		protected sealed override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage baseMessage)
		{
			var message = (CustomsAndExciseReportInboundMessage)baseMessage;
			if (message.EM_Status == EDIMessage.Status.PreProcessedOK)
			{
				var provider = GetDataProvider(message);

				if (provider != null)
				{
					var interpreterType = MessageInterpreterType;
					if (interpreterType != null && interpreterType.IsSubclassOfRawGeneric(typeof(BaseInboundMessageInterpreter<>)) && !message.EM_MessageText.IsEmpty && message.EM_LinkedObject != null)
					{
						var interpreter = (BaseInboundMessageInterpreter<TDataProvider>)Activator.CreateInstance(interpreterType, message, provider);
						message.EM_MessageInterpretation = interpreter.GetInterpretation();
					}

					var isFailure = GetIsFailure();
					SendEmailNotification(message, isFailure);
					message.EM_Status = EDIMessage.Status.ProcessedOK;

					var outgoingMessage = message.EM_LinkedObject as CustomsAndExciseReportOutboundMessage;
					var messageTypeList = outgoingMessage.Factory.GetCachedValue<CustomsAndExciseReportTypeList>();
					UpdateMessageInterpretationAndAttachEdoc(outgoingMessage, $"{messageTypeList.GetDescriptionFromCode(message.EM_MessageType)}", provider);
					outgoingMessage.EM_Status = isFailure ? EDIMessage.Status.Failed : EDIMessage.Status.Received;
				}
			}
		}

		void UpdateMessageInterpretationAndAttachEdoc(CustomsAndExciseReportMessage message, string description, TDataProvider provider)
		{
			if (provider is IXlsxProvider xlsxProvider)
			{
				using var stream = XlsConverter.ToXlsxStream(xlsxProvider, description, out var fileFormat);
				var fileName = $"{description}-{message.EM_ApplicationReference}.{fileFormat.ToString().ToLower()}";
				var docManagerInfo = message.DocManagerInfo;
				docManagerInfo.AddFileOrDocument(stream.ReadFully(), fileName, Core.Constants.RefDocTypes.CustomsFormattedFile);
				docManagerInfo.Save();
			}
		}

		protected virtual bool GetIsFailure() => false;

		protected virtual Type MessageInterpreterType => null;

		protected internal TDataProvider GetDataProvider(CustomsAndExciseReportInboundMessage message)
		{
			return message.GetDataProvider<TDataProvider>(jsonObjectType, () => message.EM_Status = EDIMessage.Status.Failed);
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsAndExcise;

		protected override IRegistryItem GetEmailGroupRegistryItem() => IECustomsDataRegistry.Instance.SendCustomsAndExciseReportAcknowledgements;

		void SendEmailNotification(CustomsAndExciseReportInboundMessage incomingMessage, bool isFailure = true)
		{
			if (incomingMessage.EM_LinkedObject is CustomsAndExciseReportOutboundMessage outgoingMessage)
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(
					factory: incomingMessage.Factory,
					relatedJob: outgoingMessage,
					messageTypeInSubject: GetEmailSubject(incomingMessage),
					body: GetEmailNotification(incomingMessage),
					isFailure: isFailure,
					branchForEmailLogo: outgoingMessage.Branch,
					sourceBusinessObject: outgoingMessage,
					getEmailAddressToSendTo: () => GetEmailAddressToSendToFromQueuedUser(outgoingMessage)
				);
			}
		}

		protected virtual string GetEmailNotification(CustomsAndExciseReportInboundMessage message) => message.EM_MessageInterpretation;

		protected virtual string GetEmailSubject(CustomsAndExciseReportInboundMessage message) => $"{MessageFriendlyName} ({message.EM_MessageType})";
	}
}
