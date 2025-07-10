namespace Enterprise.Accounting.Business.JobInvoicing.Journal
{
	interface IPopulateAJRJCharges
	{
		void PopulateJobExchangeRates(Job job);
		void PopulateDescription();
		void PopulateAccount();
		void PopulateCostCurrency();
		void PopulateSellCurrency();
		void PopulateCostCurrencyDependingOnDefaults();
		void PopulateSellCurrencyDependingOnDefaults();
		void PopulateCostAmount();
		void PopulateSellAmount();
		void PopulateCostExchangeRate();
		void PopulateSellExchangeRate();
		void PopulateRelatedJobNumber(Job job);
		void PopulateSellRatingOverride();
		void PopulateCostRatingOverride();
	}
}
