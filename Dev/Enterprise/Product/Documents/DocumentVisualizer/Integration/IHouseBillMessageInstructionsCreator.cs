using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IHouseBillMessageInstructionsCreator
	{
		IMessageInstructions GetMessageInstructions(BusinessObject parent, IDocumentPivot pivot, IHouseBillTemplate template);
	}
}
