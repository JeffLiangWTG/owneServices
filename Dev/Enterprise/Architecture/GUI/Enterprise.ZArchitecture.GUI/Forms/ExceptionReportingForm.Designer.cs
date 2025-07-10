using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ExceptionReportingForm
	{
		CargoWise.Windows.UI.KLabel ContactNominatedCallerHeaderLabel;
		CargoWise.Windows.UI.KLabel ContactNominatedCallerDetail1Label;
		CargoWise.Windows.UI.KLabel ContactNominatedCallerDetail2Label;
		CargoWise.Windows.UI.KLabel ContactNominatedCallerDetail3Label;
		CargoWise.Windows.UI.KButton BreakButton;
		Enterprise.ZArchitecture.GUI.ZPictureBox ProductLogoPictureBox;
		CargoWise.Windows.UI.KTextBox FullDetailsTextBox;
		CargoWise.Windows.UI.KPanel panel1;
		CargoWise.Windows.UI.KLabel label1;
		CargoWise.Windows.UI.KLabel ErrorIDLabel;
		CargoWise.Windows.UI.KLabel LostInformationLabel;
		CargoWise.Windows.UI.KButton ToggleFullDetailsButton;
		CargoWise.Windows.UI.KButton SendButton;
		CargoWise.Windows.UI.KTextBox ErrorDescriptionTextBox;
		CargoWise.Windows.UI.KCheckBox ShutdownEnterpriseCheckEdit;
#if DEBUG
		internal CargoWise.Windows.UI.KCheckBox ShutdownEnterpriseCheckEditForTest => ShutdownEnterpriseCheckEdit;
		internal CargoWise.Windows.UI.KTextBox ErrorDescriptionTextBoxForTest => ErrorDescriptionTextBox;
		internal CargoWise.Windows.UI.KTextBox FullDetailsTextBoxForTest => FullDetailsTextBox;
		internal CargoWise.Windows.UI.KLabel ErrorIDLabelForTest => ErrorIDLabel;
		internal CargoWise.Windows.UI.KButton SendButtonForTest => SendButton;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox ProductLogoPictureBoxForTest => ProductLogoPictureBox;
#endif
		void InitializeComponent()
		{
			this.FullDetailsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.ProductLogoPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.ErrorIDLabel = new CargoWise.Windows.UI.KLabel();
			this.LostInformationLabel = new CargoWise.Windows.UI.KLabel();
			this.ToggleFullDetailsButton = new CargoWise.Windows.UI.KButton();
			this.SendButton = new CargoWise.Windows.UI.KButton();
			this.ErrorDescriptionTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ShutdownEnterpriseCheckEdit = new CargoWise.Windows.UI.KCheckBox();
			this.BreakButton = new CargoWise.Windows.UI.KButton();
			this.ContactNominatedCallerHeaderLabel = new CargoWise.Windows.UI.KLabel();
			this.ContactNominatedCallerDetail1Label = new CargoWise.Windows.UI.KLabel();
			this.ContactNominatedCallerDetail2Label = new CargoWise.Windows.UI.KLabel();
			this.ContactNominatedCallerDetail3Label = new CargoWise.Windows.UI.KLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductLogoPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// FullDetailsTextBox
			// 
			this.FullDetailsTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.FullDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 396, true);
			this.FullDetailsTextBox.Multiline = true;
			this.FullDetailsTextBox.Name = "FullDetailsTextBox";
			this.FullDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FullDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 3, true);
			this.FullDetailsTextBox.TabIndex = 10;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.ProductLogoPictureBox);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 123, true);
			this.panel1.TabIndex = 0;
			// 
			// ProductLogoPictureBox
			// 
			this.ProductLogoPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.ProductLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ProductLogoPictureBox.Name = "ProductLogoPictureBox";
			this.ProductLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 62, true);
			this.ProductLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.ProductLogoPictureBox.TabIndex = 10;
			this.ProductLogoPictureBox.TabStop = false;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 82, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 41, true);
			this.label1.TabIndex = 1;
			this.label1.Text = Res.GetString("000B2E71-B442-4342-B699-AE1A4159BBAB", "The application has encountered a problem and may need to close. Sorry for the inconvenience.");
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ErrorIDLabel
			// 
			this.ErrorIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.ErrorIDLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 312, true);
			this.ErrorIDLabel.Name = "ErrorIDLabel";
			this.ErrorIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 16, true);
			this.ErrorIDLabel.TabIndex = 4;
			this.ErrorIDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ErrorIDLabel.Text = Res.GetString("B25F0651-B92A-465F-BFD2-FF3FA6C8EDDD", "Error ID:");
			// 
			// LostInformationLabel
			// 
			this.LostInformationLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.LostInformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 136, true);
			this.LostInformationLabel.Name = "LostInformationLabel";
			this.LostInformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 16, true);
			this.LostInformationLabel.TabIndex = 1;
			this.LostInformationLabel.Text = Res.GetString("26271EEC-1B6E-444F-9148-277C8D082F72", "If you were in the middle of something, the information you were working on might be lost.");
			// 
			// ToggleFullDetailsButton
			// 
			this.ToggleFullDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 362, true);
			this.ToggleFullDetailsButton.Name = "ToggleFullDetailsButton";
			this.ToggleFullDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.ToggleFullDetailsButton.TabIndex = 7;
			this.ToggleFullDetailsButton.Text = Res.GetString("D56F5883-6F80-4FE1-AF9A-59D050723CEF", "Show &Error Details");
			this.ToggleFullDetailsButton.Visible = false;
			this.ToggleFullDetailsButton.Click += new System.EventHandler(this.ToggleFullDetailsButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 362, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 23, true);
			this.SendButton.TabIndex = 9;
			this.SendButton.Text = Res.GetString("0F123480-EBEA-4EB0-94F2-A6DDB802B660", "&Send Error Report");
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ErrorDescriptionTextBox
			// 
			this.ErrorDescriptionTextBox.AcceptsReturn = true;
			this.ErrorDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.ErrorDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 240, true);
			this.ErrorDescriptionTextBox.Multiline = true;
			this.ErrorDescriptionTextBox.Name = "ErrorDescriptionTextBox";
			this.ErrorDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ErrorDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 64, true);
			this.ErrorDescriptionTextBox.TabIndex = 3;
			// 
			// ShutdownEnterpriseCheckEdit
			// 
			this.ShutdownEnterpriseCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShutdownEnterpriseCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 330, true);
			this.ShutdownEnterpriseCheckEdit.Name = "ShutdownEnterpriseCheckEdit";
			this.ShutdownEnterpriseCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 24, true);
			this.ShutdownEnterpriseCheckEdit.TabIndex = 4;
			this.ShutdownEnterpriseCheckEdit.Text = Res.GetString("50AFD9B2-BC3B-48E2-968E-78FCA2444E07", "E&xit the application");
			// 
			// BreakButton
			// 
			this.BreakButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 362, true);
			this.BreakButton.Name = "BreakButton";
			this.BreakButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.BreakButton.TabIndex = 7;
			this.BreakButton.Text = Res.GetString("7335809D-769B-41AC-83B4-F712B02FCCB1", "Break Into Debugger");
			this.BreakButton.Visible = false;
			this.BreakButton.Click += new System.EventHandler(this.BreakButton_Click);
			// 
			// ContactNominatedCallerHeaderLabel
			// 
			this.ContactNominatedCallerHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.ContactNominatedCallerHeaderLabel.ForeColor = System.Drawing.Color.Red;
			this.ContactNominatedCallerHeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 159, true);
			this.ContactNominatedCallerHeaderLabel.Name = "ContactNominatedCallerHeaderLabel";
			this.ContactNominatedCallerHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 16, true);
			this.ContactNominatedCallerHeaderLabel.TabIndex = 13;
			this.ContactNominatedCallerHeaderLabel.Text = Res.GetString("491EA4A0-2860-4C54-BFD1-A2B9C154511D", @"If the error is preventing you from performing critical business functions, you should:");
			// 
			// ContactNominatedCallerDetail1Label
			// 
			this.ContactNominatedCallerDetail1Label.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																								  | System.Windows.Forms.AnchorStyles.Right);
			this.ContactNominatedCallerDetail1Label.ForeColor = System.Drawing.Color.Red;
			this.ContactNominatedCallerDetail1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 179, true);
			this.ContactNominatedCallerDetail1Label.Name = "ContactNominatedCallerDetail1Label";
			this.ContactNominatedCallerDetail1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 16, true);
			this.ContactNominatedCallerDetail1Label.TabIndex = 14;
			this.ContactNominatedCallerDetail1Label.Text = Res.GetString("D53F833E-B494-40F1-8742-CF4B44FB325B", @"1. Please provide the steps in the free text box that caused the exception to occur.");
			// 
			// ContactNominatedCallerDetail2Label
			// 
			this.ContactNominatedCallerHeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																								  | System.Windows.Forms.AnchorStyles.Right);
			this.ContactNominatedCallerDetail2Label.ForeColor = System.Drawing.Color.Red;
			this.ContactNominatedCallerDetail2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 199, true);
			this.ContactNominatedCallerDetail2Label.Name = "ContactNominatedCallerDetail2Label";
			this.ContactNominatedCallerDetail2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 16, true);
			this.ContactNominatedCallerDetail2Label.TabIndex = 15;
			this.ContactNominatedCallerDetail2Label.Text = Res.GetString("D9C95BE2-9473-45DB-9962-AE1D86A88866", @"2. Take note of the Error ID number below and submit the error details by clicking Send Error Report.");
			// 
			// ContactNominatedCallerDetail3Label
			// 
			this.ContactNominatedCallerDetail3Label.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																								  | System.Windows.Forms.AnchorStyles.Right);
			this.ContactNominatedCallerDetail3Label.ForeColor = System.Drawing.Color.Red;
			this.ContactNominatedCallerDetail3Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 219, true);
			this.ContactNominatedCallerDetail3Label.Name = "ContactNominatedCallerDetail3Label";
			this.ContactNominatedCallerDetail3Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 16, true);
			this.ContactNominatedCallerDetail3Label.TabIndex = 16;
			this.ContactNominatedCallerDetail3Label.Text = Res.GetString("6545A195-8C45-4517-8FF1-E6B3DC5A6170", @"3. Log an Incident Request and include the screen shot and Error ID number.");

			// 
			// ExceptionReportingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 400, true);
			this.Controls.Add(this.ContactNominatedCallerHeaderLabel);
			this.Controls.Add(this.ContactNominatedCallerDetail1Label);
			this.Controls.Add(this.ContactNominatedCallerDetail2Label);
			this.Controls.Add(this.ContactNominatedCallerDetail3Label);
			this.Controls.Add(this.ShutdownEnterpriseCheckEdit);
			this.Controls.Add(this.ErrorDescriptionTextBox);
			this.Controls.Add(this.FullDetailsTextBox);
			this.Controls.Add(this.ErrorIDLabel);
			this.Controls.Add(this.LostInformationLabel);
			this.Controls.Add(this.ToggleFullDetailsButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.BreakButton);
			this.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExceptionReportingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductLogoPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
