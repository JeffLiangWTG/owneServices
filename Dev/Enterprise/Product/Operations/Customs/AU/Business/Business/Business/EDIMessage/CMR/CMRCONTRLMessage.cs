using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CONTRL;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCONTRLMessage : CMRIncomingMessage
	{
		public CMRCONTRLMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
		}

		public override ZString GetReport()
		{
			var outgoingInterchange = EM_LinkedObject as EDIInterchange;
			var result = ZString.Empty;
			if (outgoingInterchange == null)
			{
				var cONTRLMessage = (CONTRLMessage)GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet());
				if (cONTRLMessage.UCI.Count > 0)
				{
					var firstUCI = cONTRLMessage.UCI[0];
					var senderElements = firstUCI.InterchangeSender;
					var recipientElements = firstUCI.InterchangeRecipient;
					if (senderElements != null && recipientElements != null)
					{
						var query = new ZQuery();
						query.AddToFilter(EDIInterchangeSchema.EI_From, senderElements.SenderIdentification);
						query.AddToFilter(EDIInterchangeSchema.EI_To, recipientElements.RecipientIdentification);
						query.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, firstUCI.InterchangeControlReference);
						query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.CMR);
						outgoingInterchange = Factory.LoadTop1<EDIInterchange>(query);
					}
				}
			}

			if (outgoingInterchange == null)
			{
				result = "Outgoing interchange not found.\r\n\r\n";
			}
			else
			{
				if (EM_Status == EDIMessage.Status.Failed || EM_Status == EDIMessage.Status.Error)
				{
					result = "The outgoing interchange had errors:\r\n\r\n";
				}

				result += new CONTRLInterchangeFormatter().GetInterchangeWithErrorPointers(outgoingInterchange, EM_MessageText, new Edifact.UNOCCMRCharacterSet());
			}

			return result;
		}
	}
}
