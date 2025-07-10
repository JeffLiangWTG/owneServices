using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Edifact.D96A.Messages.CUSREP;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class RNSRequestMessageBuilder : D96AMessageBuilder<IRNSRequest, CUSREPMessage, RNSRequestMessage>
	{
		public RNSRequestMessageBuilder(IRNSRequest rnsRequestor, DocumentMessageNameCodedList documentMessageNameCoded)
			: base(rnsRequestor, MessageSubTypes.Request)
		{
			this.documentMessageNameCoded = documentMessageNameCoded;
		}

		protected override ZString GetMessageSubType()
		{
			return documentMessageNameCoded == DocumentMessageNameCodedList.PreviousCustomsDocumentMessage
							 ? RNSMessageTypes.Codes.StatusQuery : RNSMessageTypes.Codes.ArrivalCertification;
		}

		protected override void PopulateEdifactMessage()
		{
			#region PopulateUNH

			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateUNH(
				unh,
				EDIMessage.MessageNumberPlaceHolder,
				Enterprise.Edifact.D96A.Elements.MessageTypeList.CustomsConveyanceReportMessage,
				"D",
				"96A",
				ControllingAgencyList.UnEceTradeWp4UnitedNationsStandardMessagesUnsm);

			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region Message sub type

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateBGM(bgm, string.Empty, string.Empty, MessageFunctionCode, documentMessageNameCoded);
			interpretation.AddNewSegmentInterpretation(bgm, documentMessageNameCoded == DocumentMessageNameCodedList.PreviousCustomsDocumentMessage ?
				RNSMessageTypes.Descriptions.StatusQuery : RNSMessageTypes.Descriptions.ArrivalCertification, string.Empty);

			#endregion

			#region Date Of Arrival

			if (data.DateOfArrival.IsValid)
			{
				var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateDTM203(dtm, DateTimePeriodQualifierList.ArrivalDateTimeEstimated, data.DateOfArrival);
				interpretation.AddNewSegmentInterpretation(dtm, () => data.DateOfArrival.ToString("g"));
			}

			#endregion

			#region Cargo Control Number

			var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			if (!data.CargoControlNumber.IsEmpty)
			{
				var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateRFF(rff, ReferenceQualifierList.CustomsDeclarationNumber, data.CargoControlNumber);
				interpretation.AddNewSegmentInterpretation(rff, () => data.CargoControlNumber);
			}

			#endregion

			#region Transaction Number

			if (!data.TransactionNumber.IsEmpty)
			{
				var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateRFF(rff, ReferenceQualifierList.TransactionReferenceNumber, data.TransactionNumber);
				interpretation.AddNewSegmentInterpretation(rff, () => data.TransactionNumber);
			}

			#endregion

			#region Office Code

			var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			if (!data.OfficeCode.IsEmpty)
			{
				var loc = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D96AMessageUtilities.PopulateLOC(loc, PlaceLocationQualifierList.LocationOfGoods, data.OfficeCode, data.SubLocationCode.IsEmpty ? null : CodeListQualifierList.CustomsWarehouse, data.SubLocationCode, string.Empty, string.Empty);

				var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
				locInterpretation.AddElementInterpretation(() => data.OfficeCode);
				if (!data.SubLocationCode.IsEmpty)
				{
					locInterpretation.AddElementInterpretation(() => data.SubLocationCode);
				}
			}

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D96AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unh.MessageReferenceNumber);

			#endregion
		}

		readonly DocumentMessageNameCodedList documentMessageNameCoded;
	}
}
