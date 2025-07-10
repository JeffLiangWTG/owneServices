using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class CNSCUserControl
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
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CNSCSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TotalQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PackMarksTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.PackQtyCalcEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DeliveredPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.UNDGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NNIECRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.BottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ComponentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComponentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LPCOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CNSCSplitContainer)).BeginInit();
			this.CNSCSplitContainer.Panel1.SuspendLayout();
			this.CNSCSplitContainer.Panel2.SuspendLayout();
			this.CNSCSplitContainer.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.TotalQuantityCalcDropEdit.SuspendLayout();
			this.PackMarksTextBox.SuspendLayout();
			this.PackQtyCalcEdit.SuspendLayout();
			this.DeliveredPartyAddressControl.SuspendLayout();
			this.UNDGGuidFindBox.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).BeginInit();
			this.BottomSplitContainer.Panel1.SuspendLayout();
			this.BottomSplitContainer.Panel2.SuspendLayout();
			this.BottomSplitContainer.SuspendLayout();
			this.ComponentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentGrid)).BeginInit();
			this.ComponentGrid.SuspendLayout();
			this.LPCOGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CNSCPGAHeader);
			// 
			// CNSCSplitContainer
			// 
			this.CNSCSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CNSCSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.CNSCSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CNSCSplitContainer.Name = "CNSCSplitContainer";
			this.CNSCSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// CNSCSplitContainer.Panel1
			// 
			this.CNSCSplitContainer.Panel1.Controls.Add(this.DetailsGroupBox);
			// 
			// CNSCSplitContainer.Panel2
			// 
			this.CNSCSplitContainer.Panel2.Controls.Add(this.BottomSplitContainer);
			this.CNSCSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 400, true);
			this.CNSCSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(162);
			this.CNSCSplitContainer.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.TotalQuantityCalcDropEdit);
			this.DetailsGroupBox.Controls.Add(this.PackMarksTextBox);
			this.DetailsGroupBox.Controls.Add(this.PackQtyCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.DeliveredPartyAddressControl);
			this.DetailsGroupBox.Controls.Add(this.UNDGGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.CategoryDropEdit);
			this.DetailsGroupBox.Controls.Add(this.NNIECRTextBox);
			this.DetailsGroupBox.Controls.Add(this.CountIntEdit);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 162, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// TotalQuantityCalcDropEdit
			// 
			this.TotalQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).InvoiceLine.JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).InvoiceLine.JI_NetWeightUQ)));
			this.TotalQuantityCalcDropEdit.BindToAmount = "InvoiceLine.JI_NetWeight";
			this.TotalQuantityCalcDropEdit.BindToUnit = "InvoiceLine.JI_NetWeightUQ";
			this.TotalQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e42b3d66-28f0-4213-b4de-b58d84af3ee2", "Total Quantity");
			this.TotalQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 71, true);
			this.TotalQuantityCalcDropEdit.Name = "TotalQuantityCalcDropEdit";
			this.TotalQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.TotalQuantityCalcDropEdit.TabIndex = 4;
			// 
			// PackMarksTextBox
			//
			this.PackMarksTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackMarksTextBox, "CA_PackMarks");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).CA_PackMarks)));
			this.PackMarksTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("EB9376F8-8598-4E6C-A4E9-CE1E451BBEB6", "Pack Marks");
			this.PackMarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 124, true);
			this.PackMarksTextBox.Name = "PackMarksTextBox";
			this.PackMarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 21, true);
			this.PackMarksTextBox.TabIndex = 7;
			// 
			// PackQtyCalcEdit
			// 
			this.PackQtyCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackQtyCalcEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).CA_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).CA_PackUQ)));
			this.PackQtyCalcEdit.BindToAmount = "CA_PackQty";
			this.PackQtyCalcEdit.BindToUnit = "CA_PackUQ";
			this.PackQtyCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1E9BFFC6-5F76-42FB-AA9F-76B2BDF92102", "Pack Qty");
			this.PackQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 98, true);
			this.PackQtyCalcEdit.Name = "PackQtyCalcEdit";
			this.PackQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.PackQtyCalcEdit.TabIndex = 6;
			// 
			// DeliveredPartyAddressControl
			// 
			this.DeliveredPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveredPartyAddressControl, "InvoiceLine.JI_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).InvoiceLine.JI_OA_ConsigneeAddress)));
			this.DeliveredPartyAddressControl.BindToOrgList = "InvoiceLine.Lookups.Consignees";
			this.DeliveredPartyAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2265853D-6E23-4BCE-8BEE-F827BD94E347", "Delivery Party");
			this.DeliveredPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 19, true);
			this.DeliveredPartyAddressControl.Name = "DeliveredPartyAddressControl";
			this.DeliveredPartyAddressControl.PopupCaption = "";
			this.DeliveredPartyAddressControl.ReadOnly = false;
			this.DeliveredPartyAddressControl.ShowAddress = false;
			this.DeliveredPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DeliveredPartyAddressControl.TabIndex = 1;
			// 
			// UNDGGuidFindBox
			// 
			this.UNDGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDGGuidFindBox, "DangerousGoodsDGSubs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).DangerousGoodsDGSubs)));
			this.UNDGGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6C0C40E3-BC06-4963-8932-FB7AC29E437D", "UNDG Code");
			this.UNDGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 45, true);
			this.UNDGGuidFindBox.Name = "UNDGDropEdit";
			this.UNDGGuidFindBox.ShouldResize = true;
			this.UNDGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.UNDGGuidFindBox.TabIndex = 3;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).CA_Category)));
			this.CategoryDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ACDB598A-8724-47C9-A2EC-B56A7DA06742", "Category");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.ShouldResizeByMaxLength = true;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.CategoryDropEdit.TabIndex = 0;
			// 
			// NNIECRTextBox
			// 
			this.BindingSource.SetBindingMember(this.NNIECRTextBox, "CA_NNIECRSchePartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).CA_NNIECRSchePartNo)));
			this.NNIECRTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("615F330F-9040-45CD-BA6A-EE204420981C", "NNIECR Schedule Part No.");
			this.NNIECRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 71, true);
			this.NNIECRTextBox.Name = "NNIECRTextBox";
			this.NNIECRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.NNIECRTextBox.TabIndex = 5;
			// 
			// CountIntEdit
			// 
			this.CountIntEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountIntEdit, "CA_UnitQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).CA_UnitQty)));
			this.CountIntEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A34E790B-6FFF-498C-A2C3-DC15E815D434", "Count");
			this.CountIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.CountIntEdit.Name = "CountIntEdit";
			this.CountIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.CountIntEdit.TabIndex = 2;
			// 
			// BottomSplitContainer
			// 
			this.BottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BottomSplitContainer.Name = "BottomSplitContainer";
			// 
			// BottomSplitContainer.Panel1
			// 
			this.BottomSplitContainer.Panel1.Controls.Add(this.ComponentGroupBox);
			// 
			// BottomSplitContainer.Panel2
			// 
			this.BottomSplitContainer.Panel2.Controls.Add(this.LPCOGroupBox);
			this.BottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 234, true);
			this.BottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(515);
			this.BottomSplitContainer.TabIndex = 0;
			// 
			// ComponentGroupBox
			// 
			this.ComponentGroupBox.Controls.Add(this.ComponentGrid);
			this.ComponentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentGroupBox.Name = "ComponentGroupBox";
			this.ComponentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 234, true);
			this.ComponentGroupBox.TabIndex = 0;
			this.ComponentGroupBox.TabStop = false;
			this.ComponentGroupBox.Text = "Chemical Specifications";
			// 
			// ComponentGrid
			// 
			this.ComponentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComponentGrid, "Components");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).Components)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.Component)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).Components)).SyncRoot)).CA_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.Component)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).Components)).SyncRoot)).CA_NameFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.Component)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).Components)).SyncRoot)).CA_Qty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.Component)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).Components)).SyncRoot)).CA_UQ)));
			this.ComponentGrid.CaptionVisible = false;
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8E358FE8-91AD-4FE8-B8A4-A7636F82E0F1", "Name", "Name", "Name", "");
			zMultiControlColumnStyleInfo1.ColumnName = "CA_Name";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CA_NameFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("120ff1b7-30ab-4ce4-abb4-3fd11ac3a9c5", "Activity", "Activity", "Activity", "");
			zCalcEditColumnStyleInfo1.ColumnName = "CA_Qty";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("acb5d94b-4ec3-4d65-b8d1-05b17e1d2e72", "UQ", "UQ", "UQ", "");
			zDropEditColumnStyleInfo2.ColumnName = "CA_UQ";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ComponentGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ComponentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComponentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ComponentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentGrid.GridId = "5b503d8f-cac6-42e9-ab7a-3c7bb6e6a3d9";
			this.ComponentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComponentGrid.LayoutKey = "ComponentGrid";
			this.ComponentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ComponentGrid.Name = "ComponentGrid";
			this.ComponentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 215, true);
			this.ComponentGrid.TabIndex = 0;
			// 
			// LPCOGroupBox
			// 
			this.LPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOGroupBox.Name = "LPCOGroupBox";
			this.LPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 234, true);
			this.LPCOGroupBox.TabIndex = 1;
			this.LPCOGroupBox.TabStop = false;
			this.LPCOGroupBox.Text = "LPCOs";
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.CNSCPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 215, true);
			this.LPCOGridUserControl.TabIndex = 1;
			// 
			// CNSCUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 400, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CNSCSplitContainer);
			this.Name = "CNSCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CNSCSplitContainer.Panel1.ResumeLayout(false);
			this.CNSCSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CNSCSplitContainer)).EndInit();
			this.CNSCSplitContainer.ResumeLayout(false);
			this.CNSCSplitContainer.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.TotalQuantityCalcDropEdit.ResumeLayout(true);
			this.TotalQuantityCalcDropEdit.PerformLayout();
			this.PackMarksTextBox.ResumeLayout(true);
			this.PackMarksTextBox.PerformLayout();
			this.PackQtyCalcEdit.ResumeLayout(true);
			this.PackQtyCalcEdit.PerformLayout();
			this.DeliveredPartyAddressControl.ResumeLayout(true);
			this.DeliveredPartyAddressControl.PerformLayout();
			this.UNDGGuidFindBox.ResumeLayout(true);
			this.UNDGGuidFindBox.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.BottomSplitContainer.Panel1.ResumeLayout(false);
			this.BottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).EndInit();
			this.BottomSplitContainer.ResumeLayout(false);
			this.BottomSplitContainer.PerformLayout();
			this.ComponentGroupBox.ResumeLayout(false);
			this.ComponentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentGrid)).EndInit();
			this.ComponentGrid.ResumeLayout(false);
			this.ComponentGrid.PerformLayout();
			this.LPCOGroupBox.ResumeLayout(false);
			this.LPCOGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGuidFindBox UNDGGuidFindBox;
		private CargoWise.Windows.UI.KSplitContainer CNSCSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ComponentGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LPCOGroupBox;
		private Enterprise.ZArchitecture.ZGrid ComponentGrid;
		private ZAddressControl DeliveredPartyAddressControl;
		private ZCalcDropEdit PackQtyCalcEdit;
		private LongTextControl PackMarksTextBox;
		private ZIntEdit CountIntEdit;
		private ZDropEdit CategoryDropEdit;
		private ZTextBox NNIECRTextBox;
		internal LPCOGridUserControl LPCOGridUserControl;
		private CargoWise.Windows.UI.KSplitContainer BottomSplitContainer;
		private ZCalcDropEdit TotalQuantityCalcDropEdit;
	}
}
