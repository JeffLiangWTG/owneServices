using System;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public enum GLMovementDateSource
	{
		TaxRecordPostDate,
		TaxRecordRealisationDate
	}

	public interface IGLMovementCreator
	{
		void CreateNormalRecord(AccTaxTransaction taxRecord, GLMovementDateSource dateSource);
		void CreatePendingRecord(AccTaxTransaction taxRecord);
		void CreateRealisedRecord(AccTaxTransaction taxRecord);
	}

	class GLMovementCreator : IGLMovementCreator
	{
		void IGLMovementCreator.CreateNormalRecord(AccTaxTransaction taxRecord, GLMovementDateSource dateSource) => CreateNormalRecord(taxRecord, dateSource);
		void IGLMovementCreator.CreatePendingRecord(AccTaxTransaction taxRecord) => CreatePendingRecord(taxRecord);
		void IGLMovementCreator.CreateRealisedRecord(AccTaxTransaction taxRecord) => CreateRealisedRecord(taxRecord);

		static void CreateNormalRecord(AccTaxTransaction taxRecord, GLMovementDateSource dateSource)
		{
			MustBeEmpty(taxRecord.ATT_AG_TaxPendingControlAccount, taxRecord.TaxConfiguration.ETC_Code);

			var glMovement = CreateGLMovementRecord(taxRecord, TaxGLMovementTypeList.Normal.Code, dateSource);
			if (taxRecord.ATT_AG_LedgerControlAccount.IsValid)
			{
				SetRequiredGLMovementAccount(glMovement, taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_AG_LedgerControlAccount);
			}
			if (taxRecord.ATT_AG_TaxExpenseAccount.IsValid)
			{
				SetRequiredGLMovementAccount(glMovement, -taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_AG_TaxExpenseAccount);
			}
			if (taxRecord.ATT_AG_TaxControlAccount.IsValid)
			{
				var taxControlMultiplier = taxRecord.ATT_AG_LedgerControlAccount.IsEmpty ? 1 : -1;
				SetRequiredGLMovementAccount(glMovement, taxRecord.ATT_LocalTaxAmount * taxControlMultiplier, taxRecord.ATT_AG_TaxControlAccount);
			}
		}

		static void CreatePendingRecord(AccTaxTransaction taxRecord)
		{
			MustBeEmpty(taxRecord.ATT_AG_TaxExpenseAccount, taxRecord.TaxConfiguration.ETC_Code);

			var glMovement = CreateGLMovementRecord(taxRecord, TaxGLMovementTypeList.Pending.Code, GLMovementDateSource.TaxRecordPostDate);
			if (taxRecord.ATT_AG_LedgerControlAccount.IsValid)
			{
				SetRequiredGLMovementAccount(glMovement, taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_AG_LedgerControlAccount);
			}
			if (taxRecord.ATT_AG_TaxPendingControlAccount.IsValid)
			{
				SetRequiredGLMovementAccount(glMovement, -taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_AG_TaxPendingControlAccount);
			}
		}

		static void CreateRealisedRecord(AccTaxTransaction taxRecord)
		{
			MustBeEmpty(taxRecord.ATT_AG_TaxExpenseAccount, taxRecord.TaxConfiguration.ETC_Code);

			var glMovement = CreateGLMovementRecord(taxRecord, TaxGLMovementTypeList.Realised.Code, GLMovementDateSource.TaxRecordRealisationDate);
			if (taxRecord.ATT_AG_TaxPendingControlAccount.IsValid)
			{
				SetRequiredGLMovementAccount(glMovement, taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_AG_TaxPendingControlAccount);
			}
			if (taxRecord.ATT_AG_TaxControlAccount.IsValid)
			{
				SetRequiredGLMovementAccount(glMovement, -taxRecord.ATT_LocalTaxAmount, taxRecord.ATT_AG_TaxControlAccount);
			}
		}

		static AccTaxGLMovement CreateGLMovementRecord(AccTaxTransaction taxRecord, ZString movementType, GLMovementDateSource dateSource)
		{
			var glMovement = taxRecord.Factory.New<AccTaxGLMovement>();
			glMovement.ATM_ATT_TaxTransaction = taxRecord.PK;
			glMovement.ATM_Type = movementType;
			SetGLMovementDate(taxRecord, dateSource, glMovement);
			glMovement.ATM_Amount = Math.Abs(taxRecord.ATT_LocalTaxAmount);

			return glMovement;
		}

		static void SetGLMovementDate(AccTaxTransaction taxRecord, GLMovementDateSource dateSource, AccTaxGLMovement glMovement)
		{
			switch (dateSource)
			{
				case GLMovementDateSource.TaxRecordPostDate:
					{
						glMovement.ATM_Date = taxRecord.ATT_PostDate;
						break;
					}
				case GLMovementDateSource.TaxRecordRealisationDate:
					{
						glMovement.ATM_Date = taxRecord.ATT_RealisationDate;
						break;
					}
				default:
					{
						throw new ArgumentException(FormattableString.Invariant($"'{dateSource}' is invalid value."), nameof(dateSource));
					}
			}
		}

		static void SetRequiredGLMovementAccount(AccTaxGLMovement glMovement, ZDecimal amountToDefineAccountBasedOn, ZGuid glAccountPK)
		{
			var taxConfigCode = glMovement.TaxTransaction.TaxConfiguration.ETC_Code;
			if (amountToDefineAccountBasedOn > 0)
			{
				MustBeEmpty(glMovement.ATM_AG_DebitAccount, taxConfigCode);
				glMovement.ATM_AG_DebitAccount = glAccountPK;
			}
			else
			{
				MustBeEmpty(glMovement.ATM_AG_CreditAccount, taxConfigCode);
				glMovement.ATM_AG_CreditAccount = glAccountPK;
			}
		}

		static void MustBeEmpty(ZGuid accountPK, string taxConfigCode)
		{
			if (accountPK.IsValid)
			{
				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("79A3B243-E233-478F-9143-2AFC2EFBD61A", "'{0}' tax configuration has conflicting combination of GL Accounts.", taxConfigCode));
			}
		}
	}
}
