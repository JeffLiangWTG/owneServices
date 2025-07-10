namespace Enterprise.Registry.GUI
{
	partial class CodeDescriptionWithEnabledAndDefaultsRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefaultCollection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefault)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefault)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefault)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefault)(null)).IsDefaultForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CodeDescriptionWithEnabledAndDefault)(null)).IsEnabled)));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("939b3885-fd8c-4b5f-8c0f-22f19c4eecaa", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6e1f6806-bdaf-4208-8fbc-9b5c6ce12b95", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a21440ec-3c17-46b0-baae-ac0fdb4a5883", "Default");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsDefaultForBinding";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8f75348e-7544-4cf0-ad24-d13bf555eca9", "Enabled");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsEnabled";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.Grid.CopySelectedRowsAllowed = true;
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "84f5b3e3-5d34-479e-8640-e19d13ab5d25";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "zGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Grid.TabIndex = 0;
			// 
			// CodeDescriptionWithEnabledAndDefaultsRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Grid);
			this.Name = "CodeDescriptionWithEnabledAndDefaultsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid Grid;
	}
}
