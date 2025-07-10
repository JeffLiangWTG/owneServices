using System;
using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(JobTradeLaneCollectionProvider))]
	sealed class JobTradeLaneCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => ObjectFactory.GetType<IJobTradeLaneCollection>();

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.TradeLane;

		protected override int ExpectedMaxLength => JobTradeLaneSchema.EJ_Code.MaxLength;
	}
}
