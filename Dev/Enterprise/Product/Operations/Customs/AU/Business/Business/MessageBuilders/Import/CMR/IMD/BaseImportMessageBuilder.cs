using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseImportMessageBuilder : CMRCUSDECMessageBuilder
	{
		public BaseImportMessageBuilder(CusEntryHeader entryHeader, CMRMessageTypes messageType)
		{
			Messages = entryHeader.Messages;
			this.EntryHeader = entryHeader;
			this.MessageType = messageType;
			PaymentDetailRetriever = new PaymentDetailRetrieverIncludingAQIS(entryHeader.Declaration, messageType, null);
		}

		public readonly PaymentDetailRetrieverIncludingAQIS PaymentDetailRetriever;
		public readonly CMRMessageTypes MessageType;

		protected bool IsPaymentIncluded
		{
			get { return MessageType == CMRMessageTypes.LodgeWithPay || MessageType == CMRMessageTypes.Payment; }
		}

		public CMRAmendmentWithdrawalReason AmendmentWithdrawalReason
		{
			get { return fCMRAmendmentWithdrawalReason; }
			set { fCMRAmendmentWithdrawalReason = value; }
		}
		CMRAmendmentWithdrawalReason fCMRAmendmentWithdrawalReason;

		protected internal override ZString MessageInterpretation
		{
			get
			{
				if (AmendmentWithdrawalReason != null)
				{
					return AmendmentWithdrawalReason.ReasonText;
				}
				return base.MessageInterpretation;
			}
		}

		protected internal override void GenerateMessageText()
		{
			if (cUSDEC == null)
			{
				cUSDEC = new CUSDECMessage();

				if (IsWithdrawal)
				{
					PopulateHeaderGroupsForWithdrawal();
					PopulateGroup1ForWithdrawal();
					PopulateGroup6ForWithdrawal();
				}
				else
				{
					PopulateHeaderGroups();
					PopulateGroup1();
					PopulateGroup4();
					PopulateGroup5();
					PopulateGroup8();
					PopulateGroup6();
					PopulateGroup10();
					PopulateGroup30();
				}

				if (IsAmendment)
				{
					PopulateGroup1ForAmendment();
				}

				PopulateUNS1();
				PopulateUNS2();
				PopulateUNT();
			}
		}

		public ZBool IsAmendment
		{
			get
			{
				return MessageSubType == Common.MessageBuilders.MessageSubTypes.Change;
			}
		}

		#region Releated Business Objects

		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null && EntryHeader != null)
				{
					fDeclaration = EntryHeader.Declaration;
				}

				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null && EntryHeader != null && EntryHeader.InvoiceHeaders.Length > 0)
				{
					fInvoiceHeader = EntryHeader.RandomHeader;
				}

				return fInvoiceHeader;
			}
		}
		JobComInvoiceHeader fInvoiceHeader;

		#endregion

		#region Header Section

		protected virtual void PopulateHeaderGroups()
		{
			PopulateUNH();
			PopulateBGM();
			PopulateLocations();
			PopulateDTM();
			PopulateGIS();
			PopulateFII();
			PopulateFTX();
			PopulateQuestions();
		}

		protected internal void PopulateHeaderGroupsForWithdrawal()
		{
			PopulateUNH();
			PopulateBGM();
			PopulateFII();
			PopulateFTXForWithdrawal();
			PopulateWithdrawalQuestions();
		}

		#region DTM

		protected internal virtual void PopulateDTM()
		{
			PopulateDTM(DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeActual, Declaration.JE_DateOfArrival);
		}

		protected internal void PopulateDTM(DateTimePeriodFunctionCodeQualifierList dateTimePeriodFunctionCodeQualifier, ZDateTime dateTimePeriodValue)
		{
			if (!dateTimePeriodValue.IsEmpty)
			{
				DTMSegment dTM = cUSDEC.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = dateTimePeriodFunctionCodeQualifier;
				dTM.DateTimePeriod.DateTimePeriodValue = dateTimePeriodValue.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateTimePeriodFormatCode = DateTimePeriodFormatCodeList.Ccyymmdd;
			}
		}

		#endregion

		protected internal virtual void PopulateLocations()
		{
			if (!Declaration.JE_RL_NKFinalDestination.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.PlaceOfDestination, Declaration.JE_RL_NKFinalDestination, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}

			if (!Declaration.JE_RL_NKPortOfArrival.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.PortOfDischarge, Declaration.JE_RL_NKPortOfArrival, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}
		}

		#region GIS Segment

		protected internal virtual void PopulateGIS()
		{
			if (IsAmendment)
			{
				//Re-Calculate effective duty date indicator - for changed messages
				PopulateGIS(Declaration.AddInfo.ZA_EffectDutyDate_Hidden, "EFD");
			}
			else
			{
				//EFT Payment Approved Indicator
				PopulateGIS(IsPaymentIncluded, "EPA");
			}

			//EFT Payment Indicator
			GISSegment gIS = cUSDEC.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Cash ? "N" : "Y");
			gIS.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.MethodsOfPayment;
			gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		protected internal void PopulateGIS(ZBool includeIndicator, ZString processingIndicatorDescriptionCode)
		{
			if (includeIndicator)
			{
				GISSegment gIS = cUSDEC.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(processingIndicatorDescriptionCode);
				gIS.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsIndicator;
				gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		#endregion

		#region FII Segment

		protected internal void PopulateFII()
		{
			if (Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default)
			{
				#pragma warning disable IDE0001 // Prevent simplification to base class
				using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(Declaration))
				#pragma warning restore IDE0001 // Prevent simplification to base class
				{
					Declaration.JE_PaymentMethod = PaymentDetailRetriever.PartyToPayString();
				}
			}

			if (ArePaymentDetailsRequired)
			{
				BankDetails bankDetails = PaymentDetailRetriever.GetBankDetails();

				if ((IsAmendment || IsWithdrawal) && PaymentDetailRetriever.PartyToPayString() == JobDeclaration.PaymentMethods.Importer)
				{
					PopulateFII(bankDetails.AccountNumber, bankDetails.BSBNumber, bankDetails.AccountName);
				}
				else
				{
					PopulateFII(bankDetails.AccountNumber, bankDetails.BSBNumber);
				}
			}
		}

		internal void PopulateFII(ZString accountNumber, ZString bSBNumber, ZString accountName)
		{
			if (!accountNumber.IsEmpty || !bSBNumber.IsEmpty)
			{
				FIISegment fII = cUSDEC.FII.InstantiateAChildAndAddItToChildrenCollection();
				fII.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.NominatedBank;
				fII.AccountHolderIdentification.AccountHolderNumber = accountNumber.KeepChars("0123456789");

				if (!accountName.IsEmpty)
				{
					fII.AccountHolderIdentification.AccountHolderName1 = accountName.SubstringSafe(0, 35);
					fII.AccountHolderIdentification.AccountHolderName2 = accountName.SubstringSafe(35, 5);
				}

				fII.InstitutionIdentification.InstitutionBranchNumber = bSBNumber.KeepChars("0123456789");
				fII.InstitutionIdentification.CodeListResponsibleAgencyCode2 = CodeListResponsibleAgencyCodeList.AuApcaAustralianPaymentsClearingAssociation;
			}
		}

		internal void PopulateFII(ZString accountNumber, ZString bSBNumber)
		{
			PopulateFII(accountNumber, bSBNumber, ZString.Empty);
		}

		#endregion

		#region FTX

		protected internal virtual void PopulateFTX()
		{
			if (!Declaration.IsExWarehouse && Declaration.Importer != null)
			{
				PopulateFTX(TextSubjectCodeQualifierList.DeliveryInformation,
					(Declaration.DocsAndCartage != null && !Declaration.ImporterDeliveryAddress.E2_CompanyName.IsEmpty) ?
					Declaration.ImporterDeliveryAddress.E2_CompanyNameTruncated : Declaration.Importer.OH_FullNameTruncated);
			}

			if (IsAmendment)
			{
				PopulateChangeReasonStatement();
			}
		}

		protected void PopulateFTXForWithdrawal()
		{
			PopulateChangeReasonStatement();
		}

		protected internal void PopulateFTX(TextSubjectCodeQualifierList textSubjectCodeQualifier, ZString freeTextValue1)
		{
			PopulateFTX(textSubjectCodeQualifier, freeTextValue1, ZString.Empty);
		}

		protected internal void PopulateFTX(TextSubjectCodeQualifierList textSubjectCodeQualifier, ZString freeTextValue1, ZString freeTextValue2)
		{
			PopulateFTX(textSubjectCodeQualifier, freeTextValue1, freeTextValue2, ZString.Empty);
		}

		protected internal void PopulateFTX(TextSubjectCodeQualifierList textSubjectCodeQualifier, ZString freeTextValue1, ZString freeTextValue2, ZString freeTextValue3)
		{
			FTXSegment fTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectCodeQualifier = textSubjectCodeQualifier; //TextSubjectCodeQualifierList.Reason;
			fTX.TextLiteral.FreeTextValue1 = freeTextValue1;
			fTX.TextLiteral.FreeTextValue2 = freeTextValue2;
			fTX.TextLiteral.FreeTextValue3 = freeTextValue3;
		}

		protected void PopulateNoteFields(FTXSegment fTX, string noteValue)
		{
			TextSplitter splitter = new TextSplitter(512);
			splitter.Text = noteValue.Replace("\r\n", " ");
			fTX.TextLiteral.FreeTextValue1 = splitter[0];
			fTX.TextLiteral.FreeTextValue2 = splitter[1];
			fTX.TextLiteral.FreeTextValue3 = splitter[2];
			fTX.TextLiteral.FreeTextValue4 = splitter[3];
			fTX.TextLiteral.FreeTextValue5 = splitter[4];
		}

		void PopulateChangeReasonStatement()
		{
			if (AmendmentWithdrawalReason != null && !AmendmentWithdrawalReason.ReasonText.IsEmpty)
			{
				FTXSegment fTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.ChangeInformation;
				PopulateNoteFields(fTX, AmendmentWithdrawalReason.ReasonText.Replace(System.Environment.NewLine, ""));
			}
		}

		#endregion

		#region PopulateQuestions
		protected internal virtual void PopulateQuestions()
		{
		}

		protected virtual void PopulateWithdrawalQuestions()
		{
		}
		#endregion

		#endregion

		#region Segment Group 1

		protected internal virtual void PopulateGroup1()
		{
			if (Declaration.AutoAssignImporterRef || !Declaration.JE_OwnerRef.IsEmpty)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ImporterReferenceNumber, EDIMessage.OwnerReferencePlaceHolder);
			}

			if (Env.Registry.AUCustoms.AgentsReferenceDefaulting == Core.Constants.AgentsReferenceDefaulting.FAR
				|| Env.Registry.AUCustoms.AgentsReferenceDefaulting == Core.Constants.AgentsReferenceDefaulting.DEF
				|| !Declaration.JE_AgentsReference.IsEmpty)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.BrokerReference1, EDIMessage.AgentReferencePlaceHolder);
			}

			if (ArePaymentDetailsRequired)
			{
				PopulateBankAccountOwner();
			}
		}

		protected internal virtual void PopulateGroup1ForWithdrawal()
		{
			PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber, EntryHeader.EntryNumber);

			if (ArePaymentDetailsRequired)
			{
				PopulateBankAccountOwner();
			}
		}

		protected void PopulateGroup1ForAmendment()
		{
			PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber, EntryHeader.EntryNumber);
		}

		protected void PopulateRFF(SegmentGroup1 group1, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier)
		{
			PopulateRFF(group1, referenceFunctionCodeQualifier, referenceIdentifier, ZString.Empty);
		}

		protected void PopulateRFF(SegmentGroup1 group1, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier, ZString value)
		{
			if (!referenceIdentifier.IsEmpty)
			{
				RFFSegment rFF = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceFunctionCodeQualifier = referenceFunctionCodeQualifier;
				rFF.Reference.ReferenceIdentifier = referenceIdentifier;
				rFF.Reference.ReferenceVersionIdentifier = value;
			}
		}

		void PopulateBankAccountOwner()
		{
			if (PaymentDetailRetriever.PartyToPayEntry == PaymentParty.Importer && Declaration.Importer != null)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.FinancialTransactionReferenceNumber, "I");
			}
			else
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.FinancialTransactionReferenceNumber, "B");
			}
		}

		#region Group 1

		protected SegmentGroup1 fGroup1;
		protected SegmentGroup1 Group1
		{
			get
			{
				if (fGroup1 == null)
				{
					fGroup1 = cUSDEC.Group1.InstantiateAChildAndAddItToChildrenCollection();
				}

				return fGroup1;
			}
		}

		#endregion

		#endregion

		#region Segment Group 4

		protected abstract void PopulateAirDetails();
		protected void PopulateSeaDetails()
		{
			ZString lloydsNumber = Declaration.VesselNumber;
			if (!Declaration.JE_VoyageFlightNo.IsEmpty && !lloydsNumber.IsEmpty)
			{
				TDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
				TDT.ConveyanceReferenceNumber = Declaration.JE_VoyageFlightNo;
				TDT.ModeOfTransport.TransportModeNameCode = "S";

				TDT.TransportIdentification.TransportMeansIdentificationNameIdentifier = lloydsNumber;
				TDT.TransportIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.LloydsRegisterOfShipping;
			}
		}

		protected internal void PopulateGroup4()
		{
			if (!Declaration.IsExWarehouse)
			{
				if (Declaration.IsSea)
				{
					PopulateSeaDetails();
				}
				else if (Declaration.IsAir)
				{
					PopulateAirDetails();
				}
				else
				{
					TDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
					TDT.ModeOfTransport.TransportModeNameCode = Declaration.IsPost ? "P" : "O";
				}
			}
		}

		#region TDT

		TDTSegment fTDT;
		protected TDTSegment TDT
		{
			get
			{
				if (fTDT == null)
				{
					SegmentGroup4 group4 = cUSDEC.Group4.InstantiateAChildAndAddItToChildrenCollection();
					fTDT = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				}

				return fTDT;
			}
		}

		#endregion

		#endregion

		#region Segment Group 5

		protected internal virtual void PopulateGroup5() { }

		#endregion

		#region Segment Group 6

		protected internal virtual void PopulateGroup6()
		{
			PopulateNADForImporter(Declaration.Importer);
			PopulateLocalCustomsBranchIdentifierIfSpecified();
			PopulateDeliveryAddress();
		}

		protected virtual SegmentGroup6 PopulateLocalCustomsBranchIdentifierIfSpecified()
		{
			var localCustomsBranchId = Env.Registry.AUCustoms.LocalCustomsBranchIdentifier;
			var group6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
			var nAD = group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Branch;
			if (!string.IsNullOrEmpty(localCustomsBranchId))
			{
				nAD.PartyIdentificationDetails.PartyIdentifier = localCustomsBranchId.Replace(" ", "");
				nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}

			return group6;
		}

		protected internal virtual void PopulateNADForImporter(OrgHeader importer)
		{
			if (importer != null)
			{
				var importerABNFromOrganisation = importer.LocalBusinessRegNo.Trim();
				if (!importerABNFromOrganisation.IsEmpty)
				{
					var splitter = new ABNCACSplitter(importerABNFromOrganisation);

					var importerABN = splitter.ABN;
					if (!importerABN.IsEmpty)
					{
						CreateGroup6AndPopulateNAD(PartyFunctionCodeQualifierList.AuthorizedImporter, importerABN);
					}

					var importerCAC = importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CreditAgencyCode).Trim();
					if (importerCAC.IsEmpty)
					{
						importerCAC = splitter.CAC;
					}
					if (!importerCAC.IsEmpty)
					{
						CreateGroup6AndPopulateNAD(PartyFunctionCodeQualifierList.SubEntity, importerCAC);
					}
				}
				else
				{
					var customsClientID = importer.GetCustomsClientID().Trim();
					if (!customsClientID.IsEmpty)
					{
						CreateGroup6AndPopulateNAD(PartyFunctionCodeQualifierList.Importer, customsClientID.SubstringSafe(0, 11));
					}
				}
			}
		}

		protected virtual void PopulateGroup6ForWithdrawal()
		{
		}

		protected void CreateGroup6AndPopulateNAD(PartyFunctionCodeQualifierList partyFunctionCodeQualifier, ZString partyIdentifier)
		{
			var group6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
			NADSegment nAD = group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
			nAD.PartyIdentificationDetails.PartyIdentifier = partyIdentifier.Replace(" ", "");
			nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		void PopulateDeliveryAddress()
		{
			if (!Declaration.IsExWarehouse && Declaration.DocsAndCartage != null)
			{
				if (!Declaration.ImporterDeliveryAddress.E2_City.IsEmpty ||
					!Declaration.ImporterDeliveryAddress.E2_Address1.IsEmpty ||
					!Declaration.ImporterDeliveryAddress.E2_Address2.IsEmpty ||
					!Declaration.ImporterDeliveryAddress.E2_State.IsEmpty ||
					!Declaration.ImporterDeliveryAddress.E2_Postcode.IsEmpty)
				{
					var group6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
					NADSegment nAD = group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
					nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.DeliveryParty;

					// Name - Delivery Locality
					int maxLengthOfCityElement = 35;
					int maxLengthOfEntireCityField = 46;
					TextSplitter splitter = new TextSplitter(maxLengthOfCityElement);
					splitter.Text = Declaration.ImporterDeliveryAddress.E2_City.Left(maxLengthOfEntireCityField).Replace("\r\n", " ");
					nAD.NameAndAddress.NameAndAddressLine1 = splitter[0];
					nAD.NameAndAddress.NameAndAddressLine2 = splitter[1];

					// Address 1 (Line 1 + 2)
					int maxLengthOfAddressSegment = 35;
					int maxLengthOfEntireAddressField = 40;
					splitter = new TextSplitter(maxLengthOfAddressSegment);
					splitter.Text = Declaration.ImporterDeliveryAddress.E2_Address1.Left(maxLengthOfEntireAddressField).Replace("\r\n", " ");
					nAD.Street.StreetAndNumberPOBox1 = splitter[0];
					nAD.Street.StreetAndNumberPOBox2 = splitter[1];

					// Address 2 (Line 1 + 2)
					splitter = new TextSplitter(maxLengthOfAddressSegment);
					splitter.Text = Declaration.ImporterDeliveryAddress.E2_Address2.Left(maxLengthOfEntireAddressField).Replace("\r\n", " ");
					nAD.Street.StreetAndNumberPOBox3 = splitter[0];
					nAD.Street.StreetAndNumberPOBox4 = splitter[1];

					// State / Postcode / Country
					int maxLengthOfStateSegment = 10;
					int maxLengthOfPostCodeSegment = 12;
					nAD.CountrySubEntityDetails.CountrySubEntityName = Declaration.ImporterDeliveryAddress.E2_State.Left(maxLengthOfStateSegment);
					nAD.PostalIdentificationCode = Declaration.ImporterDeliveryAddress.E2_Postcode.Left(maxLengthOfPostCodeSegment);
					nAD.CountryNameCode = Core.Constants.CountryCodes.Australia;
				}
			}
		}

		#endregion

		#region Segment Group 8

		protected internal abstract void PopulateGroup8();

		protected void PopulateMOA(SegmentGroup8 group8, Money monetaryAmount, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			if (!monetaryAmount.Amount.IsEmpty || qualifier == MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount)
			{
				MOASegment mOA = group8.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;

				ZDecimal amount = monetaryAmount.Amount;

				mOA.MonetaryAmount.MonetaryAmountValue = amount.ToString(2);
				if (monetaryAmount.Currency != null)
				{
					mOA.MonetaryAmount.CurrencyIdentificationCode = monetaryAmount.Currency.Code;
				}
			}
		}

		#region Group 8

		protected SegmentGroup8 fGroup8;
		protected SegmentGroup8 Group8
		{
			get
			{
				if (fGroup8 == null)
				{
					fGroup8 = cUSDEC.Group8.InstantiateAChildAndAddItToChildrenCollection();
				}

				return fGroup8;
			}
		}

		#endregion

		#endregion

		#region Segment Group 10

		protected internal virtual void PopulateGroup10()
		{
		}

		#endregion

		#region UNS One

		protected internal void PopulateUNS1()
		{
			MessageUtilities.PopulateUNS(cUSDEC.UNS1.InstantiateAChildAndAddItToChildrenCollection(), SectionIdentificationList.HeaderDetailSectionSeparation);
		}

		#endregion

		#region Segment Group 30

		protected internal abstract void PopulateGroup30();

		#endregion

		#region UNS Two

		protected internal void PopulateUNS2()
		{
			MessageUtilities.PopulateUNS(cUSDEC.UNS2.InstantiateAChildAndAddItToChildrenCollection(), SectionIdentificationList.DetailSummarySectionSeparation);
		}

		#endregion

		bool ArePaymentDetailsRequired
		{
			get
			{
				return IsPaymentIncluded || ((IsAmendment || IsWithdrawal) && EntryHeader.IsCustomsChargePaid);
			}
		}

		protected void PopulateMarksAndNumbers(PCISegment pCI, MarkingInstructionsCodedList markingInstructionsCode, ZString marksAndNumbers)
		{
			pCI.MarkingInstructionsCoded = markingInstructionsCode;

			TextSplitter splitter = new TextSplitter(35);
			splitter.Text = marksAndNumbers.Trim().Replace("\r\n", " ");
			pCI.MarksLabels.ShippingMarks1 = splitter[0];
			pCI.MarksLabels.ShippingMarks2 = splitter[1];
			pCI.MarksLabels.ShippingMarks3 = splitter[2];
			pCI.MarksLabels.ShippingMarks4 = splitter[3];
			pCI.MarksLabels.ShippingMarks5 = splitter[4];
			pCI.MarksLabels.ShippingMarks6 = splitter[5];
			pCI.MarksLabels.ShippingMarks7 = splitter[6];
			pCI.MarksLabels.ShippingMarks8 = splitter[7];
			pCI.MarksLabels.ShippingMarks9 = splitter[8];
			pCI.MarksLabels.ShippingMarks10 = splitter[9];
		}

		protected readonly CusEntryHeader EntryHeader;
	}
}
