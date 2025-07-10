using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	partial class EntryInstructionDetailsPanelUserControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UCRNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IdentifiersUserControl = new Enterprise.Customs.MX.GUI.IdentifiersUserControl();
			this.AditionalInformationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.IdentifiersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClearanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClearanceUserControl = new Enterprise.Customs.MX.GUI.ClearanceUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.IdentifiersUserControl.SuspendLayout();
			this.AditionalInformationTabControl.SuspendLayout();
			this.IdentifiersTabPage.SuspendLayout();
			this.ClearanceTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.CusEntryInstruction);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("2BF1E58B-D4DB-4813-B56C-98DF7505850A", "Details");
			this.DetailsGroupBox.Controls.Add(this.UCRNumberTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 50, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// UCRNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRNumberTextBox, "UCRNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).UCRNumber)));
			this.UCRNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 20, true);
			this.UCRNumberTextBox.Name = "UCRNumberTextBox";
			this.UCRNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
			this.UCRNumberTextBox.TabIndex = 1;
			// 
			// identifiersUserControl1
			// 
			this.IdentifiersUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IdentifiersUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.IdentifiersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IdentifiersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IdentifiersUserControl.Name = "identifiersUserControl1";
			this.IdentifiersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 392, true);
			this.IdentifiersUserControl.TabIndex = 1;
			// 
			// zTabControl
			// 
			this.AditionalInformationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AditionalInformationTabControl.Controls.Add(this.IdentifiersTabPage);
			this.AditionalInformationTabControl.Controls.Add(this.ClearanceTabPage);
			this.AditionalInformationTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AditionalInformationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.AditionalInformationTabControl.Name = "zTabControl";
			this.AditionalInformationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 417, true);
			this.AditionalInformationTabControl.TabIndex = 2;
			// 
			// IdentifiersTabPage
			// 
			this.IdentifiersTabPage.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("03CA3C46-0257-4958-BF37-2F9CA1B62FC3", "Identifiers");
			this.IdentifiersTabPage.Controls.Add(this.IdentifiersUserControl);
			this.IdentifiersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 17, true);
			this.IdentifiersTabPage.Name = "IdentifiersTabPage";
			this.IdentifiersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IdentifiersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1141, 397, true);
			this.IdentifiersTabPage.TabIndex = 0;
			this.IdentifiersTabPage.UseVisualStyleBackColor = true;
			// 
			// ClearanceTabPage
			// 
			this.ClearanceTabPage.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("E2AF9D68-7FCC-4B1B-9F2E-0936CF7C9B03", "Clearance");
			this.ClearanceTabPage.Controls.Add(this.ClearanceUserControl);
			this.ClearanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 17, true);
			this.ClearanceTabPage.Name = "ClearanceTabPage";
			this.ClearanceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ClearanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1141, 397, true);
			this.ClearanceTabPage.TabIndex = 1;
			this.ClearanceTabPage.UseVisualStyleBackColor = true;
			// 
			// clearanceUserControl1
			// 
			this.ClearanceUserControl.AllowDrop = true;
			this.ClearanceUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClearanceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ClearanceUserControl.Name = "clearanceUserControl";
			this.ClearanceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 392, true);
			this.ClearanceUserControl.TabIndex = 0;
			// 
			// EntryInstructionDetailsPanelUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AditionalInformationTabControl);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "EntryInstructionDetailsPanelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 485, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.IdentifiersUserControl.ResumeLayout(true);
			this.IdentifiersUserControl.PerformLayout();
			this.AditionalInformationTabControl.ResumeLayout(false);
			this.AditionalInformationTabControl.PerformLayout();
			this.IdentifiersTabPage.ResumeLayout(false);
			this.IdentifiersTabPage.PerformLayout();
			this.ClearanceTabPage.ResumeLayout(false);
			this.ClearanceTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZGroupBox DetailsGroupBox;
		internal ZArchitecture.ZTextBox UCRNumberTextBox;
		private IdentifiersUserControl IdentifiersUserControl;
		private ZTabControl AditionalInformationTabControl;
		private ZTabPage IdentifiersTabPage;
		private ZTabPage ClearanceTabPage;
		private System.ComponentModel.IContainer components;
		private ClearanceUserControl ClearanceUserControl;
	}
}
