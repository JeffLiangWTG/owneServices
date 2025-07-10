using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(InvoicingBaseApprovalFilterBusinessObject))]
	public class InvoicingBaseApprovalFilterBusinessObjectTest : TransactionApprovalFilterBusinessObjectTest
	{
		public virtual void TestFilterMaxLengths()
		{
			AssertEquals("House Bill # max length", ModuleNumberFilter.MultiplyMaxLength(JobShipmentSchema.JS_HouseBill.MaxLength), FilterBO["House Bill #"].MaxLength);
			var expectedMasterBillMaxLength = ModuleNumberFilter.MultiplyMaxLength(Math.Max(JobConsolSchema.JK_MasterBillNum.MaxLength, Math.Max(JobShipmentSchema.JS_HouseBill.MaxLength, CusDecHouseBillSchema.CU_BillNum.MaxLength)));
			AssertEquals("Master Bill #/Ocean Bill # max length", expectedMasterBillMaxLength, FilterBO["Master Bill #/Ocean Bill #"].MaxLength);
			AssertEquals("Job Local Reference max length", ModuleNumberFilter.MultiplyMaxLength(JobHeaderSchema.JH_JobLocalReference.MaxLength), FilterBO["Job Local Reference"].MaxLength);
			AssertEquals("Flight/Voyage # and Vessel max length", JobVoyageSchema.JV_VoyageFlight.MaxLength, FilterBO["Flight/Voyage # and Vessel"].MaxLength);
			AssertEquals("Customs Entry # max length", ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength), FilterBO["Customs Entry #"].MaxLength);
			AssertEquals("Order # max length", ModuleNumberFilter.MultiplyMaxLength(JobOrderHeaderSchema.JD_OrderNumber.MaxLength), FilterBO["Order #"].MaxLength);
			AssertEquals($"{Business.AccountingUtils.NumberFilterTypes.JobNumber} max length", JobHeaderSchema.JH_JobNum.MaxLength, FilterBO[Business.AccountingUtils.NumberFilterTypes.JobNumber].MaxLength);
		}

		public void TestLocalJobReferenceFilter()
		{
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Job Local Reference"];

			filter.Property = Job1.JH_JobLocalReference;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));

			filter.Property = "Denys";
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestVoyageVesselFilter()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "tvfvf1";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			var transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_Vessel = vessel.RV_FK;
			transport1.JW_VoyageFlight = "2222";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "tvfvf2";
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			var transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "7654321";

			transport2.JW_Vessel = "NkVessel";
			transport2.JW_VoyageFlight = "2233";

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_IsShipping = false;
			shipment2.JS_IsShipping = false;

			var jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			var jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ModuleTextAndNkFilter voyageVesselFilter = (ModuleTextAndNkFilter)FilterBO["Flight/Voyage # and Vessel"];

			voyageVesselFilter.NkProperty = "Test";
			voyageVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			voyageVesselFilter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			voyageVesselFilter.NkProperty = "Nk";
			voyageVesselFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));

			voyageVesselFilter.NkProperty = "";
			voyageVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));
		}

		public void TestJobNumFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Job #"];

			filter.Property = Job1.JH_JobNum;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Job2.JH_JobNum;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection not to contain approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var approvalForConsol = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalForConsol.XP_ParentID = consol.PK;
			var approvalForInvoice = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalForInvoice.XP_ParentID = invoice.PK;

			Factory.Save();

			filter.Property = consol.JK_UniqueConsignRef;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			AssertContainsExactElementsInAnyOrder("Should only contain approvalForConsol", approvalForConsol, FilterCollection);

			filter.Property = invoice.AH_TransactionNum;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			AssertContainsExactElementsInAnyOrder("Should only contain approvalForInvoice", approvalForInvoice, FilterCollection);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			invoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();

			invoice.AH_JH = job.PK;

			var approval = Factory.NewWithValidTestData<GenApprovalRequest>();
			approval.XP_ParentID = invoice.PK;
			Factory.Save();

			filter.Property = job.JH_JobNum;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			AssertContainsExactElementsInAnyOrder("Expecting collection to contain invoice", approval, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			Assert("Not expecting collection to contain invoice", !FilterCollection.Contains(approval));
			AssertEquals("Expecting 2 Jobs to be featched", 2, FilterCollection.Count);
		}

		public void TestJobNumFilterWhenFilterValueIsGreaterThanFieldsMaxLength()
		{
			var jobNumberfilter = (ModuleTextFilter)FilterBO["Job #"];
			jobNumberfilter.IsActive = true;

			AssertEquals("job # filter max length", JobHeaderSchema.JH_JobNum.MaxLength, jobNumberfilter.MaxLength);

			jobNumberfilter.Property = TestObjectCreator.GetRandomString(JobHeaderSchema.JH_JobNum.MaxLength);
			AssertContains(JobHeaderSchema.JH_JobNum.Name, FilterBO.Filter.GetAsWhereClause(false));
			AssertContains(AccTransactionHeaderSchema.AH_TransactionNum.Name, FilterBO.Filter.GetAsWhereClause(false));

			jobNumberfilter.Property = TestObjectCreator.GetRandomString(ViewGenericConsolSchema.VX_Code.MaxLength);
			AssertContains(JobHeaderSchema.JH_JobNum.Name, FilterBO.Filter.GetAsWhereClause(false));
			AssertContains(ViewGenericConsolSchema.VX_Code.Name, FilterBO.Filter.GetAsWhereClause(false));
			AssertContains(AccTransactionHeaderSchema.AH_TransactionNum.Name, FilterBO.Filter.GetAsWhereClause(false));

			jobNumberfilter.Property = TestObjectCreator.GetRandomString(ViewGenericConsolSchema.VX_Code.MaxLength + 1);
			AssertContains(JobHeaderSchema.JH_JobNum.Name, FilterBO.Filter.GetAsWhereClause(false));
			AssertNotContains(ViewGenericConsolSchema.VX_Code.Name, FilterBO.Filter.GetAsWhereClause(false));
			AssertContains(AccTransactionHeaderSchema.AH_TransactionNum.Name, FilterBO.Filter.GetAsWhereClause(false));

			//Cannot set 38 charater AH_TransactionNum as JH_JobNum, JH_JobNum max length is 35. Causes a developer notification exception
			//These assertions are needed if there comes a situation where JH_JobNum max length > AH_TransactionNum max length
			if (jobNumberfilter.MaxLength > AccTransactionHeaderSchema.AH_TransactionNum.MaxLength)
			{
				jobNumberfilter.Property = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);
				AssertContains(JobHeaderSchema.JH_JobNum.Name, FilterBO.Filter.GetAsWhereClause(false));
				AssertContains(AccTransactionHeaderSchema.AH_TransactionNum.Name, FilterBO.Filter.GetAsWhereClause(false));

				jobNumberfilter.Property = TestObjectCreator.GetRandomString(AccTransactionHeaderSchema.AH_TransactionNum.MaxLength + 1);
				AssertContains(JobHeaderSchema.JH_JobNum.Name, FilterBO.Filter.GetAsWhereClause(false));
				AssertNotContains(AccTransactionHeaderSchema.AH_TransactionNum.Name, FilterBO.Filter.GetAsWhereClause(false));
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoicingBaseApprovalFilterBusinessObject();
		}

		InvoicingBaseApprovalFilterBusinessObject FilterBO;
		ActiveBusinessObjectCollection<GenApprovalRequest> FilterCollection;
		Job Job1;
		Job Job2;
		GenApprovalRequest Approval1;
		GenApprovalRequest Approval2;

		protected override void SetUp()
		{
			base.SetUp();
			FilterBO = (InvoicingBaseApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			FilterCollection = new ActiveBusinessObjectCollection<GenApprovalRequest>(Factory);

			Job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Approval1 = Factory.NewWithValidTestData<GenApprovalRequest>();
			Approval2 = Factory.NewWithValidTestData<GenApprovalRequest>();

			Approval1.XP_ParentID = Job1.PK;
			Approval2.XP_ParentID = Job2.PK;

			Factory.Save();
		}
	}
}
