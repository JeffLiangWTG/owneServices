using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Reversing.Testing;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.Testing
{
	public class ContraReversingTest : PayablesAndReceivablesReversingTest
	{
		public void TestCanReverseTransaction()
		{
			var creator = new TestObjectCreator(Factory);
			Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;
			Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;

			var contra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			contra.AH_APAccount = creator.Creditor1.PK;
			contra.AH_ARAccount = creator.Debtor1.PK;

			var contraReverseing = new ContraReversing(contra);
			AssertEquals(string.Empty, contraReverseing.CantReverseErrorMessage);
			Assert(contraReverseing.CanReverseTransaction);

			creator.Creditor1.MiscServ.OM_ARConsolidatedAccountingCategory = Enterprise.Core.Constants.AccountsCategory.WhollyOwned;
			var expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> New Transactions -> Allow Contra Transactions with different account consolidation category class";

			AssertEquals(expectedMessage, contraReverseing.CantReverseErrorMessage);
			Assert(!contraReverseing.CanReverseTransaction);
		}
	}
}
