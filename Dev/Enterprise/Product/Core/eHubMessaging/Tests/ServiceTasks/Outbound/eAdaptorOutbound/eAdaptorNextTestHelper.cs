using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.eHubMessaging.Tests
{
	public static class eAdaptorNextTestHelper
	{
		#region Utilities

		public static EDIInterchange AddSampleInterchange(BusinessObjectFactory factory)
		{
			var interchange = TestInterchangeMessage.GetNew(TestHelpers.ValidCompanyForTest(factory).FirstActiveBranch.PK, from: "FABLOVGOB", to: "ARCFOOBAR", receiveTransmit: "TRX", factory: factory, onlyCreateInterchange: false);
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange.EI_Status = "AQU";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange.EI_BodyText = "Noodle noodle chicken stroodle";
			var fount = new DirtyNumFount();
			foreach (var message in interchange.ContainedMessages.Cast<EDIMessage>())
			{
				message.MessageNumberStrategy = fount;
				message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Shipments;
			}
			interchange.Factory.Save();
			factory.Save();
			return interchange;
		}

		#endregion

		public static void RunEAdaptorOutboundServiceTask(Notifier notifier, bool runContinuously = false)
		{
			var task = new eAdaptorOutboundServiceTask_Test(runContinuously);
			task.Notifier = notifier;
			task.RunTask(CancellationToken.None);
		}

		const string Certificate = @"-----BEGIN CERTIFICATE-----
MIIEOjCCAyKgAwIBAgIIX4MTnZS2hhcwDQYJKoZIhvcNAQELBQAwgboxKTAnBgkq
hkiG9w0BCQEWGnN1cHBvcnRAd2lzZXRlY2hnbG9iYWwuY29tMQswCQYDVQQGEwJB
VTEPMA0GA1UEBwwGU3lkbmV5MRgwFgYDVQQIDA9OZXcgU291dGggV2FsZXMxGDAW
BgNVBAoMD1dpc2VUZWNoIEdsb2JhbDEeMBwGA1UECwwVRURJL0RBVC9FRElDbGll
bnROYW1lMRswGQYDVQQDDBJ3aXNldGVjaGdsb2JhbC5jb20wHhcNMjQwNzA1MDcw
MzM3WhcNMjUwNzA1MDcwMzM3WjCBujEpMCcGCSqGSIb3DQEJARYac3VwcG9ydEB3
aXNldGVjaGdsb2JhbC5jb20xCzAJBgNVBAYTAkFVMQ8wDQYDVQQHDAZTeWRuZXkx
GDAWBgNVBAgMD05ldyBTb3V0aCBXYWxlczEYMBYGA1UECgwPV2lzZVRlY2ggR2xv
YmFsMR4wHAYDVQQLDBVFREkvREFUL0VESUNsaWVudE5hbWUxGzAZBgNVBAMMEndp
c2V0ZWNoZ2xvYmFsLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB
AJh2uI1Z4DUCLis/ESVZ7qwAagTfoyWEF2O/aljFAmrjDj6DeETAq2IkIPnqnI7Q
EpfbE6uEquMlFAckoQ3z2x9kYIRqyGbKSHhFKJaa1R02VPtmDndGMQJDBstJYFO4
MSVrFMy0hsjymm0LT/3hsKDoCu+/jIZcI6n/of5eGXKbpiyDuV9bNnAym0Aek8CM
T8Zx54JIRfWPepCdz+ydy18vY+adnVtvR/HI8IUNQQGgSPcOrYalpUYEFmsnTtSY
3s53mI49uwtxkOtHRQGMiWuLp9ZPAPnDhoYS3+WZV3WACDcfeF87+NvKzcVYrzNp
uAD1LdFEtW62zkXIYrTnTRcCAwEAAaNCMEAwHwYDVR0jBBgwFoAUXcqGD5uGCnG5
v5A2eg/dn6gqhNEwHQYDVR0OBBYEFF3Khg+bhgpxub+QNnoP3Z+oKoTRMA0GCSqG
SIb3DQEBCwUAA4IBAQB5OTgdKXza9vi7dqwUF+pEzWspyskI4/WJYJV2Euqav8aU
Fw52dmFHntNndYMHtaVAuqrFNOr345E5rg5moggOjvs77ZV+Y/WpnRT7K8NzV/A/
fMVW0Om+fF7DVwJ3u1OyKZNa6Zg4lXch2D3w147A6SHvqNfqFw4XFCBnSBO8vmb9
APVcwWvHyMLFp+29sAvUKQSSyp8gJJz7oyCywHT7llGrgG229GzfB6LoK98385V8
6UscEGYFZHa5U0pJcgwlRRTiM1mYH9KXRccoKtwvGV5pkzr059Tju6nZ9diBy9Lm
Hs2Qrf72nFOdINRlsSdbyoUvvWJvm7k+ANMQXL0v
-----END CERTIFICATE-----";

		const string PrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAmHa4jVngNQIuKz8RJVnurABqBN+jJYQXY79qWMUCauMOPoN4
RMCrYiQg+eqcjtASl9sTq4Sq4yUUByShDfPbH2RghGrIZspIeEUolprVHTZU+2YO
d0YxAkMGy0lgU7gxJWsUzLSGyPKabQtP/eGwoOgK77+Mhlwjqf+h/l4ZcpumLIO5
X1s2cDKbQB6TwIxPxnHngkhF9Y96kJ3P7J3LXy9j5p2dW29H8cjwhQ1BAaBI9w6t
hqWlRgQWaydO1JjezneYjj27C3GQ60dFAYyJa4un1k8A+cOGhhLf5ZlXdYAINx94
Xzv428rNxVivM2m4APUt0US1brbORchitOdNFwIDAQABAoIBAAKe48w2M8blezKu
GlbYhWQ6e5gK2gyOiTJjO2o8NK7uqTOE4f/YifmdYl25XSiNRgyLLPrhRGi0HfSD
eis5ulX/TTNpfHlb18QNeEWicrBWWz6ZAf2l3LjLuyWqZLf7rgiVHx3nqntwxBvE
uoLEKtuRMYLueXVjxw8ogDnVlz1jjZQDQ4ajQNbvW30W3rufm+IWqZ7W/EDokWcc
9ed/eu8EptO5/mH5877gGwGEMeCOOzfLTo8wDebw5kBRXWuVqY8d8gL5+SSOBq5o
VYyv1Y+Vk9l1I3EMqqYlIvaREL+Hmdh9KmlLuGPJ+4xpbC43hTn7VF+09lzXpqaC
7D9/Ew0CgYEA9Qe3Ohe0OslAX9WiDAmL5S7G6Hi6dendPy8yWLAMq85fhcbsoF1Z
D7IyrzNTy/CSonLmiu3bTpW09lPt2s7vsInfT+99gR9rlM1u42vk2ifxcf3FQSJm
CHLnXxZT62qhk4a3ohMz6iIvSFSFOicwvgxsMNn+ieCEjWclvfK5MX0CgYEAn0oa
qcxq/lYc+RIA8fT2wbLtiKNhP/3f1se26N9OU5tcHh+BPYn/jsHWI9zB48uaXwod
1BUSW3ZKiV923vS2NxE8GoFwUeQH+fkG3TSH7TV7NJaGIuF0DxrU5TWHQPTcGRX8
JF30W6Xdkzan6ti0RhWWbcncXkbzFC15rd3x/SMCgYEAuzLC3CIB8quQf+cB33pn
o5diJXce1Tjva/dN2o3dkGChf93jJ/1JLoGw0UNAcN2B2ZQ458kitF4Rm+OxI2rX
miMrNbG9S6nKkiuE3UCv3a+IedMsIT/7fdbzRyUSxhd4C/JvVuae0fB9+R+BjVUl
mvx4p7XUDlg2TKWSIxVOQS0CgYBiIo16vu3L89G1wVnDt1+uxkWBQObRPd+Bu1j8
71aaO8Ts6gv9ld9UXCdJwN/TL8TTeLAX0UOWBbK2H5Jkme8Izh1xVv2T9iDT6JBK
B+sWQTS+mV3ab3vJMoanD+tcIX7YFatZ3GiHbhCseafKD+hApVwgF5UkoCFx9PJa
I7rKcQKBgC49p7C/nftbsPY7AKluCAcSUmPz0Y5WN07RoKv9+N46bmEyzgryzMy+
f/mQa2693fNU0+hzsyy3FJkPcXAyXskZF+QR47JdxIrb9w2f3gl5mwaRD65FahTR
NrBJ/uXxhgOnFsrZNsFqqOgodP0oeJoziC0yi4um/9e4evQ2S4Aq
-----END RSA PRIVATE KEY-----";

		public static EDICommunicationParty CreateEDICommunicationParty(BusinessObjectFactory factory, string name, string endpoint, string authMode, string authEndpoint = null, string flowCode = null)
		{
			var ediCommunicationParty = factory.NewWithValidTestData<EDICommunicationParty>();

			ediCommunicationParty.ECP_Name = name;
			ediCommunicationParty.OutboundConfig.ECC_Endpoint = endpoint;
			ediCommunicationParty.OutboundConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;

			ediCommunicationParty.OutboundConfig.Auth.ECA_AuthorizationMode = authMode;
			if (authMode == EDICommunicationAuthModesList.Codes.OAuthAuthentication)
			{
				if (authEndpoint == null)
				{
					throw new ArgumentNullException(nameof(authEndpoint));
				}
				if (flowCode == null)
				{
					throw new ArgumentNullException(nameof(flowCode));
				}
				ediCommunicationParty.OutboundConfig.Auth.ECA_AuthorizationEndpoint = authEndpoint;
				ediCommunicationParty.OutboundConfig.Auth.ECA_FlowCode = flowCode;

				ediCommunicationParty.OutboundConfig.Auth.ECA_ClientID = "Test Client ID";
				if (flowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials)
				{
					ediCommunicationParty.OutboundConfig.Auth.ECA_ClientSecret = "IAmCoolerThanYou";
				}
				else if (flowCode == EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate)
				{
					var verifiedSubject = string.Format("E=support@wisetechglobal.com,C=AU,L=Sydney,ST=New South Wales,O=WiseTech Global,OU=EDI/DAT/{0},CN=wisetechglobal.com", name);
					ediCommunicationParty.OutboundConfig.Auth.SetPrivateKey(PrivateKey);
					ediCommunicationParty.OutboundConfig.Auth.SetCertificate(Certificate, name, verifiedSubject);
				}
			}
			return ediCommunicationParty;
		}

		public static void DoWithEAdaptorNextEnabled(Action action)
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.EAdaptorNextFeature, CancellationToken.None))
				.Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				action();
			}
		}
	}

	class eAdaptorOutboundServiceTask_Test : eAdaptorOutboundServiceTask
	{
		public eAdaptorOutboundServiceTask_Test(bool runContinuously)
		{
			RunContinuously = runContinuously;
		}

		internal override bool RunContinuously { get; }
	}

	public class DirtyNumFount : IMessageNumberStrategy
	{
		int counter;
		public string GetMessageReferenceNumber()
		{
			return "++ +" + ++counter;
		}
	}

	public sealed class Notifier : INotifications
	{
		public List<string> notifications = new List<string>();

		public void Add(INotification notification)
		{
			notifications.Add(notification.Message);
		}

		public override string ToString()
		{
			return string.Join("\r\n", notifications);
		}

		public void Clear()
		{
			notifications.Clear();
		}
	}
}
