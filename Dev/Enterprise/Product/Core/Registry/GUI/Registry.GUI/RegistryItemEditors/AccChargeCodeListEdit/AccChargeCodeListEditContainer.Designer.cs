using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AccChargeCodeListEditContainer : ZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid AccChargeCodeGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AccChargeCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AccChargeCodeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.AccChargeCodeListCollectionWrapper);
			// 
			// AccChargeCodeGrid
			// 
			this.AccChargeCodeGrid.AllowNavigation = false;
			this.AccChargeCodeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AccChargeCodeGrid, "AccChargeCodeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.AccChargeCodeListCollectionWrapper)(null)).AccChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.GUI.AccChargeCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.AccChargeCodeListCollectionWrapper)(null)).AccChargeCodeList)).SyncRoot)).ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.AccChargeCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.AccChargeCodeListCollectionWrapper)(null)).AccChargeCodeList)).SyncRoot)).AccChargeCodeCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.AccChargeCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.AccChargeCodeListCollectionWrapper)(null)).AccChargeCodeList)).SyncRoot)).ChargeDescription)));
			this.AccChargeCodeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "AccChargeCodeCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AccChargeCodeListEditContainer|efd77623-572e-428d-a1ef-1f081021bc88", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCode";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AccChargeCodeListEditContainer|b9065dfc-ada2-4880-b57b-e646daf5f064", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.AccChargeCodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AccChargeCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AccChargeCodeGrid.GridId = "966fd065-907f-495b-b15e-8c3f57131592";
			this.AccChargeCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccChargeCodeGrid.LayoutKey = "zGrid1";
			this.AccChargeCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccChargeCodeGrid.Name = "AccChargeCodeGrid";
			this.AccChargeCodeGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.AccChargeCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 144, true);
			this.AccChargeCodeGrid.TabIndex = 0;
			// 
			// AccChargeCodeListEditContainer
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccChargeCodeGrid);
			this.Name = "AccChargeCodeListEditContainer";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AccChargeCodeGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
