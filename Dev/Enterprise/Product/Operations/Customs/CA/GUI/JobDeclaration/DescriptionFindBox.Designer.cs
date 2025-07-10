using System.Drawing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	partial class DescriptionFindBox
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
			this.DescriptionBox = new ZTextBox.Bare();
			this.SuspendLayout();
			// 
			// DescriptionBox
			// 
			this.DescriptionBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.DescriptionBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.DescriptionBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DescriptionBox.Name = "DescriptionBox";
			this.DescriptionBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.DescriptionBox.TabIndex = 0;
			this.DescriptionBox.Text = "";
			this.DescriptionBox.Validated += new System.EventHandler(DescriptionBox_Validated);
			this.DescriptionBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(DescriptionBox_KeyPress);
			// 
			// PopupButton
			// 
			this.PopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 0, true);
			this.PopupButton.TabIndex = 1;
			// 
			// CodeBox
			// 
			this.CodeBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 0, true);
			this.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.CodeBox.TabIndex = 2;
			this.CodeBox.Validated += new System.EventHandler(CodeBox_Validated);
			// 
			// ZCodeFindBox
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionBox);
			this.Name = "ZCodeFindBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.Controls.SetChildIndex(this.DescriptionBox, 0);
			this.Controls.SetChildIndex(this.PopupButton, 0);
			this.Controls.SetChildIndex(this.CodeBox, 0);
			this.ResumeLayout(false);
		}

		public Enterprise.ZArchitecture.ZTextBox DescriptionBox;

		#endregion
	}
}
