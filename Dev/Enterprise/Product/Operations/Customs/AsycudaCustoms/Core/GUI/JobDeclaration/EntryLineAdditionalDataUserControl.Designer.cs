namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class EntryLineAdditionalDataUserControl
	{
		private System.ComponentModel.IContainer components = null;

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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExtendInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TaxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExtendInfoTabControl.SuspendLayout();
			this.TaxOrFeeTabPage.SuspendLayout();
			this.EntryLineDutyAndTaxGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
			this.EntryLineDutyAndTaxGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// ExtendInfoTabControl
			// 
			this.ExtendInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExtendInfoTabControl.Controls.Add(this.TaxOrFeeTabPage);
			this.ExtendInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendInfoTabControl.Name = "ExtendInfoTabControl";
			this.ExtendInfoTabControl.SelectedIndex = 0;
			this.ExtendInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
			this.ExtendInfoTabControl.TabIndex = 15;
			// 
			// TaxOrFeeTabPage
			// 
			this.TaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("CDBB92E9-5C67-4DAC-ACBF-BFFBA10548CF", "Tax Or Fee");
			this.TaxOrFeeTabPage.Controls.Add(this.EntryLineDutyAndTaxGroupBox);
			this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxOrFeeTabPage.Name = "TaxOrFeeTabPage";
			this.TaxOrFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 184, true);
			this.TaxOrFeeTabPage.TabIndex = 2;
			this.TaxOrFeeTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryLineDutyAndTaxGroupBox
			// 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("A2778ED1-48C1-4CAE-8B36-B374995C7036", "Duty And Tax");
			this.EntryLineDutyAndTaxGroupBox.Controls.Add(this.EntryLineDutyAndTaxGrid);
			this.EntryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineDutyAndTaxGroupBox, true);
			this.EntryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineDutyAndTaxGroupBox.Name = "EntryLineDutyAndTaxGroupBox";
			this.EntryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 178, true);
			this.EntryLineDutyAndTaxGroupBox.TabIndex = 4;
			this.EntryLineDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineDutyAndTaxGrid
			// 
			this.EntryLineDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineDutyAndTaxGrid, "CustomsEntryHeaders.AllEntryLines.Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).Fees)).SyncRoot)).LocalCurrencyCode)));
			this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("C1F07DE2-A2EE-4E95-A9A7-6DF438828E9A", "Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("de9ef891-c513-4bdc-b48d-791b56906d9d", "Total Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "LocalCurrencyCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineDutyAndTaxGrid.GridId = "78B694FA-9E6D-435B-800B-6BEF32B49FD6";
			this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 159, true);
			this.EntryLineDutyAndTaxGrid.TabIndex = 0;
			// 
			// EntryLineAdditionalDataUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExtendInfoTabControl);
			this.Name = "EntryLineAdditionalDataUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.TaxOrFeeTabPage.ResumeLayout(false);
			this.TaxOrFeeTabPage.PerformLayout();
			this.EntryLineDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineDutyAndTaxGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
			this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineDutyAndTaxGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZTemplateTabControl ExtendInfoTabControl;
		ZArchitecture.GUI.ZTabPage TaxOrFeeTabPage;
		ZArchitecture.GUI.ZGroupBox EntryLineDutyAndTaxGroupBox;
		ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
	}
}
