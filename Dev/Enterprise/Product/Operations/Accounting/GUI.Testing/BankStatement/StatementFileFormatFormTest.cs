using System.Windows.Forms;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.BankStatement.Testing
{
	[TestedType(typeof(StatementFileFormatForm))]
	public class StatementFileFormatFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new StatementFileFormatForm(new BankStatementFormat(Factory));
		}
	}
}
