using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ComplianceDocumentSupportingControl
	{


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ComplianceDocumentSupportingGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceDocumentSupportingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ComplianceDocumentSupportingReasonCollection);
			// 
			// ComplianceDocumentSupportingGrid
			// 
			this.ComplianceDocumentSupportingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceDocumentSupportingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ComplianceDocumentSupportingReason)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ComplianceDocumentSupportingReason)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ComplianceDocumentSupportingReason)(null)).EnglishDescription)));
			this.ComplianceDocumentSupportingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentSupportingControl|0378D80A-71F6-4c8c-905A-08F30DE7178B", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentSupportingControl|ED267F3B-10FE-4311-8D66-F86662579591", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ComplianceDocumentSupportingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComplianceDocumentSupportingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComplianceDocumentSupportingGrid.GridId = "7276FFE2-1F5E-449b-964F-A878A7245FD5";
			this.ComplianceDocumentSupportingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceDocumentSupportingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceDocumentSupportingGrid.LayoutKey = "ComplianceDocumentSupportingGrid";
			this.ComplianceDocumentSupportingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceDocumentSupportingGrid.Name = "ComplianceDocumentSupportingGrid";
			this.ComplianceDocumentSupportingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			this.ComplianceDocumentSupportingGrid.TabIndex = 0;
			// 
			// ComplianceDocumentSupportingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComplianceDocumentSupportingGrid);
			this.Name = "ComplianceDocumentSupportingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceDocumentSupportingGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}