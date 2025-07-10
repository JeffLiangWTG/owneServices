using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	sealed class AdditionalInformationFormatterTest : TestCaseWithFactory
	{
		public void TestFormatGuardClause()
		{
			IAdditionalInformationFormatter formatter = new AdditionalInformationFormatter();

			AssertExceptionThrown<ArgumentNullException>("When additionalInfo is null", () => formatter.Format(additionalInfo: null));
		}

		public void TestFormat()
		{
			IAdditionalInformationFormatter formatter = new AdditionalInformationFormatter();
			CombineAssertions(() =>
			{
				var additionalInfo = Factory.New<AdditionalInfo>();
				additionalInfo.CSI_Code = "123";
				AssertEquals("Format Result", "123", formatter.Format(additionalInfo));

				additionalInfo.CSI_Description = "DESC";
				AssertEquals("Format Result", "123-DESC", formatter.Format(additionalInfo));
			});
		}
	}
}
