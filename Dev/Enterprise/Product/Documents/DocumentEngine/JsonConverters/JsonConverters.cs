using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine
{
	class ZJsonConverter<T, TData> : JsonConverter<T>, IJsonConverter where T : IJsonSerializable
	{
		public override bool CanConvert(Type objectType) => typeof(T) == objectType;

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var obj = JsonSerializer.Deserialize(ref reader, typeof(TData), options);
			if (obj != null)
			{
				return (T)GetObjectData(obj);
			}

			return default(T);
		}

		public override void Write(Utf8JsonWriter writer, T temperature, JsonSerializerOptions options)
		{
			var data = temperature.GetJsonData();
			JsonSerializer.Serialize(writer, data, options);
		}

		public Type JsonDataType => typeof(TData);

		public object GetObjectData(object value)
			=> Activator.CreateInstance(typeof(T), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { value }, null);
	}

	class DateRangeFieldJsonConverter : ZJsonConverter<DateRangeField, DateRangeFieldJsonData>
	{ }

	class DateTimeOffsetFieldJsonConverter : ZJsonConverter<DateTimeOffsetField, DateTimeOffsetFieldJsonData>
	{ }

	class DateTimeOffsetRangeFieldJsonConverter : ZJsonConverter<DateTimeOffsetRangeField, DateTimeOffsetRangeFieldJsonData>
	{ }

	class PeriodDateRangeFieldJsonConverter : ZJsonConverter<PeriodDateRangeField, PeriodDateRangeFieldJsonData>
	{ }

	class AccountingPeriodFieldJsonConverter : ZJsonConverter<AccountingPeriodField, AccountingPeriodFieldJsonData>
	{ }

	class SingleAccountingPeriodFieldJsonConverter : ZJsonConverter<SingleAccountingPeriodField, SingleAccountingPeriodFieldJsonData>
	{ }

	class SingleAccountingPeriodEndDateFieldJsonConverter : ZJsonConverter<SingleAccountingPeriodEndDateField, SingleAccountingPeriodEndDateFieldJsonData>
	{ }

	class SingleAccountingPeriodStartDateFieldJsonConverter : ZJsonConverter<SingleAccountingPeriodStartDateField, SingleAccountingPeriodStartDateFieldJsonData>
	{ }

	class DateBasedAccountingPeriodFieldJsonConverter : ZJsonConverter<DateBasedAccountingPeriodField, DateBasedAccountingPeriodFieldJsonData>
	{ }

	class AccountingPeriodsRangeFieldJsonConverter : ZJsonConverter<AccountingPeriodsRangeField, AccountingPeriodsRangeFieldJsonData>
	{ }

	class CodeLookupFieldJsonConverter : ZJsonConverter<CodeLookupField, CodeLookupFieldJsonData>
	{ }

	class LookupFieldJsonConverter : ZJsonConverter<LookupField, LookupFieldJsonData>
	{ }

	class MultipleSelectionLookupJsonConverter : ZJsonConverter<MultipleSelectionLookup, MultipleSelectionLookupJsonData>
	{ }

	class CodeListMultipleChoiceJsonConverter : ZJsonConverter<CodeListMultipleChoice, CodeListMultipleChoiceJsonData>
	{ }

	class MultipleChoiceJsonConverter : ZJsonConverter<MultipleChoice, MultipleChoiceJsonData>
	{ }

	class NumberFieldJsonConverter : ZJsonConverter<NumberField, NumberFieldJsonData>
	{ }

	class CurrentCompanyFieldJsonConverter : ZJsonConverter<CurrentCompanyField, CurrentCompanyFieldJsonData>
	{ }

	class DateFieldJsonConverter : ZJsonConverter<DateField, DateFieldJsonData>
	{ }

	class MaximumDateFieldJsonConverter : ZJsonConverter<MaximumDateField, MaximumDateFieldJsonData>
	{ }

	class MinimumDateFieldJsonConverter : ZJsonConverter<MinimumDateField, MinimumDateFieldJsonData>
	{ }

	class ReportSerializationInfoJsonConverter : ZJsonConverter<ReportSerializationInfo, ReportSerializationInfoJsonData>
	{ }

	class ReportJsonConverter : ZJsonConverter<Report, DocumentJsonData>
	{ }

	class DeliveryInstructionsJsonConverter : ZJsonConverter<DeliveryInstructions, DeliveryInstructionsJsonData>
	{ }

	class ColumnConfigurationsManagerJsonConverter : ZJsonConverter<ColumnConfigurationsManager, ColumnConfigurationsManagerJsonData>
	{ }

	class OptionalTemplateSheetJsonConverter : ZJsonConverter<OptionalTemplateSheet, OptionalTemplateSheetJsonData>
	{ }

	class OptionalTemplateSheetCollectionJsonConverter : ZJsonConverter<OptionalTemplateSheetCollection, OptionalTemplateSheetCollectionJsonData>
	{ }

	class OptionGroupJsonConverter : ZJsonConverter<OptionGroup, OptionGroupJsonData>
	{ }

	class PermitTypeChecklistFieldJsonConverter : ZJsonConverter<PermitTypeChecklistField, PermitTypeChecklistFieldJsonData>
	{ }

	class PermitTypePartItemJsonConverter : ZJsonConverter<PermitTypePartItem, PermitTypePartItemJsonData>
	{ }

	class PermitTypePartItemCollectionJsonConverter : ZJsonConverter<PermitTypePartItemCollection, PermitTypePartItemCollectionJsonData>
	{ }

	class PrimaryKeyFilterJsonConverter : ZJsonConverter<PrimaryKeyFilter, PrimaryKeyFilterJsonData>
	{ }

	class RegistrationCodeFieldJsonConverter : ZJsonConverter<RegistrationCodeField, RegistrationCodeFieldJsonData>
	{ }

	class SalesTradeLaneChecklistFieldJsonConverter : ZJsonConverter<SalesTradeLaneChecklistField, SalesTradeLaneChecklistFieldJsonData>
	{ }

	class SalesTradeLanePartItemJsonConverter : ZJsonConverter<SalesTradeLanePartItem, SalesTradeLanePartItemJsonData>
	{ }

	class SalesTradeLanePartItemCollectionJsonConverter : ZJsonConverter<SalesTradeLanePartItemCollection, SalesTradeLanePartItemCollectionJsonData>
	{ }

	class SecurityFilterFieldJsonConverter : ZJsonConverter<SecurityFilterField, SecurityFilterFieldJsonData>
	{ }

	class SortOrderJsonConverter : ZJsonConverter<RuntimeOptions.SortOrder, SortOrderJsonData>
	{ }

	class ExactTextFieldJsonConverter : ZJsonConverter<ExactTextField, ExactTextFieldJsonData>
	{ }

	class TextFieldJsonConverter : ZJsonConverter<TextField, TextFieldJsonData>
	{ }

	class TextRangeFieldJsonConverter : ZJsonConverter<TextRangeField, TextRangeFieldJsonData>
	{ }

	class GroupByJsonConverter : ZJsonConverter<GroupBy, GroupByJsonData>
	{ }

	class CollectionOfIFilterJsonConverter : ZJsonConverter<CollectionOfIFilter, CollectionOfIFilterJsonData>
	{ }

	class AccountingNumberRangeFieldJsonConverter : ZJsonConverter<AccountingNumberRangeField, AccountingNumberRangeFieldJsonData>
	{ }

	class NumberNotInRangeFieldJsonConverter : ZJsonConverter<NumberNotInRangeField, NumberNotInRangeFieldJsonData>
	{ }

	class NumberRangeFieldJsonConverter : ZJsonConverter<NumberRangeField, NumberRangeFieldJsonData>
	{ }

	class ColumnConfigurationFieldJsonConverter : ZJsonConverter<ColumnConfigurationField, ColumnConfigurationFieldJsonData>
	{ }

	class BitmapCreationExceptionJsonConverter : ZJsonConverter<BitmapCreationException, BitmapCreationExceptionJsonData>
	{ }

	class BODocDataProviderCollectionFindExceptionJsonConverter : ZJsonConverter<BODocDataProviderCollectionFindException, BODocDataProviderCollectionFindExceptionJsonData>
	{ }

	class TemplateInUseExceptionJsonConverter : ZJsonConverter<TemplateInUseException, TemplateInUseExceptionJsonData>
	{ }

	class TemplateGeneratingExceptionJsonConverter : ZJsonConverter<TemplateGeneratingException, TemplateGeneratingExceptionJsonData>
	{ }

	class TemplateFileReadExceptionJsonConverter : ZJsonConverter<TemplateFileReadException, TemplateFileReadExceptionJsonData>
	{ }

	class TemplateDefinitionExceptionJsonConverter : ZJsonConverter<TemplateDefinitionException, TemplateDefinitionExceptionJsonData>
	{ }

	class MaxConcurrentReportConnectionsExceededJsonConverter : ZJsonConverter<MaxConcurrentReportConnectionsExceeded, MaxConcurrentReportConnectionsExceededJsonData>
	{ }

	class MacroEvaluationExceptionJsonConverter : ZJsonConverter<MacroEvaluationException, MacroEvaluationExceptionJsonData>
	{ }

	class InvalidMenuTemplateFilterExceptionJsonConverter : ZJsonConverter<InvalidMenuTemplateFilterException, InvalidMenuTemplateFilterExceptionJsonData>
	{ }

	class InvalidGroupByColumnExceptionJsonConverter : ZJsonConverter<InvalidGroupByColumnException, InvalidGroupByColumnExceptionJsonData>
	{ }

	class FormulaProviderExceptionJsonConverter : ZJsonConverter<FormulaProviderException, FormulaProviderExceptionJsonData>
	{ }

	class FieldNotFoundExceptionJsonConverter : ZJsonConverter<FieldNotFoundException, FieldNotFoundExceptionJsonData>
	{ }

	class ExpressionEvaluationExceptionJsonConverter : ZJsonConverter<ExpressionEvaluationException, ExpressionEvaluationExceptionJsonData>
	{ }

	class ExcelInterfaceExceptionJsonConverter : ZJsonConverter<ExcelInterfaceException, ExcelInterfaceExceptionJsonData>
	{ }

	class DocumentMenuExceptionJsonConverter : ZJsonConverter<DocumentMenuException, DocumentMenuExceptionJsonData>
	{ }

	class DocumentEngineExceptionJsonConverter : ZJsonConverter<DocumentEngineException, DocumentEngineExceptionJsonData>
	{ }

	class DocumentConvertExceptionJsonConverter : ZJsonConverter<DocumentConvertException, DocumentConvertExceptionJsonData>
	{ }

	class DocumentAreaUnbreakableExceptionJsonConverter : ZJsonConverter<DocumentAreaUnbreakableException, DocumentAreaUnbreakableExceptionJsonData>
	{ }

	class CloneAreaExceptionJsonConverter : ZJsonConverter<CloneAreaException, CloneAreaExceptionJsonData>
	{ }

	class BoxOutOfSectionBodyExceptionJsonConverter : ZJsonConverter<BoxOutOfSectionBodyException, BoxOutOfSectionBodyExceptionJsonData>
	{ }

	class BODocDataProviderCollectionFormatExceptionJsonConverter : ZJsonConverter<BODocDataProviderCollectionFormatException, BODocDataProviderCollectionFormatExceptionJsonData>
	{ }

	class SQLExecutionExceptionJsonConverter : ZJsonConverter<SQLExecutionException, SQLExecutionExceptionJsonData>
	{ }

	class ReportSQLTimeoutExceptionJsonConverter : ZJsonConverter<ReportSQLTimeoutException, ReportSQLTimeoutExceptionJsonData>
	{ }

	class ReportProcessingExceptionJsonConverter : ZJsonConverter<ReportProcessingException, ReportProcessingExceptionJsonData>
	{ }

	class MissingWorksheetExceptionJsonConverter : ZJsonConverter<MissingWorksheetException, MissingWorksheetExceptionJsonData>
	{ }

	class MetadataHasChangedExceptionJsonConverter : ZJsonConverter<MetadataHasChangedException, MetadataHasChangedExceptionJsonData>
	{ }

	class DocumentPreviewExceptionJsonConverter : ZJsonConverter<DocumentPreviewException, DocumentPreviewExceptionJsonData>
	{ }

	class ExcelLimitationBaseExceptionJsonConverter : ZJsonConverter<ExcelLimitationBaseException, ExcelLimitationBaseExceptionJsonData>
	{ }

	class DocumentEngineTooManyRowsForThisFileFormatExceptionJsonConverter : ZJsonConverter<DocumentEngineTooManyRowsForThisFileFormatException, DocumentEngineTooManyRowsForThisFileFormatExceptionJsonData>
	{ }

	class DocumentEngineTooManyRowsExceptionJsonConverter : ZJsonConverter<DocumentEngineTooManyRowsException, DocumentEngineTooManyRowsExceptionJsonData>
	{ }

	class DocumentEngineTooManyColumnsForThisFileFormatExceptionJsonConverter : ZJsonConverter<DocumentEngineTooManyColumnsForThisFileFormatException, DocumentEngineTooManyColumnsForThisFileFormatExceptionJsonData>
	{ }

	class DocumentEngineTooManyColumnsExceptionJsonConverter : ZJsonConverter<DocumentEngineTooManyColumnsException, DocumentEngineTooManyColumnsExceptionJsonData>
	{ }

	class DocumentEngineTooLongFormulaForThisFileFormatExceptionJsonConverter : ZJsonConverter<DocumentEngineTooLongFormulaForThisFileFormatException, DocumentEngineTooLongFormulaForThisFileFormatExceptionJsonData>
	{ }

	public class DecimalJsonConverter : JsonConverter<decimal>
	{
		public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Number)
			{
				return reader.GetDecimal();	
			}
			return decimal.Parse(reader.GetString(), NumberStyles.Any);
		}

		public override void Write(Utf8JsonWriter writer, decimal temperature, JsonSerializerOptions options)
		{
			writer.WriteStringValue(new ZDecimal(temperature).ToString());
		}
	}

	public class DateTimeJsonConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return DateTimeOffset.Parse(reader.GetString()).DateTime;
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(new DateTimeOffset(value));
		}
	}

	class StringTreeNodeJsonConverter : ZJsonConverter<StringTreeNode, StringTreeNodeJsonData>
	{ }

	class CellReferenceJsonConverter : ZJsonConverter<CellReference, CellReferenceJsonData>
	{ }

	class AutoDocumentDeliveryJobJsonConverter : ZJsonConverter<AutoDocumentDeliveryJob, AutoDocumentDeliveryJobJsonData>
	{ }

	class MonthYearPeriodFieldJsonConverter : ZJsonConverter<MonthYearPeriodField, MonthYearPeriodJsonData>
	{ }
}
