using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Testing
{
	public class MalaysiaEInvoicingEligibilityDeciderTest : EInvoicingEligibilityDeciderTestBase
	{
		protected override string CountryCode => CountryCodes.Malaysia;
		protected override string ExpectedAdditionalTraceLog => @"Is Transaction Cancelled: N
Compliance Sub Type (01)
Original Transaction Type: ";

		#region IsEligible

		public void TestARInvoiceIsEligible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(TestDecider.IsTransactionEligible(invoice));
			}
		}

		public void TestARCreditNoteIsEligible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

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
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

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

		public void TestIsNotEligible_WhenIsReversed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var creditNote = TestObjectCreator.CreateARCreditNote("AR001", TestObjectCreator.AALSHI, TestObjectCreator.LocalCurrency, 1m);
				TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000m, GlbCompany.CurrentCompany.LocalCurrency, 1m);
				creditNote.AH_ComplianceSubType = "01";
				creditNote.AH_IsCancelled = true;

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);
				invoice.AH_ComplianceSubType = "01";
				invoice.AH_IsCancelled = true;

				Assert("Should not be eligible when credit note is reversed.", !TestDecider.IsTransactionEligible(creditNote));
				Assert("Should not be eligible when invoice is reversed.", !TestDecider.IsTransactionEligible(invoice));
			}
		}

		public void TestIsNotEligible_WhenIsReversal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);
				invoice.AH_GovernmentAllocatedID = "Test ID";
				Factory.Save();

				var reverseInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				reverseInvoice.AH_ComplianceSubType = "01";
				reverseInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				reverseInvoice.AH_IsCancelled = true;

				Assert("Should not be eligible when transaction is a AR Invoice Reversal.", !TestDecider.IsTransactionEligible(reverseInvoice));

				reverseInvoice.AH_TransactionType = TransactionTypes.CreditNote;

				Assert("Should not be eligible when transaction is a AR Credit Note Reversal.", !TestDecider.IsTransactionEligible(reverseInvoice));
			}
		}

		public void TestARInvoiceIsNotEligible_WhenComplianceSubTypeIsEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
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
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
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

		public void TestARAmendWithInvoiceIsEligible_WhenComplianceSubTypeIsEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
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

		public void TestARCreditNoteIsNotEligible_WhenOriginalReferenceIsEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("AR002", TestObjectCreator.AALSHI, TestObjectCreator.LocalCurrency, 100m);
				creditNote.AH_ComplianceSubType = "02";
				var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000m, GlbCompany.CurrentCompany.LocalCurrency, 1m);
				creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(creditNote));
			}
		}

		public void TestARCreditNoteIsNotEligible_WhenOriginalReferenceIsNotSuccess()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("AR002", TestObjectCreator.AALSHI, TestObjectCreator.LocalCurrency, 100m);
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.AH_ComplianceSubType = "02";
				TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 2000m, GlbCompany.CurrentCompany.LocalCurrency, 1m);

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(creditNote));
			}
		}

		public void TestARAmendWithInvoiceIsNotEligible_WhenOriginalReferenceIsNotSuccess()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "02");
				amendInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				TestObjectCreator.CreateInvoiceLine(amendInvoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				Assert(!TestDecider.IsTransactionEligible(amendInvoice));
			}
		}

		#endregion

		#region TestAmendmentWithInvoiceIsEligible

		public void TestAssertAmendmentWithInvoiceIsEligible_AmendmentReversed()
		{
			AssertAmendmentWithInvoiceIsEligible(isAmendmentReversed: true);
		}

		public void TestAssertAmendmentWithInvoiceIsEligible_AmendmentNotReversed()
		{
			AssertAmendmentWithInvoiceIsEligible(isAmendmentReversed: false);
		}

		void AssertAmendmentWithInvoiceIsEligible(bool isAmendmentReversed = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				var eInvoicingBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");
				invoice.AH_GovernmentAllocatedID = "TestID";
				Factory.Save();

				var amendInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor);
				amendInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				amendInvoice.AH_ComplianceSubType = "02";
				var amendInvoiceline = TestObjectCreator.CreateInvoiceLine(amendInvoice, 110m, invoice.TransactionCurrency, 1m);
				amendInvoiceline.AL_AT = TestObjectCreator.ExtraServiceTax.PK;

				var amendCredit = TestObjectCreator.CreateARCreditNote("AR003", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, "desc");
				amendCredit.AH_TransactionBelongsToGroup = invoice.PK;
				amendCredit.AH_ComplianceSubType = "03";
				var amendCreditline = TestObjectCreator.CreateARCreditNoteLine(amendCredit, null, TestObjectCreator.FRT, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");
				amendCreditline.AL_AT = TestObjectCreator.ExtraServiceTax.PK;

				if (isAmendmentReversed)
				{
					amendInvoice.GenerateReverseTransaction(false);
					amendInvoice.AH_IsCancelled = true;
					var matchLink1 = ((IMatching)amendInvoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
					matchLink1.AP_AH = amendInvoice.PK;
					TestObjectCreator.SetupMatchLinkMatchDate(matchLink1);

					amendCredit.GenerateReverseTransaction(false);
					amendCredit.AH_IsCancelled = true;
					var matchLink2 = ((IMatching)amendCredit).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
					matchLink2.AP_AH = amendCredit.PK;
					TestObjectCreator.SetupMatchLinkMatchDate(matchLink2);

					Factory.Save();
				}

				Factory.Save();

				if (isAmendmentReversed)
				{
					Assert("Should not be eligible when transaction is a reversed AR Invoice Amendment.", !TestDecider.IsTransactionEligible(amendInvoice));
					Assert("Should not be eligible when transaction is a reversed AR Credit Amendment.", !TestDecider.IsTransactionEligible(amendCredit));
				}
				else
				{
					Assert("Should be eligible when transaction is a not reversed AR Invoice Amendment.", TestDecider.IsTransactionEligible(amendInvoice));
					Assert("Should be eligible when transaction is a not reversed AR Credit Amendment.", TestDecider.IsTransactionEligible(amendCredit));
				}
			}
		}

		IEInvoicingEligibilityDecider TestDecider => decider ?? (decider = GetEligibilityDecider());
		IEInvoicingEligibilityDecider decider;

		#endregion

		#region AP IsTransactionEligible

		public void TestIsTransactionEligible_APInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				invoice.AH_ComplianceSubType = "tes";

				Assert(TestDecider.IsTransactionEligible(invoice));

				invoice.AH_IsCancelled = true;

				Assert(!TestDecider.IsTransactionEligible(invoice));

				invoice.AH_IsCancelled = false;
				invoice.AH_ComplianceSubType = string.Empty;

				Assert(!TestDecider.IsTransactionEligible(invoice));
			}
		}

		public void TestIsTransactionEligible_APCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				var apInvLine = (APInvoiceLine)invoice.Lines.AddNew();
				apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				invoice.AH_ComplianceSubType = "tes";
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				Factory.Save();

				var creditNote = TestObjectCreator.CreateAPCreditNote("", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1, "");
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.AH_ComplianceSubType = "02";

				Assert(TestDecider.IsTransactionEligible(creditNote));

				creditNote.AH_TransactionBelongsToGroup = ZGuid.Empty;
				Assert(TestDecider.IsTransactionEligible(creditNote));

				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.AH_IsCancelled = true;

				Assert(!TestDecider.IsTransactionEligible(creditNote));

				creditNote.AH_IsCancelled = false;
				creditNote.AH_ComplianceSubType = string.Empty;

				Assert(!TestDecider.IsTransactionEligible(creditNote));

				creditNote.AH_ComplianceSubType = "tes";
				AssertOriginalReferenceIsNotSuccess(pivot, creditNote);
			}
		}

		public void TestIsTransactionEligible_APAmendWithInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				var apInvLine = (APInvoiceLine)invoice.Lines.AddNew();
				apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoice.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				invoice.AH_ComplianceSubType = "tes";
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");

				var amendInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);
				amendInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				amendInvoice.AH_ComplianceSubType = "tes";

				Assert(TestDecider.IsTransactionEligible(amendInvoice));

				amendInvoice.AH_IsCancelled = true;

				Assert(!TestDecider.IsTransactionEligible(amendInvoice));

				amendInvoice.AH_IsCancelled = false;
				amendInvoice.AH_ComplianceSubType = string.Empty;

				Assert(!TestDecider.IsTransactionEligible(amendInvoice));

				amendInvoice.AH_ComplianceSubType = "tes";
				AssertOriginalReferenceIsNotSuccess(pivot, amendInvoice);
			}
		}

		void AssertOriginalReferenceIsNotSuccess(AccEInvoicingTransactionPivot pivot, IEInvoicingEligibilityLiteTransaction transaction)
		{
			var statusList = new List<ZString> {
				EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Sent, EInvoicingPivotState.Delivered,
				EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded, EInvoicingPivotState.Pending, EInvoicingPivotState.AwaitingReview, EInvoicingPivotState.InProcessing
			};

			foreach (var status in statusList)
			{
				pivot.AIP_Status = status;
				Assert(!TestDecider.IsTransactionEligible(transaction));
			}
		}

		#endregion

		protected override FakeEligibilityLiteTransaction CreateEligibleTransaction() =>
			new FakeEligibilityLiteTransaction
			{
				CountryCode = CountryCodes.Malaysia,
				Ledger = LedgerTypes.AccountsReceivable,
				TransactionType = TransactionTypes.Invoice,
				GovernmentAllocatedID = ZString.Empty,
				Lines = new FakeEligibilityLiteTransactionLine[] { new FakeEligibilityLiteTransactionLine() { TaxType = AccTaxRate.Types.Rated } },
				ComplianceSubType = "01",
			};

		TestObjectCreator TestObjectCreator => testObjectCreator = testObjectCreator ?? new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
