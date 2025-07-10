using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class LicenseTypeCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), SystemDataRegistry.Instance.StaffCertificateTypes.Value.GetCodeDescriptionPairList());
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new LicenseTypeCodeDescriptionPairListProvider();
		}
	}
}
