using CargoWise.Customs.IL.MessageDefinitions.DLO.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.Customs.IL.MessageDefinitions.GEN.RES_910.NG_9101_MSG_OutgoingMessageResponse;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestToCustomsDateTimeString()
		{
			AssertEquals("2024-05-16T15:21:44", new ZDateTime(2024, 05, 16, 15, 21, 44).ToCustomsDateTimeString());
		}

		public void TestHasDefaultNamespace_WithCustomPrefix()
		{
			var interchangeMessage = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath($"OutgoingMessageResponse_9101_WhenHasQ1Prefix.xml"));

			var res = Extensions.HasDefaultNamespace<Ng9101MsgOutgoingMessageResponse>(interchangeMessage);

			Assert("Has default namespace", res);
		}

		public void TestHasDefaultNamespace_WithDefaultPrefix()
		{
			var interchangeMessage = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath($"DeliveryOrderResponse_1220WithExceptionInHeader.xml"));

			var res = Extensions.HasDefaultNamespace<MnNg1220Msg22DeliveryOrderFeedBackMessage>(interchangeMessage);

			Assert("Has default namespace", res);
		}

		public void TestHasDefaultNamespace_NoDefaultNamespace()
		{
			var interchangeMessage = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath($"OutgoingMessageResponse_9101_WhenAllMessagesNotMapped.xml"));

			var res = Extensions.HasDefaultNamespace<Ng9101MsgOutgoingMessageResponse>(interchangeMessage);

			Assert("Has no default namespace", !res);
		}
	}
}
