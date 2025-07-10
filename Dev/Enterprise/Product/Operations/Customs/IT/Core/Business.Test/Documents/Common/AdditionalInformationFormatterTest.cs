using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.IT.Business.Testing;

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
			AssertEquals("Format Result", "", formatter.Format(additionalInfo));

			additionalInfo.CSI_Code = "123";
			additionalInfo.CSI_Description = "DESC";
			additionalInfo.CSI_ReferenceNumber = "REF NUM";
			AssertEquals("Format Result", "123-DESC-REF NUM", formatter.Format(additionalInfo));
		});
	}
}
