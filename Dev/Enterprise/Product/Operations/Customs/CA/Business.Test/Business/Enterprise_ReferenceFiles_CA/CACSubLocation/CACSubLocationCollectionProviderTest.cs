using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACSubLocationCollectionProvider))]
	sealed class CACSubLocationCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(CACSubLocationCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.CA.SubLocation;

		protected override int ExpectedMaxLength => CACSubLocation.Schema.CodeMaxLength;
	}
}
