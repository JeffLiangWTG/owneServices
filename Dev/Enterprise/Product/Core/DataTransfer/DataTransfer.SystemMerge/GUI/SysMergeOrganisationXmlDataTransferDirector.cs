using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Xml;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeOrganisationXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeOrganisationXmlDataTransferDirector()
			: base(new SysMergeOrganisationValueObjectDataAdapter())
		{
		}

		//TODO: By Leo Liang - Should be able to remove this method
		public override DataTransfer.Xml.XmlValueObjectSerializer Serializer
		{
			get
			{
				return new SysMergeOrganisationXmlValueObjectSerializer(Adapter.ValueObjectType);
			}
		}
	}
}
