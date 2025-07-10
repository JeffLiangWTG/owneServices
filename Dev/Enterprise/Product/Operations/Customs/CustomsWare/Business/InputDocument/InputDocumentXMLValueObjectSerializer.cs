using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class InputDocumentXMLValueObjectSerializer : XmlValueObjectSerializer
	{
		public InputDocumentXMLValueObjectSerializer()
			: base(typeof(XSD.InputDocument))
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection,
			IValueObject valueObject, IValueObjectImportContext context)
		{
			InputDocument = valueObject as XSD.InputDocument;
			return base.CreateOrUpdateFromValueObject(dataAdapter, collection, valueObject, context);
		}
		internal XSD.InputDocument InputDocument;
	}
}
