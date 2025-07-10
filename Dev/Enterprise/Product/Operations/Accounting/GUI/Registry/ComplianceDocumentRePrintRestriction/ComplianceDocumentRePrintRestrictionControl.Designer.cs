using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ComplianceDocumentRePrintRestrictionControl
	{


		#region Component Designer generated code

		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			this.ComplianceDocumentRePrintRestrictionGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceDocumentRePrintRestrictionGrid)).BeginInit();
			this.ComplianceDocumentRePrintRestrictionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ComplianceDocumentRePrintRestrictionCollection);
			// 
			// ComplianceDocumentRePrintRestrictionGrid
			// 
			this.ComplianceDocumentRePrintRestrictionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceDocumentRePrintRestrictionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ComplianceDocumentRePrintRestriction)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ComplianceDocumentRePrintRestriction)(null)).OrganizationCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((ComplianceDocumentRePrintRestriction)(null)).NumberOfReprintAllowed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ComplianceDocumentRePrintRestriction)(null)).ComplianceDocumentMenu)));
			this.ComplianceDocumentRePrintRestrictionGrid.CaptionVisible = false;

			zDropEditColumnStyleInfo1.ColumnName = "OrganizationCategory";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentRePrintRestrictionControl|EEE19C65-9BFB-464a-8A27-1005CFFCA9F7", "Organization Category");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.ColumnName = "NumberOfReprintAllowed";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentRePrintRestrictionControl|65F00A57-6385-4cf9-9F75-FD3BC8D682EE", "Number of Re-print Allowed");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ComplianceDocumentMenu";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentRePrintRestrictionControl|83DF631F-8950-48f1-8A42-1DF19F2C6877", "Compliance Document Menu");
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComplianceDocumentRePrintRestrictionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ComplianceDocumentRePrintRestrictionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComplianceDocumentRePrintRestrictionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ComplianceDocumentRePrintRestrictionGrid.CopySelectedRowsAllowed = true;
			this.ComplianceDocumentRePrintRestrictionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceDocumentRePrintRestrictionGrid.GridId = "09ED59EC-D6F3-4b7d-8795-D0B3718E5EBC";
			this.ComplianceDocumentRePrintRestrictionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceDocumentRePrintRestrictionGrid.LayoutKey = "ComplianceDocumentRePrintRestrictionGrid";
			this.ComplianceDocumentRePrintRestrictionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceDocumentRePrintRestrictionGrid.Name = "ComplianceDocumentRePrintRestrictionGrid";
			this.ComplianceDocumentRePrintRestrictionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.ComplianceDocumentRePrintRestrictionGrid.TabIndex = 0;
			// 
			// ComplianceDocumentRePrintRestrictionControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ComplianceDocumentRePrintRestrictionGrid);
			this.Name = "ComplianceDocumentRePrintRestrictionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceDocumentRePrintRestrictionGrid)).EndInit();
			this.ComplianceDocumentRePrintRestrictionGrid.ResumeLayout(false);
			this.ComplianceDocumentRePrintRestrictionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}