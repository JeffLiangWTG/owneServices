using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class LastDbRestoreUserControl
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
		void InitializeComponent()
		{
			this.operationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.toolVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dbSchemaVersionBeforeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dbSchemaVersionAfterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.completionDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.LastDbRestoreInfo);
			// 
			// operationTextBox
			// 
			this.BindingSource.SetBindingMember(this.operationTextBox, "Operation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LastDbRestoreInfo)(null)).Operation)));
			this.operationTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("358d6280-b8e4-453a-a65d-53f57045e7ad", "Operation");
			this.operationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.operationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 8, true);
			this.operationTextBox.Name = "operationTextBox";
			this.operationTextBox.ReadOnly = true;
			this.operationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.operationTextBox.TabIndex = 0;
			// 
			// toolVersionTextBox
			// 
			this.BindingSource.SetBindingMember(this.toolVersionTextBox, "ToolVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LastDbRestoreInfo)(null)).ToolVersion)));
			this.toolVersionTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("21b122f2-48fc-44a7-a470-2644a89d072e", "Tool Version");
			this.toolVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 35, true);
			this.toolVersionTextBox.Name = "toolVersionTextBox";
			this.toolVersionTextBox.ReadOnly = true;
			this.toolVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.toolVersionTextBox.TabIndex = 1;
			// 
			// dbSchemaVersionBeforeTextBox
			// 
			this.BindingSource.SetBindingMember(this.dbSchemaVersionBeforeTextBox, "DbSchemaVersionBefore");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LastDbRestoreInfo)(null)).DbSchemaVersionBefore)));
			this.dbSchemaVersionBeforeTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5458e9e7-b0db-4aaf-93d5-843dac6ae196", "Before");
			this.dbSchemaVersionBeforeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 105, true);
			this.dbSchemaVersionBeforeTextBox.Name = "dbSchemaVersionBeforeTextBox";
			this.dbSchemaVersionBeforeTextBox.ReadOnly = true;
			this.dbSchemaVersionBeforeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.dbSchemaVersionBeforeTextBox.TabIndex = 4;
			// 
			// dbSchemaVersionAfterTextBox
			// 
			this.BindingSource.SetBindingMember(this.dbSchemaVersionAfterTextBox, "DbSchemaVersionAfter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.LastDbRestoreInfo)(null)).DbSchemaVersionAfter)));
			this.dbSchemaVersionAfterTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("254a8f1d-7bc7-4642-bf6b-4785ceea5842", "After");
			this.dbSchemaVersionAfterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 131, true);
			this.dbSchemaVersionAfterTextBox.Name = "dbSchemaVersionAfterTextBox";
			this.dbSchemaVersionAfterTextBox.ReadOnly = true;
			this.dbSchemaVersionAfterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.dbSchemaVersionAfterTextBox.TabIndex = 5;
			// 
			// completionDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.completionDateTextBox, "CompletionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Registry.Business.LastDbRestoreInfo)(null)).CompletionDate)));
			this.completionDateTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("801dd636-a616-4568-8c25-2310fde8c37a", "Completion Date");
			this.completionDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 59, true);
			this.completionDateTextBox.Name = "completionDateTextBox";
			this.completionDateTextBox.ReadOnly = true;
			this.completionDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.completionDateTextBox.TabIndex = 2;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 82, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "Database Schema Version";
			// 
			// LastDbRestoreUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.completionDateTextBox);
			this.Controls.Add(this.dbSchemaVersionAfterTextBox);
			this.Controls.Add(this.dbSchemaVersionBeforeTextBox);
			this.Controls.Add(this.toolVersionTextBox);
			this.Controls.Add(this.operationTextBox);
			this.Name = "LastDbRestoreUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox operationTextBox;
		private ZArchitecture.ZTextBox toolVersionTextBox;
		private ZArchitecture.ZTextBox dbSchemaVersionBeforeTextBox;
		private ZArchitecture.ZTextBox dbSchemaVersionAfterTextBox;
		private ZArchitecture.ZTextBox completionDateTextBox;
		private ZArchitecture.ZLabel zLabel1;

	}
}
