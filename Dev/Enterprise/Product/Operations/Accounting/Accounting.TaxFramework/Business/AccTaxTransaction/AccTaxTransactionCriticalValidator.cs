using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IAccTaxTransactionCriticalValidator
	{
		CriticalValidationResult CheckConsistencyWithPivotsData();
		CriticalValidationResult CheckLinkedGLMovements();
		CriticalValidationResult CheckAmounts();
		CriticalValidationResult CheckCancelledNonSPRTransactionRealisationDate();
		CriticalValidationResult CheckHasSkippedDataRefreshBusUpdate();
	}

	public class AccTaxTransactionCriticalValidator : IAccTaxTransactionCriticalValidator
	{
		public AccTaxTransactionCriticalValidator(AccTaxTransaction parent)
		{
			Parent = parent;
		}

		AccTaxTransaction Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		protected AccTaxTransaction parent;

		CriticalValidationResult IAccTaxTransactionCriticalValidator.CheckConsistencyWithPivotsData()
		{
			if (!Parent.IsInDatabase)
			{
				var pivots = Parent.Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, Parent.PK) { FetchOnlyFromLocalCache = true });
				if (pivots.IsNullOrEmpty())
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TaxTransactionWithoutPivots, CriticalValidationMessageTemplate.TaxTransactionWithoutPivots, GetAllPropertyValuesWithPivots(pivots));
				}

				if (pivots.Sum(p => p.ATP_LocalTaxAmount) != Parent.ATT_LocalTaxAmount)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TaxTransactionWithSumOfPivotsLocalTaxAmountsMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSumOfPivotsLocalTaxAmountsMismatch, GetAllPropertyValuesWithPivots(pivots));
				}

				if (pivots.Any(p => p.ATP_IsTaxExpense == Parent.ATT_AG_TaxExpenseAccount.IsEmpty))
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, GetAllPropertyValuesWithPivots(pivots));
				}
			}
			else
			{
				if (Parent.ATT_LocalTaxAmountInfo.HasChanges)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TaxTransactionWithLocalTaxAmountChangedAfterSaving, CriticalValidationMessageTemplate.TaxTransactionWithLocalTaxAmountChangedAfterSaving, GetAllPropertyValuesWithPivots(null));
				}
				if (Parent.ATT_AG_TaxExpenseAccountInfo.HasChanges)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TaxTransactionWithTaxExpenseDataChangedAfterSaving, CriticalValidationMessageTemplate.TaxTransactionWithTaxExpenseDataChangedAfterSaving, GetAllPropertyValuesWithPivots(null));
				}
			}

			return null;
		}

		ZString GetAllPropertyValuesWithPivots(AccTaxRecordTransactionLinePivot[] pivots)
		{
			var message = new ZStringBuilder();
			message.Append(Parent.GetAllPropertyValues());

			if (pivots != null)
			{
				foreach (var pivot in pivots)
				{
					message.Append(pivot.GetAllPropertyValues());
				}
			}

			return message.ToStringWithNewLineBetweenAppends();
		}

		CriticalValidationResult IAccTaxTransactionCriticalValidator.CheckLinkedGLMovements()
		{
			if (Parent.ATT_LocalTaxAmount == 0)
			{
				if (GetGLMovementRecords(null).Any())
				{
					return new CriticalValidationResult(CriticalValidationErrorType.GLMovementExistsForLocalTaxAmountZero, CriticalValidationMessageTemplate.GLMovementExistsForLocalTaxAmountZeroErrorMessage, Parent.GetAllPropertyValues());
				}
			}
			else
			{
				if (Parent.ATT_Basis == TaxBasisList.Matching.Code)
				{
					if (!Parent.IsInDatabase)
					{
						if (!GetGLMovementRecords(TaxGLMovementTypeList.Pending.Code).Any())
						{
							return new CriticalValidationResult(CriticalValidationErrorType.GLMovementsRecordsNotFound, CriticalValidationMessageTemplate.GetMissingGLMovementsRecordErrorMessage(TaxBasisList.Matching.Code, TaxGLMovementTypeList.Pending.Code), Parent.GetAllPropertyValues());
						}
					}
					if (!Parent.ATT_RealisationDate.IsEmpty && (!Parent.IsInDatabase || Parent.ATT_RealisationDateInfo.HasChanges))
					{
						if (!GetGLMovementRecords(TaxGLMovementTypeList.Realised.Code).Any())
						{
							return new CriticalValidationResult(CriticalValidationErrorType.GLMovementsRecordsNotFound, CriticalValidationMessageTemplate.GetMissingGLMovementsRecordErrorMessage(TaxBasisList.Matching.Code, TaxGLMovementTypeList.Realised.Code), Parent.GetAllPropertyValues());
						}
					}
				}
				else if (Parent.ATT_Basis == TaxBasisList.Posting.Code)
				{
					if (!Parent.IsInDatabase && !GetGLMovementRecords(TaxGLMovementTypeList.Normal.Code).Any())
					{
						return new CriticalValidationResult(CriticalValidationErrorType.GLMovementsRecordsNotFound, CriticalValidationMessageTemplate.GetMissingGLMovementsRecordErrorMessage(TaxBasisList.Posting.Code, TaxGLMovementTypeList.Normal.Code), Parent.GetAllPropertyValues());
					}
				}
				else if (Parent.ATT_Basis == TaxBasisList.PostingOnMatching.Code)
				{
					if (!Parent.IsInDatabase || Parent.ATT_RealisationDateInfo.HasChanges)
					{
						if (Parent.ATT_RealisationDate.IsEmpty)
						{
							if (GetGLMovementRecords(null).Any())
							{
								return new CriticalValidationResult(CriticalValidationErrorType.InvalidGLMovementForNotionalTax, CriticalValidationMessageTemplate.GetInvalidGLMovementForNotionalTaxErrorMessage, Parent.GetAllPropertyValues());
							}
						}
						else if (!GetGLMovementRecords(TaxGLMovementTypeList.Normal.Code).Any())
						{
							return new CriticalValidationResult(CriticalValidationErrorType.GLMovementsRecordsNotFound, CriticalValidationMessageTemplate.GetMissingGLMovementsRecordErrorMessage(TaxBasisList.PostingOnMatching.Code, TaxGLMovementTypeList.Normal.Code), Parent.GetAllPropertyValues());
						}
					}
				}
				else
				{
					return new CriticalValidationResult(CriticalValidationErrorType.InvalidTaxTransactionBasis, CriticalValidationMessageTemplate.GetInvalidTaxTransactionBasisErrorMessage(Parent.ATT_Basis), Parent.GetAllPropertyValues());
				}
			}
			return null;

			AccTaxGLMovement[] GetGLMovementRecords(string glMovementType)
			{
				var query = new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, Parent.PK);
				if (!glMovementType.IsNullOrEmpty())
				{
					query.AddToFilter(AccTaxGLMovementSchema.ATM_Type, glMovementType);
				}
				query.FetchOnlyFromLocalCache = true;
				return Parent.Factory.Load<AccTaxGLMovement>(query);
			}
		}

		CriticalValidationResult IAccTaxTransactionCriticalValidator.CheckAmounts()
		{
			var osBaseAmt = Parent.ATT_OSTaxBaseAmount;
			var localBaseAmt = Parent.ATT_LocalTaxBaseAmount;
			var osTaxAmt = Parent.ATT_OSTaxAmount;
			var localTaxAmt = Parent.ATT_LocalTaxAmount;

			if (osBaseAmt == 0 && localBaseAmt == 0 && osTaxAmt == 0 && localTaxAmt == 0 && !Parent.ATT_IsCancelled)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.TaxTransactionShouldBeCancelledIfAllAmountsAreZero, CriticalValidationMessageTemplate.TaxTransactionShouldBeCancelledIfAllAmountsAreZeroErrorMessage, Parent.GetAllPropertyValues());
			}

			return null;
		}

		CriticalValidationResult IAccTaxTransactionCriticalValidator.CheckCancelledNonSPRTransactionRealisationDate()
		{
			if (Parent.ATT_Basis != TaxBasisList.PostingOnMatching.Code)
			{
				if ((!Parent.IsInDatabase || Parent.ATT_IsCancelledInfo.HasChanges) && Parent.ATT_IsCancelled)
				{
					if (Parent.ATT_RealisationDate.IsEmpty)
					{
						return new CriticalValidationResult(CriticalValidationErrorType.CancelledTaxTransactionRealisationDateShouldNotBeEmpty, CriticalValidationMessageTemplate.CancelledTaxTransactionRealisationDateShouldNotBeEmpty, Parent.GetAllPropertyValues());
					}
				}
			}

			return null;
		}

		CriticalValidationResult IAccTaxTransactionCriticalValidator.CheckHasSkippedDataRefreshBusUpdate()
		{
			if (ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().HasSkippedDataRefreshBusUpdate(Parent))
			{
				return new CriticalValidationResult(CriticalValidationErrorType.TaxRecordSkippedDataRefreshBusUpdateButWasSavedSuccessfully, CriticalValidationMessageTemplate.TaxRecordSkippedDataRefreshBusUpdateButWasSavedSuccessfully, Parent.GetAllPropertyValues());
			}

			return null;
		}
	}
}
