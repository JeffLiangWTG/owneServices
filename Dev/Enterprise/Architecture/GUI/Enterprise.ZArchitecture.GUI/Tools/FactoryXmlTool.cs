using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	class FactoryXmlTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name
		{
			get { return "Factory Contents as XML"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public void Show(Form form)
		{
			var zWinForm = form as ZForm;
			IBusiness business;
			string message;

			if (zWinForm == null || (business = zWinForm.DataSource as IBusiness) == null || business.Factory == null)
			{
				message = "Unable to find a factory.";
			}
			else
			{
				message = ((IBusinessObjectFactoryInternals)business.Factory).ContentsAsXMLForDebugging;
			}

			Globals.Message.ShowInformation(message, "Factory contents as XML");
		}
	}
}