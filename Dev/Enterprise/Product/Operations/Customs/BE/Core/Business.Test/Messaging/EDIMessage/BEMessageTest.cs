using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(BEMessage))]
class BEMessageTest : EDIMessageTest
{
	public void TestMessageNumberStrategy()
	{
		var number = Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", Messaging.Business.EDIInterchange.ApplicationCodes.BECustoms).PeekPreliminaryFormatted(Factory);
		var messageNotHavingMessageNum = Factory.New<BEMessage>();
		Factory.Save();
		AssertEquals(number, messageNotHavingMessageNum.EM_MessageNum);

		var messageHavingMessageNum = Factory.New<BEMessage>();
		messageHavingMessageNum.EM_MessageNum = "100";
		Factory.Save();
		AssertEquals("100", messageHavingMessageNum.EM_MessageNum);
	}

	public void TestSetDefaultValues()
	{
		AssertEquals("EM_ApplicationCode", Messaging.Business.EDIInterchange.ApplicationCodes.BECustoms, Factory.New<BEMessage>().EM_ApplicationCode);
	}
	public void TestSendersReferencePlaceHolderOverride()
	{
		var message = Factory.New<BEMEssageForTest>();
		AssertEquals(BEMessage.SendersReferencePlaceHolderHtml, message.SendersReferencePlaceHolderOverrideExposed);
	}

	public void TestGetMessageReferenceNumber()
	{
		var message = Factory.New<BEMEssageForTest>();
		message.EM_MessageNum = "48";
		AssertEquals("48", message.GetSendersReferenceExposed());
	}

	public void TestShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
	{
		var message = Factory.New<BEMEssageForTest>();
		AssertEquals(true, message.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverrideExposed());
	}

	class BEMEssageForTest : BEMessage
	{
		public BEMEssageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string SendersReferencePlaceHolderOverrideExposed => base.SendersReferencePlaceHolderOverride;

		public string GetSendersReferenceExposed() => base.GetSendersReference();

		public ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverrideExposed() => base.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride();
	}
}
