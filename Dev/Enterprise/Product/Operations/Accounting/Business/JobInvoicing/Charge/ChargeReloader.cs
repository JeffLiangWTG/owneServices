using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChargeReloader : IService
	{
		public ChargeReloader(BusinessObjectFactory factory)
		{
			Factory = factory;
			ProcessedCompanies = new HashSet<Guid>();
		}

		readonly BusinessObjectFactory Factory;
		readonly HashSet<Guid> ProcessedCompanies;

		public void Reload()
		{
			var currentCompanyPK = Environment.Env.CurrentCompanyPK;
			if (!ProcessedCompanies.Contains(currentCompanyPK))
			{
				ReloadChargesInFactoryPreservingDisplayOrder(Factory);
				ProcessedCompanies.Add(currentCompanyPK);
			}
		}

		static int BatchSize
		{
			get
			{
				return
#if DEBUG
 Globals.IsTest ? 5 :
#endif
 500;
			}
		}

		public static void ReloadChargesInFactoryPreservingDisplayOrder(BusinessObjectFactory factory)
		{
			var currentCompanyPK = Environment.Env.CurrentCompanyPK;

			var cacheOnlyChargeFilter = new ZQuery(JobChargeSchema.JR_GC, currentCompanyPK);
			cacheOnlyChargeFilter.FetchOnlyFromLocalCache = true;
			var allCharges = factory.Load<Charge>(cacheOnlyChargeFilter);

			var cacheOnlyCostFilter = new ZQuery(JobConsolCostSchema.E6_GC, currentCompanyPK);
			cacheOnlyCostFilter.FetchOnlyFromLocalCache = true;
			var allCosts = factory.Load<JobConsolCost>(cacheOnlyCostFilter);

			ReloadChargesPreservingDisplayOrder(allCharges, allCosts);
		}

		static void ProcessCharges(BusinessObjectFactory reloadingFctory, Charge[] allCharges,
			Dictionary<ZGuid, ZShort> displaySequences,
			Action<Charge> processChargeIfNonApportioned, Action<Charge> processChargeIfApportioned)
		{
			foreach (var charge in allCharges)
			{
				ThrowErrorIfFactoriesAreDifferent(charge, reloadingFctory);

				if (!displaySequences.ContainsKey(charge.PK))
				{
					displaySequences.Add(charge.PK, charge.JR_DisplaySequence);
				}

				if (!charge.JR_IsApportioned && charge.IsInDatabase && !charge.HasChanges)
				{
					processChargeIfNonApportioned?.Invoke(charge);
				}

				else if (charge.JR_IsApportioned)
				{
					processChargeIfApportioned?.Invoke(charge);
				}
			}
		}

		public static void ReloadChargesPreservingDisplayOrder(Charge[] allCharges, JobConsolCost[] allCosts = null)
		{
			if (!allCharges.Any() && (allCosts == null || !allCosts.Any()))
			{
				return;
			}

			var factory = (allCharges.FirstOrDefault<BusinessObject>() ?? allCosts.First<BusinessObject>()).Factory;
			allCosts?.ForEach((c) => ThrowErrorIfFactoriesAreDifferent(c, factory));

			using (new DisposableAction(
				() => factory.SetContext(BusinessContext.ChargeReloader),
				() => factory.RemoveContext(BusinessContext.ChargeReloader)))
			{
				var displaySequences = new Dictionary<ZGuid, ZShort>();
				var chargesToReload = new List<Charge>();
				var costsToReload = new List<JobConsolCost>();
				var bizOsInDbWithPersistentChanges = new List<BusinessObject>();
				var consolCostPKs = new HashSet<ZGuid>();
				var removedConsolCostPKs = new HashSet<ZGuid>();

				ProcessCharges(factory, allCharges, displaySequences, ProcessANonApportionedCharge, ProcessAnApportionedCharge);

				ProcessCosts(factory, consolCostPKs, bizOsInDbWithPersistentChanges, chargesToReload, costsToReload, allCosts);

				ReportBizOsWithPersistentChanges(bizOsInDbWithPersistentChanges);

				ReloadChargesAndCosts(factory, chargesToReload, costsToReload);

				SetDisplaySequences(allCharges, displaySequences);

				void ProcessANonApportionedCharge(Charge charge)
				{
					if (charge.HasChargeChanged())
					{
						bizOsInDbWithPersistentChanges.Add(charge);
					}
					else
					{
						chargesToReload.Add(charge);
					}
				}

				void ProcessAnApportionedCharge(Charge charge)
				{
					if (charge.IsInDatabase && !charge.HasChanges && !removedConsolCostPKs.Contains(charge.JR_E6))
					{
						consolCostPKs.Add(charge.JR_E6);
					}
					else if (!charge.IsInDatabase || charge.HasChanges)
					{
						consolCostPKs.Remove(charge.JR_E6);
						removedConsolCostPKs.Add(charge.JR_E6);
					}
				}
			}
		}

		static void SetDisplaySequences(Charge[] charges, Dictionary<ZGuid, ZShort> displaySequences)
		{
			foreach (Charge charge in charges.Where(c => !c.IsDeleted))
			{
				if (displaySequences.TryGetValue(charge.PK, out ZShort displaySequence) && charge.JR_DisplaySequence != displaySequence)
				{
					using (charge.SuspendSettingHasChanges())
					{
						charge.JR_DisplaySequence = displaySequence;
					}
				}
			}
		}

		static void ReportBizOsWithPersistentChanges(List<BusinessObject> bizOsInDbWithPersistentChanges)
		{
			ReportErrorCore(bizOsInDbWithPersistentChanges.Where(x => x is JobConsolCost).Cast<JobConsolCost>(), (BusinessObject x) => { return (x as JobConsolCost).GetJobConsolCostInfo(); });
		}

		static void ReportErrorCore(IEnumerable<BusinessObject> bizOsInDbWithPersistentChanges, Func<BusinessObject, string> getBOInfo)
		{
			if (bizOsInDbWithPersistentChanges == null || !bizOsInDbWithPersistentChanges.Any())
			{
				return;
			}

			var messageBuilder = new ZStringBuilder();

			bizOsInDbWithPersistentChanges.ForEach(x => messageBuilder.Append(getBOInfo(x)));

			var message = messageBuilder.ToStringWithNewLineBetweenAppends();
			ErrorReporter.ReportOnce(bizOsInDbWithPersistentChanges.First().GetType().Name + "sInDbModifiedWithoutHasChangesSet", message);
		}

		static bool HasChangedPersistentProperty(BusinessObject bizO, Func<DataRow, bool> doExtraCheckingIfBizOIsChanged = null)
		{
			var row = ((IBusinessObjectInternals)bizO).Row;
			return row.RowState != System.Data.DataRowState.Unchanged && (doExtraCheckingIfBizOIsChanged?.Invoke(row) ?? true);
		}

		static void ThrowErrorIfFactoriesAreDifferent(BusinessObject bizO, BusinessObjectFactory factoryToMatchWith)
		{
			if (bizO?.Factory != factoryToMatchWith)
			{
				throw new InvalidOperationException("Multiple Factories being used in Reload");
			}
		}

		static void ProcessCosts(BusinessObjectFactory factory, HashSet<ZGuid> consolCostPKs, List<BusinessObject> bizOsInDbWithPersistentChanges, List<Charge> chargesToReload, List<JobConsolCost> costsToReload, JobConsolCost[] allCosts = null)
		{
			if (allCosts != null)
			{
				consolCostPKs.UnionWith(allCosts.Select(c => c.PK).ToHashSet());
			}

			foreach (var consolCostPK in consolCostPKs)
			{
				var relatedCharges = factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_E6, consolCostPK) { FetchOnlyFromLocalCache = true });
				var relatedConsolCostInCache = factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, consolCostPK) { FetchOnlyFromLocalCache = true }).FirstOrDefault();

				CheckStatusOfChargesAndCost(relatedCharges, relatedConsolCostInCache, bizOsInDbWithPersistentChanges, chargesToReload, costsToReload);
			}
		}

		static void CheckStatusOfChargesAndCost(Charge[] charges, JobConsolCost cost, List<BusinessObject> bizOsInDbWithPersistentChanges, List<Charge> chargesToReload, List<JobConsolCost> costsToReload)
		{
			var reloadAll = (cost == null || (cost.IsInDatabase && !cost.HasChanges)) && charges.All(x => x.IsInDatabase && !x.HasChanges);

			if (reloadAll)
			{
				if (cost != null)
				{
					AddCostToReload(cost, bizOsInDbWithPersistentChanges, costsToReload);
				}

				if (charges != null && charges.Length > 0)
				{
					AddChargesToReload(charges, bizOsInDbWithPersistentChanges, chargesToReload);
				}
			}
		}

		static void AddChargesToReload(Charge[] charges, List<BusinessObject> bizOsInDbWithPersistentChanges, List<Charge> chargesToReload)
		{
			foreach (var charge in charges)
			{
				if (charge.HasChargeChanged())
				{
					bizOsInDbWithPersistentChanges.Add(charge);
				}
				else
				{
					chargesToReload.Add(charge);
				}
			}
		}

		static void AddCostToReload(JobConsolCost cost, List<BusinessObject> bizOsInDbWithPersistentChanges, List<JobConsolCost> costsToReload)
		{
			if (HasChangedPersistentProperty(cost))
			{
				bizOsInDbWithPersistentChanges.Add(cost);
			}
			else
			{
				costsToReload.Add(cost);
			}
		}

		static void ReloadChargesAndCosts(BusinessObjectFactory factory, List<Charge> chargeWithoutChangesPKs, List<JobConsolCost> costsWithoutChanges)
		{
			var jobPKs = chargeWithoutChangesPKs.Select(x => x.JR_JH).Distinct().ToArray(); // Get Job PKs before we reload Charges
			var decider = ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>();

			foreach (var chunkCharges in AccountingUtils.ChunksOf(chargeWithoutChangesPKs, BatchSize))
			{
				var reloadFilter = new ZDBOnlyQuery(typeof(Charge));
				reloadFilter.AddToFilter(JobChargeSchema.PK, chunkCharges.Select(c => c.PK));
				reloadFilter.ReLoadExistingRows = true;
				var charges = factory.Load<Charge>(reloadFilter);
				charges.ForEach(c => decider.RemoveSkipDataRefreshBusUpdateBusinessContexts(c));
				if (charges.Length != chunkCharges.Count)
				{
					var existingChargesPK = charges.Select(c => c.PK).ToHashSet();
					foreach (Charge charge in chunkCharges)
					{
						if (!existingChargesPK.Contains(charge.PK))
						{
							DeleteSafely(charge);
						}
					}
				}

				if (factory.HasContext(BusinessContext.InvoicingPlugInGUI))
				{
					foreach (var charge in charges)
					{
						if (charge.JR_AL_ARLine.IsValid)
						{
							factory.AddFetchHint(AccTransactionLinesSchema.PK, charge.JR_AL_ARLine);
						}
					}
				}
			}

			ReloadJobExRates(factory, jobPKs);

			foreach (var chunkCosts in AccountingUtils.ChunksOf(costsWithoutChanges, BatchSize))
			{
				var reloadFilter = new ZDBOnlyQuery(typeof(JobConsolCost));
				reloadFilter.AddToFilter(JobConsolCostSchema.PK, chunkCosts.Select(c => c.PK));
				reloadFilter.ReLoadExistingRows = true;
				var costs = factory.Load<JobConsolCost>(reloadFilter);
				if (costs.Length != chunkCosts.Count)
				{
					var existingCostsPK = costs.Select(c => c.PK).ToHashSet();
					foreach (JobConsolCost cost in chunkCosts)
					{
						if (!existingCostsPK.Contains(cost.PK))
						{
							DeleteSafely(cost);
						}
					}
				}
			}
		}

		static void ReloadJobExRates(BusinessObjectFactory factory, IEnumerable<ZGuid> jobPKs)
		{
			foreach (var chunkJobs in AccountingUtils.ChunksOf(jobPKs, BatchSize))
			{
				var getJobsFilter = new ZQuery(JobHeaderSchema.PK, chunkJobs) { FetchOnlyFromLocalCache = true };
				var jobs = factory.Load<Job>(getJobsFilter);
				var jobsWithLoadedExRates = factory.Load<Job>(getJobsFilter).Where(x =>
					{
						using (x.ExchangeRatesLoadSuspender.GetSuspender())
						{
							return x.ExchangeRates.IsLoaded;
						}
					});

				var existingExRates = jobsWithLoadedExRates.SelectMany(x => x.ExchangeRates.Cast<ExchangeRate>()).Where(x => x.IsInDatabase).ToArray();
				var existingUnmodifiedExRates = existingExRates.Where(x => !x.HasChanges).ToDictionary(x => x.PK);

				var reloadedExRatePKs = factory.Load<ExchangeRate>(GetExRatesReloadFilter(jobsWithLoadedExRates, existingExRates)).Select(x => x.PK).ToHashSet();

				foreach (var job in jobsWithLoadedExRates)
				{
					job.ExchangeRates.Load();
					using (job.ChargesLoadSuspender.GetSuspender())
					{
						job.RefreshChargeLinesExchangeRateBinding();
					}
				}

				foreach (var exRate in existingUnmodifiedExRates)     // clean up Exchange Rates not existing in Db
				{
					if (!reloadedExRatePKs.Contains(exRate.Key))
					{
						DeleteSafely(exRate.Value);
					}
				}
			}
		}

		static ZQuery GetExRatesReloadFilter(IEnumerable<Job> jobs, IEnumerable<ExchangeRate> existingExRates)
		{
			var result = new ZDBOnlyQuery(typeof(ExchangeRate));
			result.AddToFilter(JobExRateSchema.JF_JH, jobs.Select(x => x.PK));
			result.AddToFilter(JobExRateSchema.PK, SQLComparisonOperator.NotEqual, existingExRates.Where(x => x.HasChanges).Select(x => x.PK));
			result.ReLoadExistingRows = true;
			return result;
		}

		static void DeleteSafely(IBusiness bizO)
		{
			bizO.DeleteForDataRefresh();
			((IBusinessObjectInternals)bizO).Row.AcceptChanges();
			bizO.HasChanges = false;
		}
	}
}
