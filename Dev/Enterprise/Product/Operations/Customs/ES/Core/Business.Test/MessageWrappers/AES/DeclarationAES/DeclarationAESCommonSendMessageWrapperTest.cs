using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationAESCommonSendMessageWrapperTest : WrapperHelperTest<DeclarationAESCommonSendMessageWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => GetWrapper(null, Certificate));

			AssertExceptionThrown("Constructor Throws Exception if certificate is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","certificate"), () => GetWrapper(entryHeader, null));

			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(Factory.New<CusEntryHeader>(), Certificate));

			AssertExceptionThrown("Constructor Throws Exception if messageSubType is empty", typeof(ArgumentException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").","messageSubType"), () => GetWrapper(entryHeader, Certificate, ZString.Empty));
		});
	}

	public void TestAuthorisations()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Authorisations", 0, wrapper.Authorisations.Count);

			var auth1 = entryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "TRD";
			var auth2 = entryInstruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "AAA";

			wrapper = GetWrapper(entryHeader, Certificate);
			var authorisations = wrapper.Authorisations;
			AssertEquals("Expected filled Authorisations with count 2 (all codes)", 2, authorisations.Count);
			AssertSame("Cached Authorisations", wrapper.Authorisations, authorisations);
		});
	}

	public void TestCustomOfficeOfPresentation()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty CustomOfficeOfPresentation when no OfficeOfPresentation is declared", ZString.Empty, wrapper.CustomOfficeOfPresentation);

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			customsOffice1.CY_Data = "FR008889";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfPresentation with OfficeOfExport office code when declared in CustomsOffice list", "FR008889", wrapper.CustomOfficeOfPresentation);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but entryInstruction is C, Y or Z", ZString.Empty, wrapper.CustomOfficeOfPresentation);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation is declared and entryInstruction is not C, Y or Z", "FR008889", wrapper.CustomOfficeOfPresentation);

			declaration.JE_CustomsOffice = "ES005500";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but CustomsOffice starts with ES0055 or ES0056", ZString.Empty, wrapper.CustomOfficeOfPresentation);

			declaration.JE_CustomsOffice = "ES009900";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation is declared and CustomsOffice does not start with ES0055 nor ES0056", "FR008889", wrapper.CustomOfficeOfPresentation);

			var customsOffice2 = declaration.CustomsOffices.AddNew();
			customsOffice2.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			customsOffice2.CY_Data = "ES005600";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but OfficeOfExport starts with ES0055 or ES0056", ZString.Empty, wrapper.CustomOfficeOfPresentation);

			customsOffice2.CY_Data = "ES002800";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation is declared and OfficeOfExport does not start with ES0055 nor ES0056", "FR008889", wrapper.CustomOfficeOfPresentation);

			customsOffice1.CY_Data = ZString.Empty;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but empty", ZString.Empty, wrapper.CustomOfficeOfPresentation);
		});
	}

	public void TestCustomOfficeOfExport()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExport with CustomsOffice office code when no other customsOffice is declared", "ES009999", wrapper.CustomOfficeOfExport);

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			customsOffice1.CY_Data = "FR008889";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExport with OfficeOfExport office code when declared alone in CustomsOffice list", "FR008889", wrapper.CustomOfficeOfExport);

			customsOffice1.CY_Data = ZString.Empty;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExport with CustomsOffice office code when declared Empty OfficeOfExport", "ES009999", wrapper.CustomOfficeOfExport);
		});
	}

	public void TestCustomOfficeOfExit()
	{
		CombineAssertions(() =>
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "ES009999";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExit with CustomsOffice office code when no other customsOffice is declared", "ES009999", wrapper.CustomOfficeOfExit);

			var customsOffice1 = declaration.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			customsOffice1.CY_Data = "FR008889";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExit with OfficeOfExport office code when declared alone in CustomsOffice list", "FR008889", wrapper.CustomOfficeOfExit);

			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOffice1.CY_Data = "ES009998";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExit with OfficeOfExit office code when declared alone in CustomsOffice list", "ES009998", wrapper.CustomOfficeOfExit);

			customsOffice1.CY_Data = ZString.Empty;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExit with CustomsOffice office code when declared Empty OfficeOfExit an no OfficeOfExport declared", "ES009999", wrapper.CustomOfficeOfExit);

			customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExit with CustomsOffice office code when declared Empty OfficeOfExport an no OfficeOfExit declared", "ES009999", wrapper.CustomOfficeOfExit);

			customsOffice1.CY_Data = "FR008889";
			var customsOffice2 = declaration.CustomsOffices.AddNew();
			customsOffice2.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOffice2.CY_Data = "ES009998";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled CustomOfficeOfExit with OfficeOfExit office code when declared all (CustomsOffice, OfficeOfExport and OfficeOfExit)", "ES009998", wrapper.CustomOfficeOfExit);

			customsOffice2.CY_Data = ZString.Empty;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled OfficeOfExport with OfficeOfExit office code when declared all (CustomsOffice, OfficeOfExport and OfficeOfExit) but OfficeOfExit is empty", "FR008889", wrapper.CustomOfficeOfExit);
		});
	}

	public void TestNullExporter()
	{
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Exporter.ToString());
	}

	public void TestExporter()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
		orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeader.OH_Code = "Code";

		var orgAddressMain = orgHeader.MainAddress;
		orgAddressMain.OA_Address1 = "Address Main";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address Other";

		declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;
		declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

		CombineAssertions(() =>
		{
			var exporter = wrapper.Exporter;

			AssertNotNull("Expected filled Exporter", exporter);
			AssertSame("Cached Exporter", wrapper.Exporter, exporter);

			AssertEquals("Expected filled Address correctly", "Address Other", exporter.Address.Address);

			declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			declaration.JE_OH_Supplier = orgHeader.PK;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled Address correctly whena ddress not specified", "Address Main", wrapper.Exporter.Address.Address);
		});

		CombineAssertions("Exporter.Address.Country should use default territory if present.", () =>
		{
			orgAddress.OA_RN_NKCountryCode = "RS";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.Exporter.Address.Country);

			orgAddress.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Exporter.Address.Country);

			orgAddress.OA_RN_NKCountryCode = "MQ";
			wrapper = GetWrapper(entryHeader, Certificate);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.Exporter.Address.Country);
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
		orgAddressMain.OA_Email = "mail@mail.com";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Email = "mail@other.com";

		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;

		CombineAssertions(() =>
		{
			var declarant = wrapper.Declarant;

			AssertNotNull("Expected filled Declarant", declarant);
			AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			AssertEquals("ContactPerson Email is taken from the correct address, not the main one", "mail@other.com", declarant.ContactPerson.Email);
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

	public void TestGoodsShipment()
	{
		var goodsShipment = wrapper.GoodsShipment;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled GoodsShipment", goodsShipment);
			AssertSame("Cached GoodsShipment", wrapper.GoodsShipment, goodsShipment);
		});
	}

	public void TestIsComplementaryCWithMRN()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected false IsComplementaryCWithMRN when messageSubType is ORG", false, wrapper.IsComplementaryCWithMRN);

			wrapper = GetWrapper(entryHeader, Certificate, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
			AssertEquals("Expected false IsComplementaryCWithMRN when messageSubType is CMP but entryInstruction is not C and there is no MRN", false, wrapper.IsComplementaryCWithMRN);

			entryHeader.MovementReferenceNumberSetter("MRNCode");
			wrapper = GetWrapper(entryHeader, Certificate, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
			AssertEquals("Expected false IsComplementaryCWithMRN when messageSubType is CMP, there is MRN but entryInstruction is not C", false, wrapper.IsComplementaryCWithMRN);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader, Certificate, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
			AssertEquals("Expected true IsComplementaryCWithMRN when messageSubType is CMP, there is MRN and entryInstruction is C", true, wrapper.IsComplementaryCWithMRN);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("Expected cached value (entry instruction was changed but wrapper was not created ahain so value is still true)", true, wrapper.IsComplementaryCWithMRN);
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
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader, Certificate);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	CusEntryHeader entryHeader;
	DeclarationAESCommonSendMessageWrapper wrapper;

	DeclarationAESCommonSendMessageWrapper GetWrapper(CusEntryHeader entryheader, ICertificateProvider certificateData, string messageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration) => new DeclarationAESCommonSendMessageWrapper(entryheader, certificateData, messageSubType);

	protected override DeclarationAESCommonSendMessageWrapper GetProvider() => wrapper;
}
