using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class CustomsIncoTermOverrideControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IncoTermsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncoTermsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.CustomsIncoTermOverrideCollection);
			// 
			// IncoTermsGrid
			// 
			this.IncoTermsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IncoTermsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.CustomsIncoTermOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.CustomsIncoTermOverride)(null)).CustomsCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.CustomsIncoTermOverride)(null)).InternationalCode)));
			this.IncoTermsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("CustomsIncoTermOverrideControl|29e8ec8a-3973-403e-bba9-59a716a74c20", "Customs Code");
			zTextBoxColumnStyleInfo1.ColumnName = "CustomsCode";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("CustomsIncoTermOverrideControl|387c7e8d-b553-44d3-81d1-a47c772355d9", "International Code");
			zDropEditColumnStyleInfo1.ColumnName = "InternationalCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.IncoTermsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IncoTermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.IncoTermsGrid.GridId = "c4a8e740-0bbd-4296-875b-8f9241676c47";
			this.IncoTermsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncoTermsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncoTermsGrid.LayoutKey = "IncoTermsGrid";
			this.IncoTermsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncoTermsGrid.Name = "IncoTermsGrid";
			this.IncoTermsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 222, true);
			this.IncoTermsGrid.TabIndex = 0;
			// 
			// CustomsIncoTermOverrideControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermsGrid);
			this.Name = "CustomsIncoTermOverrideControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 222, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncoTermsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid IncoTermsGrid;

#if DEBUG
		internal Enterprise.ZArchitecture.ZGrid IncoTermsGridForTest => IncoTermsGrid;
#endif

	}
}
