using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.DevTools
{
	class BindingManagerTool : IDevTool
	{
		public string Name
		{
			get { return "Z-BindingManagerExplorer"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		public void Show(Form form)
		{
			var zWinForm = form as ZForm;

			if (zWinForm != null)
			{
				new ZCurrencyManagerExplorer((ZBindingContext)zWinForm.BindingContext).Show();
			}
		}
	}
}
