using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(JobApplicantCollectionProvider))]
	sealed class JobApplicantCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<Enterprise.Integration.Recruiter.IHRJobApplicantCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.HRJobApplicant;
	}
}
