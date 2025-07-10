using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Delivery
{
	sealed class DocumentDeliverable : NonPersistentBusinessObject, IDocumentDeliverable, IObsoleteValidation
	{
		public DocumentDeliverable(BusinessObjectFactory factory, IDocumentDelivery documentDelivery)
			: base(factory)
		{
			Argument.NotNull(documentDelivery, nameof(documentDelivery));

			this.document = Argument.NotNull(documentDelivery.Document, nameof(documentDelivery.Document));
			this.printInstructions = Argument.NotNull(documentDelivery.PrintInstructions, nameof(documentDelivery.PrintInstructions));
			this.eDocsInstructions = Argument.NotNull(documentDelivery.EDocsInstructions, nameof(documentDelivery.EDocsInstructions));

			DeliveryMode = documentDelivery.DeliveryMode;
			DocumentType = documentDelivery.DocumentType;
			LogParent = Argument.NotNull(documentDelivery.LogParent, nameof(documentDelivery.LogParent));
		}

		readonly IDocument document;
		readonly IPrintInstructions printInstructions;
		readonly IEDocsInstructions eDocsInstructions;

		public IDocument Document => document;
		public string DocumentName
		{
			get
			{
				if (documentName == null)
				{
					documentName = GetDocumentName();
					documentName = string.IsNullOrWhiteSpace(documentName) ? document.Name : documentName;
				}

				return documentName;
			}
		}
		string documentName;

		public string DocumentTitle => documentTitle ?? (documentTitle = printInstructions.GetDeliveryTitle(DeliveryMode));
		string documentTitle;

		public IEnumerable<KeyValuePair<string, string>> GetParametersForDocumentDeliveryLog(string documentName) => printInstructions.GetParametersForDocumentDeliveryLog(documentName);

		public string DocumentType { get; }
		public IStmALogParent LogParent { get; }
		public IDocManagerSupport EDocsParent => eDocsParent ?? (eDocsParent = eDocsInstructions.Parent as IDocManagerSupport);
		IDocManagerSupport eDocsParent;
		public bool SaveCopyToEDocs => eDocsInstructions.SaveCopyToEDocs;

		#region IDeliverable UI

		public ZString DeliveryMode { get; }

		public ZPropertyInfo DeliveryModeInfo => GetZPropertyInfo(nameof(DeliveryMode));

		public ZString AllAvailableDeliveryModes
		{
			get { return DeliveryMode; }
		}

		public IEnumerable<string> GetSupportedDeliveryMethods()
		{
			var deliveryMode = DeliveryMode.ToUpperInvariant();
			if (string.IsNullOrEmpty(deliveryMode) || !Enum.TryParse<PrintCopyType>(deliveryMode, out var mode))
			{
				return Enumerable.Empty<string>();
			}

			return DeliveryMethodHelper.GetSupportedDelvieryMethodsFor(mode);
		}

		public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType() => GetSupportedDeliveryMethods();

		public bool SupportsDeliveryMethod(string deliveryMethod)
		{
			return GetSupportedDeliveryMethods().Any(method => method == deliveryMethod?.ToUpperInvariant());
		}

		public IPrePrintProcessingResult DoPrePrintProcessing(BusinessObjectFactory deliveryFactory, bool isDraft) =>
			printInstructions is IPrePrintProcessor prePrintProcessor
				? prePrintProcessor.DoPrePrintProcessing(document, deliveryFactory, isDraft)
				: null;

		public ZString DocumentTypeCode => DocumentType;

		RefDocType RefDocType
		{
			get
			{
				if (refDocType == null && !DocumentType.IsNullOrEmpty())
				{
					refDocType = GetRefDocType();
				}

				return refDocType;
			}
		}
		RefDocType refDocType;

		RefDocType GetRefDocType()
		{
			return Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, DocumentType));
		}

		public ZPropertyInfo DocumentTypeCodeInfo => GetZPropertyInfo(nameof(DocumentTypeCode));

		public ZString DocumentTypeDescription
		{
			get
			{
				if (documentTypeDescription.IsEmpty)
				{
					documentTypeDescription = RefDocType != null ? RefDocType.RT_DescMultilingual : ZString.Empty;
				}

				return documentTypeDescription;
			}
		}
		ZString documentTypeDescription;

		public ZPropertyInfo DocumentTypeDescriptionInfo => GetZPropertyInfo(nameof(DocumentTypeDescription));

		public ZBool IncludedInPrint
		{
			get { return includedInPrint; }
			set { SetNonPersistentPropertyValue<ZBool>(IncludedInPrintInfo, ref includedInPrint, value); }
		}
		ZBool includedInPrint = true;

		public ZPropertyInfo IncludedInPrintInfo => GetZPropertyInfo(nameof(IncludedInPrint));
		public bool IncludedInPrint_ReadOnly { get; set; }

		bool IDeliverable.IsDeliveredByEmail { get; set; }

		public ZString Name => DocumentTitle;
		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));
		public ZString NameForBinding => Name;
		ZString IDeliverable.JobNumber { get; }

		#endregion

		bool IDeliverable.ContainsDataRows => false;

		public DocDeliveryPrintDetails PrinterDetails
		{
			get
			{
				if (printerDetails == null)
				{
					var copies = Convert.ToInt16(printInstructions.GetNumberOfCopies(DeliveryMode));
					printerDetails = new DocDeliveryPrintDetails(Factory ?? new BusinessObjectFactory(), copies);
				}
				return printerDetails;
			}
		}
		DocDeliveryPrintDetails printerDetails;

		ZBool IDeliverable.CoverSheetRequired => false;

		public ZGuid DeliveryGroupID
		{
			get;
			set;
		}

		public ZString DocumentDeliveredEventCode
		{
			get
			{
				if (!documentDeliveredEventCode.HasValue)
				{
					documentDeliveredEventCode = !string.IsNullOrEmpty(RefDocType?.RT_SE_NKDocumentReceivedEvent) ? RefDocType?.RT_SE_NKDocumentReceivedEvent.ToString() : Events.DocumentDeliveredCode;
				}

				return documentDeliveredEventCode.Value;
			}
		}
		ZString? documentDeliveredEventCode;

		public ZString DocumentPasswordInformationEventCode { get; set; }

		public ZString FileExtension => DeliveryExtensions.ExcelWorksheetFileExtension;

		StmMenuItem IDeliverable.MenuItem
		{
			get;
			set;
		}

		ZGuid IDeliverable.SourcePivotPK
		{
			get;
			set;
		}

		ZGuid IDeliverable.MenuTemplatePivotPK
		{
			get;
		}

		ZString IDeliverable.Identifier
		{
			get;
		}

		ZGuid IDeliverable.IdentifiablePK
		{
			get;
		}

		DeliveryInfo IDeliverable.GetDeliveryInfo(bool isDraft) => GetDeliveryInfo(isDraft, FileType.XLS);

		public DeliveryInfo GetDeliveryInfo(bool isDraft, FileType fileType)
		{
			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document)
			{
				Name = DocumentName,
				DocumentType = DocumentType,
				FileFormat = FileExtension,
				AttachedFilename = GetAttachmentFileName(),
				PrintQueue = PrinterDetails.PrintQueue,
				Copies = PrinterDetails.NumberOfCopies > short.MaxValue ? short.MaxValue : (short)PrinterDetails.NumberOfCopies
			};

			var xlsFile = document.ToXlsFile();
			xlsFile?.Save(deliveryInfo.FileContents);

			deliveryInfo.ShowDraftWatermark = isDraft;

			return deliveryInfo;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "creating valid macro expression, programmatic constant")]
		string GetAttachmentFileName()
		{
			string res = string.Empty;

			if (!string.IsNullOrWhiteSpace(printInstructions.AttachmentFilename))
			{
				var expr = $"\"{printInstructions.AttachmentFilename}\""
					.With<StandardLibrary>()
					.CreateExpression();

				var dataSource = document.Data?.Value;

				res = Convert.ToString(expr.Evaluate(dataSource), CultureInfo.InvariantCulture);
			}

			res = string.IsNullOrWhiteSpace(res)
				? DocumentName
				: res;

			const string fallbackFileName = "file";

			return ReplaceInvalidFileNameCharacters(res) ?? fallbackFileName;
		}

		string GetDocumentName()
		{
			var res = string.Empty;

			if (!string.IsNullOrWhiteSpace(printInstructions.DocumentName))
			{
				var expr = $"\"{printInstructions.DocumentName}\"" // creating valid macro expression
					.With<StandardLibrary>()
					.CreateExpression();

				var dataSource = document.Data?.Value;

				res = Convert.ToString(expr.Evaluate(dataSource), CultureInfo.InvariantCulture);
			}

			return ReplaceInvalidFileNameCharacters(res);
		}

		string ReplaceInvalidFileNameCharacters(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return fileName;
			}

			var invalidChars = Path.GetInvalidFileNameChars();

			var invalidCharIndex = fileName.LastIndexOfAny(invalidChars);

			while (invalidCharIndex >= 0)
			{
				fileName = fileName.Remove(invalidCharIndex, 1);
				invalidCharIndex = fileName.LastIndexOfAny(invalidChars);
			}

			const int fileNameMaxLength = 128;

			return fileName.Length > fileNameMaxLength
				? fileName.Substring(0, fileNameMaxLength)
				: fileName;
		}

		void IDeliverable.Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, Stream fileContent)
		{
		}

#if DEBUG

		void IDeliverable.DeleteTempFilesForTesting()
		{
		}

		int IDeliverable.RunCountForTesting => 0;

#endif

		#region Enterprise.Integration.DocumentEngine.IDocument members

		bool Enterprise.Integration.DocumentEngine.IDocument.CanIncludeInPrint { get; set; }
		string Enterprise.Integration.DocumentEngine.IDocument.DocumentDeliveryMethod => DeliveryMode;
		string Enterprise.Integration.DocumentEngine.IDocument.DocumentName => DocumentName;
		bool Enterprise.Integration.DocumentEngine.IDocument.IncludeInPrint => IncludedInPrint;

		#endregion

		public bool ShouldPrintByDefault { get; set; } = true;

		public ZByte Index { get; set; }

		#region IDisposable members

		void IDisposable.Dispose()
		{
			// nothing to dispose
		}

		#endregion
	}
}
