using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1017 : Control
	{
		public void CW1017Method()
		{
			//CW1017:Non DPI-aware code has been detected
			this.Left = 0;
			this.Top = 0;
			this.Height = 0;
			this.Width = 0;
			this.Location = new System.Drawing.Point(0, 0);
			this.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
			this.ClientSize = new System.Drawing.Size(0, 0);
			this.Margin = new Padding(0, 0, 0, 0);
			this.Size = new System.Drawing.Size(0, 0);
		}
	}
}
