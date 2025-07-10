using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class CASSCostLineTest : CASSDataTest
	{
		public abstract void TestVATAmount();

		public abstract void TestAdjustedCASSCost();

		public abstract void TestReadonlyProperties();

		public abstract void TestUpdateOriginalAmounts();

		protected override void AssertCommonReadonlyProperties()
		{
			base.AssertCommonReadonlyProperties();
			Assert("AirlinePrefix", (CASSDataForTest as CASSCostLine).AirlinePrefixInfo.ReadOnly);
			Assert("AWBSerialNumber", (CASSDataForTest as CASSCostLine).AWBSerialNumberInfo.ReadOnly);
			Assert("AgentCode", (CASSDataForTest as CASSCostLine).AgentCodeInfo.ReadOnly);
			Assert("DateAWBExecution", (CASSDataForTest as CASSCostLine).DateAWBExecutionInfo.ReadOnly);
			Assert("DateOfArrival", (CASSDataForTest as CASSCostLine).DateOfArrivalInfo.ReadOnly);
			Assert("DateOfDelivery", (CASSDataForTest as CASSCostLine).DateOfDeliveryInfo.ReadOnly);
			Assert("Origin", (CASSDataForTest as CASSCostLine).OriginInfo.ReadOnly);
			Assert("Destination", (CASSDataForTest as CASSCostLine).DestinationInfo.ReadOnly);
			Assert("DateOfArrival", (CASSDataForTest as CASSCostLine).WeightInfo.ReadOnly);
			Assert("DateOfDelivery", (CASSDataForTest as CASSCostLine).WeightUnitInfo.ReadOnly);
			Assert("Currency", (CASSDataForTest as CASSCostLine).CurrencyCodeInfo.ReadOnly);
			Assert("CostAmountRowFileValue", (CASSDataForTest as CASSCostLine).CostAmountRowFileValueInfo.ReadOnly);
			Assert("VATIndicator", (CASSDataForTest as CASSCostLine).VATIndicatorInfo.ReadOnly);
			Assert("VATAmountRowFileValue", (CASSDataForTest as CASSCostLine).VATAmountRowFileValueInfo.ReadOnly);
		}

		protected abstract void SetAmountFields(decimal seedValue);

		public void TestZDecimalsHaveCorrectDecimalPlacesCASSCostLine()
		{
			var line = CASSDataForTest as CASSCostLine;

			var currencyList = new List<string>
			{
				nameof(line.CostAmountRowFileValue),
				nameof(line.VATAmountRowFileValue),
				nameof(line.VATAmount),
				nameof(line.CASSCost)
			};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckNonLocalCurrency(currencyList, nameof(line.CurrencyISODecimalPlaces), nameof(line.CurrencyCode), line);
		}
	}
}
