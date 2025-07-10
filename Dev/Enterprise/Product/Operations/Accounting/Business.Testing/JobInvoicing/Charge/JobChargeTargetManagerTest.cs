using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class JobChargeTargetManagerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			var creator = new TestObjectCreator(Factory);
			var sisterOrgProxy = creator.DebtorSisterOrgProxy;

			Factory.Save();

			TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "SGT", "PSA"));

			//			C0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = creator.CreateGatewayConsolsAndShipments();
			Factory.Save();
			Consol = setup.gC0002;
			Shipment = setup.s0002;

			var consolJobHeader = creator.CreateJob(Consol);

			var acc = creator.CreateChargeCode("GTB");
			acc.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			Charge = consolJobHeader.Charges.AddNew();
			Charge.JR_AC = acc.PK;
		}

		Charge Charge { get; set; }
		ForwardingConsol Consol { get; set; }
		ForwardingShipment Shipment { get; set; }

		public void TestRelatedJobNumber_DbHits()
		{
			var newFactory = new BusinessObjectFactory();
			var charge = newFactory.New<Charge>();
			var jobNumber = charge.JR_Calc_RelatedJobNumber;

			// should be no dbhits since everything is in memory
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(new Dictionary<string, int>(), newFactory);

			Charge.Job.Dispose();
		}

		public void TestSetRelatedJob()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var charge2 = newFactory.Load<Charge>(Charge.PK);
			AssertEquals(Shipment.JS_UniqueConsignRef, charge2.JR_Calc_RelatedJobNumber);

			var target = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertEquals(Shipment.PK, target.JRT_RelatedJobID);
			AssertEquals(Shipment.TablePrefix, target.JRT_RelatedJobTableCode);
		}

		public void TestCacheRelatedJobWithRelatedJobID()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.CreateJob(creator.CreateShipment("S00001"));
			var charge1 = creator.CreateCharge(job1);

			var relatedJob = charge1.RelatedJob;
			Assert("RelatedJobID is empty.", charge1.RelatedJobID.IsEmpty);
			AssertNull("RelatedJob is null due to the empty RelatedJobID.", relatedJob);

			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Factory.Save();

			relatedJob = Charge.RelatedJob;
			Assert("RelatedJobID is valid.", Charge.RelatedJobID.IsValid);
			AssertNotNull("RelatedJob is not null.", relatedJob);

			var cachedRelatedJob = Factory.GetCachedValue<IJobInvoicingPlugIn>(Charge.RelatedJobID.ToStringKey(), () => null);
			AssertNotNull("CachedRelatedJob is not null.", cachedRelatedJob);
			AssertEquals("CachedRelatedJob should be equal to RelatedJob.", relatedJob, cachedRelatedJob);
		}

		public void TestSetInvoiceTarget()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var charge2 = newFactory.Load<Charge>(Charge.PK);
			AssertEquals(Consol.JK_UniqueConsignRef, charge2.JR_Calc_InvoiceTarget);
			AssertEquals(Shipment.JS_UniqueConsignRef, charge2.JR_Calc_RelatedJobNumber);

			var target = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertEquals(Consol.PK, target.JRT_InvoiceTargetID);
			AssertEquals(Consol.TablePrefix, target.JRT_InvoiceTargetTableCode);

			AssertEquals(Shipment.PK, target.JRT_RelatedJobID);
			AssertEquals(Shipment.TablePrefix, target.JRT_RelatedJobTableCode);
		}

		public void TestInvoiceTargetDuplicates()
		{
			Shipment.JS_UniqueConsignRef = "C0001";
			Consol.JK_UniqueConsignRef = "C0001";
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;

			ErrorReporter.SuppressReportingOfErrors = true;
			using (new DisposableAction(() => ErrorReporter.SuppressReportingOfErrors = false))
			{
				Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			}

			AssertHasError(Charge.JR_Calc_InvoiceTargetInfo, "Unable to select this job. Duplicate jobs with same job number detected.");
			Charge.Job.Dispose();
		}

		public void TestSetInvoiceTargetAndRelatedJob()
		{
			Assert("Precondition", !Shipment.JS_UniqueConsignRef.IsEmpty && !Consol.JK_UniqueConsignRef.IsEmpty);

			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			AssertEquals(Shipment.JS_UniqueConsignRef, Charge.JR_Calc_RelatedJobNumber);
			AssertEquals(Consol.JK_UniqueConsignRef, Charge.JR_Calc_InvoiceTarget);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var target = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertEquals(Shipment.PK, target.JRT_RelatedJobID);
			AssertEquals(Shipment.TablePrefix, target.JRT_RelatedJobTableCode);

			AssertEquals(Consol.PK, target.JRT_InvoiceTargetID);
			AssertEquals(Consol.TablePrefix, target.JRT_InvoiceTargetTableCode);
		}

		public void TestClearInvoiceTarget()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var charge2 = newFactory.Load<Charge>(Charge.PK);
			charge2.JR_Calc_InvoiceTarget = ZString.Empty;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			var target = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertEquals(Shipment.PK, target.JRT_RelatedJobID);
			AssertEquals(Shipment.TablePrefix, target.JRT_RelatedJobTableCode);

			AssertEquals(ZGuid.Empty, target.JRT_InvoiceTargetID);
			AssertEquals(ZString.Empty, target.JRT_InvoiceTargetTableCode);
		}

		public void TestClearRelatedJob()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var charge2 = newFactory.Load<Charge>(Charge.PK);
			charge2.JR_Calc_RelatedJobNumber = ZString.Empty;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			var target = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNull("Clearing RelatedJob resets both", target);
		}

		public void TestClearInvoiceTargetAndRelatedJob()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var charge2 = newFactory.Load<Charge>(Charge.PK);
			charge2.JR_Calc_RelatedJobNumber = ZString.Empty;
			charge2.JR_Calc_InvoiceTarget = ZString.Empty;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			var target = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNull(target);
		}

		public void TestDeleteChargeShouldDeleteTarget()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			Factory.Save();

			var target = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNotNull(target);

			Charge.Delete();
			Factory.Save();

			var target2 = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNull(target2);
		}

		public void TestInvalidRelatedAndTargetJobNumbers()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;

			var target = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNotNull(target);

			Charge.JR_Calc_RelatedJobNumber = "INVALID111";
			Charge.JR_Calc_InvoiceTarget = "INVALID222";

			target = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNull(target);

			AssertEquals("INVALID111", Charge.JR_Calc_RelatedJobNumber);
			AssertEquals("INVALID222", Charge.JR_Calc_InvoiceTarget);

			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = "INVALID222";

			target = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNotNull(target);

			AssertEquals(Shipment.PK, target.JRT_RelatedJobID);
			AssertEquals(Shipment.TablePrefix, target.JRT_RelatedJobTableCode);
			AssertEquals(ZGuid.Empty, target.JRT_InvoiceTargetID);
			AssertEquals(ZString.Empty, target.JRT_InvoiceTargetTableCode);
			AssertEquals("INVALID222", Charge.JR_Calc_InvoiceTarget);

			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;

			AssertEquals(Shipment.PK, target.JRT_RelatedJobID);
			AssertEquals(Shipment.TablePrefix, target.JRT_RelatedJobTableCode);
			AssertEquals(Consol.PK, target.JRT_InvoiceTargetID);
			AssertEquals(Consol.TablePrefix, target.JRT_InvoiceTargetTableCode);

			Charge.JR_Calc_RelatedJobNumber = "INVALID111";
			Charge.JR_Calc_InvoiceTarget = "INVALID222";

			target = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNull(target);

			Charge.Job.Dispose();
		}

		public void TestDeletingTargetOutsideOfManager()
		{
			Charge.JR_Calc_RelatedJobNumber = Shipment.JS_UniqueConsignRef;
			Charge.JR_Calc_InvoiceTarget = Consol.JK_UniqueConsignRef;
			Factory.Save();

			var target = Factory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, Charge.PK));
			AssertNotNull(target);

			target.Delete();
			AssertNoExceptionThrown(() => _ = Charge.JR_Calc_RelatedJobNumber);
		}
	}
}
