namespace Enterprise.BufferManagement.GUI
{
	sealed partial class AcceptabilityBandTileControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.NameAndResultLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SuspendLayout();
			// 
			// NameAndResultLabel
			// 
			this.NameAndResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.NameAndResultLabel.AutoSize = true;
			this.NameAndResultLabel.BackColor = System.Drawing.Color.Transparent;
			this.NameAndResultLabel.IsFontBold = true;
			this.NameAndResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NameAndResultLabel.Name = "NameAndResultLabel";
			this.NameAndResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 19, true);
			this.NameAndResultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NameAndResultLabel.TabIndex = 0;
			// 
			// AcceptabilityBandTileControl
			// 
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.NameAndResultLabel);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 5, 0, true);
			this.Name = "AcceptabilityBandTileControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 28, true);
			this.ResumeLayout(false);

		}

		#endregion

		private System.ComponentModel.IContainer components;
		public ZArchitecture.ZLabel NameAndResultLabel;
	}
}
