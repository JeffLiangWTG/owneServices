using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.CreditControl.GUI.Testing
{
	[TestedType(typeof(DocumentLoginForm))]
	sealed class DocumentLoginFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			SecurityLogin login = new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
			return new DocumentLoginForm(login);
		}
	}
}
