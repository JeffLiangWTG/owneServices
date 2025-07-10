using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common.JSON.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	class AccEPaymentBeneficiaryRequestToGEPConverter : AccEPaymentToGEPConverter
	{
		public GlobalElectronicPayment.GlobalElectronicPayment ConvertBeneficiaryRequestToGEP(AccEPaymentBeneficiaryRequest beneficiaryRequest)
		{
			var bankAccount = FindBankAccountForBeneficiaryRequest(beneficiaryRequest);
			var (messageType, userDetails) = GetMessageTypeAndUserDetails(beneficiaryRequest, bankAccount);
			if (!userDetails.AreAllFieldsPopulated && beneficiaryRequest.ABR_ProviderCode == ProviderCodes.OFX)
			{
				throw new GEPMessageCreationException(Res.GetString("ad388dfb-945a-4bae-b3b4-7659fd3a1810", "OFX User account information is missing."));
			}
			var latestBeneficiaryRequestWithRCVStatus = FindLatestBeneficiaryRequestWithReceivedStatus(beneficiaryRequest);

			var ePayment = CreateGlobalElectronicPayment(beneficiaryRequest.Company.GC_Code, beneficiaryRequest.Company.FirstActiveBranch.GB_Code, beneficiaryRequest.ABR_ProviderCode, messageType, userDetails);
			SetBeneficiaryRequestInfo(ePayment, beneficiaryRequest, bankAccount, latestBeneficiaryRequestWithRCVStatus);
			return ePayment;
		}

		AccEPaymentBeneficiaryRequest FindLatestBeneficiaryRequestWithReceivedStatus(AccEPaymentBeneficiaryRequest beneficiaryRequest)
		{
			var query = new ZQuery(AccEPaymentBeneficiaryRequestSchema.ABR_GC_Company, beneficiaryRequest.ABR_GC_Company);
			query.AddToFilter(AccEPaymentBeneficiaryRequestSchema.ABR_Status, EPaymentStatusCodes.BeneficiaryRequest.Received);
			query.AddToFilter(AccEPaymentBeneficiaryRequestSchema.ABR_ProviderCode, beneficiaryRequest.ABR_ProviderCode);
			query.AddToFilter(AccEPaymentBeneficiaryRequestSchema.PK, SQLComparisonOperator.NotEqual, beneficiaryRequest.PK);
			query.OrderBy = AccEPaymentBeneficiaryRequestSchema.ABR_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			return new BusinessObjectFactory().LoadTop1<AccEPaymentBeneficiaryRequest>(query);
		}

		(ZString messageType, EPaymentUserDetails userDetails) GetMessageTypeAndUserDetails(AccEPaymentBeneficiaryRequest beneficiaryRequest, AccBankAccount bankAccount)
		{
			var messageType = GEPProviderAPICommandList.Codes.SearchBeneficiary;
			return (messageType, CheckUserAuthorisation(beneficiaryRequest, bankAccount));
		}

		EPaymentUserDetails CheckUserAuthorisation(AccEPaymentBeneficiaryRequest beneficiaryRequest, AccBankAccount bankAccount)
		{
			return CheckUserAuthorisationCore(bankAccount, beneficiaryRequest.ABR_SystemCreateUser, () => beneficiaryRequest.CreatingUser).EPaymentUserDetails;
		}

		AccBankAccount FindBankAccountForBeneficiaryRequest(AccEPaymentBeneficiaryRequest beneficiaryRequest)
		{
			AccBankAccount bankAccount = null;
			var staffTokenQuery = new ZQuery(AccEPaymentStaffTokenSchema.TK_GS_NKStaffCode, beneficiaryRequest.ABR_SystemCreateUser);
			staffTokenQuery.AddToFilter(new ZQuery(AccEPaymentStaffTokenSchema.TK_GC, beneficiaryRequest.ABR_GC_Company));
			var matchingStaffTokens = beneficiaryRequest.Factory.Load<AccEPaymentStaffToken>(staffTokenQuery);
			if (matchingStaffTokens.Any())
			{
				var staffTokenForPaymentProvider = matchingStaffTokens.Cast<AccEPaymentStaffToken>().FirstOrDefault(t => t.BankAccount.AB_PaymentProvider == ProviderCodes.OFX);
				bankAccount = staffTokenForPaymentProvider?.BankAccount;
			}
			return bankAccount;
		}

		void SetBeneficiaryRequestInfo(GlobalElectronicPayment.GlobalElectronicPayment ePayment, AccEPaymentBeneficiaryRequest beneficiaryRequest, AccBankAccount bankAccount, AccEPaymentBeneficiaryRequest latestBeneficiaryRequestWithRCVStatus)
		{
			var beneficiaryRequestInfo = new BeneficiarySearchRequest()
			{
				RequestReference = beneficiaryRequest.ABR_InternalReference,
				BankAccountCode = bankAccount?.AB_Code ?? ZString.Empty,
				UpdatedDateFrom = latestBeneficiaryRequestWithRCVStatus?.ABR_SystemCreateTimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? ZString.Empty,
				StartPageNumber = StartPageNumber,
				MaxNumberOfRecordInHttpResponse = AccountingMasterFilesRegistry.Instance.BeneficiarySearchResultHttpResponsePageSize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty),
				MaxNumberOfRecordInXUE = AccountingMasterFilesRegistry.Instance.MaximumNumberOfBeneficiaryInSearchResultXUE.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty),
			};

			var jsonPayload = beneficiaryRequestInfo.ToJSON();
			var notifications = new Logger();

			var schemaResourceName = "Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment.BeneficiaryRequest.BeneficiarySearchRequestSchema.json";
			var schema = LoadJsonSchema(schemaResourceName);
			if (schema != null)
			{
				schema.ValidateJSON(jsonPayload, notifications);
				if (notifications.HasErrors)
				{
					throw new GEPMessageCreationException(notifications.ToString().Trim());
				}
			}
			else
			{
				throw new GEPMessageCreationException(Res.GetString("eb02bf8f-d460-4ddc-b614-1dfd44cb2396", "{0} schema could not be found", schemaResourceName));
			}

			ePayment.Payload = Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(jsonPayload));
		}

		internal int StartPageNumber { get; set; } = 1;
	}
}
