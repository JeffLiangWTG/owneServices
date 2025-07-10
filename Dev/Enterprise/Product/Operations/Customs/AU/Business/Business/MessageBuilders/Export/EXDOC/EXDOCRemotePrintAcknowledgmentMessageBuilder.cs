using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCRemotePrintAcknowledgmentMessageBuilder
	{
		public EXDOCRemotePrintAcknowledgmentMessageBuilder(QuarantineExDocHeader exDocHeader, string inboundMessageId)
		{
			this.exDocHeader = exDocHeader;
			sancrtMessage = new SANCRTMessage();
			this.inboundMessageId = inboundMessageId;
		}

		public ZString MessageText => sancrtMessage.ToString(new Edifact.UNOBCharacterSet());

		public RFPMessage GenerateMessage()
		{
			GenerateUNH();
			GenerateBGM();
			GenerateUNT();
			RFPMessage message = exDocHeader.Factory.New<RFPMessage>();
			message.EM_LinkedObject = exDocHeader;
			message.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.RPA;
			message.EM_MessageText = MessageText;
			return message;
		}

		void GenerateUNH()
		{
			EXDOCMessageUtilities.PopulateUNH(sancrtMessage.UNH.InstantiateAChildAndAddItToChildrenCollection(),
				MessageTypeList.InternationalMovementOfGoodsGovernmentalRegulatoryMessage,
				MessageVersionNumberList.DraftVersionUnEdifactDirectory,
				MessageReleaseNumberList.Release1997B,
				ControllingAgencyList.UnEceTradeWp4,
				"RF0801");
		}

		void GenerateBGM()
		{
			EXDOCMessageUtilities.PopulateBGM(sancrtMessage.BGM.InstantiateAChildAndAddItToChildrenCollection(),
				DocumentMessageNameCodedList.GetFromString(string.Empty),
				CodeListResponsibleAgencyCodedList.GetFromString(string.Empty),
				string.Empty,
				inboundMessageId,
				MessageFunctionCodedList.GetFromString(EXDOCMessageTypeCodes.Codes.RPA),
				ResponseTypeCodedList.GetFromString(string.Empty));
		}

		void GenerateUNT()
		{
			SegmentGroup21 group21 = sancrtMessage.Group21.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateUNT(group21.UNT.InstantiateAChildAndAddItToChildrenCollection(), sancrtMessage.CountIncludingUNT.ToString());
		}

		internal readonly SANCRTMessage sancrtMessage;
		readonly QuarantineExDocHeader exDocHeader;
		readonly string inboundMessageId;
	}
}
