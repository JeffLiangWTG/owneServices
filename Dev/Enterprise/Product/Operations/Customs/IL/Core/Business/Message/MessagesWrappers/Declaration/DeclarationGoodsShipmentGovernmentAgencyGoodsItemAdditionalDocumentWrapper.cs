using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	partial class DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper(CusSupportingInfo permit)
		{
			this.permit = permit;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument NewOrNull(CusSupportingInfo permit)
			=> permit == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper(permit);

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument.DmExtensions
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper.NewOrNull(permit);

		IIDType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument.Id
			=> IDTypeWrapper.NewOrNull(permit.CSI_ReferenceNumber);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument.LpcoExemptionCode
			=> CodeTypeWrapper.NewOrNull(permit.CSI_Procedure);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument.TypeCode
			=> CodeTypeWrapper.NewOrNull(permit.CSI_SubType);

		readonly CusSupportingInfo permit;
	}
}
