using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			Assert("ASYCUDA does not currently support cancellation", !statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			Assert("ASYCUDA does not currently support cancellation", !statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			Assert("ASYCUDA does not currently support modification", !statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			Assert("ASYCUDA currently always allow modification", statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			Assert("ASYCUDA does not currently support customs acceptance", !statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			Assert("Not submitted if MessageStatus == ''", !statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			Assert("Has been submitted if MessageStatus == 'SNT'", statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
			Assert("Not submitted if MessageStatus == 'ERR'", !statusProvider.HasManifestBeenSubmittedToCustoms(header));

			var message = header.Messages.AddNew();
			Assert(statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.Messages.RemoveAndDelete(message);
			Assert(!statusProvider.HasManifestBeenSubmittedToCustoms(header));

			message = header.Messages.AddNew();
			header.Messages.Reload(true);
			Assert(statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.Messages.RemoveAndDelete(message);
			header.Messages.Reload(true);
			Assert(!statusProvider.HasManifestBeenSubmittedToCustoms(header));

			var bill = header.Bills.AddNew();
			message = bill.Messages.AddNew();
			header.Messages.Reload(true);
			Assert(bill.MessageStatusProvider.HasManifestBeenSubmittedToCustoms(bill));
			bill.Messages.RemoveAndDelete(message);
			header.Messages.Reload(true);
			Assert(!bill.MessageStatusProvider.HasManifestBeenSubmittedToCustoms(bill));

			message = header.Messages.AddNew();
			header.Messages.Reload(true);
			Assert(bill.MessageStatusProvider.HasManifestBeenSubmittedToCustoms(bill));
			header.Messages.RemoveAndDelete(message);
			header.Messages.Reload(true);
			Assert(!bill.MessageStatusProvider.HasManifestBeenSubmittedToCustoms(bill));

			message = header.Messages.AddNew();
			Assert(bill.MessageStatusProvider.HasManifestBeenSubmittedToCustoms(bill));
			header.Messages.RemoveAndDelete(message);
			Assert(!bill.MessageStatusProvider.HasManifestBeenSubmittedToCustoms(bill));
		}

		public override void TestMessageStatusCanBeReset()
		{
			Assert("ASYCUDA does not currently support message status reset", !statusProvider.MessageStatusCanBeReset(header));
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider() => statusProvider;

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			statusProvider = header.MessageStatusProvider;
		}

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
	}
}
