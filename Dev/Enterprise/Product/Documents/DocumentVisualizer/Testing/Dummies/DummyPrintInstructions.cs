using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	public sealed class DummyPrintInstructions : IPrintInstructions
	{
		public string Title { get; set; }
		public string[] DeliveryModes { get; set; }
		public string AttachmentFilename { get; set; }
		public string DocumentName { get; set; }
		public int NumberOfCopies { get; set; } = 1;

		public string GetDeliveryTitle(string deliveryMode) => Title;
		public int GetNumberOfCopies(string deliveryMode) => NumberOfCopies;

		public IEnumerable<KeyValuePair<string, string>> GetParametersForDocumentDeliveryLog(string documentName)
		{
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name,
				documentName);

			if (string.CompareOrdinal(documentName, Title) != 0)
			{
				yield return new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					Title);
			}
		}
	}
}
