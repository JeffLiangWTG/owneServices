using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsPreviousProcedureMasterValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_Procedure()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(previousProcedureMaster.CSI_ProcedureInfo, "XX", NctsPreviousProcedureList.Codes._9DEY);
		}

		public void TestCheckCSI_Procedure_RuleNR0052()
		{
			const string messageError = "[NR0052] Type N337 is required in Tab Previous Documents of this Goods Item to use this Previous Procedure.";

			previousProcedureMaster.CSI_Procedure = "N337";
			goodsItem.PreviousDocuments.Clear();
			previousProcedureMaster.Validation.ValidateCSI_Procedure();

			AssertHasMessageError(previousProcedureMaster.CSI_ProcedureInfo, messageError);

			var document = goodsItem.PreviousDocuments.AddNew();
			document.CSI_Code = "N337";
			previousProcedureMaster.Validation.ValidateCSI_Procedure();

			AssertNoMessageError(previousProcedureMaster.CSI_ProcedureInfo, messageError);
		}

		public void TestCheckAuthorizationNumber()
		{
			SetupAuthorisationNumbers(nctsHeader);

			CombineAssertions("N337", () =>
			{
				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;

				ValidationTestHelper.AssertFieldIsNotMandatory(previousProcedureMaster.AuthorizationNumberInfo);
				previousProcedureMaster.AuthorizationNumber = "NUM";
				AssertNoMessageErrorContaining(previousProcedureMaster.AuthorizationNumberInfo, ListValidation.InvalidCodeMessageError.ToString());
				previousProcedureMaster.AuthorizationNumber = "";
			});

			CombineAssertions("9DEZ", () =>
			{
				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(previousProcedureMaster.AuthorizationNumberInfo, "NUMBER2", "NUMBER1");
			});

			CombineAssertions("9DEY", () =>
			{
				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;

				previousProcedureMaster.SimplifiedGrantAuthorizationFlag = true;
				previousProcedureMaster.Validation.ValidateAuthorizationNumber();
				AssertNoMessageErrorContaining("Authorisation Flag true", previousProcedureMaster.AuthorizationNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousProcedureMaster.SimplifiedGrantAuthorizationFlag = false;
				previousProcedureMaster.Validation.ValidateAuthorizationNumber();
				AssertHasMessageErrorContaining("Authorisation Flag false", previousProcedureMaster.AuthorizationNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousProcedureMaster.AuthorizationNumber = "1234567890";
				AssertNoMessageErrorContaining("Value entered", previousProcedureMaster.AuthorizationNumberInfo, MandatoryValidation.YouHaveNotEntered);

				ValidationTestHelper.AssertInvalidCodeMessageError(previousProcedureMaster.AuthorizationNumberInfo, "NUMBER1", "NUMBER2");
			});
		}

		public void TestCheckCSI_CustomsOffice_9DEY()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var zzd = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE00567", "Valid", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			helper.CreateCusCodeListAttribute(zzd.PK, Universal.RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			Factory.Save();

			const string messageError = "The entered Customs Office is not a Main Office in Germany.";
			CombineAssertions(() =>
			{
				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				previousProcedureMaster.CSI_CustomsOffice = "DE1234";
				AssertHasMessageError("Invalid Code", previousProcedureMaster.CSI_CustomsOfficeInfo, messageError);

				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				previousProcedureMaster.Validation.ValidateCSI_CustomsOffice();
				AssertNoMessageError("Not required for this procedure", previousProcedureMaster.CSI_CustomsOfficeInfo, messageError);

				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				previousProcedureMaster.SimplifiedGrantAuthorizationFlag = true;
				previousProcedureMaster.CSI_CustomsOffice = "DE00567";
				AssertNoMessageError("Valid Code", previousProcedureMaster.CSI_CustomsOfficeInfo, messageError);

				previousProcedureMaster.SimplifiedGrantAuthorizationFlag = true;
				previousProcedureMaster.CSI_CustomsOffice = ZString.Empty;
				AssertHasMessageErrorContaining("Mandatory", previousProcedureMaster.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				previousProcedureMaster.CSI_CustomsOffice = "DE00567";
				AssertNoMessageErrorContaining("Authorization Flag and entered", previousProcedureMaster.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			previousProcedureMaster = goodsItem.PreviousProcedureMaster;
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousProcedureMaster previousProcedureMaster;

		void SetupAuthorisationNumbers(NctsHeader nctsHeader)
		{
			var principal = Factory.New<OrgHeader>();
			principal.MainAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			var consignor = Factory.New<OrgHeader>();
			consignor.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");

			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			nctsHeader.Consignor.E2_OA_Address = consignor.MainAddress.PK;
		}
	}
}
