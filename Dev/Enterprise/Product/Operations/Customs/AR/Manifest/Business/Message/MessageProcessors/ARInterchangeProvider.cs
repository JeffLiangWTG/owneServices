using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ARInterchangeProvider : InterchangeProviderBase
	{
		public ARInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count == 1)
			{
				var message = messages.Cast<EDIMessage>().Single();
				if (!message.EM_MessageText.IsEmpty)
				{
					var ei_To = ARMessageProcessorHelper.GetInterchangeMessageTo(message.EM_MessageType);
					SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, ei_To, GlbCompany.CurrentCompany.LicenceKeyIdentifier);

					interchange.EI_SessionGUID = ZGuid.NewZGuid();
					interchange.EI_InterchangeNum = message.EM_MessageNum;
					interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;

					var headerMessage = Res.GetString("08BF0F32-6CEC-4FC6-929A-2F4FB0B2D81C",
						"Message {0} will be discarded for the following reason:",
						message.EM_MessageNum);
					var messageToLog = Res.GetString("1EEB7D82-A2C5-4C16-80A1-5212018A55C2",
						"The Interchange Body is empty even though there are {0} messages.",
						messages.Count.ToString(CultureInfo.InvariantCulture));

					ZStringBuilder stringBuilder = new ZStringBuilder();
					stringBuilder.Append(headerMessage);
					stringBuilder.Append(messageToLog);

					message.Notes.AddNew(true, ProcessingLogDescription, stringBuilder.ToStringWithNewLineBetweenAppends());
					logger.LogWarning(messageToLog);

					interchange.ContainedMessages.RemoveAll();
					interchange.Delete();
				}
			}
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.Queued;

		protected override ZString GetMessageTextToMessageBody(EDIInterchange interchange, EDIMessage message)
		{
			if (message.EM_MessageType == MessageTypes.Codes.ARA)
			{
				return ARMessageProcessorHelper.EnvelopeMessage(message.EM_MessageText);
			}
			return base.GetMessageTextToMessageBody(interchange, message);
		}
	}
}
