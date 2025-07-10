using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class AdditionalReferenceProviderTest : DataProviderTestCase<AdditionalReferenceProvider>
	{
		public void TestType()
		{
			SetUpTestData();
			supportingInfo.CSI_Code = "A";
			AssertEquals("A", Provider.Type);

			supportingInfo.CSI_Code = "VFT";
			AssertEquals("VFT", Provider.Type);
		}

		public void TestNumber()
		{
			SetUpTestData();
			supportingInfo.CSI_ReferenceNumber = "Test222";
			AssertEquals("Test222", Provider.Number);

			supportingInfo.CSI_ReferenceNumber = "More testing";
			AssertEquals("More testing", Provider.Number);
		}

		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", AdditionalReferenceProvider.NewOrNull(null));
				AssertNotNull("Valid argument", GetProvider());
			});
		}

		protected override AdditionalReferenceProvider GetProvider()
		{
			SetUpTestData();
			return AdditionalReferenceProvider.NewOrNull(supportingInfo);
		}

		void SetUpTestData()
		{
			if (supportingInfo == null)
			{
				supportingInfo = Factory.New<CusSupportingInfo>();
			}
		}
		CusSupportingInfo supportingInfo;
	}
}
