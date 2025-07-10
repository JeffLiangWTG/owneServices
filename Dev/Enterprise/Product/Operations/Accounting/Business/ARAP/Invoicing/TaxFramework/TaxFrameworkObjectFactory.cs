using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingDependency;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public static class TaxFrameworkObjectFactory
	{
		public static IInvoicingBaseTaxFrameworkViewModel GetInvoicingBaseTaxFrameworkViewModel(InvoicingBase invoice)
		{
			return invoice?.Factory.GetCachedValue<IInvoicingBaseTaxFrameworkViewModel>(GetInvoicingBaseTaxFrameworkViewModelKey(invoice), () => new InvoicingBaseTaxFrameworkViewModel(invoice));
		}

		public static InvoicingBaseTaxRecordParent GetInvoicingBaseTaxRecordParent(InvoicingBase invoice)
		{
			return invoice?.Factory.GetCachedValue(GetKey("TaxRecordParent", invoice), () => new InvoicingBaseTaxRecordParent(invoice));
		}

		public static InvoicingBaseForDisplayOtherTaxes GetInvoicingBaseForDisplayOtherTaxes(InvoicingBase invoice)
		{
			return invoice?.Factory.GetCachedValue(GetKey("DisplayOtherTaxes", invoice), () => new InvoicingBaseForDisplayOtherTaxes(invoice));
		}

		public static InvoicingLineBaseForOtherTaxesDisplay GetInvoicingLineBaseForOtherTaxesDisplay(InvoicingLineBase line)
		{
			return line?.Factory.GetCachedValue(GetKey("OtherTaxesDisplay", line), () => new InvoicingLineBaseForOtherTaxesDisplay(line));
		}

		public static InvoicingLineBaseTaxable GetInvoicingLineBaseTaxable(InvoicingLineBase line)
		{
			return line?.Factory.GetCachedValue(GetKey("TaxableLine", line), () => new InvoicingLineBaseTaxable(line));
		}

		public static IWHTAmountLoader GetWHTAmountLoader(BusinessObjectFactory factory)
		{
			return factory?.GetCachedValue<IWHTAmountLoader>("WHTAmountLoader", () => new WHTAmountLoader(factory));
		}

		public static IWithholdingJournalCreationManager GetAPJournalBasedWHTAmountCalculator(BusinessObjectFactory factory)
		{
			return factory?.GetCachedValue("APJournalBasedWHTAmountCalculator", () => ObjectFactory.Get<IAccountingDependencyFactory>().GetWithholdingJournalCreationManager());
		}

		public static ITaxRealisationEnabler GetTaxRealisationEnabler(BusinessObjectFactory factory)
		{
			return factory?.GetCachedValue<ITaxRealisationEnabler>("TaxRealisationEnabler", () => new TaxRealisationEnabler());
		}

		static string GetInvoicingBaseTaxFrameworkViewModelKey(InvoicingBase invoice) => GetKey("TaxFrameworkViewModel", invoice);

		static string GetKey(string keyPrefix, IIdentified obj) => string.Join(keyPrefix, obj.Identifier.ToStringKey());

		#region For Test only
#if DEBUG
		public static void SubstituteWHTAmountLoader_ForTestOnly(BusinessObjectFactory factory, IWHTAmountLoader loader)
		{
			var key = "WHTAmountLoader";
			factory.ClearCachedValue<IWHTAmountLoader>(key);
			factory.GetCachedValue(key, () => loader);
		}

		public static void SubstituteAPJournalBasedWHTAmountCalculator_ForTestOnly(BusinessObjectFactory factory, IWithholdingJournalCreationManager manager)
		{
			var key = "APJournalBasedWHTAmountCalculator";
			factory.ClearCachedValue<IWithholdingJournalCreationManager>(key);
			factory.GetCachedValue(key, () => manager);
		}

		public static void SubstituteTaxRealisationEnabler_ForTestOnly(BusinessObjectFactory factory, ITaxRealisationEnabler mock)
		{
			var key = "TaxRealisationEnabler";
			factory.ClearCachedValue<ITaxRealisationEnabler>(key);
			factory.GetCachedValue(key, () => mock);
		}

		public static void SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(InvoicingBase invoice, IInvoicingBaseTaxFrameworkViewModel viewModel)
		{
			var key = GetInvoicingBaseTaxFrameworkViewModelKey(invoice);
			invoice.Factory.ClearCachedValue<IInvoicingBaseTaxFrameworkViewModel>(key);
			invoice.Factory.GetCachedValue(key, () => viewModel);
		}
#endif
		#endregion
	}
}
