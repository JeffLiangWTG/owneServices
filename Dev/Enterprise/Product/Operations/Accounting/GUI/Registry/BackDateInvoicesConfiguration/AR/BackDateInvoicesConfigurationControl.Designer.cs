using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BackDateInvoicesConfigurationControl
	{


		#region Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.OverridePostDateCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.DefaultPostDateFromInvoiceDateCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.InvoiceDateConfigurationGrid = new ZArchitecture.ZGrid();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PostDateGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.InvoiceDateGroupBox = new ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceDateConfigurationGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.PostDateGroupBox.SuspendLayout();
			this.InvoiceDateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.BackDateInvoicesConfiguration);
			// 
			// OverridePostDateCheckBox
			// 
			this.OverridePostDateCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OverridePostDateCheckBox, "OverridePostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.BackDateInvoicesConfiguration)(null)).OverridePostDate)));
			this.OverridePostDateCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|f90c3231-7156-4bdd-b616-79e7e6a59596", "Allow Post Date to be manually overridden on AR Invoices and AR Credit Notes posted from Jobs");
			this.OverridePostDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverridePostDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.OverridePostDateCheckBox.Name = "OverridePostDateCheckBox";
			this.OverridePostDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 32, true);
			this.OverridePostDateCheckBox.TabIndex = 2;
			this.OverridePostDateCheckBox.UseVisualStyleBackColor = true;
			// 
			// DefaultPostDateFromInvoiceDateCheckBox
			// 
			this.DefaultPostDateFromInvoiceDateCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DefaultPostDateFromInvoiceDateCheckBox, "DefaultPostDateFromInvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.BackDateInvoicesConfiguration)(null)).DefaultPostDateFromInvoiceDate)));
			this.DefaultPostDateFromInvoiceDateCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|86c44a3e-eb2b-48c1-97f7-d29ef1e840a6", "Default Post Date from Invoice Date when user has rights to manually override Invoice Date");
			this.DefaultPostDateFromInvoiceDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DefaultPostDateFromInvoiceDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 57, true);
			this.DefaultPostDateFromInvoiceDateCheckBox.Name = "DefaultPostDateFromInvoiceDateCheckBox";
			this.DefaultPostDateFromInvoiceDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 32, true);
			this.DefaultPostDateFromInvoiceDateCheckBox.TabIndex = 3;
			this.DefaultPostDateFromInvoiceDateCheckBox.UseVisualStyleBackColor = true;
			// 
			// InvoiceDateConfigurationGrid
			// 
			this.InvoiceDateConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceDateConfigurationGrid, "InvoiceDateConfigurationCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).BrokerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).SignificantDateCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).PriorClosedPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).PriorOpenPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).CurrentPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).FuturePeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).Override)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).Today)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceDateConfiguration)(((System.Collections.IList)(((Business.BackDateInvoicesConfiguration)(null)).InvoiceDateConfigurationCollection)).SyncRoot)).ReversalRule)));
			this.InvoiceDateConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|d163ca8b-22e0-4235-836a-a6df4fbae90d", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|30931842-1e5e-41c9-a5ad-0b703b656753", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|a526d2a6-fc33-4d45-952b-d9412e37fa3c", "Mode");
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo4.Caption = null;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|48f846fe-a45a-4e87-9d91-4d89a7273500", "Broker");
			zDropEditColumnStyleInfo4.ColumnName = "BrokerCode";
			zDropEditColumnStyleInfo5.Caption = null;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|dae34fe0-0942-48bb-a0e7-3769ad802198", "Significant Date");
			zDropEditColumnStyleInfo5.ColumnName = "SignificantDateCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zDropEditColumnStyleInfo6.Caption = null;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|2e732178-d03d-49b7-b989-0913ea08c600", "Prior Closed");
			zDropEditColumnStyleInfo6.ColumnName = "PriorClosedPeriod";
			zDropEditColumnStyleInfo7.Caption = null;
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|9fa2ff03-6b6b-417d-90d0-9f81df090426", "Prior Open");
			zDropEditColumnStyleInfo7.ColumnName = "PriorOpenPeriod";
			zDropEditColumnStyleInfo8.Caption = null;
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|dfc867b6-cc9a-4a34-921a-9ba17d4afa11", "Current Period");
			zDropEditColumnStyleInfo8.ColumnName = "CurrentPeriod";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zDropEditColumnStyleInfo9.Caption = null;
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|eed3ab20-4237-4725-9557-f085669317c0", "Future Period");
			zDropEditColumnStyleInfo9.ColumnName = "FuturePeriod";
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|72d0f3b3-4679-41cf-8cc2-04a0253f721d", "Override");
			zCheckBoxColumnStyleInfo1.ColumnName = "Override";
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|3cfe3965-ac1b-4ec8-86da-1b796ae449d3", "Today");
			zCheckBoxColumnStyleInfo2.ColumnName = "Today";
			zDropEditColumnStyleInfo10.Caption = null;
			zDropEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|63c38e8e-aa59-429b-b72f-6640153f5a8f", "Reversal Rule");
			zDropEditColumnStyleInfo10.ColumnName = "ReversalRule";
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.InvoiceDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.InvoiceDateConfigurationGrid.CopySelectedRowsAllowed = true;
			this.InvoiceDateConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceDateConfigurationGrid.GridId = "7d16b22f-90fe-4dd8-addd-d7e514efbe14";
			this.InvoiceDateConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceDateConfigurationGrid.LayoutKey = "InvoiceDateConfigurationGrid";
			this.InvoiceDateConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceDateConfigurationGrid.Name = "InvoiceDateConfigurationGrid";
			this.InvoiceDateConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 167, true);
			this.InvoiceDateConfigurationGrid.TabIndex = 4;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.PostDateGroupBox);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.InvoiceDateGroupBox);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.splitContainer.TabIndex = 5;
			// 
			// PostDateGroupBox
			// 
			this.PostDateGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|366b62c4-c75b-4dcb-91dc-cf4528ff3243", "Post Date Configuration");
			this.PostDateGroupBox.Controls.Add(this.DefaultPostDateFromInvoiceDateCheckBox);
			this.PostDateGroupBox.Controls.Add(this.OverridePostDateCheckBox);
			this.PostDateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PostDateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PostDateGroupBox.Name = "PostDateGroupBox";
			this.PostDateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 105, true);
			this.PostDateGroupBox.TabIndex = 4;
			this.PostDateGroupBox.TabStop = false;
			// 
			// InvoiceDateGroupBox
			// 
			this.InvoiceDateGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateInvoicesConfigurationControl|0267d8ca-f3e3-42e0-b27a-be31c8e6395d", "Invoice Date Configuration");
			this.InvoiceDateGroupBox.Controls.Add(this.InvoiceDateConfigurationGrid);
			this.InvoiceDateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceDateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDateGroupBox.Name = "InvoiceDateGroupBox";
			this.InvoiceDateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 186, true);
			this.InvoiceDateGroupBox.TabIndex = 5;
			this.InvoiceDateGroupBox.TabStop = false;
			// 
			// BackDateInvoicesConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "BackDateInvoicesConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceDateConfigurationGrid)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.PostDateGroupBox.ResumeLayout(false);
			this.InvoiceDateGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}