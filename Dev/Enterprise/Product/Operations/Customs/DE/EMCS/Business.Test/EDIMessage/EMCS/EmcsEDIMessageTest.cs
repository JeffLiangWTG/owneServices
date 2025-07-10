using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EmcsEDIMessage))]
	public class EmcsEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsEmcsSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", DE.Messaging.EDIMessageTypeList.Codes.EMCS, message.EM_MessageType);
			});
		}

		public void TestMessageNum()
		{
			CombineAssertions(() =>
			{
				var prefix = GlbCompany.CurrentCompany.GC_Code;
				Factory.Save();
				AssertEquals("1st Generated Number", $"{prefix}00000000001", message.EM_MessageNum);

				var message2 = Factory.New<EmcsEDIMessage>();
				Factory.Save();
				AssertEquals("2nd Generated Number", $"{prefix}00000000002", message2.EM_MessageNum);
			});
		}

		public void TestFillInPlaceHolders()
		{
			var messageReferenceNumber = $"{GlbCompany.CurrentCompany.GC_Code}00000000001";
			message.EM_MessageText = $"<InterchangeControlReference>{EDIInterchange.InterchangeNumberPlaceHolderHtml}</InterchangeControlReference><MessageIdentifier>{EDIMessage.SendersReferencePlaceHolderHtml}</MessageIdentifier>";
			Factory.Save();
			AssertContains($@"<InterchangeControlReference>00000000000001</InterchangeControlReference><MessageIdentifier>{messageReferenceNumber}</MessageIdentifier>", message.EM_MessageText);
		}

		public void TestEM_LinkedObject_Declaration()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			message.EM_LinkTable = declaration.TableName;
			message.EM_LinkUniqueID = declaration.PK;
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestEM_MessageInterpretation()
		{
			message.EM_MessageInterpretation = "<Node>Test text</Node>";
			AssertContains("Interpretation should be encoded to html format", "&lt;Node&gt;Test text&lt;/Node&gt;", message.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EmcsEDIMessage>();
		}

		EmcsEDIMessage message;
	}
}
