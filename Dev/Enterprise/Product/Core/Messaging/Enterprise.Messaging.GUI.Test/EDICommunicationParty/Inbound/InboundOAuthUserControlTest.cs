using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Messaging.GUI.Test
{
	public class InboundOAuthUserControlTest : TestCaseWithFactory
	{
		public void TestClientIdAuthorityUrlScopeReadOnlyStatus()
		{
			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			using (var form = new ConfigContainerForm(commParty))
			{
				Assert(form.ConfigControl.AuthorityUrlIsReadOnly);
				Assert(form.ConfigControl.ClientIdIsReadOnly);
				Assert(((ZTextBox)form.Controls.Find("ScopeTextBox", true).First()).ReadOnly);
			}

			GlbStaff.CurrentUser.GS_LoginName = "Dummy";

			using (var form = new ConfigContainerForm(commParty))
			{
				Assert(form.ConfigControl.AuthorityUrlIsReadOnly);
				Assert(form.ConfigControl.ClientIdIsReadOnly);
				Assert(((ZTextBox)form.Controls.Find("ScopeTextBox", true).First()).ReadOnly);
			}
		}

		public void TestRegisterCertificate()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth();

			using (var form = new ConfigContainerForm(commParty))
			{
				Assert(!form.ConfigControl.RegisterCertificate("123"));

				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.RegisterCertificate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("o1");
				mockDescriptors.Setup(d => d.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), EDICommunicationAuth.CaRoot)).Returns("o2");

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					Assert(form.ConfigControl.RegisterCertificate(SampleCsrPem));
					AssertEquals("o1", auth.ECA_OperationId);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);
					AssertExceptionThrown<Exception>("A certificate renewal cannot be issued while another one is already in progress.", () => form.ConfigControl.RegisterCertificate(SampleCsrPem));

					auth.ECA_ClientID = "c1";
					Assert(form.ConfigControl.RegisterCertificate(SampleCsrPem));
					AssertEquals("o1", auth.ECA_OperationId);
					AssertEquals("o2", auth.ECA_RenewalOperationId);

					AssertExceptionThrown<Exception>("A certificate renewal cannot be issued while another one is already in progress.", () => form.ConfigControl.RegisterCertificate(SampleCsrPem));

					mockDescriptors.Verify(d => d.RegisterCertificate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
					mockDescriptors.Verify(d => d.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), EDICommunicationAuth.CaRoot), Times.Once);
				}
			}
		}

		public void TestRegisterCertificateCorrectlySetsAwaitingCertificateGenerationLabel()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth();

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.RegisterCertificate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("o1");
				mockDescriptors.Setup(d => d.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), EDICommunicationAuth.CaRoot)).Returns("");

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					_ = form.ConfigControl.RegisterCertificate(SampleCsrPem);
					Assert(((ZLabel)form.Controls.Find("awaitingCertificateGenerationLabel", true).First()).Visible);

					auth.ECA_ClientID = "c1";
					_ = form.ConfigControl.RegisterCertificate(SampleCsrPem);
					Assert(!((ZLabel)form.Controls.Find("awaitingCertificateGenerationLabel", true).First()).Visible);
				}
			}
		}

		public void TestRenewCertificateWhenCertificateHasBeenReceived()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(true, true, false, true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o1")).Returns(("t1", "c1", SampleCertificateBlob, CertificateProcessingCodes.Completed));
				mockDescriptors.Setup(d => d.DownloadCertificates("c1")).Returns(new List<byte[]> { SampleCertificateBlob });
				mockDescriptors.Setup(d => d.RolloverCertificate("c1", It.IsAny<string>(), EDICommunicationAuth.CaRoot)).Returns("o2");

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertNullOrEmpty(auth.ECA_RenewalOperationId);
					form.ConfigControl.RegisterCertificate(SampleCsrPem);
					AssertEquals("o2", auth.ECA_RenewalOperationId);

					AssertExceptionThrown<Exception>("A certificate renewal cannot be issued while another one is already in progress.", () => form.ConfigControl.RegisterCertificate(SampleCsrPem));

					mockDescriptors.Verify(d => d.DownloadCertificate("o1"), Times.Never);
					mockDescriptors.Verify(d => d.DownloadCertificates("c1"), Times.Once);
					mockDescriptors.Verify(d => d.RolloverCertificate("c1", It.IsAny<string>(), EDICommunicationAuth.CaRoot), Times.Once);
				}
			}
		}

		public void TestNoCertificateExists_WithEmptyClientIDAndEmptyOperationIdAndEmptyRenewalOperationId_NothingSet()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth();

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate(string.Empty)).Throws(new CertificateManagementException(string.Empty));

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertNullOrEmpty(auth.ECA_AuthorizationEndpoint);
					AssertNullOrEmpty(auth.ECA_ClientID);
					Assert(auth.ECA_Certificate.IsEmpty);
					AssertNullOrEmpty(auth.ECA_OperationId);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);

					mockDescriptors.Verify(d => d.DownloadCertificate(string.Empty), Times.Never);
				}
			}
		}

		public void TestNoCertificateExists_WithEmptyClientIDAndNonEmptyOperationId_NothingSet_EmptyGrid()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o1")).Throws(new CertificateManagementException(string.Empty));

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertNullOrEmpty(auth.ECA_AuthorizationEndpoint);
					AssertNullOrEmpty(auth.ECA_ClientID);
					Assert(auth.ECA_Certificate.IsEmpty);
					AssertEquals("o1", auth.ECA_OperationId);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);
					AssertNullOrEmpty(auth.ECA_Scopes);

					var scopeTextBox = (ZTextBox)form.Controls.Find("ScopeTextBox", true).First();
					AssertNullOrEmpty(scopeTextBox.Text);

					var grid = (ZGrid)form.Controls.Find("CertificatesGrid", true).First();
					AssertExceptionThrown<Exception>("No row exists.", () => grid.Select(0));

					mockDescriptors.Verify(d => d.DownloadCertificate("o1"), Times.Once);
				}
			}
		}

		public void TestCertificateReceived_WithEmptyClientIDAndNonEmptyOperationId_SetsCertificate_SetsScope()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o1")).Returns(("t1", "c1", SampleCertificateBlob, CertificateProcessingCodes.Completed));
				mockDescriptors.Setup(d => d.DownloadCertificates("c1")).Returns(new List<byte[]> { SampleCertificateBlob });

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertContains("/t1/", auth.ECA_AuthorizationEndpoint);
					AssertEquals("c1", auth.ECA_ClientID);
					AssertEquals(SampleCertificateBlob, auth.ECA_Certificate);
					AssertEquals("o1", auth.ECA_OperationId);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);
					AssertEquals("c1/.default", auth.ECA_Scopes);

					var scopeTextBox = (ZTextBox)form.Controls.Find("ScopeTextBox", true).First();
					AssertEquals("c1/.default", scopeTextBox.Text);

					mockDescriptors.Verify(d => d.DownloadCertificate("o1"), Times.Once);
					mockDescriptors.Verify(d => d.DownloadCertificates("c1"), Times.Once);
				}
			}
		}

		public void TestCertificateReceived_WithNonEmptyClientIDAndNonEmptyOperationId_SetsCertificate()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setClientID: true, setOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o1")).Returns(("t1", "c1", SampleCertificateBlob, CertificateProcessingCodes.Completed));
				mockDescriptors.Setup(d => d.DownloadCertificates("c1")).Returns(new List<byte[]> { SampleCertificateBlob });

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertContains("/t1/", auth.ECA_AuthorizationEndpoint);
					AssertEquals("c1", auth.ECA_ClientID);
					AssertEquals(SampleCertificateBlob, auth.ECA_Certificate);
					AssertEquals("o1", auth.ECA_OperationId);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);

					mockDescriptors.Verify(d => d.DownloadCertificate("o1"), Times.Once);
					mockDescriptors.Verify(d => d.DownloadCertificates("c1"), Times.Once);
				}
			}
		}

		public void TestCertificateReceived_WithNonEmptyClientIDAndNonEmptyRenewalOperationId_SetsCertificate()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setClientID: true, setOperationId: true, setRenewalOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o2")).Returns(("t1", "c1", SampleCertificateBlob, CertificateProcessingCodes.Completed));
				mockDescriptors.Setup(d => d.DownloadCertificates("c1")).Returns(new List<byte[]> { SampleCertificateBlob });

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertContains("/t1/", auth.ECA_AuthorizationEndpoint);
					AssertEquals("c1", auth.ECA_ClientID);
					AssertEquals(SampleCertificateBlob, auth.ECA_Certificate);
					AssertEquals("o2", auth.ECA_OperationId);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);

					mockDescriptors.Verify(d => d.DownloadCertificate("o2"), Times.Once);
					mockDescriptors.Verify(d => d.DownloadCertificates("c1"), Times.Once);
				}
			}
		}

		public void TestNoCertificateReceived_KeepsCertificate()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setClientID: true, setOperationId: true, setRenewalOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o2")).Throws(new CertificateManagementException(string.Empty));

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					AssertEquals("c1", auth.ECA_ClientID);
					Assert(auth.ECA_Certificate.IsEmpty);
					AssertEquals("o1", auth.ECA_OperationId);
					AssertEquals("o2", auth.ECA_RenewalOperationId);

					mockDescriptors.Verify(d => d.DownloadCertificate("o2"), Times.Once);
				}
			}
		}

		public void TestCertificateExists_NonEmptyGrid()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var mockDescriptors = new Mock<ICertificateManager>();
				mockDescriptors.Setup(d => d.DownloadCertificate("o1")).Returns(("t1", "c1", SampleCertificateBlob, CertificateProcessingCodes.Completed));
				mockDescriptors.Setup(d => d.DownloadCertificates("c1")).Returns(new List<byte[]> { SampleCertificateBlob });

				using (ObjectFactory.Substitute(mockDescriptors.Object))
				{
					form.Show();

					var grid = (ZGrid)form.Controls.Find("CertificatesGrid", true).First();
					grid.Select(0);

					AssertType(typeof(CertificateData), grid.SelectedElements[0]);
					AssertEquals(SampleCertificatePem, ((CertificateData)(grid.SelectedElements[0])).CertificatePem);

					mockDescriptors.Verify(d => d.DownloadCertificate("o1"), Times.Once);
					mockDescriptors.Verify(d => d.DownloadCertificates("c1"), Times.Once);
				}
			}
		}

		public void TestRecievedCertificate_SetsScopeTextBoxWithScopeValue_AfterCertificateRefresh()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setOperationId: true, setClientID: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var certificateManagerMock = new Mock<ICertificateManager>();
				certificateManagerMock.Setup(c => c.DownloadCertificate(It.IsAny<string>())).Returns(("t1", "c1", SampleCertificateBlob, CertificateProcessingCodes.Completed));
				using (ObjectFactory.Substitute(certificateManagerMock.Object))
				{
					form.Show();

					var refreshButton = (ZButton)form.Controls.Find("RefreshCertificatesButton", true).First();
					refreshButton.PerformClick();

					var scopeTextBox = (ZTextBox)form.Controls.Find("ScopeTextBox", true).First();
					AssertEquals(commParty.InboundConfig.Auth.ClientID + "/.default", scopeTextBox.Text);
				}
			}
		}

		public void TestRegisterCertificate_ShowsStatusAndDoesNotSetOperationIdWhenCertificateIsEmptyAndUserCancels()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth();

			using (var form = new ConfigContainerForm(commParty))
			{
				var certificateManagerMock = new Mock<ICertificateManager>();
				certificateManagerMock.Setup(c => c.DownloadCertificate(It.IsAny<string>())).Returns(("t1", "c1", ZBlob.Empty, CertificateProcessingCodes.Failed));
				certificateManagerMock.Setup(c => c.RegisterCertificate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("test");

				using (ObjectFactory.Substitute(certificateManagerMock.Object))
				{
					form.Show();

					form.ConfigControl.RegisterCertificate(SampleCsrPem);
					((ZButton)form.Controls.Find("RefreshCertificatesButton", true).First()).PerformClick();

					AssertEquals("The certificate generation has failed. Please try generating a new certificate with another certificate request file.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNullOrEmpty(auth.ECA_OperationId);
					AssertEquals(ZBlob.Empty, auth.ECA_Certificate);

					Assert(!((ZLabel)form.Controls.Find("awaitingCertificateGenerationLabel", true).First()).Visible);
					Assert(!((ZButton)form.Controls.Find("RegisterCertificateButton", true).First()).ReadOnly);
					AssertEquals("Generate Certificate", ((ZButton)form.Controls.Find("RegisterCertificateButton", true).First()).Text);
					Assert(!((ZTextBox)form.Controls.Find("CertificateRequestFileTextBox", true).First()).ReadOnly);
					Assert(!((ZButton)form.Controls.Find("OpenFileButton", true).First()).ReadOnly);
					Assert(!((ZButton)form.Controls.Find("VerifyButton", true).First()).ReadOnly);
				}
			}
		}

		public void TestRenewCertificate_ShowsStatusAndDoesNotSetRenewalOperationIdIfCertificateIsEmptyAndUserCancels()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setClientID: true, setOperationId: true, setRenewalOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var certificateManagerMock = new Mock<ICertificateManager>();
				certificateManagerMock.Setup(c => c.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), EDICommunicationAuth.CaRoot)).Returns("o2");
				certificateManagerMock.Setup(c => c.DownloadCertificate(It.IsAny<string>())).Returns(("t1", "c1", ZBlob.Empty, CertificateProcessingCodes.Failed));

				using (ObjectFactory.Substitute(certificateManagerMock.Object))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
					((ZButton)form.Controls.Find("RefreshCertificatesButton", true).First()).PerformClick();

					AssertEquals("The certificate generation has failed. Please try generating a new certificate with another certificate request file.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNullOrEmpty(auth.ECA_RenewalOperationId);
					AssertEquals("o1", auth.ECA_OperationId);

					Assert(!((ZLabel)form.Controls.Find("awaitingCertificateGenerationLabel", true).First()).Visible);
					Assert(!((ZButton)form.Controls.Find("RegisterCertificateButton", true).First()).ReadOnly);
					AssertEquals("Generate Certificate", ((ZButton)form.Controls.Find("RegisterCertificateButton", true).First()).Text);
					Assert(!((ZTextBox)form.Controls.Find("CertificateRequestFileTextBox", true).First()).ReadOnly);
					Assert(!((ZButton)form.Controls.Find("OpenFileButton", true).First()).ReadOnly);
					Assert(!((ZButton)form.Controls.Find("VerifyButton", true).First()).ReadOnly);
				}
			}
		}

		public void TestRefreshCertificate_ReportsUnkownCertificateProcessingStatusCodes()
		{
			var (auth, commParty) = CreateEDICommunicationPartyAndAuth(setClientID: true, setOperationId: true, setRenewalOperationId: true);

			using (var form = new ConfigContainerForm(commParty))
			{
				var certificateManagerMock = new Mock<ICertificateManager>();
				certificateManagerMock.Setup(c => c.DownloadCertificate(It.IsAny<string>())).Returns(("t1", "c1", ZBlob.Empty, "WAH"));

				using (ObjectFactory.Substitute(certificateManagerMock.Object))
				{
					form.Show();

					AssertEquals("Unexpected code WAH was found. Please check if this code should be added to CertificateProcessingCodes.", ErrorReporter.LastMessageReported);
				}
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestObjectVisibilityWhenIsNotSelfHosted()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var (_, commParty) = CreateEDICommunicationPartyAndAuth();

			using (var form = new ConfigContainerForm(commParty))
			{
				form.Show();
				Assert(!form.ConfigControl.IsSelfManagedCheckBoxVisible);
			}
		}

		public void TestObjectVisibilityWhenIsSelfHosted()
		{
			EnvProxy.SetHostedLocationForTest("");
			var (_, commParty) = CreateEDICommunicationPartyAndAuth();

			using (var form = new ConfigContainerForm(commParty))
			{
				form.Show();
				Assert(form.ConfigControl.IsSelfManagedCheckBoxVisible);
				Assert(form.ConfigControl.ScopeTextBoxVisible);
				Assert(form.ConfigControl.CertificateGroupBoxVisible);
				Assert(form.ConfigControl.AuthorityUrlIsReadOnly);
				Assert(form.ConfigControl.ClientIdIsReadOnly);
			}

			commParty.Configs[0].ECC_IsSelfManaged = true;
			using (var form = new ConfigContainerForm(commParty))
			{
				form.Show();
				Assert(form.ConfigControl.IsSelfManagedCheckBoxVisible);
				Assert(!form.ConfigControl.ScopeTextBoxVisible);
				Assert(!form.ConfigControl.CertificateGroupBoxVisible);
				Assert(!form.ConfigControl.AuthorityUrlIsReadOnly);
				Assert(!form.ConfigControl.ClientIdIsReadOnly);
			}
		}

		(EDICommunicationAuth auth, EDICommunicationParty commParty) CreateEDICommunicationPartyAndAuth(bool setClientID = false, bool setOperationId = false, bool setRenewalOperationId = false, bool setCertificate = false)
		{
			var auth = Factory.NewWithValidTestData<EDICommunicationAuth>();
			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;

			Factory.Save();

			var commParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			var config = commParty.Configs.AddNew();
			config.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			config.ECC_ECA_Auth = auth.PK;

			if (setClientID) { auth.ECA_ClientID = "c1"; }
			if (setOperationId) { auth.ECA_OperationId = "o1"; }
			if (setRenewalOperationId) { auth.ECA_RenewalOperationId = "o2"; }
			if (setCertificate) { auth.ECA_Certificate = SampleCertificateBlob; }

			return (auth, commParty);
		}

		readonly string SampleCsrPem = @"-----BEGIN CERTIFICATE REQUEST-----
MIIE8TCCAtkCAQAwga0xKTAnBgkqhkiG9w0BCQEWGnN1cHBvcnRAd2lzZXRlY2hn
bG9iYWwuY29tMQswCQYDVQQGEwJBVTEPMA0GA1UEBwwGU3lkbmV5MRgwFgYDVQQI
DA9OZXcgU291dGggV2FsZXMxGDAWBgNVBAoMD1dpc2VUZWNoIEdsb2JhbDERMA8G
A1UECwwIRURJL0RBVC8xGzAZBgNVBAMMEndpc2V0ZWNoZ2xvYmFsLmNvbTCCAiIw
DQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAJydrqNcFsF4nx0wYDCW8gce8LCO
CDker2zJMDU2txqRwtttKat2mrL9pUrJ+nPdFp7YJWO4LAqc1T9+rK0bp87Ez+nQ
6HArlQhAvBR3qH4nBbUReOHXyd6aVNqp220/ox6W0uCBRojU68NvzzDeY43FmEbv
Y8RTrVpuZNEVeY6geojF6AiArhcwNSHoMnvTCVWCiGpekapD7uFUXO4BELHddhoT
MEGFqRG8JUmvsj+kKY41MAUsH6NctHAqMxY66/CZTPpH/1r3sKUgLlaUEPea22/4
rPbG7GfDRTGvK2du1BEpbO08hf/AE0h4B06zhyqbP7frOi2sSRb5Px3mT7b+Ob3/
nAotROsijEtHEV6pMr6O28uND4hP3wurxZHPAI0i+0dKNNM01ZmnAtijHI3U1Rdd
DUcehO12zWaMaB80sOuTSNdWzcah+0327uY3+fc7CLt2HxLFoLkCLGBO7Ef/vpK+
uS58AT0oYwF9WMpIgj3ROZ6jAU8G7OL4m6JKWRh2e2W29DunL1nhHL9kJNMFJlMA
FnRMFLwBEZU7GSjsKU1drdFPn7GiJ1Q/7jKKSY75XkUYCXJ0gc2ladVVz3B2RFaA
ZGD5ZsS2imiZQSGjNY5DLLMTq0NtHqTPQNQZ+JbBbzhUS43WNCXNtK3kOMcqapfg
AL7KAX1m119rQ+WZAgMBAAEwDQYJKoZIhvcNAQELBQADggIBACgNhD7hz2RsUhFW
a6lZslUeLORB5709V5/qL989VppufpmmmVFigh6+ftw7HFJ32KmeXTaJHQX+PlS9
0Vy63ObDhDbK7H/g8G/n69Lo43+hhnWYcJhPt55LiR2CmXlijvMHwsbWmsOv7QZh
NsYKZW+7kIcE2Ql9e+ObrCKp9gLgIEdrTTtaVFNMWsGIU2Y3rOr5kKcwsJuHg/TA
b6lGpqp5i8wP00e7JPnY28XQs8pvI91VGS701Q6F62mfnLGwo8gknf+Ic5WIbTuv
FA/HJ4foW9gux5FrMTDdzWAWJijcpYmxiMaHHX8d93V+wR3DUJw0G4AUDP0YTrNR
ShMs9LKlQAgA8Qdg+Lhq3wImaQLIlaEcA2FER38GZYaCfC4DH4vRkOWRXdBzjNcv
982E26x5Ek5LH365heh1PfSveVZIBZPKOwLNCLXrGa4gQtG95wsqRHXOWOeh/9Qa
wx99puxtepatNQEHMDA3rbPSsqpoBfzOlyNcvQV147DRQYTPXYp84gI3tHaiYpJ7
G20jq99TMr7fXNfuskn0YG2cL4p5C1ebQJV45Rzy6b0gaJR+2qiUzDzQGFwJgJq5
zlqZUNc3Gy9U2xVqrRCNR1wVaKdDtog0bp3+znEoBhZBKImGUw7vSIwFjpUV+VCe
2edWf4T63EQOzlShlxeaNt1QOMuc
-----END CERTIFICATE REQUEST-----";

		readonly string SampleCertificatePem = @"-----BEGIN CERTIFICATE-----
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

		byte[] SampleCertificateBlob
		{
			get
			{
				return Encoding.UTF8.GetBytes(SampleCertificatePem);
			}
		}

		class ConfigContainerForm : ZForm
		{
			public ConfigContainerForm(EDICommunicationParty party)
				: base(party)
			{
				ConfigControl = new InboundOAuthUserControl();
				Controls.Add(ConfigControl);
				BindingSource.SetBindingMember(ConfigControl, ".");
			}

			public InboundOAuthUserControl ConfigControl { get; }
		}
	}
}
