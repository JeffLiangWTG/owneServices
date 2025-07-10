using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.DefaultARInvoiceDate.Testing
{
	[TestedType(typeof(DefaultARInvoiceDateProcessorTask))]
	class DefaultARInvoiceDateProcessorTaskTest : ServiceTaskTestCase<DefaultARInvoiceDateProcessorTask>
	{
		[TestDate(2015, 1, 1)]
		public void TestRunTask()
		{
			var currentUser = EnvProxy.Instance.CurrentUser;
			AssertEquals("Prerequisite: Current user's email address should be read as [a@a.com]", "a@a.com", currentUser.EmailAddress);

			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			currentCompany.Branches[0].GB_RL_NKHomePort = "MLAMB"; //UTC 0
			Factory.Save();

			var dateTimeNow = ZDateTime.Now;
			AssertEquals(0, dateTimeNow.Hour);
			AssertEquals(0, currentCompany.Branches[0].HomePort.LocationDateTime.Hour);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			DefaultARInvoiceDateProcessorTask serviceTask = new DefaultARInvoiceDateProcessorTask();
			InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals(Env.OutgoingMailManager.EmailsCreated.Count, 1);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].FromAddress, "a@a.com");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Subject, "The Invoice Date Incrementing Suspension needs to be lifted.");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Body, @"<p>Invoice Date Incrementing is suspended for Company EDI.</p><p>While the daily incrementing suspension is in effect, the ""Current Invoice Date"" will remain as 01-Jan-00.<br>To lift the suspension, go to Manage > Receivables > Receivables Transactions > Actions > Reinstate Daily Invoice Date Incrementing.</p>");

			using (Env.Instance.TemporaryServiceTaskContext(DefaultARInvoiceDateProcessorTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		// No nudging: no queue table; service task sends email each month.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("IDN");
			AssertNull("No queue table for this service task; sends email each month.", queueProvider);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			var staffGroup = Factory.New<GlbGroup>();
			var curStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staffGroup.Staff.Add(curStaff);
			curStaff.GS_EmailAddress = "a@a.com";
			Factory.Save();
			AccountingConfigurationRegistry.Instance.InvoiceDateIncrementingSuspensionNotifyGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, staffGroup.PK.ToGuid());

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "MTH");
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}
	}
}
