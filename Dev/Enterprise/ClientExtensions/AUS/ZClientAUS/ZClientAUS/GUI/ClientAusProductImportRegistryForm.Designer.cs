using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.GUI
{
	public partial class ClientAusProductImportRegistryForm : ZForm
	{
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterFindBox;
		Enterprise.ZArchitecture.ZTextBox AllProductsTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZLabel zLabel6;
		Enterprise.ZArchitecture.ZLabel zLabel7;
		Enterprise.ZArchitecture.ZLabel zLabel8;
		Enterprise.ZArchitecture.ZLabel zLabel9;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZTextBox ClientInvoicingTextBox;
		Enterprise.ZArchitecture.ZTextBox ProductUpdateTextBox;
		Enterprise.ZArchitecture.ZTextBox EmailAddressTextBox;
		Enterprise.ZArchitecture.ZTextBox BackupTextBox;
		Enterprise.ZArchitecture.ZTextBox DirToStoreTextBox;
		Enterprise.ZArchitecture.ZTextBox DirImportProductsTextBox;
		Enterprise.ZArchitecture.ZTextBox DirRejectedProductsTextBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox SupplierFindBox;
		Enterprise.ZArchitecture.GUI.ZButton BrowseStoreDirectoryButton;
		Enterprise.ZArchitecture.GUI.ZButton BrowseImportedDirectoryButton;
		Enterprise.ZArchitecture.GUI.ZButton BrowseRejectedDirectoryButton;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.ZLabel zLabel1;

		new void InitializeComponent()
		{
			this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.AllProductsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.SupplierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ClientInvoicingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProductUpdateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BackupTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirToStoreTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirImportProductsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirRejectedProductsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrowseStoreDirectoryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BrowseImportedDirectoryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BrowseRejectedDirectoryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 374, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(304);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(305);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry);
			// 
			// ImporterFindBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterFindBox, "T6_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_OH_Importer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).ImporterList)));
			this.ImporterFindBox.BindToList = "ImporterList";
			this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 24, true);
			this.ImporterFindBox.Name = "ImporterFindBox";
			this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ImporterFindBox.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Importer";
			// 
			// AllProductsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AllProductsTextBox, "T6_AllProductsFileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_AllProductsFileName)));
			this.AllProductsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AllProductsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 96, true);
			this.AllProductsTextBox.Name = "AllProductsTextBox";
			this.AllProductsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AllProductsTextBox.TabIndex = 3;
			this.AllProductsTextBox.Tag = "";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.Text = "Supplier";
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 96, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.zLabel3.TabIndex = 5;
			this.zLabel3.Text = "All Products File Name";
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 124, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.zLabel4.TabIndex = 6;
			this.zLabel4.Text = "Client Invoicing File Name";
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 152, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.zLabel5.TabIndex = 7;
			this.zLabel5.Text = "Product Update File Name";
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 180, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.zLabel6.TabIndex = 8;
			this.zLabel6.Text = "Email Address for Update File";
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 208, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.zLabel7.TabIndex = 9;
			this.zLabel7.Text = "Backup File Name";
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 236, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.zLabel8.TabIndex = 10;
			this.zLabel8.Text = "Directory to Store Files";
			// 
			// zLabel9
			// 
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 264, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.zLabel9.TabIndex = 11;
			this.zLabel9.Text = "Directory for Imported Products Log";
			// 
			// zLabel10
			// 
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 292, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.zLabel10.TabIndex = 12;
			this.zLabel10.Text = "Directory for Rejected Products Log";
			// 
			// SupplierFindBox
			// 
			this.BindingSource.SetBindingMember(this.SupplierFindBox, "T6_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).SupplierList)));
			this.SupplierFindBox.BindToList = "SupplierList";
			this.SupplierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 56, true);
			this.SupplierFindBox.Name = "SupplierFindBox";
			this.SupplierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SupplierFindBox.TabIndex = 2;
			// 
			// ClientInvoicingTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientInvoicingTextBox, "T6_ClientInvoicingFileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_ClientInvoicingFileName)));
			this.ClientInvoicingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientInvoicingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 124, true);
			this.ClientInvoicingTextBox.Name = "ClientInvoicingTextBox";
			this.ClientInvoicingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ClientInvoicingTextBox.TabIndex = 4;
			this.ClientInvoicingTextBox.Tag = "";
			// 
			// ProductUpdateTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProductUpdateTextBox, "T6_ProductUpdateFileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_ProductUpdateFileName)));
			this.ProductUpdateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProductUpdateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 152, true);
			this.ProductUpdateTextBox.Name = "ProductUpdateTextBox";
			this.ProductUpdateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ProductUpdateTextBox.TabIndex = 5;
			// 
			// EmailAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailAddressTextBox, "T6_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_Email)));
			this.EmailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 180, true);
			this.EmailAddressTextBox.Name = "EmailAddressTextBox";
			this.EmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.EmailAddressTextBox.TabIndex = 6;
			// 
			// BackupTextBox
			// 
			this.BindingSource.SetBindingMember(this.BackupTextBox, "T6_BackUpFileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_BackUpFileName)));
			this.BackupTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BackupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 208, true);
			this.BackupTextBox.Name = "BackupTextBox";
			this.BackupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.BackupTextBox.TabIndex = 7;
			// 
			// DirToStoreTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirToStoreTextBox, "T6_DirectoryToStoreFiles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_DirectoryToStoreFiles)));
			this.DirToStoreTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DirToStoreTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 236, true);
			this.DirToStoreTextBox.Name = "DirToStoreTextBox";
			this.DirToStoreTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.DirToStoreTextBox.TabIndex = 8;
			// 
			// DirImportProductsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirImportProductsTextBox, "T6_DirectoryImportedParts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_DirectoryImportedParts)));
			this.DirImportProductsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DirImportProductsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 264, true);
			this.DirImportProductsTextBox.Name = "DirImportProductsTextBox";
			this.DirImportProductsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.DirImportProductsTextBox.TabIndex = 10;
			// 
			// DirRejectedProductsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirRejectedProductsTextBox, "T6_DirectoryRejectedParts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry)(null)).T6_DirectoryRejectedParts)));
			this.DirRejectedProductsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DirRejectedProductsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 292, true);
			this.DirRejectedProductsTextBox.Name = "DirRejectedProductsTextBox";
			this.DirRejectedProductsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.DirRejectedProductsTextBox.TabIndex = 12;
			// 
			// BrowseStoreDirectoryButton
			// 
			this.BrowseStoreDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseStoreDirectoryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 235, true);
			this.BrowseStoreDirectoryButton.Name = "BrowseStoreDirectoryButton";
			this.BrowseStoreDirectoryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.BrowseStoreDirectoryButton.TabIndex = 9;
			this.BrowseStoreDirectoryButton.Text = "Browse...";
			this.BrowseStoreDirectoryButton.Click += new System.EventHandler(this.BrowseDataButton_Click);
			// 
			// BrowseImportedDirectoryButton
			// 
			this.BrowseImportedDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseImportedDirectoryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 263, true);
			this.BrowseImportedDirectoryButton.Name = "BrowseImportedDirectoryButton";
			this.BrowseImportedDirectoryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.BrowseImportedDirectoryButton.TabIndex = 11;
			this.BrowseImportedDirectoryButton.Text = "Browse...";
			this.BrowseImportedDirectoryButton.Click += new System.EventHandler(this.BrowseImportedDirectoryButton_Click);
			// 
			// BrowseRejectedDirectoryButton
			// 
			this.BrowseRejectedDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseRejectedDirectoryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 291, true);
			this.BrowseRejectedDirectoryButton.Name = "BrowseRejectedDirectoryButton";
			this.BrowseRejectedDirectoryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.BrowseRejectedDirectoryButton.TabIndex = 13;
			this.BrowseRejectedDirectoryButton.Text = "Browse...";
			this.BrowseRejectedDirectoryButton.Click += new System.EventHandler(this.BrowseRejectedDirectoryButton_Click);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 336, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsUserControl.TabIndex = 14;
			// 
			// ClientAusProductImportRegistryForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 398, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.BrowseRejectedDirectoryButton);
			this.Controls.Add(this.BrowseImportedDirectoryButton);
			this.Controls.Add(this.BrowseStoreDirectoryButton);
			this.Controls.Add(this.DirRejectedProductsTextBox);
			this.Controls.Add(this.DirImportProductsTextBox);
			this.Controls.Add(this.DirToStoreTextBox);
			this.Controls.Add(this.BackupTextBox);
			this.Controls.Add(this.EmailAddressTextBox);
			this.Controls.Add(this.ProductUpdateTextBox);
			this.Controls.Add(this.ClientInvoicingTextBox);
			this.Controls.Add(this.SupplierFindBox);
			this.Controls.Add(this.zLabel10);
			this.Controls.Add(this.zLabel9);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.zLabel7);
			this.Controls.Add(this.zLabel6);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.AllProductsTextBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ImporterFindBox);
			this.DataSourceAssemblyName = "ZClientAUS";
			this.DataSourceType = typeof(Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry);
			this.DataSourceTypeName = "Enterprise.Client.AUS.Business.ClientAUSProductImportRegistry";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 432, true);
			this.Name = "ClientAusProductImportRegistryForm";
			this.Controls.SetChildIndex(this.ImporterFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.AllProductsTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.zLabel6, 0);
			this.Controls.SetChildIndex(this.zLabel7, 0);
			this.Controls.SetChildIndex(this.zLabel8, 0);
			this.Controls.SetChildIndex(this.zLabel9, 0);
			this.Controls.SetChildIndex(this.zLabel10, 0);
			this.Controls.SetChildIndex(this.SupplierFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ClientInvoicingTextBox, 0);
			this.Controls.SetChildIndex(this.ProductUpdateTextBox, 0);
			this.Controls.SetChildIndex(this.EmailAddressTextBox, 0);
			this.Controls.SetChildIndex(this.BackupTextBox, 0);
			this.Controls.SetChildIndex(this.DirToStoreTextBox, 0);
			this.Controls.SetChildIndex(this.DirImportProductsTextBox, 0);
			this.Controls.SetChildIndex(this.DirRejectedProductsTextBox, 0);
			this.Controls.SetChildIndex(this.BrowseStoreDirectoryButton, 0);
			this.Controls.SetChildIndex(this.BrowseImportedDirectoryButton, 0);
			this.Controls.SetChildIndex(this.BrowseRejectedDirectoryButton, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
