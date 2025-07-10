using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	[TestedType(typeof(GenralEdiMessage))]
	class GenralEdiMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			var bo = (GenralEdiMessage)GetNewBusinessObject();
			AssertEquals(ApplicationCodeList.Codes.GbCcsuk, bo.EM_ApplicationCode);
			AssertEquals(GenralMessageGenerator.GenralMessageCodeShortForMessageType, bo.EM_MessageType);
			AssertEquals(GenralPurpose.Codes.Text, bo.EM_MessageSubType);
		}

		public void TestHumanReadableName()
		{
			var bo = (GenralEdiMessage)GetNewBusinessObject();
			bo.EM_MessageNum = "123";
			AssertEquals("GENRAL message 123", bo.HumanReadableName);
		}

		public void TestMakeNewOutboundFromPayload()
		{
			var bo = GenralEdiMessage.MakeNewOutboundFromPayload("My cat's breath smells like catfood", "CUK98FFW000ABC", Factory, "DAN");
			AssertContains("Message text contains our payload.  Note that OTHER tests assert the structure of the edifact, that's not tested here", "CATFOOD", bo.EM_MessageText);
			AssertEquals("CUK98FFW000ABC", bo.EM_ApplicationReference);
			var expectedFragments = new string[]
				{
					"<h3>GENRAL message (outbound)",
					"Recipient: CUK98FFW000ABC",
					"Purpose: TXT",
					"Payload:",
					"<pre><i>My cat's breath smells like catfood</i></pre>"
				};
			foreach (var expectedContentFragment in expectedFragments)
			{
				AssertContains("Message interpretation of ourbound GENRAL message should contain this expected fragment. Have you changed the HTML formatting?", expectedContentFragment, bo.EM_MessageInterpretation);
			}
			AssertEquals(EDIMessageSchema.EM_Status.Name, "QUE", bo.EM_Status);
			AssertEquals(EDIMessageSchema.EM_ReceiveTransmit.Name, "TRX", bo.EM_ReceiveTransmit);
			AssertEquals(EDIMessageSchema.EM_MessageOwner.Name + " (badge)", "DAN", bo.EM_MessageOwner);
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestGetReplyMessage()
		{
			var inboundInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNB+UNOA:2+CUKAIR98LHRCWE:IATA+CUKFFW98000CAR:IATA+120511:1108+944'UNH+950+GENRAL:0:912:UN+14A5F03D618F4FD19195B677DC248663'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++WIS TO CAR'UNT+5+950'UNZ+1+944'");
			Factory.Save();
			var genral = Factory.LoadTop1<GenralEdiMessage>(new ZQuery());
			var reply = genral.GetReplyMessage();
			AssertEquals("CUKFFW98000CAR", reply.SendingProfile);
			AssertEquals("CUKAIR98LHRCWE", reply.Pima);
			AssertEquals("", reply.ShedOrBadge);
			AssertEquals("", reply.Airport);
			AssertEquals(true, reply.IsRecipientRollYourOwn);
			AssertContains("Re: your message 950/944 dated 12-Mar-86 01:02:03", reply.Payload);
		}

		public void TestLinkedMessage()
		{
			var mockMessage1 = Factory.NewMoq<GenralEdiMessageDummyForTest_123>();
			mockMessage1.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			var ediMessage1 = mockMessage1.Object;

			var mockMessage2 = Factory.NewMoq<GenralEdiMessageDummyForTest_1234>();
			mockMessage2.Protected().Setup<string>("GetMessageReferenceNumber").Returns("1234");
			var ediMessage2 = mockMessage2.Object;
			ediMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			ediMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();

			AssertEquals("Message 1 not linked", false, ediMessage1.HasLinkedMessage);
			AssertEquals("Message 2  not linked", false, ediMessage2.HasLinkedMessage);

			ediMessage1.EM_LinkedObject = ediMessage2;
			AssertEquals("Message 1 linked", true, ediMessage1.HasLinkedMessage);
			AssertEquals("Message 1 linked to message 2", ediMessage2.PK, ediMessage1.LinkedMessage.PK);
			AssertEquals("Message 2  not linked", false, ediMessage2.HasLinkedMessage);
			mockMessage1.VerifyAll();
			mockMessage2.VerifyAll();
		}
	}

	[TestedType(typeof(GenralEdiMessageCollection))]
	class GenralEdiMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(GenralEdiMessageCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GenralEdiMessageCollection(Factory);
		}
	}

	public class GenralEdiMessageDummyForTest_123 : GenralEdiMessage
	{
		public GenralEdiMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}

	public class GenralEdiMessageDummyForTest_1234 : GenralEdiMessage
	{
		public GenralEdiMessageDummyForTest_1234(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "1234";
		}
	}
}
