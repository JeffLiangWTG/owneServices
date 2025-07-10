using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
			InitializeDV1DetailsTabPage();
			if (IsEquipmentSupported)
			{
				InitializeTransportEquipmentTabPage();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			InitTabsVisibility();
		}

		public override BaseJobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set
			{
				base.JobDeclaration = value;

				var declaration = (JobDeclaration)JobDeclaration;
				if (declaration != null)
				{
					declaration.ZG_IsHighValueOvrdInfo.ValueChanged -= JobDeclaration_ZG_IsHighValueOvrdChanged;
					declaration.ZG_IsHighValueOvrdInfo.ValueChanged += JobDeclaration_ZG_IsHighValueOvrdChanged;
				}
			}
		}

		void InitTabsVisibility()
		{
			SetDV1DetailsTabVisibility();
			SetTransportEquipmentTabVisibility();
			SetInvoiceGroupingTabVisibility();
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			BaseCustomsSupplierHeaderUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new EUImportSupplierHeaderUserControl();
			}
			else
			{
				result = new EUExportSupplierHeaderUserControl();
			}
			return result;
		}

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new EUImportInvoiceLineUserControl();
			}
			else
			{
				result = new EUExportInvoiceLineUserControl();
			}
			return result;
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new EntryMessageUserControl(JobDeclaration);

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new EUJobDeclarationUserControl();

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl() => new CustomsCusContainersWithTrackingAndAdditionalSealUserControl();

		protected override void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == DV1DetailsTabPage)
			{
				LoadDV1DetailsTabPage();
			}
			else if (sender == TransportEquipmentTabPage)
			{
				LoadTransportEquipmentTabPage();
			}
			else
			{
				base.LazyCreateControlsFired(sender, e);
			}
		}

		protected override void JobDeclaration_JE_MessageTypeChanged(object sender, EventArgs e)
		{
			base.JobDeclaration_JE_MessageTypeChanged(sender, e);
			SetDV1DetailsTabVisibility();
			SetTransportEquipmentTabVisibility();
			SetInvoiceGroupingTabVisibility();
		}

		void JobDeclaration_ZG_IsHighValueOvrdChanged(object sender, EventArgs e)
		{
			SetDV1DetailsTabVisibility();
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab == EntryInstructionDetailsTabPage)
			{
				SetEntryInstructionDetailsTabPagesVisibility();
			}
			if (MainTabControl.SelectedTab == InvoiceLinesTabPage)
			{
				SetInvoiceLineDetailsTabPagesVisibility();
			}

			UpdateInvoiceLineBottomPanelVisibility(MainTabControl.SelectedTab);
		}

		void UpdateInvoiceLineBottomPanelVisibility(TabPage selectedTab)
		{
			if (fInvoiceLines?.ParentForm is JobDeclarationForm {
				PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel: ZPanel panel })
			{
				var isInvoiceLineTab = selectedTab == InvoiceLinesTabPage;
				panel.Visible = isInvoiceLineTab;
			}
		}

		void SetEntryInstructionDetailsTabPagesVisibility()
		{
			if (fBaseCustomsEntryInstructionUserControl is EntryInstructionDetailsUserControl entryInstructionDetailsUserControl)
			{
				entryInstructionDetailsUserControl.SetTabPagesVisibility();
			}
		}

		void SetInvoiceLineDetailsTabPagesVisibility()
		{
			if (fInvoiceLines is EUImportInvoiceLineUserControl importInvoiceLineUserControl)
			{
				importInvoiceLineUserControl.SetTabPagesVisibility();
			}
		}

		#region DV1Details Tab Page

		void SetDV1DetailsTabVisibility()
		{
			var declaration = JobDeclaration as JobDeclaration;
			DV1DetailsTabPage.TabRelevant = declaration != null && declaration.ZG_IsHighValueOvrd && declaration.DV1DetailsSupport;
		}

		void InitializeDV1DetailsTabPage()
		{
			dv1DetailsTabPage = new BaseDeclarationTabPage();
			dv1DetailsTabPage.LazyCreateControls += LazyCreateControlsFired;
			dv1DetailsTabPage.CaptionResourceString = Res.GetData("58d0f3cd-a7d1-4580-9733-d434ebf120c6", "D.V.1 Details");
			dv1DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			dv1DetailsTabPage.Name = "DV1DetailsTabPage";
			dv1DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			dv1DetailsTabPage.TabIndex = 15;
			MainTabControl.SuspendLayout();
			var indexOfEntryInstructionDetailsTabPage = MainTabControl.TabPages.IndexOf(EntryInstructionDetailsTabPage);
			if (indexOfEntryInstructionDetailsTabPage > -1)
			{
				MainTabControl.TabPages.Insert(DV1DetailsTabPage, indexOfEntryInstructionDetailsTabPage);
			}
			MainTabControl.ResumeLayout(false);
		}

		void LoadDV1DetailsTabPage()
		{
			if (DV1DetailsTabPage.Controls.Count == 0 && DV1DetailsTabPage.TabVisible)
			{
				dv1UserControl = GetDV1UserControl();
				dv1UserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				dv1UserControl.Name = "DV1UserControl";
				dv1UserControl.Visible = false;
				DV1DetailsTabPage.Controls.Add(dv1UserControl);
				dv1UserControl.Visible = true;
				dv1UserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		public BaseDeclarationTabPage DV1DetailsTabPage => dv1DetailsTabPage;
		BaseDeclarationTabPage dv1DetailsTabPage;

		DV1UserControl dv1UserControl;

		protected virtual DV1UserControl GetDV1UserControl() => new DV1UserControl();

		#endregion

		#region Transport Equipment Tab Page

		void InitializeTransportEquipmentTabPage()
		{
			transportEquipmentTabPage = new BaseDeclarationTabPage();
			transportEquipmentTabPage.LazyCreateControls += LazyCreateControlsFired;
			transportEquipmentTabPage.CaptionResourceString = Res.GetData("23597f7e-743e-43b3-8fcc-450ec9cf77be", "Transport Equipment");
			transportEquipmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			transportEquipmentTabPage.Name = "TransportEquipmentTabPage";
			transportEquipmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			transportEquipmentTabPage.TabIndex = 16;
			MainTabControl.SuspendLayout();
			var indexOfPackingTabPage = MainTabControl.TabPages.IndexOf(PackingTabPage);
			if (indexOfPackingTabPage > -1)
			{
				MainTabControl.TabPages.Insert(transportEquipmentTabPage, indexOfPackingTabPage);
			}
			MainTabControl.ResumeLayout(false);
		}

		void LoadTransportEquipmentTabPage()
		{
			if (TransportEquipmentTabPage.Controls.Count == 0 && TransportEquipmentTabPage.TabVisible)
			{
				transportEquipmentUserControl = new TransportEquipmentUserControl();
				transportEquipmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				transportEquipmentUserControl.Name = "TransportEquipmentUserControl";
				transportEquipmentUserControl.Visible = false;
				TransportEquipmentTabPage.Controls.Add(transportEquipmentUserControl);
				transportEquipmentUserControl.Visible = true;
				transportEquipmentUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		void SetTransportEquipmentTabVisibility()
		{
			if (TransportEquipmentTabPage != null)
			{
				TransportEquipmentTabPage.TabRelevant = JobDeclaration is JobDeclaration declaration && declaration.EquipmentsRequired;
			}
		}

		void SetInvoiceGroupingTabVisibility()
		{
			InvoiceGroupingTabPage.TabRelevant = JobDeclaration is not null && JobDeclaration.IsImport;
		}

		bool IsEquipmentSupported => GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Germany;

		public BaseDeclarationTabPage TransportEquipmentTabPage => transportEquipmentTabPage;
		BaseDeclarationTabPage transportEquipmentTabPage;

		TransportEquipmentUserControl transportEquipmentUserControl;

		#endregion
	}
}
