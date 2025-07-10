using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class ContingencyDataEmailAddressCollection : RegistryBusinessObjectCollection
	{
		public ContingencyDataEmailAddressCollection()
		{
		}

		public ContingencyDataEmailAddressCollection(ReadOnlyCodeDescriptionPairList list) : base(list)
		{
		}

		public new ContingencyDataEmailAddress this[int i]
		{
			get { return (ContingencyDataEmailAddress)Elements[i]; }
		}

		public new ContingencyDataEmailAddress AddNew()
		{
			return (ContingencyDataEmailAddress)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ContingencyDataEmailAddressCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContingencyDataEmailAddress();
		}
	}
}
