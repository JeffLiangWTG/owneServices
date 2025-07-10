namespace Enterprise.Customs.EU.GUI
{
	partial class ShipmentDetailsUserControl
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
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentDetailsIncoTermsPlaceUserControl = new Enterprise.Customs.EU.GUI.ShipmentDetailsIncoTermsPlaceUserControl();
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl = new Enterprise.Customs.EU.GUI.ShipmentDetailsUnlocoIncoTermsPlaceUserControl();
			this.ShipmentDetailsQuantitiesUserControl = new Enterprise.Customs.EU.GUI.ShipmentDetailsQuantitiesUserControl();
			this.ShipmentDetailsCountUserControl = new Enterprise.Customs.EU.GUI.ShipmentDetailsCountUserControl();
			this.AgreedPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipmentIncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegionOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.ShipmentDetailsIncoTermsPlaceUserControl.SuspendLayout();
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.SuspendLayout();
			this.ShipmentDetailsQuantitiesUserControl.SuspendLayout();
			this.ShipmentDetailsCountUserControl.SuspendLayout();
			this.AgreedPlaceCodeFindBox.SuspendLayout();
			this.RegionOfDestinationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 3, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.GoodsLocationDropEdit.TabIndex = 10;
			// 
			// UCRTextBox
			// 
			this.UCRTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UCRTextBox, "JE_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_UCR)));
			this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 172, true);
			this.UCRTextBox.Name = "UCRTextBox";
			this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.UCRTextBox.TabIndex = 25;
			// 
			// AgentsReferenceTextBox
			// 
			this.AgentsReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AgentsReferenceTextBox, "JE_AgentsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_AgentsReference)));
			this.AgentsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 115, true);
			this.AgentsReferenceTextBox.Name = "AgentsReferenceTextBox";
			this.AgentsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.AgentsReferenceTextBox.TabIndex = 24;
			// 
			// ShipmentDetailsIncoTermsPlaceUserControl
			// 
			this.ShipmentDetailsIncoTermsPlaceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsIncoTermsPlaceUserControl, ".");
			this.ShipmentDetailsIncoTermsPlaceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 89, true);
			this.ShipmentDetailsIncoTermsPlaceUserControl.Name = "ShipmentDetailsIncoTermsPlaceUserControl";
			this.ShipmentDetailsIncoTermsPlaceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			this.ShipmentDetailsIncoTermsPlaceUserControl.TabIndex = 26;
			// 
			// ShipmentDetailsUnlocoIncoTermsPlaceUserControl
			// 
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl, ".");
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 141, true);
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.Name = "ShipmentDetailsUnlocoIncoTermsPlaceUserControl";
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.TabIndex = 26;
			// 
			// ShipmentDetailsQuantitiesUserControl
			// 
			this.ShipmentDetailsQuantitiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsQuantitiesUserControl, ".");
			this.ShipmentDetailsQuantitiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 29, true);
			this.ShipmentDetailsQuantitiesUserControl.Name = "ShipmentDetailsQuantitiesUserControl";
			this.ShipmentDetailsQuantitiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsQuantitiesUserControl.TabIndex = 27;
			// 
			// ShipmentDetailsCountUserControl
			// 
			this.ShipmentDetailsCountUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsCountUserControl, ".");
			this.ShipmentDetailsCountUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 58, true);
			this.ShipmentDetailsCountUserControl.Name = "ShipmentDetailsCountUserControl";
			this.ShipmentDetailsCountUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.ShipmentDetailsCountUserControl.TabIndex = 28;
			// 
			// AgreedPlaceCodeFindBox
			// 
			this.AgreedPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeFindBox, "ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_AgreedPlaceCode)));
			this.AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 198, true);
			this.AgreedPlaceCodeFindBox.Name = "AgreedPlaceCodeFindBox";
			this.AgreedPlaceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AgreedPlaceCodeFindBox.ParentType = null;
			this.AgreedPlaceCodeFindBox.PreBoundMaxLength = 1;
			this.AgreedPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.AgreedPlaceCodeFindBox.TabIndex = 29;
			// 
			// ShipmentIncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentIncoTermPlaceTextBox, "JE_ShipmentIncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_ShipmentIncoTermPlace)));
			this.ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 224, true);
			this.ShipmentIncoTermPlaceTextBox.Name = "ShipmentIncoTermPlaceTextBox";
			this.ShipmentIncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.ShipmentIncoTermPlaceTextBox.TabIndex = 30;
			// 
			// RegionOfDestinationDropEdit
			// 
			this.RegionOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegionOfDestinationDropEdit, "ZG_RegionOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_RegionOfDestination)));
			this.RegionOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 251, true);
			this.RegionOfDestinationDropEdit.Name = "RegionOfDestinationDropEdit";
			this.RegionOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
			this.RegionOfDestinationDropEdit.TabIndex = 31;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RegionOfDestinationDropEdit);
			this.Controls.Add(this.ShipmentIncoTermPlaceTextBox);
			this.Controls.Add(this.AgreedPlaceCodeFindBox);
			this.Controls.Add(this.ShipmentDetailsCountUserControl);
			this.Controls.Add(this.ShipmentDetailsQuantitiesUserControl);
			this.Controls.Add(this.ShipmentDetailsIncoTermsPlaceUserControl);
			this.Controls.Add(this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl);
			this.Controls.Add(this.UCRTextBox);
			this.Controls.Add(this.AgentsReferenceTextBox);
			this.Controls.Add(this.GoodsLocationDropEdit);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 275, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.ShipmentDetailsIncoTermsPlaceUserControl.ResumeLayout(true);
			this.ShipmentDetailsIncoTermsPlaceUserControl.PerformLayout();
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.ResumeLayout(true);
			this.ShipmentDetailsUnlocoIncoTermsPlaceUserControl.PerformLayout();
			this.ShipmentDetailsQuantitiesUserControl.ResumeLayout(true);
			this.ShipmentDetailsQuantitiesUserControl.PerformLayout();
			this.ShipmentDetailsCountUserControl.ResumeLayout(true);
			this.ShipmentDetailsCountUserControl.PerformLayout();
			this.AgreedPlaceCodeFindBox.ResumeLayout(true);
			this.AgreedPlaceCodeFindBox.PerformLayout();
			this.RegionOfDestinationDropEdit.ResumeLayout(true);
			this.RegionOfDestinationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

        #endregion

        internal ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
        internal ZArchitecture.ZTextBox UCRTextBox;
        internal ZArchitecture.ZTextBox AgentsReferenceTextBox;
		internal ShipmentDetailsIncoTermsPlaceUserControl ShipmentDetailsIncoTermsPlaceUserControl;
		internal ShipmentDetailsUnlocoIncoTermsPlaceUserControl ShipmentDetailsUnlocoIncoTermsPlaceUserControl;
		internal ShipmentDetailsQuantitiesUserControl ShipmentDetailsQuantitiesUserControl;
		internal ShipmentDetailsCountUserControl ShipmentDetailsCountUserControl;
		internal ZArchitecture.GUI.ZCodeFindBox AgreedPlaceCodeFindBox;
		internal ZArchitecture.ZTextBox ShipmentIncoTermPlaceTextBox;
		internal ZArchitecture.GUI.ZDropEdit RegionOfDestinationDropEdit;
	}
}
