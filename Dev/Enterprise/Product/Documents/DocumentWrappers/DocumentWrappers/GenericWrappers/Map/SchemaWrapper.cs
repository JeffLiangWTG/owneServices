using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Core;
using IWantToKnowMyParentDocumentSupporter = Enterprise.DocumentEngine.MenuCustomisationDocumentSupporter.IWantToKnowMyParentDocumentSupporter;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("TitleText")]
	public class SchemaWrapper : GenericWrapper, IWantToKnowMyParentDocumentSupporter
	{
		public SchemaWrapper(Type typeOfWrapperToMap, ZString titleText, BusinessObjectFactory factory)
			: base(null, factory)
		{
			TypeOfWrapperToMap = typeOfWrapperToMap;
			this.titleText = titleText;
		}
		internal readonly Type TypeOfWrapperToMap;
		readonly ZString titleText;

		public ZString TitleText => titleText;

		public SyntaxAndFormattingWrapperCollection SyntaxAndFormattingCommands => syntaxAndFormattingCommands ??
			(syntaxAndFormattingCommands = new SyntaxAndFormattingWrapperCollection(Factory));

		SyntaxAndFormattingWrapperCollection syntaxAndFormattingCommands;

		public TableWrapperCollection Tables
		{
			get
			{
				if (tables == null && TypeOfWrapperToMap != null)
				{
					tables = new TableWrapperCollection(GenericWrapperMapper.GetMapAsTableList(TypeOfWrapperToMap, true, true), Factory);
				}
				return tables;
			}
		}
		TableWrapperCollection tables;

		#region IWantToKnowMyParentDocumentSupporter Members
		public MenuCustomisationDocumentSupporter ParentDocumentSupporter => parentDocumentSupporter;
		MenuCustomisationDocumentSupporter parentDocumentSupporter;

		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames")]
		void IWantToKnowMyParentDocumentSupporter.SetParentDocumentSupporter(MenuCustomisationDocumentSupporter parentDocumentSupporter)
		{
			this.parentDocumentSupporter = parentDocumentSupporter;
		}
		#endregion

		public MacroWrapperCollection Macros
		{
			get
			{
				if (macros == null && ParentDocumentSupporter != null)
				{
					macros = new MacroWrapperCollection(ParentDocumentSupporter.MacroValueProviderMaps, Factory);
				}
				return macros;
			}
		}
		MacroWrapperCollection macros;

		AreaUseageWrapperCollection areaUsages;
		public AreaUseageWrapperCollection AreaUsages => areaUsages ??
			(areaUsages = new AreaUseageWrapperCollection(Factory));

		public SpecialSheetUseageWrapperCollection ReportSpecialSheetSummaryUseages
		{
			get
			{
				if (reportSpecialSheetSummaryUseages == null)
				{
					reportSpecialSheetSummaryUseages = new SpecialSheetUseageWrapperCollection(Factory, new CodeDescriptionPairList());
					reportSpecialSheetSummaryUseages.Add(ReportFilterSheetName);
					reportSpecialSheetSummaryUseages.Add(ReportSortSheetName);
					reportSpecialSheetSummaryUseages.Add(ReportOptionalTemplateSheetName);
					reportSpecialSheetSummaryUseages.Add(ReportGroupBySheetName);
					//specialSheetSummaryUseages.Add(ConstantsSheetName); // Hide the documentation for Constants sheet for now
					reportSpecialSheetSummaryUseages.Add(FlexCelScaleSheetName);
				}
				return reportSpecialSheetSummaryUseages;
			}
		}

		SpecialSheetUseageWrapperCollection reportSpecialSheetSummaryUseages;

		public SpecialSheetUseageWrapperCollection DocumentSpecialSheetSummaryUseages
		{
			get
			{
				if (documentSpecialSheetSummaryUseages == null)
				{
					documentSpecialSheetSummaryUseages = new SpecialSheetUseageWrapperCollection(Factory, new CodeDescriptionPairList());
					documentSpecialSheetSummaryUseages.Add(DocumentUserDefinedFieldSheetName);
					//specialSheetSummaryUseages.Add(ConstantsSheetName); // Hide the documentation for Constants sheet for now
					documentSpecialSheetSummaryUseages.Add(FlexCelScaleSheetName);
				}
				return documentSpecialSheetSummaryUseages;
			}
		}

		SpecialSheetUseageWrapperCollection documentSpecialSheetSummaryUseages;

		#region Document Fields used

		DocumentUDFFieldPropertiesWrapperCollection documentUserDefinedFieldPropertiesUseages;
		public DocumentUDFFieldPropertiesWrapperCollection DocumentUserDefinedFieldPropertiesUseages => documentUserDefinedFieldPropertiesUseages ??
			(documentUserDefinedFieldPropertiesUseages = new DocumentUDFFieldPropertiesWrapperCollection(Factory));

		DocumentUDFFieldBuilderWrapperCollection documentUserDefinedFieldBuilderUseages;
		public DocumentUDFFieldBuilderWrapperCollection DocumentUserDefinedFieldBuilderUseages => documentUserDefinedFieldBuilderUseages ??
			(documentUserDefinedFieldBuilderUseages = new DocumentUDFFieldBuilderWrapperCollection(Factory));

		SpecialSheetUseageWrapperCollection documentUserDefinedFieldUseages;
		SpecialSheetUseageWrapperCollection DocumentUserDefinedFieldUseages => documentUserDefinedFieldUseages ??
			(documentUserDefinedFieldUseages = new SpecialSheetUseageWrapperCollection(Factory, new FieldsUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper DocumentUserDefinedFieldSheetName =>
			DocumentUserDefinedFieldUseages.FindByUseage(FieldsUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper DocumentUserDefinedFieldDisplayName =>
			DocumentUserDefinedFieldUseages.FindByUseage(FieldsUseageCodeDescriptionList.Codes.DisplayName);

		public CodeMultilingualDescriptionWrapper DocumentUserDefinedFieldSupportedProperty =>
			DocumentUserDefinedFieldUseages.FindByUseage(FieldsUseageCodeDescriptionList.Codes.SupportedProperties);

		public CodeMultilingualDescriptionWrapper DocumentUserDefinedFieldPropertyValue =>
			DocumentUserDefinedFieldUseages.FindByUseage(FieldsUseageCodeDescriptionList.Codes.PropertyValue);

		#endregion

		#region Report Filter used

		ReportFilterPropertiesWrapperCollection reportFilterPropertiesUseages;
		public ReportFilterPropertiesWrapperCollection ReportFilterPropertiesUseages => reportFilterPropertiesUseages ??
			(reportFilterPropertiesUseages = new ReportFilterPropertiesWrapperCollection(Factory));

		ReportFilterBuiltInLookupsWrapperCollection reportFilterBuiltInLookupsUseages;
		public ReportFilterBuiltInLookupsWrapperCollection ReportFilterBuiltInLookups => reportFilterBuiltInLookupsUseages ??
			(reportFilterBuiltInLookupsUseages = new ReportFilterBuiltInLookupsWrapperCollection(Factory));

		ReportFilterCodeListLookupsWrapperCollection reportFilterCodeListLookups;

		public ReportFilterCodeListLookupsWrapperCollection ReportFilterCodeListLookups => reportFilterCodeListLookups ??
			(reportFilterCodeListLookups = new ReportFilterCodeListLookupsWrapperCollection(Factory));

		ReportFilterBuilderWrapperCollection reportFilterBuilderUseages;
		public ReportFilterBuilderWrapperCollection ReportFilterBuilderUseages => reportFilterBuilderUseages ??
			(reportFilterBuilderUseages = new ReportFilterBuilderWrapperCollection(Factory));

		SpecialSheetUseageWrapperCollection reportFilterUseages;
		SpecialSheetUseageWrapperCollection ReportFilterUseage => reportFilterUseages ??
			(reportFilterUseages = new SpecialSheetUseageWrapperCollection(Factory, new FilterUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper ReportFilterSheetName =>
			ReportFilterUseage.FindByUseage(FilterUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper ReportFilterDisplayName =>
			ReportFilterUseage.FindByUseage(FilterUseageCodeDescriptionList.Codes.DisplayName);

		public CodeMultilingualDescriptionWrapper ReportFilterSupportedProperty =>
			ReportFilterUseage.FindByUseage(FilterUseageCodeDescriptionList.Codes.SupportedProperties);

		public CodeMultilingualDescriptionWrapper ReportFilterPropertyValue =>
			ReportFilterUseage.FindByUseage(FilterUseageCodeDescriptionList.Codes.PropertyValue);

		#endregion

		#region Report GroupBys used

		SpecialSheetUseageWrapperCollection reportGroupByUseages;
		SpecialSheetUseageWrapperCollection ReportGroupByUseages => reportGroupByUseages ??
			(reportGroupByUseages = new SpecialSheetUseageWrapperCollection(Factory, new GroupByUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper ReportGroupBySheetName =>
			ReportGroupByUseages.FindByUseage(GroupByUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper ReportGroupByDisplayName =>
			ReportGroupByUseages.FindByUseage(GroupByUseageCodeDescriptionList.Codes.DisplayName);

		public CodeMultilingualDescriptionWrapper ReportGroupByFieldList =>
			ReportGroupByUseages.FindByUseage(GroupByUseageCodeDescriptionList.Codes.FieldList);

		public CodeMultilingualDescriptionWrapper ReportGroupByDefaultFlag =>
			ReportGroupByUseages.FindByUseage(GroupByUseageCodeDescriptionList.Codes.DefaultFlag);

		#endregion

		#region Report Optional Templates used

		SpecialSheetUseageWrapperCollection reportOptionalTemplateUseages;
		SpecialSheetUseageWrapperCollection ReportOptionalTemplateUseages => reportOptionalTemplateUseages ??
			(reportOptionalTemplateUseages = new SpecialSheetUseageWrapperCollection(Factory, new OptionalTemplateUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper ReportOptionalTemplateSheetName =>
			ReportOptionalTemplateUseages.FindByUseage(OptionalTemplateUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper ReportOptionalTemplateDisplayName =>
			ReportOptionalTemplateUseages.FindByUseage(OptionalTemplateUseageCodeDescriptionList.Codes.OptionalTemplateSheetName);

		#endregion

		#region Report Sort used

		SpecialSheetUseageWrapperCollection reportSortUseages;
		SpecialSheetUseageWrapperCollection ReportSortUseages => reportSortUseages ??
			(reportSortUseages = new SpecialSheetUseageWrapperCollection(Factory, new SortUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper ReportSortSheetName =>
			ReportSortUseages.FindByUseage(SortUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper ReportSortDisplayName =>
			ReportSortUseages.FindByUseage(SortUseageCodeDescriptionList.Codes.DisplayName);

		public CodeMultilingualDescriptionWrapper ReportSortFieldList =>
			ReportSortUseages.FindByUseage(SortUseageCodeDescriptionList.Codes.FieldList);

		public CodeMultilingualDescriptionWrapper ReportSortDefaultFlag =>
			ReportSortUseages.FindByUseage(SortUseageCodeDescriptionList.Codes.DefaultFlag);

		#endregion

		#region Constants Sheet used

		SpecialSheetUseageWrapperCollection constantsSheetUseages;
		SpecialSheetUseageWrapperCollection ConstantsSheetUseages => constantsSheetUseages ??
			(constantsSheetUseages = new SpecialSheetUseageWrapperCollection(Factory, new ConstantsUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper ConstantsSheetName =>
			ConstantsSheetUseages.FindByUseage(ConstantsUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper ConstantsFieldName =>
			ConstantsSheetUseages.FindByUseage(ConstantsUseageCodeDescriptionList.Codes.ConstantName);

		public CodeMultilingualDescriptionWrapper ConstantsFieldValue =>
			ConstantsSheetUseages.FindByUseage(ConstantsUseageCodeDescriptionList.Codes.ConstantValue);

		#endregion

		#region FlexCelScale used

		SpecialSheetUseageWrapperCollection flexCelScaleSheetUseages;
		SpecialSheetUseageWrapperCollection FlexCelScaleSheetUseages => flexCelScaleSheetUseages ??
			(flexCelScaleSheetUseages = new SpecialSheetUseageWrapperCollection(Factory, new FlexCelScaleUseageCodeDescriptionList()));

		public CodeMultilingualDescriptionWrapper FlexCelScaleSheetName =>
			FlexCelScaleSheetUseages.FindByUseage(FlexCelScaleUseageCodeDescriptionList.Codes.SheetName);

		public CodeMultilingualDescriptionWrapper FlexCelScaleXScale =>
			FlexCelScaleSheetUseages.FindByUseage(FlexCelScaleUseageCodeDescriptionList.Codes.XScale);

		public CodeMultilingualDescriptionWrapper FlexCelScaleYScale =>
			FlexCelScaleSheetUseages.FindByUseage(FlexCelScaleUseageCodeDescriptionList.Codes.YScale);

		#endregion
	}
}
