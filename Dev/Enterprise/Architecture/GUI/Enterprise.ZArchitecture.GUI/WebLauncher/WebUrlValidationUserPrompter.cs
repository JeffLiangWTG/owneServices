using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.WebLauncher
{
	public class WebUrlValidationUserPrompter : IWebUrlValidationUserPrompter
	{
		public bool GetUserConfirmation(string url)
		{
			if (Globals.CanShowDialogs)
			{
				var result = Globals.Message.Show(Res.GetString("6CC22A84-45F6-4114-B5C5-472F04CCCF88",
@"The following link leads to an external file and may be potentially unsafe:

{0}

Are you sure to sure to open the file?", url),
				Res.GetString("355351EB-42ED-4266-9B38-98A5E844DFA9", "Unsafe File"),
				MessageBoxButtons.OKCancel, DialogResult.Cancel);
				return result == DialogResult.OK;
			}

			return false;
		}
	}
}
