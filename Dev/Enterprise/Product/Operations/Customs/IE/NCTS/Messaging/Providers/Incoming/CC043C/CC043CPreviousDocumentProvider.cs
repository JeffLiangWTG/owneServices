using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CPreviousDocumentProvider : CC043CDocumentProvider, ISupportComplementOfInformation
	{
		public CC043CPreviousDocumentProvider(IIE043Document previousDocument) : base(previousDocument)
		{
			if (previousDocument is PreviousDocumentType06 document)
			{
				this.previousDocument = document;
			}
		}

		readonly PreviousDocumentType06 previousDocument;

		public string ComplementOfInformation => previousDocument?.ComplementOfInformation ?? ZString.Empty;
	}
}
