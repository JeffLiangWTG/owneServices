namespace Enterprise.Customs.EU.GUI
{
	partial class PrimaryOwnerOfGoodsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PrimaryOwnerOfGoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OwnerOfGoodsOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrimaryOwnerOfGoodsGroupBox.SuspendLayout();
			this.OwnerOfGoodsOrganisationFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// PrimaryOwnerOfGoodsGroupBox
			//
			this.PrimaryOwnerOfGoodsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CDB82074-2BB8-4719-BEB9-AEB67714524A", "Primary Owner of Goods", "[Annex A 3/8] Parties > Owner of the Goods");
			this.PrimaryOwnerOfGoodsGroupBox.Controls.Add(this.OwnerOfGoodsOrganisationFindBox);
			this.PrimaryOwnerOfGoodsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrimaryOwnerOfGoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrimaryOwnerOfGoodsGroupBox.Name = "PrimaryOwnerOfGoodsGroupBox";
			this.PrimaryOwnerOfGoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 41, true);
			this.PrimaryOwnerOfGoodsGroupBox.TabIndex = 0;
			this.PrimaryOwnerOfGoodsGroupBox.TabStop = false;
			// 
			// OwnerOfGoodsOrganisationFindBox
			// 
			this.OwnerOfGoodsOrganisationFindBox.AllowDrop = true;
			this.OwnerOfGoodsOrganisationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OwnerOfGoodsOrganisationFindBox, "CustomsEntryInstructions.CEI_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Owner)));
			this.OwnerOfGoodsOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.OwnerOfGoodsOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OwnerOfGoodsOrganisationFindBox.Name = "OwnerOfGoodsOrganisationFindBox";
			this.OwnerOfGoodsOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OwnerOfGoodsOrganisationFindBox.ParentType = null;
			this.OwnerOfGoodsOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 20, true);
			this.OwnerOfGoodsOrganisationFindBox.TabIndex = 0;
			// 
			// PrimaryOwnerOfGoodsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrimaryOwnerOfGoodsGroupBox);
			this.Name = "PrimaryOwnerOfGoodsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 41, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrimaryOwnerOfGoodsGroupBox.ResumeLayout(false);
			this.PrimaryOwnerOfGoodsGroupBox.PerformLayout();
			this.OwnerOfGoodsOrganisationFindBox.ResumeLayout(true);
			this.OwnerOfGoodsOrganisationFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ZArchitecture.GUI.ZGroupBox PrimaryOwnerOfGoodsGroupBox;
		internal MasterFiles.GUI.ZOrganisationFindBox OwnerOfGoodsOrganisationFindBox;
	}
}
