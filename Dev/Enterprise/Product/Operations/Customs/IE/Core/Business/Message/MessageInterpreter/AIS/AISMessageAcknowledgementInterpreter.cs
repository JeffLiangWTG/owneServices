using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class AISMessageAcknowledgementInterpreter : InboundMessageInterpreter<ITransaction>
	{
		public AISMessageAcknowledgementInterpreter(Enterprise.Messaging.Business.EDIMessage message, ITransaction provider) : base(message, provider)
		{
		}

		protected override string Summary
		{
			get
			{
				return Res.GetString(
					"DB613D0D-3681-47F0-94EF-C3FADD773AA2",
					"Message {0} ({1}) was transmitted and the following statuses apply:",
					message.EM_MessageNum,
					factory.GetCachedValue<AISOutgoingMessageTypeList>().GetDescriptionFromCode(message.EM_MessageType)
				);
			}
		}

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.TransactionId, message.EM_ApplicationReference);
			yield return (CommonResStrings.MessageStatus, provider.MessageStatus);
			yield return (Res.GetString("749BDA9E-FCC8-49C8-8BB0-24D980CE1B75", "Transaction ID Status"), provider.TransactionIdStatus);
		}
	}
}
