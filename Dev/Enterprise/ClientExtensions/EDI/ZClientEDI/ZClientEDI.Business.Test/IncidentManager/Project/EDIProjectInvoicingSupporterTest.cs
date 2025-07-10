using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDIProjectInvoicingSupporter))]
	class EDIProjectInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		EDIProject Project
		{
			get
			{
				if (project == null)
				{
					project = Factory.New<EDIProject>();
				}
				return project;
			}
		}

		EDIProject project;

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var project = Factory.New<EDIProject>();
			return project;
		}

		public void TestJobNumber()
		{
			Project.WKP_ProjectNumber = "119944";
			AssertEquals("JobNumber", "119944", ((IJobInvoicingPlugIn)Project).JobNumber);
		}

		public void TestJobInvoicingSecurity_Project()
		{
			AssertEquals("JobInvoicingSecurity", EDISecurityCheckpoints.InstallationTaskJobInvoicing, ((IJobInvoicingPlugIn)Project).InvoicingSupporter.JobInvoicingSecurity);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", EDISecurityCheckpoints.InstallationTaskAuditBilling, ((IJobInvoicingPlugIn)Project).InvoicingSupporter.AuditSecurity);
		}

		public void TestSetJobNumberFieldOnSaving()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			JobHeader job = new JobHeader.Loader(project).TryLoadOrCreate();
			Factory.Save();
			AssertEquals("JH_JobNum populated", false, job.JH_JobNum.IsEmpty);
			AssertEquals("JH_JobNum populated", project.WKP_ProjectNumber, job.JH_JobNum);
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<EDIProject>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<EDIProject>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}
	}
}
