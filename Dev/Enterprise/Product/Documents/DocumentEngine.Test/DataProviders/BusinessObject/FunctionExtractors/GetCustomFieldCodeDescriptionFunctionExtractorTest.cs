using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetCustomFieldCodeDescriptionFunctionExtractorTest : BaseFunctionExtractorTest
	{
		public void TestGetCustomFieldDescriptionOnChildrenOfCollection()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			addOnRule.XR_IsActive = true;
			addOnRule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='XXX' description='Hello World' />
          <codeDescription code='YYY' description='Bye World' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Name = DummyBizoWithCustomBizo.CustomFieldName;
			column.XC_Type = AddOnColumnDataType.Codes.String;
			column.XC_XR = addOnRule.PK;

			Factory.Save();

			var dummy = Factory.New<DummyBizoWithCustomBizo>();
			dummy.PropertyCollection = new UserDefinedPropertyCollection(dummy) { new ProcessTaskTemplateMatches(template) };
			dummy.SetUserDefinedValue(DummyBizoWithCustomBizo.CustomFieldName, (ZString)"XXX");

			var child1 = Factory.New<ChildDummyBizoWithCustomBizo>();
			child1.PropertyCollection = new UserDefinedPropertyCollection(child1) { new ProcessTaskTemplateMatches(template) };
			child1.Z0_VarCharMax = "Child 1 Text";
			child1.SetUserDefinedValue(DummyBizoWithCustomBizo.CustomFieldName, (ZString)"YYY");
			dummy.Collection.Add(child1);

			var child2 = Factory.New<ChildDummyBizoWithCustomBizo>();
			child2.PropertyCollection = new UserDefinedPropertyCollection(child2) { new ProcessTaskTemplateMatches(template) };
			child2.Z0_VarCharMax = "Child 2 Text";
			child2.SetUserDefinedValue(DummyBizoWithCustomBizo.CustomFieldName, (ZString)"ZZZ");
			dummy.Collection.Add(child2);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var stmTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<GetCustomField(CustomProperty)>-<GetCustomFieldCodeDescription(CustomProperty)>]
{B}-[<GetCustomField(CustomProperty)>-<GetCustomFieldCodeDescriptionWithType(CustomProperty, STR)>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]
{B}-[<Collection.GetCustomField(CustomProperty)>-<Collection.GetCustomFieldCodeDescription(CustomProperty)>]
{A}-[#EndOfReport]");
			stmTemplate.SO_DataContext = "UnitTest";

			var expected =
@"{B}-[XXX-Hello World]
{B}-[XXX-Hello World]
{B}-[Child 1 Text]
{B}-[YYY-Bye World]
{B}-[Child 2 Text]
{B}-[ZZZ-]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = stmTemplate.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Custom fields on children of collection should work.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.SetUserDefinedValue("Test1", (ZString)"Hello");

			var extractor = new GetCustomFieldCodeDescriptionFunctionExtractor("Test1");
			var chainLink = extractor.GetMethodInfoChainLink(dummy.GetType());

			var extractorWithType = new GetCustomFieldCodeDescriptionFunctionExtractor("Test1");
			var chainLinkWithType = extractorWithType.GetMethodInfoChainLink(dummy.GetType());

			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "Hello", chainLink.ReflectOutObject(dummy, dummy));
			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "Hello", chainLinkWithType.ReflectOutObject(dummy, dummy));
		}

		#region Test Classes

		[UserDefinedValues]
		class DummyBizoWithCustomBizo : DummyDocumentSupportable, ICustomFieldProvider
		{
			public const string CustomFieldName = "CustomProperty";

			public DummyBizoWithCustomBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public UserDefinedPropertyCollection PropertyCollection { get; set; }

			public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			{
				return new CustomBusinessObject(Factory, this, PropertyCollection);
			}
		}

		[UserDefinedValues]
		class ChildDummyBizoWithCustomBizo : ChildDummyBusinessObject, ICustomFieldProvider
		{
			public ChildDummyBizoWithCustomBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public UserDefinedPropertyCollection PropertyCollection { get; set; }

			public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			{
				return new CustomBusinessObject(Factory, this, PropertyCollection);
			}
		}

		#endregion
	}
}
