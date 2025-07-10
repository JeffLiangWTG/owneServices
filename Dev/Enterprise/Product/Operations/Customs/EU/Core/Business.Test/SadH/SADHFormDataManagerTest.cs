using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class SADHFormDataManagerTest : TestCaseWithFactory
	{
		public void TestCantPassNullIntoFirstParameterInConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new SADHFormDataManager(null); });
		}

		public void TestSADHFormDataManager()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			OrgHeader consignor = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = consignor.PK;
			OrgHeader consignee = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = consignee.PK;

			SADHFormDataManager manager = new SADHFormDataManager(declaration);

			// Assert that the data from the JobDec or some part thereof was read in.
			AssertEquals("declaration.JE_OH_Supplier", manager.FormData.D1_OH_Consignor, declaration.JE_OH_Supplier);
			AssertEquals("declaration.JE_OH_Importer", manager.FormData.D1_OH_Consignee, declaration.JE_OH_Importer);

			// Change the data on the SADHFormData object.
			OrgHeader newConsignor = Factory.New<OrgHeader>();
			manager.FormData.D1_OH_Consignor = newConsignor.PK;
			OrgHeader newConsignee = Factory.New<OrgHeader>();
			manager.FormData.D1_OH_Consignee = newConsignee.PK;

			// Assert that the data has not been written back YET.
			AssertNotEquals("declaration.JE_OH_Supplier", manager.FormData.D1_OH_Consignor, declaration.JE_OH_Supplier);
			AssertNotEquals("declaration.JE_OH_Importer", manager.FormData.D1_OH_Consignee, declaration.JE_OH_Importer);

			manager.WriteData();

			// Assert that the data was written back.
			AssertEquals("declaration.JE_OH_Supplier", manager.FormData.D1_OH_Consignor, declaration.JE_OH_Supplier);
			AssertEquals("declaration.JE_OH_Importer", manager.FormData.D1_OH_Consignee, declaration.JE_OH_Importer);
			AssertEquals(true, manager.ExecutedSuccessfully);
		}
	}
}
