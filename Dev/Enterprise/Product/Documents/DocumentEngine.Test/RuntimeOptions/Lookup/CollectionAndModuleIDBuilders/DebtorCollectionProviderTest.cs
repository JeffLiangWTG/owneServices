using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(DebtorCollectionProvider))]
	sealed class DebtorCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(DebtorCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Organisation;

		protected override int ExpectedMaxLength => OrgHeaderSchema.OH_Code.MaxLength;
	}
}
