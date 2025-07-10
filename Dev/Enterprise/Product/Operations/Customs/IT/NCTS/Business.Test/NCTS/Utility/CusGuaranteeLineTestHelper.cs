using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

static class CusGuaranteeLineTestHelper
{
	public static void AssertGuaranteeLineTransaction(BaseCusGuaranteeLineTransaction guaranteeLineTransaction, ZString appId, ZString comment, ZDateTime transactionDate, ZString transactionType, ZString transactionCategory, ZString transactionStatus, ZString reference, ZDecimal transactionValue)
	{
		AssertEquals("CPL_AppId", appId, guaranteeLineTransaction.CPL_AppId);
		AssertEquals("CPL_Comment", comment, guaranteeLineTransaction.CPL_Comment);
		AssertEquals("CPL_TransactionDate", transactionDate.ToShortDateString(), guaranteeLineTransaction.CPL_TransactionDate.ToShortDateString());
		AssertEquals("CPL_TransactionType", transactionType, guaranteeLineTransaction.CPL_TransactionType);
		AssertEquals("CPL_TransactionCategory", transactionCategory, guaranteeLineTransaction.CPL_TransactionCategory);
		AssertEquals("CPL_TransactionStatus", transactionStatus, guaranteeLineTransaction.CPL_TransactionStatus);
		AssertEquals("CPL_Reference", reference, guaranteeLineTransaction.CPL_Reference);
		AssertEquals("CPL_TranValue", transactionValue, guaranteeLineTransaction.CPL_TranValue);
	}

	public static BaseCusGuaranteeLineTransaction[] GetGuaranteeLineTransactionTestCases(BusinessObjectFactory factory, ZString appId, ZString reference)
	{
		var permitHolder = factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(factory, permitHolder, "ARG", "12345");

		var transactions = new BaseCusGuaranteeLineTransaction[5];

		(ZString Status, ZString AppId, ZString Reference)[] transactionsData =
		{
			(PermitTransactionStatusList.Codes.Pending, appId, reference),
			(PermitTransactionStatusList.Codes.Pending, appId, reference),
			(PermitTransactionStatusList.Codes.Confirmed, appId, reference),
			(PermitTransactionStatusList.Codes.Pending, "0002", reference),
			(PermitTransactionStatusList.Codes.Pending, appId, "A0010")
		};

		for (int i = 0; i < transactionsData.Length; i++)
		{
			transactions[i] = CreateGuaranteeLineTransaction(
				factory,
				permitHeader.PK,
				transactionsData[i].Status,
				transactionsData[i].AppId,
				transactionsData[i].Reference);
		}

		return transactions;
	}

	static BaseCusGuaranteeLineTransaction CreateGuaranteeLineTransaction(BusinessObjectFactory factory, ZGuid permitHeaderPk, ZString transactionStatus, ZString appId, ZString reference)
	{
		var transaction = factory.New<BaseCusGuaranteeLineTransaction>();
		transaction.CPL_CPH_PermitHeader = permitHeaderPk;
		transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
		transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
		transaction.CPL_TransactionStatus = transactionStatus;
		transaction.CPL_AppId = appId;
		transaction.CPL_Reference = reference;
		transaction.CPL_Comment = "NCTS departure test";
		return transaction;
	}
}
