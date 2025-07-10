using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionOrgProfitShareDetailsValidatorTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			orgAgentRelationship1.O3_OH_SendingAgent = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orgProfitShareDetails11 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship1, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");
			var orgProfitShareDetails12 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship1, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");

			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			orgAgentRelationship2.O3_OH_SendingAgent = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orgProfitShareDetails21 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship2, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");
			var orgProfitShareDetails22 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship2, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");

			var orgAgentRelationship3 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgProfitShareDetails31 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship3, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");
			var orgProfitShareDetails32 = testObjectCreator.CreateGatewayProfitShareRedistribution(orgAgentRelationship3, 50, 50, "AUSYD", "USLAX", "AIR", apportionmentMethod: "SHP");

			AssertValidation(new[] { orgProfitShareDetails11, orgProfitShareDetails21 }, false, "Multiple Profit Share Agreements are not supported!");

			orgProfitShareDetails11.O4_ShareLosses = false;
			orgProfitShareDetails12.O4_ShareLosses = true;
			AssertValidation(new[] { orgProfitShareDetails11, orgProfitShareDetails12 }, false, "All Profit Share Details do not share same losses.");

			orgProfitShareDetails21.O4_GatewayProfitApportionmentMethod = "SHP";
			orgProfitShareDetails22.O4_GatewayProfitApportionmentMethod = "CHG";
			AssertValidation(new[] { orgProfitShareDetails21, orgProfitShareDetails22 }, false, "All Profit Share Details do not share same Apportionment Method.");

			orgProfitShareDetails31.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeAll;
			orgProfitShareDetails32.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreight;
			AssertValidation(new[] { orgProfitShareDetails31, orgProfitShareDetails32 }, false, "All Profit Share Details do not share same Apply To.");

			testObjectCreator.CreateChargeCode("FRT");
			testObjectCreator.CreateChargeCode("BAF");
			testObjectCreator.CreateChargeCode("CAF");

			orgProfitShareDetails31.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			orgProfitShareDetails31.AgreementTypeDescription = "FRT,BAF";
			orgProfitShareDetails32.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeUserDefined;
			orgProfitShareDetails32.AgreementTypeDescription = "BAF,FRT";
			Factory.Save();
			AssertValidation(new[] { orgProfitShareDetails31, orgProfitShareDetails32 }, true, null);

			orgProfitShareDetails32.AgreementTypeDescription = "BAF,CAF";
			Factory.Save();
			AssertValidation(new[] { orgProfitShareDetails31, orgProfitShareDetails32 }, false, "All Profit Share Details do not share same Apply To.");

			void AssertValidation(IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, bool expected, string expectedError)
			{
				using (var logger = new TestLogger())
				{
					var validation = new GatewayProfitRedistributionOrgProfitShareDetailsValidator(orgProfitShareDetailsList, logger);
					AssertEquals(expected, validation.IsValid());
					AssertEquals(expectedError, logger.GetLogs(LogType.Error).FirstOrDefault());
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator testObjectCreator;
	}
}
