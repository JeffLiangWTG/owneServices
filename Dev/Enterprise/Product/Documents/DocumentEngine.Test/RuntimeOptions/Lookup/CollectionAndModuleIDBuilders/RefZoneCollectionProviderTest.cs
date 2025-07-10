using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(InternationalZonesCollectionProvider))]
	sealed class RefZoneCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefZoneHeaderCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.InternationalZone;

		protected override int ExpectedMaxLength => RefZoneHeaderSchema.FZ_Code.MaxLength;
	}
}
