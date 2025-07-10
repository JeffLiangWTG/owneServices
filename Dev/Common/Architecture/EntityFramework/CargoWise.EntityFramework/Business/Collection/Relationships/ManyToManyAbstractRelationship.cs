using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public abstract class ManyToManyAbstractRelationship : CollectionRelationship
	{
		protected ManyToManyAbstractRelationship(BusinessObject master,
			Type elementType,
			Type pivotObjectType,
			ZQuery filter) : base(elementType, filter)
		{
			this.master = master;
			this.PivotObjectType = pivotObjectType;
		}

		public abstract BusinessObject GetPivotObject(BusinessObject element);

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache) &&
					GetPivotObject(businessObject) != null;
		}

		public override BusinessObject Master
		{
			get { return master; }
		}
		protected  readonly BusinessObject master;

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

		public override void ClearHasChangesIncludingRelationship(BusinessObject businessObject)
		{
			base.ClearHasChangesIncludingRelationship(businessObject);

			var pivot = GetPivotObject(businessObject);
			if (pivot != null)
			{
				pivot.ClearHasChanges();
			}
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			using (SuppressListChanged())
			{
				var pivot = GetPivotObject(businessObject);
				if (pivot == null)
				{
					pivot = businessObject.Factory.New(PivotObjectType);
					AddToRelationshipCore(pivot, businessObject);
				}
			}
		}

		protected abstract void AddToRelationshipCore(BusinessObject pivot, BusinessObject businessObject);

		#region Refreshed event

		protected abstract void PivotDataView_ListChanged(object sender, ListChangedEventArgs e);

		protected DataView PivotDataView
		{
			get
			{
				if (pivotDataView == null)
				{
					pivotDataView = DataViewCache.GetDataView(PivotDataTable, GetPivotDataViewRowFilter(), "", DataViewRowState.CurrentRows);
				}
				return pivotDataView;
			}
		}
		DataView pivotDataView;

		protected abstract string GetPivotDataViewRowFilter();

		DataTable PivotDataTable
		{
			get
			{
				if (pivotDataTable == null)
				{
					pivotDataTable = Master.Factory.RowFactory.GetTable(PivotTableName);
				}
				return pivotDataTable;
			}
		}
		DataTable pivotDataTable;

		public IDisposable SuppressListChanged()
		{
			listChangedSuspendedIndex++;
			PivotDataView.ListChanged -= PivotDataView_ListChanged;
			return new DisposableAction(delegate
			{
				listChangedSuspendedIndex--;
				if (listChangedSuspendedIndex == 0)
				{
					PivotDataView.ListChanged += PivotDataView_ListChanged;
					OnRelationshipFilterChanged(EventArgs.Empty);
				}
			});
		}
		int listChangedSuspendedIndex;

		#endregion

		#region Implementation

		protected readonly Type PivotObjectType;

		internal string PivotTableName
		{
			get { return BusinessObjectFactory.GetTableNameFromType(PivotObjectType); }
		}

		protected bool isPivotsInitialized;

		public ZQuery AdditionalDivotFilter
		{
			get;
			set;
		}

		protected virtual string GetPivotDebugInformation(BusinessObject pivot, IEnumerable<BusinessObject> duplicatePivots)
		{
			return "";
		}

		protected virtual string DebugLogMessage => string.Empty;

		protected virtual bool IsRelevantPivot(BusinessObject pivot)
		{
			return true;
		}

		#endregion
	}
}
