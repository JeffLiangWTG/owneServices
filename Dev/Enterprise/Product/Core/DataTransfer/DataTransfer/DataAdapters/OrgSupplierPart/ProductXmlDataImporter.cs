using Enterprise.DataTransfer.Business;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ProductXmlDataImporter : XmlDataImporter
	{
		public ProductXmlDataImporter(ProductValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		protected new ProductValueObjectDataAdapter Adapter
		{
			get { return (ProductValueObjectDataAdapter)base.Adapter; }
		}

		protected internal override Xml.XmlValueObjectSerializer GetSerializer()
		{
			return new ProductXmlValueObjectSerializer(Adapter.ValueObjectType);
		}
	}
}
