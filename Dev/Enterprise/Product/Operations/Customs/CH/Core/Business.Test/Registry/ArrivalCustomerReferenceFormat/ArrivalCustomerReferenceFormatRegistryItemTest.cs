using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ArrivalCustomerReferenceFormatRegistryItem))]
class ArrivalCustomerReferenceFormatRegistryItemTest : StronglyTypedRegistryItemTestCase<ArrivalCustomerReferenceFormat>
{
	protected override StronglyTypedRegistryItem<ArrivalCustomerReferenceFormat, ArrivalCustomerReferenceFormat> GetNewRegistryItem()
	{
		return new ArrivalCustomerReferenceFormatRegistryItem("", null, null, null, Integration.RegistryStorageFlags.Company);
	}
}
