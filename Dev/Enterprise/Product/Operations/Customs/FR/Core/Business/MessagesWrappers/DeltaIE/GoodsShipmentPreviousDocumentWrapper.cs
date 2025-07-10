using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsShipmentPreviousDocumentWrapper : IGoodsShipmentPreviousDocument
	{
		GoodsShipmentPreviousDocumentWrapper(PreviousDocument document, string customsOffice)
		{
			previousDocument = Argument.NotNull(document, nameof(document));
			this.customsOffice = customsOffice;
		}
		readonly PreviousDocument previousDocument;
		readonly string customsOffice;

		public static GoodsShipmentPreviousDocumentWrapper New(PreviousDocument document, string customsOffice) => document == null ? null : new GoodsShipmentPreviousDocumentWrapper(document, customsOffice);

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = previousDocument.CSI_ReferenceNumber);
		string referenceNumber;

		public string Type => type ?? (type = previousDocument.CSI_Code);
		string type;
	}
}
