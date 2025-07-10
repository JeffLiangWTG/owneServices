using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUAImportExporterWrapperTest : WrapperHelperTest<DUAImportExporterWrapper>
	{
		public void TestAddress()
		{
			var address1 = Factory.New<OrgAddress>();
			address1.City = "BCN";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = HeaderData.ExporterCode;
			org.MainAddress.City = "MAD";
			org.Addresses.Add(address1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				invoice.JZ_OH_Supplier = org.PK;
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				var cusEntryHeader = declaration.CustomsEntryHeaders[0];
				var wrapper = DUAImportExporterWrapper.New(cusEntryHeader);
				AssertNotNull("Declaration not null", wrapper);

				AssertEquals("Expected Supplier MainAddres city", "MAD", wrapper.City);

				invoice.JZ_OA_SupplierAddress = address1.PK;
				wrapper = DUAImportExporterWrapper.New(cusEntryHeader);
				AssertEquals("Expected OA_SupplierAddress city", "BCN", wrapper.City);
			});
		}

		public void TestGetNewDUAImportExporterWrapperIfNotNull()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = HeaderData.ExporterCode;

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			CombineAssertions(() =>
			{
				CusEntryHeader cusEntryHeader = null;
				AssertNull("CusEntryHeader null", DUAImportExporterWrapper.New(cusEntryHeader));

				cusEntryHeader = Factory.New<CusEntryHeader>();
				AssertNull("Declaration null", DUAImportExporterWrapper.New(cusEntryHeader));

				var invoice = declaration.Invoices.AddNew();
				invoice.InvoiceLines.AddNew();
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				cusEntryHeader = declaration.CustomsEntryHeaders[0];
				AssertNull("OrgHeader null", DUAImportExporterWrapper.New(cusEntryHeader));

				invoice.JZ_OH_Supplier = org.PK;
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				cusEntryHeader = declaration.CustomsEntryHeaders[0];
				AssertNotNull("Declaration not null", DUAImportExporterWrapper.New(cusEntryHeader));
			});
		}

		public void TestSimplifiedProcedureType()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageSubType = HeaderData.MessageSubType;
				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected SimplifiedProcedureType empty because entryStyle is not CO", ZString.Empty, wrapper.SimplifiedProcedureType);

				declaration.JE_MessageSubType = HeaderData.MessageSubTypeCO;
				declaration.JE_RL_NKOrigin = "FRPAR";
				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected SimplifiedProcedureType empty because entryStyle is CO but Origin doesn't start with ES", ZString.Empty, wrapper.SimplifiedProcedureType);

				declaration.JE_RL_NKOrigin = "ESMAD";
				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected SimplifiedProcedureType empty because entryStyle is CO, Origin starts with ES but entry lines don't contain document type 9011", ZString.Empty, wrapper.SimplifiedProcedureType);

				var supDoc = declaration.SupportingDocuments.AddNew();
				supDoc.CSI_Code = "9011";
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];

				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected SimplifiedProcedureType A because entryStyle is CO, Origin starts with ES but entry lines contain document type 9011 and not document type 1015", "A", wrapper.SimplifiedProcedureType);

				var supDoc2 = declaration.SupportingDocuments.AddNew();
				supDoc2.CSI_Code = "1015";
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];

				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected SimplifiedProcedureType B because entryStyle is CO, Origin starts with ES but entry lines contain document type 9011 and document type 1015", "B", wrapper.SimplifiedProcedureType);
			});
		}

		public void TestExporterWhenMultipleSuppliers()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected Name correct because there is 1 supplier", OrgHeaderData.Name, wrapper.Name);

				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var orgHeader2 = Factory.New<OrgHeader>();
				orgHeader2.OH_Code = HeaderData.ExporterCode2;
				orgHeader2.Addresses.AddNew();
				invoice2.JZ_OH_Supplier = orgHeader2.PK;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];

				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected Name 00200 because there are 2 different suppliers in the entryheader", "00200", wrapper.Name);
				AssertEquals("Expected Id empty because there are 2 different suppliers in the entryheader", ZString.Empty, wrapper.Id);
				AssertEquals("Expected Address empty because there are 2 different suppliers in the entryheader", ZString.Empty, wrapper.Address);
				AssertEquals("Expected City empty because there are 2 different suppliers in the entryheader", ZString.Empty, wrapper.City);
				AssertEquals("Expected Country empty because there are 2 different suppliers in the entryheader", ZString.Empty, wrapper.Country);
				AssertEquals("Expected PostCode empty because there are 2 different suppliers in the entryheader", ZString.Empty, wrapper.PostCode);
				AssertEquals("Expected SimplifiedProcedureType empty because there are 2 different suppliers in the entryheader", ZString.Empty, wrapper.SimplifiedProcedureType);

				invoice2.JZ_OH_Supplier = orgHeader.PK;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];

				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected Name correct because there are 2 suppliers but they are the same", OrgHeaderData.Name, wrapper.Name);
			});
		}

		public void TestSupplierId()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ExporterCode;
			orgHeader.OH_FullName = OrgHeaderData.Name;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			var orgAddressMain = orgHeader.MainAddress;
			orgAddressMain.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			invoice.JZ_OH_Supplier = orgHeader.PK;
			invoice.JZ_OA_SupplierAddress = orgAddress.PK;

			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.China;
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Italy;

			CombineAssertions(() =>
			{
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];

				wrapper = DUAImportExporterWrapper.New(entryHeader);

				AssertEquals("Expected Empty Id if supplier country, origin country and destination country are not Spain", ZString.Empty, wrapper.Id);

				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Spain;
				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected Empty Id if supplier country, origin country are not Spain", ZString.Empty, wrapper.Id);

				declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Spain;
				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected Empty Id if supplier country is not Spain", ZString.Empty, wrapper.Id);

				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
				wrapper = DUAImportExporterWrapper.New(entryHeader);
				AssertEquals("Expected Id if supplier country, origin country and destination country are Spain", "NIF22222222", wrapper.Id);
			});
		}

		public void TestCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);

			Factory.Save();

			var address1 = Factory.New<OrgAddress>();
			address1.OA_RN_NKCountryCode = "ES";

			var address2 = Factory.New<OrgAddress>();
			address2.OA_RN_NKCountryCode = "MQ";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = HeaderData.ExporterCode;
			org.MainAddress.OA_RN_NKCountryCode = "RS";
			org.Addresses.Add(address1);
			org.Addresses.Add(address2);

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				invoice.JZ_OH_Supplier = org.PK;
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				var cusEntryHeader = declaration.CustomsEntryHeaders[0];
				var wrapper = DUAImportExporterWrapper.New(cusEntryHeader);
				AssertNotNull("Declaration not null", wrapper);

				AssertEquals("Expected Supplier MainAddres Country with Default Territory", "XS", wrapper.Country);

				invoice.JZ_OA_SupplierAddress = address1.PK;
				wrapper = DUAImportExporterWrapper.New(cusEntryHeader);
				AssertEquals("Expected OA_SupplierAddress Country (address1) with given code since there is no Default Territory", "ES", wrapper.Country);

				invoice.JZ_OA_SupplierAddress = address2.PK;
				wrapper = DUAImportExporterWrapper.New(cusEntryHeader);
				AssertEquals("Expected OA_SupplierAddress Country (address2) with Default Territory", "FR", wrapper.Country);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203001011";

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ExporterCode;
			orgHeader.OH_FullName = OrgHeaderData.Name;
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Address";
			invoice.JZ_OH_Supplier = orgHeader.PK;

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = DUAImportExporterWrapper.New(entryHeader);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		OrgHeader orgHeader;
		CusEntryHeader entryHeader;
		DUAImportExporterWrapper wrapper;

		protected override DUAImportExporterWrapper GetProvider() => wrapper;
	}
}
