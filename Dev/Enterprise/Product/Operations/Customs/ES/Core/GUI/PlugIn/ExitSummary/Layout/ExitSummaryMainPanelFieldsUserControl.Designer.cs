namespace Enterprise.Customs.ES.GUI
{
	partial class ExitSummaryMainPanelFieldsUserControl
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
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclEmailAddrTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrokerCodeFindBox.SuspendLayout();
			this.CertificateDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "CEH_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader)(null)).CEH_GS_NKCustomsAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader)(null)).Lookups.CustomsAgents)));
			this.BrokerCodeFindBox.BindToList = "Lookups+CustomsAgents";
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 8, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.BrokerCodeFindBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.BrokerCodeFindBox.TabIndex = 3;
			// 
			// CertificateDropEdit
			// 
			this.CertificateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateDropEdit, "CEH_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader)(null)).CEH_CustomsProfile)));
			this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 34, true);
			this.CertificateDropEdit.Name = "CertificateDropEdit";
			this.CertificateDropEdit.PreBoundMaxLength = 27;
			this.CertificateDropEdit.ShowDescriptionBox = false;
			this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.CertificateDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.CertificateDropEdit.TabIndex = 4;
			// 
			// DeclEmailAddrTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclEmailAddrTextBox, "DeclEmailAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusExitControlHeader)(null)).DeclEmailAddr)));
			this.DeclEmailAddrTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("2C5B5EFC-3AE1-41D7-9513-AD1D8C5DD7DA", "Decl. Email");
			this.DeclEmailAddrTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 60, true);
			this.DeclEmailAddrTextBox.Name = "DeclEmailAddrTextBox";
			this.DeclEmailAddrTextBox.ReadOnly = true;
			this.DeclEmailAddrTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.DeclEmailAddrTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.DeclEmailAddrTextBox.TabIndex = 5;
			// 
			// ExitSummaryMainPanelFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.CertificateDropEdit);
			this.Controls.Add(this.DeclEmailAddrTextBox);
			this.Name = "ExitSummaryMainPanelFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 168, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.CertificateDropEdit.ResumeLayout(true);
			this.CertificateDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox DeclEmailAddrTextBox;
	}
}
