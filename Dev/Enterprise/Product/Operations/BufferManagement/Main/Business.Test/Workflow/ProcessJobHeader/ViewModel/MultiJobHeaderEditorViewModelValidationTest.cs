using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class MultiJobHeaderEditorViewModelValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2015, 7, 14)]
		public void TestValidatePropertyRanges()
		{
			var viewModel = new MultiJobHeaderEditorViewModel(System.Array.Empty<BusinessObject>(), Factory);

			viewModel.EarliestStartDateDays = -99999;
			viewModel.EarliestStartDateLocal = ZDateTime.Invalid;
			viewModel.EarliestStartDateOffset = ZDateTime.Invalid;

			viewModel.AgreedDeliveryDateDays = 99999;
			viewModel.AgreedDeliveryDateLocal = ZDateTime.Invalid;
			viewModel.AgreedDeliveryDateOffset = ZDateTime.Invalid;

			viewModel.Validation.ValidateAll();

			AssertHasError(viewModel.EarliestStartDateDaysInfo, "Please enter an 'Earliest Start Date: Days' greater than or equal to -9999.");
			AssertHasError(viewModel.EarliestStartDateLocalInfo, "Enter a valid Earliest Start Date.");
			AssertHasError(viewModel.EarliestStartDateOffsetInfo, "Enter a valid Hours.");

			AssertHasError(viewModel.AgreedDeliveryDateDaysInfo, "Please enter an 'Agreed Delivery Date: Days' less than or equal to 9999.");
			AssertHasError(viewModel.AgreedDeliveryDateLocalInfo, "Enter a valid Agreed Delivery Date.");
			AssertHasError(viewModel.AgreedDeliveryDateOffsetInfo, "Enter a valid Hours.");

			viewModel.EarliestStartDateDays = -9999;
			viewModel.EarliestStartDateLocal = ZDateTime.Now;
			viewModel.EarliestStartDateOffset = new ZInt(-999).GetDateTimeFromMinutes();

			viewModel.AgreedDeliveryDateDays = 9999;
			viewModel.AgreedDeliveryDateLocal = ZDateTime.Now;
			viewModel.AgreedDeliveryDateOffset = new ZInt(999).GetDateTimeFromMinutes();

			viewModel.Validation.ValidateAll();

			AssertNoErrors(viewModel.EarliestStartDateDaysInfo);
			AssertNoErrors(viewModel.EarliestStartDateLocalInfo);
			AssertNoErrors(viewModel.EarliestStartDateOffsetInfo);

			AssertNoErrors(viewModel.AgreedDeliveryDateDaysInfo);
			AssertNoErrors(viewModel.AgreedDeliveryDateLocalInfo);
			AssertNoErrors(viewModel.AgreedDeliveryDateOffsetInfo);
		}

		public void TestValidateDateAcceptability()
		{
			var viewModel = new MultiJobHeaderEditorViewModel(System.Array.Empty<BusinessObject>(), Factory);

			viewModel.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			viewModel.Validation.ValidateAll();

			AssertNoErrors(viewModel.FH_DateAcceptabilityInfo);

			viewModel.FH_DateAcceptability = "vvv";
			viewModel.Validation.ValidateAll();

			AssertHasError(viewModel.FH_DateAcceptabilityInfo, "Enter a valid Date Acceptability.");
		}
	}
}
