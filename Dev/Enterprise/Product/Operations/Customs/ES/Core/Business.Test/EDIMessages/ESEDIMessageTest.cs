using System.IO;
using CargoWise.Data;
using Enterprise.Customs.ES.Business.EDIInterchanges;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Environment;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.ES.Business.EDIMessages.Testing
{
	[TestedType(typeof(ESEDIMessage))]
	public class ESEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<ESEDIMessage>();
			AssertEquals("EM_ApplicationCode", ApplicationCodes.ESCustomsMessage, message.EM_ApplicationCode);
		}

		public void TestEDIInterchange()
		{
			CombineAssertions(() =>
			{
				var message = Factory.New<ESEDIMessage>();
				AssertNull(message.Interchange);

				var interchange = Factory.New<Enterprise.Messaging.Business.EDIInterchange>();
				message.EM_EI = interchange.PK;
				AssertNotNull(message.Interchange);
				AssertType(typeof(ESEDIInterchange), message.Interchange);
				AssertEquals("Interchange.ShouldSendViaEHub is correct", true, message.Interchange.ShouldSendViaEHub);
			});
		}

		public void TestGetMessageReferenceNumber()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				CombineAssertions(() =>
				{
					var message = Factory.New<ESEDIMessage>();
					Factory.Save();
					AssertEquals("1", message.EM_MessageNum);

					var message2 = Factory.New<ESEDIMessage>();
					Factory.Save();
					AssertEquals("2", message2.EM_MessageNum);

					var expectedNumberFountain = Env.NumberFountains.ESCustomsEDIFACTNumberFountain("M", ApplicationCodes.ESCustomsMessage);
					expectedNumberFountain.SetNext(Factory, 13);

					var message3 = Factory.New<ESEDIMessage>();
					Factory.Save();
					AssertEquals("13", message3.EM_MessageNum);
					var message4 = Factory.New<ESEDIMessage>();
					Factory.Save();
					AssertEquals("14", message4.EM_MessageNum);
				});
			}
		}

		public void TestGetNumberFountainNumbersAndFillInPlaceHolders_ExportAmendment()
		{
			var businessObjectReference = "Reference";

			var messageText = @"UNB+UNOA:1+ESDEC33333333:ZZ+AEATADUE:ZZ+200109:1513+1++&EE++++1'
UNH+1+CUSDEC:1:921:UN:ECS003'
BGM+830+<<EXPORT_AMENDMENT_LOCAL_REF_NUMBER_PLACE_HOLDER>>+33'
CST++EX:104:141+A:105:141++:112:141+9999:113:148'
LOC+42+ES::141:009999'
GIS+0:109:141'
NAD+EX+SUP22222222:P:148++Supplier Test Org+1234 Test Street+Barcelona++98765+ES'
NAD+CN+ESIMP11111111::148++Importer Test Org+Test Street 1234+Madrid++12345+ES'
NAD+2+ESDEC33333333::148+mail.mail@mail.com+Declarant Test Org:::::O'
TOD+++FOB:106'
MOA+ZZZ::EUR'
UNS+D'
CST+1+2203001010FirstSecond:122:148+12.34:117:141+001:117:148'
FTX+AAA+++Description1'
UNS+S'
CNT+5:1'
CNT+11:0'
UNT+17+1'
UNZ+1+1'";

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				CombineAssertions(() =>
				{
					var message = Factory.New<ESEDIMessage>();
					message.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;
					message.BusinessObjectReference = businessObjectReference;
					message.EM_MessageText = messageText;
					Factory.Save();
					AssertContains("+Reference_1+", message.EM_MessageText);

					var message2 = Factory.New<ESEDIMessage>();
					message2.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;
					message2.BusinessObjectReference = businessObjectReference;
					message2.EM_MessageText = messageText;
					Factory.Save();
					AssertContains("+Reference_2+", message2.EM_MessageText);

					var expectedNumberFountain = Env.NumberFountains.ESBGMLocalReferenceSuffix(businessObjectReference);
					expectedNumberFountain.SetNext(Factory, 13);

					var message3 = Factory.New<ESEDIMessage>();
					message3.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;
					message3.BusinessObjectReference = businessObjectReference;
					message3.EM_MessageText = messageText;
					Factory.Save();
					AssertContains("+Reference_13+", message3.EM_MessageText);

					var message4 = Factory.New<ESEDIMessage>();
					message4.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendment;
					message4.BusinessObjectReference = businessObjectReference;
					message4.EM_MessageText = messageText;
					Factory.Save();
					AssertContains("+Reference_14+", message4.EM_MessageText);
				});
			}
		}
		public void TestESFormattedMessageText()
		{
			ESEDIMessage message = Factory.New<ESEDIMessage>();
			message.EM_MessageText = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand?";
			Factory.Save();
			message.Refresh();

			var expectedFormattedText = "This \r is \n is \t a test   message\r\n. This:format+I don\r\nt\r\n understand?";

			CombineAssertions(() =>
			{
				AssertEquals("Test EM_FormattedMessageText", expectedFormattedText, message.EM_FormattedMessageText);
				using (TextReader reader = message.EM_FormattedMessageTextReader)
				{
					string output = reader.ReadToEnd();
					AssertEquals("Test EM_FormattedMessageTextReader", expectedFormattedText, output);
				}
			});
		}

		public void TestESFormatterMessageTextXML()
		{
			ESEDIMessage message = Factory.New<ESEDIMessage>();
			message.EM_MessageText = "<?xml version=\"1.0\" ?><soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Header/><soapenv:Body Id=\"Body\"><T2LanexosV1Sal xmlns=\"https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adtl/T2LanexosV1Sal.xsd\"><segmentosDeServicio><IdentificadorMensaje IdenTran=\"20200508152649206061\"/></segmentosDeServicio><codigoRespuesta>7002</codigoRespuesta><descripcionRespuesta>El fichero que se anexa no tiene una extensión válida según se indica en tabla REGDFORM.</descripcionRespuesta></T2LanexosV1Sal></soapenv:Body></soapenv:Envelope>";
			Factory.Save();
			message.Refresh();

			var expectedFormattedText = "<?xml version=\"1.0\" ?>\r\n<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n<soapenv:Header/>\r\n<soapenv:Body Id=\"Body\">\r\n<T2LanexosV1Sal xmlns=\"https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adtl/T2LanexosV1Sal.xsd\">\r\n<segmentosDeServicio>\r\n<IdentificadorMensaje IdenTran=\"20200508152649206061\"/>\r\n</segmentosDeServicio>\r\n<codigoRespuesta>7002</codigoRespuesta>\r\n<descripcionRespuesta>El fichero que se anexa no tiene una extensión válida según se indica en tabla REGDFORM.</descripcionRespuesta>\r\n</T2LanexosV1Sal>\r\n</soapenv:Body>\r\n</soapenv:Envelope>";

			CombineAssertions(() =>
			{
				AssertEquals("Test EM_FormattedMessageText", expectedFormattedText, message.EM_FormattedMessageText);
				using (TextReader reader = message.EM_FormattedMessageTextReader)
				{
					string output = reader.ReadToEnd();
					AssertEquals("Test EM_FormattedMessageTextReader", expectedFormattedText, output);
				}
			});
		}
	}
}
