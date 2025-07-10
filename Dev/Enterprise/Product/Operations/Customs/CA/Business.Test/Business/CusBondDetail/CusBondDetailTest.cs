using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusBondDetail))]
	sealed class CusBondDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var bondData = (CusBondDetail)GetNewBusinessObject();
			AssertEquals(ApplicationCodeList.Codes.CACustoms, bondData.PW_ApplicationCode);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var bondData2 = newFactory.Load<CusBondDetail>(bondData.PK);
			AssertEquals(ApplicationCodeList.Codes.CACustoms, bondData2.PW_ApplicationCode);
		}

		public void TestValidationAndLookups()
		{
			var bondData = Factory.New<CusBondDetail>();
			AssertEquals(typeof(CusBondDetailValidation), bondData.Validation.GetType());
			AssertEquals(typeof(CusBondDetailLookups), bondData.Lookups.GetType());
		}

		public void TestIsBondActive()
		{
			var bondData = Factory.New<CusBondDetail>();
			Assert(bondData.IsBondActive(ZDateTime.Today));

			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			Assert(bondData.IsBondActive(ZDateTime.Today));
			Assert(!bondData.IsBondActive(ZDateTime.Today.AddDays(-11)));

			bondData.PW_BondExpiryDate = ZDateTime.Today;
			Assert(bondData.IsBondActive(ZDateTime.Today));
			Assert(!bondData.IsBondActive(ZDateTime.Today.AddDays(1)));
		}

		public void TestIsContinuousBond()
		{
			var bondData = Factory.New<CusBondDetail>();
			Assert(!bondData.IsContinuousBond);

			bondData.PW_BondType = BondTypeList.Codes.ContinuousBond;
			Assert(bondData.IsContinuousBond);

			bondData.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			Assert(!bondData.IsContinuousBond);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<CusBondDetail>();
			result.Parent = factory.NewWithValidTestData<OrgHeader>();
			return result;
		}
	}
}
