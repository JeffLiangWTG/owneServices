using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	class ImportFactoryGroupCollection
	{
		public ImportFactoryGroupCollection(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;
		readonly ConcurrentHashSet<ImportFactoryGroup> groups = new ConcurrentHashSet<ImportFactoryGroup>();

		internal void Add(ImportFactoryGroup group)
		{
			groups.TryAdd(group);
		}

		internal void Track(BusinessObject bizOToImport)
		{
			var otherFactory = bizOToImport.Factory;
			if (otherFactory != null)
			{
				var existingGroup = otherFactory.ImportFactoryGroups.FindGroup(bizOToImport);
				List<BusinessObjectFactory> factories;
				if (!(existingGroup is null))
				{
					existingGroup.Remove(bizOToImport.PK); // Ensure the same PK isn't in multiple groups.
					factories = existingGroup.Factories.Append(factory).ToList();
				}
				else
				{
					factories = new List<BusinessObjectFactory> { factory, otherFactory };
				}

				if (!otherFactory.ImportFactoryGroups.TryGetGroupForFactories(factories, out var group))
				{
					group = ImportFactoryGroup.CreateGroup(factories);
				}
				group.Add(bizOToImport.PK, bizOToImport.TableName);
			}
		}

		internal void EnsureSynchronisation(BusinessObjectFactory savedFactory)
		{
			var sourceRowFactory = savedFactory.GetRowFactory();
			foreach (var group in groups)
			{
				foreach (var pair in group.Rows)
				{
					var pk = pair.Key;
					var table = pair.Value;
					var sourceRow = sourceRowFactory.GetRow(table, pk);
					if (sourceRow != null) // Row may be deleted in the source factory. Issue here, but not to be fixed right now.
					{
						foreach (var factory in group.Factories)
						{
							factory.ThreadSentry.EnsureCurrentThreadIsOwner(); // It isn't valid to save a factory that has new rows imported from another thread.
							if (factory != savedFactory && factory.ThreadSentry.IsOwner)
							{
								var targetRow = factory.GetRowFactory().GetRow(table, pk);
								if (targetRow != null) // Row may be deleted in the target factory.
								{
									DataRefreshBus.ZDataRowSubscription.UpdateForDataRefresh(sourceRow, targetRow);
								}
							}
						}
					}
				}
				group.Clear();
			}
		}

		bool TryGetGroupForFactories(ICollection<BusinessObjectFactory> factories, out ImportFactoryGroup result)
		{
			foreach (var group in groups)
			{
				if (group.ContainsAllFactories(factories))
				{
					result = group;
					return true;
				}
			}
			result = null;
			return false;
		}

		ImportFactoryGroup FindGroup(BusinessObject bizOToImport)
		{
			var pk = bizOToImport.PK;
			foreach (var group in groups)
			{
				if (group.Contains(pk))
				{
					return group;
				}
			}
			return null;
		}
	}
}
