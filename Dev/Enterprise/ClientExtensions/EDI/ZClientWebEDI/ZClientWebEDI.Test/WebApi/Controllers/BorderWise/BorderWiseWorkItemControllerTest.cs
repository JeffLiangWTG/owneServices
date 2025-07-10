using System;
using System.Net;
using System.Web.Http.Results;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.BorderWise;

namespace Enterprise.ZClientWebCargoWiseEDI.BorderWise.Testing
{
	public class BorderWiseWorkItemControllerTest : TestCaseWithFactory
	{
		class BorderWiseWorkItemControllerForTest : BorderWiseWorkItemController
		{
			public BusinessObjectFactory Factory => DataFactory;
		}

		public void TestCreateWorkItem_ThrowsArgumentNullException_When_NoModel()
		{
			using (var controller = new BorderWiseWorkItemController())
			{
				AssertExceptionThrown<ArgumentNullException>(() => controller.CreateWorkItem(null));
			}
		}

		public void TestCreateWorkItem_ReturnsForbidden_When_ApiKeyInvalid()
		{
			const string invalidApiKey = "07a16deb-7364-4c44-aaa7-ad9e0e0b4b9d";

			using (var controller = new BorderWiseWorkItemControllerForTest())
			{
				var body = InstantiateCreateWorkItemRequestForTest("BOR", "test", "test", "test", "test", "test", "test", null, null, null, invalidApiKey);
				var query = CreateWorkItemQuery(body);

				var result = controller.CreateWorkItem(body);
				AssertEquals(HttpStatusCode.Forbidden, ((StatusCodeResult)result).StatusCode);
				AssertEquals(0, controller.Factory.Load<WorkItem>(query).Length);
			}
		}

		public void TestCreateWorkItem_ReturnsBadRequest_When_ProductIsNotEqualToBorderWise()
		{
			using (var controller = new BorderWiseWorkItemControllerForTest())
			{
				var body = InstantiateCreateWorkItemRequestForTest("GLW", "test", "test", "test", "test", "test", "test");
				var query = CreateWorkItemQuery(body);

				var result = controller.CreateWorkItem(body);
				AssertType<BadRequestErrorMessageResult>(result);
				AssertEquals("This controller currently only supports BorderWise WIs", ((BadRequestErrorMessageResult)result).Message);
				AssertEquals(0, controller.Factory.Load<WorkItem>(query).Length);
			}
		}

		public void TestCreateWorkItem_ReturnsBadRequest_When_ChangeTypeNotProvided()
		{
			using (var controller = new BorderWiseWorkItemControllerForTest())
			{
				var body = InstantiateCreateWorkItemRequestForTest("BOR", "test", "test", "", "test", "test", "test");
				var query = CreateWorkItemQuery(body);

				var result = controller.CreateWorkItem(body);
				AssertType<BadRequestErrorMessageResult>(result);
				AssertEquals("ChangeType argument must be provided", ((BadRequestErrorMessageResult)result).Message);
				AssertEquals(0, controller.Factory.Load<WorkItem>(query).Length);
			}
		}

		public void TestCreateWorkItem_ReturnsBadRequest_When_SummaryNotProvided()
		{
			using (var controller = new BorderWiseWorkItemControllerForTest())
			{
				var body = InstantiateCreateWorkItemRequestForTest("BOR", "WEB", "MOD", "PRD", "test", " ", "test");
				var query = CreateWorkItemQuery(body);

				var result = controller.CreateWorkItem(body);
				AssertType<BadRequestErrorMessageResult>(result);
				AssertEquals("Summary argument must be provided", ((BadRequestErrorMessageResult)result).Message);
				AssertEquals(0, controller.Factory.Load<WorkItem>(query).Length);
			}
		}

		public void TestCreateWorkItem_ReturnsBadRequest_When_DescriptionNotProvided()
		{
			using (var controller = new BorderWiseWorkItemControllerForTest())
			{
				var body = InstantiateCreateWorkItemRequestForTest("BOR", "WEB", "MOD", "PRD", "test", "My Summary", "  ");
				var query = CreateWorkItemQuery(body);

				var result = controller.CreateWorkItem(body);
				AssertType<BadRequestErrorMessageResult>(result);
				AssertEquals("Description argument must be provided", ((BadRequestErrorMessageResult)result).Message);
				AssertEquals(0, controller.Factory.Load<WorkItem>(query).Length);
			}
		}

		public void TestCreateWorkItem_ReturnsSuccessWithWorkItemNumber_When_ProductIsEqualToBorderWise()
		{
			using (var controller = new BorderWiseWorkItemControllerForTest())
			{
				var body = InstantiateCreateWorkItemRequestForTest("BOR", "WEB", "MOD", "PRD", "UPC", "BWW - Add some new stuff", "We need to add some new stuff into BWW pronto.", "Y", "N", DateTime.Now);
				var query = CreateWorkItemQuery(body);

				AssertEquals(0, controller.Factory.Load<WorkItem>(query).Length);

				var result = controller.CreateWorkItem(body);
				var resultingWorkItem = Factory.LoadTop1<WorkItem>(query);

				AssertNotEquals(null, resultingWorkItem);
				AssertType<OkNegotiatedContentResult<string>>(result);
				AssertEquals(resultingWorkItem.WKI_WorkItemNumber, ((OkNegotiatedContentResult<string>)result).Content);
			}
		}

		CreateWorkItemRequest InstantiateCreateWorkItemRequestForTest(string product, string productArea, string module, string changeType, string priority, string summary, string description, string capDev = null, string rnd = null, DateTime? dueDate = null, string apiKey = "b7c25e84-5c14-4467-ac23-8a36034209da")
		{
			return new CreateWorkItemRequest
			{
				Product = product,
				ProductArea = productArea,
				Module = module,
				ChangeType = changeType,
				Priority = priority,
				Summary = summary,
				Description = description,
				ApiKey = apiKey,
				CapDev = capDev,
				RND = rnd,
				DueDate = dueDate
			};
		}

		ZQuery CreateWorkItemQuery(CreateWorkItemRequest model)
		{
			var query = new ZQuery(WorkItemSchema.WKI_WorkItemType, model.Product);
			query.AddToFilter(WorkItemSchema.WKI_WorkItemArea, model.ProductArea);
			query.AddToFilter(WorkItemSchema.WKI_ActivityType, model.Module);
			query.AddToFilter(WorkItemSchema.WKI_ActivitySubtype, model.ChangeType);
			query.AddToFilter(WorkItemSchema.WKI_Priority, model.Priority);
			query.AddToFilter(WorkItemSchema.WKI_Summary, model.Summary);

			return query;
		}
	}
}
