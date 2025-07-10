namespace Enterprise.ZArchitecture.GUI
{
	partial class ZStmNotePopupEdit
	{
		protected internal ZNoteTextBox TextBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.TextBox = new ZNoteTextBox(this);
			this.SuspendLayout();
			// 
			// TextBox
			// 
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.TextBox.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right);
			this.TextBox.Name = "TextBox";
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.TextBox.TabIndex = 0;
			this.TextBox.Text = string.Empty;
			// 
			// ZStmNotePopupEdit
			// 
			this.Controls.Add(this.TextBox);
			this.DockPadding.Bottom = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
			this.Name = "ZStmNotePopupEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.ResumeLayout(false);
		}
	}
}