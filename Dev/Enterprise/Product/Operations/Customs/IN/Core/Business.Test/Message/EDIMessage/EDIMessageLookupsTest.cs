using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(EDIMessageLookups))]
sealed class EDIMessageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageTypeList()
	{
		var message = Factory.New<EDIMessage>();
		var messageTypeList = message.Lookups.MessageTypeList;
		AssertContainsExactElementsInAnyOrder("MessageTypeList values", new[] { "CGM" ,"SB" }, messageTypeList.GetAllCodes());
		AssertSame("MessageTypeList cached", messageTypeList, message.Lookups.MessageTypeList);
	}
}
