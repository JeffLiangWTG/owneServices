using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class TransactionTypePrefixCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new TransactionTypePrefix this[int index]
		{
			get { return (TransactionTypePrefix)Elements[index]; }
		}

		public new TransactionTypePrefix AddNew()
		{
			return (TransactionTypePrefix)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransactionTypePrefixCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransactionTypePrefix();
		}
	}
}
