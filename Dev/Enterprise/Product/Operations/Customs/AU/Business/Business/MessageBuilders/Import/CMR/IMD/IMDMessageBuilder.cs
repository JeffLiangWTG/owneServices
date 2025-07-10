using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDMessageBuilder : BaseImportMessageBuilder
	{
		public IMDMessageBuilder(CusEntryHeader entryHeader, CMRMessageTypes messageType)
			: base(entryHeader, messageType)
		{
		}

		#region Header Section

		protected override void PopulateHeaderGroups()
		{
			PopulateCST();
			PopulateMEA();
			PopulateEQD();
			base.PopulateHeaderGroups();
		}

		protected internal void PopulateCST()
		{
			CSTSegment cST = cUSDEC.CST.InstantiateAChildAndAddItToChildrenCollection();
			MessageUtilities.PopulateCST(cST, EntryHeader.Nature, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
		}

		protected internal override void PopulateLocations()
		{
			base.PopulateLocations();

			if (!Declaration.JE_RL_NKPortOfLoading.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.PlacePortOfLoading, Declaration.JE_RL_NKPortOfLoading, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}

			if (!EntryHeader.WarehouseCCP.IsEmpty && !EntryHeader.IsCMRNature10)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.Warehouse, EntryHeader.WarehouseCCP, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}

			if (!Declaration.JE_RL_NKPortOfFirstArrival.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.PlacePortOfFirstEntry, Declaration.JE_RL_NKPortOfFirstArrival, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}

			if (!Declaration.AddInfo.ZA_AQISInspectLocation_Hidden.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceLocationWhereSpecialTreatmentsHaveHappenedOrMustHappen;
				lOC.LocationIdentification.LocationName = Declaration.AddInfo.ZA_AQISInspectLocation_Hidden;
			}
		}

		protected internal override void PopulateDTM()
		{
			base.PopulateDTM();

			PopulateDTM(DateTimePeriodFunctionCodeQualifierList.ValuationDateCustoms, EntryHeader.EffectiveValuationDate);
			if (InvoiceHeader != null && InvoiceHeader.AddInfo != null)
			{
				PopulateDTM(DateTimePeriodFunctionCodeQualifierList.EffectiveDateTime, InvoiceHeader.AddInfo.EFD);
			}
			PopulateDTM(DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeAtInitialPort, Declaration.JE_DateOfFirstArrival);
			if (Declaration.IsNature30 && Declaration.SettlementTypeSelected)
			{
				if (Declaration.SettlementPeriodStartDate.IsValid)
				{
					PopulateDTM(DateTimePeriodFunctionCodeQualifierList.TaxPeriodStartDate, Declaration.SettlementPeriodStartDate);
				}
				if (Declaration.JE_SettlementPeriodEndDate.IsValid)
				{
					PopulateDTM(DateTimePeriodFunctionCodeQualifierList.TaxPeriodEndDate, Declaration.JE_SettlementPeriodEndDate);
				}
			}
		}

		protected internal override void PopulateGIS()
		{
			base.PopulateGIS();

			//Official receipt request
			PopulateGIS(MessageType != CMRMessageTypes.PreLodge && Env.Registry.RequestOfficialCustomsPaymentReceipt, "POR");

			//Pre-lodgement indicator
			PopulateGIS(MessageType == CMRMessageTypes.PreLodge, "PRE");

			//Line Liabilities breakdown indicator
			PopulateGIS(true, "LLB");

			//Total Line Liabilities breakdown indicator
			PopulateGIS(true, "TLB");

			if (Declaration.IsNature30 && Declaration.SettlementTypeSelected)
			{
				PopulateGIS(true, Declaration.JE_SettlementPeriodType);
				PopulateGIS(Declaration.NilReturnInd, "NRT");
			}
		}

		protected internal void PopulateMEA()
		{
			if (!Declaration.IsExWarehouse && EntryHeader.GrossWeight.Amount > 0M)
			{
				MEASegment mEA = cUSDEC.MEA.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateMEA(mEA, MeasurementAttributeCodeList.Measurement, MeasuredAttributeCodeList.GrossWeight,
					EntryHeader.GrossWeight.Unit, EntryHeader.GrossWeight.Amount.ToString(5));
			}
		}

		protected internal void PopulateEQD()
		{
			if (!Declaration.IsExWarehouse && Declaration.IsPost)
			{
				foreach (Bill bill in Declaration.Bills)
				{
					if (bill.CU_BillType == Customs.Business.BillTypeList.Codes.HouseBill)
					{
						MessageUtilities.PopulateEQD(cUSDEC.EQD.InstantiateAChildAndAddItToChildrenCollection(),
							EquipmentTypeCodeQualifierList.NoSpecialEquipmentNeeded,
							bill.CU_HouseBill.Trim().SubstringSafe(0, 9),
							CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
					}
				}
			}
		}

		#region FTX Segment

		protected internal override void PopulateFTX()
		{
			base.PopulateFTX();

			ZString amberStatement = Declaration.JE_AmberStatement;

			if (!Declaration.AddInfo.ZA_HART_Hidden.IsEmpty || !amberStatement.IsEmpty)
			{
				FTXSegment fTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.TariffStatements;
				if (!Declaration.AddInfo.ZA_HART_Hidden.IsEmpty)
				{
					fTX.TextReference.FreeTextValueCode = Declaration.AddInfo.ZA_HART_Hidden;
					fTX.TextReference.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
				}
				PopulateNoteFields(fTX, amberStatement.SubstringSafe(0, 2560));
			}

			ZString paidUnderProtestStatement = Declaration.JE_PaidUnderProtestStatement;
			if (!paidUnderProtestStatement.IsEmpty && EntryHeader.HasALineWithPUPIndicator)
			{
				FTXSegment fTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.Dispute;
				PopulateNoteFields(fTX, paidUnderProtestStatement.SubstringSafe(0, 2560));

				if (paidUnderProtestStatement.Length > 2560)
				{
					FTXSegment secondFTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
					secondFTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.Dispute;
					PopulateNoteFields(secondFTX, paidUnderProtestStatement.SubstringSafe(2560, 4000));
				}
			}
		}

		#endregion

		#endregion

		#region Segment Group 1

		protected internal override void PopulateGroup1()
		{
			base.PopulateGroup1();

			if (Declaration.IsTransportModeOther)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ReceivedNumber, Declaration.AddInfo.ZA_CustomsReceipt_Hidden);
			}

			if (InvoiceHeader != null)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.CustomsValuationDecisionNumber, InvoiceHeader.AddInfo.ZA_VAN);
			}

			if (!EntryHeader.IsCMRNature30)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.InvoicingDataSheetReferenceNumber, EntryHeader.ITOTIncoTerm);
			}

			if (Declaration.ImplementUPE && Declaration.IsUPEDeclaration)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.HeaderDocumentProperty, UnaccompaniedPersonalEffectsReferenceIdentifier);
			}

			if (EntryHeader.HasALineWithPUPIndicator)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.RelatedDocumentNumber, Declaration.AddInfo.ZA_FPUP_Hidden);
			}

			if (Declaration.IsSOFADeclaration)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.HeaderDocumentProperty, StatusOfForcesAgreementIdentifier);
			}

			foreach (JobComInvoiceHeader invoice in EntryHeader.InvoiceHeaders)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ExportReferenceNumber, invoice.AddInfo.ZA_DrawbackID);
			}

			PopulateAQISConcernTypes();
			PopulateLodgementQuestions();
			PopulateGroup2();
		}

		const string UnaccompaniedPersonalEffectsReferenceIdentifier = "UPE";
		const string StatusOfForcesAgreementIdentifier = "SOFA";

		protected internal override void PopulateGroup1ForWithdrawal()
		{
			PopulateLodgementQuestions();
			base.PopulateGroup1ForWithdrawal();
		}

		void PopulateAQISConcernTypes()
		{
			Declaration.AQISConcernTypes.SortByUniqueCode();
			foreach (AQISConcernType concernType in Declaration.AQISConcernTypes)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.QuarantineTreatmentStatusReferenceNumber, concernType.Code);
			}
		}

		void PopulateLodgementQuestions()
		{
			if (EntryHeader != null && MessageType != CMRMessageTypes.OriginalForAmendmentDetection)
			{
				foreach (CMRCusEntryCPDec currentQuestion in EntryHeader.Questions)
				{
					if (currentQuestion.IsAnswered)
					{
						ZString questionId = currentQuestion.ON_CPDecNum.ToString().PadLeft(4, '0');
						if (currentQuestion.IsAcknowledge && currentQuestion.IsYes)
						{
							PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ProfileNumber, questionId);
						}
						else if (!currentQuestion.IsAcknowledge)
						{
							PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.StatementNumber, questionId, currentQuestion.ON_AnswerCode.ToString().Trim());
						}
					}
				}
			}
		}

		void PopulateGroup2()
		{
			SegmentGroup2 group2 = null;

			if (Declaration.IsTransportModeOther && !Declaration.IsExWarehouse && Declaration.JE_TotalNoOfPacks > 0)
			{
				group2 = Group1.Group2.InstantiateAChildAndAddItToChildrenCollection();
				PACSegment pAC = group2.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pAC.NumberOfPackages = Declaration.JE_TotalNoOfPacks.ToString();
			}
			else if (Declaration.IsExWarehouse && !EntryHeader.WarehouseNumberOfPacksForMessage.IsEmpty)
			{
				group2 = Group1.Group2.InstantiateAChildAndAddItToChildrenCollection();
				PACSegment pAC = group2.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pAC.NumberOfPackages = EntryHeader.WarehouseNumberOfPacksForMessage.ToString();
			}

			if ((Declaration.IsExWarehouse || Declaration.IsTransportModeOther) && !Declaration.MarksAndNumbers.IsEmpty)
			{
				if (group2 == null)
				{
					group2 = Group1.Group2.InstantiateAChildAndAddItToChildrenCollection();
					PACSegment pAC = group2.PAC.InstantiateAChildAndAddItToChildrenCollection();
					pAC.PackagingDetails.PackagingLevelCoded = PackagingLevelCodedList.Inner;
				}

				PopulateGroup3(group2);
			}
		}

		void PopulateGroup3(SegmentGroup2 group2)
		{
			SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			MarkingInstructionsCodedList marksAndNumbersCode = Declaration.IsExWarehouse ? MarkingInstructionsCodedList.MarkFreeText : MarkingInstructionsCodedList.EntireShipment;
			PopulateMarksAndNumbers(group3.PCI.InstantiateAChildAndAddItToChildrenCollection(), marksAndNumbersCode, Declaration.MarksAndNumbers);
		}

		#endregion

		#region Segment Group 4

		protected override void PopulateAirDetails()
		{
			if (!Declaration.AirlinePrefix.IsEmpty)
			{
				TDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
				TDT.ModeOfTransport.TransportModeNameCode = "A";
				TDT.Carrier.CarrierIdentification = Declaration.AirlinePrefix;
				TDT.Carrier.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;
			}
		}

		#endregion

		#region Segment Group 6

		protected internal override void PopulateGroup6()
		{
			base.PopulateGroup6();
			PopulateBrokerLicence();
		}

		protected override void PopulateGroup6ForWithdrawal()
		{
			base.PopulateGroup6ForWithdrawal();
			PopulateBrokerLicence();
		}

		void PopulateBrokerLicence()
		{
			if (Declaration != null && !Declaration.IsEntryForAnImporter
				&& !AUCustomsDataRegistry.Instance.SoleTrader.Value
				&& MessageType != CMRMessageTypes.OriginalForAmendmentDetection)
			{
				ZString licenceNumber = GlbStaff.CurrentUser.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.BR1);

				if (MessageType == CMRMessageTypes.PreLodge && licenceNumber.IsEmpty)
				{
					licenceNumber = Env.Registry.AUCustoms.PreLodgementLicenceCode;
				}

				if (!licenceNumber.IsEmpty)
				{
					CreateGroup6AndPopulateNAD(PartyFunctionCodeQualifierList.CustomsBroker, licenceNumber.Trim());
				}
			}
		}

		#endregion

		#region Segment Group 5

		DOCSegment PopulateDOCDetails(CodeListIdentificationCodeList idCode, string passportNumber, string passportCountry, out SegmentGroup5 g5)
		{
			g5 = cUSDEC.Group5.InstantiateAChildAndAddItToChildrenCollection();
			DOCSegment dOC = g5.DOC.InstantiateAChildAndAddItToChildrenCollection();
			dOC.DocumentMessageName.DocumentNameCode = Enterprise.Edifact.D99B.Elements.DocumentNameCodeList.Passport;
			dOC.DocumentMessageName.CodeListIdentificationCode = idCode;
			dOC.DocumentMessageDetails.DocumentMessageNumber = passportNumber;
			dOC.DocumentMessageDetails.LanguageNameCode = passportCountry;
			return dOC;
		}

		protected internal override void PopulateGroup5()
		{
			if (Declaration.ImplementUPE && Declaration.IsUPEDeclaration)
			{
				SegmentGroup5 g5Importer;
				DOCSegment importerDOC = PopulateDOCDetails(Enterprise.Edifact.D99B.Elements.CodeListIdentificationCodeList.PassportNumber,
					Declaration.AddInfo.ZA_UPEImporterPassportNumber_Hidden,
					Declaration.AddInfo.ZA_UPEImporterPassportCountry_Hidden, out g5Importer);
				switch (Declaration.AddInfo.ZA_UPEImporterSex_Hidden)
				{
					case "M":
						importerDOC.DocumentMessageDetails.DocumentStatusCode = DocumentStatusCodeList.SexMale;
						break;
					case "F":
						importerDOC.DocumentMessageDetails.DocumentStatusCode = DocumentStatusCodeList.SexFemale;
						break;
				}
				importerDOC.NumberOfCopiesOfDocumentRequired = Declaration.AddInfo.ZA_UPEChildrenCount_Hidden.ToString();

				DTMSegment importerDTM = g5Importer.DTM.InstantiateAChildAndAddItToChildrenCollection();
				importerDTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = DateTimePeriodFunctionCodeQualifierList.BirthDateTime;
				importerDTM.DateTimePeriod.DateTimePeriodValue = Declaration.AddInfo.ZA_UPEImporterDOB_Hidden.ToString("yyyyMMdd");

				SegmentGroup5 g5Spouse;
				DOCSegment spouseDOC = PopulateDOCDetails(Enterprise.Edifact.D99B.Elements.CodeListIdentificationCodeList.PassportNumberSpouse,
					Declaration.AddInfo.ZA_UPESpousePassportNumber_Hidden,
					Declaration.AddInfo.ZA_UPESpousePassportCountry_Hidden, out g5Spouse);
				spouseDOC.DocumentMessageDetails.DocumentMessageSource = Declaration.AddInfo.ZA_UPESpouseName_Hidden;
			}
		}

		#endregion

		#region Segment Group 8

		protected internal override void PopulateGroup8()
		{
			if (Declaration.IsNature30)
			{
				PopulateMOA(Group8, EntryHeader.CustomsValueInAUD, MonetaryAmountTypeCodeQualifierList.CustomsValue);
			}
			else
			{
				PopulateMOA(Group8, EntryHeader.FOB, MonetaryAmountTypeCodeQualifierList.FobValue);
				PopulateMOA(Group8, EntryHeader.CIF, MonetaryAmountTypeCodeQualifierList.CostInsuranceAndFreightCifValue);
				PopulateMOA(Group8, EntryHeader.InvoiceTotal, MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount);
				PopulateTILV(EntryHeader, Group8);

				PopulateMOA(Group8, EntryHeader.Commission, MonetaryAmountTypeCodeQualifierList.CommissionAmount);
				PopulateMOA(Group8, EntryHeader.Discount, MonetaryAmountTypeCodeQualifierList.DiscountAmount);
				PopulateMOA(Group8, EntryHeader.ForeignInlandFreight, MonetaryAmountTypeCodeQualifierList.ForeignInlandFreight);
				PopulateMOA(Group8, EntryHeader.LandingCharges, MonetaryAmountTypeCodeQualifierList.LandingCharges);
				PopulateMOA(Group8, EntryHeader.OtherCharges1, MonetaryAmountTypeCodeQualifierList.OtherValuationChargesCustoms);
				PopulateMOA(Group8, EntryHeader.OtherCharges2, MonetaryAmountTypeCodeQualifierList.OtherDeductibleCharges);
				PopulateMOA(Group8, EntryHeader.OverseasFreight, MonetaryAmountTypeCodeQualifierList.InternationalFreight);
				PopulateMOA(Group8, EntryHeader.OverseasInsurance, MonetaryAmountTypeCodeQualifierList.InsuranceChargesIncurredOutsideOfCustomsTerritory);
				PopulateMOA(Group8, EntryHeader.PackingCosts, MonetaryAmountTypeCodeQualifierList.PackingCostCustoms);
			}
		}

		void PopulateTILV(CusEntryHeader entry, SegmentGroup8 group8)
		{
			Money tILV = entry.TransportAndInsuranceForMessage;

			if (tILV.IsValid)
			{
				MOASegment mOA = group8.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.InsuranceAndTransportChargesCustoms;
				mOA.MonetaryAmount.MonetaryAmountValue = tILV.Amount.Round(2).ToString(2);
				if (tILV.Currency != null)
				{
					mOA.MonetaryAmount.CurrencyIdentificationCode = tILV.Currency.Code;
				}
			}
		}

		#endregion

		#region Segment Group 10

		protected internal override void PopulateGroup10()
		{
			if (!Declaration.IsTransportModeOther && !Declaration.IsExWarehouse)
			{
				if (EntryHeader.PackingGroups.Count > 0 || (!Declaration.IsPost && IsAmendment && !EntryHeader.AmendedHouseBillContainerPacks.IsNullOrEmpty()))
				{
					var group10 = cUSDEC.Group10.InstantiateAChildAndAddItToChildrenCollection();
					MessageUtilities.PopulateDMS(group10.DMS.InstantiateAChildAndAddItToChildrenCollection(), "1");

					var pivotNoManager = EntryHeader.HighHouseContPivotNoManager;
					pivotNoManager.ReCalculateIfNeeded();
					pivotNoManager.AssignLineNumbers();

					if (Declaration.IsPost)
					{
						var lineActionCode = IsAmendment ? LineAction.Amend : LineAction.Insert;
						new IMDTransportLineForPost(Declaration, group10.Group21.InstantiateAChildAndAddItToChildrenCollection()).Populate(1, lineActionCode);
					}
					else if (IsAmendment)
					{
						foreach (var currentPack in EntryHeader.AmendedHouseBillContainerPacks)
						{
							new IMDTransportLineForSeaAndAir(currentPack, EntryHeader, group10.Group21.InstantiateAChildAndAddItToChildrenCollection()).Populate(currentPack.HouseContainerNumber, currentPack.ActionCodeForMessage(EntryHeader));
						}
					}
					else
					{
						foreach (var currentPack in EntryHeader.AllHouseBillContainerPacks)
						{
							new IMDTransportLineForSeaAndAir(currentPack, EntryHeader, group10.Group21.InstantiateAChildAndAddItToChildrenCollection()).Populate(currentPack.HouseContainerNumber, LineAction.Insert);
						}
					}
				}
			}
		}

		#endregion

		#region Segment Group 30

		protected internal override void PopulateGroup30()
		{
			bool isPreLodge = MessageType == CMRMessageTypes.PreLodge && !IsAmendment;
			ICollection unsortedLines;
			if (IsAmendment)
			{
				unsortedLines = EntryHeader.AmendedLines;
			}
			else
			{
				unsortedLines = EntryHeader.MergedLines;
			}

			ArrayList arrayLines = new ArrayList(unsortedLines);
			arrayLines.Sort(new Customs.Business.EntryLineNumberComparer());

			foreach (ICusEntryLine entryLine in arrayLines)
			{
				ZString lineActionCode = MessageType == CMRMessageTypes.OriginalForAmendmentDetection ? LineAction.Insert : entryLine.ActionCodeForMessage.ToString();
				new IMDMessageLine(entryLine, cUSDEC.Group30.InstantiateAChildAndAddItToChildrenCollection(), isPreLodge, MessageType == CMRMessageTypes.OriginalForAmendmentDetection).Populate(entryLine.CL_LineNumber, lineActionCode);
			}
		}

		#endregion

		#region Implementation

		protected internal override ZString DocumentName => "IMD";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.GoodsDeclarationForImportation;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.IMD;

		protected internal override Type TypeOfMessage => typeof(CMRIMDMessage);

		#endregion
	}
}
