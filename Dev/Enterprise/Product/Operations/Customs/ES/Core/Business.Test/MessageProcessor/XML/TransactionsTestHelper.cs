using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;

namespace Enterprise.Customs.ES.Business.Testing;

public class TransactionsTestHelper : TestCaseWithFactory
{
	public static void AssertProcessCancelationResponse_PreRequisites(int numTransactions, ZString status, ZString customsStatus, int numGuaranteeTransactions)
	{
		CombineAssertions(() =>
		{
			AssertEquals("No additional transactions created after process cancelation response", 1, numTransactions);
			AssertEquals("SRH_Status after process cancelation response is the same", "CLS", status);
			AssertEquals("SRL_CustomsStatus after process cancelation response is the same", "CLS", customsStatus);
			AssertEquals("No additional guarantee transactions created after process cancelation response", 1, numGuaranteeTransactions);
		});
	}

	public static (CusTempStorageRegLineTransaction regLineTransaction1, CusTempStorageRegLineTransaction regLineTransaction2, CusTempStorageRegLineTransaction regLineTransaction3, CusTempStorageRegLineTransaction regLineTransaction4, CusTempStorageRegLineTransaction regLineTransaction5, CusTempStorageRegLine regLine2, EU.Business.CusGuaranteeHeader guarantee) SetupTransactions(BusinessObjectFactory factory, CusTempStorageRegLine regLine, CusTempStorageRegHeader regHeader, CusEntryHeader entryHeader, ZDateTime cancelDate, ZString mrnCode)
	{
		var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, cancelDate, mrnCode);
		regLineTransaction1.SRT_PackageQty = 2;
		regLineTransaction1.SRT_GrossWeight = -1.0m;
		regLineTransaction1.SRT_BondAmount = -1.0m;
		var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, entryHeader.CH_BGMReference, cancelDate, mrnCode);
		var regLineTransaction3 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, cancelDate, mrnCode);
		regLineTransaction3.SRT_PackageQty = 1;
		regLineTransaction3.SRT_GrossWeight = -2.0m;
		regLineTransaction3.SRT_BondAmount = -1.0m;

		var regLine2 = factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, cancelDate, mrnCode);
		regLineTransaction4.SRT_PackageQty = 3;
		regLineTransaction4.SRT_GrossWeight = -3.0m;
		regLineTransaction4.SRT_BondAmount = ZDecimal.Zero;
		var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, entryHeader.CH_BGMReference, cancelDate, mrnCode);
		var guarantee = regHeader.Guarantee.CusGuarantee;

		return (regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, regLine2, guarantee);
	}

	public static void AssertProcessCancelationResponse_WithCONAndPNDTransaction_PreRequisites(CusTempStorageRegLine regLine, CusTempStorageRegLine regLine2, CusTempStorageRegHeader regHeader, EU.Business.CusGuaranteeHeader guarantee, CusTempStorageRegLineTransaction regLineTransaction1, CusTempStorageRegLineTransaction regLineTransaction2, CusTempStorageRegLineTransaction regLineTransaction3, CusTempStorageRegLineTransaction regLineTransaction4, CusTempStorageRegLineTransaction regLineTransaction5)
	{
		CombineAssertions("[PreReq]", () =>
		{
			AssertEquals("Number of transactions first RegLine before process cancelation response", 3, regLine.CusTempStorageRegLineTransactions.Count);
			AssertEquals("Number of transactions second RegLine before process cancelation response", 2, regLine2.CusTempStorageRegLineTransactions.Count);
			AssertEquals("SRH_Status before process cancelation response is CLS", "CLS", regHeader.SRH_Status);
			AssertEquals("SRL_CustomsStatus before process cancelation response is CLS", "CLS", regLine.SRL_CustomsStatus);
			AssertEquals("Number of guarantee transactions before process cancelation response", 1, guarantee.CusGuaranteeLineTransactions.Count);
			AssertEquals("Transaction1 before process cancelation response status", "CON", regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("Transaction2 before process cancelation response status", "PND", regLineTransaction2.SRT_TransactionStatus);
			AssertEquals("Transaction3 before process cancelation response status", "CON", regLineTransaction3.SRT_TransactionStatus);
			AssertEquals("Transaction4 before process cancelation response status", "CON", regLineTransaction4.SRT_TransactionStatus);
			AssertEquals("Transaction5 before process cancelation response status", "PND", regLineTransaction5.SRT_TransactionStatus);
		});
	}

	public static void AssertProcessCancelationResponse_WithCONAndPNDTransaction(CusTempStorageRegLine regLine, CusTempStorageRegLine regLine2, CusTempStorageRegHeader regHeader, CusEntryHeader entryHeader, EU.Business.CusGuaranteeHeader guarantee, CusTempStorageRegLineTransaction regLineTransaction1, CusTempStorageRegLineTransaction regLineTransaction2, CusTempStorageRegLineTransaction regLineTransaction3, CusTempStorageRegLineTransaction regLineTransaction4, CusTempStorageRegLineTransaction regLineTransaction5, ZDateTime cancelDate, ZString mrnCode, ZString transactionsAESCommentPrefix)
	{
		CombineAssertions(() =>
		{
			AssertEquals("Number of transactions first RegLine after process cancelation response has a new transaction", 4, regLine.CusTempStorageRegLineTransactions.Count);
			AssertEquals("Number of transactions second RegLine after process cancelation response has a new transaction", 3, regLine2.CusTempStorageRegLineTransactions.Count);
			AssertEquals("SRH_Status after process cancelation response is open", "OPN", regHeader.SRH_Status);
			AssertEquals("SRL_CustomsStatus after process cancelation response is open", "OPN", regLine.SRL_CustomsStatus);
			AssertEquals("Number of guarantee transactions after process cancelation response has a two new transaction", 2, guarantee.CusGuaranteeLineTransactions.Count);
			AssertEquals("Transaction1 after process cancelation response status does not change", "CON", regLineTransaction1.SRT_TransactionStatus);
			AssertEquals("Transaction2 after process cancelation response status changes to DEL", "DEL", regLineTransaction2.SRT_TransactionStatus);
			AssertEquals("Transaction3 after process cancelation response status does not change", "CON", regLineTransaction3.SRT_TransactionStatus);
			AssertEquals("Transaction4 after process cancelation response status does not change", "CON", regLineTransaction4.SRT_TransactionStatus);
			AssertEquals("Transaction5 after process cancelation response status changes to DEL", "DEL", regLineTransaction5.SRT_TransactionStatus);
			AssertTransaction(regLine, bondAmound: 2.0m, packageQty: 3, grossWeight: 3.00m, entryHeader, cancelDate, mrnCode, transactionsAESCommentPrefix);
			AssertTransaction(regLine2, bondAmound: 0.0m, packageQty: 3, grossWeight: 3.00m, entryHeader, cancelDate, mrnCode, transactionsAESCommentPrefix);
			AssertGuarantee((CusGuaranteeHeader)guarantee, tranValue: -2.0m, cancelDate, regHeader, entryHeader, transactionsAESCommentPrefix);
		});
	}

	public static void AssertProcessCancelationResponse_WithCONAndPNDTransaction_AfterProcess(CusTempStorageRegLine regLine, CusTempStorageRegLine regLine2, EU.Business.CusGuaranteeHeader guarantee)
	{
		CombineAssertions(() =>
		{ 
			AssertEquals("Number of transactions first RegLine after process again cancelation response does not change", 4, regLine.CusTempStorageRegLineTransactions.Count);
			AssertEquals("Number of transactions second RegLine after process again cancelation response does not change", 3, regLine2.CusTempStorageRegLineTransactions.Count);
			AssertEquals("Number of guarantee transactions after process again cancelation response does not change", 2, guarantee.CusGuaranteeLineTransactions.Count);
		});
	}

	static void AssertTransaction(CusTempStorageRegLine regLine, ZDecimal bondAmound, int packageQty, decimal grossWeight, CusEntryHeader entryHeader,  ZDateTime cancelDate, ZString mrnCode, ZString transactionsAESCommentPrefix)
	{
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_Comments.Contains("DUE: "));

		AssertEquals("SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
		AssertEquals("SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration, transaction.SRT_InternalReferenceType);
		AssertEquals("SRT_InternalReferenceNumber", entryHeader.CH_BGMReference, transaction.SRT_InternalReferenceNumber);
		AssertEquals("SRT_Comments", transactionsAESCommentPrefix + entryHeader.CH_BGMReference + TransactionCommentSuffix, transaction.SRT_Comments);
		AssertEquals("SRT_TransactionDate", cancelDate.ToDateTimeOffset(null), transaction.SRT_TransactionDate);
		AssertEquals("SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals("SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals("SRT_BondAmount", bondAmound, transaction.SRT_BondAmount);
		AssertEquals("SRT_Reference", mrnCode, transaction.SRT_Reference);
	}

	static void AssertGuarantee(CusGuaranteeHeader cusGuarantee, ZDecimal tranValue, ZDateTime cancelDate, CusTempStorageRegHeader regHeader, CusEntryHeader entryHeader, ZString transactionsAESCommentPrefix)
	{
		var guarantee = cusGuarantee.CusGuaranteeLineTransactions.First(x => x.CPL_Comment.Contains("DUE: ") && x.CPL_TranValue == tranValue);

		AssertEquals("CPL_TransactionType", "TRA", guarantee.CPL_TransactionType);
		AssertEquals("CPL_TransactionDate", cancelDate, guarantee.CPL_TransactionDate);
		AssertEquals("CPL_Reference", regHeader.SRH_Reference, guarantee.CPL_Reference);
		AssertEquals("CPL_TranValue", tranValue, guarantee.CPL_TranValue);
		AssertEquals("CPL_Comment", transactionsAESCommentPrefix + entryHeader.CH_BGMReference + ". MRN: " + entryHeader.MovementReferenceNumber + TransactionCommentSuffix, guarantee.CPL_Comment);
		AssertEquals("CPL_TransactionStatus", "CON", guarantee.CPL_TransactionStatus);
	}

	static CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString status, ZString referenceNum, ZDateTime cancelDate, ZString mrnCode, string referenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration)
	{
		var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = status;
		regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
		regLineTransaction1.SRT_InternalReferenceType = referenceType;
		regLineTransaction1.SRT_PhysicalInOutDate = cancelDate.ToDateTimeOffset(null);
		regLineTransaction1.SRT_Reference = mrnCode;

		return regLineTransaction1;
	}

	const string TransactionCommentSuffix = " (Canceled)";
}
