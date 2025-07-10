using System;
using System.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class TaxGLMovementGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var taxTransaction = GetTaxTransaction((Guid)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.ATM_ATT_TaxTransaction]);
			var localAmount = (decimal)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.ATM_Amount];
			var osAmount = Math.Abs(taxTransaction.ATT_OSTaxAmount);
			var postDate = (DateTime)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.ATM_Date];

			var dRLine = CreateAndPopulateDebitCreditLine((Guid)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.ATM_AG_DebitAccount], GLDAccountTypes.GLMovementDebitAccount, localAmount, osAmount, postDate, AccountingConstants.GLDTypeCodes.RealizeTaxGLMovement);
			var cRLine = CreateAndPopulateDebitCreditLine((Guid)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.ATM_AG_CreditAccount], GLDAccountTypes.GLMovementCreditAccount, localAmount * (-1), osAmount * (-1), postDate, AccountingConstants.GLDTypeCodes.RealizeTaxGLMovement);
			return new DebitCreditEntryItem[] { dRLine, cRLine };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			var taxTransaction = GetTaxTransaction((Guid)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.ATM_ATT_TaxTransaction]);
			var generalLedgerDataBasic = new DebitCreditEntry();
			generalLedgerDataBasic.CompanyPK = taxTransaction.ATT_GC.ToGuid();
			generalLedgerDataBasic.ExchangeRate = taxTransaction.ExchangeRate;
			generalLedgerDataBasic.Currency = taxTransaction.ATT_RX_NKOSTaxCurrency;
			generalLedgerDataBasic.BranchPK = taxTransaction.ATT_GB.ToGuid();
			generalLedgerDataBasic.DepartmentPK = taxTransaction.ATT_GE_Department.ToGuid();
			generalLedgerDataBasic.TaxGLMovementPK = (Guid)gLDDataSourceRow[AccTaxGLMovementSchema.Constants.PK];

			return generalLedgerDataBasic;
		}

		AccTaxTransaction GetTaxTransaction(Guid pk)
		{
			if (fTaxTransaction == null)
			{
				fTaxTransaction = ReadOnlyFactory.Load<AccTaxTransaction>(pk);
			}
			else if (fTaxTransaction.PK != pk)
			{
				fTaxTransaction = ReadOnlyFactory.Load<AccTaxTransaction>(pk);
			}

			return fTaxTransaction;
		}
		AccTaxTransaction fTaxTransaction;
	}
}
