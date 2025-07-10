using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalInformationProviderTest : DataProviderTestCase<AdditionalInformationProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AdditionalInfo missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(additionalInfo));
			});
		}

		public void TestCode()
		{
			AssertNull("Code", Provider.Code);

			additionalInfo.CSI_Code = "Code";
			AssertEquals("Code", "Code", Provider.Code);
		}

		public void TestText()
		{
			AssertNull("Text", Provider.Text);

			additionalInfo.CSI_Description = "Description";
			AssertEquals("Text", "Description", Provider.Text);
		}

		public void TestType()
		{
			additionalInfo.CSI_SubType = "Type";
			AssertEquals("Type", "Type", Provider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();

			additionalInfo = Factory.New<AdditionalInfo>();
		}
		AdditionalInfo additionalInfo;

		AdditionalInformationProvider GenerateProvider(AdditionalInfo additionalInfo) => new AdditionalInformationProvider(additionalInfo);

		protected sealed override AdditionalInformationProvider GetProvider()
		{
			return GenerateProvider(additionalInfo);
		}
	}
}
