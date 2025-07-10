using System;
using System.Linq;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class Phase5GoodsItemsTabUserControl : EU.NCTS.GUI.Phase5GoodsItemsTabUserControl
	{
		public Phase5GoodsItemsTabUserControl() : base() { }

		NctsDepartureCargoDesc GoodsItem => CurrentDataItem as NctsDepartureCargoDesc;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var currentGoodsItem = GoodsItem;
			if (currentGoodsItem != null)
			{
				PreviousDocumentsTabVisibility(currentGoodsItem.Header.ESNctsHeader.CEN_TNNArrival);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();

			base.OnCurrentDataItemChanged(e);

			HookEvents();
			var currentGoodsItem = GoodsItem;
			if (currentGoodsItem != null)
			{
				PreviousDocumentsTabVisibility(currentGoodsItem.Header.ESNctsHeader.CEN_TNNArrival);
			}
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			var currentGoodsItem = GoodsItem;
			if (currentGoodsItem != null)
			{
				currentGoodsItem.Header.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged += CEN_TNNArrivalInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var currentGoodsItem = GoodsItem;
			if (currentGoodsItem != null)
			{
				currentGoodsItem.Header.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged -= CEN_TNNArrivalInfo_ValueChanged;
			}
		}

		#endregion
			void CEN_TNNArrivalInfo_ValueChanged(object sender, EventArgs e) => PreviousDocumentsTabVisibility(GoodsItem.Header.ESNctsHeader.CEN_TNNArrival);
		
		void PreviousDocumentsTabVisibility(bool isTNN)
		{
			var prevDocTabPage = (ZTabPage)GoodsItemTabControl.AllTabPages.FirstOrDefault(x => x.Name.Equals("GoodsItemPreviousDocumentsTabPage"));
			if (prevDocTabPage != null)
			{
				prevDocTabPage.TabVisible = !isTNN;
			}
		}
	}
}
