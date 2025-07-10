using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaPackedItemControl : ZUserControl, IAdditionalTabPage
	{
		public AsycudaPackedItemControl()
		{
			InitializeComponent();
			InitializePackedItemGridAndPackedItemDetailTabPage();
			InitializePackPackedItemPivotTabPage();
			InitializeAdditionalInformationTabPage();
		}

		void InitializePackedItemGridAndPackedItemDetailTabPage()
		{
			var panelLayoutWithGrid = new AsycudaPackedItemWithGridLayout();
			PackedItemDetailsLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			var packedItemGridControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			packedItemGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			BindingSource.SetBindingMember(packedItemGridControl, ".");
			this.TopPanel.Controls.Add(packedItemGridControl);
		}

		void InitializeAdditionalInformationTabPage()
		{
			var additionalInfoUserControl = new AsycudaAdditionalInfoUserControl();
			new ControlRebinder().Rebind(additionalInfoUserControl, ((ISupportingInfoUserControls)additionalInfoUserControl).GridBindingMember, "PackedItems.AdditionalInfos");
			var grid = ((ISupportingInfoUserControls)additionalInfoUserControl).Grid;
			if (grid != null)
			{
				grid.ColumnLayoutContext = SupportingInfoColumnLayoutContext;
			}
			AdditionalInformationLayoutPanel.Controls.Add(additionalInfoUserControl);
		}

		void InitializePackPackedItemPivotTabPage()
		{
			var packPackedItemPivotControl = new AsycudaPackedItemPackingPivotControl();
			PackPackedItemPivotTabPage.Controls.Add(packPackedItemPivotControl);
			BindingSource.SetBindingMember(packPackedItemPivotControl, ".");
			packPackedItemPivotControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("B81879DA-20A3-4E7D-B226-46C8F2149DA4", "Items");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		int IAdditionalTabPage.TabPageSequence => 1;

		const string SupportingInfoColumnLayoutContext = CusSupportingInfoTypeList.Codes.AdditionalInfo;
	}
}
