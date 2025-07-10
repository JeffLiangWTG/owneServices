using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class EDITaskWithDetailsAndFilterTabTest : TaskWithDetailsAndFilterTabTest
	{
		protected override Type GetTypeOfFilterControl()
		{
			return typeof(EDITaskWithDetailsAndFilterControl);
		}

		protected override TaskWithDetailsAndFilterTab GetNewTaskWithDetailsAndFilterTab()
		{
			return new EDITaskWithDetailsAndFilterTab();
		}

		protected override ZChildForm CreateForm()
		{
			var form = new ZChildForm(Factory.New<NewWorkItem>());
			form.ControllerID = DummyControllerIDs.Dummy;
			return form;
		}
	}
}
