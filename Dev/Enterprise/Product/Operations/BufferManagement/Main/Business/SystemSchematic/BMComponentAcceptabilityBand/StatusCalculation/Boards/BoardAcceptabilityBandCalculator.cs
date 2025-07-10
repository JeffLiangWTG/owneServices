using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.MemoryManagement;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Cache;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class BoardAcceptabilityBandCalculator
	{
#nullable enable
		public static BoardSectionAcceptabilityBandResult Calculate(
			BusinessObjectFactory factory,
			List<ZGuid> componentPKs,
			ZGuid releaseGroupPK,
			IAcceptabilityBandOverride? boardBand,
			BMComponentAcceptabilityBand band)
		{
			var calculator = new AcceptabilityBandCalculator();
			var dataProvider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
			var cacheKey = string.Join(".", componentPKs.Select(c => c.ToString()));
			CalculationParameter parameter;

			if (boardBand != null)
			{
				parameter = new CalculationParameter(cacheKey, releaseGroupPK, boardBand, band);
			}
			else
			{
				parameter = new CalculationParameter(cacheKey, releaseGroupPK, band);
			}

			var distributedCacheFactory = ObjectFactory.Get<IDistributedCacheFactory>();
			using (var distributedCacheConnnectionFactory = distributedCacheFactory.CreateConnectionFactory(true))
			{
				var distributedCache = distributedCacheFactory.Create(distributedCacheConnnectionFactory);

				try
				{
					return Calculate(parameter, componentPKs, factory, dataProvider, calculator, distributedCache)[0];
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("Error when calculating AB", $"Band name: [{parameter.Band.BAB_Name}], Type: [{parameter.Band.BAB_Type}]", ex);
				}
			}

			return new BoardSectionAcceptabilityBandResult(boardBand, new AcceptabilityBandResult(false, null, null));
		}
#nullable disable

		public static BoardSectionAcceptabilityBandResult[] Calculate(
			BusinessObjectFactory factory,
			ZGuid sectionPK,
			ZGuid releaseGroupPK,
			IAcceptabilityBandOverride[] sectionBands,
			HashSet<ZGuid> workflowPKs = null)
		{
			var calculator = new AcceptabilityBandCalculator();
			var dataProvider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());

			var sectionResults = new List<BoardSectionAcceptabilityBandResult>();
			var calculatonParameters = CreateCalculationParameters(sectionPK.ToString(), releaseGroupPK, sectionBands, factory);

			var distributedCacheFactory = ObjectFactory.Get<IDistributedCacheFactory>();
			using (var distributedCacheConnectionFactory = distributedCacheFactory.CreateConnectionFactory(true))
			{
				var distributedCache = distributedCacheFactory.Create(distributedCacheConnectionFactory);

				foreach (var parameter in calculatonParameters)
				{
					try
					{
						var results = Calculate(parameter, sectionPK, factory, workflowPKs, dataProvider, calculator, distributedCache);
						sectionResults.AddRange(results);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Error when calculating AB", $"Band name: [{parameter.Band.BAB_Name}], Type: [{parameter.Band.BAB_Type}]", ex);
					}
				}
			}

			return sectionResults.ToArray();
		}

		public static BoardSectionAcceptabilityBandResult[] Calculate(
			BusinessObjectFactory factory,
			ZGuid sectionPK,
			ZGuid releaseGroupPK,
			string bandName,
			HashSet<ZGuid> workflowPKs = null)
		{
			var calculator = new AcceptabilityBandCalculator();
			var dataProvider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());

			var sectionResults = new List<BoardSectionAcceptabilityBandResult>();
			var band = factory.LoadTop1<BMComponentAcceptabilityBand>(new ZQuery(BMComponentAcceptabilityBandSchema.BAB_Name, bandName));
			if (band == null)
			{
				return sectionResults.ToArray();
			}

			var parameter = new CalculationParameter(sectionPK.ToString(), releaseGroupPK, band);

			try
			{
				var distributedCache = ObjectFactory.Get<IDistributedCacheFactory>().Create();
				var results = Calculate(parameter, sectionPK, factory, workflowPKs, dataProvider, calculator, distributedCache);
				sectionResults.AddRange(results);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error when calculating AB with defaults", $"Band Name : [{bandName}]", ex);
			}

			return sectionResults.ToArray();
		}

		static BoardSectionAcceptabilityBandResult[] Calculate(
			CalculationParameter parameter,
			ZGuid sectionPK,
			BusinessObjectFactory factory,
			HashSet<ZGuid> workflowPks,
			AcceptabilityBandDataProvider dataProvider,
			AcceptabilityBandCalculator calculator,
			IDistributedCache distributedCache)
		{
			if (TryGetFromCache(distributedCache, parameter, out var sectionResults))
			{
				return sectionResults;
			}

			var sqlBuilderParameter = new AcceptabilityBandSqlBuilderParameters(
				parameter.AcceptabilityBandPK,
				parameter.IsFilteringByReleaseGroup,
				parameter.IsFilteringBySection,
				parameter.BoundaryValues,
				parameter.ReleaseGroupPK,
				parameter.MaximumItems);

			if (parameter.IsFilteringBySection)
			{
				sqlBuilderParameter.WorkflowPKs = GetWorkflowPKs(workflowPks, sectionPK, factory, parameter.BandLastEditTimeUtc);
				sqlBuilderParameter.AreValid = sqlBuilderParameter.WorkflowPKs != null;
			}

			var results = calculator.CalculateStatus(parameter.Band, dataProvider, sqlBuilderParameter);
			results.SetAccurateAsOfTimeUtc(ZDateTime.UtcNow);

			AddInCache(distributedCache, parameter, results);

			return results.Select(r => new BoardSectionAcceptabilityBandResult(parameter.SectionBand, r)).ToArray();
		}

		// this code duplication will be fixed later :)
		static BoardSectionAcceptabilityBandResult[] Calculate(
			CalculationParameter parameter,
			List<ZGuid> componentPKs,
			BusinessObjectFactory factory,
			AcceptabilityBandDataProvider dataProvider,
			AcceptabilityBandCalculator calculator,
			IDistributedCache distributedCache)
		{
			if (TryGetFromCache(distributedCache, parameter, out var sectionResults))
			{
				return sectionResults;
			}

			var sqlBuilderParameter = new AcceptabilityBandSqlBuilderParameters(
				parameter.AcceptabilityBandPK,
				parameter.IsFilteringByReleaseGroup,
				parameter.IsFilteringBySection,
				parameter.BoundaryValues,
				parameter.ReleaseGroupPK,
				parameter.MaximumItems);

			if (parameter.IsFilteringBySection)
			{
				sqlBuilderParameter.WorkflowPKs = GetWorkflowPKs(componentPKs, parameter.ReleaseGroupPK, factory, parameter.BandLastEditTimeUtc, filterByReleaseGroup: parameter.IsFilteringByReleaseGroup);
				sqlBuilderParameter.AreValid = sqlBuilderParameter.WorkflowPKs != null;
			}

			var results = calculator.CalculateStatus(parameter.Band, dataProvider, sqlBuilderParameter);
			results.SetAccurateAsOfTimeUtc(ZDateTime.UtcNow);

			AddInCache(distributedCache, parameter, results);

			return results.Select(r => new BoardSectionAcceptabilityBandResult(parameter.SectionBand, r)).ToArray();
		}

		#region Calculate Helpers

		static CalculationParameter[] CreateCalculationParameters(
			string cacheKey,
			ZGuid releaseGroupPK,
			IEnumerable<IAcceptabilityBandOverride> sectionBands,
			BusinessObjectFactory factory)
		{
			var bands = factory
				.Load<BMComponentAcceptabilityBand>(new ZQuery(BMComponentAcceptabilityBandSchema.PK, sectionBands.Select(x => x.AcceptabilityBandPK)))
				.Where(b => b.BAB_IsActive);
			var bandPks = bands.Select(x => x.PK).ToArray();
			var filters = RelatedModuleFiltersHelper.LoadFilters(bandPks, BMComponentAcceptabilityBandSchema.Constants.Prefix, factory);
			factory.AddFetchHint(StmModuleFilterUserDataSchema.Instance, new ZQuery(StmModuleFilterUserDataSchema.S0_S9, filters.Select(x => x.PK)));

			var calculationParameters = new List<CalculationParameter>();

			foreach (var boardBand in sectionBands)
			{
				var band = bands.FirstOrDefault(b => b.PK == boardBand.AcceptabilityBandPK);

				if (band != null)
				{
					calculationParameters.Add(new CalculationParameter(cacheKey, releaseGroupPK, boardBand, band));
				}
			}

			return calculationParameters.ToArray();
		}

		class CalculationParameter
		{
			public CalculationParameter(
				string cacheKey,
				ZGuid releaseGroupPK,
				IAcceptabilityBandOverride boardBand,
				BMComponentAcceptabilityBand band)
			{
				CacheKey = cacheKey;
				SectionBand = boardBand;
				AcceptabilityBandPK = boardBand.AcceptabilityBandPK;
				ReleaseGroupPK = releaseGroupPK;
				IsFilteringByReleaseGroup = boardBand.IsFilteringByReleaseGroup;
				IsFilteringBySection = boardBand.IsFilteringBySection;
				BoundaryValues = boardBand.BoundaryValues;
				MaximumItems = boardBand.MaximumItems;
				Band = band;
				BandLastEditTimeUtc = Band.BAB_SystemLastEditTimeUtc;
			}

			public CalculationParameter(
				string cacheKey,
				ZGuid releaseGroupPK,
				BMComponentAcceptabilityBand band)
			{
				CacheKey = cacheKey;
				SectionBand = null;
				AcceptabilityBandPK = band.PK;
				ReleaseGroupPK = releaseGroupPK;
				IsFilteringByReleaseGroup = band.BAB_FiltersByReleaseGroup;
				IsFilteringBySection = band.BAB_FiltersBySection;
				BoundaryValues = band.BoundaryValues;
				MaximumItems = int.MaxValue;
				Band = band;
				BandLastEditTimeUtc = band.BAB_SystemLastEditTimeUtc;
			}

			public IAcceptabilityBandOverride SectionBand { get; }
			public string CacheKey { get; }
			public ZGuid AcceptabilityBandPK { get; }
			public ZGuid ReleaseGroupPK { get; }
			public bool IsFilteringByReleaseGroup { get; }
			public bool IsFilteringBySection { get; }
			public AcceptabilityBandBoundaryValues BoundaryValues { get; }
			public ZInt MaximumItems { get; }
			public BMComponentAcceptabilityBand Band { get; }
			public ZDateTime BandLastEditTimeUtc { get; }
		}

		public static HashSet<ZGuid> GetWorkflowPKs(ZGuid sectionPk, BusinessObjectFactory factory) => GetWorkflowPKs(null, sectionPk, factory, ZDateTime.MinSmallDateTimeValue);

		static HashSet<ZGuid> GetWorkflowPKs(HashSet<ZGuid> sectionWorkflowPKs, ZGuid sectionPk, BusinessObjectFactory factory, ZDateTime bandLastEditTimeUtc)
		{
			var cacheKey = "SectionWorkflowPKs." + sectionPk;
			var workflowPKs = localCache.GetCachedValue<ABWorkflowPKsCacheItem>(cacheKey);

			if (workflowPKs != null && workflowPKs.CreatedTimeUtc > bandLastEditTimeUtc)
			{
				return workflowPKs.PKs;
			}

			if (sectionWorkflowPKs != null)
			{
				var validPks = sectionWorkflowPKs.Where(p => p.IsValid).ToHashSet();
				localCache.SetInCache(cacheKey, new ABWorkflowPKsCacheItem { CreatedTimeUtc = ZDateTime.UtcNow, PKs = validPks });
				return validPks;
			}

			if (Globals.IsWebServiceOrWeb)
			{
				//if we are loading sectionWorkflowFilter for the Board Section Service, we are able to clear uberFactoryCache for these tables and assure the most up-to-date results
				RowFactory.ClearSpecificTableFromUberFactory(StmModuleFilter.Schema.TableName);
				RowFactory.ClearSpecificTableFromUberFactory(StmModuleFilterUserData.Schema.TableName);
				RowFactory.ClearSpecificTableFromUberFactory(BMComponent.Schema.TableName);
			}

			var sectionQuery = new ZQuery(BMBoardSectionSchema.PK, sectionPk);
			sectionQuery.IncludeBlob(BMBoardSectionSchema.MS_LayoutData); // disable lazy loading of MS_LayoutData to reduce db hits
			var section = factory.LoadTop1<BMBoardSection>(sectionQuery);

			if (section == null || section.Component == null)
			{
				return null;
			}

			var pks = WorkflowLoader //TODO: Improve, we don't need to load all ProcessHeader to just load the PKs!
				.LoadWorkflows(section, section.SectionConfiguration.Channels, section.WorkflowSectionFilter, section.TaskSectionFilter)
				.Select(w => w.PK)
				.ToHashSet();

			localCache.SetInCache(cacheKey, new ABWorkflowPKsCacheItem { CreatedTimeUtc = ZDateTime.UtcNow, PKs = pks });

			return pks;
		}

		static HashSet<ZGuid> GetWorkflowPKs(List<ZGuid> componentPKs, ZGuid releaseGroupPk, BusinessObjectFactory factory, ZDateTime lastBandEditTimeUtc, bool filterByReleaseGroup = false)
		{
			var cacheKey = "ComponentWorkflowPks." + string.Join(".", componentPKs.Select(c => c.ToString())) + (filterByReleaseGroup ? "." + releaseGroupPk.ToString() : string.Empty);
			var workflowPks = localCache.GetCachedValue<ABWorkflowPKsCacheItem>(cacheKey);

			if (workflowPks != null && workflowPks.CreatedTimeUtc > lastBandEditTimeUtc)
			{
				return workflowPks.PKs;
			}

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, componentPKs);

			if (filterByReleaseGroup)
			{
				query.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, releaseGroupPk);
			}

			var result = factory.Load<ProcessHeader>(query)
					.Select(p => p.PK)
					.ToHashSet();

			localCache.SetInCache(cacheKey, new ABWorkflowPKsCacheItem { CreatedTimeUtc = ZDateTime.UtcNow, PKs = result });

			return result;
		}

		#endregion

		#region CACHE

		class BoardAcceptabilityBandLocalCache : MemoryCacheWrapper
		{
			protected override TimeSpan GetAbsoluteExpiry(string key) => TimeSpan.FromSeconds(BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.Value);
		}

#pragma warning disable CW1021 // Even if a race condition happened, it would just cause a calculation result not to be properly cached and recalculated next time, so there is nothing really bad in it."
		static readonly BoardAcceptabilityBandLocalCache localCache = new BoardAcceptabilityBandLocalCache();
#pragma warning restore CW1021
		public static void ClearBoardAcceptabilityBandLocalCache() => localCache.Clear();

		public class ABWorkflowPKsCacheItem
		{
			public HashSet<ZGuid> PKs { get; set; }
			public ZDateTime CreatedTimeUtc { get; set; }
		}

		internal class ABCacheItem
		{
			public bool ResultFound { get; set; }
			public decimal? Value { get; set; }
			public ComponentAcceptabilityStatus Status { get; set; }
			public AcceptabilityStatusPolarity? StatusPolarity { get; set; }
			public TimeSpan CalculationDuration { get; set; }
			public DateTime AccurateAsOfTimeUtc { get; set; }
			public string AggregatedLabel { get; set; }
		}

		static string GetCacheKey(CalculationParameter parameter)
		{
			return "ABResult:" +
				parameter.AcceptabilityBandPK +
				(parameter.IsFilteringBySection ? parameter.CacheKey : string.Empty) +
				parameter.BandLastEditTimeUtc +
				(parameter.IsFilteringByReleaseGroup ? parameter.ReleaseGroupPK.ToString() : string.Empty) +
				parameter.MaximumItems;
		}

		static bool TryGetFromCache(IDistributedCache distributedCache, CalculationParameter parameter, out BoardSectionAcceptabilityBandResult[] results)
		{
			var cacheKey = GetCacheKey(parameter);
			ABCacheItem[] cacheItems = null;
			var localCacheEnabled = BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.Value > 0;

			if (localCacheEnabled)
			{
				cacheItems = localCache.GetCachedValue<ABCacheItem[]>(cacheKey);
			}

			if (cacheItems == null && BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.Value > 0)
			{
				cacheItems = distributedCache.Get<ABCacheItem[]>(cacheKey);

				if (localCacheEnabled && cacheItems != null)
				{
					localCache.AddToCache(cacheKey, cacheItems);
				}
			}

			if (cacheItems == null)
			{
				results = null;
				return false;
			}

			results = cacheItems.Select(cacheItem => new BoardSectionAcceptabilityBandResult(parameter.SectionBand,
				new AcceptabilityBandResult(cacheItem.ResultFound, cacheItem.Value, cacheItem.AggregatedLabel, parameter.AcceptabilityBandPK)
				{
					Status = cacheItem.Status,
					StatusPolarity = cacheItem.StatusPolarity,
					CalculationDuration = cacheItem.CalculationDuration,
					AccurateAsOfTimeUtc = cacheItem.AccurateAsOfTimeUtc,
					AggregatedLabel = cacheItem.AggregatedLabel,
				})).ToArray();

			return true;
		}

		static void AddInCache(IDistributedCache distributedCache, CalculationParameter parameter, AcceptabilityBandResultCollection results)
		{
			var cacheKey = GetCacheKey(parameter);
			var cacheItems = results.Select(r => new ABCacheItem
			{
				ResultFound = r.ResultFound,
				Value = r.Value,
				Status = r.Status,
				StatusPolarity = r.StatusPolarity,
				CalculationDuration = r.CalculationDuration,
				AccurateAsOfTimeUtc = r.AccurateAsOfTimeUtc.ToDateTime(),
				AggregatedLabel = r.AggregatedLabel,
			}).ToArray();

			if (BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.Value > 0)
			{
				localCache.AddToCache(cacheKey, cacheItems);
			}

			if (BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.Value > 0)
			{
				distributedCache.Set(cacheKey, cacheItems, new DistributedCacheOptions(TimeSpan.FromSeconds(BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.Value)), StaticCurrentFetcher.Instance.CurrentUserCode);
			}
		}

		#endregion
	}
}
