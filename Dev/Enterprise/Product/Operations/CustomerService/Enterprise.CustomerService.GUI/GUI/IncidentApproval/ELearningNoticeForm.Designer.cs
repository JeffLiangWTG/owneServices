namespace Enterprise.CustomerService.GUI
{
	partial class ELearningNoticeForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ELearningNoticeForm));
			this.eLearningPortalButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.raiseTrainingIncidentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.raiseTrainingIncidentLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.wiseLearningButton = new Enterprise.ZArchitecture.GUI.ZImageButton();
			this.eLearningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.eLearningCaption = new Enterprise.ZArchitecture.ZLabel();
			this.howToDocumentERequestIncidentLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 300, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 10, true);
			this.MainStatusBar.Visible = false;
			// 
			// eLearningPortalButton
			// 
			this.eLearningPortalButton.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("e29a95b6-1d33-4bbc-a129-203bb8d2f10a", "Search the WiseLearning Portal");
			this.eLearningPortalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 173, true);
			this.eLearningPortalButton.Name = "eLearningPortalButton";
			this.eLearningPortalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 23, true);
			this.eLearningPortalButton.TabIndex = 4;
			this.eLearningPortalButton.UseVisualStyleBackColor = false;
			this.eLearningPortalButton.Click += new System.EventHandler(this.eLearningPortalButton_Click);
			// 
			// raiseTrainingIncidentCheckBox
			// 
			this.raiseTrainingIncidentCheckBox.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("8a8ed8c8-e6fc-4f01-9e12-a091255ecaf3", "I searched the WiseLearning portal and did not find any relevant content");
			this.raiseTrainingIncidentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 261, true);
			this.raiseTrainingIncidentCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.raiseTrainingIncidentCheckBox.Name = "raiseTrainingIncidentCheckBox";
			this.raiseTrainingIncidentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 23, true);
			this.raiseTrainingIncidentCheckBox.TabIndex = 5;
			this.raiseTrainingIncidentCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.raiseTrainingIncidentCheckBox.CheckedChanged += new System.EventHandler(this.RaiseTrainingIncidentCheckBox_CheckedChanged);
			// 
			// raiseTrainingIncidentLinkLabel
			// 
			this.raiseTrainingIncidentLinkLabel.AutoSize = true;
			this.raiseTrainingIncidentLinkLabel.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("78621141-a947-4c95-b134-77a9ea112a98", "Raise Training Incident");
			this.raiseTrainingIncidentLinkLabel.Enabled = false;
			this.raiseTrainingIncidentLinkLabel.IsFontBold = false;
			this.raiseTrainingIncidentLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 265, true);
			this.raiseTrainingIncidentLinkLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.raiseTrainingIncidentLinkLabel.Name = "raiseTrainingIncidentLinkLabel";
			this.raiseTrainingIncidentLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.raiseTrainingIncidentLinkLabel.TabIndex = 6;
			this.raiseTrainingIncidentLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.raiseTrainingIncidentLinkLabel_LinkClicked);
			// 
			// wiseLearningButton
			// 
			this.wiseLearningButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("wiseLearningButton.BackgroundImage")));
			this.wiseLearningButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.wiseLearningButton.HotBackgroundImage = ((System.Drawing.Image)(resources.GetObject("wiseLearningButton.HotBackgroundImage")));
			this.wiseLearningButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 27, true);
			this.wiseLearningButton.Name = "wiseLearningButton";
			this.wiseLearningButton.NormalBackgroundImage = ((System.Drawing.Image)(resources.GetObject("wiseLearningButton.NormalBackgroundImage")));
			this.wiseLearningButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 125, true);
			this.wiseLearningButton.TabIndex = 3;
			this.wiseLearningButton.UseVisualStyleBackColor = false;
			this.wiseLearningButton.Click += new System.EventHandler(this.wiseLearningButton_Click);
			// 
			// eLearningLabel
			// 
			this.eLearningLabel.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("374B1C67-E544-4E9D-B82A-38DA8D2236A7", "The WiseLearning platform has a range of videos, quick reference guides, frequently asked questions, and workbook activities designed to help you get the most out of the system and troubleshoot common problems.\r\n\r\nClick through to our WiseLearning portal and use the search function to find what you’re looking for.\r\n\r\nIf you still need to raise an incident after a search, please provide us with a list of keywords you used so future users can discover new content more easily. To speed up your request, please also refer to our \'how to document your eRequest incident\' in the link below and provide as much information as possible, including screen shots and the outcome you wish to achieve.");
			this.eLearningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 42, true);
			this.eLearningLabel.Name = "eLearningLabel";
			this.eLearningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 210, true);
			this.eLearningLabel.TabIndex = 2;
			this.eLearningLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// eLearningCaption
			// 
			this.eLearningCaption.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("4b454fc1-00e4-40df-b75b-a2f30b1ad632", "Wait! Our WiseLearning portal may have the answer");
			this.eLearningCaption.IsFontBold = true;
			this.eLearningCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 14, true);
			this.eLearningCaption.Name = "eLearningCaption";
			this.eLearningCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 23, true);
			this.eLearningCaption.TabIndex = 1;
			// 
			// howToDocumentERequestIncidentLinkLabel
			// 
			this.howToDocumentERequestIncidentLinkLabel.AutoSize = true;
			this.howToDocumentERequestIncidentLinkLabel.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("8038E79A-6CF2-4FEA-86F2-C16479731B54", "How to Document your eRequest Incident");
			this.howToDocumentERequestIncidentLinkLabel.IsFontBold = false;
			this.howToDocumentERequestIncidentLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 210, true);
			this.howToDocumentERequestIncidentLinkLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.howToDocumentERequestIncidentLinkLabel.Name = "howToDocumentERequestIncidentLinkLabel";
			this.howToDocumentERequestIncidentLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 14, true);
			this.howToDocumentERequestIncidentLinkLabel.TabIndex = 7;
			this.howToDocumentERequestIncidentLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HowToDocumentERequestIncidentLinkLabel_LinkClicked);
			// 
			// ELearningNoticeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 310, true);
			this.Controls.Add(this.howToDocumentERequestIncidentLinkLabel);
			this.Controls.Add(this.eLearningCaption);
			this.Controls.Add(this.eLearningLabel);
			this.Controls.Add(this.raiseTrainingIncidentLinkLabel);
			this.Controls.Add(this.raiseTrainingIncidentCheckBox);
			this.Controls.Add(this.eLearningPortalButton);
			this.Controls.Add(this.wiseLearningButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ELearningNoticeForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.wiseLearningButton, 0);
			this.Controls.SetChildIndex(this.eLearningPortalButton, 0);
			this.Controls.SetChildIndex(this.raiseTrainingIncidentCheckBox, 0);
			this.Controls.SetChildIndex(this.raiseTrainingIncidentLinkLabel, 0);
			this.Controls.SetChildIndex(this.eLearningLabel, 0);
			this.Controls.SetChildIndex(this.eLearningCaption, 0);
			this.Controls.SetChildIndex(this.howToDocumentERequestIncidentLinkLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZImageButton wiseLearningButton;
		protected ZArchitecture.GUI.ZButton eLearningPortalButton;
		protected ZArchitecture.GUI.ZCheckBox raiseTrainingIncidentCheckBox;
		protected ZArchitecture.GUI.ZLinkLabel raiseTrainingIncidentLinkLabel;
		private ZArchitecture.ZLabel eLearningLabel;
		private ZArchitecture.ZLabel eLearningCaption;
		protected ZArchitecture.GUI.ZLinkLabel howToDocumentERequestIncidentLinkLabel;
	}
}
