using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class GuaranteeAccessCodesSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckOfficeOfGuarantee()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
				var codeIEDUB100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");

				Factory.Save();

				var validation = guaranteeAccessCodesSendingObject.Validation;
				var targetInfo = guaranteeAccessCodesSendingObject.OfficeOfGuaranteeInfo;

				guaranteeAccessCodesSendingObject.OfficeOfGuarantee = ZString.Empty;
				validation.ValidateAll();
				AssertHasError(targetInfo, "Please enter an Office of Guarantee.");

				guaranteeAccessCodesSendingObject.OfficeOfGuarantee = "XYZ";
				validation.ValidateAll();
				AssertHasMessageErrorContaining(targetInfo, "The code you have selected is not in the list");

				guaranteeAccessCodesSendingObject.OfficeOfGuarantee = "IEDUB100";
				validation.ValidateAll();
				AssertNoMessageErrorContaining(targetInfo, "The code you have selected is not in the list");
			}
		}

		public void TestCheckMasterCode()
		{
			var validation = guaranteeAccessCodesSendingObject.Validation;
			var targetInfo = guaranteeAccessCodesSendingObject.MasterCodeInfo;

			var error = "Please enter a Master Code.";
			guaranteeAccessCodesSendingObject.MasterCode = ZString.Empty;
			validation.ValidateAll();
			AssertHasError("Empty", targetInfo, error);

			guaranteeAccessCodesSendingObject.MasterCode = "XYZ";
			validation.ValidateAll();
			AssertNoErrors("Field has value", targetInfo);
		}

		public void TestCheckCurrentCode()
		{
			var validation = guaranteeAccessCodesSendingObject.Validation;
			var targetInfo = guaranteeAccessCodesSendingObject.CurrentCodeInfo;

			var error = "Please enter a Current Code.";
			guaranteeAccessCodesSendingObject.CurrentCode = ZString.Empty;
			validation.ValidateAll();
			AssertHasError("Empty", targetInfo, error);

			guaranteeAccessCodesSendingObject.CurrentCode = "XYZ";
			validation.ValidateAll();
			AssertNoErrors("Field has value", targetInfo);
		}

		public void TestCheckNewAccessCode()
		{
			var validation = guaranteeAccessCodesSendingObject.Validation;
			var targetInfo = guaranteeAccessCodesSendingObject.NewAccessCodeInfo;

			var error = "Please enter a New Access Code.";
			guaranteeAccessCodesSendingObject.NewAccessCode = ZString.Empty;
			validation.ValidateAll();
			AssertHasError("Empty", targetInfo, error);

			guaranteeAccessCodesSendingObject.NewAccessCode = "XYZ";
			validation.ValidateAll();
			AssertNoErrors("Field has value", targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			guaranteeAccessCodesSendingObject = new GuaranteeAccessCodesSendingObject(cusGuaranteeHeader);
		}
		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeAccessCodesSendingObject guaranteeAccessCodesSendingObject;
	}
}
