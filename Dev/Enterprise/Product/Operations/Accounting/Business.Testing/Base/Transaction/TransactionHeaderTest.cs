using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;
using AuthRecordConstants = Enterprise.Accounting.Integration.DataTransferConstants.AccTransactionHeaderAuthorisationRecord;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public abstract class TransactionHeaderTest : AccTransactionHeaderBusinessObjectTest, IReversingTest
	{
		#region TestIComplianceNumberResetStatusInputDataImplementation

		public void TestIComplianceNumberResetStatusInputDataImplementation_CompanyPK()
		{
			var expectedCompanyPK = ZGuid.NewZGuid();
			Header.AH_GC = expectedCompanyPK;
			AssertEquals(expectedCompanyPK, ((IComplianceNumberResetStatusInputData)Header).CompanyPK);
		}

		public void TestIComplianceNumberResetStatusInputDataImplementation_CountryCode()
		{
			var expectedCountryCode = "XX";
			Header.Company.GC_RN_NKCountryCode = expectedCountryCode;
			AssertEquals(expectedCountryCode, ((IComplianceNumberResetStatusInputData)Header).CountryCode);
		}

		public void TestIComplianceNumberResetStatusInputDataImplementation_InvoiceDate()
		{
			var expectedDate = ZDate.Today.AddMonths(-7);
			Header.AH_InvoiceDate = expectedDate.ToZDateTime().AddHours(23).AddMinutes(59).AddSeconds(59);
			AssertEquals(expectedDate, ((IComplianceNumberResetStatusInputData)Header).InvoiceDate);
		}

		public void TestIComplianceNumberResetStatusInputDataImplementation_EInvoicingStatus()
		{
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(Header);

			var expectedStatus = "ANY";
			pivot.AIP_Status = expectedStatus;
			AssertEquals(expectedStatus, ((IComplianceNumberResetStatusInputData)Header).EInvoicingStatus);
		}

		#endregion

		public virtual void TestIsPaymentApprovalItemNotPostedAndNotCurrentlyMatched()
		{
			SetupForSave();

			if (!(Header is IMatching))
			{
				Assert("Not Applicable.", true);
				return;
			}

			Header.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			Header.AH_OH = TestObjectCreator.ABIGAS.PK;
			Header.AH_TransactionNum = "Test001";

			var matched = new IMatchingCollection(Factory);
			matched.Add(Header);

			var approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_OH = TestObjectCreator.ABIGAS.PK;
			approval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			var approvalItem = Header.GetPaymentApprovalItem(approval);
			approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			Factory.Save();

			foreach (var status in typeof(PaymentApprovalStatus).GetConstantValues())
			{
				approval.AV_Status = status;
				if (status != PaymentApprovalStatus.Posted && status != PaymentApprovalStatus.Rejected && status != PaymentApprovalStatus.Cancelled)
				{
					AssertEquals($"Should be true when status is not Cancelled, Rejected nor Posted, AV_Status={status}.", true, Header.IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(approvalItem));
				}
				else
				{
					AssertEquals($"Should be false when status is Cancelled, Rejected or Posted, AV_Status={status}.", false, Header.IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(approvalItem));
				}
			}

			var currentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			currentApproval.AV_OH = TestObjectCreator.ABIGAS.PK;
			var currentApprovalItem = Header.GetPaymentApprovalItem(currentApproval);
			currentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			Header.PaymentApprovalItemsField_ForTestOnly = null;
			AssertEquals("ExistingPaymentApprovalItems", 2, Header.ExistingPaymentApprovalItems.Count);

			matched.Add(currentApproval);

			AssertEquals("Should be PaymentApprovalPKCurrentlyBeingMatched", currentApproval.PK, Header.PaymentApprovalPKCurrentlyBeingMatched_ForTestOnly);
			AssertEquals("Should be false because item is currently being matched.", false, Header.IsPaymentApprovalItemNotPostedAndNotCurrentlyMatched(currentApprovalItem));
		}

		public void TestRelatedFieldsCountOnLocalTaxAmountOtherTaxes()
		{
			var expectedValue = 100.12m * Header.Multiplier_ForTestOnly;
			Header.AH_LocalTaxAmountOtherTaxes = expectedValue;
			AssertEquals("Precondition: AH_LocalTaxAmountOtherTaxes", expectedValue, Header.AH_LocalTaxAmountOtherTaxes);
			AssertEquals("AH_LocalTaxAmountOtherTaxes_ForDisplay", Math.Abs(expectedValue), Header.AH_LocalTaxAmountOtherTaxes_ForDisplay);
			AssertEquals("AH_LocalTotal", expectedValue, Header.AH_LocalTotal);
			AssertEquals("AH_LocalTotalAmount", Math.Abs(expectedValue), Header.AH_LocalTotalAmount);
			Header.AH_LocalExTaxAmount = 0m;
			AssertEquals("AH_OutstandingAmount", GetExpectedOutstandindAmount(expectedValue), Header.AH_OutstandingAmount);
			AssertEquals("PaymentStatus", Header.AH_OutstandingAmount != 0 ? "UNPAID" : "PAID", Header.PaymentStatus);
			if (Header is IMatching matching)
			{
				AssertEquals("CanUnmatch", UnmatchingResult.Success, matching.CanUnmatch(0));
				Assert("IsMatched", !matching.IsMatched);
			}
		}

		public void TestMatchingMonitor()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var arJournal = Factory.NewWithValidTestData<ARJournal>();
			var apJournal = Factory.NewWithValidTestData<APJournal>();

			Assert("JournalMatchingMonitor is created only if transaction header is AR/AP Journal", !(invoice.MatchingMonitor is JournalMatchingMonitor) && invoice.MatchingMonitor is TransactionHeaderMatchingMonitor);
			Assert("JournalMatchingMonitor is created only if transaction header is AR/AP Journal", arJournal.MatchingMonitor is JournalMatchingMonitor);
			Assert("JournalMatchingMonitor is created only if transaction header is AR/AP Journal", apJournal.MatchingMonitor is JournalMatchingMonitor);
		}

		public void TestRelatedFieldsCountOnOSAndLocalTaxAmountOtherTaxes()
		{
			AssertRelatedFieldsCountOnOSAndLocalTaxAmountOtherTaxes(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestRelatedFieldsCountOnOSAndLocalTaxAmountOtherTaxes_EnableNewOSOutstandingAmountFeature()
		{
			AssertRelatedFieldsCountOnOSAndLocalTaxAmountOtherTaxes(isEnableNewOSOutstandingAmountFeature: true);
		}

		protected virtual void AssertRelatedFieldsCountOnOSAndLocalTaxAmountOtherTaxes(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			var header = PrepareTransactionHeaderForTest() as TransactionHeader;

			var expectedLocalTax = 100.12m * header.Multiplier_ForTestOnly;
			header.AH_LocalTaxAmountOtherTaxes = expectedLocalTax;
			var expectedOSTax = 150m * header.Multiplier_ForTestOnly;
			header.AH_OSTaxAmountOtherTaxes = expectedOSTax;
			header.AH_OutstandingAmount = 50 * header.Multiplier_ForTestOnly;
			AssertEquals("MatchedAmount", 0m, header.MatchedAmount);
			AssertEquals("AH_OSOutstandingAmountWithoutMultiplier", 74.91m * header.Multiplier_ForTestOnly, header.AH_OSOutstandingAmountWithoutMultiplier);

			var matchLink = Factory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = expectedLocalTax;
			matchLink.AP_OSAmount = header.AH_IsOSOutstandingAmountApplicable
				? expectedOSTax + 1m
				: 0m;
			header.SetMatchedAmount(matchLink, true);

			AssertEquals("LocalMatchedAmount", expectedLocalTax, header.LocalMatchedAmount);
			AssertEquals("MatchedAmount", header.AH_IsOSOutstandingAmountApplicable ? expectedOSTax + 1m : expectedOSTax, header.MatchedAmount);

			header.AH_OutstandingAmount = expectedLocalTax;
			AssertEquals("AH_OSOutstandingAmountWithoutMultiplier", expectedOSTax, header.AH_OSOutstandingAmountWithoutMultiplier);
			AssertEquals("GetHighPrecisionExchangeRate", 1.498202157m, header.GetHighPrecisionExchangeRate());
		}

		public void TestRelatedFieldsCountOnOSTaxAmountOtherTaxes()
		{
			var expectedValue = 100.12m * Header.Multiplier_ForTestOnly;
			Header.AH_LocalTaxAmountOtherTaxes = 1 * Header.Multiplier_ForTestOnly;
			Header.AH_OSTaxAmountOtherTaxes = expectedValue;
			AssertEquals("Precondition: AH_OSTaxAmountOtherTaxes", expectedValue, Header.AH_OSTaxAmountOtherTaxes);
			AssertEquals("AH_OSTaxAmountOtherTaxes_ForDisplay", Math.Abs(expectedValue), Header.AH_OSTaxAmountOtherTaxes_ForDisplay);
			AssertEquals("AH_OSTotal", expectedValue, Header.AH_OSTotal);
			AssertEquals("AH_OSTotalAmount", Math.Abs(expectedValue), Header.AH_OSTotalAmount);
			Header.AH_OSExTaxAmount = 0;
			AssertEquals("AH_Calc_OSOutstandingAmount", GetExpectedOutstandindAmount(Math.Abs(expectedValue)), Header.AH_Calc_OSOutstandingAmount);
			Header.AH_OSTotal = expectedValue + 10m * Header.Multiplier_ForTestOnly;
			Header.OnLoaded();
			AssertEquals("AH_OSExTaxAmount", 10m, Header.AH_OSExTaxAmount);
			Header.AH_OSExTaxAmount = 20;
			AssertEquals("AH_OSTotalAmount", Math.Abs(expectedValue) + 20, Header.AH_OSTotalAmount);
			Header.AH_OSTaxAmount = 5;
			AssertEquals("AH_OSTotalAmount", Math.Abs(expectedValue) + 25, Header.AH_OSTotalAmount);
		}

		#region CreateTransactionHeaderReferenceIVA

		public void TestTransactionHeaderReferenceIVA_ExistPortugalIVA_PortugalOrg()
		{
			AssertTransactionHeaderReferenceIVA(true, true, "PT123", false);
		}

		public void TestTransactionHeaderReferenceIVA_NoPortugalIVA_NotPortugalOrg()
		{
			AssertTransactionHeaderReferenceIVA(true, false, string.Empty, false);
		}

		public void TestTransactionHeaderReferenceIVA_ExistPortugalIVA_NotPortugalOrg()
		{
			AssertTransactionHeaderReferenceIVA(true, false, "NZ123", false);
		}

		public void TestTransactionHeaderReferenceIVA_NoPortugalOrg_PortugalIVA_999999990_NoPortugalCompany()
		{
			AssertTransactionHeaderReferenceIVA(false, false, "999999990", false);
		}

		public void TestTransactionHeaderReferenceIVA_PortugalOrg_PortugalIVA_PT999999990_NoPortugalCompany()
		{
			AssertTransactionHeaderReferenceIVA(false, true, "PT999999990", false);
		}

		public void TestTransactionHeaderReferenceIVA_PortugalOrg_NoPortugalIVA()
		{
			AssertTransactionHeaderReferenceIVA(true, true, string.Empty, true);
		}

		public void TestTransactionHeaderReferenceIVA_PortugalOrg_PortugalIVA_999999990()
		{
			AssertTransactionHeaderReferenceIVA(true, true, "999999990", true);
		}

		public void TestTransactionHeaderReferenceIVA_PortugalOrg_PortugalIVA_PT999999990()
		{
			AssertTransactionHeaderReferenceIVA(true, true, "PT999999990", true);
		}

		public void TestTransactionHeaderReferenceIVA_NoPortugalOrg_PortugalIVA_999999990()
		{
			AssertTransactionHeaderReferenceIVA(true, false, "999999990", true);
		}

		public void TestTransactionHeaderReferenceIVA_NoPortugalOrg_PortugalIVA_PT999999990()
		{
			AssertTransactionHeaderReferenceIVA(true, false, "PT999999990", true);
		}

		void AssertTransactionHeaderReferenceIVA(bool isCompanyFromPortugal,
			bool isOrgHeaderFromPortugal,
			string customCodePtIva,
			bool shouldCreatedReferenceIVA)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(isCompanyFromPortugal ? CountryCodes.Portugal : CountryCodes.NewZealand))
			{
				SetupForSave();
				Header.AH_TransactionNum = "Test001";

				if (Header.AH_Ledger == LedgerTypes.AccountsReceivable
					&& (Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote))
				{
					Header.AH_OH = TestObjectCreator.AALSHI.PK;
					Header.Header.OH_RL_NKClosestPort = isOrgHeaderFromPortugal ? "PTLIS" : "NZAKL";
					AssertEquals("PreCond: org country", isOrgHeaderFromPortugal, Header.Header.CountryCode == CountryCodes.Portugal);
					if (!customCodePtIva.IsNullOrEmpty())
					{
						TestObjectCreator.CreateCustomsCodes(TestObjectCreator.AALSHI, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, customCodePtIva);
					}
				}
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, Header.PK).AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.IVA);
				var referenceIVA = newFactory.LoadTop1<AccTransactionHeaderReference>(query);
				if (shouldCreatedReferenceIVA
					&& Header is InvoicingBase
					&& (Header.AH_Ledger == LedgerTypes.AccountsReceivable)
					&& (Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote))
				{
					AssertNotNull(referenceIVA);
				}
				else
				{
					AssertNull(referenceIVA);
				}
			}
		}

		#endregion

		public void TestTransactionHeaderAuthorizationNumberReference()
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, Header.PK);
			AssertNull("Percondition", Factory.LoadTop1<AccTransactionHeaderReference>(query));
			AssertNullOrEmpty("Percondition", Header.AuthorizationNumberReference);
			Header.AH_TransactionNum = "Test001";
			Header.AuthorizationNumberReference = "Test";
			Factory.Save();
			AssertEquals("Test", Header.AuthorizationNumberReference);

			var newFactory = new BusinessObjectFactory();
			var transactionHeaderReferenceATH = newFactory.LoadTop1<AccTransactionHeaderReference>(query);
			Header = newFactory.Load<TransactionHeader>(Header.PK);
			AssertNotNull(transactionHeaderReferenceATH);
			AssertEquals(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH, transactionHeaderReferenceATH.AH1_Type);
			AssertEquals("Test", transactionHeaderReferenceATH.AH1_Reference);
			AssertEquals("Test", Header.AuthorizationNumberReference);
		}

		public void TestResetInvoiceLineAmountsWithSourceReference()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsReceivable && Header.AH_TransactionType == TransactionTypes.Invoice)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					invoice.SourceReference = "FTM a";

					var line1 = (ARInvoiceLine)invoice.Lines.AddNew();
					var line2 = (ARInvoiceLine)invoice.Lines.AddNew();
					line1.AL_AC = TestObjectCreator.CC1.PK;
					line2.AL_AC = TestObjectCreator.CC1.PK;
					line1.AL_LocalExTaxAmount = 100m;
					line2.AL_LocalExTaxAmount = 50m;
					AssertEquals(150m, invoice.AH_OSTotalAmount);
					line1.AL_OverseasTotal = 110m;
					line2.AL_OverseasTotal = 60m;
					AssertEquals(150m, invoice.AH_OSTotalAmount);

					invoice.AH_OSTotalAmount = 200m;
					Assert(invoice.IsSourceReferenceUsed);
					AssertEquals("When Source Reference is used, we do not check the synchronisation between line amounts and invoice total. Bad data can be saved", 200m, invoice.AH_OSTotalAmount);
					AssertEquals(110m, line1.AL_OverseasTotal);
					AssertEquals(60m, line2.AL_OverseasTotal);

					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
					Assert("overriden amounts must be reset when the invoice is not using Source Reference anymore", !invoice.IsSourceReferenceUsed);
					AssertEquals(150m, invoice.AH_OSTotalAmount);
					AssertEquals(100m, line1.AL_OverseasTotal);
					AssertEquals(50m, line2.AL_OverseasTotal);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestPostingInvoiceWithSourceReference()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsReceivable && Header.AH_TransactionType == TransactionTypes.Invoice)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					invoice.SourceReference = "FTM a";

					var line1 = (ARInvoiceLine)invoice.Lines.AddNew();
					var line2 = (ARInvoiceLine)invoice.Lines.AddNew();
					line1.AL_AC = TestObjectCreator.CC1.PK;
					line2.AL_AC = TestObjectCreator.CC1.PK;
					line1.AL_LocalExTaxAmount = 100m;
					line2.AL_LocalExTaxAmount = 50m;
					AssertEquals(150m, invoice.AH_OSTotalAmount);

					invoice.AH_OSTotalAmount = 180m;
					AssertNotEquals("line amounts are not equals to the invoice total", line1.AL_OverseasTotal + line2.AL_OverseasTotal, invoice.AH_OSTotal);

					Factory.Save();
					Assert("invoice is saved without critical validation error", invoice.IsInDatabase);
					AssertEquals(180m, invoice.AH_OSTotal);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSourceReference()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsReceivable
				&& (Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote))
			{
				Assert("Source Ref should be enabled only for Portugal", !Header.IsSourceReferenceEnabled);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					Assert("PreCondition", Header.IsSourceReferenceEnabled);
					AssertEquals("PreCondition", string.Empty, Header.AH_ComplianceSubType);
					AssertEquals(string.Empty, Header.SourceReference);

					var complianceSubTypes = typeof(PortugalComplianceInfo.ComplianceSubTypeCodes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
					foreach (var complianceSubType in complianceSubTypes)
					{
						Header.AH_ComplianceSubType = complianceSubType;

						if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TXM)
						{
							Assert(!Header.SourceReferenceInfo.ReadOnly);
							Assert(Header.IsSourceReferenceUsed);
							AssertEquals(PortugalComplianceInfo.ReferencePrefixes.FTM, Header.SourceReference);
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TCM)
						{
							Assert(!Header.SourceReferenceInfo.ReadOnly);
							Assert(Header.IsSourceReferenceUsed);
							AssertEquals(PortugalComplianceInfo.ReferencePrefixes.NCM, Header.SourceReference);
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LTX)
						{
							Assert(!Header.SourceReferenceInfo.ReadOnly);
							Assert(Header.IsSourceReferenceUsed);
							AssertEquals(PortugalComplianceInfo.ReferencePrefixes.FTD, Header.SourceReference);
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCR)
						{
							Assert(!Header.SourceReferenceInfo.ReadOnly);
							Assert(Header.IsSourceReferenceUsed);
							AssertEquals(PortugalComplianceInfo.ReferencePrefixes.NCD, Header.SourceReference);
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCD)
						{
							Assert(!Header.SourceReferenceInfo.ReadOnly);
							Assert(Header.IsSourceReferenceUsed);
							AssertEquals(PortugalComplianceInfo.ReferencePrefixes.NDD, Header.SourceReference);
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TDM)
						{
							Assert(!Header.SourceReferenceInfo.ReadOnly);
							Assert(Header.IsSourceReferenceUsed);
							AssertEquals(PortugalComplianceInfo.ReferencePrefixes.NDM, Header.SourceReference);
						}
						else
						{
							Assert(Header.SourceReferenceInfo.ReadOnly);
							Assert(!Header.IsSourceReferenceUsed);
							Assert(Header.SourceReference.IsEmpty);
						}
					}

					Header.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					AssertEquals(PortugalComplianceInfo.ReferencePrefixes.FTM, Header.SourceReference);
					Assert(!Header.SourceReferenceInfo.ReadOnly);
					Header.SourceReference = "FTM a";
					Factory.Save();
					Assert(Header.SourceReferenceInfo.ReadOnly);

					var headerReload = new BusinessObjectFactory().Load<TransactionHeader>(Header.PK);
					AssertEquals("FTM a", headerReload.SourceReference);

					headerReload.AH_ComplianceSubType = string.Empty;
					Assert(headerReload.SourceReferenceInfo.ReadOnly);
					Assert(!headerReload.IsSourceReferenceUsed);
					Assert(headerReload.SourceReference.IsEmpty);

					headerReload.Factory.Save();
					Assert("AccTransactionHeaderReference should be deleted when SourceReference is empty", !IsRelatedHeaderReferenceCreated(headerReload));

					var newHeader = Factory.NewWithValidTestData<ARInvoice>();
					Assert("AccTransactionHeaderReference is not created", !IsRelatedHeaderReferenceCreated(newHeader));

					AssertEquals(string.Empty, newHeader.SourceReference);
					Assert("SourceReference Getter should not create AccTransactionHeaderReference", !IsRelatedHeaderReferenceCreated(newHeader));

					newHeader.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					newHeader.SourceReference = "FTM b";
					Assert("SourceReference Setter should create a new instance of AccTransactionHeaderReference", IsRelatedHeaderReferenceCreated(newHeader));

					bool IsRelatedHeaderReferenceCreated(TransactionHeader header)
					{
						var filter = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, header.PK).AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.PIR);
						return header.Factory.LoadTop1<AccTransactionHeaderReference>(filter) != null;
					}
				}
			}
			else
			{
				Assert("Need to be AR INV/CRD and PT to be enabled", !Header.IsSourceReferenceEnabled);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
				{
					Assert("Need to be AR INV/CRD to be enabled", !Header.IsSourceReferenceEnabled);
				}
			}
		}

		protected virtual ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => expectedValue;

		public virtual void TestIsTaxReportable()
		{
			Assert(!Header.IsTaxReportable);
		}

		[SuspendCriticalValidation]
		public void TestTransactionModificationDoesNotCreateTransactionPivot()
		{
			AssertEquals(0, GetTableRowCount(AccEInvoicingTransactionPivot.Schema.TableName));
			Factory.Save();
			AssertEquals(0, GetTableRowCount(AccEInvoicingTransactionPivot.Schema.TableName));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Header.AH_Desc = "some new description";
				Assert("Has changes", Header.HasChanges);
				Factory.Save();
				AssertEquals(0, GetTableRowCount(AccEInvoicingTransactionPivot.Schema.TableName));
			}
		}

		#region EInvoicing Test Cases

		[TestDate(2020, 03, 26)]
		public void TestDefaultBlankInvoiceDateForAP()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsPayable &&
				(Header.AH_TransactionType == TransactionTypes.Invoice ||
				 Header.AH_TransactionType == TransactionTypes.CreditNote ||
				 Header.AH_TransactionType == TransactionTypes.AdjustmentNote))
			{
				using (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.CurrentDate))
				{
					var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;
					AssertEquals("Invoice Date should be current adding date (ADD)", ZDateTime.Now.Date, transaction.AH_InvoiceDate.Date);
				}

				using (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.Blank))
				{
					var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;
					AssertEquals("Invoice Date should be blank (BLK)", ZDateTime.Empty, transaction.AH_InvoiceDate);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestEInvoicingTransactionPivotForSubmitActionType()
		{
			var factory = TestObjectCreator.Factory;
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(turkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				TestObjectCreator.CreateNewComplianceSequence(turkeyBranch.GB_GC, turkeyBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.TRY, 1m, TestObjectCreator.DebtorTR);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				factory.Save();

				var invoicePivotSubmit = factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(invoicePivotSubmit);
				invoicePivotSubmit.AIP_ErrorDescription = ErrorDescriptionSubmit;
				factory.Save();

				AssertEquals(AssertMessagePivotAction, Constants.EInvoicingPivotActionType.Submit, invoicePivotSubmit.AIP_ActionType);
				Assert("No Batch for this Submission Pivot", invoicePivotSubmit.AIP_AIB.IsEmpty);
				AssertEquals(ErrorDescriptionSubmit, arInvoice.EInvoicingError);

				invoicePivotSubmit.AIP_Status = Constants.EInvoicingPivotState.Discarded;
				factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Discarded pivot is visible while no other pivots are present", ErrorDescriptionSubmit, arInvoice.EInvoicingError);
					AssertEquals("Discarded pivot is visible while no other pivots are present", EInvoicingPivotState.Discarded, arInvoice.EInvoicingStatus);
				});

				var invoicingBatchDocumentAction = TestObjectCreator.CreateEInvoicingBatch(101, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				factory.Save();
				var invoicePivotDocumentAction = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatchDocumentAction, arInvoice, Core.Constants.EInvoicingPivotState.Batched, EInvoicingPivotActionType.DocumentAction);
				invoicePivotDocumentAction.AIP_ErrorDescription = "Pivot for DocumentAction";
				factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Discarded pivot is visible while no other pivots are present; DocumentAction is not shown", ErrorDescriptionSubmit, arInvoice.EInvoicingError);
					AssertEquals("Discarded pivot is visible while no other pivots are present; DocumentAction is not shown", EInvoicingPivotState.Discarded, arInvoice.EInvoicingStatus);
				});

				var invoicingBatchStatusCheck = TestObjectCreator.CreateEInvoicingBatch(102, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				factory.Save();
				var invoicePivotStatusCheck = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatchStatusCheck, arInvoice, Core.Constants.EInvoicingPivotState.Batched, EInvoicingPivotActionType.StatusCheck);
				invoicePivotStatusCheck.AIP_ErrorDescription = "Pivot for StatusCheck";
				factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Discarded pivot is visible while no other pivots are present; StatusCheck is not shown", ErrorDescriptionSubmit, arInvoice.EInvoicingError);
					AssertEquals("Discarded pivot is visible while no other pivots are present; StatusCheck is not shown", EInvoicingPivotState.Discarded, arInvoice.EInvoicingStatus);
				});

				var invoicingBatchSubmit2 = TestObjectCreator.CreateEInvoicingBatch(103, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				factory.Save();
				var invoicePivotSubmit2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatchSubmit2, arInvoice, Core.Constants.EInvoicingPivotState.Succeed);
				invoicePivotSubmit2.AIP_ErrorDescription = "Second Pivot for Submit";
				factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Discarded / null pivot pivot forces reload from database; 2nd pivot is shown and discarded pivot hidden", "Second Pivot for Submit", arInvoice.EInvoicingError);
					AssertEquals("Discarded null pivot pivot forces reload from database; 2nd pivot is shown and discarded pivot hidden", EInvoicingPivotState.Succeed, arInvoice.EInvoicingStatus);
				});
			}
		}

		public void TestEInvoicingTransactionPivotForCancelActionType()
		{
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(turkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				TestObjectCreator.CreateNewComplianceSequence(turkeyBranch.GB_GC, turkeyBranch.PK, ComplianceSubTypeCodes.EAR);
				TestObjectCreator.CreateNewComplianceSequence(turkeyBranch.GB_GC, turkeyBranch.PK, ComplianceSubTypeCodes.ICN);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.TRY, 1m, TestObjectCreator.DebtorTR);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
				Factory.Save();

				var invoicePivotSubmit = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(invoicePivotSubmit);
				invoicePivotSubmit.AIP_ErrorDescription = ErrorDescriptionSubmit;
				invoicePivotSubmit.AIP_Status = EInvoicingPivotState.Succeed;
				var invoicingBatchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivotSubmit, 101, EInvoicingBatchState.Sent);
				invoicingBatchSubmit.AIB_GovernmentAllocatedNumber = "Government Allocated Number";
				//Factory.Save();

				AssertEquals(AssertMessagePivotAction, EInvoicingPivotActionType.Submit, invoicePivotSubmit.AIP_ActionType);
				AssertEquals(ErrorDescriptionSubmit, arInvoice.EInvoicingError);
				AssertEquals("Government Allocated Number", arInvoice.EInvoicingGovernmentAllocatedNumber);

				var arCreditNote = (ARCreditNote)TestObjectCreator.ReverseTransaction(arInvoice, out _);
				arCreditNote.AH_ComplianceSubType = ComplianceSubTypeCodes.ICN;
				Factory.Save();

				var creditNotePivotCancel = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertNotNull(creditNotePivotCancel);
				creditNotePivotCancel.AIP_ErrorDescription = "Pivot for Cancel";
				creditNotePivotCancel.AIP_Status = EInvoicingPivotState.Sent;
				var creditNoteBatchCancel = TestObjectCreator.CreateEInvoicingBatchForPivot(creditNotePivotCancel, 102, EInvoicingBatchState.Sent);
				creditNoteBatchCancel.AIB_GovernmentAllocatedNumber = invoicingBatchSubmit.AIB_GovernmentAllocatedNumber;
				//Factory.Save();

				AssertEquals(AssertMessagePivotAction, EInvoicingPivotActionType.Cancel, creditNotePivotCancel.AIP_ActionType);
				AssertEquals("Pivot for Cancel", arCreditNote.EInvoicingError);
				AssertEquals(arInvoice.EInvoicingGovernmentAllocatedNumber, arCreditNote.EInvoicingGovernmentAllocatedNumber);
			}
		}

		[ExpectNoExceptions]
		public void TestUseEvaluateEligibilityAndQueue()
		{
			SetupForSave();
			Header.AH_TransactionNum = "100109";

			var transactionMock = new Mock<IEInvoicingTransaction>();
			var proxyMock = new Mock<IEInvoicingTransactionProxyFactory>();
			proxyMock.Setup(x => x.GetProxy(It.IsAny<AccTransactionHeader>())).Returns(transactionMock.Object);
			using (ObjectFactory.Substitute(proxyMock.Object))
			{
				proxyMock.Verify(x => x.GetProxy(Header), Times.Never, "EInvoicing eligibility is not evaluated until after bizo is in database");
				transactionMock.Verify(x => x.EvaluateEligibilityAndQueue(), Times.Never, "EInvoicing eligibility is not evaluated until after bizo is in database");

				Factory.Save();

				if (!(Header is InvoiceBulkBatch))
				{
					proxyMock.Verify(x => x.GetProxy(Header), Times.Once, "EInvoicingTransactionProxy must be invoked with parent transaction");
					transactionMock.Verify(x => x.EvaluateEligibilityAndQueue(), Times.AtLeastOnce, "EInvoicing eligibility must be evaluated");
				}
				else
				{
					proxyMock.Verify(x => x.GetProxy(Header), Times.Never, "EInvoicingTransactionProxy must not be invoked if parent transaction is InvoiceBulkBatch");
					transactionMock.Verify(x => x.EvaluateEligibilityAndQueue(), Times.Never, "EInvoicing eligibility must not be evaluated if parent transaction is InvoiceBulkBatch");
				}
			}
		}

		public void TestIsTransactionReferenceAssignedAndEInvoicingTransactionPivotCreated_ComplianceNumberAllocationReceivablesRegistry_MAN_Option() =>
			AssertIsTransactionReferenceAssignedAndEInvoicingTransactionPivotCreated_ComplianceNumberAllocationReceivablesRegistry(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual);

		public void TestIsTransactionReferenceAssignedAndEInvoicingTransactionPivotCreated_ComplianceNumberAllocationReceivablesRegistry_PST_Option() =>
			AssertIsTransactionReferenceAssignedAndEInvoicingTransactionPivotCreated_ComplianceNumberAllocationReceivablesRegistry(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

		void AssertIsTransactionReferenceAssignedAndEInvoicingTransactionPivotCreated_ComplianceNumberAllocationReceivablesRegistry(string registryOption)
		{
			var factory = TestObjectCreator.Factory;
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");

			TestObjectCreator.CreateNewComplianceSequence(turkeyBranch.GB_GC, turkeyBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(turkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-10), registryOption))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.TRY, 1m, TestObjectCreator.DebtorTR);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				factory.Save();

				var invoicePivotSubmit = factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				if (registryOption == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual)
				{
					AssertEquals("Compliance number", "", arInvoice.AH_TransactionReference);
					AssertNull(invoicePivotSubmit);
				}
				else
				{
					AssertEquals("EIN0025", arInvoice.AH_TransactionReference);
					AssertNotNull(invoicePivotSubmit);
					AssertEquals(EInvoicingPivotActionType.Submit, invoicePivotSubmit.AIP_ActionType);
					AssertEquals(EInvoicingPivotState.Queued, invoicePivotSubmit.AIP_Status);
				}
			}
		}

		public void TestSupportingDocumentNumber()
		{
			if (!(Header is InvoiceBulkBatch))
			{
				SetupForSave();
				Header.AH_TransactionNum = "Test001";
				Header.SupportingDocumentNumber = "Test001";
				Factory.Save();
				Assert("By default, the HasChanges property of the BO should be false", !Header.HasChanges);
				Header.SupportingDocumentNumber = "Test002";
				Assert("When SupportingDocumentNumber has been changed, the HasChanges property of the BO should be changed to true.", Header.HasChanges);
				AssertEquals("Test002", Header.SupportingDocumentNumber);
			}
			else
			{
				Assert("Exclude InvoiceBulkbAtch", true);
			}
		}

		public void TestTransactionHeaderReferenceIRD()
		{
			if (!(Header is InvoiceBulkBatch))
			{
				AssertNull(Header.TransactionHeaderReferenceIRD_ForTestOnly);

				SetupForSave();
				Header.AH_TransactionNum = "Test001";
				Header.SupportingDocumentNumber = "xyz";
				Factory.Save();

				AssertNotNull(Header.TransactionHeaderReferenceIRD_ForTestOnly);
				AssertEquals(Header.PK, Header.TransactionHeaderReferenceIRD_ForTestOnly.AH1_AH);
				AssertEquals("xyz", Header.TransactionHeaderReferenceIRD_ForTestOnly.AH1_Reference);
				AssertEquals(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD, Header.TransactionHeaderReferenceIRD_ForTestOnly.AH1_Type);
			}
			else
			{
				Assert("Exclude InvoiceBulkbAtch", true);
			}
		}

		public void TestEInvoicingTransactionPivotSubmitted()
		{
			SetupForSave();
			Header.AH_TransactionNum = "Test001";

			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(Header, status: EInvoicingPivotState.Discarded);
			Factory.Save();

			AssertNull(Header.EInvoicingTransactionPivotSubmitted);

			pivot.AIP_Status = EInvoicingPivotState.Queued;
			AssertNotNull(Header.EInvoicingTransactionPivotSubmitted);
		}

		#region GovernmentAllocatedNumber and AuthorisationDateTime Test Cases

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_ForNonEInvoicingCountry()
		{
			var australiaBranch = TestObjectCreator.CreateBranchWithCompany("AUSYD");

			Header.AH_TransactionNum = "0001";
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001001", TestObjectCreator.TRY, 1m, TestObjectCreator.DebtorTR);
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Pending);
				invoicePivot.AIP_Status = EInvoicingPivotState.Sent;
				var invoiceBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivot, 101, invoicePivot.AIP_Status);
				invoiceBatch.AIB_GovernmentAllocatedNumber = "Number";
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoice);
				authRecord.AHF_RecordType = "ARG";
				authRecord.AHF_Number = "Number";
				authRecord.AHF_DateTime = ZDateTimeOffset.Now;
				Factory.Save();

				AssertEquals("Government Allocated Number should be empty for non E-Invoicing countries", ZString.Empty, arInvoice.EInvoicingGovernmentAllocatedNumber);
				Assert("Authorisation Date Time should be empty for non E-Invoicing countries", arInvoice.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_ForBatchCountry()
		{
			var turkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			Header.AH_TransactionNum = "0002";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(turkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001002", TestObjectCreator.TRY, 1m, TestObjectCreator.DebtorTR);
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Pending);
				invoicePivot.AIP_Status = EInvoicingPivotState.Sent;
				var invoiceBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivot, 102, invoicePivot.AIP_Status);
				invoiceBatch.AIB_GovernmentAllocatedNumber = "Correct Number";
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoice);
				authRecord.AHF_RecordType = "ARG";
				authRecord.AHF_Number = "Wrong Number";
				authRecord.AHF_DateTime = ZDateTimeOffset.Now;
				Factory.Save();

				AssertEquals("Wrong Number", authRecord.AHF_Number);
				AssertEquals("Correct Number", invoiceBatch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Government Allocated Number should be read from AIB_GovernmentAllocatedNumber when configured via EInvoicingCountryComplianceInfo to do so (eg, Turkey)", "Correct Number", arInvoice.EInvoicingGovernmentAllocatedNumber);
				Assert("Authorisation Date Time should be empty for countries not using Authorisation record", arInvoice.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_ForAuthRecordCountry()
		{
			var egyptBranch = TestObjectCreator.CreateBranchWithCompany("EGIST");
			Header.AH_TransactionNum = "0003";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingEgypt(egyptBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001003", TestObjectCreator.EGP, 1m, TestObjectCreator.AALSHI);
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Pending);
				invoicePivot.AIP_Status = EInvoicingPivotState.Sent;
				var invoiceBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivot, 103, invoicePivot.AIP_Status);
				invoiceBatch.AIB_GovernmentAllocatedNumber = "Wrong Number";
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoice);
				authRecord.AHF_Number = "Correct Number";
				var testTime = authRecord.AHF_DateTime = ZDateTimeOffset.Now.AddSeconds(-15);
				Factory.Save();

				AssertEquals("Wrong Number", invoiceBatch.AIB_GovernmentAllocatedNumber);
				AssertEquals("Correct Number", authRecord.AHF_Number);
				AssertEquals("Government Allocated Number should be read from AccTransactionHeaderAuthRecord when configured via EInvoicingCountryComplianceInfo to do so (eg, Egypt)", "Correct Number", arInvoice.EInvoicingGovernmentAllocatedNumber);
				AssertEquals("Authorisation Date Time should be read from AccTransactionHeaderAuthRecord", testTime, arInvoice.EInvoicingAuthorisationDateTime);

				authRecord.AHF_DateTime = AuthRecordConstants.NullPlaceholderForDateTime;
				Factory.Save();
				Assert("EInvoicingAuthorisationDateTime should be empty when AuthorisationRecord.AHF_DateTime is legacy null placeholder", arInvoice.EInvoicingAuthorisationDateTime.IsEmpty);

				authRecord.AHF_DateTime = ZDateTimeOffset.Empty;
				Factory.Save();
				Assert("EInvoicingAuthorisationDateTime should be empty when AuthorisationRecord.AHF_DateTime is empty", arInvoice.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumber_WithNoEInvoicingTransactionPivot()
		{
			var egyptBranch = TestObjectCreator.CreateBranchWithCompany("EGIST");
			Header.AH_TransactionNum = "0004";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingEgypt(egyptBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoiceWithNoEInvoicingTransactionPivot = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001004", TestObjectCreator.EGP, 1m, TestObjectCreator.AALSHI);
				Factory.Save();
				AssertEquals("Government Allocated Number should be empty when TransactionHeader's EInvoicingTransactionPivot is null", ZString.Empty, arInvoiceWithNoEInvoicingTransactionPivot.EInvoicingGovernmentAllocatedNumber);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_WithMissingAuthRecord()
		{
			var egyptBranch = TestObjectCreator.CreateBranchWithCompany("EGIST");
			Header.AH_TransactionNum = "0005";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingEgypt(egyptBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoiceWithNoAuthRecord = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001005", TestObjectCreator.EGP, 1m, TestObjectCreator.AALSHI);
				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoiceWithNoAuthRecord, status: EInvoicingPivotState.Pending);
				invoicePivot.AIP_Status = EInvoicingPivotState.Sent;
				var invoiceBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivot, 105, invoicePivot.AIP_Status);
				invoiceBatch.AIB_GovernmentAllocatedNumber = "Wrong Number";
				Factory.Save();
				AssertEquals("Government Allocated Number should be empty when it needs to be read from AHF_Number, but the AccTransactionHeaderAuthorisationRecord is missing", ZString.Empty, arInvoiceWithNoAuthRecord.EInvoicingGovernmentAllocatedNumber);
				Assert("Authorisation Date Time should be empty when the AccTransactionHeaderAuthorisationRecord is missing", arInvoiceWithNoAuthRecord.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_WithInvalidAHF_RecordType()
		{
			var egyptBranch = TestObjectCreator.CreateBranchWithCompany("EGIST");
			Header.AH_TransactionNum = "0006";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingEgypt(egyptBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoiceWithInvalidAHF_RecordType = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001006", TestObjectCreator.EGP, 1m, TestObjectCreator.AALSHI);
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoiceWithInvalidAHF_RecordType);
				authRecord.AHF_Number = "Correct Number";
				authRecord.AHF_RecordType = "INI";
				Factory.Save();
				AssertEquals("Government Allocated Number should be empty when the AHF_RecordType is not valid or correct for the required country", ZString.Empty, arInvoiceWithInvalidAHF_RecordType.EInvoicingGovernmentAllocatedNumber);
				Assert("Authorisation Date Time should be empty when the AHF_RecordType is not valid or correct for the required country", arInvoiceWithInvalidAHF_RecordType.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_WithInvalidAHF_ParentTableCode()
		{
			// Assume we add another ParentTableCode other than AH
			var sqlToDropConstraint = @"ALTER TABLE [AccTransactionHeaderAuthorisationRecord] DROP CONSTRAINT [Constraint_AHF_ParentTableCode];
ALTER TABLE [dbo].[AccTransactionHeaderAuthorisationRecord]  ADD  CONSTRAINT [Constraint_AHF_ParentTableCode] CHECK  (([AHF_ParentTableCode]='AH' OR [AHF_ParentTableCode]='000'))
";
			Db.Connection.Command(sqlToDropConstraint).ExecuteNonQuery();

			var egyptBranch = TestObjectCreator.CreateBranchWithCompany("EGIST");
			Header.AH_TransactionNum = "0007";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingEgypt(egyptBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoiceWithInvalidAHF_ParentTableCode = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001007", TestObjectCreator.EGP, 1m, TestObjectCreator.AALSHI);
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoiceWithInvalidAHF_ParentTableCode);
				authRecord.AHF_Number = "Correct Number";
				authRecord.AHF_ParentTableCode = "000";
				Factory.Save();
				AssertEquals("Government Allocated Number should be empty when the AHF_ParentTableCode is invalid (has to be AH)", ZString.Empty, arInvoiceWithInvalidAHF_ParentTableCode.EInvoicingGovernmentAllocatedNumber);
				Assert("Authorisation Date Time should be empty when the AHF_ParentTableCode is invalid (has to be AH)", arInvoiceWithInvalidAHF_ParentTableCode.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		public void TestEInvoicingGovernmentAllocatedNumberAndAuthorisationDateTime_WithInvalidAHF_ParentId()
		{
			var egyptBranch = TestObjectCreator.CreateBranchWithCompany("EGIST");
			Header.AH_TransactionNum = "0008";
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingEgypt(egyptBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var arInvoiceWithInvalidAHF_ParentId = TestObjectCreator.CreateARInvoice<ARInvoice>("AR00001008", TestObjectCreator.EGP, 1m, TestObjectCreator.AALSHI);
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(arInvoiceWithInvalidAHF_ParentId);
				authRecord.AHF_Number = "Correct Number";
				authRecord.AHF_ParentId = ZGuid.Empty;
				Factory.Save();
				AssertEquals("Government Allocated Number should be empty when the AHF_ParentId is invalid", ZString.Empty, arInvoiceWithInvalidAHF_ParentId.EInvoicingGovernmentAllocatedNumber);
				Assert("Authorisation Date Time should be empty when the AHF_ParentId is invalid", arInvoiceWithInvalidAHF_ParentId.EInvoicingAuthorisationDateTime.IsEmpty);
			}
		}

		#endregion

		#region AuthorisationNumber

		public void TestEInvoicingAuthorisationNumber_WithAndWithoutAuthRecord()
		{
			if (!((Header.AH_Ledger == LedgerTypes.AccountsReceivable || Header.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote)))
			{
				Assert("Test not applicable", true);
				return;
			}

			var complianceInfoEInvoicingMock = new Mock<IComplianceInfoElectronicInvoicing>();
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(x => x.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(complianceInfoEInvoicingMock.Object);
			complianceInfoEInvoicingMock.Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns("AAA");

			ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

			var transactionHeader = PrepareTransactionHeaderForTest() as TransactionHeader;
			transactionHeader.Company.GC_RN_NKCountryCode = "XY";
			Assert("Authorisation number when there is no authorisation record", transactionHeader.EInvoicingAuthorisationNumber.IsEmpty);

			var transactionHeader1 = CreateTransactionHeaderAndAuthRecord("Correct Number");
			AssertEquals("Authorisation number when matching authorisation record type", "Correct Number", transactionHeader1.EInvoicingAuthorisationNumber);
		}

		public void TestEInvoicingAuthorisationNumber_DependencyOnRecordType()
		{
			if (!((Header.AH_Ledger == LedgerTypes.AccountsReceivable || Header.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote)))
			{
				Assert("Test not applicable", true);
				return;
			}

			var complianceInfoEInvoicingMock = new Mock<IComplianceInfoElectronicInvoicing>();
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(x => x.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(complianceInfoEInvoicingMock.Object);

			ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

			var expectedAHFNumber = "Correct Number";

			complianceInfoEInvoicingMock.Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns("AAA");
			var transactionHeader1 = CreateTransactionHeaderAndAuthRecord(expectedAHFNumber);
			AssertEquals("Authorisation number when matching authorisation record type", expectedAHFNumber, transactionHeader1.EInvoicingAuthorisationNumber);

			complianceInfoEInvoicingMock.Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns("BBB");
			var transactionHeader2 = CreateTransactionHeaderAndAuthRecord(expectedAHFNumber);
			Assert("Authorisation number when non-matching authorisation record type", transactionHeader2.EInvoicingAuthorisationNumber.IsEmpty);
		}

		public void TestEInvoicingAuthorisationNumber_DependencyOnParentId()
		{
			if (!((Header.AH_Ledger == LedgerTypes.AccountsReceivable || Header.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote)))
			{
				Assert("Test not applicable", true);
				return;
			}

			var complianceInfoEInvoicingMock = new Mock<IComplianceInfoElectronicInvoicing>();
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(x => x.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(complianceInfoEInvoicingMock.Object);
			complianceInfoEInvoicingMock.Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns("AAA");

			ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

			var expectedAHFNumber = "Correct Number";

			var transactionHeader = CreateTransactionHeaderAndAuthRecord(expectedAHFNumber, isCreateParentIdMismatch: true);
			Assert("Authorisation number when non-matching authorisation record type", transactionHeader.EInvoicingAuthorisationNumber.IsEmpty);

			var transactionHeader1 = CreateTransactionHeaderAndAuthRecord(expectedAHFNumber, isCreateParentIdMismatch: false);
			AssertEquals("Authorisation number when matching authorisation record type", expectedAHFNumber, transactionHeader1.EInvoicingAuthorisationNumber);
		}

		TransactionHeader CreateTransactionHeaderAndAuthRecord(ZString ahfNumber, bool isCreateParentTableCodeMismatch = false, bool isCreateParentIdMismatch = false)
		{
			var transactionHeader = PrepareTransactionHeaderForTest() as TransactionHeader;
			transactionHeader.Company.GC_RN_NKCountryCode = "XY";
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_Number = ahfNumber;
			authRecord.AHF_ParentTableCode = isCreateParentTableCodeMismatch ? "PPP" : AccTransactionHeaderSchema.Constants.Prefix;
			authRecord.AHF_RecordType = "AAA";
			authRecord.AHF_ParentId = isCreateParentIdMismatch ? ZGuid.NewZGuid() : transactionHeader.PK;

			return transactionHeader;
		}

		#endregion

		int GetTableRowCount(string tableName)
		{
			var sqlText = $"SELECT COUNT(*) FROM {tableName}";
			using (var command = Db.Connection.Command(sqlText))
			{
				return (int)command.ExecuteScalar();
			}
		}

		#endregion

		public void TestAH_MatchStatus()
		{
			Header.AH_TransactionNum = "123";
			AssertNotNull(Header.AH_MatchStatus);
			AssertEquals(ZString.Empty, Header.AH_MatchStatus);

			Header.AH_MatchStatus = "UAC";
			Factory.Save();

			AssertEquals("UAC", Header.AH_MatchStatus);
		}

		public void TestAH_MatchStatusReasonCode()
		{
			Header.AH_TransactionNum = "123";
			AssertNotNull(Header.AH_MatchStatusReasonCode);
			AssertEquals(ZString.Empty, Header.AH_MatchStatusReasonCode);

			Header.AH_MatchStatusReasonCode = "ADV";
			Factory.Save();

			AssertEquals("ADV", Header.AH_MatchStatusReasonCode);
		}

		public void TestShouldNotSetTransactionNumberToEmptyWhenTransactionIsInDB()
		{
			SetupForSave();
			Header.AH_TransactionNum = "Test001";
			Factory.Save();
			AssertNotNullOrEmpty("Transaction Number is not empty", Header.AH_TransactionNum);
			AssertEquals(true, Header.IsInDatabase);

			var oldTransactionNum = Header.AH_TransactionNum;
			Header.OnSaved(false);

			AssertEquals("Transaction number should not be reset if the header was in database.", oldTransactionNum, Header.AH_TransactionNum);
		}

		public void TestBOShouldChangedWhenInvoiceRemittanceReferenceChanged()
		{
			if (!(Header is InvoiceBulkBatch))
			{
				SetupForSave();
				Header.AH_TransactionNum = "Test001";
				Header.InvoiceRemittanceReference = "Test001";
				Factory.Save();
				Assert("By default, the HasChanges property of the BO should be false", !Header.HasChanges);
				Header.InvoiceRemittanceReference = "Test002";
				Assert("When InvoiceRemittanceReference has been changed, the HasChanges property of the BO should be changed to true.", Header.HasChanges);
				AssertEquals("Test002", Header.InvoiceRemittanceReference);
			}
			else
			{
				Assert("Exclude InvoiceBulkbAtch", true);
			}
		}

		public void TestResetTransactionNumWhileSavingFailed()
		{
			Header.AH_TransactionNum = string.Empty;

			if (Header.NumberFountainForTransactionNumber_ForTestOnly != null)
			{
				Header.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12);

				try
				{
					Factory.Save();
				}
				catch (OnSavingCriticalCheckException)
				{
				}
				finally
				{
					Header.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.NoError);
				}

				AssertEquals("Reset to empty", string.Empty, Header.AH_TransactionNum);

				ExceptionReporterTestListener.Instance.Clear();
			}
			else
			{
				Assert(true);
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionHeader()
		{
			var localList = new List<string>
				{
					nameof(Header.AH_LocalExTaxAmount),
					nameof(Header.AH_LocalTaxAmount),
					nameof(Header.AH_LocalWHTAmount),
					nameof(Header.AH_LocalTotalAmount),
					nameof(Header.AH_LocalOutstandingAmount),
					nameof(Header.OutstandingAmountMatching),
					nameof(Header.AH_LocalTotal),
					nameof(Header.LocalDebit),
					nameof(Header.LocalCredit),
					nameof(Header.LocalMatchedAmount),
					nameof(Header.BindableInvoiceAmount),
					nameof(Header.OutstandingAmountBindable),
				};

			var osList = new List<string>
				{
					nameof(Header.AH_OSExTax),
					nameof(Header.AH_OSExTaxAmount),
					nameof(Header.AH_OSTaxAmount),
					nameof(Header.AH_OSTax),
					nameof(Header.AH_OSWHTAmount),
					nameof(Header.AH_OSTotalAmount),
					nameof(Header.AH_Calc_OSOutstandingAmount),
					nameof(Header.AH_OSOutstandingAmountWithoutMultiplier),
					nameof(Header.OSOutstandingAmountMatching),
					nameof(Header.Debit),
					nameof(Header.Credit),
					nameof(Header.MatchedAmount),
					nameof(Header.BindableOSAmount)
				};

			var tester = new DecimalPlacesAttributeTester(Header, Header.Company);
			tester.CheckLocalCurrency(localList, nameof(Header.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(Header.OSCurrencyDecimals), nameof(Header.AH_RX_NKTransactionCurrency), Header);
		}

		public void TestIsAddressForSendingARDocumentsApplicable()
		{
			if ((Header is InvoicingBase) &&
					(Header.AH_Ledger == LedgerTypes.AccountsReceivable ||
					Header.AH_Ledger == LedgerTypes.AccountsPayable ||
					Header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
					Header.AH_Ledger == LedgerTypes.IncompleteTransactions)
					|| Header is TransactionPendingAllocation
					|| Header is InvoiceBatchHeader)
			{
				Assert(Header.IsAddressApplicableForDocumentSending_ForTestOnly);
			}
			else
			{
				Assert(!Header.IsAddressApplicableForDocumentSending_ForTestOnly);
			}
		}

		public void TestIsDocumentReceivedDateApplicable()
		{
			var ledger = Header.AH_Ledger;
			var transactionType = Header.AH_TransactionType;
			if ((ledger == LedgerTypes.AccountsPayable &&
					(transactionType == TransactionTypes.CreditNote ||
					transactionType == TransactionTypes.Invoice ||
					transactionType == TransactionTypes.AdjustmentNote)) ||
				ledger == LedgerTypes.TransactionsPendingAllocation ||
				ledger == LedgerTypes.UnapprovedPayableTransactions ||
				ledger == LedgerTypes.IncompleteTransactions)
			{
				Assert(Header.IsDocumentReceivedDateApplicable);
			}
			else
			{
				Assert(!Header.IsDocumentReceivedDateApplicable);
			}
		}

		public void TestAH_OA_InvoiceAddressOverride()
		{
			if (Header.IsAddressApplicableForDocumentSending_ForTestOnly)
			{
				Header.AH_OH = TestObjectCreator.ABIGAS.PK;
				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				AccTransactionHeader header = newFactory.Load<AccTransactionHeader>(Header.PK);
				AssertEquals("Expect override address is empty", ZGuid.Empty, header.AH_OA_InvoiceAddressOverride);

				TransactionHeader invoice = newFactory.Load<TransactionHeader>(Header.PK);
				AssertEquals("expect AH_OA_InvoiceAddressOverride is empty", ZGuid.Empty, invoice.AH_OA_InvoiceAddressOverride);
				AssertEquals("invoice will use DefaultOrgAddressPK if override address is empty", TestObjectCreator.ABIGAS.MainAddress.PK, invoice.DisplayInvoiceAddressOverride);

				OrgAddress newAddress = Factory.NewWithValidTestData<OrgAddress>();
				newAddress.OA_OH = TestObjectCreator.ABIGAS.PK;
				newAddress.OA_Code = "ABC";
				Header.DisplayInvoiceAddressOverride = newAddress.PK;
				Factory.Save();

				newFactory.ReloadAll<AccTransactionHeader>();
				AssertEquals("Expect override address is not empty", newAddress.PK, header.AH_OA_InvoiceAddressOverride);

				BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
				TransactionHeader header2 = newFactory2.Load<TransactionHeader>(Header.PK);
				AssertEquals("invoice will use override address if override address is not empty", newAddress.PK, header2.DisplayInvoiceAddressOverride);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDefaultOrgAddressPK()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("BORG", true, true);
			AssertDefaultOrgAddressPK(null, orgHeader);
		}

		public void TestDefaultOrgAddressPK_OverseasAgent()
		{
			var job = TestObjectCreator.CreateJob(null, 0M, TestObjectCreator.Agent, 0M);
			AssertDefaultOrgAddressPK(job, TestObjectCreator.Agent);
		}

		public void TestDefaultOrgAddressPK_LocalClient()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0M, null, 0M);
			AssertDefaultOrgAddressPK(job, TestObjectCreator.LocalClient);
		}

		void AssertDefaultOrgAddressPK(JobHeader job, OrgHeader orgHeader)
		{
			if (Header.IsAddressApplicableForDocumentSending_ForTestOnly)
			{
				var apAddress = TestObjectCreator.CreateAddress(orgHeader, "APM main office");
				apAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

				var arAddress = TestObjectCreator.CreateAddress(orgHeader, "ARM main office");
				arAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

				Header.AH_OH = orgHeader.PK;
				Header.AH_JH = job != null ? job.PK : ZGuid.Empty;
				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				TransactionHeader invoice = newFactory.Load<TransactionHeader>(Header.PK);

				AssertEquals("expect AH_OA_InvoiceAddressOverride is empty", ZGuid.Empty, invoice.AH_OA_InvoiceAddressOverride);

				if (Header.AH_Ledger == LedgerTypes.AccountsPayable || Header.AH_Ledger == LedgerTypes.IncompleteTransactions || Header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
						|| Header.AH_Ledger == LedgerTypes.TransactionsPendingAllocation || Header is Payment)
				{
					AssertEquals("AP address will be used", apAddress.PK, invoice.DisplayInvoiceAddressOverride);
				}
				else if (Header.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					if (job == null)
					{
						AssertEquals("AR address will be used", arAddress.PK, invoice.DisplayInvoiceAddressOverride);
					}
					else
					{
						AssertEquals("Main address will be used as local client or agent is set for the job", orgHeader.MainAddress.PK, invoice.DisplayInvoiceAddressOverride);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDefaultOrgAddressPKForGlobalAccount()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Test1", true, true);
			orgHeader.OH_IsGlobalAccount = ZBool.True;
			orgHeader.Addresses.RemoveAndDeleteAll();

			var address1 = orgHeader.Addresses.AddNew();
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_State = "NSW";
			address1.OA_Address1 = "SYDNEY";
			address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "ITMIL";
			address2.OA_State = "LBD";
			address2.OA_Address1 = "MILAN";
			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			var address3 = orgHeader.Addresses.AddNew();
			address3.OA_RL_NKRelatedPortCode = "GBLON";
			address3.OA_State = "LND";
			address3.OA_Address1 = "LONDON";
			address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV1", TestObjectCreator.AUD, 1.0m, 100M, 0.00m, 7.00m, 100.00m, 0.00m, 7.00m, orgHeader);
			AssertEquals("AH_OA_InvoiceAddressOverride is empty", ZGuid.Empty, invoice.AH_OA_InvoiceAddressOverride);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(address1.PK, invoice.DisplayInvoiceAddressOverride);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AssertEquals(address2.PK, invoice.DisplayInvoiceAddressOverride);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals(address3.PK, invoice.DisplayInvoiceAddressOverride);
			}
		}

		public void TestOpenQueryClaim()
		{
			var claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_QueryClaimReference = "ref";
			claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			claim.AY_AH = invoice.PK;
			AssertNotNull("Linked to open claim", invoice.OpenQueryClaim);

			claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed;
			AssertNull("Claim linked to invoice is closed", invoice.OpenQueryClaim);
		}

		public void TestRelatedClaimStatusAndQueryNumber()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var claim1 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim1.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim1.AY_AH = invoice.PK;
			var claim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim2.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed;
			claim2.AY_AH = invoice.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(["OPN", "CLS"], invoice.RelatedClaimStatus.Replace(" ", "").Split(","));
			AssertContainsExactElementsInAnyOrder([claim1.AY_QueryClaimReference, claim2.AY_QueryClaimReference], invoice.QueryNumber.Replace(" ", "").Split(","));
		}

		public void TestDisplayInvoiceAddressOverrides()
		{
			if (Header.IsAddressApplicableForDocumentSending_ForTestOnly)
			{
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Header.AH_OH = orgHeader.PK;
				Factory.Save();

				OrgAddressDependentCollection collection = Header.DisplayInvoiceAddressOverrides;
				int addressCountBefore = collection.Count;

				OrgAddress address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.OA_Code = "Code1";
				OrgAddress address2 = Factory.NewWithValidTestData<OrgAddress>();
				address2.OA_Code = "Code2";
				OrgAddress address3 = Factory.NewWithValidTestData<OrgAddress>();
				address3.OA_Code = "Code3";
				OrgAddress address4 = Factory.NewWithValidTestData<OrgAddress>();
				address4.OA_Code = "Code4";
				address4.OA_IsActive = false;

				address1.OA_OH = orgHeader.PK;
				address2.OA_OH = orgHeader.PK;
				address3.OA_OH = orgHeader.PK;
				address4.OA_OH = orgHeader.PK;

				Factory.Save();

				collection = Header.DisplayInvoiceAddressOverrides;
				AssertEquals("Expected 3 more addresses in collection", addressCountBefore + 3, collection.Count);
				Assert("First address should be in collection", collection.Contains(address1));
				Assert("Second address should be in collection", collection.Contains(address2));
				Assert("Third address should be in collection", collection.Contains(address3));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDisplayInvoiceContactOverrides()
		{
			if (Header.IsAddressApplicableForDocumentSending_ForTestOnly)
			{
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Header.AH_OH = orgHeader.PK;
				Factory.Save();

				OrgContactDependentCollection collection = Header.DisplayInvoiceContactOverrides;
				int contactCountBefore = collection.Count;

				OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
				contact1.OC_ContactName = "Code1";
				OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
				contact2.OC_ContactName = "Code2";
				OrgContact contact3 = Factory.NewWithValidTestData<OrgContact>();
				contact3.OC_ContactName = "Code3";
				OrgContact contact4 = Factory.NewWithValidTestData<OrgContact>();
				contact4.OC_ContactName = "Code4";
				contact4.OC_IsActive = false;

				contact1.OC_OH = orgHeader.PK;
				contact2.OC_OH = orgHeader.PK;
				contact3.OC_OH = orgHeader.PK;
				contact4.OC_OH = orgHeader.PK;

				Factory.Save();

				collection = Header.DisplayInvoiceContactOverrides;
				AssertEquals("Expected 3 more contacts in collection", contactCountBefore + 3, collection.Count);
				Assert("First contact should be in collection", collection.Contains(contact1));
				Assert("Second contact should be in collection", collection.Contains(contact2));
				Assert("Third contact should be in collection", collection.Contains(contact3));
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestTransactionNumberGenerator()
		{
			TransactionNumberSequenceCustomisationCollection customisation = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode;
			element.Include = true;
			element = customisation.AddNew();
			element.Length = 8;
			element.Order = 2;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Include = true;
			element = customisation.AddNew();
			element.Order = 50;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "BNE00000001BRN", Header.AH_TransactionNum);
		}

		public void TestOrgInactiveValidation()
		{
			if (!(Header is InvoiceBulkBatch))
			{
				Header.AH_OH = TestObjectCreator.AALSHI.PK;
				Assert(!Header.AH_OHInfo.ReadOnly);

				//AssertHasNotifications("This Account is inactive - it may not be used.", Header.AH_OHInfo);
				AssertNoError(Header.AH_OHInfo, "This Account is inactive - it may not be used.");

				Header.AH_OH = TestObjectCreator.ABIGAS.PK;
				AssertHasError(Header.HumanReadableName, Header.AH_OHInfo, "This Account is inactive - it may not be used.");
			}
			else
			{
				Assert("Exclude InvoiceBulkbAtch", true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public virtual void TestFetchHints()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaff staff1 = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = factory.NewWithValidTestData<GlbStaff>();
			GlbBranch branch1 = factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = factory.NewWithValidTestData<GlbBranch>();
			GlbDepartment department1 = factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = factory.NewWithValidTestData<GlbDepartment>();
			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.OB_IsCreditor = true;
			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_IsCreditor = true;
			RefCurrency currency1 = factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = factory.NewWithValidTestData<RefCurrency>();

			factory.Save();

			string currentLoginName = Env.CurrentUser.LoginName;
			ZGuid currentBranch = GlbBranch.CurrentBranch.PK;
			ZGuid currentDepartment = GlbDepartment.CurrentDepartment.PK;

			Env.SetUserContext(new UserContext(staff1.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()));
			TransactionHeader header1 = PrepareTransactionHeaderForTest() as TransactionHeader;
			header1.AH_OH = org1.PK;
			header1.AH_RX_NKTransactionCurrency = currency1.RX_Code;
			header1.AH_ExchangeRate = 1m;

			Env.SetUserContext(new UserContext(staff1.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()));
			TransactionHeader header2 = PrepareTransactionHeaderForTest() as TransactionHeader;
			header2.AH_OH = org2.PK;
			header2.AH_RX_NKTransactionCurrency = currency2.RX_Code;
			header2.AH_ExchangeRate = 1m;

			Env.SetUserContext(new UserContext(currentLoginName, currentBranch.ToGuid(), currentDepartment.ToGuid()));

			factory.Save();

			header1.Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();

			factory = new BusinessObjectFactory();
			TransactionHeader[] headers = factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, new ZGuid[] { header1.PK, header2.PK }));
			AssertEquals("Should have loaded 2 transaction headers", 2, headers.Length);
			int preLoadCount = factory.DatabaseLoadCount;
			string user = headers[0].CreatingUser;
			user = headers[1].CreatingUser;
			int postLoadCount = factory.DatabaseLoadCount;
			Assert("Should be no more DB hits", preLoadCount + 1 <= postLoadCount);

			preLoadCount = factory.DatabaseLoadCount;
			GlbBranch branch = headers[0].Branch;
			branch = headers[1].Branch;
			postLoadCount = factory.DatabaseLoadCount;
			Assert("Should be no more DB hits", preLoadCount + 1 <= postLoadCount);

			preLoadCount = factory.DatabaseLoadCount;
			GlbDepartment department = headers[0].Department;
			department = headers[1].Department;
			postLoadCount = factory.DatabaseLoadCount;
			Assert("Should be no more DB hits", preLoadCount + 1 <= postLoadCount);

			preLoadCount = factory.DatabaseLoadCount;
			RefCurrency currency = headers[0].TransactionCurrency;
			currency = headers[1].TransactionCurrency;
			postLoadCount = factory.DatabaseLoadCount;
			Assert("Should be no more DB hits", preLoadCount + 1 <= postLoadCount);
		}

		public virtual void TestTransactionNumberOnSave()
		{
			SetupForSave();
			Header.AH_TransactionNum = "TESTNUM";
			string expectedTransactionNumber = Header.NumberFountainForTransactionNumber_ForTestOnly.PeekPreliminary(Factory);
			Factory.Save();
			AssertEquals("Setting number fountain to TransactionNum", expectedTransactionNumber, Header.AH_TransactionNum);
		}

		public void TestIsInUnapprovedTransactionContext()
		{
			if (Header.Ledger_ForTestOnly == LedgerTypes.UnapprovedPayableTransactions)
			{
				Factory.Save();
				bool isInvoice = Header is UAInvoice;
				bool isCreditNote = Header is UACreditNote;
				var converter = new UnapprovedTransactionConverter(Factory);

				var convertedInvoice = converter.ConvertToAP((InvoicingBase)Header, false);
				UnapprovedTransactionCandidateCollection candidates = new UnapprovedTransactionCandidateCollection(Factory);
				candidates.Add(convertedInvoice);
				AssertEquals("Precondition: IsInUnapprovedTransactionContext", true, convertedInvoice.IsInUnapprovedTransactionContext_ForTestOnly);
				AssertEquals("Precondition: IsConvertedUAInvoiceOrCRD", true, convertedInvoice.IsConvertedUAInvoiceOrCRD);
				if (isInvoice)
				{
					AssertType("Should be UnApprovedInvoice TransactionHeader Validation", typeof(UnapprovedInvoiceCandidateValidation), convertedInvoice.Validation);
				}
				else if (isCreditNote)
				{
					AssertType("Should be UnApprovedCreditNote TransactionHeader Validation", typeof(UnapprovedCreditNoteCandidateValidation), convertedInvoice.Validation);
				}
				else
				{
					AssertType("Should be empty TransactionHeader Validation", TypeOfEmptyValidation, convertedInvoice.Validation);
				}
			}
			else
			{
				Assert("This is not a UA candidate", true);
			}
		}

		public void TestCorrectValidationForDirectDebitBatch()
		{
			if (Header is IDirectDebitBatchComponent)
			{
				if (Header is IDirectDebitBatchTransaction)
				{
					AssertEquals(false, Header.IsInDatabase);
					AssertEquals(false, Header.IsInDirectDebitBatchLineContext);
					Factory.Save();
					AssertEquals(false, Header.IsInDirectDebitBatchLineContext);

					var collection = new DirectDebitBatchLineCollection(Factory);
					collection.Add(Header);

					AssertEquals(true, Header.IsInDirectDebitBatchLineContext);
					AssertEquals(true, Header.IsInDatabase);
					AssertEquals(true, Header.IsTransactionInDatabaseReadOnly);

					var validation = Header.GetNewValidation_ForTestOnly();
					Assert(TypeOfEmptyValidation == validation.GetType());
				}
				else if (Header is DirectDebitBatchHeader)
				{
					AssertEquals(false, Header.IsInDatabase);
					AssertEquals(false, Header.IsInDirectDebitBatchLineContext);
					var validation = Header.GetNewValidation_ForTestOnly();
					Assert(typeof(DirectDebitBatchHeaderValidation) == validation.GetType());

					Factory.Save();
					AssertEquals(true, Header.IsInDatabase);
					AssertEquals(false, Header.IsInDirectDebitBatchLineContext);
					AssertEquals(true, Header.IsTransactionInDatabaseReadOnly);
					validation = Header.GetNewValidation_ForTestOnly();
					Assert(TypeOfEmptyValidation == validation.GetType());
				}
				else
				{
					throw new NotImplementedException(Header.GetType().ToString());
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestValidationWhenInMatchingContext()
		{
			if (Header is IMatching)
			{
				Assert("Should be standard TransactionHeader Validation", typeof(TransactionHeaderValidation).IsAssignableFrom(Header.Validation.GetType()));
				IMatchingCollection parentCollectionForMatching = new IMatchingCollection(Factory);
				parentCollectionForMatching.Add(Header);
				Assert("Should be Matching Validation" + " Header.Validation Type is " + Header.Validation.GetType().FullName, typeof(MatchingValidation).IsAssignableFrom(Header.Validation.GetType()));
				parentCollectionForMatching.Remove(Header);
				Assert("Should be standard TransactionHeader Validation", typeof(TransactionHeaderValidation).IsAssignableFrom(Header.Validation.GetType()));
			}
			else
			{
				Assert("Should be standard TransactionHeader Validation", typeof(TransactionHeaderValidation).IsAssignableFrom(Header.Validation.GetType()));
			}
		}

		public virtual void TestValidationTypeWhenReversing()
		{
			AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());

			Header.IsReverseTransaction = true;
			if (Header is InvoicingBase)
			{
				AssertEquals("Validation Type", TypeOfInvoicingBaseReversalValidation, Header.Validation.GetType());
			}
			else
			{
				AssertEquals("Validation Type", TypeOfReversalValidation, Header.Validation.GetType());
			}
		}

		public void TestValidationTypeWhenInOverrideTransactionDescriptionContext()
		{
			AssertEquals("IsInOverrideTransactionDescriptionContext default value", false, Header.HasContext(BusinessContext.OverrideTransactionDescription));
			AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());

			Header.SetContext(BusinessContext.OverrideTransactionDescription);
			if (Header is InvoicingBase)
			{
				AssertEquals("Validation Type", typeof(OverrideTransactionDescriptionValidation), Header.Validation.GetType());
			}
			else
			{
				AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());
			}

			Header.RemoveContext(BusinessContext.OverrideTransactionDescription);
			Header.IsInPreviewingInvoicesContext = true;

			if (Header.Validation is DepositBatchTransactionLineValidation)
			{
				AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());
			}
			else
			{
				AssertEquals("Validation Type", TypeOfEmptyValidation, Header.Validation.GetType());
			}
		}

		public void TestValidationTypeWhenInOverrideReceiptPaymentCashFlowCategoryContext()
		{
			AssertEquals("IsInOverrideReceiptPaymentCashFlowCategory default value", false, Header.HasContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory));
			AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());

			Header.SetContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory);
			if (Header is ReceiptPaymentBase)
			{
				AssertEquals("Validation Type", typeof(OverrideReceiptPaymentCashFlowCategoryValidation), Header.Validation.GetType());
			}
			else
			{
				AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());
			}
		}

		public void TestValidationTypeWhenInOverrideInvoiceAddressContactContext()
		{
			AssertEquals("IsInOverrideTransactionDescriptionContext default value", false, Header.HasContext(BusinessContext.OverrideInvoiceAddressContact));
			AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());

			Header.SetContext(BusinessContext.OverrideInvoiceAddressContact);
			if (Header is InvoicingBase)
			{
				AssertEquals("Validation Type", typeof(OverrideInvoiceAddressContactValidation), Header.Validation.GetType());
			}
			else
			{
				AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());
			}

			Header.RemoveContext(BusinessContext.OverrideInvoiceAddressContact);
			Header.IsInPreviewingInvoicesContext = true;

			if (Header.Validation is DepositBatchTransactionLineValidation)
			{
				AssertEquals("Validation Type", TypeOfValidation, Header.Validation.GetType());
			}
			else
			{
				AssertEquals("Validation Type", TypeOfEmptyValidation, Header.Validation.GetType());
			}
		}

		public virtual void TestDefaultPostDateReadOnly()
		{
			bool backPostingAlwaysAllowed = Header.Ledger_ForTestOnly == LedgerTypes.TransactionsPendingAllocation;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Header = PrepareTransactionHeaderForTest() as TransactionHeader;
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Header = PrepareTransactionHeaderForTest() as TransactionHeader;

			if (backPostingAlwaysAllowed)
			{
				Assert("Back posting is always allowed. AH_PostDate should be editable", !Header.AH_PostDateInfo.ReadOnly);
			}
			else
			{
				Assert("AH_PostDate should be readonly", Header.AH_PostDateInfo.ReadOnly);
			}

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Header = PrepareTransactionHeaderForTest() as TransactionHeader;
			if (backPostingAlwaysAllowed)
			{
				Assert("Back posting is always allowed. AH_PostDate should be editable", !Header.AH_PostDateInfo.ReadOnly);
			}
			else
			{
				Assert("AH_PostDate should be readonly", Header.AH_PostDateInfo.ReadOnly);
			}

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Header = PrepareTransactionHeaderForTest() as TransactionHeader;
			if (backPostingAlwaysAllowed)
			{
				Assert("Back posting is always allowed. AH_PostDate should be editable", !Header.AH_PostDateInfo.ReadOnly);
			}
			else
			{
				Assert("AH_PostDate should be readonly", Header.AH_PostDateInfo.ReadOnly);
			}
		}

		public void TestPostDateValidation()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Date should not have errors to begin", !Header.AH_PostDateInfo.HasErrors());
			Header.AH_PostDate = ZDateTime.Empty;
			Assert("Invalid Date should validate", Header.AH_PostDateInfo.HasErrors());
			Header.AH_PostDate = CurrentPeriod.AM_StartDate;
			Assert("Valid Date should not have errors", !Header.AH_PostDateInfo.HasErrors());
			Header.AH_PostDate = PreviousGLClosedPeriod.AM_StartDate.AddMonths(-6);
			Assert("Invalid Date should validate", Header.AH_PostDateInfo.HasErrors());
		}

		public virtual void TestAH_OSOutsantdingAmount_Reciprocal()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "SGD";
			company.GC_IsReciprocal = true;

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();

			using (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }.Set())
			{
				SetupHeaderExRatesAndAmounts(0.404858m, 100, 5410.32m, 0m);
				Header.AH_LocalOutstandingAmount = 2048.72m;
				AssertEquals("OS Outstanding Amount", 5060.35m, Header.AH_Calc_OSOutstandingAmount);
			}
		}

		public virtual void TestAH_OSOutstandingAmount()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 15.02m);
			Header.AH_LocalOutstandingAmount = 378.02m;
			AssertEquals("OS Outstanding Amount", 215.473m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_LocalOutstandingAmount = 110.540m;
			AssertEquals("OS Outstanding Amount with decimal not matching local total (i.e. invoiceamount+gstamount)",
				63.008m, Header.AH_Calc_OSOutstandingAmount);
		}

		public virtual void TestAH_OSOutstandingAmountWithoutMultiplier()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 15.02m);
			Header.AH_LocalOutstandingAmount = 378.02m;
			AssertEquals("OS Outstanding Amount", 215.473m * Header.Multiplier_ForTestOnly, Header.AH_OSOutstandingAmountWithoutMultiplier);

			Header.AH_LocalOutstandingAmount = 110.540m;
			AssertEquals("OS Outstanding Amount with decimal not matching local total (i.e. invoiceamount+gstamount)",
				63.008m * Header.Multiplier_ForTestOnly, Header.AH_OSOutstandingAmountWithoutMultiplier);
		}

		public void TestAH_OSOutstandingAmountForZeroLocalAmount()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "SGD";
			company.GC_IsReciprocal = true;

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();

			using (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }.Set())
			{
				SetupHeaderExRatesAndAmounts(0.000001m, 100, 4000m, 0m);
				AssertEquals("Precondition: AH_LocalOutstandingAmount", 0m, Header.AH_LocalOutstandingAmount);
				AssertEquals("Precondition: AH_LocalTotalAmount", 0m, Header.AH_LocalTotalAmount);
				AssertEquals("AH_Calc_OSOutstandingAmount", 0m, Header.AH_Calc_OSOutstandingAmount);
				AssertEquals("InvoiceUnpaid", false, Header.InvoiceUnpaid_ForTestOnly);
			}
		}

		public void TestAH_OSOutstandingAmountWithSmallExchangeRateForNonReciprocalCompany()
		{
			AH_OSOutstandingAmountWithSmallExchangeRateHelper(false, 13.647m, 110.540m);
		}

		public void TestAH_OSOutstandingAmountWithSmallExchangeRateForReciprocalCompany()
		{
			AH_OSOutstandingAmountWithSmallExchangeRateHelper(true, 13.641m, 1.684m);
		}

		void AH_OSOutstandingAmountWithSmallExchangeRateHelper(bool isReciprocal, ZDecimal expectedValue, ZDecimal localOutstandingAmount)
		{
			var backup = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;
				SetupHeaderExRatesAndAmounts(0.123456m, 1000, 200.453m, 15.02m);
				Header.AH_LocalOutstandingAmount = localOutstandingAmount;
				AssertEquals("OS Outstanding Amount with decimal and very small Exchange Rate", expectedValue, Header.AH_Calc_OSOutstandingAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = backup;
			}
		}

		public void TestCreatingUserID()
		{
			SetupForSave();
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "XXX";
			testUser.GS_LoginName = "TestX";
			Factory.Save();

			Header.AH_SystemCreateUser = "XXX";
			AssertEquals("Creating User ID", testUser.GS_LoginName, Header.CreatingUserID);
		}

		public void TestCreatingUser()
		{
			SetupForSave();
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "XXX";
			testUser.GS_FullName = "TestX";
			Factory.Save();

			Header.AH_SystemCreateUser = "XXX";
			AssertEquals("Creating User", testUser.GS_FullName, Header.CreatingUser);
		}

		public void TestCreateLogHasCorrectTypeOfEvent()
		{
			SetupForSave();
			AssertNotNull("Logs.AddedLog", Header.Logs.AddedLog);
			AssertNull("CreateLog is empty because Header is not saved", Header.CreateLog_ForTestOnly);

			Factory.Save();
			AssertNotNull("CreateLog", Header.CreateLog_ForTestOnly);
			AssertEquals("CreateLog should be Logs.AddedLog", Header.Logs.AddedLog.PK, Header.CreateLog_ForTestOnly.PK);
			AssertEquals("CreateLog Event type should be ADD", Events.AddedARecordToTheSystem.Code, Header.CreateLog_ForTestOnly.SL_SE_NKEvent);
		}

		public void TestAH_LocalTotalAmount()
		{
			Header.AH_LocalExTaxAmount = 50.00m;
			Header.AH_LocalTaxAmount = 5.00m;
			AssertEquals("AH_LocalTotalAmount should be 55", 55.00m, Header.AH_LocalTotalAmount);
		}

		public void TestTotalLocalInvoiceAmount()
		{
			Header.AH_LocalExTaxAmount = 50.00m;
			Header.AH_LocalTaxAmount = 5.00m;
			AssertEquals("Total Local Invoice Amount", Header.Multiplier_ForTestOnly * 55.00m, Header.AH_LocalTotal);
		}

		public void TestDaysOverdue()
		{
			Header.AH_DueDate = ZDateTime.Now.AddDays(-370);
			AssertEquals("FullyPaidDate should be empty", ZDateTime.Empty, Header.AH_FullyPaidDate);
			AssertEquals("Days Overdue should be calculated correctly", 370, Header.DaysOverdue);
			Header.AH_DueDate = ZDateTime.Now.AddDays(370);
			AssertEquals("Days Overdue should be calculated correctly", ZInt.Zero, Header.DaysOverdue);

			Header.AH_DueDate = ZDateTime.Now.AddDays(-370);
			Header.AH_FullyPaidDate = ZDateTime.Now;
			AssertNotEquals("FullyPaidDate should not be empty", ZDateTime.Empty, Header.AH_FullyPaidDate);
			AssertEquals("ONce FullyPaidDate gets set, Overdue date should be zero", ZInt.Zero, Header.DaysOverdue);
		}

		public void TestDaysBeforePaidProperties()
		{
			Assert("Pre-condition: AH_FullyPaidDate should be empty", Header.AH_FullyPaidDate.IsEmpty);
			AssertEquals("DaysFromInvoiceDateToFullyPaidDate", ZInt.Zero, Header.DaysFromInvoiceDateToFullyPaidDate);
			AssertEquals("DaysFromDueDateToFullyPaidDate", ZInt.Zero, Header.DaysFromDueDateToFullyPaidDate);

			Header.AH_FullyPaidDate = ZDateTime.Now;

			Header.AH_InvoiceDate = Header.AH_FullyPaidDate.AddDays(-10);
			AssertEquals("DaysFromInvoiceDateToFullyPaidDate", 10, Header.DaysFromInvoiceDateToFullyPaidDate);
			Header.AH_InvoiceDate = Header.AH_FullyPaidDate.AddDays(5);
			AssertEquals("DaysFromInvoiceDateToFullyPaidDate", -5, Header.DaysFromInvoiceDateToFullyPaidDate);

			Header.AH_DueDate = Header.AH_FullyPaidDate.AddDays(-3);
			AssertEquals("DaysFromDueDateToFullyPaidDate", 3, Header.DaysFromDueDateToFullyPaidDate);
			Header.AH_DueDate = Header.AH_FullyPaidDate.AddDays(2);
			AssertEquals("DaysFromDueDateToFullyPaidDate", -2, Header.DaysFromDueDateToFullyPaidDate);
		}

		public void TestTransactionHeaderNotUpdatedByRefreshBusIfAH_IsCancelledHasChanged()
		{
			var canBeSaved = Header.IsSavedByFactory;
			if (canBeSaved)
			{
				SetupForSave();
				PrepareMiscellaneousTransactionForSavingCore(Header);
				Factory.Save();

				var factory1 = Factory;
				var factory2 = new BusinessObjectFactory();

				var headerInFactory1 = Header;
				var headerInFactory2 = factory2.Load<TransactionHeader>(headerInFactory1.PK);

				CancelTransactionHeaderToBeAbleToSave(headerInFactory1);
				Assert("AH_IsCancelled", headerInFactory1.AH_IsCancelled);
				Assert("Precondition: Cancelled in factory1", headerInFactory1.IsCancelled);
				Assert("Precondition: Not cancelled in factory2", !headerInFactory2.IsCancelled);

				headerInFactory2.AH_Desc = "random stuff";
				if (!headerInFactory2.IsSavedByFactory)
				{
					ForceSavingByFactoryIfApplicable(headerInFactory2);
				}
				Assert("IsSavedByFactory", headerInFactory2.IsSavedByFactory);
				factory2.Save();

				Assert("AH_IsCancelled should not be overriden by the data refresh bus", headerInFactory1.AH_IsCancelled);
				Assert("Data refresh bus should not override the cancel flag after saving factory2", headerInFactory1.IsCancelled);
				AssertNotEquals("Description should not be overriden", "random stuff", headerInFactory1.AH_Desc);

				Assert("IsSavedByFactory", headerInFactory1.IsSavedByFactory);
				try
				{
					factory1.Save();
					Fail("Expecting concurrency error, this should fail");
				}
				catch (ZSaveConcurrencyException ex)
				{
					var handler = new Customs.Business.Testing.NotificationHandlerForTest();
					ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler);
					var expectedMsg = "While you have been working with this form, another user has made changes.";
					AssertStartsWith("Concurrency", expectedMsg, handler.ReportInformationMessage);
				}
				Assert("AH_IsCancelled should not be overriden", headerInFactory1.AH_IsCancelled);
				Assert("Should still be cancelled after getting concurrency error on saving factory1", headerInFactory1.IsCancelled);
			}
			else
			{
				Assert($"{Header.GetType()} cannot be saved", true);
			}
		}

		public void TestAH_IsCancelledHasHasStrictConcurrency()
		{
			var canBeSaved = Header.IsSavedByFactory;
			if (canBeSaved)
			{
				SetupForSave();
				PrepareMiscellaneousTransactionForSavingCore(Header);
				Factory.Save();

				var factory1 = new BusinessObjectFactory();
				factory1.RefreshEnabled = false;
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;

				var headerInFactory1 = factory1.Load<TransactionHeader>(Header.PK);
				Assert("IsInDatabase", headerInFactory1.IsInDatabase);
				CancelTransactionHeaderToBeAbleToSave(headerInFactory1);
				Assert(headerInFactory1.AH_IsCancelled);

				var headerInFactory2 = factory2.Load<TransactionHeader>(headerInFactory1.PK);
				CancelTransactionHeaderToBeAbleToSave(headerInFactory2);
				Assert(headerInFactory2.AH_IsCancelled);

				factory2.Save();
				try
				{
					factory1.Save();
					Fail("Expecting concurrency error, this should fail");
				}
				catch (ZSaveConcurrencyException ex)
				{
					var handler = new Customs.Business.Testing.NotificationHandlerForTest();
					ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler);
					var expectedMsg = @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.";
					AssertStartsWith("Strict concurrency", expectedMsg, handler.ReportInformationMessage);
				}
			}
			else
			{
				Assert($"{Header.GetType()} cannot be saved", true);
			}
		}

		public void TestIHandleDeleteError()
		{
			SetupForSave();
			PrepareMiscellaneousTransactionForSavingCore(Header);
			var header = Header;/* Factory.NewWithValidTestData(GetExpectedBusinessObjectType());*/

			var cancellable = header as ICancellable;
			AssertNotNull("Implements ICancellable", cancellable);
			Assert("IsCancelled is false", !cancellable.IsCancelled);
			Assert("IsCancelledHasChanged is false", !cancellable.IsCancelledHasChanged);

			var handleError = header as IHandleDeleteError;
			AssertNotNull("Implements IHandleDeleteError", handleError);
			Assert("RollbackAfterDeleteError on new InvoicingBase", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			var canBeSaved = header.IsSavedByFactory;

			if (canBeSaved)
			{
				Factory.Save();

				Assert("IsInDatabase", header.IsInDatabase);
				Assert("RollbackAfterDeleteError is true by default", handleError.RollbackAfterDeleteError);
				Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
			}

			// Do cancelling but no saving yet
			cancellable.IsCancelled = true;
			Assert("RollbackAfterDeleteError on saved and cancelled InvoicingBase to prevent rollback to non-cancelled state", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			cancellable.IsCancelled = false;
			AssertEquals("RollbackAfterDeleteError depends on the ability to save", canBeSaved, handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			if (canBeSaved)
			{
				var newFactory = new BusinessObjectFactory();
				var newHeader = newFactory.Load<TransactionHeader>(header.PK);
				var newHandleError = (IHandleDeleteError)newHeader;
				var newCancellable = (ICancellable)newHeader;
				// Cancel and save
				CancelTransactionHeaderToBeAbleToSave(newHeader);
				newFactory.Save();

				Assert("RollbackAfterDeleteError is back to default value when it is cancelled and saved", newHandleError.RollbackAfterDeleteError);
				Assert("RebindAfterDeleteError is always false", !newHandleError.RebindAfterDeleteError);

				newCancellable.IsCancelled = false;
				Assert("RollbackAfterDeleteError is back to default value when InvoicingBase is not cancelled", newHandleError.RollbackAfterDeleteError);
				Assert("RebindAfterDeleteError is always false", !newHandleError.RebindAfterDeleteError);
			}
		}

		protected virtual void CancelTransactionHeaderToBeAbleToSave(TransactionHeader header)
		{
			var cancellable = header as ICancellable;
			var matching = header as IMatching;

			if (IsMiscellaneousTransactionCore(header) && header.AH_OutstandingAmount.IsEmpty && matching != null)
			{
				matching.Unmatch(header.AH_LocalTotalAmount, header.AH_OSTotalAmount);
			}
			else
			{
				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(header);

				if (reversing != null && reversing.CanReverseTransaction)
				{
					reversing.Reverse();
					var reverseTransaction = reversing.ReverseTransaction as TransactionHeader;
					if (reverseTransaction != null && reverseTransaction.AH_TransactionNum.IsEmpty)
					{
						reverseTransaction.AH_TransactionNum = "_1";
					}
				}
			}

			if (cancellable != null)
			{
				if (!header.IsReversed || !cancellable.IsCancelled)
				{
					cancellable.IsCancelled = true;
				}
				Assert("IsCancelled", cancellable.IsCancelled);
			}
		}

		#region Debit & Credit Tests

		public virtual void TestLocalDebit()
		{
			Header.DebitField_ForTestOnly = 80m;
			Header.AH_ExchangeRate = 2m;
			AssertEquals("Local Debit should be 40", 40m, Header.LocalDebit);
		}

		public virtual void TestLocalCredit()
		{
			Header.CreditField_ForTestOnly = 70m;
			Header.AH_ExchangeRate = 3m;
			AssertEquals("Local Credit should be 23.33", 23.33m, Header.LocalCredit);
		}

		protected void AssertLocalDebitValueForNormalAndOpeningRecPay()
		{
			Header.AH_OSTotal = -40m;
			Header.AH_ExchangeRate = 4m;
			AssertEquals("LocalDebit should be 10", 10m, Header.LocalDebit);
		}

		protected void AssertLocalCreditValueForNormalAndOpeningRecPay()
		{
			Header.AH_OSTotal = 20m;
			Header.AH_ExchangeRate = 3m;
			AssertEquals("LocalCredit should be 6.67", 6.67m, Header.LocalCredit);
		}

		protected void AssertLocalDebitValueForDirectRecPayAndCBTrf_CBExx()
		{
			Header.AH_OSTotal = 30m;
			Header.AH_ExchangeRate = 3m;
			AssertEquals("LocalDebit should be 10", 10m, Header.LocalDebit);
		}

		protected void AssertLocalCreditValueForDirectRecPayAndCBTrf_CBExx()
		{
			Header.AH_OSTotal = -40m;
			Header.AH_ExchangeRate = 2m;
			AssertEquals("LocalCredit should be 20", 20m, Header.LocalCredit);
		}

		#endregion

		public virtual void TestDirectDebitNumber()
		{
			Assert("DirectDebitNumber should be empty", Header.DirectDebitNumber.IsEmpty);
		}

		public virtual void TestDepositBatchNumber()
		{
			Assert("DepositBatchNumber should be empty", Header.DepositBatchNumber.IsEmpty);
		}

		public virtual void TestAH_RX_NKTransactionCurrency()
		{
			Header.AH_RX_NKTransactionCurrency = ZString.Empty;
			AssertEquals("LocalExTaxAmount should be ReadOnly", true, Header.AH_LocalExTaxAmountInfo.ReadOnly);

			Header.AH_RX_NKTransactionCurrency = "NNN";
			AssertEquals("LocalExTaxAmount should not be ReadOnly", false, Header.AH_LocalExTaxAmountInfo.ReadOnly);

			Header.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("LocalExTaxAmount should be ReadOnly", true, Header.AH_LocalExTaxAmountInfo.ReadOnly);

			//not equal to local currency
			Header.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)).RX_Code;
			AssertEquals("LocalExTaxAmount should not be ReadOnly", false, Header.AH_LocalExTaxAmountInfo.ReadOnly);

			Header.IsReverseTransaction = true;
			AssertEquals("LocalExTaxAmount should be ReadOnly", true, Header.AH_LocalExTaxAmountInfo.ReadOnly);

			Header.IsReverseTransaction = false;
			AssertEquals("LocalExTaxAmount should not be ReadOnly", false, Header.AH_LocalExTaxAmountInfo.ReadOnly);

			Header.AH_ExchangeRate = 1m;
			Factory.Save();
			AssertEquals("LocalExTaxAmount should be ReadOnly", true, Header.AH_LocalExTaxAmountInfo.ReadOnly);
		}

		public void TestCreatingUserOnNew()
		{
			AssertEquals("Creating User must be blank to start off with. It's set OnSaving", ZString.Empty, Header.CreatingUser);
		}

		public void TestCreatedDateOnNew()
		{
			AssertEquals("Created Date must be blank to start off with. It's set OnSaving", ZDateTime.Empty, Header.CreatedDate);
		}

		public void TestCreateDate()
		{
			SetupForSave();
			Factory.Save();
			Assert("Created Date", (ZDateTime.Now - Header.CreatedDate) < new TimeSpan(0, 0, 30));

			Header.AH_SystemCreateTimeUtc = new ZDateTime(2022, 09, 01);
			AssertEquals("Created Date is from AH_SystemCreateTimeUtc.", Header.AH_SystemCreateTimeUtc.ToLocalBranchTime(), Header.CreatedDate);
		}

		public void TestCreatingUserReadOnly()
		{
			Assert("Creating User should always be readonly", Header.CreatingUserInfo.ReadOnly);
		}

		public void TestCreatedDateReadOnly()
		{
			Assert("Created Date should always be readonly", Header.CreatedDateInfo.ReadOnly);
		}

		public virtual void TestSetDefaultValues()
		{
			AssertEquals("Default Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Header.AH_RX_NKTransactionCurrency);
			Assert("Ledger should not be empty", Header.AH_Ledger != ZString.Empty);
			if (ZArchitecture.Core.TransactionTypes.ReceiptBatch != Header.AH_TransactionType &&
				ZArchitecture.Core.TransactionTypes.DDRBatch != Header.AH_TransactionType &&
				ZArchitecture.Core.TransactionTypes.InvoiceBatch != Header.AH_TransactionType)
			{
				Assert("AH_Desc should not be empty", Header.AH_Desc != ZString.Empty);
			}
			AssertEquals("AH_NumberOfSupportingDocuments ", AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(Header.AH_Ledger + Header.AH_TransactionType, 0), Header.AH_NumberOfSupportingDocuments);
		}

		public void TestSetDefaultValuesForAH_IsOSOutstandingAmountApplicable()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;
			AssertEquals(false, transaction.AH_IsOSOutstandingAmountApplicable);

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			transaction = PrepareTransactionHeaderForTest() as TransactionHeader;
			if (transaction.OSOutstandingAmountValueChangeMonitor.IsMonitorApplicable_ForTestOnly)
			{
				AssertEquals(true, transaction.AH_IsOSOutstandingAmountApplicable);
			}
		}

		public virtual void TestSetTransactionTypeAsDefault()
		{
			Assert("Transaction Type should not be empty", Header.AH_TransactionType != ZString.Empty);
		}

		public void TestOSTax_IsConsistentBeforeAndAfterPosting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Indonesia))
			{
				if (Header is InvoicingBase invoice && !(invoice is TransactionPendingAllocation))
				{
					var taxRate = TestObjectCreator.CreateTaxRate("TAX1", "10% VAT", 10, 1);

					var currencyCode = TestObjectCreator.USD.RX_Code;
					Header.AH_RX_NKTransactionCurrency = currencyCode;
					Header.AH_ExchangeRate = 14704M;

					invoice.Lines[0].AL_AT = taxRate.PK;
					invoice.Lines[0].AL_RX_NKTransactionCurrency = currencyCode;
					invoice.Lines[0].AL_ExchangeRate = 14703.803195M;
					invoice.Lines[0].AL_OSExTaxAmount = 5200000M;

					invoice.Lines[1].AL_AT = taxRate.PK;
					invoice.Lines[1].AL_RX_NKTransactionCurrency = currencyCode;
					invoice.Lines[1].AL_ExchangeRate = 14703.902189M;
					invoice.Lines[1].AL_OSExTaxAmount = 5460000M;

					var expectedOSTaxAmount = 1066000M;
					var expectedOSTotalAmount = 11726000M;
					AssertEquals(expectedOSTaxAmount, Header.AH_OSTax * Header.Multiplier_ForTestOnly);
					AssertEquals(expectedOSTotalAmount, Header.AH_OSTotalAmount);

					Factory.Save();

					var newFactory = new BusinessObjectFactory();
					var invoiceInNewFactory = newFactory.Load<TransactionHeader>(Header.PK);

					AssertEquals(expectedOSTaxAmount, invoiceInNewFactory.AH_OSTax * invoiceInNewFactory.Multiplier_ForTestOnly);
					AssertEquals(expectedOSTotalAmount, invoiceInNewFactory.AH_OSTotalAmount);
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
		}

		public virtual void TestOSGST()
		{
			Header.AH_ExchangeRate = 0.5m;
			Header.AH_OSTaxAmount = OSGSTTaxAmount;

			AssertEquals("OS Tax Amount", 100m, Header.AH_OSTaxAmount);
			AssertEquals("OS Tax", 100m * Header.Multiplier_ForTestOnly, Header.AH_OSTax);
			AssertEquals("OS Tax Amount converted to local", 200m, Header.AH_LocalTaxAmount);

			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			AssertEquals("OS Tax Amount", 100m, Header.AH_OSTaxAmount);
			AssertEquals("OS Tax Amount", 100m * Header.Multiplier_ForTestOnly, Header.AH_OSTax);
		}

		public virtual void TestAH_OSExTaxAmount()
		{
			Header.AH_ExchangeRate = 0.5m;
			Header.AH_OSExTaxAmount = 175m;
			AssertEquals("OS ex tax amount", 175m, Header.AH_OSExTaxAmount);
		}

		public void TestSignsAreInverted()
		{
			Header.AH_ExchangeRate = 1m;
			Header.AH_LocalExTaxAmount = 100m;
			Header.AH_LocalTaxAmount = 10m;
			Header.AH_LocalWHTAmount = 8m;
			Header.AH_OSTotalAmount = 200m;
			Header.AH_LocalOutstandingAmount = 190m;

			AssertEquals("Invoice Amount", 100m, Header.AH_LocalExTaxAmount);
			AssertEquals("GST Amount", 10m, Header.AH_LocalTaxAmount);
			AssertEquals("Withholding Tax", 8m, Header.AH_LocalWHTAmount);
			AssertEquals("OS Total", 200m, Header.AH_OSTotalAmount);
			AssertEquals("Outstanding Amount", 190m, Header.AH_LocalOutstandingAmount);

			int multiplier = Header.InvertSigns_ForTestOnly ? -1 : 1;
			AssertEquals("Row Invoice Amount", 100m * multiplier, ((IBusinessObjectInternals)Header).Row[Header.AH_InvoiceAmountInfo.Name]);
			AssertEquals("Row GST Amount", 10m * multiplier, ((IBusinessObjectInternals)Header).Row[Header.AH_GSTAmountInfo.Name]);
			AssertEquals("Withholding Tax", 8m * multiplier, ((IBusinessObjectInternals)Header).Row[Header.AH_WithholdingTaxInfo.Name]);
			AssertEquals("OS Total", 200m * multiplier, ((IBusinessObjectInternals)Header).Row[Header.AH_OSTotalInfo.Name]);
			AssertEquals("Outstanding Amount", 190m * multiplier, ((IBusinessObjectInternals)Header).Row[Header.AH_OutstandingAmountInfo.Name]);
		}

		public void TestPostPeriodToPostedDate()
		{
			Header.PostPeriod = PeriodManagementTestHelper.InvalidPeriodInt;
			AssertEquals("Should return invalid period", PeriodManagementTestHelper.InvalidPeriodInt, Header.PostPeriod);
			AssertEquals("Should set post date to invalid", ZDateTime.Empty, Header.AH_PostDate);

			Header.PostPeriod = 0;
			AssertEquals("Post date should be empty on empty period", ZDateTime.Empty, Header.AH_PostDate);
		}

		public void TestPostDateToPostPeriod()
		{
			Header.AH_PostDate = ZDateTime.Invalid;
			AssertEquals("Post Period should return 0", 0, Header.PostPeriod);

			Header.AH_PostDate = PeriodManagementTestHelper.FuturePeriod.AM_StartDate.AddDays(5);
			AssertEquals("Post Period should be future period", FuturePeriod.AM_Period, Header.PostPeriod);
		}

		public void TestAgePeriodToDueDate()
		{
			Header.AgePeriod = PeriodManagementTestHelper.InvalidPeriodInt;
			AssertEquals("Should return invalid period", PeriodManagementTestHelper.InvalidPeriodInt, Header.AgePeriod);
			AssertEquals("Due Date should be invalid", ZDateTime.Empty, Header.AH_DueDate);

			Header.AgePeriod = 0;
			AssertEquals("Due Date should be empty for empty period", ZDateTime.Empty, Header.AH_DueDate);
		}

		public void TestDueDateToAgePeriod()
		{
			Header.AH_DueDate = ZDateTime.Invalid;
			AssertEquals("Age Period should return 0", 0, Header.AgePeriod);

			Header.AH_DueDate = PeriodManagementTestHelper.FuturePeriod.AM_StartDate.AddDays(5);
			AssertEquals("Age Period should be future period", FuturePeriod.AM_Period, Header.AgePeriod);
		}

		public virtual void TestPostingToPriorNonClosedPeriodGivesWarning()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Header.AH_PostDate = PreviousOpenPeriod.AM_StartDate;
			AssertEquals("There should be a warning about posting to prior period", TransactionHeaderValidation.PreviousPostDateWarning, Header.AH_PostDateInfo.GetWarnings().GetFirstMessage());
		}

		public void TestTransactionNumberNotResetOnEdit()
		{
			SetupForSave();
			Header.Factory.Save();
			ZString currentTransactionNumber = Header.AH_TransactionNum;
			Header.AH_Desc = "Change Description so that OnSaving will run";
			Header.Factory.Save();
			AssertEquals("TransactionNumber should not be set again", currentTransactionNumber, Header.AH_TransactionNum);
		}

		public void TestExportedBatchSequence()
		{
			AssertNull("Should Be Null Until Not Exported", Header.ExportedBatchSequence);
			var batchSequence = TestObjectCreator.CreateGenExportBatchSequenceHeader(100, Header.PK, 0);
			TestObjectCreator.CreateGenExportBatchSequenceHeader(100, ZGuid.NewZGuid(), 0);
			AssertEquals("Should Have Header PK", batchSequence.PK, Header.ExportedBatchSequence.PK);
		}

		public virtual void TestDeleteTransactionInDBCausesException()
		{
			SetupForSave();

			AssertEquals(Header.AllowDelete_ForTestOnly, Header.CanDelete);
			AssertEquals(Header.AllowDelete_ForTestOnly, Header.ReasonForNotAbleToDelete.IsEmpty);

			if (Header.AllowDelete_ForTestOnly)
			{
				Assert(true);
			}
			else
			{
				Header.Factory.Save();
				bool exceptionCaught = false;
				try
				{
					Header.Delete();
				}
				catch (NotSupportedException)
				{
					exceptionCaught = true;
				}
				Assert("Exception should have been caught.", exceptionCaught);
			}
		}

		public void TestGovernmentInvoiceMenuPK()
		{
			SetupForSave();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			StmMenuItem menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "DocBuilder Invoice"));
			complianceSequence.XD_SU_MenuItem = menu.PK;
			Factory.Save();

			Header.Branch.SetCountry(Core.Constants.CountryCodes.Peru);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			Header.AH_TransactionReference = "abc-0010050";
			AssertEquals("MenuPK", complianceSequence.XD_SU_MenuItem, Header.GovernmentInvoiceMenuPK);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("MenuPK", ZGuid.Empty, Header.GovernmentInvoiceMenuPK);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.China);
			Header.AH_ComplianceSubType = ZString.Empty;
			var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Class A Invoice Preprinted"));
			AccountingConfigurationRegistry.Instance.ARLocalInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid());
			AssertEquals("China is special, it will get from local invoice registry", menuItem.PK, Header.GovernmentInvoiceMenuPK);
		}

		[TestDate(2012, 11, 11)]
		public void TestTransactionHeaderComplianceSequence()
		{
			SetupForSave();
			Factory.Save();

			AssertEquals("should be empty with null ComplianceSequence", string.Empty, Header.ComplianceSequenceWithCodeAndDesc);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_Description = "BBB";
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Header.AH_XD_ComplianceBook = complianceSequence.PK;
			Factory.Save();

			AssertEquals("should join code and description with ComplianceSequence", "AAA - BBB", Header.ComplianceSequenceWithCodeAndDesc);
		}

		public void TestGovernmentInvoiceMenuName()
		{
			SetupForSave();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			StmMenuItem menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "ARInvoice PE Factura"));
			complianceSequence.XD_SU_MenuItem = menu.PK;
			Factory.Save();

			Header.Branch.SetCountry(Core.Constants.CountryCodes.Peru);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			Header.AH_TransactionReference = "abc-0010050";
			AssertEquals("MenuName", "ARInvoice PE Factura", Header.GovernmentInvoiceMenuName);

			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.Australia);
			Header.Branch.Company.Factory.Save();
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.NotEqual, "Class A Invoice Preprinted"));
			AccountingConfigurationRegistry.Instance.ARLocalInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid());
			AssertEquals("When overridden, should always return the configured value", menuItem.SU_MenuName, Header.GovernmentInvoiceMenuName);

			Header.AH_TransactionReference = "";
			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.Peru);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			AssertEquals("ARInvoice PE Factura", Header.GovernmentInvoiceMenuName);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
			AssertEquals("ARInvoice_PE_Nota_De_Credito", Header.GovernmentInvoiceMenuName);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
			AssertEquals("ARInvoice_PE_Nota_De_Debito", Header.GovernmentInvoiceMenuName);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TBO;
			try
			{
				var menuname = Header.GovernmentInvoiceMenuName;
				Fail("Shouldn't reach this line as exception must be thrown already");
			}
			catch (DefaultGovernmentInvoiceMenuNotFoundException ex)
			{
				AssertEquals("Default government invoice menu does not exist for sub type TBO in PE", ex.UserFriendlyMessage);
			}

			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.VietNam);
			Header.Branch.Company.Factory.Save();
			Header.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			AssertEquals("VN Govt Tax Invoice", Header.GovernmentInvoiceMenuName);
			Header.AH_ComplianceSubType = "ABC";
			try
			{
				var menuname = Header.GovernmentInvoiceMenuName;
				Fail("Shouldn't reach this line as exception must be thrown already");
			}
			catch (DefaultGovernmentInvoiceMenuNotFoundException ex)
			{
				AssertEquals("Default government invoice menu does not exist for sub type ABC in VN", ex.UserFriendlyMessage);
			}

			var indonesiaComplianceSubTypes = new List<string>
			{
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.TXI,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T01,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T02,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T03,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T04,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T06,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T07,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T08,
				IndonesiaComplianceInfo.ComplianceSubTypeCodes.T09,
			};
			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.Indonesia);
			foreach (var subtype in indonesiaComplianceSubTypes)
			{
				AssertNoExceptionThrown(() =>
				{
					Header.AH_ComplianceSubType = subtype;
					AssertEquals("ARInvoice ID FakturPajak", Header.GovernmentInvoiceMenuName);
				});
			}
			Header.AH_ComplianceSubType = "ABC";
			try
			{
				var menuname = Header.GovernmentInvoiceMenuName;
				Fail("Shouldn't reach this line as exception must be thrown already");
			}
			catch (DefaultGovernmentInvoiceMenuNotFoundException ex)
			{
				AssertEquals("Default government invoice menu does not exist for sub type ABC in ID", ex.UserFriendlyMessage);
			}

			menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Class A Invoice Preprinted"));
			AccountingConfigurationRegistry.Instance.ARLocalInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuItem.PK.ToGuid());
			Header.Branch.Company.SetCountry(Core.Constants.CountryCodes.China);
			Header.Branch.Company.Factory.Save();
			Header.AH_ComplianceSubType = ZString.Empty;
			AssertEquals("China is special, it will get from local invoice registry", "Class A Invoice Preprinted", Header.GovernmentInvoiceMenuName);

			complianceSequence.XD_SU_MenuItem = ZGuid.Empty;
			Factory.Save();
			Header.Branch.SetCountry(Core.Constants.CountryCodes.Peru);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			Header.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			Header.AH_TransactionReference = "abc-0010050";
			try
			{
				var menuname = Header.GovernmentInvoiceMenuName;
				Fail("Shouldn't reach this line as exception must be thrown already");
			}
			catch (ComplianceSequenceHasNoDocumentMenuDefinedException ex)
			{
				AssertEquals(@"No Compliance Invoice Document will be Printed.
 This Compliance Book is not configured for printing.
 If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", ex.UserFriendlyMessage);
			}
		}

		public virtual void TestDeleteTransationHeaderInDatabase()
		{
			DirectDebitBatchHeader header = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			Factory.Save();

			AssertEquals(false, header.AllowDelete_ForTestOnly);

			if (!string.IsNullOrEmpty(Header.ReasonForNotAbleToDelete))
			{
				AssertEquals(Header.ReasonForNotAbleToDelete, "This transaction has been exported and cannot be deleted.");
			}
		}

		public void TestEInvoicingDetails()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TestObjectCreator.NonCurrentBranch.PK.ToGuid(), DateTime.Today.AddDays(-10)))
			{
				var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				batch.AIB_BatchNumber = 1;
				batch.AIB_EHubAllocatedNumber = "123";
				batch.AIB_GovernmentAllocatedNumber = "13579";
				var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
				pivot.AIP_Status = "QUE";
				pivot.AIP_ErrorDescription = "The transaction is invalid";
				pivot.AIP_LastResponseReceivedUtc = new ZDateTime(2017, 12, 16, 22, 18, 56);
				pivot.AIP_LastSentTimeUtc = new ZDateTime(2017, 10, 22, 18, 12, 44);
				pivot.AIP_ParentTableCode = "AH";
				pivot.AIP_AIB = batch.PK;
				pivot.AIP_ParentID = Header.PK;
				Factory.Save();

				Assert("EInvoicing details only available for countries supporting EInvoicing (eg, Turkey)", ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().DoesCountrySupportElectronicInvoicing(Header.Company.GC_RN_NKCountryCode));
				AssertEquals("EInvoicing Status should match", pivot.AIP_Status, Header.EInvoicingStatus);
				AssertEquals("EInvoicing Error should match", pivot.AIP_ErrorDescription, Header.EInvoicingError);
				AssertEquals("EInvoicing Last Response Received UTC should match", pivot.AIP_LastResponseReceivedUtc, Header.EInvoicingLastResponseReceivedUtc);
				AssertEquals("EInvoicing Last Sent Time UTC should match", pivot.AIP_LastSentTimeUtc, Header.EInvoicingLastSentTimeUtc);
				AssertEquals("E-Reporting Batch should match", batch.AIB_BatchNumber.ToString(), Header.EInvoicingBatchNumber);
				AssertEquals("E-Reporting eHub # should match", batch.AIB_EHubAllocatedNumber, Header.EInvoicingeHubAllocatedNumber);
				AssertEquals("E-Reporting Govt # should match", batch.AIB_GovernmentAllocatedNumber, Header.EInvoicingGovernmentAllocatedNumber);

				AssertEquals("EInvoicingStatus should be ReadOnly", true, Header.EInvoicingStatusInfo.ReadOnly);
				AssertEquals("EInvoicingError should be ReadOnly", true, Header.EInvoicingErrorInfo.ReadOnly);
				AssertEquals("EInvoicingLastResponseReceivedUtc should be ReadOnly", true, Header.EInvoicingLastResponseReceivedUtcInfo.ReadOnly);
				AssertEquals("EInvoicingLastSentTimeUtc should be ReadOnly", true, Header.EInvoicingLastSentTimeUtcInfo.ReadOnly);
				AssertEquals("EInvoicingBatchNumber should be ReadOnly", true, Header.EInvoicingBatchNumberInfo.ReadOnly);
				AssertEquals("EInvoicingGovernmentAllocatedNumber should be ReadOnly", true, Header.EInvoicingGovernmentAllocatedNumberInfo.ReadOnly);
				AssertEquals("EInvoicingeHubAllocatedNumber should be ReadOnly", true, Header.EInvoicingeHubAllocatedNumberInfo.ReadOnly);

				pivot.Requeue();
				AssertEquals(Core.Constants.EInvoicingPivotState.Queued, Header.EInvoicingStatus);
				AssertEquals(ZString.Empty, Header.EInvoicingError);
				AssertEquals(ZDateTime.Empty, Header.EInvoicingLastResponseReceivedUtc);
				AssertEquals(ZDateTime.Empty, Header.EInvoicingLastSentTimeUtc);
				AssertEquals(ZString.Empty, Header.EInvoicingBatchNumber);
				AssertEquals(ZString.Empty, Header.EInvoicingeHubAllocatedNumber);
				AssertEquals(ZString.Empty, Header.EInvoicingGovernmentAllocatedNumber);
			}
		}

		#region TestLocalCurrencyDecimals

		public void TestLocalCurrencyDecimals()
		{
			var company = Factory.Load(typeof(GlbCompany), Header.AH_GC);

			AssertNotNull("Pre-condition", company);
			AssertNotEquals("Header.Company should not fall back to CurrentCompany.", RuntimeHelpers.GetHashCode(GlbCompany.CurrentCompany), RuntimeHelpers.GetHashCode(Header.Company));

			Header.Company.LocalCurrency.RX_SubUnitRatio = 0;
			AssertEquals("Decimals should be 0", 0, Header.LocalCurrencyDecimals);
			Header.Company.LocalCurrency.RX_SubUnitRatio = 100;
			AssertEquals("Decimals should be 2", 2, Header.LocalCurrencyDecimals);

			Header.AH_GC = ZGuid.Invalid;

			AssertNull("Pre-condition", Factory.Load(typeof(GlbCompany), Header.AH_GC));
			AssertEquals("Header.Company should fall back to CurrentCompany.", RuntimeHelpers.GetHashCode(GlbCompany.CurrentCompany), RuntimeHelpers.GetHashCode(Header.Company));

			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals("Decimals should be 0", 0, Header.LocalCurrencyDecimals);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				AssertEquals("Decimals should be 2", 2, Header.LocalCurrencyDecimals);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		#endregion

		#region TestOSCurrencyDecimals

		public void TestOSCurrencyDecimals()
		{
			Header.AH_RX_NKTransactionCurrency = string.Empty;
			AssertNull(Header.TransactionCurrency);
			AssertEquals("Should be 2 when no currency specified", 2, Header.OSCurrencyDecimals);

			Header.AH_RX_NKTransactionCurrency = "IDR";
			AssertEquals("Decimals should be 0", 0, Header.OSCurrencyDecimals);
			Header.AH_RX_NKTransactionCurrency = "USD";
			AssertEquals("Decimals should be 2", 2, Header.OSCurrencyDecimals);
		}

		#endregion

		public void TestExchangeRateDecimalPlaces()
		{
			ZBool origIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				AssertEquals("Number of ex rate decimal places = 6", 6, Header.ExchangeRateDecimalPlaces);
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				AssertEquals("Number of ex rate decimal places = 6", 6, Header.ExchangeRateDecimalPlaces);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = origIsReciprocal;
			}
		}

		public void TestZExchangeRateObject()
		{
			Assert("The ZExchangeRate object should have type ZAccExchangeRate", Header.ExchangeRate is ZAccExchangeRate);
		}

		public virtual void TestExchangeRateTypeDependsOnLedgerAndType()
		{
			AssertEquals("The type of exchange rate should depend on the ledger", Header.Ledger_ForTestOnly == LedgerTypes.AccountsPayable ||
															Header.Ledger_ForTestOnly == LedgerTypes.UnapprovedPayableTransactions ||
															Header.Ledger_ForTestOnly == LedgerTypes.TransactionsPendingAllocation ?
															ExchangeRateType.Buy : ExchangeRateType.Sell, Header.ExchangeRate.Type);
		}

		public void TestSetReversingCodeStoreValueInReceiptType()
		{
			SetupForSave();
			if (Header.AH_TransactionType == TransactionTypes.CreditNote ||
				Header.AH_TransactionType == TransactionTypes.Invoice ||
				Header.AH_TransactionType == TransactionTypes.AdjustmentNote)
			{
				Assert("PreCondition - AH_ReceiptType is empty", Header.AH_ReceiptType.IsEmpty);
				Header.ReversingCode = "ABC";
				AssertEquals("AH_ReceiptType should have value ABC", "ABC", Header.AH_ReceiptType);
			}
			else
			{
				var originalValue = Header.AH_ReceiptType;
				Header.ReversingCode = "ABC";
				AssertEquals("AH_ReceiptType should not be changed", originalValue, Header.AH_ReceiptType);
			}
		}

		public void TestSetReversingCodeStoreValueInReceiptType_RefDatesEmpty()
		{
			if (Header.AH_TransactionType == TransactionTypes.CreditNote ||
				Header.AH_TransactionType == TransactionTypes.Invoice ||
				Header.AH_TransactionType == TransactionTypes.AdjustmentNote)
			{
				using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
				{
					SetupForSave();
					Assert("PreCondition - AH_ReceiptType is empty", Header.AH_ReceiptType.IsEmpty);

					Header.ReversingCode = "ABC";
					AssertEquals("AH_ReceiptType should be not empty", "ABC", Header.AH_ReceiptType);
					Assert("AH_OriginalReferenceStartDate should be empty", Header.AH_OriginalReferenceStartDate.IsEmpty);
					Assert("AH_OriginalReferenceEndDate should be empty", Header.AH_OriginalReferenceEndDate.IsEmpty);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestGetNewValidationReturnsNonEmptyValidationForInCompletingInvoiceContext()
		{
			var invoice01 = Factory.NewWithValidTestData<APInvoice>();
			invoice01.AH_Ledger = LedgerTypes.IncompleteTransactions;
			invoice01.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			var invoice02 = Factory.NewWithValidTestData<APCreditNote>();
			invoice02.AH_Ledger = LedgerTypes.IncompleteTransactions;
			invoice02.AH_TransactionType = TransactionTypes.IncompleteCreditNote;
			var invoice03 = Factory.NewWithValidTestData<APAdjustmentNote>();
			invoice03.AH_Ledger = LedgerTypes.IncompleteTransactions;
			invoice03.AH_TransactionType = TransactionTypes.IncompleteAdjustmentNote;

			Factory.Save();

			invoice01.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice01.AH_TransactionType = TransactionTypes.Invoice;
			invoice02.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice02.AH_TransactionType = TransactionTypes.CreditNote;
			invoice03.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice03.AH_TransactionType = TransactionTypes.AdjustmentNote;

			Assert(TypeOfEmptyValidation != invoice01.GetNewValidation_ForTestOnly().GetType());
			Assert(TypeOfEmptyValidation != invoice02.GetNewValidation_ForTestOnly().GetType());
			Assert(TypeOfEmptyValidation != invoice03.GetNewValidation_ForTestOnly().GetType());
		}

		public void TestGetNewValidationReturnsValidationForDeriectDebitBatch()
		{
			var header = Header as DirectDebitBatchHeader;
			if (header != null)
			{
				AssertEquals(true, header.ShouldValidateDirectDebitBatchComponent);
				var validationToTest = header.GetNewValidation_ForTestOnly();
				Assert(TypeOfEmptyValidation != validationToTest.GetType());

				Factory.Save();
				AssertEquals(false, header.ShouldValidateDirectDebitBatchComponent);
				validationToTest = header.GetNewValidation_ForTestOnly();
				Assert(TypeOfEmptyValidation == validationToTest.GetType());
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2016, 2, 1)]
		public virtual void TestGLJournalGetValidValidationWhileAH_PostDateHasChanges()
		{
			var glJournal = Header as GLJournal;
			if (glJournal != null)
			{
				//Validation before save
				AssertEquals("Precondition: Valid Validation", TypeOfValidation, glJournal.Validation.GetType());
				AssertNoErrors("Precondition: Valid Period", glJournal.PostPeriodInfo);

				glJournal.PostPeriod = PreviousGLClosedPeriod.AM_Period;
				AssertHasErrors("Set to a Closed Period", glJournal.PostPeriodInfo);

				glJournal.PostPeriod = PreviousOpenPeriod.AM_Period;
				AssertNoErrors("Set to an Open Period", glJournal.PostPeriodInfo);
				Factory.Save();

				//Validation after save
				AssertEquals("Use valid validation since the period is open", TypeOfValidation, glJournal.Validation.GetType());
				glJournal.PostPeriod = PreviousGLClosedPeriod.AM_Period;
				Assert(glJournal.AH_PostDateInfo.HasChanges);
				AssertHasErrors("Set to a Closed Period", glJournal.PostPeriodInfo);

				var newFactory = new BusinessObjectFactory();
				var periodManager = new PeriodManager(newFactory);

				//Close PreviousOpenPeriod
				do
				{
					periodManager.CloseSubLedgerPeriod();
					periodManager.CloseGLPeriod();
				} while (!PreviousOpenPeriod.AM_IsSubLedgerClosed || !PreviousOpenPeriod.AM_IsGeneralLedgerClosed);

				//Validation after period closed
				var glJournalInNewFactory = newFactory.Load<GLJournal>(glJournal.PK);
				Assert(!glJournalInNewFactory.AH_PostDateInfo.HasChanges);
				AssertEquals("Use empty validation while reopen a Journal in closed period", TypeOfEmptyValidation, glJournalInNewFactory.Validation.GetType());
				AssertNoErrors("Empty Validation", glJournalInNewFactory.PostPeriodInfo);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestIsInPeriodInvoiceContext()
		{
			TransactionHeader header = (TransactionHeader)PrepareTransactionHeaderForTest();
			if (header is InvoicingBase)
			{
				PeriodicInvoiceMiscInvoiceCollection colx = new PeriodicInvoiceMiscInvoiceCollection(Factory);
				colx.Add(header);
				Assert("When part of a misc invoice colx, validation should be periodic invoice item validation", header.Validation is PeriodicInvoiceItemValidation);
			}
			else
			{
				Assert(!header.IsInInvoiceBatchContext_ForTestOnly);
				AccTransactionHeaderValidation validationToTest = header.GetNewValidation_ForTestOnly();
				Assert(typeof(PeriodicInvoiceItemValidation) != validationToTest.GetType());
			}
		}

		public void TestInvoicingBatchHeaderContext()
		{
			Assert("Transaction Should not be in Invoice Batch Header Context", !Header.IsInInvoiceBatchContext_ForTestOnly);

			InvoiceBatchLineCollection testCollection = new InvoiceBatchLineCollection(Factory);
			testCollection.Add(Header);

			if (Header is InvoicingBase)
			{
				Assert(Header.IsInInvoiceBatchContext_ForTestOnly);
				AccTransactionHeaderValidation validationToTest = Header.GetNewValidation_ForTestOnly();
				AssertEquals(typeof(InvoiceBatchLineValidation), validationToTest.GetType());
			}
			else
			{
				Assert(!Header.IsInInvoiceBatchContext_ForTestOnly);
				AccTransactionHeaderValidation validationToTest = Header.GetNewValidation_ForTestOnly();
				Assert(typeof(AccTransactionHeaderValidation) != validationToTest.GetType());
			}
		}

		public void TestIsAllowedToSetExchangeRate()
		{
			Header.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			Header.AH_PostedToEFT = true;
			Assert(Header.IsAllowedToSetExchangeRate);

			Header.AH_PostedToEFT = false;
			Assert(Header.IsAllowedToSetExchangeRate);

			var foreignCurrency = TestObjectCreator.USD.Code;
			Header.AH_RX_NKTransactionCurrency = foreignCurrency;
			Assert(!Header.IsAllowedToSetExchangeRate);

			Header.AH_PostedToEFT = true;
			if (Header.AH_PostedToEFT)
			{
				Assert(Header.IsAllowedToSetExchangeRate);
			}
		}

		public void TestGetNewValidationReturnsEmptyValidationForExistingReceiptWithMatchingTransaction()
		{
			var receipt1 = TestObjectCreator.CreateARReceipt(ReceiptTypes.DirectCredit, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("000101", TestObjectCreator.AUD, 1.0M, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, -100m, 0m, 0m, -100m, 0m, 0m);
			receipt1.MatchingBaseObject.MatchedTransactions.Add(invoice);

			var receipt2 = TestObjectCreator.CreateARReceipt(ReceiptTypes.DirectCredit, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);

			Factory.Save();

			var receipt3 = TestObjectCreator.CreateARReceipt(ReceiptTypes.DirectCredit, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("000102", TestObjectCreator.AUD, 1.0M, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, -100m, 0m, 0m, -100m, 0m, 0m);
			receipt3.MatchingBaseObject.MatchedTransactions.Add(invoice3);
			receipt3.UseReceiptValidation = true;

			var validationToTest = receipt1.GetNewValidation_ForTestOnly();
			AssertNotNull("Persistent receipt with matching collection", receipt1.ParentMatchingCollection_ForTestOnly);
			AssertEquals("Persistent receipt with matching collection", MatchingCollectionTypes.MatchedTransactions, receipt1.ParentMatchingCollection_ForTestOnly.MatchingCollectionType);
			AssertEquals("Persistent receipt with matching collection should get matching validation", typeof(MatchingValidation), validationToTest.GetType());

			validationToTest = receipt2.GetNewValidation_ForTestOnly();
			AssertNull("Persistent receipt without matching collection", receipt2.ParentMatchingCollection_ForTestOnly);
			AssertEquals("Persistent receipt without matching collection also should get empty validation", typeof(TransactionHeaderEmptyValidation), validationToTest.GetType());

			validationToTest = receipt3.GetNewValidation_ForTestOnly();
			AssertNotNull("Non-Persistent receipt with matching collection", receipt3.ParentMatchingCollection_ForTestOnly);
			AssertEquals("Non-Persistent receipt with matching collection", MatchingCollectionTypes.MatchedTransactions, receipt3.ParentMatchingCollection_ForTestOnly.MatchingCollectionType);
			AssertEquals("Non-Persistent receipt with matching collection should get receipt validation", typeof(ReceiptValidation), validationToTest.GetType());

			var newFactory = new BusinessObjectFactory();
			var loadedReceipt1 = newFactory.Load<ARReceipt>(receipt1.PK);
			var loadedReceipt2 = newFactory.Load<ARReceipt>(receipt2.PK);

			validationToTest = loadedReceipt1.GetNewValidation_ForTestOnly();
			AssertNull("Loaded receipt without matching collection", loadedReceipt1.ParentMatchingCollection_ForTestOnly);
			AssertEquals("Loaded receipt without matching collection also should get empty validation", typeof(TransactionHeaderEmptyValidation), validationToTest.GetType());

			validationToTest = loadedReceipt2.GetNewValidation_ForTestOnly();
			AssertNull("Loaded receipt without matching collection", loadedReceipt2.ParentMatchingCollection_ForTestOnly);
			AssertEquals("Loaded receipt without matching collection also should get empty validation", typeof(TransactionHeaderEmptyValidation), validationToTest.GetType());

			AssertNotNull("Touching MatchingBaseObject will create ParentMatchingCollection", loadedReceipt1.MatchingBaseObject);
			AssertNotNull("Touching MatchingBaseObject will create ParentMatchingCollection", loadedReceipt1.ParentMatchingCollection_ForTestOnly);
			validationToTest = loadedReceipt1.GetNewValidation_ForTestOnly();
			AssertEquals("Loaded receipt with matching collection Will get matching validation", typeof(MatchingValidation), validationToTest.GetType());

			AssertNotNull("Touching MatchingBaseObject will create ParentMatchingCollection", loadedReceipt2.MatchingBaseObject);
			AssertNotNull("Touching MatchingBaseObject will create ParentMatchingCollection", loadedReceipt2.ParentMatchingCollection_ForTestOnly);
			validationToTest = loadedReceipt2.GetNewValidation_ForTestOnly();
			AssertEquals("Loaded receipt with matching collection Will get matching validation", typeof(MatchingValidation), validationToTest.GetType());
		}

		public virtual void TestPaymentMethods()
		{
			AssertEquals("Payment Methods list should be of valid type", OLookUpEditType.PaymentMethod, Header.PaymentMethods.LookupEditType);
			CodeDescriptionPairList testPaymentMethodsList = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
			AssertEquals("Both test and real lists should contain same members", testPaymentMethodsList.ElementsAsString, Header.PaymentMethods.ElementsAsString);
		}

		public void TestUserAllowedToBackPost()
		{
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals("UserAllowedToBackPost", false, Header.UserAllowedToBackPost);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("UserAllowedToBackPost - AccountsReceivable", Header.AH_Ledger == LedgerTypes.AccountsReceivable, Header.UserAllowedToBackPost);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("UserAllowedToBackPost - AccountsPayable",
				Header.AH_Ledger == LedgerTypes.AccountsPayable ||
				Header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
				Header.AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||
				Header.AH_Ledger == LedgerTypes.IncompleteTransactions,
				Header.UserAllowedToBackPost);

			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("UserAllowedToBackPost - CashBook", Header.AH_Ledger == LedgerTypes.CashBook, Header.UserAllowedToBackPost);

			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("UserAllowedToBackPost - JobCosting", Header.AH_Ledger == LedgerTypes.JobCosting, Header.UserAllowedToBackPost);
		}

		public void TestUserAllowedToFuturePost()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Header.AllowFuturePosting);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, Header.AllowFuturePosting);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Header.AllowFuturePosting);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			bool isApplicable =
				(
					(Header.Ledger_ForTestOnly == LedgerTypes.AccountsPayable || Header.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable) &&
					(Header.AH_TransactionType == TransactionTypes.Payment || Header.AH_TransactionType == TransactionTypes.Receipt || Header.AH_TransactionType == TransactionTypes.ReceiptBatch)
				) ||
				(
					(Header.Ledger_ForTestOnly == LedgerTypes.CashBook) &&
					(Header.AH_TransactionType == TransactionTypes.DirectPayment || Header.AH_TransactionType == TransactionTypes.DirectReceipt)
				);

			AssertEquals("Should be true for supported transactions.", isApplicable, Header.AllowFuturePosting);
		}

		public void TestAllowFuturePostingForReceiptPaymentBox()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Header.AllowFuturePostingForReceiptPaymentOnInvoice);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, Header.AllowFuturePostingForReceiptPaymentOnInvoice);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Header.AllowFuturePostingForReceiptPaymentOnInvoice);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			bool isApplicable = Header.Ledger_ForTestOnly == LedgerTypes.AccountsPayable || Header.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable ||
				(
					(Header.Ledger_ForTestOnly == LedgerTypes.CashBook) &&
					(Header.AH_TransactionType == TransactionTypes.DirectPayment || Header.AH_TransactionType == TransactionTypes.DirectReceipt)
				);

			AssertEquals("Should be true for supported transactions.", isApplicable, Header.AllowFuturePostingForReceiptPaymentOnInvoice);
		}

		public virtual void TestAH_PostDate_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			bool futureAndBackPostingIsAlwaysAllowed = Header.Ledger_ForTestOnly == LedgerTypes.TransactionsPendingAllocation;

			if (futureAndBackPostingIsAlwaysAllowed)
			{
				AssertEquals("Allowed to post future and past so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AH_PostDate_ReadOnly", Header));
			}
			else
			{
				AssertEquals("Not allowed to post future or past, so read only", true, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AH_PostDate_ReadOnly", Header));
			}

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			bool futurePostingIsAllowed =
				(
					(Header.Ledger_ForTestOnly == LedgerTypes.AccountsPayable || Header.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable) &&
					(Header.AH_TransactionType == TransactionTypes.Payment || Header.AH_TransactionType == TransactionTypes.Receipt || Header.AH_TransactionType == TransactionTypes.ReceiptBatch)
				) ||
				(
					(Header.Ledger_ForTestOnly == LedgerTypes.CashBook) &&
					(Header.AH_TransactionType == TransactionTypes.DirectPayment || Header.AH_TransactionType == TransactionTypes.DirectReceipt)
				);

			if (futurePostingIsAllowed || futureAndBackPostingIsAlwaysAllowed)
			{
				AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AH_PostDate_ReadOnly", Header));
			}
			else
			{
				AssertEquals("Not allowed to post future or past, so read only", true, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AH_PostDate_ReadOnly", Header));
			}

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			bool pastPostingIsAllowed = Header.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable ||
				Header.Ledger_ForTestOnly == LedgerTypes.AccountsPayable ||
				Header.Ledger_ForTestOnly == LedgerTypes.UnapprovedPayableTransactions ||
				Header.Ledger_ForTestOnly == LedgerTypes.TransactionsPendingAllocation ||
				Header.Ledger_ForTestOnly == LedgerTypes.IncompleteTransactions ||
				Header.Ledger_ForTestOnly == LedgerTypes.CashBook ||
				Header.Ledger_ForTestOnly == LedgerTypes.JobCosting;

			if (pastPostingIsAllowed || futurePostingIsAllowed)
			{
				AssertEquals("Allowed to post past so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AH_PostDate_ReadOnly", Header));
			}
			else
			{
				AssertEquals("Not allowed to post future or past, so read only", true, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AH_PostDate_ReadOnly", Header));
			}
		}

		public void TestAH_PostDateMinOrMaxValues()
		{
			if (Header is DepositBatchTransactionLine || Header is GLJournal)
			{
				Assert("Min/Max due date not run on this transaction type, no test required", true);
			}
			else
			{
				try
				{
					Header.AH_PostDate = ZDateTime.MinSmallDateTimeValue.AddDays(-5);
					AssertHasErrorContaining(Header.AH_PostDateInfo, "is more than 10 years old and thus is not valid");
					Header.AH_PostDate = ZDateTime.Today;
					AssertEquals("should have updated to todays date", ZDateTime.Today, Header.AH_PostDate);
					AssertNoErrors("should have no errors", Header.AH_PostDateInfo);
					Header.AH_PostDate = ZDateTime.MaxSmallDateTimeValue.AddDays(5);
					AssertHasErrorContaining(Header.AH_PostDateInfo, "is more than 5 years from now and thus is not valid.");
				}
				catch (Exception ex)
				{
					Fail("Should not throw exception: " + ex.Message);
				}
			}
		}

		public void TestAH_DueDateMinOrMaxValues()
		{
			if (Header is DepositBatchTransactionLine || Header is GLJournal)
			{
				Assert("Min/Max due date not run on this transaction type, no test required", true);
			}
			else
			{
				try
				{
					Globals.IsUserInteractive = false;
					Header.AH_DueDate = ZDateTime.MinSmallDateTimeValue.AddDays(-5);
					AssertHasErrorContaining(Header.AH_DueDateInfo, "is more than 10 years old and thus is not valid");
					Header.AH_DueDate = ZDateTime.Today;
					AssertEquals("should have updated to todays date", ZDateTime.Today, Header.AH_DueDate);
					AssertNoErrors("should have no errors", Header.AH_DueDateInfo);
					Header.AH_DueDate = ZDateTime.MaxSmallDateTimeValue.AddDays(5);
					AssertHasErrorContaining(Header.AH_DueDateInfo, "is more than 5 years from now and thus is not valid.");
				}
				catch (Exception ex)
				{
					Fail("Should not throw exception: " + ex.Message);
				}
			}
		}

		public void TestUserAllowedToBackPostForIncompleteInvoice()
		{
			Header.AH_Ledger = LedgerTypes.IncompleteTransactions;
			TestUserAllowedToBackPost();
		}

		public void TestAllowBackPosting()
		{
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Header.AllowBackPosting);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, Header.AllowBackPosting);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Header.AllowBackPosting);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			bool isApplicable =
				Header.AH_Ledger == LedgerTypes.AccountsReceivable ||
				Header.AH_Ledger == LedgerTypes.AccountsPayable ||
				Header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
				Header.AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||
				Header.AH_Ledger == LedgerTypes.IncompleteTransactions ||
				Header.AH_Ledger == LedgerTypes.CashBook ||
				Header.AH_Ledger == LedgerTypes.JobCosting;
			AssertEquals("Should be true for supported transactions.", isApplicable, Header.AllowBackPosting);
		}

		public void TestAllowBackPostingForIncompleteInvoice()
		{
			Header.AH_Ledger = LedgerTypes.IncompleteTransactions;
			TestAllowBackPosting();
		}

		public void TestAH_OSOutstandingAmountWithoutMultiplier_Case2()
		{
			TransactionHeader header = (TransactionHeader)PrepareTransactionHeaderForTest();
			header.AH_OSExTaxAmount = 10m;
			if (header.AH_OutstandingAmount != 0)
			{
				AssertEquals(header.AH_OSTotal, header.AH_OSOutstandingAmountWithoutMultiplier);
			}
			else
			{
				Assert("Value N/A for " + header.TransactionType_ForTestOnly, true);
			}
		}

		public void TestAH_OSTax()
		{
			InvoicingBase invoiceAR = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			TestObjectCreator.CreateInvoiceLine(invoiceAR, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m);
			Factory.Save();

			AssertEquals("AR  AH_OSTaxAmount", 10m, invoiceAR.AH_OSTaxAmount);
			AssertEquals("AR AH_OSTotalAmount", 110m, invoiceAR.AH_OSTotalAmount);
			AssertEquals("AR AH_GSTAmount", 10m, invoiceAR.AH_GSTAmount);
			AssertEquals("AR AH_OSTax", 10m, invoiceAR.AH_OSTax);

			InvoicingBase invoiceAP = TestObjectCreator.CreateInvoice(typeof(APInvoice), "testAP", TestObjectCreator.AUD, 1);
			TestObjectCreator.CreateInvoiceLine(invoiceAP, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m);
			Factory.Save();

			AssertEquals("AP AH_OSTaxAmount", 10m, invoiceAP.AH_OSTaxAmount);
			AssertEquals("AP AH_OSTotal", -110m, invoiceAP.AH_OSTotal);
			AssertEquals("AP AH_GSTAmount", -10m, invoiceAP.AH_GSTAmount);
			AssertEquals("AP AH_OSTax", -10m, invoiceAP.AH_OSTax);
		}

		#region TestGenerateReverseTransaction

		public virtual void TestGenerateReverseTransaction()
		{
			int reverseTransactionMultiplier = Header.InvertSignsOfOriginalTransactionOnReversing_ForTestOnly ? -1 : 1;

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			SetupHeaderForReversingCore(875M, 89M);
			using (Header.AmountsCalculationsSuspender.GetSuspender())
			{
				Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
				Header.AH_ExchangeRate = 0.8M;
				Header.AH_OSExTaxAmount = 875M;
				Header.AH_OSTaxAmount = 89M;
				Header.AH_LocalExTaxAmount = 630M;  // Incorrect Local Amount but should be copied over to reversing Transaction
				Header.AH_LocalTaxAmount = 64M; // Incorrect LocalTax Amount
				Header.AH_OutstandingAmount = 100M;
			}

			Header.GenerateReverseTransaction(true);
			TransactionHeader reversedHeader = (TransactionHeader)Header.ReverseTransaction;
			AssertNotNull("Reversed transaction should not be null", reversedHeader);
			Assert(reversedHeader.IsReverseTransaction);
			AssertEquals(Header, reversedHeader.OriginalTransaction);

			// Check the DB values
			AssertEquals(Header.AH_AG, reversedHeader.AH_AG);
			AssertEquals(Header.AH_OH, reversedHeader.AH_OH);
			AssertEquals(Header.AH_AB, reversedHeader.AH_AB);
			AssertEquals(Header.AH_GB, reversedHeader.AH_GB);
			AssertEquals(Header.AH_GE, reversedHeader.AH_GE);
			AssertEquals(Header.AH_TransactionCategory, reversedHeader.AH_TransactionCategory);
			AssertEquals(Header.AH_JH, reversedHeader.AH_JH);
			AssertEquals(Header.AH_CashBasisGSTIndicator, reversedHeader.AH_CashBasisGSTIndicator);
			AssertEquals(Header.AH_ChequeDrawer, reversedHeader.AH_ChequeDrawer);
			AssertEquals(Header.AH_ChequeOrReference, reversedHeader.AH_ChequeOrReference);
			AssertEquals(Header.AH_DrawerBank, reversedHeader.AH_DrawerBank);
			AssertEquals(Header.AH_DrawerBranch, reversedHeader.AH_DrawerBranch);
			AssertEquals(Header.AH_InvoiceTerm, reversedHeader.AH_InvoiceTerm);
			AssertEquals(Header.AH_InvoiceTermDays, reversedHeader.AH_InvoiceTermDays);
			AssertEquals(Header.AH_ReceiptType, reversedHeader.AH_ReceiptType);
			AssertEquals(Header.AH_RX_NKTransactionCurrency, reversedHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(Header.AH_AgreedPaymentMethodOverride, reversedHeader.AH_AgreedPaymentMethodOverride);
			AssertEquals(Env.Time.CurrentLocalDateTime.Date, reversedHeader.AH_DueDate.Date);
			AssertEquals("AH_OSExTaxAmount on the ReversedHeader should be -1* original Header", reverseTransactionMultiplier * 875M, reversedHeader.AH_OSExTaxAmount);
			AssertEquals("AH_OSTaxAmount on the ReversedHeader should be -1* original Header", reverseTransactionMultiplier * 89M, reversedHeader.AH_OSTaxAmount);
			AssertEquals("AH_LocalExTaxAmount on the ReversedHeader should be -1* original Header", reverseTransactionMultiplier * 630M, reversedHeader.AH_LocalExTaxAmount);
			AssertEquals("AH_LocalTaxAmount on the ReversedHeader should be -1* original Header", reverseTransactionMultiplier * 64M, reversedHeader.AH_LocalTaxAmount);
			AssertEquals("AH_OSTotalAmount on ReversedHeader should be -1* original Header", reverseTransactionMultiplier * 964M, reversedHeader.AH_OSTotalAmount);
			AssertEquals("Exchange Rate should be the same", 0.8M, reversedHeader.AH_ExchangeRate);
			AssertEquals("AH_LocalOutstandingAmount on ReversedHeader should be calculated from original Header", GetExpectedOutstandindAmountForReversing(694m) * reverseTransactionMultiplier, reversedHeader.AH_LocalOutstandingAmount);
			AssertEquals(Header.AH_PlaceOfSupply, reversedHeader.AH_PlaceOfSupply);
		}

		protected virtual ZDecimal GetExpectedOutstandindAmountForReversing(ZDecimal expectedValue) => expectedValue;

		public void TestOverrideAddressContactReversal()
		{
			var org = TestObjectCreator.CreateOrgHeader("TSTORG", false, false);
			var address = TestObjectCreator.CreateAddress(org);
			var contact = TestObjectCreator.CreateContact(org);

			Header.AH_OA_InvoiceAddressOverride = address.PK;
			Header.AH_OC_InvoiceContactOverride = contact.PK;

			Header.GenerateReverseTransaction(true);
			TransactionHeader reverseTransaction = (TransactionHeader)Header.ReverseTransaction;
			if (!(Header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || Header.AH_Ledger == LedgerTypes.General))
			{
				AssertEquals(Header.AH_OA_InvoiceAddressOverride, reverseTransaction.AH_OA_InvoiceAddressOverride);
				AssertEquals(Header.AH_OC_InvoiceContactOverride, reverseTransaction.AH_OC_InvoiceContactOverride);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region IMatching Property Tests

		public virtual void TestOSOutstandingAmountMatching()
		{
			if (GetExpectedBusinessObjectType() == typeof(DepositBatch))
			{
				//Cannot use batch created by GetNewBusinessObjectCore, because we create and save receipt inside that method, and in this unit test we want to change receipt amount from 0m to 10m.
				TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 10M, TestObjectCreator.AUDBankAccount.PK);
				Factory.Save();

				Header = Factory.New<DepositBatch>();
				Header.AH_AB = TestObjectCreator.AUDBankAccount.PK;
				((DepositBatch)Header).LoadTransactions(ZGuid.Empty);
				((DepositBatch)Header).IsSelected = true;
				Header.AH_LocalOutstandingAmount = -10M;
			}

			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Header.AH_ExchangeRate = 0.5M;
			Header.AH_OSExTaxAmount = 5M;   // LocalExTax should be 10
			Header.AH_LocalOutstandingAmount = 10M;
			var transactionTypesForCashBook = new List<string>() { TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt };
			if (Header.Ledger_ForTestOnly == LedgerTypes.CashBook && transactionTypesForCashBook.Contains(Header.TransactionType_ForTestOnly))
			{
				var multiplier = (Header.TransactionType_ForTestOnly == TransactionTypes.DirectPayment) ? -1 : 1;
				var lineAmount = (10m / 2) * multiplier;
				var oSAmount = (10m / 4) * multiplier;
				((TransactionHeaderWithLines)Header).Lines[0].AL_LineAmount = lineAmount;
				((TransactionHeaderWithLines)Header).Lines[0].AL_OSAmount = oSAmount;
				((TransactionHeaderWithLines)Header).Lines[1].AL_LineAmount = lineAmount;
				((TransactionHeaderWithLines)Header).Lines[1].AL_OSAmount = oSAmount;
			}
			AssertEquals("OSOutstandingAmountMatching should be 5 * Multiplier", 5M * Header.Multiplier_ForTestOnly, Header.OSOutstandingAmountMatching);

			if (Header is CashbookExchangeDiff)
			{
				Header.AH_LocalOutstandingAmount = 0M;
			}

			if (IsMiscellaneousTransaction)
			{
				Header.Delete(); // Cannot save Miscellaneous Transaction with a non zero outstanding Amount
			}

			Header = Factory.NewWithValidTestData<APPayment>();

			Header.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			Header.AH_ExchangeRate = 1M;
			Header.AH_OSTotalAmount = 10M;
			Header.AH_InvoiceAmount = 10M;
			Header.AH_LocalOutstandingAmount = 10M;
			Header.AH_OH = TestObjectCreator.ABIGAS.PK;

			AssertEquals("ExistingPaymentApprovalItems", 0, Header.ExistingPaymentApprovalItems.Count);

			IMatchingCollection matched = new IMatchingCollection(Factory);
			matched.Add(Header);

			PaymentApprovalBase rejectedApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			rejectedApproval.AV_OH = TestObjectCreator.ABIGAS.PK;
			PaymentApprovalItem approvalItem2 = Header.GetPaymentApprovalItem(rejectedApproval);
			approvalItem2.A2_PaymentThisRun = 10M * Header.Multiplier_ForTestOnly;
			rejectedApproval.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();
			Header.PaymentApprovalItemsField_ForTestOnly = null;

			AssertEquals("OSOutstandingAmountMatching should be 10 * Multiplier", 10M * Header.Multiplier_ForTestOnly, Header.OSOutstandingAmountMatching);
			AssertEquals("ExistingPaymentApprovalItems", 1, Header.ExistingPaymentApprovalItems.Count);

			PaymentApprovalBase currentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			currentApproval.AV_OH = TestObjectCreator.ABIGAS.PK;
			PaymentApprovalItem currentApprovalItem = Header.GetPaymentApprovalItem(currentApproval);
			currentApprovalItem.A2_PaymentThisRun = 10M * Header.Multiplier_ForTestOnly;
			currentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			Header.PaymentApprovalItemsField_ForTestOnly = null;
			AssertEquals("ExistingPaymentApprovalItems", 2, Header.ExistingPaymentApprovalItems.Count);

			matched.Add(currentApproval);

			AssertEquals("Should be PaymentApprovalPKCurrentlyBeingMatched", currentApproval.PK, Header.PaymentApprovalPKCurrentlyBeingMatched_ForTestOnly);
			AssertEquals("ExistingPaymentApprovalItems", 2, Header.ExistingPaymentApprovalItems.Count);
			var targetPaymentApprovalItemIndex = Header.ExistingPaymentApprovalItems[0].Approval.AV_Status == PaymentApprovalStatus.Rejected ? 1 : 0;
			AssertEquals("PaymentApprovalPKCurrentlyBeingMatched still in the ExistingPaymentApprovalItems, because it is not reloaded", currentApproval.PK, Header.ExistingPaymentApprovalItems[targetPaymentApprovalItemIndex].Approval.PK);

			PaymentApprovalBase postedApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			postedApproval.AV_OH = TestObjectCreator.ABIGAS.PK;
			PaymentApprovalItem approvalItem = Header.GetPaymentApprovalItem(postedApproval);
			approvalItem.A2_PaymentThisRun = 10M * Header.Multiplier_ForTestOnly;
			postedApproval.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();

			AssertEquals("OSOutstandingAmountMatching should be 10 * Multiplier", 10M * Header.Multiplier_ForTestOnly, Header.OSOutstandingAmountMatching);
			AssertEquals("ExistingPaymentApprovalItems", 2, Header.ExistingPaymentApprovalItems.Count);

			AccPaymentApproval paymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_Ledger = "AP";

			AccPaymentApprovalItem paymentApprovalItem = Factory.NewWithValidTestData<AccPaymentApprovalItem>();
			paymentApprovalItem.A2_AH = Header.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = 3M;
			Factory.Save();

			Header.PaymentApprovalItemsField_ForTestOnly = null;
			AssertEquals("ExistingPaymentApprovalItems reloaded", 3, Header.ExistingPaymentApprovalItems.Count);
			AssertEquals("OSOutstandingAmountMatching should be 7 * Multiplier", 7M * Header.Multiplier_ForTestOnly, Header.OSOutstandingAmountMatching);
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestOutstandingAmountMatchingWithApprovalItems()
		{
			IMatching headerAsIMatching = Header as IMatching;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery findAALSHIQuery = new ZQuery(OrgHeaderSchema.OH_Code, "AALSHI");
			OrgHeader aALSHI = newFactory.LoadTop1<OrgHeader>(findAALSHIQuery);
			aALSHI.CompanyData.OB_IsCreditor = true;
			newFactory.Save();

			SetupForOutstandingAmountMatchingWithApprovalItemsTest();
			Header.AH_OSExTaxAmount = 5M;

			AssertEquals("OSOutstandingAmountMatching should be 5 * Multiplier", 5M * Header.Multiplier_ForTestOnly, Header.OSOutstandingAmountMatching);
			AssertEquals("OutstandingAmountMatching should be 10 * Multiplier", 10M * Header.Multiplier_ForTestOnly, Header.OutstandingAmountMatching);

			if (headerAsIMatching != null)
			{
				AssertEquals("ExistingPaymentApprovalItems", 0, Header.ExistingPaymentApprovalItems.Count);

				IMatchingCollection matched = new IMatchingCollection(Factory);
				matched.Add(Header);

				PaymentApprovalBase currentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
				currentApproval.AV_OH = aALSHI.PK;
				PaymentApprovalItem currentApprovalItem = Header.GetPaymentApprovalItem(currentApproval);
				currentApprovalItem.A2_PaymentThisRun = 10M * Header.Multiplier_ForTestOnly;
				currentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;

				if (!IsMiscellaneousTransaction)
				{
					Factory.Save(); // Cannot save Miscellaneous Transaction with a non zero outstanding Amount
				}

				Header.PaymentApprovalItemsField_ForTestOnly = null;
				AssertEquals("ExistingPaymentApprovalItems", 1, Header.ExistingPaymentApprovalItems.Count);

				matched.Add(currentApproval);

				AssertEquals("Should be PaymentApprovalPKCurrentlyBeingMatched", currentApproval.PK, Header.PaymentApprovalPKCurrentlyBeingMatched_ForTestOnly);
				AssertEquals("PaymentApprovalPKCurrentlyBeingMatched still in the ExistingPaymentApprovalItems, because it is not reloaded", 1, Header.ExistingPaymentApprovalItems.Count);

				PaymentApprovalBase postedApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
				postedApproval.AV_OH = aALSHI.PK;
				PaymentApprovalItem approvalItem = Header.GetPaymentApprovalItem(postedApproval);
				approvalItem.A2_PaymentThisRun = 10M * Header.Multiplier_ForTestOnly;
				postedApproval.AV_Status = PaymentApprovalStatus.Posted;

				if (!IsMiscellaneousTransaction)
				{
					Factory.Save(); // Cannot save Miscellaneous Transaction with a non zero outstanding Amount
				}

				Header.OSOutstandingAmountMatchingField_ForTestOnly = 0m;
				Header.OutstandingAmountMatchingField_ForTestOnly = 0m;
				Header.PaymentApprovalItemsField_ForTestOnly = null;
				AssertEquals("OSOutstandingAmountMatching", 5M * Header.Multiplier_ForTestOnly, Header.OSOutstandingAmountMatching);
				AssertEquals("OutstandingAmountMatching", 10M * Header.Multiplier_ForTestOnly, Header.OutstandingAmountMatching);

				Header.OSOutstandingAmountMatchingField_ForTestOnly = 0m;
				Header.OutstandingAmountMatchingField_ForTestOnly = 0m;
				Header.PaymentApprovalItemsField_ForTestOnly = null;
				Header.AH_OutstandingAmount = 0m;
				AssertEquals("OSOutstandingAmountMatching should be 0", 0M, Header.OSOutstandingAmountMatching);
				AssertEquals("OutstandingAmountMatching should be 0", 0M, Header.OutstandingAmountMatching);
			}
		}

		public virtual void TestInvoiceBatchNumber()
		{
			Assert("InvoiceBatchNumber should be empty", Header.InvoiceBatchNumber.IsEmpty);
		}

		public void TestPaymentCurrentlyBeingMatched()
		{
			IMatching headerAsIMatching = Header as IMatching;

			if (headerAsIMatching != null)
			{
				IMatchingCollection matched = new IMatchingCollection(Factory);
				matched.Add(Header);

				if (Header is Payment)
				{
					AssertEquals("Payment in matching collection", Header.PK, Header.PaymentCurrentlyBeingMatched.PK);
				}
				else
				{
					AssertNull("No Payment in the matching collection", Header.PaymentCurrentlyBeingMatched);

					APPayment payment = Factory.New<APPayment>();
					matched.Add(payment);
					AssertEquals("Should be PaymentCurrentlyBeingMatched", payment.PK, Header.PaymentCurrentlyBeingMatched.PK);
				}
			}
			else
			{
				Assert("Only effects IMatching transactions", true);
			}
		}

		#endregion

		#region Local Amount Calculated Properties Tests

		public void TestLocalGSTAmountSetsAH_GSTAmountCorrectly()
		{
			Header.AH_LocalTaxAmount = 50.00m;
			ZDecimal rowAmount = (decimal)(((INeedRow)Header).Row[TransactionHeader.Schema.AH_GSTAmount]);
			AssertEquals("Multiplier should be applied correctly", 50.00m * Header.Multiplier_ForTestOnly, rowAmount);
			AssertEquals("Multiplier should be applied correctly on getting LocalGSTAmount", 50.00m, Header.AH_LocalTaxAmount);
		}

		#endregion

		#region Overseas Amount Tests

		public void TestAH_OSExTaxAmountCalculatesLocalAndBack()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 0m);

			AssertEquals("Local Amount", 351.67m, Header.AH_LocalExTaxAmount);

			AssertEquals("OS Ex Tax Amount", 200.453m, Header.AH_OSExTaxAmount);
		}

		public void TestOSTaxAmountCalculatesLocalAndBack()
		{
			SetupHeaderExRatesAndAmounts(0.5728m, 100, 0m, 53.75m);

			AssertEquals("Local Tax Amount", 93.84m, Header.AH_LocalTaxAmount);

			AssertEquals("OS Tax Amount", 53.75m, Header.AH_OSTaxAmount);
		}

		public void TestOSAmountUpdatesOnOSExTaxAndOSTax()
		{
			Header.AH_ExchangeRate = 1m;

			Header.AH_OSExTaxAmount = 50.00m;

			AssertEquals("OS Ex Tax Amount", 50.00m, Header.AH_OSExTaxAmount);

			SetupHeaderExRatesAndAmounts(1m, 100, 50.00m, 0m);

			AssertEquals("OS Total should update", 50.00m, Header.AH_OSTotalAmount);

			Header.AH_OSTaxAmount = 20.00m;
			AssertEquals("OS Total should update", 70.00m, Header.AH_OSTotalAmount);

			Header.AH_OSTaxAmount = 0;
			AssertEquals("OS Total should update", 50.00m, Header.AH_OSTotalAmount);
		}

		public virtual void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 15.02m);
			SetupForSave();
			Header.AH_OutstandingAmount = 378.02m * Header.Multiplier_ForTestOnly;
			PrepareMiscellaneousTransactionForSaving();

			Header.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionHeader loadedHeader = (TransactionHeader)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Header.PK);

			AssertEquals("OS Ex Tax Amount on Load", 200.453m, loadedHeader.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 15.02m, loadedHeader.AH_OSTaxAmount);
		}

		public void TestOSExTaxAmountIsRoundedOnSetting()
		{
			RefCurrency currency1DP = Factory.New<RefCurrency>();
			currency1DP.RX_Code = "NNN";
			currency1DP.RX_SubUnitRatio = 10;
			ZGuid currency = currency1DP.PK;
			Header.AH_RX_NKTransactionCurrency = currency1DP.RX_Code;
			Header.AH_OSExTaxAmount = 34.56m;
			AssertEquals("OSExTax", 34.6m, Header.AH_OSExTaxAmount);
			AssertEquals("OSTotalAmount", 34.6m, Header.AH_OSTotalAmount);
		}

		public void TestOSTaxAmountIsRoundedOnSetting()
		{
			RefCurrency currency2DP = Factory.New<RefCurrency>();
			currency2DP.RX_SubUnitRatio = 100;
			Header.AH_RX_NKTransactionCurrency = currency2DP.RX_Code;
			Header.AH_OSTaxAmount = 23.464m;
			AssertEquals("OSTaxAmount", 23.46m, Header.AH_OSTaxAmount);
			AssertEquals("OSTotalAmount", 23.46m, Header.AH_OSTotalAmount);
		}

		#endregion

		#region IReversingTest Members

		public void TestIsReversedImplementation()
		{
			Assert("Is Reversed should be false", !((IReversing)Header).IsReversed);
			Header.AH_IsCancelled = ZBool.True;
			Assert("Is Reversed should be true", ((IReversing)Header).IsReversed);
		}

		public void TestSetCancellationFlag()
		{
			AssertEquals("Cancellation flag should be N by default", ZBool.False, Header.AH_IsCancelled);
			((IReversing)Header).SetCancellationFlag(true);
			AssertEquals("Cancellation flag should be Y now", ZBool.True, Header.AH_IsCancelled);
			((IReversing)Header).SetCancellationFlag(false);
			AssertEquals("Cancellation flag should be N now", ZBool.False, Header.AH_IsCancelled);
		}

		public virtual void TestReverseTransactionSetComplianceSubTypeForPeru()
		{
			SetupHeaderForReversing(200.00m, 20.00m);
			Header.AH_ComplianceSubType = "TXI";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			((IReversing)Header).GenerateReverseTransaction(true);
			TransactionHeader reverseHeader = ((IReversing)Header).ReverseTransaction as TransactionHeader;
			if (Header.AH_Ledger == LedgerTypes.AccountsPayable &&
				(Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote))
			{
				AssertEquals("TCR", reverseHeader.AH_ComplianceSubType);
			}
			else
			{
				if (reverseHeader != null)
				{
					AssertEquals("Nothing happen if it's Peru, but not AP INV or CRD", "", reverseHeader.AH_ComplianceSubType);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public virtual void TestReverseTransactionDoeNotSetComplianceSubTypeForNonPeru()
		{
			SetupHeaderForReversing(200.00m, 20.00m);
			Header.AH_ComplianceSubType = "TXI";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Poland);
			((IReversing)Header).GenerateReverseTransaction(true);
			TransactionHeader reverseHeader = ((IReversing)Header).ReverseTransaction as TransactionHeader;
			if (reverseHeader != null)
			{
				AssertEquals("Nothing happen if it's not Peru", "", reverseHeader.AH_ComplianceSubType);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestReverseTransaction()
		{
			SetupHeaderForReversing(200.00m, 20.00m);
			try
			{
				((IReversing)Header).GenerateReverseTransaction(true);

				AssertEquals("Type of reversing transactions should be as defined on the business object",
					Header.TypeOfReverseTransaction_ForTestOnly, ((IReversing)Header).ReverseTransaction.GetType());

				TransactionHeader reverseHeader = ((IReversing)Header).ReverseTransaction as TransactionHeader;

				AssertNotNull("Reversing Transaction should always be a transactionheader at this level", reverseHeader);

				AssertEquals("IsReversing should be set to true for Header", true, Header.IsReversing);
				AssertEquals("IsReverseTransaction should be set to true for ReverseHeader", true, reverseHeader.IsReverseTransaction);

				AssertEquals("IsReverseTransaction should be set to false for Header", AreOriginalAndReverseTransactionsTheSame, Header.IsReverseTransaction);
				AssertEquals("IsReversing should be set to false for ReverseHeader", AreOriginalAndReverseTransactionsTheSame, reverseHeader.IsReversing);

				AssertReverseTransactionCommonDefaults(reverseHeader);

				AssertReversedAmountsCorrectlyNegated(reverseHeader);

				AssertReversingPaymentReceiptSpecificFields(reverseHeader);

				AssertReverseTransactionDueDateValue(reverseHeader);

				AssertAdditionalTransactionValuesOnReversing(reverseHeader);

				AssertEquals("Original transaction number should be set", Header.AH_TransactionNum, reverseHeader.OriginalTransactionNumber);
				AssertEquals("Original transaction type should be set", Header.AH_TransactionType, reverseHeader.OriginalTransactionType);
			}
			catch (NotSupportedException)
			{
				Assert("Transactino type does not support reversal", true);
			}
		}

		protected virtual bool AreOriginalAndReverseTransactionsTheSame
		{
			get { return false; }
		}

		protected void TestSetTransactionBelongsToGroupField(bool shouldBeEmpty)
		{
			AssertEquals("Initial belongs to group field", shouldBeEmpty, Header.AH_TransactionBelongsToGroup.IsEmpty);
			var originalValue = Header.AH_TransactionBelongsToGroup;

			try
			{
				((IReversing)Header).GenerateReverseTransaction(true);
				TransactionHeader reverseHeader = (TransactionHeader)((IReversing)Header).ReverseTransaction;

				ZGuid groupingGuid = ZGuid.NewZGuid();
				((IReversing)Header).SetTransactionBelongsToGroupField(groupingGuid);

				AssertEquals("Default behaviour at the moment is to set transaction belongs to group to PK of header," +
					"leaving the original transactionbelongstogroup as NULL",
					Header.PK, reverseHeader.AH_TransactionBelongsToGroup);

				AssertEquals("Original transaction's transactionbelongstogroup field should be as per it's original value",
					originalValue, Header.AH_TransactionBelongsToGroup);

				//NOTE: Default behaviour is to ignore the grouping Guid, but it must be on the interface
				//for other multi-row transactiontypes such as contra, transfer and bank transfer
			}
			catch (NotSupportedException)
			{
				Assert("Transactino type does not support reversal", true);
			}
		}

		public virtual void TestSetTransactionBelongsToGroupField()
		{
			TestSetTransactionBelongsToGroupField(true);
		}

		public void TestSetDescription()
		{
			SetupHeaderForReversing(200.00m, 20.00m);
			try
			{
				((IReversing)Header).GenerateReverseTransaction(true);
				IReversing reverseHeader = (TransactionHeader)((IReversing)Header).ReverseTransaction;

				ZString descriptionToSet = "Description";
				reverseHeader.SetDescription(descriptionToSet);

				AssertEquals("Description through IReversing", descriptionToSet, ((TransactionHeader)reverseHeader).AH_Desc);
			}
			catch (NotSupportedException)
			{
				Assert("Transactino type does not support reversal", true);
			}
		}

		public void TestReversingReason()
		{
			SetupHeaderForReversing(200.00m, 20.00m);
			try
			{
				((IReversing)Header).GenerateReverseTransaction(true);
				IReversing reverseHeader = (TransactionHeader)((IReversing)Header).ReverseTransaction;

				ZString descriptionToSet = "Description";
				reverseHeader.SetDescription(descriptionToSet);
				ZString reversingReason = "Reversing Reason";
				reverseHeader.ReversingReason = reversingReason;

				AssertEquals("Description after setting reversing reason", descriptionToSet + " " + reversingReason, ((TransactionHeader)reverseHeader).AH_Desc);

				AssertEquals("Reversing Reason should return same as value set", reversingReason, reverseHeader.ReversingReason);
			}
			catch (NotSupportedException)
			{
				Assert("Transactino type does not support reversal", true);
			}
		}

		[ExpectNoExceptions]
		public void TestReversingReasonExceedsMaxLength()
		{
			string reverseReason = new string('d', AccTransactionHeaderSchema.AH_Desc.MaxLength + 1);
			Header.ReversingReason = reverseReason;
		}

		protected virtual void AssertReversedAmountsCorrectlyNegated(TransactionHeader reverseHeader)
		{
			int multiplier = Header.InvertSignsOfOriginalTransactionOnReversing_ForTestOnly ? -1 : 1;
			int dBFieldMultiplier = Header.InvertSignsOfOriginalTransactionOnReversing_ForTestOnly ?
			multiplier : multiplier * (Header.InvertSigns_ForTestOnly ? -1 : 1) * (reverseHeader.InvertSigns_ForTestOnly ? -1 : 1);

			AssertEquals("Reverse OS Ex Tax Amount", Header.AH_OSExTaxAmount * multiplier, reverseHeader.AH_OSExTaxAmount);
			AssertEquals("Reverse OS Tax Amount", Header.AH_OSTaxAmount * multiplier, reverseHeader.AH_OSTaxAmount);
			AssertEquals("Reverse Local Ex Tax Amount", Header.AH_LocalExTaxAmount * multiplier, reverseHeader.AH_LocalExTaxAmount);
			AssertEquals("Reverse Local Tax Amount", Header.AH_LocalTaxAmount * multiplier, reverseHeader.AH_LocalTaxAmount);
			AssertEquals("Reverse Local Invoice Amount - DB Field", Header.AH_InvoiceAmount * dBFieldMultiplier, reverseHeader.AH_InvoiceAmount);
			AssertEquals("Reverse Local GST Amount - DB Field", Header.AH_GSTAmount * dBFieldMultiplier, reverseHeader.AH_GSTAmount);
			AssertEquals("Reverse OS Total Amount - DB Field", Header.AH_OSTotal * dBFieldMultiplier, reverseHeader.AH_OSTotal);
			AssertEquals("Reverse Transaction Outstanding Amount", Header.AH_OutstandingAmount * dBFieldMultiplier, reverseHeader.AH_OutstandingAmount);
		}

		protected virtual void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			SetupJob();
			SetupHeaderForReversingCore(aH_OSExTaxAmount, aH_OSTaxAmount);
		}

		void SetupHeaderForReversingCore(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			SetupOrganisations();

			Header.AH_PostDate = ZDateTime.Now.AddDays(-2);
			Header.AH_OH = Organisation.PK;
			Header.AH_JH = Job.PK;
			Header.AH_GB = NonCurrentBranch.PK;
			Header.AH_GE = NonCurrentDepartment.PK;
			Header.AH_AG = GLAccount.PK;
			Header.AH_AB = BankAccount.PK;
			Header.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Header.AH_ExchangeRate = 0.6m;
			Header.AH_ConsolidatedInvoiceRef = Job.JH_JobNum;
			Header.AH_OSExTaxAmount = aH_OSExTaxAmount;
			Header.AH_OSTaxAmount = aH_OSTaxAmount;
			Header.AH_CashBasisGSTIndicator = ZBool.True;
			Header.AH_ChequeDrawer = "Check Drawer";
			Header.AH_ChequeOrReference = "REF";
			Header.AH_DrawerBank = "Drawer Bank";
			Header.AH_DrawerBranch = "Drawer Branch";
			Header.AH_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			Header.AH_InvoiceTermDays = (ZByte)14;
			Header.AH_ReceiptType = ReceiptTypes.Cheque;
			Header.AH_LocalOutstandingAmount = Header.AH_LocalExTaxAmount + Header.AH_LocalTaxAmount;
			Header.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
		}

		protected virtual void AssertAdditionalTransactionValuesOnReversing(TransactionHeader reverseHeader)
		{
		}

		protected virtual void AssertReverseTransactionDueDateValue(TransactionHeader reverseHeader)
		{
			AssertEquals("Due Date should be today's date", ZDateTime.Now.Date, reverseHeader.AH_DueDate.Date);
		}

		protected virtual void AssertReversingPaymentReceiptSpecificFields(TransactionHeader reverseHeader)
		{
			AssertEquals("AH_Receipt Batch number should always be blank for new transaction", ZString.Empty, reverseHeader.AH_ReceiptBatchNo);
		}

		protected void AssertReverseTransactionCommonDefaults(TransactionHeader reverseHeader)
		{
			AssertEquals("Ledger should always be the same as original transaction",
				Header.AH_Ledger, reverseHeader.AH_Ledger);
			AssertEquals("Currency should be the same as original transaction", Header.AH_RX_NKTransactionCurrency, reverseHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be the same as original transaction", Header.AH_ExchangeRate, reverseHeader.AH_ExchangeRate);
			AssertEquals("Branch should be the same as original transaction", Header.AH_GB, reverseHeader.AH_GB);
			AssertEquals("Department should be the same as original transaction", Header.AH_GE, reverseHeader.AH_GE);
			AssertEquals("Organisation should be the same as original transaction", Header.AH_OH, reverseHeader.AH_OH);
			AssertEquals("Job should be the same as original transaction", Header.AH_JH, reverseHeader.AH_JH);
			AssertEquals("Bank Account should be the same as original transaction", Header.AH_AB, reverseHeader.AH_AB);
			AssertEquals("AH_TransactionCategory should be the same as original transaction", Header.AH_TransactionCategory, reverseHeader.AH_TransactionCategory);
			AssertEquals("Cash Basis GST Indicator", Header.AH_CashBasisGSTIndicator, reverseHeader.AH_CashBasisGSTIndicator);
			AssertEquals("Cheque/Drawer field", Header.AH_ChequeDrawer, reverseHeader.AH_ChequeDrawer);
			AssertEquals("Cheque/Drawer field", Header.AH_ChequeOrReference, reverseHeader.AH_ChequeOrReference);
			AssertEquals("Drawer Bank field should be same as original transaction",
				Header.AH_DrawerBank, reverseHeader.AH_DrawerBank);
			AssertEquals("Drawer Bank field should be same as original transaction",
				Header.AH_DrawerBranch, reverseHeader.AH_DrawerBranch);
			AssertEquals("Drawer Bank field should be same as original transaction",
				Header.AH_InvoiceTerm, reverseHeader.AH_InvoiceTerm);
			AssertEquals("Drawer Bank field should be same as original transaction",
				Header.AH_InvoiceTermDays, reverseHeader.AH_InvoiceTermDays);
			AssertEquals("Drawer Bank field should be same as original transaction",
				Header.AH_ReceiptType, reverseHeader.AH_ReceiptType);

			AssertEquals("AH_PostToGL", "N", reverseHeader.AH_PostToGL);
			AssertEquals("AH_CashBasisGSTRealise", ZBool.False, reverseHeader.AH_CashBasisGSTRealisedToGL);
			AssertEquals("AH_InvoiceApproved", ZBool.False, reverseHeader.AH_InvoiceApproved);
			AssertEquals("AH_InvoicePrinted", ZBool.False, reverseHeader.AH_InvoicePrinted);
			//AssertEquals("AH_IsClearedInCashbook", ZBool.False, ReverseHeader.AH_IsClearedInCashbook);
			AssertEquals("AH_DateClearedInCashbook", ZDateTime.Empty, reverseHeader.AH_DateClearedInCashbook);
			AssertEquals("AH_NotAllocated", ZBool.False, reverseHeader.AH_NotAllocated);
			AssertEquals("AH_POST1", ZBool.False, reverseHeader.AH_POST1);
			AssertEquals("AH_POST2", ZBool.False, reverseHeader.AH_POST2);
			AssertEquals("AH_POST3", ZBool.False, reverseHeader.AH_POST3);
			AssertEquals("AH_POST4", ZBool.False, reverseHeader.AH_POST4);
			AssertEquals("AH_PostedToEFT", ZBool.False, reverseHeader.AH_PostedToEFT);
		}

		#region ITransaction Implementation Testing

		public void TestCurrencyImplementation()
		{
			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			AssertEquals("should return foreign", ForeignCurrency.RX_Code, ((ITransaction)Header).CurrencyCode);

			Header.AH_RX_NKTransactionCurrency = ZString.Empty;
			AssertEquals("Should return empty code", ZString.Empty, ((ITransaction)Header).CurrencyCode);

			Header.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("should return local currency code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ((ITransaction)Header).CurrencyCode);
		}

		public void TestLedgerImplementation()
		{
			AssertEquals("Should be ledger of transaction header (already an abstract member", Header.Ledger_ForTestOnly, ((ITransaction)Header).Ledger);
		}

		public void TestOverseasTotalAmountImplementation()
		{
			Header.AH_OSExTaxAmount = 350.00m;
			Header.AH_OSTaxAmount = 35.00m;
			AssertEquals("Overseas total", 385.00m, ((ITransaction)Header).OverseasTotalAmount);
		}

		public virtual void TestPostDateImplementation()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(-2);
			Header.AH_PostDate = postDate;
			AssertEquals("Post Date", postDate, ((ITransaction)Header).PostDate);
		}

		public void TestTransactionDateImplementation()
		{
			ZDateTime transactionDate = ZDateTime.Now.AddDays(20);
			Header.AH_InvoiceDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)Header).TransactionDate);

			transactionDate = ZDateTime.Now.AddDays(10);
			((ITransaction)Header).TransactionDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)Header).TransactionDate);
		}

		public void TestTransactionTypeImplementation()
		{
			AssertEquals("Transaction Type", Header.TransactionType_ForTestOnly, ((ITransaction)Header).TransactionType);
		}

		#region ReversalStatusCode

		public virtual void TestReversalStatusCode_ShouldBeEmpty()
		{
			AssertEquals(nameof(Header.ReversalStatusCode), ZString.Empty, Header.ReversalStatusCode);
		}

		public virtual void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (Header as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public virtual void TestReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (Header as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		#endregion

		#endregion

		#region IDataExportBatchSource Members

		public void TestIsDataExportBatchSupported()
		{
			var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;

			if (transaction.AH_Ledger == LedgerTypes.AccountsPayable ||
				transaction.AH_Ledger == LedgerTypes.AccountsReceivable ||
				transaction.AH_Ledger == LedgerTypes.General ||
				transaction.AH_Ledger == LedgerTypes.JobCosting ||
				transaction.AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||
				(transaction.AH_Ledger == LedgerTypes.CashBook && transaction.AH_TransactionType != TransactionTypes.DDRBatch))
			{
				AssertEquals("IsDataExportBatchSupported", true, ((IDataExportBatchSource)transaction).IsDataExportBatchSupported);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRelatedBatchCollection()
		{
			var batch = TestObjectCreator.CreateDataExportBatchForHeader(Header);
			Factory.Save();
			AssertCollectionContains("Public collection contains batch", batch, Header.DataExportBatchCollection);
		}

		#endregion

		#region Concurrency testing

		[ExpectNoExceptions()]
		public void TestConcurrency()
		{
			Header.AH_PostToGL = "N";
			Factory.RefreshEnabled = false;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			AccTransactionHeader loadedHeader = (AccTransactionHeader)newFactory.Load(GetExpectedBusinessObjectType(), Header.PK);
			loadedHeader.AH_DueDate = new ZDateTime(2001, 1, 4);

			Header.AH_PostToGL = "Y";

			Factory.Save();
			newFactory.Save();//shouldn't give me a concurrency exception
		}

		#endregion

		#region Unmatching Member Testing

		public virtual void TestRelatedTransactions()
		{
			AssertEquals("Related Transactions should return null", 0, Header.RelatedTransactions.Count);
		}

		public virtual void TestAreRelatedTransactionsCreatedByMatching()
		{
			Assert("Should return false", !Header.AreRelatedTransactionsCreatedByMatching);
		}

		public virtual void TestGetTopLevelTransaction()
		{
			AssertNull("Should default to null", Header.GetTopLevelTransaction);
		}

		#endregion

		public void TestEDIMessages()
		{
			AssertEquals("Prerequisite: there should be no EDI Messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);
			Assert("EDIMessages collection should be read only", Header.EDIMessages.ReadOnly);
			Header.EDIMessages.Load();
			AssertEquals("Collection should contain 0 messages", 0, Header.EDIMessages.Count);

			EDIMessage msg1 = Factory.New<eNettEDIMessage>();
			msg1.EM_ApplicationCode = "AAA";
			msg1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg1.EM_LinkTable = TransactionHeader.Schema.TableName;
			msg1.EM_LinkUniqueID = Header.PK;

			EDIMessage msg2 = Factory.New<eNettEDIMessage>();
			msg2.EM_ApplicationCode = "AAA";
			msg2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg2.EM_LinkTable = TransactionHeader.Schema.TableName;
			msg2.EM_LinkUniqueID = ZGuid.NewZGuid();

			Factory.Save();

			Header.EDIMessages.Load();
			AssertEquals("Collection should now contain 1 message", 1, Header.EDIMessages.Count);
			Assert("Collection should contain correct message", Header.EDIMessages.Contains(msg1));
		}

		public void TestOutstandingAmountIsProtectedAndNotSynchronisedBetweenFactories()
		{
			Factory.RefreshEnabled = false;
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			invoice.AH_ChequeOrReference = "12345";

			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = invoice.AH_AB;

			APInvoiceLine line = invoice.Lines.AddNew() as APInvoiceLine;
			line.FillWithValidTestData();
			line.AL_LocalExTaxAmount = 12m;
			line.AL_OSExTaxAmount = 12m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			AssertEquals("AH_OutstandingAmount", -12m, invoice.AH_OutstandingAmount);

			Factory.Save();

			APPayment partialPay = Factory.New<APPayment>();
			partialPay.AH_InvoiceAmount = 10m;
			partialPay.AH_OSTotal = 10m;
			IMatching invoiceAsIMatching = invoice;
			invoiceAsIMatching.OSPartialPaymentAmount = -2m;
			invoiceAsIMatching.PartiallyPay();
			invoiceAsIMatching.GenerateMatchLinks();
			TransactionMatchLink payLink = invoiceAsIMatching.CurrentMatchGroup.AddNew();
			payLink.AP_AH = partialPay.PK;
			payLink.AP_Amount = 2m;
			partialPay.AH_OutstandingAmount = 8m;
			TestObjectCreator.SetupMatchLinkMatchDate(invoiceAsIMatching);

			BusinessObjectFactory differentFactory = new BusinessObjectFactory();
			APPayment differentPay = differentFactory.New<APPayment>();
			differentPay.AH_InvoiceAmount = 8m;
			differentPay.AH_OSTotal = 8m;
			IMatching invoiceInADifferentFactoryAsIMatching = differentFactory.Load<APInvoice>(invoice.PK);
			invoiceInADifferentFactoryAsIMatching.OSPartialPaymentAmount = -3m;
			invoiceInADifferentFactoryAsIMatching.PartiallyPay();
			invoiceInADifferentFactoryAsIMatching.GenerateMatchLinks();
			TransactionMatchLink differentLink = invoiceInADifferentFactoryAsIMatching.CurrentMatchGroup.AddNew();
			differentLink.AP_AH = differentPay.PK;
			differentLink.AP_Amount = 3m;
			differentPay.AH_OutstandingAmount = 5m;
			TestObjectCreator.SetupMatchLinkMatchDate(invoiceInADifferentFactoryAsIMatching);

			differentFactory.Save();

			invoice.AH_OutstandingAmount = -7m; //This line is a hack used to get rid of critical validation.

			ErrorReporter.Clear();
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
		}

		public void TestBindableInvoiceAmount_ReadOnly()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			Header.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Australia;
			Assert("BindableInvoiceAmount should be readonly by default.", Header.BindableInvoiceAmountInfo.ReadOnly);
			Header.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Germany;
			Assert("BindableInvoiceAmount should not be readonly when currency is not local", !Header.BindableInvoiceAmountInfo.ReadOnly);
			Header.IsMiscellaneousTransaction = true;
			Assert("BindableInvoiceAmount should always be readonly when IsMiscellaneousTransaction is true.", Header.BindableInvoiceAmountInfo.ReadOnly);
		}

		public void TestOSPartialPaymentAmount_ReadOnly()
		{
			Assert("OSPartialPaymentAmount should not be readonly by default.", !Header.OSPartialPaymentAmount_ReadOnly);
		}

		public void TestPropertiesSetToReadOnlyOnSaved()
		{
			if (Header.IsTransactionInDatabaseReadOnly)
			{
				Factory.Save();
				AssertPropertiesAreReadOnly(Header);
			}
			else
			{
				Assert("Not for this transaction type.", true);
			}
		}

		public virtual void TestPropertiesSetToReadOnlyOnLoaded()
		{
			if (Header.IsTransactionInDatabaseReadOnly)
			{
				Factory.Save();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				TransactionHeader header = newFactory.Load<TransactionHeader>(Header.PK);
				AssertPropertiesAreReadOnly(header);
			}
			else
			{
				Assert("Not for this transaction type.", true);
			}
		}

		protected void AssertPropertiesAreReadOnly(TransactionHeader header)
		{
			foreach (ZPropertyInfo property in header.ZPropertyInfoHash)
			{
				if (property.HasSetter)
				{
					AssertEquals(property.Name + ".ReadOnly", !header.WritableProperties_ForTestOnly.Contains(property.Name), property.ReadOnly);
				}
			}
		}

		[TestDate(2024, 08, 25)]
		public void TestComplianceNumberAllocationDateFields()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			var today = ZDateTime.Today;
			var postDate = today.AddDays(-2);
			var invoiceDate = today.AddDays(-3);
			Header.AH_PostDate = postDate;
			Header.AH_InvoiceDate = invoiceDate;

			AssertEquals(ComplianceNumberAllocationDateOptions.NoControl.Code, Header.ComplianceNumberAllocationDateOption);
			AssertEquals(ZDateTime.Empty, Header.ComplianceNumberAllocationDate);
			AssertEquals(today, Header.ComplianceNumberAllocationDateWithFallbackValue);
			AssertEquals(null, Header.ComplianceNumberAllocationDateColumn);
			Assert(!Header.IsComplianceNumberAllocationMandatory);

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currCompanyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompanyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				switch (Header.AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						AssertEquals(ComplianceNumberAllocationDateOptions.InvoiceDate.Code, Header.ComplianceNumberAllocationDateOption);
						AssertEquals(invoiceDate, Header.ComplianceNumberAllocationDate);
						AssertEquals(invoiceDate, Header.ComplianceNumberAllocationDateWithFallbackValue);
						AssertEquals(AccTransactionHeaderSchema.AH_InvoiceDate, Header.ComplianceNumberAllocationDateColumn);
						Assert(Header.IsComplianceNumberAllocationMandatory);
						break;

					case LedgerTypes.AccountsPayable:
					case LedgerTypes.IncompleteTransactions:
					case LedgerTypes.UnapprovedPayableTransactions:
					case LedgerTypes.TransactionsPendingAllocation:
						AssertEquals(ComplianceNumberAllocationDateOptions.PostDate.Code, Header.ComplianceNumberAllocationDateOption);
						AssertEquals(postDate, Header.ComplianceNumberAllocationDate);
						AssertEquals(postDate, Header.ComplianceNumberAllocationDateWithFallbackValue);
						AssertEquals(AccTransactionHeaderSchema.AH_PostDate, Header.ComplianceNumberAllocationDateColumn);
						Assert(Header.IsComplianceNumberAllocationMandatory);
						break;
					default:
						AssertEquals(ComplianceNumberAllocationDateOptions.NoControl.Code, Header.ComplianceNumberAllocationDateOption);
						AssertEquals(ZDateTime.Empty, Header.ComplianceNumberAllocationDate);
						AssertEquals(today, Header.ComplianceNumberAllocationDateWithFallbackValue);
						AssertEquals(null, Header.ComplianceNumberAllocationDateColumn);
						Assert(!Header.IsComplianceNumberAllocationMandatory);
						break;
				}
			}
		}

		public void TestGetLastDateUsedInComplianceBook()
		{
			var today = ZDateTime.Today;
			var registry = AccountingMasterFilesRegistry.Instance;
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 25);
			AssertEquals("PreCond: no invoice in sequence", 0, Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_XD_ComplianceBook, sequence.PK)).Length);

			AssertEquals("return empty date if sequence is empty", ZDateTime.Empty, Header.GetLastDateUsedInComplianceBook(sequence));

			if (IsInvoiceLedgerSupportedByComplianceNumberAllocationDate(Header))
			{
				Header.AH_XD_ComplianceBook = sequence.PK;
				Factory.Save();

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					Assert("PreCond: allocation date is valid", Header.ComplianceNumberAllocationDate.IsValid);
					AssertEquals("last date used cannot return current transaction info", ZDateTime.Empty, Header.GetLastDateUsedInComplianceBook(sequence));

					var header2 = Factory.NewWithValidTestData<ARInvoice>();
					header2.AH_XD_ComplianceBook = sequence.PK;
					header2.AH_PostDate = today.AddDays(-2);
					header2.AH_InvoiceDate = today.AddDays(-3);
					header2.AH_TransactionReference = "TXI /000000002";

					var header3 = Factory.NewWithValidTestData<ARInvoice>();
					header3.AH_XD_ComplianceBook = sequence.PK;
					header3.AH_PostDate = today.AddDays(-4);
					header3.AH_InvoiceDate = today.AddDays(-5);
					header3.AH_TransactionReference = "TXI /000000003";
					Factory.Save();

					AssertEquals("expect header3 invoice date because AR registry is INV and header3 is last transaction in book and its ledger is AR", today.AddDays(-5), Header.GetLastDateUsedInComplianceBook(sequence));

					var header4 = Factory.NewWithValidTestData<APInvoice>();
					header4.AH_XD_ComplianceBook = sequence.PK;
					header4.AH_PostDate = today.AddDays(-6);
					header4.AH_InvoiceDate = today.AddDays(-7);
					header4.AH_TransactionReference = "TXI /000000004";

					var header5 = Factory.NewWithValidTestData<APInvoice>();
					header5.AH_XD_ComplianceBook = sequence.PK;
					header5.AH_PostDate = today.AddDays(-8);
					header5.AH_InvoiceDate = today.AddDays(-9);
					header5.AH_TransactionReference = "TXI /000000005";
					Factory.Save();

					AssertEquals("expect header5 post date because AP registry is PST and header5 is last transaction in book and its ledger is AP", today.AddDays(-8), Header.GetLastDateUsedInComplianceBook(sequence));
				}

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.NoControl.Code))
				{
					AssertEquals("header5 allocation date is empty because AP registry is NOT so we fallback to the previous transaction header3 allocation date", today.AddDays(-5), Header.GetLastDateUsedInComplianceBook(sequence));
				}
			}
		}

		public void TestIsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook()
		{
			var today = ZDateTime.Today;
			var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			var registry = AccountingMasterFilesRegistry.Instance;

			var emptySequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 25);
			Assert("PreCond: ComplianceNumberAllocationDate is empty", !Header.ComplianceNumberAllocationDate.IsValid);
			Assert("PreCond: sequence Last date used is empty", !Header.GetLastDateUsedInComplianceBook(emptySequence).IsValid);
			Assert("return false if ComplianceNumberAllocationDate and last date used in book are empty", !Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(emptySequence));

			if (IsInvoiceLedgerSupportedByComplianceNumberAllocationDate(Header))
			{
				var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 25);
				var isAR = Header.AH_Ledger == LedgerTypes.AccountsReceivable;
				var previousTransactionInBook = isAR ? Factory.NewWithValidTestData<APInvoice>() : Factory.NewWithValidTestData<ARInvoice>() as InvoicingBase;
				previousTransactionInBook.AH_XD_ComplianceBook = sequence.PK;
				previousTransactionInBook.AH_PostDate = previousTransactionInBook.AH_InvoiceDate = today.AddDays(-1);
				Factory.Save();

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, isAR ? ComplianceNumberAllocationDateOptions.NoControl.Code : ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, isAR ? ComplianceNumberAllocationDateOptions.PostDate.Code : ComplianceNumberAllocationDateOptions.NoControl.Code))
				{
					Assert("PreCond: ComplianceNumberAllocationDate is empty", !Header.ComplianceNumberAllocationDate.IsValid);
					AssertEquals("PreCond: sequence Last date used is valid", today.AddDays(-1), Header.GetLastDateUsedInComplianceBook(sequence));
					Assert("return false if ComplianceNumberAllocationDate is empty", !Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(emptySequence));
				}

				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					Header.AH_PostDate = Header.AH_InvoiceDate = today.AddDays(-2);
					AssertEquals("PreCond: ComplianceNumberAllocationDate is valid", today.AddDays(-2), Header.ComplianceNumberAllocationDate);
					Assert("PreCond: sequence Last date used is empty", !Header.GetLastDateUsedInComplianceBook(emptySequence).IsValid);
					Assert("return false if last date used in book is empty", !Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(emptySequence));

					Assert("PreCond: ComplianceNumberAllocationDate is before last date used in book", Header.ComplianceNumberAllocationDate < Header.GetLastDateUsedInComplianceBook(sequence));
					Assert("return true if ComplianceNumberAllocationDate is before last date used in book", Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(sequence));

					Header.AH_PostDate = Header.AH_InvoiceDate = previousTransactionInBook.AH_PostDate;
					AssertEquals("PreCond: ComplianceNumberAllocationDate is equals to last date used in book", Header.ComplianceNumberAllocationDate, Header.GetLastDateUsedInComplianceBook(sequence));
					Assert("return false if both dates are equals", !Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(sequence));

					Header.AH_PostDate = Header.AH_InvoiceDate = today;
					Assert("PreCond: ComplianceNumberAllocationDate is after last date used in book", Header.ComplianceNumberAllocationDate > Header.GetLastDateUsedInComplianceBook(sequence));
					Assert("return false if ComplianceNumberAllocationDate is after last date used in book", !Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(sequence));
				}
			}
		}

		[TestDate(2025, 3, 21)]
		public void TestIsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook_ShouldIgnoreTime()
		{
			if (IsInvoiceLedgerSupportedByComplianceNumberAllocationDate(Header))
			{
				var sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TXI", 1, 100, 25);
				var previousTransactionInBook = Factory.NewWithValidTestData<ARInvoice>() as InvoicingBase;
				previousTransactionInBook.AH_XD_ComplianceBook = sequence.PK;
				previousTransactionInBook.AH_PostDate = new ZDateTime(2025, 3, 20, 15, 00, 0);
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					Header.AH_PostDate = new ZDateTime(2025, 3, 20);
					Assert(!Header.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(sequence));
				}
			}
			else
			{
				Assert(true);
			}
		}

		bool IsInvoiceLedgerSupportedByComplianceNumberAllocationDate(TransactionHeader transaction)
			=> transaction.AH_Ledger == LedgerTypes.AccountsReceivable
				|| transaction.AH_Ledger == LedgerTypes.AccountsPayable
				|| transaction.AH_Ledger == LedgerTypes.IncompleteTransactions
				|| transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
				|| transaction.AH_Ledger == LedgerTypes.TransactionsPendingAllocation;

		public void TestExtraProperties()
		{
			if (Header is InvoiceBulkBatch)
			{
				Assert(true);
				return;
			}

			bool originalValue = AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value;

			try
			{
				AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				Header.AH_OutstandingAmount = 100.00m;
				AssertEquals("Outstanding Amount", 100.00m, Header.OutstandingAmountBindable);

				Header.AH_InvoiceAmount = 200.00m;
				Header.AH_GSTAmount = 20.00m;
				Header.AH_OutstandingAmount = 0.00m;
				AssertEquals("Payment Status", "PAID", Header.PaymentStatus);

				Header.AH_OutstandingAmount = 220.00m;
				AssertEquals("Payment Status", "UNPAID", Header.PaymentStatus);

				Header.AH_OutstandingAmount = 100.00m;
				AssertEquals("Payment Status", "PARTPAID", Header.PaymentStatus);

				AssertEquals("Fully Paid Date", ZString.Empty, Header.FullyPaidDateBindable.ToLongTimeString().ToUpper());

				Header.AH_FullyPaidDate = new ZDateTime(2010, 10, 31, 14, 30, 0);
				AssertEquals("Fully Paid Date", "31-OCT-10 14:30", Header.FullyPaidDateBindable.ToLongTimeString().ToUpper());

				AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals("Outstanding Amount", 0m, Header.OutstandingAmountBindable);

				Header.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				Header.AH_Ledger = LedgerTypes.AccountsReceivable;
				Header.AH_TransactionType = TransactionTypes.Invoice;
				Header.AH_TransactionNum = "00001000";
				Header.AH_ConsolidatedInvoiceRef = string.Empty;
				Header.PopulateExtraProperties();

				AssertEquals("Outstanding Amount", 40.00m, Header.OutstandingAmountBindable);
				AssertEquals("Payment Status", "PARTPAID", Header.PaymentStatus);
				AssertEquals("Fully Paid Date", ZString.Empty, Header.FullyPaidDateBindable.ToLongTimeString().ToUpper());

				Header.AH_TransactionNum = "00002000";
				Header.PopulateExtraProperties();

				AssertEquals("Outstanding Amount", 100.00m, Header.OutstandingAmountBindable);
				AssertEquals("Payment Status", "UNPAID", Header.PaymentStatus);
				AssertEquals("Fully Paid Date", ZString.Empty, Header.FullyPaidDateBindable.ToLongTimeString().ToUpper());

				Header.AH_TransactionNum = "00003000";
				Header.PopulateExtraProperties();

				AssertEquals("Outstanding Amount", 0.00m, Header.OutstandingAmountBindable);
				AssertEquals("Payment Status", "PAID", Header.PaymentStatus);
				AssertEquals("Fully Paid Date", "31-OCT-10 00:00", Header.FullyPaidDateBindable.ToLongTimeString().ToUpper());

				Header.AH_TransactionNum = "";
				Header.AH_ConsolidatedInvoiceRef = "S00001000";
				Header.PopulateExtraProperties();

				AssertEquals("Outstanding Amount", 0.00m, Header.OutstandingAmountBindable);
				AssertEquals("Payment Status", "PAID", Header.PaymentStatus);
				AssertEquals("Fully Paid Date", "30-NOV-10 00:00", Header.FullyPaidDateBindable.ToLongTimeString().ToUpper());
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestAddWritableProperties()
		{
			int prevCount = Header.WritableProperties_ForTestOnly.Count;
			Header.AddWritableProperties(new string[] { "Property" });
			AssertEquals("Property should be added.", prevCount + 1, Header.WritableProperties_ForTestOnly.Count);

			Header.AddWritableProperties(new string[] { "Property" });
			AssertEquals("Already existed properties must not be added.", prevCount + 1, Header.WritableProperties_ForTestOnly.Count);
		}

		public void TestSetObjectReadOnly()
		{
			Header.WritableProperties_ForTestOnly.Clear();
			Header.SetObjectReadOnly_ForTestOnly();
			Assert("ReadOnly should be true because no writable properties", Header.ReadOnly);
		}

		public virtual void TestGetWritableProperties()
		{
			AssertEquals("GetWritableProperties().Count", 3, Header.GetWritableProperties_ForTestOnly().Count);
			AssertEquals("GetWritableProperties()[0]", "OSPartialPaymentAmount", Header.GetWritableProperties_ForTestOnly()[0]);
			AssertEquals("GetWritableProperties()[1]", "MatchStatus", Header.GetWritableProperties_ForTestOnly()[1]);
			AssertEquals("GetWritableProperties()[2]", "MatchStatusReasonCode", Header.GetWritableProperties_ForTestOnly()[2]);
		}

		public void TestTransactionCategory()
		{
			AssertEquals("Should be always equal to AH_TransactionCategory", Header.AH_TransactionCategory, Header.TransactionCategory);
			Assert("TransactionCategoryInfo should be ZWrappedPropertyInfo", Header.TransactionCategoryInfo is ZWrappedPropertyInfo);
			AssertEquals("Should wrap AH_TransactionCategoryInfo", Header.AH_TransactionCategoryInfo, ((ZWrappedPropertyInfo)Header.TransactionCategoryInfo).InnerInfo);
		}

		public void TestDeletingOfIncompleteThrowsNoException()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				SetupForSave();
				InvoicingBase invoicingBase = Header as InvoicingBase;
				if (invoicingBase != null)
				{
					invoicingBase.SaveAsIncomplete();
				}
				AssertNoExceptionThrown("No Exception Is Thrown When Deleting An Incomplete Invoice", () => { Header.Delete(); });
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRelatedGLJournals()
		{
			SetupForSave();
			InvoicingBase invoicingBase = Header as InvoicingBase;
			if (invoicingBase != null)
			{
				var journal = Factory.NewWithValidTestData<GLJournal>();
				journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
				journal.AH_TransactionBelongsToGroup = Header.PK;
				Factory.Save();
				AssertEquals(1, invoicingBase.RelatedGLJournals.Count);
				AssertEquals(journal.PK, invoicingBase.RelatedGLJournals[0].PK);
			}
			else
			{
				Assert(Header.RelatedGLJournals.IsNullOrEmpty());
			}
		}

		[TestDate(2020, 3, 21)]
		public void TestIsPrePaymentTransaction()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);

			SetupForSave();
			var invoicingBase = Header as InvoicingBase;
			if (invoicingBase != null)
			{
				invoicingBase.AH_PostDate = ZDateTime.Today;

				var journal1 = Factory.NewWithValidTestData<GLJournal>();
				journal1.AH_TransactionType = TransactionTypes.GLStandardJournal;
				journal1.AH_TransactionBelongsToGroup = Header.PK;
				journal1.AH_PostDate = invoicingBase.AH_PostDate;

				var journal2 = Factory.NewWithValidTestData<GLJournal>();
				journal2.AH_TransactionType = TransactionTypes.GLStandardJournal;
				journal2.AH_TransactionBelongsToGroup = Header.PK;
				journal2.AH_PostDate = invoicingBase.AH_PostDate.AddMonths(1);

				var journal3 = Factory.NewWithValidTestData<GLJournal>();
				journal3.AH_TransactionType = TransactionTypes.GLStandardJournal;
				journal3.AH_TransactionBelongsToGroup = Header.PK;
				journal3.AH_PostDate = invoicingBase.AH_PostDate.AddMonths(2);

				Assert(Header.IsPrePaymentTransaction);

				journal3.AH_PostDate = invoicingBase.AH_PostDate.AddMonths(-1);
				Assert(!Header.IsPrePaymentTransaction);
			}
			else
			{
				Assert(Header.IsPrePaymentTransaction);
			}
		}

		public void TestIsUsedByActiveCollectionOrderLine()
		{
			InvoicingBase invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			InvoicingBase invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			var bank = Factory.NewWithValidTestData<AccBankAccount>();

			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 100m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";

			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDateTime.Today.Date;
			order.ACO_OH_Debtor = TestObjectCreator.AALSHI.PK;
			order.ACO_OrderNumber = "000001";
			order.IncludeInBatch = true;
			order.ACO_Amount = 100m;

			var orderline = Factory.New<AccCollectionOrderLine>();
			orderline.AOL_ACO = order.PK;
			orderline.AOL_AH = invoice1.PK;
			orderline.AOL_IsCancelled = false;
			orderline.IncludeInOrder = true;
			Factory.Save();
			Assert(invoice1.IsUsedByActiveCollectionOrderLine);

			orderline.AOL_AH = invoice2.PK;
			Factory.Save();
			Assert(!invoice1.IsUsedByActiveCollectionOrderLine);
		}

		public void TestOrgHeaderHasPaymentMethod()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.ARAccountDetailsCollection.DeleteAll();
			var accountDetail1 = org.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetail1.A1_PaymentMethod = "TAX";
			accountDetail1.A1_RX_NKAccountCurrency = "AUD";
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = org.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetail2.A1_PaymentMethod = "CRQ";
			accountDetail2.A1_RX_NKAccountCurrency = "USD";
			accountDetail2.A1_IsDefaultAccount = true;

			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = org.PK;
			Assert(invoice.OrgHeaderHasPaymentMethod("TAX", "AUD"));
			Assert(!invoice.OrgHeaderHasPaymentMethod("TAX", "USD"));
			Assert(invoice.OrgHeaderHasPaymentMethod("CRQ", "USD"));
			Assert(!invoice.OrgHeaderHasPaymentMethod("CRQ", "AUD"));
		}

		public void TestIsAmendingWithARCreditNote()
		{
			if (Header is ARCreditNote)
			{
				Assert(!Header.IsAmendingWithARCreditNote);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();
				Assert(creditNote.IsAmendingWithARCreditNote);
			}
			else
			{
				Assert(!Header.IsAmendingWithARCreditNote);
			}
		}

		public virtual void TestJobDirection()
		{
			if (Header is Invoice)
			{
				Assert(true);
			}
			else
			{
				AssertEquals(Directions.Unknown, Header.JobDirection);
			}
		}

		public void TestIsAmendingOrReversal()
		{
			Header.AH_TransactionBelongsToGroup = ZGuid.Empty;
			AssertEquals("IsAmendingOrReversal", false, Header.IsAmendingOrReversal);

			var invoice = Factory.New<ARInvoice>();
			var amendment = invoice.GenerateAmendingTransaction<ARCreditNote>();
			AssertEquals("IsAmendingOrReversal", amendment is IAmending, amendment.IsAmendingOrReversal);

			amendment.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			AssertEquals("IsAmendingOrReversal", false, amendment.IsAmendingOrReversal);

			amendment.AH_IsCancelled = true;
			AssertEquals("IsAmendingOrReversal", amendment is IReversing, amendment.IsAmendingOrReversal);

			amendment.AH_TransactionBelongsToGroup = ZGuid.Empty;
			AssertEquals("IsAmendingOrReversal", false, amendment.IsAmendingOrReversal);
		}

		public void TestRoundTheExchangeRate()
		{
			Header.AH_ExchangeRate = 0.131506272M;
			AssertEquals(0.131506m, Header.AH_ExchangeRate);
		}

		public void TestUserAllowedToModifyInvoiceDateWhenReversing()
		{
			bool originalValueAR = Env.Security.ReceivablesModifyInvoiceDateWhenReversing.IsAllowed;
			bool originalValueAP = Env.Security.PayablesModifyInvoiceDateWhenReversing.IsAllowed;
			try
			{
				Env.Security.ReceivablesModifyInvoiceDateWhenReversing.IsAllowed = false;
				Env.Security.PayablesModifyInvoiceDateWhenReversing.IsAllowed = false;

				AssertEquals("UserAllowedToModifyInvoiceDateWhenReversing", false, Header.UserAllowedToModifyInvoiceDateWhenReversing);

				Env.Security.ReceivablesModifyInvoiceDateWhenReversing.IsAllowed = true;
				Env.Security.PayablesModifyInvoiceDateWhenReversing.IsAllowed = true;
				bool expectedValue = false;
				if (Header.AH_Ledger == LedgerTypes.AccountsReceivable && (Header is ARInvoice || Header is ARCreditNote))
				{
					expectedValue = true;
				}
				else if (Header.AH_Ledger == LedgerTypes.AccountsPayable && (Header is APInvoice || Header is APCreditNote))
				{
					expectedValue = true;
				}
				AssertEquals("UserAllowedToModifyInvoiceDateWhenReversing", expectedValue, Header.UserAllowedToModifyInvoiceDateWhenReversing);
			}
			finally
			{
				Env.Security.ReceivablesModifyInvoiceDateWhenReversing.IsAllowed = originalValueAR;
				Env.Security.PayablesModifyInvoiceDateWhenReversing.IsAllowed = originalValueAP;
			}
		}

		[SuspendCriticalValidation]
		public void TestAH_IsCancelledConcurrency()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var testObjectCreator = new TestObjectCreator(factory1);
			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1.0m, testObjectCreator.Debtor);
			factory1.Save();

			var reloadedInvoice = factory2.Load<ARInvoice>(invoice.PK);
			reloadedInvoice.AH_IsCancelled = true;
			factory2.Save();

			invoice.AH_IsCancelled = true;

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionHeader
PK: {invoice.PK}
RowState: Modified
";
			Assert(string.Format(
@"Message
{0}
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		[ExpectNoExceptions("No Concurrency Exception is Expected")]
		public void TestUpdatePrintedFlagHandlesConcurrencyErrors()
		{
			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TSTTRNSC", TestObjectCreator.AUD, 1.0m, 200m, 0m, 0m, 500m, 0m, 0m, TestObjectCreator.Creditor1);
			transaction.AH_InvoicePrinted = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var reloadedTransaction = newFactory.Load<APInvoice>(transaction.PK);

			transaction.UpdatePrintedFlagExposed_TestOnly();
			reloadedTransaction.UpdatePrintedFlagExposed_TestOnly();

			Assert("Printed Flag is true", transaction.AH_InvoicePrinted);
		}

		public void TestUpdatePrintedFlagHandlesConcurrencyErrors_NotifyAndMerge()
		{
			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TSTTRNSC", TestObjectCreator.AUD, 1.0m, 200m, 0m, 0m, 500m, 0m, 0m, TestObjectCreator.Creditor1);
			transaction.AH_InvoicePrinted = false;
			transaction.AH_PostedInternal = false;
			Factory.Save();

			var newfactory = new BusinessObjectFactory();
			newfactory.RefreshEnabled = false;
			var transactionInNewFactory = newfactory.Load<APInvoice>(transaction.PK);
			transactionInNewFactory.AH_PostedInternal = true;
			newfactory.Save();

			AssertNoExceptionThrown("No Concurrency Exception is Expected", () => transaction.UpdatePrintedFlagExposed_TestOnly(Factory));

			Assert("Printed Flag is true", transaction.AH_InvoicePrinted);
			Assert("Posted Internal is true", transaction.AH_PostedInternal);
		}

		public void TestUpdatePrintedFlag_NotSaveWhenIsNotUserInteractive()
		{
			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TSTTRNSC", TestObjectCreator.AUD, 1.0m, 200m, 0m, 0m, 500m, 0m, 0m, TestObjectCreator.Creditor1);
			transaction.AH_InvoicePrinted = false;
			transaction.AH_PostedInternal = false;
			Factory.Save();

			AssertEquals("Is User Interactive.", true, Globals.IsUserInteractive);
			AssertEquals("For user interactive, AH_InvoicePrinted is false before UpdatePrintedFlag.", false, transaction.AH_InvoicePrinted);
			transaction.UpdatePrintedFlagExposed_TestOnly();
			AssertEquals("For user interactive, AH_InvoicePrinted is true after UpdatePrintedFlag.", true, transaction.AH_InvoicePrinted);
			AssertEquals("Transaction does not have change because it have been save.", false, transaction.HasChanges);

			transaction.AH_InvoicePrinted = false;
			Globals.IsUserInteractive = false;
			AssertEquals("Is User Interactive.", false, Globals.IsUserInteractive);
			AssertEquals("For not user interactive, AH_InvoicePrinted is false before UpdatePrintedFlag.", false, transaction.AH_InvoicePrinted);
			transaction.UpdatePrintedFlagExposed_TestOnly();
			AssertEquals("For not user interactive, AH_InvoicePrinted is true after UpdatePrintedFlag.", true, transaction.AH_InvoicePrinted);
			AssertEquals("Transaction has change because it does not be save.", true, transaction.HasChanges);
		}

		[SuspendCriticalValidation]
		public void TestAH_TransactionNumConcurrency()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var testObjectCreator = new TestObjectCreator(factory1);
			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1.0m, testObjectCreator.Debtor);
			factory1.Save();

			var reloadedInvoice = factory2.Load<ARInvoice>(invoice.PK);
			reloadedInvoice.AH_TransactionNum = "AH_TransactionNum001";
			factory2.Save();

			invoice.AH_TransactionNum = "AH_TransactionNum002";

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionHeader
PK: {invoice.PK}
RowState: Modified
";
			Assert(string.Format(
@"Message
{0}
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		[SuspendCriticalValidation]
		public void TestFullyPaidDateHandlesConcurrencyErrors()
		{
			try
			{
				var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TSTTRNSC", TestObjectCreator.AUD, 1.0m, 200m, 0m, 0m, 500m, 0m, 0m, TestObjectCreator.Creditor1);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var reloadedTransaction = newFactory.Load<APInvoice>(transaction.PK);

				transaction.SetInvoiceAsPaid(ZDateTime.Today);
				transaction.Factory.Save();

				reloadedTransaction.SetInvoiceAsPaid(ZDateTime.Today.AddDays(-1));
				reloadedTransaction.Factory.Save();

				Assert("Concurrency Exception Expected", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Concurrency Exception Expected", typeof(ZSaveConcurrencyException), ex.GetType());
			}
		}

		[SuspendCriticalValidation]
		public void TestOutstandingAmountHandlesConcurrencyErrors()
		{
			try
			{
				var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TSTTRNSC", TestObjectCreator.AUD, 1.0m, 200m, 0m, 0m, 500m, 0m, 0m, TestObjectCreator.Creditor1);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var reloadedTransaction = newFactory.Load<APInvoice>(transaction.PK);

				transaction.AH_OutstandingAmount = 50M;
				transaction.Factory.Save();

				reloadedTransaction.AH_OutstandingAmount = 100M;
				reloadedTransaction.Factory.Save();

				Assert("Concurrency Exception Expected", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Concurrency Exception Expected", typeof(ZSaveConcurrencyException), ex.GetType());
			}
		}

		[SuspendCriticalValidation]
		public void TestAH_InvoiceStatementHandlesConcurrencyErrors()
		{
			try
			{
				var header = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.InvoiceBatch, "TEST001", 120m);
				var line = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "TEST002", 10m);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var reloadedLine = newFactory.Load<ARInvoice>(line.PK);

				line.AH_AH_InvoiceStatement = header.PK;
				line.Factory.Save();

				var header2 = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.InvoiceBatch, "TEST003", 120m);
				reloadedLine.AH_AH_InvoiceStatement = header2.PK;
				reloadedLine.Factory.Save();

				Assert("Concurrency Exception Expected", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Concurrency Exception Expected", typeof(ZSaveConcurrencyException), ex.GetType());
				AssertEquals("Error Message Column Name", true, ex.Message.Contains("<Column>AH_AH_InvoiceStatement</Column>"));
				AssertEquals("Error Message Concurrency Type", true, ex.Message.Contains("<Concurrency>Strict - Notify</Concurrency>"));
			}
		}

		[SuspendCriticalValidation]
		public void TestAH_ReceiptBatchNoInfoHandlesConcurrencyErrors()
		{
			try
			{
				var transaction = TestObjectCreator.CreateAPPayment(1M, 330M, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var reloadedTransaction = newFactory.Load<APInvoice>(transaction.PK);

				transaction.AH_ReceiptBatchNo = "123";
				transaction.Factory.Save();

				reloadedTransaction.AH_ReceiptBatchNo = "456";
				reloadedTransaction.Factory.Save();

				Assert("Concurrency Exception Expected", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Concurrency Exception Expected", typeof(ZSaveConcurrencyException), ex.GetType());
				Assert("Error Message Column Name", ex.Message.Contains("<Column>AH_ReceiptBatchNo</Column>"));
				Assert("Error Message Concurrency Type", ex.Message.Contains("<Concurrency>Strict - Notify</Concurrency>"));
			}
		}

		public void TestMatchedAmount()
		{
			AssertMatchedAmount(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestMatchedAmount_EnableNewOSOutstandingAmountFeature()
		{
			AssertMatchedAmount(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertMatchedAmount(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			var header = PrepareTransactionHeaderForTest() as TransactionHeader;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			header.AH_ExchangeRate = 0.127400M;
			header.AH_InvoiceAmount = 4045.9300M;
			header.AH_OSTotal = 31757.7200;
			AssertEquals(0m, header.MatchedAmount);

			var shouldTestNewFeature = header.OSOutstandingAmountValueChangeMonitor.IsMonitorApplicable_ForTestOnly && isEnableNewOSOutstandingAmountFeature;
			var matchLink = Factory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = 4045.9300M;
			matchLink.AP_OSAmount = shouldTestNewFeature
				? 31757.7200m + 1m
				: 0m;
			header.SetMatchedAmount(matchLink, true);

			AssertEquals("LocalMatchedAmount", matchLink.AP_Amount, header.LocalMatchedAmount);
			AssertEquals("MatchedAmount", shouldTestNewFeature ? 31758.72m : 31757.72m, header.MatchedAmount);
		}

		public void TestHasNumberFountain()
		{
			AssertEquals(Header.NumberFountainForTransactionNumber_ForTestOnly != null, Header.HasNumberFountain);
		}

		public void TestPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var header = PrepareTransactionHeaderForTest() as TransactionHeader;
				var placeOfSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeOfSupplyCode);

				header.AH_PlaceOfSupply = placeOfSupplyCode;
				AssertEquals("AH_PlaceOfSupply", placeOfSupplyCode, header.AH_PlaceOfSupply);
				AssertEquals("AH_PlaceOfSupplyType", placeOfSupplyTypeCode, header.AH_PlaceOfSupplyType);

				header.AH_PlaceOfSupply = string.Empty;
				AssertEquals("AH_PlaceOfSupply", string.Empty, header.AH_PlaceOfSupply);
				AssertEquals("AH_PlaceOfSupplyType", string.Empty, header.AH_PlaceOfSupplyType);
			}
		}

		public void TestPostedByForTransactionPendingAllocation()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var testStaff = TestObjectCreator.CreateStaff("AAA");
			testStaff.GS_LoginName = "testAAA";
			Factory.Save();

			AssertNullOrEmpty("Precondition", transaction.PostedBy);

			APInvoice invoiceTransaction = null;
			using (Env.SetTemporaryUserContext(testStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
				var line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
				line.AL_OSExTaxAmount = invoiceTransaction.ExpectedInvoiceExclTaxTotal;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoiceTransaction.Factory.Save();
			}
			AssertNotNull("Precondition", invoiceTransaction.Logs.AddedLog);
			AssertEquals("Precondition", $"{GlbStaff.CurrentUser.GS_FullName} ({GlbStaff.CurrentUser.GS_Code})", invoiceTransaction.Logs.AddedLog.SL_UserNameAndInitials);
			AssertNotEquals("Precondition", $"{GlbStaff.CurrentUser.GS_FullName} ({GlbStaff.CurrentUser.GS_Code})", invoiceTransaction.PostedBy);
			AssertEquals($"{testStaff.GS_FullName} ({testStaff.GS_Code})", invoiceTransaction.PostedBy);
		}

		public virtual void TestPostedBy()
		{
			AssertEquals($"{GlbStaff.CurrentUser.GS_FullName} ({GlbStaff.CurrentUser.GS_Code})", Header.PostedBy);
		}

		public void TestTransactionCreationRestriction()
		{
			List<string> allARAPTransactionTypes = new List<string>
				{
					TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote,
					TransactionTypes.Journal, TransactionTypes.Receipt, TransactionTypes.Payment,
					TransactionTypes.Transfer, TransactionTypes.Contra, TransactionTypes.Overpayment,
					TransactionTypes.Discount, TransactionTypes.ExchangeDifference, TransactionTypes.InvoiceBatch
				};

			var newFactory = Factory.CreateNewFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);
			var org = testObjectCreator.CreateOrgHeader("TST", true, true);
			org.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			org.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			newFactory.Save();

			Header.AH_OH = org.PK;

			ErrorReporter.Clear();
			AssertNull("Pre-condition", ErrorReporter.LastExceptionReported);

			if (Header.AH_Ledger == LedgerTypes.AccountsPayable || Header.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (allARAPTransactionTypes.Contains(Header.AH_TransactionType))
				{
					if (Header is InvoiceBulkBatch)
					{
						//InvoiceBulkBatch's AH_OH is always empty
						AssertNoExceptionThrown(() => Factory.Save());
					}
					else
					{
						var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());
						AssertEquals("Should be blocked by transaction creation restriction", nameof(CriticalValidationErrorType.CreateTransactionRestriction), ex.ErrorType);
						AssertNull("Should NOT trigger error report", ErrorReporter.LastExceptionReported);
					}
				}
				else
				{
					Fail("If you consider the new transaction type could follow existing Transaction Creation Restriction logic, please add it to allARAPTransactionTypes white list.");
				}
			}
			else
			{
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		public void TestCheckIsAmendingTransactionForPeriodicInvoice_AmendingTransactionForPeriodicInvoice()
		{
			var newFactory = Factory.CreateNewFactory();
			var invoice = newFactory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DoNotPost;

			var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();

			var (isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			Assert("The credit note is not an amending Transaction For Periodic Invoice because is not deferred invoice type", !isAmendingTransactionForPeriodicInvoice);

			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			(isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			Assert("The credit note is an amending Transaction For Periodic Invoice", isAmendingTransactionForPeriodicInvoice);
		}

		public void TestCheckIsAmendingTransactionForPeriodicInvoice_IsSingleJob()
		{
			var newFactory = Factory.CreateNewFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);
			var invoice = newFactory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();
			var (isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			AssertEquals("The credit note is not single job because credit notes don't have line", false, isSingleJob);
			AssertNull("Return job from CheckIsAmendingTransactionForPeriodicInvoice is null because the credit note is not single job", job);

			var line1 = (ARCreditNoteLine)creditNote.Lines.AddNew();
			(isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			AssertEquals("The credit note is not single job because the only line don't have job", false, isSingleJob);
			AssertNull("Return job from CheckIsAmendingTransactionForPeriodicInvoice is null because the credit note is not single job", job);

			line1.AL_JH = testObjectCreator.Job1.PK;
			(isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			AssertEquals("The credit note is single job credit note", true, isSingleJob);
			AssertEquals("Return job from CheckIsAmendingTransactionForPeriodicInvoice is same as the job of the first line of credit note", line1.Job, job);

			var line2 = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line2.AL_JH = testObjectCreator.Job2.PK;
			(isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			AssertEquals("The credit note is not single job because the credit notes have multiple jobs", false, isSingleJob);
			AssertNull("Return job from CheckIsAmendingTransactionForPeriodicInvoice is null because the credit note is not single job", job);
		}

		public void TestCreateOSOutstandingAmountValueChangeMonitor()
		{
			AssertNotNull("Evaluated by SetDefaultValue", Header.OSOutstandingAmountValueChangeMonitor);

			Header.AH_TransactionNum = Guid.NewGuid().ToString();
			Factory.Save();

			if (Header.IsInDatabase)
			{
				var newFactory = Factory.CreateNewFactory();
				newFactory.RefreshEnabled = false;
				var headerReloaded = (TransactionHeader)newFactory.Load(GetExpectedBusinessObjectType(), Header.PK);
				AssertNotNull("Evaluated by constructor", headerReloaded.OSOutstandingAmountValueChangeMonitor);
			}
		}

		public void TestUpdateOSOutstandingAmountOnSaving()
		{
			Header.AH_TransactionNum = Guid.NewGuid().ToString();
			AssertEquals(true, Header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);

			Factory.Save();

			if (Header.IsInDatabase)
			{
				AssertEquals(true, Header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);
			}

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;
			transaction.AH_TransactionNum = Guid.NewGuid().ToString();

			var isMonitorApplicable = transaction.OSOutstandingAmountValueChangeMonitor.IsMonitorApplicable_ForTestOnly;
			AssertEquals(!isMonitorApplicable, transaction.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);

			Factory.Save();

			if (transaction.IsInDatabase)
			{
				AssertEquals(true, transaction.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);
			}
		}

		public void TestComplianceDocumentStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();

				var reference = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
				reference.AH1_AH = invoice.PK;
				reference.AH1_Type = AccTransactionHeaderReferenceTypes.CDS;
				reference.AH1_Reference = ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDD.Code;

				AssertEquals("CDD - Compliance Document Record Deleted", invoice.ComplianceDocumentStatus);
			}
		}

		public void TestComplianceDocumentStatus_CRR()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();

				var reference = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
				reference.AH1_AH = invoice.PK;
				reference.AH1_Type = AccTransactionHeaderReferenceTypes.CDS;
				reference.AH1_Reference = ChinaComplianceInfo.ComplianceDocumentStatusTypes.CRR.Code;

				AssertEquals("CRR - Compliance document is reversed due to replacement", invoice.ComplianceDocumentStatus);
			}
		}

		public void TestValidationTypeWhenValidationSuspended()
		{
			var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;
			using (transaction.GetValidationSuspender())
			{
				var validation = transaction.Validation;
				Assert("Expect empty validation as just some not null value as actual validation calls will be skipped anyway.", TypeOfEmptyValidation == validation.GetType());
			}
		}

		public void TestSourceReference_Editable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var transaction = PrepareTransactionHeaderForTest() as TransactionHeader;

				AssertEquals(false, transaction.WritableProperties_ForTestOnly.Contains(transaction.SourceReferenceInfo.Name));

				transaction.IsReverseTransaction = true;
				transaction.AH_ComplianceSubType = "TCM";

				AssertEquals(true, transaction.WritableProperties_ForTestOnly.Contains(transaction.SourceReferenceInfo.Name));

				transaction.AH_ComplianceSubType = "TDM";
				AssertEquals(false, transaction.WritableProperties_ForTestOnly.Contains(transaction.SourceReferenceInfo.Name));
			}
		}

		public void TestCheckOSOutstandingAmountValueChangeMonitorForNewLedgerTransactionType()
		{
			var validTransactionList = new List<(string Ledger, string TransactionType)>();

			FillValidTransactions(
				new[] { LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable },
				new[] { TransactionTypes.Contra, TransactionTypes.Invoice, TransactionTypes.AdjustmentNote, TransactionTypes.CreditNote,
						TransactionTypes.Journal, TransactionTypes.Payment, TransactionTypes.Receipt, TransactionTypes.Transfer,
						TransactionTypes.Discount, TransactionTypes.ExchangeDifference, TransactionTypes.Overpayment });

			FillValidTransactions(
				new[] { LedgerTypes.AccountsReceivable },
				new[] { TransactionTypes.InvoiceBatch });

			FillValidTransactions(
				new[] { LedgerTypes.CashBook },
				new[] { TransactionTypes.OpeningPayment, TransactionTypes.OpeningReceipt, TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt,
						TransactionTypes.ExchangeDifference, TransactionTypes.ReceiptBatch, TransactionTypes.DDRBatch, TransactionTypes.Transfer });

			FillValidTransactions(
				new[] { LedgerTypes.General },
				new[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLNoteJournal, TransactionTypes.GLAutoJournal, TransactionTypes.GLReversingJournal });

			FillValidTransactions(
				new[] { LedgerTypes.JobCosting },
				new[] { TransactionTypes.JobRevenueJournal, TransactionTypes.Journal });

			FillValidTransactions(
				new[] { LedgerTypes.IncompleteTransactions },
				new[] { TransactionTypes.IncompleteInvoice, TransactionTypes.IncompleteAdjustmentNote, TransactionTypes.IncompleteCreditNote });

			FillValidTransactions(
				new[] { LedgerTypes.UnapprovedPayableTransactions },
				new[] { TransactionTypes.UAInvoice, TransactionTypes.UACreditNote });

			FillValidTransactions(
				new[] { LedgerTypes.TransactionsPendingAllocation },
				new[] { TransactionTypes.InvoicePendingAllocation, TransactionTypes.CreditNotePendingAllocation });

			var header = GetNewBusinessObject() as ITransaction;

			AssertEquals($@"{header.Ledger} {header.TransactionType} is a new type of transaction. Please
1> Check whether this type of transaction has non-ZERO Oversea Outstanding Amount. If yes, please add ledger / transaction type pair into OSOutstandingAmountValueChangeMonitor class -> OSOutstandingAmountTransactionList getter.
2> Add ledger / transaction type pair into validTransactionList in this unit test.",
				true, validTransactionList.Contains((header.Ledger, header.TransactionType)));

			void FillValidTransactions(string[] ledgers, string[] transactionTypes)
			{
				foreach (var ledger in ledgers)
				{
					transactionTypes.ForEach(x => validTransactionList.Add((ledger, x)));
				}
			}
		}

		public void TestCachedInvoiceTaxDate()
		{
			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateAndAssertInvoiceTaxDate();

			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateAndAssertInvoiceTaxDate();
		}

		protected virtual void CreateAndAssertInvoiceTaxDate()
		{
			AssertEquals(ZDateTime.Empty, Header.InvoiceTaxDate);
		}

		#region Implementation

		const string AssertMessagePivotAction = "Pivot Action";
		const string ErrorDescriptionSubmit = "Pivot for Submit";

		protected TransactionHeader Header;
		protected RefCurrency ForeignCurrency;
		protected AccountingPeriodTestHelper PeriodManagementTestHelper;
		protected AccPeriodManagement PreviousGLClosedPeriod;
		protected AccPeriodManagement PreviousSubLedgerClosedPeriod;
		protected AccPeriodManagement PreviousOpenPeriod;
		protected AccPeriodManagement CurrentPeriod;
		protected AccPeriodManagement FuturePeriod;
		protected OrgHeader Organisation;
		protected GlbDepartment NonCurrentDepartment;
		protected GlbBranch NonCurrentBranch;
		protected Job Job;
		protected AccGLHeader GLAccount;
		protected AccBankAccount BankAccount;
		protected ZDecimal OSGSTTaxAmount;
		protected TestObjectCreator TestObjectCreator;

		protected virtual Type TypeOfValidation
		{
			get { return typeof(TransactionHeaderValidation); }
		}

		protected virtual Type TypeOfReversalValidation
		{
			get { return typeof(TransactionReversalValidation); }
		}

		protected virtual Type TypeOfInvoicingBaseReversalValidation
		{
			get { return typeof(InvoicingBaseReversalValidation); }
		}

		protected virtual Type TypeOfEmptyValidation
		{
			get { return typeof(TransactionHeaderEmptyValidation); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator = new TestObjectCreator(Factory);
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

			TestObjectCreator.AALSHI.CompanyData.OB_IsDebtor = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_IsDebtor = true;
			TestObjectCreator.AALSHI.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_IsCreditor = true;

			TestObjectCreator.AALSHI.OH_IsActive = true;
			TestObjectCreator.ABIGAS.OH_IsActive = false;
			Factory.Save();

			Header = PrepareTransactionHeaderForTest() as TransactionHeader;

			SetupNonCurrentBranch();
			SetupNonCurrentDepartment();
			SetupGLAccount();
			SetupJob();
			SetupBankAccount();

			ForeignCurrency = Factory.New<RefCurrency>();
			ForeignCurrency.RX_Code = "AAA";
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.SetupPeriods();
			PreviousGLClosedPeriod = PeriodManagementTestHelper.PreviousGLClosedPeriod;
			PreviousSubLedgerClosedPeriod = PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod;
			PreviousOpenPeriod = PeriodManagementTestHelper.PreviousOpenPeriod;
			CurrentPeriod = PeriodManagementTestHelper.CurrentPeriod;
			FuturePeriod = PeriodManagementTestHelper.FuturePeriod;
			OSGSTTaxAmount = 100m;
		}

		protected virtual BusinessObject PrepareTransactionHeaderForTest()
		{
			var obj = (TransactionHeader)GetNewBusinessObject();
			// Cannot call Factory.NewWithValidTestData here because it does not fill user related properties, that will cause TestCreatingUserID to fail.
			// Cannot call obj.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, new PropertyDescriptor[0]) because it fills AH_TransactionNum with random string.

			if (obj.AH_TransactionType.IsEmpty)
			{
				obj.AH_TransactionType = obj.TransactionType_ForTestOnly;
			}

			if (obj.AH_Ledger.IsEmpty)
			{
				obj.AH_Ledger = obj.Ledger_ForTestOnly;
			}

			if (ShouldFillTransactionNumber)
			{
				obj.AH_TransactionNum = "00001022";
			}

			return obj;
		}

		protected virtual bool ShouldFillTransactionNumber => true;

		protected override void TearDown()
		{
			base.TearDown();

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		protected virtual void SetupForOutstandingAmountMatchingWithApprovalItemsTest()
		{
			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Header.AH_ExchangeRate = 0.5M;
			Header.AH_OSExTaxAmount = 22M;  // LocalExTax should be 44
			Header.AH_LocalOutstandingAmount = 10M;
		}

		protected void SetupNonCurrentBranch()
		{
			var filter = new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_Code);
			filter.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			NonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(filter));
			if (NonCurrentBranch == null)
			{
				NonCurrentBranch = Factory.NewWithValidTestData<GlbBranch>();
				NonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			}
		}

		protected void SetupNonCurrentDepartment()
		{
			var filter = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.GE_Code);
			NonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(filter));
			if (NonCurrentDepartment == null)
			{
				NonCurrentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			}
		}

		protected void SetupJob()
		{
			Job = Factory.NewJobForTesting<Job>();
			Job.JH_JobNum = "JobNumber";
			Job.JH_GB = GlbBranch.CurrentBranch.PK;
			Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job.JH_UniqueJobInvoiceNumber = ZShort.Zero;
		}

		protected void SetupGLAccount()
		{
			ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_AccountType, Constants.AccountType.ProfitAndLossAccount);
			GLAccount = Factory.LoadTop1<AccGLHeader>(filter);
		}

		protected void SetupBankAccount()
		{
			BankAccount = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
		}

		protected virtual void SetupOrganisations()
		{
			Organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Organisation.OH_IsDebtor = ZBool.True;
			Organisation.OH_IsCreditor = ZBool.True;

			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = (ZByte)7;

			Organisation.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;
			Organisation.CompanyData.OB_APPaymentTermDays = (ZByte)7;
		}

		protected override void SetupForSave()
		{
			base.SetupForSave();
			Header.AH_GB = GlbBranch.CurrentBranch.PK;
			Header.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Header.AH_InvoiceDate = ZDateTime.Now;
			Header.AH_PostDate = ZDateTime.Now;
			Header.AH_Ledger = Header.Ledger_ForTestOnly;
			Header.PostPeriod = PeriodManagementTestHelper.CurrentPeriod.AM_Period;
		}

		protected void SetupHeaderExRatesAndAmounts(ZDecimal exchangeRate, ZInt currencySubUnitRatio, ZDecimal aH_OSExTaxAmount, ZDecimal oSTaxAmount)
		{
			LoadForeignCurrency(currencySubUnitRatio);
			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Header.AH_ExchangeRate = exchangeRate;
			Header.AH_OSExTaxAmount = aH_OSExTaxAmount;
			Header.AH_OSTaxAmount = oSTaxAmount;
		}

		protected void LoadForeignCurrency(int subUnitRatio)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			filter.AddToFilter(JoinCondition.And, RefCurrencySchema.RX_SubUnitRatio, SQLComparisonOperator.Equal, subUnitRatio);

			RefCurrency[] foreignCurrencies = (RefCurrency[])newFactory.Load(typeof(RefCurrency), filter);
			if (foreignCurrencies.Length > 0)
			{
				ForeignCurrency = foreignCurrencies[0];
			}
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, string transactionNumber, decimal amount)
		{
			AccTransactionHeader testHeader = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testHeader.AH_Ledger = ledger;
			testHeader.AH_TransactionType = transactionType;
			testHeader.AH_TransactionNum = transactionNumber;
			testHeader.AH_InvoiceAmount = amount;
			testHeader.AH_OutstandingAmount = amount;
			testHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			testHeader.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			return testHeader;
		}

		TestObjectCreator fTestDataCreator;
		protected TestObjectCreator TestDataCreator
		{
			get
			{
				if (fTestDataCreator == null)
				{
					fTestDataCreator = new TestObjectCreator(Factory);
				}
				return fTestDataCreator;
			}
		}

		// used in IMatching partial payment test cases
		protected ZDecimal AmountWithMultiplier
		{
			get { return (Header.InvertSigns_ForTestOnly ? (ZDecimal)(-fAmountWithMultiplier) : fAmountWithMultiplier); }
			set { fAmountWithMultiplier = value; }
		}
		ZDecimal fAmountWithMultiplier;

		#region Special treatment for Miscellaneous transactions that cannot be saved with a non zerooutstanding amount

		protected bool IsMiscellaneousTransaction
		{
			get { return IsMiscellaneousTransactionCore(Header); }
		}

		bool IsMiscellaneousTransactionCore(TransactionHeader header)
		{
			return (header.AH_Ledger == LedgerTypes.AccountsReceivable || header.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(header.AH_TransactionType == TransactionTypes.ExchangeDifference || header.AH_TransactionType == TransactionTypes.Discount || header.AH_TransactionType == TransactionTypes.Overpayment);
		}

		protected void PrepareMiscellaneousTransactionForSaving()
		{
			// Need to create a Journal to match it because we cannot save miscellaneous transaction with a non zero outstanding amount
			if (!Header.AH_OutstandingAmount.IsEmpty)
			{
				PrepareMiscellaneousTransactionForSavingCore(Header);
			}
		}

		void PrepareMiscellaneousTransactionForSavingCore(TransactionHeader header)
		{
			if (IsMiscellaneousTransactionCore(header))
			{
				AccTransactionHeader journalToMatch = Factory.New<AccTransactionHeader>();

				journalToMatch.AH_InvoiceDate = DateTime.Now;
				journalToMatch.AH_Ledger = header.AH_Ledger;
				journalToMatch.AH_TransactionType = TransactionTypes.Journal;
				journalToMatch.AH_RX_NKTransactionCurrency = header.AH_RX_NKTransactionCurrency;
				journalToMatch.AH_ExchangeRate = header.AH_ExchangeRate;
				journalToMatch.AH_InvoiceAmount = -header.AH_InvoiceAmount;
				journalToMatch.AH_GSTAmount = -header.AH_GSTAmount;
				journalToMatch.AH_OutstandingAmount = -header.AH_OutstandingAmount;
				journalToMatch.AH_PostDate = header.AH_PostDate;
				journalToMatch.AH_GB = header.AH_GB;
				journalToMatch.AH_GE = header.AH_GE;
				journalToMatch.AH_TransactionNum = header.AH_TransactionNum + "J";

				AccTransactionMatchLink link1 = Factory.New<AccTransactionMatchLink>();
				AccTransactionMatchLink link2 = Factory.New<AccTransactionMatchLink>();

				link1.AP_AH = header.PK;
				link1.AP_Amount = header.AH_OutstandingAmount;
				link2.AP_AH = journalToMatch.PK;
				link2.AP_Amount = journalToMatch.AH_OutstandingAmount;
				link1.AP_MatchDate = link2.AP_MatchDate = header.AH_PostDate;
				link1.AP_MatchGroupNum = link1.AP_MatchGroupNum = "100100";

				TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
				matchlinks.Add(link1);
				matchlinks.Add(link2);
				header.AH_OutstandingAmount = 0m;
				journalToMatch.AH_OutstandingAmount = 0m;
			}
		}

		#endregion

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}
}
