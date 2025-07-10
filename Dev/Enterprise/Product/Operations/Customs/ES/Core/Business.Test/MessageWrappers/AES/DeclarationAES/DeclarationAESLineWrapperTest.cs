using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationAESLineWrapperTest : WrapperHelperTest<DeclarationAESLineWrapper>
{
	public void TestUCRReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty UCRReferenceNumber when Inv. Lines/Line Details/Commercial Reference is empty", ZString.Empty, wrapper.UCRReferenceNumber);

			invoiceLine.ZG_CommercialReference = "reference";
			wrapper = GetWrapper(entryLine);
			AssertEquals("Expected filled UCRReferenceNumber when Inv. Lines/Line Details/Commercial Reference is filled", "reference", wrapper.UCRReferenceNumber);
		});
	}

	public void TestAuthorisations()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Authorisations", 0, wrapper.Authorisations.Count);

			var auth1 = invoiceLine.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "BTI";
			var auth2 = invoiceLine.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "AAA";

			AssertType<CusAuthorizationUsage>(auth1);
			AssertType<CusAuthorizationUsage>(auth2);
			wrapper = GetWrapper(entryLine);
			var authorisations = wrapper.Authorisations;
			AssertEquals("Expected filled Authorisations with count 2 (all codes)", 2, authorisations.Count);
			AssertSame("Cached Authorisations", wrapper.Authorisations, authorisations);
		});
	}

	public void TestProcedure()
	{
		var procedure = wrapper.Procedure;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Procedure", procedure);
			AssertSame("Cached Procedure", wrapper.Procedure, procedure);
		});
	}

	public void TestNullConsignor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<NullReferenceException>("shouldDeclareConsignorInLine flag is false so Consignor is null", () => wrapper.Consignor.ToString());

			wrapper = GetWrapper(entryLine, shouldDeclareConsignorInLine: true);
			invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>("shouldDeclareConsignorInLine flag is true but consignor is not declared so Consignor is null", () => wrapper.Consignor.ToString());
		});
	}

	public void TestConsignor()
	{
		CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ExporterAddress = orgHeader.MainAddress.PK;
			wrapper = GetWrapper(entryLine, shouldDeclareConsignorInLine: true);
			var consignor = wrapper.Consignor;
			AssertNotNull("Expected filled Consignor shouldDeclareConsignorInLine flag is true", wrapper.Consignor);
			AssertSame("Cached Consignor", wrapper.Consignor, consignor);
		});
	}

	public void TestNullConsignee()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInLine flag is false so Consignee is null", () => wrapper.Consignee.ToString());

			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInLine flag is true but importer is not declared so Consignee is null", () => wrapper.Consignee.ToString());
		});
	}

	public void TestConsignee_DontSendImporterId()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Declaration orgHeader";
		var orgAddressMain = orgHeader.MainAddress;
		orgAddressMain.OA_Address1 = "Address Main 1";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address Other 1";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;
		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

		CombineAssertions(() =>
		{
			declaration.ZG_DontSendImporterId = false;
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			var consignee = wrapper.Consignee;
			AssertEquals("Expected Consignee's Name filled when there is no valid CusCode for id even when ZG_DontSendImporterId is false", "Declaration orgHeader", consignee.Name);
			AssertEquals("Expected Consignee's Id empty when there is no valid CusCode for id even when ZG_DontSendImporterId is false", ZString.Empty, consignee.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			consignee = wrapper.Consignee;
			AssertEquals("Expected Consignee's Name empty when there is a valid CusCode for id and ZG_DontSendImporterId is false", ZString.Empty, consignee.Name);
			AssertEquals("Expected Consignee's Id filled when there is a valid CusCode for id and ZG_DontSendImporterId is false", "GB333333333", consignee.Id);

			declaration.ZG_DontSendImporterId = true;
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			consignee = wrapper.Consignee;
			AssertEquals("Expected Consignee's Name filled when there is a valid CusCode for id but ZG_DontSendImporterId is true", "Declaration orgHeader", consignee.Name);
			AssertEquals("Expected Consignee's Id empty when there is a valid CusCode for id but ZG_DontSendImporterId is true", ZString.Empty, consignee.Id);
		});
	}

	public void TestConsignee()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.OH_FullName = "Declaration orgHeader";
		orgHeader1.OH_Code = "Code1";
		var orgAddressMain1 = orgHeader1.MainAddress;
		orgAddressMain1.OA_Address1 = "Address Main 1";

		var orgAddress1 = Factory.New<OrgAddress>();
		orgAddress1.OA_OH = orgHeader1.PK;
		orgAddress1.OA_Address1 = "Address Other 1";

		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_FullName = "Header orgHeader";
		orgHeader2.OH_Code = "Code2";
		var orgAddressMain2 = orgHeader2.MainAddress;
		orgAddressMain2.OA_Address1 = "Address Main 2";

		var orgAddress2 = Factory.New<OrgAddress>();
		orgAddress2.OA_OH = orgHeader2.PK;
		orgAddress2.OA_Address1 = "Address Other 2";

		CombineAssertions(() =>
		{
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader1.PK;
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			var consignee = wrapper.Consignee;
			AssertNotNull("Expected filled Consignee with declaration's importer when shouldDeclareConsigneeInLine flag is true but line has importer empty", consignee);
			AssertEquals("Expected declaration importer in Consignee", "Declaration orgHeader", consignee.Name);
			AssertEquals("Expected declaration importer in Consignee with correct Address", "Address Other 1", consignee.Address.Address);
			AssertSame("Cached Consignee", wrapper.Consignee, consignee);

			invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
			invoiceHeader.JZ_OA_BuyerAddress = orgAddress2.PK;
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			consignee = wrapper.Consignee;
			AssertNotNull("Expected filled Consignee with invoiceHeader's importer when shouldDeclareConsigneeInLine is true and importer is declared", consignee);
			AssertEquals("Expected invoiceheader importer in Consignee", "Header orgHeader", consignee.Name);
			AssertEquals("Expected invoiceheader importer in Consignee with correct Address", "Address Other 2", consignee.Address.Address);

			invoiceHeader.JZ_OA_BuyerAddress = ZGuid.Empty;
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			consignee = wrapper.Consignee;
			AssertNotNull("Expected filled Consignee with invoiceHeader's importer when shouldDeclareConsigneeInLine is true and importer is declared (without address declared)", consignee);
			AssertEquals("Expected invoiceheader importer in Consignee (without address declared)", "Header orgHeader", consignee.Name);
			AssertEquals("Expected invoiceheader importer in Consignee with correct Address (without address declared)", "Address Main 2", consignee.Address.Address);
		});

		CombineAssertions("Consignee.Address.Country should be using default territory if present.", () =>
		{
			invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
			invoiceHeader.JZ_OA_BuyerAddress = orgAddress2.PK;

			orgAddress2.OA_RN_NKCountryCode = "RS";
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.Consignee.Address.Country);

			orgAddress2.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Consignee.Address.Country);

			orgAddress2.OA_RN_NKCountryCode = "MQ";
			wrapper = GetWrapper(entryLine, shouldDeclareConsigneeInLine: true);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.Consignee.Address.Country);
		});
	}

	public void TestAdditionalSupplyActors()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty AdditionalSupplyActors when no data declared", 0, wrapper.AdditionalSupplyActors.Count);

			invoiceLine.CusSupplyChainActorReferences.AddNew();
			wrapper = GetWrapper(entryLine);
			var additionalSupplyActors = wrapper.AdditionalSupplyActors;
			AssertEquals("Expected filled AdditionalSupplyActors", 1, additionalSupplyActors.Count);
			AssertSame("Cached AdditionalSupplyActors", wrapper.AdditionalSupplyActors, additionalSupplyActors);
		});
	}

	public void TestOrigin()
	{
		CombineAssertions(() =>
		{
			var origin = wrapper.Origin;
			AssertNotNull("Expected filled Origin when entryInstruction is not B nor C", origin);
			AssertSame("Cached Origin", wrapper.Origin, origin);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			wrapper = GetWrapper(entryLine);
			AssertNull("Expected empty Origin when entryInstruction is B", wrapper.Origin);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
			AssertNotNull("Expected filled Origin when EntryInstruction is C but isComplementaryCWithMRN is true", wrapper.Origin);

			wrapper = GetWrapper(entryLine);
			AssertNull("Expected empty Origin when EntryInstruction is C but isComplementaryCWithMRN is false", wrapper.Origin);
		});
	}

	public void TestCommodity()
	{
		var commodity = wrapper.Commodity;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Commodity", commodity);
			AssertSame("Cached Commodity", wrapper.Commodity, commodity);
		});
	}

	public void TestInternalPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty InternalPackages list", 0, wrapper.InternalPackages.Count);

			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			packageInfo1.CW_PackType = InternalPackage1.Type;
			pack1.CHC_CW = packageInfo1.PK;
			invoiceLine.PackagesPivot.Add(pack1);

			var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
			packageInfo2.CW_PackType = InternalPackage2.Type;
			pack2.CHC_CW = packageInfo2.PK;
			invoiceLine.PackagesPivot.Add(pack2);

			wrapper = GetWrapper(entryLine);
			var internalPackages = wrapper.InternalPackages;

			AssertEquals("Expected filled InternalPackages", 2, internalPackages.Count);
			AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
		});
	}

	public void TestPreviousDocuments_NotC651()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "9002";

			var supdoc3 = invoiceLine.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			var supdoc4 = invoiceLine.PreviousDocuments.AddNew();
			supdoc4.CSI_Code = "9004";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (can only send 1)", 1, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_OfficePRE()
	{
		var customsOffice = declaration.CustomsOffices.AddNew();
		customsOffice.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		customsOffice.CY_Data = "FR008889";

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "9002";

			var supdoc3 = invoiceLine.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "9003";

			var supdoc4 = invoiceLine.PreviousDocuments.AddNew();
			supdoc4.CSI_Code = "9004";

			var supdoc5 = invoiceLine.PreviousDocuments.AddNew();
			supdoc5.CSI_Code = "C651";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (since there is a PRE customs office multiple docs can be sent)", 5, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_C651()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty DocumentsAndCertificates list", 0, wrapper.PreviousDocuments.Count);

			var supdoc1 = declaration.PreviousDocuments.AddNew();
			supdoc1.CSI_Code = "9001";
			supdoc1.CSI_ReferenceNumber = "ref1";

			var supdoc2 = declaration.PreviousDocuments.AddNew();
			supdoc2.CSI_Code = "C651";
			supdoc2.CSI_ReferenceNumber = "ref2";

			var supdoc3 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc3.CSI_Code = "9002";
			supdoc3.CSI_ReferenceNumber = "ref3";

			var supdoc4 = invoiceHeader.PreviousDocuments.AddNew();
			supdoc4.CSI_Code = "C651";
			supdoc4.CSI_ReferenceNumber = "ref4";

			var supdoc5 = invoiceLine.PreviousDocuments.AddNew();
			supdoc5.CSI_Code = "9003";
			supdoc5.CSI_ReferenceNumber = "ref5";

			var supdoc6 = invoiceLine.PreviousDocuments.AddNew();
			supdoc6.CSI_Code = "C651";
			supdoc6.CSI_ReferenceNumber = "ref6";

			var supdoc7 = invoiceLine.PreviousDocuments.AddNew();
			supdoc7.CSI_Code = "C651";
			supdoc7.CSI_ReferenceNumber = "ref7";

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocuments;

			AssertEquals("Expected filled PreviousDocuments (since type is C651 multiple docs can be sent)", 4, documents.Count);
			AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);
		});
	}

	public void TestPreviousDocuments_UOMAndQuantity_NotC651()
	{
		var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "9001";
		prevDoc1.CSI_ReferenceNumber = "REF1";
		prevDoc1.CSI_UnitOfQuantity = "KGM";
		prevDoc1.CSI_Quantity = 2m;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
		prevDoc2.CSI_Code = "9001";
		prevDoc2.CSI_ReferenceNumber = "REF1";
		prevDoc2.CSI_UnitOfQuantity = "KGM";
		prevDoc2.CSI_Quantity = 1m;

		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocuments.Count", 1, wrapper.PreviousDocuments.Count);
			var document = wrapper.PreviousDocuments.FirstOrDefault();
		
			AssertEquals("Expected sum of the quantities of the same previous document", 3m, document.Quantity);
			AssertEquals("Expected KGM", "KGM", document.Measurement);
		});
	}

	public void TestPreviousDocuments_UOMAndQuantity_OfficePRE()
	{
		var vehicle = invoiceLine.Vehicles.AddNew();
		vehicle.CVH_VehicleIdentificationNumber = "VIN1";
		var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "9001";
		prevDoc1.CSI_ReferenceNumber = "REF1";
		prevDoc1.CSI_Quantity = 2m;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var vehicle2 = invoiceLine2.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "VIN2";
		var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
		prevDoc2.CSI_Code = "9001";
		prevDoc2.CSI_ReferenceNumber = "REF1";
		prevDoc2.CSI_Quantity = 1m;

		var customsOffice = declaration.CustomsOffices.AddNew();
		customsOffice.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		customsOffice.CY_Data = "FR008889";

		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocuments.Count", 1, wrapper.PreviousDocuments.Count);
			var document = wrapper.PreviousDocuments.FirstOrDefault();

			AssertEquals("Expected sum of the number of vehicles if CSI_UnitOfQuantity is empty", 2m, document.Quantity);
			AssertEquals("Expected NAR if CSI_UnitOfQuantity is empty and there are vehicles", Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems, document.Measurement);
		});
	}

	public void TestPreviousDocuments_UOMAndQuantity_C651()
	{
		var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
		prevDoc1.CSI_Code = "C651";
		prevDoc1.CSI_ReferenceNumber = "REF1";
		prevDoc1.CSI_Quantity = 1m;

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var prevDoc2 = invoiceLine2.PreviousDocuments.AddNew();
		prevDoc2.CSI_Code = "C651";
		prevDoc2.CSI_ReferenceNumber = "REF1";
		prevDoc2.CSI_Quantity = 1m;

		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("PreviousDocuments.Count", 1, wrapper.PreviousDocuments.Count);
			var document = wrapper.PreviousDocuments.FirstOrDefault();

			AssertEquals("Expected the quantity of one previous document", 1m, document.Quantity);
			AssertEquals("Expected Empty", ZString.Empty, document.Measurement);
		});
	}

	public void TestSupportingDocuments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9001";

			var supdoc2 = entryInstruction.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "B002";

			var supdoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "A003";

			var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
			supdoc4.CSI_Code = "9004";

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "5005";

			var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
			supdoc6.CSI_Code = "Y004";

			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "10,00", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("CCC", "10,00", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("DDD", "10,00", subType: ZString.Empty, status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("YYY", "10,00", subType: "LIQ", status: ZString.Empty);

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transition period is false (only included those that don't start with Y and are in invoiceHeader and invoiceLines)", 3, documents.Count);
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transition period is false (only included those that don't start with Y and are in invoiceHeader and invoiceLines), ordered name", new ZString[] { "A003", "9004", "5005" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transition period is false (only included those that don't start with Y and are in invoiceHeader and invoiceLines), ordered SequenceNumber", new ZString[] { "1", "2", "3" }, documents.Select(x => x.SequenceNumber));

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
				documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and an extra 1220 one (added automatically)", 4, documents.Count);
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is false with new 1220 doc created with MRN", true, documents.Any(x => x.Name == "1220" && x.Number == "MRNCode"));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and an extra 1220 one (added automatically), ordered name", new ZString[] { "A003", "9004", "5005", "1220" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and an extra 1220 one (added automatically), ordered SequenceNumber", new ZString[] { "1", "2", "3", "4" }, documents.Select(x => x.SequenceNumber));

				invoiceLine.JI_Procedure = "1049F61";
				wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
				documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and no extra 1220 one because additionalProcedure is F61 or 144", 3, documents.Count);
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is false with new 1220 doc created with MRN", false, documents.Any(x => x.Name == "1220"));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and no extra 1220 one because additionalProcedure is F61 or 144, ordered name", new ZString[] { "A003", "9004", "5005" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and no extra 1220 one because additionalProcedure is F61 or 144, ordered SequenceNumber", new ZString[] { "1", "2", "3" }, documents.Select(x => x.SequenceNumber));

				var supdoc7 = invoiceLine.SupportingDocuments.AddNew();
				supdoc7.CSI_Code = "1220";
				supdoc7.CSI_ReferenceNumber = "Reference";
				wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
				documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and no extra 1220 one because it's alredy declared in invoiceLine", 4, documents.Count);
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is false with new 1220 doc existing in invoiceLine", true, documents.Any(x => x.Name == "1220" && x.Number == "Reference"));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and no extra 1220 one because it's alredy declared in invoiceLine, ordered name", new ZString[] { "A003", "9004", "5005", "1220" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is false (only included those that don't start with Y) and no extra 1220 one because it's alredy declared in invoiceLine, ordered SequenceNumber", new ZString[] { "1", "2", "3", "4" }, documents.Select(x => x.SequenceNumber));

				invoiceLine.SupportingDocuments.RemoveAndDelete(supdoc7);
				invoiceLine.JI_Procedure = ZString.Empty;
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 when no CMP, C and MRN", 7, documents.Count);
				AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 when no CMP, C and MRN, ordered name", new ZString[] { "A003", "B002", "BBB", "CCC", "9004", "5005", "9001" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 when no CMP, C and MRN, ordered SequenceNumber", new ZString[] { "1", "2", "3", "4", "5", "6", "7" }, documents.Select(x => x.SequenceNumber));

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
				documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and an extra 1220 one (added automatically)", 8, documents.Count);
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true with new 1220 doc created with MRN", true, documents.Any(x => x.Name == "1220" && x.Number == "MRNCode"));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and an extra 1220 one (added automatically), ordered name", new ZString[] { "A003", "B002", "BBB", "CCC", "9004", "5005", "9001", "1220" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and an extra 1220 one (added automatically), ordered SequenceNumber", new ZString[] { "1", "2", "3", "4", "5", "6", "7", "8" }, documents.Select(x => x.SequenceNumber));

				invoiceLine.JI_Procedure = "1049F61";
				wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
				documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 one because additionalProcedure is F61 or 144", 7, documents.Count);
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true with new 1220 doc created with MRN", false, documents.Any(x => x.Name == "1220"));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 one because additionalProcedure is F61 or 144, ordered name", new ZString[] { "A003", "B002", "BBB", "CCC", "9004", "5005", "9001" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 one because additionalProcedure is F61 or 144, ordered SequenceNumber", new ZString[] { "1", "2", "3", "4", "5", "6", "7" }, documents.Select(x => x.SequenceNumber));

				var supdoc7 = invoiceLine.SupportingDocuments.AddNew();
				supdoc7.CSI_Code = "1220";
				supdoc7.CSI_ReferenceNumber = "Reference";
				wrapper = GetWrapper(entryLine, isComplementaryCWithMRN: true);
				documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 one because it's alredy declared in invoiceLine", 8, documents.Count);
				AssertEquals("Expected filled SupportingDocuments when transitionPeriod is true with 1220 doc existing in invoiceLine", true, documents.Any(x => x.Name == "1220" && x.Number == "Reference"));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 one because it's alredy declared in invoiceLine, ordered name", new ZString[] { "A003", "B002", "BBB", "CCC", "9004", "5005", "1220", "9001" }, documents.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transitionPeriod is true (only included those that don't start with Y and also those in entryLine sup docs with LIQ subtype and not ACC status) and no extra 1220 one because it's alredy declared in invoiceLine, ordered SequenceNumber", new ZString[] { "1", "2", "3", "4", "5", "6", "7", "8" }, documents.Select(x => x.SequenceNumber));
			}

			AssertNoExceptionThrown("When any Supporting Document has null or empty CSI_Code no exception should be thrown", () =>
			{
				var supDoc8 = invoiceHeader.SupportingDocuments.AddNew();
				supDoc8.CSI_Code = ZString.Empty;

				var supdoc9 = declaration.SupportingDocuments.AddNew();
				supdoc9.CSI_Code = ZString.Empty;
				_ = declaration.SupportingDocuments;

				entryLine.AddEntryLineDocument<SupportingDocument>(ZString.Empty, "10,00", subType: "LIQ", status: ZString.Empty);
				wrapper = GetWrapper(entryLine);
				_ = wrapper.SupportingDocuments;
			});
		});
	}

	public void TestTransportDocuments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TransportDocuments list", 0, wrapper.TransportDocuments.Count);

			var supdoc1 = declaration.AdditionalInfos.AddNew();
			supdoc1.CSI_Code = "9001";
			supdoc1.CSI_SubType = "TRA";

			var supdoc1INF = declaration.AdditionalInfos.AddNew();
			supdoc1INF.CSI_Code = "Y001";
			supdoc1INF.CSI_SubType = "INF";

			var supdoc2 = entryInstruction.AdditionalInfos.AddNew();
			supdoc2.CSI_Code = "9002";
			supdoc2.CSI_SubType = "TRA";

			var supdoc2INF = entryInstruction.AdditionalInfos.AddNew();
			supdoc2INF.CSI_Code = "Y002";
			supdoc2INF.CSI_SubType = "INF";

			var supdoc3 = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3.CSI_Code = "9003";
			supdoc3.CSI_SubType = "TRA";

			var supdoc3INF = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3INF.CSI_Code = "Y003";
			supdoc3INF.CSI_SubType = "INF";

			var supdoc4 = invoiceLine.AdditionalInfos.AddNew();
			supdoc4.CSI_Code = "9004";
			supdoc4.CSI_SubType = "TRA";

			var supdoc5 = invoiceLine.AdditionalInfos.AddNew();
			supdoc5.CSI_Code = "9005";
			supdoc5.CSI_SubType = "TRA";

			var supdoc6 = invoiceLine.AdditionalInfos.AddNew();
			supdoc6.CSI_Code = "Y004";
			supdoc6.CSI_SubType = "INF";

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected empty TransportDocuments list when declared but transitionPeriod is false", 0, wrapper.TransportDocuments.Count);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.TransportDocuments;
				AssertEquals("Expected filled TransportDocuments with number 9001 for Misc)", true, documents.Any(x => x.Name == "9001"));
				AssertEquals("Expected filled TransportDocuments with number 9002 for EntryInstruction)", true, documents.Any(x => x.Name == "9002"));
				AssertEquals("Expected filled TransportDocuments with number 9003 for Header)", true, documents.Any(x => x.Name == "9003"));
				AssertEquals("Expected filled TransportDocuments with number 9004 for Lines)", true, documents.Any(x => x.Name == "9004"));
				AssertEquals("Expected filled TransportDocuments with number 9005 for Lines)", true, documents.Any(x => x.Name == "9005"));

				AssertEquals("Expected filled TransportDocuments when transition period is true (included those that that have subType TRA and are in declaration, entryInstruction, invoiceHeader and invoiceLine)", 5, documents.Count);
				AssertSame("Cached TransportDocuments", wrapper.TransportDocuments, documents);
			}
		});
	}

	public void TestAdditionalReferences()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty AdditionalReferences list", 0, wrapper.AdditionalReferences.Count);

			var supdoc1 = declaration.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "Y001";

			var supdoc2 = entryInstruction.SupportingDocuments.AddNew();
			supdoc2.CSI_Code = "Y002";

			var supdoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supdoc3.CSI_Code = "Y003";

			var supdoc4 = invoiceLine.SupportingDocuments.AddNew();
			supdoc4.CSI_Code = "9003";

			var supdoc5 = invoiceLine.SupportingDocuments.AddNew();
			supdoc5.CSI_Code = "9004";

			var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
			supdoc6.CSI_Code = "Y004";

			var addRef1 = invoiceLine.AdditionalInfos.AddNew();
			addRef1.CSI_SubType = "REF";

			var addRef2 = invoiceHeader.AdditionalInfos.AddNew();
			addRef2.CSI_SubType = "REF";

			var addRef3 = invoiceLine.AdditionalInfos.AddNew();
			addRef3.CSI_SubType = "INF";

			var addRef4 = invoiceHeader.AdditionalInfos.AddNew();
			addRef4.CSI_SubType = "TRA";

			var addRef5 = declaration.AdditionalInfos.AddNew();
			addRef5.CSI_SubType = "REF";

			var addRef6 = entryInstruction.AdditionalInfos.AddNew();
			addRef6.CSI_SubType = "REF";

			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("Y111", "10,00", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("Y2222", "10,00", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("DDD", "10,00", subType: ZString.Empty, status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("AAA", "10,00", subType: "LIQ", status: ZString.Empty);

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.AdditionalReferences;
				AssertEquals("Expected filled AdditionalReferences when transition period is true (only included those that start with Y and are in declaration, entryInstruction, invoiceHeader and invoiceLines and entryLine with LIQ subtype and not ACC status)", 6, documents.Count);
				AssertSame("Cached AdditionalReferences", wrapper.AdditionalReferences, documents);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.AdditionalReferences;
				AssertEquals("Expected filled AdditionalReferences when transition period is false (only included those that start with Y and are in invoiceHeader and invoiceLines), (also include AdditionalInfos where CSI_SubType = 'REF' from InvoiceLines and InvoiceHeaders).", 4, documents.Count);
				AssertSame("Cached AdditionalReferences", wrapper.AdditionalReferences, documents);
			}
		});
	}

	public void TestAdditionalInfos()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty AdditionalInfos list", 0, wrapper.AdditionalInfos.Count);

			var supdoc1 = declaration.AdditionalInfos.AddNew();
			supdoc1.CSI_Code = "9001";
			supdoc1.CSI_SubType = "TRA";
			supdoc1.CSI_ReferenceNumber = "Ref1TRA";

			var supdoc1INF = declaration.AdditionalInfos.AddNew();
			supdoc1INF.CSI_Code = "Y001";
			supdoc1INF.CSI_SubType = "INF";
			supdoc1INF.CSI_ReferenceNumber = "Ref1";

			var supdoc2 = entryInstruction.AdditionalInfos.AddNew();
			supdoc2.CSI_Code = "9002";
			supdoc2.CSI_SubType = "TRA";
			supdoc2.CSI_ReferenceNumber = "Ref2TRA";

			var supdoc2INF = entryInstruction.AdditionalInfos.AddNew();
			supdoc2INF.CSI_Code = "Y002";
			supdoc2INF.CSI_SubType = "INF";
			supdoc2INF.CSI_ReferenceNumber = "Ref2";

			var supdoc3 = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3.CSI_Code = "9003";
			supdoc3.CSI_SubType = "TRA";
			supdoc3.CSI_ReferenceNumber = "Ref3TRA";

			var supdoc3INF = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3INF.CSI_Code = "Y003";
			supdoc3INF.CSI_SubType = "INF";
			supdoc3INF.CSI_ReferenceNumber = "Ref3";

			var supdoc4 = invoiceLine.AdditionalInfos.AddNew();
			supdoc4.CSI_Code = "9004";
			supdoc4.CSI_SubType = "TRA";
			supdoc4.CSI_ReferenceNumber = "Ref4";

			var supdoc5 = invoiceLine.AdditionalInfos.AddNew();
			supdoc5.CSI_Code = "9005";
			supdoc5.CSI_SubType = "INF";
			supdoc5.CSI_ReferenceNumber = "Ref5";

			var supdoc6 = invoiceLine.AdditionalInfos.AddNew();
			supdoc6.CSI_Code = "Y004";
			supdoc6.CSI_SubType = "INF";
			supdoc6.CSI_ReferenceNumber = "Ref6";

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.AdditionalInfos;
				AssertContainsExactElementsInAnyOrder("Expected correct Number for all documents", new ZString[] { "Ref1", "Ref2", "Ref3", "Ref5", "Ref6" }, documents.Select(x => x.Number));
				AssertEquals("Expected filled AdditionalInfos when transition period is true (only included those that that have subType INF and are in declaration, entryInstruction, invoiceHeader and invoiceLines)", 5, documents.Count);
				AssertSame("Cached AdditionalInfos", wrapper.AdditionalInfos, documents);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				wrapper = GetWrapper(entryLine);
				var documents = wrapper.AdditionalInfos;
				AssertContainsExactElementsInAnyOrder("Expected correct Number for all documents", new ZString[] { ZString.Empty, ZString.Empty, ZString.Empty }, documents.Select(x => x.Number));
				AssertEquals("Expected filled AdditionalInfos when transition period is false (only included those that that have subType INF and are in invoiceHeader and invoiceLines)", 3, documents.Count);
				AssertSame("Cached AdditionalInfos", wrapper.AdditionalInfos, documents);
			}
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
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];
		entryLine = entryHeader.MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	DeclarationAESLineWrapper wrapper;

	DeclarationAESLineWrapper GetWrapper(CusEntryLine entryLine, bool shouldDeclareConsignorInLine = false, bool shouldDeclareConsigneeInLine = false, bool isComplementaryCWithMRN = false) => new DeclarationAESLineWrapper(entryLine, shouldDeclareConsignorInLine, shouldDeclareConsigneeInLine, isComplementaryCWithMRN);

	protected override DeclarationAESLineWrapper GetProvider() => wrapper;
}
