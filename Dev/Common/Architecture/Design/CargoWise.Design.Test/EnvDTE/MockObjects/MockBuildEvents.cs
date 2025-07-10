
namespace CargoWise.Design.DTE.Testing
{
	class MockBuildEvents : EnvDTE.BuildEvents
	{
		#region _dispBuildEvents_Event Members

		event EnvDTE._dispBuildEvents_OnBuildBeginEventHandler EnvDTE._dispBuildEvents_Event.OnBuildBegin
		{
			add { }
			remove { }
		}

		event EnvDTE._dispBuildEvents_OnBuildDoneEventHandler EnvDTE._dispBuildEvents_Event.OnBuildDone
		{
			add { }
			remove { }
		}

		event EnvDTE._dispBuildEvents_OnBuildProjConfigBeginEventHandler EnvDTE._dispBuildEvents_Event.OnBuildProjConfigBegin
		{
			add { }
			remove { }
		}

		event EnvDTE._dispBuildEvents_OnBuildProjConfigDoneEventHandler EnvDTE._dispBuildEvents_Event.OnBuildProjConfigDone
		{
			add { }
			remove { }
		}

		#endregion
	}
}
