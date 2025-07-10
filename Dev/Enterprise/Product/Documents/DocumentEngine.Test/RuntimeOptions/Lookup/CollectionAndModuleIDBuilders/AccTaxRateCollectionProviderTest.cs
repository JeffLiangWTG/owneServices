using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccTaxRateCollectionProvider))]
	sealed class AccTaxRateCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccTaxRateCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccTaxRate;

		protected override int ExpectedMaxLength => AccTaxRateSchema.AT_Code.MaxLength;
	}
}
