using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(APInvoicingBaseApprovalFilterBusinessObject))]
	public class APInvoicingBaseApprovalFilterBusinessObjectTest : InvoicingBaseApprovalFilterBusinessObjectTest
	{
		public void TestBranchFilter()
		{
			Invoice1.AH_GB = Branch1.PK;
			Invoice2.AH_GB = Branch2.PK;

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.JH_GB = Invoice1.AH_GB;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.JH_GB = Invoice2.AH_GB;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Branch"];

			filter.Property = Branch1.PK;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property = Branch2.PK;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestDepartmentFilter()
		{
			Invoice1.AH_GE = Department1.PK;
			Invoice2.AH_GE = Department2.PK;

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.JH_GE = Invoice1.AH_GE;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.JH_GE = Invoice2.AH_GE;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Department"];

			filter.Property = Department1.PK;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property = Department2.PK;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestCreditorFilter()
		{
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_IsCreditor = true;
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_IsCreditor = true;
			Invoice1.AH_OH = creditor1.PK;
			Invoice2.AH_OH = creditor2.PK;

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.Charges[0].JR_OH_CostAccount = Invoice1.AH_OH;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.Charges[0].JR_OH_CostAccount = Invoice2.AH_OH;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Creditor"];

			filter.Property = creditor1.PK;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain approval2chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property = creditor2.PK;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestCreditorGroupFilter()
		{
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			var creditorGroup1 = Factory.NewWithValidTestData<OrgCreditorGroup>();
			creditor1.OH_IsCreditor = true;
			creditor1.CompanyData.OB_OG_APCreditorGroup = creditorGroup1.PK;

			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			var creditorGroup2 = Factory.NewWithValidTestData<OrgCreditorGroup>();
			creditor2.OH_IsCreditor = true;
			creditor2.CompanyData.OB_OG_APCreditorGroup = creditorGroup2.PK;

			Invoice1.AH_OH = creditor1.PK;
			Invoice2.AH_OH = creditor2.PK;

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.Charges[0].JR_OH_CostAccount = Invoice1.AH_OH;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.Charges[0].JR_OH_CostAccount = Invoice2.AH_OH;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Creditor Group"];

			filter.Property = creditorGroup1.PK;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property = creditorGroup2.PK;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestInvoiceDateFilter()
		{
			Invoice1.AH_InvoiceDate = new ZDateTime(2016, 9, 1);
			Invoice2.AH_InvoiceDate = new ZDateTime(2016, 9, 5);

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.Charges[0].JR_APInvoiceDate = Invoice1.AH_InvoiceDate;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.Charges[0].JR_APInvoiceDate = Invoice2.AH_InvoiceDate;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Invoice Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 9, 1);
			filter.Property2 = new ZDateTime(2016, 9, 1);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property2 = new ZDateTime(2016, 9, 10);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestDocumentReceivedDate()
		{
			Invoice1.AH_DocumentReceivedDate = new ZDateTime(2016, 9, 1);
			Invoice2.AH_DocumentReceivedDate = new ZDateTime(2016, 9, 5);

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Document Received Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 9, 1);
			filter.Property2 = new ZDateTime(2016, 9, 1);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));

			filter.Property2 = new ZDateTime(2016, 9, 10);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
		}

		public void TestDueDateFilter()
		{
			Invoice1.AH_DueDate = new ZDateTime(2016, 9, 1);
			Invoice2.AH_DueDate = new ZDateTime(2016, 9, 5);

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.Charges[0].JR_PaymentDate = Invoice1.AH_DueDate;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.Charges[0].JR_PaymentDate = Invoice2.AH_DueDate;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Due Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 9, 1);
			filter.Property2 = new ZDateTime(2016, 9, 1);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property2 = new ZDateTime(2016, 9, 10);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestRequisitionDateFilter()
		{
			Invoice1.AH_RequisitionDate = new ZDateTime(2016, 9, 1);
			Invoice2.AH_RequisitionDate = new ZDateTime(2016, 9, 5);

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(chargeApprovalRequest1);
			invoiceCharges.RequisitionDate = Invoice1.AH_RequisitionDate;

			var job3 = SetupJobWithCharge("S003");
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);
			invoiceCharges2.PostingGUIProvider.ShowApprovalFormToSetDescription(chargeApprovalRequest2);
			invoiceCharges2.RequisitionDate = Invoice2.AH_RequisitionDate;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Requisition Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 9, 1);
			filter.Property2 = new ZDateTime(2016, 9, 1);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property2 = new ZDateTime(2016, 9, 10);
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestRequisitionStatusFilter()
		{
			Invoice1.AH_RequisitionStatus = "NRM";
			Invoice2.AH_RequisitionStatus = "URG";

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(chargeApprovalRequest1);
			invoiceCharges.RequisitionStatus = Invoice1.AH_RequisitionStatus;

			var job3 = SetupJobWithCharge("S003");
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);
			invoiceCharges2.PostingGUIProvider.ShowApprovalFormToSetDescription(chargeApprovalRequest2);
			invoiceCharges2.RequisitionStatus = Invoice2.AH_RequisitionStatus;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Requisition Status"];

			filter.Property = "NRM";
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property = "URG";
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestInvoiceCurrencyFilter()
		{
			Invoice1.AH_RX_NKTransactionCurrency = "AUD";
			Invoice2.AH_RX_NKTransactionCurrency = "USD";

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.Charges[0].JR_RX_NKCostCurrency = Invoice1.AH_RX_NKTransactionCurrency;
			var job2 = SetupJobWithCharge("S002");
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.Charges[0].JR_RX_NKCostCurrency = Invoice2.AH_RX_NKTransactionCurrency;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			var filter = (ModuleNkFilter)FilterBO["Invoice Currency"];

			filter.Property = "AUD";
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property = "USD";
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestInvoiceAmountFilter()
		{
			AssertInvoiceAmountFilter();
		}

		public void TestInvoiceAmountFilterWithComma()
		{
			AssertInvoiceAmountFilter(useComma: true);
		}

		public void AssertInvoiceAmountFilter(bool useComma = false)
		{
			TestObjectCreator.CreateInvoiceLine(Invoice1, Invoice1.TransactionCurrency, Invoice1.AH_ExchangeRate, 100m, 20m, 0m);
			TestObjectCreator.CreateInvoiceLine(Invoice2, Invoice2.TransactionCurrency, Invoice2.AH_ExchangeRate, 200m, 40m, 0m);

			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.Charges[0].JR_OSCostAmt = 40;
			var job2 = SetupJobWithCharge("S002");
			job2.Charges[0].JR_OSCostAmt = 60;
			var invoiceCharges = SetupAPInvoiceCharges(job1.Charges[0], job2.Charges[0]);
			var chargeApprovalRequest1 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest1.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix);

			var job3 = SetupJobWithCharge("S003");
			job3.Charges[0].JR_OSCostAmt = 200;
			var invoiceCharges2 = SetupAPInvoiceCharges(job3.Charges[0]);
			var chargeApprovalRequest2 = Factory.New<APInvoiceChargesApprovalRequest>();
			chargeApprovalRequest2.InitializeJobRelated(invoiceCharges2, job3.PK, job3.TablePrefix);

			Factory.Save();

			if (useComma)
			{
				var pkChargeApproval = chargeApprovalRequest1.PK;

				var query = new ZQuery(GenAddOnColumnSchema.XA_Name, APInvoiceChargesApprovalRequest.AddOnColumnNames.InvoiceOSTotalAmount);
				query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, pkChargeApproval);
				GenAddOnColumn addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
				addOnColumn.XA_Data = "-120,0000";
				Factory.Save();

				var pkChargeApproval2 = chargeApprovalRequest2.PK;

				query = new ZQuery(GenAddOnColumnSchema.XA_Name, APInvoiceChargesApprovalRequest.AddOnColumnNames.InvoiceOSTotalAmount);
				query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, pkChargeApproval2);
				addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
				addOnColumn.XA_Data = "-240,0000";
				Factory.Save();
			}

			var filter = (ModuleNumberRangeFilter)FilterBO["Transaction Amount"];

			filter.Property1 = -130m;
			filter.Property2 = -110m;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain chargeApprovalRequest1", FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
			Assert("Expecting collection not to contain chargeApprovalRequest2", !FilterCollection.Contains(chargeApprovalRequest2));

			filter.Property1 = -250m;
			filter.Property2 = -230m;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain chargeApprovalRequest1", !FilterCollection.Contains(chargeApprovalRequest1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain chargeApprovalRequest2", FilterCollection.Contains(chargeApprovalRequest2));
		}

		public void TestTaxBranchFilter()
		{
			Invoice1.AH_GB_TaxBranch = Branch1.PK;
			Invoice2.AH_GB_TaxBranch = Branch2.PK;
			Approval1.XP_ParentID = Invoice1.PK;
			Approval2.XP_ParentID = Invoice2.PK;

			var job1 = SetupJobWithCharge("S001");
			job1.JH_GB = Invoice1.AH_GB_TaxBranch;

			var job2 = SetupJobWithCharge("S002");
			job2.JH_GB = Invoice2.AH_GB_TaxBranch;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			FilterBO = (APInvoicingBaseApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)FilterBO["Tax Branch"];
			AssertNull("Should not have a TaxBranch Filter when TaxBranchReporting registry is false", filter);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			FilterBO = (APInvoicingBaseApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (ModuleGuidFilter)FilterBO["Tax Branch"];
			AssertNull("Should not have a TaxBranch Filter when company is not GST registered", filter);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			FilterBO = (APInvoicingBaseApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (ModuleGuidFilter)FilterBO["Tax Branch"];
			AssertNotNull("Should have a TaxBranch Filter when both TaxBranchReporting registry and company is GST Registered are true", filter);

			filter.Property = Branch1.PK;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Branch2.PK;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
		}

		#region Implementation

		Job SetupJobWithCharge(ZString jobNumber)
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST2.PK;
			return job;
		}

		APInvoiceCharges SetupAPInvoiceCharges(params Charge[] charges)
		{
			var invoiceCharges = new APInvoiceCharges(TestObjectCreator.Creditor1.OH_Code, "INV1", ZGuid.NewZGuid(), JobConsolSchema.Constants.Prefix, null);
			invoiceCharges.Charges.AddRange(charges);

			return invoiceCharges;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APInvoicingBaseApprovalFilterBusinessObject();
		}

		APInvoicingBaseApprovalFilterBusinessObject FilterBO;
		ActiveBusinessObjectCollection<APInvoiceChargesApprovalRequest> FilterCollection;

		GlbBranch Branch1;
		GlbBranch Branch2;
		GlbDepartment Department1;
		GlbDepartment Department2;

		APInvoiceChargesApprovalRequest Approval1;
		APInvoiceChargesApprovalRequest Approval2;
		APInvoice Invoice1;
		APInvoice Invoice2;

		protected override void SetUp()
		{
			base.SetUp();

			FilterBO = (APInvoicingBaseApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			FilterCollection = new ActiveBusinessObjectCollection<APInvoiceChargesApprovalRequest>(Factory);

			Approval1 = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			Approval2 = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();

			Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			Branch2 = Factory.NewWithValidTestData<GlbBranch>();

			Department1 = Factory.NewWithValidTestData<GlbDepartment>();
			Department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Invoice1 = Factory.NewWithValidTestData<APInvoice>();
			Invoice2 = Factory.NewWithValidTestData<APInvoice>();
		}

		#endregion
	}
}
