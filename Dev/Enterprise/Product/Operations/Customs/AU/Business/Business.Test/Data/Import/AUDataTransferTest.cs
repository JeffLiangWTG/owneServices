using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUDataTransferTest : DataTransferImplTest
	{
		public void TestGetFlatFileDataImporter()
		{
			AUDataTransferTestClass transfer = new AUDataTransferTestClass();
			AssertEquals(typeof(AUFlatFileInvoiceDataImporter), transfer.GetTypeOfFlatFileDataImporter());
		}

		class AUDataTransferTestClass : AUDataTransfer
		{
			public Type GetTypeOfFlatFileDataImporter()
			{
				var factory = new BusinessObjectFactory();
				var toJobDec = factory.New<JobDeclaration>();
				var importer = GetFlatFileImporter("", toJobDec);
				return importer.GetType();
			}
		}
	}
}
