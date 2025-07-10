using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Xml;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeCusClassificationXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeCusClassificationXmlDataTransferDirector()
			: base(new SysMergeClassificationValueObjectDataAdapter())
		{
		}

		public override XmlValueObjectSerializer Serializer
		{
			get
			{
				return new SysMergeClassificationObjectSerializer(Adapter.ValueObjectType);
			}
		}
	}
}
