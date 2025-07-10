using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5DeclarationDetailsTabUserControl : ZUserControl
	{
		public Phase5DeclarationDetailsTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			DeclarationDetailsTabControl.AddAdditionalTabs(BindingSource, LayoutProvider.AdditionalDeclarationDetailTabPages);
			DeclarationDetailsTabControl.ReorderTabs(LayoutProvider.ReorderDeclarationDetailTabPagesNames);

			if (DataSource?.MovementHeader is NctsDepartureMovementHeader moveHeader)
			{
				moveHeader.OnFactorySavingAndGrossWeightInvalid -= ShowUpdateTotalGrossWeightsDialog;
				moveHeader.OnFactorySavingAndGrossWeightInvalid += ShowUpdateTotalGrossWeightsDialog;
			}

			SetDeclarationDetailsLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (DataSource?.MovementHeader is NctsDepartureMovementHeader moveHeader)
				{
					moveHeader.OnFactorySavingAndGrossWeightInvalid -= ShowUpdateTotalGrossWeightsDialog;
				}
			}

			base.Dispose(disposing);
		}

		void SetDeclarationDetailsLayout()
		{
			DynamicTraderDetailsPanel.UpdateLayout(LayoutProvider.TraderDetailsPanelLayout);
			DynamicDepartureDetailsPanel.UpdateLayout(LayoutProvider.DepartureDetailsPanelLayout);
			BindingSource.SetBindingMember(DynamicDeclarationDetailsPanel, nameof(NctsHeader.MovementHeader));
			DynamicDeclarationDetailsPanel.UpdateLayout(LayoutProvider.DeclarationDetailsPanelLayout);
			SecurityAtDepartureDynamicLayoutPanel.UpdateLayout(LayoutProvider.SecurityAtDeparturePanelLayout);
			DynamicCustomsOfficesUserControl.UserControlType = LayoutProvider.CustomsOfficesUserControlType;
			BindingSource.SetBindingMember(DynamicGuaranteesUserControl, nameof(NctsHeader.MovementHeader));
			DynamicGuaranteesUserControl.UserControlType = LayoutProvider.GuaranteesUserControlType;
			DynamicAuthorizationsTabUserControl.UserControlType = LayoutProvider.DeclarationAuthorizationsTabControlType;
			DynamicAdditionalDocumentsTabUserControl.UserControlType = LayoutProvider.DeclarationAdditionalDocumentsTabControlType;
			DynamicSupportingDocumentsTabUserControl.UserControlType = LayoutProvider.DeclarationSupportingDocumentsTabControlType;
			DynamicPreviousDocumentsTabUserControl.UserControlType = LayoutProvider.DeclarationPreviousDocumentsTabControlType;
			BindingSource.SetBindingMember(DynamicSupplyChainActorTabUserControl, nameof(NctsHeader.MovementHeader));
			DynamicSupplyChainActorTabUserControl.UserControlType = LayoutProvider.DeclarationSupplyChainActorsTabControlType;
		}

		void ShowUpdateTotalGrossWeightsDialog(object sender, EventArgs e)
		{
			if (UpdateTotalGrossWeightDialog == DialogResult.OK)
			{
				((NctsDepartureMovementHeader)sender).UpdateBM_GrossWeightFromBills();
			}
		}

		static DialogResult UpdateTotalGrossWeightDialog => Globals.Message.Show(
			Res.GetString("609BD059-E058-4450-8823-A5693FCFBDE9", "Total Gross Weight is lower than sum of Gross Weights in House Consignments. Update Total Gross Weight with Calculated value?"),
			Res.GetString("1e94ab1c-5448-487a-978c-df5eb52c11fb", "Warning"),
			MessageBoxButtons.OKCancel,
			MessageBoxIcon.Warning);

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;
	}
}
