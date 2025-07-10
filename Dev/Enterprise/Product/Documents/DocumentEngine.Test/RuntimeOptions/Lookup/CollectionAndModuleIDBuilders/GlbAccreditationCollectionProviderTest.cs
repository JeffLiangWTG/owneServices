using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(GlbAccreditationCollectionProvider))]
	sealed class GlbAccreditationCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IGlbAccreditationCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbAccreditation;
	}
}
