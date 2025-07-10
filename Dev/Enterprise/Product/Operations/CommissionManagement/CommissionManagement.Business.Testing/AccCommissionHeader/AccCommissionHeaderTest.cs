using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(AccCommissionHeader))]
	internal class AccCommissionHeaderTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		public void TestSource()
		{
			var arInvoice = Factory.New<ARInvoice>();
			var commissionHeader = Factory.New<AccCommissionHeader>();
			commissionHeader.CH0_AH_Source = arInvoice.PK;
			AssertType(typeof(ARInvoice), commissionHeader.Source);

			var jrj = Factory.New<JobRevenueJournal>();
			var commissionHeader2 = Factory.New<AccCommissionHeader>();
			commissionHeader2.CH0_AH_Source = jrj.PK;
			AssertType(typeof(JobRevenueJournal), commissionHeader2.Source);
		}

		public void TestCH0_GroupingSourceTableCode_CH0_GroupingSourceID()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "00001005";

			var commissionHeader = Factory.New<AccCommissionHeader>();
			AssertEquals(ZString.Empty, commissionHeader.GroupingSourceNumber);
			AssertNull(commissionHeader.GroupingSourceJob);
			AssertNull(commissionHeader.GroupingSourceTransaction);

			commissionHeader.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeader.CH0_JobNumber = jobHeader.JH_JobNum;
			AssertEquals("S0001005", commissionHeader.GroupingSourceNumber);
			AssertEquals(jobHeader, commissionHeader.GroupingSourceJob);
			AssertNull(commissionHeader.GroupingSourceTransaction);

			commissionHeader.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			commissionHeader.CH0_GroupingSourceID = invoice.PK;
			AssertEquals("00001005", commissionHeader.GroupingSourceNumber);
			AssertNull(commissionHeader.GroupingSourceJob);
			AssertEquals(invoice, commissionHeader.GroupingSourceTransaction);
		}

		public void TestCH0_GroupingSourceTableCode_CH0_GroupingSourceID_JRJ()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";
			var jrj = Factory.New<JobRevenueJournal>();
			jrj.AH_TransactionNum = "00001005";

			var commissionHeader = Factory.New<AccCommissionHeader>();
			AssertEquals(ZString.Empty, commissionHeader.GroupingSourceNumber);
			AssertNull(commissionHeader.GroupingSourceJob);
			AssertNull(commissionHeader.GroupingSourceTransaction);

			commissionHeader.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeader.CH0_JobNumber = jobHeader.JH_JobNum;
			AssertEquals("S0001005", commissionHeader.GroupingSourceNumber);
			AssertEquals(jobHeader, commissionHeader.GroupingSourceJob);
			AssertNull(commissionHeader.GroupingSourceTransaction);

			commissionHeader.CH0_GroupingSourceTableCode = jrj.TablePrefix;
			commissionHeader.CH0_GroupingSourceID = jrj.PK;
			AssertEquals("00001005", commissionHeader.GroupingSourceNumber);
			AssertNull(commissionHeader.GroupingSourceJob);
			AssertEquals(jrj, commissionHeader.GroupingSourceTransaction);
		}

		public void TestGroupingSourceUniqueId()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";

			var commissionHeader1 = Factory.New<AccCommissionHeader>();
			commissionHeader1.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader1.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeader1.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeader1.CH0_GC = GlbCompany.CurrentCompany.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionNum = "00001005";

			var commissionHeader2 = Factory.New<AccCommissionHeader>();
			commissionHeader2.CH0_GroupingSourceTableCode = invoice.TablePrefix;
			commissionHeader2.CH0_GroupingSourceID = invoice.PK;

			AssertEquals("Job Commission Header", $"{GlbCompany.CurrentCompany.PK}-{jobHeader.JH_JobNum}", commissionHeader1.GroupingSourceUniqueId);
			AssertEquals("Invoice Commission Header", $"{invoice.PK}", commissionHeader2.GroupingSourceUniqueId);
		}

		public void TestCH0_SnapshotEventDescription()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();

			commissionHeader.CH0_SnapshotEventCode = "";
			AssertEquals("", commissionHeader.CH0_SnapshotEventDescription);

			commissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;
			AssertEquals(AccCommissionHeaderSnapshotEventList.Descriptions.Posted, commissionHeader.CH0_SnapshotEventDescription);

			commissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.JobClosure;
			AssertEquals(AccCommissionHeaderSnapshotEventList.Descriptions.JobClosure, commissionHeader.CH0_SnapshotEventDescription);

			commissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.JobCompletion;
			AssertEquals(AccCommissionHeaderSnapshotEventList.Descriptions.JobCompletion, commissionHeader.CH0_SnapshotEventDescription);
		}

		public void TestPropertyMaxLength()
		{
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccCommissionHeader.Schema.CH0_ProductMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccCommissionHeader.Schema.CH0_ServiceMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccCommissionHeader.Schema.CH0_SubModuleMaxLength);
		}

		#endregion

		#region Override

		[TestDate(2000, 1, 1)]
		public void TestMarkAsOverriden()
		{
			var commissionHeader = Factory.New<AccCommissionHeader>();
			var directLine = commissionHeader.Lines.AddNew();
			var groupLine = commissionHeader.LineGroups.AddNew().Lines.AddNew();

			AssertEquals(false, commissionHeader.IsOverriden);
			AssertEquals(false, directLine.IsOverriden);
			AssertEquals(false, groupLine.IsOverriden);

			commissionHeader.MarkAsOverriden();

			AssertEquals(true, commissionHeader.IsOverriden);
			AssertEquals(new ZDateTime(2000, 1, 1), commissionHeader.CH0_OverridenDateTimeUtc);

			AssertEquals(true, directLine.IsOverriden);
			AssertEquals(true, groupLine.IsOverriden);
		}

		#endregion
	}
}
