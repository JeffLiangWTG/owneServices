namespace Enterprise.Registry.GUI
{
	partial class CommunicationStatusRegistryControl
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
			this.CodeDescriptionBoolGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).BeginInit();
			this.CodeDescriptionBoolGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CommunicationStatusCollection);
			// 
			// CodeDescriptionBoolGrid
			// 
			this.CodeDescriptionBoolGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CodeDescriptionBoolGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CommunicationStatus)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CommunicationStatus)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CommunicationStatus)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CommunicationStatus)(null)).Closed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CommunicationStatus)(null)).Bool)));
			this.CodeDescriptionBoolGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("37e1c672-85ed-46bf-a04a-d32b5b1f7dff", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("949cfbaa-9a64-44f4-9323-a0ac31bc7f01", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c1f20c7a-f06b-4a92-b783-7cf91abc51a1", "Closed");
			zCheckBoxColumnStyleInfo1.ColumnName = "Closed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e5b5f890-c657-41e6-bb47-ffdd0a2166e4", "Bool.");
			zCheckBoxColumnStyleInfo2.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CodeDescriptionBoolGrid.CopySelectedRowsAllowed = true;
			this.CodeDescriptionBoolGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeDescriptionBoolGrid.GridId = "0b4d4f98-c8f0-4e4b-8eb1-95a635de7dc2";
			this.CodeDescriptionBoolGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionBoolGrid.LayoutKey = "CodeDescriptionBoolGrid";
			this.CodeDescriptionBoolGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionBoolGrid.Name = "CodeDescriptionBoolGrid";
			this.CodeDescriptionBoolGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 183, true);
			this.CodeDescriptionBoolGrid.TabIndex = 0;
			// 
			// CommunicationStatusRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionBoolGrid);
			this.Name = "CommunicationStatusRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 183, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).EndInit();
			this.CodeDescriptionBoolGrid.ResumeLayout(false);
			this.CodeDescriptionBoolGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid CodeDescriptionBoolGrid;
	}
}
