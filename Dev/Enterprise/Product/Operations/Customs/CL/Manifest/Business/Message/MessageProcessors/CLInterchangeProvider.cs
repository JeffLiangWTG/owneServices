using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLInterchangeProvider : InterchangeProviderBase
	{
		public CLInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count == 1)
			{
				var message = messages.Cast<EDIMessage>().Single();
				if (!message.EM_MessageText.IsEmpty)
				{
					var ei_To = CLInterchangeHelper.GetInterchangeMessageTo(message.EM_MessageType);

					SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, ei_To, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
					interchange.EI_InterchangeNum = message.EM_MessageNum;
					interchange.EI_TransportType = EDIInterchange.TransportType.xT;
					interchange.EI_SessionGUID = ZGuid.NewZGuid();
					interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>()
					{
						["custom.FileName"] = CLInterchangeHelper.GetOutingMessageFileName(message)
					});
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;

					var messageToLog = Res.GetString("723123C2-7C04-44C5-B914-278F9DD7C17F",
						"Message {0} will be discarded as the message text is empty.",
						message.EM_MessageNum);

					message.Notes.AddNew(true, ProcessingLogDescription, messageToLog);
					logger.LogWarning(messageToLog);

					interchange.ContainedMessages.RemoveAll();
					interchange.Delete();
				}
			}
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.Queued;

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;
	}
}
