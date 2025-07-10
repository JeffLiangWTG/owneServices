
namespace Enterprise.Client.JAS.GUI.Cognos
{
	partial class CognosCsvExportForm
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
			this.SummaryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExportProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EndPeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.jasDataExporterUserControl1 = new Enterprise.Client.JAS.GUI.JASDataExporterUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 502, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.JAS.Business.Cognos.CognosDataExporterBizO);
			// 
			// SummaryTextBox
			// 
			this.BindingSource.SetBindingMember(this.SummaryTextBox, "ExportSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.JAS.Business.Cognos.CognosDataExporterBizO)(null)).ExportSummary)));
			this.SummaryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SummaryTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SummaryTextBox.Multiline = true;
			this.SummaryTextBox.Name = "SummaryTextBox";
			this.SummaryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 279, true);
			this.SummaryTextBox.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 471, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ExportButton
			// 
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 471, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.ExportButton.TabIndex = 2;
			this.ExportButton.Text = "Export";
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.ExportProgressBar);
			this.zGroupBox1.Controls.Add(this.SummaryTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 138, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 327, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Export Summary";
			// 
			// ExportProgressBar
			// 
			this.ExportProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExportProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 301, true);
			this.ExportProgressBar.Name = "ExportProgressBar";
			this.ExportProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 23, true);
			this.ExportProgressBar.TabIndex = 1;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.EndPeriodCalcEdit);
			this.zGroupBox2.Controls.Add(this.zLabel1);
			this.zGroupBox2.Controls.Add(this.jasDataExporterUserControl1);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 11, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 121, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Export Options";
			// 
			// EndPeriodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EndPeriodCalcEdit, "EndingPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.JAS.Business.Cognos.CognosDataExporterBizO)(null)).EndingPeriod)));
			this.EndPeriodCalcEdit.Decimals = 0;
			this.EndPeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 14, true);
			this.EndPeriodCalcEdit.Name = "EndPeriodCalcEdit";
			this.EndPeriodCalcEdit.ShowGroupSeparators = false;
			this.EndPeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.EndPeriodCalcEdit.TabIndex = 0;
			this.EndPeriodCalcEdit.Text = "0";
			this.EndPeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "End Period";
			// 
			// jasDataExporterUserControl1
			// 
			this.BindingSource.SetBindingMember(this.jasDataExporterUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.JAS.Business.JASDataExporterBizO)(((Enterprise.Client.JAS.Business.Cognos.CognosDataExporterBizO)(null)))));
			this.jasDataExporterUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.jasDataExporterUserControl1.Name = "jasDataExporterUserControl1";
			this.jasDataExporterUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 75, true);
			this.jasDataExporterUserControl1.TabIndex = 1;
			// 
			// CognosCsvExportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 526, true);
			this.ControlBox = false;
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "ZClientJAS";
			this.DataSourceType = typeof(Enterprise.Client.JAS.Business.Cognos.CognosDataExporterBizO);
			this.DataSourceTypeName = "Enterprise.Client.JAS.Business.Cognos.CognosDataExporterBizO";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CognosCsvExportForm";
			this.Text = "CognosCsvExportForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal JASDataExporterUserControl jasDataExporterUserControl1;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ExportButton;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		internal Enterprise.ZArchitecture.ZTextBox SummaryTextBox;
		internal CargoWise.Windows.UI.KProgressBar ExportProgressBar;
		internal Enterprise.ZArchitecture.ZLabel zLabel1;
		internal Enterprise.ZArchitecture.ZCalcEdit EndPeriodCalcEdit;
	}
}
