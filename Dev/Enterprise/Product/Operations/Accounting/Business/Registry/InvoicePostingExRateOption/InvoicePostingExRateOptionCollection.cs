using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoicePostingExRateOptionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoicePostingExRateOptionCollection()
		{ }

		public InvoicePostingExRateOptionCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new InvoicePostingExRateOption this[int index] => (InvoicePostingExRateOption)Elements[index];

		public new InvoicePostingExRateOption AddNew() => (InvoicePostingExRateOption)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new InvoicePostingExRateOptionCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new InvoicePostingExRateOption();

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
