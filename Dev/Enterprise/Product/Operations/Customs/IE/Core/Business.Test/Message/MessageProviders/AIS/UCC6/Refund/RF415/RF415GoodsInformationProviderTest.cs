using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class RF415GoodsInformationProviderTest : DataProviderTestCase<RF415GoodsInformationProvider>
	{
		public void TestCommodityCode()
		{
			Assert(Provider.CommodityCode is GoodsInformationTypeCommodityCodeProvider);
		}

		public void TestGoodsDescription()
		{
			SetUpTestData();
			entryLine.RandomLine.JI_Description = "Description of Goods";
			AssertEquals("Description of Goods", Provider.GoodsDescription);
		}

		public void TestGoodsQuantity()
		{
			AssertNull(Provider.GoodsQuantity);
		}

		public void TestCustomsValue()
		{
			Assert(Provider.CustomsValue is MoneyProvider);
		}

		public void TestTypeOfDuty()
		{
			AssertEquals(Array.Empty<ITypeOfDuty>(), Provider.TypeOfDuty);
		}

		public void TestNetMass()
		{
			AssertEquals(0m, Provider.NetMass);
		}

		public void TestSupplementaryUnitsValue()
		{
			AssertEquals(0m, Provider.SupplementaryUnitsValue);
		}

		protected override RF415GoodsInformationProvider GetProvider()
		{
			SetUpTestData();
			return new RF415GoodsInformationProvider(entryLine, entryHeaderWrapper);
		}

		void SetUpTestData()
		{
			if (entryLineWrapper == null)
			{
				(entryHeaderWrapper, entryLineWrapper) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
				entryLine = entryLineWrapper.EntryLine;
			}
		}

		EntryHeaderWrapper entryHeaderWrapper;
		EntryLineWrapper entryLineWrapper;
		CusEntryLine entryLine;
	}
}
