namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class AttachDocumentForm
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

		protected override void InitializeComponent()
		{
			this.ResolutionCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addEdocButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.filePathBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.setErequestStatusOnlyzCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 245, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction);
			// 
			// ResolutionCommentTextBox
			// 
			this.ResolutionCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ResolutionCommentTextBox, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentCloseAction)(null)).Comment)));
			this.ResolutionCommentTextBox.CaptionResourceString = null;
			this.ResolutionCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ResolutionCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 69, true);
			this.ResolutionCommentTextBox.Multiline = true;
			this.ResolutionCommentTextBox.Name = "ResolutionCommentTextBox";
			this.ResolutionCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 96, true);
			this.ResolutionCommentTextBox.TabIndex = 4;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = null;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 216, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Text = "OK";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MessageLabel
			// 
			this.MessageLabel.CaptionResourceString = null;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 49, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 17, true);
			this.MessageLabel.TabIndex = 3;
			this.MessageLabel.Text = "Comment";
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = null;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 216, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 8;
			this.CancelButtonX.Text = "Cancel";
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// fileBrowseButton
			// 
			this.addEdocButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addEdocButton.CaptionResourceString = null;
			this.addEdocButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 25, true);
			this.addEdocButton.Name = "addEdocButton";
			this.addEdocButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.addEdocButton.TabIndex = 2;
			this.addEdocButton.Text = "Add eDoc";
			this.addEdocButton.UseVisualStyleBackColor = true;
			this.addEdocButton.Click += new System.EventHandler(this.addEdocButton_Click);
			// 
			// filePathBox
			// 
			this.filePathBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filePathBox.CaptionResourceString = null;
			this.filePathBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 26, true);
			this.filePathBox.Name = "filePathBox";
			this.filePathBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.filePathBox.TabIndex = 1;
			this.filePathBox.ReadOnly = true;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = null;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "File";
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 169, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 39, true);
			this.zLabel2.TabIndex = 5;
			this.zLabel2.Text = "Note: Comment is included in an eConversation entry with format:\r\n  Closed As (*D" +
    "ocument*) Provided - Awaiting Customer - (*Comment*)";
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// setErequestStatusOnlyzCheckBox
			// 
			this.setErequestStatusOnlyzCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.setErequestStatusOnlyzCheckBox, "SetERequestStatusOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAction)(null)).SetERequestStatusOnly)));
			this.setErequestStatusOnlyzCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("beef7276-a78d-4514-bec1-769ba709ca7b", "Set eRequest Status Only");
			this.setErequestStatusOnlyzCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 215, true);
			this.setErequestStatusOnlyzCheckBox.Name = "setErequestStatusOnlyzCheckBox";
			this.setErequestStatusOnlyzCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.setErequestStatusOnlyzCheckBox.TabIndex = 6;
			this.setErequestStatusOnlyzCheckBox.Text = "Set eRequest Status Only";
			this.setErequestStatusOnlyzCheckBox.UseVisualStyleBackColor = true;
			// 
			// AttachDocumentForm
			// 
			this.AllowDrop = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 269, true);
			this.Controls.Add(this.setErequestStatusOnlyzCheckBox);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.filePathBox);
			this.Controls.Add(this.addEdocButton);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.ResolutionCommentTextBox);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAction);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentAction";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 250, true);
			this.Name = "AttachDocumentForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.ResolutionCommentTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.addEdocButton, 0);
			this.Controls.SetChildIndex(this.filePathBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.setErequestStatusOnlyzCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZTextBox ResolutionCommentTextBox;
		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		protected ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZButton CancelButtonX;
		private ZArchitecture.GUI.ZButton addEdocButton;
		private ZArchitecture.ZTextBox filePathBox;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.GUI.ZCheckBox setErequestStatusOnlyzCheckBox;
	}
}
