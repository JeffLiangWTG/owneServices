using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceCopyCollection : RegistryBusinessObjectCollectionTemplate
	{
		public InvoiceCopyCollection()
		{
		}

		public InvoiceCopyCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new InvoiceCopy this[int i]
		{
			get { return (InvoiceCopy)Elements[i]; }
		}

		public new InvoiceCopy AddNew()
		{
			return (InvoiceCopy)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceCopyCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceCopy(CurrentFallbackLevel, CurrentFactory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject newChild)
		{
			base.SetDefaultsForNewChild(newChild);

			if (newChild is InvoiceCopy)
			{
				var invoiceCopy = newChild as InvoiceCopy;
				invoiceCopy.Order = Elements.Any() ? Elements.Cast<InvoiceCopy>().Max(x => x.Order) + 1 : 1;
			}
		}

		protected override void OnRemoved(BusinessObject deletedObject)
		{
			base.OnRemoved(deletedObject);

			if (deletedObject is InvoiceCopy)
			{
				var invoiceCopy = deletedObject as InvoiceCopy;
				Elements.Cast<InvoiceCopy>().Where(x => x.Order > invoiceCopy.Order).ToList().ForEach(y => y.Order--);
			}
		}
	}
}