using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT043V5ResponseMessageProcessorTest))]
sealed class NT043V5ResponseMessageProcessorTest : NT043BaseResponseMessageProcessorTest
{
	protected override string GetResponseMessage() => TestingData.GetNT043V5();
	protected override string GetResponseMessageWithMrnAndIdentificationNumber(string mrn) => TestingData.GetNT043V5(messageIdentification: SampleMessageIdentification, mrn: mrn);

	protected override void AssertHouseConsignmentCountryOfDestination(NctsBill bill)
	{
		AssertEquals("Bill1 - B0_RN_NKCountryOfDestination", "CH", bill.B0_RN_NKCountryOfDestination);
	}
}
