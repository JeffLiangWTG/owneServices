namespace Enterprise.VisualBoards.GUI
{
	partial class VisualBoardForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.VisualBoardTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.SystemUpdateCompleteLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 705, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.VisualBoards.Business.BoardViewModel);
			// 
			// SystemUpdateCompleteLabel
			// 
			this.SystemUpdateCompleteLabel.BackColor = System.Drawing.Color.Transparent;
			this.SystemUpdateCompleteLabel.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("7ebcbf74-0447-4165-b115-2177c387bef9", "This board will load when the timer reaches zero. Click here to refresh this board now.");
			this.SystemUpdateCompleteLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SystemUpdateCompleteLabel.Font = new System.Drawing.Font(Font.FontFamily, 12f);
			this.SystemUpdateCompleteLabel.Name = "SystemUpdateCompleteLabel";
			this.SystemUpdateCompleteLabel.TabIndex = 1;
			this.SystemUpdateCompleteLabel.TabStop = false;
			this.SystemUpdateCompleteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.SystemUpdateCompleteLabel.Visible = false;

			// 
			// VisualBoardTableLayoutPanel
			// 
			this.VisualBoardTableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
			this.VisualBoardTableLayoutPanel.ColumnCount = 2;
			this.VisualBoardTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.VisualBoardTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.VisualBoardTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisualBoardTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VisualBoardTableLayoutPanel.Name = "VisualBoardTableLayoutPanel";
			this.VisualBoardTableLayoutPanel.RowCount = 2;
			this.VisualBoardTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.VisualBoardTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.VisualBoardTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 729, true);
			this.VisualBoardTableLayoutPanel.TabIndex = 2;
			this.VisualBoardTableLayoutPanel.Visible = false;
			// 
			// VisualBoardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 729, true);
			this.Controls.Add(this.SystemUpdateCompleteLabel);
			this.Controls.Add(this.VisualBoardTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.VisualBoards.Business.BoardViewModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "VisualBoardForm";
			this.ResizeBegin += new System.EventHandler(this.VisualBoardForm_ResizeBegin);
			this.ResizeEnd += new System.EventHandler(this.VisualBoardForm_ResizeEnd);
			this.Controls.SetChildIndex(this.VisualBoardTableLayoutPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		public CargoWise.Windows.UI.KTableLayoutPanel VisualBoardTableLayoutPanel;
		Enterprise.ZArchitecture.ZLabel SystemUpdateCompleteLabel;
	}
}