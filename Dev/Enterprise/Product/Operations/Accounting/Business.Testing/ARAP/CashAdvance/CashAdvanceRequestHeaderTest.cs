using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.CashAdvance.Testing
{
	[TestedType(typeof(CashAdvanceRequestHeader))]
	public class CashAdvanceRequestHeaderTest : AccCashAdvanceRequestHeaderTest
	{
		public void TestRequestReferenceIsSetOnSaving()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new TestObjectCreator(Factory).CreateJob(shipment);
			Factory.Save();

			var header1 = CreateCashAdvanceRequest(job);
			var header2 = CreateCashAdvanceRequest(job);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { header1, header2 }.Select(x => x.CAH_RequestReferenceNumber));
		}

		public void TestJobRelatedProperties()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Office, true);

			var consol = TestObjectCreator.CreateConsol();
			consol.JK_MasterBillNum = "MB00001";
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);
			shipment.JS_HouseBill = "HB00001";
			var job = new TestObjectCreator(Factory).CreateJob(shipment);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge 01", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 250M, TestObjectCreator.Debtor);
			charge.JR_OA_SellInvoiceAddress = address.PK;
			charge.JR_IsARCashAdvance = true;
			Factory.Save();

			var header = CreateCashAdvanceRequest(job);
			TestObjectCreator.CreateCashAdvanceRequestLine(header, 250M, 250M, CashAdvanceStatusCodes.RequestLine.Requested, charge);
			Factory.Save();

			AssertEquals(nameof(header.ShipmentMasterBill), "MB00001", header.ShipmentMasterBill);
			AssertEquals(nameof(header.ShipmentHouseBill), "HB00001", header.ShipmentHouseBill);
			AssertEquals(nameof(header.JobDepartment), job.Department.GE_Code, header.JobDepartment);
			AssertEquals(nameof(header.JobBranch), job.Branch.GB_Code, header.JobBranch);
			AssertEquals(nameof(header.DebtorAddress), address.PK, header.DebtorAddress);
		}

		CashAdvanceRequestHeader CreateCashAdvanceRequest(Job job)
		{
			var header = Factory.New(GetExpectedBusinessObjectType()) as CashAdvanceRequestHeader;
			header.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			header.CAH_Ledger = LedgerTypes.AccountsReceivable;
			header.CAH_OH_Organization = TestObjectCreator.AALSHI.PK;
			header.CAH_JH_Job = job.PK;
			header.CAH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			header.CAH_OSAmount = 100m;
			header.CAH_LocalAmount = 100m;
			header.CAH_OSPaidAmount = 0m;
			header.CAH_LocalPaidAmount = 0m;
			header.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			return header;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CashAdvanceRequestHeader>();
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;
	}
}
