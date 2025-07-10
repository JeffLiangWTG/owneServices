using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceDiscountGroupMember))]
	internal class EdiPriceDiscountGroupMemberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<EdiPriceHeaderDiscount>();
			var bizo = Factory.New<EdiPriceDiscountGroupMember>();
			bizo.PGM_PHD = header.PK;
			return bizo;
		}

		public void TestDiscountNameNullRef()
		{
			var member = Factory.New<EdiPriceDiscountGroupMember>();
			AssertEquals("", member.DiscountName);
			AssertEquals(ZGuid.Empty, member.PGM_PHD);
		}
	}
}
