using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	[XmlSerializerAssembly("Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.XmlSerializers")]
	public class FtpProfileCollection : RegistryBusinessObjectCollectionTemplate
	{
		public FtpProfileCollection()
			: base()
		{
		}

		public FtpProfileCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new FtpProfile this[int i]
		{
			get { return (FtpProfile)Elements[i]; }
		}

		public new FtpProfile AddNew()
		{
			return (FtpProfile)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FtpProfileCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FtpProfile(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
