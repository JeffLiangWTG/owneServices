using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	[RegistryEditor("Enterprise.ServiceManager.Tasks.FTP.FtpProfileRegistryItemEditor, Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller")]
	public class FtpProfileDataType : NonPersistentBusinessObjectRegistryDataType<FtpProfileCollection>
	{
		public FtpProfileDataType()
		{
		}
	}
}
