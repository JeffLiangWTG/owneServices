namespace Enterprise.DataTransfer.GUI
{
	public partial class DataExportForm
	{
		#region Windows Form Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataExportForm));
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExportOutputPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OutputTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportOutputPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 74, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 52, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 22, true);
			// 
			// EnterpriseLogo
			// 
			this.EnterpriseLogo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EnterpriseLogo.Image = ((System.Drawing.Image)(resources.GetObject("EnterpriseLogo.Image")));
			this.EnterpriseLogo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			// 
			// CancelProgressButton
			// 
			this.CancelProgressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelProgressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 1, true);
			this.CancelProgressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 15, true);
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 7, true);
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 30, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 337, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 7, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(329);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(329);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataTransfer.Business.FlatFileDataExporter);
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportButton.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataExportForm|b79cbe58-3297-47ed-8549-afa4a991f154", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 215, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ExportButton.TabIndex = 0;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// ExportOutputPanel
			// 
			this.ExportOutputPanel.Controls.Add(this.OutputTextbox);
			this.ExportOutputPanel.Controls.Add(this.CloseButton);
			this.ExportOutputPanel.Controls.Add(this.ExportButton);
			this.ExportOutputPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 74, true);
			this.ExportOutputPanel.Name = "ExportOutputPanel";
			this.ExportOutputPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 245, true);
			this.ExportOutputPanel.TabIndex = 6;
			// 
			// OutputTextbox
			// 
			this.OutputTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutputTextbox, false);
			this.OutputTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OutputTextbox.Multiline = true;
			this.OutputTextbox.Name = "OutputTextbox";
			this.OutputTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 201, true);
			this.OutputTextbox.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataExportForm|7406bf24-fb2a-4dc8-995b-d6fcfd356fda", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 215, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DataExportForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 344, true);
			this.Controls.Add(this.ExportOutputPanel);
			this.DataSourceAssemblyName = "Enterprise.DataTransfer";
			this.DataSourceType = typeof(Enterprise.DataTransfer.Business.FlatFileDataExporter);
			this.DataSourceTypeName = "Enterprise.DataTransfer.Business.FlatFileDataExporter";
			this.Name = "DataExportForm";
			this.Text = "DataExportForm";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ExportOutputPanel, 0);
			this.MainPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportOutputPanel.ResumeLayout(false);
			this.ExportOutputPanel.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel ExportOutputPanel;
		protected Enterprise.ZArchitecture.ZTextBox OutputTextbox;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.GUI.ZButton ExportButton;
	}
}
