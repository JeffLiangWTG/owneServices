using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(RefCurrencyCollectionProvider))]
	sealed class RefCurrencyCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefCurrencyCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefCurrency;

		protected override int ExpectedMaxLength => RefCurrencySchema.RX_Code.MaxLength;
	}
}
