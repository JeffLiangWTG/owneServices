using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AddressListCollection : RegistryBusinessObjectCollection
	{
		public AddressListCollection()
		{
		}

		public AddressListCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		#region Add

		public new AddressListElement AddNew() => (AddressListElement)base.AddNew();

		protected override bool AllowNewCore => true;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new AddressListCollection();

		#endregion

		#region AddressListElements

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AddressListElement();

		public new AddressListElement this[int i] => (AddressListElement)Elements[i];

		#endregion

		public bool HasAddressListElement(ZString addressType, ZString controllerName) => this.Cast<AddressListElement>().Any(x => x.AddressType == addressType && x.ControllerName == controllerName);
	}
}
