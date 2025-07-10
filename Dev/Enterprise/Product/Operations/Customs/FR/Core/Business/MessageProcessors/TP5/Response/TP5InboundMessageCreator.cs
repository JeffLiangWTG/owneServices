using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class TP5InboundMessageCreator : InboundMessageCreator
	{
		protected override List<FREDIMessage> CreateMessagesFromInterchange(EDIInterchange interchange)
		{
			var message = interchange.Factory.New<NCTSFREDIMessage>();
			var root = XDocument.Parse(interchange.EI_BodyText).Root;
			var messageBody = root?.Element("MessageBody");
			var messageRootName = messageBody?.Elements().FirstOrDefault().Name.LocalName ?? ZString.Empty;

			message.EM_MessageSubType = new ZString(messageRootName).SubstringSafe(messageRootName.Length - 4, 3);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			ZString transactionId = root?.Element("EnveloppeMessage")?.Element("transactionId")?.Value ?? ZString.Empty;
			var correlationId = transactionId.Right(10);

			var linkedObject = GetLinkedObject(interchange, correlationId, message.GetCountryCodeSafe());
			if (linkedObject != null)
			{
				message.EM_LinkedObject = linkedObject;
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.Discarded;
				var note = message.Notes.AddNew();
				note.ST_NoteText = $"Couldn't locate NctsHeader using provided correlationId {correlationId}.";
			}

			message.EM_MessageText = interchange.EI_BodyText;
			return new List<FREDIMessage> { message };
		}

		BusinessObject GetLinkedObject(EDIInterchange interchange, ZString correlationId, ZString country)
		{
			CusEntryNumber entryNumber = null;
			if (!correlationId.IsEmpty)
			{
				var entryNumbers = CusEntryNumber.Load(interchange.Factory, CusEntryNumberTypes.EU.CorrelationIdentifier, correlationId, country);
				if (entryNumbers.Length > 1)
				{
					var errorMessageBuilder = new StringBuilder();
					errorMessageBuilder.AppendLine($"More than one CusEntryNum shares the same entry number, they are:");
					foreach (var number in entryNumbers)
					{
						errorMessageBuilder.AppendLine($"CE_EntryNum: {number.CE_EntryNum}, CE_SystemCreateTimeUtc: {number.CE_SystemCreateTimeUtc.ToLongTimeString()}");
					}
					ErrorReporter.ReportOnce("When looking for NCTS jobs by transaction ID, multiple matching NCTS jobs are found.", errorMessageBuilder.ToString());

					var interchangeNumber = interchange.EI_InterchangeNum.Split('.').FirstOrDefault();
					if (!interchangeNumber.IsEmpty)
					{
						foreach (var number in entryNumbers)
						{
							if (number.Parent is NctsHeader header)
							{
								if (header.IsDepartureMovement)
								{
									if (header.MovementHeader.Messages.Cast<EDIMessage>().Any(message => message.EM_InterchangeNumber == interchangeNumber))
									{
										entryNumber = number;
										break;
									}
								}
								else
								{
									if (header.Messages.Cast<EDIMessage>().Any(message => message.EM_InterchangeNumber == interchangeNumber))
									{
										entryNumber = number;
										break;
									}
								}
							}
						}
					}
				}
				else
				{
					entryNumber = entryNumbers.FirstOrDefault();
				}
			}

			return entryNumber?.Parent;
		}
	}
}
