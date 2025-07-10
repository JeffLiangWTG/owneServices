using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsShipmentItemPreviousDocumentWrapper : IGoodsShipmentItemPreviousDocument
	{
		GoodsShipmentItemPreviousDocumentWrapper(PreviousDocument document)
		{
			previousDocument = Argument.NotNull(document, nameof(document));
		}

		public static GoodsShipmentItemPreviousDocumentWrapper New(PreviousDocument document) => document == null ? null : new GoodsShipmentItemPreviousDocumentWrapper(document);

		public string CcQualifier => ccQualifier ?? (ccQualifier = CountryCodes.France);
		string ccQualifier;

		public string GoodsItemIdentifier => goodsItemIdentifier ?? (goodsItemIdentifier = previousDocument.CSI_ItemNumber.ToString());
		string goodsItemIdentifier;

		public string MeasurementUnitAndQualifier => measurementUnitAndQualifier ?? (measurementUnitAndQualifier = previousDocument.CSI_UnitOfQuantity);
		string measurementUnitAndQualifier;

		public string NumberOfPackages => numberOfPackages ?? (numberOfPackages = previousDocument.CSI_PackQty.ToString());
		string numberOfPackages;

		public double Quantity => quantity.Equals(0d) ? GetQuantity() : quantity;
		double GetQuantity()
		{
			return quantity = (double)previousDocument.CSI_Quantity;
		}
		double quantity;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = previousDocument.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = previousDocument.CSI_Code);
		string type;

		public string TypeOfPackages => typeOfPackages ?? (typeOfPackages = previousDocument.CSI_PackType);
		string typeOfPackages;

		readonly CusSupportingInfo previousDocument;
	}
}
