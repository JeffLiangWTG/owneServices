using System;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business
{
	[TestedType(typeof(CustomsOfficeCollectionProvider))]
	public class CustomsOfficeCollectionProviderTest : DocumentEngine.RuntimeOptions.Testing.CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ZZRefCusCodeListCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override int ExpectedMaxLength => 3;
	}
}
