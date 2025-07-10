using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class MessageSubTypesTest : TestCase
	{
		public void TestCode()
		{
			AssertEquals("Original", AirCargoMessage.MessageSubType.Original, MessageSubTypes.Original.Code);
			AssertEquals("Amendment", AirCargoMessage.MessageSubType.Amendment, MessageSubTypes.Amendment.Code);
			AssertEquals("PartShipment", AirCargoMessage.MessageSubType.PartShipment, MessageSubTypes.PartShipment.Code);
			AssertEquals("Withdraw", AirCargoMessage.MessageSubType.Withdraw, MessageSubTypes.Withdraw.Code);
			AssertEquals("Underbond Request", AirCargoMessage.MessageSubType.UnderbondRequest, MessageSubTypes.UnderbondRequest.Code);
			AssertEquals("Underbond Acquittal", AirCargoMessage.MessageSubType.UnderbondAcquittal, MessageSubTypes.UnderbondAcquittal.Code);
			AssertEquals("Underbond Cancel", AirCargoMessage.MessageSubType.UnderbondCancel, MessageSubTypes.UnderbondCancel.Code);
		}
	}
}
