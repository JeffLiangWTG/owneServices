using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	internal class AccEInvoicingTransactionPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAIP_ActionType()
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();

			Assert(!pivot.AIP_ActionType.IsEmpty);
			AssertNoErrors("Default Value is valid", pivot.AIP_ActionTypeInfo);

			pivot.AIP_ActionType = ZString.Empty;
			AssertHasError(pivot.AIP_ActionTypeInfo, "Please enter a value.");

			pivot.AIP_ActionType = "!#$";
			AssertHasError(pivot.AIP_ActionTypeInfo, "Enter a valid selection.");

			pivot.AIP_ActionType = EInvoicingPivotActionType.DocumentAction;
			AssertNoErrors("Set to a valid value", pivot.AIP_ActionTypeInfo);
		}

		public void TestAIP_Status()
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();

			Assert(!pivot.AIP_Status.IsEmpty);
			AssertNoErrors("Default Value is valid", pivot.AIP_StatusInfo);

			pivot.AIP_Status = ZString.Empty;
			AssertHasError(pivot.AIP_StatusInfo, "Please enter a value.");

			pivot.AIP_Status = "!#$";
			AssertHasError(pivot.AIP_StatusInfo, "Enter a valid selection.");

			pivot.AIP_Status = EInvoicingPivotState.Sent;
			AssertNoErrors("Set to a valid value", pivot.AIP_StatusInfo);
		}
	}
}
