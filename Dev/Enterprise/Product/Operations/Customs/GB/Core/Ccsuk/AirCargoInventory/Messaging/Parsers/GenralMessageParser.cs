using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.ChiefFallback;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact.D04A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class GenralMessageParser
	{
		public void ParseMessage(EDIMessage inboundMessage)
		{
			GenralMessage genral = (GenralMessage)(inboundMessage.GetAutoEdifactMessageUsingNamedFactory(CcsukEdifactMessageFactory.Factory, inboundMessage.CharacterSet));
			if (genral != null)
			{
				ParseGenral(inboundMessage, genral);
			}
		}

		public void ParseGenral(EDIMessage inboundMessage, GenralMessage genral)
		{
			string purpose = genral.BGM[0].DocumentMessageName.DocumentNameCode;  // BCM or TXT
			var group1 = genral.Group1[0];
			string senderType = group1.MSG[0].OriginOfMessage;
			var listOfLines = new List<ZString>();
			foreach (FTXSegment ftx in group1.FTX)
			{
				foreach (string line in new string[] { ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2, ftx.TextLiteral.FreeText3, ftx.TextLiteral.FreeText4, ftx.TextLiteral.FreeText5 })
				{
					listOfLines.Add(line);
				}
			}
			var sb = new ZStringBuilder(listOfLines);
			string payload = sb.ToStringWithNewLineBetweenAppends().Trim();
			if (IsLucasEnqury(inboundMessage, payload))
			{
				new LucasGenralEnquiryHandler(genral, inboundMessage, payload).DoAllProcessing();
			}
			else if (IsFallbackAnnouncement(payload, purpose, senderType))
			{
				new FallbackAnnouncementHandler(listOfLines, inboundMessage, senderType).DoAllProcessing();
			}
			else if (IsFallbackStatusUpdate(senderType, purpose, payload))
			{
				new GenralFallbackStatusUpdateHandler(payload, inboundMessage).DoAllProcessing();
				DistributePayloadAndUpdateMessageProperties(senderType, purpose, payload, inboundMessage, "Fallback");
			}
			else
			{
				DistributePayloadAndUpdateMessageProperties(senderType, purpose, payload, inboundMessage, (purpose == GenralPurpose.Codes.Broadcast ? "Broadcast" : "Text"));
			}
		}

		bool IsFallbackAnnouncement(string payload, string purpose, string senderType)
		{
			return purpose == GenralPurpose.Codes.Broadcast && senderType == GenralSender.Codes.CcsUk && payload.Contains(SharedIdentifiers.Fallback);
		}

		bool IsLucasEnqury(EDIMessage inboundMessage, ZString payload)
		{
			// See appendix A of the DEP Test Scripts document. Prefix is CUKCTM98CHF
			ZString cwFakeLucasPima = GBCustomsDataRegistry.Instance.LucasFakePimaForCwTesting.Value;
			return (inboundMessage.Interchange.EI_From.StartsWith("CUKCTM") && !inboundMessage.Interchange.EI_From.StartsWith(ChiefConstants.ChiefCcsukPimaPrefix))
					||
					(!cwFakeLucasPima.IsEmpty && inboundMessage.Interchange.EI_From == cwFakeLucasPima);
		}

		bool IsFallbackStatusUpdate(string senderType, string purpose, string payload)
		{
			return senderType == GenralSender.Codes.CcsUk && purpose == GenralPurpose.Codes.Text && payload.StartsWith("MUCR ");
		}

		internal static void DistributePayloadAndUpdateMessageProperties(string sender, string purpose, string payload, EDIMessage inboundMessage, ZString notificationType)
		{
			var purposes = new GenralPurpose();
			var senders = new GenralSender();
			string senderPima = inboundMessage.Interchange.EI_From;
			string recipientPima = inboundMessage.Interchange.EI_To;
			inboundMessage.EM_GB = RegistryPimaAndBadgeHelper.GetPrimaryBranchPkFromRegistryBasedOnPima(recipientPima, inboundMessage.Factory);

			string subject = string.Format("CCSUK general {0} message from {1} ({2})", purposes.GetDescriptionFromCode(purpose), senderPima, senders.GetDescriptionFromCode(sender));

			string body = string.Format(@"<html>
<body style='font-family: arial;'>
<h3>GENRAL message</h3>
<p>Sender: {0} ({1})<br/>
Recipient: {2}<br/>
Purpose: {3}<br/>
Payload:</p>
<p><b><pre><i>{4}</i></pre></b></p>
<p>Please ensure that the relevant staff receive this notification.</p>
</body>
</html>",
					senderPima, senders.GetDescriptionFromCode(sender),
					recipientPima,
					purposes.GetDescriptionFromCode(purpose),
					payload
					);

			UpdateMessageProperties(purpose, inboundMessage, body, senderPima, recipientPima);

			new CcsukEmailSender(inboundMessage.Factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, inboundMessage).SendEmail(subject, body,
					GBCustomsDataRegistry.Instance.NotificationCcsukGenral, notificationType, inboundMessage.Branch.Company.PK.ToGuid(), inboundMessage.Branch.PK.ToGuid(), Guid.Empty);
		}

		static void UpdateMessageProperties(ZString purpose, EDIMessage inboundMessage, string body, string senderPima, string recipientPima)
		{
			inboundMessage.EM_ApplicationReference = senderPima;
			inboundMessage.EM_MessageInterpretation = body;
			inboundMessage.EM_MessageOwner = recipientPima;
			inboundMessage.EM_MessageType = GenralMessageGenerator.GenralMessageCodeShortForMessageType;
			inboundMessage.EM_MessageSubType = purpose.Left(3);
			inboundMessage.EM_Status = EDIMessage.Status.Received;
		}
	}
}
