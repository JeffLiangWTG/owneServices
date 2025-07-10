using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDHeaderWrapperTest : WrapperHelperTest<DeclarationDVDHeaderWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if declaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(Factory.New<CusEntryHeader>()));
			});
		}

		public void TestCustomsOffice()
		{
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected filled CustomsOffice", "ES009999", wrapper.CustomsOffice);
		}

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "ES00001";
			AssertEquals("Expected filled LRN with only entry ref num when no declarant with id is declared", "ES00001", wrapper.LRN);
		}

		public void TestDeclarationType()
		{
			declaration.JE_MessageSubType = "IM";
			AssertEquals("Expected filled DeclarationType", "IM", wrapper.DeclarationType);
		}

		public void TestDeclarationSubType()
		{
			AssertEquals("Expected filled DeclarationSubType", "A", wrapper.DeclarationSubType);
		}

		public void TestTotalGrossMass()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_Weight = 0.9886M;
				AssertEquals("Expected filled TotalGrossMass when weight < 1", 0.989M, wrapper.TotalGrossMass);

				invoiceLine.JI_Weight = 200.4455m;
				AssertEquals("Weight > 1 not rounded to the upper integer unit but JI_Weight is mark as 3 decimal places", 200.446m, wrapper.TotalGrossMass);

				var entryLine2 = entryHeader.MergedLines.AddNew();
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine2.JI_Weight = 2.3211m;
				invoiceLine2.JI_CL = entryLine2.PK;
				AssertEquals("Expected filled TotalGrossMass when weight > 1 not rounded to the upper integer unit with 2 invoice lines", 202.767m, wrapper.TotalGrossMass);
			});
		}

		public void TestExporterId()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", "76", "20", "123", "Desc", "EXP", outOfWarehouse: true);
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7620123";
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "1049123";
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ExporterId when supplier is not declared", ZString.Empty, wrapper.ExporterId);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Supplier = orgHeader.PK;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ExporterId when supplier has no id", ZString.Empty, wrapper.ExporterId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected NIF ExporterId", "NIF22222222", wrapper.ExporterId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected Country+NIF ExporterId for non NAT Organizations", "ESNIF22222222", wrapper.ExporterId);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected EORI ExporterId with country code", "FR22222222", wrapper.ExporterId);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected EORI ExporterId with country code not repeated", "ES22222222", wrapper.ExporterId);

				invoiceLine.JI_Procedure = "7920123";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ExporterId when supplier has id but requested regime is not 76 or 77 in any invoiceLine", ZString.Empty, wrapper.ExporterId);
			});
		}

		public void TestConsigneeId()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", "71", "20", "123", "Desc", "EXP", outOfWarehouse: true);
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7120123";
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "1049123";
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ConsigneeId when importer is not declared", ZString.Empty, wrapper.ConsigneeId);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Importer = orgHeader.PK;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ConsigneeId when importer has no id", ZString.Empty, wrapper.ConsigneeId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected NIF ConsigneeId", "NIF22222222", wrapper.ConsigneeId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected Country+NIF ConsigneeId for non NAT Organizations", "ESNIF22222222", wrapper.ConsigneeId);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected EORI ConsigneeId with country code", "FR22222222", wrapper.ConsigneeId);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected EORI ConsigneeId with country code not repeated", "ES22222222", wrapper.ConsigneeId);

				invoiceLine.JI_Procedure = "7920123";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ConsigneeId when importer has id but requested regime is not 71, 78 or 95 (it is 79) in any invoiceLine", ZString.Empty, wrapper.ConsigneeId);

				invoiceLine.JI_Procedure = "7820123";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ConsigneeId when importer has id and requested regime is 78 in at least one invoiceLine", "ES22222222", wrapper.ConsigneeId);

				invoiceLine.JI_Procedure = "8020123";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty ConsigneeId when importer has id but requested regime is not 71, 78 or 95 (it is 80) in any invoiceLine", ZString.Empty, wrapper.ConsigneeId);

				invoiceLine.JI_Procedure = "9520123";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ConsigneeId when importer has id and requested regime is 95 in at least one invoiceLine", "ES22222222", wrapper.ConsigneeId);
			});
		}

		public void TestDeclarantUECode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", "76", "20", "123", "Desc", "EXP", outOfWarehouse: true);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", "71", "20", "123", "Desc", "EXP", outOfWarehouse: true);
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7620123";
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "7120123";
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty DeclarantUECode when declarant is not declared", ZString.Empty, wrapper.DeclarantUECode);

				var orgHeaderImporter = Factory.NewWithValidTestData<OrgHeader>();
				OrgCusCode eoriCusCodeImporter = orgHeaderImporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "11111111", "FR");
				declaration.JE_OH_Importer = orgHeaderImporter.PK;

				var orgHeaderDeclant = Factory.NewWithValidTestData<OrgHeader>();
				OrgCusCode eoriCusCodeDeclarant = orgHeaderDeclant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				orgHeaderDeclant.Addresses.AddNew();
				declaration.Declarant.OA_OH = orgHeaderDeclant.PK;

				var orgHeaderSupplier = Factory.NewWithValidTestData<OrgHeader>();
				OrgCusCode eoriCusCodeSupplier = orgHeaderSupplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "33333333", "FR");
				declaration.JE_OH_Supplier = orgHeaderSupplier.PK;

				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty DeclarantUECode when declarant id is different from consignee id and exporter id", ZString.Empty, wrapper.DeclarantUECode);

				eoriCusCodeDeclarant.OK_CustomsRegNo = "11111111";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected 00500 DeclarantUECode when declarant id is the same as importer id", "00500", wrapper.DeclarantUECode);

				eoriCusCodeDeclarant.OK_CustomsRegNo = "33333333";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected 00400 DeclarantUECode when declarant id is the same as supploer id", "00400", wrapper.DeclarantUECode);
			});
		}

		public void TestDeclarantId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DeclarantId when declarant is not declared", ZString.Empty, wrapper.DeclarantId);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.Addresses.AddNew();
				declaration.Declarant.OA_OH = orgHeader.PK;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty DeclarantId when declarant has no id", ZString.Empty, wrapper.DeclarantId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected NIF DeclarantId", "NIF22222222", wrapper.DeclarantId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected Country+NIF DeclarantId for non NAT Organizations", "ESNIF22222222", wrapper.DeclarantId);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected EORI DeclarantId with country code", "FR22222222", wrapper.DeclarantId);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected EORI DeclarantId with country code not repeated", "ES22222222", wrapper.DeclarantId);
			});
		}

		public void TestDeclarationEmail_CustomsClearanceEmailRecipient()
		{
			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, wrapper.DeclarationEmail);
			}
		}

		public void TestDeclarationEmail_MailboxEmailAddress()
		{
			const string mailboxEmail = "mail2.mail@mail.com";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.DeclarationEmail);
			}
		}

		public void TestDeclarationEmail_Empty()
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.DeclarationEmail);
			}
		}

		public void TestDeclarationOtherEmail()
		{
			declaration.ZG_OtherEmailAddr = "other.mail@mail.com";
			AssertEquals("Expected filled DeclarationOtherEmail", "other.mail@mail.com", wrapper.DeclarationOtherEmail);
		}

		public void TestRepresentativeId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty RepresentativeId when importer is not declared", ZString.Empty, wrapper.RepresentativeId);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.Addresses.AddNew();
				declaration.JE_OA_Representative = orgHeader.MainAddress.PK;

				AssertEquals("Expected empty RepresentativeId when representative has no id", ZString.Empty, wrapper.RepresentativeId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF RepresentativeId", "NIF22222222", wrapper.RepresentativeId);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF RepresentativeId for non NAT Organizations", "ESNIF22222222", wrapper.RepresentativeId);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI RepresentativeId with country code", "FR22222222", wrapper.RepresentativeId);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI RepresentativeId with country code not repeated", "ES22222222", wrapper.RepresentativeId);
			});
		}

		public void TestRepresentativeType()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = "4";
				AssertEquals("Expected filled RepresentativeType", "4", wrapper.RepresentativeType);

				declaration.JE_DeclarantType = "DIR";
				AssertEquals("Expected filled mapped RepresentativeType DIR => 2", "2", wrapper.RepresentativeType);

				declaration.JE_DeclarantType = "IND";
				AssertEquals("Expected filled mapped RepresentativeType IND => 3", "3", wrapper.RepresentativeType);
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

				AssertType<CusAuthorizationUsage>(auth1);
				AssertType<CusAuthorizationUsage>(auth2);
				wrapper = GetWrapper(entryHeader);
				var authorisations = wrapper.Authorisations;
				AssertEquals("Expected filled Authorisations with count 2 (all codes)", 2, authorisations.Count);
				AssertSame("Cached Authorisations", wrapper.Authorisations, authorisations);
			});
		}

		public void TestTransportCode()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Expected filled TransportCode Sea (1)", "1", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("Expected filled TransportCode Rail (2)", "2", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("Expected filled TransportCode Road (3)", "3", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Expected filled TransportCode Air (4)", "4", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Expected filled TransportCode Mail (5)", "5", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("Expected filled TransportCode FixedTransportInstallations (7)", "7", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("Expected filled TransportCode InlandWaterwayTransport (8)", "8", wrapper.TransportCode);

				declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
				AssertEquals("Expected filled TransportCode OwnPropulsion (9)", "9", wrapper.TransportCode);
			});
		}

		public void TestAdditionalSupplyActors()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalSupplyActors when no data declared", 0, wrapper.AdditionalSupplyActors.Count);

				entryInstruction.CusSupplyChainActorReferences.AddNew();
				wrapper = GetWrapper(entryHeader, shouldDeclareAddSupplyActorsInHeader: true);
				var additionalSupplyActors = wrapper.AdditionalSupplyActors;
				AssertEquals("Expected filled AdditionalSupplyActors when shouldDeclareSupplyChainActorsInHeader is true", 1, additionalSupplyActors.Count);
				AssertSame("Cached AdditionalSupplyActors", wrapper.AdditionalSupplyActors, additionalSupplyActors);

				wrapper = GetWrapper(entryHeader, shouldDeclareAddSupplyActorsInHeader: false);
				AssertEquals("Expected empty AdditionalSupplyActors when shouldDeclareSupplyChainActorsInHeader is false", 0, wrapper.AdditionalSupplyActors.Count);
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
				supdoc2.CSI_Code = "9002";

				var supdoc3 = invoiceLine.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "9003";

				wrapper = GetWrapper(entryHeader);
				var documents = wrapper.SupportingDocuments;
				AssertEquals("Expected filled SupportingDocuments (only included those in declaration and entry instructions)", 2, documents.Count);
				AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
			});
		}

		public void TestIsContainerised()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false IsContainerised", false, wrapper.IsContainerised);

				var containerTag = "CONTAINER";
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = containerTag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
				AssertEquals("Expected true IsContainerised", true, wrapper.IsContainerised);
			});
		}

		public void TestLocationOfGoods()
		{
			var locationOfGoods = wrapper.LocationOfGoods;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled LocationOfGoods", locationOfGoods);
				AssertSame("Cached LocationOfGoods", wrapper.LocationOfGoods, locationOfGoods);
			});
		}

		public void TestCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsDestination = "FR";
				wrapper = GetWrapper(entryHeader, shouldDeclareCountryOfDestinationInHeader: true);
				AssertEquals("Expected filled CountryOfDestination when shouldDeclareCountryOfDestinationInHeader is true", "FR", wrapper.CountryOfDestination);

				wrapper = GetWrapper(entryHeader, shouldDeclareCountryOfDestinationInHeader: false);
				AssertEquals("Expected empty CountryOfDestination when shouldDeclareCountryOfDestinationInHeader is false", ZString.Empty, wrapper.CountryOfDestination);
			});
		}

		public void TestCountryOfExport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsOrigin = "ES";
				wrapper = GetWrapper(entryHeader, shouldDeclareCountryOfExportInHeader: true);
				AssertEquals("Expected filled CountryOfExport when shouldDeclareCountryOfExporterInHeader is true", "ES", wrapper.CountryOfExport);

				wrapper = GetWrapper(entryHeader, shouldDeclareCountryOfExportInHeader: false);
				AssertEquals("Expected empty CountryOfExport when shouldDeclareCountryOfExporterInHeader is false", ZString.Empty, wrapper.CountryOfExport);
			});
		}

		public void TestUCRReferenceNumber()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_CommercialReference = "reference";
				AssertEquals("Expected empty UCRReferenceNumber when shouldDeclareUCRInHeader flag is false", ZString.Empty, wrapper.UCRReferenceNumber);

				wrapper = GetWrapper(entryHeader, shouldDeclareUCRInHeader: true);
				AssertEquals("Expected filled UCRReferenceNumber when shouldDeclareUCRInHeader flag is true", "reference", wrapper.UCRReferenceNumber);
			});
		}

		public void TestWarehouse()
		{
			CombineAssertions(() =>
			{
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when no authorization declared", wrapper.Warehouse);

				var auth1 = entryInstruction.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = "CW1";
				wrapper = GetWrapper(entryHeader);
				var warehouse = wrapper.Warehouse;
				AssertNotNull("Expected filled Warehouse when an authorization of the expected type is declared", warehouse);
				AssertSame("Cached Warehouse", wrapper.Warehouse, warehouse);

				auth1.AGC_Code = "AAA";
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when authorization code is not in the expected list", wrapper.Warehouse);
			});
		}

		public void TestPreviousDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty PreviousDocuments list", 0, wrapper.PreviousDocuments.Count);

				var prevdoc1 = declaration.PreviousDocuments.AddNew();
				prevdoc1.CSI_Code = "9001";

				var prevdoc2 = invoiceHeader.PreviousDocuments.AddNew();
				prevdoc2.CSI_Code = "9002";

				var prevdoc3 = invoiceLine.PreviousDocuments.AddNew();
				prevdoc3.CSI_Code = "9003";

				wrapper = GetWrapper(entryHeader, shouldDeclarePreviousDocumentsInHeader: true);
				var documents = wrapper.PreviousDocuments;
				AssertEquals("Expected filled PreviousDocuments when shouldDeclarePreviousDocumentsInHeader is true (only from declarationa nd invoice header)", 2, documents.Count);
				AssertSame("Cached PreviousDocuments", wrapper.PreviousDocuments, documents);

				wrapper = GetWrapper(entryHeader, shouldDeclarePreviousDocumentsInHeader: false);
				AssertEquals("Expected empty PreviousDocuments when shouldDeclarePreviousDocumentsInHeader is false", 0, wrapper.PreviousDocuments.Count);
			});
		}

		public void TestGuarantees()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Guarantees when no data declared", 0, wrapper.Guarantees.Count);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, ""), (entryInstruction.PK, ""), (entryInstruction2.PK, ""));

				wrapper = GetWrapper(entryHeader);
				var guarantees = wrapper.Guarantees;
				AssertEquals("Expected filled Guarantees with only 2 guarantees (only get the ones with same entryInstruction as the entryHeader)", 2, guarantees.Count);
				AssertSame("Cached Guarantees", wrapper.Guarantees, guarantees);
			});
		}

		public void TestCustomOfficeOfPresentation()
		{
			CombineAssertions(() =>
			{
				declaration.CustomsOffices.RemoveAndDeleteAll();
				declaration.JE_CustomsOffice = "ES009999";

				var customsOffice1 = declaration.CustomsOffices.AddNew();
				customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				customsOffice1.CY_Data = "FR008889";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled CustomOfficeOfPresentation when OfficeOfPresentation declared", "FR008889", wrapper.CustomOfficeOfPresentation);

				customsOffice1.CY_Data = ZString.Empty;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is declared but empty", ZString.Empty, wrapper.CustomOfficeOfPresentation);

				customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExport;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty CustomOfficeOfPresentation when OfficeOfPresentation is not declared", ZString.Empty, wrapper.CustomOfficeOfPresentation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		DeclarationDVDHeaderWrapper wrapper;

		DeclarationDVDHeaderWrapper GetWrapper(CusEntryHeader entryHeader, bool shouldDeclareUCRInHeader = false, bool shouldDeclareAddSupplyActorsInHeader = false, bool shouldDeclareCountryOfDestinationInHeader = false, bool shouldDeclareCountryOfExportInHeader = false, bool shouldDeclarePreviousDocumentsInHeader = false) => new DeclarationDVDHeaderWrapper(entryHeader, shouldDeclareUCRInHeader, shouldDeclareAddSupplyActorsInHeader, shouldDeclareCountryOfDestinationInHeader, shouldDeclareCountryOfExportInHeader, shouldDeclarePreviousDocumentsInHeader);

		protected override DeclarationDVDHeaderWrapper GetProvider() => wrapper;
	}
}
