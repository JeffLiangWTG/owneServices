using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		internal static DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper NewOrNull(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper(invoiceLine);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin.CountryCode => CodeTypeWrapper.NewOrNull(invoiceLine.JI_CountryOfOrigin);

		readonly JobComInvoiceLine invoiceLine;
	}
}
