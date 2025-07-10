using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json.Schema;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	public abstract class AccEPaymentToGEPConverter
	{
		protected GlobalElectronicPayment.GlobalElectronicPayment CreateGlobalElectronicPayment(ZString companyCode, ZString branchCode, ZString providerCode, ZString messageType, EPaymentUserDetails userDetails)
		{
			var globalElectronicPayment = new GlobalElectronicPayment.GlobalElectronicPayment();
			globalElectronicPayment.Header = new GlobalElectronicPaymentHeader();
			globalElectronicPayment.Header.ElectronicPaymentRequest = new GlobalElectronicPaymentHeaderElectronicPaymentRequest();
			globalElectronicPayment.Header.ElectronicPaymentRequest.MessagingSystem = providerCode;
			globalElectronicPayment.Header.ElectronicPaymentRequest.MessageType = messageType;
			globalElectronicPayment.Header.ElectronicPaymentRequest.CompanyCode = companyCode;
			globalElectronicPayment.Header.ElectronicPaymentRequest.BranchCode = branchCode;
			if (userDetails.AreAllFieldsPopulated)
			{
				globalElectronicPayment.Header.ElectronicPaymentRequest.UserPk = userDetails.UserPk.ToString();
				globalElectronicPayment.Header.ElectronicPaymentRequest.UserCode = userDetails.UserCode.ToString();
				globalElectronicPayment.Header.ElectronicPaymentRequest.UserAccountName = userDetails.UserAccountName.ToString();
			}
			globalElectronicPayment.Header.ElectronicPaymentRequest.IsProductionSystem = Env.Instance.IsProductionSystem;
			return globalElectronicPayment;
		}

		protected EPaymentUserCheckResult CheckUserAuthorisationCore(AccBankAccount bankAccount, ZString creatingUserCode, Func<GlbStaff> getCreatingUser)
		{
			AccEPaymentStaffToken matchingAuthorizedUser = null;

			var errorBuilder = new StringBuilder();
			if (bankAccount == null)
			{
				errorBuilder.AppendLine((NoResString)"The bank account is missing.");
			}
			else
			{
				if (bankAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA)
				{
					errorBuilder.AppendLine($"The bank account type is expected to be '{AccountTypeCodeDescriptionPairList.Codes.EPA}' but was '{bankAccount.AB_AccountType}'.");
				}

				matchingAuthorizedUser = bankAccount.EPaymentStaffTokenCollection.Cast<AccEPaymentStaffToken>()
					.FirstOrDefault(x =>
						x.TK_GS_NKStaffCode == creatingUserCode &&
						x.TK_Status == AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
				if (matchingAuthorizedUser == null)
				{
					errorBuilder.AppendLine($"No authorized staff token found for code '{creatingUserCode}'.");
				}
				else
				{
					if (!matchingAuthorizedUser.TK_ExpiryUtc.IsInTheFutureUtc(new TimeSpan(1, 0, 0).TotalMilliseconds))
					{
						errorBuilder.AppendLine(
							$"The users authorization is either expired or due to expire within an hour. Expiry Date (UTC): {matchingAuthorizedUser.TK_ExpiryUtc.ToBestReadableDateTimeString()}.");
					}
					if (matchingAuthorizedUser.TK_AccountName.IsEmpty)
					{
						errorBuilder.AppendLine((NoResString)"The staff token is missing an account name.");
					}
				}
			}

			var creatingUser = getCreatingUser.Invoke();
			if (creatingUser == null)
			{
				errorBuilder.AppendLine((NoResString)"The user who created the request could not be found.");
			}

			return errorBuilder.Length > 0
				? new EPaymentUserCheckResult(errorBuilder.ToString().Trim())
				: new EPaymentUserCheckResult(creatingUser.PK, matchingAuthorizedUser.TK_GS_NKStaffCode, matchingAuthorizedUser.TK_AccountName);
		}

		protected virtual GlbStaff GetCreatingUser(AccEPaymentQuote quote) => quote.CreatingUser;

		protected GlbStaff GetCreatingUser(AccEPaymentDeal deal) => deal.CreatingUser;

		protected JSchema LoadJsonSchema(ZString schemaResourceName)
		{
			var schemaText = string.Empty;
			var assembly = Assembly.GetExecutingAssembly();
			var resourceStream = assembly.GetManifestResourceStream(schemaResourceName);
			if (resourceStream != null)
			{
				using (var reader = new StreamReader(resourceStream))
				{
					schemaText = reader.ReadToEnd();
				}
				var schema = JSchema.Parse(schemaText);
				return schema;
			}
			return null;
		}

		protected class EPaymentUserCheckResult
		{
			public EPaymentUserCheckResult(ZGuid userPk, ZString userCode, ZString userAccountName)
			{
				IsValid = true;
				EPaymentUserDetails = new EPaymentUserDetails(userPk, userCode, userAccountName);
			}

			public EPaymentUserCheckResult(string reason)
			{
				ErrorMessage = reason;
			}

			public bool IsValid { get; private set; }
			public string ErrorMessage { get; private set; } = string.Empty;
			public EPaymentUserDetails EPaymentUserDetails { get; private set; } = EPaymentUserDetails.Empty;
		}

		protected class EPaymentUserDetails
		{
			internal ZGuid UserPk { get; }
			internal ZString UserCode { get; }
			internal ZString UserAccountName { get; }

			public EPaymentUserDetails(ZGuid userPk, ZString userCode, ZString userAccountName)
			{
				UserPk = userPk;
				UserCode = userCode;
				UserAccountName = userAccountName;
			}

			internal bool AreAllFieldsPopulated => !UserPk.IsEmpty && !UserCode.IsEmpty && !UserAccountName.IsEmpty;

			internal static EPaymentUserDetails Empty => new EPaymentUserDetails(ZGuid.Empty, ZString.Empty, ZString.Empty);
		}
	}
}
