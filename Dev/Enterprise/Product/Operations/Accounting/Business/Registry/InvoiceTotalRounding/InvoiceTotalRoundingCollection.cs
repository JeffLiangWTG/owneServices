using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceTotalRoundingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoiceTotalRoundingCollection()
		{
		}

		public InvoiceTotalRoundingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new InvoiceTotalRoundingCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new InvoiceTotalRounding(CurrentFactory);

		public new InvoiceTotalRounding this[int index] => (InvoiceTotalRounding)Elements[index];

		public new InvoiceTotalRounding AddNew() => (InvoiceTotalRounding)base.AddNew();
	}
}