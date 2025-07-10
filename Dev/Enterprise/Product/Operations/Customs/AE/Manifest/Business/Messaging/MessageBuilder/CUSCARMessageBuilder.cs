using System.Collections.Generic;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Messages.CUSCAR;

namespace Enterprise.Customs.AE.Manifest.Business;

public class CUSCARMessageBuilder : EDIFACTMessageBuilder<ICUSCARMessageProvider, CUSCARMessage, AEEDIMessage>
{
	public CUSCARMessageBuilder(ICUSCARMessageProvider data) : base(data, MessageSubTypes.Undefined, AECharacterSet.New())
	{
		messageInterpretation = new CUSCARMessageInterpretation(edifactMessage, characterSet);
	}
	readonly CUSCARMessageInterpretation messageInterpretation;

	protected override AEEDIMessage PopulateMessagesReturningResult()
	{
		PopulateEdifactMessage();
		var messageToSend = (AEEDIMessage)data.Messages.AddNew(typeof(AEEDIMessage));

		messageToSend.EM_MessageText = Utils.GetFormattedEDIFactText(edifactMessage.ToString(characterSet));
		messageToSend.EM_MessageInterpretation = messageInterpretation.ToHtml();

		if (ManifestMessageExtensions.AeBranchForManifestMessageFromRegistry(messageToSend.Factory) is { } branchForManifestSubmission)
		{
			messageToSend.EM_GB = branchForManifestSubmission.PK;
		}

		return messageToSend;
	}

	protected override void PopulateEdifactMessage()
	{
		CommonMessageBuilder.PopulateUNHSegment(edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection(), data.MessageHeader);
		SegmentBuilder.PopulateBGMSegment(() => edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection(), data.MessageDetails);
		CommonMessageBuilder.PopulateDTMSegment(() => edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection(), data.BillIssueDate);
		SegmentBuilder.PopulateLOCSegment(() => edifactMessage.LOC.InstantiateAChildAndAddItToChildrenCollection(), data.BillIssueLocation);
		PopulateSegmentGroup1();
		PopulateSegmentGroup2();
		SegmentBuilder.PopulateFTXSegment(() => edifactMessage.FTX.InstantiateAChildAndAddItToChildrenCollection(), data.DocumentUpdates);
		PopulateGEISegment();
		PopulateSegmentGroup5();
		PopulateSegmentGroup7();
		PopulateUNTSegment();
	}

	void PopulateSegmentGroup1()
	{
		foreach (var reference in data.References)
		{
			var segmentGroup1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			SegmentBuilder.PopulateRFFSegment(() => segmentGroup1.RFF.InstantiateAChildAndAddItToChildrenCollection(), reference);
		}
	}

	void PopulateSegmentGroup2()
	{
		foreach (var party in data.RelatedParties)
		{
			var segmentGroup2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			SegmentBuilder.PopulateNADSegment(() => segmentGroup2.NAD.InstantiateAChildAndAddItToChildrenCollection(), party);
		}
	}

	void PopulateGEISegment()
	{
		var negotiability = data.IsNegotiable ? NegotiableBills : NonNegotiableBills;
		var gEISegment = edifactMessage.GEI.InstantiateAChildAndAddItToChildrenCollection();
		gEISegment.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(negotiability);
	}
	const string NegotiableBills = "NEG";
	const string NonNegotiableBills = "NON";

	void PopulateSegmentGroup5()
	{
		foreach (var containerInfo in data.ContainerInfos)
		{
			var segmentGroup5 = edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection();
			SegmentBuilder.PopulateEQDSegment(() => segmentGroup5.EQD.InstantiateAChildAndAddItToChildrenCollection(), containerInfo.ContainerDetails);
			SegmentBuilder.PopulateTSRSegment(() => segmentGroup5.TSR.InstantiateAChildAndAddItToChildrenCollection(), containerInfo.ServiceRequirements);

			var mEASegment = segmentGroup5.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mEASegment.ValueRange.MeasurementUnitCode = MeasurementUnitCodeList.GetFromString(UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram);
			mEASegment.ValueRange.Measure = containerInfo.GoodsWeightInKgs.ToString();

			var selSegment = segmentGroup5.SEL.InstantiateAChildAndAddItToChildrenCollection();
			selSegment.TransportUnitSealIdentifier = containerInfo.SealNumber;

			var tMPProvider = () =>
			{
				var segmentGroup6 = segmentGroup5.Group6.InstantiateAChildAndAddItToChildrenCollection();
				return segmentGroup6.TMP.InstantiateAChildAndAddItToChildrenCollection();
			};
			SegmentBuilder.PopulateTMPSegment(tMPProvider, containerInfo.Temperature);
		}
	}

	void PopulateSegmentGroup7()
	{
		var billInfo = data.BillInfo;
		if (billInfo == null)
		{
			return;
		}
		var segmentGroup7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();

		var cNISegment = segmentGroup7.CNI.InstantiateAChildAndAddItToChildrenCollection();
		cNISegment.ConsolidationItemNumber = ConsolidationItemNumber;

		var totalHouseBill = billInfo.TotalHouseBills;
		var cNTSegment = segmentGroup7.CNT.InstantiateAChildAndAddItToChildrenCollection();
		cNTSegment.Control.ControlTotalQuantity = totalHouseBill.ToString();

		PopulateSegmentGroup8(segmentGroup7, billInfo.BillDetails);
	}
	const string ConsolidationItemNumber = "1";

	void PopulateSegmentGroup8(SegmentGroup7 segmentGroup7, IConsignmentDetailsProvider billDetails)
	{
		if (billDetails == null)
		{
			return;
		}

		var segmentGroup8 = segmentGroup7.Group8.InstantiateAChildAndAddItToChildrenCollection();
		foreach (var monetaryAmount in billDetails.MonetaryAmounts)
		{
			SegmentBuilder.PopulateMOASegment(() => segmentGroup8.MOA.InstantiateAChildAndAddItToChildrenCollection(), monetaryAmount);
		}

		foreach (var location in billDetails.Locations)
		{
			SegmentBuilder.PopulateLOCSegment(() => segmentGroup8.LOC.InstantiateAChildAndAddItToChildrenCollection(), location);
		}

		var rff = segmentGroup8.RFF.InstantiateAChildAndAddItToChildrenCollection();
		rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.TechnicalDocumentNumber;

		if (!string.IsNullOrEmpty(billDetails.ManifestNature))
		{
			var gei = segmentGroup8.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(billDetails.ManifestNature);
		}

		PopulateSegmentGroup11(segmentGroup8, billDetails.Parties);
		PopulateSegmentGroup14(segmentGroup8, billDetails.Packs);
	}

	void PopulateSegmentGroup11(SegmentGroup8 segmentGroup8, IReadOnlyCollection<IPartyFromOrgAddressProvider> parties)
	{
		if (data.IsNegotiable)
		{
			var segmentGroup11 = segmentGroup8.Group11.InstantiateAChildAndAddItToChildrenCollection();
			SegmentBuilder.PopulateNADSegment(segmentGroup11.NAD.InstantiateAChildAndAddItToChildrenCollection, data.Payer);
		}

		foreach (var party in parties)
		{
			var segmentGroup11 = segmentGroup8.Group11.InstantiateAChildAndAddItToChildrenCollection();
			SegmentBuilder.PopulateNADSegment(() => segmentGroup11.NAD.InstantiateAChildAndAddItToChildrenCollection(), party);
			PopulateSegmentGroup12(segmentGroup11, party.ContactCommunication);
		}
	}

	void PopulateSegmentGroup12(SegmentGroup11 segmentGroup11, IPartyContactCommunicationProvider contactCommunication)
	{
		var segmentGroup12 = segmentGroup11.Group12.InstantiateAChildAndAddItToChildrenCollection();
		var cTASegment = segmentGroup12.CTA.InstantiateAChildAndAddItToChildrenCollection();
		cTASegment.ContactDetails.ContactIdentifier = ContactIdentifier;
		SegmentBuilder.PopulateCOMSegment(() => segmentGroup12.COM.InstantiateAChildAndAddItToChildrenCollection(), contactCommunication);
	}
	const string ContactIdentifier = "COM";

	void PopulateSegmentGroup14(SegmentGroup8 segmentGroup8, IReadOnlyCollection<IGoodsInfoProvider> packs)
	{
		foreach (var pack in packs)
		{
			var segmentGroup14 = segmentGroup8.Group14.InstantiateAChildAndAddItToChildrenCollection();
			SegmentBuilder.PopulateGIDSegment(() => segmentGroup14.GID.InstantiateAChildAndAddItToChildrenCollection(), pack.Goods);
			SegmentBuilder.PopulateFTXSegment(() => segmentGroup14.FTX.InstantiateAChildAndAddItToChildrenCollection(), pack.GoodsDescription);

			foreach (var measurement in pack.Measurements)
			{
				SegmentBuilder.PopulateMEASegment(() => segmentGroup14.MEA.InstantiateAChildAndAddItToChildrenCollection(), measurement);
			}

			SegmentBuilder.PopulateSGPSegment(() => segmentGroup14.SGP.InstantiateAChildAndAddItToChildrenCollection(), pack.GoodsContainer);
			SegmentBuilder.PopulatePCISegment(() => segmentGroup14.PCI.InstantiateAChildAndAddItToChildrenCollection(), pack.GoodsMarksDescription);
			if (!string.IsNullOrEmpty(pack.CustomsGoodsIdentifier))
			{
				var cSTSegment = segmentGroup14.CST.InstantiateAChildAndAddItToChildrenCollection();
				cSTSegment.CustomsIdentityCodes1.CustomsGoodsIdentifier = pack.CustomsGoodsIdentifier;
			}

			SegmentBuilder.PopulateLOCSegment(() => segmentGroup14.LOC.InstantiateAChildAndAddItToChildrenCollection(), pack.OriginCountry);
		}
	}

	void PopulateUNTSegment()
	{
		var uNTSegment = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
		uNTSegment.NumberOfSegmentsInAMessage = edifactMessage.CountIncludingUNT.ToString();
		uNTSegment.MessageReferenceNumber = data.MessageHeader.ReferenceNumber;
	}
}
