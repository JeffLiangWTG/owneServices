using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(CountryCollectionProvider))]
	sealed class CountryCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(RefCountryCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.RefCountry;

		protected override int ExpectedMaxLength => RefCountrySchema.RN_Code.MaxLength;
	}
}
