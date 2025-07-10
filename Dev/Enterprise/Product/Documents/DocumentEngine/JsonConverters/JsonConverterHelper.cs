using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enterprise.DocumentEngine
{
	class JsonConverterHelper
	{
		public static string Serialize(object serializable) =>
			JsonSerializer.Serialize(serializable, GetJsonSerializerOptions());

		public static T Deserialize<T>(string json) where T : IJsonSerializable =>
			JsonSerializer.Deserialize<T>(json, GetJsonSerializerOptions());

		public static object Deserialize(string json, Type type) =>
			JsonSerializer.Deserialize(json, type, GetJsonSerializerOptions());

		internal static JsonSerializerOptions GetJsonSerializerOptions()
		{
			var settings = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};

			foreach (var converter in GetJsonConverters())
			{
				settings.Converters.Add(converter);
			}
			return settings;
		}

		internal static JsonConverter[] GetJsonConverters() =>
			new JsonConverter[]
			{
				new ReportSerializationInfoJsonConverter(),
				new ReportJsonConverter(),
				new DeliveryInstructionsJsonConverter(),
				new ColumnConfigurationsManagerJsonConverter(),
				new OptionalTemplateSheetCollectionJsonConverter(),
				new SortOrderJsonConverter(),
				new GroupByJsonConverter(),
				new PermitTypePartItemJsonConverter(),
				new PermitTypePartItemCollectionJsonConverter(),
				new PrimaryKeyFilterJsonConverter(),
				new SalesTradeLanePartItemJsonConverter(),
				new SalesTradeLanePartItemCollectionJsonConverter(),

				//TODO: WI00738741 - Confirm if serialization is required for AutoDocumentDeliveryJob
				new AutoDocumentDeliveryJobJsonConverter(),

				// Exceptions
				//TODO: WI00740855 - Investigate the use of serialization with these exceptions.
				new BitmapCreationExceptionJsonConverter(),
				new BODocDataProviderCollectionFindExceptionJsonConverter(),
				new TemplateInUseExceptionJsonConverter(),
				new TemplateGeneratingExceptionJsonConverter(),
				new TemplateFileReadExceptionJsonConverter(),
				new TemplateDefinitionExceptionJsonConverter(),
				new MaxConcurrentReportConnectionsExceededJsonConverter(),
				new MacroEvaluationExceptionJsonConverter(),
				new InvalidMenuTemplateFilterExceptionJsonConverter(),
				new InvalidGroupByColumnExceptionJsonConverter(),
				new FormulaProviderExceptionJsonConverter(),
				new FieldNotFoundExceptionJsonConverter(),
				new ExpressionEvaluationExceptionJsonConverter(),
				new ExcelInterfaceExceptionJsonConverter(),
				new DocumentMenuExceptionJsonConverter(),
				new DocumentEngineExceptionJsonConverter(),
				new DocumentConvertExceptionJsonConverter(),
				new DocumentAreaUnbreakableExceptionJsonConverter(),
				new CloneAreaExceptionJsonConverter(),
				new BoxOutOfSectionBodyExceptionJsonConverter(),
				new BODocDataProviderCollectionFormatExceptionJsonConverter(),
				new SQLExecutionExceptionJsonConverter(),
				new ReportSQLTimeoutExceptionJsonConverter(),
				new ReportProcessingExceptionJsonConverter(),
				new MissingWorksheetExceptionJsonConverter(),
				new MetadataHasChangedExceptionJsonConverter(),
				new DocumentPreviewExceptionJsonConverter(),
				new ExcelLimitationBaseExceptionJsonConverter(),
				new DocumentEngineTooManyRowsForThisFileFormatExceptionJsonConverter(),
				new DocumentEngineTooManyRowsExceptionJsonConverter(),
				new DocumentEngineTooManyColumnsForThisFileFormatExceptionJsonConverter(),
				new DocumentEngineTooManyColumnsExceptionJsonConverter(),
				new DocumentEngineTooLongFormulaForThisFileFormatExceptionJsonConverter(),

				// Field
				new DateRangeFieldJsonConverter(),
				new DateTimeOffsetRangeFieldJsonConverter(),
				new PeriodDateRangeFieldJsonConverter(),
				new AccountingPeriodFieldJsonConverter(),
				new CollectionOfIFilterJsonConverter(),
				new ColumnConfigurationFieldJsonConverter(),
				new AccountingNumberRangeFieldJsonConverter(),
				new AccountingPeriodsRangeFieldJsonConverter(),
				new CodeLookupFieldJsonConverter(),
				new CurrentCompanyFieldJsonConverter(),
				new DateFieldJsonConverter(),
				new DateTimeOffsetFieldJsonConverter(),
				new MaximumDateFieldJsonConverter(),
				new MinimumDateFieldJsonConverter(),
				new SingleAccountingPeriodFieldJsonConverter(),
				new SingleAccountingPeriodEndDateFieldJsonConverter(),
				new SingleAccountingPeriodStartDateFieldJsonConverter(),
				new DateBasedAccountingPeriodFieldJsonConverter(),
				new LookupFieldJsonConverter(),
				new MultipleSelectionLookupJsonConverter(),
				new CodeListMultipleChoiceJsonConverter(),
				new MultipleChoiceJsonConverter(),
				new NumberFieldJsonConverter(),
				new OptionalTemplateSheetJsonConverter(),
				new OptionGroupJsonConverter(),
				new PermitTypeChecklistFieldJsonConverter(),
				new RegistrationCodeFieldJsonConverter(),
				new SalesTradeLaneChecklistFieldJsonConverter(),
				new SecurityFilterFieldJsonConverter(),
				new ExactTextFieldJsonConverter(),
				new TextFieldJsonConverter(),
				new TextRangeFieldJsonConverter(),
				new NumberNotInRangeFieldJsonConverter(),
				new NumberRangeFieldJsonConverter(),
				new MonthYearPeriodFieldJsonConverter(),

				// StringTreeNode
				new StringTreeNodeJsonConverter(),
				new CellReferenceJsonConverter(),

				// Basic
				new DecimalJsonConverter(),
				new DateTimeJsonConverter()
			};
	}
}
