using System.Collections.Specialized;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(AlertForm))]
	internal class AlertFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AlertForm(new Alert(new StringCollection(), Factory));
		}
	}
}
