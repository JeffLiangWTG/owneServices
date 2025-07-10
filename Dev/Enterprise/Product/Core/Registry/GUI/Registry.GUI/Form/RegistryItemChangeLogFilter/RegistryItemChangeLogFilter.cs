using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	internal class RegistryItemChangeLogFilter(IFactory factory)
	{
		readonly IFactory _factory = factory;

		public IEnumerable<IRegistryItem> FilterRegistryItemsWithEvents(IList<IRegistryItem> registryItems,
			IRegistryChangeLogFilterParameters filterParameters, IProgress<int> progressCallbackHandler = null)
		{
			var regItemNames = registryItems.Select(ri => ri.Name);
			var query = new ZDBOnlyQuery(typeof(StmData));
			if (registryItems.Any())
			{
				query.AddToFilter(StmDataSchema.SD_Name, regItemNames);
			}
			var subQuery = filterParameters.GenerateSubQueryFilterFromFilterParameters(new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, StmDataSchema.PK));
			query.AddSubQuery(subQuery, JoinCondition.And);
			var logs = _factory.Load<StmData>(query).Select(sd => sd.SD_Name);
			var results = registryItems.Where(ri => logs.Contains(ri.Name));
			progressCallbackHandler?.Report(registryItems.Count);
			return results;
		}
	}
}
