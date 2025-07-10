using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IPrintInstructions
	{
		string Title { get; }
		string[] DeliveryModes { get; }
		string AttachmentFilename { get; }
		string DocumentName { get; }

		string GetDeliveryTitle(string deliveryMode);
		int GetNumberOfCopies(string deliveryMode);

		IEnumerable<KeyValuePair<string, string>> GetParametersForDocumentDeliveryLog(string documentName);
	}
}
