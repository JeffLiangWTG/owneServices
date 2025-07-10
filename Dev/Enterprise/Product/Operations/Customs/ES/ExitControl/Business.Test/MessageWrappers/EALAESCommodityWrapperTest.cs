using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESCommodityWrapperTest : WrapperHelperTest<EALAESCommodityWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if consignmentItem is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "consignmentItem"), () => GetWrapper(null, 0m, 0m));
		}

		public void TestGoodsMeasure()
		{
			CombineAssertions(() =>
			{
				var goodsMeasure = wrapper.GoodsMeasure;
				AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);
				AssertSame("Cached GoodsMeasure", wrapper.GoodsMeasure, goodsMeasure);

				AssertEquals("Expected 0 GoodsMeasure.GrossWeight when grossMass is 0", 0m, goodsMeasure.GrossWeight);
				AssertEquals("Expected false GoodsMeasure.GrossWeightSpecified when grossMass is 0", false, goodsMeasure.GrossWeightSpecified);
				AssertEquals("Expected 0 GoodsMeasure.NetWeight when netMass is 0", 0m, goodsMeasure.NetWeight);
				AssertEquals("Expected false GoodsMeasure.NetWeightSpecified when netMass is 0", false, goodsMeasure.NetWeightSpecified);

				wrapper = GetWrapper(exitConsignmentItem, 20m, 10m);
				goodsMeasure = wrapper.GoodsMeasure;
				AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);

				AssertEquals("Expected filled GoodsMeasure.GrossWeight when grossMass is not 0", 20m, goodsMeasure.GrossWeight);
				AssertEquals("Expected true GoodsMeasure.GrossWeightSpecified when grossMass is not 0", true, goodsMeasure.GrossWeightSpecified);
				AssertEquals("Expected filled GoodsMeasure.NetWeight when netMass is not 0", 10m, goodsMeasure.NetWeight);
				AssertEquals("Expected true GoodsMeasure.NetWeightSpecified when netMass is not 0", true, goodsMeasure.NetWeightSpecified);

				exitConsignmentItem.CCI_GrossMass = 20m;
				exitConsignmentItem.CCI_NetMass = 10m;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 10m);
				goodsMeasure = wrapper.GoodsMeasure;
				AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);

				AssertEquals("Expected 0 GoodsMeasure.GrossWeight when grossMass is not 0 but it's the same as the grossMass in the consignment item", 0m, goodsMeasure.GrossWeight);
				AssertEquals("Expected false GoodsMeasure.GrossWeightSpecified when grossMass is not 0 but it's the same as the grossMass in the consignment item", false, goodsMeasure.GrossWeightSpecified);
				AssertEquals("Expected 0 GoodsMeasure.NetWeight when netMass is not 0 but it's the same as the grossMass in the consignment item", 0m, goodsMeasure.NetWeight);
				AssertEquals("Expected false GoodsMeasure.NetWeightSpecified when netMass is not 0 but it's the same as the grossMass in the consignment item", false, goodsMeasure.NetWeightSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitConsignmentItem = Factory.New<CusExitConsignmentItem>();

			wrapper = GetWrapper(exitConsignmentItem, 0m, 0m);
		}
		CusExitConsignmentItem exitConsignmentItem;
		EALAESCommodityWrapper wrapper;

		EALAESCommodityWrapper GetWrapper(CusExitConsignmentItem exitConsignmentItem, ZDecimal grossMass, ZDecimal netMass) => new EALAESCommodityWrapper(exitConsignmentItem, grossMass, netMass);

		protected override EALAESCommodityWrapper GetProvider() => wrapper;
	}
}
