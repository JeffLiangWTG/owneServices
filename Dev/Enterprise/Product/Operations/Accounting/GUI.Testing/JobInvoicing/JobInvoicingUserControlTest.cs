using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobInvoicingUserControlTest : TestCaseWithFactory
	{
		public void TestTabOrder()
		{
			using (JobInvoicingUserControl control = new JobInvoicingUserControl())
			{
				AssertEquals("Tab Pages Count", 6, control.JobInvoicingTabControl_ForTestOnly.TabPages.Count);
				AssertEquals("Invoicing", control.JobInvoicingTabControl_ForTestOnly.TabPages[0].Text);
				AssertEquals("Profit and Loss", control.JobInvoicingTabControl_ForTestOnly.TabPages[1].Text);
				AssertEquals("AR Invoices", control.JobInvoicingTabControl_ForTestOnly.TabPages[2].Text);
				AssertEquals("AP Invoices", control.JobInvoicingTabControl_ForTestOnly.TabPages[3].Text);
				AssertEquals("Credit Status", control.JobInvoicingTabControl_ForTestOnly.TabPages[4].Text);
				AssertEquals("Advance Payments", control.JobInvoicingTabControl_ForTestOnly.TabPages[5].Text);
			}
		}

		public void TestSettingSecurityCheckPoint()
		{
			using (JobInvoicingUserControl control = new JobInvoicingUserControl(Env.Security.MaintainShipmentJobInvoicing))
			{
				AssertEquals(Env.Security.MaintainShipmentJobInvoicing, control.JobInvoicePrintingControl.PluginSecurity);
				AssertEquals(Env.Security.MaintainShipmentJobInvoicing, control.JobProfitLossControl.PluginSecurity);
				AssertEquals(Env.Security.MaintainShipmentJobInvoicing, control.CashAdvanceRequestUserControl.PluginSecurity);
			}
		}

		#region TestCreditReport

		public void TestCreditReportsVisibility()
		{
			CombineAssertions(() =>
			{
				AssertCreditReportsVisibility("S0010001", false, true, true, true);
				AssertCreditReportsVisibility("S0010002", false, true, false, true);
				AssertCreditReportsVisibility("S0010003", false, false, true, true);
				AssertCreditReportsVisibility("S0010004", false, false, false, true);
				AssertCreditReportsVisibility("S0010005", true, true, true, false);
				AssertCreditReportsVisibility("S0010006", true, true, false, true);
				AssertCreditReportsVisibility("S0010007", true, false, true, true);
				AssertCreditReportsVisibility("S0010008", true, false, false, true);
			});
		}

		void AssertCreditReportsVisibility(string shipmentNumber, bool hasUrl, bool enableCreditReports, bool currentCompanyCountryAvailable, bool expectedPanel1Collapsed)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = org.PK;

			Factory.Save();

			var creditReportItem = CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code);
			creditReportItem.CountryEnabledForCompany = currentCompanyCountryAvailable;
			var creditCollection = new CreditReportItemCollection() { creditReportItem };

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			if (hasUrl)
			{
				collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });
			}

			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditCollection))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCreditReports))
			using (var testForm = new ZForm(job))
			using (var control = new JobInvoicingUserControl())
			{
				control.JobChargeUserControl.Job = job;

				testForm.Controls.Add(control);

				testForm.Show();
				control.CreditStatusTabPage.Show();

				var splitContainer = (KSplitContainer)control.JobCreditStatusControl.Controls.Find("CreditStatusSplitContainer", true).First();
				AssertEquals(expectedPanel1Collapsed, splitContainer.Panel1Collapsed);
			}
		}

		public void TestAutoScrollForCostTabPage()
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			using (var testForm = new ZForm(job))
			using (var control = new JobInvoicingUserControl())
			{
				control.JobChargeUserControl.Job = job;
				testForm.Controls.Add(control);
				testForm.AutoSize = true;
				testForm.ClientSize = new Size(testForm.ClientSize.Width / 2, testForm.ClientSize.Height / 2);
				testForm.Show();

				var tabControl = testForm.Controls.Find("ChargesDetailsTabControl", true).First() as ZTabControl;
				var costTabPage = tabControl.GetTabPage("CostTabPage");
				tabControl.SelectedTab = costTabPage;

				AssertEquals(true, costTabPage.AutoScroll);
				AssertEquals(true, costTabPage.HorizontalScroll.Visible);
				AssertEquals(false, costTabPage.VerticalScroll.Visible);
			}
		}

		public void TestCreditReportWithDataSource_WithNonEmptyLocalCharges()
		{
			AssertCreditReportWithDataSource(true);
		}

		public void TestCreditReportWithDataSource_WithEmptyLocalCharges()
		{
			AssertCreditReportWithDataSource(false);
		}

		void AssertCreditReportWithDataSource(bool nonEmptyLocalCharges)
		{
			OrgHeader org = null;

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			if (nonEmptyLocalCharges)
			{
				org = Factory.NewWithValidTestData<OrgHeader>();
				job.LocalChargesPK = org.PK;
			}
			else
			{
				job.LocalChargesPK = Guid.Empty;
			}

			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() { CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code) };
			creditReportItemCollection[0].CountryEnabledForCompany = true;

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });

			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var testForm = new ZForm(job))
			using (var control = new JobInvoicingUserControl())
			{
				control.JobChargeUserControl.Job = job;

				testForm.Controls.Add(control);

				testForm.Show();
				control.CreditStatusTabPage.Show();

				var jobControl = control.JobCreditStatusControl;
				var splitContainer = (KSplitContainer)control.Controls.Find("CreditStatusSplitContainer", true).First();

				AssertEquals(1, splitContainer.Panel1.Controls.Count);

				var reportControl = splitContainer.Panel1.Controls[0] as CreditReportUserControl;
				var bindingSource = reportControl.BindingSource;
				AssertNotNull("BindingSource should not be null", bindingSource);

				var dataSource = bindingSource.DataSource as OrgHeader;
				if (nonEmptyLocalCharges)
				{
					AssertNotNull("DataSource should not be null", dataSource);
					AssertEquals(org.PK, dataSource.PK);
					AssertEquals(org.PK, job.CreditStatusBizObject.OrganisationPK);
				}
				else
				{
					AssertNull("DataSource should be null", dataSource);
					Assert(job.CreditStatusBizObject.OrganisationPK.IsEmpty);
				}
			}
		}

		public void TestDraftInvoiceGroupBoxVisibility()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var control = new JobInvoicingUserControl())
			{
				AssertEquals("Panel2 should be collapsed so DraftInvoiceGroupBox is visible", true, control.APInvoicePrintingSplitContainer.Panel2Collapsed);
			}

			using (AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var control = new JobInvoicingUserControl())
			{
				AssertEquals("Panel2 should not be collapsed so so DraftInvoiceGroupBox is not visible", false, control.APInvoicePrintingSplitContainer.Panel2Collapsed);
			}
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		#endregion
	}
}
