using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class CusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCER_TransportID_MandatoryWhenTransportTypeNotEmpty()
		{
			var report = Factory.New<CusExitReport>();
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			report.Validation.ValidateCER_TransportID();
			AssertNoNotifications(report.CER_TransportIDInfo);
		}

		public void TestCheckCER_RN_NKTransportNationality_MandatoryWhenTransportTypeNotEmpty()
		{
			var report = Factory.New<CusExitReport>();
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			report.Validation.ValidateCER_RN_NKTransportNationality();
			AssertNoNotifications(report.CER_RN_NKTransportNationalityInfo);
		}

		public void TestCheckCER_CXC_Consignment()
		{
			var header = Factory.New<CusExitHeader>();
			var consignment1 = header.CusExitConsignments.AddNew();
			var consignment2 = header.CusExitConsignments.AddNew();

			consignment1.CXC_MovementReference = "MRN1234";
			consignment1.CXC_LocalReference = "1234";

			consignment2.CXC_MovementReference = "MRN1235";
			consignment2.CXC_LocalReference = "1235";

			AssertNotEquals("Prereq: different PK", consignment1.PK, consignment2.PK);

			var report1 = header.CusExitReports.AddNew();
			var report2 = header.CusExitReports.AddNew();

			report1.CER_CXC_Consignment = consignment1.PK;
			report2.CER_CXC_Consignment = consignment1.PK;

			report2.Validation.ValidateCER_CXC_Consignment();

			AssertHasMessageErrorContaining(report2.CER_CXC_ConsignmentInfo, "This MRN is already being used in another declaration.");

			report2.CER_CXC_Consignment = consignment2.PK;

			report2.Validation.ValidateCER_CXC_Consignment();
			AssertNoMessageErrorContaining(report2.CER_CXC_ConsignmentInfo, "This MRN is already being used in another declaration.");
		}
	}
}
