
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			Assert(!Provider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			Assert(!Provider.AllowCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			Assert(!Provider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			Assert(!Provider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			var bill = header.Bills.AddNew();
			Assert("Registration Status is empty: header", !Provider.HasManifestBeenAcceptedByCustoms(header));
			Assert("Registration Status is empty: bill", !Provider.HasManifestBeenAcceptedByCustoms(bill));

			header.RegistrationStatus = "SNT";
			Assert("Registration Status is not empty: header", Provider.HasManifestBeenAcceptedByCustoms(header));
			Assert("Registration Status is not empty: bill", Provider.HasManifestBeenAcceptedByCustoms(bill));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			Assert("Not submitted, as no messages containing", !Provider.HasManifestBeenSubmittedToCustoms(header));
			var message = header.Messages.AddNew();
			message.EM_Status = "PRS";
			Assert("Submitted, as messages containing", Provider.HasManifestBeenSubmittedToCustoms(header));
			header.Messages.RemoveAndDelete(message);

			var bill = header.Bills.AddNew();
			Assert("Not submitted, as RegistrationStatus is empty", !Provider.HasManifestBeenSubmittedToCustoms(bill));
			header.RegistrationStatus = "SNT";
			Assert("Submitted, as RegistrationStatus is not empty", Provider.HasManifestBeenSubmittedToCustoms(bill));
			header.RegistrationStatus = ZString.Empty;

			Assert("Not submitted, as MessageStatus is empty", !Provider.HasManifestBeenSubmittedToCustoms(bill));
			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.NotSent;
			Assert("Not submitted, as MessageStatus == 'NOT'", !Provider.HasManifestBeenSubmittedToCustoms(bill));
			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;
			Assert("Has been submitted if MessageStatus == 'SNT'", Provider.HasManifestBeenSubmittedToCustoms(bill));
			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Error;
			Assert("Not submitted if MessageStatus == 'ERR'", !Provider.HasManifestBeenSubmittedToCustoms(bill));
		}

		public override void TestMessageStatusCanBeReset()
		{
			Assert(!Provider.MessageStatusCanBeReset(header));
		}

		public void TestGetRegistrationStatusList()
		{
			var list = Provider.GetRegistrationStatusList(header.Factory, header.AMA_RN_NKCountry, ZString.Empty);

			AssertEquals("ACP, ADD, AEO, ARV, ASC, CAN, CNR, DNL, HRC, INS, NCN, PND, RAI, RAR, REG, RHR, RIR, VAL", list.CodesAsString);
			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, Provider.GetRegistrationStatusList(header.Factory, header.AMA_RN_NKCountry, ZString.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		MessageStatusProvider Provider => provider ?? (provider = (MessageStatusProvider)GetMessageStatusProvider());
		MessageStatusProvider provider;

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider() => new MessageStatusProvider();

		AsycudaManifestHeader header;
	}
}
