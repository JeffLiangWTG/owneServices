using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(NettingPeriodCollectionProvider))]
	sealed class NettingPeriodCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(NettingSystemPeriodCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.NettingPeriod;

		protected override int ExpectedMaxLength => NettingSystemPeriodSchema.NSP_Period.MaxLength;
	}
}
