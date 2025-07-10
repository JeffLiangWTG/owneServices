using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	class AutoRatingExplorerTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public string Name
		{
			get { return "Autorating Explorer"; }
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
				var assemblyName = Path.Combine(EnvProxy.Instance.ApplicationStartupPath, "Enterprise.Rating.GUI.dll");
				var ratingAssembly = Assembly.LoadFrom(assemblyName);
				var autoRatingExplorerFormType = ratingAssembly.GetType("Enterprise.Rating.GUI.AutoRating.AutoRatingExplorerForm");
				var autoRatingExplorerForm = (Form)Activator.CreateInstance(autoRatingExplorerFormType, new object[] { zWinForm.GetTopLevelBusinessEntityForPlugIn() });
				autoRatingExplorerForm.Show();
			}
		}
	}
}