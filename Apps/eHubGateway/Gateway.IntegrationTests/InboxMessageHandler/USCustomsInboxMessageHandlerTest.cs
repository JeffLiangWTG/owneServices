using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;
using System;
using System.IO;
using System.ServiceModel;
using System.Text;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
    [TestFixture]
	public class USCustomsInboxMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestGatewayUnderMaintenance_NotFoundUSCustomseHubOutboxServiceTest()
		{
			using (new DisposableAction(() => { DropUSCustomseHubOutboxServiceTest(); }, () => { CreateUSCustomseHubOutboxServiceTest(); }))
			{
				var adapter = CreateAdapter(TestClientID, TestClientPassword);

				var sampleMessage = @"<USCustoms>
	<Header>
		<![CDATA[A    050562792      EXP20160114L90   N050562792]]>
	</Header>
	<Body>
		<![CDATA[B  13294217800E          GLENCORE LTD                                           SC1Y10PATXUNKNMB016011117075000RMV SONGA PEARL         2 22575530120160110 Y    SC270                                   N                                       SC3                             GLE160008                                       N0113294217800EEGLENCORE LTD                  ANGELA       NELSON               N02C/O KINDER MORGAN - BOSTCO      1836 MILLER CUT OFF RD          2033284980   N03LA PORTE                 TXUS77571                                           N0105056279200EFCHARTER BROKERAGE LLC         BRIAN        LONDON               N02125 PARK AVENUE                 SUITE 1800                      2123639300   N03NEW YORK                 NYUS10017                                           N0113294217800ECGLENCORE LTD                                                   NN02THREE STAMFORD PLAZA            301 TRESSER BLVD                             N03STAMFORD                 CTUS069013255          R                            CL1OS 0001HS-RMG380                                    0000000000 AC33D         CL22710190630BBL00000680220001425050   00000000000010694191EAR99NLR             Y  13294217800E          GLENCORE LTD]]>
	</Body>
	<Footer>
		<![CDATA[Z    050562792      EXP20160114L90    050562792]]>
	</Footer>
</USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USExport, "USCustoms Export", stream);
					adapter.Outbox.AddMessage(message);

					try
					{
						adapter.SendMessages();
						Assert.Fail("RegistrationException is expected since eHubAdapter wraps FaultExceptions with a message containing \"not a valid ediEnterprise licence code.\" into RegistrationException.");
					}
					catch (RegistrationException e)
					{
						StringAssert.Contains("eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code. ExceptionID: ", e.Message);
						Assert.IsInstanceOf(typeof(FaultException<ExceptionDetail>), e.InnerException);
						Assert.IsTrue(e.InnerException.ToString().Contains("Could not find any service with the name '//cargowise.com/eServices/USCustoms/eHubOutboxServiceTest' in this database."));
					}
				}
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_eManifest_MessageEnqueued()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			var sampleMessage = @"<USCustoms><Header><![CDATA[UNB+UNOA:4+MAN:ZZ+USC:ZZ+20151221:1649+733++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20151221:1649+733+UN+D:03B']]></Header><Body><![CDATA[UNH+733+CUSCAR:D:03B:UN'BGM+85:::STANDARD+ABCWMAN0000223+22'DTM+132:201411080039:203'LOC+60+0901:77'RFF+ABO:MAN733'NAD+CA+ABCW:172'TDT+11++03+:::BT++I++:146::1M8GDM9AXKP043446'TDT+11++03+:::BT++I++:274::45678912'TDT+11++03+:::BT++I++:215::EQU456:US'LOC+89+AL:163'CNI+1+:23'RFF+AAM:123321456'LOC+9+12200:78'GEI+7+135'NAD+OS+++THIS IS COMPANY NAME+IAN TEST ADDRESS1 IAN TEST ADDRESS2+LONDON+AK:163+123432A+US'CTA+IC'COM+IAN.CHEN WISETECHGLOBAL.COM:EM'NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US'GID+1'PAC+1++BOX'FTX+AAA+++SOMETHING FOR TEST'MEA+AAI++K:222.0000'SGP+EQU456:215'UNT+24+733']]></Body><Footer><![CDATA[UNE+1+733'UNZ+1+733']]></Footer></USCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USeManifest, "MAN", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				AssertInboxMessage(message.TrackingID, TestClientPK, ApplicationCode.USeManifest, TestUSCustomsPK, "MAN", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
				AssertCountOfeHubClientDialogue(1);
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_TestClient_UseTestService()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			var sampleMessage = @"<USCustoms><Header><![CDATA[UNB+UNOA:4+MAN:ZZ+USC:ZZ+20151221:1649+733++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20151221:1649+733+UN+D:03B']]></Header><Body><![CDATA[UNH+733+CUSCAR:D:03B:UN'BGM+85:::STANDARD+ABCWMAN0000223+22'DTM+132:201411080039:203'LOC+60+0901:77'RFF+ABO:MAN733'NAD+CA+ABCW:172'TDT+11++03+:::BT++I++:146::1M8GDM9AXKP043446'TDT+11++03+:::BT++I++:274::45678912'TDT+11++03+:::BT++I++:215::EQU456:US'LOC+89+AL:163'CNI+1+:23'RFF+AAM:123321456'LOC+9+12200:78'GEI+7+135'NAD+OS+++THIS IS COMPANY NAME+IAN TEST ADDRESS1 IAN TEST ADDRESS2+LONDON+AK:163+123432A+US'CTA+IC'COM+IAN.CHEN WISETECHGLOBAL.COM:EM'NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US'GID+1'PAC+1++BOX'FTX+AAA+++SOMETHING FOR TEST'MEA+AAI++K:222.0000'SGP+EQU456:215'UNT+24+733']]></Body><Footer><![CDATA[UNE+1+733'UNZ+1+733']]></Footer></USCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USeManifest, "MAN", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				AssertFromServiceName("//cargowise.com/eServices/USCustoms/eHubOutboxServiceTest");
				AssertToServiceName("//cargowise.com/eServices/USCustoms/OutboundMessageProcessingServiceTest");
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ProdClient_UseProdService()
		{
			var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
			var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ProdClient_UseProdService]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "SF", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				AssertFromServiceName("//cargowise.com/eServices/USCustoms/eHubOutboxService");
				AssertToServiceName("//cargowise.com/eServices/USCustoms/OutboundMessageProcessingService");
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_FTZ_ABIFormat_BeforeCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(FTZChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{FTZChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(FTZChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9FT                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 FTZ_ABIFormat_BeforeCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "FT", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "FT", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI12211601                                                          B013910SV9FT                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 FTZ_ABIFormat_BeforeCutOverDate                               Z3910SV9CAREDI12211601");
				}
			}
			finally
			{
				SetRemoteSettings(FTZChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_FTZ_ABIFormat_AfterCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(FTZChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{FTZChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(FTZChangeCutOverDate, DateTime.UtcNow.AddDays(-1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9FZ                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 FTZ_ABIFormat_AfterCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "FZ", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "FZ", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     FZ                                                     B  3910SV9FZ                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF      FTZ_ABIFormat_AfterCutOver  te                                Z3910SV9      122116                                                            ");
				}
			}
			finally
			{
				SetRemoteSettings(FTZChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_FTZ_ACEFormat_BeforeCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);

				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      122116     FT]]></Header><Body><![CDATA[B  3910SV9FT                                               HYEDUSCMT_187387     SF10101ACTEI 20-086695300           11               MAEU20-086695300   018     SF40490110    GB                                                                Y  3910SV9SF FTZ_ACEFormat_BeforeCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      122116]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "FT", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "FT", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     FT                                                     B  3910SV9FT                                               HYEDUSCMT_187387     SF10101ACTEI 20-086695300           11               MAEU20-086695300   018     SF40490110    GB                                                                Y  3910SV9SF FTZ_ACEFormat_BeforeCutOverDate                                    Z3910SV9      122116                                                            ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ISF_ABIFormat_BeforeCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ISF_ABIFormat_BeforeCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "SF", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "SF", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI12211601                                                          B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ISF_ABIFormat_BeforeCutOverDate                               Z3910SV9CAREDI12211601                                                          ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ISF_ABIFormat_BeforeCutOverDate_ClientRegisteredAsABI()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ISF_ABIFormat_BeforeCutOverDate_ClientRegisteredAsABI]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "SF", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "SF", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI12211601                                                          B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ISF_ABIFormat_BeforeCutOverDate_ClientRegisteredAsABI         Z3910SV9CAREDI12211601                                                         ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ISF_ABIFormat_AfterCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(-1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(TestClientID, TestClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ISF_ABIFormat_AfterCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "SF", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, TestClientPK, ApplicationCode.USImport, TestUSCustomsPK, "SF", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     SF                                                     B  3910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF      ISF_ABIFormat_AfterCutOver  te                                Z3910SV9      122116                                                           ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ISF_ABIFormat_AfterCutOverDate_ClientRegisteredAsABI()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(-1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 ISF_ABIFormat_AfterCutOverDate_ClientRegisteredAsABI]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "SF", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "SF", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     SF                                                     B  3910SV9SF                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF      ISF_ABIFormat_AfterCutOver  te_ClientRegisteredAsABI          Z3910SV9      122116                                                           ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ABI_AfterCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(-1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B01    XXXNP                                               265                  EBINVALID DIST/PORT/BROKER/OFFICE                                               EBTRANSACTION DATA REJECTED                                                     Y         NP00001                                  ABI_AfterCutOverDate ]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "NP", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "NP", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI12211601                                                          B01    XXXNP                                               265                  EBINVALID DIST/PORT/BROKER/OFFICE                                               EBTRANSACTION DATA REJECTED                                                     Y         NP00001                                  ABI_AfterCutOverDate         Z3910SV9CAREDI12211601                                                          ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_ISF_ACEFormat_BeforeCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(ISFChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{ISFChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(ISFChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(TestClientID, TestClientPassword);

				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      122116     SF]]></Header><Body><![CDATA[B  3910SV9SF                                               HYEDUSCMT_187387     SF10101ACTEI 20-086695300           11               MAEU20-086695300   018     SF40490110    GB                                                                Y  3910SV9SF ISF_ACEFormat_BeforeCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      122116]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "SF", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, TestClientPK, ApplicationCode.USImport, TestUSCustomsPK, "SF", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     SF                                                     B  3910SV9SF                                               HYEDUSCMT_187387     SF10101ACTEI 20-086695300           11               MAEU20-086695300   018     SF40490110    GB                                                                Y  3910SV9SF ISF_ACEFormat_BeforeCutOverDate                                    Z3910SV9      122116                                                            ");
				}
			}
			finally
			{
				SetRemoteSettings(ISFChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsThrowsExcptionWhenServiceBrokerIsNotRegistered()
		{
			var adapter = CreateAdapter("SAMCLIENT", TestClientPassword);

			var sampleMessage = @"<USCustoms><Header><![CDATA[UNB+UNOA:4+MAN:ZZ+USC:ZZ+20151221:1649+733++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20151221:1649+733+UN+D:03B']]></Header><Body><![CDATA[UNH+733+CUSCAR:D:03B:UN'BGM+85:::STANDARD+ABCWMAN0000223+22'DTM+132:201411080039:203'LOC+60+0901:77'RFF+ABO:MAN733'NAD+CA+ABCW:172'TDT+11++03+:::BT++I++:146::1M8GDM9AXKP043446'TDT+11++03+:::BT++I++:274::45678912'TDT+11++03+:::BT++I++:215::EQU456:US'LOC+89+AL:163'CNI+1+:23'RFF+AAM:123321456'LOC+9+12200:78'GEI+7+135'NAD+OS+++THIS IS COMPANY NAME+IAN TEST ADDRESS1 IAN TEST ADDRESS2+LONDON+AK:163+123432A+US'CTA+IC'COM+IAN.CHEN WISETECHGLOBAL.COM:EM'NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US'GID+1'PAC+1++BOX'FTX+AAA+++SOMETHING FOR TEST'MEA+AAI++K:222.0000'SGP+EQU456:215'UNT+24+733']]></Body><Footer><![CDATA[UNE+1+733'UNZ+1+733']]></Footer></USCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), "SAMCLIENT", TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USeManifest, "MAN", stream);
				adapter.Outbox.AddMessage(message);

				try
				{
					adapter.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.True(e.Message.Contains("Interchange rejected by eHub because you are not registered with WTG for US Customs messaging. Please register."));
				}
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_MID_ABIFormat_BeforeCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(MIDChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{MIDChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(MIDChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9$I                                             HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 MID_ABIFormat_BeforeCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "$I", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "$I", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI12211601                                                          B013910SV9$I                                             HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 MID_ABIFormat_BeforeCutOverDate                                 Z3910SV9CAREDI12211601                                                          ");
				 }
			}
			finally
			{
				SetRemoteSettings(MIDChangeCutOverDate, originalCutOverDate.ToString());
			}

		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_MID_ABIFormat_AfterCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(MIDChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{MIDChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(MIDChangeCutOverDate, DateTime.UtcNow.AddDays(-1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);
				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      12211601]]></Header><Body><![CDATA[B013910SV9$I                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF00024 MID_ABIFormat_AfterCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      12211601]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "$I", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "$I", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     $I                                                     B  3910SV9$I                                               HYEDUSCMT_187386     SF36ABERDEEN                           GMP      AB11 6NE       GB               SF40490110    GB                                                                Y  3910SV9SF      MID_ABIFormat_AfterCutOver  te                                Z3910SV9      122116                                                            ");
				}
			}
			finally
			{
				SetRemoteSettings(MIDChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_MID_ACEFormat_BeforeCutOverDate()
		{
			var originalCutOverDate = GetRemoteSettings(MIDChangeCutOverDate);
			Assert.NotNull(originalCutOverDate, $"{MIDChangeCutOverDate} does not exist in {WithGatewayServiceAttribute.Current.GetHttpsUri("web.config")}");
			SetRemoteSettings(MIDChangeCutOverDate, DateTime.UtcNow.AddDays(1).ToString("u"));

			try
			{
				var adapter = CreateAdapter(ABIClientID, TestAuthenticatedClientPassword);

				var sampleMessage = @"<USCustoms><Header><![CDATA[A3910SV9      122116     $I]]></Header><Body><![CDATA[B  3910SV9$I                                               HYEDUSCMT_187387     SF10101ACTEI 20-086695300           11               MAEU20-086695300   018     SF40490110    GB                                                                Y  3910SV9SF MID_ACEFormat_BeforeCutOverDate]]></Body><Footer><![CDATA[Z3910SV9      122116]]></Footer></USCustoms>";

				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
				{
					var message = new eHubMessage(Guid.NewGuid(), ABIClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USImport, "$I", stream);
					adapter.Outbox.AddMessage(message);
					adapter.SendMessages();

					AssertInboxMessage(message.TrackingID, ABIClientPK, ApplicationCode.USImport, TestUSCustomsPK, "$I", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
					AssertCountOfeHubClientDialogue(1);
					AssertLastQueueMessageContains("A3910SV9CAREDI122116     $I                                                     B  3910SV9$I                                               HYEDUSCMT_187387     SF10101ACTEI 20-086695300           11               MAEU20-086695300   018     SF40490110    GB                                                                Y  3910SV9SF MID_ACEFormat_BeforeCutOverDate                                    Z3910SV9      122116                                                            ");
				}
			}
			finally
			{
				SetRemoteSettings(MIDChangeCutOverDate, originalCutOverDate.ToString());
			}
		}

		[Test]
		public void TestUSCustomsInboxMessageHandler_AMA()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var sampleMessage = @"<USCustoms><Header><![CDATA[WASUCCR
.WTGTTST]]></Header><Body><![CDATA[FRX
ALV123
123-12312311-M
ARR/?QF20/28AUG
RFA/02AMA]]></Body><Footer><![CDATA[]]></Footer></USCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.AMA, "", stream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				AssertInboxMessage(message.TrackingID, TestClientPK, ApplicationCode.AMA, TestUSCustomsPK, "", "", "", stream.CompressAndEncode().ReadToEnd(), 1, 0);
				AssertCountOfeHubClientDialogue(1);
				AssertLastQueueMessageContains(@"<?xml version=""1.0"" encoding=""utf-8""?><Message ClientId=""TSTCLIENT"" IsProduction=""False"" MessageTrackingId=""");
				AssertLastQueueMessageContains(@""" ApplicationCode=""AMA"" MessageType=""""><![CDATA[
\x01WASUCCR
.WTGTTST 
\x02FRX
ALV123
123-12312311-M
ARR/?QF20/28AUG
RFA/02AMA
\x03]]>");
			}
		}

		[Test]
		public void TestUSCustomsThrowsApplicationExceptionOnInvalidMessage()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);

			var sampleMessage = @"<USCustoms><Header><![CDATA[UNB+UNOA:X+MAN:ZZ+USC:ZZ+20151221:1649+733++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20151221:1649+733+UN+D:03B']]></Header><Body><![CDATA[UNH+733+CUSCAR:D:03B:UN'BGM+85:::STANDARD+ABCWMAN0000223+22'DTM+132:201411080039:203'LOC+60+0901:77'RFF+ABO:MAN733'NAD+CA+ABCW:172'TDT+11++03+:::BT++I++:146::1M8GDM9AXKP043446'TDT+11++03+:::BT++I++:274::45678912'TDT+11++03+:::BT++I++:215::EQU456:US'LOC+89+AL:163'CNI+1+:23'RFF+AAM:123321456'LOC+9+12200:78'GEI+7+135'NAD+OS+++THIS IS COMPANY NAME+IAN TEST ADDRESS1 IAN TEST ADDRESS2+LONDON+AK:163+123432A+US'CTA+IC'COM+IAN.CHEN WISETECHGLOBAL.COM:EM'NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US'GID+1'PAC+1++BOX'FTX+AAA+++SOMETHING FOR TEST'MEA+AAI++K:222.0000'SGP+EQU456:215'UNT+24+733']]></Body><Footer><![CDATA[UNE+1+733'UNZ+1+733']]></Footer></USCustoms>";

			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(sampleMessage)))
			{
				var messageTrackingID = Guid.NewGuid();
				var message = new eHubMessage(messageTrackingID, TestClientID, TestUSCustomsID, MessageSchemaType.FlatFile, ApplicationCode.USeManifest, "MAN", stream);
				adapter.Outbox.AddMessage(message);

				try
				{
					adapter.SendMessages();
				}
				catch (eHubAdapterException ex)
				{
					var exMessage = ex.Message;
					var messageExceptionDictionary = ex.GetMessageExceptionDictionary();
					var expectedExMessage = @"1 errors occured during processing send request:
Message header UNB+UNOA:X+MAN:ZZ+USC:ZZ+20151221:1649+733++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20151221:1649+733+UN+D:03B' has invalid UNB segment. Failed matching pattern: (?<=UNB\+UNOA:4\+)[a-zA-Z0-9]{1,35}:ZZ\+[a-zA-Z0-9]{1,35}:ZZ
";
					Assert.That(exMessage.Contains(expectedExMessage));
					Assert.AreEqual(1, messageExceptionDictionary.Count);
					var expectedExMessageForTheInvalidMessage = @"Message header UNB+UNOA:X+MAN:ZZ+USC:ZZ+20151221:1649+733++ACE'UNG+CUSCAR+MAN:ZZ+USC:ZZ+20151221:1649+733+UN+D:03B' has invalid UNB segment. Failed matching pattern: (?<=UNB\+UNOA:4\+)[a-zA-Z0-9]{1,35}:ZZ\+[a-zA-Z0-9]{1,35}:ZZ
";
					Assert.AreEqual(expectedExMessageForTheInvalidMessage, messageExceptionDictionary[messageTrackingID]);
				}
			}
		}

		public override void TearDownCore()
		{
			DeleteeHubClientDialogue();
		}

		const string TestUSCustomsID = "USC";
		private static readonly Guid TestUSCustomsPK = new Guid("373C2973-E372-46D2-B5ED-53245A859DC9");
		const string ISFChangeCutOverDate = "ISFChangeCutOverDate";
		const string FTZChangeCutOverDate = "FTZChangeCutOverDate";
		const string MIDChangeCutOverDate = "MIDChangeCutOverDate";
		const string ABIClientID = "ENTTSTSVR";
		const string ABIClientPassword = "TSTPASSWORD";
		static readonly Guid ABIClientPK = new Guid("A0D7B0BE-BC1C-4935-B987-378831EEB0A3");
	}
}
