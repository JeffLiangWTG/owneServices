using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(LocationCollectionProvider))]
	sealed class LocationCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(LocationCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Location;

		protected override int ExpectedMaxLength => RefUNLOCOSchema.RL_Code.MaxLength;
	}
}
