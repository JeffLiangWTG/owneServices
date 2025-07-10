using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Edifact.D11B.Messages.GOVCBR;
using Enterprise.Edifact.D11B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	class ACIHouseBillMessageBuilder : D11BMessageBuilder<IACIHouseBillProvider, GOVCBRMessage, ACIHouseBillMessage>
	{
		public ACIHouseBillMessageBuilder(IACIHouseBillProvider data, MessageSubTypes messageSubType)
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
				"GOVCBR", "D", "11B", "UN", CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.Value ? "ACIHG" : string.Empty);

			interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

			#endregion

			#region PopulateBGM

			var houseCCN = data.HouseCCN.Replace(" ", "");

			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.HouseBillOfLading, houseCCN, MessageFunctionCode);

			var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
			bgmInterpretation.AddElementInterpretation(() => DocumentNameCodeList.HouseBillOfLading);
			bgmInterpretation.AddElementInterpretation(() => houseCCN);
			bgmInterpretation.AddElementInterpretation(() => MessageFunctionCode);

			#endregion

			#region PopulateIFD
			// future use
			#endregion

			#region PopulateRFF

			var rff = edifactMessage.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, data.JobIdentification);
			interpretation.AddNewSegmentInterpretation(rff, () => data.JobIdentification);

			if (!data.UCR.IsEmpty)
			{
				rff = edifactMessage.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.UniqueConsignmentReferenceNumber, data.UCR);
				interpretation.AddNewSegmentInterpretation(rff, () => data.UCR);
			}

			#endregion

			#region Group 7 Secondary Notify Parties

			foreach (ISecondaryNotifyParty secondaryNotify in data.SecondaryNotifyParties)
			{
				var group7 = edifactMessage.Group7.InstantiateAChildAndAddItToChildrenCollection();
				var nad = group7.NAD.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateNAD(nad, secondaryNotify.SecondaryNotifyType, secondaryNotify.Identifier);

				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);
				nadInterpretation.AddElementInterpretation(() => secondaryNotify.SecondaryNotifyType);
				nadInterpretation.AddElementInterpretation(() => secondaryNotify.Identifier);

				var ifd = group7.IFD.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateIFD(ifd, secondaryNotify.NoticeType);

				interpretation.AddNewSegmentInterpretation(ifd, () => secondaryNotify.NoticeType);
			}

			#endregion

			#region Group 9 PopulateDOC

			SegmentGroup9 group9;
			DOCSegment doc;

			if (!data.MovementType.IsEmpty)
			{
				group9 = edifactMessage.Group9.InstantiateAChildAndAddItToChildrenCollection();
				doc = group9.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.StatusInformation, DocumentStatusCodeList.GetFromString(data.MovementType));

				interpretation.AddNewSegmentInterpretation(doc, () => data.MovementType);
			}

			group9 = edifactMessage.Group9.InstantiateAChildAndAddItToChildrenCollection();
			doc = group9.DOC.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.CustomsManifest, data.PrimaryCCN.Replace(" ", ""));

			interpretation.AddNewSegmentInterpretation(doc, () => data.PrimaryCCN);

			#endregion

			#region Group 13 Business to Business comments

			if (!data.B2BComments.IsEmpty)
			{
				var group13 = edifactMessage.Group13.InstantiateAChildAndAddItToChildrenCollection();
				var rcs = group13.RCS.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateRCS(rcs, SectorAreaIdentificationCodeQualifierList.Government);
				var ftx = group13.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.AdditionalInformation, data.B2BComments);

				interpretation.AddNewSegmentInterpretation(rcs, () => SectorAreaIdentificationCodeQualifierList.Government);
				interpretation.AddNewSegmentInterpretation(ftx, () => data.B2BComments);
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

			#region Group 32 PopulateTDT

			var group32 = edifactMessage.Group32.InstantiateAChildAndAddItToChildrenCollection();
			var tdt = group32.TDT.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.AtBorder, data.TransportMode);

			interpretation.AddNewSegmentInterpretation(tdt, () => data.TransportMode);

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

			var group138 = edifactMessage.Group138.InstantiateAChildAndAddItToChildrenCollection();

			#region PopulateCNI

			var cni = group138.CNI.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateCNI(cni, "1");

			interpretation.AddMandatoryTriggerSegmentInterpretation(cni);

			#endregion

			#region PopulateSTS

			var sts = group138.STS.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateSTS(sts, StatusDescriptionCodeList.GetFromString(data.IsConsolidatedCargo ? "1" : "0"));

			interpretation.AddNewSegmentInterpretation(sts, Res.GetString("29c55815-aa08-43e4-a43f-9171175edadc", "Is a consolidation"), data.IsConsolidatedCargo);

			#endregion

			#region PopulateMEA

			if (data.Volume >= 0.5m)
			{
				var mea = group138.MEA.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateMEAasInteger(mea, MeasurementPurposeCodeQualifierList.ConsignmentMeasurement, data.Volume, data.VolumeUOM);
				var meaInterpretation = interpretation.AddNewSegmentInterpretation(mea);
				meaInterpretation.AddElementInterpretation(() => data.Volume.Round(0));
				meaInterpretation.AddElementInterpretation(() => data.VolumeUOM);
			}

			#endregion

			#region PopulateHAN

			if (!data.SpecialHandlingInstructions.IsEmpty)
			{
				var han = group138.HAN.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateHAN(han, data.SpecialHandlingInstructions);

				interpretation.AddNewSegmentInterpretation(han, () => data.SpecialHandlingInstructions);
			}

			#endregion

			#region Populate Consignee

			if (data.Consignee != null)
			{
				var group139 = group138.Group139.InstantiateAChildAndAddItToChildrenCollection();
				Func<NADSegment> nadFunc = () =>
				{
					return group139.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};
				PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.Consignee, () => data.Consignee);

				if (!data.Consignee.E2_Contact.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCTA(cta, data.Consignee.E2_Contact.Left(70));
					interpretation.AddNewSegmentInterpretation(cta, () => data.Consignee.E2_Contact.Left(70));
				}

				if (!data.Consignee.E2_Phone.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group140.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, data.Consignee.E2_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, () => data.Consignee.E2_Phone);
				}
			}

			#endregion

			#region Populate Shipper

			if (data.Shipper != null)
			{
				var group139 = group138.Group139.InstantiateAChildAndAddItToChildrenCollection();
				Func<NADSegment> nadFunc = () =>
				{
					return group139.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};
				PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.Consignor, () => data.Shipper);

				if (!data.Shipper.E2_Contact.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCTA(cta, data.Shipper.E2_Contact.Left(70));
					interpretation.AddNewSegmentInterpretation(cta, () => data.Shipper.E2_Contact.Left(70));
				}

				if (!data.Shipper.E2_Phone.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group140.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, data.Shipper.E2_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, () => data.Shipper.E2_Phone);
				}
			}

			#endregion

			#region Populate Deliveries

			foreach (IJobDocAddress deliveryAddress in data.DeliveryAddresses)
			{
				if (deliveryAddress.Equals(data.Consignee))
				{
					continue;
				}

				var group139 = group138.Group139.InstantiateAChildAndAddItToChildrenCollection();
				Func<NADSegment> nadFunc = () =>
				{
					return group139.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};
				PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.DeliveryParty, () => deliveryAddress);

				if (!deliveryAddress.E2_Contact.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCTA(cta, deliveryAddress.E2_Contact.Left(70));
					interpretation.AddNewSegmentInterpretation(cta, () => deliveryAddress.E2_Contact.Left(70));
				}

				if (!deliveryAddress.E2_Phone.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group140.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, deliveryAddress.E2_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, () => deliveryAddress.E2_Phone);
				}
			}

			#endregion

			#region Populate Notify

			foreach (IJobDocAddress notityParty in data.NotifyParties)
			{
				var group139 = group138.Group139.InstantiateAChildAndAddItToChildrenCollection();
				Func<NADSegment> nadFunc = () =>
				{
					return group139.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};
				PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.NotifyParty, () => notityParty);

				if (!notityParty.E2_Contact.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCTA(cta, notityParty.E2_Contact.Left(70));
					interpretation.AddNewSegmentInterpretation(cta, () => notityParty.E2_Contact.Left(70));
				}

				if (!notityParty.E2_Phone.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group140.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, notityParty.E2_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, () => notityParty.E2_Phone);
				}
			}

			#endregion

			#region Populate Place of Consolidation

			if (data.PlaceOfConsolidation != null)
			{
				var group139 = group138.Group139.InstantiateAChildAndAddItToChildrenCollection();
				Func<NADSegment> nadFunc = () =>
				{
					return group139.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};
				PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.MutuallyDefined, () => data.PlaceOfConsolidation);

				if (!data.PlaceOfConsolidation.E2_Contact.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCTA(cta, data.PlaceOfConsolidation.E2_Contact.Left(70));
					interpretation.AddNewSegmentInterpretation(cta, () => data.PlaceOfConsolidation.E2_Contact.Left(70));
				}

				if (!data.PlaceOfConsolidation.E2_Phone.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group140.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, data.PlaceOfConsolidation.E2_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, () => data.PlaceOfConsolidation.E2_Phone);
				}
			}

			#endregion

			#region Populate DG Contact

			if (data.UNDGContact != null && !data.UNDGContact.OC_ContactName.IsEmpty)
			{
				var group139 = group138.Group139.InstantiateAChildAndAddItToChildrenCollection();
				var nad = group139.NAD.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateNADName(nad, PartyFunctionCodeQualifierList.ContactParty, data.UNDGContact.OC_ContactName);
				interpretation.AddNewSegmentInterpretation(nad, Res.GetString("155228ab-099c-43df-86a9-f3834603d297", "UNDG Contact Name"), data.UNDGContact.OC_ContactName);
				if (!data.UNDGContact.OC_Phone.IsEmpty)
				{
					var group140 = group139.Group140.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group140.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group140.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, data.UNDGContact.OC_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, Res.GetString("fdbac353-5212-4777-ad43-2625816348ea", "UNDG Contact Phone"), data.UNDGContact.OC_Phone);
				}
			}

			#endregion

			#region PopulateLOC release port

			var releasePortCode = data.ReleasePortCode.IsEmpty ? data.DischargePortCode : data.ReleasePortCode;
			var releaseSubLocationCode = data.ReleaseSubLocationCode.IsEmpty ? data.DischargeSubLocationCode : data.ReleaseSubLocationCode;

			var group141 = group138.Group141.InstantiateAChildAndAddItToChildrenCollection();
			var loc = group141.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfDestination, releasePortCode, releaseSubLocationCode);
			var locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
			locInterpretation.AddElementInterpretation(() => releasePortCode);
			locInterpretation.AddElementInterpretation(() => releaseSubLocationCode);

			#endregion

			#region PopulateLOC discharge port

			if (!data.DischargePortCode.IsEmpty && !data.DischargeSubLocationCode.IsEmpty)
			{
				group141 = group138.Group141.InstantiateAChildAndAddItToChildrenCollection();
				loc = group141.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.PlaceOfDischarge, data.DischargePortCode, data.DischargeSubLocationCode);
				locInterpretation = interpretation.AddNewSegmentInterpretation(loc);
				locInterpretation.AddElementInterpretation(() => data.DischargePortCode);
				locInterpretation.AddElementInterpretation(() => data.DischargeSubLocationCode);
			}

			#endregion

			#region Populate Consolidator

			if (data.Consolidator != null)
			{
				var group142 = group138.Group142.InstantiateAChildAndAddItToChildrenCollection();
				doc = group142.DOC.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.HouseBillOfLading, string.Empty);
				interpretation.AddMandatoryTriggerSegmentInterpretation(doc);

				var group143 = group142.Group143.InstantiateAChildAndAddItToChildrenCollection();

				Func<NADSegment> nadFunc = () =>
				{
					return group143.NAD.InstantiateAChildAndAddItToChildrenCollection();
				};
				PopulateDocAddress(nadFunc, PartyFunctionCodeQualifierList.Consolidator, () => data.Consolidator);

				if (!data.Consolidator.E2_Contact.IsEmpty)
				{
					var group144 = group143.Group144.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group144.CTA.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCTA(cta, data.Consolidator.E2_Contact);
					interpretation.AddNewSegmentInterpretation(cta, () => data.Consolidator.E2_Contact);
				}

				if (!data.Consolidator.E2_Phone.IsEmpty)
				{
					var group144 = group143.Group144.InstantiateAChildAndAddItToChildrenCollection();
					var cta = group144.CTA.InstantiateAChildAndAddItToChildrenCollection();
					var com = group144.COM.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateCOM(cta, com, data.Consolidator.E2_Phone);
					interpretation.AddMandatoryTriggerSegmentInterpretation(cta);
					interpretation.AddNewSegmentInterpretation(com, () => data.Consolidator.E2_Phone);
				}
			}

			#endregion

			#region Populate DG Special Instructions

			if (!data.DGSpecialInstructions.IsEmpty)
			{
				var group146 = group138.Group146.InstantiateAChildAndAddItToChildrenCollection();
				var rcs = group146.RCS.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateRCS(rcs, SectorAreaIdentificationCodeQualifierList.Government);
				var ftx = group146.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation, data.DGSpecialInstructions);
				interpretation.AddNewSegmentInterpretation(rcs, () => SectorAreaIdentificationCodeQualifierList.Government);
				interpretation.AddNewSegmentInterpretation(ftx, () => data.DGSpecialInstructions);
			}

			#endregion

			#region Populate containers

			foreach (IHouseBillContainer container in data.Containers)
			{
				var group156 = group138.Group156.InstantiateAChildAndAddItToChildrenCollection();
				var eqd = group156.EQD.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateEQD(eqd, container.ContainerNumber.Left(16));
				interpretation.AddNewSegmentInterpretation(eqd, () => container.ContainerNumber);
				var seq = group156.SEQ.InstantiateAChildAndAddItToChildrenCollection();
				PopulateSEQTrigger(seq);
				foreach (ZString seal in container.Seals)
				{
					if (!seal.IsEmpty)
					{
						var group157 = group156.Group157.InstantiateAChildAndAddItToChildrenCollection();
						var sel = group157.SEL.InstantiateAChildAndAddItToChildrenCollection();
						D11BMessageUtilities.PopulateSEL(sel, seal.Left(15));
						interpretation.AddNewSegmentInterpretation(sel, () => seal);
						seq = group157.SEQ.InstantiateAChildAndAddItToChildrenCollection();
						PopulateSEQTrigger(seq);
					}
				}
			}

			#endregion

			#region Populate TDT trigger

			var group161 = group138.Group161.InstantiateAChildAndAddItToChildrenCollection();
			tdt = group161.TDT.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.InlandTransport, string.Empty);
			interpretation.AddMandatoryTriggerSegmentInterpretation(tdt);

			#endregion

			#region Populate lines

			foreach (IHouseBillLine line in data.Lines)
			{
				var group177 = group138.Group177.InstantiateAChildAndAddItToChildrenCollection();
				var seq = group177.SEQ.InstantiateAChildAndAddItToChildrenCollection();
				PopulateSEQTrigger(seq);
				var group193 = group177.Group193.InstantiateAChildAndAddItToChildrenCollection();
				var pac = group193.PAC.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulatePAC(pac, line.Packs, line.PacksUOM);
				var pacInterpretation = interpretation.AddNewSegmentInterpretation(pac);
				pacInterpretation.AddElementInterpretation(() => line.Packs.Round(0));
				pacInterpretation.AddElementInterpretation(() => line.PacksUOM);
				seq = group193.SEQ.InstantiateAChildAndAddItToChildrenCollection();
				PopulateSEQTrigger(seq);
				foreach (ZString marksAndNumbers in line.Marks)
				{
					if (!marksAndNumbers.IsEmpty)
					{
						var pci = group193.PCI.InstantiateAChildAndAddItToChildrenCollection();
						D11BMessageUtilities.PopulatePCI(pci, marksAndNumbers);
						interpretation.AddNewSegmentInterpretation(pci, () => marksAndNumbers);
					}
				}
				var group200 = group177.Group200.InstantiateAChildAndAddItToChildrenCollection();
				var gid = group200.GID.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateGID(gid, line.LineNumber);
				interpretation.AddNewSegmentInterpretation(gid, () => line.LineNumber);
				var ftx = group200.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D11BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.GoodsItemDescription, line.GoodsDescription);
				interpretation.AddNewSegmentInterpretation(ftx, () => line.GoodsDescription);
				if (!line.HSCode.IsEmpty)
				{
					var tcc = group200.TCC.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateTCC(tcc, line.HSCode, "SRZ");
					interpretation.AddNewSegmentInterpretation(tcc, () => line.HSCode);
				}
				foreach (ZString uNDgCode in line.DGCodes.Where(c => !c.IsEmpty))
				{
					var tcc = group200.TCC.InstantiateAChildAndAddItToChildrenCollection();
					D11BMessageUtilities.PopulateTCC(tcc, uNDgCode, "SSC");
					interpretation.AddNewSegmentInterpretation(tcc, () => uNDgCode);
				}
			}

			#endregion

			#region PopulateUNS S

			uns = edifactMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateUNS(uns, "S");
			interpretation.AddMandatoryTriggerSegmentInterpretation(uns);

			#endregion

			#region PopulateCNT

			var cnt = edifactMessage.CNT.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateCNT(cnt, ControlTotalTypeCodeQualifierList.TotalGrossWeight, data.TotalWeight, data.TotalWeightUOM);
			var cntInterpretation = interpretation.AddNewSegmentInterpretation(cnt);
			cntInterpretation.AddElementInterpretation(() => data.TotalWeight.Round(0));
			cntInterpretation.AddElementInterpretation(() => data.TotalWeightUOM);

			#endregion

			#region PopulateUNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			D11BMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(), unh.MessageReferenceNumber);
			interpretation.AddUNTInterpretation(unt, unh.MessageReferenceNumber);

			#endregion

		}

		void PopulateSEQTrigger(SEQSegment seq)
		{
			D11BMessageUtilities.PopulateSEQ(seq, ActionCodeList.NoAction);
			interpretation.AddMandatoryTriggerSegmentInterpretation(seq);
		}

		void PopulateDocAddress(Func<NADSegment> getNadFunc, PartyFunctionCodeQualifierList addressType, Expression<Func<IJobDocAddress>> addressExpression)
		{
			var address = addressExpression.Compile()();
			if (!address.E2_CompanyName.IsEmpty)
			{
				var nad = getNadFunc();
				nad.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.PartyFunctionCodeQualifier = addressType;
				var nadInterpretation = interpretation.AddNewSegmentInterpretation(nad);

				nad.PartyName.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.PartyName.PartyName1 = address.E2_CompanyName.Left(70);
				nadInterpretation.AddElementInterpretation(addressExpression, address.E2_CompanyName.Left(70));

				int addressLinesUsed = 0;
				nad.Street.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				var splitter = new TextSplitter(35) { Text = address.E2_Address1 };
				if (splitter.Count > 1)
				{
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = splitter[0].Trim();
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[1].Trim();
					addressLinesUsed = 2;
				}
				else
				{
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = address.E2_Address1.Trim();
					addressLinesUsed = 1;
				}
				nadInterpretation.AddElementInterpretation(() => address.E2_Address1);

				splitter = new TextSplitter(35) { Text = address.E2_Address2 };
				if (splitter.Count > 1)
				{
					if (addressLinesUsed == 2)
					{
						nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = splitter[0].Trim();
						nadInterpretation.AddElementInterpretation(() => address.E2_Address2.Left(35));
					}
					else
					{
						nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[0].Trim();
						nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = splitter[1].Trim();
						nadInterpretation.AddElementInterpretation(() => address.E2_Address2);
					}
				}
				else if (splitter.Count == 1)
				{
					if (addressLinesUsed == 2)
					{
						nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = address.E2_Address2;
					}
					else
					{
						nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = address.E2_Address2;
					}
					nadInterpretation.AddElementInterpretation(() => address.E2_Address2);
				}

				nad.CityName = address.E2_City.Left(35);
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_City.Left(35));

				if (address.E2_RN_NKCountryCode == Constants.CountryCodes.Canada || address.E2_RN_NKCountryCode == Constants.CountryCodes.UnitedStates)
				{
					nad.CountrySubdivisionDetails.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
					nad.CountrySubdivisionDetails.CountrySubdivisionIdentifier = address.E2_State.Left(9);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_State.Left(9));

					nad.PostalIdentificationCode = address.E2_Postcode.Left(9);
					nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_Postcode.Left(9));
				}

				nad.CountryIdentifier = address.E2_RN_NKCountryCode;
				nadInterpretation.AddElementInterpretationIfNotEmpty(() => address.E2_RN_NKCountryCode);
			}
		}

		protected override ZString GetMessageSubType()
		{
			return messageSubType == MessageSubTypes.Request ? new ZString(MessageSubTypeCodes.Codes.Change) : base.GetMessageSubType();
		}

		#endregion
	}
}
