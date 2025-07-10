using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Integration.Customs.TemporaryStorage;
using EFTAUniversalReferenceConstants = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.UniversalReferenceConstants;
using EUInterfaces = Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business
{
	public static class TemporaryStorageHelper
	{
		public static ZBool IsTemporaryStorageRegisterEnabled(ZString countryCode)
			=> !string.IsNullOrWhiteSpace(countryCode)
				&& ((new EU.Registry.EUCustomsDataRegistry()).RegisterEnabled.Value
					|| (new EU.Registry.EUCustomsDataRegistry()).RegisterEnabledDeveloperOnly.Value);

		public static ZBool IsLocationManagedInPremises(BusinessObjectFactory factory, ZString location, ZString premisesType = default) => factory.Exists(ObjectFactory.GetType(typeof(EUInterfaces.ICusTempStorageRegPremises)), GetPremisesQuery(location, premisesType));

		public static EUInterfaces.ICusTempStorageRegPremises GetManagedPremises(BusinessObjectFactory factory, ZString premisesType, ZString location) => factory.LoadTop1<EUInterfaces.ICusTempStorageRegPremises>(GetPremisesQuery(location, premisesType));

		static ZQuery GetPremisesQuery(ZString location, ZString premisesType)
		{
			var query = new ZQuery(CusTempStorageRegPremisesSchema.SRP_CustomsLocation, location);
			query.AddToFilter(CusTempStorageRegPremisesSchema.SRP_IsActive, true);
			if (!premisesType.IsEmpty)
			{
				query.AddToFilter(CusTempStorageRegPremisesSchema.SRP_Type, premisesType);
			}
			return query;
		}

		public static void CancelPendingRegLineTransactions(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType)
		{
			var transactions = GetPendingRegLineTransactions(factory, internalRefNum, internalRefType).ToList();

			transactions.ForEach(t => t.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted);
		}

		public static EUInterfaces.ICusTempStorageRegLineTransaction[] GetPendingRegLineTransactions(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType)
			=> GetRegLineTransactions(factory, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalRefNum, internalRefType);

		public static EUInterfaces.ICusTempStorageRegLineTransaction[] GetConfirmedRegLineTransactions(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType, string referenceNumber = "")
			=> GetRegLineTransactions(factory, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalRefNum, internalRefType, referenceNumber);

		static EUInterfaces.ICusTempStorageRegLineTransaction[] GetRegLineTransactions(BusinessObjectFactory factory, ZString status, ZString internalRefNum, ZString internalRefType, string referenceNumber = "")
		{
			var query = new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_TransactionStatus, status);
			_ = query.AddToFilter(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber, internalRefNum);
			_ = query.AddToFilter(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceType, internalRefType);
			if (!referenceNumber.IsNullOrEmpty())
			{
				_ = query.AddToFilter(CusTempStorageRegLineTransactionSchema.SRT_Reference, referenceNumber);
			}

			return factory.Load<EUInterfaces.ICusTempStorageRegLineTransaction>(query);
		}

		public static (IEnumerable<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)> declarationDataRegheaderList, ZString locationErrorMessage) GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(BusinessObjectFactory factory, IEnumerable<DeclarationDataToReserveTSGoods> declarationDataList, ZString code, ZString location, ZString referenceNumber, bool checkPremises = true, string jobNumber = "", Func<ZString, ZString> formatDocRef = null, bool shouldHaveLAMEMessageError = false)
		{
			return GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(factory, declarationDataList, new[] { code }, location, referenceNumber, checkPremises, jobNumber, formatDocRef, shouldHaveLAMEMessageError);
		}

		public static (IEnumerable<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)> declarationDataRegheaderList, ZString locationErrorMessage) GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(BusinessObjectFactory factory, IEnumerable<DeclarationDataToReserveTSGoods> declarationDataList, IEnumerable<ZString> codes, ZString location, ZString referenceNumber, bool checkPremises = true, string jobNumber = "", Func<ZString, ZString> formatDocRef = null, bool shouldHaveLAMEMessageError = false)
		{
			var resultDeclarationDataRegheaderList = new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>();

			var declarationDataWithDocsWithCode = declarationDataList.Where(d => codes.Contains(d.Document.CSI_Code));

			if (declarationDataWithDocsWithCode.Any())
			{
				foreach (var declarationData in declarationDataWithDocsWithCode)
				{
					var declarationDataDocRef = declarationData.Document.CSI_ReferenceNumber;
					var regHeader = GetRegHeader(factory, declarationDataDocRef);
					if (regHeader == null && declarationDataDocRef.Length >= 18 && formatDocRef != null)
					{
						regHeader = GetRegHeader(factory, formatDocRef(declarationDataDocRef));
					}
					if (regHeader != null)
					{
						var premisesLocation = checkPremises ? regHeader.Premises.SRP_CustomsLocation : ZString.Empty;
						if (checkPremises && premisesLocation != location)
						{
							var errorMessage = shouldHaveLAMEMessageError ? GetErrorMessageForLAME(regHeader.SRH_Reference, premisesLocation) : GetErrorMessage(regHeader.SRH_Reference, premisesLocation);
							return (new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>(), errorMessage);
						}
						else
						{
							resultDeclarationDataRegheaderList.Add((declarationData, regHeader));
						}
					}
					else if (shouldHaveLAMEMessageError)
					{
						var errorMessage = GetErrorMessageForLAMEMistakenTheNumber(declarationData.Document.CSI_ReferenceNumber) + "\n\n" + SuffixForErrorMessageDoYowWantToCancelDeclaration;
						return (new List<(DeclarationDataToReserveTSGoods, ICusTempStorageRegHeader)>(), errorMessage);
					}
				}
			}

			return (resultDeclarationDataRegheaderList, ZString.Empty);

			ZString GetErrorMessage(ZString docRef, ZString premisesLocation)
			{
				var refNum = jobNumber.IsNullOrEmpty() ? referenceNumber : (ZString)$"{jobNumber}/{referenceNumber}";
				return Res.GetString("1857A277-23F1-4980-8322-0325BD6FE9A6",
					"{0}: Goods in TSD Number {1} are not stored in location {2} so this declaration might be rejected by Customs. The correct location should be {3}.\n\nPlease, set the correct location before submitting this declaration to Customs.",
					refNum, docRef, location, premisesLocation);
			}

			ZString GetErrorMessageForLAME(ZString docRef, ZString premisesLocation)
			{
				return Res.GetString("6343861A-0094-4D84-B03C-63529770EE3D",
					"{0}: Goods in LAME Reception Certificate {1} are not stored in location {2}. The correct location should be {3}.\n\nPlease, set the correct location before submitting this declaration to Customs.",
					referenceNumber, docRef, location, premisesLocation);
			}

			ZString GetErrorMessageForLAMEMistakenTheNumber(ZString docRef)
			{
				return Res.GetString("83C8887B-80F9-4416-A5A8-1D965DD13CCE",
					"{0}: There is no record in the Temporary Storage Register for LAME Reception Certificate {1}. You might have mistaken the number.",
					referenceNumber, docRef);
			}
		}

		public static EUInterfaces.ICusTempStorageRegLineItem GetRegLineItemAssociatedToPreviousDocumentAndRegHeader(BusinessObjectFactory factory, ZInt prevDocLineNo, ICusTempStorageRegHeader regHeader)
		{
			var openedLinePKs = regHeader.CusTempStorageRegLines.Where(l => l.IsOpen).Select(l => l.PK).ToArray();

			var itemQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegLineItem));
			itemQuery.AddToFilter(CusTempStorageRegLineItemSchema.SRI_GoodsItemNumber, prevDocLineNo);

			var pivotQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item);
			openedLinePKs.ForEach(l => pivotQuery.AddToFilter(JoinCondition.Or, CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, l));

			itemQuery.AddSubQuery(pivotQuery, JoinCondition.And);
			return factory.LoadTop1<EUInterfaces.ICusTempStorageRegLineItem>(itemQuery);
		}

		public static ICusTempStorageRegLine GetRegLineFromLineItemWithPackTypeOrMarksOrVin(BusinessObjectFactory factory, ICusTempStorageRegLineItem lineItem, ZString packageType, ZString marksOrVin, bool isAmendment = false)
		{
			var result = GetRegLineFromLineItemCommon(factory, lineItem, AddExtraFilters);

			var regLineToReturn = result.FirstOrDefault();
			if (result != null && result.Length > 1)
			{
				regLineToReturn = result.FirstOrDefault(l => l.SRL_PackageMarks == marksOrVin);
			}

			return regLineToReturn;

			void AddExtraFilters(ZDBOnlyQuery query)
			{
				if (!isAmendment)
				{
					query.AddToFilter(CusTempStorageRegLineSchema.SRL_CustomsStatus, EFTAUniversalReferenceConstants.TemporaryStorageStatus.Open);
				}

				if (packageType == EFTAUniversalReferenceConstants.PackageType.Frame)
				{
					query.AddToFilter(CusTempStorageRegLineSchema.SRL_PackageMarks, SQLComparisonOperator.StartsWith, marksOrVin);
				}
				else
				{
					query.AddToFilter(CusTempStorageRegLineSchema.SRL_PackageType, packageType);
				}
			}
		}

		static EUInterfaces.ICusTempStorageRegLine[] GetRegLineFromLineItemCommon(BusinessObjectFactory factory, ICusTempStorageRegLineItem lineItem, Action<ZDBOnlyQuery> addSpecificFilters)
		{
			var lineQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));

			addSpecificFilters(lineQuery);

			var pivotQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line);
			pivotQuery.AddToFilter(CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item, lineItem.PK);

			lineQuery.AddSubQuery(pivotQuery, JoinCondition.And);
			return factory.Load<EUInterfaces.ICusTempStorageRegLine>(lineQuery);
		}

		public static void ReserveTemporaryStorageGoods(ZString internalRefNum, ZString internalRefType, IEnumerable<DataToReserveTSGoods> dataToReserveList)
		{
			foreach (var data in dataToReserveList)
			{
				var regLine = data.RegLine;

				regLine.SRL_CustomsStatus = EFTAUniversalReferenceConstants.TemporaryStorageStatus.Open;

				var newTransaction = CreateNewTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalRefNum, internalRefType);

				var packQty = ZInt.Zero;
				var grossWeight = ZDecimal.Zero;
				var remainingGrossWeightInLine = regLine.GrossWeightRemainingCalculated;
				var remainingPackagesInLine = regLine.PackagesRemainingCalculated;
				var entryGrossWeight = data.TotalGrossWeight;
				var entryPackQty = data.TotalPackQty;

				var conTransactions = regLine.CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed
																							&& t.SRT_InternalReferenceNumber == internalRefNum
																							&& t.SRT_InternalReferenceType == internalRefType);
				var conTransactionsABSGrossWeight = Math.Abs(conTransactions.Sum(t => t.SRT_GrossWeight));
				var conTransactionsABSPackQty = Math.Abs(conTransactions.Sum(t => t.SRT_PackageQty));
				var isAdjustment = data.IsAdjustment;

				if (regLine.IsPackageTypeBulk)
				{
					var remainingGrossWeightCalculated = isAdjustment ? (ZDecimal)(remainingGrossWeightInLine + conTransactionsABSGrossWeight) : remainingGrossWeightInLine;
					var calculatedGrossWeightWhenRemainginIsGreater = isAdjustment ? (conTransactionsABSGrossWeight - entryGrossWeight) : -entryGrossWeight;

					grossWeight = entryGrossWeight.Ceiling(0) > remainingGrossWeightCalculated
																? -remainingGrossWeightInLine
																: calculatedGrossWeightWhenRemainginIsGreater;
				}
				else if (regLine.IsPackageTypeFrame)
				{
					packQty = -1;
					grossWeight = -regLine.OpeningBalanceTransaction.SRT_GrossWeight;
				}
				else
				{
					packQty = isAdjustment ? conTransactionsABSPackQty - entryPackQty : -entryPackQty;

					var calculatedGrossWeight = data.TotalEntryPackQty == ZInt.Zero ? entryGrossWeight : (ZDecimal)(entryPackQty * entryGrossWeight / data.TotalEntryPackQty);

					if (isAdjustment)
					{
						grossWeight = remainingPackagesInLine + packQty == 0
																	? -remainingGrossWeightInLine
																	: conTransactionsABSGrossWeight - calculatedGrossWeight;
					}
					else
					{
						grossWeight = entryPackQty == remainingPackagesInLine
												? -remainingGrossWeightInLine
												: -(Math.Min(calculatedGrossWeight, remainingGrossWeightInLine));
					}
				}

				newTransaction.SRT_GrossWeight = grossWeight;
				newTransaction.SRT_PackageQty = packQty;

				if (isAdjustment)
				{
					newTransaction.SRT_Comments = AdjustmentTransactionComment;
				}
			}
		}

		public static void RestoreVehicles(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType, ICusTempStorageRegLineItem lineItem, IEnumerable<ZString> packagesVinsInDeclaration)
		{
			var vehicleLineList = GetRegLineFromLineItemCommon(factory, lineItem, AddExtraFilter);

			var vehicleLinesWithCONTransactionsList = vehicleLineList.Where(v => v.CusTempStorageRegLineTransactions.Any(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed
																																&& t.SRT_InternalReferenceNumber == internalRefNum
																																&& t.SRT_InternalReferenceType == internalRefType));

			foreach (var vehicleLine in vehicleLinesWithCONTransactionsList)
			{
				var vehicleIsDeclared = packagesVinsInDeclaration.Any(vin => vehicleLine.SRL_PackageMarks.StartsWith(vin));
				if (!vehicleIsDeclared)
				{
					var grossWeight = vehicleLine.OpeningBalanceTransaction?.SRT_GrossWeight ?? ZDecimal.Zero;
					var newTransaction = CreateNewTransactionWithExtraData(vehicleLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, internalRefNum, internalRefType, grossWeight, 1, AdjustmentTransactionComment);

					vehicleLine.SRL_CustomsStatus = EFTAUniversalReferenceConstants.TemporaryStorageStatus.Open;
				}
			}

			void AddExtraFilter(ZDBOnlyQuery query)
			{
				query.AddToFilter(CusTempStorageRegLineSchema.SRL_PackageType, EFTAUniversalReferenceConstants.PackageType.Frame);
			}
		}

		static ICusTempStorageRegHeader GetRegHeader(BusinessObjectFactory factory, ZString reference)
		{
			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, reference);
			return factory.LoadTop1<EUInterfaces.ICusTempStorageRegHeader>(query);
		}

		public static void ConfirmTemporaryStorageGoodsConsumptionIfNeeded(BusinessObjectFactory factory, ZString goodsLocation, ZString countryCode, ZString customsStatus, ZString internalRefNum, ZString internalRefType
			, ZString entryReference, ZString prevDocCode, Func<(IEnumerable<DeclarationDataToReserveTSGoods>, ZString)> getDeclarationDataToReserve
			, ZString mrnCode, ZString commentPrefix, ZString jobNumberForComment, ZDateTime mrnIssueDate, ZDateTime releaseDate, ZString writeOffCommentText, Logs logs
			, IReadOnlyList<ZString> customsStatusToCancelTemporaryStoragePendingTransactions, IReadOnlyList<ZString> customsStatusToNotCreateTemporaryStorageTransactions, IReadOnlyList<ZString> customsStatusToConfirmTemporaryStoragePendingTransactions
			, bool isLAME = false, Func<ZString, ZString> formatDocRef = null)
		{
			ConfirmTemporaryStorageGoodsConsumptionIfNeeded(factory, goodsLocation, countryCode, customsStatus, internalRefNum, internalRefType, entryReference, new[] { prevDocCode }, getDeclarationDataToReserve
				, mrnCode, commentPrefix, jobNumberForComment, mrnIssueDate, releaseDate, writeOffCommentText, logs, customsStatusToCancelTemporaryStoragePendingTransactions, customsStatusToNotCreateTemporaryStorageTransactions
				, customsStatusToConfirmTemporaryStoragePendingTransactions, isLAME, formatDocRef);
		}

		public static void ConfirmTemporaryStorageGoodsConsumptionIfNeeded(BusinessObjectFactory factory, ZString goodsLocation, ZString countryCode, ZString customsStatus, ZString internalRefNum, ZString internalRefType
			, ZString entryReference, IEnumerable<ZString> prevDocCodes, Func<(IEnumerable<DeclarationDataToReserveTSGoods>, ZString)> getDeclarationDataToReserve
			, ZString mrnCode, ZString commentPrefix, ZString jobNumberForComment, ZDateTime mrnIssueDate, ZDateTime releaseDate, ZString writeOffCommentText, Logs logs
			, IReadOnlyList<ZString> customsStatusToCancelTemporaryStoragePendingTransactions, IReadOnlyList<ZString> customsStatusToNotCreateTemporaryStorageTransactions, IReadOnlyList<ZString> customsStatusToConfirmTemporaryStoragePendingTransactions
			, bool isLAME = false, Func<ZString, ZString> formatDocRef = null)
		{
			var premisesType = isLAME ? CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility : CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

			if (!goodsLocation.IsEmpty
				&& IsTemporaryStorageRegisterEnabled(countryCode)
				&& IsLocationManagedInPremises(factory, goodsLocation, premisesType))
			{
				if (customsStatusToCancelTemporaryStoragePendingTransactions.Contains(customsStatus))
				{
					CancelPendingRegLineTransactions(factory, internalRefNum, internalRefType);
				}
				else
				{
					if (GetPendingRegLineTransactions(factory, internalRefNum, internalRefType).IsNullOrEmpty()
						&& GetConfirmedRegLineTransactions(factory, internalRefNum, internalRefType).IsNullOrEmpty()
						&& !customsStatusToNotCreateTemporaryStorageTransactions.IsNullOrEmpty()
						&& !customsStatusToNotCreateTemporaryStorageTransactions.Contains(customsStatus))
					{
						var (_, _, dataToReserve) = GetDataToReserveTemporaryStorageGoods(factory, internalRefNum, internalRefType, entryReference, prevDocCodes, goodsLocation, getDeclarationDataToReserve, checkPremisesAndRemaining: false, formatDocRef: formatDocRef, isLAME: isLAME);
						ReserveTemporaryStorageGoods(internalRefNum, internalRefType, dataToReserve);
					}
					if (customsStatusToConfirmTemporaryStoragePendingTransactions.Contains(customsStatus))
					{
						ConfirmTemporaryStorageGoodsConsumption(factory, internalRefNum, internalRefType, mrnCode, commentPrefix, jobNumberForComment, mrnIssueDate, releaseDate, writeOffCommentText, logs, isLAME: isLAME);
					}
				}
			}
		}

		public static void ConfirmTemporaryStorageGoodsConsumption(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType, ZString mrnCode, ZString commentPrefix, ZString jobNumberForComment
			, ZDateTime mrnIssueDate, ZDateTime releaseDate, ZString writeOffCommentText, Logs logs, bool isLAME = false)
		{
			var regHeadersList = new List<ICusTempStorageRegHeader>();

			var transactions = GetPendingRegLineTransactions(factory, internalRefNum, internalRefType);
			foreach (var transaction in transactions)
			{
				transaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
				transaction.SRT_ReferenceType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				transaction.SRT_Reference = mrnCode;
				transaction.SRT_TransactionDate = mrnIssueDate.ToOffset();
				transaction.SRT_PhysicalInOutDate = releaseDate.ToOffset();

				var oldComment = transaction.SRT_Comments;
				var oldCommentToAdd = oldComment.IsEmpty ? string.Empty : " - " + oldComment;
				transaction.SRT_Comments = commentPrefix + " " + jobNumberForComment + oldCommentToAdd;

				var regLine = transaction.RegLine;

				var isPackageBulk = regLine.IsPackageTypeBulk;
				var shouldSetCustomsStatusCLS = (isPackageBulk && regLine.GrossWeightRemainingCalculated == 0)
												|| (!isPackageBulk && regLine.PackagesRemainingCalculated == 0);

				SetRegLineStatus(regLine, shouldSetCustomsStatusCLS);

				var header = regLine.RegHeader;

				if (!isLAME)
				{
					transaction.SRT_BondAmount = GetBondAmountForConfirmation(regLine, transaction.SRT_GrossWeight);

					var writeOffComment = "TS " + header.SRH_Reference + writeOffCommentText + " " + mrnCode;
					var writeOffError = AddWriteOffTransactionToGuaranteeIfNeeded(header, writeOffComment, Math.Abs(transaction.SRT_BondAmount), releaseDate);

					if (!writeOffError.IsEmpty)
					{
						var logTypeCode = (NoResString)"TS Guarantee";
						var parameters = new KeyValuePair<string, string>[]
						{
						new (EventReferenceParameters.Codes.Type, logTypeCode),
						new (EventReferenceParameters.Codes.Reason, writeOffError)
						};
						_ = logs.AddNew(AutoEvents.ErrorReport, parameters);
					}
				}

				if (!regHeadersList.Contains(header))
				{
					regHeadersList.Add(header);
				}
			}

			foreach (var regHeader in regHeadersList)
			{
				SetRegHeaderStatus(regHeader);
			}
		}

		static ZDecimal GetBondAmountForConfirmation(ICusTempStorageRegLine regLine, ZDecimal transactionGrossWeight)
		{
			var result = ZDecimal.Zero;

			if (regLine.IsClosed)
			{
				result = -(regLine.BondAmountRemainingCalculated);
			}
			else if (!regLine.IsClosed && transactionGrossWeight < 0)
			{
				var transactionOBL = regLine.OpeningBalanceTransaction;
				ZDecimal bondAmount = (transactionOBL?.SRT_GrossWeight ?? ZDecimal.Zero) != ZDecimal.Zero ? Math.Abs(transactionGrossWeight) * transactionOBL.SRT_BondAmount / transactionOBL.SRT_GrossWeight : ZDecimal.Zero;
				result = -(bondAmount.Round(2));
			}

			return result;
		}

		static ZDecimal GetGuaranteeFromRegHeaderPendingAmount(ICusTempStorageRegHeader regHeader)
		{
			return regHeader is EUInterfaces.ICusTempStorageRegHeader { Guarantee: CommonGuarantee { CusGuarantee: { } guarantee } }
				? guarantee.CusGuaranteeLineTransactions.Where(t => t.CPL_Reference == regHeader.SRH_Reference && t.CPL_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed).Sum(t => t.CPL_TranValue)
				: ZDecimal.Zero;
		}

		static ZString AddWriteOffTransactionToGuaranteeIfNeeded(ICusTempStorageRegHeader regHeader, ZString writeOffComment, ZDecimal writeOffTranValue, ZDateTime writeOffDate)
		{
			var messageToReturn = ZString.Empty;

			var euRegHeader = (EUInterfaces.ICusTempStorageRegHeader)regHeader;
			var guarantee = ((CommonGuarantee)(euRegHeader.Guarantee)).CusGuarantee;
			ZDecimal tranValueSum = guarantee?.CusGuaranteeLineTransactions.Where(t => t.CPL_Reference == regHeader.SRH_Reference && t.CPL_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed).Sum(t => t.CPL_TranValue) ?? ZDecimal.Zero;

			if (tranValueSum < 0)
			{
				guarantee.AddWriteOffTransaction(regHeader.SRH_Reference, writeOffTranValue, writeOffDate, writeOffComment);
			}
			else if (tranValueSum > 0)
			{
				messageToReturn = $"Reference {regHeader.SRH_Reference} has a positive balance of {tranValueSum.Round(2).ToString(2)} EUR. " +
						$"Please check the existing transactions for this reference and create a manual adjustment if needed.";
			}

			return messageToReturn;
		}

		static CusTempStorageRegLineTransaction CreateNewTransaction(ICusTempStorageRegLine regLine, ZString status, ZString internalRefNum, ZString internalRefType)
		{
			var newTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			newTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			newTransaction.SRT_TransactionStatus = status;
			newTransaction.SRT_InternalReferenceNumber = internalRefNum;
			newTransaction.SRT_InternalReferenceType = internalRefType;

			return (CusTempStorageRegLineTransaction)newTransaction;
		}

		static CusTempStorageRegLineTransaction CreateNewTransactionWithExtraData(ICusTempStorageRegLine regLine, ZString status, ZString internalRefNum, ZString internalRefType, ZDecimal grossWeight, ZInt packageQty, ZString comments)
		{
			var newTransaction = CreateNewTransaction(regLine, status, internalRefNum, internalRefType);
			newTransaction.SRT_GrossWeight = grossWeight;
			newTransaction.SRT_PackageQty = packageQty;
			newTransaction.SRT_Comments = comments;

			return newTransaction;
		}

		static void CreateNewCancellationTransaction(CusTempStorageRegLine regLine, ZString comment, ZString internalRefNum, ZString internalRefType, ZDecimal grossWeight, ZInt packageQty, ZString mrn, ZDateTime cancellationDate, ZDecimal bondAmount)
		{
			var regLineTransaction = CreateNewTransactionWithExtraData(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, internalRefNum, internalRefType, Math.Abs(grossWeight), Math.Abs(packageQty), comment);
			regLineTransaction.SRT_ReferenceType = "MRN";
			regLineTransaction.SRT_Reference = mrn;
			regLineTransaction.SRT_TransactionDate = cancellationDate.ToOffset();
			regLineTransaction.SRT_PhysicalInOutDate = cancellationDate.ToOffset();
			regLineTransaction.SRT_BondAmount = Math.Abs(bondAmount);
		}

		static void AddGuaranteeTransactionForCancelation(ICusTempStorageRegHeader regHeader, ZDecimal totalBondAmount, string prefixComment, string internalReferenceNumber, string mrn, ZDateTime cancelationDate)
		{
			var absTotalBondAmount = Math.Abs(totalBondAmount);
			if (absTotalBondAmount <= 0)
			{ return; }

			var euRegHeader = (EUInterfaces.ICusTempStorageRegHeader)regHeader;
			var negativeTotalBondAmount = absTotalBondAmount * (-1);
			var guarantee = ((CommonGuarantee)(euRegHeader.Guarantee)).CusGuarantee;
			var comment = string.Format((NoResString)"{0} {1}. MRN: {2} (Canceled)", prefixComment, internalReferenceNumber, mrn);

			guarantee.AddTransaction(regHeader.SRH_Reference, comment, ZString.Empty, ZString.Empty, negativeTotalBondAmount, ZDecimal.Zero, status: Customs.Business.PermitTransactionStatusList.Codes.Confirmed, transactionDate: cancelationDate, checkBursting: false);
		}

		public static void SetRegHeaderStatus(ICusTempStorageRegHeader regHeader)
		{
			var shouldNotSetStatusCLS = regHeader.CusTempStorageRegLines.Any(l => !l.IsClosed);

			if (shouldNotSetStatusCLS)
			{
				regHeader.SRH_Status = EFTAUniversalReferenceConstants.TemporaryStorageStatus.Open;
			}
			else
			{
				regHeader.SRH_Status = EFTAUniversalReferenceConstants.TemporaryStorageStatus.Closed;
			}
		}

		public static void SetRegLineStatus(ICusTempStorageRegLine regLine, ZBool shouldSetCustomsStatusCLS)
		{
			if (shouldSetCustomsStatusCLS)
			{
				regLine.SRL_CustomsStatus = EFTAUniversalReferenceConstants.TemporaryStorageStatus.Closed;
			}
			else
			{
				regLine.SRL_CustomsStatus = EFTAUniversalReferenceConstants.TemporaryStorageStatus.Open;
			}
		}

		public static void ManageTemporaryStorageCancelationWhenProcessResponse(BusinessObjectFactory factory, ZString countryCode, ZString goodsLocation, ZString internalReferenceNumber, ZString commentReferenceNumber, ZString internalReferenceType, ZString mrn, ZString transactionCommentPrefix, ZString guaranteeCommentPrefix, ZDateTime cancelationDate, string premiseType = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, bool shouldNotManageTemporaryStorageCancelation = false, bool shouldCancelPendingTransactions = false, bool shouldCheckTransactionsAlreadyCanceled = false)
		{
			if (shouldNotManageTemporaryStorageCancelation
				|| !IsTemporaryStorageRegisterEnabled(countryCode)
				|| !IsLocationManagedInPremises(factory, goodsLocation, premiseType))
			{
				return;
			}

			if (shouldCancelPendingTransactions)
			{
				CancelPendingRegLineTransactions(factory, internalReferenceNumber, internalReferenceType);
			}

			var transactionsCON = GetConfirmedRegLineTransactions(factory, internalReferenceNumber, internalReferenceType, mrn);

			if (!transactionsCON.Any())
			{ return; }

			var transactionsGroupedByRegLine = transactionsCON.GroupBy(x => x.RegLine);

			foreach (var transactionsPerRegLine in transactionsGroupedByRegLine)
			{
				var totalGrossWeight = transactionsPerRegLine.Sum(x => x.SRT_GrossWeight);
				var transactionComment = string.Format((NoResString)"{0} {1} (Canceled)", transactionCommentPrefix, commentReferenceNumber);

				if (shouldCheckTransactionsAlreadyCanceled && (totalGrossWeight >= 0 || transactionsPerRegLine.Any(x => x.SRT_Comments == transactionComment)))
				{ return; }

				var regLine = transactionsPerRegLine.Key;
				var regHeader = regLine.RegHeader;

				var totalPackagesQty = transactionsPerRegLine.Sum(x => x.SRT_PackageQty);
				var totalBondAmount = transactionsPerRegLine.Sum(x => x.SRT_BondAmount);

				CreateNewCancellationTransaction((CusTempStorageRegLine)regLine, transactionComment, internalReferenceNumber, internalReferenceType, totalGrossWeight,
					totalPackagesQty, mrn, cancelationDate, totalBondAmount);

				SetRegLineStatus(regLine, false);
				SetRegHeaderStatus(regHeader);

				AddGuaranteeTransactionForCancelation(regHeader, totalBondAmount, guaranteeCommentPrefix, internalReferenceNumber, mrn, cancelationDate);
			}
		}

		public static ZString ConfirmNewADJTransactionBeforeSaving(ICusTempStorageRegLineTransaction lineTransaction)
		{
			var messageToReturn = ZString.Empty;

			lineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

			var regLine = lineTransaction.RegLine;

			SetRegLineStatus(regLine, regLine.GrossWeightRemainingCalculated == 0);

			lineTransaction.SRT_BondAmount = GetBondAmountForConfirmation(regLine, lineTransaction.SRT_GrossWeight);

			var regHeader = regLine.RegHeader;

			SetRegHeaderStatus(regHeader);

			if (lineTransaction.SRT_BondAmount < 0)
			{
				var writeOffComment = "TS " + regHeader.SRH_Reference + " / " + lineTransaction.SRT_ReferenceType + " " + lineTransaction.SRT_Reference;
				var absSRTBondAmount = Math.Abs(lineTransaction.SRT_BondAmount);
				var absTranValueSum = Math.Abs(GetGuaranteeFromRegHeaderPendingAmount(regHeader));
				var writeOffTranValue = absSRTBondAmount >= absTranValueSum ? absTranValueSum : absSRTBondAmount;
				messageToReturn = AddWriteOffTransactionToGuaranteeIfNeeded(regHeader, writeOffComment, writeOffTranValue, lineTransaction.SRT_PhysicalInOutDate.ToZDateTime());
			}
			return messageToReturn;
		}

		public static void ResetNewADJTransactionWhenSavingError(ICusTempStorageRegLineTransaction lineTransaction)
		{
			if (!lineTransaction.IsInDatabase)
			{
				lineTransaction.SRT_BondAmount = 0;
			}

			var regHeader = lineTransaction.RegLine.RegHeader;
			var euRegHeader = (EUInterfaces.ICusTempStorageRegHeader)regHeader;
			var guarantee = ((CommonGuarantee)(euRegHeader.Guarantee)).CusGuarantee;
			var expectedComment = (NoResString)"Write-off TS " + regHeader.SRH_Reference;
			var newTransactions = guarantee?.CusGuaranteeLineTransactions.Where(t => t.CPL_Comment.StartsWith(expectedComment) && !t.IsInDatabase).ToList();

			foreach (var transaction in newTransactions)
			{
				guarantee?.CusGuaranteeLineTransactions.Delete(transaction);
			}
		}

		public static (ZString popUpMessage, ZString popUpMessageVINs, IEnumerable<DataToReserveTSGoods> dataToReserveGoodsList) GetDataToReserveTemporaryStorageGoods(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType, ZString entryReference, ZString docCode, ZString goodsLocation, Func<(IEnumerable<DeclarationDataToReserveTSGoods>, ZString)> getDeclarationDataToReserve, bool isAmendment = false, bool checkPremisesAndRemaining = true, bool useCSI_ItemNumber = false, string jobNumber = "", Func<ZString, ZString> formatDocRef = null, bool isLAME = false)
		{
			return GetDataToReserveTemporaryStorageGoods(factory, internalRefNum, internalRefType, entryReference, new[] { docCode }, goodsLocation, getDeclarationDataToReserve, isAmendment, checkPremisesAndRemaining, useCSI_ItemNumber, jobNumber, formatDocRef, isLAME);
		}

		public static (ZString popUpMessage, ZString popUpMessageVINs, IEnumerable<DataToReserveTSGoods> dataToReserveGoodsList) GetDataToReserveTemporaryStorageGoods(BusinessObjectFactory factory, ZString internalRefNum, ZString internalRefType, ZString entryReference, IEnumerable<ZString> docCodes, ZString goodsLocation, Func<(IEnumerable<DeclarationDataToReserveTSGoods>, ZString)> getDeclarationDataToReserve, bool isAmendment = false, bool checkPremisesAndRemaining = true, bool useCSI_ItemNumber = false, string jobNumber = "", Func<ZString, ZString> formatDocRef = null, bool isLAME = false)
		{
			var messageToReturn = ZString.Empty;
			var messageToReturnVINs = ZString.Empty;
			var dataToReserveGoods = new List<DataToReserveTSGoods>();

			CancelPendingRegLineTransactions(factory, internalRefNum, internalRefType);

			var (entryLineDataToReserveTSGoodsList, errorInData) = getDeclarationDataToReserve();

			if (!errorInData.IsEmpty)
			{
				return (errorInData, ZString.Empty, Enumerable.Empty<DataToReserveTSGoods>());
			}

			if (entryLineDataToReserveTSGoodsList.Any())
			{
				var (declarationDataRegheaderList, locationErrorMessage) = GetDeclarationDataWithSpecificCodeDocumentsWithRegHeaderAssociated(factory, entryLineDataToReserveTSGoodsList, docCodes, goodsLocation, entryReference, checkPremises: checkPremisesAndRemaining, jobNumber, formatDocRef, isLAME);

				if (!locationErrorMessage.IsEmpty)
				{
					messageToReturn = locationErrorMessage;
				}
				else
				{
					var errorMessageForGrossWeightForVINs = ZString.Empty;
					var errorMessageForGrossWeight = ZString.Empty;

					foreach (var (declarationData, regHeader) in declarationDataRegheaderList)
					{
						var doc = declarationData.Document;
						var docLineNo = isLAME
											? (ZInt)1
											: useCSI_ItemNumber
												? doc.CSI_ItemNumber
												: doc.CSI_LineNo;
						var docRef = regHeader.SRH_Reference;
						var totalEntryGrossWeight = declarationData.TotalGrossWeight;
						var totalEntryGrossWeightForVINs = (int)declarationData.TotalGrossWeightForVINs;
						int totalEntryPackQty = isLAME && !doc.CSI_PackQty.IsEmpty ? doc.CSI_PackQty : declarationData.Packages.Sum(p => p.qty);

						var regLineItem = GetRegLineItemAssociatedToPreviousDocumentAndRegHeader(factory, docLineNo, regHeader);

						if (regLineItem == null)
						{
							messageToReturn = GetErrorMessageWhenRegLineItemIsNull(entryReference, docLineNo, docRef);
							break;
						}

						var regLinesWithPacks = new List<ICusTempStorageRegLine>();
						var regLinesWithVINs = new List<ICusTempStorageRegLine>();

						if (isAmendment)
						{
							RestoreVehicles(factory, internalRefNum, internalRefType, regLineItem, declarationData.Packages.Where(p => p.type == EFTAUniversalReferenceConstants.PackageType.Frame).Select(p => p.marksOrVin));
						}

						foreach (var package in declarationData.Packages)
						{
							var packType = isLAME && !doc.CSI_PackType.IsEmpty ? doc.CSI_PackType : package.type;
							var packMarksOrVin = package.marksOrVin;
							var packQty = isLAME && !doc.CSI_PackQty.IsEmpty ? doc.CSI_PackQty : package.qty;
							var isAdjustment = packType == EFTAUniversalReferenceConstants.PackageType.Frame && isAmendment;
							var shouldAddDataToReserve = true;

							var regLine = GetRegLineFromLineItemWithPackTypeOrMarksOrVin(factory, regLineItem, packType, package.marksOrVin, isAmendment: isAmendment);

							if (isAmendment)
							{
								var conTransactions = regLine?.CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed
																																&& t.SRT_InternalReferenceNumber == internalRefNum
																																&& t.SRT_InternalReferenceType == internalRefType);
								shouldAddDataToReserve = !(conTransactions != null && conTransactions.Any() && packType == EFTAUniversalReferenceConstants.PackageType.Frame);

								if (conTransactions != null && conTransactions.Any() && packType != EFTAUniversalReferenceConstants.PackageType.Frame)
								{
									var shouldCheckGrossWeight = package.isBulk;
									if (!shouldCheckGrossWeight)
									{
										var packagesRemaining = regLine.PackagesRemainingCalculated;
										var packagesWithCON = Math.Abs(conTransactions.Sum(t => t.SRT_PackageQty));

										var differentQty = packagesWithCON != packQty;
										isAdjustment = differentQty;
										shouldAddDataToReserve = differentQty;
										shouldCheckGrossWeight = !differentQty;

										if (checkPremisesAndRemaining && differentQty && (packagesRemaining + packagesWithCON - packQty) < 0)
										{
											return (GetErrorMessageWhenPackagesRemainingNotEnough(entryReference, docRef, docLineNo, packQty, packType, isLAME), ZString.Empty, Enumerable.Empty<DataToReserveTSGoods>());
										}
									}
									if (shouldCheckGrossWeight)
									{
										var packagesGrossWeightWithCON = Math.Abs(conTransactions.Sum(t => t.SRT_GrossWeight));
										var packQtyForCalculation = package.isBulk ? packQty + 1 : (int)packQty;
										var totalEntryPackQtyForCalculation = package.isBulk ? totalEntryPackQty + 1 : totalEntryPackQty;

										var calculatedGrossWeight = totalEntryPackQty == 0 ? (decimal)totalEntryGrossWeight : packQtyForCalculation * totalEntryGrossWeight / totalEntryPackQtyForCalculation;
										if (packagesGrossWeightWithCON != calculatedGrossWeight)
										{
											isAdjustment = true;
											shouldAddDataToReserve = true;
										}
										else
										{
											shouldAddDataToReserve = false;
										}
									}
								}
							}

							if (shouldAddDataToReserve)
							{
								if (checkPremisesAndRemaining && regLine == null && packType == EFTAUniversalReferenceConstants.PackageType.Frame)
								{
									return (GetErrorMessageWhenVINIsNotInAnyRegLine(entryReference, packMarksOrVin, docRef, docLineNo, isLAME), ZString.Empty, Enumerable.Empty<DataToReserveTSGoods>());
								}
								else if (packType != EFTAUniversalReferenceConstants.PackageType.Frame && !package.isBulk && !isAdjustment)
								{
									var packagesRemaining = regLine?.PackagesRemainingCalculated ?? ZInt.Zero;
									if (checkPremisesAndRemaining && packagesRemaining < packQty)
									{
										return (GetErrorMessageWhenPackagesRemainingNotEnough(entryReference, docRef, docLineNo, packQty, packType, isLAME), ZString.Empty, Enumerable.Empty<DataToReserveTSGoods>());
									}
								}
								else if (regLine == null && package.isBulk)
								{
									return (GetErrorMessageWhenPackagesRemainingNotEnough(entryReference, docRef, docLineNo, packQty, packType, isLAME), ZString.Empty, Enumerable.Empty<DataToReserveTSGoods>());
								}

								if (regLine != null)
								{
									dataToReserveGoods.Add(
										new DataToReserveTSGoods()
										{
											TotalGrossWeight = totalEntryGrossWeight,
											TotalPackQty = packQty,
											TotalEntryPackQty = totalEntryPackQty,
											IsAdjustment = isAdjustment,
											RegLine = regLine,
										});

									if (!regLinesWithPacks.Contains(regLine))
									{
										regLinesWithPacks.Add(regLine);
									}
									if (packType == EFTAUniversalReferenceConstants.PackageType.Frame && !regLinesWithVINs.Contains(regLine))
									{
										regLinesWithVINs.Add(regLine);
									}
								}
							}
						}
						if (regLinesWithPacks.Any())
						{
							var grossWeightRemaining = regLinesWithPacks.Sum(l => l.GrossWeightRemainingCalculated);
							if (checkPremisesAndRemaining && grossWeightRemaining < totalEntryGrossWeight)
							{
								errorMessageForGrossWeight += GetErrorMessageWhenGrossWeightRemainingNotEnough(totalEntryGrossWeight, docRef, docLineNo, grossWeightRemaining, isLAME);
							}
						}

						if (regLinesWithVINs.Any())
						{
							var grossWeightOBL = 0;
							foreach (var regline in regLinesWithVINs)
							{
								var obl = regline.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
								grossWeightOBL += (int)(obl?.SRT_GrossWeight ?? ZDecimal.Zero);
							}
							if (checkPremisesAndRemaining && grossWeightOBL != totalEntryGrossWeightForVINs)
							{
								errorMessageForGrossWeightForVINs += GetErrorMessageWhenGrossWeightIsDifferentForVins(entryReference, docRef, docLineNo, totalEntryGrossWeightForVINs, grossWeightOBL, isLAME);
							}
						}
					}
					if (!errorMessageForGrossWeight.IsEmpty)
					{
						messageToReturn = entryReference + ":\n" + errorMessageForGrossWeight + SuffixForErrorMessageDoYowWantToCancelDeclaration;
					}
					if (!errorMessageForGrossWeightForVINs.IsEmpty)
					{
						messageToReturnVINs = entryReference + ":\n" + errorMessageForGrossWeightForVINs + SuffixForErrorMessageWhenGrossWeightForVins;
					}
				}
			}

			return (messageToReturn, messageToReturnVINs, dataToReserveGoods);
		}

		static ZString AdjustmentTransactionComment => Res.GetString("2E74C12E-6827-45DD-B4BB-0033822E5251", "Adjustment for Complementary Declaration");

		static ZString GetErrorMessageWhenRegLineItemIsNull(ZString entryReference, ZInt lineNo, ZString docRef)
		{
			return Res.GetString("54E9AE0B-02EC-4CB6-92FD-118B567BC0EF",
			"{0}: There is no item line {1} in the Temporary Storage for TSD Number {2}. Please, correct data and send again.",
				entryReference, lineNo, docRef);
		}

		static ZString GetErrorMessageWhenVINIsNotInAnyRegLine(ZString entryReference, ZString vin, ZString docRef, ZInt lineNo, bool isLAME)
		{
			return isLAME
					? Res.GetString("847C4D33-9463-4827-AE68-F6E0874D90A3",
					"{0}: VIN {1} is not present in the LAME under Reception Certificate Number {2}. Please, correct data and send again.",
					entryReference, vin, docRef)
					: Res.GetString("055C774B-2A47-40C3-9F9F-9877F5E5034E",
					"{0}: VIN {1} is not present in the Temporary Storage under TSD Number {2}, Item {3}. Please, correct data and send again.",
					entryReference, vin, docRef, lineNo);
		}

		static ZString GetErrorMessageWhenGrossWeightIsDifferentForVins(ZString entryReference, ZString docRef, ZInt lineNo, ZInt totalGrossWeight, ZInt grossWeightOBL, bool isLAME)
		{
			return isLAME
					? Res.GetString("B1C412F9-F75C-433F-B8F2-3289E121A326",
					"Gross weight {1} used for the declaration is different to the gross weight entered in the LAME {2} for Reception Certificate {3}.\n\n",
					entryReference, totalGrossWeight, grossWeightOBL, docRef)
					: Res.GetString("3921D2E6-0828-4152-B881-1FC4578A4F8A",
					"Gross weight {1} used for the declaration is different to the gross weight entered in the Temporary Storage {2} for TSD Number {3}, Item {4}.\nThis can cause mismatches in the stock at ES Customs records.\n\n",
					entryReference, totalGrossWeight, grossWeightOBL, docRef, lineNo);
		}

		static ZString GetErrorMessageWhenPackagesRemainingNotEnough(ZString entryReference, ZString docRef, ZInt lineNo, ZInt packQty, ZString packType, bool isLAME)
		{
			return isLAME
					? Res.GetString("160D5EC6-E4FC-4E83-97C9-DC1FB61C6DEE",
					"{0}: There is not enough quantity of goods in the LAME for Reception Certificate Number {1}: {2} {3}. Please, correct data and send again.",
					entryReference, docRef, packQty, packType)
					: Res.GetString("72C9227E-CDBF-42C2-8E57-C8F40BDF41C1",
					"{0}: There is not enough quantity of goods in the Temporary Storage for TSD Number {1}, Item {2}: {3} {4}. Please, correct data and send again.",
					entryReference, docRef, lineNo, packQty, packType);
		}

		static ZString GetErrorMessageWhenGrossWeightRemainingNotEnough(ZDecimal totalEntryGrossWeight, ZString docRef, ZInt lineNo, ZDecimal totalGrossWeightInRegLines, bool isLAME)
		{
			return isLAME
					? Res.GetString("2409F368-C8CD-4391-ACE9-C9FC8822FBC9",
					"There might not be enough Gross Weight {0} for Reception Certificate Number {1}.\nRemaining Gross Weight in the Temporary Storage: {2}\n\n",
					totalEntryGrossWeight, docRef, totalGrossWeightInRegLines)
					: Res.GetString("EC3CD016-DA02-446D-B0AF-5F9F98730947",
					"There might not be enough Gross Weight {0} for TSD Number {1}, Item {2}.\nRemaining Gross Weight in the Temporary Storage: {3}\n\n",
					totalEntryGrossWeight, docRef, lineNo, totalGrossWeightInRegLines);
		}

		static ZString SuffixForErrorMessageWhenGrossWeightForVins => Res.GetString("5BEA9FAE-BBA5-4F3F-AA36-54D00202016A", "Would you like to cancel this action and check the gross weight declared for the vehicles?");

		static ZString SuffixForErrorMessageDoYowWantToCancelDeclaration => Res.GetString("D29BD0B7-C43F-42A2-B3D2-015E47DE1444", "Do you want to cancel this declaration to check?");

		public class DataToReserveTSGoods
		{
			public ZDecimal TotalGrossWeight;
			public ZInt TotalPackQty;
			public ZInt TotalEntryPackQty;
			public ZBool IsAdjustment;
			public ICusTempStorageRegLine RegLine;
		}

		public class DeclarationDataToReserveTSGoods
		{
			public CusSupportingInfo Document;
			public ZDecimal TotalGrossWeight;
			public ZDecimal TotalGrossWeightForVINs;
			public IEnumerable<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)> Packages;
		}

		public class DataToUpdateLocationAndReference
		{
			public ZString Location;
			public ZBool EmptyLocation;
			public ZString Reference;
			public ZBool EmptyReference;
		}
	}
}
