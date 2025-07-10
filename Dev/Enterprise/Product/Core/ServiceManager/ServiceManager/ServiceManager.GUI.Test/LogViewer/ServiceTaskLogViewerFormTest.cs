using System.Windows.Forms;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ServiceTaskLogViewerForm))]
	class ServiceTaskLogViewerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var logViewer = new ServiceTaskLogViewer();
			return new ServiceTaskLogViewerForm(logViewer);
		}
	}
}
