using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AtlasEDIMessage))]
	sealed class AtlasEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			AssertEquals(EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
		}

		public void TestFillInPlaceHolders()
		{
			message.EM_MessageText = $"<InterchangeControlReference>{EDIInterchange.InterchangeNumberPlaceHolderHtml}</InterchangeControlReference><MessageIdentifier>{EDIMessage.SendersReferencePlaceHolderHtml}</MessageIdentifier>";
			Factory.Save();
			AssertContains(@"<InterchangeControlReference>00000000000001</InterchangeControlReference><MessageIdentifier>EDIEDIDAT00000000000001</MessageIdentifier>", message.EM_MessageText);
		}

		public void TestMessageNum()
		{
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("First Message", "EDIEDIDAT00000000000001", message.EM_MessageNum);

				var message2 = Factory.New<AtlasEDIMessage>();
				Factory.Save();
				AssertEquals("Second Message", "EDIEDIDAT00000000000002", message2.EM_MessageNum);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<AtlasEDIMessage>();
		}
		AtlasEDIMessage message;
	}
}
