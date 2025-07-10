#if DEBUG
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Unit_Testing_Utility_Classes
{
	public partial class TestRigUserControl : UserControl
	{
		ZOpenFileDialog TemplateOpenFileDialog;
		CargoWise.Windows.UI.KGroupBox groupBox1;
		Enterprise.ZArchitecture.GUI.ZButton TemplateDialogButton;
		Enterprise.ZArchitecture.ZTextBox NameTextBox;
		Enterprise.ZArchitecture.GUI.ZButton UpdateTemplateButton;
		Enterprise.ZArchitecture.ZTextBox FileTextBox;
		CargoWise.Windows.UI.KLabel label1;
		CargoWise.Windows.UI.KLabel label2;
		Enterprise.ZArchitecture.ZTextBox DataContextTextBox;
		CargoWise.Windows.UI.KLabel label3;
		CargoWise.Windows.UI.KCheckBox SystemDefinedCheckBox;
		private GroupBox groupBox2;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateClientDocumentXMLButton;
		private Button DocBuilderTemplateUpdaterButton;
		private GroupBox ToolsGroupBox;
		private ZButton fixEmUppererButton;

		void InitializeComponent()
		{
			this.TemplateOpenFileDialog = new ZOpenFileDialog();
			this.groupBox1 = new CargoWise.Windows.UI.KGroupBox();
			this.UpdateTemplateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SystemDefinedCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.label3 = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.DataContextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.TemplateDialogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FileTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.groupBox2 = new CargoWise.Windows.UI.KGroupBox();
			this.fixEmUppererButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateClientDocumentXMLButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DocBuilderTemplateUpdaterButton = new CargoWise.Windows.UI.KButton();
			this.ToolsGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.ToolsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.UpdateTemplateButton);
			this.groupBox1.Controls.Add(this.SystemDefinedCheckBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.DataContextTextBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.TemplateDialogButton);
			this.groupBox1.Controls.Add(this.NameTextBox);
			this.groupBox1.Controls.Add(this.FileTextBox);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 13, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 150, true);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Insert / Update Templates";
			// 
			// UpdateTemplateButton
			// 
			this.UpdateTemplateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 120, true);
			this.UpdateTemplateButton.Name = "UpdateTemplateButton";
			this.UpdateTemplateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.UpdateTemplateButton.TabIndex = 8;
			this.UpdateTemplateButton.Text = "Insert / Update Template";
			this.UpdateTemplateButton.Click += new System.EventHandler(this.UpdateTemplateButton_Click);
			// 
			// SystemDefinedCheckBox
			// 
			this.SystemDefinedCheckBox.Checked = true;
			this.SystemDefinedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.SystemDefinedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 55, true);
			this.SystemDefinedCheckBox.Name = "SystemDefinedCheckBox";
			this.SystemDefinedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.SystemDefinedCheckBox.TabIndex = 2;
			this.SystemDefinedCheckBox.Text = "System Defined";
			// 
			// label3
			// 
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 80, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.label3.TabIndex = 5;
			this.label3.Text = "Template File:";
			// 
			// label2
			// 
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 34, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.label2.TabIndex = 3;
			this.label2.Text = "Data Context:";
			// 
			// DataContextTextBox
			// 
			this.DataContextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DataContextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 34, true);
			this.DataContextTextBox.Name = "DataContextTextBox";
			this.DataContextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DataContextTextBox.TabIndex = 4;
			this.DataContextTextBox.Text = "Statement";
			// 
			// label1
			// 
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 57, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.label1.TabIndex = 0;
			this.label1.Text = "Template Name:";
			// 
			// TemplateDialogButton
			// 
			this.TemplateDialogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 80, true);
			this.TemplateDialogButton.Name = "TemplateDialogButton";
			this.TemplateDialogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.TemplateDialogButton.TabIndex = 7;
			this.TemplateDialogButton.Text = "Browse...";
			this.TemplateDialogButton.Click += new System.EventHandler(this.TemplateDialogButton_Click);
			// 
			// NameTextBox
			// 
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 57, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.NameTextBox.TabIndex = 1;
			this.NameTextBox.Text = "Statement Of Account";
			// 
			// FileTextBox
			// 
			this.FileTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 80, true);
			this.FileTextBox.Name = "FileTextBox";
			this.FileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 20, true);
			this.FileTextBox.TabIndex = 6;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.fixEmUppererButton);
			this.groupBox2.Controls.Add(this.UpdateClientDocumentXMLButton);
			this.groupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 179, true);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 51, true);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Update Client Documents.Xml for all checked out Client Xls";
			// 
			// fixEmUppererButton
			// 
			this.fixEmUppererButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 22, true);
			this.fixEmUppererButton.Name = "fixEmUppererButton";
			this.fixEmUppererButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 23, true);
			this.fixEmUppererButton.TabIndex = 1;
			this.fixEmUppererButton.Text = "Fix Client Document Paths";
			this.fixEmUppererButton.UseVisualStyleBackColor = true;
			this.fixEmUppererButton.Click += new System.EventHandler(this.fixEmUppererButton_Click);
			// 
			// UpdateClientDocumentXMLButton
			// 
			this.UpdateClientDocumentXMLButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 22, true);
			this.UpdateClientDocumentXMLButton.Name = "UpdateClientDocumentXMLButton";
			this.UpdateClientDocumentXMLButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 23, true);
			this.UpdateClientDocumentXMLButton.TabIndex = 0;
			this.UpdateClientDocumentXMLButton.Text = "Update Client Document Xmls";
			this.UpdateClientDocumentXMLButton.UseVisualStyleBackColor = true;
			this.UpdateClientDocumentXMLButton.Click += new System.EventHandler(this.UpdateClientDocumentXMLButton_Click);
			// 
			// DocBuilderTemplateUpdaterButton
			// 
			this.DocBuilderTemplateUpdaterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.DocBuilderTemplateUpdaterButton.Name = "DocBuilderTemplateUpdaterButton";
			this.DocBuilderTemplateUpdaterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 23, true);
			this.DocBuilderTemplateUpdaterButton.TabIndex = 3;
			this.DocBuilderTemplateUpdaterButton.Text = "DocBuilder Template Updater...";
			this.DocBuilderTemplateUpdaterButton.UseVisualStyleBackColor = true;
			this.DocBuilderTemplateUpdaterButton.Click += new System.EventHandler(this.DocBuilderTemplateUpdaterButton_Click);
			// 
			// ToolsGroupBox
			// 
			this.ToolsGroupBox.Controls.Add(this.DocBuilderTemplateUpdaterButton);
			this.ToolsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 245, true);
			this.ToolsGroupBox.Name = "ToolsGroupBox";
			this.ToolsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 48, true);
			this.ToolsGroupBox.TabIndex = 4;
			this.ToolsGroupBox.TabStop = false;
			this.ToolsGroupBox.Text = "Tools";
			// 
			// TestRigUserControl
			// 
			this.Controls.Add(this.ToolsGroupBox);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "TestRigUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 512, true);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.ToolsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				TemplateOpenFileDialog?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
#endif
