using System;
using System.IO;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.MessageHandler
{
	class ESCustomsMessageHandlerTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var xmlContent = @"<ns0:ESCustoms xmlns:ns0='http://cargowise.com/ehub/products/ESCustoms'>
    <Headers>
        <BrokerCode>BSH</BrokerCode>
        <CertificateName>ESCustomsCER2021</CertificateName>
        <CertificateThumbPrint>62C12914E7E6A3B5681ADE12715AA6399C5DDE70</CertificateThumbPrint>
        <EntryReferenceNumber>ES00000019</EntryReferenceNumber>
        <TestMessage>N</TestMessage>
        <Service>T2LrecepcionV1Service</Service>
        <Operation>T2LrecepcionV1</Operation>
        <SentEDIMessageNumber>19</SentEDIMessageNumber>
        <InterchangeType>T2R</InterchangeType>
    </Headers>
    <Body>
        <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'>
            <soapenv:Header />
            <soapenv:Body>
                <T2LrecepcionV1Sal xmlns:xsd='http://www.w3.org/2001/XMLSchema'
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                    xmlns='https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adtl/T2LrecepcionV1Sal.xsd'>
                    <segmentosDeServicio>
                        <IdentificadorMensaje IdenTran='20210316083935106003' />
                    </segmentosDeServicio>
                    <codigoRespuesta>9020</codigoRespuesta>
                    <descripcionRespuesta>Se esperaba nodo: NIFdelDeclarante</descripcionRespuesta>
                </T2LrecepcionV1Sal>
            </soapenv:Body>
        </soapenv:Envelope>
    </Body>
</ns0:ESCustoms>
";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.ESCustomsMessage);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("ESCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);

			CombineAssertions(() =>
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var handler = new ESCustomsMessageHandler();
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals(ApplicationCodeList.Codes.ESCustomsMessage, interchange.EI_ApplicationCode);
				AssertEquals("T2R", interchange.EI_InterchangeType);
				AssertEquals("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Header /><soapenv:Body><T2LrecepcionV1Sal xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adtl/T2LrecepcionV1Sal.xsd\"><segmentosDeServicio><IdentificadorMensaje IdenTran=\"20210316083935106003\" /></segmentosDeServicio><codigoRespuesta>9020</codigoRespuesta><descripcionRespuesta>Se esperaba nodo: NIFdelDeclarante</descripcionRespuesta></T2LrecepcionV1Sal></soapenv:Body></soapenv:Envelope>", interchange.EI_BodyText);
				AssertEquals("", interchange.EI_FooterText);
				AssertEquals(trackingID, interchange.EI_SessionGUID);
				AssertEquals("ESCustoms", interchange.EI_From);
				AssertEquals("EDIEDIDAT", interchange.EI_To);
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("<Headers><BrokerCode>BSH</BrokerCode><CertificateName>ESCustomsCER2021</CertificateName><CertificateThumbPrint>62C12914E7E6A3B5681ADE12715AA6399C5DDE70</CertificateThumbPrint><EntryReferenceNumber>ES00000019</EntryReferenceNumber><TestMessage>N</TestMessage><Service>T2LrecepcionV1Service</Service><Operation>T2LrecepcionV1</Operation><SentEDIMessageNumber>19</SentEDIMessageNumber></Headers>", interchange.EI_HeaderText);

				AssertEquals(0, interchange.ContainedMessages.Count);
			});
		}
		public void TestCreateInterchangeESCustomsError()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var xmlContent = @"<ns0:ESCustoms xmlns:ns0='http://cargowise.com/ehub/products/ESCustoms'>
  <Headers>
      <BrokerCode>BSH</BrokerCode>
      <CertificateName>ESCustomsCER2021</CertificateName>
      <CertificateThumbPrint>62C12914E7E6A3B5681ADE12715AA6399C5DDE70</CertificateThumbPrint>
      <EntryReferenceNumber>ES00000057</EntryReferenceNumber>
      <TestMessage>Y</TestMessage>
      <Service>T2LrecepcionV1Service</Service>
      <Operation>T2LrecepcionV1</Operation>
      <SentEDIMessageNumber>70</SentEDIMessageNumber>
      <InterchangeType>T2R</InterchangeType>
  </Headers>
  <Body>
    <CommonCustomsServiceError xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://cargowise.com/xhub/products/Customs'>
      <ErrorType>401</ErrorType>
      <ErrorDescription>403: Digital certificate not detected or not selected correctly.</ErrorDescription>
      <ErrorDetail>
        <SourceParty>HYEDESCM2</SourceParty>
        <DestinationParty>ESCustomsSOAP</DestinationParty>
      </ErrorDetail>
      <InboxPK>19ce869f-12ca-4156-9f01-f8c747cc85dd</InboxPK>
      <OutboxPK>f66e9a0d-33be-4d4d-912c-33a0698f5841</OutboxPK>
      <MessageTrackingID>3ef20f73-b9c9-47f7-8354-b0a8581f4bb6</MessageTrackingID>
    </CommonCustomsServiceError>
  </Body>
</ns0:ESCustoms>
";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.ESCustomsMessage);
			message.Setup(m => m.RecipientID).Returns("EDIEDIDAT");
			message.Setup(m => m.SenderID).Returns("ESCustoms");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.ESCustomsError);

			CombineAssertions(() =>
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var handler = HandlerFactory.GetHandler(message.Object.SchemaName);
				handler.SaveMessage(message.Object, GlbCompany.CurrentCompany, new NotificationBuffer());
				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals(ApplicationCodeList.Codes.ESCustomsMessage, interchange.EI_ApplicationCode);
				AssertEquals("ERR", interchange.EI_InterchangeType);
				AssertEquals("<CommonCustomsServiceError xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://cargowise.com/xhub/products/Customs\"><ErrorType>401</ErrorType><ErrorDescription>403: Digital certificate not detected or not selected correctly.</ErrorDescription><ErrorDetail><SourceParty>HYEDESCM2</SourceParty><DestinationParty>ESCustomsSOAP</DestinationParty></ErrorDetail><InboxPK>19ce869f-12ca-4156-9f01-f8c747cc85dd</InboxPK><OutboxPK>f66e9a0d-33be-4d4d-912c-33a0698f5841</OutboxPK><MessageTrackingID>3ef20f73-b9c9-47f7-8354-b0a8581f4bb6</MessageTrackingID></CommonCustomsServiceError>", interchange.EI_BodyText);
				AssertEquals("", interchange.EI_FooterText);
				AssertEquals(trackingID, interchange.EI_SessionGUID);
				AssertEquals("ESCustoms", interchange.EI_From);
				AssertEquals("EDIEDIDAT", interchange.EI_To);
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("<Headers><BrokerCode>BSH</BrokerCode><CertificateName>ESCustomsCER2021</CertificateName><CertificateThumbPrint>62C12914E7E6A3B5681ADE12715AA6399C5DDE70</CertificateThumbPrint><EntryReferenceNumber>ES00000057</EntryReferenceNumber><TestMessage>Y</TestMessage><Service>T2LrecepcionV1Service</Service><Operation>T2LrecepcionV1</Operation><SentEDIMessageNumber>70</SentEDIMessageNumber></Headers>", interchange.EI_HeaderText);

				AssertEquals(0, interchange.ContainedMessages.Count);
			});
		}
	}
}
