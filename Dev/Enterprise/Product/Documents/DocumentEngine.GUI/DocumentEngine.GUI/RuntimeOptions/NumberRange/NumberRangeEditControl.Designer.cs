namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.NumberRange
{
	partial class NumberRangeEditControl
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
			this.editTo = new Enterprise.ZArchitecture.ZCalcEdit();
			this.editFrom = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SuspendLayout();
			// 
			// oCalcEditTo
			//
			this.editTo.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("NumberRangeUserControl|a3527dfc-2465-4f55-859e-1757bb0f7d42", "To");
			this.editTo.DecimalPlaces = 0;
			this.editTo.Decimals = 0;
			this.editTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 3, true);
			this.editTo.Name = "oCalcEditTo";
			this.editTo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.editTo.TabIndex = 1;
			this.editTo.Text = "0";
			this.editTo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// oCalcEditFrom
			//
			this.editFrom.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("NumberRangeUserControl|756428ac-c131-47bb-abd1-3bb2fca0e18d", "From");
			this.editFrom.DecimalPlaces = 0;
			this.editFrom.Decimals = 0;
			this.editFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 3, true);
			this.editFrom.Name = "oCalcEditFrom";
			this.editFrom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.editFrom.TabIndex = 0;
			this.editFrom.Text = "0";
			this.editFrom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberRangeEditControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.editFrom);
			this.Controls.Add(this.editTo);
			this.Name = "NumberRangeEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 27, true);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZCalcEdit editTo;
		public ZArchitecture.ZCalcEdit editFrom;
	}
}
