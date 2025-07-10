using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using System.Windows.Forms;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPProcessUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TreatmentActiveIngredientGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TreatmentActiveIngredientGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RFPProcessPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.QL_ProduceTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_UseByEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QL_UseByStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ProcessingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EE_TreatmentInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TreatmentActiveIngredientGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TreatmentActiveIngredientGrid)).BeginInit();
			this.TreatmentActiveIngredientGrid.SuspendLayout();
			this.RFPProcessPanel.SuspendLayout();
			this.QL_UseByEndDateEdit.SuspendLayout();
			this.QL_UseByStartDateEdit.SuspendLayout();
			this.ProcessingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProcessingGrid)).BeginInit();
			this.ProcessingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// TreatmentActiveIngredientGroupBox
			// 
			this.TreatmentActiveIngredientGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TreatmentActiveIngredientGroupBox.Controls.Add(this.TreatmentActiveIngredientGrid);
			this.TreatmentActiveIngredientGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 173, true);
			this.TreatmentActiveIngredientGroupBox.Name = "TreatmentActiveIngredientGroupBox";
			this.TreatmentActiveIngredientGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 90, true);
			this.TreatmentActiveIngredientGroupBox.TabIndex = 1;
			this.TreatmentActiveIngredientGroupBox.TabStop = false;
			this.TreatmentActiveIngredientGroupBox.Text = "Treatment Active Ingredients";
			// 
			// TreatmentActiveIngredientGrid
			// 
			this.TreatmentActiveIngredientGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TreatmentActiveIngredientGrid, "FilteredInvoiceLines.QuarantineExDocLine+Processes.Ingredients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).Ingredients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.TreatmentActiveIngredient)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).Ingredients)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.TreatmentActiveIngredient)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).Ingredients)).SyncRoot)).Description)));
			this.TreatmentActiveIngredientGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TreatmentActiveIngredientGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TreatmentActiveIngredientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TreatmentActiveIngredientGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreatmentActiveIngredientGrid.GridId = "530e3a20-1d26-4c3d-aca5-d6e705e1c1e1";
			this.TreatmentActiveIngredientGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TreatmentActiveIngredientGrid.LayoutKey = "TreatmentActiveIngredientGrid";
			this.TreatmentActiveIngredientGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.TreatmentActiveIngredientGrid.Name = "TreatmentActiveIngredientGrid";
			this.TreatmentActiveIngredientGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 73, true);
			this.TreatmentActiveIngredientGrid.TabIndex = 0;
			// 
			// RFPProcessPanel
			// 
			this.RFPProcessPanel.Controls.Add(this.QL_ProduceTypeTextBox);
			this.RFPProcessPanel.Controls.Add(this.QL_UseByEndDateEdit);
			this.RFPProcessPanel.Controls.Add(this.QL_UseByStartDateEdit);
			this.RFPProcessPanel.Controls.Add(this.ProcessingDetailsGroupBox);
			this.RFPProcessPanel.Controls.Add(this.TreatmentActiveIngredientGroupBox);
			this.RFPProcessPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPProcessPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RFPProcessPanel.Name = "RFPProcessPanel";
			this.RFPProcessPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 265, true);
			this.RFPProcessPanel.TabIndex = 0;
			// 
			// QL_ProduceTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_ProduceTypeTextBox, "FilteredInvoiceLines.QuarantineExDocLine+QL_ProduceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.QL_ProduceType)));
			this.QL_ProduceTypeTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|acecef83-96c7-4f38-9147-5166ce836503", "Produce Type");
			this.QL_ProduceTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 140, true);
			this.QL_ProduceTypeTextBox.Name = "QL_ProduceTypeTextBox";
			this.QL_ProduceTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.QL_ProduceTypeTextBox.TabIndex = 4;
			// 
			// QL_UseByEndDateEdit
			// 
			this.QL_UseByEndDateEdit.AllowDrop = true;
			this.QL_UseByEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.QL_UseByEndDateEdit, "FilteredInvoiceLines.QuarantineExDocLine+QL_UseByEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.QL_UseByEnd)));
			this.QL_UseByEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 140, true);
			this.QL_UseByEndDateEdit.Name = "QL_UseByEndDateEdit";
			this.QL_UseByEndDateEdit.TabIndex = 3;
			// 
			// QL_UseByStartDateEdit
			// 
			this.QL_UseByStartDateEdit.AllowDrop = true;
			this.QL_UseByStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.QL_UseByStartDateEdit, "FilteredInvoiceLines.QuarantineExDocLine+QL_UseByStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.QL_UseByStart)));
			this.QL_UseByStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 140, true);
			this.QL_UseByStartDateEdit.Name = "QL_UseByStartDateEdit";
			this.QL_UseByStartDateEdit.TabIndex = 2;
			// 
			// ProcessingDetailsGroupBox
			// 
			this.ProcessingDetailsGroupBox.Controls.Add(this.EE_TreatmentInfoTextBox);
			this.ProcessingDetailsGroupBox.Controls.Add(this.ProcessingGrid);
			this.ProcessingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProcessingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcessingDetailsGroupBox.Name = "ProcessingDetailsGroupBox";
			this.ProcessingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 136, true);
			this.ProcessingDetailsGroupBox.TabIndex = 0;
			this.ProcessingDetailsGroupBox.TabStop = false;
			this.ProcessingDetailsGroupBox.Text = "Processing Details";
			// 
			// EE_TreatmentInfoTextBox
			// 
			this.EE_TreatmentInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EE_TreatmentInfoTextBox, "FilteredInvoiceLines.QuarantineExDocLine+Processes.EE_TreatmentInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentInfo)));
			this.EE_TreatmentInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 112, true);
			this.EE_TreatmentInfoTextBox.Name = "EE_TreatmentInfoTextBox";
			this.EE_TreatmentInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 17, true);
			this.EE_TreatmentInfoTextBox.TabIndex = 1;
			// 
			// ProcessingGrid
			// 
			this.ProcessingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProcessingGrid, "FilteredInvoiceLines.QuarantineExDocLine+Processes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_ProcessingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).Lookups.ProcessingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_EstablishmentPostedStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_E2_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_AuthorisationEstablishmentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_EstablishmentIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_Depuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_HarvestArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_InspectionRequestedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_LeaseNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).Lookups.TreatmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentConcentration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentConcentrationUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentDurationUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentTemperature)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocEstablishmentAndTime)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuarantineExDocLine.Processes)).SyncRoot)).EE_TreatmentTemperatureUQ)));
			this.ProcessingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.Caption = "Processing Type";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "EE_ProcessingType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zTextBoxColumnStyleInfo5.Caption = "Posted Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EE_EstablishmentPostedStatusDescription";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.Caption = "Processing Establishment";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "EE_E2_Address";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfo2.Caption = "ID";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EE_AuthorisationEstablishmentID";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.Caption = "Establishment Indicator";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "EE_EstablishmentIndicator";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "EE_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "EE_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			zDateEditColumnStyleInfo3.Caption = "Depuration";
			zDateEditColumnStyleInfo3.ColumnName = "EE_Depuration";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
			zTextBoxColumnStyleInfo3.Caption = "Harvest Area";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EE_HarvestArea";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDateEditColumnStyleInfo4.Caption = "Inspection Requested Date";
			zDateEditColumnStyleInfo4.ColumnName = "EE_InspectionRequestedDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(154);
			zTextBoxColumnStyleInfo4.Caption = "Lease Number";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EE_LeaseNumber";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zDropEditColumnStyleInfo4.BindToList = "Lookups.TreatmentCode";
			zDropEditColumnStyleInfo4.Caption = "Treatment Code";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "EE_TreatmentCode";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "EE_TreatmentConcentration";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2B4DA539-6FEE-475B-A23A-6393696377BB", "Treatment Concentration");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "EE_TreatmentConcentrationUQ";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2B4DA539-6FEE-475B-A23A-6393696377BB", "Treatment Concentration");
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "EE_TreatmentDuration";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("294A90D3-CF65-4CC4-8E91-D5D1349D8C56", "Treatment Duration");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.ColumnName = "EE_TreatmentDurationUQ";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("294A90D3-CF65-4CC4-8E91-D5D1349D8C56", "Treatment Duration");
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "EE_TreatmentTemperature";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("1F23C71B-9472-4E2A-811B-6BD8360D5661", "Treatment Temperature");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "EE_TreatmentTemperatureUQ";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("1F23C71B-9472-4E2A-811B-6BD8360D5661", "Treatment Temperature");
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProcessingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ProcessingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ProcessingGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ProcessingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProcessingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ProcessingGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ProcessingGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ProcessingGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ProcessingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProcessingGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ProcessingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProcessingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ProcessingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProcessingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ProcessingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ProcessingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ProcessingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ProcessingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ProcessingGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProcessingGrid.GridId = "98649f9d-741a-4afc-a2ec-c3de51f593e2";
			this.ProcessingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProcessingGrid.LayoutKey = "ProcessingGrid";
			this.ProcessingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ProcessingGrid.Name = "ProcessingGrid";
			this.ProcessingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 93, true);
			this.ProcessingGrid.TabIndex = 0;
			// 
			// RFPProcessUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RFPProcessPanel);
			this.Name = "RFPProcessUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TreatmentActiveIngredientGroupBox.ResumeLayout(false);
			this.TreatmentActiveIngredientGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TreatmentActiveIngredientGrid)).EndInit();
			this.TreatmentActiveIngredientGrid.ResumeLayout(false);
			this.TreatmentActiveIngredientGrid.PerformLayout();
			this.RFPProcessPanel.ResumeLayout(false);
			this.RFPProcessPanel.PerformLayout();
			this.QL_UseByEndDateEdit.ResumeLayout(true);
			this.QL_UseByEndDateEdit.PerformLayout();
			this.QL_UseByStartDateEdit.ResumeLayout(true);
			this.QL_UseByStartDateEdit.PerformLayout();
			this.ProcessingDetailsGroupBox.ResumeLayout(false);
			this.ProcessingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProcessingGrid)).EndInit();
			this.ProcessingGrid.ResumeLayout(false);
			this.ProcessingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel RFPProcessPanel;
		private ZGrid TreatmentActiveIngredientGrid;
		private ZGroupBox TreatmentActiveIngredientGroupBox;
		private ZGroupBox ProcessingDetailsGroupBox;
		private ZTextBox EE_TreatmentInfoTextBox;
		private ZGrid ProcessingGrid;
		private ZTextBox QL_ProduceTypeTextBox;
		private ZDateEdit QL_UseByEndDateEdit;
		private ZDateEdit QL_UseByStartDateEdit;
	}
}
