using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public abstract class SysMergeXmlDataTransferExporter : XmlDataTransferExporter
	{
		protected SysMergeXmlDataTransferExporter(IValueObjectDataAdapter adapter)
			: base(adapter, false)
		{
		}

		protected override bool ShoulSaveFactoriesAfterExport
		{
			get { return false; }
		}
	}
}
