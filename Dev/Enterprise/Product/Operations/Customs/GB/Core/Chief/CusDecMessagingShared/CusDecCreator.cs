using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.Chief.CusDecMessagingShared;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Chief.Messaging;
using Enterprise.Customs.GB.Chief.Messaging.Converters;
using Enterprise.Edifact;
using Enterprise.Edifact.D04A.Elements;
using Enterprise.Edifact.D04A.Messages.CUSDEC;
using Enterprise.Edifact.D04A.Segments;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Chief.CusDec
{
	public class CusDecCreator
	{
		static CodeListResponsibleAgencyCodeList hmrc109;
		protected GbChiefHeader gbChiefHeader;
		protected CusEntryHeader cusEntryHeader;
		protected GbChiefImportHeader gbImportHeader;
		protected GbChiefExportHeader gbExportHeader;
		protected CUSDECMessage cusDec;
		bool headerConsigneeHasBeenAdded;
		bool headerShipperHasAlreadyBeenAdded;
		protected QueryMessageFunction queryMessageFunction;

		public CusDecCreator(JobDeclaration declaration, CusEntryHeader cusEntryHeader, QueryMessageFunction queryMessageFunction, ErrorCollector errorCollector)
		{
			this.queryMessageFunction = queryMessageFunction;
			SaveConstructorOptions(declaration, cusEntryHeader, errorCollector);
		}

		public CusDecCreator(QueryMessageFunction queryMessageFunction, ErrorCollector errorCollector)
		{
			this.queryMessageFunction = queryMessageFunction;
			this.errorCollector = errorCollector;
			hmrc109 = CodeListResponsibleAgencyCodeList.GetFromString("109");
		}

		public CusDecCreator(GbDes242MessageFunction messageFunction, ErrorCollector errorCollector)
		{
			this.errorCollector = errorCollector;
			hmrc109 = CodeListResponsibleAgencyCodeList.GetFromString("109");
		}

		public CusDecCreator(JobDeclaration declaration, CusEntryHeader cusEntryHeader,
			CusDecMessageTypeFunction declarationMessageFunctionNewAmendedDeleted, ErrorCollector errorCollector)
		{
			messageTypeNewAmendedDeleted = declarationMessageFunctionNewAmendedDeleted;
			SaveConstructorOptions(declaration, cusEntryHeader, errorCollector);
			cusEntryHeader.CH_CEI_Instruction = declaration.CustomsEntryInstructions.FirstOrDefault().PK;
			var gbDec = declaration as Business.Declaration.JobDeclaration;
			if (gbDec != null)
			{
				messageProcedure = MessageProcedureConverter.GetMessageProcedure(gbDec.JE_DeclarationType, gbDec.JE_EntrySubStyle);
			}

			if (messageProcedure == MessageProcedure.UnknownCargoWiseShowError)
			{
				errorCollector.AddError("Declaration type/substyle combination or value is invalid", new ErrorInfo("1", "mandatory", false));
			}
			headerConsigneeHasBeenAdded = false;
			headerShipperHasAlreadyBeenAdded = false;
		}

		void SaveConstructorOptions(JobDeclaration declaration, CusEntryHeader cusEntryHeader, ErrorCollector errorCollector)
		{
			this.errorCollector = errorCollector;
			this.cusEntryHeader = cusEntryHeader;
			if (declaration.IsExport)
			{
				this.gbChiefHeader = new GbChiefExportHeader(cusEntryHeader);
				gbExportHeader = gbChiefHeader as GbChiefExportHeader;
			}
			else
			{   // Import
				this.gbChiefHeader = new GbChiefImportHeader(cusEntryHeader);
				gbImportHeader = gbChiefHeader as GbChiefImportHeader;
			}

			hmrc109 = CodeListResponsibleAgencyCodeList.GetFromString("109");
		}

		public string CreateEdifactString(UNCharacterSet characterSet)
		{
			if (cusDec == null)
			{
				CreateCusDec();
			}
			return cusDec.ToString(characterSet);
		}

		public string CreateEdifactString()
		{
			if (cusDec == null)
			{
				CreateCusDec();
			}
			return cusDec.ToString(CharacterSetDefault);
		}

		public static UNCharacterSet CharacterSetDefault
		{
			get
			{
				return new UkCharSet();
			}
		}

		public static UNCharacterSet CharacterSetMcp
		{
			get
			{
				string una = @"UNA\#.? {";  // MCP's spazzy format
				char subElementDelimiterChar = una[3];
				char elementDelimiterChar = una[4];
				char escapeCharacterChar = una[6];
				char segmentDelimiterChar = una[8];
				UkCharSet characterSet = new UkCharSet(subElementDelimiterChar, elementDelimiterChar, escapeCharacterChar, segmentDelimiterChar);
				return characterSet;
			}
		}

		public CUSDECMessage CreateInterrogationMessage()
		{
			var qmf = queryMessageFunction;
			if (qmf is Interrogate_DecDucr || qmf is Interrogate_Lem || qmf is Interrogate_EAD)
			{
				CreateInterrogationShellNoUnt();
				HeaderGroup1RffABO();
			}
			else if (qmf is Interrogate_DevDucr || qmf is Interrogate_Des)
			{
				CreateInterrogationShellNoUnt();
				HeaderGroup0CST(qmf is Interrogate_Des);
				HeaderGroup1RffABO();
			}
			else if (qmf is Interrogate_DecMucr)
			{
				CreateInterrogationShellNoUnt();
				HeaderGroup1RffUCN().Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.ABO);  // even for mucrs, the ref must still be ABO.  Confusing but correct.
			}
			else if (qmf is Interrogate_Dem)
			{
				CreateInterrogationShellNoUnt();
				AddMovementNumberForDemInterrogation();
			}
			else if (qmf is Interrogate_Req)
			{
				CreateInterrogationShellNoUnt();
				HeaderGroup1RffABO();
				HeaderGroup0FtxForCancellationOrAmendment();
			}
			else
			{
				throw new NotImplementedException("This interrogation message type not supported");
			}

			FooterUNT();
			return cusDec;
		}

		void AddMovementNumberForDemInterrogation()
		{
			var how = queryMessageFunction as Interrogate_Dem;
			if (how != null)
			{
				RFFSegment rff = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.AES); //MOVT-NO
				rff.Reference.ReferenceIdentifier = how.MovementNumber; //MOVT-NO
			}
		}

		protected void CreateInterrogationShellNoUnt()
		{
			this.cusDec = new CUSDECMessage();
			UNHSegment unh = HeaderGroup0UNH();
			unh.MessageIdentifier.AssociationAssignedCode = "109" + queryMessageFunction.AsgCode;  // HMRC-ASG-CODE
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;    // SYS-CAR
			HeaderGroup0BGM(queryMessageFunction.FunctionCode, "13");
			FooterUNS1_D();
			FooterUNS2_S();
		}

		public CUSDECMessage CreateCusDec()
		{
			this.cusDec = new CUSDECMessage();
			switch (this.messageTypeNewAmendedDeleted)
			{
				case CusDecMessageTypeFunction.Original:
					CreateCusDec_New();
					break;
				case CusDecMessageTypeFunction.Replacement:
					CreateCusDec_Amendment();
					break;
				case CusDecMessageTypeFunction.Delete:
					CreateCusDec_Delete();
					break;
			}
			return this.cusDec;
		}

		void CreateCusDec_Delete()
		{
			HeaderGroup0UNHandBGMandCST();
			HeaderGroup0FtxForCancellationOrAmendment();
			HeaderGroup1RffABO();
			FooterUNS1_D();
			FooterUNS2_S();
			FooterUNT();
		}

		void CreateCusDec_Amendment()
		{
			CreateCusDec_NewWithoutUnt();
			HeaderGroup0FtxForCancellationOrAmendment();
			FooterUNT();
		}

		void CreateCusDec_New()
		{
			CreateCusDec_NewWithoutUnt();
			FooterUNT();
		}

		void CreateCusDec_NewWithoutUnt()
		{
			var locationHash = new HashSet<ZString>();

			bool isExport = gbExportHeader != null;
			bool isImport = gbImportHeader != null;
			HeaderGroup0UNHandBGMandCST();
			HeaderGroup0GeiCommon();  // Fecs etc
			HeaderGroup0LocationsCommon(locationHash);
			if (isExport)
			{
				HeaderGroup0LocationsExport(locationHash);
				HeaderGroup0DtmExport();
				HeaderGroup0GeiExport(); // Fecs etc
				HeaderGroup0SealsExportOnly();
			}
			else if (isImport)
			{
				HeaderGroup0LocationsImport();
				HeaderGroup0DtmImport();
				HeaderGroup0GeiImport(); // Fecs etc and freight appt code
			}

			HeaderGroup0FtxAdditionalInfos();
			HeaderGroup1RFF();
			HeaderGroup4TDT();
			HeaderGroup5DOCandDTM();
			HeaderGroup6Parties();
			HeaderGroup7TOD();
			SegmentGroup10 grp10 = PostHeaderGroup10UNSandDMS();
			PostHeaderGroup11(grp10);
			foreach (GbLine line in gbChiefHeader.Lines)
			{
				AddLine(line);
			}

			FooterUNS2_S();
			if (isImport || (isExport && (!gbExportHeader.IsELP && !gbExportHeader.IsESP)))
			{
				FooterCNT();
			}
		}

		void HeaderGroup0FtxForCancellationOrAmendment()
		{
			if (!gbChiefHeader.AmendmentReason.IsEmpty)
			{
				FTXSegment ftx = cusDec.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("CUS");
				var reason = gbChiefHeader.AmendmentReason.Replace(System.Environment.NewLine, " ");
				reason = ChiefTextClass.T1(reason);
				StringLineBreaker breaker = new StringLineBreaker(reason, false, StringLineBreaker.AllowedCharsCaseNotCaseSensitiveSet);
				ftx.TextLiteral.FreeText1 = breaker.GetNextLine(512);
				ftx.TextLiteral.FreeText2 = breaker.GetNextLine(512);
				ftx.TextLiteral.FreeText3 = breaker.GetNextLine(512);
				ftx.TextLiteral.FreeText4 = breaker.GetNextLine(512);
				ftx.TextLiteral.FreeText5 = breaker.GetNextLine(512);
			}
			else
			{
				errorCollector.AddError("There are no reasons/comments. It's not possible to send an XTC/REQ message without a reason. Supply one on the 'Entries' tab, top-right corner.", new ErrorInfo("FTX", "mandatory"));
			}
		}

		void HeaderGroup0FtxAdditionalInfos()
		{
			int i = 0;
			foreach (IStatement statement in this.gbChiefHeader.Statements)
			{
				FTXSegment ftx = cusDec.FTX[i];
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("ACB");
				ftx.TextReference.FreeTextDescriptionCode = statement.Statement;
				ftx.TextLiteral.FreeText1 = ChiefTextClass.T1(statement.StatementText);
				i++;
				if (i == 40)
				{
					break;
				}
			}
		}

		void FooterCNT()
		{
			if (gbChiefHeader.TotalPackages == 0)
			{
				if (!(gbImportHeader != null && gbImportHeader.IsFSD))  // FSD needs no packages
				{
					AddNewError("Total packages", ChiefDataElementsLengths.Codes.TOT_PKGS, "6");
				}
			}
			CNTSegment cnt = cusDec.CNT[0];
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.GetFromString("11");
			cnt.Control.ControlTotalQuantity = gbChiefHeader.TotalPackages.ToString();
			if (gbImportHeader != null && gbImportHeader.IsFSD)
			{
				cnt.Control.ControlTotalQuantity = "1";
				/*
				 From: CFSP, COPE (LocalCOMP) [mailto:cfsp_cope@hmrc.gsi.gov.uk]
				Sent: 05 March 2010 17:51
				To: Daniel Clarke
				Subject: RE: Total packages and FINSD statement
				Hello Daniel
				Total packages although not relevant to the FSD is a mandatory field so has to have a minimum of 1 package in it, so for the FSD it’s always declared as 1.
				 */
			}
		}

		protected void FooterUNT()
		{
			UNTSegment unt = cusDec.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = cusDec.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = SYS_MRN;// SYS-MRN
		}

		void HeaderGroup0UNHandBGMandCST()
		{
			UNHSegment unh = HeaderGroup0UNH();
			unh.MessageIdentifier.AssociationAssignedCode = "109" + gbChiefHeader.HMRC_ASG_CODE(this.messageTypeNewAmendedDeleted);  // HMRC-ASG-CODE
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder; //this.cusEntryHeader.PK.ToString().Replace("-", "");   // SYS-CAR
			string messageCode = gbChiefHeader.GetMessageCode(this.messageTypeNewAmendedDeleted);
			string funct = System.Enum.Format(typeof(CusDecMessageTypeFunction), this.messageTypeNewAmendedDeleted, "D");
			HeaderGroup0BGM(messageCode, funct);
			HeaderGroup0CST(queryMessageFunction is Interrogate_Des);
		}

		void HeaderGroup0GeiImport()
		{
			if (!gbChiefHeader.IsICR)
			{
				//FRGT-APRT-CODE - freight apportionment code. The element defines whether apportionment is to be by gross mass or value. 1 => by gross mass. If this element is not supplied apportionment is by value.
				if (gbImportHeader.FreightApportionmentIndicator == GbChiefImportHeader.FreightApportionmentIndicatorPositiveFlag)  // will be "1" or ""
				{
					AddGEI(cusDec.GEI, "5", "FA", "109").ProcessingIndicator.ProcessingIndicatorDescriptionCode =
						ProcessingIndicatorDescriptionCodeList.GetFromString(GbChiefImportHeader.FreightApportionmentIndicatorPositiveFlag);
				}

				// FEC:  DSP for DISP-CNTRY (Imports only)"
				if (gbImportHeader.FECCountryOfExport)
				{
					AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.HeaderLevel_DSP_ImportsOnly, cusDec.GEI);
				}
			}
		}

		void HeaderGroup0GeiExport()
		{
			if (gbExportHeader.FECCountryOfDestination)
			{   //PROC-INST  for DEST-CNTRY
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.CommonLevel_DST_ExportsOnly, cusDec.GEI);
			}

			if (gbExportHeader.RequestProgressReport)
			{   //PROC-INST
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.HeaderLevel_PRG_ExportsOnly, cusDec.GEI);
			}
		}

		void HeaderGroup0GeiCommon()
		{
			// Want an acceptance report?
			if (gbChiefHeader.FECAcceptanceReport)
			{
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.HeaderLevel_ACC, cusDec.GEI);
			}

			// Wants route F
			if (gbChiefHeader.FECRouteF)
			{
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.HeaderLevel_RTF, cusDec.GEI);
			}

			// TRPT-CNTRY "flag"
			if (gbChiefHeader.FECTransportNationalityAtTheBorder)
			{
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.HeaderLevel_FLG, cusDec.GEI);
			}
		}

		void PostHeaderGroup11(SegmentGroup10 grp10)
		{
			if (gbImportHeader != null && !gbImportHeader.IsICR)
			{
				var sg11 = grp10.Group11.InstantiateAChildAndAddItToChildrenCollection();
				AddAmount(sg11.MOA, gbImportHeader.OSAirTransportAmount, "144", null); //ATRPT-COST-AC
				AddAmount(sg11.MOA, gbImportHeader.FreightCharges, "64", gbImportHeader.FreightChargesCurrency);  //FRGT-CHGE-AC, FRGT-CHGE-CRRN
				AddAmount(sg11.MOA, gbImportHeader.InsuranceAmount, "70", gbImportHeader.InsuranceCurrency);  //INS-AMT-AC, INS-AMT-CRRN
				AddAmount(sg11.MOA, gbImportHeader.TotalAmountInvoiced, "39", gbImportHeader.InvoiceCurrency);  //INV-TOT-AC, INV-CRRN
				AddAmount(sg11.MOA, gbImportHeader.OtherChargesDeductionsValue, "103", gbImportHeader.OtherChargesDeductionsCurrency);  //OCD-AC, OCD-CRRN
				AddAmount(sg11.MOA, gbImportHeader.AdjustmentForVATValue, "105", gbImportHeader.AdjustmentForVATValueCurrency);  //VAT-ADJT-AC, VAT-ADJT-CRRN

				bool addDiscountAmount = !gbImportHeader.DiscountAmount.IsEmpty;
				bool addDiscountPercent = !gbImportHeader.DiscountPercentage.IsEmpty;
				if (addDiscountAmount || addDiscountPercent)
				{
					SegmentGroup19 grp19 = grp10.Group19.InstantiateAChildAndAddItToChildrenCollection();
					grp19.ALC[0].AllowanceOrChargeCodeQualifier = AllowanceOrChargeCodeQualifierList.GetFromString("A");
					if (addDiscountAmount)
					{
						AddAmount(grp19.MOA, gbImportHeader.DiscountAmount, "52", gbImportHeader.DiscountAmountCurrency);  //INV-DAMT-AC, INV-DAMT-CURR
					}
					if (addDiscountPercent)
					{
						grp19.PCD[0].PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.GetFromString("12");
						grp19.PCD[0].PercentageDetails.Percentage = gbImportHeader.DiscountPercentage.ToStringTrimZeros("#.00");  //INV-DPCT
					}
				}
			}
			else if (gbExportHeader != null && (gbExportHeader.IsEFD || gbExportHeader.IsESD))
			{
				var sg11 = grp10.Group11.InstantiateAChildAndAddItToChildrenCollection();
				AddAmount(sg11.MOA, ZDecimal.Zero, "39", gbExportHeader.InvoiceCurrency, true);  //INV-CRRN
			}
		}

		SegmentGroup10 PostHeaderGroup10UNSandDMS()
		{
			// Post-header, pre-line data:
			FooterUNS1_D();
			SegmentGroup10 grp10 = cusDec.Group10[0];
			if (gbExportHeader == null || !gbExportHeader.IsEXS)
			{
				grp10.DMS[0].DocumentMessageIdentification.DocumentIdentifier = GbTransmissionMessageGenerator.DeclarationReferenceWithBox7OwnerReferencePlaceHolder;//TDR-OWN-REF-ENT
			}
			return grp10;
		}

		protected void FooterUNS2_S()
		{
			cusDec.UNS2.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = "S";
		}

		protected void FooterUNS1_D()
		{
			cusDec.UNS1.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = "D";
		}

		void HeaderGroup7TOD()
		{
			// *** GROUP SEVEN
			if (!gbChiefHeader.TransportChargesMethodOfPayment.IsEmpty)
			{
				SegmentGroup7 grp7 = cusDec.Group7[0];
				TODSegment tod = grp7.TOD[0];
				tod.TransportChargesPaymentMethodCode = TransportChargesPaymentMethodCodeList.GetFromString(gbChiefHeader.TransportChargesMethodOfPayment);  // TRPT-CHGE-MOP
			}
		}

		void HeaderGroup6Parties()
		{
			SegmentGroup6 grp6 = cusDec.Group6[0];
			int nadIndex = -1;
			nadIndex = HeaderGroup6Party_Shipper(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_Consignee(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_Declarant(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_LC(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_PG(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_Representation(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_Warehouse(grp6, nadIndex);
			nadIndex = HeaderGroup6Party_Spoff(grp6, nadIndex);
		}

		int HeaderGroup6Party_Spoff(SegmentGroup6 grp6, int nadIndex)
		{
			// Supervising office
			if (gbChiefHeader.SupervisingOffice.IsNotMissing)
			{
				nadIndex++;
				NADSegment nadCm = grp6.NAD[nadIndex];
				nadCm.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.CM_SupervisingOffice);
				nadCm.PartyIdentificationDetails.PartyIdentifier = gbChiefHeader.SupervisingOffice.EoriCode; //SPOFF-TID
				nadCm.PartyName.PartyName1 = gbChiefHeader.SupervisingOffice.Name.GetCusDecValue(ChiefDataElementsLengths.Codes.SPOFF_NAME, ChiefTextClass.T1); //SPOFF-NAME
				nadCm.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = gbChiefHeader.SupervisingOffice.Street.GetCusDecValue(ChiefDataElementsLengths.Codes.SPOFF_STREET, ChiefTextClass.T1); //SPOFF-STREET
				nadCm.CityName = gbChiefHeader.SupervisingOffice.City.GetCusDecValue(ChiefDataElementsLengths.Codes.SPOFF_CITY, ChiefTextClass.T1);  //SPOFF-CITY
				nadCm.PostalIdentificationCode = gbChiefHeader.SupervisingOffice.PostCode.GetCusDecValue(ChiefDataElementsLengths.Codes.SPOFF_POSTCODE, ChiefTextClass.T6); //SPOFF-POSTCODE
				nadCm.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(gbChiefHeader.SupervisingOffice.CountryCode); //SPOFF-CTRY
				return nadIndex;
			}
			return nadIndex;
		}

		int HeaderGroup6Party_Warehouse(SegmentGroup6 grp6, int nadIndex)
		{
			// Warehouse
			var isIntoOrOutOfRegimeProcedure = false;

			foreach (GbLine line in gbChiefHeader.Lines)
			{
				if (!line.actualEntryLine.RandomLine.IsNull && line.actualEntryLine.RandomLine.IsIntoOrOutOfRegimeProcedure)
				{
					isIntoOrOutOfRegimeProcedure = true;
					break;
				}
			}

			if (gbChiefHeader.Premises.IsNotMissing && isIntoOrOutOfRegimeProcedure)
			{
				ZString warehouseId = gbChiefHeader.WarehouseId;
				nadIndex++;
				NADSegment nadWh = grp6.NAD[nadIndex];
				nadWh.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.WH_PremisesWarehouse);
				bool isClearanceRequest = gbChiefHeader.IsICR || gbChiefHeader.IsECR;
				if (!isClearanceRequest) // not needed for ICR & ECR
				{
					nadWh.PartyIdentificationDetails.PartyIdentifier = gbChiefHeader.WarehouseId; //PREM-ID
				}
				if (!warehouseId.EndsWith("GB", System.StringComparison.OrdinalIgnoreCase) || isClearanceRequest)
				{
					// Only foreign warehouses and CCRs need the full details
					nadWh.PartyName.PartyName1 = gbChiefHeader.Premises.Name.GetCusDecValue(ChiefDataElementsLengths.Codes.PREM_NAME, ChiefTextClass.T1); //PREM-NAME
					nadWh.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = gbChiefHeader.Premises.Street.GetCusDecValue(ChiefDataElementsLengths.Codes.PREM_STREET, ChiefTextClass.T1); //PREM-STREET
					nadWh.CityName = gbChiefHeader.Premises.City.GetCusDecValue(ChiefDataElementsLengths.Codes.PREM_CITY, ChiefTextClass.T1);  //PREM-CITY
					nadWh.PostalIdentificationCode = gbChiefHeader.Premises.PostCode.GetCusDecValue(ChiefDataElementsLengths.Codes.PREM_POSTCODE, ChiefTextClass.T6); //PREM-POSTCODE
					nadWh.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(gbChiefHeader.Premises.CountryCode); //PREM-CTRY
				}
			}
			return nadIndex;
		}

		int HeaderGroup6Party_Representation(SegmentGroup6 grp6, int nadIndex)
		{
			if (gbExportHeader != null)
			{
				IExportLine firstExportLine = gbExportHeader.Lines as IExportLine;
				if (firstExportLine != null)
				{
					nadIndex++;
					NADSegment nadAe = grp6.NAD[nadIndex];
					nadAe.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.AE_Representative);
					nadAe.PartyName.PartyName1 = firstExportLine.PrincipalsRepresentativeName.GetCusDecValue(ChiefDataElementsLengths.Codes.REPR_NAME, ChiefTextClass.T1); //REPR-NAME
					nadAe.CityName = firstExportLine.PrincipalsRepresentativeCity.GetCusDecValue(ChiefDataElementsLengths.Codes.REPR_CITY, ChiefTextClass.T1); //REPR-CITY
				}
			}
			return nadIndex;
		}

		int HeaderGroup6Party_PG(SegmentGroup6 grp6, int nadIndex)
		{
			if (gbImportHeader != null && !gbImportHeader.IsICR && !gbImportHeader.IsSFD && !gbImportHeader.GovernmentContractorTurn.IsEmpty)
			{// PG
				nadIndex++;
				NADSegment nadPg = grp6.NAD[nadIndex];
				nadPg.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString("PG");
				nadPg.PartyIdentificationDetails.PartyIdentifier = gbImportHeader.GovernmentContractorTurn;
				nadPg.PartyIdentificationDetails.CodeListResponsibleAgencyCode = hmrc109;
			}
			return nadIndex;
		}

		int HeaderGroup6Party_LC(SegmentGroup6 grp6, int nadIndex)
		{
			if (gbImportHeader != null && !gbImportHeader.IsICR && !gbImportHeader.IsSFD && !gbImportHeader.RegisteredConsigneeTurn.IsEmpty)
			{// LC
				nadIndex++;
				NADSegment nadLc = grp6.NAD[nadIndex];
				nadLc.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString("LC");
				nadLc.PartyIdentificationDetails.PartyIdentifier = gbImportHeader.RegisteredConsigneeTurn.GetCusDecValue(ChiefDataElementsLengths.Codes.RCNSGE_TURN);  //RCNSEE-TURN
				nadLc.PartyIdentificationDetails.CodeListResponsibleAgencyCode = hmrc109;
			}
			return nadIndex;
		}

		int HeaderGroup6Party_Declarant(SegmentGroup6 grp6, int nadIndex)
		{
			// Declarant
			if (this.cusEntryHeader.Declaration.RepresentationTypeNo != "1")
			{
				if ((gbChiefHeader.DeclarantOrRepresentative?.EoriCode ?? ZString.Empty) != ZString.Empty)
				{
					nadIndex++;
					NADSegment nadDt = grp6.NAD[nadIndex];
					nadDt.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.DT_Declarant);
					nadDt.PartyIdentificationDetails.PartyIdentifier = gbChiefHeader.DeclarantOrRepresentative.EoriCode; //DECLNT-TID
					if (!IsThirdCountryTcuinSoDoNotSendAddress(nadDt.PartyIdentificationDetails.PartyIdentifier))
					{
						nadDt.PartyName.PartyName1 = gbChiefHeader.DeclarantOrRepresentative.Name.GetCusDecValue(ChiefDataElementsLengths.Codes.DECLT_NAME, ChiefTextClass.T1);//DECLNT-NAME
						nadDt.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = gbChiefHeader.DeclarantOrRepresentative.Street.GetCusDecValue(ChiefDataElementsLengths.Codes.DECLT_STREET, ChiefTextClass.T1);  //DECLNT-STREET
						nadDt.CityName = gbChiefHeader.DeclarantOrRepresentative.City.GetCusDecValue(ChiefDataElementsLengths.Codes.DECLT_CITY, ChiefTextClass.T1); //DECLNT-CITY
						if (gbExportHeader != null)
						{
							nadDt.CountrySubEntityDetails.CountrySubEntityNameCode = "EN";  // DECLNT-LNG
						}
						nadDt.PostalIdentificationCode = gbChiefHeader.DeclarantOrRepresentative.PostCode.GetCusDecValue(ChiefDataElementsLengths.Codes.DECLT_POSTCODE, ChiefTextClass.T6);  //DECLNT-POSTCODE
						nadDt.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(gbChiefHeader.DeclarantOrRepresentative.CountryCode); //DECLNT-CTRY
					}
				}
				else
				{
					errorCollector.AddError("Declarant is required for this representation type.", new ErrorInfo("14", "mandatory"));
				}
			}
			return nadIndex;
		}

		int HeaderGroup6Party_Consignee(SegmentGroup6 grp6, int nadIndex)
		{
			// consignee
			if (gbChiefHeader.ConsigneeImporter.IsNotMissing)
			{
				this.headerConsigneeHasBeenAdded = true;
				nadIndex++;
				NADSegment nadCn = grp6.NAD[nadIndex];
				nadCn.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.CN_Consignee);

				nadCn.PartyIdentificationDetails.PartyIdentifier = gbChiefHeader.ConsigneeImporter.EoriCode; //CNSGE-TID
				if (!IsThirdCountryTcuinSoDoNotSendAddress(nadCn.PartyIdentificationDetails.PartyIdentifier))
				{
					nadCn.PartyName.PartyName1 = gbChiefHeader.ConsigneeName.GetCusDecValue(ChiefDataElementsLengths.Codes.CNSGE_NAME, ChiefTextClass.T1); //CNSGE-NAME
					nadCn.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = gbChiefHeader.ConsigneeAddress1.GetCusDecValue(ChiefDataElementsLengths.Codes.CNSGE_STREET, ChiefTextClass.T1); //CNSGE-STREET"
					nadCn.CityName = gbChiefHeader.ConsigneeImporter.City.GetCusDecValue(ChiefDataElementsLengths.Codes.CNSGE_CITY, ChiefTextClass.T1); //CNSGE-CITY
					if (gbExportHeader != null)
					{
						nadCn.CountrySubEntityDetails.CountrySubEntityNameCode = "EN";  // CNSGR-LNG
					}
					nadCn.PostalIdentificationCode = GetPostCode(gbChiefHeader.ConsigneeImporter, ChiefDataElementsLengths.Codes.CNSGE_POSTCODE); //CNSGE-POSTCODE
					nadCn.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(gbChiefHeader.ConsigneeImporter.CountryCode); //CNSGE-CTRY
				}
			}
			return nadIndex;
		}

		bool IsThirdCountryTcuinSoDoNotSendAddress(ZString tcuinOrEori)
		{
			if (tcuinOrEori.IsEmpty)
			{
				return false;
			}
			var countryCode = tcuinOrEori.Left(2);
			var country = RefCountry.LoadFromCountryCode(cusEntryHeader.Factory, countryCode);
			return country == null || (country.IsRecognisedByEuropeanUnionAsIssuerOfThirdCountryUniqueIdentificationNumbersOrAEO && tcuinOrEori.Length == 17);
		}

		int HeaderGroup6Party_Shipper(SegmentGroup6 grp6, int nadIndex)
		{
			// shipper
			if (gbChiefHeader.ConsignorExporter.IsNotMissing)
			{
				this.headerShipperHasAlreadyBeenAdded = true;
				nadIndex++;
				NADSegment nadCz = grp6.NAD[nadIndex];
				nadCz.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.CZ_Consignor);
				nadCz.PartyIdentificationDetails.PartyIdentifier = gbChiefHeader.ConsignorExporter.EoriCode; //CNSGR-TID
				if (!IsThirdCountryTcuinSoDoNotSendAddress(nadCz.PartyIdentificationDetails.PartyIdentifier) && !(gbExportHeader != null && gbExportHeader.IsEXS))
				{
					nadCz.PartyName.PartyName1 = gbChiefHeader.ShipperName.GetCusDecValue(ChiefDataElementsLengths.Codes.CNSGR_NAME, ChiefTextClass.T1); //CNSGR-NAME
					nadCz.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = gbChiefHeader.ShipperAddress1.GetCusDecValue(ChiefDataElementsLengths.Codes.CNSGR_STREET, ChiefTextClass.T1); //CNSGR-STREET
					nadCz.CityName = gbChiefHeader.ShipperCity.GetCusDecValue(ChiefDataElementsLengths.Codes.CNSGR_CITY, ChiefTextClass.T1); //CNSGR-CITY
					if (gbExportHeader != null)
					{
						nadCz.CountrySubEntityDetails.CountrySubEntityNameCode = "EN";  // CNSGR-LNG
					}
					nadCz.PostalIdentificationCode = GetPostCode(gbChiefHeader.ConsignorExporter, ChiefDataElementsLengths.Codes.CNSGR_POSTCODE); //CNSGR-POSTCODE
					nadCz.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(gbChiefHeader.ShipperCountryCode); //CNSGR-CTRY
				}
			}
			return nadIndex;
		}

		void HeaderGroup4TDT()
		{
			if (gbExportHeader != null)
			{
				HeaderGroup4TDTExport();
			}
			else if (gbImportHeader != null)
			{
				HeaderGroup4TDTImport();
			}
		}

		void HeaderGroup4TDTExport()
		{
			if (gbExportHeader.IsEXS || gbExportHeader.IsECR)
			{
				return;
			}
			else if (gbExportHeader.IsEFD)
			{
				SegmentGroup4 grp4 = cusDec.Group4[0];
				TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				TDTSegment tdt1 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt1.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("1");
				if (!gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_CNTRY); //TRPT-CNTRY
				}

				if (!gbChiefHeader.TransportIdentityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansIdentificationName = gbChiefHeader.TransportIdentityAtTheBorderBox21.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_ID, ChiefTextClass.T1); //"TRPT-ID"
				}

				if (!gbChiefHeader.TransportIdentityOnDepartureBox18.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_ID_INLD).IsEmpty)
				{
					tdt1.TransportIdentification.TransportMeansIdentificationName = gbChiefHeader.TransportIdentityOnDepartureBox18.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_ID_INLD, ChiefTextClass.T1); //TRPT-ID-INLD
				}

				tdt1.TransportMeansOwnershipIndicatorCode = TransportMeansOwnershipIndicatorCodeList.GetFromString("EN");   // TRPT-ID-INLD-LNG
				if (!gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty)
				{
					tdt13.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeAtTheBorderBox25.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_MODE_CODE);  //TRPT-MODE-CODE
				}

				if (!gbChiefHeader.TransportModeInlandBox26.IsEmpty)
				{
					tdt1.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeInlandBox26.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_MODE_INLD); //TRPT-MODE-INLD
				}
			}
			else if (gbExportHeader.IsESD)
			{
				if (gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty)
				{
					errorCollector.AddError("Transport mode is required for ESD export.", new ErrorInfo("25", "mandatory"));
				}
				else
				{
					SegmentGroup4 grp4 = cusDec.Group4[0];
					TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
					tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
					TDTSegment tdt1 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
					tdt1.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("1");
					if (!gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty)
					{
						tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21; //TRPT-CNTRY
					}

					if (!gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty)
					{
						tdt13.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeAtTheBorderBox25;  //TRPT-MODE-CODE
					}

					if (!gbChiefHeader.TransportModeInlandBox26.IsEmpty)
					{
						tdt1.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeInlandBox26; //TRPT-MODE-INLD
					}
				}
			}
			else if (gbExportHeader.IsESP || gbExportHeader.IsELP)
			{
				SegmentGroup4 grp4 = cusDec.Group4[0];
				TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				if (!gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21; //TRPT-CNTRY
				}

				if (!gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty)
				{
					tdt13.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeAtTheBorderBox25;  //TRPT-MODE-CODE
				}
			}
		}

		void HeaderGroup4TDTImport()
		{
			if (gbImportHeader.IsICR)
			{
				SegmentGroup4 grp4 = cusDec.Group4[0];
				TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				if (!gbChiefHeader.TransportIdentityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansIdentificationName = gbChiefHeader.TransportIdentityAtTheBorderBox21.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_ID, ChiefTextClass.T1); //"TRPT-ID"         ; //"TRPT-ID"
				}
			}
			else if (gbImportHeader.IsSFD)
			{
				SegmentGroup4 grp4 = cusDec.Group4[0];
				TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				TDTSegment tdt1 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt1.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("1");
				if (!gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21; //TRPT-CNTRY
				}

				if (!gbChiefHeader.TransportIdentityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansIdentificationName = gbChiefHeader.TransportIdentityAtTheBorderBox21.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_ID, ChiefTextClass.T1); //"TRPT-ID"
				}

				if (!gbChiefHeader.TransportModeInlandBox26.IsEmpty)
				{
					tdt1.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeInlandBox26; //TRPT-MODE-INLD
				}
			}
			else if (gbImportHeader.IsIFD)
			{
				SegmentGroup4 grp4 = cusDec.Group4[0];
				TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				TDTSegment tdt1 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt1.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("1");
				if (!gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21; //TRPT-CNTRY
				}

				if (!gbChiefHeader.TransportIdentityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansIdentificationName = gbChiefHeader.TransportIdentityAtTheBorderBox21.GetCusDecValue(ChiefDataElementsLengths.Codes.TRPT_ID, ChiefTextClass.T1); //"TRPT-ID"
				}

				if (!gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty)
				{
					tdt13.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeAtTheBorderBox25;  //TRPT-MODE-CODE
				}

				if (!gbChiefHeader.TransportModeInlandBox26.IsEmpty)
				{
					tdt1.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeInlandBox26; //TRPT-MODE-INLD
				}
			}
			else if (gbImportHeader.IsIFW || gbImportHeader.IsISW)
			{
				bool needsTrptModeCode = (!gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty && !gbImportHeader.OSAirTransportAmount.IsEmpty);
				bool needsTrptCountry = !gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty;
				if (needsTrptModeCode || needsTrptCountry)
				{
					SegmentGroup4 grp4 = cusDec.Group4[0];
					TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
					tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
					if (needsTrptCountry)
					{
						tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21; //TRPT-CNTRY
					}

					if (needsTrptModeCode)
					{
						tdt13.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeAtTheBorderBox25;  //TRPT-MODE-CODE
					}
				}
			}
			else if (gbImportHeader.IsISD)
			{
				SegmentGroup4 grp4 = cusDec.Group4[0];
				TDTSegment tdt13 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt13.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("13");
				TDTSegment tdt1 = grp4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdt1.TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("1");
				if (!gbChiefHeader.TransportNationalityAtTheBorderBox21.IsEmpty)
				{
					tdt13.TransportIdentification.TransportMeansNationalityCode = gbChiefHeader.TransportNationalityAtTheBorderBox21; //TRPT-CNTRY
				}

				if (!gbChiefHeader.TransportModeAtTheBorderBox25.IsEmpty)
				{
					tdt13.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeAtTheBorderBox25;  //TRPT-MODE-CODE
				}

				if (!gbChiefHeader.TransportModeInlandBox26.IsEmpty)
				{
					tdt1.ModeOfTransport.TransportModeNameCode = gbChiefHeader.TransportModeInlandBox26; //TRPT-MODE-INLD
				}
			}
		}

		void HeaderGroup5DOCandDTM()
		{
			if (gbExportHeader != null && gbExportHeader.IsEXS)
			{
				return;
			}
			// Group 5
			int supportingDocumentsCount = 0;
			const int maxSupportingDocuments = 40;
			SegmentGroup5 grp5 = cusDec.Group5[0];
			foreach (ISupportingDocument document in gbChiefHeader.SupportingDocuments)
			{   // header level Supporting docs
				supportingDocumentsCount++;
				DOCSegment doc = grp5.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString("916");
				doc.DocumentMessageName.DocumentName = document.Code; //HDR-DOC-CODE
				doc.DocumentMessageDetails.DocumentIdentifier = ChiefTextClass.T6(document.Reference); //HDR-DOC-REF
				doc.DocumentMessageDetails.DocumentStatusCode = DocumentStatusCodeList.GetFromString(document.Status); //HDR-DOC-STATUS
				doc.DocumentMessageDetails.DocumentSourceDescription = ChiefTextClass.T1(document.Reason);//HDR-DOC-REASON
				doc.DocumentMessageDetails.VersionIdentifier = document.Part;//HDR-DOC-PART

				if (document.Quantity > 0)
				{
					DTMSegment dtm = grp5.DTM.InstantiateAChildAndAddItToChildrenCollection();
					dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("ZZZ");
					dtm.DateTimePeriod.DateOrTimeOrPeriodText = document.Quantity.ToStringTrimZeros("#.000");//HDR-DOC-QTY
				}
				if (supportingDocumentsCount == maxSupportingDocuments)
				{
					break;
				}
			}
		}

		void HeaderGroup1RFF()
		{
			// References - RFF
			HeaderGroup1RffABO();
			HeaderGroup1RffUCN();

			var hasAtLeastOneDeferredPayment = false;
			foreach (GbLine line in gbChiefHeader.Lines)
			{
				if (line.actualEntryLine.Taxes.Cast<TaxStruct>().Any(x => x.G4_MethodOfPayment == "F" || x.G4_MethodOfPayment == "Q"))  // F = deferred, Q = MCP Security deferment
				{
					hasAtLeastOneDeferredPayment = true;
					break;
				}
			}

			if (hasAtLeastOneDeferredPayment)
			{
				if (!gbChiefHeader.FIR_DAN.IsEmpty && !gbChiefHeader.FIR_DAN_PFX.IsEmpty)
				{
					RFFSegment rffAbi = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
					rffAbi.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.ABI);
					rffAbi.Reference.ReferenceIdentifier = gbChiefHeader.FIR_DAN.GetCusDecValue(ChiefDataElementsLengths.Codes.FIR_DAN);  //FIR-DAN
					rffAbi.Reference.DocumentLineIdentifier = gbChiefHeader.FIR_DAN_PFX.GetCusDecValue(ChiefDataElementsLengths.Codes.FIR_DAN_PFX);  //FIR-DAN-PFX
				}
				if (gbImportHeader != null && !gbImportHeader.SCND_DAN.IsEmpty)
				{
					//SCND-DAN
					RFFSegment rffDa = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
					rffDa.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.DA);
					rffDa.Reference.ReferenceIdentifier = gbImportHeader.SCND_DAN.GetCusDecValue(ChiefDataElementsLengths.Codes.SCND_DAN);
					rffDa.Reference.DocumentLineIdentifier = gbImportHeader.SCND_DAN_PFX.GetCusDecValue(ChiefDataElementsLengths.Codes.SCND_DAN_PFX);
				}
			}
		}

		RFFSegment HeaderGroup1RffUCN()
		{
			return HeaderGroup1RffUCN(gbChiefHeader.MasterUniqueConsignmentReference);
		}

		protected RFFSegment HeaderGroup1RffUCN(ZString masterUcr)
		{
			RFFSegment rffUcn = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
			rffUcn.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.UCN);
			rffUcn.Reference.ReferenceIdentifier = masterUcr.GetCusDecValue(ChiefDataElementsLengths.Codes.MASTER_UCR);  //MASTER-UCR
			return rffUcn;
		}

		void HeaderGroup1RffABO()
		{
			// Just creates the ABO type RFF, which is used by NEW and DELETE cusdecs
			RFFSegment rffAbo = cusDec.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
			rffAbo.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(ChiefConstants.RffSegmentIdentifiers.ABO);
			rffAbo.Reference.ReferenceIdentifier = gbChiefHeader.DeclarationUniqueConsignmentReference; //DECLN-UCR

			if (!gbChiefHeader.DeclarationUniqueConsignmentReferencePartSuffix.IsEmpty && !(queryMessageFunction is Interrogate_DecDucr))
			{
				rffAbo.Reference.DocumentLineIdentifier = gbChiefHeader.DeclarationUniqueConsignmentReferencePartSuffix; //DECLN-PART-NO
			}
		}

		void HeaderGroup0SealsExportOnly()
		{
			// SEL (seal)
			int sealIndex = 0;
			foreach (ISeal seal in gbExportHeader.Seals)
			{
				if (!seal.SealId.IsEmpty)
				{
					cusDec.SEL[sealIndex].TransportUnitSealIdentifier = seal.SealId.GetCusDecValue(ChiefDataElementsLengths.Codes.SEAL_ID, ChiefTextClass.T1);  // SEAL-ID
					cusDec.SEL[sealIndex].SealConditionCode = SealConditionCodeList.GetFromString("EN");  //SEAL-ID-LNG - Language in which SEAL-ID is written.
					sealIndex++;
				}
			}
		}

		void HeaderGroup0LocationsCommon(HashSet<ZString> locationHash)
		{
			if (gbChiefHeader.IsFSD)
			{
				return; // no locations for FSD, thank you
			}

			// GDS-LOCN-CODE is not as simple as loc/14, it also needs element 3055
			var box30Required = gbChiefHeader.LocationOfGoodsRequirementLevel;
			if (gbChiefHeader.LocationOfGoods.IsEmpty && box30Required == RequirementLevel.Mandatory)
			{
				errorCollector.AddError("Goods location code", new ErrorInfo("30", "Mandatory"));
			}
			else if (!gbChiefHeader.LocationOfGoods.IsEmpty && (box30Required == RequirementLevel.Optional || box30Required == RequirementLevel.Mandatory))
			{
				LOCSegment newExpLoc = AddLocation(cusDec.LOC, "14");
				newExpLoc.LocationIdentification.LocationNameCode = gbChiefHeader.LocationOfGoods + gbChiefHeader.ShedPhysicalCodeFromDatabaseOrHeathrowERT; //GDS-LOCN-CODE (box 30 demands physical code, other places use virtual code)
				newExpLoc.LocationIdentification.CodeListResponsibleAgencyCode = hmrc109;
			}

			if (!gbChiefHeader.CountryOfExport.IsEmpty && !gbChiefHeader.IsECR && !gbChiefHeader.IsICR)
			{
				AddLocation(cusDec.LOC, "35").LocationIdentification.LocationNameCode = gbChiefHeader.CountryOfExport; //DISP-CNTRY
				locationHash.Add(gbChiefHeader.CountryOfExport);
			}
		}

		void HeaderGroup0LocationsExport(HashSet<ZString> locationHash)
		{
			if (!gbExportHeader.OfficeOfExit.IsEmpty && !gbExportHeader.IsECR && !gbExportHeader.IsEXS)
			{
				AddLocation(cusDec.LOC, "115").LocationIdentification.LocationNameCode = gbExportHeader.OfficeOfExit.GetCusDecValue(ChiefDataElementsLengths.Codes.EXIT_OFFICE); //"EXIT-OFFICE
			}

			if (!gbChiefHeader.CountryOfDestination.IsEmpty && !gbExportHeader.IsECR) //DEST-CNTRY
			{
				AddLocation(cusDec.LOC, "36").LocationIdentification.LocationNameCode = gbExportHeader.CountryOfDestination;
				locationHash.Add(gbChiefHeader.CountryOfDestination);
			}

			if (!gbExportHeader.IsESD && !gbExportHeader.IsEXS)
			{
				foreach (var routeCountryConverted in gbExportHeader.CountriesOfRouting)
				{
					if (!locationHash.Contains(routeCountryConverted))
					{
						AddLocation(cusDec.LOC, "49").LocationIdentification.LocationNameCode = routeCountryConverted; //ROUTE-CNTRY
						locationHash.Add(routeCountryConverted);
					}
				}
			}
		}

		void HeaderGroup0LocationsImport()
		{
			if (gbImportHeader != null && gbImportHeader.IsFSD)
			{
				return; // FSD - no locations
			}

			if (!gbImportHeader.OSAirTransportLoad.IsEmpty && !gbChiefHeader.IsICR)
			{
				var farp = AddLocation(cusDec.LOC, "76");
				farp.LocationIdentification.LocationNameCode = gbImportHeader.OSAirTransportLoad; //FARP-CODE
				farp.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.GetFromString("3");
			}

			if (!gbImportHeader.FirstEuArrival.Place.IsEmpty)
			{
				AddLocation(cusDec.LOC, "60").LocationIdentification.LocationNameCode = gbImportHeader.FirstEuArrival.Place;  // EU-ARR-LOCN-CODE
			}
		}

		void HeaderGroup0DtmImport()
		{
			// // INTD-ARR-DTM = Intended arrival date-time at EU-ARR-LOCN-CODE on a pre-arrival notification.
			/*
			 NB - INTD-ARR-DTM and EU-ARR-LOCN-CODE: Details only to be declared if a combined pre-arrival notification and Customs declaration is being submitted.  Until the rules for pre-arrival notifications are specified and pre-arrival notifications supported by CHIEF, the data element is optional.
			 */
			HeaderGroup0DtmCommon();

			if (!gbImportHeader.FirstEuArrival.Place.IsEmpty)
			{
				int dtmIndex = cusDec.DTM.Count - 1;
				dtmIndex++;
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("132");
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodText = gbImportHeader.FirstEuArrival.DateTime.ToString("yyyyMMddHHmm"); // 203 = yyyyMMddHHmm //INTD-ARR-DTM
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("203");   // 203 = yyyyMMddHHmm
			}
		}

		void HeaderGroup0DtmExport()
		{
			HeaderGroup0DtmCommon();

			// DTM segment
			int dtmIndex = cusDec.DTM.Count - 1;
			if (!gbExportHeader.GoodsDepartureDate.IsEmpty && gbExportHeader.IsESD)
			{
				dtmIndex++;
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("189");
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodText = gbExportHeader.GoodsDepartureDate.ToString("yyyyMMdd");// 102= yyyyMMdd  //GDS-DEP-DT
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("102");  // 102= yyyyMMdd
			}

			if (!gbExportHeader.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises.IsEmpty && (gbExportHeader.IsELP || gbExportHeader.IsEFD))
			{
				dtmIndex++;
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("44");
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodText = gbExportHeader.DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises.ToString("yyyyMMddHHmm");  //GDS-ARR-DTM-INLD
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("203"); // 203 = yyyyMMddHHmm
			}

			if (!gbExportHeader.DateAndTimeTheGoodsWillBeLeavingLCPPremises.IsEmpty && (gbExportHeader.IsELP || gbExportHeader.IsEFD))
			{
				dtmIndex++;
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("136");
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodText = gbExportHeader.DateAndTimeTheGoodsWillBeLeavingLCPPremises.ToString("yyyyMMddHHmm");// 203 = yyyyMMddHHmm  //GDS-DEP-DTM-INLD
				cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("203");// 203 = yyyyMMddHHmm
			}
		}

		void HeaderGroup0DtmCommon()
		{
			// ACPTNC-DTM
			if (NeedAcceptanceDateTime)
			{
				if (!gbChiefHeader.TaxPointDateAndTime.IsEmpty)
				{
					int dtmIndex = cusDec.DTM.Count - 1;
					dtmIndex++;
					cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("148");
					cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodText = gbChiefHeader.TaxPointDateAndTime.ToString("yyyyMMddHHmm");// 203 = yyyyMMddHHmm  //ACPTNC-DTM
					cusDec.DTM[dtmIndex].DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("203");// 203 = yyyyMMddHHmm
				}
				else
				{
					errorCollector.AddError("ACPTNC-DTM (tax point)", new ErrorInfo("", "mandatory"));
				}
			}
		}

		bool NeedAcceptanceDateTime
		{
			get
			{
				return (gbImportHeader != null && (gbImportHeader.IsISD || gbImportHeader.IsISW))
						||
					   (gbExportHeader != null && (gbExportHeader.IsESD))
					;
			}
		}

		void HeaderGroup0CST(ZBool isInterrogate_Des)
		{
			//CST
			if (!gbChiefHeader.IsEXS)
			{
				CSTSegment cst = cusDec.CST[0];
				cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = isInterrogate_Des ? gbChiefHeader.IsSupplementaryDeclarationType ? (ZString)Enterprise.Customs.Business.YesNoList.Codes.Yes : ZString.Empty : gbChiefHeader.DeclarationType;

				if (this.messageTypeNewAmendedDeleted != CusDecMessageTypeFunction.Delete && queryMessageFunction == null)
				{
					cst.CustomsIdentityCodes2.CustomsGoodsIdentifier = gbChiefHeader.DECLT_REP;  //DECLT-REP - do not send for XTC
				}
			}
		}

		protected void HeaderGroup0BGM(string messageCode, string msgFunc)
		{
			// BGM segment
			BGMSegment bgm = cusDec.BGM[0];
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(messageCode); //MESSAGE-CODE e.g. IFD
			bgm.DocumentMessageName.CodeListResponsibleAgencyCode = hmrc109;
			if (gbChiefHeader != null)
			{
				bgm.DocumentMessageName.DocumentName = gbChiefHeader.DECLN_CRRN;   // DECLN-CRRN - will be overridden in imp or exp
			}
			//MSG-FUNCTION e.g. 9 (new/amended/deleted etc):
			bgm.MessageFunctionCode = MessageFunctionCodeList.GetFromString(msgFunc);
		}

		protected UNHSegment HeaderGroup0UNH()
		{
			// UNH
			UNHSegment unh = cusDec.UNH[0];
			unh.MessageReferenceNumber = SYS_MRN;  //SYS-MRN
			unh.MessageIdentifier.MessageType = "CUSDEC";
			unh.MessageIdentifier.MessageVersionNumber = "D";
			unh.MessageIdentifier.MessageReleaseNumber = "04A";
			unh.MessageIdentifier.ControllingAgency = "UN";
			return unh;
		}

		void AddLine(GbLine line)
		{
			GbChiefImportLine importLine = line as GbChiefImportLine;
			GbChiefExportLine exportLine = line as GbChiefExportLine;

			if (line.actualEntryLine.CL_CustomsPostedStatus == Enterprise.Customs.Business.EntryLineStatusList.Codes.Active)
			{
				SegmentGroup30 grp30 = LineGroup30CST(line, importLine, exportLine);
				LineGroup30FTXStatements(line, grp30);
				LineGroup30LOC(line, importLine, exportLine, grp30);
				LineGroup30MEA(line, grp30);
				LineGroup30Spoff(line, grp30);
				LineGroup30Shipper(line, grp30);
				LineGroup30Consignee(line, grp30);
				LineGroup31PACandPCI(line, grp30);
				LineGroup33MOA(line, importLine, grp30);
				LineGroup35QuotaAndDangerousGoodsAndContainers(line, grp30);
				LineGroup36DescriptionOfGoods(line, grp30, line.actualEntryLine.CL_LineNumber);
				LineGroup37PreviousDocsDOC(line, grp30);
				LineGroup37SupportingDocsDOC(line, grp30);
				if (exportLine != null)
				{
					LineGroup38TOD(exportLine, grp30);
				}
				LineGroup40GEICommonFec(line, grp30);
				if (importLine != null)
				{
					LineGroup40GEIimports(importLine, grp30);
				}
				if (exportLine != null)
				{
					LineGroup40GEIExportFec(exportLine, grp30);
				}
				LineGroup40TAXandMOA(line, grp30);
			}
			else
			{
				SegmentGroup30 grp30 = cusDec.Group30.InstantiateAChildAndAddItToChildrenCollection();
				grp30.CST[0].CustomsIdentityCodes2.CustomsGoodsIdentifier = "DEL";//BASE-CMDTY-CODE or TARIC-CMDTY-CODE
			}
		}

		#region Line-level Methods

		void LineGroup40GEIimports(GbChiefImportLine importLine, SegmentGroup30 grp30)
		{
			if (importLine.FECCountryOfOrigin)
			{   //ITEM-PROC-INST
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.Item_ORG_ImportsOnly, grp30.Group40.InstantiateAChildAndAddItToChildrenCollection().GEI);
			}

			// VAL-MTHD-CODE, VM, 2
			if (!importLine.ValuationMethod.IsEmpty && !gbImportHeader.IsICR)
			{
				var geiValmethod = grp30.Group40.InstantiateAChildAndAddItToChildrenCollection().GEI.InstantiateAChildAndAddItToChildrenCollection();
				geiValmethod.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("2");
				geiValmethod.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(importLine.ValuationMethod); //VAL-MTHD-CODE
				geiValmethod.ProcessingIndicator.CodeListIdentificationCode = "VM";
				geiValmethod.ProcessingIndicator.CodeListResponsibleAgencyCode = hmrc109;
			}

			// VAL-ADJT-CODE, VA; ITEM-VAL-ADJT, 9
			if (!importLine.ValueAdjustmentCode.IsEmpty && !gbImportHeader.IsICR)
			{
				var grp40ForValuationAdjustmentAndPercentage = grp30.Group40.InstantiateAChildAndAddItToChildrenCollection();
				var geiValAdjustment = grp40ForValuationAdjustmentAndPercentage.GEI.InstantiateAChildAndAddItToChildrenCollection();
				geiValAdjustment.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("2");
				if (!importLine.ValueAdjustmentCode.IsEmpty)
				{
					geiValAdjustment.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(importLine.ValueAdjustmentCode); //VAL-ADJT-CODE
					geiValAdjustment.ProcessingIndicator.CodeListIdentificationCode = "VA";
					geiValAdjustment.ProcessingIndicator.CodeListResponsibleAgencyCode = hmrc109;

					// Send ITEM-VAL-ADJT even if it is zero. Tariff:  "If there is no percentage adjustment, enter ‘0’".... but ONLY send it if the VAL-ADJT-CODE is given, otherwise we'd have a subordinate PCD without a parent GEI.
					if (!(gbImportHeader.IsFSD || gbImportHeader.IsICR || gbImportHeader.IsSFD))    // do not send for FSD, SFD or ICR.
					{
						var pcd = grp40ForValuationAdjustmentAndPercentage.PCD.InstantiateAChildAndAddItToChildrenCollection();
						pcd.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.GetFromString("9");
						pcd.PercentageDetails.Percentage = importLine.ValueAdjustmentAmount.ToStringTrimZeros("0.000"); //ITEM-VAL-ADJT
					}
				}
			}
		}

		void LineGroup40GEICommonFec(ILine line, SegmentGroup30 grp30)
		{
			GEISegmentMessageSection geiSection = grp30.Group40[0].GEI;
			if (line.FECNetMassInKilograms)
			{   //ITEM-PROC-INST
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.ItemLevel_QV1, geiSection);
			}

			if (line.FECSupplementaryUnits)
			{  //ITEM-PROC-INST
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.ItemLevel_QV2, geiSection);
			}
		}

		void LineGroup40GEIExportFec(GbChiefExportLine line, SegmentGroup30 grp30)
		{
			GEISegmentMessageSection geiSection = grp30.Group40[0].GEI;
			if (line.FECCountryOfDestination)
			{   //ITEM-PROC-INST DST - item level
				AddFec(ProcInstTypesIncludingFecChallengeTypes.Codes.CommonLevel_DST_ExportsOnly, geiSection);
			}
		}

		void LineGroup40TAXandMOA(ILine line, SegmentGroup30 grp30)
		{
			// Group 41 - TAX and MOA
			int taxIndex = -1;
			foreach (ITax taxLine in line.Taxes)
			{
				SegmentGroup41 grp41 = grp30.Group41.InstantiateAChildAndAddItToChildrenCollection();

				if (taxIndex == 10)
				{
					break;  // only 10 TAXes allowed
				}
				TAXSegment tax = grp41.TAX[0];
				tax.DutyOrTaxOrFeeFunctionCodeQualifier = DutyOrTaxOrFeeFunctionCodeQualifierList.GetFromString("1");
				tax.DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode = DutyOrTaxOrFeeTypeNameCodeList.GetFromString(taxLine.TaxType);  //TTY-CODE
				tax.DutyTaxFeeAccountDetail.DutyOrTaxOrFeeAccountCode = taxLine.MethodOfPayment.GetCusDecValue(ChiefDataElementsLengths.Codes.MOP_CODE); //MOP-CODE
				if (!taxLine.TaxBaseQuantity.IsEmpty)
				{
					tax.DutyOrTaxOrFeeAssessmentBasisQuantity = taxLine.TaxBaseQuantity.ToStringTrimZeros("#.000"); // ITLN-BASE-QTY
				}

				if ((!taxLine.TaxRate.IsEmpty || !taxLine.TaxOverrideCode.IsEmpty) && !gbChiefHeader.IsICR)
				{
					tax.DutyTaxFeeDetail.DutyOrTaxOrFeeRateCode = taxLine.TaxRate; // TAX-RATE-ID
					tax.DutyTaxFeeDetail.DutyOrTaxOrFeeRateBasisCode = DutyOrTaxOrFeeRateBasisCodeList.GetFromString(taxLine.TaxOverrideCode); // TTY-OVR-CODE
					tax.DutyTaxFeeDetail.CodeListResponsibleAgencyCode2 = hmrc109;
				}

				if (ShouldSendAmountOfTax(taxLine, line))
				{   // only send the amount if the type is 10, otherwise we see E484 Tax Amount & DTI Entry incompatible. Tax Amount & DTI Entry incompatible, Check whether override is being used in the rate column (box 47c) or box 45 indicates a manual calculation requirement. For DTI entries the Tax Amount is not required to be completed where Duty Type 10 (Automatic Calculation) is set against the tax type.
					MOASegment moa = grp41.MOA.InstantiateAChildAndAddItToChildrenCollection();
					moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString("161");
					moa.MonetaryAmount.MonetaryAmount = taxLine.TaxAmount; // ITLN-DECL-TAX-DC
				}
				if (ShouldSendBaseAmountOfTax(taxLine, line))
				{
					MOASegment moa = grp41.MOA.InstantiateAChildAndAddItToChildrenCollection();
					moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString("56");
					moa.MonetaryAmount.MonetaryAmount = taxLine.TaxBaseAmount.ToStringTrimZeros("0.00"); // ITLN-BASE-TAX-DC
				}
				taxIndex++;
			}
		}

		static bool ShouldSendBaseAmountOfTax(ITax taxLine, ILine line)
		{
			/* As per tariff p 3-16, box 47b,
				CHIEF will normally calculate the tax base from other entered data.  The amount in the declaration currency should only be entered, therefore, in the following circumstances:
				– when code ‘M’ has been entered in Box 45 (adjustment), enter the value in accordance with the definitions of value in Volume 1 of the Tariff;
				– for cigarettes, when the code ‘VRP’ is being entered in the rate column, enter the total UK retail price of the cigarettes.
			*/

			if (!(line is GbChiefImportLine))
			{
				return false;
			}

			string fags = "2402";
			GbChiefImportLine importLine = (GbChiefImportLine)line;
			return
				importLine.ValueAdjustmentCode == EU.Business.ValuationAdjustmentCodeList.Codes.ManualNoAdjustmentCalculatedByChief
				||
				(taxLine.TaxRate == "VRP" && line.CommodityCode.StartsWith(fags));
		}

		static bool ShouldSendAmountOfTax(ITax taxLine, ILine line)
		{
			if (!(line is GbChiefImportLine))
			{
				return false;
			}

			// Tariff, vol 3, page 3-17. Amount of tax should only be sent when we're overriding Chief's calculations
			GbChiefImportLine importLine = (GbChiefImportLine)line;
			string taxTypeTenForAutomaticCalculation = "10";
			return
				(
					taxLine.TaxType == taxTypeTenForAutomaticCalculation  // as per error E00484
					||
					(
						!taxLine.TaxOverrideCode.IsEmpty    // As per tariff p 3-17, box 47c, "The following codes can be used to override CHIEF revenue calculations. The trader calculated amount is entered in Box 47d."
						&& taxLine.TaxOverrideCode != TaxRateVATOverrideListImport.Codes.ToClaimExemptionFromPaymentOfVat // if override is VAX is means no VAT is due - do not send zero by send blank
					)
					||
					importLine.ValueAdjustmentCode == EU.Business.ValuationAdjustmentCodeList.Codes.ManualNoAdjustmentCalculatedByChief // As per tariff p 3-17, box 47d, "When DTI is used CHIEF will calculate the amount due and so the box can be left blank unless Box 45 or the rate column indicates that the calculation is being one outside the system."
					||
					(
						taxLine.TaxOverrideCode.IsEmpty
						&& !taxLine.TaxAmount.IsEmpty
						&& importLine.GbHeader.IsICR  // C21 (ICR) will not use the override but if the amount is specified then send it
					)
				);
		}

		void LineGroup38TOD(IExportLine line, SegmentGroup30 grp30)
		{
			if (!gbExportHeader.IsESD && !line.TransportChargesMethodOfPayment.IsEmpty && gbChiefHeader.TransportChargesMethodOfPayment.IsEmpty)
			{
				TODSegmentMessageSection todSection = grp30.Group38[0].TOD;
				TODSegment tod = todSection[0];
				tod.TransportChargesPaymentMethodCode = TransportChargesPaymentMethodCodeList.GetFromString(line.TransportChargesMethodOfPayment); // I-TRPT-CHGE-MOP
			}
		}

		static void LineGroup37SupportingDocsDOC(ILine line, SegmentGroup30 grp30)
		{
			// Supporting Documents - Group 37
			int itemDocCount = 0;
			foreach (ISupportingDocument suppDoc in line.SupportingDocuments)
			{
				itemDocCount++;
				var grp37 = grp30.Group37.InstantiateAChildAndAddItToChildrenCollection();
				DOCSegment doc916 = grp37.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc916.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString("916");
				doc916.DocumentMessageName.DocumentName = suppDoc.Code; // ITEM-DOC-CODE
				doc916.DocumentMessageDetails.DocumentIdentifier = suppDoc.Reference.GetCusDecValue(ChiefDataElementsLengths.Codes.ITEM_DOC_REF, ChiefTextClass.T6); // ITEM-DOC-REF
				GbChiefExportLine exportLine = line as GbChiefExportLine;
				if (exportLine != null)
				{
					doc916.DocumentMessageDetails.LanguageNameCode = "EN";  //ITEM-DOC-LNG
				}
				doc916.DocumentMessageDetails.DocumentStatusCode = DocumentStatusCodeList.GetFromString(suppDoc.Status); //ITEM-DOC-STATUS
				doc916.DocumentMessageDetails.DocumentSourceDescription = suppDoc.Reason.GetCusDecValue(ChiefDataElementsLengths.Codes.ITEM_DOC_REASON, ChiefTextClass.T1); //ITEM-DOC-REASON
				doc916.DocumentMessageDetails.VersionIdentifier = suppDoc.Part; //ITEM-DOC-PART
				if (!suppDoc.Quantity.IsEmpty)
				{
					var dtmForQuantity = grp37.DTM.InstantiateAChildAndAddItToChildrenCollection();
					dtmForQuantity.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("ZZZ");
					dtmForQuantity.DateTimePeriod.DateOrTimeOrPeriodText = suppDoc.Quantity.ToStringTrimZeros("0.000");  // ITEM-DOC-QTY
				}
				if (itemDocCount == 99)
				{ break; } // 99 docs only
			}
		}

		void LineGroup37PreviousDocsDOC(ILine line, SegmentGroup30 grp30)
		{
			if (gbExportHeader != null && gbExportHeader.IsEXS)
			{
				return;
			}
			// Group 37 - previous docs
			if (!line.PreviousDocuments.Any())
			{
				if (gbImportHeader != null && !gbImportHeader.IsISD)
				{
					AddNewError("No Previous Documents", ChiefDataElementsLengths.Codes.PREV_DOC_REF, "40");
				}
			}
			SegmentGroup37 grp37 = grp30.Group37.InstantiateAChildAndAddItToChildrenCollection();
			int prevDocCount = 0;
			List<IPreviousDocument> allPreviousDocuments = new List<IPreviousDocument>(line.PreviousDocuments);
			foreach (IPreviousDocument prevDoc in allPreviousDocuments)
			{
				prevDocCount++;
				DOCSegment doc998 = grp37.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc998.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString("998");
				doc998.DocumentMessageName.DocumentName = prevDoc.Type; // PREV-DOC-TYPE
				doc998.DocumentMessageDetails.DocumentIdentifier = prevDoc.Reference.GetCusDecValue(ChiefDataElementsLengths.Codes.PREV_DOC_REF, ChiefTextClass.T1); // PREV-DOC-REF
				if (gbExportHeader != null)
				{
					doc998.DocumentMessageDetails.LanguageNameCode = "EN";  // PREV-DOC-LNG only for exports
				}
				doc998.DocumentMessageDetails.RevisionIdentifier = prevDoc.Class;  // PREV-DOC-CLASS
				if (prevDocCount == 9)
				{ break; } // nine docs only
			}
		}

		void LineGroup36DescriptionOfGoods(ILine line, SegmentGroup30 grp30, int entryLineNumber)
		{
			if (!line.DescriptionOfGoods.IsEmpty)
			{
				if (gbImportHeader != null || !gbExportHeader.IsESD)   // Imports and non-ESD exports need desc)
				{
					IMDSegment imd = grp30.Group35[0].Group36[0].IMD[0];
					RFFSegment rffZzz = grp30.Group35[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
					rffZzz.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString("ZZZ");
					var cleanGoodsDesc = ChiefTextClass.T1(line.DescriptionOfGoods);
					imd.ItemDescription.ItemDescription1 = cleanGoodsDesc.SubstringSafe(0, 255); // GDS-DESC
					if (cleanGoodsDesc.Length > 255)
					{
						int maxLengthOfDescOfGoods = 280; // defined by HMRC
						imd.ItemDescription.ItemDescription2 = cleanGoodsDesc.SubstringSafe(256, maxLengthOfDescOfGoods - 256); // GDS-DESC (2)
					}
					if (gbExportHeader != null)
					{
						imd.ItemDescription.LanguageNameCode = "EN"; // GDS-DESC-LNG
					}
				}
			}
			else
			{
				if (!(gbImportHeader != null && gbImportHeader.IsFSD))
				{
					AddNewError("Line-level description of goods for entry line #" + entryLineNumber.ToString(), ChiefDataElementsLengths.Codes.GDS_DESC, "31");
				}
			}
		}

		void LineGroup35QuotaAndDangerousGoodsAndContainers(ILine line, SegmentGroup30 grp30)
		{
			// Group 35 - RFF
			foreach (IContainer container in line.Containers)  // takes care of IsForInvoiceLIne for us
			{
				RFFSegment rffCont = grp30.Group35[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffCont.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString("AAQ");
				rffCont.Reference.ReferenceIdentifier = container.ContainerNumber.GetCusDecValue(ChiefDataElementsLengths.Codes.CNTR_NO, ChiefTextClass.T1); // CNTR-NO
			}

			GbChiefImportLine importLine = line as GbChiefImportLine;
			if (importLine != null && !importLine.QuotaOrderNumber.IsEmpty && !gbImportHeader.IsICR)
			{
				RFFSegment rffQuota = grp30.Group35[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffQuota.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString("ABJ");
				rffQuota.Reference.ReferenceIdentifier = importLine.QuotaOrderNumber.GetCusDecValue(ChiefDataElementsLengths.Codes.QTA_NO); // QTA-NO
			}

			if (!string.IsNullOrEmpty(line.UNDGCode))
			{
				RFFSegment rffUndg = grp30.Group35[0].RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffUndg.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString("UN");
				rffUndg.Reference.ReferenceIdentifier = line.UNDGCode.GetCusDecValue(ChiefDataElementsLengths.Codes.UNDG_CODE); // UNDG-CODE
			}
		}

		void LineGroup33MOA(ILine line, IImportLine importLine, SegmentGroup30 grp30)
		{
			SegmentGroup33 grp33 = null;
			bool shouldAddStatValue = !line.StatisticalValue.IsEmpty && !gbChiefHeader.IsICR;
			if (gbExportHeader != null && gbExportHeader.IsECR)
			{ shouldAddStatValue = false; }
			bool shouldAddImportItemPrice = (importLine != null && !importLine.ItemPrice.IsEmpty && !gbChiefHeader.IsICR);

			if (shouldAddImportItemPrice || shouldAddStatValue)
			{
				grp33 = grp30.Group33[0];
			}

			if (shouldAddStatValue)
			{
				MOASegment moa123 = grp33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				moa123.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString("123"); //ITEM-STAT-VAL-DC
				moa123.MonetaryAmount.MonetaryAmount = line.StatisticalValue.ToStringTrimZeros(2);
			}

			if (shouldAddImportItemPrice)
			{
				MOASegment moa38 = grp33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				moa38.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString("38"); //ITEM-PRC-AC
				moa38.MonetaryAmount.MonetaryAmount = importLine.ItemPrice.ToStringTrimZeros(2);
			}
		}

		void LineGroup30MEA(ILine line, SegmentGroup30 grp30)
		{
			var isIcr = gbImportHeader != null && gbImportHeader.IsICR;
			if (!line.GrossMassInKilograms.IsEmpty && !isIcr)
			{
				MEASegment mea = grp30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeCodeQualifier = MeasurementPurposeCodeQualifierList.GetFromString("AAH");
				mea.ValueRange.MeasurementUnitCode = "KGM";
				mea.ValueRange.Measure = line.GrossMassInKilograms.ToStringTrimZeros(3);  //ITEM-GROSS-MASS
			}

			if (!line.NetMassInKilograms.IsEmpty)
			{
				if (!(gbExportHeader != null && gbExportHeader.IsECR)
					&& !isIcr
					&& !(gbExportHeader != null && gbExportHeader.IsEXS))
				{
					MEASegment mea = grp30.MEA.InstantiateAChildAndAddItToChildrenCollection();
					mea.MeasurementPurposeCodeQualifier = MeasurementPurposeCodeQualifierList.GetFromString("AAR");
					mea.ValueRange.MeasurementUnitCode = "KGM";
					mea.ValueRange.Measure = line.NetMassInKilograms.ToStringTrimZeros(3); //ITEM-NET-MASS
				}
			}

			if (ShouldSendSupplementaryQuantity(line, isIcr))
			{
				MEASegment mea = grp30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeCodeQualifier = MeasurementPurposeCodeQualifierList.GetFromString("AAS");
				mea.ValueRange.MeasurementUnitCode = "ZZZ";
				mea.ValueRange.Measure = line.SupplementaryUnits.ToStringTrimZeros(3);  //ITEM-SUPP-UNITS
			}
			if (!line.ThirdQuantity.IsEmpty && !isIcr)
			{
				MEASegment mea = grp30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeCodeQualifier = MeasurementPurposeCodeQualifierList.GetFromString("AAT");
				mea.ValueRange.MeasurementUnitCode = "ZZZ";
				mea.ValueRange.Measure = line.ThirdQuantity.ToStringTrimZeros(3);  //ITEM-THRD-QTY
			}
		}

		static bool ShouldSendSupplementaryQuantity(ILine line, bool isIcr)
		{
			return !isIcr
				&& (
						!line.SupplementaryUnits.IsEmpty  // NB this means quantity - decimal
						||
						(
							!line.SupplementaryUnitsUQ.IsEmpty
							&&
							(
								line.IsProcedureThatAllowsZeroSupplementaryQty
								||
								line.SupplementaryUnitsUQ == QuantityUnitList.WeightAcesulfamePotatssium119
							)
						)
					);
		}

		void LineGroup30LOC(ILine line, IImportLine importLine, GbChiefExportLine exportLine, SegmentGroup30 grp30)
		{
			int locIndec = -1;
			bool needsItemOriginCountry = (importLine != null && !gbChiefHeader.IsICR) || (exportLine != null && (gbChiefHeader.IsESD || gbChiefHeader.IsEFD));
			if (!line.CountryOriginCode.IsEmpty && needsItemOriginCountry)
			{
				locIndec++;
				grp30.LOC[locIndec].LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("27");
				grp30.LOC[locIndec].LocationIdentification.LocationNameCode = line.CountryOriginCode;  //ITEM-ORIG-CNTRY
			}
			if (exportLine != null && !exportLine.CountryOfDestinationCode.IsEmpty && !gbChiefHeader.IsECR && !gbChiefHeader.IsEXS)
			{
				locIndec++;
				grp30.LOC[locIndec].LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString("36");
				grp30.LOC[locIndec].LocationIdentification.LocationNameCode = exportLine.CountryOfDestinationCode;  //ITEM-DEST-CNTRY
			}
		}

		void LineGroup30FTXStatements(ILine line, SegmentGroup30 grp30)
		{
			if (gbExportHeader != null && gbExportHeader.IsEXS)
			{
				return;
			}
			foreach (IStatement statement in line.Statements)
			{
				FTXSegment ftx = grp30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("ACB");
				ftx.TextReference.FreeTextDescriptionCode = statement.Statement.GetCusDecValue(ChiefDataElementsLengths.Codes.ITEM_AI_STMT);  //ITEM-AI-STMT
				ftx.TextLiteral.FreeText1 = statement.StatementText.GetCusDecValue(ChiefDataElementsLengths.Codes.ITEM_AI_STMT_TXT, ChiefTextClass.T1);  //ITEM-AI-STMT-TXT
				if (gbExportHeader != null)
				{
					ftx.LanguageNameCode = "EN";  // ITEM-AI-STMT-LNG - exports only, heaven only knows why
				}
			}
		}

		SegmentGroup30 LineGroup30CST(ILine line, IImportLine importLine, GbChiefExportLine exportLine)
		{
			// CST
			if (line.Procedure.IsEmpty)
			{
				AddNewError("Item-level procedure code", ChiefDataElementsLengths.Codes.CPC, "37");
			}
			SegmentGroup30 grp30 = cusDec.Group30.InstantiateAChildAndAddItToChildrenCollection();
			grp30.CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier = line.Procedure.GetCusDecValue(ChiefDataElementsLengths.Codes.CPC);  //CPC

			if (gbExportHeader != null && gbExportHeader.IsECR)
			{
				return grp30;
			}

			if (importLine != null && !gbImportHeader.IsICR)
			{
				grp30.CST[0].CustomsIdentityCodes2.CustomsGoodsIdentifier = line.CommodityCode.GetCusDecValue(ChiefDataElementsLengths.Codes.TARIC_CMDTY_CODE);  // TARIC-CMDTY-CODE
			}
			else if (exportLine != null)
			{
				grp30.CST[0].CustomsIdentityCodes2.CustomsGoodsIdentifier = line.CommodityCode.GetCusDecValue(ChiefDataElementsLengths.Codes.BASE_CMDTY_CODE); //BASE-CMDTY-CODE
			}
			var shouldAddEcSupplements = (gbImportHeader != null && !gbImportHeader.IsICR) || (gbExportHeader != null && !gbExportHeader.IsECR && !gbExportHeader.IsEXS);
			if (shouldAddEcSupplements)
			{
				grp30.CST[0].CustomsIdentityCodes3.CustomsGoodsIdentifier = line.SupplementaryCode1.GetCusDecValue(ChiefDataElementsLengths.Codes.EC_SUPPLEMENT);  //EC-SUPPLEMENT
				grp30.CST[0].CustomsIdentityCodes4.CustomsGoodsIdentifier = line.SupplementaryCode2.GetCusDecValue(ChiefDataElementsLengths.Codes.EC_SUPPLEMENT_2);  // EC-SUPPLEMENT-2
			}
			if (importLine != null && !gbImportHeader.IsICR)
			{
				grp30.CST[0].CustomsIdentityCodes5.CustomsGoodsIdentifier = importLine.PreferenceCode.GetCusDecValue(ChiefDataElementsLengths.Codes.PREFERENCE);  //PREFERENCE
			}
			return grp30;
		}

		void LineGroup30Consignee(ILine line, SegmentGroup30 grp30)
		{
			if (!this.headerConsigneeHasBeenAdded)
			{
				if (line.Consignee.IsNotMissing)
				{
					NADSegment nadLineConsignee = grp30.NAD.InstantiateAChildAndAddItToChildrenCollection();
					nadLineConsignee.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.CN_Consignee);
					nadLineConsignee.PartyIdentificationDetails.PartyIdentifier = line.Consignee.EoriCode;  //I-CNSGE-TID
					nadLineConsignee.PartyName.PartyName1 = line.Consignee.Name.GetCusDecValue(ChiefDataElementsLengths.Codes.I_CNSGE_NAME, ChiefTextClass.T1);   //I-CNSGE-NAME
					nadLineConsignee.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = line.Consignee.Street.GetCusDecValue(ChiefDataElementsLengths.Codes.I_CNSGE_STREET, ChiefTextClass.T1); //I-CNSGE-STREET
					nadLineConsignee.CityName = line.Consignee.City.GetCusDecValue(ChiefDataElementsLengths.Codes.I_CNSGE_CITY, ChiefTextClass.T1);  //I-CNSGE-CITY
					nadLineConsignee.PostalIdentificationCode = GetPostCode(line.Consignee, ChiefDataElementsLengths.Codes.I_CNSGE_POSTCODE);  //I-CNSGE-POSTCODE
					nadLineConsignee.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(line.Consignee.CountryCode);  //I-Consignee-CTRY
				}
			}
		}

		ZString GetPostCode(IOrganisation iOrganisation, string chiefElementName)
		{
			ZString postCode = iOrganisation.PostCode;
			return postCode.IsEmpty ? new ZString("NA") : postCode.GetCusDecValue(chiefElementName, ChiefTextClass.T6);  // Postcode is mandatory for chief
		}

		void LineGroup30Shipper(ILine line, SegmentGroup30 grp30)
		{
			if (!this.headerShipperHasAlreadyBeenAdded)
			{
				if (line.Shipper.IsNotMissing)
				{
					NADSegment nadLineShipper = grp30.NAD.InstantiateAChildAndAddItToChildrenCollection();
					nadLineShipper.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.CZ_Consignor);
					nadLineShipper.PartyIdentificationDetails.PartyIdentifier = line.Shipper.EoriCode;  //I-CNSGR-TID
					nadLineShipper.PartyName.PartyName1 = line.Shipper.Name.GetCusDecValue(ChiefDataElementsLengths.Codes.I_CNSGR_NAME, ChiefTextClass.T1);   //I-CNSGR-NAME
					nadLineShipper.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = line.Shipper.Street.GetCusDecValue(ChiefDataElementsLengths.Codes.I_CNSGR_STREET, ChiefTextClass.T1); //I-CNSGR-STREET
					nadLineShipper.CityName = line.Shipper.City.GetCusDecValue(ChiefDataElementsLengths.Codes.I_CNSGR_CITY, ChiefTextClass.T1);  //I-CNSGR-CITY
					nadLineShipper.PostalIdentificationCode = GetPostCode(line.Shipper, ChiefDataElementsLengths.Codes.I_CNSGR_POSTCODE);  //I-CNSGR-POSTCODE
					nadLineShipper.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(line.Shipper.CountryCode);  //I-CNSGR-CTRY
				}
			}
		}

		void LineGroup30Spoff(ILine line, SegmentGroup30 grp30)
		{
			if (line.SupervisingOffice.IsNotMissing && !gbChiefHeader.IsICR)
			{
				NADSegment nadLineSpoff = grp30.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nadLineSpoff.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(ChiefPartyTypes.Codes.CM_SupervisingOffice);
				nadLineSpoff.PartyIdentificationDetails.PartyIdentifier = line.SupervisingOffice.EoriCode;  //I-SPOFF-TID
				nadLineSpoff.PartyName.PartyName1 = line.SupervisingOffice.Name.GetCusDecValue(ChiefDataElementsLengths.Codes.I_SPOFF_NAME, ChiefTextClass.T1);   //I-SPOFF-NAME
				nadLineSpoff.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = line.SupervisingOffice.Street.GetCusDecValue(ChiefDataElementsLengths.Codes.I_SPOFF_STREET, ChiefTextClass.T1); //I-SPOFF-STREET
				nadLineSpoff.CityName = line.SupervisingOffice.City.GetCusDecValue(ChiefDataElementsLengths.Codes.I_SPOFF_CITY, ChiefTextClass.T1);  //I-SPOFF-CITY
				nadLineSpoff.PostalIdentificationCode = line.SupervisingOffice.PostCode.GetCusDecValue(ChiefDataElementsLengths.Codes.I_SPOFF_POSTCODE, ChiefTextClass.T6);  //I-SPOFF-POSTCODE
				nadLineSpoff.CountryNameCode = gbChiefHeader.ConvertFromUnToChiefCountry(line.SupervisingOffice.CountryCode);  //I-SPOFF-CTRY
			}
		}

		void LineGroup31PACandPCI(ILine line, SegmentGroup30 grp30)
		{
			// Group 31
			int packLinesCount = 0;
			foreach (IPackage iLinePackage in line.Packages)
			{
				SegmentGroup31 grp31 = grp30.Group31.InstantiateAChildAndAddItToChildrenCollection();
				PACSegment pac = grp31.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pac.PackageQuantity = IsBulk(iLinePackage.PackageKind) ? "" : iLinePackage.PackageCount.ToString(); // PKG-COUNT
				pac.PackageType.PackageTypeDescriptionCode = iLinePackage.PackageKind.IsEmpty ? new ZString("PK") : iLinePackage.PackageKind.GetCusDecValue(ChiefDataElementsLengths.Codes.PKG_KIND);    //PKG-KIND
																																																		 //group 32
				PCISegment pci = grp31.Group32[0].PCI[0];
				var fullMarks = iLinePackage.PackageMarks.GetCusDecValue(ChiefDataElementsLengths.Codes.PKG_MARKS, ChiefTextClass.T1); // PGK-MARKS, 42 chars
				pci.MarksLabels.ShippingMarksDescription1 = fullMarks.SubstringSafe(0, 35);
				if (fullMarks.Length > 35)
				{
					pci.MarksLabels.ShippingMarksDescription2 = fullMarks.SubstringSafe(35, 7); // PGK-MARKS
				}
				if (gbExportHeader != null)
				{
					pci.ContainerOrPackageContentsIndicatorCode = ContainerOrPackageContentsIndicatorCodeList.GetFromString("EN"); // PKG-MARKS-LNG
				}
				packLinesCount++;
			}

			if (packLinesCount == 0)
			{
				if (!(gbImportHeader != null && gbImportHeader.IsFSD))
				{
					AddNewError("No package lines are defined", ChiefDataElementsLengths.Codes.PKG_KIND, "31");
				}
			}
		}

		bool IsBulk(ZString packageType)
		{
			bool isBulk = false;

			switch (packageType)
			{
				case RefCusCodeBulkPackageUnitType.BulkGas:
				case RefCusCodeBulkPackageUnitType.BulkGrains:
				case RefCusCodeBulkPackageUnitType.BulkLiquid:
				case RefCusCodeBulkPackageUnitType.BulkLiquidGas:
				case RefCusCodeBulkPackageUnitType.BulkNodules:
				case RefCusCodeBulkPackageUnitType.BulkPowders:
				case RefCusCodeBulkPackageUnitType.BulkScrap:
					isBulk = true;
					break;
				default:
					isBulk = false;
					break;
			}

			return isBulk;
		}

		#endregion

		public static string SYS_MRN = "<<MSGNO PLACEHOLDER>>";

		void AddNewError(string decForUser, string chiefElementName, string boxNumber)
		{
			this.errorCollector.AddError(string.Format("{0} ({1})", decForUser, chiefElementName), new ErrorInfo(boxNumber, "mandatory"));
		}

		#region Implementation

		void AddFec(string fecType, GEISegmentMessageSection geiSection)
		{
			AddGEI(geiSection, "5", "PI", "109").ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(fecType);
		}

		GEISegment AddGEI(GEISegmentMessageSection geiSection, string processingInfoCodeQualifier, string processingInfoCodeListIdCode, string responsibleAgencyCode)
		{
			GEISegment geiObj = geiSection.InstantiateAChildAndAddItToChildrenCollection();
			geiObj.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString(processingInfoCodeQualifier);
			geiObj.ProcessingIndicator.CodeListIdentificationCode = processingInfoCodeListIdCode;
			geiObj.ProcessingIndicator.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.GetFromString(responsibleAgencyCode);
			return geiObj;
		}

		void AddAmount(MOASegmentMessageSection moaSection, ZDecimal monetaryAmount, string monetaryAmountTypeCodeQualifier, string currency)
		{
			AddAmount(moaSection, monetaryAmount, monetaryAmountTypeCodeQualifier, currency, false);
		}

		void AddAmount(MOASegmentMessageSection moaSection, ZDecimal monetaryAmount, string monetaryAmountTypeCodeQualifier, string currency, bool forceAdditionOfSegmentEvenIfMonetaryAmountIsMissing)
		{
			if (forceAdditionOfSegmentEvenIfMonetaryAmountIsMissing || !monetaryAmount.IsEmpty)
			{
				MOASegment moa = moaSection.InstantiateAChildAndAddItToChildrenCollection();
				if (!monetaryAmount.IsEmpty)
				{
					moa.MonetaryAmount.MonetaryAmount = monetaryAmount.ToString(2);
				}
				if (currency != null)
				{
					moa.MonetaryAmount.CurrencyIdentificationCode = currency;
				}
				moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString(monetaryAmountTypeCodeQualifier);
			}
		}

		LOCSegment AddLocation(LOCSegmentMessageSection locs, string locationType)
		{
			LOCSegment locationObj = locs.InstantiateAChildAndAddItToChildrenCollection();
			locationObj.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString(locationType);
			return locationObj;
		}

		readonly MessageProcedure messageProcedure;
		readonly CusDecMessageTypeFunction messageTypeNewAmendedDeleted;
		ErrorCollector errorCollector;
		#endregion

	}
}
