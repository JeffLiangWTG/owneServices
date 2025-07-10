using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.DataTransfer.GUI
{
	public class ProductXmlDataTransferDirector : XmlDataTransferDirector
	{
		public ProductXmlDataTransferDirector(ProductValueObjectDataAdapter adapter)
			: base(adapter, false)
		{
		}

		public ProductXmlDataTransferDirector()
			: this(ProductValueObjectDataAdapter.New())
		{
		}

		public override Xml.XmlValueObjectSerializer Serializer
		{
			get { return new ProductXmlValueObjectSerializer(Adapter.ValueObjectType); }
			set { base.Serializer = value; }
		}

		#region Import

		protected override XmlDataImporter NewXmlDataImporter()
		{
			return new ProductXmlDataImporter((ProductValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
