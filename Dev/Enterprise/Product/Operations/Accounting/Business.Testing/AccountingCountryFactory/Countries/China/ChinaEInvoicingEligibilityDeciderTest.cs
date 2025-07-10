using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China.Testing
{
	public class ChinaEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.China;
		protected override string ExpectedAdditionalTraceLog => @"Is Cancelled: N
Invoice Amount: 100
Compliance Sub Type (ETA) is valid: True
ChinaEInvoicingCredentials Registry: 0001

Number of Transaction Lines: 0
Registry Excluded Charges: 0 (Do Not Queue Invoices Containing Specific Charges For Transmission)
Has Excluded Transaction Lines from Registry in Invoice: False";

		[SuspendCriticalValidation]
		public void TestCancelledInvoiceShouldNotBeQueued()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var creator = new TestObjectCreator(Factory);
				var creditNote = creator.CreateARCreditNoteWithLine("0002", creator.ABIGAS, creator.AUD, 1m, "Desc", null, creator.CC1, 100m, ZDateTime.Today, false);
				Factory.Save();

				var reversing = creditNote as IReversing;
				reversing.GenerateReverseTransaction(true);
				reversing.SetCancellationFlag(true);

				var invoice = reversing.ReverseTransaction as ARInvoice;
				AssertNotNull("Reversed transaction should be an AR invoice.", invoice);
				invoice.SetCancellationFlag(true);
				invoice.AH_ComplianceSubType = "ETA";

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				AssertNull("Should not create pivot for reversed AR invoice.", pivot);
			}
		}

		public void TestEligibleTransactionHeader()
		{
			var nonCurrentBranchPK = new TestObjectCreator(Factory).NonCurrentBranch.PK;
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001");
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_GC = GlbCompany.CurrentCompany.PK;
			charge.AC_ChargeType = "MRG";
			charge.AC_Code = "NAB";
			var charge2 = Factory.NewWithValidTestData<AccChargeCode>();
			charge2.AC_GC = GlbCompany.CurrentCompany.PK;
			charge2.AC_Code = "NA2";
			charge2.AC_ChargeType = "MRG";
			Factory.Save();
			var settings = new GenericChargeConfigurationCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var setting = settings.AddNew();
			setting.ChargePK = charge.PK;

			AccountingConfigurationRegistry.Instance.DoNotQueueInvoicesContainingSpecificChargesForTransmission.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, settings);

			var testConfigurations = new[]
			{
				new { Expected = true, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = true, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = true, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = true, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = true, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },

				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = string.Empty, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = string.Empty, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = string.Empty, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, BranchPK = nonCurrentBranchPK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB, BranchPK = nonCurrentBranchPK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, BranchPK = nonCurrentBranchPK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = nonCurrentBranchPK, Amount = 100M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 0M, ChargePK = charge2.PK },
				new { Expected = false, Country = CountryCodes.China, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, BranchPK = GlbBranch.CurrentBranch.PK, Amount = 100M, ChargePK = charge.PK },
			};

			var eligibilityDecider = GetEligibilityDecider();
			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = eligibilityDecider.IsTransactionEligible(new FakeEligibilityLiteTransaction
				{
					CompanyPK = GlbCompany.CurrentCompany.PK,
					BranchPK = testConfiguration.BranchPK,
					Ledger = testConfiguration.LedgerType,
					TransactionType = testConfiguration.TransactionType,
					CountryCode = testConfiguration.Country,
					ComplianceSubType = testConfiguration.ComplianceSubType,
					InvoiceAmount = testConfiguration.Amount,
					Lines = new IEInvoicingEligibilityLiteTransactionLine[]
					{
						new FakeEligibilityLiteTransactionLine()
						{
							ChargePK = testConfiguration.ChargePK
						}
					}
				});
				AssertEquals($"REC#{index} {nameof(CountryCodes.China)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.Country} {testConfiguration.ComplianceSubType}", testConfiguration.Expected, actual);
			}
		}

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction()
		{
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001");

			return new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.China,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA,
				CompanyPK = GlbCompany.CurrentCompany.PK,
				BranchPK = GlbBranch.CurrentBranch.PK,
				InvoiceAmount = 100m,
			};
		}
	}
}
