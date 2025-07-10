using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.APAutomation.APReconciliation.Helpers;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.APReconciliation.Testing
{
	public class DraftInvoiceHeaderBasedAPReconciliationAccrualSourceTest : TestCaseWithFactory
	{
		public void TestFATDraftInvoiceReconciliation()
		{
			//Build consols
			var consols = new List<IJobCostingPlugIn>();
			for (var i = 1; i <= 10; i++)
			{
				var forwardingConsol = ObjectCreator.CreateConsol(consolNum: "C000" + i.ToString());
				var shipment = ObjectCreator.CreateShipment("S000" + i.ToString(), forwardingConsol);
				consols.Add(forwardingConsol);

				_ = ObjectCreator.CreateConsolCost(forwardingConsol, ObjectCreator.FRT, ObjectCreator.AUD, 1M, 200M, ObjectCreator.Creditor1);
			}

			//Build Shipments
			var shipments = new List<ForwardingShipment>();
			for (var i = 1; i <= 10; i++)
			{
				var shipment = ObjectCreator.CreateShipment("S100" + i.ToString());
				var job = ObjectCreator.CreateJob(shipment);
				shipments.Add(shipment);
				_ = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "charge 01", costCurrency: ObjectCreator.AUD, osCostAmt: 100M, creditor: ObjectCreator.Creditor1);
			}

			Factory.Save();

			//Build clusters with a mix of consol and jobs
			var draftInvoice = ObjectCreator.CreateDraftInvoice("ADI 0001", "ADI-IREF-001", ObjectCreator.Creditor1.PK, 3000, 0, "AUD");
			foreach (var shipment in shipments)
			{
				var cluster = ObjectCreator.AddClusterToDraftTransaction(draftInvoice, 100);
				_ = ObjectCreator.AddJobToTheCluster(cluster, shipment);
			}
			foreach (var consol in consols)
			{
				var cluster = ObjectCreator.AddClusterToDraftTransaction(draftInvoice, 200);
				_ = ObjectCreator.AddConsolToTheCluster(cluster, consol);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDraftInvoice = newFactory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);

			//Try to reconcile
			var reconProcessor = ObjectFactory.Get<IAPReconciliationProcessor>();
			var reconResult = reconProcessor.Reconcile(reloadedDraftInvoice);
			AssertEquals(APReconciliationResultTypes.Success, reconResult.Result);
			AssertEquals(20, reconResult.ReconciliableAccruals.Count());

			AssertEquals("Loaded all jobs in one DB trip and cached", 1, newFactory.GetTableHitCount(JobHeaderSchema.Constants.TableName));
			AssertEquals("Loaded all jobConsols in one DB trip and cached", 1, newFactory.GetTableHitCount(JobConsolSchema.Constants.TableName));
			AssertEquals("Loaded all jobConsolCosts in one DB trip and cached", 1, newFactory.GetTableHitCount(JobConsolCostSchema.Constants.TableName));
			AssertEquals("Loaded all jobCharges in one DB trips and cached. One trip to load all unapportioned shipment level charges", 1, newFactory.GetTableHitCount(JobChargeSchema.Constants.TableName));
			AssertEquals("No need to load any orgheaders", 0, newFactory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));
		}

		public void TestGetSettlementGroupCreditorPKs()
		{
			ObjectCreator.Creditor1.APSettlementGroupPK = ObjectCreator.Creditor1.PK;
			ObjectCreator.Creditor2.APSettlementGroupPK = ObjectCreator.Creditor1.PK;
			ObjectCreator.Creditor3.APSettlementGroupPK = ObjectCreator.Creditor3.PK;
			var draftInvoice = ObjectCreator.CreateDraftInvoice("ADI 0001", "ADI-IREF-001", ObjectCreator.Creditor1.PK, 100, 0, "AUD");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDraftInvoice = newFactory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);
			var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(reloadedDraftInvoice);

			var settlementGroupCreditorPKs = cache.GetSettlementGroupCreditorPKs();
			AssertEquals(1, settlementGroupCreditorPKs.Count);
			AssertEquals(ObjectCreator.Creditor2.PK, settlementGroupCreditorPKs[0]);
			AssertEquals("Loaded all orgheaders for the AP settlement group creditors", 1, newFactory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));

			_ = cache.GetSettlementGroupCreditorPKs();
			AssertEquals("The orgheaders for AP settlement group creditors already cached", 1, newFactory.GetTableHitCount(OrgHeaderSchema.Constants.TableName));
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
