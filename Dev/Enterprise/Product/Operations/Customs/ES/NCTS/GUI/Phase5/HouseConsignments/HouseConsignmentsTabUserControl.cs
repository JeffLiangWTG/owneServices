using System;
using System.Linq;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class HouseConsignmentsTabUserControl : EU.NCTS.GUI.HouseConsignmentsTabUserControl
	{
		public HouseConsignmentsTabUserControl()
		{
			InitializeComponent();
		}

		NctsBill Bill => CurrentDataItem as NctsBill;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var currentBill = Bill;
			if (currentBill != null)
			{
				ModifyControlsVisibilityForTNN(currentBill.Header.ESNctsHeader.CEN_TNNArrival);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();

			base.OnCurrentDataItemChanged(e);

			HookEvents();
			var currentBill = Bill;
			if (currentBill != null)
			{
				ModifyControlsVisibilityForTNN(currentBill.Header.ESNctsHeader.CEN_TNNArrival);
			}
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			var currentBill = Bill;
			if (currentBill != null)
			{
				currentBill.Header.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged += CEN_TNNArrivalInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var currentBill = Bill;
			if (currentBill != null)
			{
				currentBill.Header.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged -= CEN_TNNArrivalInfo_ValueChanged;
			}
		}

		#endregion

		void CEN_TNNArrivalInfo_ValueChanged(object sender, EventArgs e) => ModifyControlsVisibilityForTNN(Bill.Header.ESNctsHeader.CEN_TNNArrival);

		void ModifyControlsVisibilityForTNN(bool isTNN)
		{
			var isVisible = !isTNN;

			var houseConsignmentTabControl = (ZTabControl)Controls.Find("HouseConsignmentTabControl", true).FirstOrDefault();
			if (houseConsignmentTabControl != null)
			{
				var tabPages = houseConsignmentTabControl.AllTabPages;
				ChangeTabVisibilityForTNN(tabPages, "HouseConsignmentPreviousDocumentsTabPage", isVisible);

				var isInPhase5TransitionPeriod = Bill.IsInPhase5TransitionPeriod;
				var isVisibleForProvisionalPeriod = !(isTNN && isInPhase5TransitionPeriod);
				ChangeTabVisibilityForTNN(tabPages, "HouseConsignmentSupportingDocumentsTabPage", isVisibleForProvisionalPeriod);
				ChangeTabVisibilityForTNN(tabPages, "HouseConsignmentAdditionalDocumentsTabPage", isVisibleForProvisionalPeriod);
			}

			void ChangeTabVisibilityForTNN(System.Windows.Forms.TabPage[] allTabPages, string tabName, bool tabVisible)
			{
				var prevDocTabPage = (ZTabPage)allTabPages.FirstOrDefault(x => x.Name.Equals(tabName));
				if (prevDocTabPage != null)
				{
					prevDocTabPage.TabVisible = tabVisible;
				}
			}
		}
	}
}
