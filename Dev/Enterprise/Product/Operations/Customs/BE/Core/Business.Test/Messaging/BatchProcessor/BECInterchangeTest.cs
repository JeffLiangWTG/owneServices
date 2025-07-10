using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(BECInterchange))]
class BECInterchangeTest : EnterpriseBusinessObjectTestCase
{
	public void TestSetDefaultValues()
	{
		var interchange = Factory.New<BECInterchange>();
		AssertEquals(EDIInterchange.ApplicationCodes.BECustoms, interchange.EI_ApplicationCode);
	}

	public void TestIEDIInterchange()
	{
		Assert(Factory.New<BECInterchange>() is Integration.Customs.BE.IEDIInterchange);
	}
}
