using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MimeKit;

[assembly: MailSubscriber(typeof(Enterprise.Customs.AU.Declaration.Business.AUCInterchangeRetriever))]
namespace Enterprise.Customs.AU.Declaration.Business
{
	public partial class AUCInterchangeRetriever : NewBaseInterchangeRetriever, ICustomsServiceTaskProcess
	{
		public AUCInterchangeRetriever() : base() { }

		#region	Implementation

		protected override bool OnlyCreateInterchanges => true;

		protected override Customs.Business.BatchProcessor.CertificateManager GetCertificateConfig(BusinessObjectFactory factory)
		{
			var companyCode = GlbCompany.CurrentCompany.GC_Code;
			if (!cachedCertificateManagers.TryGetValue(companyCode, out CertificateManager certificateManager))
			{
				certificateManager = new CertificateManager(factory);
				cachedCertificateManagers.Add(companyCode, certificateManager);
			}
			return certificateManager;
		}

		readonly Dictionary<ZString, CertificateManager> cachedCertificateManagers = new Dictionary<ZString, CertificateManager>();

		protected override IEnumerable<ZString> GetCompanyCodesFromMailItem(MailItem item)
		{
			var auCompanies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Australia, item.Factory);
			return auCompanies.Where(company => GetMailFilterForCompanies(new[] { company }).CanProcess(item)).Select(company => company.GC_Code);
		}

		protected override ZString GetInterchangeText(MailItem item, bool decryptInNewThread)
		{
			var result = ZString.Empty;
			var certificateManager = GetCertificateConfig(item.Factory);
			if (!ProcessAsPrint(item, decryptInNewThread, certificateManager))
			{
				var interchangeText = base.GetInterchangeText(item, decryptInNewThread);
				if (!AUBatchProcessorSupporter.IsATextMessage(interchangeText, Logger, item))
				{
					result = interchangeText;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool ProcessAsPrint(MailItem item, bool decryptInNewThread, Customs.Business.BatchProcessor.CertificateManager certificate)
		{
			var result = false;

			if (item != null && item.MI_Header.IndexOf("application/pkcs7-mime") > -1)
			{
				var mimeText = item.GetDecodedEmailText(certificate.CompanyCertificate, certificate.TrustPointCertificate, certificate.EncryptionCertificateName, decryptInNewThread);
				var mimeDocument = MimeMessageExtensions.CreateMessageFromEml(mimeText);
				foreach (var mimeEntity in mimeDocument.GetFullAttachments())
				{
					if (mimeEntity is MimePart potentialPDF &&
						potentialPDF.ContentTransferEncoding == ContentEncoding.Base64 &&
						potentialPDF.ContentType.MimeType == "application/pdf")
					{
						result = ProcessPDF(potentialPDF, item);
						if (result)
						{
							item.MI_Status = "PRS";
						}
						else if (item.HeaderReceivedTimeUtc.IsValid && item.HeaderReceivedTimeUtc < ZDateTime.UtcNow.AddHours(-25))
						{
							item.MI_Status = "FAL";
							var email = new EmailDef();
							email.Subject = "Unrecognized Inbound Email found by Australian Customs Interchange Retreiver service task";
							email.Body = @"An inbound email, with the following details, was not recognised by the Australian Customs Interchange Retreiver and so has been marked as FAILED.
Received Time: " + item.MI_ReceivedDateTime.ToLongTimeString() + @"
From: " + item.MI_From + @"
To: " + item.AllRecipients + @"
Subject: " + item.MI_Subject;
							Env.OutgoingCustomsMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
							Logger.LogWarning(email.Body);
						}

						result = true;
					}
				}
			}
			else if (item != null && item.MI_Subject.Contains(EXDOCRemotePrintFileSubject))
			{
				foreach (MailAttachment potentialRemotePrintFile in item.MailAttachments)
				{
					if (potentialRemotePrintFile != null &&
						potentialRemotePrintFile.MA_Data.Length > 3 &&
						potentialRemotePrintFile.MA_Data[0] == 'P' &&
						potentialRemotePrintFile.MA_Data[1] == 'R' &&
						potentialRemotePrintFile.MA_Data[2] == '2')
					{
						item.MI_Status = ProcessRemotePrintFile(potentialRemotePrintFile.MA_Data, item) ? "PRS" : "FAL";
						result = true;
					}
					else if (potentialRemotePrintFile != null &&
						potentialRemotePrintFile.MA_Data.Length > 4 &&
						potentialRemotePrintFile.MA_Data[0] == '%' &&
						potentialRemotePrintFile.MA_Data[1] == 'P' &&
						potentialRemotePrintFile.MA_Data[2] == 'R' &&
						potentialRemotePrintFile.MA_Data[3] == '2')
					{
						item.MI_Status = ProcessRemotePrintFile(potentialRemotePrintFile.MA_Data, item, true) ? "PRS" : "FAL";
						result = true;
					}
				}

				if (!result)
				{
					Logger.LogWarning("The inbound EXDOC Remote Print Document email does not appear to contain a valid EXDOC Print. It will be processed as a standard interchange, which will probably fail (see later entries in log).");
				}
			}

			return result;
		}

		#region Process Remote Print File

		bool ProcessRemotePrintFile(ZBlob remotePrintFileData, MailItem item, bool isPdf = false)
		{
			var result = false;
			(var messageIdentifier, var exporterReference, var entryNumber, var rpfFileName) = GetDeclarationInformationFromFirstLine(remotePrintFileData.ToAscii(), isPdf);
			if (!entryNumber.IsEmpty && !exporterReference.IsEmpty)
			{
				var quarantineExDocHeader = GetExDocHeaderFromExporterReference(item.Factory, exporterReference) ?? GetExDocHeaderFromEntryNumber(item.Factory, entryNumber);
				if (quarantineExDocHeader != null)
				{
					result = true;
					SendRemotePrintFileAcknowledgementMessage(quarantineExDocHeader, messageIdentifier);
					IDocManagerSupport entityToSave = quarantineExDocHeader.Declaration;
					var contentStartIndex = isPdf
						? remotePrintFileData.IndexOf(new ASCIIEncoding().GetBytes("%PDF-"))
						: remotePrintFileData.IndexOf(new ASCIIEncoding().GetBytes(System.Environment.NewLine)) + 2;
					entityToSave.DocManagerInfo.AddFileOrDocument(remotePrintFileData.SubBlobSafe(contentStartIndex), rpfFileName + (isPdf ? ".PDF" : ".PCL"), Core.Constants.RefDocTypes.QuarantineRemotePrint, false);
					entityToSave.DocManagerInfo.Save();
					var email = new EmailDef
					{
						Subject = string.Format(CultureInfo.InvariantCulture, "RFP Quarantine Remote Print receipt notification for {0}", entryNumber),
						Body = "A Quarantine Remote Print was received." + System.Environment.NewLine + System.Environment.NewLine +
							string.Format(CultureInfo.InvariantCulture, "Job Number:                 {0}", entryNumber) + System.Environment.NewLine +
							string.Format(CultureInfo.InvariantCulture, "Receipt Date/Time:          {0}", ZDateTime.Now) + System.Environment.NewLine +
							string.Format(CultureInfo.InvariantCulture, "Message Identifier:         {0}", messageIdentifier) + System.Environment.NewLine + System.Environment.NewLine +
								"This RFP Certificate can be found and printed via the eDoc associated to the Customs Declaration (" + quarantineExDocHeader.Declaration.JobNumber + ")."
					};

					Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(email);
					Logger.Log(email.Subject);
				}
				else
				{
					result = true;
					var declaration = CreateDeclaration(item, exporterReference);
					IDocManagerSupport entityToSave = declaration;
					var contentStartIndex = isPdf
						? remotePrintFileData.IndexOf(new ASCIIEncoding().GetBytes("%PDF-"))
						: remotePrintFileData.IndexOf(new ASCIIEncoding().GetBytes(System.Environment.NewLine)) + 2;
					entityToSave.DocManagerInfo.AddFileOrDocument(remotePrintFileData.SubBlobSafe(contentStartIndex), rpfFileName + (isPdf ? ".PDF" : ".PCL"), Core.Constants.RefDocTypes.QuarantineRemotePrint, false);
					entityToSave.DocManagerInfo.Save();
					var email = new EmailDef
					{
						Subject = string.Format(CultureInfo.InvariantCulture, "RFP Quarantine Remote Print receipt notification for Entry Number {0} / Exporter Reference {1}", entryNumber, exporterReference),
						Body = "A Quarantine Remote Print has been received. The inbound EXDOC Remote Print File could not be linked to an RFP job. A new stand-alone Brokerage job has been created for this RFP." + System.Environment.NewLine + System.Environment.NewLine +
							string.Format(CultureInfo.InvariantCulture, "Receipt Date/Time:          {0}", ZDateTime.Now) + System.Environment.NewLine +
							string.Format(CultureInfo.InvariantCulture, "Message Identifier:         {0}", messageIdentifier) + System.Environment.NewLine +
							string.Format(CultureInfo.InvariantCulture, "Exporter Reference:         {0}", exporterReference) + System.Environment.NewLine + System.Environment.NewLine +
								"This RFP Certificate can be found and printed via the eDoc associated to the new Customs Declaration (" + declaration.JobNumber + ")."
					};

					var emailGroupPK = AcknowledgementEmailGroup(declaration);
					if (declaration.Factory.Exists(typeof(GlbGroup), new ZQuery(GlbGroupSchema.PK, emailGroupPK)))
					{
						Env.OutgoingCustomsMailManager.CreateAndSave(email, emailGroupPK, GroupSourceLocator.GetFromRegistryItem(AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup));
					}
					else
					{
						Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(email);
					}

					Logger.Log(email.Subject);
				}
			}

			return result;
		}

		protected Guid AcknowledgementEmailGroup(JobDeclaration declaration)
		{
			return AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.GetFallBackValueAtAllLevels(declaration.Company.PK.ToGuid(), declaration.Branch.PK.ToGuid(), Guid.Empty);
		}

		protected ZString AcknowledgementEmailMode(JobDeclaration declaration)
		{
			return AUCustomsDataRegistry.Instance.SendAQISAcknowledgements.GetFallBackValueAtAllLevels(declaration.Company.PK.ToGuid(), declaration.Branch.PK.ToGuid(), Guid.Empty);
		}

		(ZString messageIdentifier, ZString exporterReference, ZString entryNumber, ZString rpfFileName) GetDeclarationInformationFromFirstLine(ZString remotePrintFileData, bool isPdf = false)
		{
			var messageIdentifier = ZString.Empty;
			var exporterReference = ZString.Empty;
			var entryNumber = ZString.Empty;
			var rpfFileName = ZString.Empty;
			var startIndex = isPdf ? 4 : 3;
			var contextInformation = remotePrintFileData.SubstringSafe(startIndex, remotePrintFileData.IndexOf(isPdf ? "%PDF-" : System.Environment.NewLine, StringComparison.OrdinalIgnoreCase) - startIndex);
			var contextInformationCollection = contextInformation.Split(new[] { ',' });
			messageIdentifier = contextInformationCollection[0].Left(10).Trim();
			exporterReference = contextInformationCollection[0].SubstringSafe(22);
			entryNumber = contextInformationCollection[2].KeepNumericCharacters();
			rpfFileName = contextInformationCollection[4].SubstringSafe(10);
			return (messageIdentifier, exporterReference, entryNumber, rpfFileName);
		}

		QuarantineExDocHeader GetExDocHeaderFromExporterReference(BusinessObjectFactory factory, ZString exporterReference)
		{
			var declaration = factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, exporterReference))
							  ?? factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, exporterReference));

			return declaration?.QuarantineInvoice?.QuarantineExDocHeader;
		}

		QuarantineExDocHeader GetExDocHeaderFromEntryNumber(BusinessObjectFactory factory, ZString entryNumber)
		{
			QuarantineExDocHeader result = null;

			var filter = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);

			var cusEntryNumber = factory.LoadTop1<CusEntryNumber>(filter);
			if (cusEntryNumber != null)
			{
				result = factory.Load<QuarantineExDocHeader>(cusEntryNumber.CE_ParentID);
			}

			return result;
		}

		void SendRemotePrintFileAcknowledgementMessage(QuarantineExDocHeader quarantineExDocHeader, ZString messageIdentifier)
		{
			var builder = new EXDOCRemotePrintAcknowledgmentMessageBuilder(quarantineExDocHeader, messageIdentifier);
			var message = builder.GenerateMessage();
			try
			{
				message.Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Logger.LogWarning(string.Format("Send Remote Print File Acknowledgement Message save failed for job: {0}. Exception: {1}", quarantineExDocHeader.Declaration.JE_DeclarationReference, ex.Message));
			}
		}

		#endregion

		bool ProcessPDF(MimePart pdf, MailItem item)
		{
			var result = false;
			(var entryNumber, var pdfFileName, var mailType) = GetPDFInformation(item.MI_Subject, pdf.GetSafeFileName());
			if (entryNumber.IsEmpty)
			{
				if (ShouldSendUnmatchedICSReports)
				{
					//Unmatched ICS pdf reports need to be sent to the relevant group set in registry
					//Doc may be 'Need to Produce Docs' pdf, which can be received for both local declarations, (which should be matched by Entry Number), and for third party clearances (refunds), so these need to be sent to the relevant group set in the registry
					var email = new EmailDef();
					email.Subject = item.MI_Subject;
					email.Attachments.Add(new AttachmentDef(pdfFileName, pdf.GetData()));
					email.Body = mailType == NPD ? "The attached NPD document could not be matched to any local declaration." : "The attached ICS document has been received unsolicted from Customs.";
					Env.OutgoingCustomsMailManager.CreateAndSave(email, SendUnmatchedICSReportsGroup.ToGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
					result = true;
				}
			}
			else
			{
				var entryHeader = new Customs.Business.CusEntryHeader.Loader(item.Factory).FindByEntryNumberAndCurrentCompany(entryNumber);
				if (entryHeader != null)
				{
					result = true;
					if (entryHeader.Declaration != null)
					{
						IDocManagerSupport entityToSave = entryHeader.Declaration.Shipment != null ? entryHeader.Declaration.Shipment : entryHeader.Declaration;
						var docType = mailType == NPD ? "CAU" : "EPR";
						var isATD = mailType == ATD;
						entityToSave.DocManagerInfo.AddFileOrDocument(pdf.GetData(), pdfFileName, docType, isATD);
						entityToSave.DocManagerInfo.Save();
					}
				}
				else if (ShouldSendUnmatchedICSReports)
				{
					//Unmatched ICS pdf reports need to be sent to the relevant group set in registry
					//Doc may be 'Need to Produce Docs' pdf, which can be received for both local declarations, (which should be matched by Entry Number), and for third party clearances (refunds), so these need to be sent to the relevant group set in the registry
					var email = new EmailDef();
					email.Subject = item.MI_Subject;
					email.Attachments.Add(new AttachmentDef(pdfFileName, pdf.GetData()));
					email.Body = mailType == NPD ? "The attached NPD document could not be matched to any local declaration." : "The attached ICS document has been received unsolicted from Customs.";
					Env.OutgoingCustomsMailManager.CreateAndSave(email, SendUnmatchedICSReportsGroup.ToGuid(), GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
					result = true;
				}
				else if (mailType != PYR || (item.HeaderReceivedTimeUtc.IsValid && item.HeaderReceivedTimeUtc < ZDateTime.UtcNow.AddDays(-1)))
				{
					//The 1 day delay is to cater for the case where a Lodge and Pay is submitted and it is possible for the Payment
					//Receipt PDF to be processed before the declaration response which has the Entry Number used for matching.
					var email = new EmailDef();
					email.Subject = item.MI_Subject;
					email.Attachments.Add(new AttachmentDef(pdfFileName, pdf.GetData()));
					email.Body = "The attached document could not be matched, and so could not be attached to any known declaration.";
					Env.OutgoingCustomsMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
					result = true;
				}
			}
			return result;
		}

		protected override bool ShouldIgnoreEmptyInterchangeText(MailItem item)
		{
			return item.MI_Subject.Contains(AUCUpdatedPDFSubject) ||
				item.MI_Subject.Contains(AUCustomsPDFSubject) ||
				item.MI_Subject.Contains(EXDOCRemotePrintFileSubject);
		}

		const string PYR = "PYR";
		const string ATD = "ATD";
		const string FID = "FID";
		const string NPD = "NPD";

		(ZString entryNumber, ZString pdfFileName, string mailType) GetPDFInformation(string mailItemSubject, string originalPdfFileName)
		{
			var entryNumber = ZString.Empty;
			var pdfFileName = ZString.Empty;
			var mailType = "";
			var pyrIndex = mailItemSubject.IndexOf(" " + PYR + " ");
			var atdIndex = mailItemSubject.IndexOf(" " + ATD + " ");
			var fidIndex = mailItemSubject.IndexOf(" " + FID + " ");
			var npdIndex = mailItemSubject.IndexOf(" " + NPD + " ");
			if (pyrIndex > -1)
			{
				entryNumber = mailItemSubject.Substring(pyrIndex + 13, 9);
				pdfFileName = "Payment Receipt for " + entryNumber + ".pdf";
				mailType = PYR;
			}
			else if (atdIndex > -1)
			{
				entryNumber = mailItemSubject.Substring(atdIndex + 13, 9);
				pdfFileName = "Authority To Deal for " + entryNumber + ".pdf";
				mailType = ATD;
			}
			else if (fidIndex > -1)
			{
				entryNumber = mailItemSubject.Substring(fidIndex + 9, 9);
				pdfFileName = "Formal Import Declaration for " + entryNumber + ".pdf";
				mailType = FID;
			}
			else if (npdIndex > -1)
			{
				entryNumber = mailItemSubject.Substring(npdIndex + 9, 9);
				pdfFileName = "Need to Produce Documents for " + entryNumber + ".pdf";
				mailType = NPD;
			}
			else
			{
				pdfFileName = originalPdfFileName;
			}
			return (entryNumber, pdfFileName, mailType);
		}

		protected ZGuid SendUnmatchedICSReportsGroup
		{
			get { return AUCustomsDataRegistry.Instance.SendUnmatchedICSReportsToGroup.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty); }
		}

		protected ZString SendUnmatchedICSReports
		{
			get { return AUCustomsDataRegistry.Instance.SendUnmatchedICSReports.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty); }
		}

		protected bool ShouldSendUnmatchedICSReports
		{
			get { return SendUnmatchedICSReports != Core.Constants.EmailTo.NoEmails && SendUnmatchedICSReportsGroup.IsValid; }
		}

		protected override bool SendCryptoExceptionToPostMaster
		{
			get { return true; }
		}

		protected override string CryptoExceptionToPostMasterText
		{
			get
			{
				return @"An error has occurred while trying to decrypt/decode a message; please see technical details below.
This is most likely caused by a spam/virus filter or digital certificate problem.
Please refer to the following document:

" + CMRMessage.MessagingHelpUpdateNoteURL + @"

The message has not been processed.

	";
			}
		}

		protected override void LogWarningMessageForMissingCompanyCertificate(ZString companyCode)
		{
			Logger.LogWarning(string.Format(CultureInfo.InvariantCulture, "The 'Company Key File' is missing for company {0}, please check it in Registry '{1}'", companyCode, Env.Registry.RawRegistry.AUCCompanyCertificateData.GetLocation()));
		}

		protected override bool NeedCompanyCertificate(MailItem mailItem)
		{
			return (mailItem.MI_Header.IndexOf("smime.p7m") != -1) || (mailItem.MI_Header.IndexOf("application/pkcs7-mime") != -1);
		}

		protected override List<ZString> GetAttachmentExtensionsToLookFor()
		{
			var result = base.GetAttachmentExtensionsToLookFor();
			result.Add(".pra");
			return result;
		}

		protected override EDIInterchange ProcessDuplicatedInterchange(EDIInterchange newInterchange, EDIInterchange existingInterchange)
		{
			EDIInterchange result;
			if (newInterchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.EXDOC)
			{
				newInterchange.EI_InterchangeNum += ZDateTime.Now.ToString("yyyyMMddHHmmss");
				result = newInterchange;
			}
			else
			{
				result = base.ProcessDuplicatedInterchange(newInterchange, existingInterchange);
			}

			return result;
		}

		protected override void CreateAcknowledgementMessage(EDIInterchange interchangeToAcknowledge)
		{
			AUBatchProcessorSupporter.CreateCMRAcknowledgementMessage(interchangeToAcknowledge);
		}

		#region Mail Filters

		protected override IMailFilter GetMailFilter() => GetMailFilterForCompanies(new[] { GlbCompany.CurrentCompany });

		[MailFilter(MailFilterCodes.AUCInterchange)]
		public static IMailFilter BuildMailFilter()
		{
			return BuildMailFilter(GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Australia));
		}

		public static IMailFilter BuildMailFilter(IEnumerable<GlbCompany> companies)
		{
			var result = new ZQuery()
				.AddToFilter(IncomingEX1EmailFilter)
				.AddToFilter(IncomingCMREmailFilter, JoinCondition.Or)
				.AddToFilter(IncomingOneStopEmailFilter, JoinCondition.Or)
				.AddToFilter(IncomingExdocTestEmailFilter, JoinCondition.Or)
				.AddToFilter(IncomingExdocLiveEmailFilter, JoinCondition.Or);

			result.AddToFilter(GetAdditionalEDIMessageSenderFilter(companies), JoinCondition.Or);

			result
				.AddToFilter(BuildSubjectsQuery(GetAllMailBoxes(companies)))
				.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued)
				.AddToFilter(MailDBItemsSchema.MI_Direction, "RCV");

			return new QueryMailFilter(MailFilterCodes.AUCInterchange, result, alsoApplyQuery: false);
		}

		IMailFilter GetMailFilterForCompanies(IEnumerable<GlbCompany> companies)
		{
			if (companies.Count() == 1)
			{
				var comanyCode = companies.First().GC_Code;
				if (!cachedMailFilters.TryGetValue(comanyCode, out IMailFilter mailFilter))
				{
					mailFilter = BuildMailFilter(companies);
					cachedMailFilters.Add(comanyCode, mailFilter);
				}
				return mailFilter;
			}
			else
			{
				return BuildMailFilter(companies);
			}
		}

		readonly Dictionary<ZString, IMailFilter> cachedMailFilters = new Dictionary<ZString, IMailFilter>();

		static ZQuery GetAdditionalEDIMessageSenderFilter(IEnumerable<GlbCompany> companies)
		{
			return GetMI_FromFiltersOfCompanyLevelRegistry(companies, company => AUCustomsDataRegistry.Instance.AdditionalEDIMessageSender.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		static ZQuery GetMI_FromFiltersOfCompanyLevelRegistry(IEnumerable<GlbCompany> companies, Func<GlbCompany, ZString> getRegistry)
		{
			var query = new ZQuery();

			var addiEdiSenders = companies.Select(company => getRegistry(company)).Where(val => !val.IsEmpty).Distinct();
			query.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_From, addiEdiSenders);

			return query;
		}

		static ZQuery IncomingCMREmailFilter
		{
			get
			{
				var configHelper = SysConfigHelper.Instance;
				return new ZQuery(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, configHelper.AUCCustomsCCFPreviousEmailAddress)
					.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, configHelper.AUCCustomsCCFCurrentEmailAddress);
			}
		}

		static ZQuery IncomingEX1EmailFilter
			=> new ZQuery(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, "@" + MessagingConstants.eRouterACSServerName)
					.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.NotContains, "MDaemon@" + MessagingConstants.eRouterACSServerName);

		static ZQuery IncomingOneStopEmailFilter
			=> new ZQuery(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, MessagingConstants.eRouterPRAEmailAddress);

		static ZQuery IncomingExdocTestEmailFilter
			=> new ZQuery(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, AUCustomsDataRegistry.GetEXDOCTestEmailAddress());

		static ZQuery IncomingExdocLiveEmailFilter
			=> new ZQuery(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, AUCustomsDataRegistry.GetEXDOCProdEmailAddress());

		const string AUCustomsPDFSubject = "COMMERCIAL-IN-CONFIDENCE:";
		const string AUCUpdatedPDFSubject = "OFFICIAL: Sensitive:";
		internal const string EXDOCRemotePrintFileSubject = "EXDOC - Remote Print Certificate";

		static string[] GetAllMailBoxes(IEnumerable<GlbCompany> companies)
		{
			var result = new HashSet<string>();
			result.Add(AUCUpdatedPDFSubject);
			result.Add(AUCustomsPDFSubject);
			result.Add(EXDOCRemotePrintFileSubject);
			result.Add("EXDOC EDI");
			result.Add(PRAMailBox);

			if (companies != null)
			{
				foreach (var company in companies)
				{
					foreach (var branch in company.ActiveBranches)
					{
						var customsSenderID = Env.Registry.GetAUCustomsSenderID(company.PK.ToGuid(), branch.PK.ToGuid());
						if (!string.IsNullOrEmpty(customsSenderID))
						{
							result.Add(customsSenderID);
						}

						var edificeSenderID = Env.Registry.GetAUCustomsEdificeSenderID(company.PK.ToGuid(), branch.PK.ToGuid());
						if (!string.IsNullOrEmpty(edificeSenderID))
						{
							result.Add(edificeSenderID);
						}

						var seaCargoDepotMailbox = Env.Registry.GetAUCustomsSeaCargoDepotMailbox(company.PK.ToGuid(), branch.PK.ToGuid());
						if (!string.IsNullOrEmpty(seaCargoDepotMailbox))
						{
							result.Add(seaCargoDepotMailbox);
						}
					}

					if (!company.GC_CustomsRegistrationNo.IsEmpty)
					{
						result.Add(company.GC_CustomsRegistrationNo);
					}

					var companyCustomsSenderID = Env.Registry.GetAUCustomsSenderID(company.PK.ToGuid(), Guid.Empty);
					if (!string.IsNullOrEmpty(companyCustomsSenderID))
					{
						result.Add(companyCustomsSenderID);
					}

					var companyEdificeSenderID = Env.Registry.GetAUCustomsEdificeSenderID(company.PK.ToGuid(), Guid.Empty);
					if (!string.IsNullOrEmpty(companyEdificeSenderID))
					{
						result.Add(companyEdificeSenderID);
					}

					var companySeaCargoDepotMailbox = Env.Registry.GetAUCustomsSeaCargoDepotMailbox(company.PK.ToGuid(), Guid.Empty);
					if (!string.IsNullOrEmpty(companySeaCargoDepotMailbox))
					{
						result.Add(companySeaCargoDepotMailbox);
					}
				}
			}

			return result.ToArray();
		}

		internal static ZString PRAMailBox
		{
			get
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				return registrationKey.EnterpriseCode + registrationKey.ServerCode;
			}
		}

		#endregion

		public override void Dispose()
		{
			base.Dispose();
			cachedCertificateManagers.Values.ForEach(x => x.Dispose());
		}

		#region Create Declaration

		JobDeclaration CreateDeclaration(MailItem item, ZString exporterReference)
		{
			var defaultBranch = item.Factory.Load<GlbBranch>(AUCustomsDataRegistry.Instance.DefaultBranchForTransferIn.Value);
			if (defaultBranch == null || defaultBranch.Country.IsNull || defaultBranch.Country.Code != Core.Constants.CountryCodes.Australia)
			{
				return CreateDeclarationCore(item, exporterReference);
			}
			else
			{
				using (DisposableEnvironment.ForBranch(defaultBranch.PK.ToGuid()))
				{
					return CreateDeclarationCore(item, exporterReference);
				}
			}
		}

		JobDeclaration CreateDeclarationCore(MailItem item, ZString exporterReference)
		{
			var declaration = item.Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			declaration.JE_OwnerRef = exporterReference;
			declaration.Logs.AddNew(Events.StatusChange, "AQSDec#: " + declaration.DeclarationNumber);
			item.Factory.Save();
			return declaration;
		}

		#endregion

		#endregion
	}
}
