using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Engine.Test
{
	[TestedType(typeof(ArchiveSystemDescriptorProviderAttribute))]
	sealed class ArchiveSystemDescriptorProviderAttributeTest : AssemblyMetaDataAttributeTestCase<ArchiveSystemDescriptorProviderAttribute>
	{
	}
}
