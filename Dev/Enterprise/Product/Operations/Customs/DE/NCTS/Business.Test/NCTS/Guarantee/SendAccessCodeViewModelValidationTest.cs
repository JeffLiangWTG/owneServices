using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class SendAccessCodeViewModelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOfficeOfGuarantee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AAA", "AAA Office",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");
			Factory.Save();

			var guarantee = Factory.New<CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(guarantee);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, viewModel.OfficeOfGuarantee);
				viewModel.Validation.ValidateOfficeOfGuarantee();
				AssertHasError(viewModel.OfficeOfGuaranteeInfo, "Please enter an Office of Guarantee.");

				viewModel.OfficeOfGuarantee = "ABC";
				AssertHasError(viewModel.OfficeOfGuaranteeInfo, "Enter a valid Office of Guarantee.");

				viewModel.OfficeOfGuarantee = "AAA";
				AssertNoNotifications(viewModel.OfficeOfGuaranteeInfo);
			});
		}

		public void TestNewMainAccessCode()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(guarantee);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, viewModel.NewMainAccessCode);
				AssertNoNotifications("If blank we send the current code, so this is allowed.", viewModel.NewMainAccessCodeInfo);

				viewModel.NewMainAccessCode = "123";
				AssertHasError(viewModel.NewMainAccessCodeInfo, "The new Main Access Code must have 4 digits.");

				viewModel.NewMainAccessCode = "1234";
				AssertNoNotifications(viewModel.NewMainAccessCodeInfo);
			});
		}
	}
}
