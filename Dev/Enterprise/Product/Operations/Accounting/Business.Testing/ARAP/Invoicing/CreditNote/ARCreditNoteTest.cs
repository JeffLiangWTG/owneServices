using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNote))]
	public class ARCreditNoteTest : CreditNoteTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARCreditNote>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestShouldShowOriginalInvoiceReferenceFields()
		{
			AssertEquals("ShouldShowOriginalInvoiceReferenceFields should be true.", true, InvoicingBase.ShouldShowOriginalInvoiceReferenceFields);
		}

		public override void TestShouldShowOriginalInvoiceReferenceReasonFields()
		{
			AssertEquals("ShouldShowOriginalInvoiceReferenceReasonFields should be true.", true, InvoicingBase.ShouldShowOriginalInvoiceReferenceReasonFields);
		}

		public override void TestImportSingleCostPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestImportAllApportionmentsFromCostingPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public void TestLoadRelatedTransactionWithTaxID()
		{
			ChargeCode.AC_AT_GSTRate = TaxRate.PK;
			Factory.Save();

			DebtorOrg.CompanyData.SetARTaxApplicable(ZBool.True);
			DebtorOrg.CompanyData.SetAPTaxApplicable(ZBool.True);

			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine aRInvoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			aRInvoice.Lines.Add(aRInvoiceLine);
			aRInvoice.AH_OH = DebtorOrg.PK;

			aRInvoiceLine.AL_AC = ChargeCode.PK;
			aRInvoiceLine.AL_AT = TaxRate.PK;
			aRInvoiceLine.AL_TaxDate = ZDate.Today.AddDays(-3);
			aRInvoiceLine.AL_A9_VATClass = TaxMessage.PK;
			aRInvoiceLine.AL_LineAmount = 150m;
			aRInvoiceLine.AL_OSTaxAmount = 150m;
			aRInvoiceLine.AL_LocalTaxAmount = 150m;
			aRInvoiceLine.AL_GSTVAT = 150m;

			InvoicingBase invoicingBase = (InvoicingBase)Factory.New(GetExpectedBusinessObjectType());
			invoicingBase.OriginalTransactionReference = aRInvoice.PK;

			Assert(aRInvoiceLine.AL_LineAmount != 0m);
			Assert(aRInvoiceLine.AL_OSTaxAmount != 0m);

			AssertEquals(1, invoicingBase.Lines.Count);

			AssertEquals(aRInvoiceLine.AL_AT, invoicingBase.Lines[0].AL_AT);
			AssertEquals(aRInvoiceLine.AL_TaxDate, invoicingBase.Lines[0].AL_TaxDate);
			AssertEquals(aRInvoiceLine.AL_A9_VATClass, invoicingBase.Lines[0].AL_A9_VATClass);
		}

		public void TestExchangeRatesHeaderAndLine()
		{
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			TestObjectCreator.CreateExchangeRate(uSDCurrency, "SEL", 0.71m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			TestObjectCreator.CreateExchangeRate(uSDCurrency, "SEL", 0.698m, ZDateTime.Today.AddDays(0), ZDateTime.Today.AddDays(0));

			TestObjectCreator.CreateExchangeRate(uSDCurrency, "BUY", 0.71m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			TestObjectCreator.CreateExchangeRate(uSDCurrency, "BUY", 0.698m, ZDateTime.Today.AddDays(0), ZDateTime.Today.AddDays(0));

			SetupForSave();
			InvoicingBase.AH_RX_NKTransactionCurrency = "USD";
			InvoicingBase.Lines[0].AL_OSExTaxAmount = 1000m;
			InvoicingBase.Lines[1].AL_OSExTaxAmount = 1000m;
			InvoicingBase.AH_PostDate = ZDateTime.Today.AddDays(-1);
			InvoicingBase.IsDisbursementOrFinal = false;
			Factory.Save();

			var postingExRateRegistry = PostingExRateRegistry;
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");

			var creditNote = Factory.New(GetExpectedBusinessObjectType()) as CreditNote;
			creditNote.AH_ExchangeRate = 0.58m;
			creditNote.AH_RX_NKTransactionCurrency = "USD";
			creditNote.AH_PostDate = ZDateTime.Today.AddDays(0);

			creditNote.OriginalTransactionReference = InvoicingBase.PK;
			AssertEquals("Exchange rates of the header and line match [DEF] ?", creditNote.Lines[0].AL_ExchangeRate, creditNote.AH_ExchangeRate);

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			InvoicingBase creditNotePST = (InvoicingBase)Factory.New(GetExpectedBusinessObjectType());
			creditNotePST.AH_RX_NKTransactionCurrency = "USD";
			creditNotePST.AH_PostDate = ZDateTime.Today.AddDays(0);
			creditNotePST.OriginalTransactionReference = InvoicingBase.PK;

			AssertEquals("Exchange rates of the header and line match [PST]?", creditNotePST.Lines[0].AL_ExchangeRate, creditNotePST.AH_ExchangeRate);

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");

			InvoicingBase creditNoteTOD = (InvoicingBase)Factory.New(GetExpectedBusinessObjectType());
			creditNoteTOD.AH_RX_NKTransactionCurrency = "USD";
			creditNoteTOD.AH_PostDate = ZDateTime.Today.AddDays(0);
			creditNoteTOD.OriginalTransactionReference = InvoicingBase.PK;

			AssertEquals("Exchange rates of the header and line match [TOD]?", creditNoteTOD.Lines[0].AL_ExchangeRate, creditNoteTOD.AH_ExchangeRate);
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");

			InvoicingBase creditNoteINV = (InvoicingBase)Factory.New(GetExpectedBusinessObjectType());
			creditNoteINV.AH_RX_NKTransactionCurrency = "USD";
			creditNoteINV.AH_PostDate = ZDateTime.Today.AddDays(0);
			creditNoteINV.OriginalTransactionReference = InvoicingBase.PK;

			AssertEquals("Exchange rates of the header and line match [INV]?", creditNoteINV.Lines[0].AL_ExchangeRate, creditNoteINV.AH_ExchangeRate);
		}

		public void TestIsAmendingOnceSaved()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();

			Factory.Save();

			var creditNoteReloaded = new BusinessObjectFactory().Load<ARCreditNote>(creditNote.PK);
			Assert("Both manually created or from job billing, amending transactions should stay this way after saving and print original transaction details on documents", creditNoteReloaded.IsAmendingTransaction);
		}

		public void TestTransactionReference()
		{
			// Let's have original Reference
			SetupForSave(); //Lines are already added in setup
			InvoicingBase.Lines[0].AL_LineAmount = 30;
			InvoicingBase.Lines[1].AL_LineAmount = 140;

			InvoicingBase.Lines[0].AL_OSAmount = 30;
			InvoicingBase.Lines[1].AL_OSAmount = 140;

			Factory.Save();

			var creditNote = Factory.New(GetExpectedBusinessObjectType()) as ARCreditNote;
			creditNote.OriginalTransactionReference = InvoicingBase.PK;
			AssertEquals("Line Count should be 2", 2, creditNote.Lines.Count);

			creditNote.OriginalTransactionReference = InvoicingBase.PK;
			Assert(!creditNote.AH_OriginalInvoiceDate.IsEmpty);
			Assert(!creditNote.AH_OriginalTransactionNum.IsEmpty);
			Assert(creditNote.IsAmendingTransaction_StrongReference);
			AssertEquals("Line Count should still be 2", 2, creditNote.Lines.Count);

			var startDate = creditNote.AH_OriginalReferenceStartDate = ZDate.Today;
			var endDate = creditNote.AH_OriginalReferenceEndDate = startDate.AddDays(1);
			creditNote.OriginalTransactionReference = ZGuid.Empty;
			AssertEquals("OriginalReferenceStartDate should be unchanged", startDate, creditNote.AH_OriginalReferenceStartDate);
			AssertEquals("OriginalReferenceEndDate should be unchanged", endDate, creditNote.AH_OriginalReferenceEndDate);

			creditNote.OriginalTransactionReference = InvoicingBase.PK;
			AssertEquals("OriginalReferenceStartDate should be unchanged", startDate, creditNote.AH_OriginalReferenceStartDate);
			AssertEquals("OriginalReferenceEndDate should be unchanged", endDate, creditNote.AH_OriginalReferenceEndDate);

			using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
			{
				creditNote.OriginalTransactionReference = InvoicingBase.PK;
				Assert("OriginalReferenceStartDate should be empty", creditNote.AH_OriginalReferenceStartDate.IsEmpty);
				Assert("OriginalReferenceEndDate should be empty", creditNote.AH_OriginalReferenceEndDate.IsEmpty);
			}
		}

		public void TestExchangeRateUpdatedFromTransactionReferenceForLocalCurrency()
		{
			var postingExRateRegistry = PostingExRateRegistry;
			postingExRateRegistry.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, 2.37M, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			var orgHeader = TestObjectCreator.CreateOrgHeader("Org1", true, true);
			orgHeader.CompanyData.OB_RX_NKARDDefltCurrency = TestObjectCreator.USD.Code;
			orgHeader.CompanyData.OB_RX_NKAPDefltCurrency = TestObjectCreator.USD.Code;

			var originalTransaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), currency: TestObjectCreator.AUD, organisation: orgHeader);
			var type = GetExpectedBusinessObjectType();
			var creditNote = TestObjectCreator.CreateInvoice(type, organisation: TestObjectCreator.AALSHI) as ARCreditNote;

			creditNote.SubmittedFromInvoicingForm = true;
			creditNote.AH_OH = orgHeader.PK;
			AssertEquals("Transaction Currency", orgHeader.CompanyData.OB_RX_NKARDDefltCurrency, creditNote.AH_RX_NKTransactionCurrency);

			creditNote.OriginalTransactionReference = originalTransaction.PK;
			AssertEquals("Currency", TestObjectCreator.AUD.Code, creditNote.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", new ZDecimal(1), creditNote.AH_ExchangeRate);
		}

		public void TestAH_InvoiceTerm_ReadOnly()
		{
			IAmending original = InvoicingBase as IAmending;
			AssertNotNull("Should be IAmending", original);
			Assert("Should not be AmendingTransaction", !original.IsAmendingTransaction);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = true;
			AssertEquals(false, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			var amending = original.GenerateAmendingTransaction(InvoicingBase.AH_TransactionType) as InvoicingBase;
			AssertNotNull("Should generate Amending transaction", amending);
			Assert("Should be AmendingTransaction", amending.IsAmendingTransaction);

			var shipment = TestObjectCreator.CreateShipment("1001");
			amending.AH_JH = Job.PK;
			amending.InvoicingJob.PlugInData = shipment;

			var invoicingBase = amending.GetType().BaseType.BaseType;
			var info = invoicingBase.GetProperty("SecurityHelper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var securityTestHelper = info.GetValue(amending, null) as JobInvoicingSecurityHelper;
			var securityCheckPoint = securityTestHelper.GetInvSecurity(SecurityCore.AllowOverrideARInvoiceTermWAmendTransactionWCreditNote);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			AssertEquals(false, amending.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			AssertEquals(true, amending.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			AssertEquals(true, amending.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			AssertEquals(true, amending.AH_InvoiceTermInfo.ReadOnly);
		}

		public void TestAH_InvoiceTermDays_ReadOnly()
		{
			IAmending original = InvoicingBase as IAmending;
			AssertNotNull("Should be IAmending", original);
			Assert("Should not be AmendingTransaction", !original.IsAmendingTransaction);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(false, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesCreditNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			var amending = original.GenerateAmendingTransaction(InvoicingBase.AH_TransactionType) as InvoicingBase;
			AssertNotNull("Should generate Amending transaction", amending);
			Assert("Should be AmendingTransaction", amending.IsAmendingTransaction);

			var shipment = TestObjectCreator.CreateShipment("1001");
			amending.AH_JH = Job.PK;
			amending.InvoicingJob.PlugInData = shipment;

			var invoicingBase = amending.GetType().BaseType.BaseType;
			var info = invoicingBase.GetProperty("SecurityHelper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var securityTestHelper = info.GetValue(amending, null) as JobInvoicingSecurityHelper;
			var securityCheckPoint = securityTestHelper.GetInvSecurity(SecurityCore.AllowOverrideARInvoiceTermWAmendTransactionWCreditNote);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(false, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = true;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			securityCheckPoint.IsAllowed = false;
			amending.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, amending.AH_InvoiceTermDaysInfo.ReadOnly);
		}

		public void TestAH_InvoiceDate_ReadOnly()
		{
			var original = InvoicingBase as IAmending;
			AssertNotNull("Should be IAmending.", original);
			Assert("Should not be AmendingTransaction.", !original.IsAmendingTransaction);

			Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed = false;
			Assert("Should not be readonly as the security right is false.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

			var amending = original.GenerateAmendingTransaction(InvoicingBase.AH_TransactionType) as InvoicingBase;
			AssertNotNull("Should generate Amending transaction.", amending);
			Assert("Should be AmendingTransaction.", amending.IsAmendingTransaction);

			var shipment = TestObjectCreator.CreateShipment("1001");
			amending.AH_JH = Job.PK;
			amending.InvoicingJob.PlugInData = shipment;

			Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !amending.AH_InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed = false;
			Assert("Should be readonly as the security right is false.", amending.AH_InvoiceDateInfo.ReadOnly);

			using (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code))
			{
				Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed = true;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

				Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed = false;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);
			}
		}

		[TestDate(2012, 03, 01)]
		public override void TestAH_InvoiceDate()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "test001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			TestObjectCreator.CreateJob(shipment);
			Factory.Save();
			var cost = InvoicingBase.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);

			var today = ZDateTime.Today;
			InvoicingBase.AH_InvoiceTerm = InvoiceTerms.FromInvoiceDate;
			InvoicingBase.AH_InvoiceTermDays = 7;
			InvoicingBase.OriginalTransactionReference = Guid.NewGuid();
			InvoicingBase.AH_ReceiptType = "IDE";
			InvoicingBase.AH_InvoiceDate = today.AddDays(3);
			AssertEquals("AH_DueDate", today.AddDays(10), InvoicingBase.AH_DueDate);
			AssertEquals("E6_InvoiceDate", InvoicingBase.AH_InvoiceDate, cost.E6_InvoiceDate);
			Assert("AH_OriginalReferenceStartDate empty", InvoicingBase.AH_OriginalReferenceStartDate.IsEmpty);
			Assert("AH_OriginalReferenceEndDate empty", InvoicingBase.AH_OriginalReferenceEndDate.IsEmpty);

			using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: true))
			{
				InvoicingBase.AH_InvoiceDate = today.AddDays(4);
				Assert("AH_OriginalReferenceStartDate empty in PT and OrigRef / Reason set", InvoicingBase.AH_OriginalReferenceStartDate.IsEmpty);
				Assert("AH_OriginalReferenceEndDate empty in PT and OrigRef / Reason set", InvoicingBase.AH_OriginalReferenceEndDate.IsEmpty);

				InvoicingBase.OriginalTransactionReference = Guid.Empty;
				InvoicingBase.AH_InvoiceDate = today.AddDays(5);
				Assert("AH_OriginalReferenceStartDate empty in PT and Reason set", InvoicingBase.AH_OriginalReferenceStartDate.IsEmpty);
				Assert("AH_OriginalReferenceEndDate empty in PT and Reason set", InvoicingBase.AH_OriginalReferenceEndDate.IsEmpty);

				InvoicingBase.AH_ReceiptType = ZString.Empty;
				InvoicingBase.AH_InvoiceDate = today.AddDays(6);
				AssertEquals("AH_OriginalReferenceStartDate in PT", InvoicingBase.AH_InvoiceDate, InvoicingBase.AH_OriginalReferenceStartDate);
				AssertEquals("AH_OriginalReferenceEndDate in PT", InvoicingBase.AH_InvoiceDate, InvoicingBase.AH_OriginalReferenceEndDate);
			}
		}

		public void TestStampDutyCalculation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCountry("IT");

			AccTaxRate taxRate;
			AccTaxRate taxRate2;
			AccInvMsg taxInvMsg1;
			AccInvMsg taxInvMsg2;
			AccChargeCode chargeCode;
			SetupStampDutyCalculationTaxIDsAndRegistry(creator, out taxRate, out taxRate2, out chargeCode, out taxInvMsg1, out taxInvMsg2);

			AccGLHeader glHeader = creator.GLHeader1;

			AssertStampDutyCalculation(30m, 50m, true, true, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, -50m, true, true, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, 40m, false, false, taxRate, taxRate2, chargeCode, glHeader, null);
		}

		public void TestStampDutyCalculationEventOnly()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCountry("IT");

			AccTaxRate taxRate;
			AccTaxRate taxRate2;
			AccInvMsg taxInvMsg1;
			AccInvMsg taxInvMsg2;
			AccChargeCode chargeCode;
			SetupStampDutyCalculationTaxIDsAndRegistry(creator, out taxRate, out taxRate2, out chargeCode, out taxInvMsg1, out taxInvMsg2);

			AccGLHeader glHeader = creator.GLHeader1;

			AssertStampDutyCalculation(30m, 50m, true, false, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, -50m, true, false, taxRate, taxRate2, chargeCode, glHeader, taxInvMsg2);
			AssertStampDutyCalculation(30m, 40m, false, false, taxRate, taxRate2, chargeCode, glHeader, null);
		}

		public void TestStampDutyCalculationValidation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			GlbCompany.CurrentCompany.SetCountry("IT");

			AccTaxRate taxRate;
			AccTaxRate taxRate2;
			AccInvMsg taxInvMsg1;
			AccInvMsg taxInvMsg2;
			AccChargeCode chargeCode;
			SetupStampDutyCalculationTaxIDsAndRegistry(creator, out taxRate, out taxRate2, out chargeCode, out taxInvMsg1, out taxInvMsg2);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			AccGLHeader glHeader = creator.GLHeader1;

			AssertStampDutyCalculationValidation(30m, 50m, true, taxRate, taxRate2, chargeCode, glHeader);
			AssertStampDutyCalculationValidation(30m, 40m, false, taxRate, taxRate2, chargeCode, glHeader);
		}

		void AssertStampDutyCalculationValidation(decimal line1LocalExTaxAmount, decimal line2LocalExTaxAmount, bool shouldHaveValidationError, AccTaxRate taxRate, AccTaxRate taxRate2, AccChargeCode chargeCode, AccGLHeader glHeader)
		{
			ARCreditNote invoice = Factory.New<ARCreditNote>();
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_RL_NKClosestPort = "ITROM";
			invoice.AH_OH = orgHeader.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = glHeader.PK;
			line.AL_LocalExTaxAmount = line1LocalExTaxAmount;
			line.AL_AT = taxRate.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.AL_AG = glHeader.PK;
			line2.AL_LocalExTaxAmount = line2LocalExTaxAmount;
			line2.AL_AT = taxRate2.PK;

			if (shouldHaveValidationError)
			{
				invoice.RunPreSaveValidation();
				AssertEquals("Invoice should have errors", true, invoice.HasErrors);
				AssertEquals("Invoice should have error", true, invoice.Notifications.ContainsNotificationContaining("The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code."));
			}
			else
			{
				invoice.RunPreSaveValidation();
				AssertEquals("Invoice should not have error", false, invoice.Notifications.ContainsNotificationContaining("The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code."));
			}
		}

		public void TestNoDBHitsForGettingApprovalRequestOfNewARCreditNote()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);

			var creditNote = (InvoicingBase)((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			var request = Factory.New<ARCreditNoteApprovalRequest>();
			request.XP_ParentID = invoice.PK;
			request.XP_ParentTableCode = invoice.TablePrefix;
			request.XP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;

			Factory.ClearQueryCache();
			int hitCountBeforeApprovalLoaded = Factory.GetTableHitCount(GenApprovalRequestSchema.Constants.TableName);
			AssertEquals("Precondition: CreditNote is not yet saved", false, creditNote.IsInDatabase);
			AssertEquals("Precondition: Parent invoice is not yet saved", false, invoice.IsInDatabase);
			AssertEquals("Should found the approval request", request.PK, creditNote.TransactionRelatedApprovalRequest.PK);
			int hitCountAfterApprovalLoaded = Factory.GetTableHitCount(GenApprovalRequestSchema.Constants.TableName);
			AssertEquals("No DB Hits", 0, hitCountAfterApprovalLoaded - hitCountBeforeApprovalLoaded);
		}

		public void TestReasonFields_ReadOnly()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD1", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			Assert("reason fields read only when Original transaction is empty", creditNote.ReasonCodeInfo.ReadOnly);
			Assert("reason fields read only when Original transaction is empty", creditNote.ReasonDescriptionInfo.ReadOnly);

			creditNote.OriginalTransactionReference = invoice.PK;
			Assert("reason code is available when Original transaction is filled", !creditNote.ReasonCodeInfo.ReadOnly);
			Assert("reason code is empty", creditNote.ReasonCode.IsEmpty);
			Assert("reason description is not available when reason code is empty", creditNote.ReasonDescriptionInfo.ReadOnly);

			creditNote.ReasonCode = "IDE";
			Assert(!creditNote.ReasonCodeInfo.ReadOnly);
			Assert("reason description is not available when reason code is not TXT", creditNote.ReasonDescriptionInfo.ReadOnly);

			creditNote.ReasonCode = "TXT";
			Assert(!creditNote.ReasonCodeInfo.ReadOnly);
			Assert("reason description is available only when reason code is TXT", !creditNote.ReasonDescriptionInfo.ReadOnly);
		}

		public void TestReasonFields_WhenReasonCodeIsDeletedFromRegistry()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			creditNote.OriginalTransactionReference = invoice.PK;
			creditNote.ReasonCode = "TXT";
			Factory.Save();

			Assert(creditNote.IsInDatabase);
			AssertEquals("TXT", creditNote.ReasonCode);
			AssertEquals("Free Text", creditNote.ReasonDescription);
			AssertContainsExactElementsInAnyOrder("default reason codes", new[] { "IDE", "IAM", "TXT" }, creditNote.ReasonCodes.GetAllCodes());

			var reasonCodes = new CodeDescriptionPairList();
			reasonCodes.Add(new CodeDescriptionPair("AAA", "test aaa"));
			reasonCodes.Add(new CodeDescriptionPair("BBB", "test bbb"));
			AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasonCodes);

			var creditNoteReloaded = new BusinessObjectFactory().Load<ARCreditNote>(creditNote.PK);
			AssertContainsExactElementsInAnyOrder("PreCondition: default reasonCodes doesn't have TXT anymore", new[] { "AAA", "BBB" }, creditNoteReloaded.ReasonCodes.GetAllCodes());

			AssertEquals("TXT", creditNoteReloaded.ReasonCode);
			AssertEquals("Free Text", creditNoteReloaded.ReasonDescription);
		}

		public void TestReasonFields_DescriptionIsFilledAutomatically()
		{
			var creditNote1 = CreateCreditNoteWithOriginalTransactionReference("001");
			creditNote1.ReasonCode = "TXT";
			AssertEquals("reason description is automatically filled", "Free Text", creditNote1.ReasonDescription);

			creditNote1.ReasonCode = "IDE";
			AssertEquals("reason description is automatically filled", "Incorrect Data Entry", creditNote1.ReasonDescription);

			creditNote1.ReasonCode = string.Empty;
			AssertEquals("reason description is reset when code is reset", string.Empty, creditNote1.ReasonDescription);

			creditNote1.ReasonCode = "TXT";
			AssertEquals("Free Text", creditNote1.ReasonDescription);
			creditNote1.ReasonDescription = "FREE TEXT 2";
			AssertEquals("updating reason description does not change the reason code", "TXT", creditNote1.ReasonCode);
			AssertEquals("FREE TEXT 2", creditNote1.ReasonDescription);
		}

		public void TestReasonFields_CodeIsRetrievedFromSavedDescription()
		{
			AssertTest("001", "IDE", "Incorrect Data Entry");
			AssertTest("002", "IAM", "Incorrect Amounts");
			AssertTest("003", "TXT", "Random description");

			void AssertTest(string transactionNumber, string code, string description)
			{
				var creditNote = CreateCreditNoteWithOriginalTransactionReference("001");
				creditNote.ReasonCode = code;
				creditNote.ReasonDescription = description;
				Factory.Save();

				var reloadedCreditNote = new BusinessObjectFactory().Load<ARCreditNote>(creditNote.PK);
				AssertEquals(code, creditNote.ReasonCode);
				AssertEquals(description, creditNote.ReasonDescription);
			}
		}

		public void TestReasonFields_ResetWhenOriginalTransactionReferenceIsChanged()
		{
			var creditNote1 = CreateCreditNoteWithOriginalTransactionReference("001");
			creditNote1.ReasonCode = "IAM";

			AssertEquals("IAM", creditNote1.ReasonCode);
			AssertEquals("Incorrect Amounts", creditNote1.ReasonDescription);

			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV002", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			creditNote1.OriginalTransactionReference = invoice2.PK;
			AssertEquals("reason fields reset when OriginalTransactionReference changed to another invoice", string.Empty, creditNote1.ReasonCode);
			AssertEquals("reason fields reset when OriginalTransactionReference changed to another invoice", string.Empty, creditNote1.ReasonDescription);

			creditNote1.ReasonCode = "IAM";
			AssertEquals("IAM", creditNote1.ReasonCode);
			AssertEquals("Incorrect Amounts", creditNote1.ReasonDescription);

			creditNote1.OriginalTransactionReference = ZGuid.Empty;
			AssertEquals("reason fields reset when OriginalTransactionReference changed to empty", string.Empty, creditNote1.ReasonCode);
			AssertEquals("reason fields reset when OriginalTransactionReference changed to empty", string.Empty, creditNote1.ReasonDescription);
		}

		public void TestReasonFields_Saving()
		{
			var creditNote1 = CreateCreditNoteWithOriginalTransactionReference("001");
			creditNote1.ReasonCode = "TXT";
			creditNote1.ReasonDescription = string.Empty;
			Factory.Save();

			AssertReasonCodeAndDescription("reason fields are not saved if description is missing", creditNote1.PK, string.Empty, string.Empty);

			var creditNote2 = CreateCreditNoteWithOriginalTransactionReference("002");
			creditNote2.ReasonCode = string.Empty;
			creditNote2.ReasonDescription = "reason desc";
			Factory.Save();

			AssertReasonCodeAndDescription("reason fields are not saved if code is missing", creditNote2.PK, string.Empty, string.Empty);
			AssertEquals("no GenAddOnColumn data created", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "CreditNoteReason")).Length);

			Assert(creditNote1.IsInDatabase);
			creditNote1.ReasonCode = "TXT";
			creditNote1.ReasonDescription = "new DESC";
			Factory.Save();

			AssertReasonCodeAndDescription("reason fields are not saved if credit note is already saved", creditNote1.PK, string.Empty, string.Empty);
			AssertEquals("no GenAddOnColumn data created", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "CreditNoteReason")).Length);

			var creditNote3 = CreateCreditNoteWithOriginalTransactionReference("003");
			creditNote3.ReasonCode = "TXT";
			creditNote3.ReasonDescription = "new DESC";
			Factory.Save();
			AssertReasonCodeAndDescription("reason fields are saved", creditNote3.PK, "TXT", "new DESC");

			var addOnColumns = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, InvoicingBase.GenAddOnColumnReasonName).AddToFilter(GenAddOnColumnSchema.XA_ParentID, creditNote3.PK));
			AssertEquals(1, addOnColumns.Length);
			AssertEquals("XA_ParentTableCode is AH", AccTransactionHeaderSchema.Constants.Prefix, addOnColumns[0].XA_ParentTableCode);
			AssertEquals(AddOnColumnDataType.Codes.String, addOnColumns[0].XA_Type);
			AssertEquals("TXT|new DESC", addOnColumns[0].XA_Data);

			addOnColumns[0].XA_Data = "dataWithoutSeparator";
			Factory.Save();
			AssertReasonCodeAndDescription("wrong XA_Data return empty values", creditNote3.PK, string.Empty, string.Empty);

			void AssertReasonCodeAndDescription(string message, ZGuid creditNotePK, string code, string description)
			{
				var reloadedCreditNote = new BusinessObjectFactory().Load<ARCreditNote>(creditNotePK);
				AssertEquals(message, code, reloadedCreditNote.ReasonCode);
				AssertEquals(message, description, reloadedCreditNote.ReasonDescription);
			}
		}

		public void TestReasonCodeSavingLocation()
		{
			var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			Assert("The reason code is saved in XA_Type, if the max length of XA_Type is lower than the max length of the reason code, then it won't work",
				GenAddOnColumnSchema.XA_Type.MaxLength >= creditNote.ReasonCodeInfo.MaxLength);
		}

		public void TestIsReasonFieldsMandatory()
		{
			AssertIsReasonFieldsMandatory(false, false);
			AssertIsReasonFieldsMandatory(true, true);

			void AssertIsReasonFieldsMandatory(bool getIsARInvoiceReasonFieldsMandatoryValue, bool expectedCreditNoteAreReasonFieldsMandatory)
			{
				using (InvoiceBaseValidationTest.InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: getIsARInvoiceReasonFieldsMandatoryValue))
				{
					var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
					AssertNoErrors(creditNote.ReasonCodeInfo);

					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
					creditNote.OriginalTransactionReference = invoice.PK;

					if (expectedCreditNoteAreReasonFieldsMandatory)
					{
						AssertHasError("reason fields are mandatory only for Portugal and when the Original Transaction Reference is set",
						creditNote.ReasonCodeInfo, "Please enter a value.");
					}
					else
					{
						AssertNoErrors(creditNote.ReasonCodeInfo);
					}
				}
			}
		}

		public void TestReasonFields_ReasonCodes()
		{
			AssertContainsExactElementsInAnyOrder("PreCondition: registry default values are IDE, IAM and TXT",
				new string[] { "IDE", "IAM", "TXT" }, AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.Cast<CodeDescriptionPair>().Select(x => x.Code));

			var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			AssertContainsExactElementsInAnyOrder(new string[] { "IDE", "IAM", "TXT" }, creditNote.ReasonCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		ARCreditNote CreateCreditNoteWithOriginalTransactionReference(string transactionNum)
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV" + transactionNum, TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
			var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD" + transactionNum, TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			creditNote.OriginalTransactionReference = invoice.PK;
			return creditNote;
		}

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return true; }
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(ARCreditNoteValidation); }
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(ARCreditNoteLine);
		}

		protected ARCreditNote CreditNote
		{
			get { return (ARCreditNote)InvoicingBase; }
		}

		protected override void AssertEmailSendStatusForUnpostedInvoice()
		{
			AssertEquals("No Email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNumberFountainWhenTransactionTypeChanges()
		{
			InvoicingBase.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("Should be AR Credit Note number fountain", Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(), ((ARCreditNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be AR Credit Note number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARCreditNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());

			ShareSequentialTransactionNumbers item = new ShareSequentialTransactionNumbers();
			item.Value = true;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertNotEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(), ((ARCreditNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARCreditNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			item.Value = false;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);

			AssertEquals("Should be AR Credit Note number fountain", Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(), ((ARCreditNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be AR Credit Note number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARCreditNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
		}

		public void TestTransactionReferenceDetails()
		{
			// Let's have original Reference
			SetupForSave(); //Lines are already added in setup
			InvoicingBase.AH_RX_NKTransactionCurrency = new TestObjectCreator(Factory).GBP.RX_Code;
			InvoicingBase.AH_ExchangeRate = 0.78m;

			InvoicingBase.Lines[0].AL_OSTaxAmount = 10m;
			InvoicingBase.Lines[0].AL_OSExTaxAmount = 140m;

			InvoicingBase.Lines[1].AL_OSTaxAmount = 5m;
			InvoicingBase.Lines[1].AL_OSExTaxAmount = 100m;

			Factory.Save();

			var newInvoicingBase = Factory.New(GetExpectedBusinessObjectType()) as ARCreditNote;
			newInvoicingBase.OriginalTransactionReference = InvoicingBase.PK;

			AssertEquals("Line Count should be 2", 2, newInvoicingBase.Lines.Count);

			AssertEquals(InvoicingBase.AH_RX_NKTransactionCurrency, newInvoicingBase.AH_RX_NKTransactionCurrency);
			AssertEquals(InvoicingBase.AH_ExchangeRate, newInvoicingBase.AH_ExchangeRate);
			AssertEquals(-InvoicingBase.AH_OSTotal, newInvoicingBase.AH_OSTotal);
			AssertEquals(-InvoicingBase.AH_OSTaxAmount, newInvoicingBase.AH_OSTaxAmount);
			AssertEquals(-InvoicingBase.AH_InvoiceAmount, newInvoicingBase.AH_InvoiceAmount);
			AssertEquals(-InvoicingBase.AH_GSTAmount, newInvoicingBase.AH_GSTAmount);
		}

		public void TestDocManagerCode()
		{
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			AssertEquals("Code should be RCR. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "RCR", ((IDocManagerSupport)creditNote).DocManagerInfo.DocManagerCode);
		}

		public override void TestInvoiceBatchNumber()
		{
			Header.AH_ReceiptBatchNo = "300";
			AssertEquals("Invoice Batch Number should be 300", "300", Header.InvoiceBatchNumber);
		}

		#region Reversing

		public void TestReversingShipmentInvoiceSetsJobInvoiceNumber()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "S00001000";

			CreditNote.AH_JH = testJob.PK;
			CreditNote.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(CreditNote, testJob);
			Factory.Save();
			AssertEquals("Job Invoice number should be first in sequence", "S00001000", CreditNote.AH_ConsolidatedInvoiceRef);

			ARCreditNote loadedCrd = new BusinessObjectFactory().Load<ARCreditNote>(CreditNote.PK);
			IReversing beingReversed = loadedCrd;
			beingReversed.GenerateReverseTransaction(true);
			ARInvoice reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertEquals("Job Invoice number should be second in sequence", "S00001000/A", reversingInv.AH_ConsolidatedInvoiceRef);
		}

		public void TestReversingShipmentInvoiceWithCustomisedShipmentNo()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "CTEST001";

			CreditNote.AH_JH = testJob.PK;
			CreditNote.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(CreditNote, testJob);
			Factory.Save();

			AssertEquals("Job Invoice number should be first in sequence", "CTEST001", CreditNote.AH_ConsolidatedInvoiceRef);

			ARCreditNote loadedCrd = new BusinessObjectFactory().Load<ARCreditNote>(CreditNote.PK);
			IReversing beingReversed = loadedCrd;
			beingReversed.GenerateReverseTransaction(true);
			ARInvoice reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertEquals("Job Invoice number should be second in sequence", "CTEST001/A", reversingInv.AH_ConsolidatedInvoiceRef);
		}

		public void TestReversingConsolInvoiceSetsJobInvoiceNumber()
		{
			ZString consolNum = "C00001000";
			CreditNote.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNum, CreditNote.PK);

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, CreditNote.AH_ConsolidatedInvoiceRef, aRCrd.PK);

			Factory.Save();
			AssertEquals("Job Invoice number should be C00001000/A", "C00001000/A", aRCrd.AH_ConsolidatedInvoiceRef);

			ARCreditNote loadedCrd = new BusinessObjectFactory().Load<ARCreditNote>(aRCrd.PK);
			IReversing beingReversed = loadedCrd;
			beingReversed.GenerateReverseTransaction(true);
			ARInvoice reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertEquals("Job Invoice number should be the next in sequence", "C00001000/B", reversingInv.AH_ConsolidatedInvoiceRef);
		}

		public void TestReversingConsolCreditNoteSetsJobInvoiceNumber_WithLongConsolNumber()
		{
			ZString consolNumber = string.Format("C{0}1", "".PadRight(JobConsolSchema.JK_UniqueConsignRef.MaxLength - 2, '0'));

			var unrelatedInvoiceWithSimilarJobInvNum = Factory.NewWithValidTestData<ARInvoice>();
			unrelatedInvoiceWithSimilarJobInvNum.AH_ConsolidatedInvoiceRef = consolNumber.Substring(0, 9);
			Factory.Save();

			var aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, aRInv.PK);
			Factory.Save();

			var aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, aRCrd.PK);
			Factory.Save();
			AssertEquals("AR Credit Note Job Invoice Number", consolNumber + "/A", aRCrd.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = aRCrd;
			beingReversed.GenerateReverseTransaction(true);

			ARInvoice reversingCrd = beingReversed.ReverseTransaction as ARInvoice;
			AssertEquals("Job Invoice Number on reversing Invoice", consolNumber + "/B", reversingCrd.AH_ConsolidatedInvoiceRef);
		}

		#endregion

		#region Bad Debt Writting Off

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestGenerateReverseTransaction_BadDebt()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "S00001000";

			CreditNote.AH_JH = testJob.PK;
			CreditNote.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(CreditNote, testJob);

			ZGuid expectedGenericCharge1 = ZGuid.NewZGuid();
			ZString expectedDesc1 = "Description 1";
			ZGuid expectedGenericCharge2 = ZGuid.NewZGuid();
			ZString expectedDesc2 = "Description 2";
			ZGuid expectedTax = TestObjectCreator.GST1.PK;
			ZGuid badDebtGenericCharge = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			CreditNote.Lines.RemoveAndDeleteAll();
			InvoicingLineBase line1 = (InvoicingLineBase)CreditNote.Lines.AddNew();
			line1.GenericCharge = expectedGenericCharge1;
			line1.AL_Desc = expectedDesc1;
			line1.AL_AT = expectedTax;

			InvoicingLineBase line2 = (InvoicingLineBase)CreditNote.Lines.AddNew();
			line2.GenericCharge = expectedGenericCharge2;
			line2.AL_Desc = expectedDesc2;

			Factory.Save();
			AssertEquals("Job Invoice number should be first in sequence.", "S00001000", CreditNote.AH_ConsolidatedInvoiceRef);

			IReversing beingReversed = CreditNote;
			beingReversed.GenerateReverseTransaction(true);
			ARInvoice reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertNotNull("Reversing transaction should be a CreditNote", reversingInv);
			AssertEquals("Job Invoice number on reversing creditnote should be second in sequence.", "S00001000/A", reversingInv.AH_ConsolidatedInvoiceRef);

			AssertEquals("Reversing Invoice should have GenericCharge the same as in source.", expectedGenericCharge1, reversingInv.Lines[0].GenericCharge);
			AssertEquals("Reversing Invoice should have GenericCharge the same as in source.", expectedGenericCharge2, reversingInv.Lines[1].GenericCharge);

			AssertEquals("Reversing Invoice line 1 should have Tax ID the same as in source.", expectedTax, reversingInv.Lines[0].AL_AT);
			AssertEquals("Reversing Invoice line 2 should have TAX ID the same as in source.", ZGuid.Empty, reversingInv.Lines[1].AL_AT);

			IBadDebtWritingOff badDebt = CreditNote;
			AssertNotNull(badDebt);
			badDebt.IsWritingOff = true;

			CreditNote.GenerateReverseTransaction(true);
			reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertNotNull("Reversing transaction should be a CreditNote", reversingInv);

			Factory.Save();

			AssertEquals("Reversing Invoice should have empty AH_JH.", ZGuid.Empty, reversingInv.AH_JH);
			AssertEquals("Job Invoice number on reversing creditnote should be empty.", "", reversingInv.AH_ConsolidatedInvoiceRef);

			AssertEquals("Reversing Invoice line 1 should have Bad Debt GenericCharge.", badDebtGenericCharge, reversingInv.Lines[0].GenericCharge);
			AssertNotEquals("Reversing Invoice line 1 should not have old description.", expectedDesc1, reversingInv.Lines[0].AL_Desc);
			AssertEquals("Reversing Invoice line 1 should have empty AL_JH.", ZGuid.Empty, reversingInv.Lines[0].AL_JH);

			AssertEquals("Reversing Invoice line 2 should have Bad Debt GenericCharge.", badDebtGenericCharge, reversingInv.Lines[1].GenericCharge);
			AssertNotEquals("Reversing Invoice line 2 should not have old description.", expectedDesc2, reversingInv.Lines[1].AL_Desc);
			AssertEquals("Reversing Invoice line 2 should have empty AL_JH.", ZGuid.Empty, reversingInv.Lines[1].AL_JH);

			AssertEquals("Reversing Invoice line 1 should have Tax ID the same as in source.", expectedTax, reversingInv.Lines[0].AL_AT);
			AssertEquals("Reversing Invoice line 2 should have TAX ID the same as in source.", ZGuid.Empty, reversingInv.Lines[1].AL_AT);

			AssertEquals("Reversing Invoice should have IsWrittingOff the same as in source.", true, reversingInv.IsWritingOff);
		}

		public void TestSetDescriptionWrittingOff()
		{
			ZString expectedDesc1 = "Description 1";
			ZString expectedDesc2 = "Description 2";
			ZString descriptionToSet = "Another Description";

			CreditNote.Lines.RemoveAndDeleteAll();
			InvoicingLineBase line1 = (InvoicingLineBase)CreditNote.Lines.AddNew();
			line1.AL_Desc = expectedDesc1;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;

			InvoicingLineBase line2 = (InvoicingLineBase)CreditNote.Lines.AddNew();
			line2.AL_Desc = expectedDesc2;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;

			Factory.Save();

			IReversing beingReversed = CreditNote;
			beingReversed.GenerateReverseTransaction(true);
			ARInvoice reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertNotNull("Reversing transaction should be a CreditNote.", reversingInv);

			((IReversing)reversingInv).SetDescription(descriptionToSet);
			AssertEquals("Reversing Invoice should have new Description", descriptionToSet, reversingInv.AH_Desc);
			AssertEquals("Reversing Invoice line 1 should have Description the same as in source.", expectedDesc1, reversingInv.Lines[0].AL_Desc);
			AssertEquals("Reversing Invoice line 2 should have Description the same as in source.", expectedDesc2, reversingInv.Lines[1].AL_Desc);

			IBadDebtWritingOff badDebt = CreditNote;
			AssertNotNull(badDebt);
			badDebt.IsWritingOff = true;

			beingReversed.GenerateReverseTransaction(true);
			reversingInv = beingReversed.ReverseTransaction as ARInvoice;
			AssertNotNull("Reversing transaction should be a CreditNote.", reversingInv);

			((IReversing)reversingInv).SetDescription(descriptionToSet);

			AssertEquals("Reversing Invoice should have new Description.", descriptionToSet, reversingInv.AH_Desc);
			AssertEquals("Reversing Invoice line 1 should have new Description.", descriptionToSet, reversingInv.Lines[0].AL_Desc);
			AssertEquals("Reversing Invoice line 2 should have new Description.", descriptionToSet, reversingInv.Lines[1].AL_Desc);
		}

		#endregion

		[TestDate(2019, 1, 1)]
		public void TestAddInvoiceApprovalLog()
		{
			Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
			Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
			TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 1000m, 2000m);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("20002002"), false, true, true);
			var creditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
			creditNote.AH_LocalExTaxAmount = 3000M;

			SecurityTestObject.CreateTestUser(true, "", "tst", "testoverrideuser", "password");
			var testUser = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tst");
			testUser.GS_FullName = "Test User";
			SecurityForTest testSecurity = new SecurityForTest(null, testUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.UserPK = testUser.PK.ToGuid();
			SecurityTestObject.CreateTestUser(true, "", "tp1", "testapprovinguser1", "password");
			var approvingUser1 = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tp1");
			approvingUser1.GS_FullName = "Approve One";
			SecurityTestObject.CreateTestUser(true, "", "tp2", "testapprovinguser2", "password");
			var approvingUser2 = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tp2");
			approvingUser2.GS_FullName = "Approve Two";
			SecurityTestObject.CreateTestUser(true, "", "tp3", "testapprovinguser3", "password");
			var approvingUser3 = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tp3");
			approvingUser3.GS_FullName = "Approve Three";
			SecurityTestObject.CreateTestUser(true, "", "tl1", "twologinuser1", "password");
			var twoLoginuser1 = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tl1");
			twoLoginuser1.GS_FullName = "Login One";
			SecurityTestObject.CreateTestUser(true, "", "tl2", "twologinuser2", "password");
			var twoLoginuser2 = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tl2");
			twoLoginuser2.GS_FullName = "Login Two";
			SecurityTestObject.CreateTestUser(true, "", "tl3", "twologinuser3", "password");
			var twoLoginuser3 = Factory.LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)"tl3");
			twoLoginuser3.GS_FullName = "Login Three";

			var dummyProviderForSingleLoginCredential = new DummySecurityOverrideProvider(AuthorizationMode.Codes.Default);
			var dummyProviderForTwoLoginCredentials = new DummySecurityOverrideProvider(AuthorizationMode.Codes.TwoApprovers);
			var dummyProviderForSequentialLoginCredentials = new DummySecurityOverrideProvider(AuthorizationMode.Codes.SequentialApprovers);
			creditNote.SecurityOverrideProvider = dummyProviderForSingleLoginCredential;
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			AssertEquals("No Logs should be created", 0, creditNote.Logs.GetAllLogs().Count);

			creditNote.ApprovingUserPKList = new List<ZGuid> { approvingUser1.PK, approvingUser2.PK };
			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			var log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("logs should be for users in ApprovingUserPKList. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by testapprovinguser1 (Approve One), testapprovinguser2 (Approve Two)", log.SL_Reference);
			AssertEquals("log event time should match post date when not set", ZDateTime.Now, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			((DummySecurityOverrideProvider)creditNote.SecurityOverrideProvider).UserSecurityOverride = testSecurity;
			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("added log should contain proper reference. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by testoverrideuser (Test User)", log.SL_Reference);
			AssertEquals("log event time should match post date when not set", ZDateTime.Now, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			dummyProviderForSingleLoginCredential.UserPKsForTwoCredentialLogin = new List<ZGuid> { twoLoginuser1.PK, twoLoginuser2.PK };
			dummyProviderForTwoLoginCredentials.UserPKsForTwoCredentialLogin = new List<ZGuid> { twoLoginuser1.PK, twoLoginuser2.PK };
			dummyProviderForSequentialLoginCredentials.UserPKsForTwoCredentialLogin = new List<ZGuid> { twoLoginuser1.PK, twoLoginuser2.PK };
			creditNote.ApprovalDate = new ZDateTime(2018, 12, 20);

			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			AssertEquals("Even though values have been added to UserPKsForTwoCredentialLogin, should only use these if requiresTwoApprovers is true", 1, creditNote.Logs.GetAllLogs().Count);
			log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("added log should contain proper reference. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by testoverrideuser (Test User)", log.SL_Reference);
			AssertEquals("log event time should match ApprovalDate when set", creditNote.ApprovalDate, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			creditNote.SecurityOverrideProvider = dummyProviderForTwoLoginCredentials;
			creditNote.ApprovalDate = new ZDateTime(2018, 12, 15);
			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("logs should be for users in ApprovingUserPKList. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by twologinuser1 (Login One), twologinuser2 (Login Two)", log.SL_Reference);
			AssertEquals("log event time should match ApprovalDate when set", creditNote.ApprovalDate, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			dummyProviderForTwoLoginCredentials.UserPKsForTwoCredentialLogin.Clear();
			creditNote.ApprovalDate = new ZDateTime(2018, 12, 10);
			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("logs should be for users in ApprovingUserPKList. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by testapprovinguser1 (Approve One), testapprovinguser2 (Approve Two)", log.SL_Reference);
			AssertEquals("log event time should match ApprovalDate when set", creditNote.ApprovalDate, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			dummyProviderForTwoLoginCredentials.UserPKsForTwoCredentialLogin = new List<ZGuid> { twoLoginuser1.PK, twoLoginuser2.PK, twoLoginuser3.PK };
			AssertExceptionThrown<InvalidOperationException>("Maximum of two users via login may approve a credit note. SQL function ARCreditNotesIssued supports 0, 1 and 2 approvers only.", () => creditNote.AddInvoiceApprovalLog_ForTestOnly());

			dummyProviderForTwoLoginCredentials.UserPKsForTwoCredentialLogin.Clear();
			creditNote.ApprovingUserPKList = new List<ZGuid> { approvingUser1.PK, approvingUser2.PK, approvingUser3.PK, twoLoginuser1.PK, twoLoginuser2.PK, twoLoginuser3.PK };
			AssertNoExceptionThrown("Maximum of six users via ApprovingUserPKList may approve a credit note. SQL function ARCreditNotesIssued supports 0, 1 and 2 approvers only.", () => creditNote.AddInvoiceApprovalLog_ForTestOnly());
			creditNote.ApprovingUserPKList = new List<ZGuid> { approvingUser1.PK, approvingUser2.PK, approvingUser3.PK, twoLoginuser1.PK, twoLoginuser2.PK, twoLoginuser3.PK, testUser.PK };
			AssertExceptionThrown<InvalidOperationException>("Maximum of six users via ApprovingUserPKList may approve a credit note. SQL function ARCreditNotesIssued supports 0, 1 and 2 approvers only.", () => creditNote.AddInvoiceApprovalLog_ForTestOnly());

			creditNote.Logs.RemoveAndDeleteAll();
			creditNote.SecurityOverrideProvider = dummyProviderForSequentialLoginCredentials;
			creditNote.ApprovalDate = new ZDateTime(2018, 12, 15);
			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("logs should be for users in ApprovingUserPKList. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by twologinuser1 (Login One), twologinuser2 (Login Two)", log.SL_Reference);
			AssertEquals("log event time should match ApprovalDate when set", creditNote.ApprovalDate, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			dummyProviderForSequentialLoginCredentials.UserPKsForTwoCredentialLogin.Clear();
			creditNote.ApprovingUserPKList = new List<ZGuid> { approvingUser1.PK, approvingUser2.PK };
			creditNote.ApprovalDate = new ZDateTime(2018, 12, 10);
			creditNote.AddInvoiceApprovalLog_ForTestOnly();
			log = creditNote.Logs.GetAllLogs()[0];
			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			AssertEquals("added log should be ATH", Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("logs should be for users in ApprovingUserPKList. Note that SQL function ARCreditNotesIssued depends on format of this field.", "Post authorized by testapprovinguser1 (Approve One), testapprovinguser2 (Approve Two)", log.SL_Reference);
			AssertEquals("log event time should match ApprovalDate when set", creditNote.ApprovalDate, log.SL_EventTime);
			creditNote.Logs.RemoveAndDeleteAll();

			dummyProviderForSequentialLoginCredentials.UserPKsForTwoCredentialLogin = new List<ZGuid> { twoLoginuser1.PK, twoLoginuser2.PK, twoLoginuser3.PK };
			AssertExceptionThrown<InvalidOperationException>("Maximum of two users via login may approve a credit note. SQL function ARCreditNotesIssued supports 0, 1 and 2 approvers only.", () => creditNote.AddInvoiceApprovalLog_ForTestOnly());

			dummyProviderForSequentialLoginCredentials.UserPKsForTwoCredentialLogin.Clear();
			creditNote.ApprovingUserPKList = new List<ZGuid> { approvingUser1.PK, approvingUser2.PK, approvingUser3.PK, twoLoginuser1.PK, twoLoginuser2.PK, twoLoginuser3.PK };
			AssertNoExceptionThrown("Maximum of six users via ApprovingUserPKList may approve a credit note. SQL function ARCreditNotesIssued supports 0, 1 and 2 approvers only.", () => creditNote.AddInvoiceApprovalLog_ForTestOnly());
			creditNote.ApprovingUserPKList = new List<ZGuid> { approvingUser1.PK, approvingUser2.PK, approvingUser3.PK, twoLoginuser1.PK, twoLoginuser2.PK, twoLoginuser3.PK, testUser.PK };
			AssertExceptionThrown<InvalidOperationException>("Maximum of six users via ApprovingUserPKList may approve a credit note. SQL function ARCreditNotesIssued supports 0, 1 and 2 approvers only.", () => creditNote.AddInvoiceApprovalLog_ForTestOnly());
		}

		sealed class DummySecurityOverrideProvider : SecurityOverrideProvider, ISecurityOverrideProviderSupportTwoApproverLogin
		{
			public DummySecurityOverrideProvider(ZString authorizationMode)
			{
				factory = new BusinessObjectFactory();
				var dummyUser = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "dmy"));
				if (dummyUser == null)
				{
					SecurityTestObject.CreateTestUser(true, "", "dmy", "dummyuser", "password");
				}
				RequiresSequentialApprovals = authorizationMode == AuthorizationMode.Codes.SequentialApprovers;
				RequiresTwoApprovers = authorizationMode == AuthorizationMode.Codes.TwoApprovers;
			}

			readonly BusinessObjectFactory factory;

			protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
			{
				throw new NotImplementedException();
			}

			protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
			{
				var dummyUser = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "dmy"));
				return new SecurityForTest(null, dummyUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			}

			public bool RequiresTwoApprovers { get; }
			public bool RequiresSequentialApprovals { get; set; }
			public List<ZGuid> UserPKsForTwoCredentialLogin { get; set; }
		}

		public void TestCheckLineLevelSecurityRightsForCreditNoteApprovalRespectEnforceTwoApproverSetting()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 1000m, 2000m, false, Constants.AuthorizationMode.Codes.TwoApprovers);
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("20002002"), false, true, true);
				var creditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, currentBranch, currentDepartment, 1000M);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, currentBranch, currentDepartment, 2000M);
				creditNote.TransactionsForAuthorisationCalculation.AddRange(creditNote.GetLineLevelTransactionsGroupedByBranchAndDept());
				var creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
				creditNote.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping, isMiscInvoice: true, isFromApprovalModule: false);
				AssertEquals(1, creditNoteApprovingUserMapping.Count);
				AssertEquals("When posting misc invoice, approving user should be blank even if user has right, because Enforce two approver is true.", Guid.Empty, creditNoteApprovingUserMapping.Values.First());

				creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
				creditNote.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping, isMiscInvoice: false, isFromApprovalModule: false);
				AssertEquals(1, creditNoteApprovingUserMapping.Count);
				AssertEquals("When posting from job, approving user should be blank even if user has right, because Enforce two approver is true.", Guid.Empty, creditNoteApprovingUserMapping.Values.First());

				creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
				creditNote.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping, isMiscInvoice: false, isFromApprovalModule: true);
				AssertEquals(1, creditNoteApprovingUserMapping.Count);
				AssertNotEquals("When acting from approval module, approving user should NOT be blank when use has right, it should ignore the Enforce two approver setting.", Guid.Empty, creditNoteApprovingUserMapping.Values.First());
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCheckLineLevelSecurityRightsForCreditNoteApprovalRequest()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FEADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);
				using (Env.SetTemporaryUserContext("newuser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("20002002"), false);
					var creditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
					TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 1000m, 2000m);
					TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FIADepartment.PK, 1000m, 2000m);
					TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FEADepartment.PK, 1000m, 2000m);

					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, currentBranch, currentDepartment, 1000M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, currentBranch, currentDepartment, 2000M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, currentBranch, currentDepartment, 3000M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FIADepartment.PK, 1000M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FIADepartment.PK, 2000M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FIADepartment.PK, 3000M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FEADepartment.PK, 10M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FEADepartment.PK, 20M);
					TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, TestObjectCreator.NonCurrentBranch.PK, TestObjectCreator.FEADepartment.PK, 30M);

					creditNote.TransactionsForAuthorisationCalculation.AddRange(creditNote.GetLineLevelTransactionsGroupedByBranchAndDept());
					var creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
					AssertEquals(false, creditNote.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping));
					AssertEquals(3, creditNoteApprovingUserMapping.Count);
					Assert(!creditNoteApprovingUserMapping.Values.All(x => x == Guid.Empty));
					AssertEquals(2, creditNoteApprovingUserMapping.Values.Count(x => x == Guid.Empty));
					AssertEquals(1, creditNoteApprovingUserMapping.Values.Count(x => x == staff.PK.ToGuid()));

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(currentDepartment, currentBranch, staff.PK, true);
					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, true);
					Env.Security.ResetData(null, staff.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), Env.CurrentCompanyPK, true);
					AssertEquals(true, creditNote.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping));
					AssertEquals(3, creditNoteApprovingUserMapping.Count);
					Assert(creditNoteApprovingUserMapping.Values.All(x => x == staff.PK.ToGuid()));
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCheckLevelSecurityRightsForCreditNoteApprovalRequest()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				bool useCurrentBranchDepartment = false;
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(100M, useCurrentBranchDepartment, false, false, true);
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(600M, useCurrentBranchDepartment, true, false, true);
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(700M, useCurrentBranchDepartment, true, false, true);
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(700M, useCurrentBranchDepartment, true, true, true);

				useCurrentBranchDepartment = true;
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(100M, useCurrentBranchDepartment, false, false, true);
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(600M, useCurrentBranchDepartment, true, false, true);

				AssertLevelSecurityRightsForCreditNoteApprovalRequest(700M, useCurrentBranchDepartment, true, false, false);
				AssertLevelSecurityRightsForCreditNoteApprovalRequest(700M, useCurrentBranchDepartment, true, true, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		void AssertLevelSecurityRightsForCreditNoteApprovalRequest(decimal stepAmount, bool useCurrentBranchDepartment, bool firstAmountLevelAllows, bool secondAmountLevelAllows, bool expectedValue)
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("20001002"), false);
			var creditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
			TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(job.Branch.PK, job.Department.PK, 1000m, 2000m);

			const string testUserCode = "tst";
			const string testUserLogin = "newuser";
			var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)testUserCode);
			if (user == null)
			{
				SecurityTestObject.CreateTestUser(false, Env.Security.ARCreditNoteApproval.Code, testUserCode, testUserLogin, "password");
			}

			using (Env.SetTemporaryUserContext(testUserLogin, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				for (int i = 1; i <= 3; i++)
				{
					var creditNoteForAuthorisationCalculation = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);

					if (useCurrentBranchDepartment)
					{
						creditNoteForAuthorisationCalculation.AH_GB = job.Branch.PK;
						creditNoteForAuthorisationCalculation.AH_GE = job.Department.PK;
					}
					else
					{
						creditNoteForAuthorisationCalculation.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
						creditNoteForAuthorisationCalculation.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
					}

					creditNoteForAuthorisationCalculation.AH_LocalExTaxAmount = stepAmount * i;
					creditNote.TransactionsForAuthorisationCalculation.Add(creditNoteForAuthorisationCalculation);
				}

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = firstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = secondAmountLevelAllows;

				var creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
				AssertEquals(expectedValue, creditNote.CheckLevelSecurityRightsForARCreditNote(out creditNoteApprovingUserMapping));
			}
		}

		public void TestAuthorisatinoRequiredBranchDepartmentWhenAmendingOriginalTransaction()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

				invoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
				invoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;

				AssertNotEquals(invoice.AH_GB, GlbBranch.CurrentBranch.PK);
				AssertNotEquals(invoice.AH_GE, GlbDepartment.CurrentDepartment.PK);

				invoice.FillWithValidTestData();
				InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 10m);

				var creditNote = (InvoicingBase)((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(((creditNote as IAmending).OriginalTransaction as TransactionHeader).Branch.PK, ((creditNote as IAmending).OriginalTransaction as TransactionHeader).Department.PK, 1000m, 2000m);

				AssertCorrectAuthorisationRequired((ARCreditNote)creditNote, false, false);

				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 999M);
				creditNote = (InvoicingBase)((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
				AssertCorrectAuthorisationRequired((ARCreditNote)creditNote, true, false);

				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 1999M);
				creditNote = (InvoicingBase)((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
				AssertCorrectAuthorisationRequired((ARCreditNote)creditNote, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestAuthorisationRequiredBranchDepartmentWhenParentIsJob()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"), false);
				job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
				job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

				AssertNotEquals(job.JH_GB, GlbBranch.CurrentBranch.PK);
				AssertNotEquals(job.JH_GE, GlbDepartment.CurrentDepartment.PK);

				var creditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
				creditNote.AH_JH = job.PK;
				creditNote.AH_LocalExTaxAmount = 955M;
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(job.Branch.PK, job.Department.PK, 1000m, 2000m);
				AssertCorrectAuthorisationRequired(creditNote, false, false);
				creditNote.AH_LocalExTaxAmount = 1955M;
				AssertCorrectAuthorisationRequired(creditNote, true, false);
				creditNote.AH_LocalExTaxAmount = 2955M;
				AssertCorrectAuthorisationRequired(creditNote, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestAuthorisationRequiredBranchDepartmentWhenMiscellaneousCreditNote()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var creditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);

				creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
				creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				AssertNotEquals(creditNote.AH_GB, GlbBranch.CurrentBranch.PK);
				AssertNotEquals(creditNote.AH_GE, GlbDepartment.CurrentDepartment.PK);

				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(creditNote.Branch.PK, creditNote.Department.PK, 1000m, 2000m);
				creditNote.AH_LocalExTaxAmount = 955M;
				AssertCorrectAuthorisationRequired(creditNote, false, false);
				creditNote.AH_LocalExTaxAmount = 1955M;
				AssertCorrectAuthorisationRequired(creditNote, true, false);
				creditNote.AH_LocalExTaxAmount = 2955M;
				AssertCorrectAuthorisationRequired(creditNote, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAuthLogCreatedWhenAuthorisationRequired()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				SetAuthorizationLevelSettings();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				OrgHeader orgHeader = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
				ARCreditNote creditNote = Factory.New<ARCreditNote>();
				creditNote.AH_OH = orgHeader.PK;
				AccChargeCode chargeCode = TestObjectCreator.RevenueNoTaxChargeCode;
				AssertEquals("ChargeCode should have a revenue charge type", Core.Constants.ChargeType.Revenue, chargeCode.AC_ChargeType);
				ARCreditNoteLine line = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, chargeCode, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");

				creditNote.Validation.ValidateAll();
				AssertEquals(string.Format("credit note should not have errors: {0}", creditNote.Notifications.ToMessageListString()), false, creditNote.HasErrors);

				SecurityForTest testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				testSecurity.CachingEnabled = false;
				testSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
				var initialUserContext = Env.CurrentUserContext;
				Env.ClearUserContext();
				Env.SetUserContext(new UserContext(initialUserContext.User, initialUserContext.Branch.PK, initialUserContext.Department.PK));

				SecurityCore userSecurityWithAccess = testSecurity;
				creditNote.SecurityOverrideProvider = new NonInteractiveSecurityOverrideProvider();
				Enterprise.Security.Testing.SecurityTestObject.CreateTestUser(true, userSecurityWithAccess.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "tst", "testuser", "password");
				(creditNote.SecurityOverrideProvider as NonInteractiveSecurityOverrideProvider).OverrideLogin = "testuser";
				(creditNote.SecurityOverrideProvider as NonInteractiveSecurityOverrideProvider).OverridePassword = "password";
				creditNote.SecurityOverrideProvider.PromptForTemporaryAccess(userSecurityWithAccess.CreditAdjustmentNotePostingApprovalFirstLevelApproval);

				Factory.Save();

				AssertEquals("Logs count should equal 2", 2, creditNote.Logs.GetAllLogs().Count);
				StmALog authLog = (StmALog)creditNote.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code))[0];
				AssertEquals("added log should be ATH", Events.Authorised.Code, authLog.SL_SE_NKEvent);
				AssertEquals("added log should contain proper reference", "Post authorized by testuser ()", authLog.SL_Reference);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestNoAuthorizationLogCreatedWhileReversingCreditNote()
		{
			SetAuthorizationLevelSettings();
			OrgHeader orgHeader = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
			var creditNote = TestObjectCreator.CreateARCreditNote("CRD001", orgHeader, TestObjectCreator.AUD, 1M, "test CRD");
			var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.RevenueNoTaxChargeCode, 2000.00m, GlbCompany.CurrentCompany.LocalCurrency, 1.0m, "Test");

			IReversing beingReversed = creditNote;
			beingReversed.GenerateReverseTransaction(true);

			creditNote.Validation.ValidateAll();
			AssertEquals(string.Format("credit note should not have errors: {0}", creditNote.Notifications.ToMessageListString()), false, creditNote.HasErrors);

			SecurityForTest testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.CachingEnabled = false;
			testSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
			var initialUserContext = Env.CurrentUserContext;
			Env.ClearUserContext();
			Env.SetUserContext(new UserContext(initialUserContext.User, initialUserContext.Branch.PK, initialUserContext.Department.PK));

			SecurityCore userSecurityWithAccess = testSecurity;
			creditNote.SecurityOverrideProvider = new NonInteractiveSecurityOverrideProvider();
			(creditNote.SecurityOverrideProvider as NonInteractiveSecurityOverrideProvider).OverrideLogin = User.SupportUserName;
			(creditNote.SecurityOverrideProvider as NonInteractiveSecurityOverrideProvider).OverridePassword = CWSupportLoginToken.TokenForTest;
			creditNote.SecurityOverrideProvider.PromptForTemporaryAccess(userSecurityWithAccess.CreditAdjustmentNotePostingApprovalFirstLevelApproval);

			Factory.Save();

			AssertEquals("Logs count should equal 1", 1, creditNote.Logs.GetAllLogs().Count);
			var athLog = creditNote.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code));
			AssertEquals("No ATH log is created", 0, athLog.Length);
		}

		void SetAuthorizationLevelSettings()
		{
			var collection = TestObjectCreator.CreateAuthorizationModeAndSettings(1000m, 2000m);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}

		public void TestPopulateFromOriginalTransaction_HeaderDetails_SetComplianceSubType()
		{
			var originalTxn = GetNewBusinessObject() as InvoicingBase;
			originalTxn.AH_ComplianceSubType = "TES";
			originalTxn.AH_TransactionReference = "TEST01";

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creditNotes = Factory.NewWithValidTestData<ARCreditNote>();

				creditNotes.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be TES", "TES", creditNotes.AH_ComplianceSubType);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("CN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creditNotes = Factory.NewWithValidTestData<ARCreditNote>();

				creditNotes.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, creditNotes.AH_ComplianceSubType);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creditNotes = Factory.NewWithValidTestData<ARCreditNote>();

				creditNotes.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, creditNotes.AH_ComplianceSubType);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var creditNotes = Factory.NewWithValidTestData<ARCreditNote>();

				creditNotes.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, creditNotes.AH_ComplianceSubType);
			}

			originalTxn.AH_TransactionReference = string.Empty;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("VN"))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creditNotes = Factory.NewWithValidTestData<ARCreditNote>();

				creditNotes.PopulateFromOriginalTransaction(originalTxn, false);

				AssertEquals("ComplianceSubType should be empty", string.Empty, creditNotes.AH_ComplianceSubType);
			}
		}

		[TestDate(2021, 06, 01)]
		[SuspendCriticalValidation]
		public void TestGSTReversal()
		{
			GSTReversalHelper(CountryCodes.India, true, new ZDateTime(2021, 03, 20), new ZDateTime(2021, 06, 01), AssertNoError, "not be");
			GSTReversalHelper(CountryCodes.India, false, new ZDateTime(2021, 03, 20), new ZDateTime(2021, 06, 01), AssertHasError, "be");
			GSTReversalHelper(CountryCodes.Australia, true, new ZDateTime(2021, 03, 20), new ZDateTime(2021, 06, 01), AssertNoError, "not be");
			GSTReversalHelper(CountryCodes.Australia, false, new ZDateTime(2021, 03, 20), new ZDateTime(2021, 06, 01), AssertNoError, "not be");
		}

		void GSTReversalHelper(
			String countryCode,
			bool allowGSTReversal,
			ZDateTime fyDate,
			ZDateTime postDate,
			Action<string, ZPropertyInfo, string> assertMethod,
			string message)
		{
			OrgHeader orgHeader = TestObjectCreator.ABIGAS;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				PeriodManagementTestHelper = new AccountingPeriodTestHelper();
				PeriodManagementTestHelper.PostPeriodsForEntireYear(2021);
				PeriodManagementTestHelper.PostPeriodsForEntireYear(2020);
				PeriodManagementTestHelper.PostPeriodsForEntireYear(2022);

				AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = allowGSTReversal;
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();

				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 1000m, 2000m);
				AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

				var creditNote = TestObjectCreator.CreateARCreditNote("CRD001", orgHeader, TestObjectCreator.AUD, 1M, "test CRD");
				creditNote.AH_PostDate = postDate.Date;
				creditNote.AH_OH = DebtorOrg.PK;
				creditNote.AH_OriginalInvoiceDate = fyDate.Date;

				var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(
										creditNote,
										null,
										TestObjectCreator.RevenueNoTaxChargeCode,
										2000.00m,
										GlbCompany.CurrentCompany.LocalCurrency,
										1.0m,
										"Test");
				creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;
				creditNoteLine.AL_TaxDate = fyDate.Date;
				creditNoteLine.AL_OSAmount = 1000;
				creditNoteLine.AL_GovtChargeCode = "random text";
				TestObjectCreator.AddLineToCreditNote(creditNote, new ZGuid(), currentBranch, currentDepartment, 1000);

				assertMethod($"CountryCode: {countryCode}, GSTREeversalAllowed: {allowGSTReversal}\n" +
					$"Credit Note Line should {message} having error :" +
					$"{IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST}",
					creditNoteLine.AL_ATInfo,
					IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
			}
		}

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { throw new NotSupportedException(); }
		}

		AccTaxRate excludedTax;
		protected AccTaxRate ExcludedTax
		{
			get
			{
				if (excludedTax == null)
				{
					excludedTax = Factory.NewWithValidTestData<AccTaxRate>();
					excludedTax.AT_Code = "EXL";
					excludedTax.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
					excludedTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				return excludedTax;
			}
		}

		public override void TestIsAllowModifyAmendStatusCode()
		{
			var arCreditNote = PrepareTransactionHeaderForTest() as ARCreditNote;

			var arCreditWithOriginalReference = PrepareTransactionHeaderForTest() as ARCreditNote;
			arCreditWithOriginalReference.OriginalTransactionReference = Factory.NewWithValidTestData<ARInvoice>().PK;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, arCreditNote.IsAllowModifyAmendStatusCode);
			AssertEquals(false, arCreditWithOriginalReference.IsAllowModifyAmendStatusCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, arCreditNote.IsAllowModifyAmendStatusCode);
			AssertEquals(false, arCreditWithOriginalReference.IsAllowModifyAmendStatusCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, arCreditNote.IsAllowModifyAmendStatusCode);
			AssertEquals(false, arCreditWithOriginalReference.IsAllowModifyAmendStatusCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, arCreditNote.IsAllowModifyAmendStatusCode);
			AssertEquals(true, arCreditWithOriginalReference.IsAllowModifyAmendStatusCode);
		}

		public override void TestAH_Calc_AmendStatusCode()
		{
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var dummyArInvoice = Factory.NewWithValidTestData<ARInvoice>();
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(dummyArInvoice.PK, string.Empty, string.Empty);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(ZGuid.Empty, string.Empty, string.Empty);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(dummyArInvoice.PK, string.Empty, string.Empty);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(ZGuid.Empty, string.Empty, string.Empty);
			}

			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var dummyArInvoice = Factory.NewWithValidTestData<ARInvoice>();
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(dummyArInvoice.PK);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(ZGuid.Empty);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(dummyArInvoice.PK);
				AssertAH_Calc_AmendStatusCode_ARCreditNote(ZGuid.Empty);
			}

			ARCreditNote GetNewTestObj(ZGuid originalTransactionReference)
			{
				var arCredit = PrepareTransactionHeaderForTest() as ARCreditNote;
				arCredit.FillWithValidTestData();
				arCredit.OriginalTransactionReference = originalTransactionReference;

				return arCredit;
			}

			void AssertAH_Calc_AmendStatusCode_ARCreditNote(ZGuid originalTransactionPK, string expectedAmendStatusCodeFromDB = "01", string expectedAmendStatusCodeFromBizO = "02")
			{
				AssertAH_Calc_AmendStatusCode(() => GetNewTestObj(originalTransactionPK), expectedAmendStatusCodeFromDB, expectedAmendStatusCodeFromBizO);

				var aRCreditNote = GetNewTestObj(originalTransactionPK);

				aRCreditNote.AH_Calc_AmendStatusCode = "02";
				AssertEquals("PreCondition", expectedAmendStatusCodeFromBizO, aRCreditNote.AH_Calc_AmendStatusCode);

				aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
				AssertEquals("PreCondition", ZGuid.Empty, aRCreditNote.OriginalTransactionReference);

				AssertEquals("AH_Calc_AmendStatusCode should return empty string when OriginalTransactionReference is empty.", ZString.Empty, aRCreditNote.AH_Calc_AmendStatusCode);

				aRCreditNote.Delete();
			}
		}

		public override void TestAH_Calc_AmendStatusCode_ReadOnly()
		{
			var arCreditNote = PrepareTransactionHeaderForTest() as ARCreditNote;
			arCreditNote.FillWithValidTestData();

			var arCreditWithOriginalReference = PrepareTransactionHeaderForTest() as ARCreditNote;
			arCreditWithOriginalReference.FillWithValidTestData();
			arCreditWithOriginalReference.OriginalTransactionReference = Factory.NewWithValidTestData<ARInvoice>().PK;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(true, arCreditNote.AH_Calc_AmendStatusCodeInfo.ReadOnly);
			AssertEquals(true, arCreditWithOriginalReference.AH_Calc_AmendStatusCodeInfo.ReadOnly);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, arCreditNote.AH_Calc_AmendStatusCodeInfo.ReadOnly);
			AssertEquals(true, arCreditWithOriginalReference.AH_Calc_AmendStatusCodeInfo.ReadOnly);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(true, arCreditNote.AH_Calc_AmendStatusCodeInfo.ReadOnly);
			AssertEquals(true, arCreditWithOriginalReference.AH_Calc_AmendStatusCodeInfo.ReadOnly);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, arCreditNote.AH_Calc_AmendStatusCodeInfo.ReadOnly);
			AssertEquals(false, arCreditWithOriginalReference.AH_Calc_AmendStatusCodeInfo.ReadOnly);

			Factory.Save();
			AssertEquals("AH_Calc_AmendStatusCodeInfo will be read only after being saved.", true, arCreditWithOriginalReference.AH_Calc_AmendStatusCodeInfo.ReadOnly);
		}

		public void TestAH_ComplianceSubType_ReadOnly()
		{
			var amending = Factory.New<ARCreditNote>();
			var iAmending = amending as IAmending;
			iAmending.FlagAsCreatedAmending();
			AssertEquals(false, amending.AH_ComplianceSubType_ReadOnly);

			amending.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Malaysia;
			AssertEquals(true, amending.AH_ComplianceSubType_ReadOnly);

			amending.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var original = Factory.NewWithValidTestData<ARInvoice>();
			var aRCreditNote = Factory.New<ARCreditNote>();
			AssertEquals(false, aRCreditNote.AH_ComplianceSubType_ReadOnly);

			aRCreditNote.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Malaysia;
			AssertEquals(true, amending.AH_ComplianceSubType_ReadOnly);
			aRCreditNote.OriginalTransactionReference = original.PK;
			AssertEquals(false, aRCreditNote.AH_ComplianceSubType_ReadOnly);
		}

		public void TestAH_ComplianceSubTypeChangedToBlankWhenOriginalTransactionReferenceChangedToBlank()
		{
			var original = Factory.NewWithValidTestData<ARInvoice>();
			var aRCreditNote = Factory.New<ARCreditNote>();
			aRCreditNote.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Malaysia;
			aRCreditNote.OriginalTransactionReference = original.PK;
			AssertEquals(false, aRCreditNote.AH_ComplianceSubType_ReadOnly);

			aRCreditNote.AH_ComplianceSubType = "02";
			aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
			AssertEquals(ZString.Empty, aRCreditNote.AH_ComplianceSubType);
		}

		public void TestKoreaSouthSubtypeAutoAssign()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				var creditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
				creditNote.OriginalTransactionReference = invoice.PK;
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.FREEVAT.PK;
				line = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.AUD, 1M, -200M);
				line.AL_AT = TestObjectCreator.FREEVAT.PK;

				Factory.Save();

				AssertEquals("AR CRD with FREEVAT only should be assigned with 202.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.CZI, creditNote.AH_ComplianceSubType);

				invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				creditNote = TestObjectCreator.CreateARCreditNote("CRD002", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
				creditNote.OriginalTransactionReference = invoice.PK;
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.EXEMPT.PK;
				line = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.AUD, 1M, -200M);
				line.AL_AT = TestObjectCreator.EXEMPT.PK;

				Factory.Save();

				AssertEquals("AR CRD with EXEMPT only should be assigned with 401.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.CNI, creditNote.AH_ComplianceSubType);

				invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV003", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
				creditNote = TestObjectCreator.CreateARCreditNote("CRD003", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
				creditNote.OriginalTransactionReference = invoice.PK;
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.FREEVAT.PK;
				line = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.AUD, 1M, -200M);
				line.AL_AT = TestObjectCreator.FREEVAT.PK;
				line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 200M);
				line.AL_AT = TestObjectCreator.EXEMPT.PK;
				line = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.AUD, 1M, -200M);
				line.AL_AT = TestObjectCreator.EXEMPT.PK;

				Factory.Save();

				AssertEquals("AR CRD with EXEMPT and FREEVAT should be assigned with 201.", KoreaSouthComplianceInfo.ComplianceSubTypeCodes.CTI, creditNote.AH_ComplianceSubType);
			}
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions;
	}
}
