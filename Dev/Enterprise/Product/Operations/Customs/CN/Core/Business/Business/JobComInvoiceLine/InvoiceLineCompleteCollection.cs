using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is JobComInvoiceLine invoiceLine)
			{
				DefaultOriginDistrictIfNeeded(invoiceLine);
				DefaultDestinationDistrictIfNeeded(invoiceLine);
				DefaultPreferenceIfNeeded(invoiceLine);
			}
		}

		void DefaultOriginDistrictIfNeeded(JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			if (declaration?.WillGenerateExitingEntry ?? false)
			{
				var district = declaration.GetDefaultOriginDistrictCode();
				if (!district.IsEmpty)
				{
					invoiceLine.JI_OriginDistrict = district;
				}
			}
		}

		void DefaultDestinationDistrictIfNeeded(JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			if (declaration?.WillGenerateEnteringEntry ?? false)
			{
				var district = declaration.GetDefaultDestinationDistrictCode();
				if (!district.IsEmpty)
				{
					invoiceLine.JI_DestinationDistrict = district;
				}
			}
		}

		public void DefaultPreferenceIfNeeded(JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			if (declaration?.IsImport ?? false)
			{
				using (invoiceLine.SuspendDefaultingByPrimaryPreference())
				{
					invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.MostFavouredNations;
				}
			}
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new InvoiceLineCompleteCollectionFetchStrategy(this);
		}

		class InvoiceLineCompleteCollectionFetchStrategy : Customs.Business.FetchStrategies.InvoiceLineCompleteCollectionFetchStrategy
		{
			public InvoiceLineCompleteCollectionFetchStrategy(InvoiceLineCompleteCollection collection)
				: base(collection)
			{
			}

			protected new InvoiceLineCompleteCollection Collection => (InvoiceLineCompleteCollection)base.Collection;

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				var factory = Collection.Factory;
				var invoiceLines = Collection.Cast<JobComInvoiceLine>();

				foreach (var invoiceLine in invoiceLines.Where(invoiceLine => !invoiceLine.JI_Tariff.IsEmpty))
				{
					factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, invoiceLine.JI_Tariff, invoiceLine.EffectiveAssessmentDate));
				}

				foreach (var tariff in invoiceLines.Select(x => x.UniversalTariff).Where(x => x != null).Distinct())
				{
					tariff.FetchForLoadChildEditableObjectsIfNeeded();
				}

				foreach (var invoiceLine in invoiceLines.Where(invoiceLine => !invoiceLine.JI_CIQTariff.IsEmpty))
				{
					factory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.China, Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff, invoiceLine.JI_CIQTariff, invoiceLine.EffectiveAssessmentDate));
				}

				foreach (var ciqTariff in invoiceLines.Select(x => x.CIQTariff).Where(x => x != null).Distinct())
				{
					ciqTariff.FetchForLoadChildEditableObjectsIfNeeded();
				}

				var tariffs = invoiceLines.Where(x => x.CIQRequires).Select(x => x.UniversalTariff).Where(x => x != null).Distinct();
				CNRefTariffDataLoader.CacheTariffRequiredAdditionalElements(factory, tariffs);
			}
		}
	}
}
