using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public override void TestSupplierSearch()
		{
			var org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "addr1";
			org.OH_Code = "-1-";
			var declarationExp = Factory.New<JobDeclaration>();
			declarationExp.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoiceExp = declarationExp.Invoices.AddNew();
			invoiceExp.JZ_OH_Supplier = org.PK;

			var declarationImp = Factory.New<JobDeclaration>();
			declarationImp.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declarationImp.Invoices.AddNew();

			var declarationLex = Factory.New<JobDeclaration>();
			declarationLex.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declarationLex.Invoices.AddNew();

			var noMatchDec = Factory.New<JobDeclaration>();
			noMatchDec.Invoices.AddNew();

			var decSupplier = OrgHeader.New(Factory);
			decSupplier.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			decSupplier.OH_Code = "DECSUP";
			declarationExp.JE_OH_Supplier = decSupplier.PK;
			declarationImp.JE_OH_Supplier = decSupplier.PK;
			declarationLex.JE_OH_Supplier = decSupplier.PK;
			Factory.Save();

			var filter = (ModuleGuidsFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property2 = org.PK;
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier address on Invoice Header", 1, collection.Count);
			AssertEquals("Matched supplier on Invoice Header", org.PK, collection[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);

			filter.Property2 = ZGuid.NewZGuid();
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, collection.Count);

			filter.Property2 = decSupplier.PK;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 3, collection.Count);
			AssertEquals("Matched supplier on Declaration", decSupplier.PK, collection[0].JE_OH_Supplier);
			AssertEquals("Matched supplier on Declaration", decSupplier.PK, collection[1].JE_OH_Supplier);
			AssertEquals("Matched supplier on Declaration", decSupplier.PK, collection[2].JE_OH_Supplier);
		}

		public new void TestImporterSearch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Dec Main Address 1";

			var orgDiff = Factory.NewWithValidTestData<OrgHeader>();
			orgDiff.MainAddress.OA_Address1 = "DecDiff Main Address 1";

			var declarationExp = Factory.New<JobDeclaration>();
			declarationExp.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declarationExp.JE_OH_Importer = orgDiff.PK;
			var exportInvoiceHeader = declarationExp.Invoices.AddNew();
			exportInvoiceHeader.JZ_OA_BuyerAddress = org.MainAddress.PK;

			var declarationImp = Factory.New<JobDeclaration>();
			declarationImp.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declarationImp.JE_OH_Importer = org.PK;
			var importInvoiceHeader = declarationImp.Invoices.AddNew();
			importInvoiceHeader.JZ_OA_BuyerAddress = orgDiff.MainAddress.PK;
			importInvoiceHeader.JZ_OH_Buyer = orgDiff.PK;

			var declarationLex = Factory.New<JobDeclaration>();
			declarationLex.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;

			Factory.Save();

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filter = (ModuleGuidsFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property1 = org.PK;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched importer/buyer on Declaration", 2, collection.Count);
			AssertEquals("Matched buyer on Header", org.PK, collection[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Buyer);
			AssertEquals("Matched importer on Declaration", org.PK, collection[1].JE_OH_Importer);

			filter.Property1 = ZGuid.NewZGuid();
			collection.Load(filterBO.Filter);
			AssertEquals("Matched importer on Declaration", 0, collection.Count);
		}

		public void TestSupplierAndImporterSearch()
		{
			var orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
			var orgSupplierLine = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplierLine.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			var orgImporterLine = Factory.NewWithValidTestData<OrgHeader>();
			orgImporterLine.MainAddress.OA_Address1 = "Dec Importer Address 1";

			var declarationExp = Factory.New<JobDeclaration>();
			declarationExp.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declarationExp.JE_OH_Supplier = orgSupplier.PK;
			declarationExp.JE_OH_Importer = orgImporter.PK;
			var expInvHeader = declarationExp.Invoices.AddNew();
			expInvHeader.JZ_OH_Buyer = orgImporterLine.PK;
			expInvHeader.JZ_OA_SupplierAddress = orgSupplierLine.MainAddress.PK;

			var declarationImp = Factory.New<JobDeclaration>();
			declarationImp.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declarationImp.JE_OH_Importer = orgImporter.PK;
			var impInvHeader = declarationImp.Invoices.AddNew();
			impInvHeader.JZ_OA_BuyerAddress = orgImporterLine.MainAddress.PK;
			impInvHeader.JZ_OH_Supplier = orgSupplierLine.PK;

			var declarationLex = Factory.New<JobDeclaration>();
			declarationLex.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declarationLex.JE_OH_Importer = orgImporter.PK;
			declarationLex.JE_OH_Supplier = orgSupplier.PK;
			var lexInvHeader = declarationLex.Invoices.AddNew();
			lexInvHeader.JZ_OH_Buyer = orgImporterLine.PK;

			Factory.Save();

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filter = (ModuleGuidsFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property1 = orgImporterLine.PK;
			filter.Property2 = orgSupplierLine.PK;
			collection.Load(filterBO.Filter);
			AssertEquals("Matched Buyer and Supplier on header", 2, collection.Count);
			AssertEquals("Matched Buyer on Header", orgImporterLine.PK, collection[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Buyer);
			AssertEquals("Matched Supplier on Header", orgSupplierLine.PK, collection[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);

			AssertEquals("Matched Buyer on Header", orgImporterLine.PK, collection[1].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Buyer);
			AssertEquals("Matched Supplier on Header", orgSupplierLine.PK, collection[1].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);
		}

		public override void TestDeclarantFilter()
		{
			Assert("KR does not make use of the column.", true);
		}
	}
}
