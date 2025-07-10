using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015ConsignmentItemPreviousDocumentProvider : IIE013AndIE015ConsignmentItemPreviousDocument
	{
		public IE013AndIE015ConsignmentItemPreviousDocumentProvider(CusSupportingInfo previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly CusSupportingInfo previousDocument;

		public int GoodsItemNumber => previousDocument.CSI_ItemNumber;

		public string TypeOfPackages => previousDocument.CSI_UnitOfQuantity2;

		public int NumberOfPackages => previousDocument.CSI_Quantity2.ToZInt();

		public string MeasurementUnitAndQualifier => previousDocument.CSI_UnitOfQuantity;

		public decimal Quantity => previousDocument.CSI_Quantity;

		public string ComplementOfInformation => previousDocument.CSI_ReferenceNumber2;

		public string Type => previousDocument.CSI_Code;

		public string Reference => previousDocument.CSI_ReferenceNumber;
	}
}
