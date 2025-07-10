using System.IO;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	public sealed class DocumentVisualizerTestHelper : IDocumentVisualizerTestHelper
	{
		public BusinessObject CreateBusinessObjectWithDocumentVisualiserSupport(BusinessObjectFactory factory) => factory?.New<DummyWithUXmlSupport>();

		BusinessObject IDocumentVisualizerTestHelper.CreateTemplate(BusinessObjectFactory factory, string definition) => CreateTemplate(factory, definition);

		public VisualizerTemplate CreateTemplate(BusinessObjectFactory factory, string definition)
		{
			if (factory == null
				|| string.IsNullOrWhiteSpace(definition))
			{
				return null;
			}

			var worksheet = DummyWorksheet.Parse(definition);

			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			var template = factory.New<VisualizerTemplate>();

			using (var templateStream = new MemoryStream())
			{
				xls.Save(templateStream);
				template.SO_Template = templateStream.ToArray();
			}

			return template;
		}
	}
}
