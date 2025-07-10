using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.Tests
{
	[TestedType(typeof(ClientLinkModal))]
	public class ClientLinkModalBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ClientLinkModal("windowTitle");
		}
	}
}
