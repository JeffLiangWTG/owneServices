using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.TemporaryStorage;
using UniversalReferenceConstants = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.TemporaryStorage.Business
{
	public class CusTempStorageRegLineTransactionsDataTransferProcessor : DataTransferProcessor
	{
		public CusTempStorageRegLineTransactionsDataTransferProcessor(CusTempStorageRegLineTransactionFlattenedCollection importCollection, CusTempStorageRegLineTransactionFlattenedImportCollectionInfo importCollectionInfo)
		{
			this.importCollectionInfo = importCollectionInfo;
			this.importCollection = importCollection;
			this.factory = importCollection.Factory;
		}

		protected readonly IBusinessObjectCollection importCollection;
		protected readonly IImportCollectionInfo importCollectionInfo;
		protected readonly BusinessObjectFactory factory;

		public override void Import()
		{
			var rowsCount = importCollection.Count;
			TransactionsToCreate = rowsCount;

			if (rowsCount > 0)
			{
				importCollection.Factory.ActivateStringInterning();
				var originalRefreshValue = factory.RefreshEnabled;
				try
				{
					importCollection.SuspendValidation();
					importCollection.Factory.SuspendValidation();
					importCollection.Factory.RefreshEnabled = false;
					using (importCollection.SuspendListChanged())
					{
						ListChangedEventHandler listChangedHandler = (sender, e) => ReportFlattenedCollectionChanged(rowsCount, sender, e);
						importCollection.ListChanged += listChangedHandler;
						ImportCore();
						importCollection.ListChanged -= listChangedHandler;
					}
				}
				finally
				{
					importCollection.ResumeValidation();
					importCollection.Factory.ResumeValidation();
					importCollection.Factory.RefreshEnabled = originalRefreshValue;
				}
			}
		}

		void ImportCore()
		{
			var rowsProcessed = 0;

			foreach (CusTempStorageRegLineTransactionFlattened row in importCollection)
			{
				if (IsCanceled
					|| !OnProgressChanged((rowsProcessed + 1) * 100 / importCollection.Count, GetProgressChangedStatus(rowsProcessed + 1)))
				{
					break;
				}

				var currentRegLine = CheckCusTempStorageRegHeaderLineAndItem(row);
				if (currentRegLine != null)
				{
					var (type, message) = ProcessCusTempStorageRegLineTransaction(currentRegLine, row);
					if (type == MessageType.Success)
					{
						TransactionsCreated++;
					}
					else
					{
						AddErrorMessage(row, type, message);
						if (type == MessageType.Error || type == MessageType.SaveException)
						{
							TransactionsExcluded++;
						}
						else
						{
							TransactionsWithWarnings++;
						}
					}
				}
				else
				{
					AddErrorMessage(row, MessageType.Error, Res.GetString("CCF62CE0-626A-420E-837E-3FCE896CE36A", "no match or status=CLS"));
					TransactionsExcluded++;
				}
			}

			void AddErrorMessage(CusTempStorageRegLineTransactionFlattened row, MessageType messageType, string reason)
			{
				var messageTypeText = Res.GetString("48D17FAE-F3BA-495D-91A9-94C845551CF4", "Excluded");
				if (messageType == MessageType.Warning)
				{
					messageTypeText = Res.GetString("C31AB81A-9CC6-4F70-8DAB-5E2A9C2E4344", "Warning (not excluded)");
				}
				else if (messageType == MessageType.SaveException)
				{
					messageTypeText = Res.GetString("FE6D266A-1775-4B20-BBA5-34434A89186C", "Unable to save");
				}

				Log += Res.GetString("F2F7722F-C39D-41A5-9425-50F429DD3791", "Temporary Storage Register [TSD Number: {0}, Item Number: {1}, Package Type: {2}] {3}: {4}", row.SRH_Reference, row.SRI_GoodsItemNumber, row.SRL_PackageType, messageTypeText, reason) + "\r\n";
			}
		}

		string GetProgressChangedStatus(int rowsProcessed) => Res.GetString("55BC7409-C264-4951-B741-6B7524E30913", "Importing Temporary Storage Register Transactions ({0} of {1}) ...", rowsProcessed, importCollection.Count);

		CusTempStorageRegLine CheckCusTempStorageRegHeaderLineAndItem(CusTempStorageRegLineTransactionFlattened row)
		{
			var regHeaderQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
			regHeaderQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_Reference, row.SRH_Reference);
			regHeaderQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_Status, UniversalReferenceConstants.TemporaryStorageStatus.Open);
			var regPromisesSubQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegPremises), CusTempStorageRegPremisesSchema.PK);
			regPromisesSubQuery.AddToFilter(CusTempStorageRegPremisesSchema.SRP_CustomsLocation, row.SRP_CustomsLocation);
			regHeaderQuery.AddSubQuery(CusTempStorageRegHeaderSchema.SRH_SRP_Premises, regPromisesSubQuery, JoinCondition.And);

			var currentRegHeader = factory.LoadTop1<CusTempStorageRegHeader>(regHeaderQuery);

			return currentRegHeader?.CusTempStorageRegLines
				.Cast<CusTempStorageRegLine>()
				.FirstOrDefault(l => l.SRL_PackageType == row.SRL_PackageType
					&& (!l.IsPackageTypeFrame || l.SRL_PackageMarks.ToUpper() == row.SRL_PackageMarks.ToUpper())
					&& l.IsOpen
					&& l.RegLineItemPivots.Cast<CusTempStorageRegLineItemPivot>().Any(i => i.RegLineItem.SRI_GoodsItemNumber == row.SRI_GoodsItemNumber));
		}

		(MessageType, string) ProcessCusTempStorageRegLineTransaction(CusTempStorageRegLine currentRegLine, CusTempStorageRegLineTransactionFlattened row)
		{
			var grossWeightRemainingLessThan0 = Res.GetString("84BCE73B-6673-48C8-B536-B391EFDFB3B6", "There is not enough Remaining Gross Weight for this Line");

			if (currentRegLine.IsPackageTypeFrame)
			{
				if (row.SRT_PackageQty != -1)
				{
					return (MessageType.Error, Res.GetString("23CC0661-93E2-423A-99F0-12186264AF49", "For Vehicles (FR) Package Quantity must be -1"));
				}
				else
				{
					row.SRT_GrossWeight = -currentRegLine.OpeningBalanceTransaction.SRT_GrossWeight;
				}
			}
			else if (currentRegLine.IsPackageTypeBulk)
			{
				if (currentRegLine.GrossWeightRemainingCalculated + row.SRT_GrossWeight < 0)
				{
					return (MessageType.Error, grossWeightRemainingLessThan0);
				}
				else
				{
					row.SRT_PackageQty = 0;
				}
			}
			else
			{
				var newRemainingCalculated = currentRegLine.PackagesRemainingCalculated + row.SRT_PackageQty;

				if (newRemainingCalculated < 0)
				{
					return (MessageType.Error, Res.GetString("D4D1DDB7-1F7D-4F68-801F-2E5D4C7860AC", "There are not enough Remaining Packages for this Line"));
				}
				else if (newRemainingCalculated == 0)
				{
					row.SRT_GrossWeight = -currentRegLine.GrossWeightRemainingCalculated;
				}

				if (currentRegLine.GrossWeightRemainingCalculated + row.SRT_GrossWeight < 0)
				{
					return (MessageType.Error, grossWeightRemainingLessThan0);
				}
			}

			var newBondAmount = 0m;
			if (currentRegLine.GrossWeightRemainingCalculated + row.SRT_GrossWeight == 0)
			{
				newBondAmount = -currentRegLine.BondAmountRemainingCalculated;
			}
			else if (row.SRT_GrossWeight < 0)
			{
					newBondAmount = -Utilities.Round(Math.Abs(row.SRT_GrossWeight * currentRegLine.OpeningBalanceTransaction.SRT_BondAmount / currentRegLine.OpeningBalanceTransaction.SRT_GrossWeight), 0);
			}

			var newRegLineTransaction = currentRegLine.CusTempStorageRegLineTransactions.AddNew();

			newRegLineTransaction.SRT_SRL = currentRegLine.PK;
			newRegLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
			newRegLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			newRegLineTransaction.SRT_PhysicalInOutDate = row.SRT_PhysicalInOutDate.ToOffset();
			newRegLineTransaction.SRT_TransactionDate = row.SRT_TransactionDate.ToOffset();
			newRegLineTransaction.SRT_GrossWeight = row.SRT_GrossWeight;
			newRegLineTransaction.SRT_PackageQty = row.SRT_PackageQty;
			newRegLineTransaction.SRT_InternalReferenceType = row.SRT_InternalReferenceType;
			newRegLineTransaction.SRT_InternalReferenceNumber = row.SRT_InternalReferenceNumber;
			newRegLineTransaction.SRT_ReferenceType = row.SRT_ReferenceType;
			newRegLineTransaction.SRT_Reference = row.SRT_Reference;
			newRegLineTransaction.SRT_Comments = row.SRT_Comments;
			newRegLineTransaction.SRT_BondAmount = newBondAmount;

			TemporaryStorageHelper.SetRegLineStatus(currentRegLine, currentRegLine.GrossWeightRemainingCalculated == 0);

			TemporaryStorageHelper.SetRegHeaderStatus(currentRegLine.RegHeader);

			try
			{
				factory.Save();
			}
			catch (ZSaveException saveException)
			{
				return (MessageType.SaveException, saveException.InnerException.Message);
			}

			if (newBondAmount < 0)
			{
				var guaranteeHeader = GetGuaranteeHeader(currentRegLine);
				if (guaranteeHeader != null)
				{
					var pendingAmount = guaranteeHeader.GetTransactions().Where(t => t.CPL_Reference == row.SRH_Reference && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed).Sum(t => t.CPL_TranValue);

					if (pendingAmount < 0)
					{
						guaranteeHeader.AddWriteOffTransaction(row.SRH_Reference, -newBondAmount, row.SRT_PhysicalInOutDate, $"TS {row.SRH_Reference} / {row.SRT_ReferenceType} {row.SRT_Reference}");
					}
					else if (pendingAmount > 0)
					{
						return (MessageType.Warning, Res.GetString("D1C476B8-8EA6-400E-8BE6-05368222B6EF", "Reference {0} has a positive balance of {1} EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.", row.SRH_Reference, pendingAmount));
					}
				}
			}

			try
			{
				factory.Save();
			}
			catch (ZSaveException saveException)
			{
				return (MessageType.Warning, saveException.InnerException.Message);
			}

			return (MessageType.Success, string.Empty);
		}

		CusGuaranteeHeader GetGuaranteeHeader(ICusTempStorageRegLine regLine)
		{
			var ghRefNo = ((CusTempStorageRegHeader)regLine.RegHeader).Guarantee?.PW_BondNumber ?? string.Empty;
			if (!ghRefNo.IsEmpty)
			{
				var ghLoader = new CusGuaranteeHeader.Loader(factory);
				return ghLoader.Load<CusGuaranteeHeader>(ghRefNo).FirstOrDefault();
			}

			return null;
		}

		void ReportFlattenedCollectionChanged(int originalCollectionCount, object sender, ListChangedEventArgs e)
		{
			var currentCollectionCount = importCollection.Count;
			if (originalCollectionCount != currentCollectionCount)
			{
				ErrorReporter.ReportOnce("Collection changed during the import.", string.Format("Collection should not be changed. original count = {0}, current count = {1}", originalCollectionCount, currentCollectionCount));
			}
		}

		public override void Rollback()
		{
			foreach (var item in importCollection.ToArray())
			{
				importCollection.Delete(item);
			}
		}

		#region Logging

		public string Log { get; set; }

		public int TransactionsToCreate { get; set; }
		public int TransactionsCreated { get; set; }
		public int TransactionsExcluded { get; set; }
		public int TransactionsWithWarnings { get; set; }

		enum MessageType
		{
			Success,
			Warning,
			Error,
			SaveException
		}

		#endregion
	}
}
