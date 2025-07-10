using System;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class ARInvoiceNumberLengthConfigurationRegistryItem : StronglyTypedRegistryItem<int>
	{
		public ARInvoiceNumberLengthConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int defaultValue, int minValue, int maxValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ARInvoiceNumberLengthConfigurationRegistryDataType(minValue, maxValue), null, storage, options, defaultValue))
		{
		}
	}

#if DEBUG
	internal
#endif
	class ARInvoiceNumberLengthConfigurationRegistryDataType : IntRegistryDataType
	{
		public ARInvoiceNumberLengthConfigurationRegistryDataType(int lowerBound, int upperBound)
			: base(lowerBound, upperBound)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue >= 5 && proposedValue <= 7)
			{
				int maxLen = GetMaxARInvoiceNumberLength(companyPK, proposedValue);

				if (proposedValue < maxLen)
				{
					throw new RegistryValidationException(Res.GetString("ca8ac749-02e4-43b1-8704-6ffb8ba2967f",
						"You have transaction numbers in the database that exceed this length. The minimum number of digits allowed for this company is {0}.", maxLen));
				}
			}

			if (proposedValue >= 5 && proposedValue <= 8)
			{
				// -1 means it has never been used
				// 0 means that it has been used, but overridden has been unticked
				// > 0 means that it is currently being used
				var binaryValueLength = GetOverriddenStatusForNumberSequenceCustomisationRegistry(companyPK);

				if (binaryValueLength <= 0) // never used or overridden has been unticked
				{
					if (CheckDuplicationCollision(Env.NumberFountains.ARInvoiceNo, NumberFountainType.ARInvoice, proposedValue, companyPK))
					{
						throw new RegistryValidationException(Res.GetString("4ec3f054-a337-40ed-9f1a-5aeb1977c25b",
							"You have changed the AR invoice length to {0}. However, this will auto-generate a duplicate transaction number for this company that conflicts with the existing AR invoices in the system. You can either change the 'AR Invoice Number Length' or use the 'Number Sequence Customization' registry setting.",
							proposedValue));
					}
				}
			}
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			ObjectFactory.Get<IRegistryChangesNotifier>().Notify(Res.GetString("e7ec7ba5-3859-44b1-b803-9e0535a7fa78", "The changes will make effect after system restart"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int GetMaxARInvoiceNumberLength(Guid companyPK, int proposedValue)
		{
			string lengthCheck = string.Empty;
			switch (proposedValue)
			{
				case 5: lengthCheck = " AND LEN(AH_TransactionNUM) IN (6,7,8)"; break;
				case 6: lengthCheck = " AND LEN(AH_TransactionNUM) IN (7,8)"; break;
				case 7: lengthCheck = " AND LEN(AH_TransactionNUM) IN (8)"; break;
			}
			var sqlText = @"
				SELECT ISNULL(MAX(LEN(AH_TransactionNum)),0)
				FROM dbo.AccTransactionHeader
				WHERE AH_GC = @Company
				AND AH_Ledger = @AH_Ledger
				AND AH_TransactionType IN (@INV, @CRD, @ADJ)
				AND ISNUMERIC(AH_TransactionNUM) = 1"
				+ lengthCheck;

			using (var command = Db.Connection.Command(sqlText)) // Avoid use of factory to improve memory usage AND this is a very efficient query.
			{
				command.AddParameterBasedOnDbColumn("@Company", companyPK, AccTransactionHeaderSchema.AH_GC);
				command.AddParameterBasedOnDbColumn("@AH_Ledger", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				command.AddParameterBasedOnDbColumn("@INV", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				command.AddParameterBasedOnDbColumn("@CRD", TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);
				command.AddParameterBasedOnDbColumn("@ADJ", TransactionTypes.AdjustmentNote, AccTransactionHeaderSchema.AH_TransactionType);

				return (int)command.ExecuteScalar();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int GetOverriddenStatusForNumberSequenceCustomisationRegistry(Guid companyPK)
		{
			var sqlText = @"
			SELECT MAX(BinaryValueLength)
			FROM
			(
				SELECT -1 BinaryValueLength
			
				UNION ALL
			
				SELECT TOP 1 CASE WHEN LEN(SD_BinaryValue) > 0 THEN 1 ELSE 0 END BinaryValueLength
				FROM dbo.StmData
				WHERE SD_Name = 'TransactionNumberSequenceCustomisation'
				AND SD_Owner = @Company
			) InnerQuery";

			using (var command = Db.Connection.Command(sqlText)) // Avoid use of factory to improve memory usage AND this is a very efficient query.
			{
				command.AddParameterBasedOnDbColumn("@Company", companyPK, StmDataSchema.SD_Owner);
				return (int)command.ExecuteScalar();
			}
		}

		bool CheckDuplicationCollision(AccountingNumberFountainPooler pooler, NumberFountainType numberFountainType, int proposedValue, Guid companyPK)
		{
			var wrapper = new AccountingNumberFountainWrapper(pooler, numberFountainType, proposedValue);
			long minimumValue = wrapper.GetNumberFountain(ZDateTime.Now).PeekPreliminary(Db.Connection);
			return HasAnyDuplicateTransactions(companyPK, proposedValue, minimumValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool HasAnyDuplicateTransactions(Guid companyPK, int proposedValue, long min)
		{
			if (min < 0)
			{
				return false;
			}

			string minimumValueString = min.ToString(CultureInfo.InvariantCulture).PadLeft(proposedValue, '0');
			string maximumValueString = string.Empty.PadLeft(proposedValue, '9');

			var sqlText = @"
			IF EXISTS
			(
				SELECT 1
				FROM dbo.AccTransactionHeader
				WHERE AH_GC = @Company
				AND AH_Ledger = @AH_Ledger
				AND AH_TransactionType IN (@INV, @CRD, @ADJ)
				AND AH_TransactionNum BETWEEN @Min AND @Max
				AND ISNUMERIC(AH_TransactionNum) = 1
				AND LEN(AH_TransactionNum) = @NumDigits
			)
				SELECT 1 AS IsExists
			ELSE
				SELECT 0 AS IsExists";

			using (var command = Db.Connection.Command(sqlText)) // Avoid use of factory to improve memory usage AND this is a very efficient query.
			{
				command.AddParameterBasedOnDbColumn("@Company", companyPK, AccTransactionHeaderSchema.AH_GC);
				command.AddParameterBasedOnDbColumn("@AH_Ledger", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				command.AddParameterBasedOnDbColumn("@INV", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				command.AddParameterBasedOnDbColumn("@CRD", TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);
				command.AddParameterBasedOnDbColumn("@ADJ", TransactionTypes.AdjustmentNote, AccTransactionHeaderSchema.AH_TransactionType);
				command.AddParameter("@NumDigits", SqlDbType.Int, proposedValue);
				command.AddParameterBasedOnDbColumn("@Min", minimumValueString, AccTransactionHeaderSchema.AH_TransactionNum);
				command.AddParameterBasedOnDbColumn("@Max", maximumValueString, AccTransactionHeaderSchema.AH_TransactionNum);

				return (int)command.ExecuteScalar() > 0;
			}
		}
	}
}
