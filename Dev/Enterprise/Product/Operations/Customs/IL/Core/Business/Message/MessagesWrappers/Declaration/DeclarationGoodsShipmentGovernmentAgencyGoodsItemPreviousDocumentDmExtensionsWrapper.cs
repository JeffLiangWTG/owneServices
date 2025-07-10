using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions
	{
		readonly CusSupportingInfo previousDocument;

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper(CusSupportingInfo previousDocument)
		{
			this.previousDocument = previousDocument;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions NewOrNull(CusSupportingInfo previousDocument)
			=> previousDocument == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper(previousDocument);

		IQuantityType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions.QuantityQuantity => QuantityTypeWrapper.NewOrNull(previousDocument.CSI_Quantity, previousDocument.CSI_UnitOfQuantity);

		int? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions.SequenceNumeric => int.TryParse(previousDocument.CSI_ReferenceNumber2, out var result) ? result : null;
	}
}
