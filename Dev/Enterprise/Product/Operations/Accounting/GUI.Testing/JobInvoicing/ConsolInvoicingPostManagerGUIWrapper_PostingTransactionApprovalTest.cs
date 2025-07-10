using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public abstract class ConsolInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : PostManagerGUIWrapper_PostingTransactionApprovalTest
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
		protected Dictionary<ZGuid, ApportionmentListing> apportionmentsDictionary = new Dictionary<ZGuid, ApportionmentListing>();

		protected override PostManagerGUIWrapper GetNewGUIWrapperCore(params Job[] jobs)
		{
			var consol = GetConsol(jobs[0]);
			ApportionmentListing apportionments;
			if (!apportionmentsDictionary.TryGetValue(consol.PK, out apportionments))
			{
				apportionments = new ApportionmentListing(Factory, consol);
				apportionmentsDictionary.Add(consol.PK, apportionments);
			}
			return new ConsolInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, jobs, consol, null, apportionments);
		}

		protected IJobCostingPlugIn GetConsol(Job job)
		{
			return ((CommonShipment)job.PlugInData).Consols.Cast<IJobCostingPlugIn>().First();
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
	}

	[TestedType(typeof(ConsolInvoicingPostManagerGUIWrapper))]
	public class ARCreditNoteConsolInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : ConsolInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(ConsolInvoicingPostManagerGUIWrapper))]
	public class APInvoiceChargesConsolInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest : ConsolInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return false; }
		}
	}
}
