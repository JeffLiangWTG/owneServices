using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class DummyZFilterStripGridModuleWithISupportEDocsBulkDownload : DummyZFilterStripGridModule, ISupportEDocsBulkDownload
	{
		public DummyZFilterStripGridModuleWithISupportEDocsBulkDownload(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		WebEDocsDownloadEntry ISupportEDocsBulkDownload.GetRegistryWebEDocsBulkDownload()
		{
			return WebDataRegistry.Instance.WebEDocsBulkDownload.GetModule(new WebEDocsDownloadModulesList().AllModules.Code);
		}

		ZGuid ISupportEDocsBulkDownload.GetEDocsBulkDownloadRelevantPK(ZDataGrid grid, int itemIndex)
		{
			return new ZGuid(grid.DataKeys[itemIndex]);
		}
	}
}
