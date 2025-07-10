using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccGLAccountDescriptorCHSCollectionProvider))]
	sealed class AccGLAccountDescriptorCHSCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccGLAccountDescriptorCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccGLAccountDescriptor;
	}
}
