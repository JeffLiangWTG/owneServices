using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.PrintProcessing.Mailer.Email;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.PrintProcessing
{
	class EmailProcessor : MergedPrintGroupProcessor
	{
		public EmailProcessor(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate logProgress)
			: base(mergedPrintGroup)
		{
			LogProgress = logProgress;
		}

		protected override StmPrintJobGroupCollection PrintJobGroups
		{
			get { return (MergedPrintGroup.SizeInKB < AttachmentSizeLimitInKB) ? base.PrintJobGroups : SplitGroupIntoMultipleEmails(); }
		}

		protected override void PreProcess(StmPrintJobMergedCollection printJobs, int groupNumber, int totalGroupCount)
		{
			base.PreProcess(printJobs, groupNumber, totalGroupCount);
			attachmentPaths = new List<string>();
			attachmentPathsThatNeedMerging = new List<string>();
		}

		void ProcessAttachmentPathsThatNeedMerging(AttachmentDefCollection emailAttachmentList, StmPrintJob firstPrintJob, Action<AttachmentDef> processMergedAttachment, out bool hasInvalidPDF)
		{
			hasInvalidPDF = false;
			var listInvalidPDF = new List<string>();
			AttachmentDef mergedAttachment = null;
			if (attachmentPathsThatNeedMerging.Count > 0)
			{
				mergedAttachment = GetMergedPdfAttachment(firstPrintJob, listInvalidPDF);
			}

			processMergedAttachment.Invoke(mergedAttachment);

			if (listInvalidPDF.Count > 0)
			{
				var errorMsg = Res.GetString("ddfc7201-6e56-40dd-95d1-0872b1b5489f", "The PDF file(s) mentioned below are invalid and therefore have been separately attached.");
				foreach (var invalidPDF in listInvalidPDF)
				{
					emailAttachmentList.Add(new AttachmentDef(invalidPDF));
					errorMsg += System.Environment.NewLine + Path.GetFileName(invalidPDF);
				}

				LogProgress(TraceEventType.Warning, errorMsg);
				hasInvalidPDF = true;
			}
		}

		protected override void PostProcess(StmPrintJobMergedCollection printJobs, int groupNumber, int totalGroupCount)
		{
			base.PostProcess(printJobs, groupNumber, totalGroupCount);

			var emailAttachmentList = new AttachmentDefCollection();
			var firstPrintJob = printJobs[0];
			var htmlDocument = string.Empty;
			var hasInvalidPDF = false;

			if (printJobs.DeliveryGroup.SB_IsZippedDocPack)
			{
				using (var stream = new MemoryStream())
				{
					var creator = new ZipCreator();
					var files = new List<ZipStream>();

					foreach (var attachmentPath in attachmentPaths)
					{
						var entryDisplayName = new FileInfo(attachmentPath).Name;
						files.Add(new ZipStream(entryDisplayName, new FileStream(attachmentPath, FileMode.Open)));
					}

					ProcessAttachmentPathsThatNeedMerging(emailAttachmentList, firstPrintJob, mergedAttachment =>
					{
						if (mergedAttachment != null)
						{
							files.Add(new ZipStream(mergedAttachment.DisplayName, new MemoryStream(mergedAttachment.Data)));
						}

						creator.ZipStream(files, stream);

						var zipDisplayName = string.Format((NoResString)"DocPack_{0}.zip", ZDateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));  // This is a filename
						emailAttachmentList.Add(new AttachmentDef(zipDisplayName, stream.ToArray()));
					}, out hasInvalidPDF);
				}
			}
			else
			{
				foreach (var attachmentPath in attachmentPaths)
				{
					if (Path.GetExtension(attachmentPath).Equals(".HTML", StringComparison.OrdinalIgnoreCase))
					{
						var attachmentData = File.ReadAllText(attachmentPath);
						if (DocumentConverter.IsHTMLFlexCelConversion(attachmentData))
						{
							htmlDocument = DocumentConverter.PostProcessHTMLForEmail(attachmentData);
							var imagesDirectoryName = DocumentConverter.GetSafeImagesDirectoryName(attachmentPath);
							var imagesDirectoryPath = Path.Combine(Path.GetDirectoryName(attachmentPath), imagesDirectoryName);

							if (Directory.Exists(imagesDirectoryPath))
							{
								foreach (var file in Directory.EnumerateFiles(imagesDirectoryPath))
								{
									emailAttachmentList.Add(new AttachmentDef(file));
								}
							}
						}
						else
						{
							emailAttachmentList.Add(new AttachmentDef(attachmentPath));
						}
					}
					else
					{
						emailAttachmentList.Add(new AttachmentDef(attachmentPath));
					}
				}

				ProcessAttachmentPathsThatNeedMerging(emailAttachmentList, firstPrintJob, mergedAttachment =>
				{
					if (mergedAttachment != null)
					{
						emailAttachmentList.Add(mergedAttachment);
					}
				}, out hasInvalidPDF);
			}

			var eDocsAttachmentList = AllocatedPrintJobToEDoc(emailAttachmentList, firstPrintJob, printJobs);
			var emailDoc = new EmailDocument(emailAttachmentList, firstPrintJob.Staff);
			var prefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(firstPrintJob.SP_ParentTableName);

			if (!string.IsNullOrEmpty(prefix) && firstPrintJob.SP_ParentGuid.IsValid)
			{
				emailDoc.fEmailDef.SetupBusinessEntityInfo(firstPrintJob.SP_ParentGuid, prefix, string.Empty);
			}

			emailDoc.DocumentName = firstPrintJob.SP_DocumentName;
			emailDoc.Recipients = GetCopyRecipientEmailAddresses(firstPrintJob.EmailToRecipients);
			emailDoc.CCRecipients = GetCopyRecipientEmailAddresses(firstPrintJob.CarbonCopyRecipients);
			emailDoc.BCCRecipients = GetCopyRecipientEmailAddresses(firstPrintJob.BlindCarbonCopyRecipients);
			emailDoc.Subject = (firstPrintJob.DeliveryGroup.SB_EmailSubjectLine.IsEmpty ? firstPrintJob.SP_EmailSubjectLine : firstPrintJob.DeliveryGroup.SB_EmailSubjectLine);
			emailDoc.Subject += (groupNumber != 0 && totalGroupCount != 0) ? " " + string.Format((NoResString)"({0} of {1})", groupNumber.ToString(), totalGroupCount.ToString()) : "";  // too specfic to convert
			emailDoc.Subject = emailDoc.Subject.Length > 0 ? emailDoc.Subject : Res.GetString("1c691c91-4a72-467d-bdf3-130302ac3647", "No Subject");
			emailDoc.ContentType = EmailContentTypes.HTML;
			var eDocsAttachmentLinkBody = string.Empty;
			if (emailAttachmentList.Count == 0 && eDocsAttachmentList.Count > 0)
			{
				eDocsAttachmentLinkBody = $"{Res.GetString("BE8956F9-65D6-427A-92F6-448E41AF43A6", "The report exceeded the maximum attachment size allowed and could not be emailed. Please follow the applicable link to download the report")}: <br /><br />";
				eDocsAttachmentLinkBody += $"{Res.GetString("845B0EB2-4247-46EA-AE2D-189EAAEEE86E", "If you are an internal staff member, download the report via this link and authenticate with your {0} login credentials", Core.Constants.ProductName)}: " + string.Join("", eDocsAttachmentList.Select(x => $"<div><a href='{x.Item2}'>{x.Item1}</a></div>")) + "<br />";
				eDocsAttachmentLinkBody += $"{Res.GetString("5B761F4F-98DB-4D71-9F85-01AA39372038", "If you are a web portal user, to download the report, click this link and authenticate with your web portal login credentials")}: " + string.Join("", eDocsAttachmentList.Select(x => $"<div><a href='{x.Item3}'>{x.Item1}</a></div>"));
			}
			if (!string.IsNullOrWhiteSpace(htmlDocument))
			{
				emailDoc.Body = htmlDocument.Replace("</body>", $"<br />{EmailBody.Body(firstPrintJob, emailAttachmentList, true, hasInvalidPDF, eDocsAttachmentLinkBody)}</body>");
			}
			else
			{
				emailDoc.Body = EmailBody.Body(firstPrintJob, emailAttachmentList, true, hasInvalidPDF, eDocsAttachmentLinkBody);
			}

			if (!firstPrintJob.SP_EmailFromAddress.IsEmpty)
			{
				var secondarySmtpServer = SmtpConfiguration.GetSecondarySmtpServer(firstPrintJob.SP_EmailFromAddress);
				if (secondarySmtpServer != null)
				{
					emailDoc.FromAddress = secondarySmtpServer.AllowEmailsToBeSentFromUsersAddress ? firstPrintJob.SP_EmailFromAddress : secondarySmtpServer.SMTPSenderAddress;
				}
				else
				{
					if (EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress)
					{
						emailDoc.FromAddress = firstPrintJob.SP_EmailFromAddress;
					}
				}
			}

			try
			{
				emailDoc.Send();
			}
			catch (EmailDocument.MailerEmailException ex)
			{
				ReportMailerEmailException(firstPrintJob, ex.Message, GetMergedJobRecipients(printJobs));
			}
		}

		List<(string, string, string)> AllocatedPrintJobToEDoc(AttachmentDefCollection attachmentDefCollection, StmPrintJob printJob, StmPrintJobMergedCollection mergedPrintJobs)
		{
			var eDocsAttachmentList = new List<(string, string, string)>();
			if (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.Value && printJob.IsEmailJobFromReportRun)
			{
				if (mergedPrintJobs.SizeInKB > AttachmentSizeLimitInKB && mergedPrintJobs.Count == 1)
				{
					var glowPortalsUri = GlowRegistry.Instance.GlowPortalsUri.Value;
					if (string.IsNullOrEmpty(glowPortalsUri))
					{
						throw new HostedServiceException(Res.GetString("A90819C1-B74A-4C1B-8B97-02FFEBAE708C", "{0} must be configured before emailing the download link.", GlowRegistry.Instance.GlowPortalsUri.Location()));
					}

					var processor = new EDocProcessor(mergedPrintJobs, LogProgress);
					processor.Process();

					var generatedEDocs = processor.GeneratedEDocs;
					if (generatedEDocs.Count == 0 || generatedEDocs.Count != mergedPrintJobs.Count)
					{
						throw new HostedServiceException(Res.GetString("65B54B86-98B8-410D-9C0B-B7FE3ECF5984", "Document \"{0}\" of type \"{1}\" was not allocated to eDocs", printJob.SP_DocumentName, printJob.SP_DocumentType));
					}

					foreach (var eDoc in generatedEDocs)
					{
						eDocsAttachmentList.Add((
							eDoc.FileName,
							$"{glowPortalsUri.TrimEnd('/')}/goto/EDS?StmReportRunId={printJob.SP_ParentGuid}&EDocId={eDoc.UniqueKey}",
							$"{glowPortalsUri.TrimEnd('/')}/goto/EDC?StmReportRunId={printJob.SP_ParentGuid}&EDocId={eDoc.UniqueKey}"));
					}

					attachmentDefCollection.Clear();
				}

				printJob.SP_EDocsProcessed = true;
			}
			return eDocsAttachmentList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Exception Message.")]
		string GetMergedJobRecipients(StmPrintJobMergedCollection printJobs)
		{
			var debugLog = "Merged print jobs:";
			for (int i = 0; i < printJobs.Count; i++)
			{
				var printJob = printJobs[i];
				debugLog += FormattableString.Invariant($"\nJob Pk: {printJob.PK}, Type: {printJob.SP_JobType}");
				debugLog += FormattableString.Invariant($"\nRecipients: {(printJob.EmailToRecipients.Count == 0 ? "No recipient." : "")}");
				foreach (var item in printJob.EmailToRecipients)
				{
					debugLog += FormattableString.Invariant($"\nCopy Recipient PK: {item.PK}, Email: {item.SPR_EmailAddress}");
				}
			}
			return debugLog;
		}

		void ReportMailerEmailException(StmPrintJob printJob, string exceptionMessage, string mergedJobInfo)
		{
			var errorMessage = string.Format(CultureInfo.InvariantCulture, exceptionMessage +
				(NoResString)"\nPrintJob Pk: {5}" +
				(NoResString)"\nEmail Subject: {0}" +
				(NoResString)"\nUser Name: {1}" +
				(NoResString)"\nDocument Name: {2}" +
				(NoResString)"\nParent Table: {3}" +
				(NoResString)"\nParent PK: {4}" +
				(NoResString)"\n{6}", printJob.SP_EmailSubjectLine, printJob.SP_UserFullName, printJob.SP_DocumentName, printJob.SP_ParentTableName, printJob.SP_ParentGuid, printJob.PK, mergedJobInfo);

			ErrorReporter.ReportOnce(errorMessage);
		}

		AttachmentDef GetMergedPdfAttachment(StmPrintJob printJob, ICollection<string> listInvalidPDF)
		{
			var outputPdf =
				StmPrintJob.GetUniqueHumanReadableFilePath(Temp.TempPath, MakeFilenameSafe.MakeSafe(printJob.SP_EmailSubjectLine) + (NoResString)" - Merged", ".PDF");

			try
			{
				DocumentConverter.MergePDFs(attachmentPathsThatNeedMerging, outputPdf, listInvalidPDF);
				return (File.Exists(outputPdf)) ? new AttachmentDef(outputPdf) : null;
			}
			finally
			{
				if (File.Exists(outputPdf))
				{
					TempFile.Delete(outputPdf);
				}
			}
		}

		protected override void ProcessIndividualItemCore(StmPrintJob printJob)
		{
			LogProcessing(printJob);
			printJob.CreateLogOnParent(AutoEvents.DocumentSent);

			if (printJob.SP_EmailAttachmentFormat.EqualsIgnoringCase(AttachmentTypeList.Codes.Pdfc) &&
					Path.GetExtension(printJob.StoredAttachmentFilename).Equals(".PDF", StringComparison.OrdinalIgnoreCase))
			{
				attachmentPathsThatNeedMerging.Add(printJob.StoredAttachmentFilename);
			}
			else if (!string.IsNullOrEmpty(printJob.StoredAttachmentFilename))
			{
				attachmentPaths.Add(printJob.StoredAttachmentFilename);
			}
		}

		List<string> attachmentPaths;
		List<string> attachmentPathsThatNeedMerging;

		#region Implementation

		public StmPrintJobGroupCollection SplitGroupIntoMultipleEmails()
		{
			StmPrintJobGroupCollection emailCollection = new StmPrintJobGroupCollection();
			foreach (StmPrintJob printJob in MergedPrintGroup)
			{
				StmPrintJobMergedCollection existingCollection = emailCollection.GetCollectionToFit(printJob.StoredAttachmentSizeKB, AttachmentSizeLimitInKB);

				if (existingCollection != null)
				{
					existingCollection.Add(printJob);
				}
				else
				{
					emailCollection.AddNewMergedPrintCollection(printJob);
				}
			}
			return emailCollection;
		}

		StringCollection GetCopyRecipientEmailAddresses(StmPrintJobCopyRecipientCollection copyRecipients)
		{
			var results = new StringCollection();
			if (copyRecipients != null)
			{
				results.AddRange(copyRecipients.Select(cr => (string)cr.SPR_EmailAddress).ToArray());
			}
			return results;
		}

		long AttachmentSizeLimitInKB
		{
			get { return (long)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value * 1024; }
		}

		#endregion

	}
}
