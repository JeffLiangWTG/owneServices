using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentSupplyChainActorsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
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
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OwnerOrgPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.SelectedSupplyChainActorsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SelectedSupplyChainActorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SelectedSupplyChainActorsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SelectedSupplyChainActorsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedSupplyChainActorsGrid.GridId = "2bb83ee1-481f-4090-8758-9e489006a701";
			this.SelectedSupplyChainActorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedSupplyChainActorsGrid.LayoutKey = "SelectedSupplyChainActorsGrid";
			this.SelectedSupplyChainActorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectedSupplyChainActorsGrid.Name = "SelectedSupplyChainActorsGrid";
			this.SelectedSupplyChainActorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 149, true);
			this.SelectedSupplyChainActorsGrid.TabIndex = 0;
			// 
			// HouseConsignmentSupplyChainActorsGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SelectedSupplyChainActorsGrid);
			this.Name = "HouseConsignmentSupplyChainActorsGridUserControl";
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
