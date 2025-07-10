namespace Enterprise.ServiceManager.Tasks.FTP
{
	public partial class FtpConfigControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProfilesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProfilesGrid)).BeginInit();
			this.ProfilesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Tasks.FTP.FtpProfile);
			// 
			// ProfilesGrid
			// 
			this.ProfilesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProfilesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).FriendlyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).PushOrPull)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).ClobberOrMakeUnique)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).DeleteSourceOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).LocalFolder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).FindFileMask)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).RemoteLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).RemoteUsername)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).RemotePassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).RunPeriodSeconds)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).LastRunUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Tasks.FTP.FtpProfile)(null)).EmailAlertOnFailure)));
			this.ProfilesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("705e7302-baab-4f38-b2bb-be50070f253e", "Friendly Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FriendlyName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("ab49cdf9-5f40-498e-90ad-8bbc16cd5e30", "Direction");
			zDropEditColumnStyleInfo1.ColumnName = "PushOrPull";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("d729f820-274e-4f87-af3f-fdcbcbbff2fd", "Uniqueness Option");
			zDropEditColumnStyleInfo2.ColumnName = "ClobberOrMakeUnique";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("de29318c-1c62-4387-8270-c10cb30fc102", "Success Action");
			zDropEditColumnStyleInfo3.ColumnName = "DeleteSourceOption";
			zTextBoxColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("c545e033-b2ac-4742-aa57-eb71438d5a4b", "Local Folder");
			zTextBoxColumnStyleInfo2.ColumnName = "LocalFolder";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("7c2008a7-754b-4773-833e-a375742404ab", "File Mask");
			zTextBoxColumnStyleInfo3.ColumnName = "FindFileMask";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("5d6c3ce5-cf11-4900-b42a-3defe2f2a17d", "Remote Folder", "URI to remote folder, e.g. {0}").Format("ftp://server.com/path");
			zTextBoxColumnStyleInfo4.ColumnName = "RemoteLocation";
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("54e62a56-2e65-4fb1-8239-cc5f0c40e111", "Username");
			zTextBoxColumnStyleInfo5.ColumnName = "RemoteUsername";
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("79df98b9-6c5f-4849-bb54-e55c18ebd11d", "Password");
			zTextBoxColumnStyleInfo6.ColumnName = "RemotePassword";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("9956b9c8-5053-4eef-9b37-989ff928e518", "Run Period", "Run Period in Seconds");
			zCalcEditColumnStyleInfo1.ColumnName = "RunPeriodSeconds";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("bb538736-d193-4563-b72e-50c5dc53ebc1", "Last Run Time");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "LastRunUtc";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.Res.GetData("f9961c2a-9145-4723-8cab-3be24d5e1583", "Email", "Warning Email Address", "Warning Email", "Email address to which to send errors");
			zTextBoxColumnStyleInfo7.ColumnName = "EmailAlertOnFailure";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProfilesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProfilesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ProfilesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ProfilesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProfilesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ProfilesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ProfilesGrid.CopySelectedRowsAllowed = true;
			this.ProfilesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProfilesGrid.GridId = "6ee3d870-fb78-4bc6-b43e-e6f6cf968420";
			this.ProfilesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProfilesGrid.LayoutKey = "ProfilesGrid";
			this.ProfilesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProfilesGrid.Name = "ProfilesGrid";
			this.ProfilesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 173, true);
			this.ProfilesGrid.TabIndex = 0;
			// 
			// FtpConfigControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProfilesGrid);
			this.Name = "FtpConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 173, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProfilesGrid)).EndInit();
			this.ProfilesGrid.ResumeLayout(false);
			this.ProfilesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ProfilesGrid;
	}
}
