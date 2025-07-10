using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(ServiceLevelCollectionProvider))]
	sealed class ServiceLevelCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefServiceLevelCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.ServiceLevel;

		protected override int ExpectedMaxLength => RefServiceLevelSchema.RS_Code.MaxLength;
	}
}
