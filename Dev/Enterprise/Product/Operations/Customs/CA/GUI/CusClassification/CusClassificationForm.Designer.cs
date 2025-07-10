using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusClassificationForm
	{
		protected override void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			this.pGATabPage = new ZTabPage();
			this.pGARequirementsControl = new PGARequirementsControl();
			this.sIMATabPage = new ZTabPage();
			this.sIMAGroupBox = new ZGroupBox();
			this.sIMAFeeGrid = new ZArchitecture.ZGrid();
			this.sIMATopPanel = new ZPanel();
			this.sIMAMeasureDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.pGARequirementsControl.SuspendLayout();
			this.sIMATabPage.SuspendLayout();
			this.sIMAGroupBox.SuspendLayout();
			((ISupportInitialize)(this.sIMAFeeGrid)).BeginInit();
			this.sIMAFeeGrid.SuspendLayout();
			this.sIMATopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.sIMATabPage);
			this.MainTabControl.Controls.Add(this.pGATabPage);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 168, true);
			// 
			// PGATabPage
			// 
			this.pGATabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("30eaeba0-33eb-4631-b3dc-606ea50ac88c", "PGA Requirements");
			this.pGATabPage.Controls.Add(this.pGARequirementsControl);
			this.pGATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.pGATabPage.Name = "PGATabPage";
			this.pGATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.pGATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 168, true);
			this.pGATabPage.TabIndex = 4;
			this.pGATabPage.Text = "PGA Requirements";
			this.pGATabPage.UseVisualStyleBackColor = true;
			// 
			// PGARequirementsControl
			// 
			this.pGARequirementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pGARequirementsControl, "PGARequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)BusinessEntity).PGARequirements);
			this.pGARequirementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pGARequirementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.pGARequirementsControl.Name = "PGARequirementsControl";
			this.pGARequirementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 162, true);
			this.pGARequirementsControl.TabIndex = 1;
			// 
			// SIMATabPage
			// 
			this.sIMATabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f276db04-d7a9-4c62-b3c1-c00f3821945d", "SIMA");
			this.sIMATabPage.Controls.Add(this.sIMAGroupBox);
			this.sIMATabPage.Controls.Add(this.sIMATopPanel);
			this.sIMATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.sIMATabPage.Name = "SIMATabPage";
			this.sIMATabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sIMATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 168, true);
			this.sIMATabPage.TabIndex = 3;
			this.sIMATabPage.UseVisualStyleBackColor = true;
			// 
			// SIMAGroupBox
			// 
			this.sIMAGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F859F8B4-24B9-4215-ACEC-701BD41BAF62", "Duties and Taxes");
			this.sIMAGroupBox.Controls.Add(this.sIMAFeeGrid);
			this.sIMAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sIMAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 33, true);
			this.sIMAGroupBox.Name = "SIMAGroupBox";
			this.sIMAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 132, true);
			this.sIMAGroupBox.TabIndex = 4;
			this.sIMAGroupBox.TabStop = false;
			// 
			// SIMAFeeGrid
			// 
			this.sIMAFeeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.sIMAFeeGrid, "DutiesAndTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_TaxType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_ExemptCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_Override);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_RateType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_Rate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_UnitOfMeasure);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_NormalValuePerUnit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_NormalValueCurrency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_ForeignRate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((DutyAndTax)((System.Collections.IList)((CusClassification)BusinessEntity).DutiesAndTaxes).SyncRoot).C1_ForeignCurrency);
			this.sIMAFeeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "C1_TaxType";
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zDropEditColumnStyleInfo2.ColumnName = "C1_ExemptCode";
			zCheckBoxColumnStyleInfo1.ColumnName = "C1_Override";
			zDropEditColumnStyleInfo3.ColumnName = "C1_RateType";
			zCalcEditColumnStyleInfo1.ColumnName = "C1_Rate";
			zDropEditColumnStyleInfo4.ColumnName = "C1_UnitOfMeasure";
			zCalcEditColumnStyleInfo2.ColumnName = "C1_NormalValuePerUnit";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|546955BA-EE84-43CF-985C-532E521A8B09", "Normal Values");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "C1_NormalValueCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|546955BA-EE84-43CF-985C-532E521A8B09", "Normal Values");
			zCalcEditColumnStyleInfo3.ColumnName = "C1_ForeignRate";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|E2A4DF2A-A8E7-4CE2-AF21-610EFE39AC5F", "Foreign Rate");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "C1_ForeignCurrency";
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|E2A4DF2A-A8E7-4CE2-AF21-610EFE39AC5F", "Foreign Rate");
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.sIMAFeeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.sIMAFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.sIMAFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.sIMAFeeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.sIMAFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.sIMAFeeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.sIMAFeeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sIMAFeeGrid.GridId = "DCC0C75F-4316-4E9E-AA2E-40C252912E30";
			this.sIMAFeeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sIMAFeeGrid.LayoutKey = "SIMAFeeGrid";
			this.sIMAFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.sIMAFeeGrid.Name = "SIMAFeeGrid";
			this.sIMAFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 113, true);
			this.sIMAFeeGrid.TabIndex = 1;
			// 
			// SIMATopPanel
			// 
			this.sIMATopPanel.Controls.Add(this.sIMAMeasureDescriptionTextBox);
			this.sIMATopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sIMATopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.sIMATopPanel.Name = "SIMATopPanel";
			this.sIMATopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 30, true);
			this.sIMATopPanel.TabIndex = 3;
			// 
			// SIMAMeasureDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.sIMAMeasureDescriptionTextBox, "CCA_SIMADumpingDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)BusinessEntity).CCA_SIMADumpingDesc);
			this.sIMAMeasureDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9AEF0F4C-7202-470A-8B01-6089D2BEC080", "SIMA Measure");
			this.sIMAMeasureDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 5, true);
			this.sIMAMeasureDescriptionTextBox.Name = "SIMAMeasureDescriptionTextBox";
			this.sIMAMeasureDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.sIMAMeasureDescriptionTextBox.TabIndex = 1;
			// 
			// CusClassificationForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 251, true);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.Name = "CusClassificationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.pGATabPage.ResumeLayout(false);
			this.pGATabPage.PerformLayout();
			this.pGARequirementsControl.ResumeLayout(true);
			this.pGARequirementsControl.PerformLayout();
			this.sIMATabPage.ResumeLayout(false);
			this.sIMATabPage.PerformLayout();
			this.sIMAGroupBox.ResumeLayout(false);
			this.sIMAGroupBox.PerformLayout();
			((ISupportInitialize)(this.sIMAFeeGrid)).EndInit();
			this.sIMAFeeGrid.ResumeLayout(false);
			this.sIMAFeeGrid.PerformLayout();
			this.sIMATopPanel.ResumeLayout(false);
			this.sIMATopPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		PGARequirementsControl pGARequirementsControl;
		ZTabPage sIMATabPage;
		ZGroupBox sIMAGroupBox;
		ZArchitecture.ZGrid sIMAFeeGrid;
		ZPanel sIMATopPanel;
		ZArchitecture.ZTextBox sIMAMeasureDescriptionTextBox;
		ZTabPage pGATabPage;
	}
}
