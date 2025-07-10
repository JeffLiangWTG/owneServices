using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification NewOrNull(JobComInvoiceLine invoiceLine)
			=> invoiceLine == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper(invoiceLine);

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationDmExt IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification.DmExtensions => null;

		IIDType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification.ID
			=> IDTypeWrapper.NewOrNull(invoiceLine.JI_Tariff);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification.IdentificationTypeCode
			=> CodeTypeWrapper.NewOrNull(Constants.CustomsDeclaration.ClassificationIdentificationTypeCodeRegular);

		readonly JobComInvoiceLine invoiceLine;
	}
}
