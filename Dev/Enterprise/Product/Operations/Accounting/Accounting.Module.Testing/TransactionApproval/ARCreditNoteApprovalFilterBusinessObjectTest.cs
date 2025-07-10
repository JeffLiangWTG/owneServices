using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalFilterBusinessObject))]
	public class ARCreditNoteApprovalFilterBusinessObjectTest : InvoicingBaseApprovalFilterBusinessObjectTest
	{
		public override void TestFilterMaxLengths()
		{
			base.TestFilterMaxLengths();
			AssertEquals("Approval Type max length", GenApprovalRequestSchema.XP_ApprovalType.MaxLength, FilterBO["Approval Type"].MaxLength);
		}

		public void TestApprovalTypeFilterList()
		{
			var filter = (ModuleTextFilter)FilterBO["Approval Type"];
			var list = (ICodeDescriptionPairList)filter.List;
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(Core.Constants.GenApprovalRequestApprovalType.ARCreditNote));
			Assert(list.ContainsCode(Core.Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal));
		}

		public void TestApprovalTypeFilter()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var approval1 = Factory.New<ARCreditNoteApprovalRequest>();
			approval1.XP_ParentID = job.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var approval2 = Factory.New<ARCreditNoteApprovalRequest>();
			approval2.ChangeApprovalTypeForInvoiceReversal();
			approval2.XP_ParentID = invoice.PK;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Approval Type"];

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert(FilterCollection.Contains(approval1));
			Assert(FilterCollection.Contains(approval2));

			filter.Property = Core.Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert(!FilterCollection.Contains(approval1));
			Assert(FilterCollection.Contains(approval2));

			filter.Property = Core.Constants.GenApprovalRequestApprovalType.ARCreditNote;
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert(FilterCollection.Contains(approval1));
			Assert(!FilterCollection.Contains(approval2));
		}

		public void TestApprovingUserFilter1()
		{
			var filter = (ModuleNkFilter)FilterBO["Approving User 1"];
			filter.Property = "US1";
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			AssertEquals(2, FilterCollection.Count);
			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
		}

		public void TestApprovingUserFilter2()
		{
			var filter = (ModuleNkFilter)FilterBO["Approving User 2"];
			filter.Property = "US1";
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			AssertEquals(1, FilterCollection.Count);
			Assert("Expecting collection to contain approval3", FilterCollection.Contains(Approval3));
		}

		public void TestApprovingUserFilter()
		{
			var filter = (ModuleNkFilter)FilterBO["Approving User"];
			filter.Property = "US1";
			filter.IsActive = true;
			FilterCollection.AdditionalFilter = FilterBO.Filter;
			AssertEquals(3, FilterCollection.Count);
			Assert("Expecting collection to contain approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain approval2", FilterCollection.Contains(Approval2));
			Assert("Expecting collection to contain approval3", FilterCollection.Contains(Approval3));
		}

		public void TestJobNumberFilterWithChildRequests()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			var approval1 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			var approval1Child = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			var approval2 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();

			approval1.XP_ParentID = job1.PK;
			approval1Child.XP_ParentID = approval1.PK;
			approval2.XP_ParentID = job2.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Job #"];

			filter.Property = job1.JH_JobNum;
			filter.IsActive = true;

			FilterCollection.AdditionalFilter = FilterBO.Filter;

			Assert("Expecting collection to contain approval1", FilterCollection.Contains(approval1));
			Assert("Expecting collection to contain approval1", FilterCollection.Contains(approval1Child));
			Assert("Expecting collection not to contain approval2", !FilterCollection.Contains(approval2));
		}

		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARCreditNoteApprovalFilterBusinessObject();
		}

		ARCreditNoteApprovalFilterBusinessObject FilterBO;
		ActiveBusinessObjectCollection<ARCreditNoteApprovalRequest> FilterCollection;
		ARCreditNoteApprovalRequest Approval1;
		ARCreditNoteApprovalRequest Approval2;
		ARCreditNoteApprovalRequest Approval3;
		ARCreditNoteApprovalRequest Approval4;

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });

			FilterBO = (ARCreditNoteApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			FilterCollection = new ActiveBusinessObjectCollection<ARCreditNoteApprovalRequest>(Factory);

			Approval1 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			Approval2 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			Approval3 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			Approval4 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();

			Approval1.XP_GS_NKApprovingUser1 = "US1";
			Approval1.XP_GS_NKApprovingUser2 = "US2";

			Approval2.XP_GS_NKApprovingUser1 = "US1";
			Approval2.XP_GS_NKApprovingUser2 = "";

			Approval3.XP_GS_NKApprovingUser1 = "US3";
			Approval3.XP_GS_NKApprovingUser2 = "US1";

			Approval4.XP_GS_NKApprovingUser1 = "US2";
			Approval4.XP_GS_NKApprovingUser2 = "US3";

			Factory.Save();
		}

		#endregion
	}
}
