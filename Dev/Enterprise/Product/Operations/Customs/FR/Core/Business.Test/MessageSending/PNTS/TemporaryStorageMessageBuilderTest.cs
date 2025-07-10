using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class TemporaryStorageMessageBuilderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageMessageBuilderTest<TemporaryStorageMessageBuilder>
	{
		public void TestGetIETS414Message()
		{
			AssertMessageCanBePopulated<InvalidationRequestTSDMessageFunction>("IETS414");
		}

		public void TestGetIETS015Message()
		{
			AssertMessageCanBePopulated<PreLodgedTSDMessageFunction>("IETS015");
		}

		public void TestGetIETS115Message()
		{
			AssertMessageCanBePopulated<CombinedTSDMessageFunction>("IETS115");
		}

		public void TestGetIETS007Message()
		{
			AssertMessageCanBePopulated<PresentationNotificationMessageFunction>("IETS007");
		}

		public void TestGetIETS413Message()
		{
			AssertMessageCanBePopulated<AmendmentRequestTSDMessageFunction>("IETS413");
		}

		protected override ZString ExpectedEM_MessageOwner => "FRCOM_REGISTERNO";

		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.FRCustomsMessage;

		protected override TemporaryStorageMessageBuilder GetMessageBuilder(TemporaryStorageHeader header, TemporaryStorageMessageFunction function)
		{
			return new TemporaryStorageMessageBuilder(new TemporaryStorageMessageSendingObject(header), function);
		}

		protected override void SetUp()
		{
			GlbBranch.CurrentBranch.Company.GC_CustomsRegistrationNo = "FRCOM_REGISTERNO";
			GlbBranch.CurrentBranch.Company.Factory.Save();
			base.SetUp();
		}
	}
}
