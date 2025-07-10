using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(VisualizerTemplate))]
	sealed class VisualizerTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var template = Factory.New<VisualizerTemplate>();
			AssertEquals(StmTemplateTypes.Codes.Form, template.SO_TemplateType);
		}

		public void TestExtractDataRelatedInfoFromTemplate()
		{
			var template = Factory.New<VisualizerTemplate>();

			template.SO_Template = TemplateXlsBlob;

			AssertEquals(StmTemplateSchema.Constants.SO_DataContext, "UXML", template.SO_DataContext);
		}

		public void TestDataRelatedInfoResetToTemplateSettingWhileSaving()
		{
			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = TemplateXlsBlob;

			template.SO_DataContext = "SomeContext";

			Factory.Save();

			AssertEquals(StmTemplateSchema.Constants.SO_DataContext, "SomeContext", template.SO_DataContext);
		}

		public void TestSettingOtherPropertyOnTemplateWillNotAffectDataRelatedInfo()
		{
			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = TemplateXlsBlob;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedTemplate = newFactory.Load<VisualizerTemplate>(template.PK);

			reloadedTemplate.SO_IsSystemDefined = true;

			newFactory.Save();

			AssertEquals(StmTemplateSchema.Constants.SO_DataContext, "UXML", reloadedTemplate.SO_DataContext);
		}

		public void TestIsBillOfLadingTemplate()
		{
			var billOfLadingTemplate = Factory.Load<VisualizerTemplate>(new ZGuid("bd92bb18-0a32-4ea7-86fa-3fb0392be24a"));
			Assert("correctly recognized Bill Of Lading template", billOfLadingTemplate.IsBillOfLadingTemplate);

			billOfLadingTemplate.SO_Template = null;
			Assert("correctly recognized non Bill Of Lading template after updating the contents", !billOfLadingTemplate.IsBillOfLadingTemplate);

			var nonBillOfLadingTemplate = Factory.Load<VisualizerTemplate>(new ZGuid("ef9ea643-a64d-46ad-b45c-feb73c339f61"));
			Assert("correctly recognized non Bill Of Lading template", !nonBillOfLadingTemplate.IsBillOfLadingTemplate);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ZBlob templateXlsBlob;
		ZBlob TemplateXlsBlob
		{
			get
			{
				if (templateXlsBlob == null)
				{
					var tempFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
					templateXlsBlob = StmTemplateBase.GetTemplateBlobFromFile(tempFilePath);
				}
				return templateXlsBlob;
			}
		}
	}
}
