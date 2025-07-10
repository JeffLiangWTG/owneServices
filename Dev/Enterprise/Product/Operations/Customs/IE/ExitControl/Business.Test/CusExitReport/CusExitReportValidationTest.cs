using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class CusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCER_Calc_FormattedDateTime()
		{
			cusExitReport.CER_DateTime = ZDateTimeOffset.Invalid;
			cusExitReport.Validation.ValidateCER_Calc_FormattedDateTime();
			var messageError = string.Format(TypeValidation.InvalidTypeMessage, cusExitReport.CER_DateTimeInfo.HumanReadableName);
			AssertHasError(cusExitReport.CER_Calc_FormattedDateTimeInfo, messageError);
			cusExitReport.CER_DateTime = ZDateTimeOffset.Empty;
			cusExitReport.Validation.ValidateCER_Calc_FormattedDateTime();
			AssertNoError(cusExitReport.CER_Calc_FormattedDateTimeInfo, messageError);
		}

		public void TestCheckCER_DateTime()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusExitReport.CER_DateTimeInfo);
		}

		public void TestCheckCER_Behavior()
		{
			var noReportItemsErrorMessage = "There must be at least one Report Item when discrepancy is ticked.";
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(cusExitReport.CER_BehaviorInfo, "XXX", ExitReportDiscrepancyTypeList.Codes.Discrepancies);
			var exitReport = Factory.New<CusExitReport>();
			exitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			exitReport.Validation.ValidateCER_Behavior();
			AssertHasMessageError("Discrepancies must have at least 1 report item", exitReport.CER_Calc_DiscrepanciesInfo, noReportItemsErrorMessage);
			var item = exitReport.CusExitReportItems.AddNew();
			exitReport.Validation.ValidateCER_Behavior();
			AssertNoMessageError("Discrepancies must have at least 1 report item", exitReport.CER_Calc_DiscrepanciesInfo, noReportItemsErrorMessage);
		}

		public void TestCheckCER_OfficeOfExit()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Latvia", eun);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE1", "IE1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(cusExitReport.CER_OfficeOfExitInfo, "XXX", "IE1");
		}

		public void TestCheckCER_TransportMode()
		{
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportType = EU.ExitControl.Business.CusExitReportTransportTypeList.Codes._10;
			cusExitReport.CER_TransportMode = ZString.Empty;
			AssertNoMessageErrors("Not Required for Non-Presentation", cusExitReport.CER_TransportModeInfo);
		}

		public void TestCheckCER_TransportType()
		{
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.CER_TransportType = ZString.Empty;
			AssertNoMessageErrors("Not Required for Non-Presentation", cusExitReport.CER_TransportTypeInfo);
		}

		public void TestCheckCER_TransportID()
		{
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.CER_TransportID = ZString.Empty;
			AssertNoMessageErrors("Not Required for Non-Presentation", cusExitReport.CER_TransportIDInfo);
		}

		public void TestCheckCER_RN_NKTransportNationality()
		{
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.CER_RN_NKTransportNationality = ZString.Empty;
			AssertNoMessageErrors("Not Required for Non-Presentation", cusExitReport.CER_RN_NKTransportNationalityInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusExitReport = CusExitReportTest.GetNewBusinessObject(Factory).report;
			cusExitReport.CER_Type = ExitReportTypeList.Codes.Presentation;
		}

		CusExitReport cusExitReport;
	}
}
