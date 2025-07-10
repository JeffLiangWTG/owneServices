using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415GoodsInformationExportProvider : IRD415GoodsInformationExport
	{
		public RD415GoodsInformationExportProvider(CusEntryLine cusEntryLine)
		{
			entryLine = cusEntryLine;
		}
		readonly CusEntryLine entryLine;

		public IGoodsInformationOtherTypeCommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => new GoodsInformationOtherTypeCommodityCodeProvider(entryLine.RandomLine.JI_Tariff));
		CachedValue<GoodsInformationOtherTypeCommodityCodeProvider> commodityCode;

		public decimal NetMass => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsQuantity);

		public decimal SupplementaryUnits => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsSecondQuantity);

		public string GoodsDescription => entryLine.RandomLine.JI_Description;
	}
}
