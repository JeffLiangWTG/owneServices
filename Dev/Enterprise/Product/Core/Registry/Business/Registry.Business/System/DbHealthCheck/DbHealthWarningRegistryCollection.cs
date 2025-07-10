using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DbHealthWarningRegistryCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DbHealthWarningRegistryElement();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DbHealthWarningRegistryCollection();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public new DbHealthWarningRegistryElement this[int index]
		{
			get { return (DbHealthWarningRegistryElement)base[index]; }
		}

		public new DbHealthWarningRegistryElement AddNew()
		{
			return (DbHealthWarningRegistryElement)base.AddNew();
		}
	}
}
