using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	partial class ZSearchBoxResults
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ItemCountDisplay = new CargoWise.Windows.UI.KLabel();
			this.SearchBoxResults = new Enterprise.ZArchitecture.GUI.SearchBox.ZSearchListBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ItemCountDisplay
			// 
			this.ItemCountDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.ItemCountDisplay.Dock = System.Windows.Forms.DockStyle.Top;
			this.ItemCountDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ItemCountDisplay.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ItemCountDisplay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ItemCountDisplay.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemCountDisplay.Name = "ItemCountDisplay";
			this.ItemCountDisplay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 20, true);
			this.ItemCountDisplay.TabIndex = 1;
			this.ItemCountDisplay.Text = "itemCount";
			this.ItemCountDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SearchBoxResults
			// 
			this.SearchBoxResults.AlternateItemColours = true;
			this.SearchBoxResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.SearchBoxResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.SearchBoxResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SearchBoxResults.DrawItemBorder = false;
			this.SearchBoxResults.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.SearchBoxResults.ForeColor = System.Drawing.Color.Black;
			this.SearchBoxResults.FormattingEnabled = true;
			this.SearchBoxResults.IntegralHeight = false;
			this.SearchBoxResults.ItemTextSpacing = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(4);
			this.SearchBoxResults.ItemBorder = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(0);
			this.SearchBoxResults.ItemHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(17);
			this.SearchBoxResults.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.SearchBoxResults.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SearchBoxResults.Name = "SearchBoxResults";
			this.SearchBoxResults.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 232, true);
			this.SearchBoxResults.TabIndex = 0;
			// 
			// ZSearchBoxResults
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.Controls.Add(this.SearchBoxResults);
			this.Controls.Add(this.ItemCountDisplay);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 48, true);
			this.Name = "ZSearchBoxResults";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 256, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal SearchBox.ZSearchListBox SearchBoxResults;
		protected internal KLabel ItemCountDisplay;
	}
}
