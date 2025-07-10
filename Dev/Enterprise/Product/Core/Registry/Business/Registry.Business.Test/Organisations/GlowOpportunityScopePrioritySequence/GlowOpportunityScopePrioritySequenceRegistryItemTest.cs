using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityScopePrioritySequenceRegistryItem))]
	sealed class GlowOpportunityScopePrioritySequenceRegistryItemTest : StronglyTypedRegistryItemTestCase<GlowOpportunityScopePrioritySequenceCollection>
	{
		protected override StronglyTypedRegistryItem<GlowOpportunityScopePrioritySequenceCollection, GlowOpportunityScopePrioritySequenceCollection> GetNewRegistryItem()
		{
			return new GlowOpportunityScopePrioritySequenceRegistryItem("", null, null, null, RegistryStorageFlags.System, new GlowOpportunityScopePrioritySequenceCollection());
		}
	}
}
