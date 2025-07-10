using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(GlbDepartmentCollectionProvider))]
	sealed class GlbDepartmentCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbDepartmentCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbDepartment;
	}
}
