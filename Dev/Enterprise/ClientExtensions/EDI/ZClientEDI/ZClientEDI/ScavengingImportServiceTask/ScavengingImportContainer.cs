using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	class ScavengingImportContainer
	{
		readonly INotifications notifier;

		public ScavengingImportContainer(INotifications notifier)
		{
			this.notifier = Argument.NotNull(notifier, "notifier");
		}

		internal IImportService ResolveService()
		{
			var repository = new SqlOrganizationScavengingRepository(notifier);
			var nativeObjectImporter = ObjectFactory.New<INativeObjectImporter>();
			nativeObjectImporter.AlwaysUseProvidedPKs = true;
			var processor = new NativeXmlProcessor(nativeObjectImporter);

			return new ScavengingImportService(repository, processor, notifier);
		}
	}
}