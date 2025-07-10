using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.Testing
{
	sealed class ReopenClosedJobSecurityOverrideProviderTest : SecurityOverrideProviderWithJobReopenSupportTest<ReopenClosedJobSecurityOverrideProvider>
	{
		protected override bool ShouldPromptForGranted => true;

		protected override bool IsApprovalRequestButtonSupported => false;

		protected override ReopenClosedJobSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var reopenClosedJobSecurityOverrideProvider = new ReopenClosedJobSecurityOverrideProvider();
			((IReopenClosedJobSecurityOverrideProvider)reopenClosedJobSecurityOverrideProvider).AddClosedJobForSecurityProvider(new List<Job> { job });

			return reopenClosedJobSecurityOverrideProvider;
		}

		public void TestJobNumbersInMessages()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "JOB1";
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_JobNum = "JOB2";

			var provider = new ReopenClosedJobSecurityOverrideProvider();
			Action<Job> addJobToProvider = (Job job) => (provider as IReopenClosedJobSecurityOverrideProvider).AddClosedJobForSecurityProvider(new List<Job> { job });

			addJobToProvider(job1);
			Assert(provider.GetReopenClosedJobSecurityGrantedMessage_ForTestOnly().Contains(job1.JH_JobNum));
			Assert(provider.GetReopenClosedJobSecurityOverrideMessage_ForTestOnly().Contains(job1.JH_JobNum));
			Assert(provider.GetReopenRestrictedClosedJobSecurityOverrideMessage_ForTestOnly().Contains(job1.JH_JobNum));

			addJobToProvider(job2);
			Assert(provider.GetReopenClosedJobSecurityGrantedMessage_ForTestOnly().Contains(job2.JH_JobNum));
			Assert(provider.GetReopenClosedJobSecurityOverrideMessage_ForTestOnly().Contains(job2.JH_JobNum));
			Assert(provider.GetReopenRestrictedClosedJobSecurityOverrideMessage_ForTestOnly().Contains(job2.JH_JobNum));
		}
	}
}
