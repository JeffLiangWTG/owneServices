using CargoWise.Async;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.WorkflowManagement
{
	public class WorkflowParentFormGetter
	{
		public IZForm GetForm(IProcessHeader processHeader, IZForm form)
		{
			return MainThreadRunner.RunOnMainThreadSync(() =>
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkflowParentFormGetter) };

				if (processHeader.FH_P0_Template.IsValid)
				{
					var template = factory.Load<ProcessTaskTemplate>(processHeader.FH_P0_Template);
					if (template != null)
					{
						form = ZControllerFactory.Create(ControllerIDs.ProcessTemplates).ShowEditForm(template);
					}
				}
				else
				{
					var loadedProcessHeader = factory.Load<ProcessHeader>(processHeader.PK);
					form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedProcessHeader);
				}

				return form;
			});
		}
	}
}
