using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLine))]
	class CusTempStorageRegLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetSRL_PackagesRemaining()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Initially Header Status is Closed", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);
				line.SRL_PackagesRemaining = 1;
				AssertEquals("Header Status is updated to Open", TempStorageDeclarationStatusList.Codes.Open, header.SRH_Status);
				line.SRL_PackagesRemaining = 0;
				Factory.Save();
				AssertEquals("Header Status is updated to Closed", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);
			});
		}

		public void TestRegHeader()
		{
			AssertType<CusTempStorageRegHeader>(line.RegHeader);
		}

		public void TestLookups()
		{
			AssertType<CusTempStorageRegLineLookups>(line.Lookups);
		}

		public void TestCusTempStorageRegLineTransactions()
		{
			AssertType<CusTempStorageRegLineTransactionCollection>("Type", line.CusTempStorageRegLineTransactions);
		}

		public void TestCusTempStorageRegLineTransactions_CollectionCountChanged()
		{
			var transaction1 = Factory.New<CusTempStorageRegLineTransaction>();
			transaction1.SRT_PackageQty = 1;
			var transaction2 = Factory.New<CusTempStorageRegLineTransaction>();
			transaction2.SRT_PackageQty = 5;

			AssertEquals("Initially SRL_PackagesRemaining = 0", 0, line.SRL_PackagesRemaining);
			line.CusTempStorageRegLineTransactions.Add(transaction1);
			AssertEquals("SRL_PackagesRemaining added by 1", 1, line.SRL_PackagesRemaining);
			line.CusTempStorageRegLineTransactions.Add(transaction2);
			AssertEquals("SRL_PackagesRemaining added by 5", 6, line.SRL_PackagesRemaining);
			transaction1.SRT_SRL = ZGuid.Empty;
			AssertEquals("SRL_PackagesRemaining reduced by 1", 5, line.SRL_PackagesRemaining);
			line.CusTempStorageRegLineTransactions.Delete(transaction2);
			AssertEquals("SRL_PackagesRemaining reduced by 5", 0, line.SRL_PackagesRemaining);
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
			return line;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
		}
		CusTempStorageRegHeader header;
		CusTempStorageRegLine line;
	}
}
