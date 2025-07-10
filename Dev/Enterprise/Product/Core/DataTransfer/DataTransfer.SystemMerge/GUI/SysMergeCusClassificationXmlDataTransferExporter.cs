using Enterprise.DataTransfer.SystemMerge.DataAdapters;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeCusClassificationXmlDataTransferExporter : SysMergeXmlDataTransferExporter
	{
		public SysMergeCusClassificationXmlDataTransferExporter()
			: base(new SysMergeClassificationValueObjectDataAdapter())
		{
		}
	}
}