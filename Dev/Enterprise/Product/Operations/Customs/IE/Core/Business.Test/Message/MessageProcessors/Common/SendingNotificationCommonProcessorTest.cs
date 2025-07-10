using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class SendingNotificationCommonProcessorTest : TestCaseWithFactory
	{
		public void TestGetEmailGroupRegistryItem()
		{
			var processor = new SendingNotificationCommonProcessorForTest(new LoggingInformation());

			CombineAssertions(() =>
			{
				using (processor.SetApplicationCodeForEmailExposed(EDIMessage.ApplicationCodes.IECustomsExport))
				{
					AssertSame("Export", EUCustomsDataRegistry.Instance.SendExportAcknowledgements, processor.EmailGroupRegistryItemExposed);
				}

				using (processor.SetApplicationCodeForEmailExposed(EDIMessage.ApplicationCodes.IECustomsUCC5Import))
				{
					AssertSame("Import UCC5", IECustomsDataRegistry.Instance.SendImportAcknowledgements, processor.EmailGroupRegistryItemExposed);
				}

				using (processor.SetApplicationCodeForEmailExposed(EDIMessage.ApplicationCodes.IECustomsImport))
				{
					AssertSame("Import UCC6", IECustomsDataRegistry.Instance.SendImportAcknowledgements, processor.EmailGroupRegistryItemExposed);
				}

				using (processor.SetApplicationCodeForEmailExposed(EDIMessage.ApplicationCodes.IECustomsNCTS))
				{
					AssertSame("NCTS", EUCustomsDataRegistry.Instance.SendNctsAcknowledgements, processor.EmailGroupRegistryItemExposed);
				}

				using (processor.SetApplicationCodeForEmailExposed(EDIMessage.ApplicationCodes.IECustomsCommon))
				{
					AssertNull(processor.EmailGroupRegistryItemExposed);
				}
			});
		}
	}

	class SendingNotificationCommonProcessorForTest : ErrorMessageProcessor
	{
		public SendingNotificationCommonProcessorForTest(LoggingInformation logger) : base(logger) { }

		public IDisposable SetApplicationCodeForEmailExposed(ZString applicationCode) => SetApplicationCodeForEmail(applicationCode);

		public IRegistryItem EmailGroupRegistryItemExposed => GetEmailGroupRegistryItem();
	}
}
