namespace Enterprise.FaxRouter.Processor
{
	public partial class EventProgress
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ActionMessageLabel = new CargoWise.Windows.UI.KLabel();
			this.SuspendLayout();
			// 
			// ActionMessageLabel
			// 
			this.ActionMessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ActionMessageLabel.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.ActionMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 16, true);
			this.ActionMessageLabel.Name = "ActionMessageLabel";
			this.ActionMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 23, true);
			this.ActionMessageLabel.TabIndex = 2;
			// 
			// EventProgress
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 54, true);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.ActionMessageLabel });
			this.Name = "EventProgress";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "EDI Fax Gateway";
			this.ResumeLayout(false);
		}

		#endregion

		public System.Windows.Forms.Label ActionMessageLabel;
	}
}
