using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CreditReportItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new CreditReportItem AddNew() => base.AddNew() as CreditReportItem;

		public new CreditReportItem this[int index] => Elements[index] as CreditReportItem;

		protected override bool AllowSort => false;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditReportItemCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CreditReportItem();
		}
	}
}
