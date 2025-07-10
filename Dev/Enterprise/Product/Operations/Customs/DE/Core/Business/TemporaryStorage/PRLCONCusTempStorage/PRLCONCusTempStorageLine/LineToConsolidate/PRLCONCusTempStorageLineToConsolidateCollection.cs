using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageLineToConsolidateCollection : DecCusTempStorageLineCollectionFrom<PRLCONCusTempStorageLineToConsolidate, PRLCONCusTempStorageDec>
	{
		public PRLCONCusTempStorageLineToConsolidateCollection(PRLCONCusTempStorageDec declaration) : base(declaration)
		{
		}

		protected override BusinessObject AddNewCore() => AddNewCore(typeof(PRLCONCusTempStorageLineToConsolidate));

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			using (child.SuspendSettingHasChanges())
			{
				var lineToConsolidate = (PRLCONCusTempStorageLineToConsolidate)child;
				var consolidatedLine = Master?.ConsolidatedLine;
				if (consolidatedLine != null && !consolidatedLine.IsDeleted && lineToConsolidate.Pivot == null)
				{
					var pivot = Factory.New<CusTempStorageLinePivot>();
					pivot.SLR_TSL_ToLine = consolidatedLine.PK;
					pivot.SLR_TSL_FromLine = lineToConsolidate.PK;
				}
			}
		}

		protected override ZDBOnlyQuery GetRelationshipFilterBasedOnParent()
		{
			var subQuery = new ZDBOnlyQuery(typeof(CusTempStorageLine));
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(CusTempStorageLinePivot), CusTempStorageLinePivotSchema.SLR_TSL_FromLine);
			subQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);
			return subQuery;
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (!IsLoading)
			{
				var consolidatedLine = Master?.ConsolidatedLine;
				if (consolidatedLine != null && !consolidatedLine.IsDeleted)
				{
					consolidatedLine.RecalculatePackageQuantity();
				}
			}
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			var line = bizO as PRLCONCusTempStorageLineToConsolidate;
			if (line != null && line.SequenceNumberEnabled)
			{
				line.Dec?.LineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(line);
			}
		}
	}
}
