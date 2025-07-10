using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public static class ScriptTestHelper
	{
		public static AccCashBasisVAT CreateCashBasisVAT(TestObjectCreator testObjectCreator, ZString transactionnum, bool isSave = true)
		{
			var header = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), transactionnum, testObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m);
			var line = header.Lines[0];
			line.AL_AT = testObjectCreator.GSTFREE1.PK;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var cashVATRecord = testObjectCreator.CreateCashBasisVAT(line, 190m, 0m, ZDateTime.Today);
			cashVATRecord.YC_MatchGroupNum = "M001";
			if (isSave)
			{
				var valueCache = SuspendCriticalValidationAttribute.IsActive;
				using (new DisposableAction(() => SuspendCriticalValidationAttribute.IsActive = true, () => SuspendCriticalValidationAttribute.IsActive = valueCache))
				{
					testObjectCreator.Factory.Save();
				}
			}
			return cashVATRecord;
		}

		public static void CreateMultiSubAccounts(TestObjectCreator testObjectCreator, string tableName, ZGuid pk, string subAccoutTypeOrTableCode, ZGuid targetPK)
		{
			switch (tableName)
			{
				case AccTransactionHeaderSubAccountSchema.Constants.TableName:
					testObjectCreator.CreateTransactionHeaderSubAccount(pk, subAccoutTypeOrTableCode, targetPK);
					break;
				case AccTransactionLineSubAccountSchema.Constants.TableName:
					testObjectCreator.CreateTransactionLineSubAccount<AccTransactionLineSubAccount>(pk, subAccoutTypeOrTableCode, targetPK);
					break;
				case AccGLHeaderSubAccountSchema.Constants.TableName:
					var accGLHeader = testObjectCreator.Factory.Load<AccGLHeader>(pk);
					testObjectCreator.CreateGLHeaderSubAccount(accGLHeader, subAccoutTypeOrTableCode, true);
					break;
				default:
					break;
			}
			testObjectCreator.Factory.Save();
		}
	}
}
