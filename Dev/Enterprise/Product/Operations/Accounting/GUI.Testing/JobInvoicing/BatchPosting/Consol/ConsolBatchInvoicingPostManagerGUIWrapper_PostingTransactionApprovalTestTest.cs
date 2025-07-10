using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	abstract class ConsolBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest : BaseBatchInvoicingPostManagerGUIWrapperTest_PostingTransactionApprovalTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			var query = new ZQuery(GlbDepartmentSchema.GE_Code, "FEA");
			ContextForConsolInvoicePosting = Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Factory.Load<GlbDepartment>(query).First().PK.ToGuid());
		}

		protected override void TearDown()
		{
			base.TearDown();
			ContextForConsolInvoicePosting.Dispose();
		}

		IDisposable ContextForConsolInvoicePosting;

		protected override BaseBatchInvoicingPostManagerGUIWrapper GetNewBatchGUIWrapper(params Job[] jobs)
		{
			var consols = jobs.Select(x => GetConsol(x));
			return new ConsolBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, consols.ToArray(), "Posting Option Name");
		}

		protected override Job SetupJobData(string jobNumber = "S001")
		{
			var job = base.SetupJobData(jobNumber);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C" + jobNumber);
			TestObjectCreator.Agent.OH_RL_NKClosestPort = "AUSYD";
			TestObjectCreator.LocalClient.OH_RL_NKClosestPort = "HKHKG";
			consol.JK_OA_ReceivingForwarderAddress = TestObjectCreator.Agent.Addresses.First().PK;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.LocalClient.Addresses.First().PK;
			consol.Shipments.Add((BusinessObject)job.Parent);

			return job;
		}

		protected override void GetParentForPostingAction(Job job, out ZGuid expectedParentID, out ZString expectedParentTableCode)
		{
			var consol = GetConsol(job);
			expectedParentID = consol.PK;
			expectedParentTableCode = JobConsolSchema.Constants.Prefix;
		}

		protected IJobCostingPlugIn GetConsol(Job job)
		{
			return ((CommonShipment)job.PlugInData).Consols.Cast<IJobCostingPlugIn>().First();
		}
	}

	[TestedType(typeof(ConsolBatchInvoicingPostManagerGUIWrapper))]
	class ARCreditNoteConsolBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest : ConsolBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(ConsolBatchInvoicingPostManagerGUIWrapper))]
	class APInvoiceChargesConsolBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest : ConsolBatchInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return false; }
		}
	}
}
