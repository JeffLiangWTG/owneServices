using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureProvider
	{
		public DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure> GetGoodsMeasure()
			=> GetGoodsMeasureList()
				.WhereNotNull()
				.ToArray();

		IEnumerable<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure> GetGoodsMeasureList()
		{
			yield return DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty, Constants.CustomsDeclaration.MeasureQualifierInvoiceQuantity);
			yield return DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(invoiceLine.JI_CustomsSecondQuantity, invoiceLine.JI_CustomsSecondUnitQty, Constants.CustomsDeclaration.MeasureQualifierStatisticQty);
			yield return DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(invoiceLine.JI_CustomsThirdQuantity, invoiceLine.JI_CustomsThirdUnitQty, Constants.CustomsDeclaration.MeasureQualifierAdditionalQty);
		}

		readonly JobComInvoiceLine invoiceLine;
	}
}
