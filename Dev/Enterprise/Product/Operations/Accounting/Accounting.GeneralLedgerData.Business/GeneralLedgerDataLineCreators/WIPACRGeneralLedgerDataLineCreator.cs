using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class WIPACRGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			if (GLControlAccounts.Instance.WIPControlAccount == null)
			{
				throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Caption);
			}

			if (GLControlAccounts.Instance.ACRControlAccount == null)
			{
				throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Caption);
			}

			var result = new List<DebitCreditEntryItem>();
			var lineType = (string)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineType];
			var postDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate]);
			var recognizedDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_ReverseDate]);
			var lineGLAccount = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG];
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount];

			string controlAccounType;
			ZGuid controlAccountPK;
			if (lineType == TransactionLineTypes.WIP)
			{
				controlAccounType = GLDAccountTypes.AccruedRevenueControlAccount;
				controlAccountPK = GLControlAccounts.Instance.WIPControlAccount.PK;
			}
			else
			{
				controlAccounType = GLDAccountTypes.AccruedCostControlAccount;
				controlAccountPK = GLControlAccounts.Instance.ACRControlAccount.PK;
			}

			if (gLDDataSourceRow.RowState == DataRowState.Added || gLDDataSourceRow.RowState == DataRowState.Unchanged)
			{
				AddPostDateDRCRLines();
				AddReverseDateDRCRLines();
			}
			else if (gLDDataSourceRow.RowState == DataRowState.Modified)
			{
				AddReverseDateDRCRLines();
			}

			void AddPostDateDRCRLines()
			{
				if (postDate.IsValid)
				{
					var crLine = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccount, localAmount, oSAmount, postDate);
					var drLine = CreateAndPopulateDebitCreditLine(controlAccountPK, controlAccounType, localAmount * (-1), oSAmount * (-1), postDate);

					result.Add(crLine);
					result.Add(drLine);
				}
			}

			void AddReverseDateDRCRLines()
			{
				if (recognizedDate.IsValid)
				{
					var crLine = CreateAndPopulateDebitCreditLine(controlAccountPK, controlAccounType, localAmount, oSAmount, recognizedDate, AccountingConstants.GLDTypeCodes.Recognition);
					var drLine = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccount, localAmount * (-1), oSAmount * (-1), recognizedDate, AccountingConstants.GLDTypeCodes.Recognition);

					result.Add(crLine);
					result.Add(drLine);
				}
			}

			return result.ToArray();
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
		}
	}
}
