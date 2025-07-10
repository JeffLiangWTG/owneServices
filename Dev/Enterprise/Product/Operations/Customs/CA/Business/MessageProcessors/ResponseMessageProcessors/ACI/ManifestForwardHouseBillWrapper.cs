namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Edifact.D11B.Elements;
	using Enterprise.Edifact.D11B.Messages.GOVCBR;
	using Enterprise.Edifact.D11B.Segments;

	public class ManifestForwardHouseBillWrapper
	{
		public ManifestForwardHouseBillWrapper(ACIHouseBillMessage message)
		{
			this.WrappedMessage = message;
			Argument.NotNull(message, "message");
			Argument.NotNull(message.GOVCBR, "message", "Supported EDIFACT message type is D11B GOVCBR");
		}

		internal UNHSegment UNHSegment
		{
			get
			{
				return unhSegment ?? (unhSegment = (from UNHSegment seg in WrappedMessage.GOVCBR.UNH
													select seg).FirstOrDefault());
			}
		}
		UNHSegment unhSegment;

		internal BGMSegment BGMSegment
		{
			get { return bgmSegment ?? (bgmSegment = WrappedMessage.GOVCBR.BGM.Count > 0 ? WrappedMessage.GOVCBR.BGM[0] : null); }
		}
		BGMSegment bgmSegment;

		internal string DocumentName
		{
			get { return BGMSegment.DocumentMessageName.DocumentNameCode.ToString(); }
		}

		internal ZString HouseBillCCN
		{
			get
			{
				if (!houseBillCCN.HasValue)
				{
					houseBillCCN = BGMSegment != null ? BGMSegment.DocumentMessageIdentification.DocumentIdentifier : string.Empty;
				}
				return houseBillCCN.Value;
			}
		}
		ZString? houseBillCCN;

		internal MessageFunctionCodeList MessageFunction
		{
			get { return BGMSegment.MessageFunctionCode; }
		}

		internal RFFSegment RFFSNPSegment
		{
			get
			{
				return rffSNPSegment ?? (rffSNPSegment = (from RFFSegment seg in WrappedMessage.GOVCBR.RFF
														  where seg.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.SecondaryCustomsReference
														  select seg).FirstOrDefault());
			}
		}
		RFFSegment rffSNPSegment;

		internal string SnpIdentifier
		{
			get { return RFFSNPSegment != null ? RFFSNPSegment.Reference.ReferenceIdentifier : string.Empty; }
		}

		internal string SNPType
		{
			get
			{
				if (!sNPType.HasValue)
				{
					sNPType = RFFSNPSegment != null ? RFFSNPSegment.Reference.DocumentLineIdentifier : string.Empty;
				}
				return sNPType.Value;
			}
		}
		ZString? sNPType;

		internal RFFSegment RFFUCNSegment
		{
			get
			{
				return rffUCNSegment ?? (rffUCNSegment = (from RFFSegment seg in WrappedMessage.GOVCBR.RFF
														  where seg.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.UniqueConsignmentReferenceNumber
														  select seg).FirstOrDefault());
			}
		}
		RFFSegment rffUCNSegment;

		internal string UCN
		{
			get { return RFFUCNSegment != null ? RFFUCNSegment.Reference.ReferenceIdentifier : string.Empty; }
		}

		internal DOCSegment DOCMovementTypeSegment
		{
			get
			{
				return docMovementTypeSegment ?? (docMovementTypeSegment =
									(from SegmentGroup9 grp in WrappedMessage.GOVCBR.Group9
									 from DOCSegment seg in grp.DOC
									 where seg.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.StatusInformation
									 select seg).FirstOrDefault());
			}
		}
		DOCSegment docMovementTypeSegment;

		internal DocumentStatusCodeList MovementType
		{
			get { return DOCMovementTypeSegment.DocumentMessageDetails.DocumentStatusCode; }
		}

		internal DOCSegment DOCPrimaryCCNSegment
		{
			get
			{
				return docPrimaryCCNSegment ?? (docPrimaryCCNSegment =
									(from SegmentGroup9 grp in WrappedMessage.GOVCBR.Group9
									 from DOCSegment seg in grp.DOC
									 where seg.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.CustomsManifest
									 select seg).FirstOrDefault());
			}
		}
		DOCSegment docPrimaryCCNSegment;

		internal string PrimaryCCN
		{
			get
			{
				if (!primaryCCN.HasValue)
				{
					primaryCCN = DOCPrimaryCCNSegment != null ? DOCPrimaryCCNSegment.DocumentMessageDetails.DocumentIdentifier : string.Empty;
				}
				return primaryCCN.Value;
			}
		}
		ZString? primaryCCN;

		internal SegmentGroup13 Group13
		{
			get
			{
				return group13 ?? (group13 = (from SegmentGroup13 grp in WrappedMessage.GOVCBR.Group13
											  select grp).FirstOrDefault());
			}
		}
		SegmentGroup13 group13;

		internal RCSSegment RCSG13Segment
		{
			get
			{
				return rcsG13Segment ?? (rcsG13Segment = (from RCSSegment seg in Group13.RCS
														  where seg.SectorAreaIdentificationCodeQualifier == SectorAreaIdentificationCodeQualifierList.Government
														  select seg).FirstOrDefault());
			}
		}
		RCSSegment rcsG13Segment;

		internal FTXSegment FTXB2BSegment
		{
			get
			{
				return ftxB2BSegment ?? (ftxB2BSegment = (from FTXSegment seg in Group13.FTX
														  where seg.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.AdditionalInformation
														  select seg).FirstOrDefault());
			}
		}
		FTXSegment ftxB2BSegment;

		internal string B2BComments
		{
			get { return Group13 != null && FTXB2BSegment != null ? (FTXB2BSegment.TextLiteral.FreeText1 + " " + FTXB2BSegment.TextLiteral.FreeText2).TrimEnd() : string.Empty; }
		}

		internal AJTSegment AJTSegment
		{
			get
			{
				return ajtSegment ?? (ajtSegment =
									(from SegmentGroup15 grp in WrappedMessage.GOVCBR.Group15
									 from AJTSegment seg in grp.AJT
									 where seg.AdjustmentReasonDescriptionCode == AdjustmentReasonDescriptionCodeList.MutuallyDefined
									 select seg).FirstOrDefault());
			}
		}
		AJTSegment ajtSegment;

		internal string AmendmentReason
		{
			get { return AJTSegment != null ? AJTSegment.LineItemIdentifier : string.Empty; }
		}

		internal TDTSegment TDTSegment
		{
			get
			{
				return tdtSegment ?? (tdtSegment =
									(from SegmentGroup32 grp in WrappedMessage.GOVCBR.Group32
									 from TDTSegment seg in grp.TDT
									 where seg.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtBorder
									 select seg).FirstOrDefault());
			}
		}
		TDTSegment tdtSegment;

		internal string ModeOfTransport
		{
			get { return TDTSegment != null ? TDTSegment.ModeOfTransport.TransportModeNameCode : string.Empty; }
		}

		internal UNSSegment UNS1Segment
		{
			get
			{
				return uns1Segment ?? (uns1Segment = (from UNSSegment seg in WrappedMessage.GOVCBR.UNS1
													  where seg.SectionIdentification == "D"
													  select seg).FirstOrDefault());
			}
		}
		UNSSegment uns1Segment;

		internal HYNSegment HYNSegment
		{
			get
			{
				return hynSegment ?? (hynSegment = (from HYNSegment seg in WrappedMessage.GOVCBR.HYN
													where seg.HierarchyObjectCodeQualifier == HierarchyObjectCodeQualifierList.NoHierarchy
													select seg).FirstOrDefault());
			}
		}
		HYNSegment hynSegment;

		internal SegmentGroup138 Group138
		{
			get
			{
				return group138 ?? (group138 = (from SegmentGroup138 grp in WrappedMessage.GOVCBR.Group138
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup138 group138;

		internal CNISegment CNISegment
		{
			get
			{
				return cniSegment ?? (cniSegment = (from CNISegment seg in Group138.CNI
													select seg).FirstOrDefault());
			}
		}
		CNISegment cniSegment;

		internal STSSegment STSSegment
		{
			get
			{
				return stsSegment ?? (stsSegment = (from STSSegment seg in Group138.STS
													select seg).FirstOrDefault());
			}
		}
		STSSegment stsSegment;

		internal string ConsolidationIndicator
		{
			get { return STSSegment != null ? STSSegment.Status.StatusDescriptionCode : string.Empty; }
		}

		internal MEASegment MEASegment
		{
			get
			{
				return meaSegment ?? (meaSegment = (from MEASegment seg in Group138.MEA
													where seg.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.ConsignmentMeasurement
													select seg).FirstOrDefault());
			}
		}
		MEASegment meaSegment;

		internal string VolumeUnits
		{
			get { return Group138 != null && MEASegment != null ? MEASegment.ValueRange.MeasurementUnitCode : string.Empty; }
		}

		internal string Volume
		{
			get { return Group138 != null && MEASegment != null ? MEASegment.ValueRange.Measure : string.Empty; }
		}

		internal HANSegment HANSegment
		{
			get
			{
				return hanSegment ?? (hanSegment = (from HANSegment seg in Group138.HAN
													select seg).FirstOrDefault());
			}
		}
		HANSegment hanSegment;

		internal string SpecialInstructions
		{
			get { return Group138 != null && HANSegment != null ? HANSegment.HandlingInstructions.HandlingInstructionDescription : string.Empty; }
		}

		internal IEnumerable<NameAndAddress> NamesAndAddresses
		{
			get
			{
				if (Group138 != null)
				{
					foreach (SegmentGroup139 group139 in Group138.Group139)
					{
						yield return new NameAndAddress(group139);
					}
				}
			}
		}

		internal LOCSegment LOCDestinationSegment
		{
			get
			{
				if (Group138 == null)
				{
					return null;
				}

				return locDestinationSegment ?? (locDestinationSegment =
									(from SegmentGroup141 grp in Group138.Group141
									 from LOCSegment seg in grp.LOC
									 where seg.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlaceOfDestination
									 select seg).FirstOrDefault());
			}
		}
		LOCSegment locDestinationSegment;

		internal string PortOfDestination
		{
			get { return LOCDestinationSegment != null ? LOCDestinationSegment.LocationIdentification.LocationIdentifier : string.Empty; }
		}

		internal string SubLocationOfDestination
		{
			get { return LOCDestinationSegment != null ? LOCDestinationSegment.RelatedLocationOneIdentification.FirstRelatedLocationIdentifier : string.Empty; }
		}

		internal LOCSegment LOCDischargeSegment
		{
			get
			{
				if (Group138 == null)
				{
					return null;
				}

				return locDischargeSegment ?? (locDischargeSegment =
									(from SegmentGroup141 grp in Group138.Group141
									 from LOCSegment seg in grp.LOC
									 where seg.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlaceOfDischarge
									 select seg).FirstOrDefault());
			}
		}
		LOCSegment locDischargeSegment;

		internal string PortOfDischarge
		{
			get { return LOCDischargeSegment != null ? LOCDischargeSegment.LocationIdentification.LocationIdentifier : string.Empty; }
		}

		internal string SubLocationOfDischarge
		{
			get { return LOCDischargeSegment != null ? LOCDischargeSegment.RelatedLocationOneIdentification.FirstRelatedLocationIdentifier : string.Empty; }
		}

		internal SegmentGroup142 Group142
		{
			get
			{
				if (Group138 == null)
				{
					return null;
				}

				return group142 ?? (group142 = (from SegmentGroup142 grp in Group138.Group142
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup142 group142;

		internal DOCSegment DOCTriggerSegment
		{
			get
			{
				return docTriggerSegment ?? (docTriggerSegment = (from DOCSegment seg in Group142.DOC
																  where seg.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.HouseBillOfLading
																  select seg).FirstOrDefault());
			}
		}
		DOCSegment docTriggerSegment;

		internal SegmentGroup143 Group143
		{
			get
			{
				if (Group142 == null)
				{
					return null;
				}

				return group143 ?? (group143 = (from SegmentGroup143 grp in Group142.Group143
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup143 group143;

		internal NameAndAddress Consolidator
		{
			get
			{
				return new NameAndAddress(Group143);
			}
		}

		internal SegmentGroup146 Group146
		{
			get
			{
				if (Group138 == null)
				{
					return null;
				}

				return group146 ?? (group146 = (from SegmentGroup146 grp in Group138.Group146
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup146 group146;

		internal RCSSegment RCSG146Segment
		{
			get
			{
				return rcsG146Segment ?? (rcsG146Segment = (from RCSSegment seg in Group13.RCS
															where seg.SectorAreaIdentificationCodeQualifier == SectorAreaIdentificationCodeQualifierList.Government
															select seg).FirstOrDefault());
			}
		}
		RCSSegment rcsG146Segment;

		internal FTXSegment FTXDGSegment
		{
			get
			{
				if (Group146 == null)
				{
					return null;
				}

				return ftxDGSegment ?? (ftxDGSegment = (from FTXSegment seg in Group146.FTX
														where seg.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation
														select seg).FirstOrDefault());
			}
		}
		FTXSegment ftxDGSegment;

		public string DangerousGoodsSpecialInstructions
		{
			get { return FTXDGSegment != null ? (FTXDGSegment.TextLiteral.FreeText1 + " " + FTXDGSegment.TextLiteral.FreeText2).TrimEnd() : string.Empty; }
		}

		internal IEnumerable<IContainerAndSeals> ContainersAndSeals
		{
			get
			{
				if (Group138 != null)
				{
					foreach (SegmentGroup156 group156 in Group138.Group156)
					{
						yield return new ContainerAndSeals(group156);
					}
				}
			}
		}

		internal SegmentGroup161 Group161
		{
			get
			{
				if (Group138 == null)
				{
					return null;
				}

				return group161 ?? (group161 = (from SegmentGroup161 grp in Group138.Group161
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup161 group161;

		internal TDTSegment TDTTriggerSegment
		{
			get
			{
				return tdtTriggerSegment ?? (tdtTriggerSegment = (from TDTSegment seg in Group161.TDT
																  where seg.TransportStageCodeQualifier == TransportStageCodeQualifierList.InlandTransport
																  select seg).FirstOrDefault());
			}
		}
		TDTSegment tdtTriggerSegment;

		internal IEnumerable<IConsignmentLine> ConsignmentLines
		{
			get
			{
				if (Group138 != null)
				{
					foreach (SegmentGroup177 group177 in Group138.Group177)
					{
						yield return new ConsignmentLine(group177);
					}
				}
			}
		}

		internal UNSSegment UNS2Segment
		{
			get
			{
				return uns2Segment ?? (uns2Segment = (from UNSSegment seg in WrappedMessage.GOVCBR.UNS2
													  where seg.SectionIdentification == "S"
													  select seg).FirstOrDefault());
			}
		}
		UNSSegment uns2Segment;

		internal CNTSegment CNTSegment
		{
			get
			{
				return cntSegment ?? (cntSegment = (from CNTSegment seg in WrappedMessage.GOVCBR.CNT
													where seg.Control.ControlTotalTypeCodeQualifier == ControlTotalTypeCodeQualifierList.TotalGrossWeight
													select seg).FirstOrDefault());
			}
		}
		CNTSegment cntSegment;

		internal string CargoWeightUnits
		{
			get { return CNTSegment != null ? CNTSegment.Control.MeasurementUnitCode : string.Empty; }
		}

		internal string CargoWeight
		{
			get { return CNTSegment != null ? CNTSegment.Control.ControlTotalQuantity : string.Empty; }
		}

		internal UNTSegment UNTSegment
		{
			get
			{
				return untSegment ?? (untSegment = (from UNTSegment seg in WrappedMessage.GOVCBR.UNT
													select seg).FirstOrDefault());
			}
		}
		UNTSegment untSegment;

		internal ACIHouseBillMessage WrappedMessage { get; private set; }
	}

	internal interface INameAndAddress
	{
		NADSegment NADSegment { get; }
		PartyFunctionCodeQualifierList AddressType { get; }
		string Name { get; }
		string Address { get; }
		string Country { get; }
		string Address1 { get; }
		string Address2 { get; }
		string City { get; }
		string PostCode { get; }
		string State { get; }

		CTASegment CTASegment { get; }
		string ContactName { get; }
		CTASegment CTAAHSegment { get; }
		COMSegment COMSegment { get; }
		string Telephone { get; }
	}

	class NameAndAddress : INameAndAddress
	{
		internal NameAndAddress(SegmentGroup139 group139)
		{
			this.group139 = group139;
		}

		readonly SegmentGroup139 group139;

		internal NameAndAddress(SegmentGroup143 group143)
		{
			this.group143 = group143;
		}

		readonly SegmentGroup143 group143;

		public NADSegment NADSegment
		{
			get
			{
				return nadSegment ?? (nadSegment = (from NADSegment seg in (group143 == null ? group139.NAD : group143.NAD) select seg).FirstOrDefault());
			}
		}
		NADSegment nadSegment;

		public PartyFunctionCodeQualifierList AddressType
		{
			get { return NADSegment.PartyFunctionCodeQualifier; }
		}

		public string Name
		{
			get { return NADSegment.PartyName.PartyName1; }
		}

		public string Country
		{
			get { return NADSegment.CountryIdentifier; }
		}

		public string Address1
		{
			get { return NADSegment.Street.StreetAndNumberOrPostOfficeBoxIdentifier1; }
		}

		public string Address2
		{
			get { return NADSegment.Street.StreetAndNumberOrPostOfficeBoxIdentifier2; }
		}

		public string City
		{
			get { return NADSegment.CityName; }
		}

		public string PostCode
		{
			get { return NADSegment.PostalIdentificationCode; }
		}

		public string State
		{
			get { return NADSegment.CountrySubdivisionDetails.CountrySubdivisionIdentifier; }
		}

		public string Address
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(NADSegment.Street.StreetAndNumberOrPostOfficeBoxIdentifier1);
				builder.AppendIfNotEmpty(NADSegment.Street.StreetAndNumberOrPostOfficeBoxIdentifier2);
				builder.AppendIfNotEmpty(NADSegment.Street.StreetAndNumberOrPostOfficeBoxIdentifier3);
				builder.AppendIfNotEmpty(NADSegment.CityName);
				builder.AppendIfNotEmpty(NADSegment.CountrySubdivisionDetails.CountrySubdivisionIdentifier + " " + NADSegment.PostalIdentificationCode);
				builder.AppendIfNotEmpty(NADSegment.CountryIdentifier);
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public CTASegment CTASegment
		{
			get
			{
				return ctaSegment ?? (ctaSegment = GetCTASegment(ContactFunctionCodeList.InformationContact));
			}
		}
		CTASegment ctaSegment;

		CTASegment GetCTASegment(ContactFunctionCodeList contactFunction)
		{
			if (group143 == null)
			{
				return (from SegmentGroup140 grp in group139.Group140
						from CTASegment seg in grp.CTA
						where seg.ContactFunctionCode == contactFunction
						select seg).FirstOrDefault();
			}
			else
			{
				return (from SegmentGroup144 grp in group143.Group144
						from CTASegment seg in grp.CTA
						where seg.ContactFunctionCode == contactFunction
						select seg).FirstOrDefault();
			}
		}

		public CTASegment CTAAHSegment
		{
			get
			{
				return ctaAHSegment ?? (ctaAHSegment = GetCTASegment(ContactFunctionCodeList.CoordinationContact));
			}
		}
		CTASegment ctaAHSegment;

		public string ContactName
		{
			get { return CTASegment != null ? CTASegment.ContactDetails.ContactName : string.Empty; }
		}

		public COMSegment COMSegment
		{
			get
			{
				return comSegment ?? (comSegment = GetCOMSegment());
			}
		}
		COMSegment comSegment;

		COMSegment GetCOMSegment()
		{
			if (group143 == null)
			{
				return (from SegmentGroup140 grp in group139.Group140
						from COMSegment seg in grp.COM
						select seg).FirstOrDefault();
			}
			else
			{
				return (from SegmentGroup144 grp in group143.Group144
						from COMSegment seg in grp.COM
						select seg).FirstOrDefault();
			}
		}

		public string Telephone
		{
			get { return COMSegment != null ? COMSegment.CommunicationContact.CommunicationAddressIdentifier : string.Empty; }
		}
	}

	internal interface IContainerAndSeals
	{
		EQDSegment EQDSegment { get; }
		string ContainerNumber { get; }
		SEQSegment SEQSegment { get; }
		IEnumerable<IContainerSeal> ContainerSeals { get; }
	}

	class ContainerAndSeals : IContainerAndSeals
	{
		internal ContainerAndSeals(SegmentGroup156 group156)
		{
			this.group156 = group156;
		}

		readonly SegmentGroup156 group156;

		public EQDSegment EQDSegment
		{
			get
			{
				return eqdSegment ?? (eqdSegment = (from EQDSegment seg in group156.EQD
													where seg.EquipmentTypeCodeQualifier == EquipmentTypeCodeQualifierList.Container
													select seg).FirstOrDefault());
			}
		}
		EQDSegment eqdSegment;

		public string ContainerNumber
		{
			get { return EQDSegment != null ? EQDSegment.EquipmentIdentification.EquipmentIdentifier : string.Empty; }
		}

		public SEQSegment SEQSegment
		{
			get
			{
				return seqSegment ?? (seqSegment = (from SEQSegment seg in group156.SEQ
													where seg.ActionCode == ActionCodeList.NoAction
													select seg).FirstOrDefault());
			}
		}
		SEQSegment seqSegment;

		public IEnumerable<IContainerSeal> ContainerSeals
		{
			get
			{
				foreach (SegmentGroup157 group157 in group156.Group157)
				{
					yield return new ContainerSeal(group157);
				}
			}
		}
	}

	internal interface IContainerSeal
	{
		SELSegment SELSegment { get; }
		string SealNumber { get; }
		SEQSegment SEQSegment { get; }
	}

	class ContainerSeal : IContainerSeal
	{
		internal ContainerSeal(SegmentGroup157 group157)
		{
			this.group157 = group157;
		}

		readonly SegmentGroup157 group157;

		public SELSegment SELSegment
		{
			get
			{
				return selSegment ?? (selSegment = (from SELSegment seg in group157.SEL
													select seg).FirstOrDefault());
			}
		}
		SELSegment selSegment;

		public string SealNumber
		{
			get { return SELSegment != null ? SELSegment.TransportUnitSealIdentifier : string.Empty; }
		}

		public SEQSegment SEQSegment
		{
			get
			{
				return seqSegment ?? (seqSegment = (from SEQSegment seg in group157.SEQ
													where seg.ActionCode == ActionCodeList.NoAction
													select seg).FirstOrDefault());
			}
		}
		SEQSegment seqSegment;
	}

	internal interface IConsignmentLine
	{
		SEQSegment SEQG177Segment { get; }
		SegmentGroup193 Group193 { get; }
		PACSegment PACSegment { get; }
		string CargoQuantity { get; }
		string CargoUnitOfMeasure { get; }
		SEQSegment SEQG193Segment { get; }
		PCISegment PCISegment { get; }
		string MarksAndNumbers { get; }
		SegmentGroup200 Group200 { get; }
		GIDSegment GIDSegment { get; }
		string LineNumber { get; }
		FTXSegment FTXSegment { get; }
		string CargoDescription { get; }
		TCCSegment TCCHSSegment { get; }
		string HsCommodityCode { get; }
		IEnumerable<IUNDGCode> UNDGCodes { get; }
	}

	class ConsignmentLine : IConsignmentLine
	{
		internal ConsignmentLine(SegmentGroup177 group177)
		{
			this.group177 = group177;
		}

		readonly SegmentGroup177 group177;

		public SEQSegment SEQG177Segment
		{
			get
			{
				return seqG177Segment ?? (seqG177Segment = (from SEQSegment seg in group177.SEQ
															where seg.ActionCode == ActionCodeList.NoAction
															select seg).FirstOrDefault());
			}
		}
		SEQSegment seqG177Segment;

		public SegmentGroup193 Group193
		{
			get
			{
				return group193 ?? (group193 = (from SegmentGroup193 grp in group177.Group193
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup193 group193;

		public PACSegment PACSegment
		{
			get
			{
				return pacSegment ?? (pacSegment = (from PACSegment seg in Group193.PAC
													select seg).FirstOrDefault());
			}
		}
		PACSegment pacSegment;

		public string CargoQuantity
		{
			get { return PACSegment != null ? PACSegment.PackageQuantity : string.Empty; }
		}

		public string CargoUnitOfMeasure
		{
			get { return PACSegment != null ? PACSegment.PackageType.TypeOfPackages : string.Empty; }
		}

		public SEQSegment SEQG193Segment
		{
			get
			{
				return seqG193Segment ?? (seqG193Segment = (from SEQSegment seg in Group193.SEQ
															where seg.ActionCode == ActionCodeList.NoAction
															select seg).FirstOrDefault());
			}
		}
		SEQSegment seqG193Segment;

		public PCISegment PCISegment
		{
			get
			{
				return pciSegment ?? (pciSegment = (from PCISegment seg in Group193.PCI
													select seg).FirstOrDefault());
			}
		}
		PCISegment pciSegment;

		public string MarksAndNumbers
		{
			get { return PCISegment != null ? PCISegment.MarksLabels.ShippingMarksDescription1 : string.Empty; }
		}

		public SegmentGroup200 Group200
		{
			get
			{
				return group200 ?? (group200 = (from SegmentGroup200 grp in group177.Group200
												select grp).FirstOrDefault());
			}
		}
		SegmentGroup200 group200;

		public GIDSegment GIDSegment
		{
			get
			{
				return gidSegment ?? (gidSegment = (from GIDSegment seg in Group200.GID
													select seg).FirstOrDefault());
			}
		}
		GIDSegment gidSegment;

		public string LineNumber
		{
			get { return GIDSegment != null ? GIDSegment.GoodsItemNumber : string.Empty; }
		}

		public FTXSegment FTXSegment
		{
			get
			{
				return ftxSegment ?? (ftxSegment = (from FTXSegment seg in Group200.FTX
													where seg.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsItemDescription
													select seg).FirstOrDefault());
			}
		}
		FTXSegment ftxSegment;

		public string CargoDescription
		{
			get { return FTXSegment != null ? (FTXSegment.TextLiteral.FreeText1 + " " + FTXSegment.TextLiteral.FreeText2).TrimEnd() : string.Empty; }
		}

		public TCCSegment TCCHSSegment
		{
			get
			{
				return tccHsSegment ?? (tccHsSegment = (from TCCSegment seg in Group200.TCC
														where seg.CommodityRateDetail.CodeListIdentificationCode == "SRZ"
														select seg).FirstOrDefault());
			}
		}
		TCCSegment tccHsSegment;

		public string HsCommodityCode
		{
			get { return TCCHSSegment != null ? TCCHSSegment.CommodityRateDetail.CommodityIdentificationCode : string.Empty; }
		}

		public IEnumerable<IUNDGCode> UNDGCodes
		{
			get
			{
				return from TCCSegment seg in Group200.TCC
					   where seg.CommodityRateDetail.CodeListIdentificationCode == "SSC"
					   select new UNDGCodeSegment(seg);
			}
		}
	}

	internal interface IUNDGCode
	{
		TCCSegment TCCSegment { get; }
		string UNDGCode { get; }
	}

	class UNDGCodeSegment : IUNDGCode
	{
		internal UNDGCodeSegment(TCCSegment tccSegment)
		{
			TCCSegment = tccSegment;
		}
		public TCCSegment TCCSegment { get; private set; }

		public string UNDGCode
		{
			get { return TCCSegment != null ? TCCSegment.CommodityRateDetail.CommodityIdentificationCode : string.Empty; }
		}
	}
}
