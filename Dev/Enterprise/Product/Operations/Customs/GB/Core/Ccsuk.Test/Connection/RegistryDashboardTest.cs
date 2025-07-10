using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.Connection;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.Testing
{
	public class RegistryDashboardTest : TransactionedTestCase
	{
		public void TestRegistryDashboardUpdate()
		{
			var initialDateTime = ZDateTime.UtcNow.ToDateTime();
			var logger = new TestServiceLogger();
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Anything");
			GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Anything");
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL = "127.2.3.4";
			var pair = new CcsukIpaddressesSetting()
			{
				FriendlyName = "Sweety",
				LocalIpAddress = "127.1.2.3",
				CcsukParticipantIpAddress = "172.22.1.2",
				Transport = "C5"
			};
			var connection = new TcpIpSenderReceiverForRegistryDashboardTest(logger, pair);

			connection.ConnectAndLogonAndStartReceivingInboundMessages();
			var nowDateTime = ZDateTime.UtcNow.ToDateTime();

			CombineAssertions("Registry properties after logon", () =>
			{
				Assert("CcsukLastLogonDateTime should have been updated", GBCustomsDataRegistry.Instance.CcsukLastLogonDateTime.Value >= initialDateTime && GBCustomsDataRegistry.Instance.CcsukLastLogonDateTime.Value <= nowDateTime);
				AssertEquals("CcsukIsConnected", true, GBCustomsDataRegistry.Instance.CcsukIsConnected.Value);
				AssertEquals("CcsukConnectedProcessController", System.Environment.MachineName, GBCustomsDataRegistry.Instance.CcsukConnectedProcessController.Value);
				AssertEquals("CcsukProcessID", System.Diagnostics.Process.GetCurrentProcess().Id, GBCustomsDataRegistry.Instance.CcsukProcessID.Value);
			});

			connection.Shutdown();

			AssertEquals("CcsukIsConnected after shutdown", false, GBCustomsDataRegistry.Instance.CcsukIsConnected.Value);
		}

		class TcpIpSenderReceiverForRegistryDashboardTest : TcpIpSenderReceiver
		{
			public TcpIpSenderReceiverForRegistryDashboardTest(ILogger logger, CcsukIpaddressesSetting pair)
				: base(logger, pair)
			{
			}

			protected override void ConnectAndLogonAndStartReceivingInboundMessagesCore()
			{
				Logon();
			}

			protected override Body UploadAndGetResponse(Body payloadToUpload, EDIInterchange outboundInterchange)
			{
				var parser = new ResponseParser();
				return parser.LoadMessageFromText("SM02TESTHOSTXY0000");
			}
		}
	}
}
