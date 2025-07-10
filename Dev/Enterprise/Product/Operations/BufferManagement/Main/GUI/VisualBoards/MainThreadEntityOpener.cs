using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public static class MainThreadEntityOpener
	{
		public static void OpenEntityEditFormOnMainThread<TBizo>(TBizo bizo, ControllerID controllerID)
			where TBizo : BusinessObject
		{
			if (!bizo.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("f7669cce-5ffd-4616-a63d-b95cdfe9f4fd", "Please save the form first."));
			}
			else
			{
				var bizoPK = bizo.PK;

				MainThreadRunner.RunOnMainThread(() =>
				{
					var factory = new BusinessObjectFactory { NameForDebugging = nameof(MainThreadEntityOpener) };
					var loadedBizo = factory.Load<TBizo>(bizoPK);

					if (loadedBizo != null)
					{
						IZForm form;

						if (controllerID == ControllerIDs.ProcessHeader)
						{
							// Ensures the right workflow is selected when the form is shown.
							form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedBizo);
						}
						else
						{
							form = ZControllerFactory.Create(controllerID).ShowEditForm(loadedBizo);
						}

						((ZForm)form)?.Activate();
					}
				});
			}
		}
	}
}
