using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface ITransactionInfoHelper
	{
		string GetRegistrationCode(OrganizationAddress organizationAddress, ZString countryOfIssueCode, ZString registrationNumberTypeCode);
		string GetStateDescriptionByCountry(string countryCode, string stateCode, BusinessObjectFactory factory);
		long GetRandomLog(int seed, long min, long max);
		int GetOSCurrencyDecimals(BusinessObjectFactory factory, TransactionInfo transaction);

		/// <summary>
		/// Gets a unique identifier for a transaction within an AccEInvoicingBatch, suitable for use as a dictionary key.
		/// Note this DOES NOT match the AccTransactionHeader unique index; ONLY use this for transactions within a single AccEInvoicingBatch.
		/// </summary>
		string GetUniqueIdentifierWithinBatch(TransactionInfo transaction);

		/// <summary>
		/// Gets a unique identifier for a transaction within an AccEInvoicingBatch, suitable for use as a dictionary key.
		/// Note this DOES NOT match the AccTransactionHeader unique index; ONLY use this for transactions within a single AccEInvoicingBatch.
		/// </summary>
		string GetUniqueIdentifierWithinBatch(InvoicingBase transaction);

		string GetUniqueIdentifierWithinBatch(UniversalEventTransactionDataObject transaction);

		string GetUniqueIdentifierWithinBatch(string ledger, string transactionType, string orgCode, string transactionNumber);

		string GetGovernmentNumber(List<AuthorizationDetails> authorizationDetails);
	}

	class TransactionInfoHelper : ITransactionInfoHelper
	{
		string ITransactionInfoHelper.GetRegistrationCode(OrganizationAddress organizationAddress, ZString countryOfIssueCode, ZString registrationNumberTypeCode)
		{
			var returnedValue = organizationAddress?.RegistrationNumberCollection?.Where(number =>
								(number.Type?.Code ?? ZString.Empty) == registrationNumberTypeCode
								&& (number.CountryOfIssue?.Code ?? ZString.Empty) == countryOfIssueCode);

			return returnedValue?.FirstOrDefault()?.Value;
		}

		string ITransactionInfoHelper.GetStateDescriptionByCountry(string countryCode, string stateCode, BusinessObjectFactory factory)
		{
			return new RefCountryStates.Loader(factory).LoadRefCountryStatesFromCode(stateCode, countryCode)?.RW_Description ?? string.Empty;
		}

		long ITransactionInfoHelper.GetRandomLog(int seed, long min, long max)
		{
			return new Random(seed).NextLong(min, max);
		}

		int ITransactionInfoHelper.GetOSCurrencyDecimals(BusinessObjectFactory factory, TransactionInfo transaction)
		{
			var osCurrencyCode = transaction.OSCurrency?.Code;

			if (osCurrencyCode.HasValue)
			{
				var currency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, osCurrencyCode.Value);
				return currency?.Decimals ?? 0;
			}

			return 0;
		}

		string ITransactionInfoHelper.GetUniqueIdentifierWithinBatch(TransactionInfo transaction)
			=> transaction == null ? string.Empty
			: TransactionUniqueKey(transaction.Ledger, transaction.TransactionType.ToString(), transaction.OrganizationAddress?.OrganizationCode, transaction.Number);

		string ITransactionInfoHelper.GetUniqueIdentifierWithinBatch(InvoicingBase transaction)
			=> transaction == null ? string.Empty
			: TransactionUniqueKey(transaction.AH_Ledger, transaction.AH_TransactionType, transaction.Header?.OH_Code, transaction.AH_TransactionNum);

		string ITransactionInfoHelper.GetUniqueIdentifierWithinBatch(UniversalEventTransactionDataObject transaction)
			=> transaction == null ? string.Empty
			: TransactionUniqueKey(transaction.TransactionLedger, transaction.TransactionType, transaction.TransactionOrgHeaderCode, transaction.TransactionNumber);

		string ITransactionInfoHelper.GetUniqueIdentifierWithinBatch(string ledger, string transactionType, string orgCode, string transactionNumber)
			=> TransactionUniqueKey(ledger, transactionType, orgCode, transactionNumber);

		static string TransactionUniqueKey(string ledger, string transactionType, string orgCode, string transactionNumber)
			=> FormattableString.Invariant($"{ledger}:{transactionType}:{orgCode}:{transactionNumber}");

		string ITransactionInfoHelper.GetGovernmentNumber(List<AuthorizationDetails> authorizationDetails)
			=> authorizationDetails?.FirstOrDefault(x => x.Purpose?.Code.ToString() ==
			DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode)?.GovernmentNumber ?? string.Empty;
	}
}
