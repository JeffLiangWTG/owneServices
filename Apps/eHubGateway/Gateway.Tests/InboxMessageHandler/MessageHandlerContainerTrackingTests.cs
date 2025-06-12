using System;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests
{
  [TestClass]
  public class MessageHandlerContainerTrackingTests
  {

    [TestMethod]
    public void InboxOutboxMessageHandler_GetHandler_ContainerTracking()
    {
      var gctMsg = new eHubGatewayMessage
      {
        ApplicationCode = "UDM",
        ClientID = "CW1CW1CW1"
      };

      var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
      mockPartyAccessor.Stub(x => x.IsCW1System("CW1CW1CW1")).Return(true);

      MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
      var handler = MessageHandlerFactory.CreateMessageHandler("CONTAINER_TRACKING", gctMsg);
      Assert.IsInstanceOfType(handler, typeof(InboxOutboxMessageHandler));
    }

    [TestMethod]
    public void DefaultInboxMessageHandler_GetHandler_ContainerTracking()
    {
      var gctMsg = new eHubGatewayMessage
      {
        ApplicationCode = "UDM",
        ClientID = "CW1CW1CW1"
      };

      var mockPartyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
      mockPartyAccessor.Stub(x => x.IsCW1System("CW1CW1CW1")).Return(false);

      MessageHandlerFactory.GetPartyAccessor = () => mockPartyAccessor;
      var handler = MessageHandlerFactory.CreateMessageHandler("CONTAINER_TRACKING", gctMsg);
      Assert.IsInstanceOfType(handler, typeof(DefaultInboxMessageHandler));
    }
  }
}
