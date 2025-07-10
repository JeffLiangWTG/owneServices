using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GoodsItemsTabUserControl : ZUserControl
	{
		public Phase5GoodsItemsTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var layoutProvider = LayoutProvider;

			SetupGoodsItemAdditionalDocumentsTabUserControl();

			GoodsItemTabControl.AddAdditionalTabs(BindingSource, layoutProvider.AdditionalGoodsItemTabPages);
			GoodsItemTabControl.ReorderTabs(layoutProvider.ReorderGoodsItemTabPageNames);

			var goodsItemDetailsPanelLayoutWithGrid = layoutProvider.GoodsItemDetailsPanelLayoutWithGrid;
			DynamicGoodsItemDetailsPanel.UpdateLayout(goodsItemDetailsPanelLayoutWithGrid);
			SetupGoodsItemsGrid(goodsItemDetailsPanelLayoutWithGrid);

			InitializeUNDGDataItemFormManager();
			UpdateGoodsItemPackagesAndContainersDynamicCreationUserControlLayout();
		}

		void GoodsItemPackagesAndContainersDynamicCreationUserControl_HostedControlCreated(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				UpdateGoodsItemPackagesAndContainersDynamicCreationUserControlLayout();
			}
		}

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;

		void SetupGoodsItemsGrid(IPanelLayoutWithGridProvider panelLayoutWithGridProvider)
		{
			var userControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGridProvider.GridUserControlType);
			GridContainer.Controls.Add(userControl);
			BindingSource.SetBindingMember(userControl, ".");
			userControl.Dock = DockStyle.Fill;
		}

		void SetupGoodsItemAdditionalDocumentsTabUserControl()
		{
			var userControl = (ZUserControl)Activator.CreateInstance(LayoutProvider.GoodsItemAdditionalDocumentsTabUserControlType);
			GoodsItemAdditionalDocumentsTabPage.Controls.Add(userControl);
			BindingSource.SetBindingMember(userControl, "AdditionalInfos");
			userControl.Dock = DockStyle.Fill;
			userControl.AllowDrop = true;
			userControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			userControl.Name = "GoodsItemAdditionalDocumentsTabUserControl";
			userControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			userControl.TabIndex = 0;
		}

		void InitializeUNDGDataItemFormManager()
		{
			var undgUserControl = (UNDangerousGoodsUserControl)DynamicGoodsItemDetailsPanel.Controls[nameof(GoodsItemDetailsUserControl.UNDangerousGoodsUserControl)];
			if (undgUserControl != null)
			{
				var grid = GridContainer.FindSingle<ZGrid>();
				new UNDGDataItemFormManager(grid).Initialize(undgUserControl.UNDangerousGoodsButton);
			}
		}

		void UpdateGoodsItemPackagesAndContainersDynamicCreationUserControlLayout()
		{
			if (GoodsItemPackagesAndContainersDynamicCreationUserControl.HostedControl is DynamicLayoutPanel dynamicLayoutPanel)
			{
				dynamicLayoutPanel.UpdateLayout(LayoutProvider.GoodsItemPackagesAndContainersPanelLayout);
				var column = dynamicLayoutPanel.FindSingle<ZPanel>("ColumnSeparator0");
				dynamicLayoutPanel.FindSingle<ZDynamicControlCreationUserControl>("DynamicPackagesUserControl").AllowOverlap(column);
			}
		}

		Control GridContainer => GoodsItemsSplitContainer.Panel1;
	}
}
