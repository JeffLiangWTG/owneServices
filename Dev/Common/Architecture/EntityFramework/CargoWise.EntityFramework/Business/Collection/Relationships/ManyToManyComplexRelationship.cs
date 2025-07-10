using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ManyToManyComplexRelationship : ManyToManyAbstractRelationship
	{
		public ManyToManyComplexRelationship(
			BusinessObject master,
			Type elementType,
			Type pivotObjectType,
			ZQuery filter,
			List<SchemaColumn> pivotTableFKsToMaster,
			List<SchemaColumn> masterTableFKsToPivots,
			List<SchemaColumn> pivotTableFKsToElements,
			List<SchemaColumn> elementTableFKsToPivots,
			SchemaColumn pivotTableRelationshipColumn = null,
			object pivotTableRelationshipValue = null)
			: base(master, elementType, pivotObjectType, filter)
		{
			Argument.NotNull(pivotTableFKsToMaster, "pivotTableFKsToMaster");
			Argument.NotNull(masterTableFKsToPivots, "masterTableFKsToPivots");
			Argument.NotNull(pivotTableFKsToElements, "pivotTableFKsToElements");
			Argument.NotNull(elementTableFKsToPivots, "elementTableFKsToPivots");

			foreach (var pivotTableFKToMaster in pivotTableFKsToMaster)
			{
				if (pivotTableFKToMaster.TableName != BusinessObjectFactory.GetTableNameFromType(pivotObjectType))
				{
					#pragma warning disable CA2208 // Suppress because pivotTableFKToMaster will not match the name of the parameter pivotTableFKsToMaster in method signature
					throw new ArgumentException("pivotTableFKToMaster must be a column on the pivot table", nameof(pivotTableFKToMaster));
					#pragma warning restore CA2208
				}
			}

			foreach (var masterTableFKToPivots in masterTableFKsToPivots)
			{
				if (masterTableFKToPivots.TableName != BusinessObjectFactory.GetTableNameFromType(master.GetType()))
				{
					// If the master is a NonPersistentBusinessObject, which is linked to a BusinessObject, override the NonPersistentBusinessObject's PKSchemaColumn to return the BusinessObject's PK SchemaColumn.
					#pragma warning disable CA2208 // Suppress because masterTableFKToPivots will not match the name of the parameter masterTableFKsToPivots in method signature
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "masterTableFKToPivots must be a column on the master table. masterTableFKToPivots.TableName: {0}. master type: {1}", masterTableFKToPivots.TableName, master.GetType().FullName), nameof(masterTableFKToPivots));
					#pragma warning restore CA2208
				}
			}

			foreach (var pivotTableFKToElements in pivotTableFKsToElements)
			{
				if (pivotTableFKToElements.TableName != BusinessObjectFactory.GetTableNameFromType(pivotObjectType))
				{
					#pragma warning disable CA2208 // Suppress because pivotTableFKToElements will not match the name of the parameter pivotTableFKsToElements in method signature
					throw new ArgumentException("pivotTableFKToElements must be a column on the pivot table", nameof(pivotTableFKToElements));
					#pragma warning restore CA2208
				}
			}

			foreach (var elementTableFKToPivots in elementTableFKsToPivots)
			{
				if (elementTableFKToPivots.TableName != BusinessObjectFactory.GetTableNameFromType(elementType))
				{
					#pragma warning disable CA2208 // Suppress because elementTableFKToPivots will not match the name of the parameter elementTableFKsToPivots in method signature
					throw new ArgumentException("elementTableFKToPivots must be a column on the pivot table", nameof(elementTableFKToPivots));
					#pragma warning restore CA2208
				}
			}

			this.PivotTableFKsToMaster = pivotTableFKsToMaster;
			this.MasterTableFKsToPivots = masterTableFKsToPivots;
			this.PivotTableFKsToElements = pivotTableFKsToElements;
			this.ElementTableFKsToPivots = elementTableFKsToPivots;
			this.PivotTableRelationshipColumn = pivotTableRelationshipColumn;
			this.PivotTableRelationshipValue = pivotTableRelationshipValue;
		}

		protected internal readonly List<SchemaColumn> PivotTableFKsToMaster;
		protected internal readonly List<SchemaColumn> MasterTableFKsToPivots;
		protected internal readonly List<SchemaColumn> PivotTableFKsToElements;
		protected internal readonly List<SchemaColumn> ElementTableFKsToPivots;
		protected internal readonly SchemaColumn PivotTableRelationshipColumn;
		protected internal readonly object PivotTableRelationshipValue;

		ImmutableArray<object> MasterFksToPivot
		{
			get
			{
				var result = new List<object>();
				foreach (var masterTableFKToPivots in MasterTableFKsToPivots)
				{
					if (masterTableFKToPivots.IsPKColumn)
					{
						result.Add(master.PK);
					}
					else
					{
						result.Add(master[masterTableFKToPivots]);
					}
				}
				return result.ToImmutableArray();
			}
		}

		ImmutableArray<object> GetPivotFksOfElement(BusinessObject element)
		{
			var result = new List<object>();
			foreach (var elementTableFKToPivots in ElementTableFKsToPivots)
			{
				if (elementTableFKToPivots.IsPKColumn)
				{
					result.Add(element.PK);
				}
				else
				{
					result.Add(element[elementTableFKToPivots]);
				}
			}
			return result.ToImmutableArray();
		}

		public override BusinessObject GetPivotObject(BusinessObject element)
		{
			InitPivotsIfRequired(element.Factory);
			_ = pivots.TryGetValue(GetPivotFksOfElement(element), out var result);
			return result;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as ManyToManyComplexRelationship;
			var result = rhs != null;

			result = result && base.Equals(rhs);
			result = result && Master == rhs.Master;
			result = result && PivotObjectType == rhs.PivotObjectType;
			result = result && ElementType == rhs.ElementType;
			return result;
		}

		public override int GetHashCode()
		{
			return Master.PK.GetHashCode() ^ ElementType.GetHashCode() ^ PivotObjectType.GetHashCode();
		}

		#endregion

		#region Overrides

		[SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "waiting for an SQL/ZQuery dark wizard to come along and improve this code")]
		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				var query = new ZQuery();
				if (this.pivots.Count == 0)
				{
					if (isPivotsInitialized)
					{
						query = ZQuery.NoResultQuery;
					}
				}
				else
				{
					var matchOneOfThesePivots = new ZQuery();
					foreach (var values in pivots.Keys)
					{
						var maybeThisPivot = new ZQuery();
						for (var i = 0; i < ElementTableFKsToPivots.Count; ++i)
						{
							var column = ElementTableFKsToPivots[i];
							var value = values[i];
							maybeThisPivot.AddToFilter(column, value);
						}
						matchOneOfThesePivots.AddToFilter(maybeThisPivot, JoinCondition.Or);
					}
					query.AddToFilter(matchOneOfThesePivots);

					//TST: Alternate implementation that's ZDBOnlyQuery.
					//Similar to https://stackoverflow.com/questions/1136380/sql-where-in-clause-multiple-columns/1136381#1136381 .
					//Can try implementing this/fixing bugs if above code is too slow and can't be salvaged.

					/*
					var elementTable = BusinessObjectFactory.GetTableNameFromType(ElementType);
					var pivotTable = BusinessObjectFactory.GetTableNameFromType(PivotObjectType);
					var subQuery = new StringBuilder();
					for (var i = 0; i < ElementTableFKsToPivots.Count; ++i)
					{
					  subQuery.AppendLine($"AND {elementTable}.{ElementTableFKsToPivots[i].Name} = p.{PivotTableFKsToElements[i].Name}");
					}
					var sql = $@"EXISTS (
					SELECT 1 FROM {pivotTable} p
					WHERE p.{PivotTableFKToMaster.Name} = @MasterKey
					{subQuery.ToString()}
					)";
					var parameter = ZSqlParameter.New("@MasterKey", MasterFkToPivot, PivotTableFKToMaster);
					var parameterCollection = new ZSqlParameterCollection(parameter);
					query = new ZDBOnlyQuery(ElementType);
					query.AddFilterAndZSQLParameterCollection(sql, parameterCollection);
*/
				}

				return query;
			}
		}

		protected override BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
		{
			using (SuppressListChanged())
			{
				isPivotsInitialized = false;
				InitPivotsIfRequired(factory);
				var result = new List<BusinessObject>();
				foreach (var elementFKs in pivots.Keys)
				{
					foreach (var element in GetElementsByKey(factory, elementFKs).Where(e => e != null && e.MatchesFilter(filter)))
					{
						result.Add(element);
						if (filter.MaximumRows != null && (int)filter.MaximumRows == result.Count)
						{
							break;
						}
					}
				}

				return result.ToArray();
			}
		}

		protected virtual BusinessObject[] GetElementsByKey(BusinessObjectFactory factory, ImmutableArray<object> elementFKs)
		{
			var query = new ZQuery();
			for (var i = 0; i < elementFKs.Length; ++i)
			{
				var elementTableFKToPivots = ElementTableFKsToPivots[i];
				query.AddToFilter(elementTableFKToPivots, elementFKs[i]);
			}
			return factory.Load(ElementType, query);
		}

		protected override void AddToRelationshipCore(BusinessObject pivot, BusinessObject businessObject)
		{
			var elementFksToPivot = GetPivotFksOfElement(businessObject);

			this.pivots.Add(elementFksToPivot, pivot);
			for (var i = 0; i < elementFksToPivot.Length; ++i)
			{
				var elementFkToPivot = elementFksToPivot[i];
				var pivotTableFKToElements = PivotTableFKsToElements[i];
				if (pivotTableFKToElements.IsPKColumn)
				{
					ErrorReporter.ReportOnce("pivotTableFKToElements.IsPKColumn");
				}
				else
				{
					pivot[pivotTableFKToElements] = elementFkToPivot;
				}
			}

			var masterFksToPivot = MasterFksToPivot;
			for (var i = 0; i < PivotTableFKsToMaster.Count; ++i)
			{
				pivot[PivotTableFKsToMaster[i]] = masterFksToPivot[i];
			}
			if (PivotTableRelationshipColumn != null)
			{
				pivot[PivotTableRelationshipColumn] = PivotTableRelationshipValue;
			}
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			using (SuppressListChanged())
			{
				var pivot = GetPivotObject(businessObject);
				if (pivot != null)
				{
					var elementFkToPivot = GetPivotFksOfElement(businessObject);
					this.pivots.Remove(elementFkToPivot);
					pivot.Delete();
				}
			}
		}

		#endregion

		#region Refreshed event

		protected override void PivotDataView_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded ||
				e.ListChangedType == ListChangedType.ItemDeleted ||
				(e.ListChangedType == ListChangedType.ItemChanged && (e.PropertyDescriptor == null || PivotTableFKsToElements.Select(x => x.Name).Contains(e.PropertyDescriptor.Name))) ||
				(e.ListChangedType == ListChangedType.ItemChanged && (e.PropertyDescriptor == null || PivotTableFKsToMaster.Select(x => x.Name).Contains(e.PropertyDescriptor.Name)))
				)
			{
				this.pivots.Clear();
				foreach (DataRowView row in PivotDataView)
				{
					var pivot = Master.Factory.Load(PivotObjectType, (Guid)row[0]);
					if (!pivot.IsDeleted)
					{
						var elementFk = GetElementFksOfPivot(pivot);
						if (!elementFk.IsEmpty && !pivots.ContainsKey(elementFk))
						{
							this.pivots.Add(elementFk, pivot);
						}
					}
				}

				OnRelationshipFilterChanged(EventArgs.Empty);
			}
		}

		protected virtual ImmutableArray<object> GetElementFksOfPivot(BusinessObject pivot)
		{
			var result = new List<object>();
			foreach (var pivotTableFKToElements in PivotTableFKsToElements)
			{
				result.Add(pivot[pivotTableFKToElements]);
			}
			return result.ToImmutableArray();
		}

		protected override string GetPivotDataViewRowFilter()
		{
			var result = "";
			var masterFksToPivot = MasterFksToPivot;
			for (var i = 0; i < masterFksToPivot.Length; ++i)
			{
				if (i >= 1)
				{
					result += " AND ";
				}
				result += PivotTableFKsToMaster[i].Name + "='" + masterFksToPivot[i] + "'";
			}
			return result;
		}

		#endregion

		#region Implementation

		internal readonly Dictionary<ImmutableArray<object>, BusinessObject> pivots = new Dictionary<ImmutableArray<object>, BusinessObject>(new ArrayComparer());

		class ArrayComparer : EqualityComparer<ImmutableArray<object>>
		{
			public override bool Equals(ImmutableArray<object> x, ImmutableArray<object> y)
			{
				if (x.Length != y.Length)
				{
					return false;
				}
				for (var i = 0; i < x.Length; ++i)
				{
					if (!(x[i].Equals(y[i])))
					{
						return false;
					}
				}
				return true;
			}

			public override int GetHashCode(ImmutableArray<object> obj)
			{
				unchecked
				{
					var result = 17;
					for (var i = 0; i < obj.Length; ++i)
					{
						var x = obj[i];
						result += (i * 2 - 1) * x.GetHashCode();
					}
					return result;
				}
			}
		}

		void InitPivotsIfRequired(BusinessObjectFactory factory)
		{
			if (!isPivotsInitialized)
			{
				isPivotsInitialized = true;

				if (factory == null)
				{
					factory = Master.Factory;
				}

				using (SuppressListChanged())
				{
					pivots.Clear();
					InitPivots(factory, PivotTableFKsToMaster, PivotTableFKsToElements);
				}
			}
		}

		protected virtual void InitPivots(BusinessObjectFactory factory, List<SchemaColumn> pivotTableFKsToMaster, IEnumerable<SchemaColumn> pivotTableFKsToElements)
		{
			var query = new ZQuery();
			var masterFksToPivot = MasterFksToPivot;
			for (var i = 0; i < masterFksToPivot.Length; ++i)
			{
				query.AddToFilter(pivotTableFKsToMaster[i], masterFksToPivot[i]);
			}
			foreach (var pivotTableFKToElements in pivotTableFKsToElements)
			{
				if (pivotTableFKToElements.IsNullable)
				{
					query.AddToFilter(pivotTableFKToElements, SQLComparisonOperator.NotEqual, DBNull.Value);
				}
			}

			if (AdditionalDivotFilter != null)
			{
				query.AddToFilter(AdditionalDivotFilter);
			}
			query.FetchOnlyFromLocalCache = !Master.IsInDatabase;
			var allPivots = factory.Load(PivotObjectType, query);

			foreach (var pivot in allPivots.Where(p => IsRelevantPivot(p)))
			{
				var key = GetElementFksOfPivot(pivot);
				if (!pivots.ContainsKey(key))
				{
					pivots.Add(key, pivot);
				}
				else
				{
					ErrorReporter.ReportOnce("InitPivotsIfRequired_DuplicatePivots_" + pivotTableFKsToMaster.Select(x => x.Name).Aggregate((x, y) => x + ", " + y),
						string.Format(
@"There was an attempt to load duplicate pivot to ManyToManyComplexRelationship in collection with:
Master business object of type '{0}', with PK '{1}'
Collection element type '{2}', with PivotTableFKToElements.Name '{3}', Value '{4}'
Pivots load query:
{5}
query.FetchOnlyFromLocalCache: {6}
Pivot Debug information:
{7}
Debug log: {8}",
							Master.GetType().FullName,
							Master.PK.ToString(),
							ElementType.FullName,
							pivotTableFKsToElements.Select(x => x.Name).Aggregate((x, y) => x + ", " + y),
							key.ToString(),
							query.LiteralTextADOFormatted,
							query.FetchOnlyFromLocalCache,
							GetPivotDebugInformation(pivot, allPivots.Where(p => IsRelevantPivot(p))),
							DebugLogMessage));
				}
			}
		}

		#endregion
	}
}
