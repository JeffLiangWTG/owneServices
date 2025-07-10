using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Xml;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeEdocsXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeEdocsXmlDataTransferDirector()
			: base(new SysMergeEdocsValueObjectDataAdapter())
		{
		}

		public override XmlValueObjectSerializer Serializer
		{
			get
			{
				return new SysMergeEdocsXmlValueObjectSerializer(Adapter.ValueObjectType);
			}
		}
	}
}
