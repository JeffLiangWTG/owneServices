using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Xml;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	public class SysMergeProductXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeProductXmlDataTransferDirector()
			: base(new SysMergeProductValueObjectDataAdapter())
		{
		}

		//TODO: By Leo Liang - Should be able to remove this method
		public override XmlValueObjectSerializer Serializer
		{
			get
			{
				return new SysMergeProductXmlValueObjectSerializer(Adapter.ValueObjectType);
			}
		}

		protected override bool UseTransaction => false;
	}
}
