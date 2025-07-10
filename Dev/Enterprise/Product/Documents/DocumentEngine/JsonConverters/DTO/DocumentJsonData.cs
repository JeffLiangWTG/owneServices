using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text.Json.Serialization;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine
{
	class ReportSerializationInfoJsonData
	{
		public string User { get; set; }
		public Guid Branch { get; set; }
		public Guid Department { get; set; }
		public DocumentJsonData Report { get; set; }
		public string Language { get; set; }
		public DeliveryInstructionsJsonData Instructions { get; set; }
	}

	class DocumentJsonData
	{
		public string ReportName { get; set; }
		public CollectionOfIFilterJsonData FilterCollection { get; set; }
		public OptionalTemplateSheetCollectionJsonData OptionalTemplateSheetCollection { get; set; }
		public SortOrderJsonData SelectedSortOrder { get; set; }
		public GroupByJsonData SelectedGroupBy { get; set; }
		public bool BreakPageOverride { get; set; }
		public ColumnConfigurationsManagerJsonData ColumnHeadingManager { get; set; }
		public bool OverrideReportDbOption { get; set; }
		public string PageOrientation { get; set; }
		public int TimeOut { get; set; }
		public bool IsEdwDataSource { get; set; }
		public bool ShouldUpdateSchedulableFilters { get; set; }
		public int MaxDop { get; set; }
	}

	class CollectionOfIFilterJsonData
	{
		public List<BaseFieldJsonData> Filters { get; set; }
	}

	class DeliveryInstructionsJsonData
	{
		public string CoverNote { get; set; }
		public string Language { get; set; }
		public bool IncludeCoverNote { get; set; }
		public int NumberOfCopies { get; set; }
	}

	class BitmapCreationExceptionJsonData
	{
		public Decimal Resolution { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public PixelFormat PixFmt { get; set; }
		public string InnerExceptionMessage { get; internal set; }
	}

	class BaseExceptionJsonData
	{
		public string Message { get; set; }
		public string InnerExceptionMessage { get; set; }
	}

	class BODocDataProviderCollectionFindExceptionJsonData : BaseExceptionJsonData
	{ }

	class TemplateInUseExceptionJsonData : BaseExceptionJsonData
	{ }

	class TemplateGeneratingExceptionJsonData : BaseExceptionJsonData
	{ }

	class TemplateFileReadExceptionJsonData : BaseExceptionJsonData
	{ }

	class TemplateDefinitionExceptionJsonData : BaseExceptionJsonData
	{
		public CellReference CellReference { get; set; }
	}

	class MaxConcurrentReportConnectionsExceededJsonData : BaseExceptionJsonData
	{
		public string ReportInfo { get; set; }
	}

	class MacroEvaluationExceptionJsonData : BaseExceptionJsonData
	{ }

	class InvalidMenuTemplateFilterExceptionJsonData : BaseExceptionJsonData
	{ }

	class InvalidGroupByColumnExceptionJsonData : BaseExceptionJsonData
	{
		public string InvalidColumnName { get; set; }
	}

	class FormulaProviderExceptionJsonData : BaseExceptionJsonData
	{ }

	class FieldNotFoundExceptionJsonData : BaseExceptionJsonData
	{ }

	class ExpressionEvaluationExceptionJsonData : BaseExceptionJsonData
	{ }

	class ExcelInterfaceExceptionJsonData : BaseExceptionJsonData
	{
		public ExcelInterfaceExceptionType Type { get; set; }
	}

	class DocumentMenuExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentEngineExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentConvertExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentAreaUnbreakableExceptionJsonData : BaseExceptionJsonData
	{ }

	class CloneAreaExceptionJsonData : BaseExceptionJsonData
	{ }

	class BoxOutOfSectionBodyExceptionJsonData : BaseExceptionJsonData
	{ }

	class BODocDataProviderCollectionFormatExceptionJsonData : BaseExceptionJsonData
	{ }

	class SQLExecutionExceptionJsonData : BaseExceptionJsonData
	{ }

	class ReportSQLTimeoutExceptionJsonData : BaseExceptionJsonData
	{ }

	class ReportProcessingExceptionJsonData : BaseExceptionJsonData
	{ }

	class MissingWorksheetExceptionJsonData : BaseExceptionJsonData
	{
		public string WorksheetName { get; set; }
		public string TemplateName { get; set; }
	}

	class MetadataHasChangedExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentPreviewExceptionJsonData : BaseExceptionJsonData
	{ }

	class ExcelLimitationBaseExceptionJsonData : BaseExceptionJsonData
	{
		public ExcelLimitationsHelper.LimitationType LimitationType { get; set; }
	}

	class DocumentEngineTooManyRowsForThisFileFormatExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentEngineTooManyRowsExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentEngineTooManyColumnsForThisFileFormatExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentEngineTooManyColumnsExceptionJsonData : BaseExceptionJsonData
	{ }

	class DocumentEngineTooLongFormulaForThisFileFormatExceptionJsonData : BaseExceptionJsonData
	{ }

	public class PermitTypePartItemJsonData
	{
		public string Column { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public bool Include { get; set; }
		public PermitTypePartItemCollectionJsonData SubItemsCollection { get; set; }
	}

	public class PermitTypePartItemCollectionJsonData
	{
		public List<PermitTypePartItemJsonData> IncludedItems { get; set; }
	}

	class PrimaryKeyFilterJsonData
	{
		public Guid GuidValue { get; set; }
		public string FieldName { get; set; }
	}

	public class SalesTradeLanePartItemJsonData
	{
		public string Column { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public bool Include { get; set; }
		public SalesTradeLanePartItemCollectionJsonData SubItemsCollection { get; set; }
	}

	public class SalesTradeLanePartItemCollectionJsonData
	{
		public List<SalesTradeLanePartItemJsonData> IncludedItems { get; set; }
	}

	class ColumnConfigurationsManagerJsonData
	{
		public string Headings { get; set; }
	}

	class OptionalTemplateSheetCollectionJsonData
	{
		public List<OptionalTemplateSheetJsonData> Sheets { get; set; }
	}

	class BaseDocumentFieldJsonData
	{
		public string DisplayName { get; set; }
		public string FieldList { get; set; }
		public bool Selected { get; set; }
	}

	class SortOrderJsonData : BaseDocumentFieldJsonData
	{ }

	class GroupByJsonData : BaseDocumentFieldJsonData
	{ }

#if DEBUG
	[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
#endif
	[JsonDerivedType(typeof(ColumnConfigurationFieldJsonData), "ColumnConfigurationFieldJsonData")]
	[JsonDerivedType(typeof(DateBasedAccountingPeriodFieldJsonData), "DateBasedAccountingPeriodFieldJsonData")]
	[JsonDerivedType(typeof(AccountingPeriodFieldJsonData), "AccountingPeriodFieldJsonData")]
	[JsonDerivedType(typeof(AccountingPeriodsRangeFieldJsonData), "AccountingPeriodsRangeFieldJsonData")]
	[JsonDerivedType(typeof(CurrentCompanyFieldJsonData), "CurrentCompanyFieldJsonData")]
	[JsonDerivedType(typeof(CodeListMultipleChoiceJsonData), "CodeListMultipleChoiceJsonData")]
	[JsonDerivedType(typeof(CodeLookupFieldJsonData), "CodeLookupFieldJsonData")]
	[JsonDerivedType(typeof(MaximumDateFieldJsonData), "MaximumDateFieldJsonData")]
	[JsonDerivedType(typeof(MinimumDateFieldJsonData), "MinimumDateFieldJsonData")]
	[JsonDerivedType(typeof(DateFieldJsonData), "DateFieldJsonData")]
	[JsonDerivedType(typeof(PeriodDateRangeFieldJsonData), "PeriodDateRangeFieldJsonData")]
	[JsonDerivedType(typeof(DateRangeFieldJsonData), "DateRangeFieldJsonData")]
	[JsonDerivedType(typeof(DateTimeOffsetFieldJsonData), "DateTimeOffsetFieldJsonData")]
	[JsonDerivedType(typeof(DateTimeOffsetRangeFieldJsonData), "DateTimeOffsetRangeFieldJsonData")]
	[JsonDerivedType(typeof(LookupFieldJsonData), "LookupFieldJsonData")]
	[JsonDerivedType(typeof(MultipleChoiceJsonData), "MultipleChoiceJsonData")]
	[JsonDerivedType(typeof(MultipleSelectionLookupJsonData), "MultipleSelectionLookupJsonData")]
	[JsonDerivedType(typeof(NumberFieldJsonData), "NumberFieldJsonData")]
	[JsonDerivedType(typeof(NumberNotInRangeFieldJsonData), "NumberNotInRangeFieldJsonData")]
	[JsonDerivedType(typeof(NumberRangeFieldJsonData), "NumberRangeFieldJsonData")]
	[JsonDerivedType(typeof(OptionalTemplateSheetJsonData), "OptionalTemplateSheetJsonData")]
	[JsonDerivedType(typeof(AccountingNumberRangeFieldJsonData), "AccountingNumberRangeFieldJsonData")]
	[JsonDerivedType(typeof(OptionGroupJsonData), "OptionGroupJsonData")]
	[JsonDerivedType(typeof(SecurityFilterFieldJsonData), "SecurityFilterFieldJsonData")]
	[JsonDerivedType(typeof(SingleAccountingPeriodFieldJsonData), "SingleAccountingPeriodFieldJsonData")]
	[JsonDerivedType(typeof(SingleAccountingPeriodEndDateFieldJsonData), "SingleAccountingPeriodEndDateFieldJsonData")]
	[JsonDerivedType(typeof(SingleAccountingPeriodStartDateFieldJsonData), "SingleAccountingPeriodStartDateFieldJsonData")]
	[JsonDerivedType(typeof(TextFieldJsonData), "TextFieldJsonData")]
	[JsonDerivedType(typeof(ExactTextFieldJsonData), "ExactTextFieldJsonData")]
	[JsonDerivedType(typeof(TextRangeFieldJsonData), "TextRangeFieldJsonData")]
	[JsonDerivedType(typeof(ZMultiLineTextFieldJsonData), "ZMultiLineTextFieldJsonData")]
	[JsonDerivedType(typeof(RegistrationCodeFieldJsonData), "RegistrationCodeFieldJsonData")]
	[JsonDerivedType(typeof(SalesTradeLaneChecklistFieldJsonData), "SalesTradeLaneChecklistFieldJsonData")]
	[JsonDerivedType(typeof(PermitTypeChecklistFieldJsonData), "PermitTypeChecklistFieldJsonData")]
	[JsonDerivedType(typeof(MonthYearPeriodJsonData), "MonthYearPeriodJsonData")]
	public class BaseFieldJsonData
	{
		public string DisplayName { get; set; }
		public string FieldName { get; set; }
	}

	public class BoolCodeDescription
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public bool Value { get; set; }
	}

	class ColumnConfigurationFieldJsonData : BaseFieldJsonData
	{
		public string LinkPK { get; set; }
		public string ReportID { get; set; }
		public bool Scheduled { get; set; }
		public string Description { get; set; }
		public string LinkCode { get; set; }
		public string UniqueDescription { get; set; }
		public string ManagerSaveToFilterField { get; set; }
		public string Name { get; set; }
	}

	public class DateBasedAccountingPeriodFieldJsonData : AccountingPeriodFieldJsonData
	{ }

	public class AccountingPeriodFieldJsonData : BaseFieldJsonData
	{
		public SchedulableStore<int> SinglePeriod { get; set; }
		public SchedulableStore<int> FromPeriod { get; set; }
		public SchedulableStore<int> ToPeriod { get; set; }
		public SchedulableStore<int> YearToPeriod { get; set; }
		public bool UseSinglePeriod { get; set; }
		public bool UsePeriodRange { get; set; }
		public bool UseYearToPeriod { get; set; }
		public bool UseAllPeriods { get; set; }
	}

	public class AccountingPeriodsRangeFieldJsonData : BaseFieldJsonData
	{
		public bool RequireBothFromAndToPeriods { get; set; }
		public SchedulableStore<int> PeriodFrom { get; set; }
		public SchedulableStore<int> PeriodTo { get; set; }
	}

	public class CurrentCompanyFieldJsonData : BaseFieldJsonData
	{
	}

	public class CodeListMultipleChoiceJsonData : BaseFieldJsonData
	{
		public string Value { get; set; }
	}

	public class CodeLookupFieldJsonData : BaseFieldJsonData
	{
		public string Value { get; set; }
		public string CollectionProviderName { get; set; }
	}

	public class MaximumDateFieldJsonData : DateFieldJsonData
	{ }

	public class MinimumDateFieldJsonData : DateFieldJsonData
	{ }

	public class DateFieldJsonData : BaseDateFieldJsonData<DateTime>
	{
	}

	public class DateTimeOffsetFieldJsonData : BaseDateFieldJsonData<DateTimeOffset>
	{
	}

	public class BaseDateFieldJsonData<T> : BaseFieldJsonData where T : struct
	{
		public SchedulableStore<T> Value { get; set; }
		public string DateFormat { get; set; }
	}

	public class PeriodDateRangeFieldJsonData : DateRangeFieldJsonData
	{ }

	public class DateRangeFieldJsonData : BaseDateRangeFieldJsonData<DateTime>
	{
	}

	public class DateTimeOffsetRangeFieldJsonData : BaseDateRangeFieldJsonData<DateTimeOffset>
	{
	}

	public class BaseDateRangeFieldJsonData<T> : BaseFieldJsonData where T : struct
	{
		public SchedulableStore<T> ValueLow { get; set; }
		public SchedulableStore<T> ValueHigh { get; set; }
		public string DateFormat { get; set; }
		public bool ConvertToUtc { get; set; }
		public bool RequireBothFromAndToDates { get; set; }
		public bool SubstituteMinDateForNullFrom { get; set; }
		public bool SubstituteMaxDateForNullTo { get; set; }
	}

	public class LookupFieldJsonData : BaseFieldJsonData
	{
		public Guid Value { get; set; }
		public string LookupType { get; set; }
		public string CollectionProviderName { get; set; }
	}

	public class MultipleChoiceJsonData : BaseFieldJsonData
	{
		public string Value { get; set; }
		public string List { get; set; }
	}

	public class MultipleSelectionLookupJsonData : BaseFieldJsonData
	{
		public string ModuleID { get; set; }
		public string LookupType { get; set; }
		public bool IsFilterValueExcluded { get; set; }
		public string ValueAsString { get; set; }
		public bool SerialisedByPK { get; set; }
		public List<ColumnInfo> Columns { get; set; }
		public bool UseCodesForWhereClause { get; set; }
	}

	public class NumberFieldJsonData : BaseFieldJsonData
	{
		public decimal Value { get; set; }
		public int DecimalPlaces { get; set; }
	}

	public class NumberNotInRangeFieldJsonData : BaseFieldJsonData
	{
		public decimal? From { get; set; }
		public decimal? To { get; set; }
	}

	public class NumberRangeFieldJsonData : BaseFieldJsonData
	{
		public decimal? From { get; set; }
		public decimal? To { get; set; }
	}

	public class OptionalTemplateSheetJsonData : BaseFieldJsonData
	{
		public bool Selected { get; set; }
		public string Name { get; set; }
	}

	public class AccountingNumberRangeFieldJsonData : BaseFieldJsonData
	{
		public string From { get; set; }
		public string To { get; set; }
	}

	public class OptionGroupJsonData : BaseFieldJsonData
	{
		public List<BoolCodeDescription> Options { get; set; } = new List<BoolCodeDescription>();
	}

	public class SecurityFilterFieldJsonData : BaseFieldJsonData
	{
		public string SecurityRight { get; set; }
		public Guid ItemGuid { get; set; }
		public bool HideDeniedRights { get; set; }
		public bool GroupByStaff { get; set; }
	}

	public class SingleAccountingPeriodFieldJsonData : BaseFieldJsonData
	{
		public SchedulableStore<int> SinglePeriod { get; set; }
	}

	public class SingleAccountingPeriodEndDateFieldJsonData : SingleAccountingPeriodFieldJsonData
	{ }

	public class SingleAccountingPeriodStartDateFieldJsonData : SingleAccountingPeriodFieldJsonData
	{ }

	public class TextFieldJsonData : BaseFieldJsonData
	{
		public string Value { get; set; }
		public string FilterMethod { get; set; }
	}

	public class ExactTextFieldJsonData : TextFieldJsonData
	{ }

	public class TextRangeFieldJsonData : BaseFieldJsonData
	{
		public string From { get; set; }
		public string To { get; set; }
	}

	public class ZMultiLineTextFieldJsonData : TextFieldJsonData
	{ }

	public class RegistrationCodeFieldJsonData : BaseFieldJsonData
	{
		public string CodeCountry { get; set; }
		public string CustomType { get; set; }
		public string FieldNameCodeCountry { get; set; }
		public string FieldNameCustomType { get; set; }
	}

	public class SalesTradeLaneChecklistFieldJsonData : BaseFieldJsonData
	{
		public SalesTradeLanePartItemCollectionJsonData RootItemsCollection { get; set; }
		public string ModeField { get; set; }
		public string TypeField { get; set; }
	}

	public class PermitTypeChecklistFieldJsonData : BaseFieldJsonData
	{
		public string SubTypeField { get; set; }
		public PermitTypePartItemCollectionJsonData RootItemsCollection { get; set; }
	}

	class StringTreeNodeJsonData
	{
		public string Value { get; set; }
		public CellReferenceJsonData CellReference { get; set; }
		public List<StringTreeNodeJsonData> Children { get; set; }
	}

	class CellReferenceJsonData
	{
		public string SheetName { get; set; }
		public string Cell { get; set; }
	}

	class AutoDocumentDeliveryJobJsonData
	{
		public Guid BusinessObjectPK { get; set; }
		public string BusinessObjectType { get; set; }
		public Guid PrinterQueuePK { get; set; }
		public Guid DocumentCommandPK { get; set; }
		public bool SendToDocManager { get; set; }
		public bool SendToEDocs { get; set; }
	}

	class MonthYearPeriodJsonData : BaseFieldJsonData
	{
		public int Month { get; set; }
		public int Year { get; set; }
	}
}
