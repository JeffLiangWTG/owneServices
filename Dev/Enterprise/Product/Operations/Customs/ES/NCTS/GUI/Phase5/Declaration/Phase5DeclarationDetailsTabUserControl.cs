using System;
using System.Linq;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Phase5DeclarationDetailsTabUserControl : EU.NCTS.GUI.Phase5DeclarationDetailsTabUserControl
	{
		public Phase5DeclarationDetailsTabUserControl()
		{
			InitializeComponent();
		}

		NctsHeader Header => (NctsHeader)DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var currentHeader = Header;
			if (currentHeader != null)
			{
				ModifyControlsVisibilityForTNN(currentHeader.ESNctsHeader.CEN_TNNArrival);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnhookEvents();

			base.OnCurrentDataItemChanged(e);

			HookEvents();
			var currentHeader = Header;
			if (currentHeader != null)
			{
				ModifyControlsVisibilityForTNN(currentHeader.ESNctsHeader.CEN_TNNArrival);
			}
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			var currentHeader = Header;
			if (currentHeader != null)
			{
				currentHeader.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged += CEN_TNNArrivalInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var currentHeader = Header;
			if (currentHeader != null)
			{
				currentHeader.ESNctsHeader.CEN_TNNArrivalInfo.ValueChanged -= CEN_TNNArrivalInfo_ValueChanged;
			}
		}

		#endregion

		void CEN_TNNArrivalInfo_ValueChanged(object sender, EventArgs e) => ModifyControlsVisibilityForTNN(Header.ESNctsHeader.CEN_TNNArrival);

		void ModifyControlsVisibilityForTNN(bool isTNN)
		{
			var isVisible = !isTNN;
			var guaranteesGroupBox = (ZGroupBox)Controls.Find("GuaranteesGroupBox", true).FirstOrDefault();
			guaranteesGroupBox.Visible = isVisible;

			var declarationDetailsTabControl = (ZTabControl)Controls.Find("DeclarationDetailsTabControl", true).FirstOrDefault();
			if (declarationDetailsTabControl != null)
			{
				var tabPages = declarationDetailsTabControl.AllTabPages;
				ChangeTabVisibilityForTNN(tabPages, "AuthorizationsTabPage", isVisible);
				ChangeTabVisibilityForTNN(tabPages, "SupplyChainActorTabPage", isVisible);
				ChangeTabVisibilityForTNN(tabPages, "PreviousDocumentsTabPage", isVisible);

				var isInPhase5TransitionPeriod = Header.IsInPhase5TransitionPeriod;
				var isVisibleForProvisionalPeriod = !(isTNN && isInPhase5TransitionPeriod);
				ChangeTabVisibilityForTNN(tabPages, "SupportingDocumentsTabPage", isVisibleForProvisionalPeriod);
				ChangeTabVisibilityForTNN(tabPages, "AdditionalDocumentsTabPage", isVisibleForProvisionalPeriod);
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
