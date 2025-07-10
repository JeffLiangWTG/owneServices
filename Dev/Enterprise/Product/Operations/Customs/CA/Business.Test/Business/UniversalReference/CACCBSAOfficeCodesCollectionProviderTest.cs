using System;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACCBSAOfficeCodesCollectionProvider))]
	sealed class CACCBSAOfficeCodesCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ZZRefCusCodeListCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override int ExpectedMaxLength => UniversalReferenceConstants.CBSAOfficeCodeMaxLength;
	}
}
