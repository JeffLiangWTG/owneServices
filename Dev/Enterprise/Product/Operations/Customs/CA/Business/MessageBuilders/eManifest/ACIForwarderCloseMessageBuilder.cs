using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Edifact.D11B.Messages.GOVCBR;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	class ACIForwarderCloseMessageBuilder : D11BMessageBuilder<IACIForwarderCloseProvider, GOVCBRMessage, ACIForwarderCloseMessage>
	{
		public ACIForwarderCloseMessageBuilder(IACIForwarderCloseProvider data, MessageSubTypes messageSubType)
			: base(data, messageSubType)
		{
		}

		#region Overrides of EDIFACTMessageBuilder

		protected override void PopulateEdifactMessage()
		{
			#region PopulateUNH

			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateUNH(
				unh,
				Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder,
				"GOVCBR", "D", "11B", "UN", CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.Value ? "ACIHCM" : string.Empty);

			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region PopulateBGM

			var previousCCN = messageSubType == MessageSubTypes.Withdraw ?
				data.PreviousCCNForWithdraw.Replace(" ", "") :
				data.PreviousCCN.Replace(" ", "");

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.GeneralCargoSummaryManifestReport, previousCCN, MessageFunctionCode);

			var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
			bgmInterpretation.AddElementInterpretation(() => DocumentNameCodeList.GeneralCargoSummaryManifestReport);
			bgmInterpretation.AddElementInterpretation(() => previousCCN);
			bgmInterpretation.AddElementInterpretation(() => MessageFunctionCode);

			#endregion

			#region PopulateRFF

			var rff = edifactMessage.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, data.JobIdentification);
			interpretation.AddNewSegmentInterpretation(rff, () => data.JobIdentification);

			#endregion

			#region PopulateNAD

			var group7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();
			var nad = group7.NAD.InstantiateAChildAndAddItToChildrenCollection();
			var carrierCode = messageSubType == MessageSubTypes.Withdraw ? data.CarrierCodeForWithdraw : data.CarrierCode;
			D11BMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.FreightForwarder, carrierCode);
			interpretation.AddNewSegmentInterpretation(nad, () => carrierCode);

			#endregion

			#region PopulateDOC

			if (messageSubType != MessageSubTypes.Withdraw)
			{
				foreach (ZString houseCCN in data.RelatedCCNs)
				{
					var relatedCCN = houseCCN.Replace(" ", "");

					var group9 = edifactMessage.Group9.InstantiateAChildAndAddItToChildrenCollection();
					var doc = group9.DOC.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.CustomsManifest, relatedCCN);
					interpretation.AddNewSegmentInterpretation(doc, () => relatedCCN);
				}
			}

			#endregion

			#region Group 15 PopulateAJT

			if (!data.AmendmentReason.IsEmpty)
			{
				var group15 = edifactMessage.Group15.InstantiateAChildAndAddItToChildrenCollection();
				var ajt = group15.AJT.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateAJT(ajt, AdjustmentReasonDescriptionCodeList.MutuallyDefined, data.AmendmentReason);
				interpretation.AddNewSegmentInterpretation(ajt, () => data.AmendmentReason);
			}

			#endregion

			#region PopulateUNS D

			var uns = edifactMessage.UNS1.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateUNS(uns, "D");
			interpretation.AddMandatoryTriggerSegmentInterpretation(uns);

			#endregion

			#region PopulateHYN

			var hyn = edifactMessage.HYN.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateHYN(hyn, HierarchyObjectCodeQualifierList.NoHierarchy);
			interpretation.AddMandatoryTriggerSegmentInterpretation(hyn);

			#endregion

			#region PopulateUNS S

			uns = edifactMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateUNS(uns, "S");
			interpretation.AddMandatoryTriggerSegmentInterpretation(uns);

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unh.MessageReferenceNumber);

			#endregion
		}

		protected override ZString GetMessageSubType()
		{
			return messageSubType == MessageSubTypes.Request ? new ZString(MessageSubTypeCodes.Codes.Change) : base.GetMessageSubType();
		}

		#endregion
	}
}
