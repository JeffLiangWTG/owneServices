using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(BulkCancelCommissionLinesAction))]
	public class BulkCancelCommissionLinesActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var commissionLine1 = Factory.New<ViewCommissionLine>();
			var commissionLine2 = Factory.New<ViewCommissionLine>();
			var commissionLine3 = Factory.New<ViewCommissionLine>();

			var bulkCancelAction = new BulkCancelCommissionLinesAction(Factory);
			bulkCancelAction.Initialise(new[] { commissionLine1, commissionLine2, commissionLine3 });

			AssertContainsExactElementsInAnyOrder(
				new[] { commissionLine1, commissionLine2, commissionLine3 },
				bulkCancelAction.CancelCommissionLineActions.Select(x => x.CommissionLine));
		}

		public void TestExecute()
		{
			var commissionLine1 = Factory.New<ViewCommissionLine>();
			var commissionLine2 = Factory.New<ViewCommissionLine>();
			var commissionLine3 = Factory.New<ViewCommissionLine>();

			var bulkCancelAction = new BulkCancelCommissionLinesAction(Factory);
			bulkCancelAction.Initialise(new[] { commissionLine1, commissionLine2, commissionLine3 });
			bulkCancelAction.Execute();

			CombineAssertions(() =>
			{
				AssertEquals("commissionLine1", true, commissionLine1.IsCancelled);
				AssertEquals("commissionLine2", true, commissionLine2.IsCancelled);
				AssertEquals("commissionLine3", true, commissionLine3.IsCancelled);
			});
		}

		public void TestRemoveErrorLines()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var canceledCommissionLine = Factory.New<ViewCommissionLine>();
			canceledCommissionLine.VCL_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);
			var paidCommissionLine = Factory.New<ViewCommissionLine>();
			paidCommissionLine.VCL_PaidDateTimeUtc = new ZDateTime(2003, 3, 3);

			var action = new BulkCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine, canceledCommissionLine, paidCommissionLine });
			var linesRemoved = action.RemoveErrorLines();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					commissionLine
				},
				action.CancelCommissionLineActions.Select(x => x.CommissionLine));

			AssertEquals(2, linesRemoved);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var commissionLine1 = Factory.New<ViewCommissionLine>();
			var commissionLine2 = Factory.New<ViewCommissionLine>();
			var commissionLine3 = Factory.New<ViewCommissionLine>();

			var action = new BulkCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine1, commissionLine2, commissionLine3 });
			return action;
		}

		#endregion
	}

	public class BulkCancelCommissionLinesActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckContainsAtLeastOne()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new BulkCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine });

			action.RunPreSaveValidation();
			AssertNoRowError(action, "No entity commissions have been selected to be canceled.");

			action.CancelCommissionLineActionCollection.RemoveAndDeleteAll();
			action.RunPreSaveValidation();
			AssertHasRowError(action, "No entity commissions have been selected to be canceled.");
		}
	}
}
