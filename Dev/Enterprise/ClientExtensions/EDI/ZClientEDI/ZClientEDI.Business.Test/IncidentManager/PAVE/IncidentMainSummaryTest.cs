using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentMainSummaryTest : TestCaseWithFactory
	{
		IncidentService Service;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Service = new IncidentService();
		}

		protected override void TearDown()
		{
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
			base.TearDown();
		}

		#region Update Summary
		public void TestUpdateSummary_ShouldWork_WithIncidentMain()
		{
			var incident = Factory.New<IncidentMainBase>();

			Factory.Save();

			var summary = "oh no :(";
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = summary,
				PreviousHash = HashHelper.GetHash(incident.IM_Description),
			};

			var result = Service.TryUpdateSummary(incident.PK.ToGuid(), updateRequest, out PaveError error);

			AssertNull(error);
			AssertEquals(expected: true, result);

			incident = new BusinessObjectFactory().Load<IncidentMainBase>(incident.PK);

			AssertEquals(summary, incident.IM_Description);
		}

		public void TestUpdateSummary_ShouldReturnFalse_WhenPreviousHashDoesNotMatch_WithIncidentMain()
		{
			var incident = Factory.New<IncidentMainBase>();
			incident.IM_Description = "This should return false";

			Factory.Save();

			var summary = "oh no";

			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = summary,
				PreviousHash = "wrong",
			};

			var result = Service.TryUpdateSummary(incident.PK.ToGuid(), updateRequest, out PaveError error);

			AssertEquals(expected: false, result);
			AssertNotNull(error);
		}
		#endregion
	}
}
