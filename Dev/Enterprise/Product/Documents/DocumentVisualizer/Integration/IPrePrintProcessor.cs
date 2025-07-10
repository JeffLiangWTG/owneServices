using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IPrePrintProcessor
	{
		IPrePrintProcessingResult DoPrePrintProcessing(IDocument document, BusinessObjectFactory deliveryFactory, bool isDraft);
	}
}
