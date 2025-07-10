using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.SystemMerge.Xml.Testing
{
	class SysMergeOrganisationXmlValueObjectSerializerForTesting : SysMergeOrganisationXmlValueObjectSerializer
	{
		public SysMergeOrganisationXmlValueObjectSerializerForTesting(IValueObjectDataAdapter adapter)
			: base(adapter.ValueObjectType)
		{
		}
	}
}
