using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral
{
	public class GenralEdiMessage : EDIMessage
	{
		public GenralEdiMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static GenralEdiMessage MakeNewOutboundFromPayload(ZString payload, ZString recipientPima, BusinessObjectFactory factory, ZString sendingProfilePima, bool isPreformattedIntoLInesOf70 = false, string commonAccessReference = "")
		{
			var edifact = new GenralMessageGenerator().MakeMessageToParticipant(payload, new UkCharSet(), isPreformattedIntoLInesOf70, commonAccessReference);
			var genralEdiMessage = factory.New<GenralEdiMessage>();
			genralEdiMessage.EM_MessageText = edifact;
			genralEdiMessage.EM_ApplicationReference = recipientPima;  // On saving, the CGI service task will find this, package it into an inerchange, and send it to this recipient.
			genralEdiMessage.EM_MessageInterpretation = string.Format(@"<html>
<body style='font-family: arial;'>
<h3>GENRAL message (outbound)</h3>
<p>Recipient: {0}<br/>
Purpose: {1}<br/>
Payload:</p>
<p><b><pre><i>{2}</i></pre></b></p>
</body>
</html>",
					recipientPima,
					genralEdiMessage.EM_MessageSubType,
					payload);
			genralEdiMessage.EM_MessageOwner = sendingProfilePima;
			genralEdiMessage.EM_Status = EDIMessage.Status.Queued;
			genralEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			genralEdiMessage.MessageNumberStrategy = new GbMessageNumberStrategy(genralEdiMessage.Factory, ApplicationCodeList.Codes.GbCcsuk);
			return genralEdiMessage;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			EM_MessageType = GenralMessageGenerator.GenralMessageCodeShortForMessageType; // GEN
			EM_MessageSubType = GenralPurpose.Codes.Text;
		}

		public GenralEdiMessage LinkedMessage
		{
			get
			{
				return HasLinkedMessage ? Factory.Load<GenralEdiMessage>(EM_LinkUniqueID) : null;
			}
		}

		public ZBool HasLinkedMessage { get { return !EM_LinkUniqueID.IsEmpty && EM_LinkTable == EDIMessage.Schema.TableName; } }

		protected override ZString HumanReadableNameCore
		{
			get { return "GENRAL message " + EM_MessageNum; }
		}

		public static ZString ConvertLucasGenralPima(EDIMessage inboundMessage)
		{
			var senderPima = inboundMessage.Interchange.EI_From;
			if (senderPima.StartsWith("CUKCTM", StringComparison.OrdinalIgnoreCase) && !senderPima.Contains("/", StringComparison.OrdinalIgnoreCase))
			{
				// see DEP script page 37
				var list = RefCusCodeListTypes.GetCachedList(inboundMessage.Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, ZDateTime.Today);
				var lucasPima = (from ICodeDescription p in list where p.Code.StartsWith(senderPima, StringComparison.OrdinalIgnoreCase) select p).FirstOrDefault();
				return lucasPima != null ? lucasPima.Code : senderPima.ToString();
			}
			return senderPima;  // bounce back to sender
		}

		public override void OnSaving()
		{
			base.OnSaving();
			EM_MessageText = EM_MessageText.Replace(GbTransmissionMessageGenerator.SysCarPlaceHolder, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(this));
		}

		public NonPersistentGenralEdiMessageForNew GetReplyMessage()
		{
			var newMessage = new NonPersistentGenralEdiMessageForNew(Factory);
			try
			{
				newMessage.IsRecipientShed = false;
				newMessage.IsRecipientRollYourOwn = true;
				newMessage.SendingProfile = Interchange.EI_To.Left(CcsukConstants.PimaMaxLength);
				newMessage.Pima = Interchange.EI_From.Left(newMessage.PimaInfo.MaxLength);
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }
			newMessage.Payload = string.Format("Re: your message {0}/{1} dated {2}{3}", EM_MessageNum, Interchange.EI_InterchangeNum, Interchange.EI_SystemCreateTimeUtc, System.Environment.NewLine);
			return newMessage;
		}

		internal static List<string[]> SplitLargePayloadIntoManageableFractions(string[] linesOfTextMessage)
		{
			var pageIntroText = "Part Message - {0} of {1}";
			var messageContinuesText = "Continues in next message...";
			var finalFooterTExt = "End of Message Set";
			var numberOfLines = linesOfTextMessage.Length;
			var finalResultList = new List<string[]>();
			var blockOf18 = new List<string>();
			var messageIndex = 1;
			var totalMessagesRequired = Math.Ceiling(numberOfLines / (double)18);
			// Break every 18 lines	and add header and footer
			for (int lineNum = 1; lineNum < numberOfLines + 1; lineNum++)
			{
				blockOf18.Add(linesOfTextMessage[lineNum - 1]);
				if (lineNum % 18 == 0 || lineNum == numberOfLines)
				{
					// Flush
					blockOf18.Insert(0, string.Format(pageIntroText, messageIndex, totalMessagesRequired));
					blockOf18.Add(lineNum == numberOfLines ? finalFooterTExt : messageContinuesText);
					finalResultList.Add(blockOf18.ToArray());
					blockOf18 = new List<string>();
					messageIndex++;
				}
			}
			return finalResultList;
		}
	}
}
