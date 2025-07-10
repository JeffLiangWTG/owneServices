using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class WriteOffTransactionCreator
	{
		const string CharInGuaranteeSeventhPosL = "L";

		public ZString AddGuaranteesWriteOffTransactionsEntryHeader(CusEntryHeader entryHeader, LoggingInformation logger = null)
		{
			var hasPositiveBalance = false;
			var hasCreatedTransaction = false;

			var guaranteesInDeclaration = entryHeader.Declaration.Guarantees.Cast<ESGuarantee>().Where(x => x.EntryInstruction == entryHeader.EntryInstruction && x.PW_BondNumber.Substring(6, 1) == CharInGuaranteeSeventhPosL);

			foreach (ESGuarantee guarantee in guaranteesInDeclaration)
			{
				var reference = guarantee.PW_BondNumber;

				(hasPositiveBalance, hasCreatedTransaction) = AddGuaranteeWriteOffTransaction(entryHeader.Factory, entryHeader.MovementReferenceNumber, entryHeader.CH_BGMReference, false, EUGuaranteeTypeList.Codes.IMP,
																							reference, entryHeader.CountryCode, ZDateTime.Empty, hasPositiveBalance, hasCreatedTransaction, logger);
			}

			return hasPositiveBalance ? NotWrittenOffText : (hasCreatedTransaction ? WrittenOffText : ExcludedText);
		}

		protected (ZBool posBal, ZBool newTransac) AddGuaranteeWriteOffTransaction(BusinessObjectFactory factory, ZString mrn, ZString boReference, ZBool isNcts, ZString guaranteeType, ZString guaranteeReference, ZString countryCode, ZDateTime admissionDate, ZBool hasPositiveBalance, ZBool hasCreatedTransaction, LoggingInformation logger = null)
		{
			var guaranteeHeader = CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(factory, guaranteeReference, countryCode, guaranteeType);
			var isValidDate = guaranteeHeader != null && (guaranteeHeader.CPH_EndDate.IsEmpty || guaranteeHeader.CPH_EndDate > ZDate.Today);
			if (isValidDate)
			{
				var transactionsAmount = GetTransactionsAmount(guaranteeHeader, mrn, boReference);
				if (transactionsAmount > 0)
				{
					hasPositiveBalance = true;
					if (logger != null)
					{
						logger.LogError($"Reference {guaranteeReference} has a positive balance of {transactionsAmount.Round(2)} EUR. " +
							$"Please check the existing transactions for this reference and create a manual adjustment if needed.");
					}
				}
				else if (transactionsAmount < 0)
				{
					var tranValue = Math.Abs(transactionsAmount);
					AddGuaranteeTransaction(mrn, boReference, isNcts, guaranteeHeader, tranValue, admissionDate);
					hasCreatedTransaction = true;
				}
			}

			return (hasPositiveBalance, hasCreatedTransaction);
		}

		void AddGuaranteeTransaction(ZString mrn, ZString boReference, ZBool isNcts, CusGuaranteeHeader guaranteeHeader, decimal tranValue, ZDateTime admissionDate)
		{
			var tranDate = admissionDate.IsEmpty ? ZDateTime.Now : admissionDate;
			var transactionCommentSuffix = isNcts ? (NoResString)"NCTS Departure " : string.Empty;

			guaranteeHeader.AddWriteOffTransaction(mrn, tranValue, tranDate, transactionCommentSuffix + boReference);
		}

		ZDecimal GetTransactionsAmount(CusGuaranteeHeader guaranteeHeader, ZString mrn, ZString boReference)
		{
			var permitLinesTransactions = guaranteeHeader.GetTransactions()?.Cast<SharedCusPermitLineTransaction>();
			var pendingAmount = permitLinesTransactions?.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed &&
															  (x.CPL_Reference == mrn || x.CPL_Reference == boReference)).Sum(x => x.CPL_TranValue) ?? ZDecimal.Zero;
			return pendingAmount;
		}

		protected ZString NotWrittenOffText => Res.GetString("51877487-891B-433E-B5CB-C613B296D323", "Not Written Off (at least one positive balance)");
		protected ZString WrittenOffText => Res.GetString("61DB79F8-38A3-47AE-B4BC-75ED52C644E7", "Written Off");
		protected ZString ExcludedText => Res.GetString("6CC9F55A-0833-484B-9DEF-BB9C15665E4D", "Excluded (no pending debt)");
	}
}
