using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.CusTempStorage.UCC6.Testing
{
	sealed class TemporaryStorageMessageBuilderTest : CusTempStorage.Testing.TemporaryStorageMessageBuilderAbstractTest<TemporaryStorageMessageBuilder>
	{
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

		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override TemporaryStorageMessageBuilder GetMessageBuilder(EU.Business.CusTempStorage.TemporaryStorageHeader header, EU.Business.CusTempStorage.TemporaryStorageMessageFunction function)
		{
			return new TemporaryStorageMessageBuilder(new TemporaryStorageMessageSendingObject((TemporaryStorageHeader)header), function);
		}
	}
}
