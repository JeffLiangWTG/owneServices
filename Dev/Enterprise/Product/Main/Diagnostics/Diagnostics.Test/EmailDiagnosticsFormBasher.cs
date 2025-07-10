using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(EmailDiagnosticsForm))]
	public class EmailDiagnosticsFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EmailDiagnosticsForm();
		}
	}
}
