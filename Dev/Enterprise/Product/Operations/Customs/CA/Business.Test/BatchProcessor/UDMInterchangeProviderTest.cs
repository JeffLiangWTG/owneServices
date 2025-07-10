using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class UDMInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestDate(2008, 8, 27, 12, 6, 25)]
		public override void TestMessagesPopulateNewInterchange()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var company1 = Factory.New<GlbCompany>();
			var branch1_1 = company1.Branches.AddNew();
			var branch1_2 = company1.Branches.AddNew();
			var company2 = Factory.New<GlbCompany>();
			var branch2_1 = company2.Branches.AddNew();

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message1.EM_GB = branch1_1.PK;
			message1.EM_MessageText = @"<root>Message 1 text</root>";

			var message2 = collection.AddNew();
			message2.EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message2.EM_GB = branch1_1.PK;
			message2.EM_MessageText = @"<root>Message 2 text</root>";

			var message3 = collection.AddNew();
			message3.EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message3.EM_GB = branch1_2.PK;
			message3.EM_MessageText = @"<root>Message 3 text</root>";

			var message4 = collection.AddNew();
			message4.EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message4.EM_GB = branch2_1.PK;
			message4.EM_MessageText = @"<root>Message 4 text</root>";

			var interchanges = new CAUDMInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message3.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message4.EM_Status);

			AssertEquals(4, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			}

			string expectedBodyText1 = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID><<SenderNetworkIDPlaceHolder>></SenderID><RecipientID><<RecipientNetworkIDPlaceHolder>></RecipientID><InterchangeNumber><<INTERCHANGENUMBERPLACEHOLDER>></InterchangeNumber></Header><Body><root>Message 1 text</root></Body></UniversalInterchange>";
			string expectedBodyText2 = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID><<SenderNetworkIDPlaceHolder>></SenderID><RecipientID><<RecipientNetworkIDPlaceHolder>></RecipientID><InterchangeNumber><<INTERCHANGENUMBERPLACEHOLDER>></InterchangeNumber></Header><Body><root>Message 3 text</root></Body></UniversalInterchange>";
			string expectedBodyText3 = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID><<SenderNetworkIDPlaceHolder>></SenderID><RecipientID><<RecipientNetworkIDPlaceHolder>></RecipientID><InterchangeNumber><<INTERCHANGENUMBERPLACEHOLDER>></InterchangeNumber></Header><Body><root>Message 4 text</root></Body></UniversalInterchange>";

			var interchange1 = interchanges.First(ei => ei.EI_GB == branch1_1.PK && ei.EI_To == "CACustomsTest");
			AssertEquals("EI_From", company1.LicenceKeyIdentifier, interchange1.EI_From);
			AssertMultilineASCIIEquals("EI_BodyText", expectedBodyText1, interchange1.EI_BodyText);
			var interchange2 = interchanges.First(ei => ei.EI_GB == branch1_2.PK && ei.EI_To == "CACustomsTest");
			AssertEquals("EI_From", company1.LicenceKeyIdentifier, interchange2.EI_From);
			AssertMultilineASCIIEquals("EI_BodyText", expectedBodyText2, interchange2.EI_BodyText);
			var interchange3 = interchanges.First(ei => ei.EI_GB == branch2_1.PK && ei.EI_To == "CACustomsTest");
			AssertEquals("EI_From", company2.LicenceKeyIdentifier, interchange3.EI_From);
			AssertMultilineASCIIEquals("EI_BodyText", expectedBodyText3, interchange3.EI_BodyText);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new CAUDMInterchangeProvider(collection);
		}
	}
}
