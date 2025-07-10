using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class NeoUpgradeLicenceCollection : RegistryBusinessObjectCollectionTemplate
	{
		public NeoUpgradeLicenceCollection()
			: base() { }

		public NeoUpgradeLicenceCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new NeoUpgradeLicence this[int index] => (NeoUpgradeLicence)Elements[index];

		public new NeoUpgradeLicence AddNew() => (NeoUpgradeLicence)base.AddNew();

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NeoUpgradeLicenceCollection(fallbackLevel, factory);

		protected override BusinessObject CreateNonPersistentBusinessObject() => new NeoUpgradeLicence(CurrentFallbackLevel, CurrentFactory);

		#endregion
	}
}

