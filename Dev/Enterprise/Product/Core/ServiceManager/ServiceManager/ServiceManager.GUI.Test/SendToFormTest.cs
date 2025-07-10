using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(SendToForm))]
	public class SendToFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SendToForm();
		}
	}
}
