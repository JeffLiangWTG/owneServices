using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.Accounting.Business.AccountingCriticalValidationDependency.Testing
{
	public class AccountingCriticalValidationInfoCollectionHelperTest : TestCaseWithFactory
	{
		public void TestAddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			var jobCharges = job.Charges.ToArray<Charge>();
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, job, TestObjectCreator.CC1, 100M);
			Func<ZString> getMoreInfo = () => "Get More Info of APInvoice Line";
			var carryForwardAmountID = ZGuid.NewZGuid();

			IAccountingCriticalValidationInfoCollectionHelper accountingCVInfoCollectionHelper = new AccountingCriticalValidationInfoCollectionHelper();

			AssertContains("Precondition: GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns() first time", "There is no data collected for this PK", GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(carryForwardAmountID, apInvoiceLine, jobCharges, 0, 0, getMoreInfo);
			AssertContains("Should not report error when OS and Local amounts are 0", "There is no data collected for this PK", GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(carryForwardAmountID, apInvoiceLine, jobCharges, -100m, 0, getMoreInfo);
			AssertContains("Should not report error when OS amount is 0", "There is no data collected for this PK", GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(carryForwardAmountID, apInvoiceLine, jobCharges, 0, -100m, getMoreInfo);
			AssertContains("Should not report error when Local amount is 0", "There is no data collected for this PK", GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(carryForwardAmountID, apInvoiceLine, jobCharges, -100m, -100m, getMoreInfo);
			AssertContains("Should not report error when OS and Local amounts have same signs", "There is no data collected for this PK", GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(carryForwardAmountID, apInvoiceLine, jobCharges, 50m, -50m, getMoreInfo);
			var collectedInfo = GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns();

			var expectedInfo1 =
$@"
APInvoiceLinePostingCarryForwardAmountWithDifferentSigns:
Current Line PK: {apInvoiceLine.PK}
localAmount: 50, oSAmount: -50
IsPopulatedFromImportedJobCharge: False
Get More Info of APInvoice Line

Charges for carry forward calculation:
	PK = {jobCharges[0].PK}
	Type = Charge";
			var expectedInfo2 =
$@"

StackTrace: TestAddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns";

			AssertContainsInOrder("Should report an error because the OS and Local amounts have different signs and error reported Key: APInvoiceLinePostingCarryForwardAmountWithDifferentSigns", collectedInfo, expectedInfo1, expectedInfo2);

			string GetInfoForAPInvoiceLinePostingCarryForwardAmountWithDifferentSigns() => CriticalValidationInfoCollectorService.GetOrCreateService(apInvoiceLine.Factory).GetInfo(carryForwardAmountID, CriticalValidationInfoCollectorServiceKeyType.APInvoiceLinePostingCarryForwardAmountWithDifferentSigns);
		}

		public void TestAddInfo_CarryForwardChargeAmountWithDifferentSigns()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			var jobCharge = job.Charges[0];
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100M);
			var amountWrapper = new TransactionLineJobChargeTransformer.AmountWrapper(50m, -50m, true, ZGuid.NewZGuid());
			Func<ZString> getMoreInfo = () => "Get More Info Of Job Charge Line";
			IAccountingCriticalValidationInfoCollectionHelper accountingCVInfoCollectionHelper = new AccountingCriticalValidationInfoCollectionHelper();

			AssertContains("Precondition: GetInfoForCarryForwardChargeAmountWithDifferentSigns() first time", "There is no data collected for this PK", GetInfoForCarryForwardChargeAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_CarryForwardChargeAmountWithDifferentSigns(jobCharge, invoiceLine, amountWrapper, 0, 0, getMoreInfo);
			AssertContains("Should not report error when OS and Local amounts are 0", "There is no data collected for this PK", GetInfoForCarryForwardChargeAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_CarryForwardChargeAmountWithDifferentSigns(jobCharge, invoiceLine, amountWrapper, -100m, 0, getMoreInfo);
			AssertContains("Should not report error when OS amount is 0", "There is no data collected for this PK", GetInfoForCarryForwardChargeAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_CarryForwardChargeAmountWithDifferentSigns(jobCharge, invoiceLine, amountWrapper, 0, -100m, getMoreInfo);
			AssertContains("Should not report error when Local amount is 0", "There is no data collected for this PK", GetInfoForCarryForwardChargeAmountWithDifferentSigns());

			accountingCVInfoCollectionHelper.AddInfo_CarryForwardChargeAmountWithDifferentSigns(jobCharge, invoiceLine, amountWrapper, -100m, -100m, getMoreInfo);
			AssertContains("Should not report error when OS and Local amounts have same signs", "There is no data collected for this PK", GetInfoForCarryForwardChargeAmountWithDifferentSigns());

			string getCollectedInfoOfAPInvoiceLine = CriticalValidationInfoCollectorService.GetOrCreateService(invoiceLine.Factory).GetInfo(amountWrapper.InfoCollectionID, CriticalValidationInfoCollectorServiceKeyType.APInvoiceLinePostingCarryForwardAmountWithDifferentSigns);

			accountingCVInfoCollectionHelper.AddInfo_CarryForwardChargeAmountWithDifferentSigns(jobCharge, invoiceLine, amountWrapper, 50m, -50m, getMoreInfo);
			var collectedInfo = GetInfoForCarryForwardChargeAmountWithDifferentSigns();

			var expectedInfo1 =
$@"
CarryForwardChargeAmountWithDifferentSigns:
OSAmount: -50, LocalAmount: 50
IsOSCurrencyApplicable: True
Get More Info Of Job Charge Line
{getCollectedInfoOfAPInvoiceLine}

Charge with opposite amounts:
	PK = {jobCharge.PK}
	Type = Charge";
			var expectedInfo2 =
$@"
All Invoice Lines:
	PK = {invoiceLine.PK}
	Type = APInvoiceLine";
			var expectedInfo3 =
$@"

StackTrace: TestAddInfo_CarryForwardChargeAmountWithDifferentSigns";

			AssertContainsInOrder("Should report an error because the OS and Local amounts have different signs and error reported Key: CarryForwardChargeAmountWithDifferentSigns", collectedInfo, expectedInfo1, expectedInfo2, expectedInfo3);

			string GetInfoForCarryForwardChargeAmountWithDifferentSigns() => CriticalValidationInfoCollectorService.GetOrCreateService(jobCharge.Factory).GetInfo(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.CarryForwardChargeAmountWithDifferentSigns);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
