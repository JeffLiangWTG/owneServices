using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransaction))]
sealed class CusTempStorageRegLineTransactionTest : EnterpriseBusinessObjectTestCase
{
	public void TestRegLine()
	{
		var regLine = Factory.New<CusTempStorageRegLine>();
		var regLineTransaction = Factory.New<CusTempStorageRegLineTransaction>();
		regLineTransaction.SRT_SRL = regLine.PK;
		AssertType<CusTempStorageRegLine>(regLineTransaction.RegLine);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_ArrivalDate = ZDate.Today;
		header.SRH_PresentationDate = ZDate.Today;
		header.SRH_AppCode = "123";
		header.SRH_Status = "OK";
		header.SRH_Reference = "TEST";

		var line = (CusTempStorageRegLine)header.CusTempStorageRegLines.AddNew();
		line.SRL_SRH = header.PK;
		line.SRL_LimitDate = ZDate.Today;
		line.SRL_LineNumber = 1;
		line.SRL_PackagesRemaining = 0;
		line.SRL_LocationOfGoods = "GB";

		var transaction = (CusTempStorageRegLineTransaction)line.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_SRL = line.PK;
		transaction.SRT_TransactionType = "TRN";
		transaction.SRT_GrossWeight = 1;
		transaction.SRT_InternalReferenceNumber = "R1";

		return transaction;
	}
}
