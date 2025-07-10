using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFormStrategyTest : TestCaseWithDummy
	{
		public void TestZFormAdornments()
		{
			using (var form = new ZForm(Dummy))
			{
				AssertNotNull("There should be status bar", form.MainStatusBar);
				AssertNotNull("There should be menu", form.Menu);
			}
		}

		public void TestSuppresseNewFormInTransactionWarning()
		{
			Assert(!ZFormStrategy.CanFormBeCreatedDuringDbTransaction(typeof(Form)));
			using (ZFormStrategy.SuppresseNewFormInTransactionWarning())
			{
				Assert(ZFormStrategy.CanFormBeCreatedDuringDbTransaction(typeof(Form)));
				using (ZFormStrategy.SuppresseNewFormInTransactionWarning())
				{
					Assert(ZFormStrategy.CanFormBeCreatedDuringDbTransaction(typeof(Form)));
				}
				Assert(ZFormStrategy.CanFormBeCreatedDuringDbTransaction(typeof(Form)));
			}
			Assert(!ZFormStrategy.CanFormBeCreatedDuringDbTransaction(typeof(Form)));
		}
	}
}
