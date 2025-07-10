using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Diagnostics
{
	public partial class DocumentSectionsForm : ZChildForm
	{
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		ZGrid zSectionGrid;
		ZGrid zDocumentGrid;
		ZGroupBox zGroupBoxSections;
		ZDropEdit CategoryDropEdit;
		ZGroupBox zGroupBoxDocuments;

		System.ComponentModel.Container components = null;

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

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBoxSections = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zSectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBoxDocuments = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBoxSections.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zSectionGrid)).BeginInit();
			this.zSectionGrid.SuspendLayout();
			this.zGroupBoxDocuments.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zDocumentGrid)).BeginInit();
			this.zDocumentGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 769, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Diagnostics.DocumentSections);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.zGroupBoxSections);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBoxDocuments);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 769, true);
			this.splitContainer1.SplitterDistance = 403;
			this.splitContainer1.TabIndex = 1;
			// 
			// zGroupBoxSections
			// 
			this.zGroupBoxSections.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("f0fcfdc0-bfd9-4756-8751-a4c4f710de1c", "Sections");
			this.zGroupBoxSections.Controls.Add(this.CategoryDropEdit);
			this.zGroupBoxSections.Controls.Add(this.zSectionGrid);
			this.zGroupBoxSections.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBoxSections.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBoxSections.Name = "zGroupBoxSections";
			this.zGroupBoxSections.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 403, true);
			this.zGroupBoxSections.TabIndex = 1;
			this.zGroupBoxSections.TabStop = false;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CategoryFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Diagnostics.DocumentSections)(null)).CategoryFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).Categories)));
			this.CategoryDropEdit.BindToList = "Categories";
			this.CategoryDropEdit.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("96e63b9f-19f5-4d84-9507-5c114c57943f", "Filter by Category");
			this.CategoryDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 22, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.PreBoundMaxLength = 15;
			this.CategoryDropEdit.ShowDescriptionBox = false;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.CategoryDropEdit.TabIndex = 1;
			TypeDescriptor.AddAttributes(CategoryDropEdit, new SuppressFormsLocalizedTestAttribute());

			// 
			// zSectionGrid
			// 
			this.zSectionGrid.AllowNavigation = false;
			this.zSectionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zSectionGrid, "AvailableSections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).SectionName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).TypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).IsOverridden)));
			this.zSectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("1d0219f7-7021-4e5f-a593-bd5abf68588b", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "SectionName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("f6b23397-4a2e-40c9-9a04-acca1e7606a6", "Category");
			zTextBoxColumnStyleInfo2.ColumnName = "Category";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("71061589-1ac0-4bde-97b2-54e1bf095c9f", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "TypeCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("3a893fa1-e249-4547-b41a-9992e7a3cd44", "Is Overridden");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsOverridden";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zSectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zSectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zSectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zSectionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zSectionGrid.CopySelectedRowsAllowed = true;
			this.zSectionGrid.GridId = "7e501239-b299-41e9-af60-0fffc624d1b5";
			this.zSectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zSectionGrid.LayoutKey = "zSectionGrid";
			this.zSectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 47, true);
			this.zSectionGrid.Name = "zSectionGrid";
			this.zSectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 350, true);
			this.zSectionGrid.TabIndex = 2;
			// 
			// zGroupBoxDocuments
			// 
			this.zGroupBoxDocuments.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("2c67a5e3-54da-491e-a197-491609a93d17", "Documents");
			this.zGroupBoxDocuments.Controls.Add(this.zDocumentGrid);
			this.zGroupBoxDocuments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBoxDocuments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBoxDocuments.Name = "zGroupBoxDocuments";
			this.zGroupBoxDocuments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 362, true);
			this.zGroupBoxDocuments.TabIndex = 1;
			this.zGroupBoxDocuments.TabStop = false;
			// 
			// zDocumentGrid
			// 
			this.zDocumentGrid.AllowNavigation = false;
			this.zDocumentGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDocumentGrid, "AvailableSections.Documents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).DocumentTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).Context)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).MenuItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).PrintOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).SectionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).FilterList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Diagnostics.Document)(((System.Collections.IList)(((Enterprise.Diagnostics.EXTemplateSection)(((System.Collections.IList)(((Enterprise.Diagnostics.DocumentSections)(null)).AvailableSections)).SyncRoot)).Documents)).SyncRoot)).IsSystemDefined)));
			this.zDocumentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("287cab44-c649-49c1-a03e-3d2cfb331767", "Title");
			zTextBoxColumnStyleInfo4.ColumnName = "DocumentTitle";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("9cd4f82a-a111-4095-a7e4-2d5d30db0950", "Context");
			zTextBoxColumnStyleInfo5.ColumnName = "Context";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("67f86f70-add9-4783-a2a5-549fa2eea0fd", "Menu Item Name");
			zTextBoxColumnStyleInfo6.ColumnName = "MenuItemName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("23d07c13-e86d-4fcd-9509-c12122f08efc", "Order");
			zTextBoxColumnStyleInfo7.ColumnName = "PrintOrder";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("68e53128-01cd-4bf4-8adb-dbf13db81044", "Type");
			zTextBoxColumnStyleInfo8.ColumnName = "SectionType";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("37195096-80a9-4459-a859-30fa59a09e73", "Filter");
			zTextBoxColumnStyleInfo9.ColumnName = "FilterList";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("22715cd9-3c0b-457e-84f6-6af9ca5f3a76", "Is System Defined");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsSystemDefined";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.zDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.zDocumentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.zDocumentGrid.CopySelectedRowsAllowed = true;
			this.zDocumentGrid.GridId = "60e47a13-511b-40c4-aba5-d0fa4eb2fb3b";
			this.zDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zDocumentGrid.LayoutKey = "zDocumentGrid";
			this.zDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.zDocumentGrid.Name = "zDocumentGrid";
			this.zDocumentGrid.ReadOnly = true;
			this.zDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 337, true);
			this.zDocumentGrid.TabIndex = 0;
			// 
			// DocumentSectionsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Main.DiagnosticsAndTesting.Res.GetData("669b50af-c0ec-4657-9ce0-4fe92c0abbfe", "Document Sections");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 791, true);
			this.Controls.Add(this.splitContainer1);
			this.DataSourceType = typeof(Enterprise.Diagnostics.DocumentSections);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "DocumentSectionsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.zGroupBoxSections.ResumeLayout(false);
			this.zGroupBoxSections.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zSectionGrid)).EndInit();
			this.zSectionGrid.ResumeLayout(false);
			this.zSectionGrid.PerformLayout();
			this.zGroupBoxDocuments.ResumeLayout(false);
			this.zGroupBoxDocuments.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zDocumentGrid)).EndInit();
			this.zDocumentGrid.ResumeLayout(false);
			this.zDocumentGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
