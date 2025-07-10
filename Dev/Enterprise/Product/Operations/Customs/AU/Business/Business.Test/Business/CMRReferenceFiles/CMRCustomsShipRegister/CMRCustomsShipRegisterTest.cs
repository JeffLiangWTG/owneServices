using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCustomsShipRegister))]
	sealed class CMRCustomsShipRegisterTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRCustomsShipRegister.New(Factory);
	}
}
