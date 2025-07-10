using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class StandardDocumentDescriptorTest : TestCaseWithFactory
	{
		#region TestDocumentDescriptorName

		public void TestDocumentDescriptorName_TemplateHasName()
		{
			var content = @"#Config:Name=""AAA"":DataContext=""UXML"":Language=""EN-AU""
#End
#Body
#End";

			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();
			template.SO_DataContext = "UXML";

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "BBB";

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var worksheet = DummyWorksheet.Parse(content);
			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			using (var templateStream = new MemoryStream())
			{
				xls.Save(templateStream);
				template.SO_Template = templateStream.ToArray();
			}

			var descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("TEST"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals("Should be AAA from the template name.", "AAA", descriptor.Name);
			AssertEquals("Should be AAA from the descriptor name.", "AAA", descriptor.MessageInstructions.DocumentName);

			AssertEquals(nameof(descriptor.DataContext), "UXML", descriptor.DataContext);
			AssertEquals(nameof(descriptor.EnableTranslation), false, descriptor.EnableTranslation);
		}

		public void TestDocumentDescriptorName_TemplateDoNotHaveName()
		{
			var content = @"#Config:DataContext=""UXML""
#End
#Body
#End";

			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();
			template.SO_DataContext = "UXML";

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "BBB";

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var worksheet = DummyWorksheet.Parse(content);
			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			using (var templateStream = new MemoryStream())
			{
				xls.Save(templateStream);
				template.SO_Template = templateStream.ToArray();
			}

			var descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("TEST"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals("Should be BBB from the pivot's document title as the template name is null.", "BBB", descriptor.Name);
			AssertEquals("Should be BBB from the descriptor name.", "BBB", descriptor.MessageInstructions.DocumentName);

			AssertEquals(nameof(descriptor.DataContext), "UXML", descriptor.DataContext);
			AssertEquals(nameof(descriptor.EnableTranslation), false, descriptor.EnableTranslation);
		}

		#endregion

		#region TestGetLoadedDataHasOverride

		public void TestGetLoadedDataHasOverride()
		{
			var dummy = Factory.New<Forwarding.IForwardingConsol>();

			const string xml =
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""ReleaseType"">
    <Entity>
      <Property Name=""Code"">
        <Value>XXX</Value>
      </Property>
    </Entity>
  </Property>
</Entity>";

			var xmlDoc = XDocument.Parse(xml);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = dummy.PK;
			documentData.JDD_ParentTableCode = dummy.TablePrefix;
			documentData.JDD_Name = "xxx";

			documentData.WriteXml(xmlDoc);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var otherDummy = (BusinessObject)otherFactory.Load<Forwarding.IForwardingConsol>(dummy.PK);

			var result = otherDummy.TryCreate(DataContext.UXML);

			Assert("successfully retrieved data", !result.IsFaulted);

			var expr = "\"<ReleaseType.Code>\"".CreateExpression();
			expr.Evaluate(result.Value);

			AssertEquals("Dummy does not have overridden values applied from the saved note xml", false, result.Value.IsOverriddenIncludingChildren);

			var changeset = result.Value.GetOverriddenValuesXml();

			AssertMultilineASCIIEquals("there's no changeset as the override has not been applied", string.Empty, changeset.ToXmlString());
		}

		#endregion

		#region MessageInstructions

		#region TestMessageInstructions_DocumentName

		public void TestMessageInstructions_DocumentName()
		{
			var scope = new MacroScope();
			var context = new IMacroLibrary[] { new StandardLibrary() }.CreateContext();

			var worksheet = DummyWorksheet.Parse(@"#Config:Name=""AAA""
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				scope,
				context,
				"AAA");

			AssertEquals("AAA", messageInstructions.DocumentName);

			messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				scope,
				context,
				"BBB");

			AssertEquals("BBB", messageInstructions.DocumentName);
		}

		#endregion

		#region TestMessageInstructions_DataContext

		public void TestMessageInstructions_DataContext()
		{
			var scope = new MacroScope();
			var context = new IMacroLibrary[] { new StandardLibrary() }.CreateContext();

			var worksheet = DummyWorksheet.Parse(@"#Config:Name=""AAA"":DataContext=""Foo""
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate, scope, context, null);
			AssertEquals("Foo", messageInstructions.DataContext);
		}

		#endregion

		#region TestMessageInstructions_AllowSendMessage

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_AllowSendMessage()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);

			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals(false, descriptor.MessageInstructions.AllowSendMessage);
		}

		public void TestMessageInstructions_AllowSendMessage_UsingMacro()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var dynamicShipment = shipment.MakeDynamic();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Shipping Instruction"":AllowSendMessage=@data.PortOfLoading.Code.Substring(0, 2)==""AU"":
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);

			var messageInstructions = new StandardMessageInstructions(worksheetTemplate,
				new MacroScope(dynamicShipment),
				new IMacroLibrary[] { new StandardLibrary() }.CreateContext(), "TEST");

			AssertEquals(true, messageInstructions.AllowSendMessage);
		}

		#endregion

		#region TestMessageInstructions_AllowSendMessageWithdrawal

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_AllowSendMessageWithdrawal()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);

			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals(false, descriptor.MessageInstructions.AllowSendMessageWithdrawal);
		}

		#endregion

		#region TestMessageInstructions_AllowResetToOriginal

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_AllowResetToOriginal()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);
			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals(true, descriptor.MessageInstructions.AllowResetToOriginal);
		}

		#endregion

		#region TestMessageInstructions_GetMessageRecipient

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_GetMessageRecipient()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);

			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals("Steven", descriptor.MessageInstructions.Recipient);
		}

		#endregion

		#region TestMessageInstructions_GeteHubClientID

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_GeteHubClientID()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);
			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals("client", descriptor.MessageInstructions.EHubClientID);
		}

		#endregion

		#endregion

		#region PrintInstructions

		#region TestPrintInstructions_Title

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrintInstructions_Title()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "title";

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals("title", descriptor.PrintInstructions.Title);
		}

		#endregion

		#region TestMessageInstructions_DeliveryMode

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_DeliveryMode()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_PrintCopyType = nameof(PrintCopyType.EML);

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			IDocumentDescriptor descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertContainsExactElementsInAnyOrder("Delivery Modes",
				new[] { "EML" },
				descriptor.PrintInstructions.DeliveryModes);
		}

		#endregion

		#region TestMessageInstructions_NumberOfCopies

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageInstructions_NumberOfCopies()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestFiles.MessagingConfigTestTemplateFilePath);

			var descriptor = new StandardDocumentDescriptor(documentPivot,
				bizObj.LoadOrCreateDocumentData("xxx"),
				new StandardTemplate(template.GetFlexCelWorksheet()),
				new MacroScope(),
				System.Array.Empty<IMacroLibrary>().CreateContext());

			AssertEquals(1, descriptor.PrintInstructions.GetNumberOfCopies(nameof(PrintCopyType.PRN)));
		}

		#endregion

		#endregion
	}
}
