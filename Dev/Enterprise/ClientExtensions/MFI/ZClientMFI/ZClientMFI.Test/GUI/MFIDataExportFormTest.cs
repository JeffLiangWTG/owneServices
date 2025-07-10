using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.MFI.GUI.Testing
{
	[TestedType(typeof(MFIMenu.MFIDataExportForm))]
	public class MFIDataExportFormTest : DataExportFormTest
	{
		protected override DataExportForm NewExportForm(FlatFileDataExporterForTesting exporter, CollectionWrapperBusinessObjectReader reader)
		{
			return new MFIMenu.MFIDataExportForm(exporter, reader);
		}
	}
}
