using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.DefaultARInvoiceDate.Testing
{
	class DefaultARInvoiceDateNotifierTest : TestCaseWithFactory
	{
		[TestDate(2015, 1, 1)]
		public void TestProcess()
		{
			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			currentCompany.Branches[0].GB_RL_NKHomePort = "MLAMB"; //UTC 0
			Factory.Save();

			var dateTimeNow = ZDateTime.Now;
			AssertEquals(0, dateTimeNow.Hour);
			AssertEquals(0, currentCompany.Branches[0].HomePort.LocationDateTime.Hour);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staffGroup = Factory.New<GlbGroup>();
			var curStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staffGroup.Staff.Add(curStaff);
			curStaff.GS_EmailAddress = "a@a.com";
			Factory.Save();
			AccountingConfigurationRegistry.Instance.InvoiceDateIncrementingSuspensionNotifyGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, staffGroup.PK.ToGuid());

			var currentUser = EnvProxy.Instance.CurrentUser;
			AssertEquals("Prerequisite: Current user's email address should be read as [a@a.com]", "a@a.com", currentUser.EmailAddress);

			var notifier = new DefaultARInvoiceDateNotifier(Factory);
			var buffer = new NotificationBuffer();

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			notifier.Process(buffer);
			AssertEquals(0, buffer.Events.Count());

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "MTH");
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2015, 1, 1));
			notifier.Process(buffer);
			AssertEquals(0, buffer.Events.Count());

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2014, 12, 1));
			notifier.Process(buffer);
			AssertEquals(2, buffer.Events.Count());
			buffer.Events.Contains(string.Format("Sending email notification to Company {0}", Env.CurrentCompany.Code));
			buffer.Events.Contains(string.Format("Email notification sent to Company {0}", Env.CurrentCompany.Code));

			AssertEquals(Env.OutgoingMailManager.EmailsCreated.Count, 1);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].FromAddress, "a@a.com");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Subject, "The Invoice Date Incrementing Suspension needs to be lifted.");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Body, @"<p>Invoice Date Incrementing is suspended for Company EDI.</p><p>While the daily incrementing suspension is in effect, the ""Current Invoice Date"" will remain as 31-Dec-14.<br>To lift the suspension, go to Manage > Receivables > Receivables Transactions > Actions > Reinstate Daily Invoice Date Incrementing.</p>");

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[ExpectNoExceptions]
		public void TestProcessOnCompanyWithoutBranch()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_IsActive = true;
			company.GC_Code = "TES";
			company.GC_Name = "Company for test";
			company.GC_RN_NKCountryCode = "AU";
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.GC_State = "NSW";
			company.GC_City = "Alexandria";
			company.GC_RX_NKLocalCurrency = "AUD";
			Factory.Save();

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "MTH");
			var notifier = new DefaultARInvoiceDateNotifier(Factory);
			var buffer = new NotificationBuffer();
			notifier.Process(buffer);
		}
	}
}
