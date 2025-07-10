using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CPreviousDocumentWithGoodsItemNumberProvider : CC043CDocumentProvider, ISupportComplementOfInformation, ISupportGoodsItemNumber
	{
		public CC043CPreviousDocumentWithGoodsItemNumberProvider(IIE043Document previousDocument) : base(previousDocument)
		{
			if (previousDocument is PreviousDocumentType04 document)
			{
				this.previousDocument = document;
			}
		}

		readonly PreviousDocumentType04 previousDocument;

		public string ComplementOfInformation => previousDocument?.ComplementOfInformation ?? ZString.Empty;
		public string GoodsItemNumber => previousDocument?.GoodsItemNumber ?? ZString.Empty;
	}
}
