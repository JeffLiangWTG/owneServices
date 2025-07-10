using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AWSPrivateCARegistryItem))]
	class AWSPrivateCARegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AWSPrivateCACollection>
	{
		protected override StronglyTypedRegistryItem<AWSPrivateCACollection, AWSPrivateCACollection> GetNewRegistryItem()
		{
			return new AWSPrivateCARegistryItem(
				"SystemToSystemPrivateCAListManager",
				null,
				null,
				null,
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport
			);
		}

		protected override AWSPrivateCACollection ValidValue
		{
			get
			{
				var cAArnCAArnCollection = new AWSPrivateCACollection()
				{
					new AWSPrivateCA()
					{
						IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust,
						Arn = "pc:ca:arn",
						IsEnabled = ZBool.True,
						AccessKey = "Test1",
						SecretKey = "Test1"
					}
				};

				return cAArnCAArnCollection;
			}
		}
	}
}
