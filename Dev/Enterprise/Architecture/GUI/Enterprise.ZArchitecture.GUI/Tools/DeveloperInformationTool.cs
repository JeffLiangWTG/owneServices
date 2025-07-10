using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DevTools
{
	public class DeveloperInformationTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name => "Developer Information Form";

		public bool AddAsButton => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public void Show(Form form)
		{
			if (form is ZForm oWinForm)
			{
				Globals.Message.ShowInformation(FormDebugInfo.GetActiveControlInfoFromForm(oWinForm), "Developer Information");
			}
		}
	}
}
