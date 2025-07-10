using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitHeaderWorkflowInformationProviderTest : TestCaseWithFactory
	{
		public void TestDestination()
		{
			AssertEquals(ZString.Empty, workflowInformationProvider.Destination);
		}

		public void TestOrigin()
		{
			AssertEquals(ZString.Empty, workflowInformationProvider.Origin);
		}

		public void TestBusinessContext()
		{
			AssertEquals(TrackingConstants.BusinessContext.NoBusinessContext,
				workflowInformationProvider.BusinessContext);
		}

		public void TestCompanies()
		{
			AssertSequencesEqual(new[] { cusExitHeader.Company.PK }, workflowInformationProvider.Companies);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusExitHeader = Factory.New<CusExitHeader>();
			workflowInformationProvider = new CusExitHeaderWorkflowInformationProvider(cusExitHeader);
		}

		CusExitHeader cusExitHeader;
		CusExitHeaderWorkflowInformationProvider workflowInformationProvider;
	}
}
