
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GoodsItemSupplyChainActorsGridUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.SelectedSupplyChainActorsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedSupplyChainActorsGrid)).BeginInit();
			this.SelectedSupplyChainActorsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference>);
			// 
			// SelectedSupplyChainActorsGrid
			// 
			this.SelectedSupplyChainActorsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SelectedSupplyChainActorsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(null)).CFR_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(null)).CFR_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(null)).OwnerOrgPK)));
			this.SelectedSupplyChainActorsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OwnerOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.SelectedSupplyChainActorsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SelectedSupplyChainActorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SelectedSupplyChainActorsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.SelectedSupplyChainActorsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedSupplyChainActorsGrid.GridId = "7DC4E1DC-7AC5-4E85-9153-F0D8462DE173";
			this.SelectedSupplyChainActorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedSupplyChainActorsGrid.LayoutKey = "SelectedSupplyChainActorsGrid";
			this.SelectedSupplyChainActorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectedSupplyChainActorsGrid.Name = "SelectedSupplyChainActorsGrid";
			this.SelectedSupplyChainActorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 149, true);
			this.SelectedSupplyChainActorsGrid.TabIndex = 2;
			// 
			// GoodsItemSupplyChainActorsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SelectedSupplyChainActorsGrid);
			this.Name = "GoodsItemSupplyChainActorsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 149, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedSupplyChainActorsGrid)).EndInit();
			this.SelectedSupplyChainActorsGrid.ResumeLayout(false);
			this.SelectedSupplyChainActorsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid SelectedSupplyChainActorsGrid;
	}
}
