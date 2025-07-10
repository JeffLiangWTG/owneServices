using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCEventProcessor : IJobDeclarationEventProcessor
	{
		public NEXDOCEventProcessor()
		{
		}

		BusinessObjectFactory factory;
		IXmlImportLogger logger;
		JobDeclaration jobDeclaration;
		QuarantineExDocHeader exDocHeader;
		NEXDOCEventDecoder nexdocEventDecoder;

		ZString MostRecentLoggedError => logger.Logs.LastOrDefault(l => l.Type == LogType.Error)?.Message ?? ZString.Empty;

		public bool ProcessMessage(IXmlSessionTracker logger, IXmlEventValueObject eventData, IEDIMessage message, BusinessObject parent)
		{
			factory = parent?.Factory;
			this.logger = logger;

			bool processed = false;
			if (eventData.IsNEXDOCS())
			{
				if (eventData.IsNEXDOCSMessageReceived())
				{
					if (parent is QuarantineNexDocNotification notification)
					{
						ProcessNexDocNotification(eventData, notification);
					}
					else
					{
						jobDeclaration = parent as JobDeclaration;
						ProcessMessageReceived(eventData);
					}

					processed = true;
				}
				else if (eventData.IsNEXDOCSCertificatePrint())
				{
					processed = true;
				}
			}

			return processed;
		}

		void ProcessNexDocNotification(IXmlEventValueObject eventData, QuarantineNexDocNotification notification)
		{
			nexdocEventDecoder = new NEXDOCEventDecoder(eventData, logger);
			if (nexdocEventDecoder.Process())
			{
				switch (nexdocEventDecoder.MessageType)
				{
					case RFPMessageInterpretationGenerator.Constants.ReponseType.Error:
						{
							exDocHeader = jobDeclaration?.QuarantineInvoice?.QuarantineExDocHeader;

							if (exDocHeader != null)
							{
								exDocHeader.AddInfo.ZH_AmendmentResponseStatus = ZString.Empty;
							}

							notification.QN_AcknowledgeStatus = NEXDOCAcknowledgeStatus.Codes.Error;
							SendErrorReport(notification, CreateEmail(notification, nexdocEventDecoder.MessageType));

							break;
						}

					default:
						{
							SendAcknowledgementReport(notification, CreateEmail(notification, nexdocEventDecoder.MessageType));
							break;
						}
				}
			}
		}

		#region ProcessMessageReceived

		void ProcessMessageReceived(IXmlEventValueObject eventData)
		{
			var errorMessage = ZString.Empty;
			exDocHeader = jobDeclaration?.QuarantineInvoice?.QuarantineExDocHeader;
			if (exDocHeader == null)
			{
				errorMessage = jobDeclaration == null ? "Object to be processed is not a Declaration" : "Could not find Quarantine Header in Declaration";
				logger.LogBoth(LogType.Error, errorMessage);
			}
			else
			{
				nexdocEventDecoder = new NEXDOCEventDecoder(eventData, logger);
				if (nexdocEventDecoder.Process())
				{
					exDocHeader.AddInfo.ZH_AmendmentResponseStatus = ZString.Empty;
					switch (nexdocEventDecoder.MessageType)
					{
						case RFPMessageInterpretationGenerator.Constants.ReponseType.Success:
							{
								ProcessAccepted();
								jobDeclaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
								break;
							}

						case RFPMessageInterpretationGenerator.Constants.ReponseType.Notify:
							{
								ProcessNotification();
								if (jobDeclaration.JE_MessageStatus == RFPMessage.Status.AwaitingResponse)
								{
									jobDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
								}
								break;
							}

						case RFPMessageInterpretationGenerator.Constants.ReponseType.Receipt:
							{
								var nextPendingReissueMessage = exDocHeader.Messages
										.OfType<XmlEDIMessage>()
										.OrderBy(message => message.EM_SystemCreateTimeUtc)
										.FirstOrDefault
										(
											message => message.EM_MessageType == EDIMessageSubTypeList.Codes.XmlUniversalShipment
											&& message.EM_MessageSubType == NEXDOCMessageType.Codes.ReissueCertificate
											&& message.EM_Status == EDIMessage.Status.Pending
										);

								if (nextPendingReissueMessage != null && nextPendingReissueMessage.Interchange != null)
								{
									nextPendingReissueMessage.EM_Status = EDIMessage.Status.Sent;
									nextPendingReissueMessage.Interchange.EI_Status = EDIInterchange.Status.eHubQueued;
								}
								else if (jobDeclaration.JE_MessageStatus == RFPMessage.Status.AwaitingResponse)
								{
									jobDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
								}

								break;
							}

						case RFPMessageInterpretationGenerator.Constants.ReponseType.Replace:
							{
								if (jobDeclaration.JE_MessageStatus == RFPMessage.Status.AwaitingResponse)
								{
									jobDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
								}
								break;
							}

						default:
							{
								jobDeclaration.JE_MessageStatus = EDIMessage.Status.Rejected;
								break;
							}
					}

					if (nexdocEventDecoder.EmailNotificationRequired)
					{
						var responseEmail = CreateEmail(exDocHeader, nexdocEventDecoder.MessageType);
						if (nexdocEventDecoder.MessageType == RFPMessageInterpretationGenerator.Constants.ReponseType.Success || nexdocEventDecoder.MessageType == RFPMessageInterpretationGenerator.Constants.ReponseType.Notify)
						{
							SendAcknowledgementReport(exDocHeader, responseEmail);
						}
						else
						{
							SendImpedimentReport(exDocHeader, responseEmail);
						}
					}
				}
				else
				{
					errorMessage = MostRecentLoggedError;
				}
			}

			if (!errorMessage.IsEmpty)
			{
				SendErrorReport(null, CreateErrorEmail(errorMessage));
			}
		}

		void ProcessAccepted()
		{
			var customsAuthorityNumber = nexdocEventDecoder.CustomsAuthorityNumber;
			if (!customsAuthorityNumber.IsEmpty && (jobDeclaration.DeclarationNumber != customsAuthorityNumber || jobDeclaration.JE_EntryStatus != CustomsEntryStatus.Cancelled.Code))
			{
				jobDeclaration.DeclarationNumber = customsAuthorityNumber;
				jobDeclaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;

				ProcessOtherDeclarationsWithSameECNNumber(customsAuthorityNumber);
			}

			if (!nexdocEventDecoder.RexNumber.IsEmpty)
			{
				exDocHeader.QH_RequestForPermitNumber = nexdocEventDecoder.RexNumber;
			}

			if (!nexdocEventDecoder.ComplianceStatus.IsEmpty)
			{
				exDocHeader.RequestForPermitStatus = nexdocEventDecoder.ComplianceStatus.Left(3);
			}

			if (!nexdocEventDecoder.LastAmendDateTime.IsEmpty)
			{
				exDocHeader.QH_LastAmendDateTime = nexdocEventDecoder.LastAmendDateTime;
			}

			if (!nexdocEventDecoder.ExporterReference.IsEmpty)
			{
				exDocHeader.InvoiceHeader.JZ_ExporterReference = nexdocEventDecoder.ExporterReference.Left(JobComInvoiceHeader.Schema.JZ_ExporterReferenceMaxLength);
			}

			if (!nexdocEventDecoder.PermitNumber.IsEmpty && exDocHeader.QH_ExportPermitNumber.IsEmpty)
			{
				exDocHeader.QH_ExportPermitNumber = nexdocEventDecoder.PermitNumber.Left(QuarantineExDocHeader.Schema.QH_ExportPermitNumberMaxLength);
			}

			UpdateInvoiceLinesOnAccepted(nexdocEventDecoder.Lines);
		}

		void UpdateInvoiceLinesOnAccepted(NEXDOCEventLineDecoder[] eventLines)
		{
			foreach (JobComInvoiceLine invoiceLine in exDocHeader.InvoiceHeader.InvoiceLines)
			{
				var quarantineLine = invoiceLine.QuarantineExDocLine;
				if (quarantineLine != null)
				{
					quarantineLine.UpdateProcessesToLodged();

					var eventLine = eventLines.FirstOrDefault(x => x.LineNumber == invoiceLine.JI_LineNo);
					if (eventLine != null && !eventLine.PrimaryCertificateTemplateCode.IsEmpty)
					{
						quarantineLine.QL_HCFormatAllocated = eventLine.PrimaryCertificateTemplateCode;

						if (!eventLine.PrimaryCertificateEndorsementNumber.IsEmpty)
						{
							quarantineLine.QL_HCFormatAllocated += "/" + eventLine.PrimaryCertificateEndorsementNumber;
						}
					}
				}
			}
		}

		void ProcessNotification()
		{
			switch (nexdocEventDecoder.NotificationType)
			{
				case NEXDOCNotificationType.Codes.RexAtComp:
					exDocHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
					break;
				case NEXDOCNotificationType.Codes.RexCancellationApproved:
					exDocHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CancCancelled;
					break;
				case NEXDOCNotificationType.Codes.MessageFromIcs:
					{
						if (nexdocEventDecoder.NotificationText.Contains(CancelledNotificationText, StringComparison.InvariantCultureIgnoreCase))
						{
							jobDeclaration.JE_EntryStatus = Common.AU.CustomsEntryStatus.Cancelled.Code;
						}
					}
					break;
			}
		}

		const string CancelledNotificationText = "WITHDRAWN:THE EXPORT DECLARATION OR CARGO REPORT HAS BEEN WITHDRAWN";

		void ProcessOtherDeclarationsWithSameECNNumber(ZString number)
		{
			var entryNumberQuery = new ZQuery();
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, number);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, new[] { CANType.CustomsAuthorityNumber.Code });
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, jobDeclaration.TableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			var declarationPKs = factory.Load<AUCusEntryNumber>(entryNumberQuery)
				.Select(c => c.CE_ParentID)
				.Where(c => c != jobDeclaration.PK)
				.ToArray();

			if (declarationPKs.Any())
			{
				var declarationQuery = new ZQuery();
				declarationQuery.AddToFilter(JobDeclarationSchema.PK, declarationPKs);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_EntryStatus, SQLComparisonOperator.NotEqual, CustomsEntryStatus.Transferred.Code);
				var declarations = factory.Load<BaseJobDeclaration>(declarationQuery);
				declarations.ForEach(c => c.JE_EntryStatus = CustomsEntryStatus.Transferred.Code);
			}
		}

		#endregion

		#region MailHelper

		EmailDef CreateEmail(INEXDOCResponse response, string responseType)
		{
			HtmlTableCreator creator = null;

			var notices = nexdocEventDecoder.Notices;
			if (notices.Any())
			{
				creator = new HtmlTableCreator(new[] { "Message" });
				notices.ForEach(exdocNotice => creator.WriteRow(exdocNotice.narrativeMessage));
			}
			else if (nexdocEventDecoder.ValidationNotices.Any())
			{
				creator = new HtmlTableCreator(new[] { "Ln #", "Error", "Message" });
				nexdocEventDecoder.ValidationNotices.ForEach(exdocNotice => creator.WriteRow(exdocNotice.lineNumber, exdocNotice.errorMessageIdentifier, exdocNotice.narrativeMessage));
			}

			string emailTemplateHtml;
			using (var stream = GetType().Assembly.GetManifestResourceStream(response?.HtmlTemplatePath ?? "Enterprise.Customs.AU.Declaration.Business.Data.Xml.Universal.NEXDOC.HtmlTemplates.Response.html"))
			{
				using (var reader = new StreamReader(stream))
				{
					emailTemplateHtml = reader.ReadToEnd();
				}
			}

			var jobNumber = response?.JobNumber ?? ZString.Empty;
			emailTemplateHtml = emailTemplateHtml.Replace("{JOBNUMBER}", jobNumber);
			emailTemplateHtml = emailTemplateHtml.Replace("{REXNUMBER}", response?.RexNumber ?? ZString.Empty);
			emailTemplateHtml = emailTemplateHtml.Replace("{REXSTATUS}", response?.RexStatus ?? ZString.Empty);
			emailTemplateHtml = emailTemplateHtml.Replace("{EXPORTPERMITNUMBER}", response?.ExportPermitNumber ?? ZString.Empty);
			emailTemplateHtml = emailTemplateHtml.Replace("{CUSTOMSAUTHORITYNUMBER}", response?.CustomsAuthorityNumber ?? ZString.Empty);
			emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", creator != null ? creator.ToHtml() : string.Empty);

			string failureText = responseType == RFPMessageInterpretationGenerator.Constants.ReponseType.Success ? "Accepted" : responseType == RFPMessageInterpretationGenerator.Constants.ReponseType.Notify ? "Notification" : "Rejected";
			string subject = jobNumber.IsEmpty
				? "NEXDOCS Notification " + nexdocEventDecoder.NotificationType
				: string.Format(CultureInfo.InvariantCulture, "REX {0} for {1}", failureText, jobNumber);

			var emailSender = new HtmlNotificationEmailSender();
			return emailSender.CreateEmail(subject, emailTemplateHtml);
		}

		EmailDef CreateErrorEmail(ZString errorMessage)
		{
			return new EmailDef()
			{
				Subject = "ERROR PROCESSING:",
				Body = string.Concat("FATAL PROCESSING ERROR: ", errorMessage, System.Environment.NewLine, System.Environment.NewLine)
			};
		}

		void SendAcknowledgementReport(BusinessObject parent, EmailDef email)
		{
			MailHelper.SendReport(email, parent, AcknowledgementEmailMode, AcknowledgementEmailGroup);
		}

		void SendImpedimentReport(BusinessObject parent, EmailDef email)
		{
			MailHelper.SendReport(email, parent, ImpedimentEmailMode, ImpedimentEmailGroup);
		}

		void SendErrorReport(BusinessObject parent, EmailDef email)
		{
			MailHelper.SendReport(email, parent, ErrorEmailMode, ErrorEmailGroup);
		}

		MessageProcessorReportMailer MailHelper => mailHelper ?? (mailHelper = new MessageProcessorReportMailer(factory, logger));
		MessageProcessorReportMailer mailHelper;

		ZGuid AcknowledgementEmailGroup => AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.GetFallBackValueAtAllLevels(Company, Branch, Department);
		ZString AcknowledgementEmailMode => AUCustomsDataRegistry.Instance.SendAQISAcknowledgements.GetFallBackValueAtAllLevels(Company, Branch, Department);
		ZGuid ImpedimentEmailGroup => AUCustomsDataRegistry.Instance.SendAQISImpedimentsToGroup.GetFallBackValueAtAllLevels(Company, Branch, Department);
		ZString ImpedimentEmailMode => AUCustomsDataRegistry.Instance.SendAQISImpediments.GetFallBackValueAtAllLevels(Company, Branch, Department);
		ZGuid ErrorEmailGroup => AUCustomsDataRegistry.Instance.SendAQISErrorsToGroup.Value;
		ZString ErrorEmailMode => AUCustomsDataRegistry.Instance.SendAQISErrors.Value;

		Guid Company => (jobDeclaration?.Company ?? GlbCompany.CurrentCompany).PK.ToGuid();
		Guid Branch => (jobDeclaration?.Branch ?? GlbBranch.CurrentBranch).PK.ToGuid();
		Guid Department => Guid.Empty;

		#endregion
	}
}
