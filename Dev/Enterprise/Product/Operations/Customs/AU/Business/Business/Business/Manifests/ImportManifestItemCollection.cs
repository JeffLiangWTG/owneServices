
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportManifestItemCollection : NonPersistentBusinessObjectCollection<ImportManifestItem>
	{
		public ImportManifestItemCollection(CusSeaManTranHead manifest, JobSailing sailing, bool populate)
			: base(manifest.Factory)
		{
			this.Manifest = manifest;
			this.sailing = sailing;

			if (manifest != null && populate)
			{
				PopulateParentElements();
			}
		}

		public readonly CusSeaManTranHead Manifest;
		public int SavedBillsCount { get; set; }
		readonly JobSailing sailing;

		public void PopulateParentElements()
		{
			foreach (CusSeaManOBLHeader oceanBill in Manifest.OceanBills)
			{
				if (!oceanBill.IsInDatabase)
				{
					Add(new ImportManifestItem(oceanBill, sailing, this, Factory));
				}
			}

			foreach (CusSeaManArrivalPort port in Manifest.Arrivals)
			{
				foreach (CusSeaManOBLHeaderCargoLine cargoLine in port.CargoLines)
				{
					if (!cargoLine.IsInDatabase)
					{
						ImportManifestItem currentManifestItem = ProcessCargoLine(cargoLine, sailing, Factory);
						if (cargoLine.Port.Messages.IsMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.CARLST }, string.Empty, ignoreDiscardedMessages: true))
						{
							currentManifestItem.BillNumberInfo.AddWarning("Cargolist is already reported to Customs. Any additions to Cargolist must be done via Customs Import Manifest");
						}
					}
				}
			}
		}

		ImportManifestItem ProcessCargoLine(CusSeaManOBLHeaderCargoLine cargoLine, JobSailing sailing, BusinessObjectFactory factory)
		{
			foreach (ImportManifestItem item in this)
			{
				if (item.IsCargoList && item.BillNumber == cargoLine.BO_OceanBill)
				{
					item.RelatedBusinessObjectList.Add(cargoLine);
					return item;
				}
			}
			ImportManifestItem result = new ImportManifestItem(cargoLine, sailing, this, factory);
			Add(result);

			return result;
		}

		#region Items Selection

		public void SelectItems(ZString port, bool shouldSave)
		{
			foreach (ImportManifestItem item in ItemsView)
			{
				if (item.DischargePort == port)
				{
					item.ShouldSave = shouldSave;
				}
			}
		}

		public void SelectAll(bool shouldSave)
		{
			foreach (ImportManifestItem item in ItemsView)
			{
				item.ShouldSave = shouldSave;
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ImportManifestItem(null, sailing, this, Factory);
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Items saving

		public bool HasItemsToSave
		{
			get { return hasItemsToSave; }
		}
		bool hasItemsToSave;

		public void RefineItemsToSave()
		{
			foreach (ImportManifestItem item in this)
			{
				if (!item.ShouldSave || item.HasWarnings)
				{
					item.DeleteRelatedBusinessObjects();
				}
				else
				{
					hasItemsToSave = true;
				}
			}

			isNotRefreshViewItems = true;
			ItemsView.RemoveAll();
			RemoveAndDeleteAll();
			isNotRefreshViewItems = false;
		}

		public bool IsRefinedForSave
		{
			get { return isRefinedForSave; }
			set { isRefinedForSave = value; }
		}
		bool isRefinedForSave;

		#endregion

		#region Lookups

		public ImportManifestItemCollection ItemsView
		{
			get
			{
				if (itemsView == null)
				{
					itemsView = new ImportManifestItemCollection(Manifest, sailing, false);
					RefreshItemsView();
				}

				return itemsView;
			}
		}
		ImportManifestItemCollection itemsView;

		#endregion

		#region Refresh Items View

		public void RefreshItemsView()
		{
			if (!isNotRefreshViewItems)
			{
				foreach (ImportManifestItem item in this)
				{
					if (item.DischargePort != item.SelectedPort && item.SelectedPort != "ALL" && ItemsView.Contains(item))
					{
						item.ShouldSave = false;
						ItemsView.Remove(item);
					}
					else if ((item.DischargePort == item.SelectedPort || item.SelectedPort == "ALL") && !ItemsView.Contains(item))
					{
						ItemsView.Add(item);
					}
				}
			}
		}

		bool isNotRefreshViewItems;

		#endregion
	}
}
