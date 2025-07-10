using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionZQueryTest : TestCaseWithFactory
	{
		public void TestQuery()
		{
			OperationalAction action1 = Factory.New<OperationalAction>();
			OperationalAction action2 = Factory.New<OperationalAction>();
			DocumentCommand document1 = Factory.New<DocumentCommand>();
			DocumentCommand document2 = Factory.New<DocumentCommand>();
			action1.SU_BusinessContext = OperationalAction.GetFullBusinessContext(BusinessContext.ARInvoice);
			action2.SU_BusinessContext = OperationalAction.GetFullBusinessContext(BusinessContext.BookingLoadList);
			action1.SU_GS_NKStaffCode = "";
			action2.SU_GS_NKStaffCode = "AAA";
			document1.SU_BusinessContext = OperationalAction.GetFullBusinessContext(BusinessContext.ARInvoice);
			document2.SU_BusinessContext = nameof(BusinessContext.ARInvoice);
			OperationalAction[] actions = Factory.Load<OperationalAction>(new OperationalActionZQuery());
			AssertCollectionContains("Contains(action1)", action1, actions);
			AssertCollectionContains("Contains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
			actions = Factory.Load<OperationalAction>(new OperationalActionZQuery(BusinessContext.ARInvoice));
			AssertCollectionContains("Contains(action1)", action1, actions);
			AssertCollectionNotContains("NotContains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
			actions = Factory.Load<OperationalAction>(new OperationalActionZQuery(BusinessContext.BookingLoadList));
			AssertCollectionNotContains("NotContains(action1)", action1, actions);
			AssertCollectionContains("Contains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
			actions = Factory.Load<OperationalAction>(new OperationalActionZQuery(BusinessContext.ARInvoice, "AAA"));
			AssertCollectionContains("Contains(action1)", action1, actions);
			AssertCollectionNotContains("NotContains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
			actions = Factory.Load<OperationalAction>(new OperationalActionZQuery(BusinessContext.ARInvoice, "BBB"));
			AssertCollectionContains("Contains(action1)", action1, actions);
			AssertCollectionNotContains("NotContains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
			actions = Factory.Load<OperationalAction>(new OperationalActionZQuery(BusinessContext.BookingLoadList, "AAA"));
			AssertCollectionNotContains("NotContains(action1)", action1, actions);
			AssertCollectionContains("Contains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
			actions = Factory.Load<OperationalAction>(new OperationalActionZQuery(BusinessContext.BookingLoadList, "BBB"));
			AssertCollectionNotContains("NotContains(action1)", action1, actions);
			AssertCollectionNotContains("NotContains(action2)", action2, actions);
			AssertCollectionNotContains("NotContains(document1)", document1, actions);
			AssertCollectionNotContains("NotContains(document2)", document2, actions);
		}
	}
}
