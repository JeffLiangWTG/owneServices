using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionFinalizer : NonPersistentBusinessObject
	{
		public CommissionFinalizer()
			: base()
		{
		}

		public int MaxRowsToLoad => OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.Value;

		#region Find

		public void Find(FilterStripBusinessObject filterBizObj)
		{
			var filter = filterBizObj.Filter;
			if (!filter.IsTopNQuery)
			{
				filter.MaximumRows = MaxRowsToLoad + 1;
			}

			var newFactory = new BusinessObjectFactory();
			CommissionFinalizerLineItemCollection.SwapFactoryAndRemoveAll(newFactory);
			ViewCommissionLineCollection.AdditionalFilter = ZQuery.NoResultQuery;   // clear the collection before changing factory, otherwise dependent collections will unnecessarily refresh themselves
			((IActiveBusinessObjectCollection)ViewCommissionLineCollection).SetFactory(newFactory);
			ViewCommissionLineCollection.AdditionalFilter = filter;
		}

		#endregion

		#region ApprovalRequest

		public AccCommissionApprovalRequest GetNewApprovalRequest(BusinessObjectFactory factory)
		{
			var request = factory.New<AccCommissionApprovalRequest>();
			foreach (var lineItem in CommissionFinalizerLineItems.Where(x => x.IsSelected))
			{
				var item = request.Items.AddNew();
				using (item.SuspendSettingHasChanges())
				{
					item.CRI_CL0 = lineItem.ViewCommissionLine.PK;
					item.CRI_IsSelected = true;
				}
			}

			return request;
		}

		#endregion

		#region CommissionPayment

		public CommissionPayment GetNewCommissionPayment()
		{
			var commissionLinesForPayment =
				from item in CommissionFinalizerLineItems
				let line = item.ViewCommissionLine
				where
					item.IsSelected &&
					line != null
				select line;

			return new CommissionPayment(CommissionFinalizerLineItemCollection.Factory, commissionLinesForPayment);
		}

		#endregion

		#region Related Business Objects

		#region ViewCommissionLineCollection

		public ViewCommissionLineCollection ViewCommissionLineCollection
		{
			get
			{
				if (viewCommissionLineCollection == null)
				{
					viewCommissionLineCollection = new ViewCommissionLineCollection(new BusinessObjectFactory());
					viewCommissionLineCollection.AdditionalFilter = ZQuery.NoResultQuery;
				}

				return viewCommissionLineCollection;
			}
		}
		ViewCommissionLineCollection viewCommissionLineCollection;

		#endregion

		#region CommissionFinalizerLineItemCollection

		public CommissionFinalizerLineItemCollection CommissionFinalizerLineItemCollection
		{
			get
			{
				if (commissionFinalizerLineItemCollection == null)
				{
					commissionFinalizerLineItemCollection = new CommissionFinalizerLineItemCollection(ViewCommissionLineCollection.Factory);
					RefreshCommissionFinalizerLineItemCollection();
					ViewCommissionLineCollection.CountChanged += ViewCommissionLineCollection_CountChanged;
				}

				return commissionFinalizerLineItemCollection;
			}
		}
		CommissionFinalizerLineItemCollection commissionFinalizerLineItemCollection;

		public IEnumerable<CommissionFinalizerLineItem> CommissionFinalizerLineItems
		{
			get { return CommissionFinalizerLineItemCollection.Cast<CommissionFinalizerLineItem>(); }
		}

		void ViewCommissionLineCollection_CountChanged(object sender, System.EventArgs e)
		{
			RefreshCommissionFinalizerLineItemCollection();
		}

		void RefreshCommissionFinalizerLineItemCollection()
		{
			using (commissionFinalizerLineItemCollection.SuspendListChanged())
			{
				commissionFinalizerLineItemCollection.RemoveAndDeleteAll();

				foreach (var commissionLine in ViewCommissionLineCollection)
				{
					commissionFinalizerLineItemCollection.Add(new CommissionFinalizerLineItem(commissionLine));
				}
			}
		}

		#endregion

		#region CommissionFinalizerLineItemGroupingCollection

		public TopLevelCommissionFinalizerLineItemGroupingCollection CommissionFinalizerLineItemGroupingCollection
		{
			get
			{
				if (commissionFinalizerLineItemGroupingCollection == null)
				{
					commissionFinalizerLineItemGroupingCollection = new TopLevelCommissionFinalizerLineItemGroupingCollection(CommissionFinalizerLineItemCollection);
					commissionFinalizerLineItemGroupingCollection.Init();
				}

				return commissionFinalizerLineItemGroupingCollection;
			}
		}
		TopLevelCommissionFinalizerLineItemGroupingCollection commissionFinalizerLineItemGroupingCollection;

		#endregion

		#endregion
	}
}
