using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsConfidenceThresholdsRegistryItem))]
	sealed class DpsConfidenceThresholdsRegistryItemTest : StronglyTypedRegistryItemTestCase<DpsConfidenceThresholdsBusinessObject>
	{
		protected override StronglyTypedRegistryItem<DpsConfidenceThresholdsBusinessObject, DpsConfidenceThresholdsBusinessObject> GetNewRegistryItem()
			=> new DpsConfidenceThresholdsRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new DpsConfidenceThresholdsBusinessObject());
	}
}
