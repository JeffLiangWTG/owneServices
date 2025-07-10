using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.SystemMerge.Xml.Testing
{
	class SysMergeEdocsXmlValueObjectSerializerForTesting : SysMergeEdocsXmlValueObjectSerializer
	{
		public SysMergeEdocsXmlValueObjectSerializerForTesting(IValueObjectDataAdapter adapter)
			: base(adapter.ValueObjectType)
		{
		}
	}
}
