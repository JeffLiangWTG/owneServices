using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class LogsView : NonPersistentBusinessObject, IObsoleteValidation
	{
		public LogsView(IStmALogParent parent, LogsToShow defaultLogsToShow)
		{
			this.Parent = parent;
			collection = new StmALogCollectionView(parent);
			collection.LogsToShow = defaultLogsToShow;
		}

		public readonly IStmALogParent Parent;

		public StmALogCollectionView Collection
		{
			get { return collection; }
		}
		readonly StmALogCollectionView collection;

		public LogsToShow LogsToShow
		{
			get { return Collection.LogsToShow; }
			set { Collection.LogsToShow = value; }
		}

		#region IncludeCancelled

		public ZBool IncludeCancelled
		{
			get { return Collection.IncludeCancelled; }
			set
			{
				Collection.IncludeCancelled = value;
				IncludeCancelledInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeCancelledInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeCancelled)); }
		}

		#endregion

		#region IncludeEstimates

		public ZBool IncludeEstimates
		{
			get { return Collection.IncludeEstimates; }
			set
			{
				Collection.IncludeEstimates = value;
				IncludeEstimatesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeEstimatesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeEstimates)); }
		}

		#endregion

		#region BizObjNameToShowEventsFor

		[BusinessObjectTestExclude]
		[CargoWise.ComponentModel.MaxLength(30)]
		public ZString BizObjNameToShowEventsFor
		{
			get { return Collection.BizObjNameToShowEventsFor; }
			set
			{
				Collection.BizObjNameToShowEventsFor = value;
				BizObjNameToShowEventsForInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BizObjNameToShowEventsForInfo
		{
			get { return GetZPropertyInfo(nameof(BizObjNameToShowEventsFor)); }
		}

		public CodeDescriptionPairList BizObjNameToShowEventsFor_List
		{
			get { return Collection.BizObjNameToShowEventsFor_List; }
		}

		#endregion
	}
}
