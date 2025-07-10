using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class GeneralLedgerCombinedDataSource :  IAccountingNumberFountainDataSource
	{
		public GeneralLedgerCombinedDataSource(string journalNumber, string journalRuleCode)
		{
			JournalNumber = journalNumber;
			JournalRuleCode = journalRuleCode;
		}

		public GeneralLedgerCombinedDataSource(string ledger, string transactionType, string transactionNum, string journalNumber, string journalRuleCode)
		{
			Ledger = ledger;
			TransactionType = transactionType;
			TransactionNum = transactionNum;
			JournalNumber = journalNumber;
			JournalRuleCode = journalRuleCode;
		}

		public GeneralLedgerCombinedDataSource(Guid pk, ZDateTime postDate, string ledger, string transactionType, string transactionNum, Guid branchPk, Guid departmentPk, Guid transactionHeaderPk, Guid taxGlMovement, int sequence, BusinessObjectFactory factory)
		{
			Pk = pk;
			PostDate = postDate;
			Ledger = ledger;
			TransactionType = transactionType;
			TransactionNum = transactionNum;
			BranchPk = branchPk;
			DepartmentPk = departmentPk;
			TransactionHeaderPk = transactionHeaderPk;
			TaxGLMovement = taxGlMovement;
			Sequence = sequence;
			Factory = factory;
		}

		public string UniqueKey => Ledger + "_" + TransactionType + "_" + TransactionNum;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => PostDate;

		GlbBranch IAccountingNumberFountainDataSource.Branch => Factory.Load<GlbBranch>(BranchPk);

		GlbDepartment IAccountingNumberFountainDataSource.Department => Factory.Load<GlbDepartment>(DepartmentPk);

		public readonly Guid Pk;
		public readonly ZDateTime PostDate;
		public readonly Guid BranchPk;
		public readonly Guid DepartmentPk;
		public readonly string Ledger;
		public readonly string TransactionType;
		public readonly string TransactionNum;
		public readonly Guid TransactionHeaderPk;
		public readonly Guid TaxGLMovement;
		public readonly int Sequence;
		public readonly BusinessObjectFactory Factory;
		public string JournalNumber { get; set; }
		public string JournalRuleCode { get; set; }
	}
}
