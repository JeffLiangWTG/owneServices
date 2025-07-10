using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AWBDocumentPivotControl : ZUserControl
	{
		private Enterprise.ZArchitecture.ZGrid HAWBDocumentPivotGrid;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.HAWBDocumentPivotGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HAWBDocumentPivotGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.AWBDocumentPivotCollection);
			// 
			// HAWBDocumentPivotGrid
			// 
			this.HAWBDocumentPivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HAWBDocumentPivotGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.AWBDocumentPivot)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.AWBDocumentPivot)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.AWBDocumentPivot)(null)).Title)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.GUI.AWBDocumentPivot)(null)).Printed)));
			this.HAWBDocumentPivotGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("fd85e3f0-5ff5-4c7a-8db0-328e63ffc409", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = Res.GetString("cb69d60a-a464-4139-a92b-f616c91a11c5", "The name know in the document customization.");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("0afea50e-a5d0-4471-8613-f4132c62a437", "Title");
			zTextBoxColumnStyleInfo2.ColumnName = "Title";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.ToolTip = Res.GetString("4e7b7970-b256-49d6-8bc8-63ac26021cbb", "The title to display for this document.");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("72887796-f404-4e88-a24c-d9cfc06a077d", "Printed?");
			zCheckBoxColumnStyleInfo1.ColumnName = "Printed";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.ToolTip = Res.GetString("170a1af7-5527-49fe-b6fb-c1d4d535adef", "Should this document be printed?");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.HAWBDocumentPivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HAWBDocumentPivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HAWBDocumentPivotGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.HAWBDocumentPivotGrid.GridId = "9c83b46a-3196-49a6-8354-78b43ced818a";
			this.HAWBDocumentPivotGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HAWBDocumentPivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HAWBDocumentPivotGrid.LayoutKey = "HAWBDocumentPivotGrid";
			this.HAWBDocumentPivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HAWBDocumentPivotGrid.Name = "HAWBDocumentPivotGrid";
			this.HAWBDocumentPivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 288, true);
			this.HAWBDocumentPivotGrid.TabIndex = 0;
			// 
			// HAWBDocumentPivotControl
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.HAWBDocumentPivotGrid);
			this.Name = "HAWBDocumentPivotControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HAWBDocumentPivotGrid)).EndInit();
			this.ResumeLayout(false);
		}

		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2;

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
