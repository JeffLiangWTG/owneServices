using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.SystemMerge.Xml.Testing
{
	class SysMergeClassifcationXmlValueObjectSerializerForTesting : SysMergeClassificationObjectSerializer
	{
		public SysMergeClassifcationXmlValueObjectSerializerForTesting(IValueObjectDataAdapter adapter)
			: base(adapter.ValueObjectType)
		{
		}
	}
}
