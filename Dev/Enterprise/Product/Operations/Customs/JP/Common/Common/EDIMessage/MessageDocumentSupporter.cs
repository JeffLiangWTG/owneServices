using System.Collections.Generic;
using CargoWise.Customs.JP.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Common;

public sealed class MessageDocumentSupporter(EDIMessage ediMessage) : EDIMessageDocumentSupporter(ediMessage)
{
	public const string ExportPermitMessageDocument = ".ExportPermitMessageDocument";
	public const string InspectionNoticeMessageDocument = ".InspectionNoticeMessageDocument";
	public const string MismatchInformationDocument = ".MismatchInformationDocument";
	public const string HBLCargoRegistrationInformationMessageDocument = ".SAS0711MessageDocument";
	public const string ImportPermitMessageDocument = ".ImportPermitMessageDocument";
	public const string MoveInNoticeMessageDocument = ".MoveInNoticeMessageDocument";
	public const string CancellationOfTransshipmentReport = ".CancellationOfTransshipmentReport";
	public const string HBLCargoCancellationInformationMessageDocument = ".HBLCargoCancellationInformation";
	public const string EACNoticeInformationDocument = ".EACNoticeInformationDocument";
	public const string TransshipmentNoticeSubmission = ".TransshipmentNoticeSubmission";

	new EDIMessage EdiMessage => (EDIMessage)base.EdiMessage;

	protected override List<DataContextValue> GetSupportedBODataSources()
	{
		var result = base.GetSupportedBODataSources();
		result.Add(new DataContextValue(HBLCargoRegistrationInformationMessageDocument));
		result.Add(new DataContextValue(ExportPermitMessageDocument));
		result.Add(new DataContextValue(InspectionNoticeMessageDocument));
		result.Add(new DataContextValue(MismatchInformationDocument));
		result.Add(new DataContextValue(ImportPermitMessageDocument));
		result.Add(new DataContextValue(MoveInNoticeMessageDocument));
		result.Add(new DataContextValue(CancellationOfTransshipmentReport));
		result.Add(new DataContextValue(HBLCargoCancellationInformationMessageDocument));
		result.Add(new DataContextValue(EACNoticeInformationDocument));
		result.Add(new DataContextValue(TransshipmentNoticeSubmission));

		return result;
	}

	protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
	{
		return dataContextValue.FullDataContext switch
		{
			ExportPermitMessageDocument when EdiMessage.IsExportPermitMessage => [GetExportPermitMessageDocumentWrapper()],
			MismatchInformationDocument when EdiMessage.IsMismatchInformationMessage => [GetMismatchInformationMessageDocumentWrapper()],
			InspectionNoticeMessageDocument when EdiMessage.IsInspectionInformationMessage => [new InspectionNoticeMessageDocumentWrapper(InboundMessageParseResult, Factory)],
			ImportPermitMessageDocument when EdiMessage.IsImportPermitMessage => [GetImportPermitMessageDocumentWrapper()],
			MoveInNoticeMessageDocument when EdiMessage.IsMoveInNoticeMessage => [new MoveInNoticeMessageWrapper(InboundMessageParseResult, Factory)],
			CancellationOfTransshipmentReport when EdiMessage.IsCancellataionOfTransshipmentReportMessage => [new CancellationOfTransshipmentReportDocumentWrapper(InboundMessageParseResult, Factory)],
			HBLCargoCancellationInformationMessageDocument when EdiMessage.IsHBLCargoCancellationInformationMessage => [new HBLCancellationInformationDocumentWrapper(InboundMessageParseResult, Factory)],
			EACNoticeInformationDocument when EdiMessage.IsEACNoticeInformationMessage => [new EACNoticeMessageDocumentWrapper(InboundMessageParseResult, Factory)],
			TransshipmentNoticeSubmission when EdiMessage.IsTransshipmentNoticeSubmissionInformationMessage => [new TransshipmentNoticeSubmissionInformationDocumentWrapper(InboundMessageParseResult, Factory)],
			HBLCargoRegistrationInformationMessageDocument when EdiMessage.IsHBLCargoRegistrationInformationMessage => [new HBLCargoRegistrationInformationWrapper(InboundMessageParseResult, Factory)],
			_ => base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun),
		};
	}

	protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
	}

	public ExportPermitMessageDocumentWrapper GetExportPermitMessageDocumentWrapper() => new(InboundMessageParseResult, Factory);

	public MismatchInformationMessageDocumentWrapper GetMismatchInformationMessageDocumentWrapper() => new(InboundMessageParseResult, Factory);

	public ImportPermitMessageDocumentWrapper GetImportPermitMessageDocumentWrapper() => new(InboundMessageParseResult, Factory);

	public override string GetFilterValue(DocumentFilters filterName)
	{
		return filterName switch
		{
			DocumentFilters.IsJPInspectionNoticeDocumentSupport => EdiMessage.IsInspectionInformationMessage ? YesNoList.Codes.Yes : YesNoList.Codes.No,
			DocumentFilters.IsJPExportPermitMessageDocumentSupport => EdiMessage.IsExportPermitMessage ? YesNoList.Codes.Yes : YesNoList.Codes.No,
			DocumentFilters.IsJPImportPermitMessageDocumentSupport => EdiMessage.IsImportPermitMessage ? YesNoList.Codes.Yes : YesNoList.Codes.No,
			DocumentFilters.IsJPDiscrepancyNoticeDocumentSupport => EdiMessage.IsMismatchInformationMessage ? YesNoList.Codes.Yes : YesNoList.Codes.No,
			_ => base.GetFilterValue(filterName),
		};
	}

	IJPInboundMessageParseResult InboundMessageParseResult
	{
		get
		{
			if (inboundMessageParseResult == null)
			{
				var parser = NACCSFactoryService.GetInboundMessageParser(EdiMessage.Factory);
				inboundMessageParseResult = parser.Parse(EdiMessage.EM_MessageData);
			}

			return inboundMessageParseResult;
		}
	}
	IJPInboundMessageParseResult inboundMessageParseResult;
}
