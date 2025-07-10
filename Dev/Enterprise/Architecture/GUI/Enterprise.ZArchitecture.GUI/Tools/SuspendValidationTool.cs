using System.Windows.Forms;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DevTools
{
	public class SuspendValidationTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name
		{
			get { return "Suspend/Resume Validation"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public void Show(Form form)
		{
			var oWinForm = form as ZForm;
			var bizO = oWinForm != null ? oWinForm.BusinessEntity : null;
			var factory = bizO != null ? bizO.Factory : null;

			if (factory != null)
			{
				if (factory.IsValidationSuspended)
				{
					oWinForm.NoValidationOnSave = false;
					factory.ResumeValidation();
					Globals.Message.ShowInformation(string.Format("Validation on factory (Instance {0}, Name {1}) has been resumed.", factory._Instance, factory.NameForDebugging));
				}
				else
				{
					oWinForm.NoValidationOnSave = true;
					factory.SuspendValidation();
					Globals.Message.ShowInformation(string.Format("Validation on factory (Instance {0}, Name {1}) has been suspended.", factory._Instance, factory.NameForDebugging));
				}
			}
		}
	}
}
