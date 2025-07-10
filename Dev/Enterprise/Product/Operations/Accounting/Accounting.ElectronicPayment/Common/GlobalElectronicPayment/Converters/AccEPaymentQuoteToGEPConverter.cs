using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	public class AccEPaymentQuoteToGEPConverter : AccEPaymentToGEPConverter
	{
		public GlobalElectronicPayment.GlobalElectronicPayment ConvertQuoteToGEP(AccEPaymentQuote quote)
		{
			var (messageType, userDetails) = GetMessageTypeAndUserDetails(quote);
			var ePayment = CreateGlobalElectronicPayment(quote.Company.GC_Code, quote.PaymentApproval.Branch.GB_Code, quote.QU_ProviderCode, messageType, userDetails);
			SetUniversalTransaction(ePayment, quote);
			return ePayment;
		}

		void SetUniversalTransaction(GlobalElectronicPayment.GlobalElectronicPayment ePayment, AccEPaymentQuote quote)
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)Db.Connection).ADOConnection, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
			var universalTransaction = new AccEPaymentQuoteExporter().CreateUniversalTransaction(dataAccess, quote);
			ePayment.Payload = System.Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(universalTransaction.ToXmlFragment()));
		}

		(ZString messageType, EPaymentUserDetails userDetails) GetMessageTypeAndUserDetails(AccEPaymentQuote quote)
		{
			return ShouldGetRates(quote)
				? (GEPProviderAPICommandList.Codes.GetRates, EPaymentUserDetails.Empty)
				: (GEPProviderAPICommandList.Codes.GetAQuote, CheckUserAuthorisation(quote));
		}

		EPaymentUserDetails CheckUserAuthorisation(AccEPaymentQuote quote)
		{
			var userAuthorisationResult = CheckUserAuthorisationCore(quote.PaymentApproval.BankAccount, quote.QU_SystemCreateUser, () => GetCreatingUser(quote));

			return userAuthorisationResult.IsValid
				? userAuthorisationResult.EPaymentUserDetails
				: throw CreateGEPMessageCreationException(userAuthorisationResult.ErrorMessage);
		}

		static bool ShouldGetRates(AccEPaymentQuote quote)
		{
			var ePaymentAccount = quote.PaymentApproval.BankAccount;

			return ePaymentAccount != null && ePaymentAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA;
		}

		static GEPMessageCreationException CreateGEPMessageCreationException(string errorMessage)
		{
			return new GEPMessageCreationException(errorMessage)
			{
				UserFriendlyMessage = Res.GetString("75175e3b-11c9-41f1-b813-8c3526572d40", "Error generating FX Quote Request. Please try again.")
			};
		}
	}
}
