using System;
using System.Globalization;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMessageProcessor : CustomsMessageProcessor
	{
		public EXDOCMessageProcessor(LoggingInformation logger)
			: base(logger, EDIInterchange.ApplicationCodes.EXDOC, "Request for Permit")
		{
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			ediMessage = message;
			string result = EDIMessage.Status.Error;
			SANCRTMessage sancrtMessage = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOBCharacterSet()) as SANCRTMessage;

			try
			{
				if (sancrtMessage == null)
				{
					throw new InvalidFormatException("Corrupted or Malformed Response Message. Message does not conform to UN-EDIFACT Standard. Cannot Process.");
				}

				messageDecoder = new EXDOCMessageDecoder(sancrtMessage, message.Factory);
				messageDecoder.Process();

				if (IsTransferOrForward)
				{
					if (ExDocHeader == null)
					{
						CreateDeclaration();
					}
					else
					{
						UpdateDeclaration();
					}

					result = EDIMessage.Status.Received;
				}
				else
				{
					if (ExDocHeader == null)
					{
						throw new InvalidFormatException("Could not find Job Declaration");
					}

					if (messageDecoder.IsAcceptedWithdrawal)
					{
						WithdrawalDeclaration();
					}
					else if ((messageDecoder.IsAcknowledgement && messageDecoder.IsAccepted) || messageDecoder.IsAccpetTransfer)
					{
						RFPAcknowledgementUpdateDeclaration();
					}
					else if (messageDecoder.IsReissue || messageDecoder.IsEnquiryAdvice)
					{
						UpdateDeclaration();
					}
					else if (messageDecoder.IsCertificateRequestAcknowledgement && messageDecoder.IsAccepted)
					{
						CertificateRequestAcknowledgementUpdateDeclaration();
					}

					if (!messageDecoder.IsAccepted)
					{
						ediMessage.EM_MessageType = IsAccepted ? EDIMessage.Status.Acknowledged : EDIMessage.Status.Rejected;
						ExDocHeader.Declaration.JE_MessageStatus = ediMessage.EM_MessageType;
					}
					result = EDIMessage.Status.Received;
				}

				ExDocHeader.Messages.Add(ediMessage);
				ediMessage.Factory.Save();
				CompileResponseEmail();

				if (IsAccepted)
				{
					SendAcknowledgementReport(ExDocHeader, responseEmail);
				}
				else
				{
					SendImpedimentReport(ExDocHeader, responseEmail);
				}
			}
			catch (InvalidFormatException messageProcessingException)
			{
				string emailBodyHeader = "FATAL PROCESSING ERROR: " + messageProcessingException.Message + System.Environment.NewLine + System.Environment.NewLine;
				string messageText = ediMessage.EM_FormattedMessageText.Replace("'", "'" + System.Environment.NewLine);

				responseEmail.Subject = "ERROR PROCESSING:";
				responseEmail.Body = emailBodyHeader + messageText;

				SendErrorReport(null, responseEmail);
			}

			return result;
		}

		#region ExDocHeader

		QuarantineExDocHeader ExDocHeader
		{
			get
			{
				if (quarantineExDocHeader == null && !messageDecoder.RequestIdentificationNumber.IsEmpty)
				{
					ZQuery filter = new ZQuery(CusEntryNumSchema.CE_EntryNum, messageDecoder.RequestIdentificationNumber);
					filter.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
					filter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);
					CusEntryNumber cusEntryNumber = ediMessage.Factory.LoadTop1<CusEntryNumber>(filter);
					if (cusEntryNumber != null)
					{
						quarantineExDocHeader = ediMessage.Factory.Load<QuarantineExDocHeader>(cusEntryNumber.CE_ParentID);
					}
				}

				if (quarantineExDocHeader == null && !messageDecoder.ExporterReference.IsEmpty)
				{
					ZQuery filter = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, messageDecoder.ExporterReference);
					filter.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine);
					JobDeclaration declaration = ediMessage.Factory.LoadTop1<JobDeclaration>(filter);
					if (declaration != null && declaration.Invoices.Count > 0 && declaration.Invoices[0] != null)
					{
						quarantineExDocHeader = declaration.Invoices[0].QuarantineExDocHeader;
					}
					else
					{
						filter = new ZQuery(JobDeclarationSchema.JE_OwnerRef, messageDecoder.ExporterReference);
						filter.AddToFilter(JobDeclarationSchema.JE_MessageType, Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine);
						declaration = ediMessage.Factory.LoadTop1<JobDeclaration>(filter);
						if (declaration != null && declaration.Invoices.Count > 0 && declaration.Invoices[0] != null)
						{
							quarantineExDocHeader = declaration.Invoices[0].QuarantineExDocHeader;
						}
					}
				}

				return quarantineExDocHeader;
			}
		}

		#endregion

		#region Response Email

		void CompileResponseEmail()
		{
			HtmlTableCreator creator = null;
			if (messageDecoder.Notices.Count > 0)
			{
				creator = new HtmlTableCreator(new[] { "Ln #", "Error", "Message" });
				foreach (EXDOCMessageDecoderNotice notice in messageDecoder.Notices)
				{
					creator.WriteRow(notice.lineNumber, notice.errorMessageIdentifier, notice.narrativeMessage);
				}
			}

			string requestType;
			string entryNumber;
			string entryStatus;
			if (messageDecoder.IsCertificateRequestAcknowledgement)
			{
				requestType = "Certificate Request";
				entryNumber = ExDocHeader.CertificateRequestNumber;
				entryStatus = ExDocHeader.CertificateStatus;
			}
			else
			{
				requestType = "RFP";
				entryNumber = ExDocHeader.QH_RequestForPermitNumber;
				entryStatus = ExDocHeader.RequestForPermitStatus;
			}

			string emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.AU.Declaration.Business.MessageProcessors.EXDOC.HtmlTemplates.Response.htm"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{0}", ExDocHeader.Declaration.JE_DeclarationReference);
			emailTemplateHtml = emailTemplateHtml.Replace("{1}", entryNumber);
			emailTemplateHtml = emailTemplateHtml.Replace("{2}", new EXDOCComplianceStatusCodesForCusEntryNumber().GetDescriptionFromCode(entryStatus));
			emailTemplateHtml = emailTemplateHtml.Replace("{3}", ExDocHeader.QH_ExportPermitNumber);
			emailTemplateHtml = emailTemplateHtml.Replace("{4}", ExDocHeader.Declaration.DeclarationNumber);
			emailTemplateHtml = emailTemplateHtml.Replace("{5}", requestType);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator != null ? creator.ToHtml() : string.Empty);
			if (missingLines != null)
			{
				emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtm2-->", "<br />The response contains the following data for lines not found on this declaration. Were these lines deleted after sending the message to Quarantine?<br />");
				emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtm3-->", MissingLines.ToHtml());
			}
			else
			{
				emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtm2-->", string.Empty).Replace("<!--DynamicHtm3-->", string.Empty);
			}

			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			string failureText = IsAccepted ? "Accepted" : "Rejected";
			string subject = string.Format("{0} {1} for {2}", requestType, failureText, ExDocHeader.Declaration.JE_DeclarationReference);
			responseEmail = emailSender.CreateEmail(subject, emailTemplateHtml);
		}

		ZBool IsAccepted
		{
			get { return messageDecoder.IsAccepted || messageDecoder.IsReissue || messageDecoder.IsTransfer || messageDecoder.IsForward; }
		}

		ZBool IsTransferOrForward
		{
			get { return messageDecoder.IsTransfer || messageDecoder.IsForward; }
		}

		#endregion

		#region Email Recipients

		protected override ZGuid AcknowledgementEmailGroup
		{
			get
			{
				var declaration = ExDocHeader.Declaration;
				return AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			}
		}

		protected override ZString AcknowledgementEmailMode
		{
			get
			{
				var declaration = ExDocHeader.Declaration;
				return AUCustomsDataRegistry.Instance.SendAQISAcknowledgements.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return AUCustomsDataRegistry.Instance.SendAQISErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return AUCustomsDataRegistry.Instance.SendAQISErrors.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get
			{
				var declaration = ExDocHeader.Declaration;
				return AUCustomsDataRegistry.Instance.SendAQISImpedimentsToGroup.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			}
		}

		protected override ZString ImpedimentEmailMode
		{
			get
			{
				var declaration = ExDocHeader.Declaration;
				return AUCustomsDataRegistry.Instance.SendAQISImpediments.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			}
		}

		#endregion

		#region Declaration Update

		#region Create Declaration

		void CreateDeclaration()
		{
			var defaultBranch = ediMessage.Factory.Load<GlbBranch>(AUCustomsDataRegistry.Instance.DefaultBranchForTransferIn.Value);
			if (defaultBranch == null || defaultBranch.Country.IsNull || defaultBranch.Country.Code != Core.Constants.CountryCodes.Australia)
			{
				CreateDeclarationCore();
			}
			else
			{
				using (DisposableEnvironment.ForBranch(defaultBranch.PK.ToGuid()))
				{
					CreateDeclarationCore();
				}
			}
		}

		void CreateDeclarationCore()
		{
			JobDeclaration declaration = ediMessage.Factory.New<JobDeclaration>();
			DeclarationUpdate(declaration);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			InvoiceHeaderUpdate(invoiceHeader);
			ShipsCompartmentsUpdate(invoiceHeader);
			CreateInvoiceLine(invoiceHeader);
			if (!totalFOBAmount.IsEmpty)
			{
				invoiceHeader.JZ_InvoiceAmount = totalFOBAmount;
			}
		}

		void CreateInvoiceLine(JobComInvoiceHeader invoiceHeader)
		{
			foreach (EXDOCMessageDecoderLine messageDecoderLine in messageDecoder.Lines)
			{
				JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
				InvoiceLineUpdate(line, messageDecoderLine);
				LinkContainersToInvoiceLine(line, messageDecoderLine);
			}
		}

		#endregion

		void WithdrawalDeclaration()
		{
			ediMessage.EM_MessageType = EDIMessage.Status.Acknowledged;
			ExDocHeader.Declaration.JE_MessageStatus = ediMessage.EM_MessageType;
			ExDocHeader.QH_ExportPermitNumber = ZString.Empty;
			ExDocHeader.QH_RequestForPermitNumber = ZString.Empty;
			ExDocHeader.RequestForPermitStatus = ZString.Empty;
			ExDocHeader.Declaration.DeclarationNumber = ZString.Empty;
			ExDocHeader.Declaration.JE_EntryStatus = ZString.Empty;
		}

		void UpdateDeclaration()
		{
			ediMessage.EM_MessageType = IsAccepted ? EDIMessage.Status.Acknowledged : EDIMessage.Status.Rejected;
			ExDocHeader.Declaration.JE_MessageStatus = ediMessage.EM_MessageType;
			DeclarationUpdate(ExDocHeader.Declaration);
			InvoiceHeaderUpdate(ExDocHeader.InvoiceHeader);
			ShipsCompartmentsUpdate(ExDocHeader.InvoiceHeader);
			foreach (EXDOCMessageDecoderLine messageDecoderLine in messageDecoder.Lines)
			{
				JobComInvoiceLine line = ExDocHeader.InvoiceHeader.JobComInvoiceLines.GetByLineNo(messageDecoderLine.LineNumber);
				if (line == null)
				{
					line = ExDocHeader.InvoiceHeader.JobComInvoiceLines.AddNew();
					line.JI_LineNo = messageDecoderLine.LineNumber;
				}

				InvoiceLineUpdate(line, messageDecoderLine);
				LinkContainersToInvoiceLine(line, messageDecoderLine);
			}
		}

		#region Acknowledgement

		void RFPAcknowledgementUpdateDeclaration()
		{
			AcknowledgementUpdateStatusAndLines();

			if (!messageDecoder.CustomsAuthorityNumber.IsEmpty)
			{
				ExDocHeader.Declaration.DeclarationNumber = messageDecoder.CustomsAuthorityNumber;
				ExDocHeader.Declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			}

			ExDocHeader.QH_ExportPermitNumber = messageDecoder.ExportPermitNumber;

			ExDocHeader.QH_RequestForPermitNumber = messageDecoder.RequestIdentificationNumber;
			ExDocHeader.RequestForPermitStatus = messageDecoder.ComplianceStatus.Left(3);
		}

		void CertificateRequestAcknowledgementUpdateDeclaration()
		{
			AcknowledgementUpdateStatusAndLines();
			ExDocHeader.InvoiceHeader.QuarantineExDocHeader.CertificateRequestNumber = messageDecoder.RequestIdentificationNumber;
			ExDocHeader.InvoiceHeader.QuarantineExDocHeader.CertificateStatus = messageDecoder.CertificateRequestStatus.Left(1);
		}

		void AcknowledgementUpdateStatusAndLines()
		{
			ediMessage.EM_MessageType = IsAccepted ? EDIMessage.Status.Acknowledged : EDIMessage.Status.Rejected;
			ExDocHeader.Declaration.JE_MessageStatus = ediMessage.EM_MessageType;

			foreach (EXDOCMessageDecoderLine messageDecoderLine in messageDecoder.Lines)
			{
				JobComInvoiceLine line = ExDocHeader.InvoiceHeader.JobComInvoiceLines.GetByLineNo(messageDecoderLine.LineNumber);
				if (line != null)
				{
					line.QuarantineExDocLine.QL_HealthCertificateDescription = messageDecoderLine.GeneratedProductDescription;
					line.QuarantineExDocLine.QL_HCFormatAllocated = ToZStringWithDelimiterBetweenAppends(messageDecoderLine.AssignedCertificateTemplates, line.QuarantineExDocLine.QL_HCFormatAllocatedInfo.MaxLength);
					line.QuarantineExDocLine.QL_HCNumber = ToZStringWithDelimiterBetweenAppends(messageDecoderLine.AssignedCertificateNumbers, line.QuarantineExDocLine.QL_HCNumberInfo.MaxLength);
					if (messageDecoderLine.ExtraCertificates.Length > 0)
					{
						line.QuarantineExDocLine.QL_ExtraCertificate = ToZStringWithDelimiterBetweenAppends(messageDecoderLine.ExtraCertificates, line.QuarantineExDocLine.QL_ExtraCertificateInfo.MaxLength);
					}
				}
				else
				{
					MissingLines.WriteRow(messageDecoderLine.LineNumber.ToString(),
							messageDecoderLine.GeneratedProductDescription,
							ToZStringWithDelimiterBetweenAppends(messageDecoderLine.AssignedCertificateTemplates, QuarantineExDocLineSchema.QL_HCFormatAllocated.MaxLength),
							ToZStringWithDelimiterBetweenAppends(messageDecoderLine.AssignedCertificateNumbers, QuarantineExDocLineSchema.QL_HCNumber.MaxLength),
							messageDecoderLine.ExtraCertificates.Length > 0 ? ToZStringWithDelimiterBetweenAppends(messageDecoderLine.ExtraCertificates, QuarantineExDocLineSchema.QL_ExtraCertificate.MaxLength) : ZString.Empty);
				}
			}
		}

		HtmlTableCreator MissingLines
		{
			get
			{
				return missingLines ?? (missingLines = new HtmlTableCreator(new[] { "Ln #", "Description", "Allocated format", "HC Number", "Extra Cert" }));
			}
		}
		HtmlTableCreator missingLines;

		static ZString ToZStringWithDelimiterBetweenAppends(ZStringBuilder builder, int maxLength)
		{
			ZString result = builder.ToStringWithDelimiterBetweenAppends(",");
			return result.Left(maxLength);
		}

		#endregion

		#region Implementation

		#region LinkContainersToInvoiceLine

		void LinkContainersToInvoiceLine(JobComInvoiceLine line, EXDOCMessageDecoderLine messageDecoderLine)
		{
			foreach (EXDOCMessageDecoderContainer container in messageDecoderLine.Containers)
			{
				AddContainerIfNotExists(container, line);
				NonPersistentCusContainer cusContainer = line.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(container.containerNumber);
				if (!cusContainer.IsForInvoiceLine)
				{
					cusContainer.IsForInvoiceLine = true;
				}
				cusContainer.NetWeightInKG = container.iMA1NetWeight;
				cusContainer.GrossWeightInKG = container.iMA1GrossWeight;
				line.QuarantineExDocLine.QL_IMA1SerialNumber = container.iMA1SerialNumber;
				line.QuarantineExDocLine.QL_IMA1QuotaYear = container.iMA1QuotaYear;
			}
		}

		void AddContainerIfNotExists(EXDOCMessageDecoderContainer container, JobComInvoiceLine line)
		{
			CusContainer cusContainer = line.InvoiceHeader.JobDeclaration.CusContainers.Find(container.containerNumber);
			if (cusContainer == null)
			{
				cusContainer = line.InvoiceHeader.JobDeclaration.CusContainers.AddNew();
				cusContainer.CO_ContainerNumber = container.containerNumber;
				cusContainer.CO_Seal = container.containerSeal;
			}
		}

		#endregion

		void DeclarationUpdate(JobDeclaration declaration)
		{
			declaration.JE_OH_Supplier = messageDecoder.Supplier;
			if (messageDecoder.IsToOrderImporter)
			{
				declaration.JE_ToOrder = true;
				declaration.JE_ToOrderComment = messageDecoder.ConsigneeCity;
			}
			else
			{
				declaration.JE_OH_Importer = messageDecoder.Importer;
			}

			if (!messageDecoder.UnmatchedNoteInformation.IsEmpty)
			{
				StmNote[] unmatchOrgNote = declaration.NotesOfDeclarationOrShipment.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);

				if (unmatchOrgNote.Length > 0)
				{
					unmatchOrgNote[0].ST_NoteText = messageDecoder.UnmatchedNoteInformation;
				}
				else
				{
					declaration.NotesOfDeclarationOrShipment.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, messageDecoder.UnmatchedNoteInformation);
				}
			}
			declaration.JE_MessageType = Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			declaration.JE_OH_Forwarder = messageDecoder.Forwarder;
			declaration.JE_TransportMode = messageDecoder.TransportMode;
			declaration.JE_VoyageFlightNo = messageDecoder.VoyageFlightNumber;
			declaration.JE_VesselName = messageDecoder.VesselName;
			declaration.JE_OH_ShippingLine = messageDecoder.Carrier;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.JE_RL_NKPortOfLoading = messageDecoder.LoadingPort;
			declaration.JE_RL_NKOrigin = messageDecoder.LoadingPort;
			declaration.JE_RL_NKPortOfArrival = messageDecoder.DischargePort;
			declaration.JE_RL_NKFinalDestination = messageDecoder.FinalDestination;
			declaration.JE_RL_NKPortOfFirstArrival = messageDecoder.TransitCountry;
			if (!messageDecoder.CustomsAuthorityNumber.IsEmpty)
			{
				declaration.DeclarationNumber = messageDecoder.CustomsAuthorityNumber;
				declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
				declaration.Logs.AddNew(Events.StatusChange, "AQSDec#: " + declaration.DeclarationNumber);
			}
			AddDeclarationNote(declaration, PredefinedNoteTypes.Instance.EXDOCNotifyText.Description, messageDecoder.NotifyPartyText);
			AddDeclarationNote(declaration, PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description, messageDecoder.LetterOfCreditText);
			AddDeclarationNote(declaration, PredefinedNoteTypes.Instance.EXDOCAdditionalInformation.Description, messageDecoder.AdditionalInformation);
			AddDeclarationNote(declaration, PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description, messageDecoder.AmendmentReason);
		}

		void AddDeclarationNote(JobDeclaration declaration, ZString description, ZString noteText)
		{
			if (!noteText.IsEmpty)
			{
				StmNote[] notes = declaration.NotesOfDeclarationOrShipment.FindByDescription(description);
				if (notes.Length > 0)
				{
					notes[0].ST_NoteText = noteText;
				}
				else
				{
					StmNote note = declaration.NotesOfDeclarationOrShipment.AddNew();
					note.ST_Description = description;
					note.ST_NoteText = noteText;
				}
			}
		}

		#endregion

		#endregion

		#region Invoice Header Update

		void InvoiceHeaderUpdate(JobComInvoiceHeader invoiceHeader)
		{
			if (invoiceHeader.JZ_InvoiceNumber.IsEmpty)
			{
				invoiceHeader.JZ_InvoiceNumber = "1";
			}

			invoiceHeader.JZ_OH_Supplier = messageDecoder.Supplier;
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = messageDecoder.ProduceType;
			invoiceHeader.QuarantineExDocHeader.QH_ExportPermitNumber = messageDecoder.ExportPermitNumber;
			invoiceHeader.QuarantineExDocHeader.QH_RequestForPermitNumber = messageDecoder.RequestIdentificationNumber;
			invoiceHeader.QuarantineExDocHeader.RequestForPermitStatus = messageDecoder.ComplianceStatus.Left(3);
			invoiceHeader.JobDeclaration.JE_ExportDate = messageDecoder.DepartureDate;
			invoiceHeader.QuarantineExDocHeader.QH_RL_NKBorderInspectionPort = messageDecoder.BorderInspectionPort;
			invoiceHeader.QuarantineExDocHeader.QH_RN_NKOriginCountry = messageDecoder.ProductSourceCountry;
			invoiceHeader.QuarantineExDocHeader.QH_CertificateRequiredLocation = messageDecoder.CertificateRequiredLocation;
			invoiceHeader.QuarantineExDocHeader.QH_AQISRegion = messageDecoder.AqisRegion;
			invoiceHeader.JobDeclaration.JE_OwnerRef = messageDecoder.ExporterReference;
			invoiceHeader.QuarantineExDocHeader.QH_ExporterDeclaration = messageDecoder.ExporterDeclaration;
			invoiceHeader.QuarantineExDocHeader.QH_InspectorComments = messageDecoder.InspectorComments;
			invoiceHeader.QuarantineExDocHeader.QH_OriginCatchZone = messageDecoder.OriginCatchingZone;
			invoiceHeader.QuarantineExDocHeader.QH_LotNumber = messageDecoder.LotNumber;
			invoiceHeader.QuarantineExDocHeader.QH_TemperatureUM = messageDecoder.TemperatureUnit;
			invoiceHeader.QuarantineExDocHeader.QH_AbsoluteTemperature = messageDecoder.AbsoluteTemperature;
			invoiceHeader.QuarantineExDocHeader.QH_MinimumTemperature = messageDecoder.MinimumTemperature;
			invoiceHeader.QuarantineExDocHeader.QH_MaximumTemperature = messageDecoder.MaximumTemperature;
			invoiceHeader.QuarantineExDocHeader.QH_AvAnimalAge = messageDecoder.AvAnimalAge;
			invoiceHeader.QuarantineExDocHeader.QH_ApprovedCertifier = messageDecoder.ApprovedCertifier;
			if (!messageDecoder.FobCurrencyUnit.IsEmpty)
			{
				RefCurrency currency = invoiceHeader.Factory.Load<RefCurrency>(messageDecoder.FobCurrencyUnit);
				if (currency != null)
				{
					invoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;
				}
			}
			invoiceHeader.QuarantineExDocHeader.QH_CertificatePrintIndicator = messageDecoder.CertificatePrintIndicator;
			invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByContainer = messageDecoder.SeparateCertificateContainerIndicator;
			invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByMarks = messageDecoder.SeparateCertificateMarksIndicator;
			invoiceHeader.QuarantineExDocHeader.QH_SplitHealthCertByPacker = messageDecoder.SeparateCertificatePackerIndicator;
			invoiceHeader.QuarantineExDocHeader.QH_ShipsStores = messageDecoder.ShipsStoresIndicator;
			invoiceHeader.QuarantineExDocHeader.QH_ForwardStatus = messageDecoder.ForwardStatus;
			invoiceHeader.QuarantineExDocHeader.QH_AMLCQuota = messageDecoder.AMLCQuotaIndicator;
			invoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = messageDecoder.CustomsAgentIndicator;
			invoiceHeader.JobDeclaration.JE_MessageSubType = messageDecoder.DeclarationEstimate;
			if (!messageDecoder.DeclarationOfComplianceIndicator.IsEmpty)
			{
				invoiceHeader.QuarantineExDocHeader.QH_DecOfCompliance = messageDecoder.DeclarationOfComplianceIndicator;
			}
			if (!messageDecoder.TrueAndCompleteIndicator.IsEmpty)
			{
				invoiceHeader.QuarantineExDocHeader.QH_TrueAndCompleteIndicator = messageDecoder.TrueAndCompleteIndicator;
			}
			if (!messageDecoder.ImportedProductIndicator.IsEmpty)
			{
				invoiceHeader.QuarantineExDocHeader.QH_ImportedProductFlag = messageDecoder.ImportedProductIndicator;
			}
			invoiceHeader.QuarantineExDocHeader.QH_ForwardeeEDIUserIdentifier = messageDecoder.ForwardeeEDIUserIdentifier;
			invoiceHeader.QuarantineExDocHeader.QH_StartHoldSeal = messageDecoder.StartSealNumber;
			invoiceHeader.QuarantineExDocHeader.QH_EndHoldSeal = messageDecoder.EndSealNumber;
			invoiceHeader.QuarantineExDocHeader.QH_InspectionRequestedDate = messageDecoder.InspectionRequestedDate;
			invoiceHeader.QuarantineExDocHeader.QH_AuthorisedStartDate = messageDecoder.AuthorisedStartDate;
			invoiceHeader.QuarantineExDocHeader.QH_AuthorisedEndDate = messageDecoder.AuthorisedEndDate;
			invoiceHeader.QuarantineExDocHeader.QH_AuthorisationEstablishment = messageDecoder.AuthorisationEstablishment;
			invoiceHeader.QuarantineExDocHeader.QH_AuthorisingOfficerID = messageDecoder.AuthorisingOfficeIdentifier;
			invoiceHeader.QuarantineExDocHeader.QH_StorageEstablishment = messageDecoder.StoreageEstablishmentNumber;
		}

		#endregion

		#region Invoice Line Update

		void InvoiceLineUpdate(JobComInvoiceLine line, EXDOCMessageDecoderLine messageDecoderLine)
		{
			line.JI_Tariff = messageDecoderLine.AheccCode;
			line.JI_InvoiceUQ = new CustomsQtyUnitToRFPPackType().GetCodeFromDescription(messageDecoderLine.OuterPackType);
			line.JI_CustomsUnitQty = new CustomsQtyUnitToRFPQtyUnit().GetCodeFromDescription(messageDecoderLine.NetQuantityUnit);
			line.JI_InvoiceQuantity = (ZDecimal)messageDecoderLine.OuterPackCount;
			line.JI_CustomsQuantity = messageDecoderLine.NetQuantity;
			line.QuarantineExDocLine.QL_NetQuantity = messageDecoderLine.NetQuantity;
			line.QuarantineExDocLine.QL_NetQuantityUnit = messageDecoderLine.NetQuantityUnit;
			line.QuarantineExDocLine.QL_ImperialNetWeight = messageDecoderLine.ImperialNetWeight;
			line.QuarantineExDocLine.QL_ImperialNetWeightUnit = messageDecoderLine.ImperialNetWeightUnit;
			SetGrossWeight(messageDecoderLine, line);
			line.QuarantineExDocLine.QL_GrossMetricWeightUnit = messageDecoderLine.MetricGrossWeightUnit;
			line.QuarantineExDocLine.QL_DrainedWeight = messageDecoderLine.DrainedWeight;
			line.QuarantineExDocLine.QL_DrainedWeightUnit = messageDecoderLine.DrainedWeightUnit;
			line.QuarantineExDocLine.QL_PercentOfMilkProtein = messageDecoderLine.PercentageOfMilkProtein;
			line.QuarantineExDocLine.QL_PercentOfMilkFat = messageDecoderLine.PercentageOfMilkFat;
			line.QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures = messageDecoderLine.TotalWeightOfMilkProteinInMixtures;
			line.QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures = messageDecoderLine.TotalWeightOfMilkFatInMixtures;
			line.QuarantineExDocLine.QL_BeefVealWeightAmount = messageDecoderLine.BeefVealWeight;
			line.QuarantineExDocLine.QL_ChemicalLeanPercentage = messageDecoderLine.ChecimalLeanPercentage;
			line.QuarantineExDocLine.QL_PreservationType = messageDecoderLine.PreservationType;
			line.QuarantineExDocLine.QL_ProductType = messageDecoderLine.ProductType;
			line.QuarantineExDocLine.QL_PackType = messageDecoderLine.PackType;
			line.QuarantineExDocLine.QL_SupplimentaryCode = messageDecoderLine.SupplimentaryCode;
			line.QuarantineExDocLine.QL_CutCode = messageDecoderLine.CutCode;
			if (!messageDecoderLine.ExporterDefinedProductDescription.IsEmpty)
			{
				line.JI_Description = messageDecoderLine.ExporterDefinedProductDescription;
			}

			line.QuarantineExDocLine.QL_MeatInspectionDescription = messageDecoderLine.LineItemDescription;
			line.QuarantineExDocLine.QL_AddtionalProductDescription = messageDecoderLine.AdditionalProductDescription;
			line.QuarantineExDocLine.QL_CommercialProductDescription = messageDecoderLine.CommercialProductDescription;
			line.QuarantineExDocLine.QL_ProductDescriptionQualityQualifier = messageDecoderLine.ProductQualityQualification;
			line.QuarantineExDocLine.QL_ProductDescriptionLocationQualifier = messageDecoderLine.ProductLocationQualification;
			line.QuarantineExDocLine.QL_NatureOfCommodity = messageDecoderLine.NatureOfCommodity;
			line.QuarantineExDocLine.QL_TreatmentType = messageDecoderLine.TreatmentType;
			line.QuarantineExDocLine.QL_BatchCode = messageDecoderLine.BatchNumber;
			line.QuarantineExDocLine.QL_LabelApprovalNumber = messageDecoderLine.LabelApprovalNumber;
			line.QuarantineExDocLine.QL_LabelApprovalIndicator = messageDecoderLine.LabelApprovalIndicator;
			line.QuarantineExDocLine.QL_UngradedProductIndicator = messageDecoderLine.UngradedProductIndicator;
			line.QuarantineExDocLine.QL_HalalProductIndicator = messageDecoderLine.HalalProductIndicator;
			line.QuarantineExDocLine.QL_FinalConsumer = messageDecoderLine.FinalConsumerIndicator;
			line.JI_Drawback = messageDecoderLine.DutyDrawback;
			line.JI_MotorVehiclePlan = messageDecoderLine.MotorVehiclePlan;
			line.JI_Texco = messageDecoderLine.Texaco;
			line.QuarantineExDocLine.QL_QuotaApprovalRef = messageDecoderLine.AMLCQuotaApproval;
			line.QuarantineExDocLine.QL_UseByStart = messageDecoderLine.DurabilityStartDate;
			line.QuarantineExDocLine.QL_UseByEnd = messageDecoderLine.DurabilityEndDate;
			line.QuarantineExDocLine.QL_SaltingDate = messageDecoderLine.SaltingDate;
			if (!messageDecoderLine.ProductSourceState.IsEmpty)
			{
				line.JI_AUState = messageDecoderLine.ProductSourceState;
			}

			line.QuarantineExDocLine.QL_AddtionalDeclarationComments = messageDecoderLine.AdditionalDeclarationText;
			line.QuarantineExDocLine.QL_StatementNumber1 = messageDecoderLine.CodedStatement1;
			line.QuarantineExDocLine.QL_StatementNumber2 = messageDecoderLine.CodedStatement2;
			line.QuarantineExDocLine.QL_StatementNumber3 = messageDecoderLine.CodedStatement3;
			line.QuarantineExDocLine.QL_StatementNumber4 = messageDecoderLine.CodedStatement4;
			line.QuarantineExDocLine.QL_StatementNumber5 = messageDecoderLine.CodedStatement5;
			line.QuarantineExDocLine.QL_StatementText = messageDecoderLine.FreeTextStatement;
			line.QuarantineExDocLine.QL_GrowerNumber = messageDecoderLine.GrowerNumber;
			line.QuarantineExDocLine.QL_ImportAuthorityCode = messageDecoderLine.ImportAuthorityCode;
			line.QuarantineExDocLine.QL_ClientLineItemID = messageDecoderLine.ClientLineItemID;

			if (!messageDecoderLine.FOBAmount.IsEmpty)
			{
				line.JI_LinePrice = messageDecoderLine.FOBAmount;
				totalFOBAmount += line.JI_LinePrice;
			}

			line.QuarantineExDocLine.QL_HCFormatRequested = messageDecoderLine.RequestedPrimaryCertificateTemplate;
			line.QuarantineExDocLine.QL_HCNumber = ToZStringWithDelimiterBetweenAppends(messageDecoderLine.AssignedCertificateNumbers, line.QuarantineExDocLine.QL_HCNumberInfo.MaxLength);
			line.QuarantineExDocLine.QL_HCFormatAllocated = ToZStringWithDelimiterBetweenAppends(messageDecoderLine.AssignedCertificateTemplates, line.QuarantineExDocLine.QL_HCFormatAllocatedInfo.MaxLength);
			if (messageDecoderLine.ExtraCertificates.Length > 0)
			{
				line.QuarantineExDocLine.QL_ExtraCertificate = ToZStringWithDelimiterBetweenAppends(messageDecoderLine.ExtraCertificates, line.QuarantineExDocLine.QL_ExtraCertificateInfo.MaxLength);
			}

			line.QuarantineExDocLine.QL_HealthCertificateDescription = messageDecoderLine.GeneratedProductDescription;
			line.JI_RelatedExportPermitNumber = messageDecoderLine.ExportPermitNumber;
			line.JI_RelatedExportPermitAuthority = messageDecoderLine.ExportPermitAuthority;
			line.JI_RelatedExportPermitDate = messageDecoderLine.ExportPermitDate;
			//AMLC Performance Exporter Number
			if (line.JI_LineNo <= messageDecoder.Permits.Count)
			{
				line.JI_TempImportNum = messageDecoder.Permits[line.JI_LineNo - 1].importLicenseNumber;
				line.JI_TempImportDate = messageDecoder.Permits[line.JI_LineNo - 1].importLicenseDate;
			}
			line.QuarantineExDocLine.QL_OuterPackType = messageDecoderLine.OuterPackType;
			line.QuarantineExDocLine.QL_OuterPackCount = messageDecoderLine.OuterPackCount;
			line.QuarantineExDocLine.QL_OuterPackAccuracy = messageDecoderLine.OuterPackAccuracy.IsEmpty ? (ZString)EXDOCPackAccuracyCodes.Codes.EqualTo : messageDecoderLine.OuterPackAccuracy;
			line.QuarantineExDocLine.QL_ShippingMarks = messageDecoderLine.ShippingMarks;
			line.QuarantineExDocLine.QL_OuterPackWeight = messageDecoderLine.OuterPackWeight;
			line.QuarantineExDocLine.QL_OuterPackWeightUnit = messageDecoderLine.OuterPackWeightUnit;
			line.QuarantineExDocLine.QL_IntermediatePackCount = messageDecoderLine.IntermediatePackCount;
			line.QuarantineExDocLine.QL_IntermediatePackType = messageDecoderLine.IntermediatePackType;
			line.QuarantineExDocLine.QL_IntermediatePackAccuracy = messageDecoderLine.IntermediatePackAccuracy;
			line.QuarantineExDocLine.QL_IntermediatePackWeight = messageDecoderLine.IntermediatePackWeight;
			line.QuarantineExDocLine.QL_IntermediatePackWeightUnit = messageDecoderLine.IntermediatePackWeightUnit;
			line.QuarantineExDocLine.QL_InnerPackCount = messageDecoderLine.InnerPackCount;
			line.QuarantineExDocLine.QL_InnerPackType = messageDecoderLine.InnerPackType;
			line.QuarantineExDocLine.QL_InnerPackAccuracy = messageDecoderLine.InnerPackAccuracy;
			line.QuarantineExDocLine.QL_InnerPackWeight = messageDecoderLine.InnerPackWeight;
			line.QuarantineExDocLine.QL_InnerPackWeightUnit = messageDecoderLine.InnerPackWeightUnit;
			line.QuarantineExDocLine.QL_AqisCustomsWeight = messageDecoderLine.CustomsWeight;
			line.QuarantineExDocLine.QL_AqisCustomsWeightUQ = messageDecoderLine.CustomsWeightUQ;
			line.QuarantineExDocLine.QL_DominantProduct = messageDecoderLine.DominantProduct;
			line.QuarantineExDocLine.QL_AdditionalProducts = messageDecoderLine.AdditionalProducts;
		}

		void SetGrossWeight(EXDOCMessageDecoderLine messageDecoderLine, JobComInvoiceLine line)
		{
			if (!messageDecoderLine.MetricGrossWeight.IsWithinSqlPrecisionAndScale(JobComInvoiceLineSchema.JI_Weight.Precision, JobComInvoiceLineSchema.JI_Weight.Scale))
			{
				if (messageDecoderLine.MetricGrossWeightUnit == EXDOCMetricWeightUnitCodes.Codes.Kilogram)
				{
					var value = (messageDecoderLine.MetricGrossWeight / 1000);
					if (value != line.JI_Weight)
					{
						var parsedValue = ZDecimal.ParseSafe(value.ToString(CultureInfo.CurrentCulture), 0m);
						line.QuarantineExDocLine.QL_GrossMetricWeight = parsedValue.Truncate(JobComInvoiceLineSchema.JI_Weight.Scale);
					}
					line.JI_WeightUQ = Core.Constants.Weight.Tonnes;
				}
				else
				{
					line.QuarantineExDocLine.QL_GrossMetricWeight = 0m;
					line.JI_WeightUQ = ZString.Empty;
				}
			}
			else
			{
				line.QuarantineExDocLine.QL_GrossMetricWeight = messageDecoderLine.MetricGrossWeight;
				line.JI_WeightUQ = new CustomsWeightUnitToRFPWeightUnit().GetCodeFromDescription(messageDecoderLine.MetricGrossWeightUnit);
			}
		}

		#endregion

		#region Ships Compartments Update

		void ShipsCompartmentsUpdate(JobComInvoiceHeader invoiceHeader)
		{
			if (messageDecoder.Compartments != null)
			{
				foreach (EXDOCMessageDecoderShipsCompartment messageCompartment in messageDecoder.Compartments)
				{
					QuarantineExDocShipsCompartment compartment = invoiceHeader.QuarantineExDocHeader.Compartments.FindByCompartmentNumber(messageCompartment.compartmentNumbers)
						?? invoiceHeader.QuarantineExDocHeader.Compartments.AddNew();

					compartment.QC_Compartments = messageCompartment.compartmentNumbers;
					compartment.QC_InspectionDate = messageCompartment.inspectionDate;
					compartment.QC_RL_NKInspectionPort = messageCompartment.InspectionPort;
				}
			}
		}

		#endregion

		ZDecimal totalFOBAmount;
		EDIMessage ediMessage;
		EXDOCMessageDecoder messageDecoder;
		QuarantineExDocHeader quarantineExDocHeader;
		protected EmailDef responseEmail = new EmailDef();
	}
}
