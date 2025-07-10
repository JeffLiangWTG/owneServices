using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESExportOperationWrapperTest : WrapperHelperTest<EALAESExportOperationWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if Exit Report is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "exitReport"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if Consignment is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "Consignment"), () => GetWrapper(Factory.New<CusExitReport>()));
			});
		}

		public void TestMRN()
		{
			exitConsignment.CXC_MovementReference = "MRNNumber";
			AssertEquals("Expected filled MRN", "MRNNumber", wrapper.MRN);
		}

		public void TestStoringFlag()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Expected false StoringFlag if Trasport Mode is Air", expected: true, wrapper.StoringFlag);

				exitReport.CER_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Expected true StoringFlag if Trasport Mode is Road", expected: false, wrapper.StoringFlag);

				exitReport.CER_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Expected false StoringFlag if Trasport Mode is Sea", expected: true, wrapper.StoringFlag);

				exitReport.CER_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("Expected true StoringFlag if Trasport Mode is Rail", expected: false, wrapper.StoringFlag);

				exitReport.CER_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				AssertEquals("Expected false StoringFlag if Trasport Mode is OwnPropulsion", expected: true, wrapper.StoringFlag);
			});
		}

		public void TestDiscrepanciesExist()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_Calc_Discrepancies = true;
				AssertEquals("Expected filled DiscrepanciesExist if exists", true, wrapper.DiscrepanciesExist);

				exitReport.CER_Calc_Discrepancies = false;
				AssertEquals("Expected filled DiscrepanciesExist if not exists", false, wrapper.DiscrepanciesExist);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitConsignment = exitHeader.CusExitConsignments.AddNew();
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;

			wrapper = GetWrapper(exitReport);
		}
		CusExitConsignment exitConsignment;
		CusExitReport exitReport;
		EALAESExportOperationWrapper wrapper;

		EALAESExportOperationWrapper GetWrapper(CusExitReport exitReport) => new EALAESExportOperationWrapper(exitReport);
		protected override EALAESExportOperationWrapper GetProvider() => wrapper;
	}
}
