using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	public class DummyNonPersistentBusinessObject : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DummyNonPersistentBusinessObject();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
		}

		#region DummyRelatedBusinessObjectCollection

		protected DummyRelatedBusinessObjectCollection fDummyRelatedBusinessObjectCollections;
		public DummyRelatedBusinessObjectCollection Related
		{
			get
			{
				if (fDummyRelatedBusinessObjectCollections == null)
				{
					fDummyRelatedBusinessObjectCollections = new DummyRelatedBusinessObjectCollection(Factory);
					RegisterEditableChildObject(fDummyRelatedBusinessObjectCollections);
				}
				return fDummyRelatedBusinessObjectCollections;
			}
		}

		#endregion
	}
}
