using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	internal class DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper(CusSupportingInfo permit)
		{
			this.permit = permit;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions NewOrNull(CusSupportingInfo permit)
			=> permit == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensionsWrapper(permit);

		IIDType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions.ExternalAttachmentID
			=> null;

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions.LpcoTypeCode
			=> CodeTypeWrapper.NewOrNull(permit.CSI_IssuerType);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions.RequirementLicenseType
			=> CodeTypeWrapper.NewOrNull(permit.CSI_Code);

		int? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentDmExtensions.SequenceNumeric
			=> permit.CSI_LineNo;

		readonly CusSupportingInfo permit;
	}
}
