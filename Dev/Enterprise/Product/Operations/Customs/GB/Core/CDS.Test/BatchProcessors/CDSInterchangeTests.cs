using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSInterchange))]
	sealed class CDSInterchangeTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<CDSInterchange>();
			AssertEquals(EDIMessage.ApplicationCodes.GbCustomsDeclarationServices, interchange.EI_ApplicationCode);
		}

		public void TestCreateCdsInterchangeFromXML()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""UTF-8""?><SynchronousResponse>
			   <status>202</status>
               <code>ACCEPTED</code>
               <ResponseHeaders>
                              <x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
               </ResponseHeaders>
</SynchronousResponse>
<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""E2680E3226A04FB5919518DDD9406C9F"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>");
			var interchange = CDSInterchange.CreateCdsInterchangeFromXML(Factory, xmlDoc);

			var helper = interchange.CCSUKProcessingInstructionHelper;
			AssertNotNull(helper);
			AssertEquals("CCSUK", helper.SenderId);
			AssertEquals("WISETECHGLOBAL", helper.RecipientId);
			AssertEquals("E2680E3226A04FB5919518DDD9406C9F", helper.ExtCorrelationId);
			AssertEquals(new ZGuid("E2680E3226A04FB5919518DDD9406C9F"), helper.CorrelationId);
			AssertEquals("77685C3D14D53E25E0540003BA9676AB", helper.XConversationId);
			AssertEquals(new ZGuid("77685C3D14D53E25E0540003BA9676AB"), helper.ConversationId);

			var res = interchange.GBCustomsBusinessResponse;
			AssertNotNull(interchange.GBCustomsBusinessResponse);
			AssertEquals(@"<GBCustomsBusinessResponse>
<ResponseHeader>
	<ConversationID>77685c3d14d53e25e0540003ba9676ab</ConversationID>
</ResponseHeader>
<ResponseBody><SynchronousResponse><status>202</status><code>ACCEPTED</code><ResponseHeaders><x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id></ResponseHeaders></SynchronousResponse></ResponseBody>
</GBCustomsBusinessResponse>", res.Xml);
			AssertEquals("77685c3d14d53e25e0540003ba9676ab", res.ConversationId);
			AssertNotNull(res.SynchronousResponse);
			AssertNull(res.MetaData);
			Assert(!res.Responses.Any());

			AssertEquals(EDIInterchange.Status.Received, interchange.EI_ReceiveTransmit);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("CCSUK", interchange.EI_From);
			AssertEquals("WISETECHGLOBAL", interchange.EI_To);
		}
	}
}
