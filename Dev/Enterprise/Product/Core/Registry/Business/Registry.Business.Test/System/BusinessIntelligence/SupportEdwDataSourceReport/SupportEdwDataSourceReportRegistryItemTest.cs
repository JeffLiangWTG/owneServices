using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupportEdwDataSourceReportRegistryItem))]
	public class SupportEdwDataSourceReportRegistryItemTest : StronglyTypedRegistryItemTestCase<SupportEdwDataSourceReportCollection>
	{
		protected override StronglyTypedRegistryItem<SupportEdwDataSourceReportCollection, SupportEdwDataSourceReportCollection> GetNewRegistryItem()
		{
			return new SupportEdwDataSourceReportRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, SupportEdwDataSourceReportCollection.GetDefault());
		}
	}
}
