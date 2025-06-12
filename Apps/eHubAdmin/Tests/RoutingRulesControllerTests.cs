using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rhino.Mocks;
using System.Web;
using System.Web.Routing;

namespace eServices.eHubAdmin.Tests
{
    [TestClass]
    public class RoutingRulesControllerTests
    {
        eHubTransactionsContext mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
        TestDbSet<eHubRoutingRule> testRoutingRules = new TestDbSet<eHubRoutingRule>();
        TestDbSet<eHubClient> testClients = new TestDbSet<eHubClient>();

        [TestInitialize]
        public void RoutingRulesController_TestInitialize()
        {
            mockContext.Stub(x => x.eHubRoutingRules).Return(testRoutingRules);
            mockContext.Stub(x => x.eHubClients).Return(testClients);

            var rule1 = testRoutingRules.Add(new eHubRoutingRule
            {
                RR_PK = new Guid(1, 1, 1, new byte[8]),
                RR_Group_MatchMultiple = false
            });
            var client1 = testClients.Add(new eHubClient { CC_ID = "SVC1", CC_FriendlyName = "Service 1", CC_RR = rule1.RR_PK, eHubRoutingRule = rule1 });

            var rule2 = testRoutingRules.Add(new eHubRoutingRule { RR_PK = new Guid(1, 1, 2, new byte[8]) });
            var client2 = testClients.Add(new eHubClient { CC_ID = "SVC2", CC_FriendlyName = "Service 2", CC_RR = rule2.RR_PK, eHubRoutingRule = rule2 });
        }


        [TestMethod]
        public void RoutingRulesController_Index()
        {
            var routingRulesController = new RoutingRulesController(mockContext);
            var result = routingRulesController.Index() as ViewResult;
            Assert.IsNotNull(result);
            var resultData = (result.Model as List<eHubClient>).Select(c => new { c.CC_ID, c.CC_FriendlyName, c.CC_RR });
            var jsonResult = JsonConvert.SerializeObject(resultData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Assert.AreEqual(
@"[
  {
    ""CC_ID"": ""SVC1"",
    ""CC_FriendlyName"": ""Service 1"",
    ""CC_RR"": ""00000001-0001-0001-0000-000000000000""
  },
  {
    ""CC_ID"": ""SVC2"",
    ""CC_FriendlyName"": ""Service 2"",
    ""CC_RR"": ""00000001-0001-0002-0000-000000000000""
  }
]",
            jsonResult);
        }

        [TestMethod]
        public void RoutingRulesController_Details()
        {
            var routingRulesController = new RoutingRulesController(mockContext);
            var result = routingRulesController.Details("SVC1") as ViewResult;
            Assert.IsNotNull(result);
            var jsonResult = JsonConvert.SerializeObject(result.Model, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Assert.AreEqual(
@"{
  ""Facts"": [],
  ""ServiceProviders"": [],
  ""RuleID"": ""SVC1"",
  ""ServiceName"": ""Service 1"",
  ""Timestamp"": ""0001-01-01T00:00:00"",
  ""MatchMultiple"": false,
  ""SubRules"": [],
  ""ReferenceID"": ""00000001-0001-0001-0000-000000000000""
}",
            jsonResult);
        }
    }
}
