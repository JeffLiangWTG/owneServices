using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ExchangeRatesCollection : DependentBusinessObjectCollection<ExchangeRate, Job>
	{
		public ExchangeRatesCollection(Job parentJob, BusinessObjectFactory factory)
			: base(parentJob, factory)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (IsUpdatingByDataRefreshBus && !isAddingNewRate)
			{
				using (ParentJob.SuspendSettingHasChangesOnAllChildren())
				{
					ParentJob.RefreshChargeLinesExchangeRateBinding();
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is ExchangeRate exRate)
			{
				exRate.JF_IsTransformed = true;
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			if (elementToRemove.HasContext(Context.AddingNewRate))
			{
				var message = (elementToRemove as ExchangeRate)?.BuildMessageAboutDeletedOrRemovedExchangeRate((NoResString)"Removed");
				ErrorReporter.ReportOnce("RemovingExRateInTheProcessOfAddingIt", message);
			}

			base.Remove(elementToRemove);

			if (IsDeletingForDataRefresh)
			{
				elementToRemove.SetContext(Context.DeletedByDataRefresh);
			}

			if (Factory.HasContext(BusinessContext.DeletingExchangeRate) || IsDeletingForDataRefresh)
			{
				var hashCode = GetHashCode();
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(elementToRemove.PK,
					CriticalValidationInfoCollectorServiceKeyType.ExchangeRatesCollectionRemoveMethodInfo, () =>
					{
						string result;
						if (Contains(elementToRemove))
						{
							result = FormattableString.Invariant($"Hash Code = {hashCode}, Contains ? InCollection");
						}
						else
						{
							result = FormattableString.Invariant($"Hash Code = {hashCode}, Contains ? NotInCollection");
						}

						return result;
					});
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (IsDeletingForDataRefresh)
			{
				using (ParentJob.SuspendSettingHasChangesOnAllChildren())
				{
					ParentJob.RefreshChargeLinesExchangeRateBinding();
				}
			}
		}

		public enum Context
		{
			AddingNewRate,
			DeletedByDataRefresh
		}

		public ExchangeRate AddRate(RefCurrency currency, ZDecimal exRate, ZGuid orgPk, ExchangeRateOrgTypeEnum orgType, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable, bool ignoreExistingGenerics = false)
		{
			if (currency == null || currency.RX_Code == ParentJob.Company.GC_RX_NKLocalCurrency)
			{
				return null;
			}

			var rate = GetExchangeRate(currency.RX_Code, orgPk, orgType, invoiceCurrencyType, ignoreExistingGenerics);

			if (rate != null)
			{
				return rate;
			}

			try
			{
				isAddingNewRate = true;
				rate = base.AddNew();

				using (rate.SetTempContext(Context.AddingNewRate))
				using (ParentJob.IsSettingHasChangesSuspended ? rate.SuspendSettingHasChanges() : null)
				{
					rate.JF_IsTransformed = false;
					rate.OrgType = orgType;
					rate.JF_OH_Org = orgPk;
					rate.JF_RX_NKRateCurrency = currency.RX_Code;
					rate.JF_InvoiceCurrencyType = invoiceCurrencyType.ToCode();

					if (rate.OrgType.IsDebtor())
					{
						var org = Factory.Load<OrgHeader>(orgPk);
						var ledgerType = rate.OrgType.ToLedger();
						var date = AccExchangeRateConfigurationRateFinder.GetPreferredExchangeRateDate(ParentJob.ExchangeRateConfigurationRateConsumer, org, rate.JF_RX_NKRateCurrency, ledgerType, invoiceCurrencyType);
						ParentJob.GetCFXPairFromOrganization(org, rate.JF_RX_NKRateCurrency, date, out var cfxPercent, out var cfxMinimum);

						rate.JF_CFXPercent = cfxPercent;
						rate.JF_CFXMinimum = cfxMinimum;
					}

					if (exRate != 0)
					{
						rate.JF_BaseRate = exRate;
					}
				}
			}
			finally
			{
				isAddingNewRate = false;
			}

			return rate;
		}

		public ExchangeRate GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateOrgTypeEnum orgType, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable, bool exactMatchRequired = false)
		{
			var matchingInvoiceCurrencyTypes = new[] { (ZString)invoiceCurrencyType.ToCode(), ZString.Empty };

			var exRateMatchingByOrg = this.Cast<ExchangeRate>()
											.Where(r => r.JF_RX_NKRateCurrency == currencyCode
												&& r.JF_OH_Org == orgPk
												&& r.OrgType == orgType
												&& matchingInvoiceCurrencyTypes.Contains(r.EffectiveInvoiceCurrencyType))
											.OrderBy(r => Array.IndexOf(matchingInvoiceCurrencyTypes, r.EffectiveInvoiceCurrencyType))
											.ThenBy(r => r.JF_IsTransformed ? 0 : 1)
											.ThenBy(r => r.IsInDatabase ? 0 : 1)
											.FirstOrDefault();

			if (exRateMatchingByOrg != null || exactMatchRequired)
			{
				return exRateMatchingByOrg;
			}

			var matchingOrgTypes = orgType.GetMatchingOrgTypes().ToArray();

			return this.Cast<ExchangeRate>()
				.Where(r => r.JF_RX_NKRateCurrency == currencyCode &&
							matchingInvoiceCurrencyTypes.Contains(r.EffectiveInvoiceCurrencyType) &&
							matchingOrgTypes.Contains(r.OrgType) &&
							(r.JF_OH_Org.IsEmpty || r.JF_OH_Org == orgPk) && r.JF_IsTransformed)
				.OrderBy(r => Array.IndexOf(matchingOrgTypes, r.OrgType))
				.ThenBy(r => Array.IndexOf(matchingInvoiceCurrencyTypes, r.EffectiveInvoiceCurrencyType))
				.FirstOrDefault();
		}

		public ExchangeRate FindByRefCurrency(RefCurrency currency)
		{
			if (currency != null)
			{
				foreach (ExchangeRate rate in this)
				{
					if (rate.JF_RX_NKRateCurrency == currency.RX_Code)
					{
						return rate;
					}
				}
			}
			return null;
		}

		#region Implementation
		bool isAddingNewRate;
		internal Job ParentJob
		{
			get { return Master; }
		}

		#endregion
	}
}
