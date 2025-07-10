using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusDataHeaderDocumentWrapper))]
	class CusDataHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntryNumberSpaced()
		{
			header.EntryNumber = "12345abcdeABCDE";
			AssertEquals("Should have been correctly spaced", "1 2 3 4 5 a b c d e A B C D E", new CusDataHeaderDocumentWrapper(header).EntryNumberSpaced);
		}

		public void TestCustomsInvoiceDocument()
		{
			var wrapper = new CusDataHeaderDocumentWrapper(header);
			AssertSame(invoiceHeader, wrapper.InvoiceHeader);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var orgHeader1 = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Buyer = orgHeader1.PK;
			AssertSame(orgHeader1, wrapper.SoldTo.Organization.Organisation);
			AssertSame(orgHeader1, wrapper.InvoiceOwner.Organization.Organisation);
			var orgHeader2 = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			declaration.JE_OH_Importer = orgHeader2.PK;
			wrapper = new CusDataHeaderDocumentWrapper(header);
			AssertSame(orgHeader2, wrapper.SoldTo.Organization.Organisation);
			AssertSame(orgHeader2, wrapper.InvoiceOwner.Organization.Organisation);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var orgHeader3 = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = orgHeader3.PK;
			wrapper = new CusDataHeaderDocumentWrapper(header);
			AssertSame(orgHeader3, wrapper.Seller.Organization.Organisation);
			AssertSame(orgHeader3, wrapper.InvoiceOwner.Organization.Organisation);
			var orgHeader4 = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = orgHeader4.PK;
			wrapper = new CusDataHeaderDocumentWrapper(header);
			AssertSame(orgHeader4, wrapper.Seller.Organization.Organisation);
			AssertSame(orgHeader4, wrapper.InvoiceOwner.Organization.Organisation);
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2017, 1, 1);
			AssertEquals(new ZDateTime(2017, 1, 1), wrapper.InvoiceDate);
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertEquals(ZString.Empty, wrapper.DeliveryTerm);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("CIF", wrapper.DeliveryTerm);
		}

		public void TestBusinessObjectToLogAgainst()
		{
			var testWrapper = new CusDataHeaderDocumentWrapper(header);
			AssertEquals(header, ((IBODocDataProvider)testWrapper).BusinessObjectToLogAgainst);
		}

		public void TestClientLogo()
		{
			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 3);
			var buyer = OrgHeader.New(Factory);
			var supplier = OrgHeader.New(Factory);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(buyer.PK.ToGuid(), Guid.Empty, Guid.Empty, image1);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(supplier.PK.ToGuid(), Guid.Empty, Guid.Empty, image2);
			invoiceHeader.JZ_OH_Buyer = buyer.PK;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(1, new CusDataHeaderDocumentWrapper(header).ClientLogo.Width);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(2, new CusDataHeaderDocumentWrapper(header).ClientLogo.Width);
		}

		public void TestClientLogoWithEmptyClientPK()
		{
			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 3);
			var buyer = OrgHeader.New(Factory);
			var supplier = OrgHeader.New(Factory);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(buyer.PK.ToGuid(), Guid.Empty, Guid.Empty, image1);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(supplier.PK.ToGuid(), Guid.Empty, Guid.Empty, image2);
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull(new CusDataHeaderDocumentWrapper(header).ClientLogo);
			invoiceHeader.JZ_OH_Buyer = buyer.PK;
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNull(new CusDataHeaderDocumentWrapper(header).ClientLogo);
		}

		public void TestContainers()
		{
			var declaration = ContainerWrapperTest.PrepareDataForContainerWrapperTest(Factory);
			var entryHeader = declaration.ActiveEntryHeaders[0];
			AssertEquals("CONTAINER1,CONTAINER2,CONTAINER3", new CusDataHeaderDocumentWrapper(entryHeader).Containers.Cast<EntryHeaderContainer>().Select(x => x.ContainerNumber).JoinAsString());
		}

		public void TestSupportingDocuments()
		{
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			Factory.Save();
			code1.Attributes.AddNew("Export", ZString.Empty);
			code2.Attributes.AddNew("Export", ZString.Empty);
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supportingDoc1 = invoiceLine.CusSupportingDocuments.AddNew();
			supportingDoc1.CSI_Code = "CD1";
			supportingDoc1.CSI_ReferenceNumber = "NUM1";
			var supportingDoc2 = invoiceLine.CusSupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "CD2";
			supportingDoc2.CSI_ReferenceNumber = "NUM2";
			var supportingDoc3 = invoiceLine.CusSupportingDocuments.AddNew();
			supportingDoc3.CSI_Code = "CD2";
			supportingDoc3.CSI_ReferenceNumber = "NUM2";
			var supportingDoc4 = invoiceLine.CusSupportingDocuments.AddNew();
			supportingDoc4.CSI_Code = "CD4";
			supportingDoc4.CSI_ReferenceNumber = "NUM4";
			AssertEquals("Code 1NUM1;Code 2NUM2", new CusDataHeaderDocumentWrapper(header).SupportingDocuments.Cast<EntryHeaderSupportingDocument>().Select(x => x.DocumentTypeDesc + x.DocumentNumber).JoinAsString(";"));
		}

		public void TestCustomsOffice()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(new BusinessObjectFactory(), "CUSOF", "OFC", "Test Customs Office");
			declaration.JE_CustomsOffice = "OFC";
			var wrapper = new CusDataHeaderDocumentWrapper(header);
			AssertEquals("OFC", wrapper.CustomsOffice.Code);
			AssertEquals("Test Customs Office", wrapper.CustomsOffice.Description);
		}

		public void TestFormulaPricingConfirm()
		{
			var wrapper = new CusDataHeaderDocumentWrapper(header);
			invoiceHeader.JZ_Calc_FormulaPricingConfirm = ZString.Empty;

			AssertNotNull(wrapper.FormulaPricingConfirm);
			AssertEquals(ZString.Empty, wrapper.FormulaPricingConfirm.Code);

			wrapper = new CusDataHeaderDocumentWrapper(header);
			invoiceHeader.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertEquals(ConfirmationTypeList.Codes.Yes, wrapper.FormulaPricingConfirm.Code);

			wrapper = new CusDataHeaderDocumentWrapper(header);
			invoiceHeader.JZ_Calc_FormulaPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertEquals(ConfirmationTypeList.Codes.No, wrapper.FormulaPricingConfirm.Code);
		}

		public void TestTemporaryPricingConfirm()
		{
			var wrapper = new CusDataHeaderDocumentWrapper(header);
			invoiceHeader.JZ_Calc_TemporaryPricingConfirm = ZString.Empty;

			AssertNotNull(wrapper.TemporaryPricingConfirm);
			AssertEquals(ZString.Empty, wrapper.TemporaryPricingConfirm.Code);

			wrapper = new CusDataHeaderDocumentWrapper(header);
			invoiceHeader.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Yes;
			AssertEquals(ConfirmationTypeList.Codes.Yes, wrapper.TemporaryPricingConfirm.Code);

			wrapper = new CusDataHeaderDocumentWrapper(header);
			invoiceHeader.JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.No;
			AssertEquals(ConfirmationTypeList.Codes.No, wrapper.TemporaryPricingConfirm.Code);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			return new CusDataHeaderDocumentWrapper(entryHeader);
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.ActiveEntryHeaders.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			entryLine = header.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();
		}
		JobDeclaration declaration;
		CusEntryHeader header;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
	}
}
