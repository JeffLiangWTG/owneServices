using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ReversalDatesForm))]
	public class ReversalDatesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var collection = new TransactionHeaderCollection(Factory);
			var creator = new TestObjectCreator(Factory);
			invoice = creator.CreateARInvoice<ARInvoice>("00001000", creator.AUD, 1.0m, creator.ABIGAS);
			collection.Add(invoice);
			invoice.GenerateReverseTransaction(false);
			collection.Add(invoice.ReverseInvoice);
			collection.HasChanges = false;
			var shipment = creator.CreateShipment("S00001000");
			var codes = new OperationsJobConfigurationCodes(shipment);
			var changeTransactionDatesBusinessObject = new ChangeTransactionDatesBusinessObject(Env.Security.MaintainShipmentJobInvoicing, Factory, codes);
			return new ReversalDatesForm(collection, changeTransactionDatesBusinessObject);
		}

		ARInvoice invoice;

		public void TestReverseDateFormContext()
		{
			using (ReversalDatesForm form = GetFormToBash() as ReversalDatesForm)
			{
				var factory = form.ReversedInvoices.Factory;
				Assert("ReverseDateForm context should be set", factory.HasContext(BusinessContext.ReverseDateForm));
				form.Show();
				Assert("ReverseDateForm context should be set", factory.HasContext(BusinessContext.ReverseDateForm));
				form.CancelButtonOnYesNoCancelPanel.PerformClick();
				Assert("ReverseDateForm context should be cleared", !factory.HasContext(BusinessContext.ReverseDateForm));
			}
		}

		public void TestFormShowErrorMessage()
		{
			using (ReversalDatesForm form = GetFormToBash() as ReversalDatesForm)
			{
				form.Show();
				invoice.AH_PostDate = new ZDateTime(1900, 01, 01);
				invoice.ReverseInvoice.AH_PostDate = new ZDateTime(1900, 01, 02);

				var userNotif = UnitTestUserNotification.Instance;
				var factory = form.ReversedInvoices.Factory;

				userNotif.ClearMessagesAndAnswers();
				form.YesButtonOnYesNoCancelPanel.PerformClick();
				AssertEquals("Expect error message on YES click", "There are errors - can't save.", userNotif.LastMessage.Text);
				Assert("ReverseDateForm context should NOT be cleared because error detected", factory.HasContext(BusinessContext.ReverseDateForm));

				userNotif.ClearMessagesAndAnswers();
				form.NoButtonOnYesNoCancelPanel.PerformClick();
				AssertEquals("Expect error message on NO click", "There are errors - can't save.", userNotif.LastMessage.Text);
				Assert("ReverseDateForm context should NOT be cleared because error detected", factory.HasContext(BusinessContext.ReverseDateForm));
			}
		}

		public void TestYesButton()
		{
			using (ReversalDatesForm form = GetFormToBash() as ReversalDatesForm)
			{
				form.Show();
				form.YesButtonOnYesNoCancelPanel.PerformClick();
				AssertEquals("YesButton", DialogResult.Yes, form.DialogResult);
			}
		}

		public void TestNoButton()
		{
			using (ReversalDatesForm form = GetFormToBash() as ReversalDatesForm)
			{
				form.Show();
				form.NoButtonOnYesNoCancelPanel.PerformClick();
				AssertEquals("NoButton", DialogResult.No, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			using (ReversalDatesForm form = GetFormToBash() as ReversalDatesForm)
			{
				form.Show();
				form.CancelButtonOnYesNoCancelPanel.PerformClick();
				AssertEquals("CancelButton", DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestMakeRequiredFieldsEditableForReversing()
		{
			var registry = AccountingConfigurationRegistry.Instance;
			registry.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var configuration = new BackDateInvoicesConfiguration();
			configuration.OverridePostDate = true;
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			var creator = new TestObjectCreator(Factory);
			creator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				true, true);
			registry.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

			using (ReversalDatesForm form = GetFormToBash() as ReversalDatesForm)
			{
				form.Show();
				var revInv = form.ReversedInvoices[0];
				AssertEquals("AH_InvoiceDateInfo.ReadOnly", false, revInv.AH_InvoiceDateInfo.ReadOnly);
				AssertEquals("AH_PostDateInfo.ReadOnly", false, revInv.AH_PostDateInfo.ReadOnly);
			}
		}

		Form GetFormToBashCoreWithLine(ZDate date)
		{
			var collection = new TransactionHeaderCollection(Factory);
			var creator = new TestObjectCreator(Factory);
			invoice = creator.CreateARInvoice<ARInvoice>("00001001", creator.AUD, 1.0m, creator.ABIGAS);
			invoice.AH_PostDate = new ZDateTime(2021, 03, 01);
			invoice.AH_OriginalInvoiceDate = (new ZDateTime(2021, 06, 01)).Date;
			collection.Add(invoice);
			invoice.GenerateReverseTransaction(false);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AT = creator.GST1.PK;
			line.AL_TaxDate = date;
			line.AL_OSExTaxAmount = 1000;
			line.AL_GovtChargeCode = "random code";
			line.AL_AG = creator.GLHeader1.PK;
			collection.Add(invoice.ReverseInvoice);
			collection.HasChanges = false;
			var shipment = creator.CreateShipment("S00001001", true);
			var codes = new OperationsJobConfigurationCodes(shipment);
			var changeTransactionDatesBusinessObject = new ChangeTransactionDatesBusinessObject(Env.Security.MaintainShipmentJobInvoicing, Factory, codes);
			Factory.Save();
			return new ReversalDatesForm(collection, changeTransactionDatesBusinessObject);
		}

		[TestDate(2021, 06, 01)]
		public void TestErrorIfPostedDateAfterAllowedPeriod()
		{
			var failingPostDate = new ZDateTime(2021, 06, 01);
			var passingPostDate = new ZDateTime(2021, 05, 25);
			var originalInvoiceDate = new ZDateTime(2021, 03, 01);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("IN"))
			{
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;

				var periodManagementTestHelper = new AccountingPeriodTestHelper();
				periodManagementTestHelper.PostPeriodsForEntireYear(2021);
				periodManagementTestHelper.PostPeriodsForEntireYear(2020);
				periodManagementTestHelper.PostPeriodsForEntireYear(2022);

				var registry = AccountingConfigurationRegistry.Instance;
				var creator = new TestObjectCreator(Factory);
				var configuration = new BackDateInvoicesConfiguration();

				configuration.OverridePostDate = true;
				configuration.InvoiceDateConfigurationCollection.RemoveAll();
				creator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
					InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
					InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
					InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
					InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
					InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
					true, true);

				registry.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);
				registry.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				registry.IndiaGSTReversalAllowedPeriod.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);

				AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

				Factory.SetContext(BusinessContext.ReverseDateForm);
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				creator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 1000m, 2000m);

				using (ReversalDatesForm form = GetFormToBashCoreWithLine(originalInvoiceDate.Date) as ReversalDatesForm)
				{
					form.Show();

					var revInv = form.ReversedInvoices[1];
					revInv.AH_OH = creator.Debtor1.PK;
					revInv.AH_PostDate = failingPostDate;
					revInv.Factory.Save();

					AssertEquals("AH_InvoiceDateInfo.ReadOnly", false, revInv.AH_InvoiceDateInfo.ReadOnly);
					AssertEquals("AH_PostDateInfo.ReadOnly", false, revInv.AH_PostDateInfo.ReadOnly);
					AssertEquals("Form should have errors", true, revInv.HasErrors);
					AssertHasError($"Credit Note should have error: " +
						$"{IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV}",
						revInv.AH_PostDateInfo,
						IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);

					revInv.AH_PostDate = passingPostDate;
					AssertEquals("Form should have no errors", false, revInv.HasErrors);
					AssertNoError($"Credit Note should not have error: " +
						$"{IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV}",
						revInv.AH_PostDateInfo,
						IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);
				}
				Factory.RemoveContext(BusinessContext.ReverseDateForm);
			}
		}
	}
}
