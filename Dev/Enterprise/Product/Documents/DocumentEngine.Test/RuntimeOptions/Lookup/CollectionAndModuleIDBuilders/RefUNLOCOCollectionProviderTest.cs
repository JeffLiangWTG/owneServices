using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(RefUNLOCOCollectionProvider))]
	sealed class RefUNLOCOCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefUNLOCOCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefUNLOCO;

		protected override int ExpectedMaxLength => RefUNLOCOSchema.RL_Code.MaxLength;
	}
}
