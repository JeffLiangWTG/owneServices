using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure NewOrNull(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper(invoiceLine);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure.CurrentCode => CodeTypeWrapper.NewOrNull(invoiceLine.JI_Procedure);

		readonly JobComInvoiceLine invoiceLine;
	}
}
