
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	partial class ImportSiscomexOfficesUserControl
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
			this.ClearanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WarehouseAreaIDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.WarehouseAreaIDConcatenatedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SectorCustomOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearanceEnclosureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearanceOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntranceLocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntranceOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClearanceGroupBox.SuspendLayout();
			this.SectorCustomOfficeDropEdit.SuspendLayout();
			this.ClearanceEnclosureDropEdit.SuspendLayout();
			this.ClearanceOfficeDropEdit.SuspendLayout();
			this.EntranceLocationGroupBox.SuspendLayout();
			this.EntranceOfficeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ClearanceGroupBox
			// 
			this.ClearanceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ClearanceGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("d9836381-f0b8-43dc-a5ad-724f788a5382", "Clearance Local");
			this.ClearanceGroupBox.Controls.Add(this.WarehouseAreaIDButton);
			this.ClearanceGroupBox.Controls.Add(this.WarehouseAreaIDConcatenatedTextBox);
			this.ClearanceGroupBox.Controls.Add(this.SectorCustomOfficeDropEdit);
			this.ClearanceGroupBox.Controls.Add(this.ClearanceEnclosureDropEdit);
			this.ClearanceGroupBox.Controls.Add(this.ClearanceOfficeDropEdit);
			this.ClearanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ClearanceGroupBox.Name = "ClearanceGroupBox";
			this.ClearanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 120, true);
			this.ClearanceGroupBox.TabIndex = 0;
			this.ClearanceGroupBox.TabStop = false;
			// 
			// WarehouseAreaIDButton
			// 
			this.WarehouseAreaIDButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WarehouseAreaIDButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("276C808A-79AD-4E21-AFAA-72F495B5102D", "More..");
			this.WarehouseAreaIDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 95, true);
			this.WarehouseAreaIDButton.Name = "WarehouseAreaIDButton";
			this.WarehouseAreaIDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.WarehouseAreaIDButton.TabIndex = 5;
			this.WarehouseAreaIDButton.ToolTipCaption = null;
			this.WarehouseAreaIDButton.UseVisualStyleBackColor = true;
			this.WarehouseAreaIDButton.Click += new System.EventHandler(this.WarehouseAreaIDButton_Click);
			// 
			// WarehouseAreaIDConcatenatedTextBox
			// 
			this.WarehouseAreaIDConcatenatedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WarehouseAreaIDConcatenatedTextBox, "WarehouseAreasConcatenated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).WarehouseAreasConcatenated)));
			this.WarehouseAreaIDConcatenatedTextBox.CaptionResourceString = null;
			this.WarehouseAreaIDConcatenatedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 95, true);
			this.WarehouseAreaIDConcatenatedTextBox.Name = "WarehouseAreaIDConcatenatedTextBox";
			this.WarehouseAreaIDConcatenatedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.WarehouseAreaIDConcatenatedTextBox.TabIndex = 4;
			// 
			// SectorCustomOfficeDropEdit
			// 
			this.SectorCustomOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SectorCustomOfficeDropEdit, "JE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_SubLocationOfGoods)));
			this.SectorCustomOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 70, true);
			this.SectorCustomOfficeDropEdit.Name = "SectorCustomOfficeDropEdit";
			this.SectorCustomOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.SectorCustomOfficeDropEdit.TabIndex = 3;
			// 
			// ClearanceEnclosureDropEdit
			// 
			this.ClearanceEnclosureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClearanceEnclosureDropEdit, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.ClearanceEnclosureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 46, true);
			this.ClearanceEnclosureDropEdit.Name = "ClearanceEnclosureDropEdit";
			this.ClearanceEnclosureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.ClearanceEnclosureDropEdit.TabIndex = 2;
			// 
			// ClearanceOfficeDropEdit
			// 
			this.ClearanceOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClearanceOfficeDropEdit, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.ClearanceOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 22, true);
			this.ClearanceOfficeDropEdit.Name = "ClearanceOfficeDropEdit";
			this.ClearanceOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.ClearanceOfficeDropEdit.TabIndex = 1;
			// 
			// EntranceLocationGroupBox
			// 
			this.EntranceLocationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EntranceLocationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("bce51ec3-88a2-4b72-a121-097ce3bc8d26", "Entrance Location");
			this.EntranceLocationGroupBox.Controls.Add(this.EntranceOfficeDropEdit);
			this.EntranceLocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 128, true);
			this.EntranceLocationGroupBox.Name = "EntranceLocationGroupBox";
			this.EntranceLocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 53, true);
			this.EntranceLocationGroupBox.TabIndex = 1;
			this.EntranceLocationGroupBox.TabStop = false;
			// 
			// EntranceOfficeDropEdit
			// 
			this.EntranceOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntranceOfficeDropEdit, "EntranceOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).EntranceOfficeCode)));
			this.EntranceOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 22, true);
			this.EntranceOfficeDropEdit.Name = "EntranceOfficeDropEdit";
			this.EntranceOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.EntranceOfficeDropEdit.TabIndex = 0;
			// 
			// ImportSiscomexOfficesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntranceLocationGroupBox);
			this.Controls.Add(this.ClearanceGroupBox);
			this.Name = "ImportSiscomexOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 186, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClearanceGroupBox.ResumeLayout(false);
			this.ClearanceGroupBox.PerformLayout();
			this.SectorCustomOfficeDropEdit.ResumeLayout(true);
			this.SectorCustomOfficeDropEdit.PerformLayout();
			this.ClearanceEnclosureDropEdit.ResumeLayout(true);
			this.ClearanceEnclosureDropEdit.PerformLayout();
			this.ClearanceOfficeDropEdit.ResumeLayout(true);
			this.ClearanceOfficeDropEdit.PerformLayout();
			this.EntranceLocationGroupBox.ResumeLayout(false);
			this.EntranceLocationGroupBox.PerformLayout();
			this.EntranceOfficeDropEdit.ResumeLayout(true);
			this.EntranceOfficeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ClearanceGroupBox;
		internal ZArchitecture.GUI.ZDropEdit ClearanceOfficeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ClearanceEnclosureDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SectorCustomOfficeDropEdit;
		internal ZArchitecture.GUI.ZGroupBox EntranceLocationGroupBox;
		internal ZArchitecture.GUI.ZDropEdit EntranceOfficeDropEdit;
		internal ZArchitecture.GUI.ZButton WarehouseAreaIDButton;
		internal ZArchitecture.ZTextBox WarehouseAreaIDConcatenatedTextBox;
	}
}
