using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAUniversalXMLInterchange))]
	sealed class CAUniversalXMLInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShouldSendVicEHubCore()
		{
			var testInterchange = Factory.New<CAUniversalXMLInterchange>();
			Assert(testInterchange.ShouldSendViaEHub);
		}

		[TestDate(2016, 01, 01)]
		public void TestInterchangeNumberAndHeader()
		{
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIEPID");

			CombineAssertions("Test", () =>
			{
				var testInterchange = Factory.New<CAUniversalXMLInterchange>();
				testInterchange.EI_To = "CACustomsTest";
				var testMessage = Factory.New<EDIMessage>();
				testInterchange.EI_BodyText = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID><<SenderNetworkIDPlaceHolder>></SenderID><RecipientID><<RecipientNetworkIDPlaceHolder>></RecipientID><InterchangeNumber><<INTERCHANGENUMBERPLACEHOLDER>></InterchangeNumber></Header><Body><root>Message 2 text</root></Body></UniversalInterchange>";
				testInterchange.ContainedMessages.Add(testMessage);
				Factory.Save();

				var testInterchangeReload = new BusinessObjectFactory().Load<EDIInterchange>(testInterchange.PK);
				AssertEquals("20160101000000000000000001", testInterchangeReload.EI_InterchangeNum);
				AssertEquals("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID>CLIEPID</SenderID><RecipientID>CBSATID</RecipientID><InterchangeNumber>1</InterchangeNumber></Header><Body><root>Message 2 text</root></Body></UniversalInterchange>", testInterchangeReload.EI_BodyText);
			});
			CombineAssertions("Test2", () =>
			{
				var testInterchange = Factory.New<CAUniversalXMLInterchange>();
				testInterchange.EI_To = "CACustomsTest";
				var testMessage = Factory.New<EDIMessage>();
				testInterchange.EI_BodyText = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID></SenderID><RecipientID><<RecipientNetworkIDPlaceHolder>></RecipientID><InterchangeNumber><<INTERCHANGENUMBERPLACEHOLDER>></InterchangeNumber></Header><Body><root>Message 2 text</root></Body></UniversalInterchange>";
				testInterchange.ContainedMessages.Add(testMessage);
				Factory.Save();

				var testInterchangeReload = new BusinessObjectFactory().Load<EDIInterchange>(testInterchange.PK);
				AssertEquals("20160101000000000000000002", testInterchangeReload.EI_InterchangeNum);
				AssertEquals("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID></SenderID><RecipientID>CBSATID</RecipientID><InterchangeNumber>2</InterchangeNumber></Header><Body><root>Message 2 text</root></Body></UniversalInterchange>", testInterchangeReload.EI_BodyText);
			});

			var mockRegistry = new Mock<IProductRegistration>();
			var mockRegoKeyRegistry = new Mock<IProductRegistrationKey>();
			mockRegoKeyRegistry.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			mockRegistry.Setup(m => m.Key).Returns(mockRegoKeyRegistry.Object);
			ObjectFactory.Substitute(mockRegistry.Object);

			CombineAssertions("Prod", () =>
			{
				var testInterchange = Factory.New<CAUniversalXMLInterchange>();
				testInterchange.EI_To = "CACustoms";
				var testMessage = Factory.New<EDIMessage>();

				testInterchange.EI_BodyText = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID><<SenderNetworkIDPlaceHolder>></SenderID><RecipientID><<RecipientNetworkIDPlaceHolder>></RecipientID><InterchangeNumber><<INTERCHANGENUMBERPLACEHOLDER>></InterchangeNumber></Header><Body><root>Message 2 text</root></Body></UniversalInterchange>";
				testInterchange.ContainedMessages.Add(testMessage);
				Factory.Save();

				var testInterchangeReload = new BusinessObjectFactory().Load<EDIInterchange>(testInterchange.PK);
				AssertEquals("20160101000000000000000001", testInterchangeReload.EI_InterchangeNum);
				AssertEquals("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Header><SenderID>CLIEPID</SenderID><RecipientID>CBSAPID</RecipientID><InterchangeNumber>1</InterchangeNumber></Header><Body><root>Message 2 text</root></Body></UniversalInterchange>", testInterchangeReload.EI_BodyText);
			});
		}

		public void TestReplaceBatchNumberPlaceHolder()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDZZ"))
			using (CACustomsDataRegistry.Instance.ShouldBatchNumberBeByInterchange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testInterchange = Factory.New<CAUniversalXMLInterchange>();
				testInterchange.EI_To = "CACustoms";
				var testMessage = Factory.New<EDIMessage>();
				testMessage.EM_MessageText = "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
				testMessage.EM_IsTestMessage = true;
				testInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				testInterchange.EI_BodyText = "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
				testInterchange.ContainedMessages.Add(testMessage);
				Factory.Save();
				AssertEquals("Hello, 001 Bye", testMessage.EM_MessageText);
				AssertEquals("Hello, 001 Bye", testInterchange.EI_BodyText);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIEPID");
		}
	}
}
