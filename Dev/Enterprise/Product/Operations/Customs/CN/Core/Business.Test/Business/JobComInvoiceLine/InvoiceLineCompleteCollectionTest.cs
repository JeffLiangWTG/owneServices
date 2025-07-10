using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestDefaultDistrictForNewChild()
		{
			var org1 = Factory.New<OrgHeader>();
			var org1_CCD = org1.CustomsCodes.AddNew();
			org1_CCD.OK_CodeType = "CCD";
			org1_CCD.OK_RN_NKCodeCountry = "CN";
			org1_CCD.OK_CustomsRegNo = "1111111111";
			AssertEquals(org1_CCD.OK_CustomsRegNo, org1.LocalCustomsClientCode);

			var org2 = Factory.New<OrgHeader>();
			var org2_CCD = org2.CustomsCodes.AddNew();
			org2_CCD.OK_CodeType = "CCD";
			org2_CCD.OK_RN_NKCodeCountry = "CN";
			org2_CCD.OK_CustomsRegNo = "2222222222";
			AssertEquals(org2_CCD.OK_CustomsRegNo, org2.LocalCustomsClientCode);

			var org3 = Factory.New<OrgHeader>();
			var org3_CCD = org3.CustomsCodes.AddNew();
			org3_CCD.OK_CodeType = "CCD";
			org3_CCD.OK_RN_NKCodeCountry = "CN";
			org3_CCD.OK_CustomsRegNo = "3333333333";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_OH_Supplier = org1.PK;
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header1 = declaration1.Invoices.AddNew();
			var lineCollection1 = new InvoiceLineCompleteCollection(declaration1);
			var invoiceLine1 = lineCollection1.AddNew();
			AssertEquals(org1_CCD.OK_CustomsRegNo.Left(5), invoiceLine1.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine1.JI_DestinationDistrict);
			var invoiceLine12 = lineCollection1.AddNew();
			AssertEquals(org1_CCD.OK_CustomsRegNo.Left(5), invoiceLine12.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine12.JI_DestinationDistrict);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_OH_Supplier = org1.PK;
			declaration2.JE_OH_Manufacturer = org3.PK;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header2 = declaration2.Invoices.AddNew();
			var lineCollection2 = new InvoiceLineCompleteCollection(declaration2);
			var invoiceLine2 = lineCollection2.AddNew();
			AssertEquals(org3_CCD.OK_CustomsRegNo.Left(5), invoiceLine2.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine2.JI_DestinationDistrict);
			var invoiceLine22 = lineCollection2.AddNew();
			AssertEquals(org3_CCD.OK_CustomsRegNo.Left(5), invoiceLine22.JI_OriginDistrict);
			AssertEquals(ZString.Empty, invoiceLine22.JI_DestinationDistrict);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_OH_Importer = org2.PK;
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header3 = declaration3.Invoices.AddNew();
			var lineCollection3 = new InvoiceLineCompleteCollection(declaration3);
			var invoiceLine3 = lineCollection3.AddNew();
			AssertEquals(ZString.Empty, invoiceLine3.JI_OriginDistrict);
			AssertEquals(org2_CCD.OK_CustomsRegNo.Left(5), invoiceLine3.JI_DestinationDistrict);
			var invoiceLine32 = lineCollection3.AddNew();
			AssertEquals(ZString.Empty, invoiceLine32.JI_OriginDistrict);
			AssertEquals(org2_CCD.OK_CustomsRegNo.Left(5), invoiceLine32.JI_DestinationDistrict);

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_OH_Importer = org2.PK;
			declaration4.JE_OH_Buyer = org3.PK;
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header4 = declaration4.Invoices.AddNew();
			var lineCollection4 = new InvoiceLineCompleteCollection(declaration4);
			var invoiceLine4 = lineCollection4.AddNew();
			AssertEquals(ZString.Empty, invoiceLine4.JI_OriginDistrict);
			AssertEquals(org3_CCD.OK_CustomsRegNo.Left(5), invoiceLine4.JI_DestinationDistrict);
			var invoiceLine42 = lineCollection4.AddNew();
			AssertEquals(ZString.Empty, invoiceLine42.JI_OriginDistrict);
			AssertEquals(org3_CCD.OK_CustomsRegNo.Left(5), invoiceLine42.JI_DestinationDistrict);
		}

		public void TestDefaultPreference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			AssertEquals("Default Primary Preference to MFN for Import job", "MFN", invoiceLine1.JI_PrimaryPreference);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Not default Primary Preference for Export job", "", invoiceLine1.JI_PrimaryPreference);
		}

		public void TestTypedIndexer()
		{
			var collection = new InvoiceLineCompleteCollection(Declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
			AssertType<JobComInvoiceLine>(invoiceLine);
			AssertType<JobComInvoiceLine>(collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration);

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override BaseJobDeclaration GetMeANewJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
