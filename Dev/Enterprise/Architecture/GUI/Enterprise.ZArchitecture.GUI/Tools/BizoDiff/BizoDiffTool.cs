using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	class BizoDiffTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name => "Bizo Diff Form";

		public bool AddAsButton => false;

		public void Show(Form form)
		{
			var zWinForm = form as ZForm;
			if (zWinForm != null)
			{
				zWinForm.ShowBizoDiffForm();
			}
		}
	}
}
