using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgAddressCapapabilityCollection))]
	public class DocOrgAddressCapapabilityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocOrgAddressCapapabilityCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.MainAddress;
			OrgAddressCapabilityWrapper addressCapability = address.AddressCapability.GetAddressCapabilityOnCode(nameof(AddressType.OFC));
			return DocOrgAddressCapability.New(addressCapability, Factory);
		}

		protected override DocOrgAddressCapapabilityCollection GetCollectionToTest()
		{
			return new DocOrgAddressCapapabilityCollection(Factory);
		}
	}
}
