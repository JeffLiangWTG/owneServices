namespace Enterprise.Accounting.Registry.GUI
{
	partial class CrossTradeDebtorConfigurationControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.ConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CrossTradeDebtorConfigGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ConfigurationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CrossTradeDebtorConfigGrid)).BeginInit();
            this.CrossTradeDebtorConfigGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader);
            // 
            // ConfigurationGroupBox
            // 
            this.ConfigurationGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("92b8ec44-6907-4acb-86dc-7741c3d7606f", "Cross Trade Debtor Defaulting Configuration");
            this.ConfigurationGroupBox.Controls.Add(this.CrossTradeDebtorConfigGrid);
            this.ConfigurationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
            this.ConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 374, true);
            this.ConfigurationGroupBox.TabIndex = 7;
            this.ConfigurationGroupBox.TabStop = false;
            // 
            // CrossTradeDebtorConfigGrid
            // 
            this.CrossTradeDebtorConfigGrid.AllowNavigation = false;
            this.CrossTradeDebtorConfigGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CrossTradeDebtorConfigGrid, "Configurations");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader)(null)).Configurations)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.CrossTradeDebtorConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader)(null)).Configurations)).SyncRoot)).JobType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.CrossTradeDebtorConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader)(null)).Configurations)).SyncRoot)).DirectionCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.CrossTradeDebtorConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader)(null)).Configurations)).SyncRoot)).Mode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.CrossTradeDebtorConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader)(null)).Configurations)).SyncRoot)).ChargePaymentType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.CrossTradeDebtorConfiguration)(((System.Collections.IList)(((Enterprise.Accounting.Business.CrossTradeDebtorConfigurationHeader)(null)).Configurations)).SyncRoot)).Debtor)));
            this.CrossTradeDebtorConfigGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7121582a-81ed-4f94-a795-7d2358f0958a", "Job Type");
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "JobType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5a977a9d-2027-4cf9-a12d-e9974e0162ae", "Direction");
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cb519a05-6b1f-4d5f-ab39-a296ba935e08", "Mode");
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "Mode";
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cb5f15dc-978d-4d0d-b1af-2d369c196ad1", "Charges Prepaid/Collect", "Charges Prepaid/Collect", "Charges Prepaid/Collect", "");
            zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo4.ColumnName = "ChargePaymentType";
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8470cd92-a6c0-409e-9eef-5381b022c4fe", "Default Debtor");
            zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            zDropEditColumnStyleInfo5.ColumnName = "Debtor";
            zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            this.CrossTradeDebtorConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.CrossTradeDebtorConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.CrossTradeDebtorConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.CrossTradeDebtorConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.CrossTradeDebtorConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
            this.CrossTradeDebtorConfigGrid.GridId = "911A930D-F66C-4AE7-AF95-FA0B8BF3CCFB";
            this.CrossTradeDebtorConfigGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CrossTradeDebtorConfigGrid.LayoutKey = "RevenueRecognitionGrid";
            this.CrossTradeDebtorConfigGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
            this.CrossTradeDebtorConfigGrid.Name = "CrossTradeDebtorConfigGrid";
            this.CrossTradeDebtorConfigGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 323, true);
            this.CrossTradeDebtorConfigGrid.TabIndex = 5;
            // 
            // CrossTradeDebtorConfigurationControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ConfigurationGroupBox);
            this.Name = "CrossTradeDebtorConfigurationControl";
            this.ShouldSerializeTabPageMethods = false;
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 374, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ConfigurationGroupBox.ResumeLayout(false);
            this.ConfigurationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CrossTradeDebtorConfigGrid)).EndInit();
            this.CrossTradeDebtorConfigGrid.ResumeLayout(false);
            this.CrossTradeDebtorConfigGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ConfigurationGroupBox;
		private ZArchitecture.ZGrid CrossTradeDebtorConfigGrid;
	}
}
