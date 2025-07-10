using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.GUI.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.GUI.Testing.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class InvoicingPostManagerGUIWrapperTest : PostManagerGUIWrapperTest
	{
		void TestSellExchangeRateWhenPostingRevenueCharges(string exRateOption)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].JobType = "ALL";
			config.InvoiceDateConfigurationCollection[0].SignificantDateCode = "ADD";
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = "ADD";
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.InvoiceDateConfigurationCollection[0].Override = true;
			config.InvoiceDateConfigurationCollection[0].ReversalRule = "STD";
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 6m, new DateTime(2019, 5, 1), new DateTime(2019, 12, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 7m, new DateTime(2019, 5, 1), new DateTime(2019, 12, 31));
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, localClientOrg: TestObjectCreator.ABIGAS);
			var debtor = TestObjectCreator.Debtor;
			debtor.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);

			var exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exchangeRate.JF_BaseRate = 2m;
			Factory.Save();

			var charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 100m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
			AssertEquals(2m, charge.JR_OSSellExRate);

			InitializeBackDateARInvoiceWithBackDateDialogResultYes();
			var wrapper = new TestPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job);
			wrapper.Post();

			var expectedRate = exRateOption == ExRateOption.Default.Code ? 2M : 7M;
			AssertEquals(expectedRate, charge.JR_OSSellExRate);
		}

		[TestDate(2019, 5, 10)]
		public void TestSellExchangeRateWhenPostingRevenueCharges_PostDateOption()
		{
			TestSellExchangeRateWhenPostingRevenueCharges(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		[TestDate(2019, 5, 10)]
		public void TestSellExchangeRateWhenPostingRevenueCharges_InvoiceDateOption()
		{
			TestSellExchangeRateWhenPostingRevenueCharges(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2019, 5, 10)]
		public void TestSellExchangeRateWhenPostingRevenueCharges_TodayOption()
		{
			TestSellExchangeRateWhenPostingRevenueCharges(ExRateOption.TodayExchangeRate.Code);
		}

		[TestDate(2019, 5, 10)]
		public void TestSellExchangeRateWhenPostingRevenueCharges_DefaultOption()
		{
			TestSellExchangeRateWhenPostingRevenueCharges(ExRateOption.Default.Code);
		}

		public void TestJobCreationExceptionWhenPostingGatewayConsol()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "USLAX", "C001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol: gatewayConsol);
			var job = TestObjectCreator.CreateJob(gatewayConsol);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100M, TestObjectCreator.Debtor);
			charge.JR_APInvoiceNum = "12344568797";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_PaymentDate = ZDateTime.Today;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);
			var shipment2 = testObjectCreator.CreateShipment("S002", "AUSYD", "USLAX");
			var gatewayConsol1 = newFactory.Load<ForwardingConsol>(gatewayConsol.PK);
			gatewayConsol1.Shipments.Add(shipment2);
			newFactory.Save();
			Factory.Save();

			var shipmentJob = testObjectCreator.CreateJob(shipment2); // Acquire Shipment mutex but don't release it.
			AssertNotNull("Job exists", shipment2.Job);

			var guiWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, null);

			try
			{
				guiWrapper.Post();
				var expected = string.Format(@"You cannot post because job {0} has errors. Please fix errors before posting.
 - Invoicing Job: You have created the job S002 on another form, but haven't saved it yet.
Please close or save other forms that use job S002 to continue.
", job.JH_JobNum);
				AssertEquals("Error message is shown", expected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				newFactory.Save(); // Release Shipment mutex.
			}
		}

		void SetUpRegistryForTest()
		{
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo1000 = GetNewAuthorisationSetting(valuesForTest, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, 1000, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);
			var upTo2000 = GetNewAuthorisationSetting(valuesForTest, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, 2000, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var upTo3000 = GetNewAuthorisationSetting(valuesForTest, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, 3000, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var above3000 = GetNewAuthorisationSetting(valuesForTest, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, 3000, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection, ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		public void Test_LocalExTaxAmountUsedToCalculateLocalTax()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Chile);
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.Addresses[0].PK;
			job.Company.GC_RX_NKLocalCurrency = "CLP";

			TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Chile IVA", 19);
			TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 664.987M);
			var chargeCurrency = TestObjectCreator.USD;
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "DESC", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 2.51M, TestObjectCreator.ABIGAS);
			charge1.JR_AT_SellGSTRate = taxRate.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_GE = TestObjectCreator.FESDepartment.PK;

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "DESC", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 2.59M, TestObjectCreator.ABIGAS);
			charge2.JR_AT_SellGSTRate = taxRate.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_GE = TestObjectCreator.FESDepartment.PK;

			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC4, "DESC", chargeCurrency, 0M, TestObjectCreator.AALSHI, chargeCurrency, 3.51M, TestObjectCreator.ABIGAS);
			charge3.JR_AT_SellGSTRate = taxRate.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge3.JR_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();

			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			AssertNoExceptionThrown("Should be posted without any Critical Validation exception.", () => wrapper.Post());

			var newFactory = new BusinessObjectFactory();
			var charges = newFactory.Load<Charge>(new ZQuery(JobChargeSchema.JR_JH, job.PK)).OrderBy(x => x.JR_OSSellAmt);
			AssertEquals(3, charges.Count());

			var largestCharge = charges.Last();
			AssertEquals(0.67M, largestCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals(444M, largestCharge.JR_Sell_LocalGSTAmount);
		}

		public void TestUnapprovedTransactionPostingFromJobPromptsPrintingCostConfirmationDocument()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test frt", TestObjectCreator.AUD, 1500M, TestObjectCreator.AALSHI, "abc1234", TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			SetUpRegistryForTest();

			SecurityCheckpoint unapprovedInvoiceCheckPoint = new UnapprovedTransactionValidationHelper().GetSecurityCheckPoint(charge.JR_OSCostAmt);
			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			Env.Security.APUnapprovedInvoicesSecondApproval.IsAllowed = false;
			Env.Security.APUnapprovedInvoicesThirdApproval.IsAllowed = false;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.PrintOptionWhenUnapprovedAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			InvoicingPostManagerGUIWrapper wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, null);
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();
				string expectedMessage = @"Do you want to print a Cost Confirmation Document for this transaction?";
				Assert("Message should start with Do you want to print invoice", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedMessage));
			}
		}

		public void TestRequestRelatedTransactionPostingPrintingCostConfirmationDocument()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test frt", TestObjectCreator.AUD, 1500M, TestObjectCreator.AALSHI, "abc1234", TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			SetUpRegistryForTest();

			SecurityCheckpoint unapprovedInvoiceCheckPoint = new UnapprovedTransactionValidationHelper().GetSecurityCheckPoint(charge.JR_OSCostAmt);
			Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
			Env.Security.APInvoiceApproval_SecondApproval.IsAllowed = false;
			Env.Security.APInvoiceApproval_ThirdApproval.IsAllowed = false;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.PrintOptionWhenUnapprovedAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			InvoicingPostManagerGUIWrapper wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, null);
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog => ZFormModaliser.ResultToReturnFromShowDialog = dialog.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				wrapper.Post();
				string expectedMessage = @"Do you want to preview a Cost Confirmation Document for transactions related to new approval requests?";
				AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType("LastFormShownDialogForTest", typeof(CostConfirmationDocTypePopupForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("Any other form should not be shown", ZFormModaliser.LastFormShownForTest);
				PreviewFormTestHelper.CloseOpenedForms();
			}
		}

		public void TestPostInvoicePopupMessageAskUserDoYouWantToPrint()
		{
			try
			{
				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				Factory.Save();
				Charge charge;
				Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
				testJob.PlugInData = shipment;
				testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				testJob.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
				Factory.Save();

				charge = testJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OSSellAmt = 150m;
				charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Factory.Save();

				InvoicingPostManagerGUIWrapper wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, testJob, null);
				using (Form form = new Form())
				{
					wrapper.ParentForm = form;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					wrapper.Post();
					ExceptionReporterTestListener.Instance.Clear();
					string expectedMessage = @"Do you want to print invoice";
					Assert("Message should start with Do you want to print invoice", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(expectedMessage));
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestPostingEntryFeeChargeForNZ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var gSTRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				gSTRate.AT_Type = AccTaxRate.Types.Rated;
				gSTRate.AT_RN_NKCountry = "NZ";
				gSTRate.SetRateNumerator_ForTestOnly(15);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				org.MiscServ.OM_ARWHTApplicable = false;

				var entryFeeChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				entryFeeChargeCode.AC_Code = "ENTRYFEE";
				entryFeeChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				entryFeeChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
				entryFeeChargeCode.AC_AT_GSTRate = gSTRate.PK;

				EntryChargeTypeSettingCollection chargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				EntryChargeTypeSetting typeAndCode1 = chargeTypesAndCodes.AddNew();
				typeAndCode1.ChargeType = NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee;
				typeAndCode1.AC_ChargeCode = entryFeeChargeCode.PK;

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeTypesAndCodes);

				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");

				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S001001");
				shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge1 = TestObjectCreator.CreateCharge(job, entryFeeChargeCode, "test charge1", currency, 0M, null, ZString.Empty, currency, NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly, org);
				charge1.JR_AT_SellGSTRate = gSTRate.PK;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "test charge2", currency, 0M, null, ZString.Empty, currency, 525M, org);
				charge2.JR_AT_SellGSTRate = gSTRate.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				Factory.Save();

				var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
				AssertNoExceptionThrown(() => { wrapper.Post(); });
			}
		}

		#region TEST: Post With Credit Card Payment via eNett

		public void TestPostWithCreditCardPaymentViaENett()
		{
			bool originalComPayEnabled = AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_IsDebtor = true;
				orgHeader.OH_IsCreditor = true;
				orgHeader.CompanyData.SetAPTaxApplicable(false);

				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;
				consignor.OH_Code = "CONSIGNOR";

				OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
				cusCode.OK_CustomsRegNo = "123456";
				cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				AssertEquals("eNettRegistrationNumber", "123456", orgHeader.ENettRegistrationNumber);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipment.ConsignorPK = consignor.PK;
				Factory.Save();

				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZZAUDAcc";
				var bankAccount = TestObjectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", TestObjectCreator.AUD, "123456", "12345678", header);
				bankAccount.AB_DebitCreditCardExpiry = "0699";
				bankAccount.AB_DebitCreditCardName = "MR JOHN SMITH";
				var encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
				bankAccount.AB_DebitCreditCardNumber = encoder.Encrypt("1234567812345678");
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
				bankAccount.AB_AccountNum = "**** **** ***4 5678";

				ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
				query.AddToFilter(GlbDepartmentSchema.GE_Misc, false);
				GlbDepartment department = Factory.LoadTop1<GlbDepartment>(query);

				Job job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				job.JH_GE = department.PK;
				Charge charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OSCostAmt = 100m;
				charge1.JR_OH_CostAccount = orgHeader.PK;
				charge1.JR_OSSellAmt = 100m;
				charge1.JR_OH_SellAccount = orgHeader.PK;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge1.JR_PaymentType = ReceiptTypes.eNettCreditCard;
				charge1.JR_APInvoiceNum = "TEST2222";
				charge1.JR_APInvoiceDate = ZDateTime.Now;
				charge1.JR_AB = bankAccount.PK;
				charge1.JR_ChequeNo = "111";
				charge1.JR_GE = department.PK;

				Charge charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC2.PK;
				charge2.JR_OSCostAmt = 200m;
				charge2.JR_OH_CostAccount = orgHeader.PK;
				charge2.JR_OSSellAmt = 200m;
				charge2.JR_OH_SellAccount = orgHeader.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge2.JR_PaymentType = ReceiptTypes.eNettCreditCard;
				charge2.JR_APInvoiceNum = "TEST2222";
				charge2.JR_APInvoiceDate = ZDateTime.Now;
				charge2.JR_AB = bankAccount.PK;
				charge2.JR_ChequeNo = "111";
				charge2.JR_GE = department.PK;

				Factory.Save();

				AssertEquals("Shipment should not have errors. Errors: " + shipment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment.HasErrors);

				job.RunPreSaveValidation();
				AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);

				int initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;
				eNettWebServiceWrapper.UseRealWebService_ForTesting = false;

				MockENettWebService.Instance.SetupForTesting("CARGOWISE");

				bool isCreditCardSecurityCodeFormShown = false;
				bool shouldContinueWithSecurityCode = false;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form.GetType() == typeof(CreditCardSecurityCodeForm))
					{
						var securityCodeBizo = (PaymentCreditCardSecurityCode)((ZForm)form).BusinessEntity;
						securityCodeBizo.CardSecurityCode = "333";
						securityCodeBizo.Continue = shouldContinueWithSecurityCode;

						isCreditCardSecurityCodeFormShown = true;
					}
				});
				var postManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job, null);
				postManagerWrapper.Post();
				Assert(nameof(isCreditCardSecurityCodeFormShown), isCreditCardSecurityCodeFormShown);

				var invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST2222").AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
				AssertNull("APInvoice: posting canceled by user in CreditCardSecurityCodeForm", invoice);

				query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", shipment.JS_UniqueConsignRef));
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.eNettCreditCard);
				query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);

				var payment = Factory.LoadTop1<APPayment>(query);
				AssertNull("APPayment: posting canceled", payment);

				AssertEquals("CountProcessCreditCardWasInvoked: posting canceled", 0, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);

				isCreditCardSecurityCodeFormShown = false;
				shouldContinueWithSecurityCode = true;

				initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;

				postManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job, null);
				postManagerWrapper.Post();
				Assert(nameof(isCreditCardSecurityCodeFormShown), isCreditCardSecurityCodeFormShown);

				invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST2222").AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
				AssertNotNull("APInvoice", invoice);

				query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", shipment.JS_UniqueConsignRef));
				query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.eNettCreditCard);
				query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);

				payment = Factory.LoadTop1<APPayment>(query);
				AssertNotNull("APPayment", payment);
				AssertEquals("APPayment.AH_InvoiceAmount", 300.00m, payment.AH_InvoiceAmount);

				AssertEquals("CountProcessCreditCardWasInvoked", 1, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalComPayEnabled);
			}
		}

		#endregion

		[TestDate(2011, 04, 10)]
		public void TestPostDefaultingPostDate()
		{
			setupRegistryForPostDateDefaulting();

			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.OH_IsCreditor = true;
			orgHeader.CompanyData.SetAPTaxApplicable(false);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime arrivalDate = new ZDateTime(2011, 04, 05);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_E_ARV = arrivalDate;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_AccountNum = "ZZAUDAcc";
			var bankAccount = TestObjectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", TestObjectCreator.AUD, "123456", "12345678", header);

			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			query.AddToFilter(GlbDepartmentSchema.GE_Misc, false);
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(query);

			Job job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = department.PK;
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_OH_CostAccount = orgHeader.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_OH_SellAccount = orgHeader.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_PaymentType = ReceiptTypes.Cash;
			charge1.JR_APInvoiceNum = "TEST2222";
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_AB = bankAccount.PK;
			charge1.JR_ChequeNo = "111";
			charge1.JR_GE = department.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 200m;
			charge2.JR_OH_CostAccount = orgHeader.PK;
			charge2.JR_OSSellAmt = 200m;
			charge2.JR_OH_SellAccount = orgHeader.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_PaymentType = ReceiptTypes.Cash;
			charge2.JR_APInvoiceNum = "TEST2222";
			charge2.JR_APInvoiceDate = ZDateTime.Now;
			charge2.JR_AB = bankAccount.PK;
			charge2.JR_ChequeNo = "111";
			charge2.JR_GE = department.PK;

			Factory.Save();

			AssertEquals("Shipment should not have errors. Errors: " + shipment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment.HasErrors);

			job.RunPreSaveValidation();
			AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);

			InvoicingPostManagerGUIWrapper postManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job, null);
			postManagerWrapper.Post();

			APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST2222").AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
			AssertNotNull("APInvoice", invoice);
			AssertEquals("APInvoice.AH_PostDate", arrivalDate, invoice.AH_PostDate);

			query = new ZQuery(AccTransactionHeaderSchema.AH_Desc, string.Format("AP Payment {0}", shipment.JS_UniqueConsignRef));
			query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.Cash);
			query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);

			APPayment payment = Factory.LoadTop1<APPayment>(query);
			AssertNotNull("APPayment", payment);
			AssertEquals("APPayment.AH_InvoiceAmount", 300.00m, payment.AH_InvoiceAmount);
			AssertEquals("APPayment.AH_PostDate", arrivalDate, payment.AH_PostDate);
		}

		#region TransactionDescriptionDefaulting

		[TestDate(2011, 04, 01)]
		public void TestTransactionDescriptionDefaultingOverriddenOnJob()
		{
			SetupJobDataForBackDateARInvoiceTests();

			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.OH_IsCreditor = true;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime arrivalDate = new ZDateTime(2011, 04, 05);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "CONSIGNOR";

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_E_ARV = arrivalDate;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_AccountNum = "ZZAUDAcc";
			var bankAccount = TestObjectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", TestObjectCreator.AUD, "123456", "12345678", header);

			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			query.AddToFilter(GlbDepartmentSchema.GE_Misc, false);
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(query);

			Job job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = department.PK;
			job.JH_Description = "Job level overridden description";

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_OH_CostAccount = orgHeader.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_OH_SellAccount = orgHeader.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_PaymentType = ReceiptTypes.Cash;
			charge1.JR_APInvoiceNum = "TEST2222";
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_AB = bankAccount.PK;
			charge1.JR_ChequeNo = "111";
			charge1.JR_GE = department.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 200m;
			charge2.JR_OH_CostAccount = orgHeader.PK;
			charge2.JR_OSSellAmt = 200m;
			charge2.JR_OH_SellAccount = orgHeader.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_PaymentType = ReceiptTypes.Cash;
			charge2.JR_APInvoiceNum = "TEST2222";
			charge2.JR_APInvoiceDate = ZDateTime.Now;
			charge2.JR_AB = bankAccount.PK;
			charge2.JR_ChequeNo = "111";
			charge2.JR_GE = department.PK;

			Factory.Save();

			AssertEquals("Shipment should not have errors. Errors: " + shipment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, shipment.HasErrors);

			job.RunPreSaveValidation();
			AssertEquals(string.Format("Job {0} should not have errors. Errors: {1}", job.JH_JobNum, job.NotificationsIncludingChildren.ToUniqueMessageListString()), false, job.HasErrors);

			InvoicingPostManagerGUIWrapper postManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job, null);
			postManagerWrapper.Post();

			APInvoice invoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST2222").AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
			AssertNotNull("APInvoice", invoice);
			AssertEquals("APInvoice.AH_Desc", "Job level overridden description", invoice.AH_Desc);

			query = new ZQuery(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.Cash);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);

			APPayment payment = Factory.LoadTop1<APPayment>(query);
			AssertNotNull("APPayment", payment);
			AssertEquals("APPayment.AH_Desc", "Job level overridden description", payment.AH_Desc);
		}

		#endregion

		#region Test InvoiceDateConfigurationHelper

		[TestDate(2011, 08, 11)]
		public void TestShipmentExportAir()
		{
			CreateAndAssertShipmentInvoice(Constants.TransportModes.Air, "AUSYD", "SGSIN", new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2011, 08, 11)]
		public void TestShipmentExportSea()
		{
			CreateAndAssertShipmentInvoice(Constants.TransportModes.Sea, "AUSYD", "SGSIN", new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2011, 08, 11)]
		public void TestShipmentImportAir()
		{
			CreateAndAssertShipmentInvoice(Constants.TransportModes.Air, "SGSIN", "AUSYD", new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 04, 01));
		}

		[TestDate(2011, 08, 11)]
		public void TestShipmentImportSea()
		{
			CreateAndAssertShipmentInvoice(Constants.TransportModes.Sea, "SGSIN", "AUSYD", new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 04, 01));
		}

		void CreateAndAssertShipmentInvoice(string transportMode, string origin, string destination, ZDateTime departureDate, ZDateTime arrivalDate, ZDateTime invoiceDate)
		{
			Job job = null;
			try
			{
				SetupRegistryForShipmentsAndConsols();

				ForwardingShipment shipment;
				SetupShipmentJob(out shipment, out job, transportMode, origin, destination, departureDate, arrivalDate);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null).Post();

				AssertInvoiceDate(invoiceDate);
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void AssertInvoiceDate(ZDateTime date)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ARInvoice[] invoices = newFactory.Load<ARInvoice>(new ZQuery());

			AssertEquals("one invoice should be created", 1, invoices.Length);
			AssertEquals("Invoice Date", date, invoices[0].AH_InvoiceDate);
		}

		void SetupShipmentJob(out ForwardingShipment shipment, out Job job, ZString transportMode, ZString origin, ZString destination, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			shipment = TestObjectCreator.CreateShipment("S00001001", origin, destination);
			shipment.JS_TransportMode = transportMode;
			shipment.JS_E_DEP = departureDate;
			shipment.JS_E_ARV = arrivalDate;

			job = TestObjectCreator.CreateJob(shipment);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 200m;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

			Factory.Save();
		}

		void SetupRegistryForShipmentsAndConsols()
		{
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;

			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();

			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "AIR", "ALL", "DEP", "EPM", "SGN", "SGN", "ADD", false, true);
			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "SEA", "ALL", "DEP", "EPP", "SGN", "SGN", "ADD", true, true);
			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "AIR", "ALL", "ARV", "EPM", "SGN", "SGN", "ADD", false, false);
			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "SEA", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD", false, false);

			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "EXP", "AIR", "", "DEP", "EPM", "SGN", "SGN", "ADD", false, true);
			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "EXP", "SEA", "", "DEP", "EPP", "SGN", "SGN", "ADD", true, true);
			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "IMP", "AIR", "", "ARV", "EPM", "SGN", "SGN", "ADD", false, false);
			TestObjectCreator.AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "IMP", "SEA", "", "ARV", "EPP", "SGN", "SGN", "ADD", false, false);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);
		}

		#endregion

		#region Post Date Defaulting

		void setupRegistryForPostDateDefaulting()
		{
			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			addPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "ALL", "ALL", "", "ARV", "EPM", "SGN", "SGN", "ADD");
			addPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "ALL", "ALL", "", "DEP", "EPM", "SGN", "SGN", "ADD");
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
		}

		void addPostDateConfiguration(BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration, string jobType, string direction, string mode, string broker,
									string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod)
		{
			PostDateConfiguration config = backDateAPInvoicesConfiguration.PostDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
		}

		#endregion

		#region InvoicePostingExchangeRateOption
		[TestDate(2015, 5, 10)]
		public override void TestPostWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupForInvoicePostingExchangeRateOption();
			var shipmentPK = CreateShipmentForInvoicePostingExchangeRateOption();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<ForwardingShipment>(shipmentPK);
			var job = new Job.Loader(shipment).Load();

			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, newFactory, job, null);
			wrapper.Post();
			AssertShipmentAndAPInvoiceForInvoicePostingExchangeRateOption(shipmentPK);

			//AR
			newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipmentPK);
			job = new Job.Loader(shipment).Load();
			wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, newFactory, job, null);
			wrapper.Post();
			AssertShipmentAndARInvoiceForInvoicePostingExchangeRateOption(shipmentPK);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2018, 7, 26)]
		public void TestPostCostCreatingCreditNoteWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			#region setup

			AssertEquals("precondition", new ZDateTime(2018, 7, 26).Date, ZDateTime.Now.Date);

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");//inv date equal today,So it is the same to set up TOD or INV.
			TestObjectCreator.CreateExchangeRate(this.USD, "BUY", 6.66M, new DateTime(2018, 7, 1), new DateTime(2018, 7, 30));

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			#endregion

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			job.ExchangeRates.AddRate(TestObjectCreator.USD, 6.22M, TestObjectCreator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);

			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge",
				TestObjectCreator.USD, -100M, TestObjectCreator.AALSHI, "charge",
				TestObjectCreator.USD, 0M);

			var oldLocalCostAmt = jobCharge.JR_LocalCostAmt;

			AssertNotEquals(job.ExchangeRates[0].JF_BaseRate, job.ExchangeRates[0].JF_TodayRate);

			jobCharge.JR_APInvoiceNum = TestObjectCreator.GetRandomString(8);
			jobCharge.JR_InvoiceType = "CUR";

			AssertEquals("USD", jobCharge.JR_CostCurrency);
			AssertEquals(job.ExchangeRates[0].JF_BaseRate, jobCharge.JR_OSCostExRate);
			AssertEquals(6.22M, job.ExchangeRates[0].JF_BaseRate);

			jobCharge.JR_APInvoiceDate = new ZDateTime(2018, 7, 26);
			Factory.Save();

			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job, null);
			wrapper.Post();

			Assert(ExceptionReporterTestListener.Instance.Count == 0);
			AssertEquals(6.66M, job.ExchangeRates[0].JF_TodayRate);
			AssertEquals("After posting Cost JF_BaseRate should be changed to JF_TodayRate.", job.ExchangeRates[0].JF_TodayRate, job.ExchangeRates[0].JF_BaseRate);
			AssertEquals("After posting Cost JR_OSCostExRate should be changed to TodayRate.", job.ExchangeRates[0].JF_TodayRate, jobCharge.JR_OSCostExRate);
			AssertNotEquals("Because TodayRate is not equal to BaseRate, after posting Cost JR_LocalCostAmt should be not equal old value.", oldLocalCostAmt, jobCharge.JR_LocalCostAmt);
			var accTransactionLines = Factory.Load<AccTransactionLines>(jobCharge.JR_AL_APLine);
			AssertEquals("Charge amount should be equal to line amount.", -accTransactionLines.AL_LineAmount, jobCharge.JR_LocalCostAmt);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 1)]
		public override void TestBackDatingARAPInvoiceWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupForBackDatingWithInvoicePostingExchangeRateOption();

			SetupJobData();
			SetupCostAndSellChargeData(Job1, 100, TestObjectCreator.CC1, 150);
			var charge1 = Job1.Charges[0];

			charge1.JR_RX_NKCostCurrency = "USD";
			charge1.JR_RX_NKSellCurrency = "USD";
			charge1.JR_InvoiceType = "CUR";

			AssertEquals(5.01m, charge1.JR_OSCostExRate);
			AssertEquals(5.01m, charge1.JR_OSSellExRate);

			charge1.JR_OSCostAmt = 100;
			charge1.JR_OSSellAmt = 150;

			AssertEquals(19.96M, charge1.JR_LocalCostAmt);
			AssertEquals(29.94M, charge1.JR_LocalSellAmt);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1, null);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Func<ChangeTransactionDatesMessageBox> getShownDialog;
			Func<IBusiness> getShownDialogBusinessEntity;
			HelperMethodsForTests.SetZFormModaliserToCatchShownDialogByType(out getShownDialog, out getShownDialogBusinessEntity);

			var invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(0, invoices.Length);

			wrapper.Post();
			invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(2, invoices.Length);

			var apInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AP"));
			var arInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, "AR"));

			AssertNotNull(apInvoice);
			AssertNotNull(arInvoice);

			AssertNotNull("Should have shown question about back dating", getShownDialog());
			var lastBizo = getShownDialogBusinessEntity() as ChangeTransactionDatesBusinessObject;
			AssertNotNull(lastBizo);

			var dateTimeExpected = new DateTime(2015, 4, 30);
			AssertEquals("Should have shown dialog box with correct InvoiceDate", dateTimeExpected, lastBizo.InvoiceDate.Date);
			AssertEquals("Should have shown dialog box with correct PostDate", dateTimeExpected, lastBizo.PostDate.Date);

			AssertEquals(dateTimeExpected, apInvoice.AH_PostDate.Date);

			AssertEquals(dateTimeExpected, arInvoice.AH_InvoiceDate.Date);
			AssertEquals(dateTimeExpected, arInvoice.AH_PostDate.Date);
			AssertEquals(dateTimeExpected, arInvoice.AH_DueDate.Date);

			//charges and lines should be updated to new rate and new amount.
			AssertEquals("USD", charge1.JR_RX_NKSellCurrency);
			AssertEquals(4.30m, charge1.JR_OSSellExRate);
			AssertEquals(150m, charge1.JR_OSSellAmt);
			AssertEquals(34.88M, charge1.JR_LocalSellAmt);

			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals(4.30m, charge1.JR_OSCostExRate);
			AssertEquals(100m, charge1.JR_OSCostAmt);
			AssertEquals(23.26M, charge1.JR_LocalCostAmt);

			AssertEquals("USD", apInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_PostedToEFT", true, apInvoice.AH_PostedToEFT);
			AssertEquals("AH_OSTotal", -110m, apInvoice.AH_OSTotal);
			AssertEquals("AH_LocalTotal", -25.59m, apInvoice.AH_LocalTotal);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 25.59.", 4.298554m, apInvoice.AH_ExchangeRate);
			var line1 = apInvoice.Lines[0];
			AssertEquals("USD", line1.AL_RX_NKTransactionCurrency);
			AssertEquals(4.30M, line1.AL_ExchangeRate.Round(2));
			AssertEquals(100m, line1.AL_OSExTaxAmount);
			AssertEquals(23.26M, line1.AL_LocalExTaxAmount);

			AssertEquals("USD", arInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(4.30M, arInvoice.AH_ExchangeRate);
			var line2 = arInvoice.Lines[0];
			AssertEquals("USD", line2.AL_RX_NKTransactionCurrency);
			AssertEquals(4.30M, line2.AL_ExchangeRate.Round(2));
			AssertEquals(150m, line2.AL_OSExTaxAmount);
			AssertEquals(34.88M, line2.AL_LocalExTaxAmount);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestCFXJournalAmountIsRecelculatedOnPostingWithDifferentExRateOptions()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			TestObjectCreator.ABIGAS.CompanyData.AccCFXConfigurations.SetUplifts("SHP", "ALL", "ALL", 2m, 0.2m);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.7m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.75m, ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.8m, ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(3));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.85m, ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(4));
			Factory.Save();

			foreach (var option in ExRateOption.CodeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				var backDateConfiguration = new BackDateInvoicesConfiguration();
				var expectedCfxAmount = decimal.Zero;
				var expectedLocalAmt = decimal.Zero;
				var expectedSellRate = decimal.Zero;

				if (option == ExRateOption.Default.Code)
				{
					expectedCfxAmount = 14.40m;
					expectedLocalAmt = 734.40m;
					expectedSellRate = 0.612m;
				}
				else if (option == ExRateOption.TodayExchangeRate.Code)
				{
					expectedCfxAmount = 16.80m;
					expectedLocalAmt = 856.80m;
					expectedSellRate = 0.714m;
				}
				else if (option == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
					|| option == ExRateOption.EarliestOfInvoiceOrTaxDate.Code)
				{
					expectedCfxAmount = 16.80m;
					expectedLocalAmt = 856.80m;
					expectedSellRate = 0.714m;

					// Moving Invoice Date with ARTerms
					var term = TestObjectCreator.ABIGAS.CompanyData.ARTerms[0];
					term.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
					term.PY_InvoiceTerm = InvoiceTermsList.FromShipmentDate.Code;
				}
				else if (option == ExRateOption.ExchangeRateBasedOnPostDate.Code)
				{
					//CFX Amount must be updated with (Today + 3)'s exchange rate !!!
					expectedCfxAmount = 19.20m;
					expectedLocalAmt = 979.20m;
					expectedSellRate = 0.816m;

					var term = TestObjectCreator.ABIGAS.CompanyData.ARTerms[0];
					term.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
					term.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
					backDateConfiguration = new BackDateInvoicesConfiguration() { OverridePostDate = true };
				}

				AssertCFXJournalAmountIsRecelculatedOnPosting(option, backDateConfiguration, expectedCfxAmount, expectedLocalAmt, expectedSellRate);
			}
		}

		void AssertCFXJournalAmountIsRecelculatedOnPosting(string postingExchangeRateOption, BackDateInvoicesConfiguration backDateConfiguration, decimal expectedCfxAmount, decimal expectedLocalAmt, decimal expectedSellRate)
		{
			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, backDateConfiguration))
			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingExchangeRateOption))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001" + postingExchangeRateOption);
				shipment.JS_E_ARV = ZDateTime.Today.AddDays(2);

				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.ABIGAS, 0, TestObjectCreator.Agent, 0);
				job.PlugInData = shipment;
				TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 0.6m);

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Test", TestObjectCreator.AUD, 0m, null,
					TestObjectCreator.USD, 1200m, TestObjectCreator.ABIGAS);
				charge.JR_SellTaxDate = ZDate.Today.AddDays(4);
				AssertEquals(734.40m, charge.JR_LocalSellAmt);
				AssertEquals("Charge CFX amount. Option: " + postingExchangeRateOption, 14.40m, charge.JR_CFXAmt);

				Factory.Save();

				var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form.GetType() == typeof(ChangeTransactionDatesMessageBox))
					{
						var changeDatesBizo = (ChangeTransactionDatesBusinessObject)((ZForm)form).BusinessEntity;
						changeDatesBizo.PostDate = ZDateTime.Today.AddDays(3);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					}
				});

				var existingTransactions = Factory.Load<AccTransactionHeader>(new ZQuery());

				wrapper.Post();

				var invoiceQuery = new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, existingTransactions.Select(x => x.PK));
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				var invoices = Factory.Load<InvoicingBase>(invoiceQuery);
				AssertEquals("Should found one transaction. Option: " + postingExchangeRateOption, 1, invoices.Length);

				AssertEquals("Should be one Charge. Option: " + postingExchangeRateOption, 1, job.Charges.Count);
				AssertEquals(charge.PK, job.Charges[0].PK);
				Assert("Charge Revenue is posted. Option: " + postingExchangeRateOption, charge.IsRevenuePosted);
				Assert("Charge CFX is posted. Option: " + postingExchangeRateOption, charge.IsCFXPosted);
				AssertEquals("Charge CFX amount should not be changed. Option: " + postingExchangeRateOption, expectedCfxAmount, charge.JR_CFXAmt);
				AssertNotNull(charge.CFXLine);
				AssertEquals("CFX Journal amount should be as expected. Option: " + postingExchangeRateOption, -expectedCfxAmount, charge.CFXLine.AL_LineAmount);
				AssertEquals("Local Sell amount should be as expected. Option: " + postingExchangeRateOption, expectedLocalAmt, charge.JR_LocalSellAmt);
				AssertEquals("Local Sell rate should be as expected. Option: " + postingExchangeRateOption, expectedSellRate, charge.JR_OSSellExRate);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		[TestDate(2018, 3, 15)]
		public void TestCFXJournalWhenExchangeIsUpdated()
		{
			AssertEquals("precondition", new ZDateTime(2018, 3, 15).Date, ZDateTime.Now.Date);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			TestObjectCreator.CreateUSDBuyRate(0.84m, new DateTime(2018, 3, 15));

			var shipment = TestObjectCreator.CreateShipment("S0001234");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.AALSHI, 5M, TestObjectCreator.ABIGAS, 2M);
			var rate = CreateExchangeRate(job, TestObjectCreator.USD, 0.77M);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeA",
				TestObjectCreator.AUD, 1000M, TestObjectCreator.AALSHI, "chargeA",
				TestObjectCreator.USD, 770M, TestObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			AssertEquals("CFX", 2m, charge.JR_LineCFX);
			AssertEquals("CFX Amount,should be 770/0.7546 - 770/0.77 = 20.41", 20.41m, charge.JR_CFXAmt);

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			job = new Job.Loader(shipment).Load();
			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, newFactory, job, null);
			wrapper.Post();

			AssertEquals("Job Ex Rate should be updated to new rate", 0.84m, rate.JF_BaseRate);
			AssertEquals("CFX Amount,should be 770/0.8232 - 770/0.84 = 18.7", 18.7m, charge.JR_CFXAmt);
		}

		[TestDate(2015, 5, 10)]
		public void TestARAPInvoiceWithInvoicePostingExchangeRateOptionInvoiceTypeFIN()
		{
			SetupForInvoicePostingExchangeRateOption();

			var shipment = TestObjectCreator.CreateShipment("S0001234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			var usdExRate = job.ExchangeRates.AddNew();
			usdExRate.JF_RX_NKRateCurrency = "USD";
			usdExRate.JF_BaseRate = 3.1m;

			var chargeA = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeA",
				TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "chargeA",
				TestObjectCreator.USD, 100M, TestObjectCreator.ABIGAS);

			chargeA.JR_InvoiceType = "FIN";
			chargeA.JR_OSCostExRate = 3.1m;

			AssertEquals("USD", chargeA.JR_CostCurrency);
			AssertEquals(3.1m, chargeA.JR_OSCostExRate);

			Factory.Save();

			//AR
			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			job = new Job.Loader(shipment).Load();
			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, newFactory, job, null);
			wrapper.Post();

			AssertEquals(1, job.Charges.Count);
			AssertEquals("USD", job.Charges[0].JR_CostCurrency);
			AssertEquals(5.1m, job.Charges[0].JR_OSCostExRate); //charge should be updated to new rate
			AssertEquals("Job Ex Rate should be updated to new rate", 5.1m, usdExRate.JF_BaseRate);
			AssertEquals("USD", job.Charges[0].JR_SellCurrency);
			AssertEquals(5.1m, job.Charges[0].JR_OSSellExRate); //charge should be updated to new rate

			//invoice should be posted in AUD
			var arQuery = new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK);
			arQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, "AR");
			arQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);

			var arInvoice = newFactory.LoadTop1<ARInvoice>(arQuery);
			AssertEquals(1, arInvoice.Lines.Count);
			AssertEquals("AUD", arInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals(1m, arInvoice.AH_ExchangeRate);
			AssertEquals("AUD", arInvoice.Lines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(1m, arInvoice.Lines[0].AL_ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestBranchLevelPosting_JobBranch_TakesPrecedenceOverRegistryFallback()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var shipment = TestObjectCreator.CreateShipment("S0001234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			job.JH_GB = branch1.PK;

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeA",
				TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, "chargeA",
				TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = "FIN";
			charge1.JR_GB = branch1.PK;

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "chargeA",
				TestObjectCreator.AUD, 200M, TestObjectCreator.AALSHI, "chargeA",
				TestObjectCreator.AUD, 200M, TestObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = "FIN";
			charge2.JR_GB = branch2.PK;

			Factory.Save();

			//AR
			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			job = new Job.Loader(shipment).Load();
			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, newFactory, job, null);
			wrapper.Post();

			Assert("both charges are posted", job.Charges.Cast<Charge>().All(x => x.IsRevenuePosted));

			var arQuery = new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK);
			arQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, "AR");
			arQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);

			var arInvoice = newFactory.LoadTop1<ARInvoice>(arQuery);
			AssertEquals(2, arInvoice.Lines.Count);
			AssertEquals(1, arInvoice.Lines.Cast<InvoicingLineBase>().Count(x => x.AL_GB == branch1.PK));
			AssertEquals(1, arInvoice.Lines.Cast<InvoicingLineBase>().Count(x => x.AL_GB == branch2.PK));

			AssertEquals("Header branch is selected as job branch", job.JH_GB, arInvoice.AH_GB);
		}

		[TestDate(2016, 1, 15)]
		public void TestPostWithInvoicePostingExchangeRateOptionWithIgnoredInvoice()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			(new AccountingPeriodTestHelper()).SetupPeriods();

			TestObjectCreator.CreateUSDBuyRate(1.5m, new DateTime(2015, 12, 31));
			TestObjectCreator.CreateUSDBuyRate(1.8m, new DateTime(2016, 1, 6));

			TestObjectCreator.CreateUSDBuyRate(1.4m, new DateTime(2016, 1, 15));

			var shipment = TestObjectCreator.CreateShipment("S1234ABC");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			var chargeA = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeA",
				TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "chargeA",
				TestObjectCreator.USD, 100M, null);

			var chargeB = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "chargeB",
				TestObjectCreator.USD, 200M, TestObjectCreator.AALSHI, "chargeB",
				TestObjectCreator.USD, 200M, null);

			chargeA.JR_APInvoiceDate = new ZDateTime(2015, 12, 31);
			chargeB.JR_APInvoiceDate = new ZDateTime(2016, 1, 6);

			chargeA.CostExchangeRate.SetBuyRate_ForTestOnly(1.5m);
			chargeA.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.5m);

			chargeB.CostExchangeRate.SetBuyRate_ForTestOnly(1.8m);
			chargeB.RevenueExchangeRate.SetBuyRate_ForTestOnly(1.8m);

			chargeA.JR_APInvoiceNum = "INV1";
			chargeB.JR_APInvoiceNum = "INV2";
			Factory.Save();

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceCharges = new APInvoiceCharges(chargeB.CostAccount.OH_Code, chargeB.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			invoiceCharges.Charges.Add(chargeB);
			request.InitializeJobRelated(invoiceCharges, job.PK, job.TablePrefix);
			chargeB.JR_APInvoiceNum += "_1";
			Factory.Save();

			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Costs, Factory, job, null);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var isChargeBExcludedFromPosting = false;
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var result = DialogResult.None;
					var dialogType = dialog.GetType();

					if (dialogType == typeof(APInvoicePostingWithApprovalRequestSummaryForm))
					{
						var apInvoiceChargesCollection = ((dialog as APInvoicePostingWithApprovalRequestSummaryForm).BusinessEntity as APInvoiceChargesCollection);
						var chargesB = apInvoiceChargesCollection.Cast<APInvoiceCharges>().First(x => x.InvoiceNumber == "INV2_1");
						isChargeBExcludedFromPosting = chargesB.IsExcludedFromPosting = true;
						result = DialogResult.OK;
					}
					else
					{
						throw new NotImplementedException(dialog.ToString());
					}

					ZFormModaliser.ResultToReturnFromShowDialog = result;
				});

			AssertNull(Factory.LoadTop1<APInvoice>(new ZQuery()));

			wrapper.Post();
			Factory.Save();
			AssertEquals(true, isChargeBExcludedFromPosting);

			var apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(1, apInvoices.Length);

			Assert(chargeA.JR_OSCostExRate == 1.5m && chargeA.JR_OSSellExRate == 1.8m);
			Assert(chargeB.JR_OSCostExRate == 1.5m && chargeB.JR_OSSellExRate == 1.8m);

			Assert(chargeA.IsCostPosted && !chargeA.IsRevenuePosted);
			Assert(!chargeB.IsCostPosted && !chargeB.IsRevenuePosted);

			AssertEquals("USD", job.ExchangeRates[0].JF_RX_NKRateCurrency);
			AssertEquals(1.5m, job.ExchangeRates[0].JF_BaseRate);
			AssertEquals("AH_PostedToEFT", true, apInvoices[0].AH_PostedToEFT);

			AssertEquals("AH_PostedToEFT", true, apInvoices[0].AH_PostedToEFT);
			AssertEquals("AH_OSTotal", -110m, apInvoices[0].AH_OSTotal);
			AssertEquals("AH_LocalTotal", -73.34m, apInvoices[0].AH_LocalTotal);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 73.3400.", 1.499864m, apInvoices[0].AH_ExchangeRate);
			AssertEquals("INV1", apInvoices[0].AH_TransactionNum);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		#endregion

		public void TestPostingFactoryRefresh()
		{
			AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);

			var job = SetupDataForPostingFactoryRefreshTest("S001001");
			var wrapper = new TestInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, () => { });
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				AssertEquals("Precondition: total number of charges should be 2", 2, wrapper.TotalNumberOfCharges);
				AssertEquals("Refresh is enabled", true, wrapper.IsTransactionFactoryRefreshEnabled);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals("Refresh is enabled as total number of charges is not greater than the registry defined value", true, wrapper.IsTransactionFactoryRefreshEnabled);
			}

			AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

			job = SetupDataForPostingFactoryRefreshTest("S001002");
			wrapper = new TestInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, () => { });
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				AssertEquals("Precondition: total number of charges should be 2", 2, wrapper.TotalNumberOfCharges);
				AssertEquals("Refresh is enabled", true, wrapper.IsTransactionFactoryRefreshEnabled);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals("Refresh is enabled as total number of charges is not greater than the registry defined value", true, wrapper.IsTransactionFactoryRefreshEnabled);
			}

			AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1);

			job = SetupDataForPostingFactoryRefreshTest("S001003");
			wrapper = new TestInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, () => { });
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				AssertEquals("Precondition: total number of charges should be 2", 2, wrapper.TotalNumberOfCharges);
				AssertEquals("Refresh is enabled", true, wrapper.IsTransactionFactoryRefreshEnabled);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();
				AssertEquals("Refresh should be disabled as total number of charges is greater than the registry defined value", false, wrapper.IsTransactionFactoryRefreshEnabled);
			}
		}

		Job SetupDataForPostingFactoryRefreshTest(string shipmentNumber)
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge1", TestObjectCreator.AUD, 1500M, TestObjectCreator.AALSHI, "abc" + shipmentNumber, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "test charge2", TestObjectCreator.AUD, 2000M, TestObjectCreator.AALSHI, "def" + shipmentNumber, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			return job;
		}

		public void TestPostingDbHit()
		{
			AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge1", TestObjectCreator.AUD, 1500M, TestObjectCreator.AALSHI, "abc1234", TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "test charge2", TestObjectCreator.AUD, 2000M, TestObjectCreator.AALSHI, "def1234", TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "test charge3", TestObjectCreator.AUD, 2000M, TestObjectCreator.AALSHI, "ghi1234", TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			using (InvoicingPluginToFreight invoicingPluginToFreight = new InvoicingPluginToFreight(shipment))
			{
				var wrapper = new TestInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, () => { invoicingPluginToFreight.UpdateJobChargeGrid(); });
				using (Form form = new Form())
				{
					wrapper.ParentForm = form;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					int beforePostingLineDBHits = Factory.GetTableHitCount(AccTransactionLinesSchema.Constants.TableName);
					int beforePostingChargeDBHits = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
					wrapper.Post();
					int afterPostingLineDBHits = Factory.GetTableHitCount(AccTransactionLinesSchema.Constants.TableName);
					int afterPostingChargeDBHits = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
					ExceptionReporterTestListener.Instance.Clear();
					AssertEquals("Precondition: 3 charges shoud be posted", 3, job.Charges.Cast<Charge>().Count(x => x.JR_IsCostPosted || x.JR_IsRevenuePosted));
					AssertEquals("Should be 1 hit", 1, afterPostingLineDBHits - beforePostingLineDBHits);
					AssertEquals("Should be 1 hit", 1, afterPostingChargeDBHits - beforePostingChargeDBHits);
				}
			}
		}

		[ExpectNoExceptions("This transaction exchange rate is less than or equal to 0")]
		[TestDate(2019, 02, 28)]
		public void TestPostOverseasAgentInForeignCurrencyWhenNoRatePresent()
		{
			PostingExRateRegistryAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolSendingAgent = TestObjectCreator.ABIGAS;
			var consolReceivingAgent = TestObjectCreator.AALSHI;
			consol.JK_OA_SendingForwarderAddress = consolSendingAgent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = consolReceivingAgent.MainAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var currency = TestObjectCreator.USD;
			AssertEquals("Precondition: No exchange rate for today", 0M, Env.CurrentCompany.ExchangeRate.TodaysRate(currency.Code, ExchangeRateType.Buy));
			AssertEquals("Precondition: No exchange rate for today", 0M, Env.CurrentCompany.ExchangeRate.TodaysRate(currency.Code, ExchangeRateType.Sell));

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "test charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.USD, 100M, consolSendingAgent);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("Precondition: Charge is treated as agent charge", charge.IsAgentCharge);
			Factory.Save();
			var postManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, job, null);
			try
			{
				postManagerWrapper.Post();
			}
			catch (Exception ex)
			{
				Fail(string.Format("No exception should be thrown. But following exception was thrown:{0}{1}", System.Environment.NewLine, ex.Message));
			}
			string expectedMessage = "This job cannot be posted. The \"AR Invoice Posting Exchange Rate Option\" has been set to \"Today Exchange Rate\".\r\nBut the USD exchange rate is not set for the date 28-Feb-19. Please check your data and try again.";
			AssertEquals("Error message is shown as no exchange rate is set for USD", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			consolReceivingAgent.CompanyData.OB_IsDebtor = true;
			charge.JR_OH_SellAccount = consolReceivingAgent.PK;
			Assert("Precondition: Charge is treated as agent charge", charge.IsAgentCharge);
			Factory.Save();
			postManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, job, null);
			try
			{
				postManagerWrapper.Post();
			}
			catch (Exception ex)
			{
				Fail(string.Format("No exception should be thrown. But following exception was thrown:{0}{1}", System.Environment.NewLine, ex.Message));
			}
			AssertEquals("Error message is shown as no exchange rate is set for USD", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2020, 10, 10)]
		[ExpectNoExceptions]
		public void TestExchangeRateZeroCriticalValidationNotThrown_ShipmentLevelPosting_ForeignSellInvoiceCurrency()
		{
			var today = ZDateTime.Today;
			var backDate = new ZDateTime(2020, 09, 30);

			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.5M, today, today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.6M, backDate, backDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.7M, today, today);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.DefaultPostDateFromInvoiceDate = true;
			config.OverridePostDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var localClient = TestObjectCreator.LocalClient;
			var agent = CreateOrgHeader("TESTAGENT", true, true);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			Factory.Save();

			var localCurrency = TestObjectCreator.AUD;
			var charge = TestObjectCreator.CreateCharge(job, localCurrency, localCurrency, 300M, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, TestObjectCreator.FRT, "FIN");
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100M;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			AssertNoExceptionThrown("No CV with 'This transaction exchange rate is less than or equal to 0.' should occur", () => jobPostManagerWrapper.Post());

			var expectedErrorMessage = "AR Invoice number " + @"
The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 30-Sep-20. Please check your data and try again.";

			AssertMultilineASCIIEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			var sellExchangeRate_USD = 0.75M;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", sellExchangeRate_USD, backDate, backDate);
			Factory.Save();

			jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			jobPostManagerWrapper.Post();

			var arInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice));

			AssertNotNull(arInvoice);

			AssertEquals("AR Invoice post date", backDate, arInvoice.AH_PostDate);
			AssertEquals("AR Invoice exchange rate", sellExchangeRate_USD, arInvoice.AH_ExchangeRate);
		}

		[TestDate(2020, 10, 10)]
		[ExpectNoExceptions]
		public void TestExchangeRateZeroCriticalValidationNotThrown_ShipmentLevelPosting_ForeignCurrencyInvoice()
		{
			var today = ZDateTime.Today;
			var backDate = new ZDateTime(2020, 09, 30);

			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.5M, today, today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.6M, backDate, backDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.7M, today, today);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.DefaultPostDateFromInvoiceDate = true;
			config.OverridePostDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var localClient = TestObjectCreator.LocalClient;
			var agent = CreateOrgHeader("TESTAGENT", true, true);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			Factory.Save();

			var localCurrency = TestObjectCreator.AUD;
			var foreignCurrency = TestObjectCreator.USD;
			var charge = TestObjectCreator.CreateCharge(job, localCurrency, foreignCurrency, 300M, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, TestObjectCreator.FRT, InvoiceTypesList.Codes.ForeignCurrencyInvoice);
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100M;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			AssertNoExceptionThrown("No CV with 'This transaction exchange rate is less than or equal to 0.' should occur", () => jobPostManagerWrapper.Post());

			var expectedErrorMessage = "AR Invoice number " + @"
The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 30-Sep-20. Please check your data and try again.";

			AssertMultilineASCIIEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			var sellExchangeRate_USD = 0.75M;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", sellExchangeRate_USD, backDate, backDate);
			Factory.Save();

			jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			jobPostManagerWrapper.Post();

			var arInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice));

			AssertNotNull(arInvoice);

			AssertEquals("AR Invoice post date", backDate, arInvoice.AH_PostDate);
			AssertEquals("AR Invoice exchange rate", sellExchangeRate_USD, arInvoice.AH_ExchangeRate);
		}

		[TestDate(2020, 10, 10)]
		[ExpectNoExceptions]
		public void TestExchangeRateZeroCriticalValidationNotThrown_ShipmentLevelPosting_ForeignCurrencyCharge_LocalCurrencyInvoice()
		{
			var today = ZDateTime.Today;
			var backDate = new ZDateTime(2020, 09, 30);

			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.5M, today, today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.6M, backDate, backDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.7M, today, today);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.DefaultPostDateFromInvoiceDate = true;
			config.OverridePostDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var localClient = TestObjectCreator.LocalClient;
			var agent = CreateOrgHeader("TESTAGENT", true, true);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			Factory.Save();

			var localCurrency = TestObjectCreator.AUD;
			var charge = TestObjectCreator.CreateCharge(job, localCurrency, localCurrency, 300M, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, TestObjectCreator.FRT, "FIN");
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100M;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			AssertNoExceptionThrown("No CV with 'This transaction exchange rate is less than or equal to 0.' should occur", () => jobPostManagerWrapper.Post());

			var expectedErrorMessage = "AR Invoice number " + @"
The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 30-Sep-20. Please check your data and try again.";

			AssertMultilineASCIIEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			var sellExchangeRate_USD = 0.75M;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", sellExchangeRate_USD, backDate, backDate);
			Factory.Save();

			jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			jobPostManagerWrapper.Post();

			var arInvoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable)
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice));

			AssertNotNull(arInvoice);

			AssertEquals("AR Invoice post date", backDate, arInvoice.AH_PostDate);
			AssertEquals("AR Invoice currency", localCurrency.RX_Code, arInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("AR Invoice exchange rate", 1M, arInvoice.AH_ExchangeRate);
			AssertEquals("AR Invoice OS amount", 93.33M, arInvoice.AH_OSTotalAmount);
			AssertEquals("AR Invoice Local amount", 93.33M, arInvoice.AH_LocalTotalAmount);
		}

		[TestDate(2020, 10, 10)]
		[ExpectNoExceptions]
		public void TestErrorMessageShowsCorrectDateWhenExchangeRateIsZero()
		{
			var today = ZDateTime.Today;
			var backDate = new ZDateTime(2020, 09, 30);

			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.5M, today, today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.6M, backDate, backDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.7M, today, today);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");

			AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BackDateInvoicesConfiguration config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.InvoiceDateConfigurationCollection[0].Override = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var localClient = TestObjectCreator.LocalClient;
			var agent = CreateOrgHeader("TESTAGENT", true, true);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);

			Factory.Save();

			var localCurrency = TestObjectCreator.AUD;
			var charge = TestObjectCreator.CreateCharge(job, localCurrency, localCurrency, 300M, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, TestObjectCreator.FRT, "FIN");
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100M;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var jobPostManagerWrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, Factory, job, null);
			AssertNoExceptionThrown("No CV with 'This transaction exchange rate is less than or equal to 0.' should occur", () => jobPostManagerWrapper.Post());

			var expectedErrorMessage = "AR Invoice number " + @"
The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 30-Sep-20. Please check your data and try again.";

			AssertMultilineASCIIEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region NothingPostedHandler

		protected override PostManagerGUIWrapper PrepareTestDataForNothingPostedHandler()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, 0.6m);

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "zero amount charge", TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 0M, TestObjectCreator.LocalClient);

			Factory.Save();

			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, null);
			wrapper.DoTestPostTransactions = true;

			return wrapper;
		}

		protected override void AssertNothingPostedHandlerCore(PostManagerGUIWrapper wrapper)
		{
			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();

				var expectedErrorMessage = GetExpectedMessageForNothingPosted();
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNothingPostedHandler_WhenOverrideNothingPostedMessage()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, 0.6m);

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "zero amount charge", TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 0M, TestObjectCreator.LocalClient);

			Factory.Save();

			var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, job, null);
			wrapper.DoTestPostTransactions = true;
			wrapper.OverriddenNothingPostedMessage = (NoResString)"Test Overridden Nothing Posted Message.";

			using (Form form = new Form())
			{
				wrapper.ParentForm = form;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wrapper.Post();
				ExceptionReporterTestListener.Instance.Clear();

				AssertEquals("Test Overridden Nothing Posted Message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Job job = Job1;
			Factory.Save();
#if !WINZOR
			MockENettWebService.ClearInstance();
#endif
		}

#if !WINZOR
		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}
#endif

		protected override PostManagerGUIWrapper GUIWrapper
		{
			get
			{
				if (GUIWrapper_inner == null)
				{
					GUIWrapper_inner = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Job1, null);
					GUIWrapper_inner.DoTestPostTransactions = true;
				}
				return GUIWrapper_inner;
			}
		}
		InvoicingPostManagerGUIWrapper GUIWrapper_inner;

		#endregion

		public class TestInvoicingPostManagerGUIWrapper : TestPostManagerGUIWrapper
		{
			public TestInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory factory, Job job, Action refreshJobCharges = null)
					: base(postingOption, factory, job, refreshJobCharges)
			{
			}

			public bool IsTransactionFactoryRefreshEnabled
			{
				get { return TransactionFactory.RefreshEnabled; }
			}

			public int TotalNumberOfCharges
			{
				get { return PostManager.TotalNumberOfCharges; }
			}
		}
	}
}
