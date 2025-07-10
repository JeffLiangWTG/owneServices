using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ViewLocationCollectionProvider))]
	sealed class ViewLocationCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ViewLocationCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.ViewLocation;
	}
}
