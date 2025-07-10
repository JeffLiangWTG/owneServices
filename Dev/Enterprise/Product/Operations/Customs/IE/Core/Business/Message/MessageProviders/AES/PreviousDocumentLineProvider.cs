using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class PreviousDocumentLineProvider : IPreviousDocumentLine
	{
		public PreviousDocumentLineProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}
		readonly PreviousDocument previousDocument;

		public string LineNumber => previousDocument.CSI_ItemNumber.ToString();

		public string PackageType => previousDocument.CSI_PackType;

		public string PackageQuantity => previousDocument.CSI_PackQty.ToString();

		public string MeasurementUnit => previousDocument.CSI_UnitOfQuantity;

		public decimal Quantity => previousDocument.CSI_Quantity;

		public string Type => previousDocument.CSI_Code;

		public string Reference => previousDocument.CSI_ReferenceNumber;
	}
}
