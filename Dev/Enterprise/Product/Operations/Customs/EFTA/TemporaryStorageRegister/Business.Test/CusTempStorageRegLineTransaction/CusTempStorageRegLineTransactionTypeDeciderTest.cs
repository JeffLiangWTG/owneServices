using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionTypeDecider))]
sealed class CusTempStorageRegLineTransactionTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForLoad_Default()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "A1";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var transaction = line.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_TransactionType = "OBL";
		Factory.Save();

		AssertEquals("Type", typeof(CusTempStorageRegLineTransaction), new BusinessObjectFactory().Load<CusTempStorageRegLineTransaction>(transaction.PK).GetType());
	}

	public void TestGetTypeForLoad_SUM()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "SUM";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var transaction = line.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_TransactionType = "OBL";
		Factory.Save();
		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.DE.ICusTempStorageRegLineTransaction>(), new BusinessObjectFactory().Load<CusTempStorageRegLineTransaction>(transaction.PK).GetType());
	}

	public void TestGetTypeForLoad_IST()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "IST";
		header.SRH_Reference = "REF";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var transaction = line.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_TransactionType = "OBL";
		Factory.Save();
		AssertEquals("Type", ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageRegLineTransaction>(), new BusinessObjectFactory().Load<CusTempStorageRegLineTransaction>(transaction.PK).GetType());
	}

	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineTransaction), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineTransaction), Decider.GetTypeForNew());
	}

	public void TestGetRegLineType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetRegLineType());
	}

	CusTempStorageRegLineTransactionTypeDecider Decider => decider ??= new CusTempStorageRegLineTransactionTypeDecider();
	CusTempStorageRegLineTransactionTypeDecider decider;
}
