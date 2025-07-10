using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class DummyProcessor : EmailImportBatchProcessor
	{
		public DummyProcessor(StringRegistryItem registryPath, NotificationBuffer buffer)
			: base(registryPath, buffer) { }

		protected override IMailFilter MailFilter => QueryMailFilter.AllQueuedItems_ForTesting;
	}
}
