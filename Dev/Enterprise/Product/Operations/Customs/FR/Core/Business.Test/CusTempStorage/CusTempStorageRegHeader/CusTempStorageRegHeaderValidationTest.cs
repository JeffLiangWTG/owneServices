using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using TransactionTypes = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageRegHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckSRH_Status()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "Job001";
			var regLine1 = header.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			var transaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_PackageQty = 10;
			transaction1.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			var transaction2 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -10;
			transaction2.SRT_TransactionType = TransactionTypes.Codes.Transaction;
			header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			header.Validation.ValidateSRH_Status();
			AssertHasError("Temp. Storage Register Header can't be open if the sum of packages in all transactions equals zero", header.SRH_StatusInfo, "Temp. register header status can't be open if the sum of packages in all transactions equals zero.");
		}
	}
}
