using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatisticsFoldupInfoRegistryItem))]
	sealed class StatisticsFoldupInfoRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<StatisticsFoldupInfoCollection>
	{
		protected override StronglyTypedRegistryItem<StatisticsFoldupInfoCollection, StatisticsFoldupInfoCollection> GetNewRegistryItem()
		{
			return new StatisticsFoldupInfoRegistryItem("", null, null, null, new StatisticsFoldupInfoCollection());
		}
	}
}
