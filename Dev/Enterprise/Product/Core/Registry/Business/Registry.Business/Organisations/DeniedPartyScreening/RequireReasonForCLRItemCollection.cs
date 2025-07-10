using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RequireReasonForCLRItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new RequireReasonForCLRItem AddNew() => base.AddNew() as RequireReasonForCLRItem;

		public new RequireReasonForCLRItem this[int index] => Elements[index] as RequireReasonForCLRItem;

		public RequireReasonForCLRItemCollection CopyElementValuesFrom(RequireReasonForCLRItemCollection collection)
		{
			foreach (var item in collection)
			{
				Add(item);
			}

			return this;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RequireReasonForCLRItemCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RequireReasonForCLRItem();
		}
	}
}
