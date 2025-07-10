using System.Web.Http.Results;
using CargoWise.Bi.BusinessIntelligence.Testing;
using CargoWise.Data.Testing;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence;

namespace CargoWise.Bi.Deployment.AnalysisServices.Testing
{
	class SsasLastUpdatedCubeTest : BaseBusinessIntelligenceTest
	{
		[UseSnapshotProtection]
		public void TestInvalidModel()
		{
			var cubeName = "SampleModel_ModelTablesTest";
			var testBiController = new BiControllerForTest(cubeName);

			var results = testBiController.GetLastModifiedDate(cubeName);
			AssertContains("Invalid table with valid credentials should throw exception", "does not have access to the 'SampleModel_ModelTablesTest' database, or the database does not exist.", ((BadRequestErrorMessageResult)results).Message);
			AssertContains("Message should return correct exception type", "Microsoft.AnalysisServices.AdomdClient.AdomdErrorResponseException", ((BadRequestErrorMessageResult)results).Message);
		}
	}

	class BiControllerForTest : BiController
	{
		public BiControllerForTest(string tabularModelName)
		{
			this.tabularModelName = tabularModelName;
		}
		readonly string tabularModelName;

		protected override string GetTabularModelName(string businessArea)
		{
			return tabularModelName;
		}
	}
}
