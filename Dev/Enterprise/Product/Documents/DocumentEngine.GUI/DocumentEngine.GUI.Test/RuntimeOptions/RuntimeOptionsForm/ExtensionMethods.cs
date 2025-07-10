using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	static class ExtensionMethods
	{
		internal static List<TForm> FindAll<TForm>(this FormCollection forms) where TForm : Form
		{
			List<TForm> result = new List<TForm>();

			foreach (Form form in forms)
			{
				TForm resultForm = form as TForm;

				if (resultForm != null)
				{
					result.Add(resultForm);
				}
			}

			return result;
		}
	}
}
