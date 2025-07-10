using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	class CusTempStorageRegLineTransactionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInternalReferenceTypeList()
		{
			var list = lookups.InternalReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString when no premises are declared", "ABD, DES, DUA, DUE, DVD, EXS, G5, H7, LAM, OTH, T2L, TRA, TSM", list.CodesAsString);

				var premises = Factory.New<CusTempStorageRegPremises>();
				premises.SRP_Code = "TEST";
				premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
				header.SRH_SRP_Premises = premises.PK;
				list = lookups.InternalReferenceTypeList;
				AssertEquals("CodesAsString when premises type is LAM", "ABD, DES, DUE, LAM, OTH, T2L", list.CodesAsString);

				premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
				list = lookups.InternalReferenceTypeList;
				AssertEquals("CodesAsStringwhem premises type is ADT", "ABD, DES, DUA, DVD, EXS, G5, H7, OTH, TRA, TSM", list.CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
			var transaction = line.CusTempStorageRegLineTransactions.AddNew();
			lookups = transaction.Lookups;
		}
		CusTempStorageRegHeader header;
		CusTempStorageRegLineTransactionLookups lookups;
	}
}
