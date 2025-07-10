using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.PrintProcessing
{
	class EDocProcessor : MergedPrintGroupProcessor
	{
		public EDocProcessor(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate logProgress)
			: base(mergedPrintGroup)
		{
			LogProgress = logProgress;
		}

		internal List<IeDoc> GeneratedEDocs { get; } = new List<IeDoc>();

		protected override void ProcessIndividualItemCore(StmPrintJob printJob)
		{
			var user = printJob?.JobSubmittedBy;
			using (user != null ? Env.SetTemporaryUserContext(
				user.PK.ToGuid(),
				(!printJob.SP_GB.IsEmpty && printJob.SP_GB.IsValid) ? printJob.SP_GB.ToGuid() : GlbBranch.CurrentBranch?.PK.ToGuid() ?? user.HomeBranch?.PK.ToGuid() ?? Guid.Empty,
				GlbDepartment.CurrentDepartment?.PK.ToGuid() ?? user.HomeDepartment?.PK.ToGuid() ?? Guid.Empty)
				: null)
			{
				var docManagerFactory = CopyToDocManager(printJob);
				if (docManagerFactory != null)
				{
					BusinessObjectFactory.SaveTogether(docManagerFactory);
				}
			}
		}

		protected virtual ITransactionParticipant CopyToDocManager(StmPrintJob printJob)
		{
			if (printJob.DocManagerSupportsBusinessContext)
			{
				var fileName = string.Empty;
				try
				{
					ZString documentName = printJob.SP_DocumentName;
					ZString language = "";
					if (documentName.Contains(StmPrintJob.LanguageDelimiter, StringComparison.Ordinal))
					{
						var lastIndexOfLanguageDelimiter = documentName.LastIndexOf(StmPrintJob.LanguageDelimiter, StringComparison.Ordinal);
						if (documentName.Length - lastIndexOfLanguageDelimiter == 6) // e.g. after the delimiter is the exact 5 characters of a language
						{
							language = documentName.Substring(lastIndexOfLanguageDelimiter + 1);
							documentName = documentName.Substring(0, lastIndexOfLanguageDelimiter);
						}
					}

					var shouldEncryptExcelFileForOpeningAccess = false;
					var dataType = string.Empty;

					switch (printJob.BlobType)
					{
						case Core.Constants.FileFormats.XLS:
						case Core.Constants.FileFormats.XLSX:
							var fileFormatForXlsConversion = printJob.GetFileFormatForXlsConversion();
							var emailAttachmentFormat = printJob.SP_EmailAttachmentFormat;
							if (emailAttachmentFormat == AttachmentType.XLS || emailAttachmentFormat == AttachmentType.XLSX)
							{
								if (SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.Value)
								{
									fileFormatForXlsConversion = emailAttachmentFormat;
									dataType = printJob.BlobType;
									shouldEncryptExcelFileForOpeningAccess = !printJob.SP_ExcelEncryptedPassword.IsEmpty;
								}
							}
							var formatType = (OutputFormatType)Enum.Parse(typeof(OutputFormatType), fileFormatForXlsConversion);
							fileName = Temp.GetTempFileNameWithExtension(fileFormatForXlsConversion);
							if (formatType == OutputFormatType.TIF)
							{
								DocumentConverter.ConvertFromExcel(printJob.SP_CustomProperties, fileName, formatType, printJob.Watermark, TIFColourDepthForLoggingToEDocs, printJob.SP_IsLocalCulture, printJob.SP_FlexCelLineSpacing);
							}
							else
							{
								DocumentConverter.ConvertFromExcel_IncludingHTML(printJob.SP_CustomProperties, fileName, formatType, printJob.Watermark, Env.Registry.PDFTIFColourDepth, printJob.SP_IsLocalCulture, printJob.SP_FlexCelLineSpacing, 0, printJob.ShouldSign, printJob.SignOption, branch: printJob.SP_GB, shouldResetExcelModifyPWD: true);
							}
							break;

						case Core.Constants.FileFormats.XML:
						case Core.Constants.FileFormats.CSV:
						case Core.Constants.FileFormats.TIF:
						case Core.Constants.FileFormats.PDF:
						case Core.Constants.FileFormats.TXT:
							fileName = Temp.GetTempFileNameWithExtension(printJob.BlobType);
							FileSaveHelper.SaveBlobAsFile(printJob.SP_CustomProperties, fileName);
							break;

						default:
							throw new ApplicationException("File format not supported: " + printJob.BlobType);
					}

					if (printJob.SP_DocumentType == Core.Constants.StatementCollectionLetterType.StatementOfAccount)
					{
						printJob.SP_DocumentType = GetAccurateDocumentType(documentName);
					}

					if (printJob.SP_RelatedBusinessContext == Core.Constants.DocManagerCodes.Quotation)
					{
						printJob.SP_DocumentType = Core.Constants.RefDocTypes.SystemQuotation;
					}

					var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
					var documentFactory = documentFactoryProvider.GetFactory(new BusinessObjectFactory());
					if (LogProgress != null)
					{
						((ILogSource)documentFactory).Log += args => { LogProgress(args.EventType, args.Message); };
					}

					try
					{
						if (LogProgress != null)
						{
							LogProgress(TraceEventType.Information, Res.GetString("b412d1d6-6e43-488c-bb3b-9cc80fc9a659",
								"Allocating document \"{0}\" of type \"{2}\" with subject \"{1}\" to eDocs for ({3}, {4})",
								documentName, printJob.SP_EmailSubjectLine, printJob.SP_DocumentType, printJob.SP_RelatedBusinessContext, printJob.SP_ParentGuid));
						}

						if (string.IsNullOrEmpty(dataType))
						{
							dataType = printJob.GetDataTypeForAllocateDocument(fileName);
						}

						if (shouldEncryptExcelFileForOpeningAccess)
						{
							DocumentProtector.EncryptExcelFileOpeningAccess(fileName, Env.Registry.ExcelPasswordForOpening, false);
						}

						var document = documentFactory.CreateAndAllocateDocument(
							printJob.SP_ParentGuid, printJob.SP_RelatedBusinessContext, fileName, printJob.SP_DocumentType, printJob.SP_EmailSubjectLine,
							printJob.SP_SendToEDocs, GetAttachmentFileName(printJob.SP_EmailAttachments), language, dataType, printJob.PK.ToString());

						if (LogProgress != null)
						{
							if (document != null)
							{
								if (document is IeDoc eDoc)
								{
									GeneratedEDocs.Add(eDoc);
								}
								LogProgress(TraceEventType.Information, Res.GetString("2f90f5a7-ea76-4ec5-b9cb-85fc5fca01ae",
									"Finished allocating document \"{0}\" of type \"{1}\"", documentName, printJob.SP_DocumentType));
							}
							else
							{
								LogProgress(TraceEventType.Warning, Res.GetString("7f860e7d-ace3-4cbd-92c4-78363342e2fc",
									"Document \"{0}\" of type \"{1}\" was not allocated to eDocs", documentName, printJob.SP_DocumentType));
							}
						}
					}
					catch (Exception exception)
					{
						if (LogProgress != null)
						{
							LogProgress(TraceEventType.Error, Res.GetString("29d0d0b7-ed27-4258-8cb5-bff997b228fc",
								"Error allocating document \"{0}\" with subject \"{1}\" to eDocs.\r\n\r\nError details:\r\n{2}",
								documentName, printJob.SP_EmailSubjectLine, exception.ToString()));
						}

						if (printJob.SP_RetryAttempts == 0 &&
							ExceptionVisibilityAttribute.Evaluate(exception) == ExceptionVisibility.User && printJob.JobSubmittedBy != null)
						{
							NotifyUserOfFailedDocument(exception.Message, fileName, printJob.JobSubmittedBy.GS_EmailAddress);
						}

						throw;
					}

					return documentFactory;
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(fileName);
				}
			}
			return null;
		}

		string GetAttachmentFileName(string fileName)
		{
			string result = PathValidation.GetSafeFilename(fileName);
			var extension = Path.GetExtension(result);
			if (extension != null)
			{
				result = result.Remove(result.Length - extension.Length, extension.Length);
			}

			return result;
		}

		static string GetAccurateDocumentType(string documentName)
		{
			var type = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

			if (string.Compare(documentName, AccountingMasterFilesRegistry.Instance.StatementDocumentName.Value, StringComparison.OrdinalIgnoreCase) == 0)
			{
			}
			else if (string.Compare(documentName, AccountingMasterFilesRegistry.Instance.CollectionAndDemandFirstReminderDocumentName.Value, StringComparison.OrdinalIgnoreCase) == 0)
			{
				type = Core.Constants.StatementCollectionLetterType.FirstReminder;
			}
			else if (string.Compare(documentName, AccountingMasterFilesRegistry.Instance.CollectionAndDemandSecondReminderDocumentName.Value, StringComparison.OrdinalIgnoreCase) == 0)
			{
				type = Core.Constants.StatementCollectionLetterType.SecondReminder;
			}
			else if (string.Compare(documentName, AccountingMasterFilesRegistry.Instance.CollectionLetterDocumentName.Value, StringComparison.OrdinalIgnoreCase) == 0)
			{
				type = Core.Constants.StatementCollectionLetterType.CollectionLetter;
			}
			else if (string.Compare(documentName, AccountingMasterFilesRegistry.Instance.DemandLetterDocumentName.Value, StringComparison.OrdinalIgnoreCase) == 0)
			{
				type = Core.Constants.StatementCollectionLetterType.DemandLetter;
			}

			return type;
		}

		void NotifyUserOfFailedDocument(string reason, string fileName, string emailAddress)
		{
			var email = new EmailDef();
			email.Subject = Res.GetString("bfd965ca-4354-446b-aeb9-4a59d5fda177", "Failure to create eDoc");
			email.Body = Res.GetString("789f1cc3-907d-4317-a3a5-b41a090938fc", "Your print job failed to be converted to an eDoc entry.\r\nThe reason is shown below.\r\n{0}\r\n\r\nPlease contact your system administrator to resolve this issue.", reason);
			email.Attachments.Add(new AttachmentDef(fileName));
			email.AddRecipientForUserCommunication(emailAddress);
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		#region Color Depth

		ColourDepth TIFColourDepthForLoggingToEDocs
		{
			get
			{
				if (!isTIFColourDepthSet)
				{
					isTIFColourDepthSet = true;
					switch (SystemDataRegistry.Instance.DocManagerTIFColourDepth.Value)
					{
						case Core.Constants.ColourDepth.Colour256:
							tifColourDepthForLoggingToEDocs = ColourDepth.Colour256;
							break;

						case Core.Constants.ColourDepth.BlackAndWhite:
						default:
							tifColourDepthForLoggingToEDocs = ColourDepth.BlackAndWhite;
							break;
					}
				}
				return tifColourDepthForLoggingToEDocs;
			}
		}

		ColourDepth tifColourDepthForLoggingToEDocs = ColourDepth.BlackAndWhite;
		bool isTIFColourDepthSet;

		#endregion
	}
}
