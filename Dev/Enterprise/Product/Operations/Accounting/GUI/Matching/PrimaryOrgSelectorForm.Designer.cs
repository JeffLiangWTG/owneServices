using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	partial class PrimaryOrgSelectorForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.OrganisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContinueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelOperationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExplanationLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelectorCollection);
			// 
			// OrganisationsGrid
			// 
			this.OrganisationsGrid.AllowNavigation = false;
			this.OrganisationsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrganisationsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelector)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelector)(null)).OrganisationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelector)(null)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelector)(null)).TotalLocalOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelector)(null)).TransactionsCount)));
			this.OrganisationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|1dd2225f-243e-4ade-b9f0-9e6678deb890", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "OrganisationCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|65a101f0-615d-4ca9-b6f5-2d810bc9cb1d", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "OrganisationName";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|927c10e6-1df4-45d3-8117-6fa449c25478", "Total", "Total Outstanding Amt.", "Local Total Outstanding Amount", "");
			zCalcEditColumnStyleInfo1.ColumnName = "TotalLocalOutstandingAmount";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|7514fca1-3a13-4474-b548-01be6f7d68e2", "Count", "Trn. Count", "Transactions Count", "");
			zCalcEditColumnStyleInfo2.ColumnName = "TransactionsCount";
			this.OrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrganisationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrganisationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrganisationsGrid.GridId = "54EF0BFF-4558-469D-8C3F-E30AACAF8947";
			this.OrganisationsGrid.CopySelectedRowsAllowed = true;
			this.OrganisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationsGrid.IsWholeRowSelectedOnClick = true;
			this.OrganisationsGrid.LayoutKey = "DeptChargesGrid";
			this.OrganisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 29, true);
			this.OrganisationsGrid.Name = "OrganisationsGrid";
			this.OrganisationsGrid.ReadOnly = true;
			this.OrganisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 119, true);
			this.OrganisationsGrid.TabIndex = 0;
			// 
			// ContinueButton
			// 
			this.ContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|cf425c94-d49c-48a4-9473-b9e7fc748855", "Continue");
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 152, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.ContinueButton.TabIndex = 1;
			this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// CancelOperationButton
			// 
			this.CancelOperationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelOperationButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|7b697991-b659-465e-ac9f-05ef806c7201", "Cancel");
			this.CancelOperationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 152, true);
			this.CancelOperationButton.Name = "CancelOperationButton";
			this.CancelOperationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.CancelOperationButton.TabIndex = 2;
			this.CancelOperationButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ExplanationLabel
			// 
			this.ExplanationLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|0d65be95-d16c-4530-8f50-98b02b7184b5", "Please select primary organization");
			this.ExplanationLabel.ForeColor = System.Drawing.Color.Black;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExplanationLabel, false);
			this.ExplanationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.ExplanationLabel.Name = "ExplanationLabel";
			this.ExplanationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 22, true);
			this.ExplanationLabel.TabIndex = 3;
			// 
			// PrimaryOrgSelectorForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 205, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PrimaryOrgSelectorForm|bfcfad14-8e74-4ba2-9c83-317a7fa69f34", "Primary Organization");
			this.Controls.Add(this.ExplanationLabel);
			this.Controls.Add(this.CancelOperationButton);
			this.Controls.Add(this.ContinueButton);
			this.Controls.Add(this.OrganisationsGrid);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Matching.PrimaryOrgSelectorCollection);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 241, true);
			this.Name = "PrimaryOrgSelectorForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OrganisationsGrid, 0);
			this.Controls.SetChildIndex(this.ContinueButton, 0);
			this.Controls.SetChildIndex(this.CancelOperationButton, 0);
			this.Controls.SetChildIndex(this.ExplanationLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		internal Enterprise.ZArchitecture.ZGrid OrganisationsGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton ContinueButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CancelOperationButton;
		private Enterprise.ZArchitecture.ZLabel ExplanationLabel;
	}
}
