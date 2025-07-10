using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegisterProcessor<T> where T : BusinessObject, ISavingProvider<T>
{
	public CusTempStorageRegisterProcessor(ITemporaryStorageRegisterTransactionDataProvider provider, LoggingInformation logger, Action<ZString> notifierWhenInsufficient = null)
	{
		this.provider = Argument.NotNull(provider, nameof(provider));
		this.notifierWhenInsufficient = notifierWhenInsufficient;
		Logger = logger;
	}

	public void CalculateTransactionsAndLockMutexIfNeededForAddingTransaction()
	{
		CalculateTransactionsAndLockMutexIfNeeded(GetTransactionPackageQtyAndGrossWeightForAddingTransaction);
	}

	public void CalculateTransactionsAndLockMutexIfNeededForRollingBackTransaction()
	{
		CalculateTransactionsAndLockMutexIfNeeded(GetTransactionPackageQtyAndGrossWeightForRollingBackTransaction);
	}

	delegate (int, decimal) GetTransactionPackageQtyAndGrossWeightHandler(IList<CusTempStorageRegLineTransaction> matchingTransactions, TemporaryStorageRegisterTransactionData transaction);

	(int TransacitonPackageQty, decimal TransactionGrossWeight) GetTransactionPackageQtyAndGrossWeightForAddingTransaction(IList<CusTempStorageRegLineTransaction> matchingTransactions, TemporaryStorageRegisterTransactionData transaction)
	{
		var alreadyTakenPackageQty = matchingTransactions.Sum(x => x.SRT_PackageQty);
		var transactionPackageQty = -alreadyTakenPackageQty - transaction.PackageQuantity;
		var alreadyTakenGrossWeight = matchingTransactions.Sum(GetSignedGrossWeight);
		var transactionGrossWeight = -alreadyTakenGrossWeight - transaction.GrossMass;
		transactionGrossWeight = Math.Abs(transactionGrossWeight);

		return (transactionPackageQty, transactionGrossWeight);
	}

	(int TransacitonPackageQty, decimal TransactionGrossWeight) GetTransactionPackageQtyAndGrossWeightForRollingBackTransaction(IList<CusTempStorageRegLineTransaction> matchingTransactions, TemporaryStorageRegisterTransactionData transaction)
	{
		var alreadyTakenPackageQty = matchingTransactions.Sum(x => x.SRT_PackageQty);
		var transactionPackageQty = -alreadyTakenPackageQty;
		var alreadyTakenGrossWeight = matchingTransactions.Sum(GetSignedGrossWeight);
		var transactionGrossWeight = -alreadyTakenGrossWeight;
		transactionGrossWeight = Math.Abs(transactionGrossWeight);

		return (transactionPackageQty, transactionGrossWeight);
	}

	void CalculateTransactionsAndLockMutexIfNeeded(GetTransactionPackageQtyAndGrossWeightHandler getTransactionPackageQtyAndGrossWeight)
	{
		transactionData = provider.GetTemporaryStorageRegisterTransactionData().ToList();
		foreach (var grouping in transactionData.GroupBy(x => x.PreviousRegisterHeader))
		{
			var registerHeader = grouping.Key;
			if (registerHeader != null)
			{
				if (!registerHeader.Mutex.HasLock && registerHeader.LockMutex())
				{
					foreach (var transaction in grouping)
					{
						transaction.ErrorMessages.Clear();
						var registerLines = registerHeader.CusTempStorageRegLines;
						var registerLine = registerLines.FirstOrDefault(x => x.SRL_LineNumber == transaction.RegisterLineNo);
						if (registerLine == null)
						{
							registerLine = registerLines.FirstOrDefault(x => x.SRL_LineNumber == 1);
							transaction.RegisterLineNo = 1;
						}
						if (registerLine == null)
						{
							transaction.ErrorMessages.Add(Res.GetString("7D28C297-918C-4C3C-8E47-0DB73059DB9C", "Register {0}: unable to find the Register Line #1 to default to.", registerHeader.SRH_Reference));
						}
						else
						{
							var matchingTransactions = registerLine.CusTempStorageRegLineTransactions
								.Where(x => x.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.Transaction
											&& x.SRT_InternalReferenceNumber == transaction.InternalReferenceNumber
											&& x.SRT_Reference == transaction.CustomsReferenceNumber).Cast<CusTempStorageRegLineTransaction>().ToList();
							var (transactionPackageQty, transactionGrossWeight) = getTransactionPackageQtyAndGrossWeight(matchingTransactions, transaction);

							if (transactionPackageQty == 0 && transactionGrossWeight == 0m)
							{
								transaction.ErrorMessages.Add(Res.GetString("7C5E8699-4C5D-47E3-A31F-43771F7E22D4", "Register {0}, Line Number {1} is already up to date, no transaction added.", registerHeader.SRH_Reference, transaction.RegisterLineNo));
							}
							else
							{
								transaction.PackageQuantity = transactionPackageQty;
								transaction.GrossMass = transactionGrossWeight;
							}
						}
					}
				}
				else
				{
					grouping.ForEach(x => x.ErrorMessages.Add(Res.GetString("B8A76E61-26F6-4AD9-A0F5-B06421DA84F5", "Cannot add the requested weight/package quantity for register {0}; someone else is locking the Register.", registerHeader.SRH_Reference)));
				}
			}
			else
			{
				grouping.ForEach(x => x.ErrorMessages.Add(Res.GetString("9D312639-6BC9-4B61-8DA9-B53F336844E3", "No register found.")));
			}
		}

		transactionData.ForEach(t => t.ErrorMessages.Distinct().ForEach(x => Logger.Log(x)));
	}

	public void AddTransactionsWhenSaving(T parentBizO)
	{
		if (transactionData?.Any() ?? false)
		{
			void Handler(T bizO)
			{
				foreach (var transaction in transactionData)
				{
					if (!(transaction.ErrorMessages.Count > 0))
					{
						transaction.PreviousRegisterHeader?.AddNewRegisterTransaction(Logger, transaction.RegisterLineNo, transaction.CustomsReferenceNumber, transaction.ReferenceType, transaction.InternalReferenceNumber, transaction.InternalReferenceType, transaction.GrossMass, transaction.PackageQuantity, transaction.Comments, notifierWhenInsufficient: notifierWhenInsufficient);
					}
				}

				parentBizO.Saving -= Handler;
			}

			parentBizO.Saving += Handler;
		}
	}

	public void UnlockRegistersMutexes()
	{
		if (transactionData == null)
		{
			return;
		}

		foreach (var grouping in transactionData.GroupBy(x => x.PreviousRegisterHeader))
		{
			var registerHeader = grouping.Key;
			if (registerHeader != null)
			{
				var mutex = registerHeader.Mutex;
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}
	}

	static decimal GetSignedGrossWeight(CusTempStorageRegLineTransaction transaction)
	{
		return transaction.SRT_PackageQty > 0 ? transaction.SRT_GrossWeight : -transaction.SRT_GrossWeight;
	}

	readonly ITemporaryStorageRegisterTransactionDataProvider provider;

	List<TemporaryStorageRegisterTransactionData> transactionData;

	readonly Action<ZString> notifierWhenInsufficient;

	public readonly LoggingInformation Logger;
}
