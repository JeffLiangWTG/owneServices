namespace Enterprise.Customs.GB.GVMS.Testing
{
	class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowOriginalMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowManifestCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			AssertEquals(false, statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			AssertEquals(false, statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			AssertEquals(expected: false, statusProvider.HasManifestBeenSubmittedToCustoms(header));

			var message = Factory.New<GVMSEDIMessage>();
			header.Messages.Add(message);

			AssertEquals(expected: false, statusProvider.HasManifestBeenSubmittedToCustoms(header));

			Factory.Save();
			AssertEquals(expected: true, statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			AssertEquals(false, statusProvider.MessageStatusCanBeReset(header));
		}
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			statusProvider = header.MessageStatusProvider;
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider()
		{
			return statusProvider;
		}

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
	}
}
