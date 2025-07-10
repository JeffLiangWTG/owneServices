using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.SystemMerge.Xml.Testing
{
	internal class SysMergeProductXmlValueObjectSerializerForTesting : SysMergeProductXmlValueObjectSerializer
	{
		public SysMergeProductXmlValueObjectSerializerForTesting(IValueObjectDataAdapter adapter)
			: base(adapter.ValueObjectType)
		{
			this.adapter = adapter;
		}

		readonly IValueObjectDataAdapter adapter;

		public BusinessObject CreateOrUpdateFromValueObjectExposed(IValueObject valueObject, IValueObjectImportContext context)
		{
			return CreateOrUpdateFromValueObject(adapter, null, valueObject, context);
		}
	}
}
