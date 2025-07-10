using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AddressCollectionProvider))]
	sealed class AddressCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(OrgAddressCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.OrgAddresses;

		protected override int ExpectedMaxLength => OrgAddressSchema.OA_Code.MaxLength;
	}
}
