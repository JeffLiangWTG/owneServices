using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	[TestedType(typeof(EDIAdministrationPanelForm))]
	public class EDIAdministrationPanelFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EDIAdministrationPanelForm(new AdministrationPanelManager(Factory));
		}
	}
}
