using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;
using IWantToKnowMyParentDocumentSupporter = Enterprise.DocumentEngine.MenuCustomisationDocumentSupporter.IWantToKnowMyParentDocumentSupporter;
using StmMenuItemBaseCollection = Enterprise.DocumentEngine.Business.StmMenuItemBaseCollection;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map.Testing
{
	[TestedType(typeof(SchemaWrapper))]
	sealed class SchemaWrapperTest : Base.Testing.GenericWrapperTest
	{
		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestMacrosShouldContainsFunctionExtractorProviders()
		{
			var menuCustomisation = DocumentMenuCustomisation.New(null, null, Factory);
			var documentSupporter = new DocumentMenuCustomisationDocumentSupporter(menuCustomisation);

			var wrapper = new SchemaWrapper(null, "", Factory);
			((IWantToKnowMyParentDocumentSupporter)wrapper).SetParentDocumentSupporter(documentSupporter);
			var usages = wrapper.Macros.OfType<MacroWrapper>().Select(x => x.Useage.ToString()).ToArray();

			AssertCollectionContains("<GetCustomField({CustomFieldName})>", usages);
			AssertCollectionContains("<GetCustomFieldWithType({CustomFieldName}, {CustomFieldType})>", usages);
			AssertCollectionContains("<GetCustomFieldCodeDescription({CustomFieldName})>", usages);
			AssertCollectionContains("<GetCustomFieldCodeDescriptionWithType({CustomFieldName}, {CustomFieldType})>", usages);
			AssertCollectionContains("<GetEventLastDateTime({EventCode})>", usages);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestIWantToKnowMyParentDocumentSupporter()
		{
			SchemaWrapper wrapper = new SchemaWrapper(null, "", Factory);
			IWantToKnowMyParentDocumentSupporter gratefulChild = wrapper;
			AssertNotNull("Wrapper should implement IWantToKnowMyParentDocumentSupporter", gratefulChild);
			StmMenuItemBaseCollection menuItemCollection = new StmMenuItemBaseCollection(Factory);
			DocumentMenuCustomisation menuCustomisation = DocumentMenuCustomisation.New(null, null, Factory);
			MenuCustomisationDocumentSupporter documentSupporter = new DocumentMenuCustomisationDocumentSupporter(menuCustomisation);
			gratefulChild.SetParentDocumentSupporter(documentSupporter);
			AssertEquals("ParentDocumentSupporter should return it's namesake", documentSupporter, gratefulChild.ParentDocumentSupporter);
			AssertNotNull("wrapper.Macros should be available after the ParentDocumentSupporter is set.", wrapper.Macros);
			Assert("wrapper.Macros.Count > 50 (there should at least be a FEW macros....)", wrapper.Macros.Count > 10);
		}

		public override void TestWrapperMappingsEmpty()
		{
			SchemaWrapper wrapperEmpty = new SchemaWrapper(null, "", Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.TitleText", ZString.Empty, wrapperEmpty.TitleText);

			Assert("wrapperEmpty.Chapters.Count > 5", wrapperEmpty.SyntaxAndFormattingCommands.Count > 5);
			AssertEquals("wrapperEmpty.Tables", null, wrapperEmpty.Tables);
			AssertEquals("wrapperEmpty.Macros", null, wrapperEmpty.Macros);
		}

		public void TestWrapperMappingFull()
		{
			SchemaWrapper wrapperFull = new SchemaWrapper(typeof(SchemaWrapper), "Overall We Gloat", Factory);
			AssertEquals("wrapperFull.ToString()", "Overall We Gloat", wrapperFull.ToString());
			AssertEquals("wrapperFull.TitleText", "Overall We Gloat", wrapperFull.TitleText);

			Assert("wrapperFull.Chapters.Count > 5", wrapperFull.SyntaxAndFormattingCommands.Count > 5);
			AssertEquals("wrapperFull.Tables.Count", 7, wrapperFull.Tables.Count);
			Assert("wrapperFull.ReportFilterPropertiesUseages.Count > 5", wrapperFull.ReportFilterPropertiesUseages.Count > 5);
			Assert("wrapperFull.ReportFilterBuiltInLookups.Count > 5", wrapperFull.ReportFilterBuiltInLookups.Count > 5);
			Assert("wrapperFull.ReportFilterBuilderUseages.Count > 5", wrapperFull.ReportFilterBuilderUseages.Count > 5);

			AssertEquals("Display Name", wrapperFull.ReportFilterDisplayName.Useage);
			AssertEquals("The label displayed in the Filters section in the Report Filter Form", wrapperFull.ReportFilterDisplayName.Explanation);

			AssertEquals("Supported Properties", wrapperFull.ReportFilterSupportedProperty.Useage);
			AssertEquals("The property", wrapperFull.ReportFilterSupportedProperty.Explanation);

			AssertEquals("Property Value", wrapperFull.ReportFilterPropertyValue.Useage);
			AssertEquals("The value of each property", wrapperFull.ReportFilterPropertyValue.Explanation);
		}

		public void TestReportFilterCodeListLookups()
		{
			var wrapper = new SchemaWrapper(null, "", Factory);
			AssertNotNull(wrapper.ReportFilterCodeListLookups);
			Assert(wrapper.ReportFilterCodeListLookups.Count > 0);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Schema                                      (Default Field: TitleText)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ConstantsFieldName                      CodeMultilingualDescription
ConstantsFieldValue                     CodeMultilingualDescription
ConstantsSheetName                      CodeMultilingualDescription
DocumentUserDefinedFieldDisplayName     CodeMultilingualDescription
DocumentUserDefinedFieldPropertyValue   CodeMultilingualDescription
DocumentUserDefinedFieldSheetName       CodeMultilingualDescription
DocumentUserDefinedFieldSupportedProperty  CodeMultilingualDescription
FlexCelScaleSheetName                   CodeMultilingualDescription
FlexCelScaleXScale                      CodeMultilingualDescription
FlexCelScaleYScale                      CodeMultilingualDescription
ReportFilterDisplayName                 CodeMultilingualDescription
ReportFilterPropertyValue               CodeMultilingualDescription
ReportFilterSheetName                   CodeMultilingualDescription
ReportFilterSupportedProperty           CodeMultilingualDescription
ReportGroupByDefaultFlag                CodeMultilingualDescription
ReportGroupByDisplayName                CodeMultilingualDescription
ReportGroupByFieldList                  CodeMultilingualDescription
ReportGroupBySheetName                  CodeMultilingualDescription
ReportOptionalTemplateDisplayName       CodeMultilingualDescription
ReportOptionalTemplateSheetName         CodeMultilingualDescription
ReportSortDefaultFlag                   CodeMultilingualDescription
ReportSortDisplayName                   CodeMultilingualDescription
ReportSortFieldList                     CodeMultilingualDescription
ReportSortSheetName                     CodeMultilingualDescription
TitleText                               String

AreaUsages                              AreaUseage Collection
DocumentUserDefinedFieldBuilderUseages  DocumentUDFFieldBuilder Collection
DocumentUserDefinedFieldPropertiesUseages  DocumentUDFFieldProperties Collection
Macros                                  Macro Collection
ReportFilterBuilderUseages              ReportFilterBuilder Collection
ReportFilterBuiltInLookups              ReportFilterBuiltInLookups Collection
ReportFilterCodeListLookups             ReportFilterCodeListLookups Collection
ReportFilterPropertiesUseages           ReportFilterProperties Collection
DocumentSpecialSheetSummaryUseages      SpecialSheetUseage Collection
ReportSpecialSheetSummaryUseages        SpecialSheetUseage Collection
SyntaxAndFormattingCommands             SyntaxAndFormatting Collection
Tables                                  Table Collection";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"ConstantsFieldName : Constant Name
ConstantsFieldValue : Constant Value
ConstantsSheetName : Constants
DocumentUserDefinedFieldDisplayName : Display Name
DocumentUserDefinedFieldPropertyValue : Property Value
DocumentUserDefinedFieldSheetName : Fields
DocumentUserDefinedFieldSupportedProperty : Supported Properties
FlexCelScaleSheetName : #FlexCelScale
FlexCelScaleXScale : X Scale
FlexCelScaleYScale : Y Scale
Registry : (No Default Field Value Available on Registry)
ReportFilterDisplayName : Display Name
ReportFilterPropertyValue : Property Value
ReportFilterSheetName : Filters
ReportFilterSupportedProperty : Supported Properties
ReportGroupByDefaultFlag : Default Flag
ReportGroupByDisplayName : Display Name
ReportGroupByFieldList : Field List
ReportGroupBySheetName : GroupBy
ReportOptionalTemplateDisplayName : Optional Template Sheet Name
ReportOptionalTemplateSheetName : Optional Templates
ReportSortDefaultFlag : Default Flag
ReportSortDisplayName : Display Name
ReportSortFieldList : Field List
ReportSortSheetName : Sort"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new SchemaWrapper(typeof(SchemaWrapper), "DipShit", Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new SchemaWrapper(null, "", Factory);
		}
	}
}
