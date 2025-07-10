namespace CargoWise.Macros.GUI
{
	partial class MacroEvaluationForm
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
			this.inputTextBox = new CargoWise.Windows.UI.KTextBox();
			this.runButton = new CargoWise.Windows.UI.KButton();
			this.resultsTextBox = new CargoWise.Windows.UI.KTextBox();
			this.loaderPictureBox = new CargoWise.Windows.UI.KPictureBox();
			this.outputPanel = new CargoWise.Windows.UI.KPanel();
			this.performancePanel = new CargoWise.Windows.UI.KPanel();
			this.performanceTextBox = new CargoWise.Windows.UI.KTextBox();
			this.variablesPanel = new CargoWise.Windows.UI.KPanel();
			this.variablesTextBox = new CargoWise.Windows.UI.KTextBox();
			this.mainContainer = new System.Windows.Forms.SplitContainer();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.informationsTabControl = new System.Windows.Forms.TabControl();
			this.resultsTabPage = new System.Windows.Forms.TabPage();
			this.variablesTabPage = new System.Windows.Forms.TabPage();
			this.astTabPage = new System.Windows.Forms.TabPage();
			this.astPanel = new CargoWise.Windows.UI.KPanel();
			this.astTextBox = new CargoWise.Windows.UI.KTextBox();
			this.expressionTabPage = new System.Windows.Forms.TabPage();
			this.expressionPanel = new CargoWise.Windows.UI.KPanel();
			this.expressionTextBox = new CargoWise.Windows.UI.KTextBox();
			this.performanceTabPage = new System.Windows.Forms.TabPage();
			((System.ComponentModel.ISupportInitialize)(this.loaderPictureBox)).BeginInit();
			this.outputPanel.SuspendLayout();
			this.performancePanel.SuspendLayout();
			this.variablesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
			this.mainContainer.Panel1.SuspendLayout();
			this.mainContainer.Panel2.SuspendLayout();
			this.mainContainer.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.informationsTabControl.SuspendLayout();
			this.resultsTabPage.SuspendLayout();
			this.variablesTabPage.SuspendLayout();
			this.astTabPage.SuspendLayout();
			this.astPanel.SuspendLayout();
			this.expressionTabPage.SuspendLayout();
			this.expressionPanel.SuspendLayout();
			this.performanceTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// inputTextBox
			// 
			this.inputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.inputTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.inputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.inputTextBox.Multiline = true;
			this.inputTextBox.Name = "inputTextBox";
			this.inputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.inputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 226, true);
			this.inputTextBox.TabIndex = 0;
			// 
			// runButton
			// 
			this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.runButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(791, 11, true);
			this.runButton.Name = "runButton";
			this.runButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.runButton.TabIndex = 2;
			this.runButton.Text = "Run (F5)";
			this.runButton.UseVisualStyleBackColor = true;
			// 
			// resultsTextBox
			// 
			this.resultsTextBox.BackColor = System.Drawing.Color.White;
			this.resultsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.resultsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultsTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.resultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.resultsTextBox.Multiline = true;
			this.resultsTextBox.Name = "resultsTextBox";
			this.resultsTextBox.ReadOnly = true;
			this.resultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.resultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 560, true);
			this.resultsTextBox.TabIndex = 4;
			// 
			// loaderPictureBox
			// 
			this.loaderPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.loaderPictureBox.Image = global::CargoWise.Macros.GUI.Properties.Resources.loader;
			this.loaderPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(795, 18, true);
			this.loaderPictureBox.Name = "loaderPictureBox";
			this.loaderPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 11, true);
			this.loaderPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.loaderPictureBox.TabIndex = 5;
			this.loaderPictureBox.TabStop = false;
			this.loaderPictureBox.Visible = false;
			// 
			// outputPanel
			// 
			this.outputPanel.Controls.Add(this.resultsTextBox);
			this.outputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 560, true);
			this.outputPanel.TabIndex = 6;
			// 
			// performancePanel
			// 
			this.performancePanel.Controls.Add(this.performanceTextBox);
			this.performancePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.performancePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.performancePanel.Name = "performancePanel";
			this.performancePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.performancePanel.TabIndex = 15;
			// 
			// performanceTextBox
			// 
			this.performanceTextBox.BackColor = System.Drawing.Color.White;
			this.performanceTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.performanceTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.performanceTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.performanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.performanceTextBox.Multiline = true;
			this.performanceTextBox.Name = "performanceTextBox";
			this.performanceTextBox.ReadOnly = true;
			this.performanceTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.performanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.performanceTextBox.TabIndex = 14;
			// 
			// variablesPanel
			// 
			this.variablesPanel.Controls.Add(this.variablesTextBox);
			this.variablesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.variablesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.variablesPanel.Name = "variablesPanel";
			this.variablesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.variablesPanel.TabIndex = 10;
			// 
			// variablesTextBox
			// 
			this.variablesTextBox.BackColor = System.Drawing.Color.White;
			this.variablesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.variablesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.variablesTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.variablesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.variablesTextBox.Multiline = true;
			this.variablesTextBox.Name = "variablesTextBox";
			this.variablesTextBox.ReadOnly = true;
			this.variablesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.variablesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.variablesTextBox.TabIndex = 5;
			// 
			// mainContainer
			// 
			this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainContainer.Name = "mainContainer";
			this.mainContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainContainer.Panel1
			// 
			this.mainContainer.Panel1.Controls.Add(this.inputTextBox);
			this.mainContainer.Panel1.Controls.Add(this.optionsPanel);
			// 
			// mainContainer.Panel2
			// 
			this.mainContainer.Panel2.Controls.Add(this.informationsTabControl);
			this.mainContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.mainContainer.SplitterDistance = 271;
			this.mainContainer.TabIndex = 9;
			// 
			// optionsPanel
			// 
			this.optionsPanel.Controls.Add(this.loaderPictureBox);
			this.optionsPanel.Controls.Add(this.runButton);
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.optionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 226, true);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 45, true);
			this.optionsPanel.TabIndex = 9;
			// 
			// informationsTabControl
			// 
			this.informationsTabControl.Controls.Add(this.resultsTabPage);
			this.informationsTabControl.Controls.Add(this.variablesTabPage);
			this.informationsTabControl.Controls.Add(this.astTabPage);
			this.informationsTabControl.Controls.Add(this.expressionTabPage);
			this.informationsTabControl.Controls.Add(this.performanceTabPage);
			this.informationsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.informationsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.informationsTabControl.Name = "informationsTabControl";
			this.informationsTabControl.SelectedIndex = 0;
			this.informationsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 586, true);
			this.informationsTabControl.TabIndex = 17;
			// 
			// resultsTabPage
			// 
			this.resultsTabPage.Controls.Add(this.outputPanel);
			this.resultsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.resultsTabPage.Name = "resultsTabPage";
			this.resultsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 560, true);
			this.resultsTabPage.TabIndex = 11;
			this.resultsTabPage.Text = "Results";
			this.resultsTabPage.UseVisualStyleBackColor = true;
			// 
			// variablesTabPage
			// 
			this.variablesTabPage.Controls.Add(this.variablesPanel);
			this.variablesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.variablesTabPage.Name = "variablesTabPage";
			this.variablesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.variablesTabPage.TabIndex = 13;
			this.variablesTabPage.Text = "Variables";
			this.variablesTabPage.UseVisualStyleBackColor = true;
			// 
			// astTabPage
			// 
			this.astTabPage.Controls.Add(this.astPanel);
			this.astTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.astTabPage.Name = "astTabPage";
			this.astTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.astTabPage.TabIndex = 17;
			this.astTabPage.Text = "AST";
			this.astTabPage.UseVisualStyleBackColor = true;
			// 
			// astPanel
			// 
			this.astPanel.Controls.Add(this.astTextBox);
			this.astPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.astPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.astPanel.Name = "astPanel";
			this.astPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.astPanel.TabIndex = 0;
			// 
			// astTextBox
			// 
			this.astTextBox.BackColor = System.Drawing.Color.White;
			this.astTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.astTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.astTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.astTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.astTextBox.Multiline = true;
			this.astTextBox.Name = "astTextBox";
			this.astTextBox.ReadOnly = true;
			this.astTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.astTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.astTextBox.TabIndex = 6;
			// 
			// expressionTabPage
			// 
			this.expressionTabPage.Controls.Add(this.expressionPanel);
			this.expressionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.expressionTabPage.Name = "expressionTabPage";
			this.expressionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.expressionTabPage.TabIndex = 18;
			this.expressionTabPage.Text = "Expression";
			this.expressionTabPage.UseVisualStyleBackColor = true;
			// 
			// expressionPanel
			// 
			this.expressionPanel.Controls.Add(this.expressionTextBox);
			this.expressionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.expressionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.expressionPanel.Name = "expressionPanel";
			this.expressionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.expressionPanel.TabIndex = 11;
			// 
			// expressionTextBox
			// 
			this.expressionTextBox.BackColor = System.Drawing.Color.White;
			this.expressionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.expressionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.expressionTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.expressionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.expressionTextBox.Multiline = true;
			this.expressionTextBox.Name = "expressionTextBox";
			this.expressionTextBox.ReadOnly = true;
			this.expressionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.expressionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.expressionTextBox.TabIndex = 5;
			// 
			// performanceTabPage
			// 
			this.performanceTabPage.Controls.Add(this.performancePanel);
			this.performanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.performanceTabPage.Name = "performanceTabPage";
			this.performanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.performanceTabPage.TabIndex = 16;
			this.performanceTabPage.Text = "Performance";
			this.performanceTabPage.UseVisualStyleBackColor = true;
			// 
			// MacroEvaluationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 861, true);
			this.Controls.Add(this.mainContainer);
			this.KeyPreview = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 900, true);
			this.Name = "MacroEvaluationForm";
			this.ShowIcon = false;
			this.Text = "Evaluate Macro";
			((System.ComponentModel.ISupportInitialize)(this.loaderPictureBox)).EndInit();
			this.outputPanel.ResumeLayout(false);
			this.outputPanel.PerformLayout();
			this.performancePanel.ResumeLayout(false);
			this.performancePanel.PerformLayout();
			this.variablesPanel.ResumeLayout(false);
			this.variablesPanel.PerformLayout();
			this.mainContainer.Panel1.ResumeLayout(false);
			this.mainContainer.Panel1.PerformLayout();
			this.mainContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
			this.mainContainer.ResumeLayout(false);
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.informationsTabControl.ResumeLayout(false);
			this.resultsTabPage.ResumeLayout(false);
			this.variablesTabPage.ResumeLayout(false);
			this.astTabPage.ResumeLayout(false);
			this.astPanel.ResumeLayout(false);
			this.astPanel.PerformLayout();
			this.expressionTabPage.ResumeLayout(false);
			this.expressionPanel.ResumeLayout(false);
			this.expressionPanel.PerformLayout();
			this.performanceTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KTextBox inputTextBox;
		private CargoWise.Windows.UI.KButton runButton;
		private CargoWise.Windows.UI.KTextBox resultsTextBox;
		private CargoWise.Windows.UI.KTextBox variablesTextBox;
		private CargoWise.Windows.UI.KTextBox performanceTextBox;
		private CargoWise.Windows.UI.KPictureBox loaderPictureBox;
		private CargoWise.Windows.UI.KPanel outputPanel;
		private CargoWise.Windows.UI.KPanel performancePanel;
		private CargoWise.Windows.UI.KPanel variablesPanel;
		private System.Windows.Forms.SplitContainer mainContainer;
		private System.Windows.Forms.Panel optionsPanel;
		private System.Windows.Forms.TabControl informationsTabControl;
		private System.Windows.Forms.TabPage resultsTabPage;
		private System.Windows.Forms.TabPage variablesTabPage;
		private System.Windows.Forms.TabPage performanceTabPage;
		private System.Windows.Forms.TabPage astTabPage;
		private System.Windows.Forms.TabPage expressionTabPage;
		private CargoWise.Windows.UI.KTextBox astTextBox;
		private CargoWise.Windows.UI.KPanel astPanel;
		private CargoWise.Windows.UI.KPanel expressionPanel;
		private CargoWise.Windows.UI.KTextBox expressionTextBox;
	}
}

