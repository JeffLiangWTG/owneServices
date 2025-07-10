using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	public class FtpProfileCollectionRegistryItem : StronglyTypedRegistryItem<FtpProfileCollection>
	{
		public FtpProfileCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new FtpProfileDataType(), storage))
		{
		}
	}
}
