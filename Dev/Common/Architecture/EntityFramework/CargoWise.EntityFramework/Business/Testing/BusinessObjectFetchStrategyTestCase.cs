#if DEBUG
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class BusinessObjectFetchStrategyTestCase : TestCaseWithFactory
	{
		protected abstract IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory);

		// Avoid making this field virtual.
		// If you find yourself in a need to override exceptions, then most likely:
		// either table by its nature does not require many hits (like RefCurrency), and should be added to common exceptions;
		// or you are trying to hide a problem instead of fixing it.
		static readonly ImmutableHashSet<string> IgnoredTables = ImmutableHashSet.Create
		(
			RefCurrencySchema.Constants.TableName
		);

		protected void TestFetchForView(BusinessObject bo1, BusinessObject bo2)
		{
			AssertNotEquals($"2 objects for {nameof(TestFetchForView)} must not be the same.", bo1, bo2);

			var pkSchemaColumn = bo1.PKSchemaColumn;
			var tableName = bo1.TableName;

			var propsWithDifferentDbHits = new SortedDictionary<string, IList<string>>();

			foreach (var info in GetBusinessObjectProperties(bo1))
			{
				var dbHits1 = MeasureDbHits(pkSchemaColumn, tableName, info.Name, bo1.PK);
				var dbHits2 = MeasureDbHits(pkSchemaColumn, tableName, info.Name, bo2.PK);
				var dbHits12 = MeasureDbHits(pkSchemaColumn, tableName, info.Name, bo1.PK, bo2.PK);

				var tablesWithDifferentDbHits = GetTablesWithDifferentDbHits(dbHits1, dbHits2, dbHits12);

				var messages = tablesWithDifferentDbHits.Select(p => $"{p.TableName} ({p.Hits1} for first object, {p.Hits2} for second object, {p.Hits12} for both objects)").ToList();

				if (messages.Any())
				{
					propsWithDifferentDbHits.Add(info.Name, messages);
				}
			}

			AssertGroupedErrorList("Following properties make extra db hits to listed tables without fetch hints. Add fetch hints in the FetchForView method.", propsWithDifferentDbHits);
		}

		Dictionary<string, int> MeasureDbHits(SchemaGuidColumn pkSchemaColumn, string tableName, string propName, params ZGuid[] boPks)
		{
			var factory = NewFactory();

			var collection = CreateCollectionToTest(factory);
			LoadCollection(collection, pkSchemaColumn, boPks);

			factory.ResetDatabaseLoadCount();

			collection.FetchStrategy.FetchForView(collection.ToArray(), new TableColumn[] { new TableColumn(tableName, propName) });
			foreach (BusinessObject obj in collection)
			{
				obj.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(tableName, propName) });
			}

			foreach (BusinessObject obj in collection)
			{
				_ = obj.ZPropertyInfoHash.GetPropertySafe(propName).Value;
			}

			return factory.TableSelects
				.Where(x => !IgnoredTables.Contains(x.TableName) && x.Value > 0)
				.ToDictionary(x => x.TableName, x => x.Value);
		}

		void LoadCollection(IBusinessObjectCollection collection, SchemaGuidColumn pkSchemaColumn, params ZGuid[] boPks)
		{
			var additionalFilter = new ZQuery(pkSchemaColumn, boPks);

			if (collection is BusinessObjectCollection legacyCollection)
			{
				legacyCollection.Load(additionalFilter);
			}
			else if (collection is IActiveBusinessObjectCollection activeCollection)
			{
				activeCollection.AdditionalFilter = additionalFilter;
				// ensures loading of collection
				_ = activeCollection.Count;
			}
			else
			{
				throw new InvalidOperationException($"Cannot apply filter to {collection?.GetType().Name}.");
			}

			AssertEquals("(pre-condition) collection loaded correctly", boPks.Length, collection.Count);
		}

		List<(string TableName, int Hits1, int Hits2, int Hits12)> GetTablesWithDifferentDbHits(Dictionary<string, int> dbHits1, Dictionary<string, int> dbHits2, Dictionary<string, int> dbHits12)
		{
			var res = new List<(string TableName, int Hits1, int Hits2, int Hits12)>();

			foreach (var tableName in dbHits1.Keys.Union(dbHits2.Keys))
			{
				dbHits1.TryGetValue(tableName, out var hits1);
				dbHits2.TryGetValue(tableName, out var hits2);
				dbHits12.TryGetValue(tableName, out var hits12);

				if (hits1 != hits12 || hits2 != hits12)
				{
					res.Add((TableName: tableName, Hits1: hits1, Hits2: hits2, Hits12: hits12));
				}
			}

			return res;
		}

		IEnumerable<ZPropertyInfo> GetBusinessObjectProperties(BusinessObject bo)
		{
			return from ZPropertyInfo propertyInfo in bo.ZPropertyInfoHash
				   let propertyDescriptor = propertyInfo.PropertyDescriptor
				   where typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType)
					   && !bo.Table.Columns.Contains(propertyInfo.Name)
					   && propertyInfo.PropertyType != typeof(ZGuid)
				   let componentType = propertyDescriptor.ComponentType
				   select propertyInfo;
		}
	}
}
#endif
