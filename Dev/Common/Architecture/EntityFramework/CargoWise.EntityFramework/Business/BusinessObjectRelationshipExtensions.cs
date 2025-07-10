using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public static class BusinessObjectRelationshipExtensions
	{
		public static IDisposable MarkAsInConstruction(this Guid pK, SchemaGuidColumn schemaPK, BusinessObjectFactory factory)
		{
			var pksInConstruction = factory.GetCachedValue(PkInConstructionKey(schemaPK), () => new List<Guid>());
			pksInConstruction.Add(pK);
			return new DisposableAction(() => pksInConstruction.Remove(pK));
		}

		public static IDisposable MarkAsInConstruction(this DataRow row, SchemaGuidColumn schemaPK, BusinessObjectFactory factory)
		{
			var rowPK = row[schemaPK.Name];

			if (rowPK.Equals(System.DBNull.Value))
			{
				var rowsInConstruction = factory.GetCachedValue(RowsInConstructionKey(schemaPK), () => new List<DataRow>());
				rowsInConstruction.Add(row);
				return new DisposableAction(() => rowsInConstruction.Remove(row));
			}
			else
			{
				return ((Guid)rowPK).MarkAsInConstruction(schemaPK, factory);
			}
		}

		public static IDisposable MarkAsInDeletion(this BusinessObject element)
		{
			var elementsInDeletion = element.Factory.GetCachedValue(ElementsInDeletionKey(element.TableName), () => new List<BusinessObject>());
			elementsInDeletion.Add(element);
			return new DisposableAction(() => elementsInDeletion.Remove(element));
		}

		public static bool IsInConstruction(this DataRow row, SchemaGuidColumn schemaPK, BusinessObjectFactory factory)
		{
			var rowPK = (Guid)row[schemaPK.Name];
			return factory.GetCachedValue(RowsInConstructionKey(schemaPK), () => new List<DataRow>()).Contains(row) ||
				factory.GetCachedValue(PkInConstructionKey(schemaPK), () => new List<Guid>()).Contains(rowPK);
		}

		public static bool IsInDeletion(this BusinessObject element)
		{
			return element.Factory.GetCachedValue(ElementsInDeletionKey(element.TableName), () => new List<BusinessObject>()).Contains(element);
		}

		static string RowsInConstructionKey(SchemaGuidColumn schemaPK)
		{
			return schemaPK.TableName + ".RowsInConstruction";
		}

		static string ElementsInDeletionKey(string tableName)
		{
			return tableName + ".ElementsInDeletion";
		}

		static string PkInConstructionKey(SchemaGuidColumn schemaPK)
		{
			return schemaPK.TableName + ".PkInConstruction";
		}
	}
}
