using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BEMessageProcessorHelperTest : TestCase
{
	public void TestOutgoingMessageTypes()
	{
		var messageTypeList = BEMessageProcessorHelper.GetOutgoingMessageTypes().ToList();
		CombineAssertions(() =>
		{
			Assert("OutgoingMessageTypes should contain NCT", messageTypeList.Contains(SendMessageTypes.Codes.NCT));
			Assert("OutgoingMessageTypes should contain AES", messageTypeList.Contains(SendMessageTypes.Codes.AES));
		});
	}

	public void TestGetMessageDomainCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Domain Code for NCT", "NCTSP5", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.NCT));
			AssertEquals("Domain Code for AES", "AES", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.AES));
			AssertEquals("Domain Code for PN", "PN/TS", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.PN));
			AssertEquals("Domain Code for TS", "PN/TS", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.TS));
			AssertEquals("Domain Code for IMP", "IDMS", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.IMP));
			AssertEquals("Domain Code for TSD", "TSD", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.TSD));
			AssertEquals("Domain Code for REN", "REN", BEMessageProcessorHelper.GetMessageDomainCode(SendMessageTypes.Codes.REN));
			AssertEquals("Default Domain Code is empty", ZString.Empty, BEMessageProcessorHelper.GetMessageDomainCode("test"));
		});
	}
}
