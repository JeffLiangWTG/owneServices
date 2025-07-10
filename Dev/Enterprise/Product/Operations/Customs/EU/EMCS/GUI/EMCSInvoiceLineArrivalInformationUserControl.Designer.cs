using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class EMCSInvoiceLineArrivalInformationUserControl
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
			this.ShortageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExplanationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExciseProductZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActualQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ReportOfReceiptGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeclaredValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AcceptedQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.RefusedQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReasonGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShortageGroupBox.SuspendLayout();
			this.ExciseProductZDropEdit.SuspendLayout();
			this.ActualQuantityCalcDropEdit.SuspendLayout();
			this.ReportOfReceiptGroupBox.SuspendLayout();
			this.DeclaredValueCalcDropEdit.SuspendLayout();
			this.AcceptedQuantityCalcDropEdit.SuspendLayout();
			this.RefusedQuantityCalcDropEdit.SuspendLayout();
			this.ReasonGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReasonGrid)).BeginInit();
			this.ReasonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine);
			// 
			// ShortageGroupBox
			// 
			this.ShortageGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("ce1a4150-7057-406e-9c60-0b66b7a256ee", "Excess or Shortage");
			this.ShortageGroupBox.Controls.Add(this.ExplanationTextBox);
			this.ShortageGroupBox.Controls.Add(this.ExciseProductZDropEdit);
			this.ShortageGroupBox.Controls.Add(this.ActualQuantityCalcDropEdit);
			this.ShortageGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ShortageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
			this.ShortageGroupBox.Name = "ShortageGroupBox";
			this.ShortageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 113, true);
			this.ShortageGroupBox.TabIndex = 1;
			this.ShortageGroupBox.TabStop = false;
			// 
			// ExplanationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExplanationTextBox, "Outturn.C5_OutturnResultReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.C5_OutturnResultReason)));
			this.ExplanationTextBox.CaptionResourceString = null;
			this.ExplanationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ExplanationTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ExplanationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 57, true);
			this.ExplanationTextBox.Multiline = true;
			this.ExplanationTextBox.Name = "ExplanationTextBox";
			this.ExplanationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ExplanationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 48, true);
			this.ExplanationTextBox.TabIndex = 2;
			// 
			// ExciseProductZDropEdit
			// 
			this.ExciseProductZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExciseProductZDropEdit, "ZG_ExciseProductCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_ExciseProductCode)));
			this.ExciseProductZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 14, true);
			this.ExciseProductZDropEdit.Name = "ExciseProductZDropEdit";
			this.ExciseProductZDropEdit.ShouldResizeByMaxLength = true;
			this.ExciseProductZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 20, true);
			this.ExciseProductZDropEdit.TabIndex = 0;
			// 
			// ActualQuantityCalcDropEdit
			// 
			this.ActualQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.ActualQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.UnitQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.CustomsUQList)));
			this.ActualQuantityCalcDropEdit.BindToAmount = "Outturn.ActualQuantity";
			this.ActualQuantityCalcDropEdit.BindToList = "Lookups.CustomsUQList";
			this.ActualQuantityCalcDropEdit.BindToUnit = "Outturn.UnitQuantity";
			this.ActualQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("267ce9e0-c147-49d0-b39b-38cc4eac6e48", "Quantity");
			this.ActualQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 40, true);
			this.ActualQuantityCalcDropEdit.Name = "ActualQuantityCalcDropEdit";
			this.ActualQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ActualQuantityCalcDropEdit.TabIndex = 1;
			this.ActualQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ReportOfReceiptGroupBox
			// 
			this.ReportOfReceiptGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("cfdb5e65-fcbb-4fd8-8206-77c084473b4b", "Report of Receipt");
			this.ReportOfReceiptGroupBox.Controls.Add(this.DeclaredValueCalcDropEdit);
			this.ReportOfReceiptGroupBox.Controls.Add(this.AcceptedQuantityCalcDropEdit);
			this.ReportOfReceiptGroupBox.Controls.Add(this.RefusedQuantityCalcDropEdit);
			this.ReportOfReceiptGroupBox.Controls.Add(this.ReasonGroupBox);
			this.ReportOfReceiptGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ReportOfReceiptGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportOfReceiptGroupBox.Name = "ReportOfReceiptGroupBox";
			this.ReportOfReceiptGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 115, true);
			this.ReportOfReceiptGroupBox.TabIndex = 0;
			this.ReportOfReceiptGroupBox.TabStop = false;
			// 
			// DeclaredValueCalcDropEdit
			// 
			this.DeclaredValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclaredValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_DeclaredValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.UnitQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.CustomsUQList)));
			this.DeclaredValueCalcDropEdit.BindToAmount = "ZG_DeclaredValue";
			this.DeclaredValueCalcDropEdit.BindToList = "Lookups.CustomsUQList";
			this.DeclaredValueCalcDropEdit.BindToUnit = "Outturn.UnitQuantity";
			this.DeclaredValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 25, true);
			this.DeclaredValueCalcDropEdit.Name = "DeclaredValueCalcDropEdit";
			this.DeclaredValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.DeclaredValueCalcDropEdit.TabIndex = 0;
			this.DeclaredValueCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// AcceptedQuantityCalcDropEdit
			// 
			this.AcceptedQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AcceptedQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.ObservedDifference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.UnitQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.CustomsUQList)));
			this.AcceptedQuantityCalcDropEdit.BindToAmount = "Outturn.ObservedDifference";
			this.AcceptedQuantityCalcDropEdit.BindToList = "Lookups.CustomsUQList";
			this.AcceptedQuantityCalcDropEdit.BindToUnit = "Outturn.UnitQuantity";
			this.AcceptedQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 52, true);
			this.AcceptedQuantityCalcDropEdit.Name = "AcceptedQuantityCalcDropEdit";
			this.AcceptedQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.AcceptedQuantityCalcDropEdit.TabIndex = 1;
			this.AcceptedQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// RefusedQuantityCalcDropEdit
			// 
			this.RefusedQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefusedQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.C5_RejectedQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.UnitQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.CustomsUQList)));
			this.RefusedQuantityCalcDropEdit.BindToAmount = "Outturn.C5_RejectedQuantity";
			this.RefusedQuantityCalcDropEdit.BindToList = "Lookups.CustomsUQList";
			this.RefusedQuantityCalcDropEdit.BindToUnit = "Outturn.UnitQuantity";
			this.RefusedQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 78, true);
			this.RefusedQuantityCalcDropEdit.Name = "RefusedQuantityCalcDropEdit";
			this.RefusedQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.RefusedQuantityCalcDropEdit.TabIndex = 2;
			this.RefusedQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ReasonGroupBox
			// 
			this.ReasonGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ReasonGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("ee33bb13-aba8-48f0-98f5-6ccbbaa6bb4f", "Reason");
			this.ReasonGroupBox.Controls.Add(this.ReasonGrid);
			this.ReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 17, true);
			this.ReasonGroupBox.Name = "ReasonGroupBox";
			this.ReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 92, true);
			this.ReasonGroupBox.TabIndex = 3;
			this.ReasonGroupBox.TabStop = false;
			// 
			// ReasonGrid
			// 
			this.ReasonGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReasonGrid, "Outturn.ReportOfReceiptReasons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.ReportOfReceiptReasons)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.ReportOfReceiptReason)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.ReportOfReceiptReasons)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.ReportOfReceiptReason)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Outturn.ReportOfReceiptReasons)).SyncRoot)).CY_Data)));
			this.ReasonGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("2a7eae56-9614-4cc7-9ba6-301d59dfa972", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			this.ReasonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReasonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReasonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReasonGrid.GridId = "d322a548-7523-4cf1-be15-be70e84ed6de";
			this.ReasonGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReasonGrid.LayoutKey = "ReasonGrid";
			this.ReasonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReasonGrid.Name = "ReasonGrid";
			this.ReasonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 73, true);
			this.ReasonGrid.TabIndex = 0;
			// 
			// EMCSInvoiceLineArrivalInformationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShortageGroupBox);
			this.Controls.Add(this.ReportOfReceiptGroupBox);
			this.Name = "EMCSInvoiceLineArrivalInformationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 229, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShortageGroupBox.ResumeLayout(false);
			this.ShortageGroupBox.PerformLayout();
			this.ExciseProductZDropEdit.ResumeLayout(true);
			this.ExciseProductZDropEdit.PerformLayout();
			this.ActualQuantityCalcDropEdit.ResumeLayout(true);
			this.ActualQuantityCalcDropEdit.PerformLayout();
			this.ReportOfReceiptGroupBox.ResumeLayout(false);
			this.ReportOfReceiptGroupBox.PerformLayout();
			this.DeclaredValueCalcDropEdit.ResumeLayout(true);
			this.DeclaredValueCalcDropEdit.PerformLayout();
			this.AcceptedQuantityCalcDropEdit.ResumeLayout(true);
			this.AcceptedQuantityCalcDropEdit.PerformLayout();
			this.RefusedQuantityCalcDropEdit.ResumeLayout(true);
			this.RefusedQuantityCalcDropEdit.PerformLayout();
			this.ReasonGroupBox.ResumeLayout(false);
			this.ReasonGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReasonGrid)).EndInit();
			this.ReasonGrid.ResumeLayout(false);
			this.ReasonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZGroupBox ReportOfReceiptGroupBox;
		private ZGroupBox ReasonGroupBox;
		private ZGrid ReasonGrid;
		private ZCalcDropEdit RefusedQuantityCalcDropEdit;
		private ZGroupBox ShortageGroupBox;
		private ZCalcDropEdit ActualQuantityCalcDropEdit;
		private ZCalcDropEdit AcceptedQuantityCalcDropEdit;
		private ZDropEdit ExciseProductZDropEdit;
		private ZCalcDropEdit DeclaredValueCalcDropEdit;
		internal ZTextBox ExplanationTextBox;
	}
}
