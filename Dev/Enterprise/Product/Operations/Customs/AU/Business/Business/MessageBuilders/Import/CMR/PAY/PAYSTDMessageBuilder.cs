using System;
using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.REMADV;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PAYSTDMessageBuilder : CMRMessageBuilder
	{
		public PAYSTDMessageBuilder(CusEntryHeader entryHeader, EFTPaymentInformationCollection eFTPayInfos)
		{
			MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			Messages = entryHeader.Messages;
			this.entryHeader = entryHeader;
			this.declaration = entryHeader.Declaration;
			paymentDetailRetriever = new PaymentDetailRetrieverIncludingAQIS(declaration, CMRMessageTypes.Payment, eFTPayInfos);
			eFTPaymentInformation = eFTPayInfos.GetElementByEntryHeader(entryHeader);
		}

		readonly EFTPaymentInformation eFTPaymentInformation;
		readonly PaymentDetailRetrieverIncludingAQIS paymentDetailRetriever;

		#region Generating Messages

		protected internal override void GenerateMessageText()
		{
			if (message == null)
			{
				message = new REMADVMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateDTM();
				PopulateRFFs();
				PopulateFII();
				PopulateGISs();
				SegmentGroup1 group1 = message.Group1.InstantiateAChildAndAddItToChildrenCollection();
				PopulateNADs(group1);
				PopulateUNS();
				PopulateMOAs();
				PopulateUNT();
			}
		}

		void PopulateDTM()
		{
			MessageUtilities.PopulateDTM(message.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.PaymentInstructionDateTime, ZDateTime.Today.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
		}

		void PopulateRFFs()
		{
			PopulateRFF(message.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber, entryHeader.EntryNumber);
			PopulateRFF(message.RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.FinancialTransactionReferenceNumber, PaymentPartyCode);
		}

		void PopulateRFF(RFFSegment rFF, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, string referenceIdentifier)
		{
			rFF.Reference.ReferenceFunctionCodeQualifier = referenceFunctionCodeQualifier;
			rFF.Reference.ReferenceIdentifier = referenceIdentifier;
		}

		void PopulateFII()
		{
			SetPaymentMethodOnDeclarationIfDefault();
			BankDetails bankDetails = paymentDetailRetriever.GetBankDetails();
			if (!bankDetails.AccountNumber.IsEmpty && !bankDetails.BSBNumber.IsEmpty)
			{
				FIISegment fII = message.FII.InstantiateAChildAndAddItToChildrenCollection();
				fII.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.NominatedBank;
				fII.AccountHolderIdentification.AccountHolderNumber = bankDetails.AccountNumber;
				fII.InstitutionIdentification.InstitutionBranchNumber = bankDetails.BSBNumber;
				fII.InstitutionIdentification.CodeListResponsibleAgencyCode2 = CodeListResponsibleAgencyCodeList.AuApcaAustralianPaymentsClearingAssociation;
			}
		}

		void SetPaymentMethodOnDeclarationIfDefault()
		{
			if (declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default && eFTPaymentInformation.CustomsChargeAmountPayableNow > 0)
			{
				declaration.JE_PaymentMethod = paymentDetailRetriever.PartyToPayString();
			}
		}

		void PopulateGISs()
		{
			if (Env.Registry.RequestOfficialCustomsPaymentReceipt)
			{
				PopulateGIS("POR", CodeListIdentificationCodeList.CustomsIndicator);
			}
			PopulateGIS("Y", CodeListIdentificationCodeList.FinancialRouting);
			PopulateGIS(declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Cash ? "N" : "Y", CodeListIdentificationCodeList.MethodsOfPayment);
		}

		void PopulateGIS(string processingIndicator, CodeListIdentificationCodeList codeIdentificationCode)
		{
			GISSegment gIS = message.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(processingIndicator);
			gIS.ProcessingIndicator_X.CodeListIdentificationCode = codeIdentificationCode;
			gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		void PopulateNADs(SegmentGroup1 group1)
		{
			if (declaration.Importer != null)
			{
				var importerABN = declaration.Importer.LocalBusinessRegNo;
				var importerCID = declaration.Importer.GetCustomsClientID();

				if (!importerABN.IsEmpty)
				{
					ABNCACSplitter splitter = new ABNCACSplitter(importerABN);
					PopulateNAD(group1.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.Importer, splitter.ABN);
				}
				else if (!importerCID.IsEmpty)
				{
					PopulateNAD(group1.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.Importer, importerCID.Left(11));
				}
			}
			if (Env.Registry.AUCustoms.LocalCustomsBranchIdentifier.Length > 0)
			{
				PopulateNAD(group1.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.Branch, Env.Registry.AUCustoms.LocalCustomsBranchIdentifier);//"AA33HF"
			}
		}

		void PopulateNAD(NADSegment nAD, PartyFunctionCodeQualifierList qualifier, string partyID)
		{
			nAD.PartyFunctionCodeQualifier = qualifier;
			nAD.PartyIdentificationDetails.PartyIdentifier = partyID;
			nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		void PopulateUNS()
		{
			MessageUtilities.PopulateUNS(message.UNS.InstantiateAChildAndAddItToChildrenCollection(), SectionIdentificationList.DetailSummarySectionSeparation);
		}

		void PopulateMOAs()
		{
			PopulateMOA(MonetaryAmountTypeCodeQualifierList.TotalAmount, eFTPaymentInformation.CustomsChargeAmountPayableNow);
			PopulateMOA(MonetaryAmountTypeCodeQualifierList.AdditionalAmountCoveredInspectionCosts, eFTPaymentInformation.AQISServicePaymentAmountPayableNow);
		}

		void PopulateMOA(MonetaryAmountTypeCodeQualifierList qualifier, ZDecimal monetaryAmount)
		{
			if (!monetaryAmount.IsEmpty)
			{
				MOASegment mOA = message.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
				mOA.MonetaryAmount.MonetaryAmountValue = monetaryAmount.ToString(2);
			}
		}

		#endregion

		#region Implementation

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		REMADVMessage message;

		protected internal override UNHSegment UNH => message.UNH[0];

		protected internal override MessageTypeList UNHMessageType => MessageTypeList.RemittanceAdviceMessage;

		protected internal override BGMSegment BGM => message.BGM[0];

		protected internal override UNTSegment UNT => message.UNT[0];

		protected internal override SegmentGroup EdifactMessage => message;

		protected internal override ZString DocumentName => "PAYSTD";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.RemittanceAdvice;

		protected internal override Type TypeOfMessage => typeof(CMRPAYSTDMessage);

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.PAYSTD;

		internal string PaymentPartyCode
		{
			get
			{
				JobDeclaration declaration = entryHeader.Declaration;
				string result = "";
				if (declaration.IsDrawback)
				{
					result = Constants.BankAccountOwnerType.DrawbackClaimant;
				}
				else
				{
					PaymentParty partyToPayEntry = paymentDetailRetriever.PartyToPayEntry;
					if (partyToPayEntry == PaymentParty.Broker || partyToPayEntry == PaymentParty.SecondBroker)
					{
						result = Constants.BankAccountOwnerType.Broker;
					}
					else if (partyToPayEntry == PaymentParty.Importer)
					{
						result = Constants.BankAccountOwnerType.Importer;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
