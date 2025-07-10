using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	public class DummyRegistryBusinessObjectCollection : RegistryBusinessObjectCollection
	{
		public DummyRegistryBusinessObjectCollection()
		{
		}

		public DummyRegistryBusinessObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DummyRegistryBusinessObjectCollection(ReadOnlyCodeDescriptionPairList list) : base(list)
		{
		}

		public DummyRegistryBusinessObjectCollection(ReadOnlyCodeDescriptionPairList list, int codeMaxLength)
				: base(list, codeMaxLength)
		{
		}

		public new DummyRegistryBusinessObject this[int i]
		{
			get { return (DummyRegistryBusinessObject)Elements[i]; }
		}

		public new DummyRegistryBusinessObject AddNew()
		{
			return (DummyRegistryBusinessObject)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DummyRegistryBusinessObjectCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyRegistryBusinessObject(CurrentFactory);
		}
	}
}
