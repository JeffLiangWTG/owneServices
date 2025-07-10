using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.AccountingIServices
{
	internal class NumberFountainTransactionDataProvider : BizoDataRowRelatedValues<object>
	{
		public static void Initialize(TransactionHeader transaction)
		{
			var saveCount = GetDataRowRelatedValue(transaction, Flags.SaveCount);
			var shouldInitialize = saveCount == null || (int)saveCount != transaction.Factory.SaveCount;
			if (shouldInitialize)
			{
				SetDataRowRelatedValue(transaction, Flags.SaveCount, transaction.Factory.SaveCount);
				SetInvalidStateFlagMessage(transaction, null);
				GetAllDataProviders().ForEach(x => x.ResetData(transaction));
			}
		}

		public static (bool IsCorrect, string ErrorMessage) CheckDataIsCorrect(TransactionHeader transaction)
		{
			foreach (var dataProvider in GetAllDataProviders())
			{
				var result = dataProvider.CheckDataIsCorrect(transaction);
				if (!result.IsCorrect)
				{
					return result;
				}
			}

			return (true, "");
		}

		public static bool GetIsTaxReported(TransactionHeader transaction) => GetIsTaxReportedDataProvider().GetData(transaction);

		public static bool GetIsSelfBilling(TransactionHeader transaction) => GetIsSelfBillingDataProvider().GetData(transaction);

		public static bool GetIsCorrected(TransactionHeader transaction) => GetIsCorrectedDataProvider().GetData(transaction);

		#region Implementation

		static TransactionDataProvider<bool>[] GetAllDataProviders() => new[]
		{
			GetIsTaxReportedDataProvider(),
			GetIsSelfBillingDataProvider(),
			GetIsCorrectedDataProvider()
		};

		static TransactionDataProvider<bool> GetIsTaxReportedDataProvider() => new TransactionDataProvider<bool>(Flags.TaxReported, tran => tran.IsTaxReportable);

		static TransactionDataProvider<bool> GetIsSelfBillingDataProvider() => new TransactionDataProvider<bool>(Flags.SelfBilling, tran => tran.IsSelfBillingInvoice);

		static TransactionDataProvider<bool> GetIsCorrectedDataProvider() => new TransactionDataProvider<bool>(Flags.Corrected, tran => tran.IsAmendingOrReversal);

		static void SetDataRowRelatedValue(BusinessObject bizoForDataRow, Flags flagName, object flagValue) => BizoDataRowRelatedValuesAccessor<NumberFountainTransactionDataProvider>.SetDataRowRelatedValue(bizoForDataRow, flagName.ToString(), flagValue);

		static object GetDataRowRelatedValue(BusinessObject bizoForDataRow, Flags flagName) => BizoDataRowRelatedValuesAccessor<NumberFountainTransactionDataProvider>.GetDataRowRelatedValue(bizoForDataRow, flagName.ToString());

		static void SetInvalidStateFlagMessage(BusinessObject bizo, string errorMessage) => SetDataRowRelatedValue(bizo, Flags.InvalidState, errorMessage);

		static string GetInvalidStateFlagMessage(BusinessObject bizo) => (string)GetDataRowRelatedValue(bizo, Flags.InvalidState);

		class TransactionDataProvider<T> where T : struct
		{
			internal TransactionDataProvider(Flags dataType, Func<TransactionHeader, T> getTransactionData)
			{
				this.getTransactionData = getTransactionData;
				this.dataType = dataType;
			}

			public T GetData(TransactionHeader transaction)
			{
				var currentValue = getTransactionData(transaction);
				var currentDataState = CheckDataIsCorrect(transaction, currentValue);
				var result = currentDataState.OriginalValue.GetValueOrDefault();
				var invalidStateErrorMessage = GetInvalidStateFlagMessage(transaction);
				if (invalidStateErrorMessage == null)
				{
					if (currentDataState.IsCorrect)
					{
						if (!currentDataState.OriginalValue.HasValue)
						{
							SetDataRowRelatedValue(transaction, dataType, (T?)currentValue);
							result = currentValue;
						}
					}
					else
					{
						SetInvalidStateFlagMessage(transaction, GetErrorMessage(currentDataState.OriginalValue, currentValue) + (NoResString)" It was an attempt to retrieve different value of the same data type for the same transaction in the same saving session.");
					}
				}

				return result;
			}

			public (bool IsCorrect, string ErrorMessage) CheckDataIsCorrect(TransactionHeader transaction)
			{
				var currentValue = getTransactionData(transaction);
				var (isCorrect, originalValue) = CheckDataIsCorrect(transaction, currentValue);
				var invalidStateErrorMessage = GetInvalidStateFlagMessage(transaction);
				if (invalidStateErrorMessage != null)
				{
					return (false, invalidStateErrorMessage);
				}
				else if (!isCorrect)
				{
					return (false, GetErrorMessage(originalValue, currentValue));
				}

				return (true, "");
			}

			string GetErrorMessage(T? originalValue, T currentValue) => Invariant($"{dataType}: original value: {originalValue}, current value: {currentValue}.");

			(bool IsCorrect, T? OriginalValue) CheckDataIsCorrect(TransactionHeader transaction, T currentValue)
			{
				var result = (T?)GetDataRowRelatedValue(transaction, dataType);
				if (result.HasValue)
				{
					var originalValue = result.Value;
					return (originalValue.Equals(currentValue), originalValue);
				}

				return (true, null);
			}

			public void ResetData(TransactionHeader transaction)
			{
				var result = (T?)GetDataRowRelatedValue(transaction, dataType);
				if (result.HasValue)
				{
					SetDataRowRelatedValue(transaction, dataType, null);
				}
			}

			readonly Func<TransactionHeader, T> getTransactionData;
			readonly Flags dataType;
		}

		enum Flags
		{
			InvalidState,
			TaxReported,
			SelfBilling,
			Corrected,
			SaveCount
		}

		#endregion
	}
}
