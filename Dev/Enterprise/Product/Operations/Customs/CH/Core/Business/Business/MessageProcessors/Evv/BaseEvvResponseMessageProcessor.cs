using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Extensions;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseEvvResponseMessageProcessor<T> : BaseResponseMessageProcessor<T> where T : IMessageDetail
{
	public BaseEvvResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.EVV };

	protected void CreateDocuments(CHEDIMessage message, IEvvCommonProvider evvResponse, string dataContext)
	{
		var templateName = GetTemplateName(evvResponse);
		if (templateName != null && message.EM_LinkedObject is IDocManagerSupport docManagerSupport)
		{
			docManagerSupport.DocManagerInfo.ForceToUseAnotherFactory(message.Factory);
			var documentFactory = docManagerSupport.DocManagerInfo.MasterFactory;
			var storageMain = documentFactory.RetrieveExistingOrCreateStorageMain(message.EM_LinkedObject, Core.Constants.DocManagerCodes.CustomsEntry);
			var evvDocumentWrapper = GetEVVDocumentWrapper(dataContext, message);
			var filename = evvDocumentWrapper.DocumentFilename;
			CreateDocument(storageMain, evvDocumentWrapper, templateName, filename, dataContext);
			CreateDocument(storageMain, EvvSignatureDocumentWrapper.New(message, message.Factory), ExcelTemplatesNames.EvvValidationReportTemplateName, filename + ValidationReportFilenameSuffix, CHEDIMessageDocumentSupporter.EVVValidationDocument);
			documentFactory.Save();
		}
	}

	void CreateDocument(IStorageMain storageMain, DocumentWrapper documentWrapper, string templateName, string filename, string dataContext)
	{
		var stmTemplate = documentWrapper.Factory.Load<StmTemplate>(new ZQuery().AddToFilter(StmTemplateSchema.SO_Name, templateName).AddToFilter(StmTemplateSchema.SO_DataContext, dataContext)).FirstOrDefault();
		var excelTemplate = ExcelTemplateRetriever.GetTemplate(templateName, dataContext, documentWrapper.Factory);
		if (stmTemplate  != null && excelTemplate != null)
		{
			using (var report = new Report(null, excelTemplate, documentWrapper, excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			using (var memoryStream = new MemoryStream())
			{
				report.StTemplate = stmTemplate;
				report.Save(memoryStream);
				var binaryData = DocumentConverter.ConvertFromExcel(memoryStream.ToArray(), OutputFormatType.PDF, ZArchitecture.Environment.ColourDepth.Colour256, localCulture: true);
				storageMain.AddFileOrDocument(binaryData, filename + ".pdf", Core.Constants.RefDocTypes.MiscellaneousDocument);
			}
		}
	}

	string GetTemplateName(IEvvCommonProvider evvResponse) => evvResponse.DocumentType switch
	{
		EvvDocumentType.Codes.RefundVAT => ExcelTemplatesNames.EvvRefundVATTemplateName,
		EvvDocumentType.Codes.RefundCustomsDuties => ExcelTemplatesNames.EvvRefundDutiesTemplateName,
		EvvDocumentType.Codes.TaxationDecisionCustomsDuties => ExcelTemplatesNames.EvvDutiesTemplateName,
		EvvDocumentType.Codes.TaxationDecisionVAT => ExcelTemplatesNames.EvvVATTemplateName,
		_ => null
	};

	BaseEVVDocumentWrapper GetEVVDocumentWrapper(string dataContext, CHEDIMessage message) => dataContext switch
	{
		CHEDIMessageDocumentSupporter.EVVRefundDocument => EVVRefundDocumentWrapper.New(message, message.Factory),
		CHEDIMessageDocumentSupporter.EVVTaxationDocument => EVVTaxationDocumentWrapper.New(message, message.Factory),
		_ => null
	};

	protected static string GetDocumentTypeFromMessageSubType(ZString messageSubtype)
	{
		switch (messageSubtype)
		{
			case MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat:
				return EvvDocumentType.Codes.RefundVAT;
			case MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties:
				return EvvDocumentType.Codes.RefundCustomsDuties;
			case MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties:
				return EvvDocumentType.Codes.TaxationDecisionCustomsDuties;
			case MessageSubTypeCodeList.Codes.TaxationDecisionVat:
				return EvvDocumentType.Codes.TaxationDecisionVAT;
			default:
				return string.Empty;
		}
	}

	const string ValidationReportFilenameSuffix = "_validationReport";

	protected void UpdateStatementLineStatus(BusinessObjectFactory factory, string mrn, string messageSubType, string status)
	{
		var summaryLine = new CustomsSummaryLine.Loader(factory).LoadCustomsSummaryLine(mrn, BordereauChargeTypeList.GetChargeType(messageSubType));
		if (summaryLine != null)
		{
			summaryLine.B3_Status = status;
		}
	}
}
