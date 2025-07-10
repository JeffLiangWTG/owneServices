using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageDecProvider))]
	sealed class REXDISCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<REXDISCusTempStorageDecProvider>
	{
		public void TestDeclarationSubType()
		{
			TempStorageDec.STH_DeclarationSubType = TemporaryStorageProcedureTypeList.Codes.C2;
			AssertEquals(TemporaryStorageProcedureTypeList.Codes.C2, ((IREXDISTempStorageDec)TempStorageDecWrapped).DeclarationSubType);
		}

		public void TestStorageLineType()
		{
			TempStorageDec.CusTempStorageLines.AddNew();
			AssertType<REXDISCusTempStorageReExportLineProvider>(TempStorageDecWrapped.StorageLines.First());
		}

		public void TestHeaderType()
		{
			TempStorageDec.STH_SJH = Factory.New<CusTempStorageJobHeader>().PK;
			AssertType<REXDISCusTempStorageJobHeaderProvider>(TempStorageDecWrapped.Header);
		}

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<REXDISCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new REXDISCusTempStorageDecProvider(TempStorageDec);
	}
}
