using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDMessageLine : BaseMessageLine
	{
		public IMDMessageLine(ICusEntryLine entryLine, SegmentGroup30 group30, bool isPreLodgeDeclaration, bool generatingForAmendmentDetection)
			: base(entryLine, group30)
		{
			fIsPreLodgeDeclaration = isPreLodgeDeclaration;
			this.generatingForAmendmentDetection = generatingForAmendmentDetection;
		}

		public override void Populate(int lineNumber, string lineActionCode)
		{
			IsLineDeleted = lineActionCode == LineAction.Delete;

			PopulateCST(lineActionCode);
			PopulateFTX();
			PopulateLOC();
			PopulateDTM();

			if (IsLineDeleted)
			{
				PopulateMEAForDeletedLineInAmendment();
			}
			else
			{
				PopulateMEA();
			}

			PopulateNAD();
			PopulateGroup31AndGroup32();
			PopulateGroup33();
			PopulateGroup35();
			PopulateRefundReasonCode(lineActionCode);
			PopulateGroup37();
			PopulateGroup40();
			PopulateGroup41();
		}

		public bool IsLineDeleted
		{
			get { return fIsLineDeleted; }
			set { fIsLineDeleted = value; }
		}
		bool fIsLineDeleted;

		#region CST Segment

		protected internal override void PopulateCST(string lineActionCode)
		{
			base.PopulateCST(lineActionCode);
			if (!EntryLine.NatureTypeForCMR.IsEmpty)
			{
				CSTForGroup30.CustomsIdentityCodes2.CustomsCodeIdentification = EntryLine.NatureTypeForCMR;
				CSTForGroup30.CustomsIdentityCodes2.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}

			CargoReportHelper.DoICSRelease(() =>
			{
				if (EntryLine.IsNature10)
				{
					var splitter = new ABNCACSplitter(EntryLine.ConsignorVendor);
					var vendor = splitter.ABN.IsEmpty ? EntryLine.ConsignorVendor : splitter.ABN;
					PopulateGroup30NAD(PartyFunctionCodeQualifierList.Vendor, vendor);
				}
			});
		}

		#endregion

		#region FTX Segment

		protected internal override void PopulateFTX()
		{
			if (!EntryLine.WRN.IsEmpty && EntryLine.WRN.Length == 11)
			{
				ZString lineDescription =
					"ENTRY=" + EntryLine.WRN.Trim() + ", LINE=" + EntryLine.WRL.ToString() + " : " + EntryLine.Description.Trim();
				PopulateFTX(lineDescription);
			}
			else
			{
				base.PopulateFTX();
			}

			foreach (char currentAmberLineValue in EntryLine.OrderedAMBs)
			{
				FTXSegment fTX = Group30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.TariffStatements;
				fTX.TextReference.FreeTextValueCode = GetAmberLineProcessingCode(char.ToUpper(currentAmberLineValue));
				fTX.TextReference.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		ZString GetAmberLineProcessingCode(char value)
		{
			ZString aMBCodeType = ZString.Empty;

			switch (value)
			{
				case 'C':
					aMBCodeType = "CLASS";
					break;
				case 'D':
					aMBCodeType = "DUMP";
					break;
				case 'O':
					aMBCodeType = "ORIGIN";
					break;
				case 'P':
					aMBCodeType = "PREFER";
					break;
				case 'Q':
					aMBCodeType = "QUANTITY";
					break;
				case 'T':
					aMBCodeType = "TREATCODE";
					break;
				case 'V':
					aMBCodeType = "VALUE";
					break;
			}

			return aMBCodeType;
		}

		#endregion

		#region LOC Segment

		protected internal void PopulateLOC()
		{
			PopulateLOC(LocationFunctionCodeQualifierList.CountryOfOrigin, EntryLine.ORG);

			ZString pOC = EntryLine.IsGeneralRate ? ZString.Empty : EntryLine.POC;
			PopulateLOC(LocationFunctionCodeQualifierList.CountryOfSource, pOC);
			PopulateLOC(LocationFunctionCodeQualifierList.CountryOfExportationDespatch, EntryLine.DCX);
			PopulateLOC(LocationFunctionCodeQualifierList.Warehouse, EntryLine.WAR);
		}

		internal void PopulateLOC(LocationFunctionCodeQualifierList locationFunctionCodeQualifier, ZString value)
		{
			if (!value.IsEmpty)
			{
				LOCSegment lOC = Group30.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.LocationFunctionCodeQualifier = locationFunctionCodeQualifier;
				lOC.LocationIdentification.LocationNameCode = value;
				lOC.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization;
			}
		}

		#endregion

		#region DTM Segment

		protected internal void PopulateDTM()
		{
			if (!EntryLine.FOD.IsEmpty)
			{
				DTMSegment dTM = Group30.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = DateTimePeriodFunctionCodeQualifierList.OrderDateTime;
				dTM.DateTimePeriod.DateTimePeriodValue = EntryLine.FOD.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateTimePeriodFormatCode = DateTimePeriodFormatCodeList.Ccyymmdd;
			}
		}

		#endregion

		#region MEA Segment

		protected internal override void PopulateMEA()
		{
			if (EntryLine.IsNature20 || EntryLine.IsNature30)
			{
				if (EntryLine.WRQ > 0 && EntryLine.WRU != EntryLine.CustomsUnitQty)
				{
					PopulateMEAForN20OrN30(EntryLine.WRQ, EntryLine.WRU);
					base.PopulateMEA();
				}
				else
				{
					if (EntryLine.CustomsQuantity > 0 && !EntryLine.CustomsUnitQty.IsEmpty)
					{
						ZString uQ = GetUnitOfQuantity(EntryLine.CustomsUnitQty);
						PopulateMEAForN20OrN30(EntryLine.CustomsQuantity, uQ);
					}

					PopulateSecondUnitOfQuantity();
				}
			}
			else
			{
				base.PopulateMEA();
			}

			if (EntryLine.IsNature30)
			{
				PopulateMEA(MeasurementAttributeCodeList.PercentageOfAlcoholByVolume, "ISS", EntryLine.ISS, 2);
			}
			PopulateMEA(MeasurementAttributeCodeList.UsableOrConsumableContent, "LCO", EntryLine.LCP, 2);
		}

		protected void PopulateMEAForDeletedLineInAmendment()
		{
			if (EntryLine.IsNature20 || EntryLine.IsNature30)
			{
				if (EntryLine.WRQ > 0 && EntryLine.WRU != EntryLine.CustomsUnitQty)
				{
					PopulateMEAForN20OrN30(EntryLine.WRQ, EntryLine.WRU);
				}
				else
				{
					if (EntryLine.CustomsQuantity > 0 && !EntryLine.CustomsUnitQty.IsEmpty)
					{
						ZString uQ = GetUnitOfQuantity(EntryLine.CustomsUnitQty);
						PopulateMEAForN20OrN30(EntryLine.CustomsQuantity, uQ);
					}
				}
				if (EntryLine.IsNature30)
				{
					PopulateMEA(MeasurementAttributeCodeList.PercentageOfAlcoholByVolume, "ISS", EntryLine.ISS, 2);
				}
			}
		}

		void PopulateMEAForN20OrN30(ZDecimal measurementValue, ZString measurementUnitCode)
		{
			if (!measurementUnitCode.IsEmpty)
			{
				MEASegment mEA = Group30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementAttributeCode = MeasurementAttributeCodeList.LineItemMeasurement;
				mEA.MeasurementDetails.NonDiscreteMeasurementNameCode = NonDiscreteMeasurementNameCodeList.GetFromString("WAR");
				mEA.ValueRange.MeasurementUnitCode = measurementUnitCode;
				mEA.ValueRange.MeasurementValue = measurementValue.ToString(5);
			}
		}

		#endregion

		#region NAD Segment

		protected internal void PopulateNAD()
		{
			PopulateGroup30NAD(PartyFunctionCodeQualifierList.Supplier, EntryLine.SupplierCode);

			if (!EntryLine.IsNature30)
			{
				PopulateGroup30NAD(PartyFunctionCodeQualifierList.AuthorizedTraderTransit, CargoHelper.GetTraderIdentificationNumber(EntryLine.Supplier));
			}
		}

		void PopulateGroup30NAD(PartyFunctionCodeQualifierList partyFunctionCodeQualifier, ZString partyIdentifier)
		{
			if (!partyIdentifier.IsEmpty)
			{
				var group30Nad = Group30.NAD.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateNAD(group30Nad, partyFunctionCodeQualifier, partyIdentifier, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		#endregion

		#region Segment Group 31 and 32

		protected internal void PopulateGroup31AndGroup32()
		{
			PopulateAQISPackingInformation();

			if (group31OfAQISPackage != null)
			{
				PopulateGroup32(group31OfAQISPackage);
			}
			else if (EntryLine != null && EntryLine.Questions != null && EntryLine.Questions.IsAnyCPQuestionAnswered && !generatingForAmendmentDetection)
			{
				SegmentGroup31 group31 = Group30.Group31.InstantiateAChildAndAddItToChildrenCollection();
				PACSegment pAC = group31.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pAC.PackagingDetails.PackagingLevelCoded = PackagingLevelCodedList.Inner;
				PopulateGroup32(group31);
			}
		}

		void PopulateAQISPackingInformation()
		{
			int numberOfPackagesAdded = 0;

			if (EntryLine.OrderedAQISPackages != null)
			{
				foreach (AQISPackage currentPackage in EntryLine.OrderedAQISPackages)
				{
					if (currentPackage.Number > 0 && !currentPackage.Type.IsEmpty)
					{
						numberOfPackagesAdded++;

						SegmentGroup31 group31 = Group30.Group31.InstantiateAChildAndAddItToChildrenCollection();
						PACSegment pAC = group31.PAC.InstantiateAChildAndAddItToChildrenCollection();
						pAC.NumberOfPackages = currentPackage.Number.ToString();
						pAC.PackageType.PackageTypeDescriptionCode = currentPackage.Type.Trim();
						pAC.PackageType.CodeListIdentificationCode = CodeListIdentificationCodeList.ItemType;
						pAC.PackageType.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAqisAustralianQuarantineAndInspectionService;

						if (numberOfPackagesAdded == 10)
						{
							group31OfAQISPackage = group31;
							break;
						}
					}
				}
			}
		}
		SegmentGroup31 group31OfAQISPackage;

		void PopulateGroup32(SegmentGroup31 group31)
		{
			if (EntryLine != null)
			{
				EntryLine.Questions.Sort(CusEntryCPDecSchema.ON_CPDecNum.Name, System.ComponentModel.ListSortDirection.Ascending);

				foreach (CMRCusEntryCPDec currentQuestion in EntryLine.Questions)
				{
					ZInt riskID = currentQuestion.RiskId;
					if (currentQuestion.IsAnswered && !riskID.IsEmpty)
					{
						SegmentGroup32 group32 = group31.Group32.InstantiateAChildAndAddItToChildrenCollection();
						PCISegment pCIOne = group32.PCI.InstantiateAChildAndAddItToChildrenCollection();
						pCIOne.MarkingInstructionsCoded = MarkingInstructionsCodedList.GetFromString("1");

						FTXSegment fTX = group32.FTX.InstantiateAChildAndAddItToChildrenCollection();
						fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.RiskAndHandlingInformation;

						fTX.TextLiteral.FreeTextValue1 = riskID.ToString().PadLeft(5, '0');
						fTX.TextLiteral.FreeTextValue2 = currentQuestion.ON_CPDecNum.ToString().PadLeft(5, '0');
						fTX.TextLiteral.FreeTextValue3 = currentQuestion.ON_AnswerCode;
						fTX.TextLiteral.FreeTextValue4 = currentQuestion.ON_Permit;
					}
				}
			}
		}

		#endregion

		#region Segement Group 33

		protected internal override void PopulateGroup33()
		{
			Group33 = Group30.Group33.InstantiateAChildAndAddItToChildrenCollection();

			if (EntryLine.IsNature30)
			{
				PopulateMOA(Group33, EntryLine.CustomsValue, MonetaryAmountTypeCodeQualifierList.CustomsValue);
			}
			else
			{
				PopulateMOA(Group33, EntryLine.Price, MonetaryAmountTypeCodeQualifierList.InvoiceItemAmount);
			}

			PopulateTILV(EntryLine, Group33);
			PopulateMOA(Group33, EntryLine.DumpingExportPrice, MonetaryAmountTypeCodeQualifierList.DumpingExportValue);
			PopulateMOA(Group33, EntryLine.PriceAdjustment, MonetaryAmountTypeCodeQualifierList.AdjustedAmount);

			PopulateManualDuty(EntryLine, Group33);
			PopulateMOA(Group33, EntryLine.StandardDutyOverriden, MonetaryAmountTypeCodeQualifierList.StandardDuty);
		}

		void PopulateManualDuty(ICusEntryLine entryLine, SegmentGroup33 group33)
		{
			Money manualDutyAmount = entryLine.ManualDutyAmount;
			if (manualDutyAmount != null && manualDutyAmount.Amount > 0 || entryLine.SendZeroManualDuty)
			{
				MOASegment mOA = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.DutyAmount;
				mOA.MonetaryAmount.MonetaryAmountValue = manualDutyAmount.Amount.ToString(2);
				if (manualDutyAmount.Currency != null)
				{
					mOA.MonetaryAmount.CurrencyIdentificationCode = manualDutyAmount.Currency.Code;
				}
			}
		}

		void PopulateTILV(ICusEntryLine entryLine, SegmentGroup33 group33)
		{
			Money tILV = entryLine.TransportAndInsuranceForMessage;

			if (tILV != null && tILV.IsValid)
			{
				MOASegment mOA = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.InsuranceAndTransportChargesCustoms;
				mOA.MonetaryAmount.MonetaryAmountValue = tILV.Amount.ToString(2);
				if (tILV.Currency != null)
				{
					mOA.MonetaryAmount.CurrencyIdentificationCode = tILV.Currency.Code;
				}
			}
		}

		#endregion

		#region Segment Group 35

		protected internal override void PopulateGroup35()
		{
			base.PopulateGroup35();

			//Line Valuation Advice Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.CustomsValuationDecisionNumber, EntryLine.VAN);

			if (!fIsPreLodgeDeclaration)
			{
				//Security Identifier
				PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.CustomsGuaranteeNumber, EntryLine.SCN);
			}

			//Preference Scheme Type
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.SchemePlanNumber, EntryLine.PST);
			//Preference Rule Type
			PopulatePreferenceRuleType(Group35);

			//Dumping Specification Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.AntiDumpingCaseNumber, EntryLine.DSN);
			//Luxury Car Tax Exemption Code
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.DeferredPaymentReference, EntryLine.LCTE);

			//Second Treatment Code
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.RelatedDocumentNumber, EntryLine.TR2);
			//Second Tariff Classification Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.TariffNumber, EntryLine.CL2.Replace(".", ""));
			//Treatment Code
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.CustomsItemNumber, EntryLine.TreatmentCode);
			//Treatment Code Rate Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.RateNoteNumber, EntryLine.TRN);
			//Instrument Security Code
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.Suffix, EntryLine.ISC);
			//Tariff Advice Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.PolicyNumber, EntryLine.TAN);
			//Tariff Classification Rate Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.RateCodeNumber, EntryLine.RNO);
			//Import Credit Number
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.ImportLicenceNumber, EntryLine.ICN);

			//ELAC Number
			PopulateELACNumber(Group35);
			//Tariff Classification Instrument Number
			PopulateInstrumentNumberAndType(Group35, ReferenceFunctionCodeQualifierList.GovernmentAgencyReferenceNumber, EntryLine.TCI_InstrumentNo, EntryLine.TCI_InstrumentType);
			//Second Treatment Instrument Number
			PopulateInstrumentNumberAndType(Group35, ReferenceFunctionCodeQualifierList.SecondaryCustomsReference, EntryLine.TI2_InstrumentNo, EntryLine.TI2_InstrumentType);
			//Preference Instrument Number
			PopulateInstrumentNumberAndType(Group35, ReferenceFunctionCodeQualifierList.CustomsPreferenceInquiryNumber, EntryLine.PRI_InstrumentNo, EntryLine.PRI_InstrumentType);
			//Vehicle Identification Number
			PopulateVehicleIDNumbers(Group35);
			//Dumping Exemption Type
			PopulateDumpingExemptionType(Group35, EntryLine.DXT);
			//Treatment Instrument Number and Treatment Instrument Type
			PopulateInstrumentNumberAndType(Group35, ReferenceFunctionCodeQualifierList.PrimaryReference, EntryLine.InstrumentCode, EntryLine.InstrumentType);

			//Warehouse entry number, line number and Multiple Clearance Code
			PopulateWRNAndWMC(Group35);

			//AQIS Permit Number
			PopulateAQISPermitNumber(Group35);
			//AQIS Commodity Code
			PopulateAQISCommodityCode(Group35);
			//AQIS Document Type and Number
			PopulateAQISDocumentInformation(Group35);
			//AQIS Line Containers
			PopulateAQISLineContainerNumbers(Group35);

			if (!EntryLine.IsExWarehouse)
			{
				//Valuation Basis Type
				PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.QuantityValuationNumber, EntryLine.ValuationBasisForCMR);
			}
		}

		void PopulatePreferenceRuleType(SegmentGroup35 group35)
		{
			if (!EntryLine.IsNature20)
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.TechnicalRegulation, EntryLine.IsGeneralRate ? ZString.Empty : EntryLine.PRT, null);
			}
		}

		void PopulateDumpingExemptionType(SegmentGroup35 group35, ZString dumpingExemptionType)
		{
			if (dumpingExemptionType == "C")
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ActionAuthorizationNumber, "COUNTRY");
			}
			else if (dumpingExemptionType == "S")
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ActionAuthorizationNumber, "SUPPLIER");
			}
			else if (dumpingExemptionType == "G")
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ActionAuthorizationNumber, "GOODS");
			}
		}

		void PopulateELACNumber(SegmentGroup35 group35)
		{
			foreach (ZString anElac in EntryLine.OrderedELAs)
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ReportNumber, anElac);
			}
		}

		void PopulateVehicleIDNumbers(SegmentGroup35 group35)
		{
			foreach (ZString anID in EntryLine.OrderedVIDs)
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.VehicleIdentificationNumberVin, anID.SubstringSafe(0, 35));
			}
		}

		void PopulateInstrumentNumberAndType(SegmentGroup35 group35, ReferenceFunctionCodeQualifierList functionCode, ZString instrumentNo, ZString instrumentType)
		{
			ZString finalInstrumentType = instrumentType;
			if (instrumentType == CustomsInstrumentTypeList.Codes.TariffConcession)
			{
				finalInstrumentType = "TC";
			}

			PopulateRFF(group35, functionCode, instrumentNo, finalInstrumentType);
		}

		void PopulateWRNAndWMC(SegmentGroup35 group35)
		{
			ZString wRN = EntryLine.WRN;

			if (!EntryLine.WMC.IsEmpty)
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.AdditionalReferenceNumber, EntryLine.WMC);
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.WarehouseEntryNumber, EntryLine.WRN);
				PopulateWarehouseLineNumber(group35);
			}
			else if (EntryLine.WRN.Length == 11)
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.AdditionalReferenceNumber, EntryLine.WRN.SubstringSafe(0, 8));
			}
			else
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.WarehouseEntryNumber, EntryLine.WRN);
				PopulateWarehouseLineNumber(group35);
			}
		}

		void PopulateWarehouseLineNumber(SegmentGroup35 group35)
		{
			ZString wRL = EntryLine.WRL.ToString();
			if (wRL != "0")
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.LineItemReferenceNumber, wRL);
			}
		}

		void PopulateAQISPermitNumber(SegmentGroup35 group35)
		{
			if (EntryLine.OrderedAQISPermitIds != null)
			{
				foreach (AQISPermitId currentPermitNumber in EntryLine.OrderedAQISPermitIds)
				{
					PopulateRFF(group35, ReferenceFunctionCodeQualifierList.AuthorizationNumber, currentPermitNumber.Code);
				}
			}
		}

		void PopulateAQISCommodityCode(SegmentGroup35 group35)
		{
			if (EntryLine.OrderedAQISCommodityCodes != null)
			{
				foreach (AQISCommodityCode aQISCommodityCode in EntryLine.OrderedAQISCommodityCodes)
				{
					PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ArticleNumber, aQISCommodityCode.Code);
				}
			}
		}

		void PopulateAQISDocumentInformation(SegmentGroup35 group35)
		{
			if (EntryLine.OrderedAQISDocuments != null)
			{
				foreach (AQISDocument document in EntryLine.OrderedAQISDocuments)
				{
					PopulateRFF(group35, ReferenceFunctionCodeQualifierList.PrincipalReferenceNumber, document.Number, document.Type);
				}
			}
		}

		void PopulateAQISLineContainerNumbers(SegmentGroup35 group35)
		{
			var containersPivot = EntryLine.ContainersPivot;
			if (containersPivot != null && !((CusEntryLine)EntryLine).IsNonAQISAEPLine)
			{
				foreach (var container in EntryLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Select(x => x.Container)
					.Where(x => x != null).OrderBy(x => x.CO_ContainerNumber))
				{
					PopulateRFF(group35, ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, container.CO_ContainerNumber);
				}
			}
		}

		#endregion

		#region Populate Refund Reason Code

		internal void PopulateRefundReasonCode(string lineActionCode)
		{
			if (lineActionCode == LineAction.Delete || lineActionCode == LineAction.Amend || generatingForAmendmentDetection)
			{
				if (!EntryLine.RefundReasonCode.IsEmpty)
				{
					PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.DeclarantsReferenceNumber, EntryLine.RefundReasonCode);
				}
			}
		}

		#endregion

		#region Segment Group 37

		protected internal void PopulateGroup37()
		{
			AQISPremisesIdAndProcessingTypeCollection orderedAQISPremiseIDProcessingType = EntryLine.OrderedAQISPremisesIdAndProcessingTypes;
			AQISProducerCodeCollection orderedAQISProducerCodes = EntryLine.OrderedAQISProducerCodes;
			AQISEntityIdCollection orderedAQISEntityIds = EntryLine.OrderedAQISEntityIds;
			var isN30 = EntryLine.IsNature30;

			for (int i = 0; i < 10; i++)
			{
				ZString premisesID = (orderedAQISPremiseIDProcessingType != null && orderedAQISPremiseIDProcessingType.Count > i) ? orderedAQISPremiseIDProcessingType[i].PremisesId : ZString.Empty;
				ZString processingType = (orderedAQISPremiseIDProcessingType != null && orderedAQISPremiseIDProcessingType.Count > i) ? orderedAQISPremiseIDProcessingType[i].ProcessingType : ZString.Empty;
				ZString producerCode = (orderedAQISProducerCodes != null && orderedAQISProducerCodes.Count > i) ? orderedAQISProducerCodes[i].Code : ZString.Empty;
				ZString entityID = (orderedAQISEntityIds != null && orderedAQISEntityIds.Count > i) ? orderedAQISEntityIds[i].Code : ZString.Empty;

				if (!premisesID.IsEmpty || !processingType.IsEmpty || !producerCode.IsEmpty || !entityID.IsEmpty)
				{
					SegmentGroup37 group37 = Group30.Group37.InstantiateAChildAndAddItToChildrenCollection();
					DOCSegment dOC = group37.DOC.InstantiateAChildAndAddItToChildrenCollection();
					dOC.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString("1");

					if (!processingType.IsEmpty || !premisesID.IsEmpty)
					{
						LOCSegment lOC = group37.LOC.InstantiateAChildAndAddItToChildrenCollection();
						lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceLocationWhereSpecialTreatmentsHaveHappenedOrMustHappen;
						PopulateAQISPremisesID(lOC, premisesID);
						lOC.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAqisAustralianQuarantineAndInspectionService;
						PopulateAQISProcessingType(lOC, processingType);
					}

					PopulateAQISProducerCode(group37, producerCode);
					if (!isN30)
					{
						PopulateAQISEntityID(group37, entityID);
					}
				}
			}
		}

		void PopulateAQISPremisesID(LOCSegment lOC, ZString premisesID)
		{
			if (!premisesID.IsEmpty)
			{
				lOC.LocationIdentification.LocationNameCode = premisesID;
			}
		}

		void PopulateAQISProcessingType(LOCSegment lOC, ZString processingType)
		{
			if (!processingType.IsEmpty)
			{
				lOC.LocationIdentification.LocationName = processingType;
			}
		}

		void PopulateAQISProducerCode(SegmentGroup37 group37, ZString producerCode)
		{
			if (!producerCode.IsEmpty)
			{
				NADSegment nAD = group37.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Producer;
				nAD.PartyIdentificationDetails.PartyIdentifier = producerCode;
				nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAqisAustralianQuarantineAndInspectionService;
			}
		}

		void PopulateAQISEntityID(SegmentGroup37 group37, ZString entityID)
		{
			if (!entityID.IsEmpty)
			{
				NADSegment nAD = group37.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.PartyDesignatedToExecuteSanitaryProcedures;
				nAD.PartyIdentificationDetails.PartyIdentifier = entityID;
				nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAqisAustralianQuarantineAndInspectionService;
			}
		}

		#endregion

		#region Segment Group 40

		protected internal override void PopulateGroup40()
		{
			base.PopulateGroup40();

			//Luxury Car Tax Payable Indicator
			PopulateGISInGroup40(EntryLine.LCTI, "LCT");

			//Luxury Car Tax Quote Indicator
			PopulateGISInGroup40(EntryLine.LCTQ, "LCQ");

			//Manual Line Processing Indicator
			PopulateGISInGroup40(EntryLine.MLPI, "MLP");

			//Paid Under Protest Indicator
			PopulateGISInGroup40(EntryLine.PUP, "PUP");

			//Related Transaction Indicator
			PopulateGISInGroup40(EntryLine.REL, "REL");

			//Security Calculate Indicator
			if (fIsPreLodgeDeclaration && (EntryLine.Header.IsCMRNature10 || EntryLine.Header.IsCMRNature1020))
			{
				PopulateGISInGroup40(EntryLine.SEC, "SEC");
			}
		}

		#endregion

		#region Segment Group 41

		protected internal void PopulateGroup41()
		{
			SegmentGroup41 group41 = Group30.Group41.InstantiateAChildAndAddItToChildrenCollection();
			PopulateTAX(group41, DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem, DutyTaxFeeTypeNameCodeList.AntiDumpingDuty, EntryLine.DRE);

			if (EntryLine.OtherDutyFactor != null)
			{
				PopulateTAX(group41, DutyTaxFeeFunctionQualifierList.TaxRelatedInformation, DutyTaxFeeTypeNameCodeList.CustomsDuty, EntryLine.OtherDutyFactor.Amount);
			}
		}

		void PopulateTAX(SegmentGroup41 group41, DutyTaxFeeFunctionQualifierList dutyTaxFeeFunctionQualifier, DutyTaxFeeTypeNameCodeList dutyTaxFeeTypeNameCode, ZDecimal value)
		{
			if (value > 0M)
			{
				TAXSegment tAX = group41.TAX.InstantiateAChildAndAddItToChildrenCollection();
				tAX.DutyTaxFeeFunctionQualifier = dutyTaxFeeFunctionQualifier;
				tAX.DutyTaxFeeType.DutyTaxFeeTypeNameCode = dutyTaxFeeTypeNameCode;
				tAX.DutyTaxFeeDetail.DutyTaxFeeRate = value.ToString(4);
			}
		}

		#endregion

		#region Implementation

		readonly bool fIsPreLodgeDeclaration;
		readonly bool generatingForAmendmentDetection;

		#endregion
	}
}
