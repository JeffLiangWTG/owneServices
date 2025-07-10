namespace Enterprise.Registry.GUI
{
	partial class HBLDeliveryModeControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.HBLDeliveryModeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DefaultHBLDeliveryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HBLDeliveryModeGrid)).BeginInit();
			this.HBLDeliveryModeGrid.SuspendLayout();
			this.DefaultHBLDeliveryModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.HBLDeliveryModes);
			// 
			// HBLDeliveryModeGrid
			// 
			this.HBLDeliveryModeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HBLDeliveryModeGrid, "Modes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryModes)(null)).Modes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HBLDeliveryMode)(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryModes)(null)).Modes)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HBLDeliveryMode)(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryModes)(null)).Modes)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HBLDeliveryMode)(((System.Collections.IList)(((Enterprise.Registry.Business.HBLDeliveryModes)(null)).Modes)).SyncRoot)).ShowInList)));
			this.HBLDeliveryModeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b1a49418-69b4-4a36-a037-0527f75ddacc", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bdd73549-545a-4872-b37f-de9050068141", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("01d440a9-76f6-499e-a8ea-b3d64138112e", "Show in List");
			zCheckBoxColumnStyleInfo1.ColumnName = "ShowInList";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HBLDeliveryModeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HBLDeliveryModeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HBLDeliveryModeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.HBLDeliveryModeGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.HBLDeliveryModeGrid.GridId = "d9d9752c-161a-4406-810d-f64ca5a74704";
			this.HBLDeliveryModeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HBLDeliveryModeGrid.LayoutKey = "HBLDeliveryModeGrid";
			this.HBLDeliveryModeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
			this.HBLDeliveryModeGrid.Name = "HBLDeliveryModeGrid";
			this.HBLDeliveryModeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 358, true);
			this.HBLDeliveryModeGrid.TabIndex = 1;
			// 
			// DefaultHBLDeliveryModeDropEdit
			// 
			this.DefaultHBLDeliveryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultHBLDeliveryModeDropEdit, "DefaultHBLDeliveryMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.HBLDeliveryModes)(null)).DefaultHBLDeliveryMode)));
			this.DefaultHBLDeliveryModeDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("46ecd422-b52b-46fe-a298-f113f63afce5", "Default");
			this.DefaultHBLDeliveryModeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DefaultHBLDeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 3, true);
			this.DefaultHBLDeliveryModeDropEdit.Name = "DefaultHBLDeliveryModeDropEdit";
			this.DefaultHBLDeliveryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 20, true);
			this.DefaultHBLDeliveryModeDropEdit.TabIndex = 0;
			// 
			// HBLDeliveryModeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultHBLDeliveryModeDropEdit);
			this.Controls.Add(this.HBLDeliveryModeGrid);
			this.Name = "HBLDeliveryModeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HBLDeliveryModeGrid)).EndInit();
			this.HBLDeliveryModeGrid.ResumeLayout(false);
			this.HBLDeliveryModeGrid.PerformLayout();
			this.DefaultHBLDeliveryModeDropEdit.ResumeLayout(true);
			this.DefaultHBLDeliveryModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid HBLDeliveryModeGrid;
		private ZArchitecture.GUI.ZDropEdit DefaultHBLDeliveryModeDropEdit;
	}
}
