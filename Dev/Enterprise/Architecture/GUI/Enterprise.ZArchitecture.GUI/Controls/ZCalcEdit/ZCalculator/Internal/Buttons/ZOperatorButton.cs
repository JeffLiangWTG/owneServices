using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class ZOperatorButton : ZCalculatorButton
	{
		public ZOperatorButton()
		{
			fKey = Keys.None;
		}

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(Keys.None)]
		public Keys Key
		{
			get { return fKey;  }
			set { fKey = value; }
		}

		Keys fKey;
	}
}