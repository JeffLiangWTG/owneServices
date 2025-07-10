using System;
using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(JobVoyageCollectionProvider))]
	sealed class JobVoyageCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IJobVoyageCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.JobSeaVoyage;
	}
}
