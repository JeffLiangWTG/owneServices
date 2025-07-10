using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class EUICS2MessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestGetMessageBuilder()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			CombineAssertions(() =>
			{
				AssertGetMessageBuilder<F10MessageBuilder>(MessageTypes.Codes.F10);
				AssertGetMessageBuilder<F13MessageBuilder>(MessageTypes.Codes.F13);
				AssertGetMessageBuilder<F14MessageBuilder>(MessageTypes.Codes.F14);
				AssertGetMessageBuilder<F15MessageBuilder>(MessageTypes.Codes.F15);
				AssertGetMessageBuilder<F16MessageBuilder>(MessageTypes.Codes.F16);
				AssertGetMessageBuilder<F17MessageBuilder>(MessageTypes.Codes.F17);
				AssertGetMessageBuilder<F22MessageBuilder>(MessageTypes.Codes.F22);
				AssertGetMessageBuilder<F23MessageBuilder>(MessageTypes.Codes.F23);
				AssertGetMessageBuilder<F24MessageBuilder>(MessageTypes.Codes.F24);
				AssertGetMessageBuilder<F25MessageBuilder>(MessageTypes.Codes.F25);
				AssertGetMessageBuilder<F26MessageBuilder>(MessageTypes.Codes.F26);
				AssertGetMessageBuilder<F40MessageBuilder>(MessageTypes.Codes.F40);
				AssertGetMessageBuilder<F41MessageBuilder>(MessageTypes.Codes.F41);
				AssertGetMessageBuilder<F43MessageBuilder>(MessageTypes.Codes.F43);
				AssertGetMessageBuilder<F44MessageBuilder>(MessageTypes.Codes.F44);
				AssertGetMessageBuilder<F50MessageBuilder>(MessageTypes.Codes.F50);
				AssertGetMessageBuilder<F51MessageBuilder>(MessageTypes.Codes.F51);

				AssertGetMessageBuilder<A10MessageBuilder>(MessageTypes.Codes.A10);
				AssertGetMessageBuilder<A13MessageBuilder>(MessageTypes.Codes.A13);
				AssertGetMessageBuilder<A14MessageBuilder>(MessageTypes.Codes.A14);
				AssertGetMessageBuilder<A15MessageBuilder>(MessageTypes.Codes.A15);
				AssertGetMessageBuilder<A16MessageBuilder>(MessageTypes.Codes.A16);
				AssertGetMessageBuilder<A17MessageBuilder>(MessageTypes.Codes.A17);
				AssertGetMessageBuilder<A22MessageBuilder>(MessageTypes.Codes.A22);
				AssertGetMessageBuilder<A23MessageBuilder>(MessageTypes.Codes.A23);
				AssertGetMessageBuilder<A24MessageBuilder>(MessageTypes.Codes.A24);
				AssertGetMessageBuilder<A26MessageBuilder>(MessageTypes.Codes.A26);
				AssertGetMessageBuilder<A40MessageBuilder>(MessageTypes.Codes.A40);
				AssertGetMessageBuilder<A41MessageBuilder>(MessageTypes.Codes.A41);
				AssertGetMessageBuilder<A50MessageBuilder>(MessageTypes.Codes.A50);
				AssertGetMessageBuilder<A51MessageBuilder>(MessageTypes.Codes.A51);

				AssertGetMessageBuilder<Q04MessageBuilder>(MessageTypes.Codes.Q04);

				AssertGetMessageBuilder<R02MessageBuilder>(MessageTypes.Codes.R02);
				AssertGetMessageBuilder<R03MessageBuilder>(MessageTypes.Codes.R03);
				AssertGetMessageBuilder<N06MessageBuilder>(MessageTypes.Codes.N06);
			});

			void AssertGetMessageBuilder<T>(string messageType)
			{
				AssertType<T>(messageType, EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(messageType, manifestHeader, null));
			}
		}

		public void TestSendInvalidationRequest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertType<Q04MessageBuilder>(EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(MessageTypes.Codes.Q04, manifestHeader, null));
		}

		public void TestSendArrivalNotification()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertType<N06MessageBuilder>(EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(MessageTypes.Codes.N06, manifestHeader, null));
		}

		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			EUICS2MessageBuilderLoader.Instance.GetMessageBuilder("INVALID", null, null);
			AssertEquals($"Invalid EU ICS2 Message Builder for code: INVALID", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
