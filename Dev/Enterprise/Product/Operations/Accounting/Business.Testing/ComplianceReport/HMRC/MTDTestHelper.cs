using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using static Enterprise.Accounting.Business.ComplianceReport.HMRC.FraudPreventionDataProvider;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDTestHelper
	{
		public static MTDSubmissionDataColumns CreateSubmissionData(BusinessObjectFactory factory, AccComplianceReport report)
		{
			var submissionData = new MTDSubmissionDataColumns(factory, report);
			submissionData.ComputedByCW1.Box1_VATDue = 1000M;
			submissionData.ComputedByCW1.Box2_VATDueReverseChg = 500M;
			submissionData.ComputedByCW1.Box4_VATReclaimed = 200M;
			submissionData.ComputedByCW1.Box6_TotalSalesExVAT = 2500M;
			submissionData.ComputedByCW1.Box7_TotalPurchaseExVAT = 3500M;
			submissionData.ComputedByCW1.Box8_GoodsSalesECMembersExVAT = 700M;
			submissionData.ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT = 100M;
			submissionData.Declaration = true;
			submissionData.ReturnDueDate = ZDateTime.Today.Date;
			submissionData.UpdateValuesToSubmitToHMRC();
			return submissionData;
		}

		public static MTDSubmissionDataColumns CreateSubmissionDataWithAllColumns(BusinessObjectFactory factory, AccComplianceReport report)
		{
			var submissionData = new MTDSubmissionDataColumns(factory, report);
			submissionData.ComputedByCW1.Box1_VATDue = 1500m;
			submissionData.ComputedByCW1.Box2_VATDueReverseChg = 100m;
			submissionData.ComputedByCW1.Box4_VATReclaimed = 1000m;
			submissionData.ComputedByCW1.Box6_TotalSalesExVAT = 200m;
			submissionData.ComputedByCW1.Box7_TotalPurchaseExVAT = 200m;
			submissionData.ComputedByCW1.Box8_GoodsSalesECMembersExVAT = 300m;
			submissionData.ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT = 369m;

			submissionData.UnsubmitedPreviousValues.Box1_VATDue = 500m;
			submissionData.UnsubmitedPreviousValues.Box2_VATDueReverseChg = 0m;
			submissionData.UnsubmitedPreviousValues.Box4_VATReclaimed = 0m;
			submissionData.UnsubmitedPreviousValues.Box6_TotalSalesExVAT = 2800m;
			submissionData.UnsubmitedPreviousValues.Box7_TotalPurchaseExVAT = 0m;
			submissionData.UnsubmitedPreviousValues.Box8_GoodsSalesECMembersExVAT = 0m;
			submissionData.UnsubmitedPreviousValues.Box9_GoodsPurchaseECMembersExVAT = 31m;

			submissionData.GroupMemberTotal.Box1_VATDue = 6000m;
			submissionData.GroupMemberTotal.Box2_VATDueReverseChg = 400m;
			submissionData.GroupMemberTotal.Box4_VATReclaimed = 4000m;
			submissionData.GroupMemberTotal.Box6_TotalSalesExVAT = 800m;
			submissionData.GroupMemberTotal.Box7_TotalPurchaseExVAT = 800m;
			submissionData.GroupMemberTotal.Box8_GoodsSalesECMembersExVAT = 1200m;
			submissionData.GroupMemberTotal.Box9_GoodsPurchaseECMembersExVAT = 1000m;

			submissionData.Adjustments.Box1_VATDue = -150m;
			submissionData.Adjustments.Box2_VATDueReverseChg = 40m;
			submissionData.Adjustments.Box4_VATReclaimed = 5m;
			submissionData.Adjustments.Box6_TotalSalesExVAT = 8m;
			submissionData.Adjustments.Box7_TotalPurchaseExVAT = 80m;
			submissionData.Adjustments.Box8_GoodsSalesECMembersExVAT = 10m;
			submissionData.Adjustments.Box9_GoodsPurchaseECMembersExVAT = 10m;

			submissionData.UpdateValuesToSubmitToHMRC();
			return submissionData;
		}

		public static AccComplianceReport SetupReportAndConfiguration(TestObjectCreator objectCreator, BusinessObjectFactory factory, ZDate from, ZDate to)
		{
			var report = factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "MTD";
			report.ACR_DateFrom = from;
			report.ACR_DateTo = to;
			report.ACR_GC_Company = Env.CurrentCompanyPK;
			report.OAuthClientAuthorisation += (o, e) => e.AuthorisationCode = MTDTestHelper.MockAuthorizationCode;
			objectCreator.CreateConfigurationForComplianceReport(report, "AL");
			return report;
		}

		public static MTDVATData GetSampleSubmittedVATData()
		{
			return new MTDVATData()
			{
				finalised = true,
				netVatDue = 15M,
				periodKey = "#001",
				totalAcquisitionsExVAT = 45M,
				totalValueGoodsSuppliedExVAT = 55M,
				totalValuePurchasesExVAT = 75M,
				totalValueSalesExVAT = 70M,
				totalVatDue = 15M,
				vatDueAcquisitions = 5M,
				vatDueSales = 10M,
				vatReclaimedCurrPeriod = 0M
			};
		}

		public static MTDClient SetupClient(AccComplianceReport report, MTDHttpClientHandlerMock clientHandlerMock)
		{
			var client = new MTDClient(report);
			AddOKResponseMessage_PostRefreshTokenRequest(client, clientHandlerMock);
			client.SetFraudPreventionData_ForTestOnly(SetUpFraudPreventionDataProviderForTest());
			return client;
		}

		public void SetupMockHttpClientForMTD()
		{
			MTDClient.HttpClientProvider.TestInstance.SetClient_ForTestOnly(TestHttpClient);
		}

		public HttpClient TestHttpClient
		{
			get
			{
				if (testHttpClient == null)
				{
					testHttpClient = new HttpClient(MTDHttpClientHandler);
					testHttpClient.BaseAddress = new Uri(MockHost);
				}
				return testHttpClient;
			}
		}
		HttpClient testHttpClient;

		public MTDHttpClientHandlerMock MTDHttpClientHandler
		{
			get
			{
				if (mtdHttpClientHandler == null)
				{
					mtdHttpClientHandler = MTDHttpClientHandlerMock.New();
				}
				return mtdHttpClientHandler;
			}
		}
		MTDHttpClientHandlerMock mtdHttpClientHandler;

		static void AddOKResponseMessage_PostRefreshTokenRequest(MTDClient client, MTDHttpClientHandlerMock clientHandlerMock)
		{
			var response = @"{
  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
  ""token_type"": ""bearer"",
  ""expires_in"": 14400,
  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF"",
  ""scope"": ""both""
}";
			clientHandlerMock.AddJsonResponse(FormattableString.Invariant($"{MTDTestHelper.MockHost}/{new MTDPostRefreshTokenRequest(client).Path}"), System.Net.HttpStatusCode.OK, response);
		}

		public const string MockAuthorizationCode = "bcb8cae0a1c046f38ecb863820d441ef";

		public static string MockHost => AccountingConfigurationRegistry.Instance.MTDTestWebServiceUrl.Value;

		#region Fraud Prevention

		public static FraudPreventionDataProvider SetUpFraudPreventionDataProviderForTest()
		{
			var mockedUserThreeLetterAcronym = new Mock<IUser>(MockBehavior.Strict);
			mockedUserThreeLetterAcronym.Setup(m => m.Initials).Returns("TLA");
			mockedUserThreeLetterAcronym.Setup(m => m.IsTwoFactorAuthenticationEnabled).Returns(true);
			mockedUserThreeLetterAcronym.Setup(m => m.EmailAddress).Returns("test.user@company.com");
			mockedUserThreeLetterAcronym.Setup(m => m.PK).Returns(Guid.Parse("F658DA12-CF68-40CD-AC49-4D523721E94E"));
			mockedUserThreeLetterAcronym.Setup(m => m.LastActivityDateTimeUtc).Returns(ZDateTime.UtcNow.AddMinutes(-5));

			return new FraudPreventionDataProviderTestImpl()
				.SetupManagementObjectSearcher("select * from Win32_ComputerSystemProduct", new[]
				{
					new ManagementObjectProxyTestImpl(new Dictionary<string, object>
					{
						{ "UUID", "0A61AF61-CFED-4102-AFBD-A65573EADFAA" }
					})
				})
				.SetupManagementObjectSearcher("select * from Win32_ComputerSystem", new[]
				{
					new ManagementObjectProxyTestImpl(new Dictionary<string, object>
					{
						{ "CurrentTimeZone", (short)TimeSpan.FromHours(10).TotalMinutes },
						{ "Manufacturer", "WiseTechGlobal" },
						{ "Model", "The Ultimate Machine" },
					})
				})
				.SetupManagementObjectSearcher("select * from Win32_OperatingSystem", new[]
				{
					new ManagementObjectProxyTestImpl(new Dictionary<string, object>
					{
						{ "Caption", "Operating System 99" },
						{ "Version", "99.9.99999" },
					})
				})
				.SetupNetworkInterfaces(
					new NetworkInterfaceProxyTestImpl
					{
						OperationalStatus = OperationalStatus.Up,
						NetworkInterfaceType = NetworkInterfaceType.Ethernet,
						PhysicalAddress = new PhysicalAddress(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }),
						IPProperties = new IPInterfacePropertiesProxyTestImpl
						{
							UnicastAddresses = new[]
							{
								new UnicastIPAddressInformationProxyTestImpl
								{
									Address = new IPAddress(new byte[] { 192, 168, 0, 1 }),
									IPv4Mask = new IPAddress(new byte[] { 255, 255, 0, 0 })
								}
							}
						}
					},
					new NetworkInterfaceProxyTestImpl
					{
						OperationalStatus = OperationalStatus.Up,
						NetworkInterfaceType = NetworkInterfaceType.Loopback,
						PhysicalAddress = new PhysicalAddress(new byte[] { 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 }),
						IPProperties = new IPInterfacePropertiesProxyTestImpl
						{
							UnicastAddresses = new[]
							{
								new UnicastIPAddressInformationProxyTestImpl
								{
									Address = new IPAddress(new byte[] { 127, 0, 0, 1 }),
									IPv4Mask = new IPAddress(new byte[] { 255, 255, 255, 0 })
								}
							}
						}
					},
					new NetworkInterfaceProxyTestImpl
					{
						OperationalStatus = OperationalStatus.Up,
						NetworkInterfaceType = NetworkInterfaceType.Ethernet,
						PhysicalAddress = new PhysicalAddress(new byte[] { 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 }),
						IPProperties = new IPInterfacePropertiesProxyTestImpl
						{
							UnicastAddresses = new[]
							{
								new UnicastIPAddressInformationProxyTestImpl
								{
									Address = new IPAddress(new byte[] { 10, 61, 220, 57 }),
									IPv4Mask = new IPAddress(new byte[] { 255, 255, 248, 0 })
								}
							}
						}
					},
					new NetworkInterfaceProxyTestImpl
					{
						OperationalStatus = OperationalStatus.Up,
						NetworkInterfaceType = NetworkInterfaceType.Ethernet,
						PhysicalAddress = new PhysicalAddress(new byte[] { 0xF1, 0xD2, 0xB3, 0x94, 0x75, 0x56, 0x37, 0x18 }),
						IPProperties = new IPInterfacePropertiesProxyTestImpl
						{
							UnicastAddresses = new[]
							{
								new UnicastIPAddressInformationProxyTestImpl
								{
									Address = new IPAddress(new byte[] { 11, 0, 0, 1 }),
									IPv4Mask = new IPAddress(new byte[] { 255, 0, 0, 0 })
								}
							}
											}
										})
				.SetupOpenForm(new FormProxyTestImpl
				{
					ContainsFocus = true,
					Width = 1024,
					Height = 768
				})
				.SetupScreens(
					new PhysicalScreenInfoTestImpl { Depth = 32, WorkingArea = new Rectangle(0, 0, 1920, 1080), DpiX = 168 },
					new PhysicalScreenInfoTestImpl { Depth = 32, WorkingArea = new Rectangle(1920, 0, 3840, 2160), DpiX = 144 })
				.SetupSoftwareData(new SoftwareDataProxyTestImpl
				{
					CurrentUser = mockedUserThreeLetterAcronym.Object,
					ProductName = "WTG Test",
					VersionNumber = new VersionNumber(99, 99, 9999, 9999)
				});
		}

		sealed class FraudPreventionDataProviderTestImpl : FraudPreventionDataProvider
		{
			readonly Dictionary<string, IEnumerable<IManagementObjectProxy>> managementObjectSearcherItems = new Dictionary<string, IEnumerable<IManagementObjectProxy>>();
			List<INetworkInterfaceProxy> networkInterfaceItems = new List<INetworkInterfaceProxy>();
			readonly List<IFormProxy> applicationOpenFormProxies = new List<IFormProxy>();
			ICachedScreenInfoProxy cachedScreenInfoProxy;
			ISoftwareDataProxy softwareDataProxy;

			public FraudPreventionDataProviderTestImpl SetupManagementObjectSearcher(string queryString, IEnumerable<IManagementObjectProxy> managementObjectSearcherItem)
			{
				managementObjectSearcherItems[queryString] = managementObjectSearcherItem;
				return this;
			}

			public FraudPreventionDataProviderTestImpl SetupNetworkInterfaces(params INetworkInterfaceProxy[] networkInterfaceProxies)
			{
				networkInterfaceItems = networkInterfaceProxies.ToList();
				return this;
			}

			public FraudPreventionDataProviderTestImpl SetupScreens(params IPhysicalScreenInfo[] screens)
			{
				cachedScreenInfoProxy = new CachedScreenInfoProxyTestImpl(screens);
				return this;
			}

			public FraudPreventionDataProviderTestImpl SetupOpenForm(IFormProxy formProxyItem)
			{
				applicationOpenFormProxies.Add(formProxyItem);
				return this;
			}

			public FraudPreventionDataProviderTestImpl SetupSoftwareData(ISoftwareDataProxy softwareDataProxy)
			{
				this.softwareDataProxy = softwareDataProxy;
				return this;
			}

			internal override IManagementObjectSearcherProxy GetManagementObjectSearcherProxy(string queryString) => new ManagementObjectSearcherProxyImpl(managementObjectSearcherItems[queryString]);
			internal override INetworkInterfaceProxy[] GetAllNetworkInterfaceProxies() => networkInterfaceItems.ToArray();
			internal override IEnumerable<IFormProxy> ApplicationOpenFormProxies => applicationOpenFormProxies;
			internal override ICachedScreenInfoProxy CachedScreenInfoProxyInstance => cachedScreenInfoProxy;
			internal override ISoftwareDataProxy SoftwareDataProxy => softwareDataProxy;
		}

		sealed class ManagementObjectSearcherProxyImpl : IManagementObjectSearcherProxy
		{
			readonly IManagementObjectProxy[] results;

			public ManagementObjectSearcherProxyImpl(IEnumerable<IManagementObjectProxy> results)
			{
				this.results = results.ToArray();
			}

			public void Dispose()
			{
			}

			public IEnumerable<IManagementObjectProxy> Get() => results;
		}

		sealed class ManagementObjectProxyTestImpl : IManagementObjectProxy
		{
			readonly Dictionary<string, object> properties;

			public ManagementObjectProxyTestImpl(Dictionary<string, object> properties)
			{
				this.properties = properties;
			}

			public object this[string propertyName] => properties[propertyName];
		}

		sealed class NetworkInterfaceProxyTestImpl : INetworkInterfaceProxy
		{
			public OperationalStatus OperationalStatus { get; set; }
			public NetworkInterfaceType NetworkInterfaceType { get; set; }
			public PhysicalAddress PhysicalAddress { get; set; }
			public IIPInterfacePropertiesProxy IPProperties { get; set; }
			public IIPInterfacePropertiesProxy GetIPProperties() => IPProperties;
			public PhysicalAddress GetPhysicalAddress() => PhysicalAddress;
		}

		sealed class IPInterfacePropertiesProxyTestImpl : IIPInterfacePropertiesProxy
		{
			public IEnumerable<IUnicastIPAddressInformationProxy> UnicastAddresses { get; set; }
		}

		sealed class UnicastIPAddressInformationProxyTestImpl : IUnicastIPAddressInformationProxy
		{
			public IPAddress Address { get; set; }
			public IPAddress IPv4Mask { get; set; }
		}

		sealed class CachedScreenInfoProxyTestImpl : ICachedScreenInfoProxy
		{
			public CachedScreenInfoProxyTestImpl(params IPhysicalScreenInfo[] screens)
			{
				Screens = screens.ToArray();
			}

			public IEnumerable<IPhysicalScreenInfo> Screens { get; }
		}

		sealed class PhysicalScreenInfoTestImpl : IPhysicalScreenInfo
		{
			public bool IsPrimary { get; set; }

			public Rectangle WorkingArea { get; set; }

			public Rectangle Bounds { get; set; }

			public int Depth { get; set; }

			public uint DpiX { get; set; }

			public uint DpiY { get; set; }
		}

		sealed class FormProxyTestImpl : IFormProxy
		{
			public bool ContainsFocus { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }

			IEnumerable<IFormProxy> IFormProxy.GetApplicationOpenFormProxies()
			{
				throw new NotImplementedException();
			}
		}

		sealed class SoftwareDataProxyTestImpl : ISoftwareDataProxy
		{
			public IUser CurrentUser { get; set; }
			public string ProductName { get; set; }
			public VersionNumber VersionNumber { get; set; }
		}

		#endregion
	}
}
