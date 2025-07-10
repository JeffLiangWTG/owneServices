using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionApportionmentCalculatorTest : TestCaseWithFactory
	{
		public void TestImplementation_InputsAreNullOrEmpty()
		{
			//to ensure that system do not throw any exception if any of the value is passed as null or empty,
			//it should just return share as 0 in all such cases.
			AssertImplementation(null, null, null, null);
			AssertImplementation(Array.Empty<ForwardingConsol>(), null, null);
			AssertImplementation(null, Array.Empty<ForwardingShipment>(), null);
			AssertImplementation(Array.Empty<ForwardingConsol>(), Array.Empty<ForwardingShipment>(), null);
			AssertImplementation(Array.Empty<ForwardingConsol>(), Array.Empty<ForwardingShipment>(), "");

			var shipment = testObjectCreator.CreateShipment("SHP001");
			AssertImplementation(Array.Empty<ForwardingConsol>(), new[] { shipment }, null);
			AssertImplementation(Array.Empty<ForwardingConsol>(), new[] { shipment }, "");
			AssertImplementation(Array.Empty<ForwardingConsol>(), new[] { shipment }, null, isNullCriteria: true);

			var consol = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", -100);
			AssertImplementation(new[] { consol }, null, null, null);

			void AssertImplementation(IEnumerable<ForwardingConsol> consols,
				IEnumerable<IJobInvoicingPlugIn> shipments,
				string profitApportionmentMethod,
				string agreementType = "ALL",
				AccChargeCode[] accChargeCodes = null,
				bool isNullCriteria = false)
			{
				var criteria = isNullCriteria ? null : new GatewayProfitRedistributionApportionmentCriteria(Factory, forwardingProfitShareRedistribution, profitApportionmentMethod, agreementType, false, accChargeCodes);
				var calculator = CreateCalculator(consols, shipments, criteria);
				AssertCalculation(calculator, Guid.Empty, 0m);
			}
		}

		public void TestCalculateProfitPerShipment()
		{
			var consol1 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", -100);
			var consol2 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 200);
			var consol3 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 300);

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1");
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2");
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3");
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4");

			var consols = new[] { consol1, consol2, consol3 };
			var shipments = new[] { shipment1, shipment2, shipment3, shipment4 };

			//Total Profit without loss / Number of Shipment = (200+300)/4 = 500/4 = 125
			var calculator = CreateCalculator(consols, shipments, "SHP");
			//calling CalculateProfitPerShipment multiple times should not affect calculation
			calculator.CalculateProfitPerShipment();
			calculator.CalculateProfitPerShipment();
			AssertCalculation(calculator, shipment1.PK, 125m);
			AssertCalculation(calculator, shipment2.PK, 125m);
			AssertCalculation(calculator, shipment3.PK, 125m);
			AssertCalculation(calculator, shipment4.PK, 125m);

			//duplicate shipments should not affect calculation
			shipments = new[] { shipment1, shipment1, shipment2, shipment2, shipment3, shipment3, shipment4, shipment4 };
			calculator = CreateCalculator(consols, shipments, "SHP");
			AssertCalculation(calculator, shipment1.PK, 125m);
			AssertCalculation(calculator, shipment2.PK, 125m);
			AssertCalculation(calculator, shipment3.PK, 125m);
			AssertCalculation(calculator, shipment4.PK, 125m);

			//duplicate consols should not affect calculation
			consols = new[] { consol1, consol1, consol2, consol2, consol3, consol3 };
			calculator = CreateCalculator(consols, shipments, "SHP");
			AssertCalculation(calculator, shipment1.PK, 125m);
			AssertCalculation(calculator, shipment2.PK, 125m);
			AssertCalculation(calculator, shipment3.PK, 125m);
			AssertCalculation(calculator, shipment4.PK, 125m);
		}

		public void TestGetShipmentShare()
		{
			var consol1 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 100);
			var consol2 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 200);
			var consol3 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 300);

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1", chargeable: 60, weight: 48, volume: 0.36);
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2", chargeable: 44, weight: 44, volume: 0.24);
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3", chargeable: 24, weight: 24, volume: 0.12);
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4", chargeable: 100, weight: 92, volume: 0.6);

			AssertChargeablePreCondition(shipment1, 60m, "KG");
			AssertChargeablePreCondition(shipment2, 44m, "KG");
			AssertChargeablePreCondition(shipment3, 24m, "KG");
			AssertChargeablePreCondition(shipment4, 100m, "KG");

			var consols = new[] { consol1, consol2, consol3 };
			var shipments = new[] { shipment1, shipment2, shipment3, shipment4 };

			var calculator = CreateCalculator(consols, shipments, "");
			AssertCalculation(calculator, shipment1.PK, 0m);
			AssertCalculation(calculator, shipment2.PK, 0m);
			AssertCalculation(calculator, shipment3.PK, 0m);
			AssertCalculation(calculator, shipment4.PK, 0m);

			//Total Profit / Number of Shipment = 600/4 = 150
			calculator = CreateCalculator(consols, shipments, "SHP");
			AssertCalculation(calculator, shipment1.PK, 150m);
			AssertCalculation(calculator, shipment2.PK, 150m);
			AssertCalculation(calculator, shipment3.PK, 150m);
			AssertCalculation(calculator, shipment4.PK, 150m);

			//(Total Profit / Total Chargeable) * Individual Shipment Chargeable(ISC) = (600/228) * ISC
			calculator = CreateCalculator(consols, shipments, "CHG");
			AssertCalculation(calculator, shipment1.PK, 157.89m); //(600/228) * 60
			AssertCalculation(calculator, shipment2.PK, 115.79m); //(600/228) * 44 
			AssertCalculation(calculator, shipment3.PK, 63.16m);  //(600/228) * 24
			AssertCalculation(calculator, shipment4.PK, 263.16m); //(600/228) * 100

			//(Total Profit / Total Weight) * Individual Shipment Weight(ISW) = (600/208) * ISW
			calculator = CreateCalculator(consols, shipments, "GWT");
			AssertCalculation(calculator, shipment1.PK, 138.46m); //(600/208) * 48
			AssertCalculation(calculator, shipment2.PK, 126.92m); //(600/208) * 44 
			AssertCalculation(calculator, shipment3.PK, 69.23m);  //(600/208) * 24
			AssertCalculation(calculator, shipment4.PK, 265.38m); //(600/208) * 92

			//(Total Profit / Total Volume) * Individual Shipment Volume(ISV) = (600/1.32) * ISV
			calculator = CreateCalculator(consols, shipments, "GVT");
			AssertCalculation(calculator, shipment1.PK, 163.64m); //(600/1.32) * 48
			AssertCalculation(calculator, shipment2.PK, 109.09m); //(600/1.32) * 44 
			AssertCalculation(calculator, shipment3.PK, 54.55m);  //(600/1.32) * 24
			AssertCalculation(calculator, shipment4.PK, 272.73m); //(600/1.32) * 92
		}

		public void TestGetShipmentShare_UnitsAreNotSame()
		{
			var consol1 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 100, transportMode: "SEA");
			var consol2 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 200, transportMode: "SEA");

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1", weight: 100, weightUnit: "KG", volume: 0.75, volumeUnit: "M3", transportMode: "AIR");
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2", weight: 110, weightUnit: "LB", volume: 0.01, volumeUnit: "M3", transportMode: "SEA");
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3", weight: 1, weightUnit: "LB", volume: 3000, volumeUnit: "L", transportMode: "SEA");

			AssertChargeablePreCondition(shipment1, 125m, "KG");
			AssertChargeablePreCondition(shipment2, 0.05m, "M3");
			AssertChargeablePreCondition(shipment3, 3.0m, "M3");

			var consols = new[] { consol1, consol2 };
			var shipments = new[] { shipment1, shipment2, shipment3 };

			//Air Conversion Factor - 6000 CC/KG and 166 CI/LB
			//Sea Conversion Factor - 1000 KG/M3 and 100 LB/CF

			//Total Chargeable in KG = 125 + (0.05 * 1000) + (3.0 * 1000) = 125 + 50 + 3000 = 3175
			//(Total Profit / Total Chargeable in KG) * Individual Shipment Converted(in KG) Chargeable(ISC) = (300/3175) * ISC
			var calculator = CreateCalculator(consols, shipments, "CHG");
			AssertCalculation(calculator, shipment1.PK, 11.81m); //(300/3175) * 125
			AssertCalculation(calculator, shipment2.PK, 4.72m);  //(300/3175) * 50
			AssertCalculation(calculator, shipment3.PK, 283.46m);//(300/3175) * 3000

			//1 LB = 0.45359237 Kg
			//Total Weight in KG = 100 + (110 * 0.45359237) + (1 * 0.45359237) = 100 + 49.89516070 + 0.45359237 = 150.34875307
			//(Total Profit / Total Weight in KG) * Individual Shipment Converted(in KG) Weight(ISW) = (300/150.34875307) * ISW
			calculator = CreateCalculator(consols, shipments, "GWT");
			AssertCalculation(calculator, shipment1.PK, 199.54m);//(300/150.34875307) * 100
			AssertCalculation(calculator, shipment2.PK, 99.56m); //(300/150.34875307) * 49.89516070
			AssertCalculation(calculator, shipment3.PK, 0.91m);  //(300/150.34875307) * 0.45359237

			//1 L = 0.001 M3
			//Total Volume in M3 = 0.75 + 0.01 + (3000 * 0.001) = 0.75 + 0.01 + 3.00 = 3.76
			//(Total Profit / Total Volume in M3) * Individual Shipment Converted(in M3) Volume(ISV) = (300/3.76) * ISV
			calculator = CreateCalculator(consols, shipments, "GVT");
			AssertCalculation(calculator, shipment1.PK, 59.84m); //(300/3.76) * 0.75
			AssertCalculation(calculator, shipment2.PK, 0.80m);  //(300/3.76) * 0.01
			AssertCalculation(calculator, shipment3.PK, 239.36m);//(300/3.76) * 3.00
		}

		public void TestGetShipmentShare_ShareLosses()
		{
			var consol1 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", -100);
			var consol2 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 200);
			var consol3 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 300);

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1");
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2");
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3");
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4");

			var consols = new[] { consol1, consol2, consol3 };
			var shipments = new[] { shipment1, shipment2, shipment3, shipment4 };

			//Total Profit with loss / Number of Shipment = (-100+200+300)/4 = 400/4 = 100
			var calculator = CreateCalculator(consols, shipments, "SHP", true);
			AssertCalculation(calculator, shipment1.PK, 100m);
			AssertCalculation(calculator, shipment2.PK, 100m);
			AssertCalculation(calculator, shipment3.PK, 100m);
			AssertCalculation(calculator, shipment4.PK, 100m);
			AssertEquals(@"Starting Consols' profit Calculation...
CN00001 Total Profit (AUD): -100.
CN00001 Total Profit for redistribution (AUD): -100.
CN00001 Total Profit (AUD): 200.
CN00001 Total Profit for redistribution (AUD): 200.
CN00001 Total Profit (AUD): 300.
CN00001 Total Profit for redistribution (AUD): 300.
Total Consols Profit (AUD): 400
Total Consols Profit for redistribution (AUD): 400
Calculating Total Shipments' chargeables...
Apportionment Method: SHP
SHP1 chargeable by Profit Apportionment Method: 1
SHP2 chargeable by Profit Apportionment Method: 1
SHP3 chargeable by Profit Apportionment Method: 1
SHP4 chargeable by Profit Apportionment Method: 1
Total Shipments' chargeables: 4
SHP1 Shares: 100
SHP2 Shares: 100
SHP3 Shares: 100
SHP4 Shares: 100", logger.DumpLogs());

			logger = new TestLogger();//to clear logs

			//Total Profit without loss / Number of Shipment = (200+300)/4 = 500/4 = 125
			calculator = CreateCalculator(consols, shipments, "SHP", false);
			AssertCalculation(calculator, shipment1.PK, 125m);
			AssertCalculation(calculator, shipment2.PK, 125m);
			AssertCalculation(calculator, shipment3.PK, 125m);
			AssertCalculation(calculator, shipment4.PK, 125m);
			AssertEquals(@"Starting Consols' profit Calculation...
CN00001 Total Profit (AUD): -100.
CN00001 Total Profit for redistribution (AUD): 0.
CN00001 Total Profit (AUD): 200.
CN00001 Total Profit for redistribution (AUD): 200.
CN00001 Total Profit (AUD): 300.
CN00001 Total Profit for redistribution (AUD): 300.
Total Consols Profit (AUD): 400
Total Consols Profit for redistribution (AUD): 500
Calculating Total Shipments' chargeables...
Apportionment Method: SHP
SHP1 chargeable by Profit Apportionment Method: 1
SHP2 chargeable by Profit Apportionment Method: 1
SHP3 chargeable by Profit Apportionment Method: 1
SHP4 chargeable by Profit Apportionment Method: 1
Total Shipments' chargeables: 4
SHP1 Shares: 125
SHP2 Shares: 125
SHP3 Shares: 125
SHP4 Shares: 125", logger.DumpLogs());
		}

		public void TestGetShipmentShare_ApplyTo()
		{
			var chargeCode1 = testObjectCreator.CreateChargeCode("CH1");
			var chargeCode2 = testObjectCreator.CreateChargeCode("CH2");

			var consol1 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 100, chargeCode1.PK);
			var consol2 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 200, chargeCode1.PK);
			var consol3 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 300, chargeCode2.PK);

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1");
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2");
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3");
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4");

			var consols = new[] { consol1, consol2, consol3 };
			var shipments = new[] { shipment1, shipment2, shipment3, shipment4 };

			var criteria = new GatewayProfitRedistributionApportionmentCriteria(Factory, forwardingProfitShareRedistribution, "SHP", OrgProfitShareDetailsLookups.AgreementTypeUserDefined, false, new[] { chargeCode1 });
			var calculator = CreateCalculator(consols, shipments, criteria);

			//Total Profit after filering chargeCode2 / Number of Shipment = (100+200)/4 = 300/4 = 75
			AssertCalculation(calculator, shipment1.PK, 75m);
			AssertCalculation(calculator, shipment2.PK, 75m);
			AssertCalculation(calculator, shipment3.PK, 75m);
			AssertCalculation(calculator, shipment4.PK, 75m);
		}

		public void TestGetShipmentShare_Precision()
		{
			//this test is to ensure that we do not round off to get max precision in profit calculation,
			//once we generate profit, we will round off
			var consol1 = profitShareTestHelper.CreateConsol("AUSYD", "SGSIN", "CN00001", 3.35m);

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1");
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2");
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3");
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4");

			var consols = new[] { consol1 };
			var shipments = new[] { shipment1, shipment2, shipment3, shipment4 };

			var criteria = new GatewayProfitRedistributionApportionmentCriteria(Factory, forwardingProfitShareRedistribution, "SHP", OrgProfitShareDetailsLookups.AgreementTypeAll, false, null);
			var calculator = CreateCalculator(consols, shipments, criteria);

			AssertCalculation(calculator, shipment1.PK, 0.8375m, roundOff: false);
			AssertCalculation(calculator, shipment2.PK, 0.8375m, roundOff: false);
			AssertCalculation(calculator, shipment3.PK, 0.8375m, roundOff: false);
			AssertCalculation(calculator, shipment4.PK, 0.8375m, roundOff: false);
		}

		void AssertChargeablePreCondition(ForwardingShipment shipment, decimal chargeable, string chargeableUnit)
		{
			AssertEquals("pre-condition:ChargeableAmount", chargeable, shipment.JS_ActualChargeable);
			AssertEquals("pre-condition:ChargeableUnit", chargeableUnit, shipment.JS_ChargeableUnit);
		}

		GatewayProfitRedistributionApportionmentCalculator CreateCalculator(IEnumerable<ForwardingConsol> consols,
			IEnumerable<IJobInvoicingPlugIn> shipments,
			string profitApportionmentMethod,
			bool shareLosses = false)
		{
			return CreateCalculator(consols, shipments,
				new GatewayProfitRedistributionApportionmentCriteria(Factory, forwardingProfitShareRedistribution, profitApportionmentMethod, OrgProfitShareDetailsLookups.AgreementTypeAll, shareLosses, null));
		}

		GatewayProfitRedistributionApportionmentCalculator CreateCalculator(IEnumerable<ForwardingConsol> consols,
			IEnumerable<IJobInvoicingPlugIn> shipments,
			GatewayProfitRedistributionApportionmentCriteria criteria)
		{
			var calculator = new GatewayProfitRedistributionApportionmentCalculator(consols?.Select(consol => new ProfitShareForwardingConsolWrapper(consol)), shipments, criteria, logger);
			calculator.CalculateProfitPerShipment();
			return calculator;
		}

		void AssertCalculation(GatewayProfitRedistributionApportionmentCalculator calculator,
			ZGuid shipmentPK,
			decimal expectedShareAmount, bool roundOff = true)
		{
			var result = calculator.GetShipmentShare(shipmentPK);

			if (roundOff)
			{
				//we are rounding off, by default, so our test are readable. but, we won't round off until we assign profit.
				result = AccountingUtils.Round(result, GlbCompany.CurrentCompany.LocalCurrency);
			}

			AssertEquals(expectedShareAmount, result);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);
			forwardingProfitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();
			logger = new TestLogger();
		}

		protected override void TearDown()
		{
			base.TearDown();

			testObjectCreator = null;
			profitShareTestHelper = null;
			forwardingProfitShareRedistribution = null;
			logger.Dispose();
			logger = null;
		}

		TestObjectCreator testObjectCreator;
		ProfitShareTestHelper profitShareTestHelper;
		ForwardingProfitShareRedistribution forwardingProfitShareRedistribution;
		IDisposableProfitShareRedistributionLogger logger;
	}
}
