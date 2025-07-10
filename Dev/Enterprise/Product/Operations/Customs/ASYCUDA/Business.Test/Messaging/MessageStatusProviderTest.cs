using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public abstract class MessageStatusProviderTest : TestCaseWithFactory
	{
		public abstract void TestAllowOriginalMessage();

		public abstract void TestAllowModificationMessage();

		public abstract void TestAllowCancellationMessage();

		public abstract void TestAllowManifestCancellationMessage();

		public abstract void TestHasManifestBeenAcceptedByCustoms();

		public abstract void TestHasManifestBeenSubmittedToCustoms();

		public abstract void TestMessageStatusCanBeReset();

		public virtual void TestGetMessageFunctionSubTypeForSend()
		{
			var provider = GetMessageStatusProvider();
			var header = Factory.New<AsycudaManifestHeader>();

			AssertEquals("Should get MessageFunctionSubTypeForSend from the header.", header.MessageFunctionSubTypeForSend, provider.GetMessageFunctionSubTypeForSend(header));
			AssertEquals("Should get ORG from the header.", MessageSubTypeCodes.Codes.Original, provider.GetMessageFunctionSubTypeForSend(header));
			AssertEquals("Should get an empty value when the header is null.", string.Empty, provider.GetMessageFunctionSubTypeForSend(null));
		}

		public virtual void TestGetMessageFunctionSubTypeForAmend()
		{
			var provider = GetMessageStatusProvider();
			var header = Factory.New<AsycudaManifestHeader>();

			AssertEquals("Should get MessageFunctionSubTypeForAmend from the header.", header.MessageFunctionSubTypeForAmend, provider.GetMessageFunctionSubTypeForAmend(header));
			AssertEquals("Should get CHG from the header.", MessageSubTypeCodes.Codes.Change, provider.GetMessageFunctionSubTypeForAmend(header));
			AssertEquals("Should get an empty value when the header is null.", string.Empty, provider.GetMessageFunctionSubTypeForAmend(null));
		}

		public virtual void TestGetMessageFunctionSubTypeForCancel()
		{
			var provider = GetMessageStatusProvider();
			var header = Factory.New<AsycudaManifestHeader>();

			AssertEquals("Should get MessageFunctionSubTypeForCancel from the header.", header.MessageFunctionSubTypeForCancel, provider.GetMessageFunctionSubTypeForCancel(header));
			AssertEquals("Should get CNL from the header.", MessageSubTypeCodes.Codes.Cancellation, provider.GetMessageFunctionSubTypeForCancel(header));
			AssertEquals("Should get an empty value when the header is null.", string.Empty, provider.GetMessageFunctionSubTypeForCancel(null));
		}

		protected abstract MessageStatusProvider GetMessageStatusProvider();
	}
}
