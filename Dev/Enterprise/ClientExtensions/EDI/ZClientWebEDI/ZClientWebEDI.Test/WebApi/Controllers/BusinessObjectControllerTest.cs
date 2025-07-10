using System;
using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class BusinessObjectControllerTest : TestCaseWithFactory
	{
		public void TestUpdateBusinessObject_WhenNoPKGiven_ReturnsBadRequest()
		{
			var json = @"{""Something"": ""Not a GUID""}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			var controller = new ControllerForTest<WorkItemProcessTask>(_ =>
			{
			});
			var response = controller.UpdateBusinessObject(obj);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var responseString = response.Content.ReadAsStringAsync().Result;
			AssertContains($"Request must contain an object identifier", responseString);
			AssertContains(controller.RouteName, responseString);
		}

		public void TestUpdateBusinessObject_WhenInvalidPK_ReturnsBadRequest()
		{
			var json = @"{""PK"": ""Not a GUID""}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			var controller = new ControllerForTest<WorkItemProcessTask>(_ =>
			{
			});
			var response = controller.UpdateBusinessObject(obj);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var responseString = response.Content.ReadAsStringAsync().Result;
			AssertContains($"'Not a GUID' is not a valid identifier", responseString);
			AssertContains(controller.RouteName, responseString);
		}

		public void TestUpdateBusinessObject_WhenCantFindBizO_ReturnsNotFound()
		{
			var pk = Guid.NewGuid();
			var json = $@"{{""PK"": ""{pk}""}}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			var controller = new ControllerForTest<WorkItemProcessTask>(_ =>
			{
			});
			var response = controller.UpdateBusinessObject(obj);
			AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
			var responseString = response.Content.ReadAsStringAsync().Result;
			AssertContains($"{nameof(WorkItemProcessTask)} '{pk}' could not be found", responseString);
			AssertContains(controller.RouteName, responseString);
		}

		public void TestUpdateBusinessObject_WhenNoError_ReturnsOK()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = "WRK";
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = "WRK";
			Factory.Save();
			var pk = Guid.NewGuid();
			var json = $@"{{""PK"": ""{task.PK}""}}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			var controller = new ControllerForTest<WorkItemProcessTask>(_ =>
			{
			});
			var response = controller.UpdateBusinessObject(obj);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestUpdateBusinessObject_DoesNotCatchExceptions()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Status = "WRK";
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = "WRK";
			Factory.Save();
			var pk = Guid.NewGuid();
			var json = $@"{{""PK"": ""{task.PK}""}}";
			var obj = (JObject)JsonConvert.DeserializeObject(json);
			var exceptionMessage = Guid.NewGuid().ToString();
			var controller = new ControllerForTest<WorkItemProcessTask>(_ =>
			{
				throw new InvalidOperationException(exceptionMessage);
			});
			AnonymousMethod apiCall = () => controller.UpdateBusinessObject(obj);
			AssertExceptionThrown<InvalidOperationException>("Exception should not be caught", exceptionMessage, apiCall);
		}
	}
}