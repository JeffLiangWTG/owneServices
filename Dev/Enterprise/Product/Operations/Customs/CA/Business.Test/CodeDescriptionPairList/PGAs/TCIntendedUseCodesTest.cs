using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TCIntendedUseCodesTest : TestCase
	{
		public void TestConvertFromOGDCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("TC01", TCIntendedUseCodes.ConvertFromOGDCode(ImportReasonCodes.Codes.Sale));
				AssertEquals("TC02", TCIntendedUseCodes.ConvertFromOGDCode(ImportReasonCodes.Codes.Export));
				AssertEquals("TC03", TCIntendedUseCodes.ConvertFromOGDCode(ImportReasonCodes.Codes.NotRegulated));
				AssertEquals("TC04", TCIntendedUseCodes.ConvertFromOGDCode(ImportReasonCodes.Codes.Retread));
				AssertEquals(string.Empty, TCIntendedUseCodes.ConvertFromOGDCode(ImportReasonCodes.Codes.Modification));
				AssertEquals(string.Empty, TCIntendedUseCodes.ConvertFromOGDCode(ImportReasonCodes.Codes.Testing));
			});
		}
	}
}
