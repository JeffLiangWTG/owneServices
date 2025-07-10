using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonSendMessageWrapperTest : WrapperHelperTest<ImportH1CommonSendMessageWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: entryHeader", () => GetWrapper(null, Certificate));

			AssertExceptionThrown("Constructor Throws Exception if certificate is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: certificate", () => GetWrapper(entryHeader, null));

			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: Declaration", () => GetWrapper(Factory.New<CusEntryHeader>(), Certificate));
		});
	}

	public void TestOperation()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Operation should be A when no MRN", "A", wrapper.Operation);

			entryHeader.MovementReferenceNumber = "21ES";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Operation should be M when MRN", "M", wrapper.Operation);
		});
	}

	public void TestCustomsOfficeOfImport()
	{
		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.JE_CustomsOffice = "ES009999";
		wrapper = GetWrapper(entryHeader, Certificate);
		AssertEquals("Expected filled CustomsOfficeOfImport with CustomsOffice office code", "ES009999", wrapper.CustomsOfficeOfImport);
	}

	public void TestCustomOfficeOfPresentation()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected CustomsOffice when no OfficeOfPresentation is declared", "ES009999", wrapper.CustomOfficeOfPresentation);

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOffice1.CY_Data = "FR008889";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected OfficeOfPresentation office code when declared in CustomsOffice list", "FR008889", wrapper.CustomOfficeOfPresentation);
		});
	}

	public void TestNullImporter()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Importer.ToString());
	}

	public void TestImporter()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
		orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeader.OH_Code = "Code";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;

		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

		CombineAssertions(() =>
		{
			var importer = wrapper.Importer;

			AssertNotNull("Expected filled Importer", importer);
			AssertSame("Cached Importer", wrapper.Importer, importer);
		});
	}

	public void TestNullDeclarant()
	{
		declaration.Declarant.OA_OH = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
	}

	public void TestDeclarant()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddressMain = orgHeader.MainAddress;

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;

		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;

		CombineAssertions(() =>
		{
			var declarant = wrapper.Declarant;

			AssertNotNull("Expected filled Declarant", declarant);
			AssertSame("Cached Declarant", wrapper.Declarant, declarant);
		});
	}

	public void TestNullRepresentative()
	{
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
	}

	public void TestRepresentative()
	{
		CombineAssertions(() =>
		{
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			declaration.JE_OA_Representative = orgAddress.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;

			var representative = wrapper.Representative;

			AssertNotNull("Expected filled Representative", representative);
			AssertSame("Cached Representative", wrapper.Representative, representative);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader, Certificate);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	ImportH1CommonSendMessageWrapper wrapper;

	ImportH1CommonSendMessageWrapper GetWrapper(CusEntryHeader entryheader, ICertificateProvider certificateData) => new ImportH1CommonSendMessageWrapper(entryheader, certificateData);

	protected override ImportH1CommonSendMessageWrapper GetProvider() => wrapper;
}
