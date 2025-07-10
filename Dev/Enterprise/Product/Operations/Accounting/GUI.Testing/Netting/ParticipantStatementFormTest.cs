using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Netting.Testing
{
	[TestedType(typeof(ParticipantStatementForm))]
	public class ParticipantStatementFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ParticipantStatementForm();
		}
	}
}
