using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCargoDescFee))]
	class NctsCargoDescFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<NctsCargoDescFeeValidation>(GetNewNctsCargoDescFee(Factory).Validation);
		}

		public void TestLookups()
		{
			AssertType<NctsCargoDescFeeLookups>(GetNewNctsCargoDescFee(Factory).Lookups);
		}

		public void TestTypeDecider()
		{
			AssertType<NctsCargoDescFeeTypeDecider>(NctsCargoDescFee.TypeDecider);
		}

		public void TestSupportsClone()
		{
			AssertEquals("SupportsClone", true, GetNewBusinessObject().SupportsClone());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewNctsCargoDescFee(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewNctsCargoDescFee(factory);

		NctsCargoDescFee GetNewNctsCargoDescFee(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			return goodsItem.Fees.AddNew();
		}
	}
}
