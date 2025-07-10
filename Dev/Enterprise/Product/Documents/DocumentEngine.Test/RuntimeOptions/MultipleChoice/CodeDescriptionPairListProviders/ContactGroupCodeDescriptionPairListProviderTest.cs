using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ContactGroupCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ContactGroupCodeDescriptionPairListProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			ReadOnlyCodeDescriptionPairList expected = Enterprise.MasterFiles.Business.OrgCodeLists.ContactType_List;
			ReadOnlyCodeDescriptionPairList actual = new ContactGroupCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			AssertListEqual(actual, expected);
		}
	}
}
