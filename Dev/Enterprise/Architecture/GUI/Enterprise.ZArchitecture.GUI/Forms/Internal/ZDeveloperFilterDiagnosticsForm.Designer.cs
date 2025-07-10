namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZDeveloperFilterDiagnosticsForm
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
		private void InitializeComponent()
		{
			this.bottomPanel = new CargoWise.Windows.UI.KPanel();
			this.showButton = new CargoWise.Windows.UI.KButton();
			this.closeButton = new CargoWise.Windows.UI.KButton();
			this.generatedFilterCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.relationshipFilterCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.additionalFilterCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.queryAnalyzer = new CargoWise.Windows.UI.KRadioButton();
			this.operationButtonsGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.showXml = new CargoWise.Windows.UI.KRadioButton();
			this.indexSearchAnalyzer = new CargoWise.Windows.UI.KRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.operationButtonsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.showButton);
			this.bottomPanel.Controls.Add(this.closeButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 29, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// showButton
			// 
			this.showButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.showButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 4, true);
			this.showButton.Name = "showButton";
			this.showButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.showButton.TabIndex = 0;
			this.showButton.Text = "Show";
			this.showButton.UseVisualStyleBackColor = true;
			this.showButton.Click += new System.EventHandler(this.showButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 4, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 1;
			this.closeButton.Text = "Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// generatedFilterCheckBox
			// 
			this.generatedFilterCheckBox.AutoSize = true;
			this.generatedFilterCheckBox.Checked = true;
			this.generatedFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.generatedFilterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 48, true);
			this.generatedFilterCheckBox.Name = "generatedFilterCheckBox";
			this.generatedFilterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.generatedFilterCheckBox.TabIndex = 1;
			this.generatedFilterCheckBox.Text = "Generated Filter";
			this.generatedFilterCheckBox.UseVisualStyleBackColor = true;
			// 
			// relationshipFilterCheckBox
			// 
			this.relationshipFilterCheckBox.AutoSize = true;
			this.relationshipFilterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 72, true);
			this.relationshipFilterCheckBox.Name = "relationshipFilterCheckBox";
			this.relationshipFilterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 17, true);
			this.relationshipFilterCheckBox.TabIndex = 2;
			this.relationshipFilterCheckBox.Text = "Relationship Filter";
			this.relationshipFilterCheckBox.UseVisualStyleBackColor = true;
			// 
			// additionalFilterCheckBox
			// 
			this.additionalFilterCheckBox.AutoSize = true;
			this.additionalFilterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 96, true);
			this.additionalFilterCheckBox.Name = "additionalFilterCheckBox";
			this.additionalFilterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.additionalFilterCheckBox.TabIndex = 3;
			this.additionalFilterCheckBox.Text = "Additional Filter";
			this.additionalFilterCheckBox.UseVisualStyleBackColor = true;
			// 
			// queryAnalyzer
			// 
			this.queryAnalyzer.AutoSize = true;
			this.queryAnalyzer.Checked = true;
			this.queryAnalyzer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.queryAnalyzer.Name = "queryAnalyzer";
			this.queryAnalyzer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.queryAnalyzer.TabIndex = 0;
			this.queryAnalyzer.TabStop = true;
			this.queryAnalyzer.Text = "Query Analyzer";
			this.queryAnalyzer.UseVisualStyleBackColor = true;
			// 
			// operationButtonsGroupBox
			// 
			this.operationButtonsGroupBox.Controls.Add(this.showXml);
			this.operationButtonsGroupBox.Controls.Add(this.queryAnalyzer);
			this.operationButtonsGroupBox.Controls.Add(this.generatedFilterCheckBox);
			this.operationButtonsGroupBox.Controls.Add(this.additionalFilterCheckBox);
			this.operationButtonsGroupBox.Controls.Add(this.relationshipFilterCheckBox);
			this.operationButtonsGroupBox.Controls.Add(this.indexSearchAnalyzer);
			this.operationButtonsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.operationButtonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.operationButtonsGroupBox.Name = "operationButtonsGroupBox";
			this.operationButtonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 149, true);
			this.operationButtonsGroupBox.TabIndex = 0;
			this.operationButtonsGroupBox.TabStop = false;
			// 
			// showXml
			// 
			this.showXml.AutoSize = true;
			this.showXml.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 120, true);
			this.showXml.Name = "showXml";
			this.showXml.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.showXml.TabIndex = 4;
			this.showXml.Text = "Xml";
			this.showXml.UseVisualStyleBackColor = true;
			// 
			// indexSearchAnalyzer
			// 
			this.indexSearchAnalyzer.AutoSize = true;
			this.indexSearchAnalyzer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 144, true);
			this.indexSearchAnalyzer.Name = "indexSearchAnalyzer";
			this.indexSearchAnalyzer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.indexSearchAnalyzer.TabIndex = 5;
			this.indexSearchAnalyzer.Text = "Index Search Analyzer";
			this.indexSearchAnalyzer.UseVisualStyleBackColor = true;
			this.indexSearchAnalyzer.Visible = false;
			// 
			// ZDeveloperFilterDiagnosticsForm
			// 
			this.AcceptButton = this.showButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 178, true);
			this.Controls.Add(this.operationButtonsGroupBox);
			this.Controls.Add(this.bottomPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ZDeveloperFilterDiagnosticsForm";
			this.Text = "Filter Diagnostics";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.operationButtonsGroupBox.ResumeLayout(false);
			this.operationButtonsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KPanel bottomPanel;
		private CargoWise.Windows.UI.KButton closeButton;
		private CargoWise.Windows.UI.KCheckBox generatedFilterCheckBox;
		private CargoWise.Windows.UI.KCheckBox relationshipFilterCheckBox;
		private CargoWise.Windows.UI.KCheckBox additionalFilterCheckBox;
		private CargoWise.Windows.UI.KRadioButton queryAnalyzer;
		private CargoWise.Windows.UI.KGroupBox operationButtonsGroupBox;
		private CargoWise.Windows.UI.KButton showButton;
		private CargoWise.Windows.UI.KRadioButton showXml;
		private CargoWise.Windows.UI.KRadioButton indexSearchAnalyzer;
	}
}
