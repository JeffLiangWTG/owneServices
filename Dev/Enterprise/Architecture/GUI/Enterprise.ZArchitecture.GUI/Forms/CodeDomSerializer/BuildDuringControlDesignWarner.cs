using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal class BuildDuringControlDesignWarner
	{
		public BuildDuringControlDesignWarner(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public void EnsureWarnOfBuildDuringDesign()
		{
			BuildEvents.OnBuildBegin -= new EnvDTE._dispBuildEvents_OnBuildBeginEventHandler(BuildEvents_OnBuildBegin);
			BuildEvents.OnBuildBegin += new EnvDTE._dispBuildEvents_OnBuildBeginEventHandler(BuildEvents_OnBuildBegin);
		}

		#region Implementation

		readonly IServiceProvider serviceProvider;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "This code is run in the Visual Studio process")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant, This code is run in the Visual Studio process, Run in Visual Studio")]
		void BuildEvents_OnBuildBegin(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
		{
			if (DTE != null)
			{
				foreach (EnvDTE.Window window in DTE.Windows)
				{
					if (window.Visible && window.Caption.Contains("[Design]"))
					{
						DTE.ExecuteCommand("Build.Cancel", "");
						MessageBox.Show(
							"You can't build at this time, otherwise controls may disappear from your form or user control.\r\nClose all designed Windows Forms controls and try again.\r\n\r\nVote Clinty");
					}
				}
			}
		}

		EnvDTE.DTE DTE
		{
			get { return (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE)); }
		}

		EnvDTE.Events Events
		{
			get { return events ?? (events = DTE.Events); }
		}
		EnvDTE.Events events;

		EnvDTE.BuildEvents BuildEvents
		{
			get { return buildEvents ?? (buildEvents = Events.BuildEvents); }
		}
		EnvDTE.BuildEvents buildEvents;

		#endregion
	}
}
