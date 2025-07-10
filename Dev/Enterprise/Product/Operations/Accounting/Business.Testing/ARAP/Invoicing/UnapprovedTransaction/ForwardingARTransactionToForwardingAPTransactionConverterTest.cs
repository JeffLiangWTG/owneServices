using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class ForwardingARTransactionToForwardingAPTransactionConverterTest : ARTransactionToAPTransactionConverterBaseTest
	{
		[TestDate(2021, 5, 4)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSisterCompanyConsolInvoiceImport_TaxDateUseTodaysDate()
		{
			AssertSisterCompanyConsolInvoiceImport_TaxDate(TaxDateDefaultingOption.Code.Today);
		}

		[TestDate(2021, 5, 4)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSisterCompanyConsolInvoiceImport_TaxDateUseInvoiceDate()
		{
			AssertSisterCompanyConsolInvoiceImport_TaxDate(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		void AssertSisterCompanyConsolInvoiceImport_TaxDate(string taxDateDefaultingOption)
		{
			AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.GSTFREE1.PK.ToGuid());
			var invoiceDate = new ZDateTime(2021, 4, 28);
			differentCompany.OrgProxy.CompanyData.OB_IsCreditor = true;
			differentBranchOrgProxy.CompanyData.OB_APVATConfig = "DEF"; //set Is AP Tax Applicable, SetAPTaxApplicable(true);
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol(GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort, differentCompanyOrgProxy.OH_RL_NKClosestPort, "C0001");
			var shipment = TestObjectCreator.CreateShipment("S001000", consol);
			TestObjectCreator.CreateJob(shipment, false);

			ARInvoice arInvoice = null;
			using (new TemporaryUserContext() { DepartmentPK = TestObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var jobInDifferentCompany = TestObjectCreator.CreateJob(shipment, false);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.0m, GlbCompany.CurrentCompany.OrgProxy, invoiceDate);
				arInvoice.AH_OSTotalAmount = 3m;
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);
				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, jobInDifferentCompany, TestObjectCreator.CC1, 3m, TestObjectCreator.USD, 1.0m);
				aRInvoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				Factory.Save();
			}

			using (AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ConsolCostDefaultApportionmentMethodConfiguration()))
			{
				// create a charge code with the same AC_Code in the current company, in order to match with the one from the AR invoice line.
				TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, GlbCompany.CurrentCompany);
				Factory.Save();
				var collection = new TaxDateDefaultingOptionCollection();
				var taxDateOption = collection.AddNew();
				taxDateOption.JobType = "FCN";
				taxDateOption.DirectionCode = "ALL";
				taxDateOption.Mode = "ALL";
				taxDateOption.Ledger = "AP";
				taxDateOption.TaxDateOption = taxDateDefaultingOption;
				using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
				{
					var converter = new UnapprovedTransactionConverter(Factory);
					var resultofconverter = converter.ConvertToAPUnsafe(arInvoice, Factory, true);
					AssertNotNull(resultofconverter);

					AssertEquals(1, resultofconverter.ConsolCosting.ConsolCosts.Count);
					AssertEquals(1, resultofconverter.Lines.Count);
					var invoiceLine = resultofconverter.Lines.Cast<InvoicingLineBase>().First();
					if (taxDateDefaultingOption == TaxDateDefaultingOption.Code.Today)
					{
						AssertEquals(new ZDate(2021, 5, 4), resultofconverter.ConsolCosting.ConsolCosts[0].E6_TaxDate);
						AssertEquals(new ZDate(2021, 5, 4), invoiceLine.AL_TaxDate);
					}
					else if (taxDateDefaultingOption == TaxDateDefaultingOption.Code.InvoiceDate)
					{
						AssertEquals(new ZDate(2021, 4, 28), resultofconverter.ConsolCosting.ConsolCosts[0].E6_TaxDate);
						AssertEquals(new ZDate(2021, 4, 28), invoiceLine.AL_TaxDate);
					}
					else
					{
						Fail("Invalid tax date defaulting optoin");
					}
				}
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSisterCompanyConsolInvoiceImport_ARAPInvoicePostingExchangeRateOptionIsDEF_UseJobExchangeRateDefaultIsTrue()
		{
			AssertSisterCompanyConsolInvoiceImport(true, true);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSisterCompanyConsolInvoiceImport_ARAPInvoicePostingExchangeRateOptionIsDEF_UseJobExchangeRateDefaultIsFalse()
		{
			AssertSisterCompanyConsolInvoiceImport(true, false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSisterCompanyConsolInvoiceImport_ARAPInvoicePostingExchangeRateOptionIsNotDEF_UseJobExchangeRateDefaultIsTrue()
		{
			AssertSisterCompanyConsolInvoiceImport(false, true);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSisterCompanyConsolInvoiceImport_ARAPInvoicePostingExchangeRateOptionIsNotDEF_UseJobExchangeRateDefaultIsFalse()
		{
			AssertSisterCompanyConsolInvoiceImport(false, false);
		}

		void AssertSisterCompanyConsolInvoiceImport(bool isRegistryDEF, bool shouldUseJobExRateFlag)
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsol.Code, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.CustomsRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoiceDate = ZDateTime.Today.AddDays(-6);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.56M, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.78M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.CustomsRate, 3.22M, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.CustomsRate, 4.39M, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			differentCompany.OrgProxy.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol(GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort, differentCompanyOrgProxy.OH_RL_NKClosestPort, "C0001");
			var shipment = TestObjectCreator.CreateShipment("S001000", consol);
			TestObjectCreator.CreateJob(shipment, false);

			ARInvoice arInvoice = null;
			using (new TemporaryUserContext() { DepartmentPK = TestObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var jobInDifferentCompany = TestObjectCreator.CreateJob(shipment, false);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.0m, GlbCompany.CurrentCompany.OrgProxy, invoiceDate);
				arInvoice.AH_OSTotalAmount = 3m;
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);
				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, jobInDifferentCompany, TestObjectCreator.CC1, 3m, TestObjectCreator.USD, 1.0m);
				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				Factory.Save();
			}

			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryDEF ? AccountingConstants.InvoicePostingExchangeRateOption.Default.Code : AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shouldUseJobExRateFlag))
			using (AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ConsolCostDefaultApportionmentMethodConfiguration()))
			{
				AssertNotEquals("Pre-condition: Default apportionment method is not manual", AllocationMethod.Manual, consol.GetApportionmentMethod(TestObjectCreator.FRT));
				var converter = new UnapprovedTransactionConverter(Factory);
				var resultofconverter = converter.ConvertToAPUnsafe(arInvoice, Factory, true);
				AssertNotNull(resultofconverter);

				AssertEquals(1, resultofconverter.ConsolCosting.ConsolCosts.Count);
				AssertEquals(AllocationMethod.Manual, resultofconverter.ConsolCosting.ConsolCosts[0].E6_ApportionmentMethod);
				AssertEquals(1, resultofconverter.Lines.Count);
				var invoiceLine = resultofconverter.Lines.Cast<InvoicingLineBase>().First();
				var expectedExRate = isRegistryDEF ? (shouldUseJobExRateFlag ? (ZDecimal)4.39m : resultofconverter.AH_ExchangeRate) : (shouldUseJobExRateFlag ? (ZDecimal)3.22m : resultofconverter.AH_ExchangeRate);
				BusinessObjectBaseTestCase.AssertZDecimalEquals("Expected line exchange rate.", expectedExRate, invoiceLine.AL_ExchangeRate, 0.00001m);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestShowErrorMessageIfMutexExceptionOccursWhileConvertToAPTransactionFromARTransaction()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			differentCompany.OrgProxy.CompanyData.OB_IsCreditor = true;
			var consol = TestObjectCreator.CreateConsol(GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort, differentCompanyOrgProxy.OH_RL_NKClosestPort, "C0001");
			var shipment = TestObjectCreator.CreateShipment("S001000", consol);
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			TestObjectCreator.CreateJob(shipment, false);

			ARInvoice arInvoice = null;
			using (new TemporaryUserContext() { DepartmentPK = TestObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var jobInDifferentCompany = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.0M, GlbCompany.CurrentCompany.OrgProxy);
				arInvoice.AH_OSTotalAmount = 3M;
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);
				var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, jobInDifferentCompany, TestObjectCreator.CC1, 3M, TestObjectCreator.USD, 1.0M);
				TestObjectCreator.CreateCharge(aRInvoiceLine1);
				Factory.Save();
			}

			Job jobWithMutex = TestObjectCreator.CreateJob(shipment1, createWithMutex: true);
			var newFactory = new BusinessObjectFactory();
			var converter = new UnapprovedTransactionConverter(newFactory);
			var apInvoice = converter.ConvertToAPUnsafe(arInvoice, newFactory, false, true, true, true);
			jobWithMutex.Dispose();

			AssertNotNull(apInvoice);
			AssertEquals(true, apInvoice.HasErrors);
			var expectedErrorMessage = "Error: You have created the job S001001 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S001001 to continue.";
			AssertHasRowError("APInovice should have a mutex conflict error on creating job", apInvoice, expectedErrorMessage);
		}

		public void TestGeneratedConsolCostsAreNotSplitBasedUponLineRelatedJobNumber()
		{
			var consol = TestObjectCreator.CreateConsol(GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort, differentCompanyOrgProxy.OH_RL_NKClosestPort, "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S1111", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", consol);
			Factory.Save();

			InvoicingBase arInvoice = null;
			using (new TemporaryUserContext() { DepartmentPK = TestObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = differentBranch1.PK.ToGuid() }.Set())
			{
				var shipment1Job = TestObjectCreator.CreateJob(shipment1, false);
				TestObjectCreator.CreateJob(shipment2, false);
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, GlbCompany.CurrentCompany.OrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				Assert("Posted from Forwarding Consol", arInvoice.AH_JH.IsEmpty);

				foreach (var relatedJob in new[] { null, shipment1, shipment2 })
				{
					var aRInvoiceLine1 = TestObjectCreator.CreateInvoiceLine(arInvoice, shipment1Job, TestObjectCreator.CC1, 100, TestObjectCreator.USD, 1.0m);
					var charge = TestObjectCreator.CreateChargeWithTarget(aRInvoiceLine1, relatedJob, null);
				}

				Factory.Save();
			}

			InvoicingBase convertedAPInvoice = null;
			using (AccountingConfigurationRegistry.Instance.CombineInvoiceLinesByChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				convertedAPInvoice = converter.ConvertToAP(arInvoice, true);
			}

			var consolCosts = convertedAPInvoice.ConsolCosting.ConsolCosts.Cast<JobConsolCost>();
			AssertEquals("Costs are not split by different related job numbers", 1, consolCosts.Count());
		}

		public void TestAPCreditNoteReferenceCorrectAPInvoice()
		{
			AssertAPCreditNoteOriginalTransactionReferenceSet(false, false);
		}

		public void TestAPCreditNoteReferenceCorrectAPInvoiceWithAutoImport()
		{
			AssertAPCreditNoteOriginalTransactionReferenceSet(true, false);
		}

		public void TestAPCreditNoteReferenceCorrectAPInvoiceWithComplianceNumberRegistry()
		{
			AssertAPCreditNoteOriginalTransactionReferenceSet(false, true);
		}

		public void TestAPCreditNoteReferenceCorrectAPInvoiceWithComplianceNumberRegistryAndAutoImport()
		{
			AssertAPCreditNoteOriginalTransactionReferenceSet(true, true);
		}

		void AssertAPCreditNoteExchangeRateWhenOriginalTransactionReferenceIsSet(InvoicingBase arCreditNote, InvoicingBase apCreditNote, ARTransactionToAPTransactionConverterBase arToApConverter, bool isAutoImport)
		{
			AssertEquals("AP Credit Note should use AP Invoice Exchange Rate in first place", 3.1111m, apCreditNote.AH_ExchangeRate);

			using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry("AP", GlbCompany.CurrentCompany.PK.ToGuid(), false))
			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "INV"))
			{
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, "BUY", 2.1111m, arCreditNote.AH_InvoiceDate, arCreditNote.AH_InvoiceDate);
				var converterResultfARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, isAutoImport);
				AssertNotNull(converterResultfARCreditNote);
				apCreditNote = converterResultfARCreditNote.convertedAPTransaction;
				AssertEquals("AP Credit Note should use exchange rate of invoice date when registry is set", 2.1111m, apCreditNote.AH_ExchangeRate);
			}
		}

		void AssertAPCreditNoteOriginalTransactionReferenceSet(bool isAutoImport, bool isComplianceNumerRegistrySet)
		{
			if (isComplianceNumerRegistrySet)
			{
				AccountingConfigurationRegistry.Instance.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			}
			var shipment = TestObjectCreator.CreateShipment("S00001017");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("S00002798", TestObjectCreator.EUR, 1m, GlbCompany.CurrentCompany.OrgProxy);
			arInvoice.AH_ConsolidatedInvoiceRef = "1234";
			arInvoice.AH_TransactionReference = isComplianceNumerRegistrySet ? "ComplianceNumber" : String.Empty;
			arInvoice.AH_InvoiceDate = new ZDate(2015, 01, 01);
			var aRInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC1, 3m, TestObjectCreator.USD, 1.0m);
			TestObjectCreator.CreateCharge(aRInvoiceLine);
			Factory.Save();

			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var converterResultforARInvoice = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, Factory, false, false, false, isAutoImport);
			AssertNotNull(converterResultforARInvoice);
			var apInvoice = converterResultforARInvoice.convertedAPTransaction;
			AssertNotNull(apInvoice);
			if (isComplianceNumerRegistrySet)
			{
				AssertEquals("AP Invoice Transaction Number should have AR Invoice Compliance Number value", arInvoice.AH_TransactionReference, apInvoice.AH_TransactionNum);
				AssertEquals("AP Invoice AH_ChequeOrReference should have ARInvoice Transaction Number value", arInvoice.AH_TransactionNum, apInvoice.AH_ChequeOrReference);
			}
			apInvoice.AH_ExchangeRate = 3.1111m;

			var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD00001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			arCreditNote.ReasonCode = "IDE";
			arCreditNote.OriginalTransactionReference = arInvoice.PK;
			arCreditNote.Lines[0].AL_OSExTaxAmount = 2m;
			Factory.Save();

			var converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, isAutoImport);
			AssertNotNull(converterResultforARCreditNote);
			var apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
			AssertNotNull(apCreditNote);
			AssertEquals("AP Credit Note should reference the right AP Invoice", apInvoice.PK, apCreditNote.OriginalTransactionReference);
			AssertEquals("AP Credit Note lines should reference to AR Credit Note lines", -2.00m, apCreditNote.Lines[0].AL_OSExTaxAmount);
			AssertAPCreditNoteExchangeRateWhenOriginalTransactionReferenceIsSet(arCreditNote, apCreditNote, arToApConverter, isAutoImport);
			AssertEquals(arCreditNote.AH_DueDate, apCreditNote.AH_DueDate);
		}

		public void TestAPCreditNoteOSAmountNoChangedAfterOriginalTransactionReferenceIsSet()
		{
			AssertAPCreditNoteOSAmountNoChangedAfterOriginalTransactionReferenceIsSet(false);
		}

		public void TestAPCreditNoteOSAmountNoChangedAfterOriginalTransactionReferenceIsSetWithAutoImport()
		{
			AssertAPCreditNoteOSAmountNoChangedAfterOriginalTransactionReferenceIsSet(true);
		}

		void AssertAPCreditNoteOSAmountNoChangedAfterOriginalTransactionReferenceIsSet(bool isAutoImport)
		{
			var aRInvoiceLine = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1, 111.22m, 10, 100, 10);
			Factory.Save();

			var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD00001", TestObjectCreator.AALSHI, TestObjectCreator.TWD, 1m);
			arCreditNote.ReasonCode = "IDE";
			arCreditNote.OriginalTransactionReference = aRInvoiceLine.PK;
			arCreditNote.Lines[0].AL_OSExTaxAmount = 111.22m;
			Factory.Save();

			var taiWanCompany = TestObjectCreator.CreateNewCompany("CNC", Core.Constants.CountryCodes.Taiwan);
			taiWanCompany.GC_OH_OrgProxy = TestObjectCreator.ActiveOrg.PK;
			var taiWanBranch = TestObjectCreator.CreateNewBranch(taiWanCompany, "CNB");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, taiWanBranch.PK.ToGuid(), TestObjectCreator.FESDepartment.PK.ToGuid()))
			{
				var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
				var converterResultforARInvoice = arToApConverter.ConvertToAPTransactionFromARTransaction(aRInvoiceLine, Factory, false, false, false, isAutoImport);
				AssertNotNull(converterResultforARInvoice);
				var apInvoice = converterResultforARInvoice.convertedAPTransaction;
				AssertNotNull(apInvoice);

				GlbCompany.CurrentCompany.SetCurrency("TWD");
				var converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, isAutoImport);
				AssertNotNull(converterResultforARCreditNote);
				var apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
				AssertNotNull(apCreditNote);
				AssertEquals("AL_OSExTaxAmount of AP Credit Note should not be changed after  setting OriginalTransactionReference", 111.22m, apCreditNote.Lines[0].AL_OSExTaxAmount);
			}
		}

		public void TestDateAndOriginalInvoiceNumberWhenNoOriginalFound()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001018");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("S00002799", TestObjectCreator.EUR, 1m, GlbCompany.CurrentCompany.OrgProxy);
			arInvoice.AH_ConsolidatedInvoiceRef = "1235";
			var aRInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC1, 3m, TestObjectCreator.USD, 1.0m);
			TestObjectCreator.CreateCharge(aRInvoiceLine);
			Factory.Save();

			var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD0002", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			arCreditNote.ReasonCode = "IDE";
			arCreditNote.OriginalTransactionReference = arInvoice.PK;
			Factory.Save();

			arInvoice.AH_InvoiceDate = new ZDate(2020, 01, 20);
			arInvoice.AH_TransactionNum = "00001080";
			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
			AssertNotNull(converterResultforARCreditNote);
			var apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
			AssertNotNull(apCreditNote);
			AssertEquals("APCreditNote Original Invoice Number is ARInvoice Transaction Num", "00001080", apCreditNote.AH_OriginalTransactionNum);
			AssertEquals("APCreditNote Original Invoice Date is ARInvoice Invoice Date", new ZDate(2020, 01, 20), apCreditNote.AH_OriginalInvoiceDate);

			//Test with ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber registry
			AccountingConfigurationRegistry.Instance.ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			arInvoice.AH_TransactionReference = "ComplianceNumber";
			arInvoice.AH_InvoiceDate = new ZDate(2021, 02, 20);
			converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
			AssertNotNull(converterResultforARCreditNote);
			apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
			AssertNotNull(apCreditNote);
			AssertEquals("APCreditNote Original Invoice Number is ARInvoice Compliance Number ", "ComplianceNumber", apCreditNote.AH_OriginalTransactionNum);
			AssertEquals("APCreditNote Original Invoice Date is ARInvoice Invoice Date", new ZDate(2021, 02, 20), apCreditNote.AH_OriginalInvoiceDate);
		}

		public void TestDateAndOriginalInvoiceNumberWhenAmendNotLinked()
		{
			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			arCreditNote.ReasonCode = "IDE";
			arCreditNote.AH_OriginalInvoiceDate = new ZDate(2023, 03, 20);
			arCreditNote.AH_OriginalTransactionNum = "TESTCRD1";
			arCreditNote.OriginalTransactionReference = ZGuid.Empty;
			var line = Factory.NewWithValidTestData<ARCreditNoteLine>();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_LocalExTaxAmount = 100m;
			line.AL_RX_NKTransactionCurrency = "AUD";
			arCreditNote.Lines.Add(line);
			Factory.Save();

			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
			AssertNotNull(converterResultforARCreditNote);
			var apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
			AssertEquals("AP Credit Note should have empty Original Transaction Reference", Guid.Empty, apCreditNote.OriginalTransactionReference);
			AssertEquals("AP Credit Note Invoice Number is AR Credit Note Invoice Number", "TESTCRD1", apCreditNote.AH_OriginalTransactionNum);
			AssertEquals("AP Credit Note Invoice Date is AR Credit Note Invoice Date", new ZDate(2023, 03, 20), apCreditNote.AH_OriginalInvoiceDate);

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>(arCreditNote.AH_OriginalTransactionNum, apCreditNote.TransactionCurrency, 1.0m, 16m, 0m, 0m, 16m, 0m, 0m);
			apInvoice.AH_OH = apCreditNote.AH_OH;
			apInvoice.AH_InvoiceDate = arCreditNote.AH_OriginalInvoiceDate;
			converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
			apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
			AssertEquals("Assign AP Credit Note Transaction Reference when there is a AP Invoice that match the query", apInvoice.PK, apCreditNote.OriginalTransactionReference);
			AssertEquals("AP Credit Note Invoice Number is AP Invoice Number", "TESTCRD1", apCreditNote.AH_OriginalTransactionNum);
			AssertEquals("AP Credit Note Invoice Date is AP Invoice Date", new ZDate(2023, 03, 20), apCreditNote.AH_OriginalInvoiceDate);
		}

		public void TestAmendingAPCreditNoteCurrencyAndExchangeRate()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001017");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("S00002798", TestObjectCreator.EUR, 1m, GlbCompany.CurrentCompany.OrgProxy);
			arInvoice.AH_ConsolidatedInvoiceRef = "1234";
			arInvoice.AH_InvoiceDate = new ZDate(2015, 01, 01);
			arInvoice.AH_RX_NKTransactionCurrency = "EUR";
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, job, TestObjectCreator.CC1, 3m, TestObjectCreator.EUR, 1.0m);
			TestObjectCreator.CreateCharge(arInvoiceLine);
			Factory.Save();

			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());
			var converterResultforARInvoice = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, Factory, false, false, false, false);
			AssertNotNull(converterResultforARInvoice);
			var apInvoice = converterResultforARInvoice.convertedAPTransaction;
			AssertNotNull(apInvoice);
			apInvoice.AH_ExchangeRate = 6.7890m;

			var arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD00001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 5.6025m, arCreditNote.AH_InvoiceDate, arCreditNote.AH_InvoiceDate);
			arCreditNote.ReasonCode = "IDE";
			arCreditNote.OriginalTransactionReference = arInvoice.PK;
			arCreditNote.AH_RX_NKTransactionCurrency = "USD";
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
				var apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
				AssertEquals("Register YES and Different transaction currency, AP Credit Note should always get Currency from AR Credit Note", "USD", apCreditNote.ExchangeRate.Currency);
				AssertEquals("Register YES and Different transaction currency, then AP Credit Note should use default exchange rate", 5.6025m, apCreditNote.ExchangeRate.Rate);

				apInvoice.AH_RX_NKTransactionCurrency = "USD";
				converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
				apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
				AssertEquals("Register YES and Same transaction currency, AP Credit Note should always get Currency from AR Credit Note", "USD", apCreditNote.ExchangeRate.Currency);
				AssertEquals("Register YES and Same transaction currency, then AP Credit Note should use AP Invoice exchange rate", 6.7890m, apCreditNote.ExchangeRate.Rate);
			}

			using (AccountingConfigurationRegistry.Instance.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
				var apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
				AssertEquals("Register NO and Different transaction currency, AP Credit Note should always get Currency from AR Credit Note", "USD", apCreditNote.ExchangeRate.Currency);
				AssertEquals("Register NO and Different transaction currency, then AP Credit Note should use default exchange rate", 5.6025m, apCreditNote.ExchangeRate.Rate);

				apInvoice.AH_RX_NKTransactionCurrency = "USD";
				converterResultforARCreditNote = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, false);
				apCreditNote = converterResultforARCreditNote.convertedAPTransaction;
				AssertEquals("Register NO and Same transaction currency, AP Credit Note should always get Currency from AR Credit Note", "USD", apCreditNote.ExchangeRate.Currency);
				AssertEquals("Register NO and Same transaction currency, then AP Credit Note should use default exchange rate", 5.6025m, apCreditNote.ExchangeRate.Rate);
			}
		}

		[TestDate(2021, 06, 12)]
		public void TestDueDateReportedFromInvoiceToCreditNote_Intercompany()
		{
			var originalBranch = GlbBranch.CurrentBranch;
			var today = ZDateTime.Today;

			periodManagementTestHelper.PostPeriodsForEntireYear(today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(today.Year, differentCompany.PK);
			Factory.Save();

			ARInvoice arInvoice;
			ARCreditNote arCreditNote;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var debtor = originalBranch.OrgProxy;

				arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV00001", TestObjectCreator.EUR, 1m, debtor);
				arInvoice.AH_InvoiceDate = today.AddDays(-5);
				arInvoice.AH_PostDate = today;

				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1.0m, 50, differentBranch1.PK);

				Factory.Save();
				AssertEquals(arInvoice.AH_InvoiceDate, arInvoice.AH_DueDate);

				arCreditNote = TestObjectCreator.CreateARCreditNote("ARCRD00001", debtor, TestObjectCreator.EUR, 1m);
				arCreditNote.OriginalTransactionReference = arInvoice.PK;
				AssertEquals(arInvoice.AH_TransactionNum, arCreditNote.AH_OriginalTransactionNum);
				AssertEquals(arInvoice.AH_InvoiceDate.Date, arCreditNote.AH_OriginalInvoiceDate);

				TestObjectCreator.CreateInvoiceLine(arCreditNote, TestObjectCreator.EUR, 1.0m, 10, differentBranch1.PK);

				Factory.Save();
				AssertEquals(today, arCreditNote.AH_DueDate);
			}

			var creditor = differentBranch1.OrgProxy;
			var apTerm = creditor.CompanyData;
			apTerm.OB_APPaymentTerms = "INV";
			apTerm.OB_APPaymentTermDays = 30;

			Factory.Save();

			var arToApConverter = new ARTransactionToAPTransactionConverterBase(new NotificationBuffer());

			(var apInvoice, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arInvoice, Factory, false, false, false, true);
			AssertNotNull(apInvoice);
			apInvoice.AH_GB = originalBranch.PK;
			AssertEquals(arInvoice.AH_DueDate, apInvoice.AH_DueDate);

			(var apCreditNote, _) = arToApConverter.ConvertToAPTransactionFromARTransaction(arCreditNote, Factory, false, false, false, true);
			AssertNotNull(apCreditNote);
			apCreditNote.AH_GB = originalBranch.PK;
			AssertEquals(arCreditNote.AH_DueDate, apCreditNote.AH_DueDate);
		}
	}
}
