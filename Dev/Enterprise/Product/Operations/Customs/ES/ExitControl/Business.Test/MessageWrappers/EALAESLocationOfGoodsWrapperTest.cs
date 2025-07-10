using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESLocationOfGoodsWrapperTest : WrapperHelperTest<EALAESLocationOfGoodsWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestLocationType()
		{
			AssertEquals("Expected filled LocationType", "B", wrapper.LocationType);
		}

		public void TestLocationQualifier()
		{
			AssertEquals("Expected filled LocationQualifier", "Y", wrapper.LocationQualifier);
		}

		public void TestLocationId()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_Location = "ES00123456";
				AssertEquals("Expected filled LocationId", "ES00123456", wrapper.LocationId);

				exitReport.CER_Location = "ES0012345678";
				AssertEquals("Expected filled LocationId without 4 first chr when is longer than 10 characters and start by ES", "12345678", wrapper.LocationId);

				exitReport.CER_Location = "FR0012345678";
				AssertEquals("Expected filled LocationId when is longer than 10 characters but not start by ES", "FR00123456", wrapper.LocationId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			CusExitHeader exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			CusExitConsignment exitConsignment = exitHeader.CusExitConsignments.AddNew();
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;

			wrapper = new EALAESLocationOfGoodsWrapper(exitReport);
		}
		CusExitReport exitReport;
		EALAESLocationOfGoodsWrapper wrapper;

		protected override EALAESLocationOfGoodsWrapper GetProvider() => wrapper;
	}
}
