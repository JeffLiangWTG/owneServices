using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseSEACRAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var house = Bizo as CusSCAHouse;
			var result = ((CusSCAHouseSEACRAmendmentGeneratorForTest)Generator).UniqueIdentifierInfos;
			AssertEquals("Length", 4, result.Length);
			AssertEquals("Result[0]", house.OceanBill.CB_LloydsIMOInfo, result[0]);
			AssertEquals("Result[1]", house.OceanBill.CB_VoyageInfo, result[1]);
			AssertEquals("Result[2]", house.OceanBill.CB_OceanBillInfo, result[2]);
			AssertEquals("Result[3]", house.CA_HouseBillInfo, result[3]);
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusSCAHouseSEACRAmendmentGeneratorForTest(bizo as CusSCAHouse);

		protected override Type ExpectedMessageType => typeof(CMRSEACRMessage);

		protected override BusinessObject GetSavedBizo()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var result = ocean.HouseBills.AddNew();
			Factory.Save();
			return result;
		}

		sealed class CusSCAHouseSEACRAmendmentGeneratorForTest : CusSCAHouseSEACRAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusSCAHouseSEACRAmendmentGeneratorForTest(CusSCAHouse house) : base(house)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
