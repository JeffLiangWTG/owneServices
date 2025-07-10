using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class AccEPaymentDealToGEPConverterTest : TestCaseWithFactory
	{
		public void TestPayReasonIsIncludedInPayload()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var australiaCompany = objectCreator.CreateCompanyAndBranch("AUMEL");
			australiaCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var sydneyBranch = australiaCompany.FirstActiveBranch;
			var melbourneBranch = objectCreator.CreateNewBranch(australiaCompany, "BB2");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(australiaCompany.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), australiaCompany.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote1;
			EPaymentQuote quote2;
			EPaymentDeal deal1;
			EPaymentDeal deal2;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), sydneyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote1 = testHelper.CreateQuote(australiaCompany, ofxBankAccount);
				quote2 = testHelper.CreateQuote(australiaCompany, ofxBankAccount);
				quote2.PaymentApproval.AV_GB = melbourneBranch.PK;

				FillQuote(quote1);
				FillQuote(quote2);
				Factory.Save();

				deal1 = testHelper.CreateDeal(quote1);
				deal2 = testHelper.CreateDeal(quote2);
				Factory.Save();
			}

			var paymentApproval1 = Factory.Load<PaymentApprovalBase>(deal1.Quote.QU_AV);
			AssertEquals("Precondition : paymentApproval1 branch must be sydneyBranch", sydneyBranch.PK, paymentApproval1.AV_GB);
			paymentApproval1.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.EmployeePaymentSalaryWages;
			var beneficiary1 = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails1 = paymentApproval1.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails1.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails1.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary1.PK;

			var paymentApproval2 = Factory.Load<PaymentApprovalBase>(deal2.Quote.QU_AV);
			AssertEquals("Precondition : paymentApproval2 branch must be melbourneBranch", melbourneBranch.PK, paymentApproval2.AV_GB);
			paymentApproval2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.SoftwareConsultancyImplementation;
			var beneficiary2 = objectCreator.CreateEPaymentBeneficiary("fa1f8587-3a9a-424b-ace3-47173ef9ab24");
			var accountDetails2 = paymentApproval2.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails2.RemoveAndDeleteAll();
			var accountDetail2 = accountDetails2.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_EPaymentBeneficiaryId = beneficiary2.PK;
			Factory.Save();

			var converter = new AccEPaymentDealToGEPConverter();
			var ePayment1 = converter.ConvertDealToGEP(deal1);
			var ePayment2 = converter.ConvertDealToGEP(deal2);

			var expectedPayloadForDeal1 = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Employee payment, salary\/wages"",""payReference"":""""}]}";
			var payLoadForDeal1 = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment1.Payload));
			AssertEquals("JSON payload", expectedPayloadForDeal1, payLoadForDeal1);

			var expectedPayloadForDeal2 = @"{""dealInternalReference"":""00001001"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""fa1f8587-3a9a-424b-ace3-47173ef9ab24"",""amount"":1000,""payReason"":""Software consultancy\/implementation"",""payReference"":""""}]}";
			var payLoadForDeal2 = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment2.Payload));
			AssertEquals("JSON payload", expectedPayloadForDeal2, payLoadForDeal2);
		}

		#region Payment Reference

		public void TestPaymentReference_PRN()
		{
			AssertPaymentReferenceCore(EPaymentReferenceTypes.PaymentReferenceNum, null, ",\"payReference\":\"Payment Reference For Test\"");
		}

		public void TestPaymentReference_TXT()
		{
			AssertPaymentReferenceCore(EPaymentReferenceTypes.FreeText, "Payment Reference For Test", ",\"payReference\":\"Payment Reference For Test\"");
		}

		public void TestPaymentReference_TXT_NoMatchedCurrency()
		{
			AssertPaymentReferenceCore(EPaymentReferenceTypes.FreeText, "Payment Reference For Test", string.Empty, Core.Constants.CurrencyCodes.Australia);
		}

		public void TestPaymentReference_INV()
		{
			AssertPaymentReferenceCore(EPaymentReferenceTypes.InvoiceNumbers, null, ",\"payReference\":\"I001 I002\"");
		}

		#endregion

		void AssertPaymentReferenceCore(string paymentReferenceType, string paymentReference, string expectedPaymentReference, string currency = Core.Constants.CurrencyCodes.UnitedStates)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(testObjectCreator);

			var australiaCompany = testObjectCreator.CreateCompanyAndBranch("AUMEL");
			australiaCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var sydneyBranch = australiaCompany.FirstActiveBranch;
			sydneyBranch.GB_Code = "SY1";
			var melbourneBranch = testObjectCreator.CreateNewBranch(australiaCompany, "ME1");
			var quoteCreatingUser = testObjectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(australiaCompany.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), australiaCompany.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentDeal deal1;
			EPaymentDeal deal2;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), sydneyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var quote1 = testHelper.CreateQuote(australiaCompany, ofxBankAccount);
				var quote2 = testHelper.CreateQuote(australiaCompany, ofxBankAccount);
				quote1.PaymentApproval.AV_GB = sydneyBranch.PK;
				quote2.PaymentApproval.AV_GB = melbourneBranch.PK;

				FillQuote(quote1);
				FillQuote(quote2);
				Factory.Save();

				var apInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "I001", testObjectCreator.USD, 3m, 600m, 0m, 200m, 0m, testObjectCreator.Creditor1, testObjectCreator.GLHeader1.PK);
				var apInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "I002", testObjectCreator.USD, 3m, 300m, 0m, 100m, 0m, testObjectCreator.Creditor1, testObjectCreator.GLHeader1.PK);
				Factory.Save();

				var invoices = new InvoicingBaseCollection(Factory);
				invoices.AddRange(new[] { apInvoice1, apInvoice2 });
				var paymentApprovalBase = Factory.Load<PaymentApprovalBase>(quote1.PaymentApproval.PK);
				paymentApprovalBase.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());
				Factory.Save();

				deal1 = testHelper.CreateDeal(quote1);
				deal2 = testHelper.CreateDeal(quote2);
				Factory.Save();
			}

			var paymentApproval1 = Factory.Load<PaymentApprovalBase>(deal1.Quote.QU_AV);
			AssertEquals("Precondition : paymentApproval1 branch is SYD", sydneyBranch.PK, paymentApproval1.AV_GB);
			paymentApproval1.AV_ChequeOrReference = "Payment Reference For Test";

			var beneficiary1 = testObjectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetailsCollection1 = paymentApproval1.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetailsCollection1.RemoveAndDeleteAll();

			var accountDetails1 = testObjectCreator.AddAPBankAccountDetails(paymentApproval1.PayeeOrganisation, EPaymentMethods.EPaymentViaOFX, currency);
			accountDetails1.A1_EPaymentBeneficiaryId = beneficiary1.PK;
			accountDetails1.A1_EPaymentReferenceType = paymentReferenceType;
			accountDetails1.A1_EPaymentReference = paymentReference;

			Factory.Save();

			var paymentApproval2 = Factory.Load<PaymentApprovalBase>(deal2.Quote.QU_AV);
			AssertEquals("Precondition : paymentApproval2 branch is MEL", melbourneBranch.PK, paymentApproval2.AV_GB);

			var beneficiary2 = testObjectCreator.CreateEPaymentBeneficiary("fa1f8587-3a9a-424b-ace3-47173ef9ab24");
			var accountDetailsCollection2 = paymentApproval2.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetailsCollection2.RemoveAndDeleteAll();

			var accountDetails2 = testObjectCreator.AddAPBankAccountDetails(paymentApproval2.PayeeOrganisation, EPaymentMethods.EPaymentViaOFX, currency);
			accountDetails2.A1_EPaymentBeneficiaryId = beneficiary2.PK;

			Factory.Save();

			var converter = new AccEPaymentDealToGEPConverter();
			var ePayment1 = converter.ConvertDealToGEP(deal1);
			var ePayment2 = converter.ConvertDealToGEP(deal2);

			var payeeId1 = (currency == Core.Constants.CurrencyCodes.UnitedStates) ? "f94f9527-8160-4ea4-8a4f-9be892c81dc4" : string.Empty;
			var payeeId2 = (currency == Core.Constants.CurrencyCodes.UnitedStates) ? "fa1f8587-3a9a-424b-ace3-47173ef9ab24" : string.Empty;

			var expectedPayloadForDeal1 = "{\"dealInternalReference\":\"00001000\",\"quoteProviderReference\":\"testreference\",\"bankAccountCode\":\"EPA\",\"minFundAmount\":400,\"maxFundAmount\":400,\"fundCurrency\":\"AUD\",\"payAmount\":1000,\"payCurrency\":\"USD\",\"paymentItems\":[{\"payeeId\":\"" + payeeId1 + "\",\"amount\":1000,\"payReason\":\"\"" + expectedPaymentReference + "}]}";
			AssertEquals(expectedPayloadForDeal1, Encoding.UTF8.GetString(Convert.FromBase64String(ePayment1.Payload)));
			if (currency == Core.Constants.CurrencyCodes.Australia)
			{
				var expectedPayloadForDeal2 = "{\"dealInternalReference\":\"00001001\",\"quoteProviderReference\":\"testreference\",\"bankAccountCode\":\"EPA\",\"minFundAmount\":400,\"maxFundAmount\":400,\"fundCurrency\":\"AUD\",\"payAmount\":1000,\"payCurrency\":\"USD\",\"paymentItems\":[{\"payeeId\":\"" + payeeId2 + "\",\"amount\":1000,\"payReason\":\"\"}]}";
				AssertEquals(expectedPayloadForDeal2, Encoding.UTF8.GetString(Convert.FromBase64String(ePayment2.Payload)));
			}
			else
			{
				var expectedPayloadForDeal2 = "{\"dealInternalReference\":\"00001001\",\"quoteProviderReference\":\"testreference\",\"bankAccountCode\":\"EPA\",\"minFundAmount\":400,\"maxFundAmount\":400,\"fundCurrency\":\"AUD\",\"payAmount\":1000,\"payCurrency\":\"USD\",\"paymentItems\":[{\"payeeId\":\"" + payeeId2 + "\",\"amount\":1000,\"payReason\":\"\",\"payReference\":\"\"}]}";
				AssertEquals(expectedPayloadForDeal2, Encoding.UTF8.GetString(Convert.FromBase64String(ePayment2.Payload)));
			}
		}
		[TestDate(2021, 4, 9)]
		public void TestMinimumFundAmountAndMaximumFundAmountInThePayLoadWithRebookExpiredQuotesBasedOnExRateToleranceRegistrySetting()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			var config = new ExchangeRateToleranceConfiguration();
			config.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForUSD = new ExchangeRateTolerance
			{
				Currency = Core.Constants.CurrencyCodes.UnitedStates,
				ExchangeRateTolerancePercentage = 5
			};
			config.ExchangeRateToleranceCollection.Add(toleranceForUSD);

			var converter = new AccEPaymentDealToGEPConverter();
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config))
			{
				AssertPayload(Guid.Empty, false);
				AssertPayload(Guid.Empty, true);
				AssertPayload(deal.AED_GC_Company.ToGuid(), false);
				AssertPayload(deal.AED_GC_Company.ToGuid(), true);
			}

			config.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var zeroToleranceForUSD = new ExchangeRateTolerance
			{
				Currency = Core.Constants.CurrencyCodes.UnitedStates,
				ExchangeRateTolerancePercentage = 0
			};
			config.ExchangeRateToleranceCollection.Add(zeroToleranceForUSD);

			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config))
			{
				AssertPayload(Guid.Empty, false);
				AssertPayload(Guid.Empty, true);
				AssertPayload(deal.AED_GC_Company.ToGuid(), false);
				AssertPayload(deal.AED_GC_Company.ToGuid(), true);
			}

			void AssertPayload(Guid companyPk, bool isRegistryEnabled)
			{
				using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, isRegistryEnabled))
				{
					var ePayment = converter.ConvertDealToGEP(deal);
					var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
					var exchangeRateTolerance = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.GetFallBackValueAtAllLevels(paymentApproval.AV_GC.ToGuid(), Guid.Empty, Guid.Empty).ExchangeRateToleranceCollection.Cast<ExchangeRateTolerance>().FirstOrDefault(setting => setting.Currency == paymentApproval.AV_RX_NKPaymentCurrency);
					var expectedPayload = string.Empty;
					if (isRegistryEnabled && exchangeRateTolerance.ExchangeRateTolerancePercentage > 0)
					{
						expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":380,""maxFundAmount"":420,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";
					}
					else
					{
						expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";
					}
					var message = "Registry is " + (isRegistryEnabled && exchangeRateTolerance.ExchangeRateTolerancePercentage > 0 ? "enabled" : "disabled") + " at " + (companyPk == Guid.Empty ? "system" : "company") + " level, so payload should contain " + (isRegistryEnabled && exchangeRateTolerance.ExchangeRateTolerancePercentage > 0 ? " minFundAmount <= localPayAmount <= maxFundAmount " : " minFundAmount == localPayAmount == maxFundAmount") + ".";
					AssertEquals(message, expectedPayload, payLoad);
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_PayMessageType_OrganisationDoesNotHaveABankAccountWithEPOMethodAndSameCurrency()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.NewZealand;
			accountDetail2.A1_IsDefaultAccount = true;
			Factory.Save();

			GlobalElectronicPayment ePayment;
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":"""",""amount"":1000,""payReason"":""Services trade""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		public void TestConvertDealToGEP_PayMessageType_OrganisationHasABankAccountWithEPOMethodAndSameCurrencyButNotLinkedToBeneficiary()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				quote.QU_Status = QuoteStatusCodes.Accepted;
				quote.QU_ProviderReference = "testreference";
				quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quote.QU_FromAmount = 400m;
				quote.QU_ExchangeRate = 2.5m;
				quote.QU_ExchangeRateInverted = 0.4m;
				quote.QU_FeeAmount = 10m;
				quote.QU_RX_NKFeeCurrency = quote.QU_RX_NKToCurrency;
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			Factory.Save();

			Assert(accountDetail2.A1_EPaymentBeneficiaryId.IsEmpty);

			GlobalElectronicPayment ePayment;
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":"""",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		public void TestConvertDealToGEP_PayMessageType_OrganisationHasABankAccountWithEPOMethodAndSameCurrencyAndLinkedToBeneficiaryButBeneficiaryProviderReferenceIsEmpty()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			Assert(!accountDetail2.A1_EPaymentBeneficiaryId.IsEmpty);
			Assert(accountDetail2.EPaymentBeneficiary.ABF_ProviderReference.IsEmpty);

			GlobalElectronicPayment ePayment;
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""" + beneficiary.PK.ToString() + @""",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		public void TestConvertDealToGEP_PayMessageType_OrganisationHasABankAccountWithEPOMethodAndSameCurrencyAndLinkedToBeneficiaryAndBeneficiaryProviderReferenceIsNotEmpty()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			Assert(!accountDetail2.A1_EPaymentBeneficiaryId.IsEmpty);
			Assert(!accountDetail2.EPaymentBeneficiary.ABF_ProviderReference.IsEmpty);

			GlobalElectronicPayment ePayment;
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_ThrowsGEPMessageCreationException_WhenBankAccountTypeIsNotEPA()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			ofxBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;

			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				AssertExceptionThrown<GEPMessageCreationException>(
					"The bank account must be EPA",
					"The bank account type is expected to be 'EPA' but was 'BNK'.",
					() => converter.ConvertDealToGEP(deal));
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_ThrowsGEPMessageCreationException_WhenAuthorizedUserDoesNotExist()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.Empty, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised);
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				AssertExceptionThrown<GEPMessageCreationException>(
					"Only authorized users can create deals",
					"No authorized staff token found for code 'YOU'.",
					() => converter.ConvertDealToGEP(deal));
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_ThrowsGEPMessageCreationException_WhenTokenExpiryIsLessThanOneHour()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var authExpirationUtc = ZDateTime.UtcNow.AddMinutes(20);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, authExpirationUtc, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				AssertExceptionThrown<GEPMessageCreationException>(
					"Only authorized users can create deals",
					$"The users authorization is either expired or due to expire within an hour. Expiry Date (UTC): {authExpirationUtc.ToBestReadableDateTimeString()}.",
					() => converter.ConvertDealToGEP(deal));
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_ThrowsGEPMessageCreationException_WhenTokenAlreadyExpired()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var authExpirationUtc = ZDateTime.UtcNow.AddMinutes(-1);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, authExpirationUtc, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				AssertExceptionThrown<GEPMessageCreationException>(
					"Only authorized users can create deals",
					$"The users authorization is either expired or due to expire within an hour. Expiry Date (UTC): {authExpirationUtc.ToBestReadableDateTimeString()}.",
					() => converter.ConvertDealToGEP(deal));
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_PayMessageType_ExchangeToleranceRegistryIsEmpty()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			GlobalElectronicPayment ePayment;
			var config = new ExchangeRateToleranceConfiguration();
			config.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config))
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", "OFX", ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_PayMessageType_ExchangeToleranceRegistrySetupForTwoCompanies()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var company2 = objectCreator.CreateCompanyAndBranch("NZAKL");
			company2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount1 = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var ofxBankAccount2 = testHelper.CreateOFXPaymentProviderBankAccount(company2.PK);
			ofxBankAccount2.AB_Code = "EP2";
			var staffToken1 = testHelper.CreateStaffToken(ofxBankAccount1.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken1.TK_AccountName = "This name is provided by OFX.";
			var staffToken2 = testHelper.CreateStaffToken(ofxBankAccount2.PK, ZDateTime.UtcNow.AddHours(2), company2.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken2.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote1;
			EPaymentDeal deal1;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote1 = testHelper.CreateQuote(company1, ofxBankAccount1);
				FillQuote(quote1);
				Factory.Save();

				deal1 = testHelper.CreateDeal(quote1);

				var paymentApproval1 = Factory.Load<PaymentApprovalBase>(quote1.QU_AV);
				paymentApproval1.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				var beneficiary1 = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
				var accountDetails1 = paymentApproval1.PayeeOrganisation.CompanyData.AccountDetailsCollection;
				accountDetails1.RemoveAndDeleteAll();
				var accountDetail1 = accountDetails1.AddNew();
				accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				accountDetail1.A1_IsDefaultAccount = true;
				accountDetail1.A1_EPaymentBeneficiaryId = beneficiary1.PK;

				Factory.Save();
			}

			EPaymentQuote quote2;
			EPaymentDeal deal2;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote2 = testHelper.CreateQuote(company2, ofxBankAccount2);
				FillQuote(quote2);
				Factory.Save();

				deal2 = testHelper.CreateDeal(quote2);

				var paymentApproval2 = Factory.Load<PaymentApprovalBase>(quote2.QU_AV);
				paymentApproval2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				var beneficiary2 = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
				var accountDetails2 = paymentApproval2.PayeeOrganisation.CompanyData.AccountDetailsCollection;
				accountDetails2.RemoveAndDeleteAll();
				var accountDetail2 = accountDetails2.AddNew();
				accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				accountDetail2.A1_IsDefaultAccount = true;
				accountDetail2.A1_EPaymentBeneficiaryId = beneficiary2.PK;

				Factory.Save();
			}

			var configForCompany1 = new ExchangeRateToleranceConfiguration();
			configForCompany1.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForUSDCompany1 = new ExchangeRateTolerance
			{
				Currency = "USD",
				ExchangeRateTolerancePercentage = 5
			};
			configForCompany1.ExchangeRateToleranceCollection.Add(toleranceForUSDCompany1);

			var configForCompany2 = new ExchangeRateToleranceConfiguration();
			configForCompany2.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForUSDCompany2 = new ExchangeRateTolerance
			{
				Currency = "USD",
				ExchangeRateTolerancePercentage = 10
			};
			configForCompany2.ExchangeRateToleranceCollection.Add(toleranceForUSDCompany2);

			GlobalElectronicPayment ePayment1;
			GlobalElectronicPayment ePayment2;
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCompany1))
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCompany2))
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					ePayment1 = converter.ConvertDealToGEP(deal1);
				}
				using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					ePayment2 = converter.ConvertDealToGEP(deal2);
				}
			}

			AssertEquals("MessagingSystem", "OFX", ePayment1.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment1.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment1.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment1.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment1.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment1.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment1.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment1.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload1 = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":380,""maxFundAmount"":420,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad1 = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment1.Payload));
			AssertEquals("JSON payload", expectedPayload1, payLoad1);

			AssertEquals("MessagingSystem", "OFX", ePayment2.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment2.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company2.GC_Code, ePayment2.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company2.FirstActiveBranch.GB_Code, ePayment2.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment2.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment2.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment2.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment2.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload2 = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EP2"",""minFundAmount"":360,""maxFundAmount"":440,""fundCurrency"":""NZD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad2 = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment2.Payload));
			AssertEquals("JSON payload", expectedPayload2, payLoad2);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_PayMessageType_EnableFundingEPaymentDealsfromForeignCurrencyBankAccounts()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company = objectCreator.CreateCompanyAndBranch("AUMEL");
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;

			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company.PK);
			var staffToekn = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToekn.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();

			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var accBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				accBankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.China;
				Factory.Save();

				var aPPaymentBatchPoster = Factory.NewWithValidTestData<APPaymentBatchPoster>();
				aPPaymentBatchPoster.APB_AB_FundingBankAccount = accBankAccount.PK;
				Factory.Save();

				paymentApproval.InitializeForPaymentBatch(() => false);
				paymentApproval.AV_AB = ofxBankAccount.PK;
				paymentApproval.AV_AK = testHelper.ObjectCreator.USDChequeBook.PK;
				paymentApproval.AV_PaymentType = ReceiptTypes.EPayment;
				paymentApproval.AV_RX_NKPaymentCurrency = "USD";
				paymentApproval.AV_PayExRate = 2.5m;
				paymentApproval.AV_Amount = 1000m;
				paymentApproval.AV_GB = company.FirstActiveBranch.PK;
				paymentApproval.AV_GC = company.PK;
				paymentApproval.AV_APB_PaymentBatch = aPPaymentBatchPoster.PK;
				Factory.Save();

				quote = paymentApproval.TryToCreateQuote(ofxBankAccount.AB_PaymentProvider.ToString(), false).Quote;
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail = accountDetails.AddNew();
			accountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail.A1_IsDefaultAccount = true;
			accountDetail.A1_EPaymentBeneficiaryId = beneficiary.PK;

			Factory.Save();

			GlobalElectronicPayment ePayment;
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", "OFX", ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""CNY"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_PayMessageType_EnableFundingEPaymentDealsfromForeignCurrencyBankAccounts_BatchPosterIsNull()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company = objectCreator.CreateCompanyAndBranch("AUMEL");
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;

			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company.PK);
			var staffToekn = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToekn.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company, ofxBankAccount);
				FillQuote(quote);
				Factory.Save();

				deal = testHelper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
			var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail = accountDetails.AddNew();
			accountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail.A1_IsDefaultAccount = true;
			accountDetail.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			GlobalElectronicPayment ePayment;
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var converter = new AccEPaymentDealToGEPConverter();
				ePayment = converter.ConvertDealToGEP(deal);
			}

			AssertEquals("MessagingSystem", "OFX", ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", true, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", true, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", true, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var payLoad = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("JSON payload", expectedPayload, payLoad);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertDealToGEP_FromSinglePayment()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var helper = new EPaymentTestHelper(testObjectCreator);
			var converter = new AccEPaymentDealToGEPConverter();
			var ofxBankAccount = testObjectCreator.CreateEPaymentBankAccount();
			var quoteCreatingUser = testObjectCreator.CreateStaff("OFX");
			var company = testObjectCreator.CreateCompanyAndBranch("AUMEL");
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var staffToken = helper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";

			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = helper.CreateQuote(company, ofxBankAccount);
				FillQuote(quote);
				deal = helper.CreateDeal(quote);
				Factory.Save();
			}

			var paymentApproval = quote.PaymentApproval;
			AssertEquals(ZGuid.Empty, paymentApproval.AV_APB_PaymentBatch);

			var ePayment = converter.ConvertDealToGEP(deal);
			var payload = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertContains("\"fundCurrency\":\"AUD\"", payload);

			paymentApproval.AV_AB_FundingBankAccount = testObjectCreator.USDBankAccount.PK;
			ePayment = converter.ConvertDealToGEP(deal);
			payload = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertContains("\"fundCurrency\":\"USD\"", payload);
		}

		void FillQuote(EPaymentQuote quote)
		{
			quote.QU_Status = QuoteStatusCodes.Accepted;
			quote.QU_ProviderReference = "testreference";
			quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
			quote.QU_FromAmount = 400m;
			quote.QU_ExchangeRate = 2.5m;
			quote.QU_ExchangeRateInverted = 0.4m;
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = quote.QU_RX_NKToCurrency;
		}
	}
}
