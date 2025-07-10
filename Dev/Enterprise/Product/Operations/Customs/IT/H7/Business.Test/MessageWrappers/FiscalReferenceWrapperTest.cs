using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(FiscalReferenceWrapper))]
sealed class FiscalReferenceWrapperTest : DataProviderTestCase<FiscalReferenceWrapper>
{
	public void TestIdentificationNumber()
	{
		AssertEquals(nameof(FiscalReferenceWrapper.IdentificationNumber), "123456", Provider.IdentificationNumber);
	}

	public void TestRole()
	{
		AssertEquals(nameof(FiscalReferenceWrapper.Role), "FR5", Provider.Role);
	}

	protected override FiscalReferenceWrapper GetProvider()
	{
		return new FiscalReferenceWrapper("123456", FiscalReferenceCodeList.Codes.FR5_Vendor);
	}
}
