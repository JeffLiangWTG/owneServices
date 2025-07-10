using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Countries.Jordan
{
	public class JordanEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		#region IsEligible

		public void TestARInvoiceIsEligible()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(TestDecider.IsTransactionEligible(invoice));
			}
		}

		public void TestARCreditNoteIsEligible()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = Constants.EInvoicingPivotState.Succeed;
				_ = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("AR002", TestObjectCreator.AALSHI, TestObjectCreator.LocalCurrency, 100m);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.AH_ComplianceSubType = "02";
				var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000m, GlbCompany.CurrentCompany.LocalCurrency, 1m);
				creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;

				Factory.Save();

				Assert(TestDecider.IsTransactionEligible(creditNote));
			}
		}

		public void TestARAmendWithInvoiceIsEligible()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = Constants.EInvoicingPivotState.Succeed;
				_ = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "02");
				amendInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				TestObjectCreator.CreateInvoiceLine(amendInvoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(TestDecider.IsTransactionEligible(amendInvoice));
			}
		}

		#endregion

		#region IsNotEligible

		public void TestARInvoiceIsNotEligible_WhenComplianceSubTypeIsEmpty()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleConfigurationCollection()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(invoice));
			}
		}

		public void TestARCreditNoteIsNotEligible_WhenComplianceSubTypeIsEmpty()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleConfigurationCollection()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("AR002", TestObjectCreator.AALSHI, TestObjectCreator.LocalCurrency, 100m);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000m, GlbCompany.CurrentCompany.LocalCurrency, 1m);

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(creditNote));
			}
		}

		public void TestARAmendWithInvoiceIsNotEligible_WhenComplianceSubTypeIsEmpty()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleConfigurationCollection()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				amendInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				TestObjectCreator.CreateInvoiceLine(amendInvoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(amendInvoice));
			}
		}

		public void TestAPInvoiceIsNotEligible()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(invoice));
			}
		}

		#endregion

		#region Implementation

		protected override string CountryCode => Constants.CountryCodes.Jordan;

		protected override string ExpectedAdditionalTraceLog => "Have Compliance Sub Type: True";

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = Constants.CountryCodes.Jordan,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				GovernmentAllocatedID = ZString.Empty,
				Lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated } },
				ComplianceSubType = "TXI",
			};

		#endregion

		IEInvoicingEligibilityDecider TestDecider => decider ?? (decider = GetEligibilityDecider());
		IEInvoicingEligibilityDecider decider;

		TestObjectCreator TestObjectCreator => testObjectCreator = testObjectCreator ?? new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
