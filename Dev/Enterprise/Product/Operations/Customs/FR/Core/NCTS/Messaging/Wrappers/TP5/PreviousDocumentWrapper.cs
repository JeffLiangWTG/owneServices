using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class PreviousDocumentWrapper : IPreviousDocument
	{
		PreviousDocumentWrapper(NctsPreviousDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		readonly NctsPreviousDocument document;

		public static PreviousDocumentWrapper New(NctsPreviousDocument document) => document == null ? null : new PreviousDocumentWrapper(document);

		public int? GoodsItemNumber => goodsItemNumber ?? (goodsItemNumber = document.CSI_ItemNumber);
		int? goodsItemNumber;

		public string TypeOfPackages => typeOfPackages ?? (typeOfPackages = document.CSI_UnitOfQuantity2);
		string typeOfPackages;

		public decimal? NumberOfPackages => numberOfPackages ?? (numberOfPackages = document.CSI_Quantity2);
		decimal? numberOfPackages;

		public string MeasurementUnitAndQualifier => measurementUnitAndQualifier ?? (measurementUnitAndQualifier = document.CSI_UnitOfQuantity);
		string measurementUnitAndQualifier;

		public decimal? Quantity => quantity ?? (quantity = document.CSI_Quantity);
		decimal? quantity;

		public string ComplementOfInformation => complementOfInformation ?? (complementOfInformation = document.CSI_ReferenceNumber2);
		string complementOfInformation;

		public string Type => type ?? (type = document.CSI_Code);
		string type;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = document.CSI_ReferenceNumber);
		string referenceNumber;
	}
}
