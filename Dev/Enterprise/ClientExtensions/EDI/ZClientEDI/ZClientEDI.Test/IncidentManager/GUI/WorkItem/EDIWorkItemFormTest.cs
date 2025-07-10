using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(EDIWorkItemForm))]
	public class EDIWorkItemFormTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var workItem = Factory.New<EDIWorkItem>();
			var result = new EDIWorkItemForm(workItem);
			result.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1524);
			result.ControllerID = ControllerIDs.WorkItem;
			return result;
		}

		#endregion
	}
}
