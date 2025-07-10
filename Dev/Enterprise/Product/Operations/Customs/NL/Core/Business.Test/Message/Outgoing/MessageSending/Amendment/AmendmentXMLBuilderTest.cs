//using System.Xml.Linq;
//using CargoWise.EntityFramework.Testing;
//using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
//using Enterprise.Customs.NL.Business.Message.Wrappers.Testing;
//using NUnit.Framework;

//namespace Enterprise.Customs.NL.Business.Testing
//{
//	sealed class AmendmentXMLBuilderTest : TestCaseWithFactory
//	{
//		[TestDate(2022, 02, 24, 15, 48, 23)]
//		public void TestMakeAmendmentXml()
//		{
//			var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
//			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
//			CombineAssertions(() =>
//			{
//				var expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedMessageMakeAmendmentXml.xml");
//				AssertXMLContains(GetXMLMessage(expectedMessage), GetMessageWithAmendmentDetails(messageSendingObject));

//				expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedMessageMakeAmendmentXmlWithEmptyDetails.xml");
//				AssertXMLContains(GetXMLMessage(expectedMessage), GetMessageWithEmptyAmendmentDetails(messageSendingObject));
//			});
//		}

//		string GetMessageWithAmendmentDetails(JobDeclarationMessageSendingObject messageSendingObject)
//		{
//			var testXML = new XElement("Declaration");
//			testXML.Add("ID", "MRN123");
//			var customsValuationXML = new XElement("CustomsValuation");
//			customsValuationXML.Add("FreightChargeAmount", 25.5);
//			customsValuationXML.Add(new XAttribute("currencyId", "EUR"));
//			testXML.Add(customsValuationXML);
//			messageSendingObject.AmendmentDetails = new AmendmentDetails(null, "", "");
//			messageSendingObject.AmendmentDetails.Amendments = new AmendmentObjectWrapper { Xml = testXML };
//			var builder = new AmendmentXMLBuilder(messageSendingObject);
//			var result = builder.CreateMessage(false);

//			return GetXMLMessage(result);
//		}

//		string GetMessageWithEmptyAmendmentDetails(JobDeclarationMessageSendingObject messageSendingObject)
//		{
//			messageSendingObject.AmendmentDetails = null;
//			var builder = new AmendmentXMLBuilder(messageSendingObject);
//			var result = builder.CreateMessage(false);

//			return GetXMLMessage(result);
//		}

//		string GetXMLMessage(string message) => message.Replace(" ", "").Replace("\r\n", "");
//	}
//}
