using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpProfileCollectionRegistryItem))]
	sealed class FtpProfileCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<FtpProfileCollection>
	{
		protected override StronglyTypedRegistryItem<FtpProfileCollection, FtpProfileCollection> GetNewRegistryItem()
		{
			return new FtpProfileCollectionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);
		}
	}
}
