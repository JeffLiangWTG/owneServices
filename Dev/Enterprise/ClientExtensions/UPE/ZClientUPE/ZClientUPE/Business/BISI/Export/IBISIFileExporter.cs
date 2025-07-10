using System.Collections.Generic;

namespace Enterprise.Client.UPE.Business.BISI
{
	public interface IBISIFileExporter
	{
		void SaveDateUploadedAndBISIUploadData();
		BISIExportResult ExportToFile(string targetFileName, ExportInformation exportInformation);
		IReadOnlyList<IShipmentData> LastUploadedCompletedShipments { get; }
	}
}
