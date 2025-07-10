using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	abstract class QueuedForBatchProcessor : DeliveryMethod
	{
		#region Deliver

		protected sealed override void DeliverCore(INotifications notifications = null)
		{
			ZGuid generatedJobPk = ZGuid.Empty;

			if (DeliveryInfos.Count > 1 && ConsolidateReports)
			{
				var deliveryInfo = GetFirstXLSDeliveryInfo();
				if (deliveryInfo != null)
				{
					var fileFormat = string.Equals(deliveryInfo.FileFormat, Core.Constants.FileFormats.XLSX, StringComparison.OrdinalIgnoreCase) ? Core.Constants.FileFormats.XLSX : Core.Constants.FileFormats.XLS;

					var fileContents = MergeFilesIntoOneXLS(fileFormat);
					if (!deliveryInfo.HasMergeIntoPrimaryExcelTemplateFailed)
					{
						deliveryInfo.SetFileContents(fileContents, fileFormat);
						if (!string.IsNullOrEmpty(EmailSubjectForConsolidateReports))
						{
							deliveryInfo.EmailSubjectLine = EmailSubjectForConsolidateReports;
						}
						if (!string.IsNullOrEmpty(AttachedFileNameForConsolidateReports))
						{
							deliveryInfo.AttachedFilename = AttachedFileNameForConsolidateReports;
						}
						generatedJobPk = CreateAndSaveUnprocessedJob(deliveryInfo, SendToEDocs, true, notifications);
					}
				}

				deliveryInfo = GetFirstTIFDeliveryInfo();

				bool tiffsMerged = false;

				if (MergeTiffsOnDelivery && deliveryInfo != null)
				{
					var stream = MergeFilesIntoOneTIF();
					if (stream != null)
					{
						deliveryInfo.SetFileContents(stream, (NoResString)"tif");
						generatedJobPk = CreateAndSaveUnprocessedJob(deliveryInfo, SendToEDocs, true, notifications);
						tiffsMerged = true;
					}
				}

				foreach (DeliveryInfo info in DeliveryInfos)
				{
					if (info.DeliveryFormat == DeliveryInfo.DeliveryFormats.File
						|| (!tiffsMerged && info.DeliveryFormat == DeliveryInfo.DeliveryFormats.TIFF)
						|| info.HasMergeIntoPrimaryExcelTemplateFailed)
					{
						generatedJobPk = CreateAndSaveUnprocessedJob(info, SendToEDocs, false, notifications);
					}
				}
			}
			else
			{
				foreach (DeliveryInfo info in DeliveryInfos)
				{
					// Save the job in a separate factory to conserve memory.
					// We could be saving thousands of documents here, each with a 500KB Excel file.
					generatedJobPk = CreateAndSaveUnprocessedJob(info, SendToEDocs, false, notifications);
				}
			}

			if (generatedJobPk.IsEmpty)
			{
				Instructions?.ActionForFailedToDeliver?.Invoke();
			}
		}

		protected virtual bool MergeTiffsOnDelivery
		{
			get
			{
				return false;
			}
		}

#if DEBUG
		internal bool MergeTiffsOnDeliveryForTesting
		{
			get { return MergeTiffsOnDelivery; }
		}
#endif

#if DEBUG
		internal BusinessObjectFactory FactoryOverrideForTest { get; set; }
#endif

		#endregion

		#region Save Strategy

		public FactoryStrategy SaveStrategy
		{
			get
			{
				if (fSaveStrategy == null)
				{
					fSaveStrategy = new FactoryStrategy.SaveInChunks();
				}
				return fSaveStrategy;
			}
			set { fSaveStrategy = value; }
		}

		FactoryStrategy fSaveStrategy;

		protected override void SetPropertiesFromDeliveryInstructions(DeliveryInstructions instructions)
		{
			base.SetPropertiesFromDeliveryInstructions(instructions);
			if (instructions.FactorySaveStrategy != null)
			{
				SaveStrategy = instructions.FactorySaveStrategy;
			}
		}

		#endregion

		#region Implementation

		protected string GetEmailAttachmentFormat(DeliveryInfo info, string attachmentType)
		{
			if (attachmentType.Equals(AttachmentTypeList.Codes.Pdfc, StringComparison.OrdinalIgnoreCase) && this is Email)
			{
				return AttachmentTypeList.Codes.Pdfc;
			}

			// NOTE - we are checking the type of the DeliveryFormat rather than Instructions.TIFAttachmentsOnly because
			// during multi doc pack delivery the doc pack may be set to PDF / XLS etc.
			switch (info.DeliveryFormat)
			{
				case DeliveryInfo.DeliveryFormats.Document:
				case DeliveryInfo.DeliveryFormats.Report:
					if (attachmentType.Equals(AttachmentTypeList.Codes.CsvWithHeadings, StringComparison.OrdinalIgnoreCase))
					{
						return AttachmentTypeList.Codes.Csv;
					}

					if (attachmentType.Equals(AttachmentTypeList.Codes.Txt_Comm, StringComparison.OrdinalIgnoreCase)
						|| attachmentType.Equals(AttachmentTypeList.Codes.Txt_Pipe, StringComparison.OrdinalIgnoreCase)
						|| attachmentType.Equals(AttachmentTypeList.Codes.Txt_Semi, StringComparison.OrdinalIgnoreCase))
					{
						return AttachmentTypeList.Codes.Txt;
					}

					return attachmentType.ToUpperInvariant();

				case DeliveryInfo.DeliveryFormats.File:
					return OrgConstants.AttachmentType.FIL;
				case DeliveryInfo.DeliveryFormats.TIFF:
					return OrgConstants.AttachmentType.TIF;

				default:
					return "";
			}
		}

		protected virtual bool ConsolidateReports
		{
			get { return true; }
		}

		void CheckDeliveryGroupID(DeliveryInfo info)
		{
			if (info.DeliveryGroupID == ZGuid.Empty)
			{
				return;
			}

			if (info.DeliveryGroupID == ZGuid.Invalid)
			{
				throw new InvalidDeliveryGroupException(info.Name, ZGuid.Invalid, "Delivery group id is invalid");
			}

			var group = SaveStrategy.GetFactory().LoadTop1<StmDeliveryGroup>(new ZQuery(StmDeliveryGroupSchema.PK, info.DeliveryGroupID)) ?? throw new InvalidDeliveryGroupException(info.Name, info.DeliveryGroupID, "Delivery group id does not exist in db");
		}

		protected ZGuid CreateAndSaveUnprocessedJob(DeliveryInfo info, bool sendToEDocs, bool isMergedDelivery, INotifications notifications = null)
		{
			CheckDeliveryGroupID(info);

			byte[] buffer;

			buffer = new byte[info.FileContents.Length];
			info.FileContents.Seek(0, SeekOrigin.Begin);
			int bytesRead = info.FileContents.Read(buffer, 0, (int)info.FileContents.Length);
			if (bytesRead != info.FileContents.Length)
			{
				throw new DocumentEngineException("BytesRead != Info.FileContents.Length");
			}

			BusinessObjectFactory factory;

#if DEBUG
			if (FactoryOverrideForTest != null)
			{
				factory = FactoryOverrideForTest;
			}
			else
#endif
			{
				factory = SaveStrategy.GetFactory();
			}

			factory.RefreshEnabled = false;

			StmPrintJob printJob = factory.New<StmPrintJob>();
			printJob.SP_DocumentName = ((ZString)(info.Name +
				StmPrintJob.LanguageDelimiter + Language)).Left(printJob.SP_DocumentNameInfo.MaxLength);
			printJob.SP_RunDateTime = info.RunDateTime.IsEmpty ? ZDateTime.UtcNow : info.RunDateTime;
			printJob.SP_JobType = PrintType.ToString();
			printJob.SP_EmailSubjectLine = ((ZString)info.EmailSubjectLine).Left(printJob.SP_EmailSubjectLineInfo.MaxLength);
			if (ForcedParentDocManagerInfo != null)  // Tested in GB
			{
				printJob.SP_ParentTableName = ForcedParentDocManagerInfo.BusinessEntity.TableName;
				printJob.SP_ParentGuid = ForcedParentDocManagerInfo.BusinessEntity.PK;
				printJob.SP_RelatedBusinessContext = ForcedParentDocManagerInfo.DocManagerCode;
			}
			else
			{
				printJob.SP_ParentTableName = info.ParentTableName;
				printJob.SP_ParentGuid = info.ParentGuid;
				printJob.SP_RelatedBusinessContext = info.RelatedBusinessContext;
			}
			printJob.SP_DocumentType = info.DocumentType;
			printJob.SP_Group = GroupId;
			printJob.SP_Sequence = Sequence++;
			printJob.SP_GS_NKJobSubmittedBy = info.JobSubmittedBy;
			printJob.SP_SB_DeliveryGroup = info.DeliveryGroupID;
			printJob.SP_EmailFromAddress = info.EmailFromAddress;
			printJob.SP_EmailSignature = info.EmailSignature;
			printJob.SP_IsLocalCulture = info.IsLocalDocument;
			printJob.SP_FlexCelLineSpacing = info.LineSpacing;
			printJob.SP_SendToEDocs = sendToEDocs;
			printJob.SP_SignBy = GetSignBy(info, printJob.SP_GB);

			printJob.SP_PDFEncryptedPassword = info.PDFEncryptionPassword;

			if (info.Watermark != null)
			{
				printJob.SP_WatermarkText = info.Watermark.AsText;
				printJob.SP_WatermarkImage = info.Watermark.AsImage;
			}

			if (Enum.TryParse<PrintType>(printJob.SP_JobType, true, out var jobType) && jobType == PrintType.PRN && DocumentsDataRegistry.Instance.DeliverDocumentsToPrintersInPdfFormat.Value &&
					(info.FileFormat.Equals("XLS", StringComparison.OrdinalIgnoreCase) ||
					 info.FileFormat.Equals("XLSX", StringComparison.OrdinalIgnoreCase)))
			{
				printJob.SP_CustomProperties = DocumentConverter.ConvertFromExcel(buffer, string.Empty, OutputFormatType.PDF, printJob.Watermark, printJob.SP_IsLocalCulture, printJob.SP_FlexCelLineSpacing);
				printJob.SP_EmailAttachments = info.AttachedFilename + ".PDF";
			}
			else
			{
				printJob.SP_CustomProperties = buffer;
				printJob.SP_EmailAttachments = info.AttachedFilename + '.' + info.FileFormat;
			}

			if (info.Instructions != null)
			{
				info.Instructions.MarkPrintJobEDocsProcessedIfNeeded(printJob);
			}

			try
			{
				SetAdditionalProperties(printJob, info);
				info.Protector?.PasswordProtectForOpening(printJob, info.AttachedFilename);

				var service = PrintJobDeliveryInfosLink.GetInstance(printJob.Factory);
				service.Add(printJob, isMergedDelivery
					? DeliveryInfos.ToArray()
					: new[] { info });

				SaveStrategy.SaveChunk(factory);
			}
			catch (ZSaveErrorAfterCommitInDbException)
			{
				notifications?.AddWarning(Res.GetString("9D7DD9F7-8CAA-44EC-9CFC-C34121146D03", "An error occurred after the Print Job was saved to the database, but the process will continue as normal."));
			}
			catch (Exception ex) when (ex is InvalidPrinterException || ex is EmailHasNoRecipientsException)
			{
				Globals.Message.Show(ex.Message);
			}

			if (printJob.SP_CustomProperties.IsEmpty && printJob.AttachmentRequiresConversion)
			{
				var templateList = new List<ZString>();
				if (info.DocumentPack?.StmMenuCommand is DocumentCommand documentCommand)
				{
					var pivotList = documentCommand.Documents.ToArray();
					foreach (StmMenuTemplatePivotBase pivot in pivotList)
					{
						templateList.Add(string.Format((NoResString)"PK : {0}, Name : {1}", pivot?.Template.PK, pivot?.Template.SO_Name));
					}
				}

				var template = string.Join("\r\n", templateList.Select(temp => $"[{temp}]"));
				var errorMessage = $@"PrintJob's CustomProperties is empty: MenuItemName: [{info.Instructions?.DocumentPackTitle ?? ZString.Empty}]
TemplateList: {template}
Email Subject: [{printJob.SP_EmailSubjectLine}]
Parent Table Name: [{printJob.SP_ParentTableName}]";

				ErrorReporter.ReportOnce("SP_CustomProperties is empty", errorMessage);
			}
			return printJob.PK;
		}

		static string GetSignBy(DeliveryInfo info, ZGuid branch)
		{
			return (info.SignBy == DocumentsSignBy.DOS) && (branch.IsEmpty || !DocumentsDataRegistry.Instance.EnableDocumentSigningService.GetFallBackValueAtAllLevels(Guid.Empty, branch.ToGuid(), Guid.Empty))
				? DocumentsSignBy.NON
				: info.SignBy;
		}

		static readonly ZGuid GroupId = ZGuid.NewZGuid();
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
#if DEBUG
		internal
#endif
 static ZInt Sequence = 1;

		protected abstract PrintType PrintType { get; }

		protected virtual void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
		}

		#endregion
	}
}

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class InvalidDeliveryGroupException : ArgumentException
	{
#if NETFRAMEWORK
		protected InvalidDeliveryGroupException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public InvalidDeliveryGroupException(string name, ZGuid pk, string reason)
			: base(string.Format((NoResString)"'{0}' cannot be delivered, delivery group ID is '{1}', reason: '{2}'.", name, pk, reason))
		{
			Name = name;
			PK = pk.ToString();
			Reason = reason;
		}

		public string Name { get; private set; }
		public string PK { get; private set; }
		public string Reason { get; private set; }
	}

	[Serializable]
	public class InvalidPrinterException : Exception
	{
		public InvalidPrinterException(string message) : base(message) { }
#if NETFRAMEWORK
		protected InvalidPrinterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
