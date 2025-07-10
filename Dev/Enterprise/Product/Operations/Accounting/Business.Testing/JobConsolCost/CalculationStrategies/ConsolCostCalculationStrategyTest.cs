using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[TestedType(typeof(JobConsolCost))]
	public class ConsolCostCalculationStrategyTest : JobConsolCostTest
	{
		protected override JobConsolCost GetCost(IJobCostingPlugIn consol)
		{
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			AppLists.Add(apps);
			return apps.CostsCollection.TryAddNew();
		}

		protected override void TearDown()
		{
			foreach (ApportionmentListing apps in AppLists)
			{
				if (apps != null)
				{
					apps.ReleaseMutexes();
				}
			}

			base.TearDown();
		}

		readonly List<ApportionmentListing> AppLists = new List<ApportionmentListing>();
		public void TestDontCallHasChangesWhenLoadingJobCharges()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			IJobInvoicingPlugIn shipment1 = consol.Shipments.AddNew();
			IJobInvoicingPlugIn shipment2 = consol.Shipments.AddNew();
			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apportionments.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 100m;
			cost.PrepareForPosting();
			Factory.Save();
			Assert(!cost.HasChanges);
			Assert(!cost.ApportionmentCharges[0].HasChanges);
			Assert(!cost.ApportionmentCharges[1].HasChanges);
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			Assert(cost.HasChanges);
			Assert(cost.ApportionmentCharges[0].HasChanges);
			Assert(cost.ApportionmentCharges[1].HasChanges);
			JobConsolCost secondCost = (JobConsolCost)((System.ComponentModel.IBindingList)apportionments.CostsCollection).AddNew();
			Assert(cost.HasChanges);
			Assert(cost.ApportionmentCharges[0].HasChanges);
			Assert(cost.ApportionmentCharges[1].HasChanges);
		}

		public void TestUpdateApportionmentChargesListing()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualChargeable = 500m;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			ZDateTime invoiceDate = ZDateTime.Now.AddDays(1);
			Job job = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")).PK;
			JobConsolCost cost = GetCost(consol);
			AppLists[AppLists.Count - 1].IsActivated = true;
			ZQuery cusdsbQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB");
			cusdsbQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			cost.E6_AC_ChargeCode = Factory.LoadTop1<AccChargeCode>(cusdsbQuery).PK;
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			cost.E6_InvoiceNum = "abc123";
			cost.E6_InvoiceDate = invoiceDate;
			cost.E6_PaymentDate = invoiceDate;
			cost.E6_CostReference = "ABC";
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = creator.AUDBankAccount.PK;
			cost.E6_AK_ChequeBook = creator.AUDChequeBook.PK;
			cost.E6_ChequeOrReference = creator.AUDChequeBook.AK_StartNo.ToString();
			cost.RemoveNonApplicableCharges();
			JobConsolCost costPosted = GetCost(consol);
			costPosted.E6_AC_ChargeCode = creator.CC1.PK;
			costPosted.E6_OSCostAmount = 100m;
			costPosted.E6_OH_Creditor = creator.AALSHI.PK;
			costPosted.E6_InvoiceNum = "abc123";
			costPosted.E6_InvoiceDate = invoiceDate;
			costPosted.E6_PaymentDate = invoiceDate;
			costPosted.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			costPosted.E6_AB_BankAccount = creator.AUDBankAccount.PK;
			costPosted.E6_AK_ChequeBook = creator.AUDChequeBook.PK;
			costPosted.E6_ChequeOrReference = creator.AUDChequeBook.AK_StartNo.ToString();
			costPosted.ApportionmentCharges[0].SetShipmentInfo(null);
			AssertEquals("ChargeableUnits must be equal to zero bacause ShipmentInfo is null", 0m, costPosted.ApportionmentCharges[0].JR_Chargeable);
			costPosted.UpdateApportionmentChargesListing();
			AssertEquals("ChargeableUnits must be equal to 500 bacause ShipmentInfo is setted", 500m, costPosted.ApportionmentCharges[0].JR_Chargeable);
			Factory.Save();
			var invoice = creator.CreateAPInvoice<APInvoice>("001", creator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, creator.AALSHI);
			costPosted.E6_AH_APInvoice = invoice.PK;
			costPosted.ApportionmentCharges[0].ReverseAccrual(ZDateTime.Now);
			costPosted.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
			costPosted.ApportionmentCharges[0].SetAmountsToLinkedLinesForTests();
			Factory.Save();
			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			Charge[] chargesInDB = loadFactory.Load<Charge>(new ZQuery(JobChargeSchema.JR_E6, cost.PK));
			AssertEquals("Should be one charge in DB", 1, chargesInDB.Length);
			cost.ApportionmentCharges.RemoveAll();
			cost.UpdateApportionmentChargesListing();
			AssertEquals("Should be 2 charges in cost", 2, cost.ApportionmentCharges.Count);
			AssertEquals("Department must be CIA", "CIA", cost.ApportionmentCharges[0].Department.GE_Code);
			AssertEquals("Department must be BRN", "BRN", cost.ApportionmentCharges[1].Department.GE_Code);
			cost.E6_ApportionmentMethod = "SHP";
			ZQuery chargeNotInDBQuery = new ZQuery(JobChargeSchema.JR_E6, cost.PK);
			chargeNotInDBQuery.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, chargesInDB[0].PK);
			ApportionSplitCharge chargeNotInDB = Factory.Load<ApportionSplitCharge>(chargeNotInDBQuery)[0];
			Assert(!chargeNotInDB.IsInDatabase);
			AssertEquals(cost.E6_OH_Creditor, chargeNotInDB.JR_OH_CostAccount);
			AssertEquals(cost.E6_InvoiceNum, chargeNotInDB.JR_APInvoiceNum);
			AssertEquals(cost.E6_InvoiceDate, chargeNotInDB.JR_APInvoiceDate);
			AssertEquals(cost.E6_PaymentDate, chargeNotInDB.JR_PaymentDate);
			AssertEquals(cost.E6_CostReference, chargeNotInDB.JR_CostReference);
			AssertEquals(cost.E6_AB_BankAccount, chargeNotInDB.JR_AB);
			AssertEquals(cost.E6_AK_ChequeBook, chargeNotInDB.JR_AK);
			AssertEquals(cost.E6_PaymentType, chargeNotInDB.JR_PaymentType);
			AssertEquals(cost.E6_ChequeOrReference, chargeNotInDB.JR_ChequeNo);
		}

		public void TestHasChangesSuspendedWhenGettingJob()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			Job shipment1Job = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge shipment1JobCharge = shipment1Job.Charges.AddNew();
			shipment1JobCharge.JR_AC = creator.MRG100.PK;
			Job shipment2Job = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			Assert(shipment1Job.IsInDatabase);
			Assert(shipment2Job.IsInDatabase);
			Assert(shipment1JobCharge.IsInDatabase);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			Assert(!((IBusinessObjectState)shipment1Job).HasChangesNotIncludingChildren);
			cost.GetJob(shipment1);
			Assert(!((IBusinessObjectState)shipment1Job).HasChangesNotIncludingChildren);
		}

		public void TestOnE6_AK_ChequeBookSetChequeNumberSetWhenChequeBookIsSet()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_CurrentNo = 5;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			JobConsolCost cost = GetCost(consol);
			AccChequeBook chequeBook1 = Factory.New<AccChequeBook>();
			chequeBook1.AK_StartNo = 1000;
			chequeBook1.AK_LastNo = 1999;
			chequeBook1.AK_CurrentNo = 1493;
			AccChequeBook chequeBook2 = Factory.New<AccChequeBook>();
			chequeBook2.AK_StartNo = 2000;
			chequeBook2.AK_LastNo = 2999;
			chequeBook2.AK_CurrentNo = 2875;
			cost.E6_AK_ChequeBook = chequeBook1.PK;
			AssertEquals("Cheque Number is setted when ChequeBook is seted", "1493", cost.E6_ChequeOrReference);
			cost.E6_AK_ChequeBook = ZGuid.Empty;
			AssertEquals("Cheque Number is empty when ChequeBook is empty", "", cost.E6_ChequeOrReference);
			cost.E6_AK_ChequeBook = ZGuid.Invalid;
			AssertEquals("Cheque Number is empty when ChequeBook is invalid", "", cost.E6_ChequeOrReference);
			cost.E6_AK_ChequeBook = chequeBook2.PK;
			AssertEquals("Cheque Number is setted when ChequeBook is set again", "2875", cost.E6_ChequeOrReference);
			cost.E6_AK_ChequeBook = ZGuid.Missing;
			AssertEquals("Cheque Number is empty when ChequeBook is Missing", "", cost.E6_ChequeOrReference);
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = testBank.PK;
			cost.E6_AK_ChequeBook = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", cost.IsChequeNumberAutoAllocated);
			AssertEquals("Cheque Number is empty when Auto Allocation is enabled", "", cost.E6_ChequeOrReference);
		}

		public void TestOnE6_ChequeOrReferenceSetNewCurrentChequeNoIsSet()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(bankAccount);
			JobConsolCost cost = GetCost(consol);
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChequeBook chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook1.AK_StartNo = 1000;
			chequeBook1.AK_LastNo = 1999;
			chequeBook1.AK_CurrentNo = 1493;
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = bankAccount.PK;
			cost.E6_AK_ChequeBook = testBookWithAutoAllocation.PK;
			cost.E6_AC_ChargeCode = chargeCode.PK;
			Assert("AutoAllocation should be enabled", cost.IsChequeNumberAutoAllocated);
			cost.E6_ChequeOrReference = "22";
			AssertEquals("AK_CurrentNo on cheque book should remain old", 0m, testBookWithAutoAllocation.AK_CurrentNo);
			cost.E6_AK_ChequeBook = chequeBook1.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !cost.IsChequeNumberAutoAllocated);
			Factory.Save();
			cost.E6_ChequeOrReference = "1494.5";
			AssertHasError("E6_ChequeOrReference should have error", cost.E6_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");
			cost.E6_ChequeOrReference = "1499";
			AssertEquals("New ChequeBook CurrentNo is setted when Cheque Number is seted", 1500m, chequeBook1.AK_CurrentNo);
		}

		public void TestChangingChequeBookToAutoAllocateWillResetJR_ChequeNo()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_CurrentNo = 5;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			JobConsolCost testCost = GetCost(consol);
			testCost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testCost.E6_AB_BankAccount = testBank.PK;
			testCost.E6_AK_ChequeBook = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !testCost.IsChequeNumberAutoAllocated);
			AssertEquals("JR_ChequeNo should change", "55", testCost.E6_ChequeOrReference);
			Assert("ChequeNumberInfo should not be read only", !testCost.E6_ChequeOrReferenceInfo.ReadOnly);
			testCost.E6_AK_ChequeBook = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testCost.IsChequeNumberAutoAllocated);
			Assert("JR_ChequeNo should be reset", testCost.E6_ChequeOrReference.IsEmpty);
			Assert("ChequeNumberInfo should be read only", testCost.E6_ChequeOrReferenceInfo.ReadOnly);
			testCost.E6_AK_ChequeBook = testChequeBook.PK;
			Assert("ChequeNumberInfo should not be read only", !testCost.E6_ChequeOrReferenceInfo.ReadOnly);
		}

		public void TestSetValidChequeDigits()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			JobConsolCost cost = GetCost(consol);
			AccChequeBook chequeBook1 = Factory.New<AccChequeBook>();
			chequeBook1.AK_StartNo = 1000;
			chequeBook1.AK_LastNo = 1999;
			chequeBook1.AK_CurrentNo = 1493;
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 6;
			cost.E6_AB_BankAccount = bankAccount.PK;
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			cost.E6_ChequeOrReference = "1499";
			AssertEquals("New ChequeBook CurrentNo is setted when Cheque Number is seted", "001499", cost.E6_ChequeOrReference);
		}

		public void TestSetupInvoiceAndPaymentDetailsFromRelatedCost_SupplierCostReference()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost1 = GetCost(consol);
			cost1.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			cost1.E6_OH_Creditor = testObjectCreator.AALSHI.PK;
			cost1.E6_InvoiceNum = "111111";
			cost1.E6_InvoiceDate = ZDateTime.Today;
			cost1.E6_CostReference = "ABC1";
			JobConsolCost cost2 = GetCost(consol);
			cost2.E6_AC_ChargeCode = testObjectCreator.CC2.PK;
			cost2.E6_InvoiceNum = "111111";
			AssertNotEquals("Precondition: Supplier Cost Reference on first and second Consol Costs are different", cost1.E6_CostReference, cost2.E6_CostReference);
			cost2.E6_OH_Creditor = cost1.E6_OH_Creditor;
			AssertEquals("Supplier Cost Reference on first Consol Cost should be copied to second Consol Cost", cost1.E6_CostReference, cost2.E6_CostReference);
		}

		public void TestErrorReportingInPopulateJobNumbersWithoutShipmentsOnConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.JK_UniqueConsignRef = "C001";
			JobConsolCost cost = GetCost(consol);
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			charge.JR_IsUsedForApportionment = true;
			AssertNull("Precondition: ", charge.InvoicingJob);
			ApportionSplitCharge charge2 = cost.ApportionmentCharges.AddNew();
			AssertNull("Precondition: ", charge2.InvoicingJob);
			try
			{
				cost.UpdateApportionmentChargesListing();
				var chargeToTest = cost.ApportionmentCharges[0];
				chargeToTest.RunPreSaveValidation();
				AssertHasError("JR_IsUsedForApportionment should have error", chargeToTest.JR_IsUsedForApportionmentInfo, "This apportioned charge belongs to job: " + chargeToTest.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
			}
			catch (InvalidOperationException ex)
			{
				Fail(string.Format("No exception must be thrown but was {0} with message: {1}", ex.GetType(), ex.Message));
			}

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "JOB123";
			charge.JR_JH = job.PK;
			charge.JR_IsUsedForApportionment = true;
			AssertNotNull("Precondition: ", charge.InvoicingJob);
			AssertNull("Precondition: ", charge.InvoicingJob.PlugInData);
			Job job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "JOB234";
			charge2.JR_JH = job2.PK;
			charge2.JR_IsUsedForApportionment = true;
			AssertNotNull("Precondition: ", charge2.InvoicingJob);
			AssertNull("Precondition: ", charge2.InvoicingJob.PlugInData);
			try
			{
				cost.UpdateApportionmentChargesListing();
				ApportionSplitCharge chargeToTest = (ApportionSplitCharge)cost.ApportionmentCharges.FindByPK(charge.PK);
				chargeToTest.JR_IsUsedForApportionment = true;
				chargeToTest.RunPreSaveValidation();
				AssertHasError("JR_IsUsedForApportionment should have error", chargeToTest.JR_IsUsedForApportionmentInfo, "This apportioned charge belongs to job: " + chargeToTest.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
			}
			catch (InvalidOperationException ex)
			{
				Fail(string.Format("No exception must be thrown but was {0} with message: {1}", ex.GetType(), ex.Message));
			}
			catch (Exception ex)
			{
				Fail(string.Format("No other exception must be thrown but was {0} with message: {1}", ex.GetType(), ex.Message));
			}
		}

		public void TestErrorReportingInPopulateJobNumbersWithInactiveShipmentsOnConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.CreateShipmentJobHeaderWithMutex();
			Factory.Save();
			consol.JK_UniqueConsignRef = "C001";
			JobConsolCost cost = GetCost(consol);
			cost.E6_AC_ChargeCode = this.ObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100m;
			Factory.Save();
			ApportionSplitCharge charge = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull("Precondition: charge", charge);
			AssertNotNull("Precondition: charge.InvoicingJob", charge.InvoicingJob);
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = tempFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInOtherFactory.JS_IsCancelled = true;
			tempFactory.Save();
			try
			{
				BusinessObjectFactory otherFactory = new BusinessObjectFactory();
				var consolInOtherFactory = otherFactory.Load<ForwardingConsol>(consol.PK);
				ApportionmentListing apps = new ApportionmentListing(otherFactory, consolInOtherFactory);
				AppLists.Add(apps);
				var costInOtherFactory = apps.CostsCollection.ToArray<JobConsolCost>().FirstOrDefault();
				costInOtherFactory.UpdateApportionmentChargesListing();
				var chargeToTest = costInOtherFactory.ApportionmentCharges[0];
				chargeToTest.RunPreSaveValidation();
				AssertHasError("JR_IsUsedForApportionment should have error", chargeToTest.JR_IsUsedForApportionmentInfo, "This apportioned charge belongs to job: " + chargeToTest.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
			}
			catch (InvalidOperationException ex)
			{
				Fail(string.Format("No exception must be thrown but was {0} with message: {1}", ex.GetType(), ex.Message));
			}
			catch (Exception ex)
			{
				Fail(string.Format("No other exception must be thrown but was {0} with message: {1}", ex.GetType(), ex.Message));
			}
		}

		public void TestDataRefreshDeleteChargeFromConsolCost()
		{
			var factoryA = new BusinessObjectFactory();
			var factoryB = new BusinessObjectFactory();

			var objectCreator = new TestObjectCreator(factoryA);
			var consol = objectCreator.CreateConsol("TSTC");
			var shipment1 = objectCreator.CreateShipment("S001", consol);
			var shipment2 = objectCreator.CreateShipment("S002", consol);
			var cost = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.Creditor1);
			cost.E6_LocalCostAmount = 100;
			cost.E6_AT_TaxRate = objectCreator.GSTFREE1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_RX_NKCurrency = objectCreator.AUD.Code;
			cost.UpdateApportionmentChargesListing();

			factoryA.Save();

			var shipment1B = factoryB.Load<ForwardingShipment>(shipment2.PK);
			cost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;
			cost.UpdateApportionmentChargesListing();

			factoryA.Save();

			cost.Delete();
			var charge = factoryB.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_JH, shipment1B.JobHeader.PK));
			charge.Delete();

			AssertNoExceptionThrown(() => factoryB.Save());
		}
	}
}
