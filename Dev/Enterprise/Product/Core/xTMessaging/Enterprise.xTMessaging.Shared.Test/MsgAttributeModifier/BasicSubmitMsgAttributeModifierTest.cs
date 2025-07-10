using System;
using CargoWise.EntityFramework.Testing;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared.Test
{
	sealed class BasicSubmitMsgAttributeModifierTest : TestCaseWithFactory
	{
		public void TestAddParserFields_6Characters()
		{
			var submitMsg = new SubmitMsgMessage();
			submitMsg.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "destty");
			var modifier = new BasicSubmitMsgAttributeModifier();
			AssertEquals(submitMsg.Parserattr.ContainsKey((int)StdParserFieldId.PfReceiver), false);
			modifier.AddParserFields(submitMsg);
			AssertEquals(submitMsg.Parserattr[(int)StdParserFieldId.PfReceiver], "DESTTY");
		}

		public void TestAddParserFields_9Characters()
		{
			var submitMsg = new SubmitMsgMessage();
			submitMsg.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "destinati");
			var modifier = new BasicSubmitMsgAttributeModifier();
			AssertEquals(submitMsg.Parserattr.ContainsKey((int)StdParserFieldId.PfReceiver), false);
			modifier.AddParserFields(submitMsg);
			AssertEquals(submitMsg.Parserattr[(int)StdParserFieldId.PfReceiver], "DESATI");
		}

		public void TestAddParserFields_Empty()
		{
			var submitMsg = new SubmitMsgMessage();
			submitMsg.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "");
			var modifier = new BasicSubmitMsgAttributeModifier();
			var exception = AssertExceptionThrown<ArgumentException>(() => modifier.AddParserFields(submitMsg));
			AssertEquals(exception.Message, "The Destination Party cannot be null or empty.");
		}

		public void TestAddParserFields_OtherLength()
		{
			var submitMsg = new SubmitMsgMessage();
			submitMsg.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "destination");
			var modifier = new BasicSubmitMsgAttributeModifier();
			var exception = AssertExceptionThrown<ArgumentException>(() => modifier.AddParserFields(submitMsg));
			AssertEquals(exception.Message, "The Destination Party should be 6 or 9 character long CargoWise register code.");
		}

		public void TestAddParserFields_ReceiverTransformer()
		{
			var submitMsg = new SubmitMsgMessage();
			submitMsg.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "destination");
			var modifier = new BasicSubmitMsgAttributeModifier(_ => "abcdef");
			modifier.AddParserFields(submitMsg);
			AssertEquals(submitMsg.Parserattr[(int)StdParserFieldId.PfReceiver], "ABCDEF");
		}

		public void TestAddParserFields_ReceiverValue()
		{
			var submitMsg = new SubmitMsgMessage();
			submitMsg.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "destination");
			var modifier = new BasicSubmitMsgAttributeModifier("abcdef");
			modifier.AddParserFields(submitMsg);
			AssertEquals(submitMsg.Parserattr[(int)StdParserFieldId.PfReceiver], "ABCDEF");
		}
	}
}
