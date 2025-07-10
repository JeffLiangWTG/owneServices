using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class XtErrorResponseMessageProcessorTest : TestCaseWithFactory
{
	protected (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { (MessageTypeCodeList.Codes.Import, Events.MessageStatusChange), (MessageTypeCodeList.Codes.EBD, Events.DocumentNotDelivered), (MessageTypeCodeList.Codes.ECM, Events.EComStatusChange) };

	string ExpectedFriendlyName => "XT Error Response Message Processor";

	string ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	XtErrorResponseMessageProcessor MessageProcessor => new XtErrorResponseMessageProcessor(Logger);

	LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestFriendlyName()
	{
		AssertEquals(ExpectedFriendlyName, MessageProcessor.MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCode, MessageProcessor.ApplicationCode);
	}

	public void TestResponseErrorMessage()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var (entryHeader, receivedEdiMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, messageType.MessageType, string.Empty, MessageSubTypeCodeList.Codes.Rejected, MessageResponse);
				Factory.Save();

				MessageProcessor.ProcessMessage(receivedEdiMessage);
				Factory.Save();

				var sentEdiMessage = receivedEdiMessage.Factory.GetOutgoingMessageFromSessionId(receivedEdiMessage.Interchange.EI_SessionGUID);
				var logEntry = Factory.Load<StmALog>(new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_Parent, entryHeader.PK)).FirstOrDefault();

				AssertEquals($"MessageType:{messageType.MessageType} - EM_LinkTable should be CusEntryHeader", "CusEntryHeader", receivedEdiMessage.EM_LinkTable);
				AssertEquals($"MessageType:{messageType.MessageType} - EM_LinkUniqueID", sentEdiMessage.EM_LinkUniqueID, receivedEdiMessage.EM_LinkUniqueID);
				AssertEquals($"MessageType:{messageType.MessageType} - Event type", messageType.ExpectedEvent, logEntry.Event);

				switch (messageType.MessageType)
				{
					case MessageTypeCodeList.Codes.Import:
						AssertEquals($"MessageType:{messageType.MessageType} - CH_Status", CHLogicalStatusList.Codes.Failed, entryHeader.CH_Status);
						break;
					case MessageTypeCodeList.Codes.EBD:
						AssertContains($"MessageType:{messageType.MessageType} - Event reference", filename, logEntry.SL_Reference);
						break;
					case MessageTypeCodeList.Codes.ECM:
						AssertEquals($"MessageType:{messageType.MessageType} - LastEComStatus", CHLogicalStatusList.Codes.Failed, entryHeader.CH_LastEComplaintStatus);
						break;
					default:
						break;
				}
			}
		});
	}

	#region Message Response

	ZString filename => "e-dec_Import_EL_HYESASCM2000000033_24CHEI000050827349_1_CHE326684996_72.pdf";

	protected ZString MessageResponse => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<EventTime>2024-06-07 07:14:28.259</EventTime>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Reason>Error Message
Contract: xt-contract:/Customs/CH Soap/CH Soap Document Contract
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: OpenSSL SSL_connect: SSL_ERROR_SYSCALL in connection to ws.ebd-a.bazg.admin.ch:443 
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: error:14094415:SSL routines:ssl3_read_bytes:sslv3 alert certificate expired
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: OpenSSL SSL_connect: SSL_ERROR_SYSCALL in connection to ws.ebd-a.bazg.admin.ch:443 
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: error:14094415:SSL routines:ssl3_read_bytes:sslv3 alert certificate expired
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: error:14094415:SSL routines:ssl3_read_bytes:sslv3 alert certificate expired
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: error:14094415:SSL routines:ssl3_read_bytes:sslv3 alert certificate expired
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: error:14094415:SSL routines:ssl3_read_bytes:sslv3 alert certificate expired
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: OpenSSL SSL_connect: SSL_ERROR_SYSCALL in connection to ws.ebd-a.bazg.admin.ch:443 
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: OpenSSL SSL_connect: SSL_ERROR_SYSCALL in connection to ws.ebd-a.bazg.admin.ch:443 
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: OpenSSL SSL_connect: SSL_ERROR_SYSCALL in connection to ws.ebd-a.bazg.admin.ch:443 
Reference Object: xt-httpclientaddress:{{0ba78fca-79ad-4e16-85b0-bbe629653e82}}
Reference Object: xt-node:{{56c3e0d6-4715-4c14-9894-0c1f0300c678}}

Error description: Message transmission to https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1 rejected by peer: OpenSSL SSL_connect: SSL_ERROR_SYSCALL in connection to ws.ebd-a.bazg.admin.ch:443 
</Reason>
					<MessageType>XER</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>OriginalMessage</Type>
						<Value><![CDATA[<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <ebdDocumentImportRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportRequest/v1"">
      <uidNumber>CHE326684996</uidNumber>
      <customsDeclarationNumber>24CHEI000050827349.1</customsDeclarationNumber>
      <accompanyingDocuments>
        <accompanyingDocument>
          <filename>{filename}</filename>
          <type>aBD</type>
          <content>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KgxOuQV5ROyZ60eK/wGx3/BfCmVuZHN0cmVhbQplbmRvYmoKMTEgMCBvYmo8PC9Db250ZW50cyA5IDAgUi9UeXBlL1BhZ2UvUmVzb3VyY2VzPDwvUHJvY1NldCBbL1BERiAvVGV4dCAvSW1hZ2VCIC9JbWFnZUMgL0ltYWdlSV0vRm9udDw8L0YxIDMgMCBSL0YyIDYgMCBSL0YzIDggMCBSPj4vWE9iamVjdDw8L1hmMyA0IDAgUi9YZjQgNSAwIFIvWGYxIDEgMCBSL1hmMiAyIDAgUj4+Pj4vUGFyZW50IDEwIDAgUi9NZWRpYUJveFswIDAgNTk1IDg0Ml0+PgplbmRvYmoKMyAwIG9iajw8L1N1YnR5cGUvVHlwZTEvVHlwZS9Gb250L0Jhc2VGb250L0hlbHZldGljYS9FbmNvZGluZy9XaW5BbnNpRW5jb2Rpbmc+PgplbmRvYmoKOCAwIG9iajw8L1N1YnR5cGUvVHlwZTEvVHlwZS9Gb250L0Jhc2VGb250L1RpbWVzLVJvbWFuL0VuY29kaW5nL1dpbkFuc2lFbmNvZGluZz4+CmVuZG9iago2IDAgb2JqPDwvU3VidHlwZS9UeXBlMS9UeXBlL0ZvbnQvQmFzZUZvbnQvSGVsdmV0aWNhLUJvbGQvRW5jb2RpbmcvV2luQW5zaUVuY29kaW5nPj4KZW5kb2JqCjQgMCBvYmogPDwvU3VidHlwZS9Gb3JtL0ZpbHRlci9GbGF0ZURlY29kZS9UeXBlL1hPYmplY3QvTWF0cml4IFsxIDAgMCAxIDAgMF0vRm9ybVR5cGUgMS9SZXNvdXJjZXM8PC9Qcm9jU2V0IFsvUERGIC9UZXh0IC9JbWFnZUIgL0ltYWdlQyAvSW1hZ2VJXS9Gb250PDwvRjEgMyAwIFI+Pj4+L0JCb3hbMCAwIDAgMjAwXS9MZW5ndGggNTI+PnN0cmVhbQp4nHMK4dJ3M1QwNDBQCEnjMlQw0DO3MDEG0gpF6SAuEIJIU6B0LpeGZkgWl2sIFwAa0wqDCmVuZHN0cmVhbQplbmRvYmoKNSAwIG9iaiA8PC9TdWJ0eXBlL0Zvcm0vRmlsdGVyL0ZsYXRlRGVjb2RlL1R5cGUvWE9iamVjdC9NYXRyaXggWzEgMCAwIDEgMCAwXS9Gb3JtVHlwZSAxL1Jlc291cmNlczw8L1Byb2NTZXQgWy9QREYgL1RleHQgL0ltYWdlQiAvSW1hZ2VDIC9JbWFnZUldL0ZvbnQ8PC9GMiA2IDAgUj4+L1hPYmplY3Q8PC9YZjUgNyAwIFI+Pj4+L0JCb3hbMCAwIDE3Mi4wOCA3NS43XS9MZW5ndGggMTQxPj5zdHJlYW0KeJxtjrsOwjAMRfd8hUdYUudN1rZBMCJlYEeNSEWLCoXvxwUkMuA7+txHHVm1lSAQYmICkCRIxnMhIA5sFfKYHufbJd/nbh37HyM9t6oA2y7lMc/5uVAhsgmQe2+c+hi40uSy3Do4DVAdk4H2CgdW/x/wBpfYspPKsHhJ3ezCHukMbqRTmrZ8y1+5Cy+HCmVuZHN0cmVhbQplbmRvYmoKNyAwIG9iaiA8PC9TdWJ0eXBlL0Zvcm0vRmlsdGVyL0ZsYXRlRGVjb2RlL1R5cGUvWE9iamVjdC9NYXRyaXggWzEgMCAwIDEgMCAwXS9Gb3JtVHlwZSAxL1Jlc291cmNlczw8L1Byb2NTZXQgWy9QREYgL1RleHQgL0ltYWdlQiAvSW1hZ2VDIC9JbWFnZUldL0ZvbnQ8PC9GMSAzIDAgUj4+Pj4vQkJveFswIDAgMTY4LjggMzIuMjFdL0xlbmd0aCAzMDM+PnN0cmVhbQp4nJVUu04EMQzs8xUpoQl+JE7Sgg5Bn18AJCSa+/+CLLd56OyGTRPNrsfjyWzAQ1/XLwe+BEKPQTxFf/1wFOINglBOKPbdH3S8u0FFQ0j3dRgH16LH3Lf339Vw1nLfnDJ4YKsDidZGVfMxDXWrL6fBt2Gz7zYrjNoN045kzRYNJclQkkR3EFAIajYxPJHpyfIug/Y9G0qyKKTqucpUsmFRsxWFVOVu5cG1JqiGH9U4GYQpZI2KkIw8QdWS0cinGVA09CCBjjsbGmkas2kk629hXcyWHM7aMrRSitGIB1pJxRXV7cvEhpNJ+5iKoVLmtbExinU2YgmS6RD8C/p0naavfpE9N/f0euzbAR63G/pcQu29CbJvP+6B4svb5R36k6BQ5m4DPrZvd2nuF9Xf/+gKZW5kc3RyZWFtCmVuZG9iagoyIDAgb2JqIDw8L1N1YnR5cGUvRm9ybS9GaWx0ZXIvRmxhdGVEZWNvZGUvVHlwZS9YT2JqZWN0L01hdHJpeCBbMSAwIDAgMSAwIDBdL0Zvcm1UeXBlIDEvUmVzb3VyY2VzPDwvUHJvY1NldCBbL1BERiAvVGV4dCAvSW1hZ2VCIC9JbWFnZUMgL0ltYWdlSV0vRm9udDw8L0YxIDMgMCBSPj4+Pi9CQm94WzAgMCAyNTUuNiAyMDBdL0xlbmd0aCA1Nj4+c3RyZWFtCniccwrh0nczVDA0MFAISeMyVDDQM7cwMQbSCkXpIC4QgkhToHQul0aIa3CIZkgWl2sIFwBKaAvDCmVuZHN0cmVhbQplbmRvYmoKMSAwIG9iaiA8PC9TdWJ0eXBlL0Zvcm0vRmlsdGVyL0ZsYXRlRGVjb2RlL1R5cGUvWE9iamVjdC9NYXRyaXggWzEgMCAwIDEgMCAwXS9Gb3JtVHlwZSAxL1Jlc291cmNlczw8L1Byb2NTZXQgWy9QREYgL1RleHQgL0ltYWdlQiAvSW1hZ2VDIC9JbWFnZUldL0ZvbnQ8PC9GMSAzIDAgUj4+Pj4vQkJveFswIDAgNTAgNTBdL0xlbmd0aCAyOT4+c3RyZWFtCniccwrh0nczVLBQCEnj0jDUDMnicg3hAgAw2QRwCmVuZHN0cmVhbQplbmRvYmoKMTAgMCBvYmo8PC9LaWRzWzExIDAgUl0vVHlwZS9QYWdlcy9Db3VudCAxPj4KZW5kb2JqCjEyIDAgb2JqPDwvVHlwZS9DYXRhbG9nL1BhZ2VzIDEwIDAgUj4+CmVuZG9iagoxMyAwIG9iajw8L01vZERhdGUoRDoyMDI0MDUwNzE1MzIzMSswMicwMCcpL0NyZWF0aW9uRGF0ZShEOjIwMjQwNTA3MTUzMjMxKzAyJzAwJykvUHJvZHVjZXIoaVRleHQgMS40IFwoYnkgbG93YWdpZS5jb21cKSk+PgplbmRvYmoKeHJlZgowIDE0CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMzc5MiAwMDAwMCBuIAowMDAwMDAzNTE1IDAwMDAwIG4gCjAwMDAwMDIwNjQgMDAwMDAgbiAKMDAwMDAwMjMzMiAwMDAwMCBuIAowMDAwMDAyNjAxIDAwMDAwIG4gCjAwMDAwMDIyNDAgMDAwMDAgbiAKMDAwMDAwMjk4OCAwMDAwMCBuIAowMDAwMDAyMTUxIDAwMDAwIG4gCjAwMDAwMDAwMTUgMDAwMDAgbiAKMDAwMDAwNDAzOCAwMDAwMCBuIAowMDAwMDAxODM2IDAwMDAwIG4gCjAwMDAwMDQwOTAgMDAwMDAgbiAKMDAwMDAwNDEzNiAwMDAwMCBuIAp0cmFpbGVyCjw8L0luZm8gMTMgMCBSL0lEIFs8ODllYTdlMzcwZTQ4MTg4Y2Y0ZmMyY2Q5YmEwZTZiODE+PDdhYjY1NDg5YjlmOTdiMDI4MjQxMzhkMjVjMGMxYWUzPl0vUm9vdCAxMiAwIFIvU2l6ZSAxND4+CnN0YXJ0eHJlZgo0MjY2CiUlRU9GCg==</content>
        </accompanyingDocument>
      </accompanyingDocuments>
    </ebdDocumentImportRequest>
  </soapenv:Body>
</soapenv:Envelope>]]></Value>
					</Context>
					<Context>
						<Type>OriginalAttributes</Type>
						<Value><![CDATA[toprot-name : xt-httpclientaddress
toobj : xt-node:/Customs/CH Soap/CH Soap Document Http Client
parseinfoin.std.sender : HYECM2
custom.SourceParty : HYESASCM2
syncflags : 256
fromparty : 
state : 1023
seqidin : 0
datasizeout : 7058
custom.MessageType : EBD
parseinfoin.std.receiver : HYECM2
procduration : 4827
cfgversion : 1591452
msgid : 10074901
internalid : 289631
creationtime : 1717743849
msgtype : 1
sequuidin : 00000000-0000-0000-0000-000000000000
owner : 18446744073709551615
seqnoin : 0
acktimeout : 0
flags : 16
ackprot : 0
fromobj : xt-application:/Framework/Direct Endpoints/HYECM2
msginfo : 
seqtypeout : 0
flagstext : MF_PROC
datahashout : d33eb8f240fde83a1ed1cdac98113551
cw1.certificate : -----BEGIN CERTIFICATE-----
MIIHQTCCBSmgAwIBAgIQPtfP28521UkcmDYgO8dHIzANBgkqhkiG9w0BAQsFADB9
MQswCQYDVQQGEwJDSDEOMAwGA1UEChMFQWRtaW4xETAPBgNVBAsTCFNlcnZpY2Vz
MSIwIAYDVQQLExlDZXJ0aWZpY2F0aW9uIEF1dGhvcml0aWVzMScwJQYDVQQDEx5T
d2lzcyBHb3Zlcm5tZW50IFJlZ3VsYXIgQ0EgMDEwHhcNMjAwMzEyMjEyNDA4WhcN
MjMwMzEyMjEyNDA4WjCBkTELMAkGA1UEBhMCQ0gxOzA5BgNVBAoMMlRoZSBGZWRl
cmFsIEF1dGhvcml0aWVzIG9mIHRoZSBTd2lzcyBDb25mZWRlcmF0aW9uMRQwEgYD
VQQLDAtBbndlbmR1bmdlbjEMMAoGA1UECwwDWktWMSEwHwYDVQQDDBhUZXN0IFJa
MSBXaW4gU0lTQSAwMFAyWTAwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
AQCpZpUSj6ApOlwptEkON+QtT5LoQtTClmRMNUkllDh6eb81drfUGbJKGEnwoZgR
oES+q9mRzHuC5bZvuhe10brIrOQh7j7Hn0NQ8rH+EA5EVJE/F99YW0IK0Ttf2EEa
ezjiu5ACfDhBTBwsITRf7q//w6KWoBWFGpGeuKK43os5KxAgVwm2/Vvhjsg8FcPo
9VyuRubXiRg9HqXFzMvVvziBkHXKZHF+6Unt8gte9xyanHKFDY9APJ2+QDEDugSj
PJA1dDqNxOvdEydeWpGAoegnBCgVRkjcB3cuFhGWF3Wo8SzJ2ZX0rn//PKQF5aVI
cS+kyhhVB0+2nbtvSbB/AD53AgMBAAGjggKmMIICojAfBgNVHSMEGDAWgBRNd7Xk
722cw5ugOofhpu4IpznnizAdBgNVHQ4EFgQU2OsiIsw0PThvCyK+QegDgvZeWksw
DAYDVR0TAQH/BAIwADCBwAYDVR0gBIG4MIG1MIGyBghghXQBEQMWGTCBpTBEBggr
BgEFBQcCARY4aHR0cDovL3d3dy5wa2kuYWRtaW4uY2gvY3BzL0NQU18yXzE2Xzc1
Nl8xXzE3XzNfMjFfMS5wZGYwXQYIKwYBBQUHAgIwURpPVGhpcyBpcyB0aGUgU3dp
c3MgR292ZXJubWVudCBSZWd1bGFyIENBMCAxIENQUyBmb3IgWktWIGF1dGhlbnRp
Y2F0aW9uIHB1cnBvc2VzLjCBxwYDVR0fBIG/MIG8MDGgL6AthitodHRwOi8vd3d3
LnBraS5hZG1pbi5jaC9jcmwvUmVndWxhckNBMDEuY3JsMIGGoIGDoIGAhn5sZGFw
Oi8vYWRtaW5kaXIuYWRtaW4uY2g6Mzg5L2NuPVN3aXNzJTIwR292ZXJubWVudCUy
MFJlZ3VsYXIlMjBDQSUyMDAxLG91PUNlcnRpZmljYXRpb24lMjBBdXRob3JpdGll
cyxvdT1TZXJ2aWNlcyxvPUFkbWluLGM9Q0gwDgYDVR0PAQH/BAQDAgSwMB0GA1Ud
JQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDBDAfBgNVHREEGDAWgRRUZXN0LVJaMS1X
aW5Ac2lzYS5jaDB1BggrBgEFBQcBAQRpMGcwNwYIKwYBBQUHMAKGK2h0dHA6Ly93
d3cucGtpLmFkbWluLmNoL2FpYS9SZWd1bGFyQ0EwMS5jcnQwLAYIKwYBBQUHMAGG
IGh0dHA6Ly93d3cucGtpLmFkbWluLmNoL2FpYS9vY3NwMA0GCSqGSIb3DQEBCwUA
A4ICAQCV5DNo1IKPHJiOfawsaXyWyx2il5ce+LrNc8D2EmBLaSk85YQFWpGY2U2c
kCjYvXG7IXyEARHJDvli8XKsBXjMs5ZLrrQhqj77qKI8v345X8yfdHIVWzcQfjtl
OMlRL8OftbPKoaaQvWhZtaeNOqBDOzimX45vcIusZAlhixFx/2o9zNnclg/HMfck
d2AqyUaIMcSkTt8xJ/05f3ctUccMgZpGn9BKXPlSSbiNSEvc5hzlc6//yW5t8LhD
cfhp8hYjXFrB3KgCcEUMuMr5s71G800v9IVwvTNQMNpkP/v/3L2AG2Ml7SJlhlc6
wYkrbzjvmf8I/Sv3xsk2nZdJrCzRrF8EzxvBBGgapmNULQxfKa/eJ6QyiHkQ8Aj6
KP+wYJf7KJp6mr6O6JrA6vZm67qifeENlG2T8RFq92EaKPCl4+DrufzOejbxRZnC
HQwx28/hHFXMAPSgLLRWRHNS+IQri+CHrEzeLXLGU9D8Ejd75yoF59dB0jGNZLUY
E0YC2E2Pz+SD4ivcdX/yB/zzdUfgCyNcZqsdQr01y+RkGBi1m3/h2Bsc0HRA0S+t
YiFrvdPmhgz2G1gAk1CIOIsJgav4zhkhCyAHAdt/EzpE96FOh/29Ts41ZGjWRevA
15l/4G9pVtUbsFI2RRNwgCnhmMF8p4za2PnuYUOiJdHHi5bx2g==
-----END CERTIFICATE-----

priority : 1
cw1.key : -----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAqWaVEo+gKTpcKbRJDjfkLU+S6ELUwpZkTDVJJZQ4enm/NXa3
1BmyShhJ8KGYEaBEvqvZkcx7guW2b7oXtdG6yKzkIe4+x59DUPKx/hAORFSRPxff
WFtCCtE7X9hBGns44ruQAnw4QUwcLCE0X+6v/8OilqAVhRqRnriiuN6LOSsQIFcJ
tv1b4Y7IPBXD6PVcrkbm14kYPR6lxczL1b84gZB1ymRxfulJ7fILXvccmpxyhQ2P
QDydvkAxA7oEozyQNXQ6jcTr3RMnXlqRgKHoJwQoFUZI3Ad3LhYRlhd1qPEsydmV
9K5//zykBeWlSHEvpMoYVQdPtp27b0mwfwA+dwIDAQABAoIBAAsPdpZCqGtosxHK
AqcaARzwQCBIFMortZrAM+lvNwzQHCrqeHAxyVFLKrC6bEpiU3q7j7YUStAuHW5I
ETo+2lo156NLge/YtzV20yZKeBQADYe4EE4djz9Tj/PfE8kd2IX9RxbSc33X8VL0
TA4jcHxbkqcRBratGGrrlTxK035E9VPfqeMaCu7pjyPlPXH1IxDsIqHxnUNjhvFN
EisfYxSsNU7uR1GVpXorcLPsKMStSqucpJ0Qkw3m7vziiGwItDGGIYz6jdK/QFkK
mPmg40Yfn1b8hXg1wotRux1DTxwS+1ncLRCeD/Gfq36Zxb23tJSxcrUbH1NH/Xft
J4HGV/ECgYEA0WKAojD4a4TTxXhiRlxN94InePO+aJB6Wt8eNQ7ybq1LWObo00Rv
LkarUIFf5bUkxRMjUIM9+w3EOWFO0RmLF3KsTPBilXnB1MWqwCjtXJyCBmAgHM1D
bZKJYkj0Y6c5d9zPSnpbtG6sIB7z21I2BB+u32OCdDN0HMJGl04I6G8CgYEAzx1D
H91e+f9ATWbEV14xeYKPkmEsq6pw0kzOVDy+5Xc4utLgLCV60n71MXsfo34F3ZyN
jzkf5Gkrzj63OSCwcOjuZHwN35z8isGr7ZFEYAvNAecsP0c6kdQ/x/NPrdVSCP4P
NIQ/QgqD4nj3ZGfvh958ei5+MNyGfIUCp1g4vnkCgYBduQCpGNBpmCgOsQcURYbk
rg6rFjd5qIoMGOj1iP49SjZ60ckPoAdAKIPQYkihkoal7B4XkNdyg8P8hzD3ab6X
vU2gls0Z6Uk4y03V+7vbg3a0Edzt53cyGbCDHPK/lH+lxmHHDaP0zMEhE5nF5zOk
fuYQugMPrmRf5xof6d8U4wKBgQC0wfgsXjqe+yu2ocMn8RhC0TTWbyLaa7VthQTq
Nd3xTJzhSFvHi6pm/dCmr6tMOBeMcy6E2jlAUp2dsdSc8i+rS0+LuU0uZMdvnlmb
zBWdsY7pY0WVsGeokFJeArb+otem63x/vA+tZpn11Ncs9RzzOrTuuZb+s+hX0oof
XPqhGQKBgDdbbaWRtkDuDoTgNHdMtz152RStwG0JVnvJW/MilX92s4urj1B+siaw
CNQiLqSjh7e05hIxlmYfhd1bx5opfjIou0zIkcg+1XsW4TiPmhZqCVBv22LNsNgN
24pX1ogLBxgc4VViJeZC72ogc7bSKpi4nn29BHzFnvy269SfQZ0y
-----END RSA PRIVATE KEY-----

seqidout : 0
any.certificate : xt-certificate:{{54dc64c0-e2c2-4d8e-a2c6-90c82c0fe1bd}}
msguuid : d45430e5-a8b2-43b4-9204-73591424be61
toparty : 
knownversion : 0
folderin : 
toprot : 1282
httpclient.url : https://ws.ebd-a.bazg.admin.ch/services/EbdDocumentImportService/v1
fromprot : 903
fromprot-name : xt-application
wfinst : 00000000-0000-0000-0000-000000000000
seqnoout : 0
sequuidout : 00000000-0000-0000-0000-000000000000
syncreply : 0
ackprot-name : xt-none
acklevel : 0
seqtypein : 0
datahashin : d33eb8f240fde83a1ed1cdac98113551
contractobj : xt-contract:/Customs/CH Soap/CH Soap Document Contract
laststate : 5121
statename : ST_END
curracklevel : 0
filenameout : 
custom.MessageTrackingID : 8b346a05-627a-48d1-b907-7ae1e057407d
version : 26
custom.DestinationParty : CHCustomsEbdSOAP
filenamein : 
datasizein : 7058
archiveflags : 17
custom.ApplicationCode : CHC]]></Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>";

	#endregion
}
