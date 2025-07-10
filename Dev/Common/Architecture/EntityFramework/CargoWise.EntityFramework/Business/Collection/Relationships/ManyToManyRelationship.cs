using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// A many to many relationship that has a pivot object with a dependent master.
	/// </summary>
	public class ManyToManyRelationship : ManyToManyRelationship<SchemaGuidColumn, ZGuid, SchemaGuidColumn, ZGuid>
	{
		public ManyToManyRelationship(BusinessObject master, Type elementType, Type pivotObjectType)
			: this(master, elementType, pivotObjectType, null, GetPivotTableFKToMaster(master, pivotObjectType), GetPivotTableFKToElements(elementType, pivotObjectType))
		{
		}

		public ManyToManyRelationship(BusinessObject master, Type elementType, Type pivotObjectType, ZQuery filter, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements)
			: base(master, elementType, pivotObjectType, filter, pivotTableFKToMaster ?? GetPivotTableFKToMaster(master, pivotObjectType), master.PKSchemaColumn, pivotTableFKToElements ?? GetPivotTableFKToElements(elementType, pivotObjectType), GetElementTablePkColumn(elementType))
		{
		}

		static SchemaGuidColumn GetPivotTableFKToMaster(BusinessObject master, Type pivotObjectType)
		{
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			var pivotTableName = BusinessObjectFactory.GetTableNameFromType(pivotObjectType);
			var pivotTablePrefix = schemaResolver.GetColumnNamePrefix(pivotTableName);
			var columnName = pivotTablePrefix + "_" + master.TablePrefix; // eg, JN_JS
			return (SchemaGuidColumn)schemaResolver.GetSchemaColumn(columnName, pivotTableName);
		}

		static SchemaGuidColumn GetPivotTableFKToElements(Type elementType, Type pivotObjectType)
		{
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			var elementTableName = BusinessObjectFactory.GetTableNameFromType(elementType);
			var elementTablePrefix = schemaResolver.GetColumnNamePrefix(elementTableName);

			var pivotTableName = BusinessObjectFactory.GetTableNameFromType(pivotObjectType);
			var pivotTablePrefix = schemaResolver.GetColumnNamePrefix(pivotTableName);

			var columnName = pivotTablePrefix + "_" + elementTablePrefix; // eg, JN_JS
			return (SchemaGuidColumn)schemaResolver.GetSchemaColumn(columnName, pivotTableName);
		}

		static SchemaGuidColumn GetElementTablePkColumn(Type elementType)
		{
			var elementTableName = BusinessObjectFactory.GetTableNameFromType(elementType);
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(elementTableName);
		}
	}

	public class ManyToManyRelationship<TMasterColumn, TMasterFk, TElementColumn, TElementFk> : ManyToManyAbstractRelationship
		where TMasterColumn : SchemaColumn
		where TMasterFk : struct, IZType
		where TElementColumn : SchemaColumn
		where TElementFk : struct, IZType
	{
		public ManyToManyRelationship(
			BusinessObject master,
			Type elementType,
			Type pivotObjectType,
			ZQuery filter,
			TMasterColumn pivotTableFKToMaster,
			TMasterColumn masterTableFKToPivots,
			TElementColumn pivotTableFKToElements,
			TElementColumn elementTableFKToPivots)
			: base(master, elementType, pivotObjectType, filter)
		{
			Argument.NotNull(pivotTableFKToMaster, "pivotTableFKToMaster");
			Argument.NotNull(masterTableFKToPivots, "masterTableFKToPivots");
			Argument.NotNull(pivotTableFKToElements, "pivotTableFKToElements");
			Argument.NotNull(elementTableFKToPivots, "elementTableFKToPivots");

			CheckColumnCorrectType(pivotTableFKToMaster, "pivotTableFKToMaster", typeof(TMasterFk));
			CheckColumnCorrectType(masterTableFKToPivots, "masterTableFKToPivots", typeof(TMasterFk));
			CheckColumnCorrectType(pivotTableFKToElements, "pivotTableFKToElements", typeof(TElementFk));
			CheckColumnCorrectType(elementTableFKToPivots, "elementTableFKToPivots", typeof(TElementFk));

			if (pivotTableFKToMaster.TableName != BusinessObjectFactory.GetTableNameFromType(pivotObjectType))
			{
				throw new ArgumentException("pivotTableFKToMaster must be a column on the pivot table", nameof(pivotTableFKToMaster));
			}

			if (masterTableFKToPivots.TableName != BusinessObjectFactory.GetTableNameFromType(master.GetType()))
			{
				// If the master is a NonPersistentBusinessObject, which is linked to a BusinessObject, override the NonPersistentBusinessObject's PKSchemaColumn to return the BusinessObject's PK SchemaColumn.
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "masterTableFKToPivots must be a column on the master table. masterTableFKToPivots.TableName: {0}. master type: {1}", masterTableFKToPivots.TableName, master.GetType().FullName), nameof(masterTableFKToPivots));
			}

			if (pivotTableFKToElements.TableName != BusinessObjectFactory.GetTableNameFromType(pivotObjectType))
			{
				throw new ArgumentException("pivotTableFKToElements must be a column on the pivot table", nameof(pivotTableFKToElements));
			}

			if (elementTableFKToPivots.TableName != BusinessObjectFactory.GetTableNameFromType(elementType))
			{
				throw new ArgumentException("elementTableFKToPivots must be a column on the pivot table", nameof(elementTableFKToPivots));
			}

			this.PivotTableFKToMaster = pivotTableFKToMaster;
			this.MasterTableFKToPivots = masterTableFKToPivots;
			this.PivotTableFKToElements = pivotTableFKToElements;
			this.ElementTableFKToPivots = elementTableFKToPivots;

			this.MasterFkIsPk = typeof(TMasterFk).IsAssignableFrom(typeof(ZGuid)) && MasterTableFKToPivots.IsPKColumn;
			this.ElementFkIsPk = typeof(TElementFk).IsAssignableFrom(typeof(ZGuid)) && ElementTableFKToPivots.IsPKColumn;
		}

		static void CheckColumnCorrectType(SchemaColumn parameter, string name, Type expectedType)
		{
			if (!expectedType.IsAssignableFrom(parameter.GetEquivalentZType()))
			{
				throw new ArgumentException("Column must have ZType of " + expectedType.FullName, name);
			}
		}

		protected internal readonly TMasterColumn PivotTableFKToMaster;
		protected internal readonly TMasterColumn MasterTableFKToPivots;
		protected internal readonly TElementColumn PivotTableFKToElements;
		protected internal readonly TElementColumn ElementTableFKToPivots;
		readonly bool MasterFkIsPk;
		readonly bool ElementFkIsPk;

		TMasterFk MasterFkToPivot
		{
			get { return MasterFkIsPk ? (TMasterFk)(IZType)Master.PK : (TMasterFk)Master[MasterTableFKToPivots]; }
		}

		TElementFk GetPivotFkOfElement(BusinessObject element)
		{
			return ElementFkIsPk ? (TElementFk)(IZType)element.PK : (TElementFk)element[ElementTableFKToPivots];
		}

		public override BusinessObject GetPivotObject(BusinessObject element)
		{
			InitPivotsIfRequired(element.Factory);
			_ = pivots.TryGetValue(GetPivotFkOfElement(element), out var result);
			return result;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as ManyToManyRelationship<TMasterColumn, TMasterFk, TElementColumn, TElementFk>;
			var result = rhs != null;

			result = result && base.Equals(rhs);
			result = result && Master == rhs.Master;
			result = result && PivotObjectType == rhs.PivotObjectType;
			result = result && ElementType == rhs.ElementType;
			result = result && PivotTableFKToMaster == rhs.PivotTableFKToMaster;
			result = result && MasterTableFKToPivots == rhs.MasterTableFKToPivots;
			result = result && PivotTableFKToElements == rhs.PivotTableFKToElements;
			result = result && ElementTableFKToPivots == rhs.ElementTableFKToPivots;
			return result;
		}

		public override int GetHashCode()
		{
			return Master.PK.GetHashCode() ^ ElementType.GetHashCode() ^ PivotObjectType.GetHashCode() ^ PivotTableFKToMaster.GetHashCode() ^ MasterTableFKToPivots.GetHashCode() ^ PivotTableFKToElements.GetHashCode() ^ ElementTableFKToPivots.GetHashCode();
		}

		#endregion

		#region Overrides

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
					var guids = new TElementFk[this.pivots.Count];
					this.pivots.Keys.CopyTo(guids, 0);
					query.AddToFilter(ElementTableFKToPivots, guids);
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
				foreach (var elementFK in pivots.Keys)
				{
					foreach (var element in GetElementsByKey(factory, elementFK).Where(e => e != null && e.MatchesFilter(filter)))
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

		protected virtual BusinessObject[] GetElementsByKey(BusinessObjectFactory factory, TElementFk elementFK)
		{
			var element = ElementFkIsPk ? factory.Load(ElementType, (ZGuid)(IZType)elementFK) : factory.LoadFromUniqueKey(ElementType, ElementTableFKToPivots, elementFK);
			return new[] { element };
		}

		public override bool HasChangesIncludingRelationship(BusinessObject businessObject)
		{
			var result = base.HasChangesIncludingRelationship(businessObject) ||
				!businessObject.IsInDatabase;
			if (!result)
			{
				var pivot = GetPivotObject(businessObject);
				result = pivot != null && pivot.HasChanges;
			}

			return result;
		}

		protected override void AddToRelationshipCore(BusinessObject pivot, BusinessObject businessObject)
		{
			var elementFkToPivot = GetPivotFkOfElement(businessObject);

			this.pivots.Add(elementFkToPivot, pivot);
			pivot[PivotTableFKToElements] = elementFkToPivot;
			pivot[PivotTableFKToMaster] = MasterFkToPivot;
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			using (SuppressListChanged())
			{
				var pivot = GetPivotObject(businessObject);
				if (pivot != null)
				{
					var elementFkToPivot = GetPivotFkOfElement(businessObject);
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
				(e.ListChangedType == ListChangedType.ItemChanged && (e.PropertyDescriptor == null || e.PropertyDescriptor.Name == PivotTableFKToElements.Name)) ||
				(e.ListChangedType == ListChangedType.ItemChanged && (e.PropertyDescriptor == null || e.PropertyDescriptor.Name == PivotTableFKToMaster.Name)))
			{
				this.pivots.Clear();
				foreach (DataRowView row in PivotDataView)
				{
					var pivot = Master.Factory.Load(PivotObjectType, (Guid)row[0]);
					if (!pivot.IsDeleted)
					{
						var elementFk = GetElementFkOfPivot(pivot);
						if (!elementFk.IsEmpty && !pivots.ContainsKey(elementFk))
						{
							this.pivots.Add(elementFk, pivot);
						}
					}
				}

				OnRelationshipFilterChanged(EventArgs.Empty);
			}
		}

		protected virtual TElementFk GetElementFkOfPivot(BusinessObject pivot)
		{
			return (TElementFk)pivot[PivotTableFKToElements];
		}

		protected override string GetPivotDataViewRowFilter()
		{
			return PivotTableFKToMaster.Name + "='" + MasterFkToPivot + "'";
		}

		#endregion

		#region Implementation

		internal readonly Dictionary<TElementFk, BusinessObject> pivots = new Dictionary<TElementFk, BusinessObject>();

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
					InitPivots(factory, PivotTableFKToMaster, PivotTableFKToElements);
				}
			}
		}

		protected virtual void InitPivots(BusinessObjectFactory factory, TMasterColumn pivotTableFKToMaster, TElementColumn pivotTableFKToElements)
		{
			var query = new ZQuery(pivotTableFKToMaster, MasterFkToPivot);
			if (pivotTableFKToElements.IsNullable)
			{
				query.AddToFilter(pivotTableFKToElements, SQLComparisonOperator.NotEqual, DBNull.Value);
			}

			if (AdditionalDivotFilter != null)
			{
				query.AddToFilter(AdditionalDivotFilter);
			}
			query.FetchOnlyFromLocalCache = !Master.IsInDatabase;
			var allPivots = factory.Load(PivotObjectType, query);

			foreach (var pivot in allPivots.Where(p => IsRelevantPivot(p)))
			{
				var key = GetElementFkOfPivot(pivot);
				if (!pivots.ContainsKey(key))
				{
					pivots.Add(key, pivot);
				}
				else
				{
					ErrorReporter.ReportOnce("InitPivotsIfRequired_DuplicatePivots_" + pivotTableFKToMaster.Name,
						string.Format(
@"There was an attempt to load duplicate pivot to ManyToManyRelationship in collection with:
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
							pivotTableFKToElements.Name,
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
