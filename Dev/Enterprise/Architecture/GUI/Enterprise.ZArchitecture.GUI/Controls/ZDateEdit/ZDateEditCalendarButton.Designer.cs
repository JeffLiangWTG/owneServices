using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class ZDateEditCalendarButton
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZDateEditCalendarButton));
			// 
			// CalendarButton
			// 
			this.Image = ((System.Drawing.Image)(resources.GetObject("CalendarButton.Image")));
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Name = "CalendarButton";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 21, true);
			this.TabIndex = 8;
			this.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.UseVisualStyleBackColor = true;
		}

		#endregion

	}
}
