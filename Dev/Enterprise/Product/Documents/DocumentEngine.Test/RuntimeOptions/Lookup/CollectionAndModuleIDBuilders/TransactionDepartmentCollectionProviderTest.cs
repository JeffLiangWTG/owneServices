using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(TransactionDepartmentCollectionProvider))]
	sealed class TransactionDepartmentCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbDepartmentCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbDepartment;
	}
}
