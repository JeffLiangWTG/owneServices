using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(LearningCentreCampaignCollectionProvider))]
	sealed class LearningCentreCampaignCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<ILearningCentreCampaignCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.LearningCentreCampaign;
	}
}
