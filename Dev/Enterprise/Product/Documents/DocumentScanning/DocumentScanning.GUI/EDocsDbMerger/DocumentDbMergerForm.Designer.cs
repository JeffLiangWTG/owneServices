namespace Enterprise.DocumentScanning.GUI
{
	partial class DocumentDbMergerForm
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
			this.components = new System.ComponentModel.Container();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.zPanelChart = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.startButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.barChartLabel = new Enterprise.ZArchitecture.ZLabel();
			this.progressBar = new CargoWise.Windows.UI.KProgressBar();
			this.progressInPercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.consoleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.stopButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.statusTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanelChart.SuspendLayout();
			this.SuspendLayout();

			// MainStatusBar
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 540, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);

			// timer
			this.timer.Interval = 10000;
			this.timer.Tick += new System.EventHandler(this.Timer_Tick);

			// zPanelChart
			this.zPanelChart.AutoScroll = true;
			this.zPanelChart.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.zPanelChart.Controls.Add(this.statusTableLayoutPanel);
			this.zPanelChart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 25, true);
			this.zPanelChart.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 230, true);
			this.zPanelChart.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 170, true);
			this.zPanelChart.Name = "zPanelChart";
			this.zPanelChart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 230, true);
			this.zPanelChart.TabIndex = 5;

			// startButton
			this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.startButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbMergerForm|ebfbcc5b-65ad-4c3e-9f2c-ec25d9168071", "Start");
			this.startButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 511, true);
			this.startButton.Name = "startButton";
			this.startButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.startButton.TabIndex = 2;
			this.startButton.ToolTipCaption = null;
			this.startButton.UseVisualStyleBackColor = true;
			this.startButton.Click += new System.EventHandler(this.StartButton_Click);

			// barChartLabel
			this.barChartLabel.AutoSize = true;
			this.barChartLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.barChartLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.barChartLabel.Name = "barChartLabel";
			this.barChartLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.barChartLabel.TabIndex = 4;

			// progressBar
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 261, true);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 23, true);
			this.progressBar.TabIndex = 7;

			// progressInPercentLabel
			this.progressInPercentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.progressInPercentLabel.BackColor = System.Drawing.SystemColors.Control;
			this.progressInPercentLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.progressInPercentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.progressInPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(734, 261, true);
			this.progressInPercentLabel.Name = "progressInPercentLabel";
			this.progressInPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 23, true);
			this.progressInPercentLabel.TabIndex = 8;
			this.progressInPercentLabel.Text = "100%";
			this.progressInPercentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

			// closeButton
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbMergerForm|33c964df-820e-47f7-bf34-e57f616fe357", "Close");
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(697, 511, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 1;
			this.closeButton.ToolTipCaption = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);

			// consoleTextBox
			this.consoleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.consoleTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.consoleTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbMergerForm|35768217-2c90-4749-bacd-aedb225f62e4", "Merge Results", "Shows the results of merging the databases.");
			this.consoleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.consoleTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.consoleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 290, true);
			this.consoleTextBox.Multiline = true;
			this.consoleTextBox.Name = "consoleTextBox";
			this.consoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.consoleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 215, true);
			this.consoleTextBox.TabIndex = 10;
			this.consoleTextBox.ReadOnly = true;

			// stopButton
			this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.stopButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbMergerForm|67bf4f25-d304-41fb-8070-71e09febe469", "Stop");
			this.stopButton.Enabled = false;
			this.stopButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 511, true);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.stopButton.TabIndex = 3;
			this.stopButton.ToolTipCaption = null;
			this.stopButton.UseVisualStyleBackColor = true;
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);

			// statusTableLayoutPanel
			this.statusTableLayoutPanel.ColumnCount = 1;
			this.statusTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50)));
			this.statusTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.statusTableLayoutPanel.Name = "statusTableLayoutPanel";
			this.statusTableLayoutPanel.RowCount = 2;
			this.statusTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.statusTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.statusTableLayoutPanel.TabIndex = 0;

			// DocumentDbMergerForm
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbMergerForm|e9ef7965-ff28-4eb3-aee7-393fbdddc80a", "eDocs Database Merger");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 564, true);
			this.Controls.Add(this.progressInPercentLabel);
			this.Controls.Add(this.stopButton);
			this.Controls.Add(this.consoleTextBox);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.zPanelChart);
			this.Controls.Add(this.startButton);
			this.Controls.Add(this.barChartLabel);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.Name = "DocumentDbMergerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DocumentDbMergerForm_FormClosing);
			this.Controls.SetChildIndex(this.barChartLabel, 0);
			this.Controls.SetChildIndex(this.startButton, 0);
			this.Controls.SetChildIndex(this.zPanelChart, 0);
			this.Controls.SetChildIndex(this.progressBar, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.consoleTextBox, 0);
			this.Controls.SetChildIndex(this.stopButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.progressInPercentLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanelChart.ResumeLayout(false);
			this.zPanelChart.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZTextBox consoleTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton startButton;
		System.Windows.Forms.Timer timer;
		Enterprise.ZArchitecture.GUI.ZPanel zPanelChart;
		Enterprise.ZArchitecture.ZLabel barChartLabel;
		CargoWise.Windows.UI.KProgressBar progressBar;
		Enterprise.ZArchitecture.ZLabel progressInPercentLabel;
		Enterprise.ZArchitecture.GUI.ZButton closeButton;
		Enterprise.ZArchitecture.GUI.ZButton stopButton;
		CargoWise.Windows.UI.KTableLayoutPanel statusTableLayoutPanel;
	}
}
