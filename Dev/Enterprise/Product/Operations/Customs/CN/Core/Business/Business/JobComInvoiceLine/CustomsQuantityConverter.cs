using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		public CustomsQuantityConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo) : base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		public override ZDecimal CalculateFromNetWeightToCustomsQtyCore()
		{
			return InvoiceLine.UnitConverter.Convert(InvoiceLine.JI_NetWeight, InvoiceLine.JI_NetWeightUQ, (ZString)customsUnitOfQuantityInfo.Value);
		}
	}
}
