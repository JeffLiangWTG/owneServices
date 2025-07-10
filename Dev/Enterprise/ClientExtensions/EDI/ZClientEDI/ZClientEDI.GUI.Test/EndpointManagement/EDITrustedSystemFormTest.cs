using System.Windows.Forms;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.EndpointManagement.GUI.Testing
{
	[TestedType(typeof(EdiTrustedSystemForm))]
	public class EdiTrustedSystemFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			return new EdiTrustedSystemForm(sys);
		}
	}
}
