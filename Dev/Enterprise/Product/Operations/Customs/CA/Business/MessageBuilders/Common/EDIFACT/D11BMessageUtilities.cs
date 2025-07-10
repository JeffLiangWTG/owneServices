using CargoWise.Types;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Edifact.D11B.Segments;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public static class D11BMessageUtilities
	{
		#region Populate Details

		public static void PopulateUNH(UNHSegment unh, string messageReferenceNumber, string messageType, string messageVersionNumber, string messageReleaseNumber, string controllingAgency, string associationAssignedCode)
		{
			unh.MessageReferenceNumber = messageReferenceNumber;
			unh.MessageIdentifier.MessageType = messageType;
			unh.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			unh.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			unh.MessageIdentifier.ControllingAgency = controllingAgency;
			unh.MessageIdentifier.AssociationAssignedCode = associationAssignedCode;
		}

		public static void PopulateBGM(BGMSegment bgm, DocumentNameCodeList messageType, string referenceNumber, MessageFunctionCodeList messageFunction)
		{
			bgm.DocumentMessageName.DocumentNameCode = messageType;
			bgm.DocumentMessageIdentification.DocumentIdentifier = referenceNumber;
			bgm.MessageFunctionCode = messageFunction;
		}

		public static void PopulateRFF(RFFSegment rff, ReferenceCodeQualifierList referenceQualifier, string referenceNumber)
		{
			rff.Reference.ReferenceCodeQualifier = referenceQualifier;
			rff.Reference.ReferenceIdentifier = referenceNumber;
		}

		public static void PopulateNAD(NADSegment nad, PartyFunctionCodeQualifierList type, string identifier)
		{
			nad.PartyFunctionCodeQualifier = type;
			nad.PartyIdentificationDetails.PartyIdentifier = identifier;
		}

		public static void PopulateIFD(IFDSegment ifd, string descriptionCode)
		{
			ifd.InformationDetail.InformationDetailDescriptionCode = descriptionCode;
		}

		public static void PopulateDOC(DOCSegment doc, DocumentNameCodeList name, DocumentStatusCodeList code)
		{
			doc.DocumentMessageName.DocumentNameCode = name;
			doc.DocumentMessageDetails.DocumentStatusCode = code;
		}

		public static void PopulateDOC(DOCSegment doc, DocumentNameCodeList name, string identifier)
		{
			doc.DocumentMessageName.DocumentNameCode = name;
			doc.DocumentMessageDetails.DocumentIdentifier = identifier;
		}

		public static void PopulateRCS(RCSSegment rcs, SectorAreaIdentificationCodeQualifierList sector)
		{
			rcs.SectorAreaIdentificationCodeQualifier = sector;
		}

		public static void PopulateFTX(FTXSegment ftx, TextSubjectCodeQualifierList qualifier, string text)
		{
			ftx.TextSubjectCodeQualifier = qualifier;
			if (text.Length > 256)
			{
				ftx.TextLiteral.FreeText1 = text.Substring(0, 256);
				ftx.TextLiteral.FreeText2 = text.Substring(256);
			}
			else
			{
				ftx.TextLiteral.FreeText1 = text;
			}
		}

		public static void PopulateAJT(AJTSegment ajt, AdjustmentReasonDescriptionCodeList reason, string identifier)
		{
			ajt.AdjustmentReasonDescriptionCode = reason;
			ajt.LineItemIdentifier = identifier;
		}

		public static void PopulateTDT(TDTSegment tdt, TransportStageCodeQualifierList qualifier, string transportMode)
		{
			tdt.TransportStageCodeQualifier = qualifier;
			if (!string.IsNullOrEmpty(transportMode))
			{
				tdt.ModeOfTransport.TransportModeNameCode = transportMode;
			}
		}

		public static void PopulateUNS(UNSSegment uns, string sectionID)
		{
			uns.SectionIdentification = sectionID;
		}

		public static void PopulateHYN(HYNSegment hyn, HierarchyObjectCodeQualifierList qualifier)
		{
			hyn.HierarchyObjectCodeQualifier = qualifier;
		}

		public static void PopulateCNI(CNISegment cni, string itemNumber)
		{
			cni.ConsolidationItemNumber = itemNumber;
		}

		public static void PopulateSTS(STSSegment sts, StatusDescriptionCodeList statusCode)
		{
			sts.Status.StatusDescriptionCode = statusCode;
		}

		public static void PopulateMEAasInteger(MEASegment mea, MeasurementPurposeCodeQualifierList qualifier, ZDecimal value, ZString uom)
		{
			mea.MeasurementPurposeCodeQualifier = qualifier;
			var roundedValue = value.Round(0);
			mea.ValueRange.MeasurementUnitCode = uom;
			mea.ValueRange.Measure = roundedValue.ToStringTrimZeros();
		}

		public static void PopulateHAN(HANSegment han, string handlingInstructions)
		{
			han.HandlingInstructions.HandlingInstructionDescription = handlingInstructions;
		}

		public static void PopulateCTA(CTASegment cta, string contactMame)
		{
			cta.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
			cta.ContactDetails.ContactName = contactMame;
		}

		public static void PopulateCOM(CTASegment cta, COMSegment com, string contactNumber)
		{
			cta.ContactFunctionCode = ContactFunctionCodeList.CoordinationContact;
			com.CommunicationContact.CommunicationAddressIdentifier = contactNumber;
			com.CommunicationContact.CommunicationMeansTypeCode = CommunicationMeansTypeCodeList.Telephone;
		}

		public static void PopulateNADName(NADSegment nad, PartyFunctionCodeQualifierList qualifier, string name)
		{
			nad.PartyFunctionCodeQualifier = qualifier;
			nad.PartyName.PartyName1 = name;
		}

		public static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList qualifier, string portCode, string subLocationCode)
		{
			loc.LocationFunctionCodeQualifier = qualifier;
			loc.LocationIdentification.LocationIdentifier = portCode;
			loc.RelatedLocationOneIdentification.FirstRelatedLocationIdentifier = subLocationCode;
		}

		public static void PopulateEQD(EQDSegment eqd, string containerNumber)
		{
			eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container;
			eqd.EquipmentIdentification.EquipmentIdentifier = containerNumber;
		}

		public static void PopulateSEQ(SEQSegment seq, ActionCodeList action)
		{
			seq.ActionCode = action;
		}

		public static void PopulateSEL(SELSegment sel, string seal)
		{
			sel.TransportUnitSealIdentifier = seal;
		}

		public static void PopulatePAC(PACSegment pac, ZDecimal packs, string units)
		{
			pac.PackageQuantity = packs.Round(0).ToString();
			pac.PackageType.TypeOfPackages = units;
		}

		public static void PopulatePCI(PCISegment pci, string marks)
		{
			pci.MarksLabels.ShippingMarksDescription1 = marks;
		}

		public static void PopulateGID(GIDSegment gid, ZInt lineNo)
		{
			gid.GoodsItemNumber = lineNo.ToString();
		}

		public static void PopulateTCC(TCCSegment tcc, string codeValue, string codeID)
		{
			tcc.CommodityRateDetail.CommodityIdentificationCode = codeValue;
			tcc.CommodityRateDetail.CodeListIdentificationCode = codeID;
		}

		public static void PopulateCNT(CNTSegment cnt, ControlTotalTypeCodeQualifierList qualifier, ZDecimal value, ZString uom)
		{
			cnt.Control.ControlTotalTypeCodeQualifier = qualifier;
			var roundedValue = value.Round(0);
			cnt.Control.ControlTotalQuantity = roundedValue.ToStringTrimZeros();
			cnt.Control.MeasurementUnitCode = uom;
		}

		public static void PopulateUNT(UNTSegment uNT, string numberOfSegmentsInTheMessage, string messageReferenceNumber)
		{
			uNT.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			uNT.MessageReferenceNumber = messageReferenceNumber;
		}

		#endregion
	}
}
