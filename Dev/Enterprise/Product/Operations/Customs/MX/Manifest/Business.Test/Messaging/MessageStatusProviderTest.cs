namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			var bill = header.Bills.AddNew();
			AssertEquals(false, statusProvider.AllowCancellationMessage(header));

			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "ACP";

			AssertEquals(true, statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowManifestCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			var bill = header.Bills.AddNew();
			AssertEquals(false, statusProvider.AllowModificationMessage(header));

			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "ACP";
			AssertEquals(true, statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			var bill = header.Bills.AddNew();
			AssertEquals(true, statusProvider.AllowOriginalMessage(header));

			bill.ABL_BillStatus = "ACP";
			bill.ABL_MessageStatus = "ACP";
			AssertEquals(false, statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			AssertEquals(false, statusProvider.MessageStatusCanBeReset(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
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
