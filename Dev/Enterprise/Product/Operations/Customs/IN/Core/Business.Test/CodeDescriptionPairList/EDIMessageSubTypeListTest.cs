using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(EDIMessageSubTypeList))]
sealed class EDIMessageSubTypeListTest : TestCaseWithFactory
{
	public void TestGetNegativeSubType()
	{
		AssertEquals(EDIMessageSubTypeList.Codes.AirCgmNegativeAcknowledgement, EDIMessageSubTypeList.GetNegativeSubType(EDIMessageSubTypeList.Codes.AirCgm));
		AssertEquals(EDIMessageSubTypeList.Codes.SeaCgmNegativeAcknowledgement, EDIMessageSubTypeList.GetNegativeSubType(EDIMessageSubTypeList.Codes.SeaCgm));
		AssertEquals(EDIMessageSubTypeList.Codes.ShippingBillNegativeAcknowledgement, EDIMessageSubTypeList.GetNegativeSubType(EDIMessageSubTypeList.Codes.ShippingBillFresh));
		AssertEquals(ZString.Empty, EDIMessageSubTypeList.GetNegativeSubType("InvalidSubType"));
	}
}
