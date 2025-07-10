using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class BRCInboundMessageCreator : IInboundMessageCreator
	{
		public BRCInboundMessageCreator(LoggingInformation logger)
		{
			this.logger = logger;
		}
		protected readonly LoggingInformation logger;

		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			if (interchange is BREDIInterchange)
			{
				var responseMessage = interchange.EI_BodyText;
				UniversalEventWrapper universalEvent = null;

				if (interchange.EI_TransportType == EDIInterchange.TransportType.xT || interchange.EI_TransportType == EDIInterchange.TransportType.eAdaptor)
				{
					if (responseMessage.Contains("<UniversalInterchange ")
						&& XDocument.Parse(responseMessage)?.Root?.XPathSelectElement((NoResString)"//*[local-name()='UniversalInterchange']//*[local-name()='Body']")?.FirstNode is XElement bodyNode)
					{
						responseMessage = bodyNode.ToString();
					}

					if (responseMessage.Contains("<UniversalEvent "))
					{
						universalEvent = new UniversalEventWrapper(responseMessage);
						if (ExtractResponseMessageAsMessageText)
						{
							responseMessage = universalEvent.GetResponseMessage();
						}
					}
				}

				var (messageType, messageSubType) = GetMessageTypeAndSubType(interchange, responseMessage, universalEvent);

				if (messageType.IsEmpty)
				{
					logger?.LogError($"Interchange {interchange.EI_InterchangeNum}: Can not determine Message Type for the Interchange Message");
				}
				else
				{
					var messageTexts = GetMessageTexts(messageType, messageSubType, responseMessage, universalEvent).ToArray();
					if (messageTexts.Length == 0)
					{
						logger?.LogWarning($"No message has been created for interchange {interchange.EI_InterchangeNum}");
					}
					else
					{
						var sequence = 1;
						foreach (var messageText in messageTexts)
						{
							CreateNewMessage(interchange, messageText, messageType, messageSubType, sequence++);
						}
					}
				}
			}
		}

		protected virtual bool ExtractResponseMessageAsMessageText => true;

		protected virtual bool UseSequenceAsMessageNumber => false;

		protected virtual (ZString Type, ZString SubType) GetMessageTypeAndSubType(EDIInterchange interchange, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			var messageSubType = ZString.Empty;

			switch (universalEventData?.MessageType)
			{
				case MessageConstants.MessageType.BER:
					messageSubType = EDIMessageSubTypeList.Codes.Error;
					break;
				case MessageConstants.MessageType.RES:
					messageSubType = EDIMessageSubTypeList.Codes.Success;
					break;
			}

			return (interchange.EI_InterchangeType, messageSubType);
		}

		protected virtual IEnumerable<ZString> GetMessageTexts(ZString messageType, ZString messageSubType, ZString responseMessage, UniversalEventWrapper universalEventData) => new[] { responseMessage };

		EDIMessage CreateNewMessage(EDIInterchange interchange, ZString messageText, ZString messageType, ZString messageSubType, int sequence)
		{
			var message = interchange.ContainedMessages.AddNew();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.BRCustoms;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = messageText;
			message.EM_GB = GetBranchPK(interchange);
			message.EM_MessageNum = UseSequenceAsMessageNumber ? new ZString(sequence.ToString()) : interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);

			return message;
		}

		ZGuid GetBranchPK(EDIInterchange interchange)
		{
			var branchPK = interchange.EI_GB;
			var companyCountry = interchange.Branch?.Company?.GC_RN_NKCountryCode ?? string.Empty;
			if (companyCountry != Core.Constants.CountryCodes.Brazil && GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Brazil) is GlbBranch randomBranch)
			{
				branchPK = randomBranch.PK;
			}
			return branchPK;
		}
	}
}
