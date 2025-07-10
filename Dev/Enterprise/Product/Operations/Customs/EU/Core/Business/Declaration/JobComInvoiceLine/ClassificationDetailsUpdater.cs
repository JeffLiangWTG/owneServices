using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ClassificationDetailsUpdater : Customs.Business.ClassificationDetailsUpdater
	{
		public ClassificationDetailsUpdater(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override void UpdateWhenJI_CCIsSetCore(BaseCusClassification classification)
		{
			base.UpdateWhenJI_CCIsSetCore(classification);

			if (!invoiceLine.JI_Tariff.IsEmpty)
			{
				var declaration = invoiceLine.Declaration;
				if (declaration != null && declaration.IsExport)
				{
					invoiceLine.JI_Tariff = TariffFormatter.GetExportTariffNumber(invoiceLine.JI_Tariff);
				}
			}
		}
	}
}
