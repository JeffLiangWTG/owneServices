
namespace Enterprise.Customs.GB.GUI.Registry
{
	partial class CredentialsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CredentialsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CredentialsGrid)).BeginInit();
			this.CredentialsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Registry.CredentialsSetting);
			// 
			// CredentialsGrid
			// 
			this.CredentialsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CredentialsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).BadgeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).ExistingBadges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).Printer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).Username)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).Password)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).FallbackForShed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).PreferredAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).CcsukFallbackAgentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).WebServiceFailureCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).IsMaritimeLoader)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).DataTestStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).ReceiverID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.CredentialsSetting)(null)).SenderID)));
			this.CredentialsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "ExistingBadges";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ef02a94b-07b3-440d-8525-1ad41164f37b", "Badge Code (mnemonic)");
			zDropEditColumnStyleInfo1.ColumnName = "BadgeCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("89dec7aa-3f76-4f98-b22d-5a0157950eaa", "Badge/Company or NES Role");
			zTextBoxColumnStyleInfo1.ColumnName = "Company";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(164);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8dedcfa1-95e0-41c1-894b-e2404e1f636d", "Output Device/NES Location/PIMA/CDS Topic");
			zTextBoxColumnStyleInfo2.ColumnName = "Printer";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(189);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("67cbc7b5-8359-476c-ae10-e3baa86c2dfa", "Username");
			zTextBoxColumnStyleInfo3.ColumnName = "Username";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("806467cb-2b8f-4095-b355-2fcd5a9656d0", "Password");
			zTextBoxColumnStyleInfo4.ColumnName = "Password";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("91d88ad3-635e-4cf3-917d-7eadf887fdd1", "Fallback Shed", "Agent is fallback for this shed");
			zTextBoxColumnStyleInfo5.ColumnName = "FallbackForShed";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c8986759-f6f5-4427-8287-2470bddb0467", "Preferred Agent", "Badge code of shed\'s preferred agent. This agent will be automatically nominated by default. ");
			zTextBoxColumnStyleInfo6.ColumnName = "PreferredAgent";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("85574598-6767-48c1-99ea-3ea06eb4d52c", "Fallback Type", "Export fallback agent type");
			zDropEditColumnStyleInfo2.ColumnName = "CcsukFallbackAgentType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("56e1e833-bc3c-4c34-84e7-dc2e40e6131d", "Fails", "Login Failure Count", "Failures", "The number of sequential login failures for this credential.");
			zCalcEditColumnStyleInfo1.ColumnName = "WebServiceFailureCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5d4bf88d-4bc0-48b0-a4b2-6a75922b1a2b", "Loader", "Is Maritime Loader (WTG only)");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsMaritimeLoader";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7068e752-3829-433e-a79c-e9c44554e9ea", "Test State");
			zDropEditColumnStyleInfo3.ColumnName = "DataTestStatus";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ReceiverID";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("12345678-3829-433e-a79c-e9c44554e9ea", "Receiver ID");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "SenderID";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("87654321-3829-433e-a79c-e9c44554e9ea", "Sender ID");
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CredentialsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CredentialsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CredentialsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CredentialsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CredentialsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CredentialsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialsGrid.GridId = "a34d018f-b165-4d4e-ad70-78242e6bb418";
			this.CredentialsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CredentialsGrid.LayoutKey = "CredentialsGrid";
			this.CredentialsGrid.LimitedColumns = null;
			this.CredentialsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialsGrid.Name = "CredentialsGrid";
			this.CredentialsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.CredentialsGrid.TabIndex = 0;
			// 
			// CredentialsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialsGrid);
			this.Name = "CredentialsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CredentialsGrid)).EndInit();
			this.CredentialsGrid.ResumeLayout(false);
			this.CredentialsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public Enterprise.ZArchitecture.ZGrid CredentialsGrid;


		#endregion

	}
}
