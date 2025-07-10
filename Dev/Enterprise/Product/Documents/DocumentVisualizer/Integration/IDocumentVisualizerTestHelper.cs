using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDocumentVisualizerTestHelper
	{
		BusinessObject CreateBusinessObjectWithDocumentVisualiserSupport(BusinessObjectFactory factory);
		BusinessObject CreateTemplate(BusinessObjectFactory factory, string definition);
	}
}
