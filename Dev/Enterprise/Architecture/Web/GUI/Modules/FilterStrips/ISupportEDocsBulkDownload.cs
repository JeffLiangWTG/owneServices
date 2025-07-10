using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface ISupportEDocsBulkDownload
	{
		WebEDocsDownloadEntry GetRegistryWebEDocsBulkDownload();
		ZGuid GetEDocsBulkDownloadRelevantPK(ZDataGrid grid, int itemIndex);
		List<ZGuid> GetEDocsBulkDownloadRelatedPKs(ZDataGrid grid, ZGuid gridItemPK);
		List<ZGuid> GetEDocsBulkDownloadRelevantAndRelatedPKs(ZDataGrid grid, int itemIndex);
		ZString GetPersistantBizoHumanReadableName(ZDataGrid grid, ZGuid gridItemPK);
	}
}
