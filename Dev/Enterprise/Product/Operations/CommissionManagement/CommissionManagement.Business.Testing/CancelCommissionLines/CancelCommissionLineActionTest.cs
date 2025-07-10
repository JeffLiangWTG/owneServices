using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CancelCommissionLineAction))]
	public class CancelCommissionLineActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExecute()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new CancelCommissionLineAction(commissionLine);

			AssertEquals("Precondition", false, commissionLine.IsCancelled);

			action.Execute();

			AssertEquals(true, commissionLine.IsCancelled);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CancelCommissionLineAction(Factory.New<ViewCommissionLine>());
		}

		#endregion
	}

	public class CancelCommissionLineActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAlreadyPaid()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new CancelCommissionLineAction(commissionLine);

			commissionLine.VCL_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);
			action.RunPreSaveValidation();
			AssertHasRowError(action, "Already paid.");

			commissionLine.VCL_PaidDateTimeUtc = ZDateTime.Empty;
			action.RunPreSaveValidation();
			AssertNoRowErrors(action);
		}

		public void TestCheckAlreadyCancelled()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new CancelCommissionLineAction(commissionLine);

			commissionLine.VCL_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);
			action.RunPreSaveValidation();
			AssertHasRowError(action, "Already canceled.");

			commissionLine.VCL_CancelledDateTimeUtc = ZDateTime.Empty;
			action.RunPreSaveValidation();
			AssertNoRowErrors(action);
		}
	}
}
