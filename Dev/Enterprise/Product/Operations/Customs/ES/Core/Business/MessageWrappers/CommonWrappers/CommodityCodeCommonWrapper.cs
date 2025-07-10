using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommodityCodeCommonWrapper : ICommodityCodeCommon
	{
		public CommodityCodeCommonWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		protected readonly JobComInvoiceLine invoiceLine;

		const int tarifCodeLength = 6;
		const int tarifCodeCombinedLength = 2;

		public ZString TariffCode => invoiceLine.JI_Tariff.SubstringSafe(0, invoiceLine.JI_Tariff.Length < tarifCodeLength ? invoiceLine.JI_Tariff.Length : tarifCodeLength);

		public ZString TariffCodeCombined => invoiceLine.JI_Tariff.SubstringSafe(tarifCodeLength, tarifCodeCombinedLength);
	}
}
