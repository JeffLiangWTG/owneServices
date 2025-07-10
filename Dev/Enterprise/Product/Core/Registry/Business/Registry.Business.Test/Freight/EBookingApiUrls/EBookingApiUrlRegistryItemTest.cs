using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EBookingApiUrlRegistryItem))]
	sealed class EBookingApiUrlRegistryItemTest : StronglyTypedRegistryItemTestCase<EBookingApiUrls>
	{
		protected override StronglyTypedRegistryItem<EBookingApiUrls, EBookingApiUrls> GetNewRegistryItem()
		{
			return new EBookingApiUrlRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.Default, new EBookingApiUrls(EBookingApiUrls.Constants.ProdCode));
		}
	}
}
