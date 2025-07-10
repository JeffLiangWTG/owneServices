using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void FetchForMergeCore()
		{
			try
			{
				isImport = BusinessObject.IsImport;
				base.FetchForMergeCore();
			}
			finally
			{
				isImport = false;
			}
		}
		bool isImport;

		protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			var cnInvoiceLine = (JobComInvoiceLine)invoiceLine;
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, cnInvoiceLine.PK);
			Factory.AddFetchHint(TariffViewSchema.Instance, TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, cnInvoiceLine.JI_Tariff, cnInvoiceLine.EffectiveAssessmentDate));
		}

		protected override void AddMergeFetchHintsAfterInvoiceLines()
		{
			base.AddMergeFetchHintsAfterInvoiceLines();
			if (isImport)
			{
				foreach (JobComInvoiceLine invoiceLine in BusinessObject.InvoiceLines)
				{
					var tariff = invoiceLine.UniversalTariff;
					if (tariff != null)
					{
						Factory.AddFetchHint(RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, tariff.PK);
						Factory.AddFetchHint(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, tariff.PK);
					}
				}

				var rateTypes = new[]
				{
					Constants.UniversalReferenceConstants.RefCusRateTypes.CustomsDuty,
					Constants.UniversalReferenceConstants.RefCusRateTypes.VAT,
					Universal.Constants.RateTypes.Excise,
					Universal.Constants.RateTypes.AntiDumping,
					Universal.Constants.RateTypes.Countervailing,
				};
				foreach (var rateType in rateTypes)
				{
					var cusRefRateCodeViewQuery = new ZQuery(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.China);
					cusRefRateCodeViewQuery.AddToFilter(CusRefRateCodeViewSchema.ZY1_RateType, rateType);
					Factory.AddFetchHint(CusRefRateCodeViewSchema.Instance, cusRefRateCodeViewQuery);
				}
			}
		}
	}
}
