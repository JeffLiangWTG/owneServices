using Enterprise.DataTransfer.SystemMerge.DataAdapters;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeOrganisationXmlDataTransferExporter : SysMergeXmlDataTransferExporter
	{
		public SysMergeOrganisationXmlDataTransferExporter()
			: base(new SysMergeOrganisationValueObjectDataAdapter())
		{
		}
	}
}
