using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Modules.AirCargo.Testing
{
	[TestedType(typeof(DeveloperToolsForm))]
	public class DeveloperToolsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DeveloperToolsForm();
		}
	}
}
