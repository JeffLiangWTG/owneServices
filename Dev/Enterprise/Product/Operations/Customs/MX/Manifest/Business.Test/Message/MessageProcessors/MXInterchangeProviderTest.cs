using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class MXInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new MXInterchangeProvider(new LoggingInformation(), collection);
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var message1 = CreateAndPopulateMessage(MessageTypes.Codes.MXA) as TestEdiMessage;
			var message2 = CreateAndPopulateMessage(MessageTypes.Codes.MXA) as TestEdiMessage;
			var message3 = CreateAndPopulateMessage(MessageTypes.Codes.MXD) as TestEdiMessage;
			var message4 = CreateAndPopulateMessage(MessageTypes.Codes.MXE) as TestEdiMessage;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message1, message2, message3, message4 });

			var logger = new LoggingInformation();
			var provider = new MXInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();

			message1.Reload();
			message2.Reload();
			message3.Reload();
			message4.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 4, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message2.EM_EI, message3.EM_EI);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message3.EM_EI, message4.EM_EI);

				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI);
				var interchange3 = interchanges.FirstOrDefault(x => x.PK == message3.EM_EI);
				var interchange4 = interchanges.FirstOrDefault(x => x.PK == message4.EM_EI);

				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);

				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);

				AssertNotNull("Interchange 3 is linked to message 3", interchange3);
				AssertEquals("Message 3 is sent", EDIMessage.Status.Sent, message3.EM_Status);

				AssertNotNull("Interchange 4 is linked to message 4", interchange4);
				AssertEquals("Message 4 is sent", EDIMessage.Status.Sent, message4.EM_Status);

				AssertEquals("EI_GP", message1.EM_GP, interchange1.EI_GP);
				AssertEquals("EI_GP", message2.EM_GP, interchange2.EI_GP);
			});
		}

		public void TestDoNotSendInterchangeWithEmptyBody()
		{
			var message = CreateAndPopulateMessage(MessageTypes.Codes.MXD) as TestEdiMessage;
			message.EM_MessageText = "";

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new MXInterchangeProvider(logger, messageCollection);

			AssertEquals(0, provider.Interchanges.Length);
			AssertContains("The Interchange Body is empty even though there are 1 messages.", logger.UserLogStrings[0]);

			message.EM_MessageText = "MESSAGE TEXT";
			provider = new MXInterchangeProvider(logger, messageCollection);
			AssertEquals(1, provider.Interchanges.Length);
		}

		public void TestInterchangeType()
		{
			var message = CreateAndPopulateMessage(MessageTypes.Codes.MXA) as TestEdiMessage;

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new MXInterchangeProvider(logger, messageCollection);
			Factory.Save();

			AssertType(typeof(MXInterchange), provider.Interchanges.Single());
		}

		public void TestNewSeaInterchangeWithEnvelope()
		{
			var credential = Business.GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "USERNAME";
			credential.GP_CurrentPassword = "PASSWORD";

			GlbCompany.CurrentCompany.Factory.Save();

			var message = CreateAndPopulateMessage(MessageTypes.Codes.MXA) as TestEdiMessage;
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message });

			var logger = new LoggingInformation();
			var provider = new MXInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();
			message.Reload();

			AssertEquals("Number Of Interchanges", 1, interchanges.Length);

			var interchange1 = interchanges.FirstOrDefault(x => x.PK == message.EM_EI);

			AssertNotNull("Interchange 1 is linked to message 1", interchange1);
			AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message.EM_Status);
			Assert("Message with Envelope", interchange1.EI_BodyText.Contains("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ven=\"http://www.ventanillaunica.gob.mx\" xmlns:man=\"http://www.ventanillaunica.gob.mx/ManifiestoMaritimo309SO\">"));
		}

		public void TestNewAirInterchangeWithEnvelope()
		{
			var testItem = new MXWsVucem(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSResponse = "http://127.0.0.1/wsdl?";
			testItem.AirModeWSUsername = "ADMINVUCEM1";
			testItem.AirModeWSPassword = "9974567891";
			MXCustomsDataRegistry.Instance.WSVucem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testItem);

			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "USERNAME";
			credential.GP_CurrentPassword = "PASSWORD";

			GlbCompany.CurrentCompany.Factory.Save();

			var message = CreateAndPopulateMessage(MessageTypes.Codes.MXE) as TestEdiMessage;
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message });

			var logger = new LoggingInformation();
			var provider = new MXInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();
			message.Reload();

			AssertEquals("Number Of Interchanges", 1, interchanges.Length);

			var interchange1 = interchanges.FirstOrDefault(x => x.PK == message.EM_EI);
			AssertNotNull("Interchange 1 is linked to message 1", interchange1);
			AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message.EM_Status);

			Assert("Message with Envelope", interchange1.EI_BodyText.Contains("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ser=\"http://www.service.ws.recepcion.aereos.privados.www.ventanillaunica.gob.mx\">"));
		}

		public static string Unzip(byte[] compressed)
		{
			string ret = null;
			using (var inputMemory = new MemoryStream(compressed))
			{
				using (var gz = new GZipStream(inputMemory, CompressionMode.Decompress))
				{
					using (var sr = new StreamReader(gz, Encoding.UTF8))
					{
						ret = sr.ReadToEnd();
					}
				}
			}
			return ret;
		}

		public EDIMessage CreateAndPopulateMessage(ZString messageType)
		{
			var message = Factory.New<TestEdiMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			message.EM_ApplicationReference = "REF";
			message.EM_EI = new ZGuid();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageOwner = "OWNER";
			message.EM_MessageText = "MESSAGE TEXT";
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GP = new ZGuid();

			return message;
		}
	}
}
