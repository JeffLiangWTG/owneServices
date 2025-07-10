using System.Drawing;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public class GLNumberDot : ZLabel
	{
		public GLNumberDot()
		{
			Font = new Font(Font.Name, 30, FontStyle.Bold);
			AutoSize = true;
			Text = ".";
		}
	}
}
