using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.BufferManagement.Service.Test
{
	public class IncidentServiceTest : TestCaseWithFactory
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

		public void TestUpdateSummary_ShouldThrownException_WhenHashNullOrEmpty()
		{
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = "This should not update",
				PreviousHash = null
			};

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var result = Service.TryUpdateSummary(Guid.NewGuid(), updateRequest, out PaveError error);
			});
		}

		public void TestUpdateSummary_ShouldThrownException_WhenSummaryNull()
		{
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = null,
				PreviousHash = Guid.NewGuid().ToString(),
			};

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var result = Service.TryUpdateSummary(Guid.NewGuid(), updateRequest, out PaveError error);
			});
		}

		public void TestUpdateSummary_ShouldWork_WithIncidentRequest()
		{
			var incident = Factory.New<IncidentRequest>();

			Factory.Save();

			var summary = "oh no :(";
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = summary,
				PreviousHash = HashHelper.GetHash(incident.INC_Summary),
			};

			var result = Service.TryUpdateSummary(incident.PK.ToGuid(), updateRequest, out PaveError error);

			AssertNull(error);
			AssertEquals(expected: true, result);

			incident = new BusinessObjectFactory().Load<IncidentRequest>(incident.PK);

			AssertEquals(summary, incident.INC_Summary);
		}

		public void TestUpdateSummary_ShouldReturnFalse_WhenPreviousHashDoesNotMatch_WithIncidentRequest()
		{
			var incident = Factory.New<IncidentRequest>();
			incident.INC_Summary = "This should return false";

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
