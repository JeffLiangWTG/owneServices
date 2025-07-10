namespace Enterprise.DataTransfer.Native.Generator
{
	partial class NativeSchemaGenerationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (this.TemporaryContext != null)
				{
					this.TemporaryContext.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			this.nativeSchemaGenerationFormBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.nativeSchemaGenerationFormBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
			this.tabPage3 = new CargoWise.Windows.UI.KTabPage();
			this.schemaTextBox = new CargoWise.Windows.UI.KTextBox();
			this.panel2 = new CargoWise.Windows.UI.KPanel();
			this.entitySetNameTextBox = new CargoWise.Windows.UI.KTextBox();
			this.entitySetNameLabel = new CargoWise.Windows.UI.KLabel();
			this.previewButton = new CargoWise.Windows.UI.KButton();
			this.generateSchemaButton = new CargoWise.Windows.UI.KButton();
			this.webServiceTabControl = new CargoWise.Windows.UI.KTabControl();
			this.tabPage1 = new CargoWise.Windows.UI.KTabPage();
			this.button1 = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.nativeSchemaGenerationFormBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nativeSchemaGenerationFormBindingSource1)).BeginInit();
			this.tabPage3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.webServiceTabControl.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// nativeSchemaGenerationFormBindingSource
			// 
			this.nativeSchemaGenerationFormBindingSource.DataSource = typeof(Enterprise.DataTransfer.Native.Generator.NativeSchemaGenerationForm);
			// 
			// nativeSchemaGenerationFormBindingSource1
			// 
			this.nativeSchemaGenerationFormBindingSource1.DataSource = typeof(Enterprise.DataTransfer.Native.Generator.NativeSchemaGenerationForm);
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.schemaTextBox);
			this.tabPage3.Controls.Add(this.panel2);
			this.tabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
			this.tabPage3.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.tabPage3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 1026, true);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Schema";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// schemaTextBox
			// 
			this.schemaTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.schemaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 63, true);
			this.schemaTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.schemaTextBox.Multiline = true;
			this.schemaTextBox.Name = "schemaTextBox";
			this.schemaTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.schemaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1444, 959, true);
			this.schemaTextBox.TabIndex = 83;
			this.schemaTextBox.WordWrap = false;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.entitySetNameTextBox);
			this.panel2.Controls.Add(this.entitySetNameLabel);
			this.panel2.Controls.Add(this.previewButton);
			this.panel2.Controls.Add(this.generateSchemaButton);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.panel2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.panel2.Name = "panel2";
			this.panel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1444, 59, true);
			this.panel2.TabIndex = 84;
			// 
			// entitySetNameTextBox
			// 
			this.entitySetNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 16, true);
			this.entitySetNameTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.entitySetNameTextBox.Name = "entitySetNameTextBox";
			this.entitySetNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 22, true);
			this.entitySetNameTextBox.TabIndex = 67;
			// 
			// entitySetNameLabel
			// 
			this.entitySetNameLabel.AutoSize = true;
			this.entitySetNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 20, true);
			this.entitySetNameLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 0, 4, 0, true);
			this.entitySetNameLabel.Name = "entitySetNameLabel";
			this.entitySetNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
			this.entitySetNameLabel.TabIndex = 66;
			this.entitySetNameLabel.Text = "Entity Set Name:";
			// 
			// previewButton
			// 
			this.previewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 12, true);
			this.previewButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 28, true);
			this.previewButton.TabIndex = 65;
			this.previewButton.Text = "Preview Results";
			this.previewButton.UseVisualStyleBackColor = true;
			this.previewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// generateSchemaButton
			// 
			this.generateSchemaButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 12, true);
			this.generateSchemaButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.generateSchemaButton.Name = "generateSchemaButton";
			this.generateSchemaButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 28, true);
			this.generateSchemaButton.TabIndex = 53;
			this.generateSchemaButton.Text = "Generate Schema";
			this.generateSchemaButton.UseVisualStyleBackColor = true;
			this.generateSchemaButton.Click += new System.EventHandler(this.GenerateSchemaButton_Click);
			// 
			// webServiceTabControl
			// 
			this.webServiceTabControl.Controls.Add(this.tabPage3);
			this.webServiceTabControl.Controls.Add(this.tabPage1);
			this.webServiceTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webServiceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.webServiceTabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.webServiceTabControl.Name = "webServiceTabControl";
			this.webServiceTabControl.SelectedIndex = 0;
			this.webServiceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1460, 1055, true);
			this.webServiceTabControl.TabIndex = 78;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.button1);
			this.tabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 1026, true);
			this.tabPage1.TabIndex = 3;
			this.tabPage1.Text = "Import/Export";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 40, true);
			this.button1.Name = "button1";
			this.button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 88, true);
			this.button1.TabIndex = 0;
			this.button1.Text = "Import XML";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.importButton_Click);
			// 
			// NativeSchemaGenerationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1460, 1055, true);
			this.Controls.Add(this.webServiceTabControl);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.Name = "NativeSchemaGenerationForm";
			((System.ComponentModel.ISupportInitialize)(this.nativeSchemaGenerationFormBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nativeSchemaGenerationFormBindingSource1)).EndInit();
			this.tabPage3.ResumeLayout(false);
			this.tabPage3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.webServiceTabControl.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.BindingSource nativeSchemaGenerationFormBindingSource;
		private System.Windows.Forms.BindingSource nativeSchemaGenerationFormBindingSource1;
		private CargoWise.Windows.UI.KTabPage tabPage3;
		private CargoWise.Windows.UI.KTextBox schemaTextBox;
		private CargoWise.Windows.UI.KPanel panel2;
		private CargoWise.Windows.UI.KTextBox entitySetNameTextBox;
		private CargoWise.Windows.UI.KLabel entitySetNameLabel;
		private CargoWise.Windows.UI.KButton previewButton;
		private CargoWise.Windows.UI.KButton generateSchemaButton;
		private CargoWise.Windows.UI.KTabControl webServiceTabControl;
		private CargoWise.Windows.UI.KTabPage tabPage1;
		private CargoWise.Windows.UI.KButton button1;
	}
}

