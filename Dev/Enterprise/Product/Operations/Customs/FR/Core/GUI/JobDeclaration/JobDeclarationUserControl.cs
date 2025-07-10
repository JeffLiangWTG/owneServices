using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class JobDeclarationUserControl : EUJobDeclarationUserControl
	{
		public override BaseJobDeclaration JobDeclaration
		{
			get => base.JobDeclaration;
			set
			{
				UnhookJobDeclarationEvents(FRDeclaration);
				base.JobDeclaration = value;
				HookJobDeclarationEvents(FRDeclaration);
			}
		}
		public JobDeclarationUserControl()
		{
			this.InitializeComponent();
			SetPositionOfLocationCodeDropEdit();
		}

		void LoadZDropEditDeltaMode()
		{
			if (this.DeltaModeDropEdit.List == null)
			{
				return;
			}
			else
			{
				if (this.DeltaModeDropEdit.List.Count > 1)
				{
					this.DeltaModeDropEdit.Text = "";
					FRDeclaration.JE_DeltaMode = "";
				}
				if (this.DeltaModeDropEdit.List.Count == 1)
				{
					this.DeltaModeDropEdit.Text = this.DeltaModeDropEdit.List[0].ToString();
					FRDeclaration.JE_DeltaMode = this.DeltaModeDropEdit.Text;
				}
			}
		}

		void SetPositionOfLocationCodeDropEdit()
		{
			this.LocationCodeDropEditWithFixedWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(GoodsLocationDropEdit.Location.X + GoodsLocationDropEdit.Width * 2,
																																		GoodsLocationDropEdit.Location.Y,
																																		false);
		}
		void HookJobDeclarationEvents(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_OA_DeclarantAddressInfo.ValueChanged += OnUpdateAunthorizationOwnerChanged;
				declaration.JE_OH_ImporterInfo.ValueChanged += OnUpdateAunthorizationOwnerChanged;
				declaration.JE_OH_SupplierInfo.ValueChanged += OnUpdateAunthorizationOwnerChanged;
				declaration.JE_MessageTypeInfo.ValueChanged += OnUpdateAunthorizationOwnerChanged;
				declaration.JE_DeclarantTypeInfo.ValueChanged += OnUpdateAunthorizationOwnerChanged;
			}
		}
		void UnhookJobDeclarationEvents(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_OA_DeclarantAddressInfo.ValueChanged -= OnUpdateAunthorizationOwnerChanged;
				declaration.JE_OH_ImporterInfo.ValueChanged -= OnUpdateAunthorizationOwnerChanged;
				declaration.JE_OH_SupplierInfo.ValueChanged -= OnUpdateAunthorizationOwnerChanged;
				declaration.JE_MessageTypeInfo.ValueChanged -= OnUpdateAunthorizationOwnerChanged;
				declaration.JE_DeclarantTypeInfo.ValueChanged -= OnUpdateAunthorizationOwnerChanged;
			}
		}
		void OnUpdateAunthorizationOwnerChanged(object sender, EventArgs e)
		{
			LoadZDropEditDeltaMode();
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			AirRouteTypeDropEdit.Visible = (JobDeclaration.IsAir && JobDeclaration.IsImport);
			GoodsLocationDropEdit.Visible = false;
			BadgeCodeDropEdit.Visible = true;
			BadgeCodeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			if (JobDeclaration.IsExport)
			{
				ExportExitTypeDropEdit.Visible = true;
				ExportExitReasonTextBox.Visible = true;
				JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 212, true);
			}
			else
			{
				ExportExitTypeDropEdit.Visible = false;
				ExportExitReasonTextBox.Visible = false;
				JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 170, true);
			}
			var isUCC6 = FRDeclaration.IsUCC6;
			DeltaModeDropEdit.Visible = !isUCC6 && !FRDeclaration.ShowSubmitMenuItem;
			JE_DeclarationLanguageDropEdit.Visible = isUCC6;
			ZG_AgreedPlaceCodeDropEdit.Visible = !isUCC6;
			ZG_AgreedPlaceCodeFindBox.Visible = isUCC6;
		}

		protected override Type GetOrganizationImportUserControlType() => typeof(ImportOrganizationUserControl);

		protected override ZDocAddressControl GetImporterDocAddressControl() => new FRDocAddressControl();

		protected override ZDocAddressControl GetSupplierDocAddressControl() => new FRDocAddressControl();

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				UnhookJobDeclarationEvents(FRDeclaration);
			}
			base.Dispose(disposing);
		}

		protected override void SetRightTabControlSelectTab()
		{
			if (RightTabControl != null)
			{
				RightTabControl.SelectedTab = OrganisationsTabPage;
			}
		}
	}
}
