using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.ARAP.Invoicing.Testing
{
	public class InvoiceRoundingLineCreatorTest : TestCaseWithFactory
	{
		public void TestInvoiceAmountRounderType()
		{
			var roundingLineCreator = new InvoiceRoundingLineCreator();
			AssertType<InvoiceAmountRounder>(roundingLineCreator.InvoiceAmountRounder_ExposedForTestOnly);
		}

		public void TestTaxHelperType()
		{
			var roundingLineCreator = new InvoiceRoundingLineCreator();
			AssertType<TaxHelper>(roundingLineCreator.TaxHelper_ExposedForTestOnly);
		}

		public void TestAddRoundingLineWithJobInvoice()
		{
			AssertAddRoundingLine(true);
		}

		public void TestAddRoundingLineWithNonJobInvoice()
		{
			AssertAddRoundingLine(false);
		}

		public void TestAddRoundingLine_EmptyTaxID()
		{
			AssertAddRoundingLine(true, false);
		}

		void AssertAddRoundingLine(bool isJobRelated, bool isGSTMandatory = true)
		{
			var invoiceAmountRounder = new Mock<IInvoiceAmountRounder>(MockBehavior.Strict);

			var roundingLineCreator = new InvoiceRoundingLineCreator();
			roundingLineCreator.SubstituteInvoiceAmountRounder_ForTestOnly(invoiceAmountRounder.Object);

			invoiceAmountRounder.Setup(x => x.ApplyCustomRounding(100M, "USD", InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor, It.IsAny<Action<string>>())).Returns(0.37M);

			var taxHelper = new Mock<ITaxHelper>(MockBehavior.Strict);
			taxHelper.Setup(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>())).Returns(isGSTMandatory);
			roundingLineCreator.SubstituteTaxHelper_ForTestOnly(taxHelper.Object);

			SetupRoundingRegistries();
			var testAccounting = new Business.ExternalInterface.Accounting();
			testAccounting.SetMainNotReportableTaxIDConfiguration(GlbCompany.CurrentCompany.PK.ToGuid(), creator.ExtraServiceTax.PK.ToGuid());

			var branch1 = creator.CreateNewBranch(GlbCompany.CurrentCompany, "B01");
			var branch2 = creator.CreateNewBranch(GlbCompany.CurrentCompany, "B02");

			var department1 = creator.CreateDepartment("D01");
			var department2 = creator.CreateDepartment("D02");

			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", creator.USD, 0.6M, 100M, 0M, 166.67M, 0M, creator.LocalClient, creator.CC3.PK);
			invoice.AH_GB = branch1.PK;
			invoice.AH_GE = department1.PK;

			if (isJobRelated)
			{
				var job = creator.CreateJob(creator.LocalClient, 0m, creator.Agent, 0m);
				job.JH_GB = branch2.PK;
				job.JH_GE = department2.PK;

				invoice.AH_JH = job.PK;
			}

			invoice.IgnoreValidationSuspended = true;
			Assert("Precondtion: invoice's IgnoreValidationSuspended is true.", invoice.IgnoreValidationSuspended);

			AssertEquals("invoice has 1 line before apply rounding", 1, invoice.Lines.Count);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(invoice);

			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Once);
			taxHelper.Verify(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>()), Times.Once);

			AssertEquals("invoice has 2 lines after apply rounding", 2, invoice.Lines.Count);
			var newLine = invoice.Lines[1];

			Assert("invoice's IgnoreValidationSuspended is still true after rounding.", invoice.IgnoreValidationSuspended);
			Assert("new line should not have error", !newLine.HasErrors);

			if (isJobRelated)
			{
				AssertEquals(branch2.PK, newLine.AL_GB);
				AssertEquals(department2.PK, newLine.AL_GE);
			}
			else
			{
				AssertEquals(branch1.PK, newLine.AL_GB);
				AssertEquals(department1.PK, newLine.AL_GE);
			}

			AssertEquals(creator.NonAccrualChargeCode.PK, newLine.GenericCharge);
			AssertEquals("USD", newLine.AL_RX_NKTransactionCurrency);
			AssertEquals(0.6M, newLine.AL_ExchangeRate);
			AssertEquals(0.37M, newLine.AL_OSExTaxAmount);
			AssertEquals(100.37M, InvoiceRoundingLineCreator.GetRoundingAmountCachedValue(invoice));

			if (isGSTMandatory)
			{
				AssertEquals(creator.ExtraServiceTax.PK, newLine.AL_AT);
			}
			else
			{
				AssertEquals(ZGuid.Empty, newLine.AL_AT);
			}

			taxHelper.Invocations.Clear();
			invoiceAmountRounder.Reset();
			invoiceAmountRounder.Setup(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>())).Returns(0M);

			invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			AssertEquals("invoice has 1 line before apply rounding", 1, invoice.Lines.Count);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(invoice);

			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Once);
			taxHelper.Verify(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>()), Times.Never);
			AssertEquals("invoice still has 1 line after apply rounding", 1, invoice.Lines.Count);
		}

		public void TestAddRoundingLine_NotApplied()
		{
			var invoiceAmountRounder = new Mock<IInvoiceAmountRounder>(MockBehavior.Strict);

			var roundingLineCreator = new InvoiceRoundingLineCreator();
			roundingLineCreator.SubstituteInvoiceAmountRounder_ForTestOnly(invoiceAmountRounder.Object);

			invoiceAmountRounder.Setup(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>())).Returns(0.01M);

			SetupRoundingRegistries();
			var apInvoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "00000001", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(apInvoice);
			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Never);
			AssertEquals("AP invoice is not applied rounding.", 1, apInvoice.Lines.Count);

			SetupRoundingRegistries();
			var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(arInvoice1);
			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Never);
			AssertEquals("AR invoice1 is not applied rounding because Registry 'InvoiceTotalRoundingChargeCode' is not set.", 1, arInvoice1.Lines.Count);

			SetupRoundingRegistries();
			var arInvoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000002", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new InvoiceTotalRoundingCollection());
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(arInvoice2);
			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Never);
			AssertEquals("AR invoice2 is not applied rounding because Registry 'InvoiceTotalRounding' is not set.", 1, arInvoice2.Lines.Count);

			SetupRoundingRegistries();
			var arInvoice3 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000003", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			Factory.Save();
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(arInvoice3);
			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Never);
			AssertEquals("AR invoice3 is not applied rounding because it's in Database.", 1, arInvoice3.Lines.Count);

			var arInvoice4 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000004", creator.EUR, 1.2M, 100M, 0M, 120M, 0M);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(arInvoice4);
			invoiceAmountRounder.Verify(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()), Times.Never);
			AssertEquals("AR invoice4 is not applied rounding because it's currency doesn't match InvoiceTotalRounding registry setting.", 1, arInvoice3.Lines.Count);
		}

		public void TestAddRoundingLine_ReportIssue()
		{
			var invoiceAmountRounder = new Mock<IInvoiceAmountRounder>(MockBehavior.Strict);

			var roundingLineCreator = new InvoiceRoundingLineCreator();
			roundingLineCreator.SubstituteInvoiceAmountRounder_ForTestOnly(invoiceAmountRounder.Object);

			invoiceAmountRounder.Setup(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>()))
				.Callback((ZDecimal amount, ZString currency, InvoiceRoundingOption roundingOption, InvoiceRoundCurrencyUnit roundCurrencyUnit, Action<string> reportError) => reportError("33")).Returns(0.01M);

			SetupRoundingRegistries();
			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", creator.AUD, 1M, 100.12M, 0M, 100.123M, 0M);
			AssertEquals("invoice has 1 line before apply rounding", 1, invoice.Lines.Count);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(invoice);

			AssertEquals("invoice has 2 lines after apply rounding", 2, invoice.Lines.Count);
			AssertEquals("InvoiceRoundingLineCreator_AddRoundingLine", ErrorReporter.LastKeyReported);
			AssertContains("Ledger: AR, Transaction Type: INV, Total OS Amount: 100.12, Currency: AUD", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			invoiceAmountRounder.Reset();

			invoiceAmountRounder.Setup(x => x.ApplyCustomRounding(It.IsAny<ZDecimal>(), It.IsAny<ZString>(), It.IsAny<InvoiceRoundingOption>(), It.IsAny<InvoiceRoundCurrencyUnit>(), It.IsAny<Action<string>>())).Returns(0.123M);

			SetupRoundingRegistries();
			invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000002", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			AssertEquals("invoice has 1 line before apply rounding", 1, invoice.Lines.Count);
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(invoice);

			AssertEquals("invoice has 2 lines after apply rounding", 2, invoice.Lines.Count);
			AssertEquals("InvoiceRoundingLineCreator_AddRoundingLine", ErrorReporter.LastKeyReported);
			AssertContains("Ledger: AR, Transaction Type: INV, Total OS Amount: 100.12, Currency: AUD", ErrorReporter.LastMessageReported);
			AssertContains("transaction OS total amount 100.12 is not equal to original amount 100 + rounding amount 0.123.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestRoundingAmountCachedValue()
		{
			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", creator.AUD, 1M, 100M, 0M, 100M, 0M);

			AssertEquals(0M, InvoiceRoundingLineCreator.GetRoundingAmountCachedValue(invoice));

			new InvoiceRoundingLineCreator().SetRoundingAmountCachedValue_ForTestOnly(invoice, 101M);
			AssertEquals(101M, InvoiceRoundingLineCreator.GetRoundingAmountCachedValue(invoice));
		}

		public void TestIsRoundingLine()
		{
			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			AssertIsRoundingLine(arInvoice.Lines[0], true);

			var arCrediteNote = creator.CreateInvoiceWithLine(typeof(ARCreditNote), "AR002", creator.AUD, 1M, 200M, 0M, 200M, 0M);
			AssertIsRoundingLine(arCrediteNote.Lines[0], true);

			var apInvoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "AR001", creator.AUD, 1M, 100M, 0M, 100M, 0M);
			AssertIsRoundingLine(apInvoice.Lines[0], false);
		}

		public void TestAddRoundingLine_HasSkipTaxIdAndTaxMessageMappingValidationContext()
		{
			var invoiceAmountRounder = new Mock<IInvoiceAmountRounder>(MockBehavior.Strict);

			var roundingLineCreator = new InvoiceRoundingLineCreator();
			roundingLineCreator.SubstituteInvoiceAmountRounder_ForTestOnly(invoiceAmountRounder.Object);

			invoiceAmountRounder.Setup(x => x.ApplyCustomRounding(100M, "USD", InvoiceRoundingOption.ARU, InvoiceRoundCurrencyUnit.TenMinor, It.IsAny<Action<string>>())).Returns(0.37M);

			var taxHelper = new Mock<ITaxHelper>(MockBehavior.Strict);
			taxHelper.Setup(x => x.IsGSTMandatory(It.IsAny<DependentTransactionLine>())).Returns(false);
			roundingLineCreator.SubstituteTaxHelper_ForTestOnly(taxHelper.Object);

			SetupRoundingRegistries();
			var testAccounting = new Business.ExternalInterface.Accounting();
			testAccounting.SetMainNotReportableTaxIDConfiguration(GlbCompany.CurrentCompany.PK.ToGuid(), creator.ExtraServiceTax.PK.ToGuid());

			var branch1 = creator.CreateNewBranch(GlbCompany.CurrentCompany, "B01");
			var department1 = creator.CreateDepartment("D01");

			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "00000001", creator.USD, 0.6M, 100M, 0M, 166.67M, 0M, creator.LocalClient, creator.CC3.PK);
			invoice.AH_GB = branch1.PK;
			invoice.AH_GE = department1.PK;

			invoice.IgnoreValidationSuspended = true;
			((IInvoiceRoundingLineCreator)roundingLineCreator).AddRoundingLine(invoice);
			var taxMappingHelper = new TaxIdAndTaxMessageMappingHelper();

			AssertEquals(true, invoice.Lines[1].HasContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation));
		}

		void AssertIsRoundingLine(InvoicingLineBase invoiceLine, bool isRoundingLineType)
		{
			AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.NonAccrualChargeCode.PK.ToGuid());
			IInvoiceRoundingLineCreator roundingLineCreator = new InvoiceRoundingLineCreator();

			AssertEquals(false, roundingLineCreator.IsRoundingLine(invoiceLine));

			invoiceLine.AL_JH = ZGuid.NewZGuid();
			AssertEquals(false, roundingLineCreator.IsRoundingLine(invoiceLine));

			invoiceLine.AL_JH = ZGuid.Empty;
			invoiceLine.AL_AC = creator.NonAccrualChargeCode.PK;
			AssertEquals(isRoundingLineType, roundingLineCreator.IsRoundingLine(invoiceLine));

			invoiceLine.AL_AC = creator.CC1.PK;
			AssertEquals(false, roundingLineCreator.IsRoundingLine(invoiceLine));
		}

		void SetupRoundingRegistries()
		{
			var roundingCollection = new InvoiceTotalRoundingCollection();
			var rounding1 = roundingCollection.AddNew();
			rounding1.Currency = creator.AUD.Code;
			rounding1.RoundingOption = "ARU";
			rounding1.RoundToCurrencyUnit = "0.05";

			var rounding2 = roundingCollection.AddNew();
			rounding2.Currency = creator.USD.Code;
			rounding2.RoundingOption = "ARU";
			rounding2.RoundToCurrencyUnit = "0.10";

			AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, roundingCollection);
			AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creator.NonAccrualChargeCode.PK.ToGuid());
		}

		public void TestShouldApplyRounding()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals(false, InvoiceRoundingLineCreator.ShouldApplyRounding(apInvoice));

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			AssertEquals(true, InvoiceRoundingLineCreator.ShouldApplyRounding(arInvoice));

			Factory.Save();
			AssertEquals(false, InvoiceRoundingLineCreator.ShouldApplyRounding(arInvoice));
		}

		#region Implementation

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			creator = new TestObjectCreator(Factory);
		}

		#endregion
	}
}
