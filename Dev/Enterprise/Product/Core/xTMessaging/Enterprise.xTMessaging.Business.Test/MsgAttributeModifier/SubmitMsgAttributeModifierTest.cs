using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Shared;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Business.Test
{
	sealed class SubmitMsgAttributeModifierTest : TestCase
	{
		public void TestAddParserFields()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var testMessage = new SubmitMsgMessage { Msgattr = { [Constants.CustomMsgAttributes.DestinationParty] = "ENTTST" } };
			var tester = new SubmitMsgAttributeModifier();
			tester.AddParserFields(testMessage);
			AssertMessage(testMessage, "ENTTST");
		}

		public void TestAddParserFields_EmptyDestinationParty()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var testMessage = new SubmitMsgMessage();
			var tester = new SubmitMsgAttributeModifier();
			tester.AddParserFields(testMessage);
			AssertMessage(testMessage, "");
		}

		public void TestAddParserFields_DestinationPartyWithSpace()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var testMessage = new SubmitMsgMessage { Msgattr = { [Constants.CustomMsgAttributes.DestinationParty] = " ENTTST " } };
			var tester = new SubmitMsgAttributeModifier();
			tester.AddParserFields(testMessage);
			AssertMessage(testMessage, "ENTTST");
		}

		void AssertMessage(SubmitMsgMessage messageToBeAssertted, string expectedEI_To)
		{
			var parseAttrForTest = messageToBeAssertted.Parserattr;
			AssertEquals(2, parseAttrForTest.Count);
			AssertEquals(true, parseAttrForTest.ContainsKey((int)StdParserFieldId.PfSender));
			AssertEquals(true, parseAttrForTest.ContainsKey((int)StdParserFieldId.PfReceiver));
			AssertEquals("ENTSVR", parseAttrForTest[(int)StdParserFieldId.PfSender]);
			AssertEquals(expectedEI_To, parseAttrForTest[(int)StdParserFieldId.PfReceiver]);
		}
	}
}
