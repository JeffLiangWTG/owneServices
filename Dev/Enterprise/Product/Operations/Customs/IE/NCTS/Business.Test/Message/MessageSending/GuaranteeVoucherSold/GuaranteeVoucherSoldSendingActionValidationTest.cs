using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class GuaranteeVoucherSoldSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckCheckExpiryDate()
		{
			var endDateMessage = "Please enter the end date of the guarantee.";
			var validation = guaranteeVoucherSoldSendingAction.Validation;
			validation.ValidateAll();
			AssertHasRowMessageError("Empty check for CusGuarantee End Date.", guaranteeVoucherSoldSendingAction, endDateMessage);

			header.CPH_EndDate = ZDate.Today.AddDays(1);
			validation.ValidateAll();
			AssertNoRowError("Empty check for CusGuarantee End Date, check passes.", guaranteeVoucherSoldSendingAction, endDateMessage);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTROS";
			Factory.Save();

			var validation = guaranteeVoucherSoldSendingAction.Validation;
			var targetInfo = guaranteeVoucherSoldSendingAction.HolderOfTransitProcedureInfo;

			var error = "Please enter a Transit Holder.";
			guaranteeVoucherSoldSendingAction.HolderOfTransitProcedure = "";
			validation.ValidateAll();
			AssertHasError("Holder of Transit is blank", targetInfo, error);

			guaranteeVoucherSoldSendingAction.HolderOfTransitProcedure = "XXX";
			validation.ValidateAll();
			AssertHasError("Holder of Transit is invalid", targetInfo, "Enter a valid Transit Holder.");

			guaranteeVoucherSoldSendingAction.HolderOfTransitProcedure = "TESTROS";
			validation.ValidateAll();
			AssertNoErrors("Holder of Transit is valid", targetInfo);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			var countryCode = Core.Constants.CountryCodes.Australia;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeIEDUB100 = helper.CreateNewOrGetExistingCusCodeList(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");

			Factory.Save();

			var validation = guaranteeVoucherSoldSendingAction.Validation;
			var targetInfo = guaranteeVoucherSoldSendingAction.CustomsOfficeOfGuaranteeInfo;

			guaranteeVoucherSoldSendingAction.CustomsOfficeOfGuarantee = "";
			validation.ValidateAll();
			AssertHasError("Office of Guarantee is blank", targetInfo, "Please enter a Guarantee Office.");

			guaranteeVoucherSoldSendingAction.CustomsOfficeOfGuarantee = "XXX";
			validation.ValidateAll();
			AssertHasError("Office of Guarantee is invalid", targetInfo, "Enter a valid Guarantee Office.");

			guaranteeVoucherSoldSendingAction.CustomsOfficeOfGuarantee = "IEDUB100";
			validation.ValidateAll();
			AssertNoErrors("Office of Guarantee is valid", targetInfo);
		}

		public void TestVoucherAmount()
		{
			var validation = guaranteeVoucherSoldSendingAction.Validation;
			var targetInfo = guaranteeVoucherSoldSendingAction.VoucherAmountInfo;

			var error = "If TIR Carnet is YES, then Voucher Amount is required.";
			guaranteeVoucherSoldSendingAction.VoucherAmount = 0m;
			validation.ValidateAll();
			AssertNoErrors("Voucher Amount is not required", targetInfo);

			guaranteeVoucherSoldSendingAction.TIRCarnet = true;
			validation.ValidateAll();
			AssertHasError("Voucher Amount is zero", targetInfo, error);

			guaranteeVoucherSoldSendingAction.VoucherAmount = 15000m;
			validation.ValidateAll();
			AssertNoErrors("Voucher Amount is valid", targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeVoucherSoldSendingAction = new GuaranteeVoucherSoldSendingAction(header);
		}
		CusGuaranteeHeader header;
		GuaranteeVoucherSoldSendingAction guaranteeVoucherSoldSendingAction;
	}
}
