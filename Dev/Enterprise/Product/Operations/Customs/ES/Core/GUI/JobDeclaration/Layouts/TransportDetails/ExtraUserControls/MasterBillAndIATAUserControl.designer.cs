namespace Enterprise.Customs.ES.GUI
{
	partial class MasterBillAndIATAUserControl
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
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.IATALoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MasterBillTextBox.SuspendLayout();
			this.IATALoadPortCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// MasterBillTextBox
			// 
			this.MasterBillTextBox.AllowAlphaInMAWP = false;
			this.MasterBillTextBox.AllowDrop = true;
			this.MasterBillTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_MasterBill)));
			this.MasterBillTextBox.FormattedMasterBill = "";
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.MasterBillTextBox.TabIndex = 0;
			// 
			// IATALoadPortCodeFindBox
			// 
			this.IATALoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IATALoadPortCodeFindBox, "JE_IATALoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_IATALoadPort)));
			this.IATALoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 0, true);
			this.IATALoadPortCodeFindBox.Name = "IATALoadPortCodeFindBox";
			this.IATALoadPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.IATALoadPortCodeFindBox.ParentType = null;
			this.IATALoadPortCodeFindBox.PreBoundMaxLength = 3;
			this.IATALoadPortCodeFindBox.ShowDescriptionBox = false;
			this.IATALoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.IATALoadPortCodeFindBox.TabIndex = 1;
			// 
			// MasterBillAndIATAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MasterBillTextBox);
			this.Controls.Add(this.IATALoadPortCodeFindBox);
			this.Name = "MasterBillAndIATAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MasterBillTextBox.ResumeLayout(true);
			this.MasterBillTextBox.PerformLayout();
			this.IATALoadPortCodeFindBox.ResumeLayout(true);
			this.IATALoadPortCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZMasterBillControl MasterBillTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox IATALoadPortCodeFindBox;
	}
}
