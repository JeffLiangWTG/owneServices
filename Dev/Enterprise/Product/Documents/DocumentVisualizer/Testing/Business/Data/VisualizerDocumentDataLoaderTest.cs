using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class VisualizerDocumentDataLoaderTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var dummy = Factory.New<DummyWithUXmlSupport>();

			var menu = Factory.New<VisualizerMenuItem>();
			menu.SU_MenuName = "test";

			var template = Factory.New<StmTemplate>();

			var documentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot.SI_SU = menu.PK;
			documentPivot.SI_SO = template.PK;

			var documentData = dummy.LoadOrCreateDocumentData("xxx");

			var loader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			AssertContainsExactElementsInAnyOrder("unsaved document data", documentData, loader.Load(dummy));

			Factory.Save();

			Factory.ClearQueryCache();

			AssertContainsExactElementsInAnyOrder("saved document data", documentData, loader.Load(dummy));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "IDocumentSupportable is a required redundant cast")]
		public void TestLoadDocument()
		{
			var shipmentMenuItem = Factory.New<VisualizerMenuItem>();
			shipmentMenuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			shipmentMenuItem.SU_MenuName = "TST1";

			var shipmentTemplate = Factory.New<StmTemplate>();
			shipmentTemplate.SO_Name = "Shipment Form";
			shipmentTemplate.SO_TemplateType = "FRM";

			var shipmentDocumentPivot = Factory.New<VisualizerMenuTemplatePivot>();
			shipmentDocumentPivot.SI_DocumentTitle = "Test DOC 1";
			shipmentDocumentPivot.SI_DataStoreName = "AAA";
			shipmentDocumentPivot.SI_SU = shipmentMenuItem.PK;
			shipmentDocumentPivot.SI_SO = shipmentTemplate.PK;

			Factory.Save();

			ReleaseFactory();

			var dummy = Factory.New<DummyWithUXmlSupport>();

			AssertEquals("prerequsite - dummy data BusinessContext", BusinessContext.Test, ((IDocumentSupportable)dummy).DocumentSupporter.BusinessContext);

			var menu1 = Factory.New<VisualizerMenuItem>();
			menu1.SU_BusinessContext = nameof(BusinessContext.Test);
			menu1.SU_MenuName = "TST1";

			var menu2 = Factory.New<VisualizerMenuItem>();
			menu2.SU_BusinessContext = nameof(BusinessContext.Test);
			menu2.SU_MenuName = "TST2";

			var template1 = Factory.New<StmTemplate>();
			template1.SO_Name = "AAA Form Report";
			template1.SO_TemplateType = "FRM";

			var template2 = Factory.New<StmTemplate>();
			template2.SO_Name = "BBB Document Report";
			template2.SO_TemplateType = "DOC";

			var documentPivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot1.SI_DocumentTitle = "Test DOC 1";
			documentPivot1.SI_DataStoreName = "AAA";
			documentPivot1.SI_SU = menu1.PK;
			documentPivot1.SI_SO = template1.PK;

			var documentPivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			documentPivot2.SI_DocumentTitle = "TEST DOC 2";
			documentPivot2.SI_DataStoreName = "BBB";
			documentPivot2.SI_SU = menu2.PK;
			documentPivot2.SI_SO = template2.PK;

			Factory.Save();

			var documentData = dummy.LoadOrCreateDocumentData("AAA");

			var loader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			AssertEquals("unsaved document data", documentData, loader.Load(dummy, "AAA"));

			Factory.Save();

			Factory.ClearQueryCache();

			AssertEquals("unsaved document data", documentData, loader.Load(dummy, "AAA"));

			loader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();

			AssertNull(loader.Load(dummy, "Test DOC 2"));
		}
	}
}
