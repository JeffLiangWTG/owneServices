using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared.Common;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using WTG.RtfConverter;

namespace Enterprise.BufferManagement.Service.Test
{
	public class ProjectServiceTest : TestCaseWithFactory
	{
		ProjectService Service;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Service = new ProjectService();
		}

		protected override void TearDown()
		{
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
			base.TearDown();
		}

		public void TestGetDetails_ShouldReturnCorrectResponse()
		{
			var project = Factory.New<IProject>();

			var content = "<p>Project One Details</p>";

			project.WKP_Details = ZBlob.FromUTF8(new HtmlToRtfConverter().Convert(content));

			Factory.Save();

			var result = Service.GetDetails(project.PK.ToGuid());

			AssertEquals(expected: content, result.Content);
		}

		#region Update Details

		public void TestUpdateDetails()
		{
			var project = Factory.New<IProject>();
			Factory.Save();

			var content = "<p>new details content</p>";

			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = content,
				PreviousHash = HashHelper.GetHash(project.WKP_Details.ToUTF8()),
			};

			var result = Service.TryUpdateDetails(project.PK.ToGuid(), updateRequest);

			Assert(result.Success);
			AssertNull(result.Error);

			var newBlob = ZBlob.FromUTF8(new HtmlToRtfConverter().Convert(updateRequest.NewContent));
			project = new BusinessObjectFactory().Load<IProject>(project.PK);

			AssertEquals(newBlob, project.WKP_Details);
			AssertEquals(expected: updateRequest.NewContent, result.DetailsResponse.Content);
			AssertEquals(expected: HashHelper.GetHash(project.WKP_Details.ToUTF8()), result.DetailsResponse.Hash);
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

			var project = Factory.New<IProject>();
			Factory.Save();

			AssertNoExceptionThrown(() => Service.TryUpdateDetails(project.PK.ToGuid(), updateRequest4));
		}

		public void TestUpdateDetails_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var project = Factory.New<IProject>();
			Factory.Save();
			var content = "<p>new details content</p>";
			var updateRequest = new UpdateContentWithHashRequest()
			{
				NewContent = content,
				PreviousHash = "wrong",
			};

			var result = Service.TryUpdateDetails(project.PK.ToGuid(), updateRequest);

			Assert(!result.Success);
			AssertNotNull(result.Error);
		}

		#endregion

		#region Update Summary

		public void TestUpdateSummary_ShouldThrowException_WhenSummaryIsNull()
		{
			var request = new UpdateContentWithHashRequest
			{
				PreviousHash = Guid.NewGuid().ToString(),
				NewContent = null,
			};
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateSummary(Guid.NewGuid(), request, out _));
		}

		public void TestUpdateSummary_ShouldThrowException_WhenHashIsNullOrEmpty()
		{
			var request = new UpdateContentWithHashRequest
			{
				PreviousHash = null,
				NewContent = "foo",
			};
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateSummary(Guid.NewGuid(), request, out _));

			request.PreviousHash = string.Empty;
			AssertExceptionThrown<ArgumentNullException>(() => Service.TryUpdateSummary(Guid.NewGuid(), request, out _));
		}

		public void TestUpdateSummary_ShouldWork()
		{
			var project = Factory.New<IProject>();
			project.WKP_Summary = "hello there";
			Factory.Save();

			var request = new UpdateContentWithHashRequest
			{
				PreviousHash = HashHelper.GetHash(project.WKP_Summary),
				NewContent = "foo bar",
			};

			var result = Service.TryUpdateSummary(project.PK.ToGuid(), request, out PaveError error);
			AssertNull(error);
			Assert("Attempt to update project summary should be successful", result);

			project = new BusinessObjectFactory().Load<IProject>(project.PK);
			AssertEquals(request.NewContent, project.WKP_Summary);
		}

		public void TestUpdateSummary_ShouldReturnFalse_WhenPreviousHashDoesNotMatch()
		{
			var project = Factory.New<IProject>();
			project.WKP_Summary = "hello world";
			Factory.Save();

			var request = new UpdateContentWithHashRequest
			{
				PreviousHash = "hello there",
				NewContent = "foobar",
			};

			var result = Service.TryUpdateSummary(project.PK.ToGuid(), request, out PaveError error);
			Assert("Attempt to update summary should fail", !result);
			AssertNotNull(error);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.FieldChangedByAnotherUserMessage, error.Messages.FirstOrDefault());
		}

		#endregion

	}
}
