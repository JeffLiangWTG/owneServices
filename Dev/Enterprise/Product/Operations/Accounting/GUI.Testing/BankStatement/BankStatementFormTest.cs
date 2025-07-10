using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.BankStatement.Testing
{
	[TestedType(typeof(BankStatementForm))]
	public class BankStatementFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public class MockBankStatementForm : BankStatementForm
		{
			public MockBankStatementForm(Business.Base.AccStatement.BankStatement bankStatement)
				: base(bankStatement)
			{
			}

			public ZGrid StatementGrid_Exposed
			{
				get { return UnreconciledStatementsGrid; }
			}
		}

		protected override Form GetFormToBashCore()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			Business.Base.AccStatement.BankStatement testStatment = testFactory.New(typeof(Business.Base.AccStatement.BankStatement)) as Business.Base.AccStatement.BankStatement;

			return new BankStatementForm(testStatment);
		}

		public void TestFormUp()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			Business.Base.AccStatement.BankStatement testStatment = testFactory.New(typeof(Business.Base.AccStatement.BankStatement)) as Business.Base.AccStatement.BankStatement;

			using (MockBankStatementForm testForm = new MockBankStatementForm(testStatment))
			{
				testForm.Show();
				Assert(testForm.StatementGrid_Exposed.Visible);
			}
		}
	}
}
