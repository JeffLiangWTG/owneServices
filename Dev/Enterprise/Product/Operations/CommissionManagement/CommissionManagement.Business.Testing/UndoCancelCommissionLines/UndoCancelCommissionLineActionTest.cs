using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(UndoCancelCommissionLineAction))]
	public class UndoCancelCommissionLineActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExecute()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_CancelledDateTimeUtc = ZDateTime.Today;
			var action = new UndoCancelCommissionLineAction(commissionLine);

			AssertEquals("Precondition", true, commissionLine.IsCancelled);

			action.Execute();

			AssertEquals(false, commissionLine.IsCancelled);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UndoCancelCommissionLineAction(Factory.New<ViewCommissionLine>());
		}

		#endregion
	}

	public class UndoCancelCommissionLineActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAlreadyCancelled()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new UndoCancelCommissionLineAction(commissionLine);

			commissionLine.VCL_CancelledDateTimeUtc = ZDateTime.Empty;
			action.RunPreSaveValidation();
			AssertHasRowError(action, "Not canceled.");

			commissionLine.VCL_CancelledDateTimeUtc = ZDateTime.Today;
			action.RunPreSaveValidation();
			AssertNoRowErrors(action);
		}
	}
}
