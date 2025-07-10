namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowOriginalMessage()
		{
			Assert(!statusProvider.AllowOriginalMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			Assert(!statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowCancellationMessage()
		{
			Assert(!statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			Assert(!statusProvider.AllowManifestCancellationMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			Assert("When header AMA_MessageStatus is empty", !statusProvider.HasManifestBeenAcceptedByCustoms(header));

			header.AMA_MessageStatus = "AWA";
			Assert("When header AMA_MessageStatus is not empty", statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			Assert(!statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			Assert(!statusProvider.MessageStatusCanBeReset(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			statusProvider = header.MessageStatusProvider;
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider()
			=> statusProvider;

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
	}
}
