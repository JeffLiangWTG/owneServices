using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using static Enterprise.Customs.Universal.Constants;
using CusAuthorizationUsage = Enterprise.Customs.EU.NCTS.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		NctsDepartureMovementHeader CreateDepartureMovement()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader;
		}

		public void TestRuleC0387_WhenSecurityIsEXIAndCircumstanceIsXXX()
		{
			var departureMovement = CreateDepartureMovement();
			var targetInfo = departureMovement.BM_ForeignDestPortKCodeInfo;

			departureMovement.BM_TypeOfSecurity = "EXI";
			departureMovement.BM_SpecificCircumstance = "XXX";
			departureMovement.BM_ForeignDestPortKCode = ZString.Empty;

			departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageErrors("Rule C0387 should not trigger for security = EXI and circumstance = XXX", targetInfo);

			departureMovement.BM_TypeOfSecurity = "EXI";
			departureMovement.BM_SpecificCircumstance = "A20";
			departureMovement.BM_ForeignDestPortKCode = ZString.Empty;

			departureMovement.Validation.ValidateBM_ForeignDestPortKCode();
			AssertHasMessageErrors("[C0387] You have not entered an UNLOCO (Place Of Unloading) Or a Country.",
				targetInfo);
		}

		public void TestCheckBM_ExportTransportMode()
		{
			var targetInfo = departureMovement.BM_ExportTransportModeInfo;
			var errorMessage = targetInfo.HumanReadableName + " is mandatory for Security Declaration.";

			departureMovement.BM_CustomsStatus = "PRE";
			departureMovement.BM_TypeOfSecurity = "ENT";
			departureMovement.BM_ExportTransportMode = "";
			AssertHasMessageErrorContaining("Should have message error", targetInfo, errorMessage);

			departureMovement.BM_ExportTransportMode = "1";
			AssertNoMessageErrorContaining("Non-empty vale should not have message error", targetInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = "EXI";
			departureMovement.BM_ExportTransportMode = "";
			AssertHasMessageErrorContaining("Should have message error", targetInfo, errorMessage);

			departureMovement.BM_CustomsStatus = "DCA";
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertHasMessageErrorContaining("Non-PRE should have YouHaveNotEntered message error", targetInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_CustomsStatus = "PRE";
			departureMovement.BM_TypeOfSecurity = "BTH";
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertHasMessageErrorContaining("Should have message error", targetInfo, errorMessage);

			departureMovement.BM_TypeOfSecurity = "NON";
			departureMovement.Validation.ValidateBM_ExportTransportMode();
			AssertHasMessageErrorContaining("NON should have YouHaveNotEntered message error", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_ForeignDestPortKCodeMandatory()
		{
			var targetInfo = departureMovement.BM_ForeignDestPortKCodeInfo;
			var expectedErrorMssg = "[C0191] You have not entered a Country/Region Code or an UNLOCO for Place of Unloading.";

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageError("No mandatory check when BM_TypeOfSecurity NON", targetInfo, expectedErrorMssg);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			validation.ValidateBM_ForeignDestPortKCode();
			AssertNoMessageError("No mandatory check when BM_TypeOfSecurity EXI", targetInfo, expectedErrorMssg);

			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			validation.ValidateBM_ForeignDestPortKCode();
			AssertHasMessageError("Mandatory check when BM_TypeOfSecurity EXI", targetInfo, expectedErrorMssg);

			departureMovement.BM_ForeignDestPortKCode = "ESAGP";
			AssertNoMessageError("Mandatory check passes.", targetInfo, expectedErrorMssg);
		}

		public void TestCheckBM_AdditionalDeclarationType()
		{
			var targetInfo = departureMovement.BM_AdditionalDeclarationTypeInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, new ZString[] { "X" }, new ZString[] { NctsTypeOfAdditionalDeclarationList.Codes.A, NctsTypeOfAdditionalDeclarationList.Codes.D });

			departureMovement.BM_AdditionalDeclarationType = string.Empty;
			AssertHasMessageErrorContaining("Message error for empty.", targetInfo, $"{NctsConstants.ValidationRuleMessagePrefixes.TR0017}{MandatoryValidation.YouHaveNotEntered}");
			departureMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			AssertNoMessageErrors("Validation passes.", targetInfo);
		}

		public void TestCheckDateLimit_IsInFuture()
		{
			var messageError = "Date Limit cannot be earlier or equal to current date.";
			var targetInfo = departureMovement.BM_ExportDateInfo;
			departureMovement.IsSimplifiedNctsProcedure = true;
			CombineAssertions(() =>
			{
				departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
				validation.ValidateBM_ExportDate();
				AssertHasMessageError("Date in the past", targetInfo, messageError);

				departureMovement.BM_ExportDate = ZDateTime.Today;
				validation.ValidateBM_ExportDate();
				AssertHasMessageError("Date is today", targetInfo, messageError);

				departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(30);
				validation.ValidateBM_ExportDate();
				AssertNoMessageErrors("Date in the future.", targetInfo);

				departureMovement.BM_EntryDate = ZDateTime.Today.AddDays(-2);
				departureMovement.BM_ExportDate = ZDateTime.Today.AddDays(-1);
				departureMovement.BM_CustomsStatus = "MRN";
				validation.ValidateBM_ExportDate();
				AssertNoMessageErrors("Date in the past but customs status is set", targetInfo);
			});
		}

		public void TestCheckDateLimit_Required()
		{
			TestCheckDateLimitRequiredInner("Phase5, CusAuthorizationUsages are on NctsDepartureMovementHeader", departureMovement.CusAuthorizationUsages, "[C0839] You have not entered a Date Limit.");

			departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			TestCheckDateLimitRequiredInner("Phase4, CusAuthorizationUsages are on NctsHeader", departureMovement.Header.CusAuthorizationUsages, "[C0839] You have not entered a Control Result Date.");

			void TestCheckDateLimitRequiredInner(string testCaseDescription, IBusinessObjectCollection<CusAuthorizationUsage> cusAuthorizationUsages, string messageError)
			{
				var targetInfo = departureMovement.BM_ExportDateInfo;
				departureMovement.IsSimplifiedNctsProcedure = true;
				CombineAssertions(testCaseDescription, () =>
				{
					departureMovement.BM_AdditionalDeclarationType = "A";
					validation.ValidateBM_ExportDate();
					AssertNoMessageErrors("Additional Declaration Type = A, but no Authorisation Code has been added", targetInfo);

					var auth = cusAuthorizationUsages.AddNew();
					auth.AGC_Code = "ACR";
					validation.ValidateBM_ExportDate();
					AssertHasMessageError("Additional Declaration Type = A, Authorisation Code = C521", targetInfo, messageError);

					auth.AGC_Code = "SSE";
					validation.ValidateBM_ExportDate();
					AssertNoMessageErrors("Additional Declaration Type = A, Authorisation Code != C521", targetInfo);
				});
			}
		}

		public void TestCheckTirCarnetExpiryDateMandatory()
		{
			departureMovement.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			departureMovement.TirCarnetExpiryDate = ZDateTime.Empty;
			departureMovement.Validation.ValidateTirCarnetExpiryDate();
			AssertNoNotifications(departureMovement.TirCarnetExpiryDateInfo);
		}

		public void TestCheckTransportTypeAtDeparture()
		{
			var messageError = "You have not entered a Type of ID.";
			var targetInfo = departureMovement.BM_TransportAtDepartureTypeInfo;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				departureMovement.InlandTransportModeAtDeparture = "1";
				departureMovement.TransportTypeAtDeparture = "";
				AssertHasMessageError(targetInfo, messageError);

				departureMovement.TransportTypeAtDeparture = "10";
				AssertNoMessageError(targetInfo, messageError);
			}
		}

		public void TestCheckTransportAtDeparture()
		{
			var targetInfo = departureMovement.TransportAtDepartureInfo;

			AssertTransportAtDepartureRequired(targetInfo, "1", "10");
			AssertTransportAtDepartureRequired(targetInfo, "1", "11");

			AssertTransportAtDepartureRequired(targetInfo, "2", "20");
			AssertTransportAtDepartureRequired(targetInfo, "2", "21");

			AssertTransportAtDepartureRequired(targetInfo, "3", "30");

			AssertTransportAtDepartureRequired(targetInfo, "4", "40");
			AssertTransportAtDepartureRequired(targetInfo, "4", "41");

			AssertTransportAtDepartureRequired(targetInfo, "8", "80");
			AssertTransportAtDepartureRequired(targetInfo, "8", "81");
		}

		void AssertTransportAtDepartureRequired(ZPropertyInfo targetInfo, string transportMode, string transportType)
		{
			departureMovement.InlandTransportModeAtDeparture = transportMode;
			departureMovement.TransportTypeAtDeparture = transportType;
			departureMovement.TransportAtDeparture = "";
			AssertHasMessageErrorContaining($"TransportTypeAtDeparture = {transportType}", targetInfo, MandatoryValidation.YouHaveNotEntered);
			departureMovement.TransportAtDeparture = "1234";
			AssertNoMessageErrorContaining($"TransportTypeAtDeparture = {transportType}", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTransportCountryAtDeparture()
		{
			var targetInfo = departureMovement.TransportCountryAtDepartureInfo;

			AssertTransportAtDepartureCountryRequired(targetInfo, "1", true);
			AssertTransportAtDepartureCountryRequired(targetInfo, "2", false);
			AssertTransportAtDepartureCountryRequired(targetInfo, "3", true);
			AssertTransportAtDepartureCountryRequired(targetInfo, "4", true);
			AssertTransportAtDepartureCountryRequired(targetInfo, "8", true);
		}

		void AssertTransportAtDepartureCountryRequired(ZPropertyInfo targetInfo, string transportMode, bool isRequired)
		{
			departureMovement.InlandTransportModeAtDeparture = transportMode;
			departureMovement.TransportCountryAtDeparture = "";
			if (isRequired)
			{
				AssertHasMessageErrorContaining($"InlandTransportModeAtDeparture = {transportMode}", targetInfo, MandatoryValidation.YouHaveNotEntered);
				departureMovement.TransportCountryAtDeparture = "IE";
			}
			AssertNoMessageErrorContaining($"InlandTransportModeAtDeparture = {transportMode}", targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_ActiveBorderIdentificationType()
		{
			var targetInfo = departureMovement.BM_ActiveBorderIdentificationTypeInfo;

			departureMovement.BM_ExportTransportMode = "1";
			departureMovement.BM_ActiveBorderIdentificationType = "";
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_ExportTransportMode = "2";
			departureMovement.BM_ActiveBorderIdentificationType = "";
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_ExportTransportMode = "3";
			departureMovement.BM_ActiveBorderIdentificationType = "";
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_ExportTransportMode = "4";
			departureMovement.BM_ActiveBorderIdentificationType = "";
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_ExportTransportMode = "8";
			departureMovement.BM_ActiveBorderIdentificationType = "";
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.BM_ExportTransportMode = "9";
			departureMovement.BM_ActiveBorderIdentificationType = "";
			departureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			(_, departureMovement) = NctsDepartureMovementHeaderTest.GetNewBusinessObject(Factory);
			validation = departureMovement.Validation;
		}
		NctsDepartureMovementHeader departureMovement;
		NctsDepartureMovementHeaderValidation validation;
	}
}
