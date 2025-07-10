using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Service;

class SnapshotQueryService : ISnapshotQueryService
{
	[ThreadSafe]
	static SnapshotQueryService instance;
	public static SnapshotQueryService Instance => instance ??= new SnapshotQueryService();

	IGlowIndexQueryEngine glowQueryEngine;
	protected IGlowIndexQueryEngine GlowIndexQueryEngine => glowQueryEngine ??= ObjectFactory.Get<IGlowIndexQueryEngine>();

	public int QuerySnapShotResult(Snapshot snapshot)
	{
		var moduleFilterId = snapshot.ModuleFilter.ModuleFilterId;
		var modelID = snapshot.ModuleFilter.ModuleId;
		var moduleFilter = GetModuleFilterById(moduleFilterId) ?? throw new InvalidOperationException("The 'module filter' could not be found.");

		if (!moduleFilter.S9_IsIndexSearch)
		{
			throw new InvalidOperationException($"Indexed search is not enabled for the module filter '{moduleFilter.S9_FilterName}'.");
		}

		using var module = (ZFilterModule)ZModuleFactory.Instance.Create(modelID);
		var filterBusinessObject = module.FilterBusinessObject;
		filterBusinessObject.LoadLayout(moduleFilter);

		var entityType = filterBusinessObject.IndexSearchFields?.EntityType;
		var glowQueries = filterBusinessObject.GetIndexSearchQueries().ToList();

		return GetQueryResultCount(glowQueries, entityType);
	}

	int GetQueryResultCount(List<IGlowQuery> queries, string entityType)
	{
		Argument.NotNull(queries, "queries");
		Argument.NotNullOrEmpty(entityType, "entityType");

		if (queries.Count > 0)
		{
			return GlowIndexQueryEngine.Query(new GlowIndexQueryParam(queries, entityType, 0, true))?.MaximumResults ?? 0;
		}

		return 0;
	}

	public StmModuleFilter GetModuleFilterById(ZGuid moduleFilterId) => Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.PK, moduleFilterId));

	BusinessObjectFactory factory;
	BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();

	ModuleList moduleList;
	public ModuleList ModuleList => moduleList ??= new ModuleList();
}
