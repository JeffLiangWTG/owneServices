using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_JobDetailedAnalysisClientAgentTwoPeriodSummaryTest : ScriptTest
	{
		[TestDate(2015, 12, 01)]
		public void TestShipmentsFromCLMTypeConsolAreExcluded()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			consol.JK_TransportMode = "AIR";

			consol.JK_AgentType = "CLM";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment = testObjectCreator.CreateShipment("S001001", consol);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(-10);
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(-7);
			var job = testObjectCreator.CreateJob(shipment, false);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			cost.E6_OH_Creditor = testObjectCreator.AALSHI.PK;
			cost.E6_IsTaxAmountOverridden = true;
			cost.E6_OSGSTAmount_Calc = 0M;
			cost.E6_OSCostAmount = 100M;
			TestObjectCreator.SetAPInvoiceInfo(cost, "3", ZDateTime.Today, ZDateTime.Today.AddDays(25));

			Factory.Save();

			var results = RunScript();
			AssertEquals("Since the consol is attached to a CLM consol, no record should be there to display", 0, results.Rows.Count);

			consol.JK_AgentType = "AGT";
			Factory.Save();

			results = RunScript();
			AssertEquals("Since the consol is no longer attached to a CLM consol, 1 record should be there to display", 1, results.Rows.Count);
		}

		[TestDate(2016,02,10)]
		public void TestShipmentWithoutConsolAreIncluded()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateShipment("S001001");
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(-10);
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(-7);
			var job = testObjectCreator.CreateJob(shipment, false);

			var apInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, testObjectCreator.AALSHI, testObjectCreator.CC1.PK, ZDateTime.Today, false);
			var apLine = apInvoice.Lines[0];
			apLine.AL_JH = job.PK;

			testObjectCreator.CreateJobCharge(apLine, job, testObjectCreator.CC1, testObjectCreator.AUD);

			Factory.Save();

			var results = RunScript();
			AssertEquals("1 record should be returned", 1, results.Rows.Count);
		}

		DataTable RunScript()
		{
			string sql = string.Format(@"SELECT *
							FROM csfn_JobDetailedAnalysisClientAgentTwoPeriodSummary('{0}','AU','1900-01-01 00:00:00','2016-02-20 23:59:29','1900-01-01 00:00:00','2016-02-20 23:59:29','','F',NULL,NULL,NULL,NULL,NULL,NULL
							,NULL,NULL,NULL,NULL,'','',NULL,NULL,'','',NULL)", GlbCompany.CurrentCompany.PK.ToString());
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
