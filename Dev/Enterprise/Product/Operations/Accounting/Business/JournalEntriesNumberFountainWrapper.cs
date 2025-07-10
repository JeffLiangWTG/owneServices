using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	public class JournalEntriesNumberFountainWrapper : AccountingNumberFountainWrapper
	{
		public JournalEntriesNumberFountainWrapper(AccountingNumberFountainPooler numberFountainPooler, ZString numberFountainKey, ZString groupCode)
			: base(numberFountainPooler, NumberFountainType.None)
		{
			journalEntriesNumberFountainKey = numberFountainKey;
			journalEntriesClassificationGroupCode = groupCode;
		}

		readonly ZString journalEntriesNumberFountainKey;
		readonly ZString journalEntriesClassificationGroupCode;

		protected override string GenerateCore(IAccountingNumberFountainDataSource dataSource)
		{
			var companyPk = dataSource.Branch.GB_GC.ToGuid();
			var journalEntriesNumberCustomisationRegistry = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			var customisationCollection = journalEntriesNumberCustomisationRegistry.NumberSequenceCustomisations;

			return GenerateNumber(customisationCollection, journalEntriesNumberFountainKey, dataSource, companyPk);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "This method should not be split.")]
		protected override string GetValue(TransactionNumberSequenceCustomisation element, long seed, IAccountingNumberFountainDataSource dataSource)
		{
			var result = base.GetValue(element, seed, dataSource);
			if (!result.IsNullOrEmpty())
			{
				return result;
			}

			var branch = dataSource.Branch;
			var department = dataSource.Department;
			var postDate = dataSource.PostDate;

			switch (element.ElementName)
			{
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesBranchCode:
					result = branch.GB_Code.SubstringSafe(0, ZInt.ParseSafe(element.Code, 3));
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesDepartmentCode:
					result = department.GE_Code.SubstringSafe(0, ZInt.ParseSafe(element.Code, 3));
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsDigits:
					result = GetDetailFromCode(postDate.Year, element.Code);
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarYearAsLetter:
					result = GetLetterFromYear(postDate.Year);
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits:
					result = postDate.Month.ToString("00", CultureInfo.InvariantCulture);
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAsLetter:
					result = "ABCDEFGHIJKL"[postDate.Month - 1].ToString(CultureInfo.InvariantCulture);
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCode:
					result = journalEntriesClassificationGroupCode;
					break;
				case JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.JournalEntriesClassificationGroupCodePrefix:
					result = GetJournalEntriesClassificationGroupCodePrefix(dataSource);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix:
					result = GetTransactionTypePrefix(dataSource);
					break;
			}

			return result;
		}

		ZString GetTransactionTypePrefix(IAccountingNumberFountainDataSource dataSource)
		{
			var result = ZString.Empty;

			 if (dataSource is GeneralLedgerCombinedDataSource numberFountainDataSource)
			{
				var transactionTypePrefixCollection = AccountingConfigurationRegistry.Instance.TransactionTypePrefix.GetFallBackValueAtAllLevels(dataSource.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).Cast<TransactionTypePrefix>();
				var macthedPrefix = transactionTypePrefixCollection.FirstOrDefault(x => x.Ledger == numberFountainDataSource.Ledger && x.TransactionType == numberFountainDataSource.TransactionType);
				if (macthedPrefix != null)
				{
					result = macthedPrefix.Prefix;
				}
			}

			return result;
		}

		ZString GetJournalEntriesClassificationGroupCodePrefix(IAccountingNumberFountainDataSource dataSource)
		{
			var result = ZString.Empty;

			if (!journalEntriesClassificationGroupCode.IsEmpty)
			{
				var groupCollection = AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.GetFallBackValueAtAllLevels(dataSource.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
				var groupCode = groupCollection.Cast<JournalEntriesClassificationGroupCode>().FirstOrDefault(x => x.Code == journalEntriesClassificationGroupCode);
				if (groupCode != null)
				{
					result = groupCode.Prefix;
				}
			}

			return result;
		}

		protected override bool IsFountainMandatory(TransactionNumberSequenceCustomisation element, IAccountingNumberFountainDataSource dataSource)
		{
			var companyPk = dataSource.Branch.GB_GC.ToGuid();
			var journalEntriesNumberCustomisationRegistry = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			var sequenceResetOption = journalEntriesNumberCustomisationRegistry.SequenceResetOption;
			return element.ElementName == TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits || (sequenceResetOption == AccountingConstants.JournalEntriesNumberCustomisationSequenceResetOption.MONTH && element.ElementName == JournalEntriesNumberCustomisation.JournalEntriesNumberCustomisationElementNames.CalendarMonthAs2Digits);
		}
	}
}
