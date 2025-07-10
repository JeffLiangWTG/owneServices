using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class CountrySpecificValidationHelperTest : TestCaseWithFactory
	{
		public void TestShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;
			line.TransactionHeader.AH_TransactionType = TransactionTypes.AdjustmentNote;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var (shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				AssertEquals("shouldAddErrorOrWarning should be false when not entry in Portugal", false, shouldAddErrorOrWarning);
				AssertEquals("addError should be false when not entry in Portugal", false, addError);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				var (shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				AssertEquals("shouldAddErrorOrWarning should be false when entry in Portugal and ledger type is AP", false, shouldAddErrorOrWarning);
				AssertEquals("addError should be false when entry in Portugal and ledger type is AP", false, addError);

				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				(shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				AssertEquals("shouldAddErrorOrWarning should be false when entry in Portugal and ledger type is AR but transaction type is ADJ", false, shouldAddErrorOrWarning);
				AssertEquals("addError should be false when entry in Portugal and ledger type is AR but transaction type is ADJ", false, addError);

				line.TransactionHeader.AH_TransactionType = TransactionTypes.Invoice;
				(shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				AssertEquals("shouldAddErrorOrWarning should be true when entry in Portugal and ledger type is AR and transaction type is INV or CRD but is outside the expected value for the selected tax rate", true, shouldAddErrorOrWarning);
				AssertEquals("addError should be true when entry in Portugal and ledger type is AR and transaction type is INV or CRD but is outside the expected value for the selected tax rate", true, addError);

				using (AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					(shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
					AssertEquals("shouldAddErrorOrWarning should be false because IsHeaderTaxAmountCalculationSuspended is true", false, shouldAddErrorOrWarning);
					AssertEquals("addError should be false because IsHeaderTaxAmountCalculationSuspended is true", false, addError);
				}

				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.LCD;
				(shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				AssertEquals("shouldAddErrorOrWarning should be true when entry in Portugal and ledger type is AR but is outside the expected value for the selected tax rate", true, shouldAddErrorOrWarning);
				AssertEquals("addError should be false when entry in Portugal and ledger type is AR but is outside the expected value for the selected tax rate", false, addError);

				line.AL_OSTaxAmount = 1m;
				(shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				AssertEquals("shouldAddErrorOrWarning should be false when entry in Portugal and ledger type is AR and is the expected value for the selected tax rate", false, shouldAddErrorOrWarning);
				AssertEquals("addError should be false when entry in Portugal and ledger type is AR but is outside the expected value for the selected tax rate", false, addError);
			}
		}

		public void TestShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount_NotSupportedComplianceSubTypes()
		{
			var invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var (shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);
				Assert(shouldAddErrorOrWarning);
				Assert(addError);

				var notSupportedComplianceSubTypes = new string[]
				{
					PortugalComplianceInfo.ComplianceSubTypeCodes.CBC,
					PortugalComplianceInfo.ComplianceSubTypeCodes.CBD,
					PortugalComplianceInfo.ComplianceSubTypeCodes.CBI,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LCD,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LCR,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LTX,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TCM,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TDM,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TXM,
				};

				foreach (var notSupportedComplianceSubType in notSupportedComplianceSubTypes)
				{
					invoice.AH_ComplianceSubType = notSupportedComplianceSubType;
					(shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(line);

					Assert(shouldAddErrorOrWarning);
					Assert($"addError should be false for compliance sub type {notSupportedComplianceSubType}", !addError);
				}
			}
		}

		public void TestShouldValidateAL_OSTaxAmount()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.TransactionHeader.AH_TransactionType = TransactionTypes.AdjustmentNote;
			invoice.AH_IsCancelled = true;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("It should be false because current company is not Portugal", false, CountrySpecificValidationHelper.ShouldValidateAL_OSTaxAmount(line));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals("It should be false because AH_Ledger is AP", false, CountrySpecificValidationHelper.ShouldValidateAL_OSTaxAmount(line));

				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				AssertEquals("It should be false because transaction type is ADJ", false, CountrySpecificValidationHelper.ShouldValidateAL_OSTaxAmount(line));

				line.TransactionHeader.AH_TransactionType = TransactionTypes.Invoice;
				AssertEquals("It should be false because validation is not InvoicingLineBaseValidation", false, CountrySpecificValidationHelper.ShouldValidateAL_OSTaxAmount(line));

				invoice.AH_IsCancelled = false;
				AssertEquals("It should be true because validation is InvoicingLineBaseValidation", true, CountrySpecificValidationHelper.ShouldValidateAL_OSTaxAmount(line));
			}
		}

		[TestDate(2018, 07, 07)]
		public void TestAddWarningIfDateIsInTheFuture()
		{
			var countries = new string[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Portugal, Core.Constants.CountryCodes.Japan };

			foreach (var country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					var warningMsg = @"Invoice Date is in the future. Please check the Invoice Date against the current system date and time. If this invoice is posted, future invoices cannot use current or previous date.";
					using (invoice.SuspendValidationTesting())
					{
						invoice.AH_InvoiceDate = new ZDateTime(2018, 07, 07);
						CountrySpecificValidationHelper.AddWarningIfDateIsInTheFuture(invoice.AH_InvoiceDateInfo);
						AssertNoWarning(invoice.AH_InvoiceDateInfo, warningMsg);
						invoice.AH_InvoiceDate = new ZDateTime(2018, 07, 08);
						CountrySpecificValidationHelper.AddWarningIfDateIsInTheFuture(invoice.AH_InvoiceDateInfo);
						if (country == Enterprise.Core.Constants.CountryCodes.Portugal)
						{
							AssertHasWarning(invoice.AH_InvoiceDateInfo, warningMsg);
						}
						else
						{
							AssertNoWarning(invoice.AH_InvoiceDateInfo, warningMsg);
						}
					}
				}
			}
		}

		public void TestAddErrorOrWarningIfNoTaxMessage()
		{
			var countries = new string[] { Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Portugal, Core.Constants.CountryCodes.Japan };
			var org = TestObjectCreator.AALSHI;
			var msg1 = TestObjectCreator.CreateTaxMsg("MSG1", "MSG1", "english msg1", "local msg1");
			var msgTaxRequired = "Tax Message is Required.";
			var msgTaxRequiredIfZero = "Tax Amount is zero, please enter a Tax Message.";
			var msgTaxMessageRequired = "Please enter a valid Tax Message when using this Tax ID.";

			foreach (var country in countries)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-30)))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_OH = org.PK;
					var linePosted = (InvoicingLineBase)invoice.Lines.AddNew();
					using (linePosted.SuspendValidationTesting())
					{
						AssertEquals(0m, linePosted.AL_OSTaxAmount);
						linePosted.AL_A9_VATClass = msg1.PK;
						CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, TestObjectCreator.GST1.PK, () => 0m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
						AssertNoWarning(linePosted.AL_A9_VATClassInfo, msgTaxRequiredIfZero);
						linePosted.AL_A9_VATClass = ZGuid.Empty;
						CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, ZGuid.Empty, () => 0m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
						AssertNoWarning(linePosted.AL_A9_VATClassInfo, msgTaxRequiredIfZero);
						CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, TestObjectCreator.GST1.PK, () => 0m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
						AssertNoWarning(linePosted.AL_A9_VATClassInfo, msgTaxRequiredIfZero);

						using (AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredAlways))
						{
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, TestObjectCreator.GST1.PK, () => 0m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
							AssertHasError(linePosted.AL_A9_VATClassInfo, msgTaxRequired);
						}

						using (AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero))
						{
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, TestObjectCreator.GST1.PK, () => 100m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
							AssertHasError(linePosted.AL_A9_VATClassInfo, msgTaxRequiredIfZero);
						}

						using (AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero))
						{
							linePosted.AL_AT = TestObjectCreator.KDV18W5.PK;
							linePosted.AL_OSTaxAmount = 100m;
							linePosted.AL_A9_VATClass = ZGuid.Empty;
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, linePosted.AL_AT, () => linePosted.AL_OSExTaxAmount, () => linePosted.AL_OSTaxAmount, () => linePosted.AL_OSExtraTaxAmount, linePosted.IsAP(), linePosted.IsAR());
							AssertHasError(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);

							linePosted.AL_A9_VATClass = msg1.PK;
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, linePosted.AL_AT, () => linePosted.AL_OSExTaxAmount, () => linePosted.AL_OSTaxAmount, () => linePosted.AL_OSExtraTaxAmount, linePosted.IsAP(), linePosted.IsAR());
							AssertNoWarning(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);
							AssertNoError(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);
						}

						using (AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax))
						{
							linePosted.AL_AT = TestObjectCreator.FREEVAT.PK;
							linePosted.AL_OSExTaxAmount = 100m;
							linePosted.AL_A9_VATClass = ZGuid.Empty;
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, linePosted.AL_AT, () => 100m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
							AssertHasError(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);

							linePosted.AL_A9_VATClass = msg1.PK;
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, linePosted.AL_AT, () => 100m, () => 0m, () => 0m, linePosted.IsAP(), linePosted.IsAR());
							AssertNoWarning(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);
							AssertNoError(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);

							linePosted.AL_AT = TestObjectCreator.KDV18W5.PK;
							linePosted.AL_OSExTaxAmount = 100m;
							linePosted.AL_A9_VATClass = ZGuid.Empty;
							AssertEquals(-9m, linePosted.AL_OSExtraTaxAmount);
							AssertEquals(9m, linePosted.AL_OSTaxAmount);
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, linePosted.AL_AT, () => linePosted.AL_OSExTaxAmount, () => linePosted.AL_OSTaxAmount, () => linePosted.AL_OSExtraTaxAmount, linePosted.IsAP(), linePosted.IsAR());
							AssertHasError(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);

							linePosted.AL_A9_VATClass = msg1.PK;
							AssertEquals(9m, linePosted.AL_OSTaxAmount);
							AssertEquals(-9m, linePosted.AL_OSExtraTaxAmount);
							AssertEquals(100m, linePosted.AL_OSExTaxAmount);
							CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(linePosted.AL_A9_VATClassInfo, linePosted.AL_AT, () => linePosted.AL_OSExTaxAmount, () => linePosted.AL_OSTaxAmount, () => linePosted.AL_OSExtraTaxAmount, linePosted.IsAP(), linePosted.IsAR());
							AssertNoWarning(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);
							AssertNoError(linePosted.AL_A9_VATClassInfo, msgTaxMessageRequired);
						}
					}
				}
			}
		}

		[TestDate(2018, 07, 07)]
		public void TestGetTransactionsWithWarningAboutInvoiceDateInTheFuture()
		{
			var transactions = new TransactionCreatorHashtable();
			var aPInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice1.AH_InvoiceDate = new ZDateTime(2018, 07, 10);

			var aPInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice2.AH_InvoiceDate = new ZDateTime(2018, 07, 07);

			transactions.AddAPInvoice(aPInvoice1, TestObjectCreator.AALSHI.OH_Code, aPInvoice1.AH_TransactionNum);
			transactions.AddAPInvoice(aPInvoice2, TestObjectCreator.AALSHI.OH_Code, aPInvoice2.AH_TransactionNum);
			aPInvoice1.Lines.RemoveAndDeleteAll();
			aPInvoice2.Lines.RemoveAndDeleteAll();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals(1, CountrySpecificValidationHelper.GetTransactionsWithWarningAboutInvoiceDateInTheFuture(transactions, ZDateTime.Today).Count());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(0, CountrySpecificValidationHelper.GetTransactionsWithWarningAboutInvoiceDateInTheFuture(transactions, ZDateTime.Today).Count());
			}
		}

		public void TestNeedToCheckCompanyAndOrgsRegistrationNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Assert(CountrySpecificValidationHelper.NeedToCheckCompanyAndOrgsRegistrationNumber());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Assert(!CountrySpecificValidationHelper.NeedToCheckCompanyAndOrgsRegistrationNumber());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(AutoAccPeriodManagement.Schema.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected TestObjectCreator TestObjectCreator;
	}
}
