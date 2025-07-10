using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransaction))]
	class CusTempStorageRegLineTransactionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSRT_GrossWeight()
		{
			AssertEquals("Caption", "Gross Weight", DataBoundResourceStrings.GetDataForProperty(transaction.SRT_GrossWeightInfo).Caption);
		}

		public void TestSRT_TransactionType()
		{
			AssertEquals("ReadOnly", true, transaction.SRT_TransactionTypeInfo.ReadOnly);
		}

		public void TestSRT_PackageQtySetterUpdatesPackagesRemaining()
		{
			header.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			var transaction2 = Factory.New<CusTempStorageRegLineTransaction>();
			transaction2.SRT_PackageQty = -4;
			CombineAssertions(() =>
			{
				AssertEquals("Initially line.SRL_PackagesRemaining = 0", 0, line.SRL_PackagesRemaining);
				AssertEquals("Initially header.SRH_Status = Closed", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);

				transaction.SRT_PackageQty = 5;
				transaction.SRT_TransactionType = "OBL";
				AssertEquals("line.SRL_PackagesRemaining after SRT_PackageQty is increased by 5", 5, line.SRL_PackagesRemaining);
				AssertEquals("header.SRH_Status after SRT_PackageQty is increased by 5", TempStorageDeclarationStatusList.Codes.Open, header.SRH_Status);

				line.CusTempStorageRegLineTransactions.Add(transaction2);
				AssertEquals("line.SRL_PackagesRemaining after transaction2 is added", 1, line.SRL_PackagesRemaining);
				AssertEquals("header.SRH_Status after transaction2 is added", TempStorageDeclarationStatusList.Codes.Open, header.SRH_Status);

				transaction2.SRT_PackageQty = -5;
				transaction2.SRT_TransactionType = "ADJ";
				Factory.Save();
				AssertEquals("line.SRL_PackagesRemaining after transaction2 is updated", 0, line.SRL_PackagesRemaining);
				AssertEquals("header.SRH_Status after transaction2 is updated", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);

				line.CusTempStorageRegLineTransactions.Delete(transaction2);
				AssertEquals("line.SRL_PackagesRemaining after transaction2 is deleted", 5, line.SRL_PackagesRemaining);
				AssertEquals("header.SRH_Status after transaction2 is deleted", TempStorageDeclarationStatusList.Codes.Open, header.SRH_Status);
			});
		}

		public void TestSRT_Reference()
		{
			AssertEquals("TSD Number", DataBoundResourceStrings.GetDataForProperty(transaction.SRT_ReferenceInfo).Caption);
		}

		public void TestSRT_InternalReferenceNumber()
		{
			AssertEquals("Job Reference", DataBoundResourceStrings.GetDataForProperty(transaction.SRT_InternalReferenceNumberInfo).Caption);
		}

		public void TestReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Transaction is editable when it's not saved", false, transaction.ReadOnly);

				Factory.Save();
				AssertEquals("Transaction is readonly after it's saved", true, transaction.ReadOnly);
			});
		}

		public void TestRegLine()
		{
			AssertType<CusTempStorageRegLine>(transaction.RegLine);
		}

		public void TestLookups()
		{
			AssertType<CusTempStorageRegLineTransactionLookups>(transaction.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusTempStorageRegLineTransactionValidation>(transaction.Validation);
		}

		public void TestSRT_SystemCreateTimeUtc_ReadOnly()
		{
			AssertEquals(true, transaction.SRT_SystemCreateTimeUtcInfo.ReadOnly);
		}

		public void TestSRT_SystemCreateUser_ReadOnly()
		{
			AssertEquals(true, transaction.SRT_SystemCreateUserInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
			return line.CusTempStorageRegLineTransactions.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			header.SRH_Reference = "TEST";
			line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
			transaction = line.CusTempStorageRegLineTransactions.AddNew();
		}
		CusTempStorageRegHeader header;
		CusTempStorageRegLine line;
		CusTempStorageRegLineTransaction transaction;
	}
}
