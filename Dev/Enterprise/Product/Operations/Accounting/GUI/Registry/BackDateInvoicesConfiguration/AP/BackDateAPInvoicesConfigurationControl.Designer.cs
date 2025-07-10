using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BackDateAPInvoicesConfigurationControl
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
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PostDateConfigurationGrid = new ZArchitecture.ZGrid();
			this.PostDateGroupBox = new ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PostDateConfigurationGrid)).BeginInit();
			this.PostDateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.BackDateAPInvoicesConfiguration);
			// 
			// PostDateConfigurationGrid
			// 
			this.PostDateConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PostDateConfigurationGrid, "PostDateConfigurationCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).BrokerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).SignificantDateCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).PriorClosedPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).PriorOpenPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).CurrentPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).FuturePeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PostDateConfiguration)(((System.Collections.IList)(((Business.BackDateAPInvoicesConfiguration)(null)).PostDateConfigurationCollection)).SyncRoot)).ReversalRule)));
			this.PostDateConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|d163ca8b-22e0-4235-836a-a6df4fbae90d", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|30931842-1e5e-41c9-a5ad-0b703b656753", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|a526d2a6-fc33-4d45-952b-d9412e37fa3c", "Mode");
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo4.Caption = null;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|48f846fe-a45a-4e87-9d91-4d89a7273500", "Broker");
			zDropEditColumnStyleInfo4.ColumnName = "BrokerCode";
			zDropEditColumnStyleInfo5.Caption = null;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|dae34fe0-0942-48bb-a0e7-3769ad802198", "Significant Date");
			zDropEditColumnStyleInfo5.ColumnName = "SignificantDateCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zDropEditColumnStyleInfo6.Caption = null;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|2e732178-d03d-49b7-b989-0913ea08c600", "Prior Closed");
			zDropEditColumnStyleInfo6.ColumnName = "PriorClosedPeriod";
			zDropEditColumnStyleInfo7.Caption = null;
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|9fa2ff03-6b6b-417d-90d0-9f81df090426", "Prior Open");
			zDropEditColumnStyleInfo7.ColumnName = "PriorOpenPeriod";
			zDropEditColumnStyleInfo8.Caption = null;
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|dfc867b6-cc9a-4a34-921a-9ba17d4afa11", "Current Period");
			zDropEditColumnStyleInfo8.ColumnName = "CurrentPeriod";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zDropEditColumnStyleInfo9.Caption = null;
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|eed3ab20-4237-4725-9557-f085669317c0", "Future Period");
			zDropEditColumnStyleInfo9.ColumnName = "FuturePeriod";
			zDropEditColumnStyleInfo10.Caption = null;
			zDropEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e3cd474-32ad-42c6-bc99-9360312d96ce", "Reversal Rule");
			zDropEditColumnStyleInfo10.ColumnName = "ReversalRule";
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.PostDateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.PostDateConfigurationGrid.CopySelectedRowsAllowed = true;
			this.PostDateConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PostDateConfigurationGrid.GridId = "C161F2EA-6149-46D1-BFDC-A2AF1AFDC554";
			this.PostDateConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PostDateConfigurationGrid.LayoutKey = "PostDateConfigurationGrid";
			this.PostDateConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PostDateConfigurationGrid.Name = "PostDateConfigurationGrid";
			this.PostDateConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 276, true);
			this.PostDateConfigurationGrid.TabIndex = 6;
			// 
			// PostDateGroupBox
			// 
			this.PostDateGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDateAPInvoicesConfigurationControl|34c95966-bff6-4abc-b775-4acb1aa7f999", "Post Date Configuration");
			this.PostDateGroupBox.Controls.Add(this.PostDateConfigurationGrid);
			this.PostDateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PostDateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PostDateGroupBox.Name = "PostDateGroupBox";
			this.PostDateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			this.PostDateGroupBox.TabIndex = 7;
			this.PostDateGroupBox.TabStop = false;
			// 
			// BackDateAPInvoicesConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PostDateGroupBox);
			this.Name = "BackDateAPInvoicesConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PostDateConfigurationGrid)).EndInit();
			this.PostDateGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}