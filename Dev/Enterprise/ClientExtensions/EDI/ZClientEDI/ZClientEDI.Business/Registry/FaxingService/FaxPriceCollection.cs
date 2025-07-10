using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class FaxPriceCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new FaxPrice this[int index]
		{
			get { return (FaxPrice)Elements[index]; }
		}

		public new FaxPrice AddNew()
		{
			return (FaxPrice)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FaxPriceCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FaxPrice();
		}

		#endregion
	}
}

