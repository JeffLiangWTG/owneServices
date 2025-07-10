using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionValidatorTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			var expectedLogs = @"Starting Consols' Validations ...
There is no consol for processing.";
			AssertValidation(System.Array.Empty<ForwardingConsol>(), System.Array.Empty<OrgProfitShareDetails>(), false, "There is no consol for processing.");

			var consol = testObjectCreator.CreateConsol();
			consol.JK_UniqueConsignRef = "CN00001";
			var gatewayConsol = profitShareTestHelper.CreateConsol("CN00002");

			expectedLogs = @"Starting Consols' Validations ...
CN00001 is not a Gateway Consol.
CN00001 has failed to pass validation.
CN00002 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
There is no profit share rule for processing.";
			AssertValidation(new[] { consol, gatewayConsol }, System.Array.Empty<OrgProfitShareDetails>(), false, "CN00001 is not a Gateway Consol.", "There is no profit share rule for processing.");

			expectedLogs = @"Starting Consols' Validations ...
CN00002 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
There is no profit share rule for processing.";
			AssertValidation(new[] { gatewayConsol }, System.Array.Empty<OrgProfitShareDetails>(), false, "There is no profit share rule for processing.");

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			orgAgentRelationship1.O3_OH_SendingAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orgProfitShareDetails11 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship1, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");
			var orgProfitShareDetails12 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship1, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "CHG");

			expectedLogs = @"Starting Consols' Validations ...
CN00002 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
All Profit Share Details do not share same Apportionment Method.
ProfitShare rules' Validations is completed.
Starting Shipments' Validations ...
There is no shipment for processing.";
			AssertValidation(new[] { gatewayConsol }, new[] { orgProfitShareDetails11, orgProfitShareDetails12 }, false, "All Profit Share Details do not share same Apportionment Method.", "There is no shipment for processing.");

			orgProfitShareDetails12.O4_GatewayProfitApportionmentMethod = "SHP";
			expectedLogs = @"Starting Consols' Validations ...
CN00002 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
ProfitShare rules' Validations is completed.
Starting Shipments' Validations ...
There is no shipment for processing.";
			AssertValidation(new[] { gatewayConsol }, new[] { orgProfitShareDetails11, orgProfitShareDetails12 }, false, "There is no shipment for processing.");

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			profitShareTestHelper.CreateShipment("S000001", pickupAgent: pickupAgent, forwardingConsols: new[] { gatewayConsol }, changeAgentsAsCreditor: false);

			expectedLogs = @"Starting Consols' Validations ...
CN00002 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
ProfitShare rules' Validations is completed.
Starting Shipments' Validations ...
S000001: Pickup Agent is not a creditor.
S000001 has failed to pass validation.
Shipments' Validations is completed.";
			AssertValidation(new[] { gatewayConsol }, new[] { orgProfitShareDetails11, orgProfitShareDetails12 }, false, "S000001: Pickup Agent is not a creditor.");

			pickupAgent.OH_IsCreditor = true;
			expectedLogs = @"Starting Consols' Validations ...
CN00002 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
ProfitShare rules' Validations is completed.
Starting Shipments' Validations ...
S000001 is validated successfully.
Shipments' Validations is completed.";
			AssertValidation(new[] { gatewayConsol }, new[] { orgProfitShareDetails11, orgProfitShareDetails12 }, true);

			void AssertValidation(IEnumerable<ForwardingConsol> forwardingConsols, IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, bool expected, params string[] expectedErrors)
			{
				using (var logger = new TestLogger())
				{
					var profitShareForwardingConsolWrapper = forwardingConsols.Select(x => new ProfitShareForwardingConsolWrapper(x));
					var validation = new GatewayProfitRedistributionValidator(Factory, profitShareForwardingConsolWrapper, orgProfitShareDetailsList, logger);
					AssertEquals(expected, validation.IsValid());
					AssertEquals(expectedLogs, logger.DumpLogs());
					AssertContainsExactElementsInAnyOrder(expectedErrors, logger.GetLogs(LogType.Error).ToArray());
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PSR";
			chargeCode.AC_Desc = "Profit Share Redistribution";
			Factory.Save();

			profitShareTestHelper.SetRegistry(chargeCode.PK);
		}

		ProfitShareTestHelper profitShareTestHelper;
		TestObjectCreator testObjectCreator;
	}
}
