using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(RefVesselCollectionProvider))]
	sealed class RefVesselCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefVesselCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefVessel;

		protected override int ExpectedMaxLength => RefVesselSchema.RV_Code.MaxLength;
	}
}
