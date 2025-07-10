using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract partial class MatchingModule : ZFilterGridModule
	{
		public MatchingModule() : base()
		{
		}

		protected string UnmatchMenuText
		{
			get { return f_UnmatchMenuText ?? (f_UnmatchMenuText = Res.GetString("d6adce03-2c12-4b7b-ba28-c051101f93b9", "&Unmatch")); }
		}
		string f_UnmatchMenuText;

		protected abstract ControllerID GetControllerID { get; }

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController newController = ZControllerFactory.Create(GetControllerID);
			if (newController is MatchingController)
			{
				((MatchingController)newController).RefreshGrid += new EventHandler(MatchingModule_RefreshGrid);
			}
			return newController;
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("ee5caeef-4152-4c00-a58d-6462f3d4e07d", "Unmatch", "Un-matches the selected item to revert the transactions to outstanding status (shortcut Del)");
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			if (DataTransferMenuItem != null)
			{
				menuItems.Remove(DataTransferMenuItem);
			}
			return menuItems.ToArray();
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return new SortInfo(UnmatchingRow.Schema.MatchGroupNum, ListSortDirection.Ascending); }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new UnmatchingRowCollection(Factory);
		}

		protected void MatchingModule_RefreshGrid(object sender, EventArgs e)
		{
			PerformSearch();
		}

		#region Collection Loading

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var filterHelper = ((MatchingBaseFilterBusinessObject)FilterBusinessObject).MatchGroupFilter;
			filterHelper.MaximumRows = query.MaximumRows;

			try
			{
				var bizos = UnmatchingRowCollection.LoadFilterHelper(factory, filterHelper);
				return PerformSearchResult.Success(factory, query, bizos.Cast<BusinessObject>().ToArray(), true);
			}
			finally
			{
				filterHelper.MaximumRows = null;
			}
		}

		#endregion

		#region UpdateMatchedTransactionGridInFilterControl

		void UpdateMatchedTransactionGridInFilterControl()
		{
			if (FilterBusinessObject is MatchingBaseFilterBusinessObject matchingFilterBusinessObject)
			{
				if (GridCollection is UnmatchingRowCollection)
				{
					if (GridCollection.Count == 0)
					{
						matchingFilterBusinessObject.ClearCurrentTransactionsForMatchGroup();
					}
					else if (GridCollection.Count == 1)
					{
						matchingFilterBusinessObject.SetCurrentTransactionsForMatchGroup(((UnmatchingRowCollection)GridCollection)[0]);
					}
				}
			}
		}

		#endregion

		void GridCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsPerformingSearch)
			{
				ResultCountMessage.UpdateResultCountMessage(GridCollection.Count);
				UpdateModuleResultsCache();
			}
		}
	}
}
