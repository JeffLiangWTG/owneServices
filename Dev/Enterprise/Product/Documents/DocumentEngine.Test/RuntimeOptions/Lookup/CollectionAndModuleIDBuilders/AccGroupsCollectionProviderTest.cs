using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccGroupsCollectionProvider))]
	sealed class AccGroupsCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccGroupsCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccGroups;

		protected override int ExpectedMaxLength => AccGroupsSchema.AR_Code.MaxLength;
	}
}
