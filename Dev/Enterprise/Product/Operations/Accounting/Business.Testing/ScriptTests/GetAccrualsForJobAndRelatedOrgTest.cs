using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GetAccrualsForJobAndRelatedOrgTest : ScriptTest
	{
		public void TestGetAccrualsForJobAndRelatedOrg()
		{
			using (AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var currency = TestObjectCreator.AUD;
				var creditor = TestObjectCreator.AALSHI;
				var settlementGroupOrg = TestObjectCreator.Agent;
				creditor.AddRelatedParty(settlementGroupOrg.PK, RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

				var childOrg = TestObjectCreator.LocalClient;
				childOrg.AddRelatedParty(creditor.PK, RelatedPartyTypeList.Codes.APSettlementGroup, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
				var shipment = TestObjectCreator.CreateShipment("S001001", consol);
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
				var jobCharge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Job charge 1", currency, 100m, creditor, null, 0M, null);
				var jobCharge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "Job charge 2", TestObjectCreator.USD, 200m, creditor, null, 0M, null);
				var jobCharge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Job charge 3", currency, 300m, settlementGroupOrg, null, 0M, null);
				var jobCharge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "Job charge 4", currency, 400m, TestObjectCreator.LocalClient, null, 0M, null);

				var apportionemntListing = new ApportionmentListing(Factory, consol);
				var consolCost = apportionemntListing.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				consolCost.E6_OH_Creditor = creditor.PK;
				consolCost.E6_ApportionmentMethod = "SHP";
				consolCost.E6_OSCostAmount = 100M;

				Factory.Save();

				var apportionedCharge = job.Charges.Where(x => x.JR_E6 != ZGuid.Empty).FirstOrDefault();

				var resultTable = RunScript(job, creditor.PK, currency.RX_Code);
				var expectedResult = @"
JR_PK                                JR_JH                                JR_OH_CostAccount                    JR_OSCostAmt          JR_OSCostGSTAmt       JR_AT_CostGSTRate                    JR_IsCostTaxAmountOverridden   JR_RX_NKCostCurrency JR_E6                                E6_OSCostAmount       E6_OSGSTAmount        E6_AT_TaxRate         E6_IsTaxAmountOverridden   AC_PK                      AC_Code    OH_Code      JH_GS_NKRepOps AL_SystemCreateUser OrgType            JH_JobNum                           JK_UniqueConsignRef
------------------------------------ ------------------------------------ ------------------------------------ --------------------- --------------------- ------------------------------------ ------------------------------ -------------------- ------------------------------------ --------------------- --------------------- --------------------- -------------------------- -------------------------- ---------- ------------ -------------- ------------------- ------------------ ----------------------------------- --------------------
JobCharge1PK                         JobPK                                OriginalCreditorPK                   100.00                0.00                  JobCharge1TaxPK                      0                              AUD                  NULL                                 NULL                  NULL                  NULL                  NULL                       charge3PK                  ZZCC3      AALSHI       E              E                   OriginalOrg        S001001                             NULL
apportionedChargePK                  JobPK                                OriginalCreditorPK                   100.00                0.00                  appChargeTaxPK                       0                              AUD                  JR_E6                                100.00                0.00                  appChargeTaxPK        0                          charge2PK                  ZZCC2      AALSHI       E              E                   OriginalOrg        S001001                             C001001
JobCharge3PK                         JobPK                                settlementGroupOrgPK                 300.00                0.00                  NULL                                 0                              AUD                  NULL                                 NULL                  NULL                  NULL                  NULL                       charge3PK                  ZZCC3      ZAgent       E              E                   SettlementGroupOrg S001001                             NULL
JobCharge4PK                         JobPK                                childOrgPK                           400.00                0.00                  NULL                                 0                              AUD                  NULL                                 NULL                  NULL                  NULL                  NULL                       charge4PK                  ZZCC4      ZLOCCLT      E              E                   ChildOrg           S001001                             NULL
";
				var pkReplacment = new List<Tuple<ZGuid, string>>
					(
						new[]
						{
						new Tuple<ZGuid, string>(job.PK, "JobPK")
						, new Tuple<ZGuid, string>(jobCharge1.PK, "JobCharge1PK")
						, new Tuple<ZGuid, string>(jobCharge3.PK, "JobCharge3PK")
						, new Tuple<ZGuid, string>(jobCharge4.PK, "JobCharge4PK")
						, new Tuple<ZGuid, string>(apportionedCharge.PK, "apportionedChargePK")
						, new Tuple<ZGuid, string>(consolCost.PK, "JR_E6")
						, new Tuple<ZGuid, string>(creditor.PK, "OriginalCreditorPK")
						, new Tuple<ZGuid, string>(settlementGroupOrg.PK, "settlementGroupOrgPK")
						, new Tuple<ZGuid, string>(childOrg.PK, "childOrgPK")
						, new Tuple<ZGuid, string>(jobCharge1.JR_AT_CostGSTRate, "JobCharge1TaxPK")
						, new Tuple<ZGuid, string>(apportionedCharge.JR_AT_CostGSTRate, "appChargeTaxPK")
						, new Tuple<ZGuid, string>(TestObjectCreator.CC2.PK, "charge2PK")
						, new Tuple<ZGuid, string>(TestObjectCreator.CC3.PK, "charge3PK")
						, new Tuple<ZGuid, string>(TestObjectCreator.CC4.PK, "charge4PK")
						}
					);
				AssertTableAsTextFromSQLServerManagenentStudio("", resultTable, expectedResult, new List<string>(), pkReplacment);
			}
		}

		DataTable RunScript(Job job, ZGuid orgPK, ZString currency)
		{
			var sql = @"
SELECT
	JR_PK
	, JR_JH
	, JR_OH_CostAccount
	, JR_OSCostAmt
	, JR_OSCostGSTAmt
	, JR_AT_CostGSTRate
	, JR_IsCostTaxAmountOverridden
	, JR_RX_NKCostCurrency
	, JR_E6
	, E6_OSCostAmount
	, E6_OSGSTAmount
	, E6_AT_TaxRate
	, E6_IsTaxAmountOverridden
	, AC_PK
	, AC_Code
	, OH_Code
	, JH_GS_NKRepOps
	, AL_SystemCreateUser
	, OrgType
	, JH_JobNum
	, JK_UniqueConsignRef
FROM
	GetAccrualsForJobAndRelatedOrg(@JobList, @CurrencyList, @OrgPK, @CompanyPK)";

			var jobTable = new DataTable();
			jobTable.Columns.Add("Value", typeof(Guid));
			jobTable.Rows.Add(job.PK.ToGuid());

			var currencyTable = new DataTable();
			currencyTable.Columns.Add("Value", typeof(string));
			currencyTable.Rows.Add(currency);

			var cmd = Db.Connection.Command(sql); // Calling DB function, which cannot be loaded through BusinessObjectFactory
			cmd.AddTableValuedParameter("@JobList", "dbo.TVP_uniqueidentifier", jobTable); //for now the table will include only one job, as we are reading the first line only. But later when we will read all the lines then TVP will be useful
			cmd.AddTableValuedParameter("@CurrencyList", "dbo.TVP_char_3", currencyTable); //for now the table will include only one currency, as we are reading the first line only. But later when we will read all the lines then TVP will be useful
			cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
			cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());

			return DataUtils.GetDataTableFromCommand(cmd);
		}
	}
}
