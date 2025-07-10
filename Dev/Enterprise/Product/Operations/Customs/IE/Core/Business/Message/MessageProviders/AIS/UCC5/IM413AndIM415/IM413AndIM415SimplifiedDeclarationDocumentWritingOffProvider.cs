using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	class IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider : SimplifiedDeclarationDocumentWritingOffProvider, IGoodsShipmentItemTypeSimplifiedDeclarationDocumentsWritingOff
	{
		public static IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider New(PreviousDocument previousDocument)
		{
			IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider result = null;
			if (previousDocument != null)
			{
				result = new IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider(previousDocument);
			}
			return result;
		}

		IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider(PreviousDocument previousDocument) : base(previousDocument)
		{
			this.previousDocument = previousDocument;
		}

		readonly PreviousDocument previousDocument;

		public string MeasurementUnit => previousDocument.CSI_UnitOfQuantity;

		public decimal Quantity => previousDocument.CSI_Quantity;

		public string PackType => previousDocument.CSI_PackType;

		public string PackQuantity => previousDocument.CSI_PackQty.ToString();
	}
}
