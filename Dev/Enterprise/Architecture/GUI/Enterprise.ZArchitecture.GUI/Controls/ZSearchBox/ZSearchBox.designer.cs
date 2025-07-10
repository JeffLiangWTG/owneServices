namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	partial class ZSearchBox
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
			searchResultsDisplay.Dispose();

			#if !WINZOR
			globalMouseHook.MouseDown -= CloseResultsWhenMouseClicksOutsideResults;
			globalMouseHook.Uninstall();
			globalMouseHook.Dispose();
			#endif

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pnSearch = new CargoWise.Windows.UI.KPanel();
			this.btnSearch = new CargoWise.Windows.UI.KButton();
			this.pnSearchBox = new CargoWise.Windows.UI.KPanel();
			this.tbSearch = new CargoWise.Windows.UI.KTextBox();
			this.searchResultsDisplay = new Enterprise.ZArchitecture.GUI.SearchBox.ZSearchBoxResults();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.pnSearch.SuspendLayout();
			this.pnSearchBox.SuspendLayout();
			this.searchResultsDisplay.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnSearch
			// 
			this.pnSearch.BackColor = System.Drawing.Color.White;
			this.pnSearch.Controls.Add(this.btnSearch);
			this.pnSearch.Controls.Add(this.pnSearchBox);
			this.pnSearch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnSearch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.pnSearch.Name = "pnSearch";
			this.pnSearch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 30, true);
			this.pnSearch.TabIndex = 7;
			// 
			// btnSearch
			// 
			this.btnSearch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnSearch.BackColor = System.Drawing.Color.LightGray;
			this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.btnSearch.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnSearch.FlatAppearance.BorderSize = 0;
			this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSearch.Font = new System.Drawing.Font("Segoe UI Symbol", 11F);
			this.btnSearch.ForeColor = System.Drawing.Color.Black;
			this.btnSearch.IsCaptionOverridden = true;
			this.btnSearch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 0, true);
			this.btnSearch.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 30, true);
			this.btnSearch.TabIndex = 7;
			this.btnSearch.Text = "🔎";
			this.btnSearch.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			this.btnSearch.ToolTipCaption = null;
			this.btnSearch.UseCompatibleTextRendering = true;
			this.btnSearch.UseVisualStyleBackColor = false;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// pnSearchBox
			// 
			this.pnSearchBox.BackColor = System.Drawing.Color.Transparent;
			this.pnSearchBox.Controls.Add(this.tbSearch);
			this.pnSearchBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnSearchBox.ForeColor = System.Drawing.Color.Black;
			this.pnSearchBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pnSearchBox.Name = "pnSearchBox";
			this.pnSearchBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.pnSearchBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 30, true);
			this.pnSearchBox.TabIndex = 9;

			// 
			// tbSearch
			// 
			this.tbSearch.BackColor = System.Drawing.Color.White;
			this.tbSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tbSearch.Dock = System.Windows.Forms.DockStyle.Left;
			this.tbSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tbSearch.ForeColor = System.Drawing.Color.Black;
			this.tbSearch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.tbSearch.MaxLength = 256;
			this.tbSearch.Name = "tbSearch";
			this.tbSearch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 16, true);
			this.tbSearch.TabIndex = 6;
			this.tbSearch.Text = "Search";
			this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
			this.tbSearch.Enter += new System.EventHandler(this.tbSearch_Enter);
			this.tbSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbSearch_KeyDown);
			this.tbSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbSearch_KeyPress);
			this.tbSearch.PlaceHolderText = "Search";
			// 
			// searchResultsDisplay
			// 
			this.searchResultsDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.searchResultsDisplay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.searchResultsDisplay.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.searchResultsDisplay.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 32, true);
			this.searchResultsDisplay.Name = "searchResultsDisplay";
			this.searchResultsDisplay.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.searchResultsDisplay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 256, true);
			this.searchResultsDisplay.TabIndex = 0;
			// 
			// ZSearchBox
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.pnSearch);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.Black;
			this.Name = "ZSearchBox";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 32, true);
			this.BackColorChanged += new System.EventHandler(this.SearchBox_BackColorChanged);
			this.EnabledChanged += new System.EventHandler(this.SearchBox_EnabledChanged);
			this.ForeColorChanged += new System.EventHandler(this.SearchBox_ForeColorChanged);
			this.Click += new System.EventHandler(this.SearchBox_Click);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.SearchBox_Layout);
			this.Leave += new System.EventHandler(this.SearchBox_Leave);
			this.MouseEnter += new System.EventHandler(this.SearchBox_MouseEnter_SetHoverColour);
			this.MouseLeave += new System.EventHandler(this.SearchBox_MouseLeave_ClearHoverColour);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.pnSearch.ResumeLayout(false);
			this.pnSearch.PerformLayout();
			this.pnSearchBox.ResumeLayout(false);
			this.pnSearchBox.PerformLayout();
			this.searchResultsDisplay.ResumeLayout(true);
			this.searchResultsDisplay.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZSearchBoxResults searchResultsDisplay;
		protected internal CargoWise.Windows.UI.KButton btnSearch;
		protected internal CargoWise.Windows.UI.KTextBox tbSearch;
		protected internal CargoWise.Windows.UI.KPanel pnSearchBox;
		protected internal CargoWise.Windows.UI.KPanel pnSearch;
	}
}
