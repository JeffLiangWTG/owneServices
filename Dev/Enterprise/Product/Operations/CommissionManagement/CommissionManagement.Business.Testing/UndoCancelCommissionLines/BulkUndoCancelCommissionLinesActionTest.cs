using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(BulkUndoCancelCommissionLinesAction))]
	public class BulkUndoCancelCommissionLinesActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var commissionLine1 = Factory.New<ViewCommissionLine>();
			var commissionLine2 = Factory.New<ViewCommissionLine>();
			var commissionLine3 = Factory.New<ViewCommissionLine>();

			var bulkCancelAction = new BulkUndoCancelCommissionLinesAction(Factory);
			bulkCancelAction.Initialise(new[] { commissionLine1, commissionLine2, commissionLine3 });

			AssertContainsExactElementsInAnyOrder(
				new[] { commissionLine1, commissionLine2, commissionLine3 },
				bulkCancelAction.UndoCancelCommissionLineActions.Select(x => x.CommissionLine));
		}

		public void TestExecute()
		{
			var commissionLine1 = Factory.New<ViewCommissionLine>();
			commissionLine1.VCL_CancelledDateTimeUtc = ZDateTime.Today;
			var commissionLine2 = Factory.New<ViewCommissionLine>();
			commissionLine2.VCL_CancelledDateTimeUtc = ZDateTime.Today;
			var commissionLine3 = Factory.New<ViewCommissionLine>();
			commissionLine3.VCL_CancelledDateTimeUtc = ZDateTime.Today;

			var bulkCancelAction = new BulkUndoCancelCommissionLinesAction(Factory);
			bulkCancelAction.Initialise(new[] { commissionLine1, commissionLine2, commissionLine3 });
			bulkCancelAction.Execute();

			CombineAssertions(() =>
			{
				AssertEquals("commissionLine1", false, commissionLine1.IsCancelled);
				AssertEquals("commissionLine2", false, commissionLine2.IsCancelled);
				AssertEquals("commissionLine3", false, commissionLine3.IsCancelled);
			});
		}

		public void TestRemoveErrorLines()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var canceledCommissionLine = Factory.New<ViewCommissionLine>();
			canceledCommissionLine.VCL_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);
			var paidCommissionLine = Factory.New<ViewCommissionLine>();
			paidCommissionLine.VCL_PaidDateTimeUtc = new ZDateTime(2003, 3, 3);

			var action = new BulkUndoCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine, canceledCommissionLine, paidCommissionLine });
			var linesRemoved = action.RemoveErrorLines();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					canceledCommissionLine
				},
				action.UndoCancelCommissionLineActions.Select(x => x.CommissionLine));

			AssertEquals(2, linesRemoved);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var commissionLine1 = Factory.New<ViewCommissionLine>();
			var commissionLine2 = Factory.New<ViewCommissionLine>();
			var commissionLine3 = Factory.New<ViewCommissionLine>();

			var action = new BulkUndoCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine1, commissionLine2, commissionLine3 });
			return action;
		}

		#endregion
	}

	public class BulkUndoCancelCommissionLinesActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckContainsAtLeastOne()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new BulkUndoCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine });

			action.RunPreSaveValidation();
			AssertNoRowError(action, "No entity commissions have been selected to be undo canceled.");

			action.UndoCancelCommissionLineActionCollection.RemoveAndDeleteAll();
			action.RunPreSaveValidation();
			AssertHasRowError(action, "No entity commissions have been selected to be undo canceled.");
		}
	}
}
