using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccGLHeaderCollectionProvider))]
	sealed class AccGLHeaderCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccGLHeaderCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccGLHeader;
	}
}
