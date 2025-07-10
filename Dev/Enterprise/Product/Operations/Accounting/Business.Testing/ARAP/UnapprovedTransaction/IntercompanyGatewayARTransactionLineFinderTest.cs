using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class IntercompanyGatewayARInvoiceLineFinderTest : TestCaseWithFactory
	{
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: arTransaction")]
		public void TestExceptionThrownForPassingNullTransaction()
		{
			IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(null);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "This method only accpets an AR Transaction.")]
		public void TestExceptionThrownWhenNonARTransactionIsPassed()
		{
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV1");
			AssertEquals("Precondition", LedgerTypes.AccountsPayable, apInvoice.AH_Ledger);
			IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(apInvoice);
		}

		public void TestMiscellaneousARInvoice()
		{
			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 100m, 0m, TestObjectCreator.GLHeader1.PK);
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 200m, 0m, TestObjectCreator.GLHeader2.PK);
				Factory.Save();
			}
			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(0, invoiceLinesAssociatedWithGateway.Count);
		}

		public void TestShipmentARInvoice()
		{
			var shipment = TestObjectCreator.CreateShipment("S1234");
			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job = TestObjectCreator.CreateJob(shipment);
				arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				var arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC1, 100m, TestObjectCreator.EUR, 1m);
				TestObjectCreator.CreateCharge(arInvoiceLine1);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC2, 200m, TestObjectCreator.EUR, 1m);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();
			}
			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(0, invoiceLinesAssociatedWithGateway.Count);
		}

		public void TestForwardingConsolARInvoice()
		{
			var forwardingConsol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S1234", forwardingConsol);
			InvoicingBase arInvoice = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job = TestObjectCreator.CreateJob(shipment, false);
				arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_OSTotalAmount = 3m;
				arInvoice.AH_ConsolidatedInvoiceRef = forwardingConsol.JK_UniqueConsignRef;
				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC2, 200m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);
			}
			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(0, invoiceLinesAssociatedWithGateway.Count);
		}

		public void TestGatewayARInvoice()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			InvoicingLineBase arInvoiceLine1 = null;
			InvoicingLineBase arInvoiceLine2 = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, shipment1);
				arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC2, 200m, TestObjectCreator.AUD, 1m);
				var charge2 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, shipment2, gatewayConsol);
				Factory.Save();
			}

			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(2, invoiceLinesAssociatedWithGateway.Count);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine1.PK), shipment1.PK, shipment1.JS_UniqueConsignRef, shipment1.PK, shipment1.JS_UniqueConsignRef, JobShipmentSchema.Constants.Prefix);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine2.PK), shipment2.PK, shipment2.JS_UniqueConsignRef, gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
		}

		public void TestPeriodicInvoiceWithLinesAssociatedWithShipemntAndGatewayConsol()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			InvoicingLineBase arInvoiceLine1 = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
				arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, gatewayConsol);
				var arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment1Job, TestObjectCreator.CC2, 200m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(arInvoiceLine2);
				var arInvoiceLine3 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment2Job, TestObjectCreator.CC3, 300m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(arInvoiceLine3);
				Factory.Save();
			}
			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(1, invoiceLinesAssociatedWithGateway.Count);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine1.PK), shipment1.PK, shipment1.JS_UniqueConsignRef, gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
		}

		public void TestJobParentIsInitializedBeforeCheckingIsGatewayBillingEnabled()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			InvoicingLineBase arInvoiceLine1 = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, shipment1);
				Factory.Save();
			}

			var arInvoiceInNewFactory = new BusinessObjectFactory().Load<ARInvoice>(arInvoice.PK);
			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoiceInNewFactory);
			AssertEquals(1, invoiceLinesAssociatedWithGateway.Count);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine1.PK), shipment1.PK, shipment1.JS_UniqueConsignRef, shipment1.PK, shipment1.JS_UniqueConsignRef, JobShipmentSchema.Constants.Prefix);
		}

		public void TestGatewayARInvoiceWithBlankTargetJobAndBlankRelatedJob()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			InvoicingLineBase arInvoiceLine1 = null;
			InvoicingLineBase arInvoiceLine2 = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				var charge1 = TestObjectCreator.CreateCharge(arInvoiceLine1);
				arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC2, 200m, TestObjectCreator.AUD, 1m);
				var charge2 = TestObjectCreator.CreateCharge(arInvoiceLine2);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var targetForCharge1 = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, charge1.PK));
				AssertNull(targetForCharge1);
				var targetForCharge2 = newFactory.LoadTop1<JobChargeTarget>(new ZQuery(JobChargeTargetSchema.JRT_JR, charge2.PK));
				AssertNull(targetForCharge2);
			}

			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(2, invoiceLinesAssociatedWithGateway.Count);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine1.PK), ZGuid.Empty, ZString.Empty, gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine2.PK), ZGuid.Empty, ZString.Empty, gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
		}

		public void TestGatewayARInvoiceWithBlankTargetJobAndNonBlankRelatedJob()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			InvoicingLineBase arInvoiceLine1 = null;
			InvoicingLineBase arInvoiceLine2 = null;
			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				arInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				var charge1 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine1, shipment1, null);
				arInvoiceLine2 = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC2, 200m, TestObjectCreator.AUD, 1m);
				var charge2 = TestObjectCreator.CreateChargeWithTarget(arInvoiceLine2, shipment2, null);
				Factory.Save();
			}

			var invoiceLinesAssociatedWithGateway = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(arInvoice);
			AssertEquals(2, invoiceLinesAssociatedWithGateway.Count);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine1.PK), shipment1.PK, shipment1.JS_UniqueConsignRef, gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
			AssertGatewayARTransactionLineDetails(invoiceLinesAssociatedWithGateway.First(x => x.LinePK == arInvoiceLine2.PK), shipment2.PK, shipment2.JS_UniqueConsignRef, gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
		}

		#region Helpers

		void AssertGatewayARTransactionLineDetails(InvoiceLineAssociatedWithGatewayJob gatewayARInvoiceLine, ZGuid expectedRelatedJobID, ZString expectedRelatedJobNumber, ZGuid expectedTargetJobID, ZString expectedTargetJobNumber, ZString expectedTargetJobParentTableCode)
		{
			AssertEquals(expectedRelatedJobID, gatewayARInvoiceLine.RelatedJobPk);
			AssertEquals(expectedRelatedJobNumber, gatewayARInvoiceLine.RelatedJobNumber);
			AssertEquals(expectedTargetJobID, gatewayARInvoiceLine.TargetJobPk);
			AssertEquals(expectedTargetJobNumber, gatewayARInvoiceLine.TargetJobNumber);
			AssertEquals(expectedTargetJobParentTableCode, gatewayARInvoiceLine.TargetJobParentTableCode);
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		OrgHeader sisterCompanyOrgProxy;
		GlbCompany sisterCompany;
		GlbBranch sisterBranch;

		protected override void SetUp()
		{
			base.SetUp();

			sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();
			}

			var periodMgmtTestHelper = new AccountingPeriodTestHelper();
			periodMgmtTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodMgmtTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, sisterCompany.PK);
		}

		#endregion
	}
}
