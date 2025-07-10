using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase4ReadOnlyProviderTest : TestCase
	{
		public void TestImplementation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_LineNo_ReadOnly", false, provider.CSI_LineNo_ReadOnly);
				AssertEquals("CSI_Status_ReadOnly", false, provider.CSI_Status_ReadOnly);
				AssertEquals("CSI_Code_ReadOnly", false, provider.CSI_Code_ReadOnly);
				AssertEquals("CSI_ReferenceNumber_ReadOnly", false, provider.CSI_ReferenceNumber_ReadOnly);
				AssertEquals("CSI_ReferenceNumber2_ReadOnly", false, provider.CSI_ReferenceNumber2_ReadOnly);
				AssertEquals("CSI_Description_ReadOnly", false, provider.CSI_Description_ReadOnly);
				AssertEquals("CSI_ItemNumber_ReadOnly", false, provider.CSI_ItemNumber_ReadOnly);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new NctsSupportingDocumentPhase4ReadOnlyProvider();
		}
		ISupportingDocumentReadOnlyConditions provider;
	}
}
