using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class ImportInvoiceLineCalculationsLayoutBuilder : ColumnLayoutBuilder<JobComInvoiceLine, CommonInvoiceLineCalculationsControlBag>
	{
		public override CommonInvoiceLineCalculationsControlBag CommonBag => CommonInvoiceLineCalculationsControlBag.Instance;

		protected override void SetDefaultCaptions()
		{
			SetCaption(
				CommonBag.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl,
				_ => Res.GetData("250DC581-51A1-4754-BAF7-6C257B975965", "{0} Amount", "{0} value for current line item").Format(Customs.Business.BaseJobComInvoiceLine.ConsumptionTaxDescription),
				_ => null
			);
		}

		protected override void SetDefaultVisibilities()
		{
			var euBag = InvoiceLineSummaryControlBag.Instance;
			SetVisibility(CommonBag.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl, NotStatisticalValueOnly, StatisticalValueOnlyDependencyGetters);
			SetVisibility(CommonBag.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl, NotStatisticalValueOnly, StatisticalValueOnlyDependencyGetters);
			SetVisibility(euBag.GSTVATDeferredConvertToLocalCurrencyControl, NotStatisticalValueOnly, StatisticalValueOnlyDependencyGetters);
			SetVisibility(euBag.CustomsValueConvertToLocalCurrencyControl, NotStatisticalValueOnly, StatisticalValueOnlyDependencyGetters);
			SetVisibility(euBag.ValueForVatConvertToLocalCurrencyControl, NotStatisticalValueOnly, StatisticalValueOnlyDependencyGetters);
			SetVisibility(euBag.StatisticalValueConvertToLocalCurrencyControl, (JobComInvoiceLine _) => true, Array.Empty<Func<JobComInvoiceLine, ZPropertyInfo>>());
			SetVisibility(CommonBag.CIFConvertToLocalCurrencyControl, NotStatisticalValueOnly, StatisticalValueOnlyDependencyGetters);
		}

		static bool NotStatisticalValueOnly(JobComInvoiceLine line) => line.EntryInstruction is not CusEntryInstruction instruction || !instruction.IsH2;

		static Func<JobComInvoiceLine, ZPropertyInfo>[] StatisticalValueOnlyDependencyGetters => new Func<JobComInvoiceLine, ZPropertyInfo>[]
		{
			line => line.JI_CEIInfo,
			line => line.EntryInstruction?.CEI_StyleInfo
		};
	}
}
