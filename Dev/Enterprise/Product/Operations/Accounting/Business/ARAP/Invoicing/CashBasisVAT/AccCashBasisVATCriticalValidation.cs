using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class AccCashBasisVATCriticalValidation : CriticalValidation<AccCashBasisVAT>, IClearCacheProvider
	{
		public AccCashBasisVATCriticalValidation(AccCashBasisVAT parent)
			: base(parent)
		{
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			if (Parent.IsInDatabase && Parent.HasChanges)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognition_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognition_AlreadySavedErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (Parent.YC_GC != GlbCompany.CurrentCompany.PK)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognition_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognition_CurrentCompanyErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (Parent.TransactionLine == null)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognition_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognition_TransactionLineErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			var parentTransactionLine = Parent.TransactionLine;

			if (parentTransactionLine.AL_GSTVATBasis != AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTaxRecord_NotCashErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (parentTransactionLine.AL_AT.IsEmpty)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTaxRecord_NoTaxIDErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (Parent.YC_PostDate.IsEmpty)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectPostDate_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectPostDateErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (Parent.YC_TaxBaseAmount == 0)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTaxRecord_ZeroTaxBasisErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (Parent.YC_TaxAmount == 0 && parentTransactionLine.AL_GSTVAT != 0)
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTaxRecord_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTaxRecord_NotZeroRelatedLineTaxAmountErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			if (Parent.YC_TaxAmount != 0 && Math.Sign(Parent.YC_TaxBaseAmount) != Math.Sign(Parent.YC_TaxAmount))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithDifferentSigns_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithDifferentSignsErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			var transactionPK = parentTransactionLine.AL_AH;
			var cashBasisVATsByTransactionPK = GetCachedValue(Parent.Factory);
			if (!cashBasisVATsByTransactionPK.TryGetValue(transactionPK, out var cashBasisVATsInCacheAndDB))
			{
				var lineQuery = new ZQuery(AccTransactionLinesSchema.AL_AH, transactionPK);
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, parentTransactionLine.AL_GC);
				lineQuery.FetchOnlyFromLocalCache = true;
				var transactionLinesInLocalCache = Parent.Factory.Load<AccTransactionLines>(lineQuery);

				var cashBasisVATsInLocalCacheQuery = new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, transactionLinesInLocalCache.Select(line => line.PK)) { FetchOnlyFromLocalCache = true };
				var cashBasisVATsInLocalCache = Parent.Factory.Load<AccCashBasisVAT>(cashBasisVATsInLocalCacheQuery);

				cashBasisVATsByTransactionPK[transactionPK] = cashBasisVATsInCacheAndDB = Parent.Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, cashBasisVATsInLocalCache.Select(cashBasisVAT => cashBasisVAT.YC_AL_TransactionLine)));
			}

			var allCashBasisVATsForTransactionLine = cashBasisVATsInCacheAndDB.Where(cashBasisVAT => cashBasisVAT.YC_AL_TransactionLine == Parent.YC_AL_TransactionLine);

			var matchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, Parent.YC_MatchGroupNum);
			matchLinkFilter.AddToFilter(AccTransactionMatchLinkSchema.AP_AH, transactionPK);
			var matchLinks = Parent.YC_MatchGroupNum.IsEmpty ? Array.Empty<TransactionMatchLink>() : Parent.Factory.Load<TransactionMatchLink>(matchLinkFilter);

			var oppositeTaxRecords = from taxRecord in allCashBasisVATsForTransactionLine
									 where taxRecord.PK != Parent.PK &&
																	taxRecord.YC_MatchGroupNum == Parent.YC_MatchGroupNum &&
																	taxRecord.YC_TaxBaseAmount == -Parent.YC_TaxBaseAmount &&
																	taxRecord.YC_TaxAmount == -Parent.YC_TaxAmount
															 select taxRecord;

			if (Math.Sign(parentTransactionLine.AL_LineAmount) == Math.Sign(Parent.YC_TaxBaseAmount) &&
				Math.Sign(parentTransactionLine.AL_GSTVAT) == Math.Sign(Parent.YC_TaxAmount))
			{
				if (Parent.YC_MatchGroupNum.IsEmpty)
				{
					yield return CheckAmountsIsFullAndInvoiceIsZeroValue(parentTransactionLine, false);

					var parentTransactionHeader = parentTransactionLine.TransactionHeader;
					if (Parent.YC_PostDate != parentTransactionHeader.AH_FullyPaidDate)
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectPostDate_1,
							CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectPostDate_PaidDateErrorMessage,
							Parent.GetCashBasisVATInfo());
					}
				}
				else
				{
					if (matchLinks.Any())
					{
						var matchLink = matchLinks.First();
						if (Parent.YC_PostDate != matchLink.AP_MatchDate)
						{
							yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectPostDate_1,
								CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectPostDate_MatchDateErrorMessage,
								Parent.GetCashBasisVATInfo());
						}
					}
					else
					{
						var isReveresedTaxRecordExist = oppositeTaxRecords.Any();
						if (!isReveresedTaxRecordExist)
						{
							yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognition_1,
								CriticalValidationMessageTemplate.CashBasisTaxRecognition_MatchGroupNumberErrorMessage,
								Parent.GetCashBasisVATInfo());
						}
					}
				}
			}
			else
			{
				if (Parent.YC_MatchGroupNum.IsEmpty)
				{
					yield return CheckAmountsIsFullAndInvoiceIsZeroValue(parentTransactionLine, true);
				}

				var isOriginalTaxRecordExist = oppositeTaxRecords.Any();
				if (!isOriginalTaxRecordExist)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithOppositeToLineSigns_1,
						CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithOppositeToLineSigns_ExistingRecordErrorMessage,
						Parent.GetCashBasisVATInfo());
				}

				if (matchLinks.Any())
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithOppositeToLineSigns_1,
						CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithOppositeToLineSigns_unmatchingErrorMessage,
						Parent.GetCashBasisVATInfo());
				}
			}

			decimal taxBasisTotal = 0;
			decimal taxTotal = 0;
			foreach (var taxRecord in allCashBasisVATsForTransactionLine)
			{
				taxBasisTotal += taxRecord.YC_TaxBaseAmount;
				taxTotal += taxRecord.YC_TaxAmount;
			}

			if (Math.Abs(parentTransactionLine.AL_LineAmount) < Math.Abs(taxBasisTotal))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTotal_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTotal_ExceedLinedLineAmountErrorMessage,
					Parent.GetCashBasisVATInfo());
			}
			if (taxBasisTotal != 0 && Math.Sign(parentTransactionLine.AL_LineAmount) != Math.Sign(taxBasisTotal)) //this is for consitnacy only as code can't go here because above we check we can reverse only an existing tax record with the same amounts
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTotal_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTotal_SignLinkedLineAmountErrorMessage,
					Parent.GetCashBasisVATInfo());
			}
			if (Math.Abs(parentTransactionLine.AL_GSTVAT) < Math.Abs(taxTotal))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTotal_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTotal_OriginalTaxAmountErrorMessage,
					Parent.GetCashBasisVATInfo());
			}
			if (taxTotal != 0 && Math.Sign(parentTransactionLine.AL_GSTVAT) != Math.Sign(taxTotal)) //this is for consitnacy only as code can't go here because above we check we can reverse only an existing tax record with the same amounts
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithIncorrectTotal_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithIncorrectTotal_LinkedLineTaxAmountErrorMessage,
					Parent.GetCashBasisVATInfo());
			}
		}

		CriticalValidationResult CheckAmountsIsFullAndInvoiceIsZeroValue(AccTransactionLines parentTransactionLine, bool oppositeSigns)
		{
			int multiplier = oppositeSigns ? -1 : 1;
			if (parentTransactionLine.AL_LineAmount != multiplier * Parent.YC_TaxBaseAmount ||
				parentTransactionLine.AL_GSTVAT != multiplier * Parent.YC_TaxAmount)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithEmptyMatchGroupNumber_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithEmptyMatchGroupNumber_ZeroValueTransactionFullyPaidErrorMessage,
					Parent.GetCashBasisVATInfo());
			}
			var parentTransactionHeader = parentTransactionLine.TransactionHeader;
			if (parentTransactionHeader.AH_LocalTotal != 0)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.CashBasisTaxRecognitionWithEmptyMatchGroupNumber_1,
					CriticalValidationMessageTemplate.CashBasisTaxRecognitionWithEmptyMatchGroupNumber_ZeroValueTransactionErrorMessage,
					Parent.GetCashBasisVATInfo());
			}

			return new CriticalValidationResult();
		}

		ClearCacheDelegate IClearCacheProvider.GetClearCacheDelegate() => ClearCache;

		static Dictionary<ZGuid, AccCashBasisVAT[]> GetCachedValue(BusinessObjectFactory factory) => factory.GetCachedValue(cacheKey, () => new Dictionary<ZGuid, AccCashBasisVAT[]>());

		static void ClearCache(BusinessObjectFactory factory) => factory.ClearCachedValue<Dictionary<ZGuid, AccCashBasisVAT[]>>(cacheKey);

		const string cacheKey = "CashBasisVATTaxRecordsDataLoadedInCriticalValidation";
	}
}
