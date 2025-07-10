using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class ICSMessageStatusProviderTest : MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();
			AssertEquals(expected: false, provider.AllowCancellationMessage(manifest));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();
			AssertEquals(expected: false, provider.AllowManifestCancellationMessage(manifest));
		}

		public override void TestAllowModificationMessage()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();
			AssertEquals(expected: false, provider.AllowModificationMessage(manifest));
		}

		public override void TestAllowOriginalMessage()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();
			AssertEquals(expected: false, provider.AllowOriginalMessage(manifest));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();
			AssertEquals(expected: false, provider.HasManifestBeenAcceptedByCustoms(manifest));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();

			AssertEquals(expected: false, provider.HasManifestBeenSubmittedToCustoms(manifest));

			var message = Factory.New<IcsSsGreatBritainEDIMessage>();
			manifest.Messages.Add(message);

			AssertEquals(expected: false, provider.HasManifestBeenSubmittedToCustoms(manifest));

			Factory.Save();
			AssertEquals(expected: true, provider.HasManifestBeenSubmittedToCustoms(manifest));
		}

		public override void TestMessageStatusCanBeReset()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var provider = GetMessageStatusProvider();
			AssertEquals(expected: false, provider.MessageStatusCanBeReset(manifest));
		}

		protected override MessageStatusProvider GetMessageStatusProvider() => new ICSMessageStatusProvider();
	}
}
