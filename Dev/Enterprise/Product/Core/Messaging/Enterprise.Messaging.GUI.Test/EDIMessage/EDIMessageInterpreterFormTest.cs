using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Testing
{
	[TestedType(typeof(EDIMessageInterpreterForm))]
	sealed class EDIMessageInterpreterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EDIMessageInterpreterForm(new EDIMessageInterpreter());
		}
	}
}
