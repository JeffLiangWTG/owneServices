using System.Diagnostics.CodeAnalysis;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup.Tools
{
	partial class NetworkMonitorForm
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
			if (disposing)
			{
				if (components != null)
				{
					chart1.Dispose();
					monitor.Dispose();
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		new void InitializeComponent()
		{
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(1D, 0D);
			System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint2 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(1D, 0D);
			System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
			this.chart1 = new CargoWise.Windows.UI.KChart();
			this.button1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.button2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.radioButton1 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 397, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(796, 24, true);
			// 
			// chart1
			// 
			this.chart1.BackColor = System.Drawing.Color.Transparent;
			chartArea1.AxisX.Title = "Seconds Elapsed";
			chartArea1.AxisY.Title = "Round Loop Delay in ms";
			chartArea1.Name = "ChartArea1";
			chartArea2.AxisY.Title = "Network throughput in MB/s";
			chartArea2.Name = "ChartArea2";
			chartArea2.Visible = false;
			this.chart1.ChartAreas.Add(chartArea1);
			this.chart1.ChartAreas.Add(chartArea2);
			legend1.Name = "Legend1";
			legend2.Enabled = false;
			legend2.Name = "Legend2";
			this.chart1.Legends.Add(legend1);
			this.chart1.Legends.Add(legend2);
			this.chart1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 110, true);
			this.chart1.Name = "chart1";
			series1.ChartArea = "ChartArea1";
			series1.Legend = "Legend1";
			series1.Name = "Network Delay";
			series1.Points.Add(dataPoint1);
			series2.ChartArea = "ChartArea2";
			series2.Legend = "Legend2";
			series2.Name = "Throughput";
			series2.Points.Add(dataPoint2);
			this.chart1.Series.Add(series1);
			this.chart1.Series.Add(series2);
			this.chart1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 277, true);
			this.chart1.TabIndex = 0;
			title1.Name = "Title1";
			this.chart1.Titles.Add(title1);
			// 
			// button1
			// 
			this.button1.Text = Start;
			this.button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 60, true);
			this.button1.Name = "button1";
			this.button1.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 36, true);
			this.button1.TabIndex = 1;
			this.button1.ToolTipCaption = null;
			this.button1.UseVisualStyleBackColor = true;
			this.button1.AutoEllipsis = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Text = Start;
			this.button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 60, true);
			this.button2.Name = "button2";
			this.button2.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.button2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 36, true);
			this.button2.TabIndex = 2;
			this.button2.ToolTipCaption = null;
			this.button2.UseVisualStyleBackColor = true;
			this.button2.AutoEllipsis = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 18, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 25, true);
			this.label1.TabIndex = 3;
			this.label1.Text = "Network Delay Monitor";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 18, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 25, true);
			this.label2.TabIndex = 4;
			this.label2.Text = "Throughput Test";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// radioButton1
			// 
			this.radioButton1.AutoCheck = false;
			this.radioButton1.AutoSize = true;
			this.radioButton1.CaptionResourceString = CargoWise.Main.Res.GetData("5CF24148-78F5-4051-95DE-F1DCE54F9083", "1 MB");
			this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 60, true);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 14, true);
			this.radioButton1.TabIndex = 5;
			this.radioButton1.TabStop = true;
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoCheck = false;
			this.radioButton2.AutoSize = true;
			this.radioButton2.CaptionResourceString = CargoWise.Main.Res.GetData("A0F83676-C55A-4EA3-911B-9A1F73D35E47", "10 MB");
			this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(537, 77, true);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 14, true);
			this.radioButton2.TabIndex = 6;
			this.radioButton2.TabStop = true;
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// NetworkMonitorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("852F2401-584B-45A9-9015-A71ACA1CFFB0", "Network Monitor Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(796, 421, true);
			this.Controls.Add(this.radioButton2);
			this.Controls.Add(this.radioButton1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.chart1);
			this.Name = "NetworkMonitorForm";
			this.Controls.SetChildIndex(this.chart1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.button1, 0);
			this.Controls.SetChildIndex(this.button2, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.radioButton1, 0);
			this.Controls.SetChildIndex(this.radioButton2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KChart chart1;
		private ZButton button1;
		private ZButton button2;
		private KLabel label1;
		private KLabel label2;
		private ZRadioButton radioButton1;
		private ZRadioButton radioButton2;
	}
}
