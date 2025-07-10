namespace Enterprise.Customs.MX.GUI
{
	partial class IdentifiersUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo identifierMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo identifierMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo identifierMultiControlColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.IdentifiersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IdentifiersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IdentifiersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IdentifiersGrid)).BeginInit();
			this.IdentifiersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.JobComInvoiceLine);
			// 
			// IdentifiersGroupBox
			// 
			this.IdentifiersGroupBox.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("EA84B937-D2FA-4CB3-9633-2980CF26A6CC", "Identifiers");
			this.IdentifiersGroupBox.Controls.Add(this.IdentifiersGrid);
			this.IdentifiersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IdentifiersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IdentifiersGroupBox.Name = "IdentifiersGroupBox";
			this.IdentifiersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 327, true);
			this.IdentifiersGroupBox.TabIndex = 0;
			this.IdentifiersGroupBox.TabStop = false;
			// 
			// IdentifiersGrid
			// 
			this.IdentifiersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IdentifiersGrid, "Identifiers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).Applicability)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).Complement1FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).FillGuidance1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).Complement2FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).FillGuidance2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).Complement3FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Identifier)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Identifiers)).SyncRoot)).FillGuidance3)));
			this.IdentifiersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "Applicability";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			identifierMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			identifierMultiControlColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			identifierMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
			identifierMultiControlColumnStyleInfo1.FieldTypeColumnName = "Complement1FieldType";
			identifierMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "FillGuidance1";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			identifierMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			identifierMultiControlColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber2";
			identifierMultiControlColumnStyleInfo2.DefaultCollectionIndex = 0;
			identifierMultiControlColumnStyleInfo2.FieldTypeColumnName = "Complement2FieldType";
			identifierMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "FillGuidance2";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			identifierMultiControlColumnStyleInfo3.BindToDecimalPlaces = null;
			identifierMultiControlColumnStyleInfo3.ColumnName = "CSI_Description";
			identifierMultiControlColumnStyleInfo3.DefaultCollectionIndex = 0;
			identifierMultiControlColumnStyleInfo3.FieldTypeColumnName = "Complement3FieldType";
			identifierMultiControlColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.ColumnName = "FillGuidance3";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.IdentifiersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.IdentifiersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IdentifiersGrid.ColumnStyles.Add(identifierMultiControlColumnStyleInfo1);
			this.IdentifiersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IdentifiersGrid.ColumnStyles.Add(identifierMultiControlColumnStyleInfo2);
			this.IdentifiersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.IdentifiersGrid.ColumnStyles.Add(identifierMultiControlColumnStyleInfo3);
			this.IdentifiersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.IdentifiersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IdentifiersGrid.GridId = "4a2ef33a-79d2-4943-8b67-f70d230018ae";
			this.IdentifiersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IdentifiersGrid.LayoutKey = "IdentifiersGrid";
			this.IdentifiersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IdentifiersGrid.Name = "IdentifiersGrid";
			this.IdentifiersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 308, true);
			this.IdentifiersGrid.TabIndex = 0;
			// 
			// IdentifiersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IdentifiersGroupBox);
			this.Name = "IdentifiersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 327, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IdentifiersGroupBox.ResumeLayout(false);
			this.IdentifiersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IdentifiersGrid)).EndInit();
			this.IdentifiersGrid.ResumeLayout(false);
			this.IdentifiersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox IdentifiersGroupBox;
		internal ZArchitecture.ZGrid IdentifiersGrid;
	}
}
