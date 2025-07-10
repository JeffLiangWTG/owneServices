using System;
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
	public class DRWBCKMessageBuilder : CMRCUSDECMessageBuilder
	{
		public DRWBCKMessageBuilder(JobDeclaration declaration)
		{
			Messages = declaration.Messages;
			this.declaration = declaration;
			paymentDetailRetriever = new PaymentDetailRetriever(this.declaration);
		}
		readonly JobDeclaration declaration;
		readonly PaymentDetailRetriever paymentDetailRetriever;

		protected internal override void GenerateMessageText()
		{
			if (cUSDEC == null)
			{
				cUSDEC = new CUSDECMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateGIS();
				PopulateFII();
				PopulateFTX();
				PopulateGroup1();
				PopulateGroup6();
				MessageUtilities.PopulateUNS(cUSDEC.UNS1[0], SectionIdentificationList.HeaderDetailSectionSeparation);
				PopulateGroup10();
				PopulateGroup30();
				MessageUtilities.PopulateUNS(cUSDEC.UNS2[0], SectionIdentificationList.DetailSummarySectionSeparation);
				PopulateUNT();
			}
		}

		protected void PopulateGIS()
		{
			GISSegment gIS = cUSDEC.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = AUCProcessingIndicatorDescriptionCodeList.PayeeDeclarationIndicator;
			gIS.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsIndicator;
			gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		void PopulateFII()
		{
			//TODO: "Drawback claiment" and "Other" bank account types,  Importer is invalid
			SetPaymentMethodOnDeclarationIfDefault();
			BankDetails bankDetails = paymentDetailRetriever.GetBankDetails();
			if (!bankDetails.AccountNumber.IsEmpty && !bankDetails.BSBNumber.IsEmpty)
			{
				FIISegment fII = cUSDEC.FII.InstantiateAChildAndAddItToChildrenCollection();
				fII.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.NominatedBank;
				fII.AccountHolderIdentification.AccountHolderNumber = bankDetails.AccountNumber;
				ZString accountName = bankDetails.AccountName;
				if (accountName.IsEmpty && paymentDetailRetriever.PartyToPayEntry == PaymentParty.Broker)
				{
					if (GlbBranch.CurrentBranch.OrgProxy != null)
					{
						accountName = GlbBranch.CurrentBranch.OrgProxy.OH_FullNameTruncated;
					}
					if (accountName.IsEmpty)
					{
						accountName = GlbCompany.CurrentCompany.GC_Name;
					}
				}
				if (!accountName.IsEmpty)
				{
					fII.AccountHolderIdentification.AccountHolderName1 = accountName.SubstringSafe(0, 35);
					fII.AccountHolderIdentification.AccountHolderName2 = accountName.SubstringSafe(35, 5);
				}
				fII.InstitutionIdentification.InstitutionBranchNumber = bankDetails.BSBNumber;
				fII.InstitutionIdentification.CodeListResponsibleAgencyCode2 = CodeListResponsibleAgencyCodeList.AuApcaAustralianPaymentsClearingAssociation;
			}
		}

		void SetPaymentMethodOnDeclarationIfDefault()
		{
			if (declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default)
			{
				declaration.JE_PaymentMethod = paymentDetailRetriever.PartyToPayString();
			}
		}

		protected virtual void PopulateFTX()
		{
			if (declaration.AutoAssignImporterRef || !declaration.JE_OwnerRef.IsEmpty)
			{
				PopulateFTX(TextSubjectCodeQualifierList.CustomerRemarks, EDIMessage.OwnerReferencePlaceHolder, ZString.Empty, ZString.Empty);
			}

			ZString amberStatement = declaration.JE_AmberStatement;

			if (!declaration.DrawbackHeaderAmberReasonCode.IsEmpty || !amberStatement.IsEmpty)
			{
				FTXSegment fTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.TariffStatements;
				if (!declaration.DrawbackHeaderAmberReasonCode.IsEmpty)
				{
					fTX.TextReference.FreeTextValueCode = declaration.DrawbackHeaderAmberReasonCode;
					fTX.TextReference.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
				}
				PopulateNoteFields(fTX, amberStatement.SubstringSafe(0, 2560));
			}
		}

		protected void PopulateFTX(TextSubjectCodeQualifierList textSubjectCodeQualifier, ZString freeTextValue1, ZString freeTextValue2, ZString freeTextValue3)
		{
			FTXSegment fTX = cUSDEC.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectCodeQualifier = textSubjectCodeQualifier;
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

		protected SegmentGroup1 group1;
		protected SegmentGroup1 Group1
		{
			get
			{
				if (group1 == null)
				{
					group1 = cUSDEC.Group1.InstantiateAChildAndAddItToChildrenCollection();
				}
				return group1;
			}
		}

		protected void PopulateGroup1()
		{
			if (Env.Registry.AUCustoms.AgentsReferenceDefaulting == Core.Constants.AgentsReferenceDefaulting.FAR
				|| Env.Registry.AUCustoms.AgentsReferenceDefaulting == Core.Constants.AgentsReferenceDefaulting.DEF
				|| !declaration.JE_AgentsReference.IsEmpty)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.BrokerReference1, EDIMessage.AgentReferencePlaceHolder);
			}
			PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ApplicableInstructionsOrStandards, declaration.DrawbackHeaderAssesmentMethod);
			PopulateBankAccountOwner();
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
			//TODO: "Drawback claiment" and "Other" bank account types , I is invalid??
			if (paymentDetailRetriever.PartyToPayEntry == PaymentParty.Importer && declaration.Importer != null)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.FinancialTransactionReferenceNumber, "I");
			}
			else
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.FinancialTransactionReferenceNumber, "B");
			}
		}

		protected SegmentGroup6 fGroup6;
		protected SegmentGroup6 Group6
		{
			get
			{
				if (fGroup6 == null)
				{
					fGroup6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
				}
				return fGroup6;
			}
		}

		protected virtual void PopulateGroup6()
		{
			PopulateNADForClaimant(declaration.Importer);
			PopulateNADForContact();
			PopulateCTACOM();
			PopulateNAD(Group6, PartyFunctionCodeQualifierList.Branch, Env.Registry.AUCustoms.LocalCustomsBranchIdentifier);
		}

		protected virtual void PopulateNADForClaimant(OrgHeader importer)
		{
			if (importer != null)
			{
				var importerABNFromOrganisation = importer.LocalBusinessRegNo.Trim();
				var importerCIDFromOrganisation = importer.GetCustomsClientID();
				if (!importerABNFromOrganisation.IsEmpty)
				{
					ABNCACSplitter splitter = new ABNCACSplitter(importerABNFromOrganisation);
					ZString importerABN = splitter.ABN;

					if (!importerABN.IsEmpty)
					{
						PopulateNAD(Group6, PartyFunctionCodeQualifierList.Claimant, importerABN);
					}
					if (!splitter.CAC.IsEmpty)
					{
						PopulateNAD(Group6, PartyFunctionCodeQualifierList.SubEntity, splitter.CAC);
					}
				}
				else if (!importerCIDFromOrganisation.IsEmpty)
				{
					PopulateNAD(Group6, PartyFunctionCodeQualifierList.Importer, importerCIDFromOrganisation.SubstringSafe(0, 11));
				}
			}
		}

		protected void PopulateNAD(SegmentGroup6 group6, PartyFunctionCodeQualifierList partyFunctionCodeQualifier, ZString partyIdentifier)
		{
			NADSegment nAD = group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
			nAD.PartyIdentificationDetails.PartyIdentifier = partyIdentifier.Replace(" ", "");
			nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		protected void PopulateNADForContact()
		{
			NADSegment nAD = Group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.ContactParty1;
			ZString contactName = GlbStaff.CurrentUser.GS_FullName;
			nAD.PartyName.PartyName1 = contactName.SubstringSafe(0, 35);
			nAD.PartyName.PartyName2 = contactName.SubstringSafe(35, 70);
		}

		protected void PopulateCTACOM()
		{
			ZString phone = declaration.DrawbackContactPhoneNumber;
			if (!phone.IsEmpty)
			{
				CTASegment cTA = Group6.CTA.InstantiateAChildAndAddItToChildrenCollection();
				cTA.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
				COMSegment cOM = Group6.COM.InstantiateAChildAndAddItToChildrenCollection();
				cOM.CommunicationContact.CommunicationNumberCodeQualifier = CommunicationNumberCodeQualifierList.Telephone;
				cOM.CommunicationContact.CommunicationNumber = phone;
			}
		}

		protected void PopulateGroup10()
		{
			SegmentGroup10 group10 = cUSDEC.Group10.InstantiateAChildAndAddItToChildrenCollection();
			DMSSegment dMS = group10.DMS.InstantiateAChildAndAddItToChildrenCollection();
			dMS.DocumentMessageIdentification.DocumentMessageNumber = "1";
			SegmentGroup16 group16 = group10.Group16.InstantiateAChildAndAddItToChildrenCollection();
			PACSegment pAC = group16.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pAC.NumberOfPackages = "1";

			//TODO: replace with correct declarations and user supplied answers

			SegmentGroup17 group17 = group16.Group17.InstantiateAChildAndAddItToChildrenCollection();
			PCISegment pCI = group17.PCI.InstantiateAChildAndAddItToChildrenCollection();
			pCI.MarkingInstructionsCoded = MarkingInstructionsCodedList.DoNotMarkSuppliersCompanyName;

			RFFSegment rFF = group17.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.StatementNumber;
			rFF.Reference.ReferenceIdentifier = "00283";
			rFF.Reference.ReferenceVersionIdentifier = "N";
		}

		protected void PopulateGroup30()
		{
			int lineNumber = 0;
			foreach (JobComInvoiceLine line in declaration.SortedInvoiceLines)
			{
				DRWBCKSegmentGroup30Builder group30Builder = new DRWBCKSegmentGroup30Builder(line, cUSDEC.Group30[lineNumber], lineNumber);
				group30Builder.PopulateSegment();
				lineNumber++;
			}
		}

		#region Implementation

		protected internal override ZString DocumentName => "DRWBCK";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.RequestForPayment;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.DRWBCK;

		protected internal override Type TypeOfMessage => typeof(CMRDRWBCKMessage);

		#endregion
	}
}
