using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	[CodeAlive("This class will be used for messages in the future.")]
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapper(CusSupportingInfo previousDocument)
		{
			this.previousDocument = previousDocument;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument NewOrNull(CusSupportingInfo previousDocument)
			=> previousDocument == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentWrapper(previousDocument);

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensions IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument.DmExtensions => DeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocumentDmExtensionsWrapper.NewOrNull(previousDocument);

		IIDType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument.ID => IDTypeWrapper.NewOrNull(previousDocument.CSI_ReferenceNumber);

		decimal? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument.SequenceNumeric => previousDocument.CSI_ItemNumber;

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument.TypeCode => CodeTypeWrapper.NewOrNull(previousDocument.CSI_Code);

		readonly CusSupportingInfo previousDocument;
	}
}
