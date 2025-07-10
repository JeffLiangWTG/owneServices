using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	class EDITaskWithDetailsAndFilterControlBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dummy = Factory.New<NewWorkItem>();
			ZForm result = new ZForm(dummy);
			result.CaptionRenderingEnabled = true;
			EDITaskWithDetailsAndFilterControl control = new EDITaskWithDetailsAndFilterControl();
			control.BindToFilter = "";
			control.BindToGrid = "TasksView";
			control.SetDataBinding(dummy, "");
			result.Height = control.Height + 100;
			result.Width = control.Width + 100;
			result.Controls.Add(control);
			result.ControllerID = DummyControllerIDs.Dummy;
			return result;
		}
	}
}
