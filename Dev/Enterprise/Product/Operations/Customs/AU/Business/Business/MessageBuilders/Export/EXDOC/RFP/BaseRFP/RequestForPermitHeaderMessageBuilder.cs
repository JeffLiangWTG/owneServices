using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class RequestForPermitHeaderMessageBuilder
	{
		protected RequestForPermitHeaderMessageBuilder(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
		{
			this.invoiceHeader = invoiceHeader;
			this.MessageTypeToSend = messageTypeToSend;
			sANCRT = new SANCRTMessage();
		}

		public ZString MessageText
		{
			get { return sANCRT.ToString(new Edifact.UNOBCharacterSet()); }
		}

#if DEBUG
		public ZString MessageTextForTesting
		{
			get { return sANCRT.ToString(new Edifact.UNOACharacterSet()); }
		}
#endif

		public RFPMessage GenerateCopyRFPMessage()
		{
			GenerateUNH();
			GenerateBGM();
			GenerateExporterReference();
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateOwnerExportNumber(group2);
			GenerateRFPFooter();
			RFPMessage message = invoiceHeader.Factory.New<RFPMessage>();
			message.EM_LinkUniqueID = invoiceHeader.QuarantineExDocHeader.PK;
			message.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.CPY;
			message.EM_MessageText = MessageText;
			invoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			return message;
		}

		public RFPMessage GenerateTransferRFPMessage()
		{
			GenerateUNH();
			GenerateBGM();
			GenerateCancelTransferIndicator();
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateOwnerExportNumber(group2);
			GenerateTransfereeExporterNumber(group2);
			SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			GenerateTransfereeEDIUserIdentifier(group3);
			GenerateRFPFooter();
			RFPMessage message = invoiceHeader.Factory.New<RFPMessage>();
			message.EM_LinkUniqueID = invoiceHeader.QuarantineExDocHeader.PK;
			message.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.TRF;
			message.EM_MessageText = MessageText;
			invoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			return message;
		}

		public RFPMessage GenerateTransferInRFPMessage()
		{
			GenerateUNH();
			GenerateBGM();
			GenerateExporterReference();
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateOwnerExportNumber(group2);
			GenerateRFPFooter();
			RFPMessage message = invoiceHeader.Factory.New<RFPMessage>();
			message.EM_LinkUniqueID = invoiceHeader.QuarantineExDocHeader.PK;
			message.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.TRF;
			message.EM_MessageText = MessageText;
			invoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			return message;
		}

		public RFPMessage GenerateWithdrawlRFPMessage()
		{
			GenerateUNH();
			GenerateBGM();
			GenerateExporterReference();
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateOwnerExportNumber(group2);
			GenerateRFPFooter();
			RFPMessage message = invoiceHeader.Factory.New<RFPMessage>();
			message.EM_LinkUniqueID = invoiceHeader.QuarantineExDocHeader.PK;
			message.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.CAN;
			message.EM_MessageText = MessageText;
			invoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			return message;
		}

		public RFPMessage GenerateAmendmentRFPMessage()
		{
			GenerateRFPHeader();
			GenerateRFPLines();
			GenerateRFPFooter();
			RFPMessage message = invoiceHeader.Factory.New<RFPMessage>();
			message.EM_LinkUniqueID = invoiceHeader.QuarantineExDocHeader.PK;
			message.EM_MessageType = EXDOCMessageTypeCodes.Descriptions.RPL;
			message.EM_MessageText = MessageText;
			invoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			if (!invoiceHeader.QuarantineExDocHeader.ManualAmendmentReasonForMessaging.IsEmpty)
			{
				invoiceHeader.QuarantineExDocHeader.AddInfo.ZH_AmendmentResponseStatus = RFPMessage.Status.AwaitingResponse;
			}

			return message;
		}

		public RFPMessage GenerateRFPMessage()
		{
			GenerateRFPHeader();
			GenerateRFPLines();
			GenerateRFPFooter();
			RFPMessage message = invoiceHeader.Factory.New<RFPMessage>();
			message.EM_LinkUniqueID = invoiceHeader.QuarantineExDocHeader.PK;
			message.EM_MessageType = MessageTypeToSend == EXDOCMessageTypeCodes.Codes.ORD ? EXDOCMessageTypeCodes.Descriptions.ORD : EXDOCMessageTypeCodes.Descriptions.LDG;
			message.EM_MessageText = MessageText;
			invoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			return message;
		}

		void GenerateRFPHeader()
		{
			GenerateUNH();
			GenerateBGM();
			GenerateDTM();
			GenerateLOC();
			GenerateRFF();
			GenerateFTX();
			GenerateMEA();
			GenerateMOA();
			GenerateGIS();
			GenerateGroup1();
			GenerateGroup2And3();
			GenerateGroup4();
			GenerateGroup6And7();
			GenerateGroup8And9();
			GenerateApprovedCertifier();
		}

		protected abstract void GenerateRFPLines();

		void GenerateRFPFooter()
		{
			GenerateUNT();
		}

		void GenerateUNH()
		{
			EXDOCMessageUtilities.PopulateUNH(sANCRT.UNH.InstantiateAChildAndAddItToChildrenCollection(),
				MessageTypeList.InternationalMovementOfGoodsGovernmentalRegulatoryMessage,
				MessageVersionNumberList.DraftVersionUnEdifactDirectory,
				MessageReleaseNumberList.Release1997B,
				ControllingAgencyList.UnEceTradeWp4,
				"RF0801");
		}

		protected abstract DocumentMessageNameCodedList CommodityType();

		void GenerateBGM()
		{
			if (MessageTypeToSend == EXDOCMessageTypeCodes.Codes.AcceptTransferIn || MessageTypeToSend == EXDOCMessageTypeCodes.Codes.DeclineTransferIn)
			{
				EXDOCMessageUtilities.PopulateBGM(sANCRT.BGM.InstantiateAChildAndAddItToChildrenCollection(),
					CommodityType(),
					CodeListResponsibleAgencyCodedList.GetFromString("AQ"),
					(invoiceHeader.JobDeclaration.IsAir && MessageTypeToSend != EXDOCMessageTypeCodes.Codes.TRF) ? "0" : "9",
					invoiceHeader.QuarantineExDocHeader.QH_RequestForPermitNumber,
					MessageFunctionCodedList.GetFromString("92"),
					MessageTypeToSend == EXDOCMessageTypeCodes.Codes.AcceptTransferIn ? ResponseTypeCodedList.Accepted : ResponseTypeCodedList.Rejected);
			}
			else
			{
				EXDOCMessageUtilities.PopulateBGM(sANCRT.BGM.InstantiateAChildAndAddItToChildrenCollection(),
					CommodityType(),
					CodeListResponsibleAgencyCodedList.GetFromString("AQ"),
					(invoiceHeader.JobDeclaration.IsAir && MessageTypeToSend != EXDOCMessageTypeCodes.Codes.TRF) ? "0" : "9",
					invoiceHeader.QuarantineExDocHeader.QH_RequestForPermitNumber,
					MessageFunctionCodedList.GetFromString(MessageTypeToSend),
					ResponseTypeCodedList.GetFromString(ZString.Empty));
			}
		}

		protected virtual void GenerateDTM()
		{
		}

		void GenerateLOC()
		{
			GenerateBorderInspection();
			GenerateLoadingPort();
			GenerateDischargePort();
			GenerateDestinationCity();
			if (invoiceHeader.QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.DestinationCountry))
			{
				GenerateDestinationCountry();
			}

			GenerateProductSourceCountry();
			GenerateTransitCountry();
			GenerateCertificateRequiredLocation();
			GenerateAQISRegion();
		}

		void GenerateBorderInspection()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_RL_NKBorderInspectionPort.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						PlaceLocationQualifierList.PlaceOfCustomsExamination,
						invoiceHeader.QuarantineExDocHeader.QH_RL_NKBorderInspectionPort);
			}
		}

		void GenerateLoadingPort()
		{
			if (invoiceHeader.JobDeclaration.PortOfLoading != null)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						PlaceLocationQualifierList.PlacePortOfLoading,
						invoiceHeader.JobDeclaration.PortOfLoading.Code.Right(3));
			}
		}

		void GenerateDischargePort()
		{
			if (invoiceHeader.JobDeclaration.PortOfArrival != null)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						PlaceLocationQualifierList.PortOfDischarge,
						invoiceHeader.JobDeclaration.PortOfArrival.Code);
			}
		}

		void GenerateDestinationCity()
		{
			if (invoiceHeader.JobDeclaration.FinalDestination != null)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.PlaceOfDestination,
					invoiceHeader.JobDeclaration.FinalDestination.Description);
			}
		}

		void GenerateDestinationCountry()
		{
			if (!invoiceHeader.JobDeclaration.JE_RL_NKFinalDestination.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.CountryOfUltimateDestination,
					invoiceHeader.JobDeclaration.JE_RL_NKFinalDestination.Left(2));
			}
		}

		void GenerateProductSourceCountry()
		{
			EXDOCMessageUtilities.PopulateLOC(
				sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
				PlaceLocationQualifierList.CountryOfSource,
				invoiceHeader.QuarantineExDocHeader.QH_RN_NKOriginCountry);
		}

		void GenerateTransitCountry()
		{
			if (invoiceHeader.JobDeclaration.PortOfFirstArrival != null && invoiceHeader.JobDeclaration.PortOfArrival != null && invoiceHeader.JobDeclaration.PortOfFirstArrival.RL_RN_NKCountryCode != invoiceHeader.JobDeclaration.PortOfArrival.RL_RN_NKCountryCode)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						PlaceLocationQualifierList.CountryOfTransit,
						invoiceHeader.JobDeclaration.PortOfFirstArrival.RL_RN_NKCountryCode);
			}
		}

		void GenerateCertificateRequiredLocation()
		{
			if (invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator != EXDOCCertificatePrintCodes.Codes.NotRequired && !invoiceHeader.QuarantineExDocHeader.QH_CertificateRequiredLocation.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						PlaceLocationQualifierList.PlaceOfDocumentIssue,
						invoiceHeader.QuarantineExDocHeader.QH_CertificateRequiredLocation);
			}
		}

		protected virtual void GenerateAQISRegion()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AQISRegion.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(sANCRT.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.RegionOfProduction,
					invoiceHeader.QuarantineExDocHeader.QH_AQISRegion);
			}
		}

		void GenerateRFF()
		{
			GenerateExporterReference();
			GenerateCustomsAuthorityNumber();
		}

		void GenerateExporterReference()
		{
			if (invoiceHeader.JobDeclaration.JE_UseOwnerRefAsQuarantineRef && !invoiceHeader.JobDeclaration.JE_OwnerRef.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateRFF(sANCRT.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.DeclarantsReferenceNumber,
						invoiceHeader.JobDeclaration.JE_OwnerRef);
			}
			else
			{
				EXDOCMessageUtilities.PopulateRFF(sANCRT.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.DeclarantsReferenceNumber,
						EDIMessage.SendersReferencePlaceHolder);
			}
		}

		void GenerateCustomsAuthorityNumber()
		{
			if (invoiceHeader.JobDeclaration.ExportEntryNumber != null)
			{
				EXDOCMessageUtilities.PopulateRFF(sANCRT.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.GoodsDeclarationNumber,
						invoiceHeader.JobDeclaration.ExportEntryNumber.CE_EntryNum);
			}
		}

		void GenerateFTX()
		{
			GenerateExporterDeclaration();
			GenerateInspectorComments();
			GenerateNotifyParty();
			GenerateLetterOfCredit();
			GenerateAdditionalInformation();
			GenerateOriginCatchingZone();
			GenerateLotNumber();
			if (invoiceHeader.QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.AmendedInformation))
			{
				GenerateAmendmentText();
			}

			GenerateAvAnimalAgeText();

			if (AUCustomsDataRegistry.Instance.IsErrata44Effective)
			{
				GenerateAMLCQuotaYear();
			}

			GenerateQuotaType();
		}

		void GenerateExporterDeclaration()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ExporterDeclaration.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateFTX(sANCRT,
						TextSubjectQualifierList.Declaration,
						invoiceHeader.QuarantineExDocHeader.QH_ExporterDeclarationInfo.MaxLength,
						MaxElementLength,
						invoiceHeader.QuarantineExDocHeader.QH_ExporterDeclaration);
			}
		}

		void GenerateInspectorComments()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_InspectorComments.IsEmpty)
			{
				const int MaxInspectorCommentChars = 210;
				EXDOCMessageUtilities.PopulateFTX(sANCRT,
				TextSubjectQualifierList.CertificationStatements,
				MaxInspectorCommentChars,
				MaxElementLength,
				invoiceHeader.QuarantineExDocHeader.QH_InspectorComments);
			}
		}

		protected virtual void GenerateNotifyParty()
		{
			if (!invoiceHeader.JobDeclaration.EXDOCNotifyText.IsEmpty)
			{
				const int MaxNotifyTextChars = 27225;
				const int MaxNotifyElementLength = 55;
				EXDOCMessageUtilities.PopulateFTX(
					sANCRT,
					TextSubjectQualifierList.PartyInstructions,
					MaxNotifyTextChars,
					MaxNotifyElementLength,
					invoiceHeader.JobDeclaration.EXDOCNotifyText);
			}
		}

		protected virtual void GenerateLetterOfCredit()
		{
			if (!invoiceHeader.JobDeclaration.EXDOCLetterOfCredit.IsEmpty)
			{
				const int MaxLetterOfCreditChars = 1000;
				EXDOCMessageUtilities.PopulateFTX(
					sANCRT,
					TextSubjectQualifierList.LetterOfCreditInformation,
					MaxLetterOfCreditChars,
					MaxElementLength,
					invoiceHeader.JobDeclaration.EXDOCLetterOfCredit);
			}
		}

		protected virtual void GenerateAdditionalInformation()
		{
		}

		protected virtual void GenerateOriginCatchingZone()
		{
		}

		protected virtual void GenerateLotNumber()
		{
		}

		protected virtual void GenerateAmendmentText()
		{
		}

		protected virtual void GenerateAvAnimalAgeText()
		{
		}

		void GenerateMEA()
		{
			GenerateTranshipmentStorageTemperature();
		}

		protected virtual void GenerateTranshipmentStorageTemperature()
		{
		}

		void GenerateMOA()
		{
			GenerateFOBCurrencyUnit();
		}

		void GenerateFOBCurrencyUnit()
		{
			if (invoiceHeader.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Dairy || invoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit)
			{
				if (invoiceHeader.Invoice_Currency != null)
				{
					EXDOCMessageUtilities.PopulateMOA(sANCRT.MOA.InstantiateAChildAndAddItToChildrenCollection(),
						MonetaryAmountTypeQualifierList.FobValue,
						invoiceHeader.Invoice_Currency.RX_Code);
				}
			}
		}

		void GenerateGIS()
		{
			GenerateCertificatePrintIndicator();
			GenerateSeperateCertificateContainerIndicator();
			GenerateSeperateCertificateMarksIndicator();
			GenerateSeperateCertificatePackerIndicator();
			GenerateShipStoresIndicator();
			GenerateRFPForwardStatus();
			GenerateAMLCQuotaIndicator();
			GenerateCustomsAgentIndicator();
			GenerateDeclarationEstimateIndicator();
			GenerateDeclarationOfComplianceIndicator();
			GenerateTrueAndCompleteIndicator();
			GenerateImportedProductFlag();
			GenerateProductUseIndicator();
		}

		void GenerateCertificatePrintIndicator()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT,
						invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator,
						CertificatePrintIndicatorQualifier);
			}
		}

		void GenerateSeperateCertificateContainerIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
								invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByContainer ? "Y" : "N",
								SeperateCertificateContainerIndicatorQualifier);
		}

		void GenerateSeperateCertificateMarksIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
								invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByMarks ? "Y" : "N",
								SeperateCertificateMarksIndicatorQualifier);
		}

		void GenerateSeperateCertificatePackerIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
								invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByPacker ? "Y" : "N",
								SeperateCertificatePackerIndicatorQualifier);
		}

		protected virtual void GenerateShipStoresIndicator()
		{
		}

		void GenerateRFPForwardStatus()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ForwardStatus.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT, invoiceHeader.QuarantineExDocHeader.QH_ForwardStatus, RFPForwardStatusQualifier);
			}
		}

		protected virtual void GenerateAMLCQuotaIndicator()
		{
		}

		protected virtual void GenerateAMLCQuotaYear()
		{
		}

		protected virtual void GenerateQuotaType()
		{
		}

		void GenerateCustomsAgentIndicator()
		{
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
								invoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit ? "Y" : "N",
								CustomsAgentIndicatorQualifier);
		}

		void GenerateDeclarationEstimateIndicator()
		{
			if (invoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit)
			{
				EXDOCMessageUtilities.PopulateGIS(sANCRT,
					invoiceHeader.JobDeclaration.JE_MessageSubType.Left(1),
					DeclarationEstimateIndicatorQualifier);
			}
		}

		protected virtual void GenerateDeclarationOfComplianceIndicator()
		{
		}

		protected virtual void GenerateTrueAndCompleteIndicator()
		{
		}

		protected virtual void GenerateImportedProductFlag()
		{
		}

		protected virtual void GenerateProductUseIndicator()
		{
		}

		void GenerateGroup1()
		{
			SegmentGroup1 group1;
			List<ZString> permitList = new List<ZString>();
			foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
			{
				if (!invoiceLine.JI_TempImportNum.IsEmpty && !permitList.Contains(invoiceLine.JI_TempImportNum))
				{
					permitList.Add(invoiceLine.JI_TempImportNum);
					group1 = sANCRT.Group1.InstantiateAChildAndAddItToChildrenCollection();
					GenerateImportPermitNumber(group1, invoiceLine.JI_TempImportNum);
					GenerateImportPermitDate(group1, invoiceLine.JI_TempImportDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
				}
			}

			if (invoiceHeader.QuarantineExDocHeader.RecommendationLetters != null && AUCustomsDataRegistry.Instance.IsErrata44Effective)
			{
				foreach (RecommendationLetter letter in invoiceHeader.QuarantineExDocHeader.RecommendationLetters)
				{
					var group = sANCRT.Group1.InstantiateAChildAndAddItToChildrenCollection();
					GenerateRecommendationLetterNumber(group, letter.ZA_LetterNumber);
					GenerateRecommendationLetterDate(group, letter.ZA_LetterDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
				}
			}
		}

		void GenerateImportPermitNumber(SegmentGroup1 group1, ZString temporaryImportNumber)
		{
			EXDOCMessageUtilities.PopulateDOC(
				group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
				DocumentMessageNameCodedList.ImportLicence,
				temporaryImportNumber.Trim());
		}

		void GenerateImportPermitDate(SegmentGroup1 group1, ZString temporaryImportDate)
		{
			EXDOCMessageUtilities.PopulateDTM(group1.DTM.InstantiateAChildAndAddItToChildrenCollection(),
				DateTimePeriodQualifierList.DocumentMessageDateTime,
				temporaryImportDate.Trim(),
				DateTimePeriodFormatQualifierList.Ccyymmdd);
		}

		void GenerateRecommendationLetterNumber(SegmentGroup1 group1, ZString recommendationLetterNumber)
		{
			EXDOCMessageUtilities.PopulateDOC(
				group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
				DocumentMessageNameCodedList.RelatedDocument,
				recommendationLetterNumber.Trim());
		}

		void GenerateRecommendationLetterDate(SegmentGroup1 group1, ZString recommendationLetterDate)
		{
			EXDOCMessageUtilities.PopulateDTM(group1.DTM.InstantiateAChildAndAddItToChildrenCollection(),
				DateTimePeriodQualifierList.PreparationDateTimeOfDocument,
				recommendationLetterDate.Trim(),
				DateTimePeriodFormatQualifierList.Ccyymmdd);
		}

		void GenerateGroup2And3()
		{
			GenerateOwnerExportNumberAndForwardeeEDIUserIdentifier();
			GenerateConsigneeNameAndAddress();
			GenerateConsigneeRepresentativeName();
		}

		void GenerateOwnerExportNumberAndForwardeeEDIUserIdentifier()
		{
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			GenerateOwnerExportNumber(group2);
			GenerateForwardeeEDIUserIdentifier(group3);
		}

		void GenerateOwnerExportNumber(SegmentGroup2 group2)
		{
			if (!invoiceHeader.EXDOCExporterNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(
					group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.Exporter,
					invoiceHeader.EXDOCExporterNumber);
			}
		}

		void GenerateForwardeeEDIUserIdentifier(SegmentGroup3 group3)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ForwardeeEDIUserIdentifier.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateCTA(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.Agent,
					invoiceHeader.QuarantineExDocHeader.QH_ForwardeeEDIUserIdentifier);
			}
		}

		OrgHeader Consignee => invoiceHeader.JobDeclaration.Consignee;

		void GenerateConsigneeNameAndAddress()
		{
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateConsigneeName(group2);
			GenerateConsigneeAddress(group2);
			GenerateConsigneeTRACESApproval(group2);
			GenerateConsigneePhone(group2);
		}

		void GenerateConsigneeName(SegmentGroup2 group2)
		{
			if (Consignee != null && !Consignee.OH_FullName.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
						PartyQualifierList.Consignee,
						ConsigneeReferenceNumber,
						NameComponentQualifierList.WholeName,
						Consignee.OH_FullName.SubstringSafe(0, MaxNameAddressElementLength));
			}
			else if (invoiceHeader.JobDeclaration.JE_ToOrder)
			{
				EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
				PartyQualifierList.Consignee,
				ZString.Empty,
				NameComponentQualifierList.WholeName,
				"TO ORDER");
			}
		}

		protected virtual ZString ConsigneeReferenceNumber
		{
			get
			{
				return Consignee != null ? Consignee.GetCustomsClientID() : ZString.Empty;
			}
		}

		void GenerateConsigneeAddress(SegmentGroup2 group2)
		{
			if (Consignee != null && !Consignee.MainAddress.AddressAsASingleLineWithoutCompanyName.IsEmpty)
			{
				var state = Consignee.MainAddress.StateCode.IsNumbersOnlyOrEmpty ? Consignee.MainAddress.State : Consignee.MainAddress.StateCode;
				EXDOCMessageUtilities.PopulateADR(
					group2.ADR.InstantiateAChildAndAddItToChildrenCollection(),
					AddressFormatCodedList.UnstructuredAddress,
					Consignee.MainAddress.OA_Address1.SubstringSafe(0, MaxNameAddressElementLength),
					Consignee.MainAddress.OA_Address2.SubstringSafe(0, MaxNameAddressElementLength),
					!Consignee.CityFallback.IsEmpty ? Consignee.CityFallback : (ZString)"UNKNOWN",
					Consignee.MainAddress.OA_PostCode,
					Consignee.UNLOCO != null ? Consignee.UNLOCO.RL_RN_NKCountryCode : ZString.Empty,
					state);
			}
			else if (invoiceHeader.JobDeclaration.JE_ToOrder)
			{
				EXDOCMessageUtilities.PopulateADR(group2.ADR.InstantiateAChildAndAddItToChildrenCollection(),
					AddressFormatCodedList.UnstructuredAddress,
					ZString.Empty,
					ZString.Empty,
					invoiceHeader.JobDeclaration.JE_ToOrderComment,
					ZString.Empty,
					invoiceHeader.JobDeclaration.JE_RL_NKFinalDestination.Left(2).Trim(),
					ZString.Empty);
			}
		}

		void GenerateConsigneeTRACESApproval(SegmentGroup2 group2)
		{
			if (Consignee != null)
			{
				var eUTracesApprovalID = Consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.EUTracesID, Core.Constants.CountryCodes.Australia);
				if (!eUTracesApprovalID.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateGroup2FTX(group2.FTX.InstantiateAChildAndAddItToChildrenCollection(),
						TextSubjectQualifierList.MutuallyDefined,
						eUTracesApprovalID);
				}
			}
		}

		protected virtual void GenerateConsigneePhone(SegmentGroup2 group2)
		{
			if (Consignee != null && !Consignee.MainAddress.OA_Phone.IsEmpty)
			{
				SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
				EXDOCMessageUtilities.PopulateCTA(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.Consignee, ZString.Empty);
				EXDOCMessageUtilities.PopulateCOM(group3.COM.InstantiateAChildAndAddItToChildrenCollection(), CommunicationChannelQualifierList.Telephone,
					Consignee.MainAddress.OA_Phone);
			}
		}

		void GenerateConsigneeRepresentativeName()
		{
			SegmentGroup2 group2 = sANCRT.Group2.InstantiateAChildAndAddItToChildrenCollection();
			SegmentGroup3 group3 = group2.Group3.InstantiateAChildAndAddItToChildrenCollection();
			GenerateConsigneeRepresentativeName(group2);
			GenerateConsigneeRepresentativeName(group3);
		}

		protected virtual void GenerateConsigneeRepresentativeName(SegmentGroup2 group2)
		{
		}

		protected virtual void GenerateConsigneeRepresentativeName(SegmentGroup3 group3)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_ConsigneeAgentName.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateCTARepName(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.Agent,
					invoiceHeader.QuarantineExDocHeader.QH_ConsigneeAgentName);
			}
		}

		void GenerateGroup4()
		{
			SegmentGroup4 group4 = sANCRT.Group4.InstantiateAChildAndAddItToChildrenCollection();
			GenerateTransportDetails(group4);
			GenerateDepartureDate(group4);
		}

		void GenerateTransportDetails(SegmentGroup4 group4)
		{
			var declaration = invoiceHeader.JobDeclaration;
			string transportModeCode = EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode(declaration.TransportMode);
			if (!string.IsNullOrEmpty(transportModeCode))
			{
				var idOfTheMeansOfTransport = declaration.IsAir ? declaration.JE_VoyageFlightNo.KeepAlphabeticCharacters() : declaration.JE_VesselName;
				EXDOCMessageUtilities.PopulateTDT(
					group4.TDT.InstantiateAChildAndAddItToChildrenCollection(),
					TransportStageQualifierList.AtDeparture,
					declaration.JE_VoyageFlightNo,
					transportModeCode,
					declaration.ShippingLine != null ? declaration.ShippingLine.OH_FullNameTruncated : ZString.Empty,
					idOfTheMeansOfTransport);
			}
		}

		void GenerateDepartureDate(SegmentGroup4 group4)
		{
			if (!invoiceHeader.JobDeclaration.JE_ExportDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group4.DTM.InstantiateAChildAndAddItToChildrenCollection(),
						DateTimePeriodQualifierList.DepartureDateTime,
						invoiceHeader.JobDeclaration.JE_ExportDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
						DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		void GenerateGroup6And7()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_StartHoldSeal.IsEmpty || !invoiceHeader.QuarantineExDocHeader.QH_EndHoldSeal.IsEmpty)
			{
				SegmentGroup6 group6 = sANCRT.Group6.InstantiateAChildAndAddItToChildrenCollection();
				SegmentGroup7 group7 = group6.Group7.InstantiateAChildAndAddItToChildrenCollection();
				GenerateVesselHold(group6);
				GenerateVesselHoldSeals(group7);
			}
		}

		void GenerateVesselHold(SegmentGroup6 group6)
		{
			EXDOCMessageUtilities.PopulateEQD(group6.EQD.InstantiateAChildAndAddItToChildrenCollection());
		}

		void GenerateVesselHoldSeals(SegmentGroup7 group7)
		{
			EXDOCMessageUtilities.PopulateSEL(group7.SEL.InstantiateAChildAndAddItToChildrenCollection(),
								invoiceHeader.QuarantineExDocHeader.QH_StartHoldSeal,
								invoiceHeader.QuarantineExDocHeader.QH_EndHoldSeal);
		}

		void GenerateGroup8And9()
		{
			GenerateInspectionProcessInformation();
			GenerateShipsHoldInspectionInformation();
			GenerateStorageProcessInformation();
		}

		bool ContainsProcessInformation
		{
			get
			{
				return !invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate.IsEmpty ||
						!invoiceHeader.QuarantineExDocHeader.QH_AuthorisedStartDate.IsEmpty ||
						!invoiceHeader.QuarantineExDocHeader.QH_AuthorisedEndDate.IsEmpty ||
						!invoiceHeader.QuarantineExDocHeader.QH_AuthorisationEstablishment.IsEmpty ||
						!invoiceHeader.QuarantineExDocHeader.QH_AuthorisingOfficerID.IsEmpty;
			}
		}

		void GenerateInspectionProcessInformation()
		{
			if (ContainsProcessInformation)
			{
				SegmentGroup8 group8 = sANCRT.Group8.InstantiateAChildAndAddItToChildrenCollection();
				SegmentGroup9 group9 = group8.Group9.InstantiateAChildAndAddItToChildrenCollection();
				GenerateInspectionProcess(group8);
				if (invoiceHeader.QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.InspectionRequestedDate))
				{
					GenerateInspectionRequestedDate(group8);
				}

				if (invoiceHeader.QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.RFPAuthorisedStartDate))
				{
					GenerateAuthorisedStartDate(group8);
				}

				if (invoiceHeader.QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.RFPAuthorisedEndDate))
				{
					GenerateAuthorisedEndDate(group8);
				}

				if (invoiceHeader.QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.RFPAuthorisationEstablishmentNumber))
				{
					GenerateAuthorisationEstablishment(group9);
				}

				GenerateAuthorisingOfficerIdentifier(group9);
			}
		}

		void GenerateShipsHoldInspectionInformation()
		{
			SegmentGroup8 group8 = sANCRT.Group8.InstantiateAChildAndAddItToChildrenCollection();
			foreach (QuarantineExDocShipsCompartment vesselHoldInspection in invoiceHeader.QuarantineExDocHeader.Compartments)
			{
				GenerateShipsHoldInspectionProcess(group8);
				GenerateCompartments(group8, vesselHoldInspection.QC_Compartments);
				GenerateInspectPort(group8, vesselHoldInspection.QC_RL_NKInspectionPort);
				GenerateShipsHoldInspectionDate(group8, vesselHoldInspection.QC_InspectionDate);
			}
		}

		void GenerateStorageProcessInformation()
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_StorageEstablishment.IsEmpty)
			{
				SegmentGroup8 group8 = sANCRT.Group8.InstantiateAChildAndAddItToChildrenCollection();
				SegmentGroup9 group9 = group8.Group9.InstantiateAChildAndAddItToChildrenCollection();
				GenerateStorageProcess(group8);
				GenerateStorageEstablishmentNumber(group9);
			}
		}

		protected virtual void GenerateInspectionProcess(SegmentGroup8 group8)
		{
			const string InspectionProcess = "IN";
			EXDOCMessageUtilities.PopulatePRC(group8.PRC.InstantiateAChildAndAddItToChildrenCollection(),
								ProcessTypeIdentificationList.GetFromString(InspectionProcess));
		}

		void GenerateStorageProcess(SegmentGroup8 group8)
		{
			const string StorageProcess = "ST";
			EXDOCMessageUtilities.PopulatePRC(group8.PRC.InstantiateAChildAndAddItToChildrenCollection(),
								ProcessTypeIdentificationList.GetFromString(StorageProcess));
		}

		protected virtual void GenerateInspectionRequestedDate(SegmentGroup8 group8)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group8.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.RequestDate,
					invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected virtual void GenerateAuthorisedStartDate(SegmentGroup8 group8)
		{
		}

		protected virtual void GenerateAuthorisedEndDate(SegmentGroup8 group8)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AuthorisedEndDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group8.DTM.InstantiateAChildAndAddItToChildrenCollection(),
						DateTimePeriodQualifierList.TestCompletionDate,
						invoiceHeader.QuarantineExDocHeader.QH_AuthorisedEndDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
						DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		void GenerateAuthorisationEstablishment(SegmentGroup9 group9)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AuthorisationEstablishment.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(group9.PNA.InstantiateAChildAndAddItToChildrenCollection(),
						PartyQualifierList.PartyPerformingInspection,
						invoiceHeader.QuarantineExDocHeader.QH_AuthorisationEstablishment);
			}
		}

		protected virtual void GenerateAuthorisingOfficerIdentifier(SegmentGroup9 group9)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_AuthorisingOfficerID.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(group9.PNA.InstantiateAChildAndAddItToChildrenCollection(),
						PartyQualifierList.AuthorizingOfficial,
						invoiceHeader.QuarantineExDocHeader.QH_AuthorisingOfficerID);
			}
		}

		protected virtual void GenerateShipsHoldInspectionProcess(SegmentGroup8 group8)
		{
		}

		protected virtual void GenerateCompartments(SegmentGroup8 group8, ZString compartments)
		{
		}

		protected virtual void GenerateInspectPort(SegmentGroup8 group8, ZString inspectionPort)
		{
		}

		protected virtual void GenerateShipsHoldInspectionDate(SegmentGroup8 group8, ZDateTime inspectionDateTime)
		{
		}

		protected void GenerateStorageEstablishmentNumber(SegmentGroup9 group9)
		{
			EXDOCMessageUtilities.PopulatePNA(group9.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.ManufacturingPlant,
					invoiceHeader.QuarantineExDocHeader.QH_StorageEstablishment);
		}

		protected virtual void GenerateApprovedCertifier()
		{
		}

		void GenerateCancelTransferIndicator()
		{
			const string CancelTransferProcess = "CT";
			EXDOCMessageUtilities.PopulateGIS(sANCRT,
					invoiceHeader.QuarantineExDocHeader.QH_CancelTransferIndicator ? "Y" : "N",
					CancelTransferProcess);
		}

		void GenerateTransfereeExporterNumber(SegmentGroup2 group2)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_TransfereeExporterNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
						PartyQualifierList.TransferTo,
						invoiceHeader.QuarantineExDocHeader.QH_TransfereeExporterNumber);
			}
		}

		void GenerateTransfereeEDIUserIdentifier(SegmentGroup3 group3)
		{
			if (!invoiceHeader.QuarantineExDocHeader.QH_TransfereeEDIUserIdentifier.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateCTA(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
						ContactFunctionCodedList.Agent,
						invoiceHeader.QuarantineExDocHeader.QH_TransfereeEDIUserIdentifier);
			}
		}

		void GenerateUNT()
		{
			SegmentGroup21 group21 = sANCRT.Group21.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateUNT(group21.UNT.InstantiateAChildAndAddItToChildrenCollection(), sANCRT.CountIncludingUNT.ToString());
		}

		#region Implementation

		public const string DeclarationEstimateIndicatorQualifier = "DCT";
		public const string CustomsAgentIndicatorQualifier = "ACS";
		public const string AMLCQuotaIndicatorQualifier = "QI";
		public const string SeperateCertificatePackerIndicatorQualifier = "SP";
		public const string SeperateCertificateMarksIndicatorQualifier = "SM";
		public const string SeperateCertificateContainerIndicatorQualifier = "SC";
		public const string CertificatePrintIndicatorQualifier = "PHC";
		public const string RFPForwardStatusQualifier = "FW";
		public const string TrueAndCompleteIndicator = "TAC";
		public const string ProductUseIndicator = "PUI";
		public const string DeclarationOfComplianceIndicator = "DOC";

		protected readonly string MessageTypeToSend;
		protected SANCRTMessage sANCRT;
		public const int MaxElementLength = 70;
		public const int MaxAmendmentReasonChars = 254;
		public const int MaxAmendmentReasonElementChars = 35;
		public const int MaxAvAnimalAgeChars = 20;
		public const int MaxNameAddressElementLength = 35;

		protected JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}
