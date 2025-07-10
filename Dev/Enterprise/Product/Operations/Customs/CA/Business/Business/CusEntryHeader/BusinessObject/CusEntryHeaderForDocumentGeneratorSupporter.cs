using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business
{
	public partial class CusEntryHeader
	{
		public const string B3AsAccountedActionCode = "B3A";
		public const string CACustomsInvoiceActioCode = "CCI";

		#region Override Document Generator Supporter members

		protected override ZString GetDocumentNameForDocumentGeneratorCore(ZString actionCode)
		{
			if (actionCode == B3AsAccountedActionCode)
			{
				return "B3 (As Accounted)";
			}
			else if (actionCode == CACustomsInvoiceActioCode)
			{
				return "CA Customs Invoice";
			}
			else if (!actionCode.IsEmpty)
			{
				throw new InvalidOperationException($"{actionCode} is not supported for document generator in CA customs.");
			}

			return ZString.Empty;
		}

		protected override ZBool GenerateCustomsDocumentForDocumentGeneratorCore(ZString actionCode)
		{
			var documentAdded = false;
			reasonForUnableToGenerateDocument = ZString.Empty;

			if (Declaration is JobDeclaration declaration && (actionCode == B3AsAccountedActionCode || actionCode == CACustomsInvoiceActioCode))
			{
				var documentFactory = declaration.DocManagerInfo.MasterFactory;
				var storageMain = documentFactory.RetrieveExistingOrCreateStorageMain(declaration, Constants.DocManagerCodes.JobDeclaration);

				if (actionCode == B3AsAccountedActionCode)
				{
					var b3ExcelTemplate = ExcelTemplateRetriever.GetTemplate("B3ImportDocument", ".B3ImportEntry", null);
					if (b3ExcelTemplate != null)
					{
						var lastSentAcceptedB3Message = B3Message.GetLastSentAcceptedB3Message(this);
						if (lastSentAcceptedB3Message != null && lastSentAcceptedB3Message.Interchange != null)
						{
							var b3DataProvider = BODocDataProvider.Get(new B3ImportDocumentWrapper(lastSentAcceptedB3Message));
							SaveReportToEDocs(b3ExcelTemplate, b3DataProvider, storageMain, Constants.RefDocTypes.CustomsAuthority);
							documentAdded = true;
						}
						else
						{
							reasonForUnableToGenerateDocument = "system cannot find the accepted B3 message for this job";
						}
					}
				}
				else if (actionCode == CACustomsInvoiceActioCode)
				{
					var invoiceExcelTemplate = ExcelTemplateRetriever.GetTemplate("CACustomsInvoice", Constants.DataContext.CommercialInvoice, null);
					if (invoiceExcelTemplate != null)
					{
						if (declaration.Invoices.Count > 0)
						{
							foreach (var invoice in declaration.Invoices)
							{
								var invoiceDataProvider = BODocDataProvider.Get(DocumentWrapperFactory.CreateCustomsWrapper(Constants.DataContext.JobComInvoiceHeader, invoice, Constants.CountryCodes.Canada));
								SaveReportToEDocs(invoiceExcelTemplate, invoiceDataProvider, storageMain, Constants.RefDocTypes.Invoice);
								documentAdded = true;
							}
						}
						else
						{
							reasonForUnableToGenerateDocument = "there is no invoice header found for this job";
						}
					}
				}

				if (documentAdded)
				{
					documentFactory.Save();
				}
			}

			return documentAdded;
		}
		ZString reasonForUnableToGenerateDocument;

		protected override ZString GetReasonForUnableToGenerateCustomsDocumentCore()
		{
			return reasonForUnableToGenerateDocument;
		}

		void SaveReportToEDocs(ExcelTemplateReadFromStmTemplateTable template, IBODocDataProvider docDataProvider, IStorageMain storageMain, string documentType)
		{
			using (var report = new Report(null, template, docDataProvider, template.TemplateName, null, DocumentDirection.ANY, false))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				var binaryData = DocumentConverter.ConvertFromExcel(outputStream.ToArray(), OutputFormatType.PDF, ColourDepth.BlackAndWhite);
				storageMain.AddFileOrDocument(binaryData, template.TemplateName + ".pdf", documentType, false);
			}
		}

		#endregion
	}
}
