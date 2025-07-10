using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class ExitNotificationCusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCER_AdditionalDeclarationType()
		{
			cusExitReport.CER_AdditionalDeclarationType = "T";
			AssertHasMessageError("Value not in list", cusExitReport.CER_AdditionalDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);
			cusExitReport.CER_AdditionalDeclarationType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;
			AssertNoMessageError("Value is in list", cusExitReport.CER_AdditionalDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCER_TransportMode()
		{
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportType = EU.ExitControl.Business.CusExitReportTransportTypeList.Codes._10;
			cusExitReport.CER_TransportMode = ZString.Empty;
			AssertNoMessageErrors("Not Required for ExitNotification", cusExitReport.CER_TransportModeInfo);
		}

		public void TestCheckCER_TransportType()
		{
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.CER_TransportType = ZString.Empty;
			AssertNoMessageErrors("Not Required for ExitNotification", cusExitReport.CER_TransportTypeInfo);
		}

		public void TestCheckCER_TransportID()
		{
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.CER_TransportID = ZString.Empty;
			AssertNoMessageErrors("Not Required for ExitNotification", cusExitReport.CER_TransportIDInfo);
		}

		public void TestCheckCER_RN_NKTransportNationality()
		{
			cusExitReport.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.CER_RN_NKTransportNationality = ZString.Empty;
			AssertNoMessageErrors("Not Required for ExitNotification", cusExitReport.CER_RN_NKTransportNationalityInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusExitReport = CusExitReportTest.GetNewBusinessObject(Factory).report;
			cusExitReport.CER_Type = ExitReportTypeList.Codes.ExitNotification;
		}

		CusExitReport cusExitReport;
	}
}
