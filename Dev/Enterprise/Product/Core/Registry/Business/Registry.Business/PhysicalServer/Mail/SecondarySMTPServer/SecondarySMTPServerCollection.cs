using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SecondarySMTPServerCollection : RegistryBusinessObjectCollectionTemplate<SecondarySMTPServer>
	{
		public SecondarySMTPServerCollection()
			: base(null, null)
		{
		}

		#region Overriden

		protected override BusinessObject CreateNonPersistentBusinessObject() => new SecondarySMTPServer();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new SecondarySMTPServerCollection();

		#endregion
	}
}
