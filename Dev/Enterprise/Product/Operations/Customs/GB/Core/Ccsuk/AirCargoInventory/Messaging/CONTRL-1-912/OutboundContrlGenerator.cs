using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class OutboundContrlGenerator : CusAwbToInventoryMessageGenerator
	{
		public OutboundContrlGenerator(CcsukTransmissionMessageFunction.CONTRL function)
			: base(function.InboundMessage.Factory)
		{
			messageFunction = function;
		}

		public override ZString MessageInterpretation
		{
			get
			{
				var actionEnglish = new List<string> { "3", "4", "6" }.Contains(messageFunction.ActionCoded) ? "Rejected" : "Accepted";
				return string.Format("{0} <h3>CONTRL Message</h3> <p>{1}</p> <p>{2}</p>", MessagePrettierCss.CSS, actionEnglish, messageFunction.FreeText);
			}
		}

		protected override EDIMessage GetNewMessageCore()
		{
			messageFunction.InboundMessage.Saving += InboundMessage_Saving;
			newOutboundMessage = messageFunction.InboundMessage.Factory.New<GbEDIMessage>();
			return newOutboundMessage;
		}

		protected override ZString SenderPimaCore
		{
			get { return messageFunction.InboundMessage.Interchange.EI_To.Replace("/", ""); }
		}

		public override ZString RecipientPima
		{
			get { return messageFunction.InboundMessage.Interchange.EI_From.Replace("/", ""); }
		}

		public override string MakeMessageText()
		{
			var contrl = MakeContrlAndUnhAndUciAndUcm();
			MakeUcx(contrl);
			if (!messageFunction.FreeText.IsEmpty)
			{
				var ftx = contrl.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("AAA");
				ftx.TextLiteral.FreeTextValue1 = messageFunction.FreeText.Left(70);
				ftx.TextLiteral.FreeTextValue2 = messageFunction.FreeText.SubstringSafe(70, 70);
				ftx.TextLiteral.FreeTextValue3 = messageFunction.FreeText.SubstringSafe(140, 70);
				ftx.TextLiteral.FreeTextValue4 = messageFunction.FreeText.SubstringSafe(210, 70);
				ftx.TextLiteral.FreeTextValue5 = messageFunction.FreeText.SubstringSafe(280, 70);
			}
			var unt = contrl.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unt.NumberOfSegmentsInTheMessage = contrl.CountIncludingUNT.ToString();
			return contrl.ToString(messageFunction.InboundMessage.CharacterSet);
		}

		void MakeUcx(OutboundCONTRLMessage contrl)
		{
			var ucx = contrl.Group1[0].UCX.InstantiateAChildAndAddItToChildrenCollection();
			ucx.ActionCode = messageFunction.ActionCoded;
			ucx.ErrorCode = messageFunction.ErrorCoded;
		}

		OutboundCONTRLMessage MakeContrlAndUnhAndUciAndUcm()
		{
			var contrl = new OutboundCONTRLMessage();
			var unh = MakeUnh(contrl);

			if (messageFunction.InboundMessage != null)
			{
				if (messageFunction.InboundMessage.Interchange != null)
				{
					MakeUCI(contrl);
				}
				if (messageFunction.UnhSegment != null)
				{
					unh.CommonAccessReference = messageFunction.UnhSegment.CommonAccessReference;
					MakeUcm(contrl);
				}
			}
			return contrl;
		}

		UNHSegment MakeUnh(OutboundCONTRLMessage contrl)
		{
			var unh = contrl.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = "CONTRL";
			unh.MessageIdentifier.MessageVersionNumber = "1";
			unh.MessageIdentifier.MessageReleaseNumber = "912";
			unh.MessageIdentifier.ControllingAgency = "UN";
			return unh;
		}

		void MakeUcm(OutboundCONTRLMessage contrl)
		{
			var group1 = contrl.Group1.InstantiateAChildAndAddItToChildrenCollection();
			var ucm = group1.UCM.InstantiateAChildAndAddItToChildrenCollection();
			ucm.ActionCoded = messageFunction.ActionCoded;
			ucm.MessageReferenceNumber = messageFunction.InboundMessage.EM_MessageNum;
			ucm.MessageIdentifier.MessageType = messageFunction.UnhSegment.MessageIdentifier.MessageType;
			ucm.MessageIdentifier.MessageVersionNumber = messageFunction.UnhSegment.MessageIdentifier.MessageVersionNumber;
			ucm.MessageIdentifier.MessageReleaseNumber = messageFunction.UnhSegment.MessageIdentifier.MessageReleaseNumber;
			ucm.MessageIdentifier.ControllingAgency = messageFunction.UnhSegment.MessageIdentifier.ControllingAgency;
			ucm.MessageIdentifier.AssociationAssignedCode = messageFunction.UnhSegment.MessageIdentifier.AssociationAssignedCode;
		}

		void MakeUCI(OutboundCONTRLMessage contrl)
		{
			var uci = contrl.UCI.InstantiateAChildAndAddItToChildrenCollection();
			uci.InterchangeSender.SenderIdentification = messageFunction.InboundMessage.Interchange.EI_From;
			uci.InterchangeRecipient.RecipientIdentification = messageFunction.InboundMessage.Interchange.EI_To;
			uci.InterchangeControlReference = messageFunction.InboundMessage.Interchange.EI_InterchangeNum;
			uci.ActionCoded = messageFunction.ActionCoded;
		}

		void InboundMessage_Saving(EDIMessage inboundMessageToSave)
		{
			if (newOutboundMessage != null)
			{
				newOutboundMessage.EM_LinkTable = inboundMessageToSave.EM_LinkTable;
				newOutboundMessage.EM_LinkUniqueID = inboundMessageToSave.EM_LinkUniqueID;
			}
		}

		readonly CcsukTransmissionMessageFunction.CONTRL messageFunction;
		EDIMessage newOutboundMessage;
	}
}
