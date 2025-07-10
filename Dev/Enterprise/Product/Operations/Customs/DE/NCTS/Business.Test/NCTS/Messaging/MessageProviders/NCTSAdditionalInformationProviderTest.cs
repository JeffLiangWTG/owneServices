using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSAdditionalInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<INCTSAdditionalInformation>
	{
		public void TestCode() => CombineAssertions(() =>
		{
			AssertEquals("Code", Provider.Code);
			info.CSI_Code = ZString.Empty;
			AssertNull(Provider.Code);
		});

		public void TestText() => CombineAssertions(() =>
		{
			AssertEquals("Text", Provider.Text);
			info.CSI_Description = ZString.Empty;
			AssertNull(Provider.Text);
		});

		protected override void SetUp()
		{
			base.SetUp();

			info = Factory.NewWithValidTestData<CusSupportingInfo>();
			info.CSI_Code = "Code";
			info.CSI_Description = "Text";
		}

		protected override INCTSAdditionalInformation GetProvider() => NCTSAdditionalInformationProvider.NewOrNull(info);

		CusSupportingInfo info;
	}
}
