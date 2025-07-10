using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	class AccEPaymentDealToGEPConverter : AccEPaymentToGEPConverter
	{
		public GlobalElectronicPayment.GlobalElectronicPayment ConvertDealToGEP(AccEPaymentDeal deal)
		{
			var (messageType, userDetails) = GetMessageTypeAndUserDetails(deal);
			var ePayment = CreateGlobalElectronicPayment(deal.Company.GC_Code, deal.Quote.PaymentApproval.Branch.GB_Code, deal.AED_ProviderCode, messageType, userDetails);
			SetPaymentRequest(ePayment, deal);
			return ePayment;
		}

		void SetPaymentRequest(GlobalElectronicPayment.GlobalElectronicPayment ePayment, AccEPaymentDeal deal)
		{
			var quote = deal.Quote;
			var paymentApprovalPk = quote.PaymentApproval.PK;
			var paymentApproval = deal.Factory.Load<PaymentApprovalBase>(paymentApprovalPk);

			var providerReference = quote.QU_ProviderReference;
			var bankAccountCode = paymentApproval.BankAccount.AB_Code;
			var toCurrency = quote.QU_RX_NKToCurrency;
			var toAmount = quote.QU_ToAmount;
			var fromCurrency = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval).GetFundingCurrency();
			var apAccountDetails = GetAPAccountDetails(paymentApproval);
			var beneficiaryId = FindBeneficiaryProviderReference(apAccountDetails);
			var paymentReference = PaymentRequestPropertyHelper.CreatePaymentReference(paymentApproval, apAccountDetails);
			var payReasonDescription = EPaymentPropertyHelper.GetPaymentReasonDescriptionForProvider(paymentApproval.AV_GC.ToGuid(), deal.AED_ProviderCode, paymentApproval.AV_EPaymentReasonCode);

			var paymentRequest = new PaymentRequest();
			paymentRequest.DealInternalReference = deal.AED_InternalReference;
			paymentRequest.QuoteIdUsedForPayment = providerReference;
			paymentRequest.BankAccountCode = bankAccountCode;
			paymentRequest.FundCurrency = fromCurrency;
			paymentRequest.PayAmount = toAmount;
			paymentRequest.PayCurrency = toCurrency;

			var paymentLine = new PaymentLine() { PayeeId = beneficiaryId, Amount = toAmount, PayReason = payReasonDescription, PayReference = paymentReference };
			paymentRequest.PaymentItems = new List<PaymentLine>() { paymentLine };

			if (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.GetFallBackValueAtAllLevels(deal.AED_GC_Company.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var (minFromAmount, maxFromAmount) = CalculateTolerance(paymentApproval);
				paymentRequest.MinimumFundAmount = minFromAmount;
				paymentRequest.MaximumFundAmount = maxFromAmount;
			}
			else
			{
				paymentRequest.MinimumFundAmount = paymentApproval.AV_Calc_LocalAmount;
				paymentRequest.MaximumFundAmount = paymentApproval.AV_Calc_LocalAmount;
			}

			var jsonPayload = paymentRequest.ToJSON();
			var notifications = new Logger();

			var schemaResourceName = "Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment.PaymentRequest.PaymentRequestSchema.json";
			var schema = LoadJsonSchema(schemaResourceName);
			schema.ValidateJSON(jsonPayload, notifications);

			if (notifications.HasErrors)
			{
				throw new GEPMessageCreationException(notifications.ToString().Trim(), null);
			}

			ePayment.Payload = Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(jsonPayload));
		}

		ZString FindBeneficiaryProviderReference(AccAPAccountDetails matchingAccountDetail)
		{
			ZString beneficiaryId = ZString.Empty;
			if (matchingAccountDetail?.EPaymentBeneficiary != null)
			{
				var providerReferenceForBeneficiary = matchingAccountDetail.EPaymentBeneficiary.ABF_ProviderReference;
				beneficiaryId = providerReferenceForBeneficiary.IsEmpty ? (ZString)matchingAccountDetail.EPaymentBeneficiary.PK.ToString() : providerReferenceForBeneficiary;
			}
			return beneficiaryId;
		}

		(ZDecimal minAmount, ZDecimal maxAmount) CalculateTolerance(PaymentApprovalBase paymentApproval)
		{
			var localAmount = paymentApproval.AV_Calc_LocalAmount;
			var exchangeRateTolerance = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.GetFallBackValueAtAllLevels(paymentApproval.AV_GC.ToGuid(), Guid.Empty, Guid.Empty).ExchangeRateToleranceCollection.Cast<ExchangeRateTolerance>().FirstOrDefault(setting => setting.Currency == paymentApproval.AV_RX_NKPaymentCurrency);
			var exRateTolerancePercentage = exchangeRateTolerance?.ExchangeRateTolerancePercentage ?? ZDecimal.Zero;

			var maxAmount = localAmount + ((localAmount * exRateTolerancePercentage) / 100);
			var minAmount = localAmount - ((localAmount * exRateTolerancePercentage) / 100);
			return (minAmount, maxAmount);
		}

		(ZString messageType, EPaymentUserDetails userDetails) GetMessageTypeAndUserDetails(AccEPaymentDeal deal)
		{
			var messageType = GEPProviderAPICommandList.Codes.CreateADeal;
			return (messageType, CheckUseAuthorisation(deal));
		}

		EPaymentUserDetails CheckUseAuthorisation(AccEPaymentDeal deal)
		{
			var result = CheckUserAuthorisationCore(deal.Quote.PaymentApproval.BankAccount, deal.Quote.QU_SystemCreateUser, () => GetCreatingUser(deal));

			return result.IsValid
				? result.EPaymentUserDetails
				: throw CreateGEPMessageCreationException(result.ErrorMessage);
		}

		AccAPAccountDetails GetAPAccountDetails(PaymentApprovalBase paymentApproval)
		{
			var collection = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			return collection.GetAccountDetails(EPaymentMethods.EPaymentViaOFX, paymentApproval.AV_RX_NKPaymentCurrency, true);
		}

		static GEPMessageCreationException CreateGEPMessageCreationException(string errorMessage)
		{
			return new GEPMessageCreationException(errorMessage)
			{
				UserFriendlyMessage = Res.GetString("b702bafa-f516-41ff-940b-63bba75fb502", "Error generating FX Deal Request. Please try again.")
			};
		}
	}
}
