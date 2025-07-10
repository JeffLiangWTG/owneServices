using System.Windows;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZTimeZoneFindBox
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
			this.TimeZoneComboBox = new KComboBox();
			this.TimeZoneComboBox.SuspendLayout();
			this.SuspendLayout();
			//
			// timeZoneComboBox
			//
			this.TimeZoneComboBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.TimeZoneComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			//
			// PopupButton
			//
			this.PopupButton.Dock = System.Windows.Forms.DockStyle.Left;
			//
			// ZTimeZoneFindBox
			//
			this.Controls.Add(this.TimeZoneComboBox);
			this.Name = "ZTimeZoneFindBox";
			this.Controls.SetChildIndex(this.TimeZoneComboBox, 0);
			this.Controls.SetChildIndex(this.PopupButton, 0);
			this.ResumeLayout();
			this.ResumeLayout(false);
		}

		#endregion

		public KComboBox TimeZoneComboBox;
	}
}
