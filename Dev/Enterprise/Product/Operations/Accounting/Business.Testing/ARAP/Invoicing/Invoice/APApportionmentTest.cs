using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class APApportionmentTest : TestCaseWithFactory
	{
		#region TestImportingChargesDoesntAttemptToModifyApportionedLine

		public void TestImportingChargesDoesntAttemptToModifyApportionedLine()
		{
			bool oldIsAllowed = Env.Security.ReopenJob.IsAllowed;

			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var shipment1 = consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var creator = new TestObjectCreator(factory);
			var chargeCode = creator.CC1;

			var shipment1Job = Job.CreateWithMutex(factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_JobNum = "S00003333";
			shipment1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			factory.Save();

			var shipment1Charge = shipment1Job.Charges.AddNew();
			shipment1Charge.JR_AC = creator.CC2.PK;
			shipment1Charge.JR_OSCostAmt = 100m;

			Env.Security.ReopenJob.IsAllowed = true;
			shipment1Job.JH_Status = JobHeaderStatus.Closed.Code;
			Env.Security.ReopenJob.IsAllowed = false;

			factory.Save();

			var jobCharges = new List<Charge>();
			jobCharges.Add(shipment1Charge);
			var invoice = GetInvoiceBase(factory);
			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_OSCostAmount = 200m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.SetIsUsedForApportionment();

				invoice.ImportSingleCostAndRevalidateLines(cost, line1);

				var shipmentJob1Line = (InvoicingLineBase)invoice.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, shipment1Job.PK))[0];
				var shipmentJob2Line = (InvoicingLineBase)invoice.Lines.Find(new ZQuery(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, shipmentJob1Line.PK))[0];

				AssertEquals("ReopenJob.IsAllowed = false && JobStatus = Closed, Should have Security Warning", true, shipmentJob1Line.AL_JHInfo.HasWarning(InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage));
				AssertEquals("ReopenJob.IsAllowed = false && JobStatus = Working, Should NOT have Security Warning", false, shipmentJob2Line.AL_JHInfo.HasWarning(InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage));

				invoice.Lines.ApportionedInvoiceLineModified += new ApportionedInvoiceLineModifiedEventHandler(APInvoiceTest_ApportionedInvoiceLineModified);

				invoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), (InvoicingLineBase)invoice.Lines.AddNew());

				AssertEquals("Should have 3 lines now", 3, invoice.Lines.Count);
				Assert("Shouldn't try to modify apportioned line when importing charges", !LineModified);

				Env.Security.ReopenJob.IsAllowed = oldIsAllowed;
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestImportingChargesDoesntAttemptToModifyApportionedLineAndValidateThem()
		{
			bool oldIsAllowed = Env.Security.ReopenJob.IsAllowed;

			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var creator = new TestObjectCreator(factory);

			var shipment1Job = creator.CreateJob(shipment1, newFactory: factory);
			shipment1Job.JH_JobNum = "S00003333";

			var shipment1Charge = shipment1Job.Charges.AddNew();
			shipment1Charge.JR_AC = creator.CC2.PK;
			shipment1Charge.JR_OSCostAmt = 100m;

			Env.Security.ReopenJob.IsAllowed = true;
			shipment1Job.JH_Status = JobHeaderStatus.Closed.Code;
			Env.Security.ReopenJob.IsAllowed = false;

			factory.Save();

			var jobCharges = new List<Charge>();
			jobCharges.Add(shipment1Charge);
			var invoice = GetInvoiceBase(factory);
			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_OSCostAmount = 200m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.SetIsUsedForApportionment();

				invoice.ImportSingleCost(cost, line1);

				var shipmentJob1Line = (InvoicingLineBase)invoice.Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, shipment1Job.PK))[0];
				var shipmentJob2Line = (InvoicingLineBase)invoice.Lines.Find(new ZQuery(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, shipmentJob1Line.PK))[0];

				AssertEquals("Validation can reduce performance and should be done only in single line processing cases where it needed.", false, shipmentJob1Line.AL_JHInfo.HasWarning(InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage));

				invoice.Lines.ApportionedInvoiceLineModified += new ApportionedInvoiceLineModifiedEventHandler(APInvoiceTest_ApportionedInvoiceLineModified);

				invoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), (InvoicingLineBase)invoice.Lines.AddNew());

				AssertEquals("Should have 3 lines now", 3, invoice.Lines.Count);
				Assert("Shouldn't try to modify apportioned line when importing charges", !LineModified);

				Env.Security.ReopenJob.IsAllowed = oldIsAllowed;
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		protected abstract InvoicingBase GetInvoiceBase(BusinessObjectFactory factory);
		protected abstract InvoicingLineBase GetInvoiceLineBase(BusinessObjectFactory factory);

		#region TestDeleteInMemoryJobsCreatedThatHaveNoCharges

		public void TestDeleteInMemoryJobsCreatedThatHaveNoCharges()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);

			GlbDepartment fEADept = GetFEADept();
			GlbDeptCharges defaultCharge = fEADept.DeptCharges.AddNew();
			defaultCharge.GD_AC = creator.CC3.PK;
			defaultCharge.GD_SequenceNumber = 1;

			ForwardingConsol consol = factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				shipment.JS_RL_NKDestination = "USLAX"; //Export
				shipment.JS_TransportMode = "AIR";
			}

			AccChargeCode chargeCode = creator.CC1;

			factory.Save();

			InvoicingBase testInvoice = GetInvoiceBase(factory);
			testInvoice.SubmittedFromInvoicingForm = true;
			testInvoice.AH_OH = creator.AALSHI.PK;
			testInvoice.AH_TransactionNum = "INV1";

			try
			{
				JobConsolCost cost = testInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 0m;

				testInvoice.ImportAllApportionmentsFromCosting();

				factory.Save();

				factory = new BusinessObjectFactory();

				Job[] jobs = (Job[])factory.Load(typeof(Job), new ZQuery().AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("Should only be one job saved", 1, jobs.Length);
			}
			finally
			{
				testInvoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestDeleteInMemoryJobsCreatedThatHaveNoChargesAndNullDefaultBranch()
		{
			//Arrange
			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 1;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToLoginUserDefault = 0;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, rule);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);

			GlbDepartment fEADept = GetFEADept();
			GlbDeptCharges defaultCharge = fEADept.DeptCharges.AddNew();
			defaultCharge.GD_AC = creator.CC3.PK;
			defaultCharge.GD_SequenceNumber = 1;

			ForwardingConsol consol = factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				shipment.JS_RL_NKDestination = "USLAX"; //Export
				shipment.JS_TransportMode = "AIR";
			}

			AccChargeCode chargeCode = creator.CC1;

			factory.Save();

			InvoicingBase testInvoice = GetInvoiceBase(factory);
			testInvoice.SubmittedFromInvoicingForm = true;
			testInvoice.AH_OH = creator.AALSHI.PK;
			testInvoice.AH_TransactionNum = "INV1";

			//Act
			try
			{
				JobConsolCost cost = testInvoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 100m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 0m;

				testInvoice.ImportAllApportionmentsFromCosting();

				factory.Save();

				factory = new BusinessObjectFactory();

				Job[] jobs = (Job[])factory.Load(typeof(Job), new ZQuery().AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				//Assert
				AssertEquals("Should only be one job saved", 1, jobs.Length);
			}
			finally
			{
				testInvoice.ClearApportionmentJobMutexes();
			}
		}

		GlbDepartment GetFEADept()
		{
			return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
		}

		#endregion

		public void TestDoOtherLinesExistForImportedApportionment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			Factory.Save();

			var invoice = GetInvoiceBase(Factory);

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.SetIsUsedForApportionment();

				invoice.ImportAllApportionmentsFromCosting();

				Assert("Second line is related and as such this should be true", invoice.DoOtherLinesExistForImportedApportionment(cost.PK, new InvoicingLineBase[] { invoice.Lines[0] }));
				Assert("All Lines are related to apportionment", !invoice.DoOtherLinesExistForImportedApportionment(cost.PK, (InvoicingLineBase[])invoice.Lines.ToArray(typeof(InvoicingLineBase))));
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestDeveloperExceptionShouldNotBeReportedWhenSettingAPInvoiceCurrencyToLocalWithApportionedConsolCost()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			var invoice = GetInvoiceBase(Factory);
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_ExchangeRate = 0.6m;

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.SetIsUsedForApportionment();
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 0.6m;

				invoice.ImportAllApportionmentsFromCosting();

				AssertEquals("USD", invoice.Lines[0].AL_RX_NKTransactionCurrency);
				AssertEquals(0.6m, invoice.Lines[0].AL_ExchangeRate);
				AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

				ErrorReporter.Clear();
				invoice.AH_RX_NKTransactionCurrency = "AUD";
				AssertEquals("No dev exception thrown", 0, ErrorReporter.TotalErrorCount);
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
				ErrorReporter.Clear();
			}
		}

		public void TestRemoveAllLinesRelatingToApportionment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();

			var invoice = GetInvoiceBase(Factory);

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_OSCostAmount = 10m;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;

				invoice.ImportAllApportionmentsFromCosting();

				var line3 = (InvoicingLineBase)invoice.Lines.AddNew();

				int invoiceLinesListChangedHitCount = 0;
				var listChangedHandler = new ListChangedEventHandler(
					(sender, e) =>
					{ invoiceLinesListChangedHitCount++; }
				);
				((IBindingList)invoice.Lines).ListChanged += listChangedHandler;

				using (invoice.GetReportingDeletedApportionmentChargesSuspender())
				{
					invoice.RemoveAllLinesRelatingToConsolCost(cost.PK, new List<InvoicingLineBase>(new InvoicingLineBase[] { invoice.Lines[0] }));
					AssertEquals("ListChanged on invoice.Lines should be called once at we use ListChanged suspender", 1, invoiceLinesListChangedHitCount);

					invoice.Lines.RemoveAndDelete(invoice.Lines[0]);
				}
				AssertEquals("Should have removed two top lines, leaving line3 in list", 1, invoice.Lines.Count);
				AssertEquals("Should be line 3 in list", line3, invoice.Lines[0]);
			}
			finally
			{
				shipment1.Job.Dispose();
				shipment2.Job.Dispose();
			}
		}

		public void TestDontImportNonApplicableLine()
		{
			var testDataFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(testDataFactory);
			var consol = testDataFactory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			testDataFactory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoice = GetInvoiceBase(newFactory);

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = objectCreator.CC1.PK;
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.E6_OSCostAmount = 30m;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 30m;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 0m;

				invoice.ImportSingleCost(cost, (InvoicingLineBase)invoice.Lines.AddNew());

				AssertEquals("Should only import one line", 1, invoice.Lines.Count);
				AssertEquals("Line should be for $30", 30.00m, invoice.Lines[0].AL_OSExTaxAmount);
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestImportWholeApportionment()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator testDataCreator = new TestObjectCreator(testDataFactory);
			AccChargeCode dSBChargeCode = testDataCreator.CreateChargeCode("DISB", "Disbursement Charge Code", Core.Constants.ChargeType.Disbursement, 100m, null, null, "ALL");

			ForwardingConsol consol = testDataFactory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = testDataCreator.CreateShipment("S00001001", consol);
			ForwardingShipment shipment2 = testDataCreator.CreateShipment("S00001002", consol);
			ForwardingShipment shipment3 = testDataCreator.CreateShipment("S00001003", consol);

			Job jobForShipment1 = testDataCreator.CreateJob(shipment1);
			Job jobForShipment2 = testDataCreator.CreateJob(shipment2);
			Job jobForShipment3 = testDataCreator.CreateJob(shipment3);

			testDataFactory.Save();

			InvoicingBase invoice = GetInvoiceBase(Factory);

			invoice.AH_RX_NKTransactionCurrency = testDataCreator.USD.RX_Code;
			invoice.AH_ExchangeRate = 0.7m;
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_JH = jobForShipment1.PK;

			AssertEquals("Should only be 1 line in invoice", 1, invoice.Lines.Count);

			JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = dSBChargeCode.PK;
			cost.E6_OSCostAmount = 60m;
			cost.E6_OSGSTAmount_Calc = 8.5m;
			cost.E6_ApportionmentMethod = AllocationMethod.Manual;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 10.00m;
			cost.ApportionmentCharges[1].JR_OSCostAmt = 20.00m;
			cost.ApportionmentCharges[2].JR_OSCostAmt = 30.00m;

			cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc = 1.50m;
			cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc = 2.50m;
			cost.ApportionmentCharges[2].JR_OSCostGSTAmt_Calc = 4.50m;

			invoice.ImportSingleCost(cost, line1);

			AssertEquals("Apportionment should have imported 3 lines into invoice, replacing originating line", 3, invoice.Lines.Count);

			InvoicingLineBase line2 = invoice.Lines[1];
			InvoicingLineBase line3 = invoice.Lines[2];

			foreach (InvoicingLineBase line in invoice.Lines)
			{
				AssertNotNull("Each line should have apportionment split charge property set", line.IsPopulatedFromImportedApportionment);
				AssertEquals("Charge code should be set to each line", dSBChargeCode.PK, line.GenericCharge);
				AssertEquals("Charge code should be set to each line", dSBChargeCode.PK, line.AL_AC);
				AssertEquals("Branch should be set on line", line.ApportionmentChargeImportedFrom.JR_GB, line.AL_GB);
				AssertEquals("Department should be set on line", line.ApportionmentChargeImportedFrom.JR_GE, line.AL_GE);
				AssertEquals("Tax Rate should be set to Apportionment Tax Rate", cost.E6_AT_TaxRate, line.AL_AT);

				if (invoice is APInvoice)
				{
					AssertEquals("Is Final should be true on lines", line.ApportionmentChargeImportedFrom.IsFinal, line.AL_IsFinalCharge);
					Assert("Is Final should NOT be read only any more", !line.AL_IsFinalChargeInfo.ReadOnly);
					Assert("Is Final should not have any error", !line.AL_IsFinalChargeInfo.HasErrors());
				}
			}

			AssertEquals("OS Ex Tax Amount", 10.00m, line1.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 1.50m, line1.AL_OSTaxAmount);

			AssertEquals("OS Ex Tax Amount", 20.00m, line2.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 2.50m, line2.AL_OSTaxAmount);

			AssertEquals("OS Ex Tax Amount", 30.00m, line3.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 4.50m, line3.AL_OSTaxAmount);
		}

		public void TestImportApportionmentsFromList()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator testDataCreator = new TestObjectCreator(testDataFactory);
			AccTaxRate gSTRate = testDataCreator.GST1;
			AccChargeCode dSBChargeCode = testDataCreator.CreateChargeCode("DISB", "Disbursement Charge Code", Core.Constants.ChargeType.Disbursement, 100m, null, null, "ALL");

			ForwardingConsol consol1 = testDataFactory.New<ForwardingConsol>();
			ForwardingShipment consol1Shipment1 = testDataCreator.CreateShipment("S00001001", consol1);
			Job job1 = testDataCreator.CreateJob(consol1Shipment1);
			ForwardingShipment consol1Shipment2 = testDataCreator.CreateShipment("S00001002", consol1);
			Job job2 = testDataCreator.CreateJob(consol1Shipment2);
			ForwardingShipment consol1Shipment3 = testDataCreator.CreateShipment("S00001003", consol1);
			Job job3 = testDataCreator.CreateJob(consol1Shipment3);

			ForwardingConsol consol2 = testDataFactory.New<ForwardingConsol>();
			ForwardingShipment consol2Shipment1 = testDataCreator.CreateShipment("S00001004", consol2);
			Job job4 = testDataCreator.CreateJob(consol2Shipment1);
			ForwardingShipment consol2Shipment2 = testDataCreator.CreateShipment("S00001005", consol2);
			Job job5 = testDataCreator.CreateJob(consol2Shipment2);
			ForwardingShipment consol2Shipment3 = testDataCreator.CreateShipment("S00001006", consol2);
			Job job6 = testDataCreator.CreateJob(consol2Shipment3);

			testDataFactory.Save();

			InvoicingBase invoice = GetInvoiceBase(Factory);

			invoice.Lines.RemoveAndDeleteAll();

			JobConsolCost consol1Cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol1);
			consol1Cost.E6_AC_ChargeCode = dSBChargeCode.PK;
			consol1Cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consol1Cost.E6_AT_TaxRate = gSTRate.PK;
			consol1Cost.E6_OSCostAmount = 300.00m;
			consol1Cost.SetIsUsedForApportionment();

			JobConsolCost consol2Cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);
			consol2Cost.E6_AC_ChargeCode = dSBChargeCode.PK;
			consol2Cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consol2Cost.E6_AT_TaxRate = gSTRate.PK;
			consol2Cost.E6_OSCostAmount = 300.00m;
			consol2Cost.SetIsUsedForApportionment();

			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Should be two apportionments in list of imported apportionments", 2, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Should be 6 lines imported into invoice", 6, invoice.Lines.Count);

			InvoicingLineBase consol1Shipment1Line = null;
			InvoicingLineBase consol1Shipment2Line = null;
			InvoicingLineBase consol1Shipment3Line = null;
			InvoicingLineBase consol2Shipment1Line = null;
			InvoicingLineBase consol2Shipment2Line = null;
			InvoicingLineBase consol2Shipment3Line = null;

			foreach (InvoicingLineBase line in invoice.Lines)
			{
				if (line.AL_JH == job1.PK)
				{
					consol1Shipment1Line = line;
				}
				else if (line.AL_JH == job2.PK)
				{
					consol1Shipment2Line = line;
				}
				else if (line.AL_JH == job3.PK)
				{
					consol1Shipment3Line = line;
				}
				else if (line.AL_JH == job4.PK)
				{
					consol2Shipment1Line = line;
				}
				else if (line.AL_JH == job5.PK)
				{
					consol2Shipment2Line = line;
				}
				else if (line.AL_JH == job6.PK)
				{
					consol2Shipment3Line = line;
				}
				else
				{
					Fail("Should not be any other lines on invoice at this point");
				}
			}

			AssertNotNull("Should have found all lines imported", consol1Shipment1Line);
			AssertNotNull("Should have found all lines imported", consol1Shipment2Line);
			AssertNotNull("Should have found all lines imported", consol1Shipment3Line);
			AssertNotNull("Should have found all lines imported", consol2Shipment1Line);
			AssertNotNull("Should have found all lines imported", consol2Shipment2Line);
			AssertNotNull("Should have found all lines imported", consol2Shipment3Line);
		}

		public void TestImportApportionmentsFromListWhenApportionmentsHaveBeenRemoved()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator testDataCreator = new TestObjectCreator(testDataFactory);
			AccTaxRate gSTRate = testDataCreator.GST1;
			AccChargeCode dSBChargeCode = testDataCreator.CreateChargeCode("DISB", "Disbursement Charge Code", Core.Constants.ChargeType.Disbursement, 100m, null, null, "ALL");

			ForwardingConsol consol1 = testDataFactory.New<ForwardingConsol>();
			ForwardingShipment consol1Shipment1 = testDataCreator.CreateShipment("S00001001", consol1);
			Job job1 = testDataCreator.CreateJob(consol1Shipment1);
			ForwardingShipment consol1Shipment2 = testDataCreator.CreateShipment("S00001002", consol1);
			Job job2 = testDataCreator.CreateJob(consol1Shipment2);
			ForwardingShipment consol1Shipment3 = testDataCreator.CreateShipment("S00001003", consol1);
			Job job3 = testDataCreator.CreateJob(consol1Shipment3);

			ForwardingConsol consol2 = testDataFactory.New<ForwardingConsol>();
			ForwardingShipment consol2Shipment1 = testDataCreator.CreateShipment("S00001004", consol2);
			Job job4 = testDataCreator.CreateJob(consol2Shipment1);
			ForwardingShipment consol2Shipment2 = testDataCreator.CreateShipment("S00001005", consol2);
			Job job5 = testDataCreator.CreateJob(consol2Shipment2);
			ForwardingShipment consol2Shipment3 = testDataCreator.CreateShipment("S00001006", consol2);
			Job job6 = testDataCreator.CreateJob(consol2Shipment3);

			testDataFactory.Save();

			InvoicingBase invoice = GetInvoiceBase(Factory);

			invoice.Lines.RemoveAndDeleteAll();

			JobConsolCost consol1Cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol1);
			consol1Cost.E6_AC_ChargeCode = dSBChargeCode.PK;
			consol1Cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consol1Cost.E6_AT_TaxRate = gSTRate.PK;
			consol1Cost.E6_OSCostAmount = 300m;
			consol1Cost.SetIsUsedForApportionment();

			JobConsolCost consol2Cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);
			consol2Cost.E6_AC_ChargeCode = dSBChargeCode.PK;
			consol2Cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consol2Cost.E6_AT_TaxRate = gSTRate.PK;
			consol2Cost.E6_OSCostAmount = 300m;
			consol2Cost.SetIsUsedForApportionment();

			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Should be two apportionments in list of imported apportionments", 2, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Should be 6 lines imported into invoice", 6, invoice.Lines.Count);

			InvoicingLineBase consol1Shipment1Line = null;
			InvoicingLineBase consol1Shipment2Line = null;
			InvoicingLineBase consol1Shipment3Line = null;
			InvoicingLineBase consol2Shipment1Line = null;
			InvoicingLineBase consol2Shipment2Line = null;
			InvoicingLineBase consol2Shipment3Line = null;

			foreach (InvoicingLineBase line in invoice.Lines)
			{
				if (line.AL_JH == job1.PK)
				{
					consol1Shipment1Line = line;
				}
				else if (line.AL_JH == job2.PK)
				{
					consol1Shipment2Line = line;
				}
				else if (line.AL_JH == job3.PK)
				{
					consol1Shipment3Line = line;
				}
				else if (line.AL_JH == job4.PK)
				{
					consol2Shipment1Line = line;
				}
				else if (line.AL_JH == job5.PK)
				{
					consol2Shipment2Line = line;
				}
				else if (line.AL_JH == job6.PK)
				{
					consol2Shipment3Line = line;
				}
				else
				{
					Fail("Should not be any other lines on invoice at this point");
				}
			}

			AssertNotNull("Should have found all lines imported", consol1Shipment1Line);
			AssertNotNull("Should have found all lines imported", consol1Shipment2Line);
			AssertNotNull("Should have found all lines imported", consol1Shipment3Line);
			AssertNotNull("Should have found all lines imported", consol2Shipment1Line);
			AssertNotNull("Should have found all lines imported", consol2Shipment2Line);
			AssertNotNull("Should have found all lines imported", consol2Shipment3Line);

			invoice.ConsolCosting.ConsolCosts.Remove(consol2Cost);
			invoice.ImportAllApportionmentsFromCosting();

			AssertEquals("Should only be 3 lines now", 3, invoice.Lines.Count);

			Assert("Line for shipment 1 and consol 2 should be deleted as it related to a removed apportionment", consol2Shipment1Line.IsDeleted);
			Assert("Line for shipment 2 and consol 2 should be deleted as it related to a removed apportionment", consol2Shipment2Line.IsDeleted);
			Assert("Line for shipment 3 and consol 2 should be deleted as it related to a removed apportionment", consol2Shipment3Line.IsDeleted);
		}

		TestObjectCreator fObjectCreator;
		protected TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}

		bool LineModified;
		void APInvoiceTest_ApportionedInvoiceLineModified(InvoicingLineBase sender, EventArgs e)
		{
			LineModified = true;
		}

		#endregion

	}
}
