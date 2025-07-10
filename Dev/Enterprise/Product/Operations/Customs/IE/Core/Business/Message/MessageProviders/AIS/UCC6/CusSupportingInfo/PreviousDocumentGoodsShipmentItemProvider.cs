using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class PreviousDocumentGoodsShipmentItemProvider : IPreviousDocumentGoodsShipmentItem
	{
		readonly PreviousDocument previousDocument;

		public PreviousDocumentGoodsShipmentItemProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		}

		public string Type => previousDocument.CSI_Code;

		public string Reference => previousDocument.CSI_ReferenceNumber;

		public DateTime DateOfAcceptance => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(previousDocument.CSI_DateOfIssue);

		public string CcQualifier => null;

		public string GoodsItemIdentifier => previousDocument.CSI_ItemNumber.ToString();

		public string MeasurementUnitAndQualifier => previousDocument.CSI_UnitOfQuantity;

		public string NumberOfPackages => previousDocument.CSI_Quantity2.ToString();

		public decimal Quantity => previousDocument.CSI_Quantity;

		public string TypeOfPackages => previousDocument.CSI_UnitOfQuantity2;
	}
}
