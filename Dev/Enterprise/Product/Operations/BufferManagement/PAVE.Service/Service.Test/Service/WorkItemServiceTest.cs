using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using WTG.RtfConverter;

namespace Enterprise.BufferManagement.Service.Test
{
	public class WorkItemServiceTest : TestCaseWithFactory
	{
		WorkItemService Service;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Service = new WorkItemService();
		}

		protected override void TearDown()
		{
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
			base.TearDown();
		}

		#region Update Details

		public void TestUpdateDetails()
		{
			var workItem = Factory.New<IWorkItem>();
			Factory.Save();

			var content = "<p>new details content</p>";

			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = content,
				PreviousHash = HashHelper.GetHash(workItem.WKI_Details.ToUTF8()),
			};

			var result = Service.TryUpdateDetails(workItem.PK.ToGuid(), updateRequest);

			Assert(result.Success);
			AssertNull(result.Error);

			var newBlob = ZBlob.FromUTF8(new HtmlToRtfConverter().Convert(updateRequest.NewContent));
			workItem = new BusinessObjectFactory().Load<IWorkItem>(workItem.PK);

			AssertEquals(newBlob, workItem.WKI_Details);
			AssertEquals(expected: updateRequest.NewContent, result.DetailsResponse.Content);
			AssertEquals(expected: HashHelper.GetHash(workItem.WKI_Details.ToUTF8()), result.DetailsResponse.Hash);
		}

		public void TestUpdateDetails_ShouldThrownException_WhenHashNullOrEmpty()
		{
			var updateRequest1 = new UpdateContentWithHashRequest()
			{
				NewContent = "Details",
				PreviousHash = null
			};
			var updateRequest2 = new UpdateContentWithHashRequest()
			{
				NewContent = "Notes",
				PreviousHash = string.Empty,
			};
			var updateRequest3 = new UpdateContentWithHashRequest()
			{
				NewContent = null,
				PreviousHash = "hash",
			};
			var updateRequest4 = new UpdateContentWithHashRequest()
			{
				NewContent = string.Empty,
				PreviousHash = "hash",
			};

			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateDetails(Guid.NewGuid(), updateRequest1));
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateDetails(Guid.NewGuid(), updateRequest2));
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateDetails(Guid.NewGuid(), updateRequest3));

			var workItem = Factory.New<IWorkItem>();
			Factory.Save();

			AssertNoExceptionThrown(() => Service.TryUpdateDetails(workItem.PK.ToGuid(), updateRequest4));
		}

		public void TestUpdateDetails_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var workItem = Factory.New<IWorkItem>();
			Factory.Save();
			var content = "<p>new details content</p>";
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = content,
				PreviousHash = "wrong",
			};

			var result = Service.TryUpdateDetails(workItem.PK.ToGuid(), updateRequest);

			Assert(!result.Success);
			AssertNotNull(result.Error);
		}

		#endregion

		#region Update Summary

		public void TestUpdateSummary_ShouldThrownException_WhenHashNullOrEmpty()
		{
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = "New summary content",
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

		public void TestUpdateSummary_ShouldWork()
		{
			var workItem = Factory.New<IWorkItem>();

			workItem.WKI_WorkItemNumber = "WI00000001";
			workItem.WKI_Summary = "WI00000001 Summary";
			workItem.WKI_Status = "OPN";

			Factory.Save();

			var summary = "WI00000001 Summary - Updated Summary";

			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = summary,
				PreviousHash = HashHelper.GetHash(workItem.WKI_Summary),
			};

			var result = Service.TryUpdateSummary(workItem.PK.ToGuid(), updateRequest, out PaveError error);

			AssertNull(error);
			AssertEquals(expected: true, result);

			workItem = new BusinessObjectFactory().Load<IWorkItem>(workItem.PK);

			AssertEquals(summary, workItem.WKI_Summary);
		}

		public void TestUpdateSummary_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var workItem = Factory.New<IWorkItem>();

			workItem.WKI_WorkItemNumber = "WI00000001";
			workItem.WKI_Summary = "WI00000001 Summary";
			workItem.WKI_Status = "OPN";

			Factory.Save();

			var summary = "WI00000001 Summary - Updated Summary";

			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = summary,
				PreviousHash = "wrong",
			};

			var result = Service.TryUpdateSummary(workItem.PK.ToGuid(), updateRequest, out PaveError error);

			AssertEquals(expected: false, result);
			AssertNotNull(error);
		}

		#endregion
	}
}
