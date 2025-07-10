using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// IDataRefreshBusSubscriber implementation to manage DataRefreshBus business object add
	/// operations for ActiveBusinessObjectCollections.
	/// </summary>
	internal class ActiveBusinessObjectCollectionDataRefreshAdder : IService, IDataRefreshBusSubscriber
	{
		protected ActiveBusinessObjectCollectionDataRefreshAdder(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static void NotifyCollectionIndexCreated(IActiveBusinessObjectCollectionIndex collection)
		{
			var instance = GetInstance(collection);
			instance.collectionTracker.NotifyCollectionIndexCreated(collection);
		}

		public static void NotifyCollectionIndexDisposed(IActiveBusinessObjectCollectionIndex collection)
		{
			var instance = GetInstance(collection);
			instance.collectionTracker.NotifyCollectionIndexDisposed(collection);
		}

		static ActiveBusinessObjectCollectionDataRefreshAdder GetInstance(IActiveBusinessObjectCollectionIndex collection)
		{
			var manyToManyRelationship = collection.Relationship as ManyToManyRelationship;
			return manyToManyRelationship != null ? GetInstance(manyToManyRelationship.PivotTableName, collection.Factory) : GetInstance(collection.ElementType, collection.Factory);
		}

		#region IDataRefreshBusSubscriber Members

		BusinessObjectFactory IDataRefreshBusSubscriber.Factory
		{
			get { return factory; }
		}

		void IDataRefreshBusSubscriber.UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			foreach (var group in publishedObjects.OfType<BusinessObject>().GroupBy(bizo => bizo.Table))
			{
				var table = group.Key;
				var primaryKeyColumnNumber = table.PrimaryKey[0].Ordinal;

				if (factory.ThreadSentry.IsOwner)
				{
					var targetTable = factory.GetRowFactory().GetTable(table.TableName);
					var bizosNotInTable = group.Where(publishedBizo => !ExistsInTable(targetTable, (Guid)publishedBizo.Row[primaryKeyColumnNumber]));
					var matchingBizos = collectionTracker.GetBizosMatchingInAnyCollectionFilter(bizosNotInTable);
					if (matchingBizos != null)
					{
						foreach (var bizo in matchingBizos)
						{
							var pk = (Guid)bizo.Row[primaryKeyColumnNumber];
							if (!ExistsInTable(targetTable, pk))
							{
								targetTable.ImportRow(bizo.Row);
							}
						}
					}
				}
				else if (factory.ThreadSentry.IsPostable)
				{
					foreach (var publishedBizO in group)
					{
						var itemArray = publishedBizO.Row.ItemArray;
						var type = publishedBizO.GetType();
						var exceptionMessage = FormattableString.Invariant($"{nameof(ActiveBusinessObjectCollectionDataRefreshAdder)}.{nameof(IDataRefreshBusSubscriber.UpdatedByDataRefresh)}\r\nTable Name: {table.TableName}\r\nFactory: {factory._Instance}, {factory.NameForDebugging}\r\nStack Trace: {new StackTrace()}");
						factory.ThreadSentry.Post(DoUpdateOnDestinationThread, Tuple.Create(type, itemArray, table.TableName, primaryKeyColumnNumber), exceptionMessage);
					}
				}
			}
		}

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

		void DoUpdateOnDestinationThread(object sender)
		{
			var tuple = (Tuple<Type, object[], string, int>)sender;
			var type = tuple.Item1;
			var itemArray = tuple.Item2;
			var tableName = tuple.Item3;
			var primaryKeyColumnNumber = tuple.Item4;

			var targetTable = factory.GetRowFactory().GetTable(tableName);
			if (!ExistsInTable(targetTable, (Guid)itemArray[primaryKeyColumnNumber]))
			{
				factory.ImportFromItemArray(type, itemArray);
			}
		}

		static bool ExistsInTable(ZDataTable table, Guid pk)
			=> table.GetRowIncludingDeleted(pk) != null;

		#endregion

		#region GetInstance

		static ActiveBusinessObjectCollectionDataRefreshAdder GetInstance(Type elementType, BusinessObjectFactory factory)
		{
			string tableName = BusinessObjectFactory.GetTableNameFromType(elementType);
			return GetInstance(tableName, factory);
		}

		internal static ActiveBusinessObjectCollectionDataRefreshAdder GetInstance(string tableName, BusinessObjectFactory factory)
		{
			FactoryService service = factory.ServiceContainer.GetService<FactoryService>();
			if (service == null)
			{
				service = new FactoryService(factory);
				factory.ServiceContainer.AddService(service);
			}
			return service.GetInstance(tableName);
		}

		class FactoryService : IService
		{
			public FactoryService(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public ActiveBusinessObjectCollectionDataRefreshAdder GetInstance(string tableName)
			{
				ActiveBusinessObjectCollectionDataRefreshAdder result;
				instances.TryGetValue(tableName, out result);
				if (result == null)
				{
					result = new ActiveBusinessObjectCollectionDataRefreshAdder(factory);
					factory.RefreshManager.StartManaging(tableName, result);
					instances[tableName] = result;
				}
				return result;
			}

			readonly BusinessObjectFactory factory;
			readonly Dictionary<string, ActiveBusinessObjectCollectionDataRefreshAdder> instances = new Dictionary<string, ActiveBusinessObjectCollectionDataRefreshAdder>();
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;

		internal readonly ActiveBusinessObjectCollectionTracker collectionTracker = new ActiveBusinessObjectCollectionTracker();

		#endregion
	}
}
