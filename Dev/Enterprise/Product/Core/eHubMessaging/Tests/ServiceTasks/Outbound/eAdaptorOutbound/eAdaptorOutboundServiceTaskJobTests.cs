using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.eHubMessaging.Tests.ServiceTasks;
using Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests
{
	[TestedType(typeof(eAdaptorOutboundServiceTaskJob))]
	class eAdaptorOutboundServiceTaskJobTests : OutboundEDIInterchangesServiceTaskJobTests<eAdaptorOutboundServiceTaskJob>
	{
		public void TestAdapterOutboxLimits()
		{
			var rule = new OutboundSendLimitsRule
			{
				SendCountLimit = 9,
				SendSizeLimit = 999999
			};
			eAdaptorRegistry.Instance.eAdaptorOutboundSendLimitsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertEquals(9, serviceTaskJob.Object.AdapterOutboxCountLimit);
			AssertEquals(999999 * 1024, serviceTaskJob.Object.AdapterOutboxSizeLimitInBytes);
		}

		[TestDate]
		public void TestGetCandidatePK()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			AssertEquals("Precondition: EDIMessage table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals("Precondition: EDIInterchange table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			const string recipient = "BLAHBLAHB";
			Assert("Precondition: Current company should have at least 2 branches", CurrentCompany.Branches.Count >= 2);
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			CreateTestInterchanges(recipient, interchangeNumberStrategy, messageNumberStrategy, EDIInterchange.Status.eHubQueued, EDIInterchange.Status.eHubPending, EDIInterchange.Status.Failed);
			var eAdaptorInterchanges = CreateTestInterchanges(recipient, interchangeNumberStrategy, messageNumberStrategy, EDIInterchange.Status.eAdaptorQueued, EDIInterchange.Status.Sent, EDIInterchange.Status.Failed);
			AssertEquals("Precondition: 20 Interchange should be generated for testing", 20, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Object.CurrentRecipient = recipient;

			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.Branches[0].PK;
			var interchangeCandidates = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);
			AssertContainsExactElementsInAnyOrder(new[] { eAdaptorInterchanges[0].PK, eAdaptorInterchanges[4].PK }, interchangeCandidates.Select(c => new ZGuid(c.PK)));

			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.Branches[1].PK;
			interchangeCandidates = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);
			AssertContainsExactElementsInAnyOrder(new[] { eAdaptorInterchanges[2].PK, eAdaptorInterchanges[6].PK }, interchangeCandidates.Select(c => new ZGuid(c.PK)));
		}

		public void TestSendMessages_MessageSpecifiedException()
		{
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < 15; i++)
			{
				interchangeList.Add(CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy));
			}
			Factory.Save();

			var exceptionData = new CargoWise.eHub.Common.SerializableDictionary<Guid, string>
			{
				{ interchangeList[0].PK.ToGuid(), "Interchange failed to send." }
			};
			var ex = new eHubAdapterException("1 Interchange failed", exceptionData);
			var adapter = new Mock<EHubAdapterMock>() { CallBase = true };
			adapter.SetupSequence(m => m.SendMessages()).Throws(ex);

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(adapter.Object));
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("Error: 9 interchange(s) sent to {0}. 1 interchange(s) failed to send to {0}.\r\nInternal Exception: CargoWise.eHub.Adapter.eHubAdapterException: 1 Interchange failed", ServiceTaskName));
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("5 interchange(s) sent to {0}.", ServiceTaskName));

			var reloadFactory = new BusinessObjectFactory();
			var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
			AssertEquals(1, reloadedInterchanges.Count(x => x.EI_Status == "FAL" && x.EI_RetryCount == 0 && x.PK == interchangeList[0].PK));
			AssertEquals(14, reloadedInterchanges.Count(x => x.EI_Status == "SNT" && x.EI_RetryCount == 0));
		}

		public void TestCreateeHubMessageWithXMLDeclaration()
		{
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();

			var interchange1 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange1.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange1.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange1.EI_BodyText = "Test ABL Message";
			interchange1.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;

			var interchange2 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange2.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange2.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange2.EI_BodyText = "Test CON Message";
			interchange2.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;

			var interchange3 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange3.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange3.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange3.EI_BodyText = "Test CMV Message";
			interchange3.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.ContainerMovements;

			var interchange4 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange4.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange4.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange4.EI_BodyText = "Test EVT Message";
			interchange4.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Events;

			var interchange5 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange5.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange5.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange5.EI_BodyText = "Test FTR Message";
			interchange5.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.FinancialTransactions;

			var interchange6 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange6.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange6.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange6.EI_BodyText = "Test ORD Message";
			interchange6.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Orders;

			var interchange7 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange7.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange7.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange7.EI_BodyText = "Test PRD Message";
			interchange7.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Products;

			var interchange8 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange8.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange8.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange8.EI_BodyText = "Test SHP Message";
			interchange8.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Shipments;

			var interchange9 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange9.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange9.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange9.EI_BodyText = "Test WHD Message";
			interchange9.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.WhsDockets;

			var interchange10 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange10.EI_ApplicationCode = ApplicationCodeList.Codes.CIM;
			interchange10.EI_BodyText = "Test FHL Message";
			interchange10.ContainedMessages[0].EM_MessageType = EDIMessageTypeList.Codes.FHL;

			var interchange11 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange11.EI_ApplicationCode = ApplicationCodeList.Codes.CIM;
			interchange11.EI_BodyText = "Test FWB Message";
			interchange11.ContainedMessages[0].EM_MessageType = EDIMessageTypeList.Codes.FWB;

			var interchange12 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange12.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			interchange12.EI_HeaderText = "Header";
			interchange12.EI_BodyText = "Test USI Message";
			interchange12.EI_FooterText = "Footer";
			interchange12.ContainedMessages[0].EM_MessageType = "AA";

			var interchange13 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange13.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsExport;
			interchange13.EI_HeaderText = "Header";
			interchange13.EI_BodyText = "Test USE Message";
			interchange13.EI_FooterText = "Footer";
			interchange13.ContainedMessages[0].EM_MessageType = "BB";

			var interchange14 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange14.EI_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			interchange14.EI_HeaderText = "Header";
			interchange14.EI_BodyText = "Test AMS Message";
			interchange14.EI_FooterText = "Footer";
			interchange14.ContainedMessages[0].EM_MessageType = "CC";

			var interchange15 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange15.EI_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			interchange15.EI_HeaderText = "<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange15.EI_BodyText = "Test NDM Message";
			interchange15.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;

			var interchange16 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange16.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange16.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange16.EI_BodyText = "<body>Test ABL Message</body>";
			interchange16.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			var interchange17 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange17.EI_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			interchange17.EI_BodyText = "<InputDocument xmlns='http://www.customsware.com/schema/api'><Credentials><UserID>Blah</UserID></Credentials></InputDocument>";
			interchange17.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.CustomsWare;

			var interchange18 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange18.EI_ApplicationCode = ApplicationCodeList.Codes.NZCustoms;
			interchange18.EI_BodyText = "UNH+258+CUSDEC:D:96B:UN'BGM+929+B00159089+9'CST++10:105:143'LOC+9+AUSYD'LOC+11+NZAKL'LOC+41+NZAKL'DTM+151:20121130:102'MEA+WT+AAD+KGM:200'RFF+MB:08111111111'RFF+HWB:EAUDECOLOGNE1'PAC+23++PK'TDT+20++4+++++:::QF253'NAD+AL+00782903F:ZZZ:143'NAD+CB+00009908C:ZZZ:143'UNS+D'DMS+1001001+935'TOD+++FOB:106:143'CST+1+5601210000G:169:143'FTX+AAA+++WADDING ETC OF COTTON'LOC+27+AU'LOC+35+AU'NAD+SU+00710841Y:ZZZ:143'MOA+14:1200.00:NZD'CUX+2++1.00'MOA+40:1200'MOA+64:150'MOA+70:12'GIS+Y:109:143'TAX+1+GST'MOA+161:204.30'GIS+Q:116:143'UNS+S'CNT+4:1'CNT+5:1'CNT+11:23'TAX+3+CUD++1200'MOA+161:0.00'TAX+3+GST'MOA+161:204.30'TAX+4+TOT'MOA+161:204.30'GIS+C:134:143'AUT+BHJEDCFKIB@DGAE@+65432198B'UNT+44+258'";
			interchange18.EI_HeaderText = "UNA:+.? 'UNB+UNOA:2+00009908C:ZZZ+CUSSWT:ZZZ+121130:1314+229'";
			interchange18.EI_FooterText = "UNZ+1+229'";
			interchange18.ContainedMessages[0].EM_ApplicationCode = ApplicationCodeList.Codes.NZCustoms;

			var interchange19 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange19.EI_ApplicationCode = ApplicationCodeList.Codes.NZMAFeBACCa;
			interchange19.EI_BodyText = "Message has content.";
			interchange19.ContainedMessages[0].EM_ApplicationCode = ApplicationCodeList.Codes.NZMAFeBACCa;
			interchange19.ContainedMessages[0].EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-16""?>
<MessagingResponse xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.maf.govt.nz/Messaging/Response/2008/03"">
	<CallerRefID>31</CallerRefID>
	<ReceiptNumber>PTPNE865PR</ReceiptNumber>
	<MetaData>
		<ebacca:EBACCAResponse xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Response/2008/03/"">
			<ebacca:OrganisationCode>00009908C</ebacca:OrganisationCode>
		</ebacca:EBACCAResponse>
	</MetaData>
</MessagingResponse>";

			var interchange20 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange20.EI_ApplicationCode = ApplicationCodeList.Codes.AUCMR;
			interchange20.EI_BodyText = @"Mime-Version: 1.0
Content-Transfer-Encoding: base64
Content-Type: application/pkcs7-mime; smime-type=signed-data; name = ""smime.p7m""

MIIJdAYJKoZIhvcNAQcCoIIJZTCCCWECAQExDjAMBggqhkiG9w0CBQUAMIIDPQYJKoZIhvcNAQcB
oIIDLgSCAypNaW1lLVZlcnNpb246IDEuMA0KQ29udGVudC1UcmFuc2Zlci1FbmNvZGluZzogYmFz
ZTY0DQpDb250ZW50LVR5cGU6IGFwcGxpY2F0aW9uL2VkaWZhY3Q7IG5hbWU9QUFMMzY0UF8zLmVk
aQ0KQ29udGVudC1EaXNwb3NpdGlvbjogYXR0YWNobWVudDsgZmlsZW5hbWU9QUFMMzY0UF8zLmVk
aQ0KDQpWVTVCT2lzdVB5QW5WVTVDSzFWT1QwTTZNeXRCUVV3ek5qUlFPanBCUVV3ek5qUlFLMEZC
UVRNek5rTXJNVE13TWpJd09qRXhNVGtyDQpNeXNyS3lzeEt5c3hKMVZPU0NzeEswTlZVME5CVWpw
RU9qazVRanBWVGlkQ1IwMHJPVE16T2pvNlFVbFNRMUlyUVRZd01EQXhNREUwDQpMME5OTWpFNk1T
czVKMUpHUml0UVVUcFFUeWRTUmtZclNGZENPa2d3TURBeFF5ZFNSa1lyVFZkQ09qQTRNVFEwTkRR
d01EQXhKMDVCDQpSQ3REVGlzclRrRk5SVG82SUNBZ0lDQkJWU2RPUVVRclExb3JLMDVCVFVVNk9p
QWdJQ0FnUjBJblRrRkVLMVpYS3pReE1EWTFPRGswDQpOekkwT2pvNU5TZFVSRlFyTWpBck16SXJL
ellyVVVZNk9qTW5URTlES3pnclFWVlRXVVE2T2pZblRFOURLemMySzBkQ1RFaFNPam8yDQpKMHhQ
UXlzeE1pdEJWVk5aUkRvNk5pZE1UME1yT1RFclIwSk1TRkk2T2pZblJGUk5LekUzT0RveU1ERXpN
REl5TVRveE1ESW5RMDVKDQpLekVuVWtaR0sxVkRUanBCTmpBd01ERXdNVFFuVFU5Qkt6UTBPak13
TUM0d01EcEJWVVFuUjBsVEsxTkJRem94TURrNk9UVW5SMGxFDQpLekVuVUVGREt6TW5SbFJZSzBG
QlFTc3JLMWhZV0ZoWVdGaFlXRmhZV0NkTlJVRXJRVUZGSzBjclMwYzZNeTR3TUNkVlRsUXJNak1y
DQpNU2RWVGxvck1Tc3pKdz09DQqgggS3MIIEszCCBBygAwIBAgIQdx9ck3b+9UD/EcwVKlGbSTAN
BgkqhkiG9w0BAQQFADCBjzEbMBkGA1UEChMSVmVyaVNpZ24gQXVzdHJhbGlhMRcwFQYDVQQLEw5H
YXRla2VlcGVyIFBLSTE4MDYGA1UECxMvVGVybXMgb2YgdXNlIGF0IGh0dHBzOi8vd3d3LmVzaWdu
LmNvbS5hdS9HS1JQQS8xHTAbBgNVBAMTFEdhdGVrZWVwZXIgVFlQRSAzIENBMB4XDTExMDgxMTAw
MDAwMFoXDTEzMDgxMTIzNTk1OVowZzELMAkGA1UEBhMCQVUxDDAKBgNVBAgTA05TVzEeMBwGA1UE
ChQVQ0FSR09XSVNFIEVESSBQVFkgTFREMRIwEAYDVQQLFAlDYXJnb1dpc2UxFjAUBgNVBAMTDWVk
aUVudGVycHJpc2UwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAJeRV75qMVs+dOSiDaOc0qOe
TrGn0dzJRxo5RZsh2A+Qfgshv91gQz/e8mdVkrICQ18sRVpA7VgUCTncS1Wdwzrf+IIzhq9+GQoK
au0F/EpBsWGBSkv8TgiEeMIcONSPmmnGx2Zoi67Hb+5BhzpGTEHbxIZnOmZ47iOXD92tQl9tAgMB
AAGjggI1MIICMTAMBgNVHRMBAf8EAjAAMIIBBgYDVR0fBIH+MIH7MIH4oIH1oIHyhj1odHRwOi8v
b25zaXRlY3JsLmVzaWduLmNvbS5hdS9HYXRla2VlcGVyVHlwZTNDQS9MYXRlc3RDUkwuY3JshoGw
bGRhcDovL2RpcmVjdG9yeS5lc2lnbi5jb20uYXUvY249R2F0ZWtlZXBlciBUWVBFIDMgQ0Esb3U9
VGVybXMgb2YgdXNlIGF0IGh0dHBzOi8vd3d3LmVzaWduLmNvbS5hdS9HS1JQQS8sb3U9R2F0ZWtl
ZXBlciBQS0ksbz1WZXJpU2lnbiBBdXN0cmFsaWE/Y2VydGlmaWNhdGVyZXZvY2F0aW9ubGlzdDti
aW5hcnkwHwYDVR0jBBgwFoAUtdsZxo7xZOfcgKfpIo4eJgQltDQwHQYDVR0OBBYEFGCCUYUk7YTk
WiBDm9fLoeGFUncDMDUGCCsGAQUFBwEBBCkwJzAlBggrBgEFBQcwAYYZaHR0cHM6Ly9vY3NwLmVz
aWduLmNvbS5hdTAOBgNVHQ8BAf8EBAMCBPAwHAYDVR0RBBUwE4ERY21yQGNhcmdvd2lzZS5jb20w
FwYGKiQBgk0BBA0WCzQxMDY1ODk0NzI0MEYGA1UdIAQ/MD0wOwYKKiSp/LRjgk0CCDAtMCsGCCsG
AQUFBwIBFh9odHRwczovL3d3dy5lc2lnbi5jb20uYXUvR0tSUEEvMBEGCWCGSAGG+EIBAQQEAwIH
gDANBgkqhkiG9w0BAQQFAAOBgQBMw2KbKttq62RlxiOyh8DLBqvAo0o4EwEnJ3JB5VuUopEPnwTN
UJ/3e/1sA6qp8VSR0w2nk5X9eBa6K2ydABWzvL1ZRKsoT77fXTF2jUw/Dxmf7fZJSdG+xiawH8nO
oODU8JXNy0H0VO3bO26mFOB2Jb41S7tHQWCiOl/onXn5LTGCAU4wggFKAgEBMIGkMIGPMRswGQYD
VQQKExJWZXJpU2lnbiBBdXN0cmFsaWExFzAVBgNVBAsTDkdhdGVrZWVwZXIgUEtJMTgwNgYDVQQL
Ey9UZXJtcyBvZiB1c2UgYXQgaHR0cHM6Ly93d3cuZXNpZ24uY29tLmF1L0dLUlBBLzEdMBsGA1UE
AxMUR2F0ZWtlZXBlciBUWVBFIDMgQ0ECEHcfXJN2/vVA/xHMFSpRm0kwDAYIKoZIhvcNAgUFADAN
BgkqhkiG9w0BAQEFAASBgE8R54fOgqSzm+2cJpKrAfj7eOLQJfMIwSnxqGkXY/b129sa8woTgP+4
XnVtxOCyvGyYo+t9Q+JhgnhKOuRxWrGyu3r3tU/pMHqFWNVy3pYa47s117ehf7JT8vBLL1m8uamd
PinFLTfyHq2HNwhbrgdtndfjGQlpQ3Xg0Njl/b/Y
";
			interchange20.EI_HeaderText = "";
			interchange20.EI_FooterText = "";
			interchange20.ContainedMessages[0].EM_ApplicationCode = ApplicationCodeList.Codes.AUCMR;

			var interchange21 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange21.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange21.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange21.EI_BodyText = "Test ORG Message";
			interchange21.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Organizations;

			var expectedReference = CurrentCompany.LicenceKeyIdentifier;
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange1), interchange1, EDIMessageSchemaNameList.Descriptions.AgencyBillsOfLading,
				MessageSchemaType.Xml, "Test ABL Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange2), interchange2, EDIMessageSchemaNameList.Descriptions.Consols,
				MessageSchemaType.Xml, "Test CON Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange3), interchange3, EDIMessageSchemaNameList.Descriptions.ContainerMovements,
				MessageSchemaType.Xml, "Test CMV Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange4), interchange4, EDIMessageSchemaNameList.Descriptions.Events,
				MessageSchemaType.Xml, "Test EVT Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange5), interchange5, EDIMessageSchemaNameList.Descriptions.FinancialTransactions,
				MessageSchemaType.Xml, "Test FTR Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange6), interchange6, EDIMessageSchemaNameList.Descriptions.Orders,
				MessageSchemaType.Xml, "Test ORD Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange7), interchange7, EDIMessageSchemaNameList.Descriptions.Products,
				MessageSchemaType.Xml, "Test PRD Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange8), interchange8, EDIMessageSchemaNameList.Descriptions.Shipments,
				MessageSchemaType.Xml, "Test SHP Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange9), interchange9, EDIMessageSchemaNameList.Descriptions.WhsDockets,
				MessageSchemaType.Xml, "Test WHD Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange15), interchange15, EDIMessageSchemaNameList.Descriptions.NativeDataMessaging,
			MessageSchemaType.Xml, "Test NDM Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange10), interchange10, EDIMessageSchemaNameList.Descriptions.FHL,
				MessageSchemaType.FlatFile, "Test FHL Message", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange11), interchange11, EDIMessageSchemaNameList.Descriptions.FWB,
				MessageSchemaType.FlatFile, "Test FWB Message", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange12), interchange12, "AA",
				MessageSchemaType.FlatFile, @"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Test USI Message]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange13), interchange13, "BB",
				MessageSchemaType.FlatFile, @"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Test USE Message]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange14), interchange14, "CC",
				MessageSchemaType.FlatFile, @"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Test AMS Message]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange16), interchange16, EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging,
				MessageSchemaType.Xml, "<?xml version=\"1.0\" encoding=\"utf-8\"?><body>Test ABL Message</body>", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange17), interchange17, EDIMessageSchemaNameList.Descriptions.CustomsWare,
				MessageSchemaType.Xml, "<?xml version=\"1.0\" encoding=\"utf-8\"?><InputDocument xmlns='http://www.customsware.com/schema/api'><Credentials><UserID>Blah</UserID></Credentials></InputDocument>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange18), interchange18, EDIMessageSchemaNameList.Descriptions.NZCustoms,
				MessageSchemaType.Xml, @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>" + expectedReference + "</Reference><Type>Text</Type><Authentication /><Content>VU5BOisuPyAnVU5CK1VOT0E6MiswMDAwOTkwOEM6WlpaK0NVU1NXVDpaWlorMTIxMTMwOjEzMTQrMjI5J1VOSCsyNTgrQ1VTREVDOkQ6OTZCOlVOJ0JHTSs5MjkrQjAwMTU5MDg5KzknQ1NUKysxMDoxMDU6MTQzJ0xPQys5K0FVU1lEJ0xPQysxMStOWkFLTCdMT0MrNDErTlpBS0wnRFRNKzE1MToyMDEyMTEzMDoxMDInTUVBK1dUK0FBRCtLR006MjAwJ1JGRitNQjowODExMTExMTExMSdSRkYrSFdCOkVBVURFQ09MT0dORTEnUEFDKzIzKytQSydURFQrMjArKzQrKysrKzo6OlFGMjUzJ05BRCtBTCswMDc4MjkwM0Y6WlpaOjE0MydOQUQrQ0IrMDAwMDk5MDhDOlpaWjoxNDMnVU5TK0QnRE1TKzEwMDEwMDErOTM1J1RPRCsrK0ZPQjoxMDY6MTQzJ0NTVCsxKzU2MDEyMTAwMDBHOjE2OToxNDMnRlRYK0FBQSsrK1dBRERJTkcgRVRDIE9GIENPVFRPTidMT0MrMjcrQVUnTE9DKzM1K0FVJ05BRCtTVSswMDcxMDg0MVk6WlpaOjE0MydNT0ErMTQ6MTIwMC4wMDpOWkQnQ1VYKzIrKzEuMDAnTU9BKzQwOjEyMDAnTU9BKzY0OjE1MCdNT0ErNzA6MTInR0lTK1k6MTA5OjE0MydUQVgrMStHU1QnTU9BKzE2MToyMDQuMzAnR0lTK1E6MTE2OjE0MydVTlMrUydDTlQrNDoxJ0NOVCs1OjEnQ05UKzExOjIzJ1RBWCszK0NVRCsrMTIwMCdNT0ErMTYxOjAuMDAnVEFYKzMrR1NUJ01PQSsxNjE6MjA0LjMwJ1RBWCs0K1RPVCdNT0ErMTYxOjIwNC4zMCdHSVMrQzoxMzQ6MTQzJ0FVVCtCSEpFRENGS0lCQERHQUVAKzY1NDMyMTk4QidVTlQrNDQrMjU4J1VOWisxKzIyOSc=</Content></NZCustoms>", string.Empty, string.Empty, "NZCustoms");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange19), interchange19, EDIMessageSchemaNameList.Descriptions.NZCustoms,
				MessageSchemaType.Xml, @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>" + expectedReference + "</Reference><Type>Xml</Type><Authentication /><Content>TWVzc2FnZSBoYXMgY29udGVudC4=</Content></NZCustoms>", string.Empty, string.Empty, "NZCustoms");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange20), interchange20, EDIMessageSchemaNameList.Descriptions.AUCustoms,
				MessageSchemaType.Xml, @"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>" + expectedReference + "</Reference><Content>TWltZS1WZXJzaW9uOiAxLjANCkNvbnRlbnQtVHJhbnNmZXItRW5jb2Rpbmc6IGJhc2U2NA0KQ29udGVudC1UeXBlOiBhcHBsaWNhdGlvbi9wa2NzNy1taW1lOyBzbWltZS10eXBlPXNpZ25lZC1kYXRhOyBuYW1lID0gInNtaW1lLnA3bSINCg0KTUlJSmRBWUpLb1pJaHZjTkFRY0NvSUlKWlRDQ0NXRUNBUUV4RGpBTUJnZ3Foa2lHOXcwQ0JRVUFNSUlEUFFZSktvWklodmNOQVFjQg0Kb0lJRExnU0NBeXBOYVcxbExWWmxjbk5wYjI0NklERXVNQTBLUTI5dWRHVnVkQzFVY21GdWMyWmxjaTFGYm1OdlpHbHVaem9nWW1Geg0KWlRZMERRcERiMjUwWlc1MExWUjVjR1U2SUdGd2NHeHBZMkYwYVc5dUwyVmthV1poWTNRN0lHNWhiV1U5UVVGTU16WTBVRjh6TG1Waw0KYVEwS1EyOXVkR1Z1ZEMxRWFYTndiM05wZEdsdmJqb2dZWFIwWVdOb2JXVnVkRHNnWm1sc1pXNWhiV1U5UVVGTU16WTBVRjh6TG1Waw0KYVEwS0RRcFdWVFZDVDJsemRWQjVRVzVXVlRWRFN6RldUMVF3VFRaTmVYUkNVVlYzZWs1cVVsRlBhbkJDVVZWM2VrNXFVbEZMTUVaQw0KVVZSTmVrNXJUWEpOVkUxM1RXcEpkMDlxUlhoTlZHdHlEUXBOZVhOeVMzbHplRXQ1YzNoS01WWlBVME56ZUVzd1RsWlZNRTVDVldwdw0KUlU5cWF6VlJhbkJXVkdsa1ExSXdNSEpQVkUxNlQycHZObEZWYkZOUk1VbHlVVlJaZDAxRVFYaE5SRVV3RFFwTU1FNU9UV3BGTmsxVA0KY3pWS01VcEhVbWwwVVZWVWNGRlVlV1JUVW10WmNsTkdaRU5QYTJkM1RVUkJlRkY1WkZOU2ExbHlWRlprUTA5cVFUUk5WRkV3VGtSUg0KZDAxRVFYaEtNRFZDRFFwU1EzUkVWR2x6Y2xSclJrNVNWRzgyU1VOQlowbERRa0pXVTJSUFVWVlJjbEV4YjNKTE1EVkNWRlZWTms5cA0KUVdkSlEwRm5VakJKYmxSclJrVkxNVnBZUzNwUmVFMUVXVEZQUkdzd0RRcE9la2t3VDJwdk5VNVRaRlZTUmxGeVRXcEJjazE2U1hKTA0KZWxseVZWVlpOazlxVFc1VVJUbEVTM3BuY2xGV1ZsUlhWVkUyVDJwWmJsUkZPVVJMZW1NeVN6QmtRMVJGYUZOUGFtOHlEUXBLTUhoUQ0KVVhsemVFMXBkRUpXVms1YVVrUnZOazVwWkUxVU1FMXlUMVJGY2xJd1NrMVRSa2syVDJwWmJsSkdVazVMZWtVelQwUnZlVTFFUlhwTg0KUkVsNVRWUnZlRTFFU1c1Uk1EVktEUXBMZWtWdVZXdGFSMHN4VmtSVWFuQkNUbXBCZDAxRVJYZE5WRkZ1VkZVNVFrdDZVVEJQYWsxMw0KVFVNMGQwMUVjRUpXVlZGdVVqQnNWRXN4VGtKUmVtOTRUVVJyTms5VVZXNVNNR3hGRFFwTGVrVnVWVVZHUkV0NlRXNVNiRkpaU3pCRw0KUWxGVGMzSkxNV2haVjBab1dWZEdhRmxYUm1oWlYwTmtUbEpWUlhKUlZVWkdTekJqY2xNd1l6Wk5lVFIzVFVOa1ZsUnNVWEpOYWsxeQ0KRFFwTlUyUldWR3h2Y2sxVGMzcEtkejA5RFFxZ2dnUzNNSUlFc3pDQ0JCeWdBd0lCQWdJUWR4OWNrM2IrOVVEL0Vjd1ZLbEdiU1RBTg0KQmdrcWhraUc5dzBCQVFRRkFEQ0JqekViTUJrR0ExVUVDaE1TVm1WeWFWTnBaMjRnUVhWemRISmhiR2xoTVJjd0ZRWURWUVFMRXc1SA0KWVhSbGEyVmxjR1Z5SUZCTFNURTRNRFlHQTFVRUN4TXZWR1Z5YlhNZ2IyWWdkWE5sSUdGMElHaDBkSEJ6T2k4dmQzZDNMbVZ6YVdkdQ0KTG1OdmJTNWhkUzlIUzFKUVFTOHhIVEFiQmdOVkJBTVRGRWRoZEdWclpXVndaWElnVkZsUVJTQXpJRU5CTUI0WERURXhNRGd4TVRBdw0KTURBd01Gb1hEVEV6TURneE1USXpOVGsxT1Zvd1p6RUxNQWtHQTFVRUJoTUNRVlV4RERBS0JnTlZCQWdUQTA1VFZ6RWVNQndHQTFVRQ0KQ2hRVlEwRlNSMDlYU1ZORklFVkVTU0JRVkZrZ1RGUkVNUkl3RUFZRFZRUUxGQWxEWVhKbmIxZHBjMlV4RmpBVUJnTlZCQU1URFdWaw0KYVVWdWRHVnljSEpwYzJVd2daOHdEUVlKS29aSWh2Y05BUUVCQlFBRGdZMEFNSUdKQW9HQkFKZVJWNzVxTVZzK2RPU2lEYU9jMHFPZQ0KVHJHbjBkekpSeG81UlpzaDJBK1FmZ3NodjkxZ1F6L2U4bWRWa3JJQ1ExOHNSVnBBN1ZnVUNUbmNTMVdkd3pyZitJSXpocTkrR1FvSw0KYXUwRi9FcEJzV0dCU2t2OFRnaUVlTUljT05TUG1tbkd4MlpvaTY3SGIrNUJoenBHVEVIYnhJWm5PbVo0N2lPWEQ5MnRRbDl0QWdNQg0KQUFHamdnSTFNSUlDTVRBTUJnTlZIUk1CQWY4RUFqQUFNSUlCQmdZRFZSMGZCSUgrTUlIN01JSDRvSUgxb0lIeWhqMW9kSFJ3T2k4dg0KYjI1emFYUmxZM0pzTG1WemFXZHVMbU52YlM1aGRTOUhZWFJsYTJWbGNHVnlWSGx3WlRORFFTOU1ZWFJsYzNSRFVrd3VZM0pzaG9Hdw0KYkdSaGNEb3ZMMlJwY21WamRHOXllUzVsYzJsbmJpNWpiMjB1WVhVdlkyNDlSMkYwWld0bFpYQmxjaUJVV1ZCRklETWdRMEVzYjNVOQ0KVkdWeWJYTWdiMllnZFhObElHRjBJR2gwZEhCek9pOHZkM2QzTG1WemFXZHVMbU52YlM1aGRTOUhTMUpRUVM4c2IzVTlSMkYwWld0bA0KWlhCbGNpQlFTMGtzYnoxV1pYSnBVMmxuYmlCQmRYTjBjbUZzYVdFL1kyVnlkR2xtYVdOaGRHVnlaWFp2WTJGMGFXOXViR2x6ZER0aQ0KYVc1aGNua3dId1lEVlIwakJCZ3dGb0FVdGRzWnhvN3haT2ZjZ0tmcElvNGVKZ1FsdERRd0hRWURWUjBPQkJZRUZHQ0NVWVVrN1lUaw0KV2lCRG05ZkxvZUdGVW5jRE1EVUdDQ3NHQVFVRkJ3RUJCQ2t3SnpBbEJnZ3JCZ0VGQlFjd0FZWVphSFIwY0hNNkx5OXZZM053TG1Weg0KYVdkdUxtTnZiUzVoZFRBT0JnTlZIUThCQWY4RUJBTUNCUEF3SEFZRFZSMFJCQlV3RTRFUlkyMXlRR05oY21kdmQybHpaUzVqYjIwdw0KRndZR0tpUUJnazBCQkEwV0N6UXhNRFkxT0RrME56STBNRVlHQTFVZElBUS9NRDB3T3dZS0tpU3AvTFJqZ2swQ0NEQXRNQ3NHQ0NzRw0KQVFVRkJ3SUJGaDlvZEhSd2N6b3ZMM2QzZHk1bGMybG5iaTVqYjIwdVlYVXZSMHRTVUVFdk1CRUdDV0NHU0FHRytFSUJBUVFFQXdJSA0KZ0RBTkJna3Foa2lHOXcwQkFRUUZBQU9CZ1FCTXcyS2JLdHRxNjJSbHhpT3loOERMQnF2QW8wbzRFd0VuSjNKQjVWdVVvcEVQbndUTg0KVUovM2UvMXNBNnFwOFZTUjB3Mm5rNVg5ZUJhNksyeWRBQld6dkwxWlJLc29UNzdmWFRGMmpVdy9EeG1mN2ZaSlNkRyt4aWF3SDhuTw0Kb09EVThKWE55MEgwVk8zYk8yNm1GT0IySmI0MVM3dEhRV0NpT2wvb25YbjVMVEdDQVU0d2dnRktBZ0VCTUlHa01JR1BNUnN3R1FZRA0KVlFRS0V4SldaWEpwVTJsbmJpQkJkWE4wY21Gc2FXRXhGekFWQmdOVkJBc1REa2RoZEdWclpXVndaWElnVUV0Sk1UZ3dOZ1lEVlFRTA0KRXk5VVpYSnRjeUJ2WmlCMWMyVWdZWFFnYUhSMGNITTZMeTkzZDNjdVpYTnBaMjR1WTI5dExtRjFMMGRMVWxCQkx6RWRNQnNHQTFVRQ0KQXhNVVIyRjBaV3RsWlhCbGNpQlVXVkJGSURNZ1EwRUNFSGNmWEpOMi92VkEveEhNRlNwUm0wa3dEQVlJS29aSWh2Y05BZ1VGQURBTg0KQmdrcWhraUc5dzBCQVFFRkFBU0JnRThSNTRmT2dxU3ptKzJjSnBLckFmajdlT0xRSmZNSXdTbnhxR2tYWS9iMTI5c2E4d29UZ1ArNA0KWG5WdHhPQ3l2R3lZbyt0OVErSmhnbmhLT3VSeFdyR3l1M3IzdFUvcE1IcUZXTlZ5M3BZYTQ3czExN2VoZjdKVDh2QkxMMW04dWFtZA0KUGluRkxUZnlIcTJITndoYnJnZHRuZGZqR1FscFEzWGcwTmpsL2IvWQ0K</Content></AUCustoms>", string.Empty, string.Empty, "AUCustoms");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange21), interchange21, EDIMessageSchemaNameList.Descriptions.Organizations,
				MessageSchemaType.Xml, "Test ORG Message", "Blah@BLah.com", "BLAH.txt");
		}

		public void TestCreateeHubMessageWithXMLDeclaration_ChunkReadingWorksWell()
		{
			var fileName = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Xml, 100 * 1024 * 1024, false);
			try
			{
				var interchangeNumberStrategy = new TestMessageNumberStrategy();
				var messageNumberStrategy = new TestMessageNumberStrategy();
				var interchange = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
				interchange.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
				interchange.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;

				long fileLengthInBytes;
				using (var messageBodyText = new FileStream(fileName, FileMode.Open))
				{
					fileLengthInBytes = messageBodyText.Length;
					interchange.SetEI_BodyTextSource(new TextReaderSource(messageBodyText));
					Factory.Save();
				}

				var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);

				using (var eHubMessage = serviceTaskJob.Object.CreateeHubMessage(interchange))
				{
					AssertNotNull(eHubMessage);
					AssertEquals("Blah@BLah.com", eHubMessage.EmailSubject);
					AssertEquals("BLAH.txt", eHubMessage.Filename);
					AssertEquals(CurrentCompany.LicenceKeyIdentifier, eHubMessage.SenderID);
					AssertEquals("BLAHBLAHB", eHubMessage.RecipientID);
					AssertEquals(interchange.EI_SessionGUID.ToGuid(), eHubMessage.TrackingID);
					AssertEquals(EDIMessageSchemaNameList.Descriptions.AgencyBillsOfLading, eHubMessage.SchemaName);
					AssertEquals(MessageSchemaType.Xml, eHubMessage.SchemaType);
					AssertEquals(EHubMessageBuilderForXml.XmlDeclaration.Length + fileLengthInBytes, eHubMessage.MessageStream.Length);
					var notes = interchange.Notes.FindByDescription("eHub Error");
					AssertEquals(0, notes.Length);
				}
			}
			finally
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}
		}

		public void TestSendMessages_OneOutgoingMessageSizeOverGatewayMaxReceiveSize()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			using (var disposables = new DisposableList(3))
			{
				var interchangeBody = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
                     xmlns:DIS=""http://cbp.dhs.gov/DIS""
                     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://cbp.dhs.gov/DIS ../MessageEnvelope.xsd"">
  <DIS:MessageBody>
    <DIS:DocumentSubmissionPackage>
      <DIS:DocumentData>
        <DIS:DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!</DIS:DocumentObject>
      </DIS:DocumentData>
    </DIS:DocumentSubmissionPackage>
  </DIS:MessageBody>
</DIS:MessageEnvelope>";
				var interchangeList = new List<EDIInterchange>();
				var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
				var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
				var container = (BusinessObject)Factory.New<Integration.Freight.ICommonContainer>();
				var documentManager = container as IDocManagerSupport;
				var messageCount = 5;
				var eDoc1 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes(new string('$', 500)), disposables);
				var eDoc2 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes(new string('$', 3000)), disposables);
				var eDoc3 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes(new string('$', 1200)), disposables);
				documentManager.DocManagerInfo.Save();

				var utcNow = DateTime.UtcNow;
				for (int i = 1; i <= messageCount; i++)
				{
					var trackingID = new Guid("00000000-0000-0000-0000-00000000000" + i);
					var message = Factory.NewWithValidTestData<EDIMessage>();
					message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
					message.EM_LinkedObject = container;

					var attachment = message.MessageAttachments.AddNew();
					attachment.EG_FileName = "Attachment" + i + ".pdf";
					attachment.EG_EdiMsgDocType = "APP";
					attachment.EG_StorageDocsGuid = i == 2 ? eDoc2.UniqueKey : i == 3 ? eDoc3.UniqueKey : eDoc1.UniqueKey;

					string messageText = interchangeBody.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", "!dOcUmEnTiMaGePlAcEHoLdEr:" + attachment.PK + "!");

					var interchange = Factory.New<EDIInterchange>();
					interchange.ContainedMessages.Add(message);
					interchange.EI_InterchangeNum = "0000000" + i;
					interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
					interchange.EI_IsActive = ZBool.True;
					interchange.EI_Status = EDIInterchangeStatusList.Codes.eAdaptorQueued;
					interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
					interchange.EI_GB = CurrentCompany.FirstActiveBranch.PK;
					interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISSubmission;
					interchange.EI_BodyText = messageText;
					interchange.EI_From = "ABC Corp";
					interchange.EI_To = "US DIS";
					interchange.EI_SessionGUID = trackingID;
					interchange.EI_SystemCreateTimeUtc = utcNow.AddSeconds(i);
					interchangeList.Add(interchange);
				}
				Factory.Save();

				var outboxSizeLimit = 3072;
				var maxReceiveSizeLimit = 4000;

				int callCount = 0;
				adapterMock.Setup(x => x.SendMessages())
					.Callback(() =>
					{
						callCount++;
						if (callCount == 1)
						{
							var outboxSize = (adapterMock.Object.Outbox as MessageOutboxMock).SizeInBytes;
							AssertEquals(1, adapterMock.Object.Outbox.Count);
							AssertLessThan("First batch's size should be less than outbox size limit", outboxSize, outboxSizeLimit);
						}
						else if (callCount == 2)
						{
							var outboxSize = (adapterMock.Object.Outbox as MessageOutboxMock).SizeInBytes;
							AssertEquals(1, adapterMock.Object.Outbox.Count);
							AssertGreaterThan("Second batch's size should be larger than max receive size", outboxSize, maxReceiveSizeLimit);
						}
						else if (callCount == 3)
						{
							var outboxSize = (adapterMock.Object.Outbox as MessageOutboxMock).SizeInBytes;
							AssertEquals(1, adapterMock.Object.Outbox.Count);
							AssertLessThan("Third batch's size should be less than outbox size limit", outboxSize, outboxSizeLimit);
						}
						else if (callCount == 4)
						{
							var outboxSize = (adapterMock.Object.Outbox as MessageOutboxMock).SizeInBytes;
							AssertEquals(2, adapterMock.Object.Outbox.Count);
							AssertLessThan("Final batch should contain 2 outgoing messages and its size is less than outbox size limit", outboxSize, outboxSizeLimit);
						}
					});

				serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
				serviceTaskJob.Object.CurrentRecipient = "US DIS";
				serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);
				serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(outboxSizeLimit);
				serviceTaskJob.Setup(m => m.GatewayMaxReceivedMessageLimitInBytes).Returns(maxReceiveSizeLimit);

				serviceTaskJob.Object.Execute(CancellationToken.None);
				serviceTaskJob.Object.Notifier.AssertNotificationExists($"1 interchange(s) sent to {ServiceTaskName}.", 3);
				serviceTaskJob.Object.Notifier.AssertNotificationExists($"2 interchange(s) sent to {ServiceTaskName}.");
				serviceTaskJob.Object.Notifier.AssertNotificationContains($"'{interchangeList[1].PK}' item has a prepared outgoing message, which causes the current batch");
				serviceTaskJob.Object.Notifier.AssertNotificationContains($"'{interchangeList[3].PK}' item has a prepared outgoing message, which causes the current batch");

				var reloadFactory = new BusinessObjectFactory();
				var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
				AssertEquals(5, reloadedInterchanges.Count(x => x.EI_Status == "SNT" && x.EI_RetryCount == 0));
				adapterMock.VerifyAll();
			}
		}

		public void TestSendMessages_MultipleBatchWhenExceedingGatewayLimit()
		{
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < 15; i++)
			{
				interchangeList.Add(CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, interchangeSize: 1));
			}
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(100);
			serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(9);
			serviceTaskJob.Setup(m => m.GatewayMaxReceivedMessageLimitInBytes).Returns(5);
			serviceTaskJob.Object.Execute(CancellationToken.None);
			Factory.ReloadAll<EDIInterchange>();
			serviceTaskJob.Object.Notifier.AssertNotificationExists($"9 interchange(s) sent to {ServiceTaskName}.");
			serviceTaskJob.Object.Notifier.AssertNotificationExists($"6 interchange(s) sent to {ServiceTaskName}.");
			foreach (var item in interchangeList)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}

			var reloadFactory = new BusinessObjectFactory();
			var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
			foreach (var item in reloadedInterchanges)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}
		}

		public void TestPendingItemsBatchSize()
		{
			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertEquals("Default value was incorrect", 1000, serviceTaskJob.Object.PendingItemsBatchSize);

			eAdaptorRegistry.Instance.eAdaptorOutboundPendingItemsBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10000);
			AssertEquals("Value was not updated", 10000, serviceTaskJob.Object.PendingItemsBatchSize);
		}

		public void TestPendingItemsSearchLimit()
		{
			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertEquals("Default value was incorrect", 100000, serviceTaskJob.Object.PendingItemsSearchLimit);

			eAdaptorRegistry.Instance.eAdaptorOutboundPendingItemsSearchLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10000);
			AssertEquals("Value was not updated", 10000, serviceTaskJob.Object.PendingItemsSearchLimit);
		}

		public void TestInterchangesAreNotLocked()
		{
			using (var messageSent = new AutoResetEvent(false))
			using (var executeCompleteEvent = new AutoResetEvent(false))
			using (var lockEvent = new AutoResetEvent(false))
			{
				var testFailedTimeoutSpan = TimeSpan.FromSeconds(30);
				var interchange = CreateTestInterchange("BLAHBLAHB", new TestMessageNumberStrategy(), new TestMessageNumberStrategy(), interchangeSize: 1);
				Factory.Save();

				var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
				mockOutbox.Setup(m => m.AddMessage(It.IsAny<IeHubMessage>()));
				mockOutbox.Setup(m => m.Count).Returns(1);
				mockOutbox.Setup(m => m.Clear());
				mockOutbox.Setup(m => m.SizeInKiloBytes).Returns(1);

				var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
				mockAdapter.Setup(m => m.Dispose());
				mockAdapter.Setup(m => m.Outbox).Returns(mockOutbox.Object);
				mockAdapter.Setup(a => a.SendMessages())
					.Callback(new Action(() => messageSent.Set()));
				var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object), mockInterchangeCandidates: false);

				var task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						Assert("Should be able to obtain lock before execute", Db.Connection.TryGetLock(MutexConstants.MessageMutexPrefix + interchange.EI_SessionGUID, out var mutex));
						using (mutex)
						{
							lockEvent.Set();
							Assert("Message was not sent", messageSent.WaitOne(testFailedTimeoutSpan));
							Assert("Execute did not complete while we were holding the lock", executeCompleteEvent.WaitOne(testFailedTimeoutSpan));
						}
					}
				});
				Assert("Lock event didn't fire", lockEvent.WaitOne(testFailedTimeoutSpan));
				mockServiceTaskJob.Object.Execute(CancellationToken.None);
				executeCompleteEvent.Set();
				Assert("lock task did not complete", task.Wait(testFailedTimeoutSpan));
				Factory.ReloadAll<EDIInterchange>();
				AssertEquals("Status should be pending, as the send should have been successful", mockServiceTaskJob.Object.InterchangeSuccessStatus, interchange.EI_Status.ToString());
			}
		}

		public override void TestExecuteInternal_CorrectlyHandlesLocks_DuringJobRun()
		{
			var company = CreateCompanyWithBranch(Factory);
			var settingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { CurrentCompany });
			var serviceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(new EHubAdapterMock()), settingsManager.Object, company, new NotificationBuffer(), mockInterchangeCandidates: false);
			_ = settingsManager.Setup(m => m.ClearCache());

			var interchange = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company, "RC1", new TestMessageNumberStrategy(), new TestMessageNumberStrategy());
			interchange.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
			SetInterchangeProperties(interchange, company.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());

			Factory.Save();

			var lockWasReleased = false;
			serviceTaskJob.Object.AfterProcessingBatch += (batch) =>
			{
				using (var dbConnection = Db.NewExtraConnectionToMainDb())
				{
					var extraLockParams = string.Join(string.Empty, serviceTaskJob.Object.ExtraColumnsToBatchPendingInterchangesBy.Select(c => $"_{batch[c.Name]?.ToString() ?? "(null)"}"));
					var isAbleToLock = dbConnection.TryGetLock($"{serviceTaskJob.Object.MutexPrefix}{(ZGuid)batch[EDIInterchangeSchema.Constants.EI_GB]}_{batch[EDIInterchangeSchema.Constants.EI_To]}{extraLockParams}", out var recipientMutex);

					if (isAbleToLock)
					{
						recipientMutex.Dispose();
					}
					lockWasReleased = isAbleToLock;
				}
			};

			serviceTaskJob.Object.Execute(CancellationToken.None);

			Assert("Lock should have been released after processing one batch of messages, but was not", lockWasReleased);
		}

		protected override bool ShouldSkipDeveloperCompany
		{
			get { return false; }
		}

		protected override eHubMessaging.ServiceTasks.AdapterType ServiceAdapterType => eHubMessaging.ServiceTasks.AdapterType.Adapter;

		protected override void AdditionalServiceTaskJobSetup_Moq(Mock<eAdaptorOutboundServiceTaskJob> mockServiceTaskJob, GlbCompany company, bool mockInterchangeCandidates = true)
		{
			base.AdditionalServiceTaskJobSetup_Moq(mockServiceTaskJob, company, mockInterchangeCandidates);
			mockServiceTaskJob.Setup(m => m.ExtraColumnsToBatchPendingInterchangesBy).Returns(Enumerable.Empty<SchemaColumn>());
		}

		protected override string GetExpectedTableIndexHint()
		{
			return EDIInterchangeSchema.Constants.Indexes.NR_RX__EI_ReceiveTransmit_EI_Status_EI_SystemCreateTimeUtc_AQU;
		}

		protected override string GetInterchangeQueuedStatus()
		{
			return EDIInterchange.Status.eAdaptorQueued;
		}

		protected override string GetInterchangePendingStatus()
		{
			return EDIInterchange.Status.Sent;
		}

		void AssertEHubMessage(IeHubMessage message, EDIInterchange interchange, string schemaName, MessageSchemaType schemaType, string messageText, string emailSubject, string fileName, string recipientID = "BLAHBLAHB")
		{
			AssertNotNull(message);
			AssertEquals(emailSubject, message.EmailSubject);
			AssertEquals(fileName, message.Filename);
			AssertEquals(CurrentCompany.LicenceKeyIdentifier, message.SenderID);
			AssertEquals(recipientID, message.RecipientID);
			AssertEquals(interchange.EI_SessionGUID.ToGuid(), message.TrackingID);
			AssertEquals(schemaName, message.SchemaName);
			AssertEquals(schemaType, message.SchemaType);
			AssertEquals(messageText, new StreamReader(message.MessageStream).ReadToEnd());
			var notes = interchange.Notes.FindByDescription("eHub Error");
			AssertEquals(0, notes.Length);
			message.Dispose();
		}

		public override ICompanySettingsManager CreateCompanySettingsManager() => new eAdaptorMessagingCompanySettingsManager();
	}
}
