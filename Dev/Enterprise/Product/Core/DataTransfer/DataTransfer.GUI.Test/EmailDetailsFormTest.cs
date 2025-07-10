using System.Windows.Forms;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.GUI.Testing
{
	[TestedType(typeof(EmailDetailsForm))]
	sealed class EmailDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ExportInstructions instructions = new ExportInstructions();
			return new EmailDetailsForm(instructions.EmailProperties);
		}
	}
}
