using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.CusTempStorage.UCC5.Testing
{
	sealed class TemporaryStorageMessageBuilderTest : CusTempStorage.Testing.TemporaryStorageMessageBuilderAbstractTest<TemporaryStorageMessageBuilder>
	{
		/// **********************
		/// Uncomment below tests when message builders and providers are complete for UCC5
		/// **********************

		public void TestGetIETS313Message()
		{
			AssertMessageCanBePopulated<AmendmentMessageFunction>("TS313");
		}

		public void TestGetIETS314Message()
		{
			AssertMessageCanBePopulated<InvalidationMessageFunction>("TS314");
		}

		public void TestGetIETS315Message()
		{
			AssertMessageCanBePopulated<DeclarationMessageFunction>("TS315");
		}

		public void TestGetIETS332Message()
		{
			AssertMessageCanBePopulated<PresentationNotificationMessageFunction>("TS332");
		}

		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsUCC5Import;

		protected override TemporaryStorageMessageBuilder GetMessageBuilder(EU.Business.CusTempStorage.TemporaryStorageHeader header, EU.Business.CusTempStorage.TemporaryStorageMessageFunction function)
		{
			return new TemporaryStorageMessageBuilder(new TemporaryStorageMessageSendingObject((TemporaryStorageHeader)header), function);
		}
	}
}
