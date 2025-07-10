using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentScanning.OCR
{
	public partial class CursorConstrainingForm : KForm
	{
		void InitializeComponent()
		{
			// 
			// CursorConstrainingForm
			// 
			this.AutoScaleMode = AutoScaleMode.None;

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 1, true);
			this.ControlBox = false;
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 0.1F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 1, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 1, true);
			this.Name = "CursorConstrainingForm";
			this.Opacity = 0;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.CursorConstrainingForm_Load);
			this.Activated += new System.EventHandler(this.CursorConstrainingForm_Activated);
		}
	}
}
