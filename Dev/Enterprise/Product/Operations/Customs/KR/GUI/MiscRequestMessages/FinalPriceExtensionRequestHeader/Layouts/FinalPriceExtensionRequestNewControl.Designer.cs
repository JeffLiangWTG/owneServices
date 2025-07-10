namespace Enterprise.Customs.KR.GUI
{
	partial class FinalPriceExtensionRequestNewControl
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
            this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BranchGuidFindBox.SuspendLayout();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.MessageTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader);
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 22, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.MessageTypeDropEdit.TabIndex = 0;
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 44, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 1;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 65, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchGuidFindBox.ParentType = null;
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.BranchGuidFindBox.TabIndex = 2;
			// 
			// FinalPriceExtensionRequestNewControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.BranchGuidFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Controls.Add(this.MessageTypeDropEdit);
            this.Name = "FinalPriceExtensionRequestNewControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 126, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BranchGuidFindBox.ResumeLayout(true);
            this.BranchGuidFindBox.PerformLayout();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.MessageTypeDropEdit.ResumeLayout(true);
            this.MessageTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		public ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
	}
}
