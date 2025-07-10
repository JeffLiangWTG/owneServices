using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using SupplyChainActorRoleList = Enterprise.Customs.Business.SupplyChainActorRoleList;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSMessageWrapperTest : WrapperHelperTest<EXSSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null CusEntryHeader", () => GetWrapper(null, Certificate));
				AssertExceptionThrown<ArgumentNullException>("Null Certificate", () => GetWrapper(entryHeader, null));
				var declaration = Factory.New<JobDeclaration>();
				var newEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => GetWrapper(newEntryHeader, Certificate));
			});
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				var header = wrapper.Header;

				AssertNotNull("Expected not null Header", header);
				AssertSame("Cached Header", wrapper.Header, header);
			});
		}

		public void TestTransportDocument()
		{
			AssertEquals(typeof(AdditionalInfoCollection), invoiceHeader.AdditionalInfos.GetType());

			AssertTransportDocuments("entryInstruction", entryInstruction.AdditionalInfos, null);

			AssertTransportDocuments("invoiceHeader", invoiceHeader.AdditionalInfos, entryInstruction.AdditionalInfos);

			AssertTransportDocuments("invoiceLine", invoiceLine.AdditionalInfos, invoiceHeader.AdditionalInfos);

			AssertTransportDocuments("declaration", declaration.AdditionalInfos, invoiceLine.AdditionalInfos);

			void AssertTransportDocuments(ZString additionalInfoType, AdditionalInfoCollection additionalInfoToAdd, AdditionalInfoCollection additionalInfoToRemove)
			{
				if (additionalInfoToRemove != null)
				{
					additionalInfoToRemove.RemoveAndDeleteAll();
				}

				CombineAssertions(additionalInfoType , () =>
				{
					var addInfo1 = additionalInfoToAdd.AddNew();
					addInfo1.CSI_SubType = "AA";
					addInfo1.CSI_Code = "N380";
					addInfo1.CSI_ReferenceNumber = "AAAAAAA";
					wrapper = GetWrapper(entryHeader, Certificate);
					AssertEquals("When AdditionalInfo in with subtype AA", null, wrapper.TransportDocument);

					var addInfo5 = additionalInfoToAdd.AddNew();
					addInfo5.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
					addInfo5.CSI_Code = "N381";
					addInfo5.CSI_ReferenceNumber = "BBBBBBBB";
					wrapper = GetWrapper(entryHeader, Certificate);
					AssertEquals("When one line AdditionalInfo in with subtype " + AdditionalDocList.Codes.TransportDocuments, "BBBBBBBB", wrapper.TransportDocument.Number);

					var addInfo6 = additionalInfoToAdd.AddNew();
					addInfo6.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
					addInfo6.CSI_Code = "N382";
					addInfo6.CSI_ReferenceNumber = "CCCCCCCCCC";
					wrapper = GetWrapper(entryHeader, Certificate);
					AssertEquals("When two lines AdditionalInfo in with subtype " + AdditionalDocList.Codes.TransportDocuments, "BBBBBBBB", wrapper.TransportDocument.Number);
				});
			}
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_FullName = "NameInHeader";
				var fullname1 = orgHeader1.OH_FullName;
				var orgAddressMain1 = orgHeader1.MainAddress;
				orgAddressMain1.OA_Address1 = "Address Main 1";
				var orgAddressOther1 = Factory.New<OrgAddress>();
				orgAddressOther1.OA_OH = orgHeader1.PK;
				orgAddressOther1.OA_Address1 = "Address Other 1";

				var orgHeader2 = Factory.New<OrgHeader>();
				orgHeader2.OH_FullName = "NameInHeader";
				var fullname2 = orgHeader2.OH_FullName;
				var orgAddressMain2 = orgHeader2.MainAddress;
				orgAddressMain2.OA_Address1 = "Address Main 2";
				var orgAddressOther2 = Factory.New<OrgAddress>();
				orgAddressOther2.OA_OH = orgHeader1.PK;
				orgAddressOther2.OA_Address1 = "Address Other 2";

				invoiceLine.JI_Tariff = "2203001012";
				declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when no consignor in declaration and no lines with consignor", null, wrapper.Consignor);

				invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader1.PK;
				declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddressOther1.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignor in declaration and no lines with consignor (Name)", fullname1, wrapper.Consignor.Name);
				AssertEquals("Expected when consignor in declaration and no lines with consignor (Address)", orgAddressOther1.OA_Address1, wrapper.Consignor.Address);

				invoiceHeader.JZ_OH_Supplier = orgHeader2.PK;
				declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when no consignor in declaration and one line with consignor (Name)", fullname2, wrapper.Consignor.Name);
				AssertEquals("Expected when consignor in declaration and no lines with consignor (Address Main)", orgAddressMain2.OA_Address1, wrapper.Consignor.Address);

				invoiceHeader.JZ_OH_Supplier = orgHeader2.PK;
				invoiceHeader.JZ_OA_SupplierAddress = orgAddressOther2.PK;
				declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when no consignor in declaration and one line with consignor (Name)", fullname2, wrapper.Consignor.Name);
				AssertEquals("Expected when consignor in declaration and no lines with consignor (Address Other)", orgAddressOther2.OA_Address1, wrapper.Consignor.Address);

				invoiceHeader.JZ_OH_Supplier = orgHeader1.PK;
				invoiceHeader.JZ_OA_SupplierAddress = orgAddressOther1.PK;
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader2.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignor in declaration and consignor in lines but differents (Name)", fullname1, wrapper.Consignor.Name);
				AssertEquals("Expected when consignor in declaration and consignor in lines but differents (Address Other)", orgAddressOther1.OA_Address1, wrapper.Consignor.Address);
				invoiceHeader.JZ_OH_Supplier = orgHeader2.PK;
				invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignor in declaration and consignor in lines equals (Name)", fullname2, wrapper.Consignor.Name);
				AssertEquals("Expected when consignor in declaration and consignor in lines equals (Address Main)", orgAddressMain2.OA_Address1, wrapper.Consignor.Address);

				var entryLine1 = entryHeader.MergedLines.AddNew();
				var invoiceHeader1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;

				invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignor in declaration and two lines and differents consignors", null, wrapper.Consignor);
				invoiceHeader1.JZ_OH_Supplier = orgHeader2.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignor in declaration and two lines and equals consignors (Name)", fullname1, wrapper.Consignor.Name);

				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader1.PK;
				invoiceHeader.JZ_OH_Supplier = orgHeader2.PK;
				invoiceHeader1.JZ_OH_Supplier = orgHeader2.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignor in declaration is different that the consignors in lines is the same (Name)", fullname2, wrapper.Consignor.Name);
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_FullName = "NameInHeader";
				var fullname1 = orgHeader1.OH_FullName;
				var orgAddressMain1 = orgHeader1.MainAddress;
				orgAddressMain1.OA_Address1 = "Address Main 1";
				var orgAddressOther1 = Factory.New<OrgAddress>();
				orgAddressOther1.OA_OH = orgHeader1.PK;
				orgAddressOther1.OA_Address1 = "Address Other 1";

				var orgHeader2 = Factory.New<OrgHeader>();
				orgHeader2.OH_FullName = "NameInHeader";
				var fullname2 = orgHeader2.OH_FullName;
				var orgAddressMain2 = orgHeader2.MainAddress;
				orgAddressMain2.OA_Address1 = "Address Main 2";
				var orgAddressOther2 = Factory.New<OrgAddress>();
				orgAddressOther2.OA_OH = orgHeader1.PK;
				orgAddressOther2.OA_Address1 = "Address Other 2";

				invoiceLine.JI_Tariff = "2203001012";
				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when no consignee in declaration and no lines with consignee", null, wrapper.Consignee);

				invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
				declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader1.PK;
				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddressOther1.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignee in declaration and no lines with consignee (Name)", fullname1, wrapper.Consignee.Name);
				AssertEquals("Expected when consignee in declaration and no lines with consignee (Address)", orgAddressOther1.OA_Address1, wrapper.Consignee.Address);

				invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when no consignee in declaration and one line with consignee (Name)", fullname2, wrapper.Consignee.Name);
				AssertEquals("Expected when consignee in declaration and no lines with consignee (Address Main)", orgAddressMain2.OA_Address1, wrapper.Consignee.Address);

				invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
				invoiceHeader.JZ_OA_BuyerAddress = orgAddressOther2.PK;
				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when no consignee in declaration and one line with consignee (Name)", fullname2, wrapper.Consignee.Name);
				AssertEquals("Expected when consignee in declaration and no lines with consignee (Address Other)", orgAddressOther2.OA_Address1, wrapper.Consignee.Address);

				invoiceHeader.JZ_OH_Buyer = orgHeader1.PK;
				invoiceHeader.JZ_OA_BuyerAddress = orgAddressOther1.PK;
				declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader2.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignee in declaration and consignee in lines but differents (Name)", fullname1, wrapper.Consignee.Name);
				AssertEquals("Expected when consignee in declaration and consignee in lines but differents (Address Other)", orgAddressOther1.OA_Address1, wrapper.Consignee.Address);
				invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
				invoiceHeader.JZ_OA_BuyerAddress = ZGuid.Empty;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignee in declaration and consignee in lines equals (Name)", fullname2, wrapper.Consignee.Name);
				AssertEquals("Expected when consignee in declaration and consignee in lines equals (Address Main)", orgAddressMain2.OA_Address1, wrapper.Consignee.Address);

				var entryLine1 = entryHeader.MergedLines.AddNew();
				var invoiceHeader1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;

				invoiceHeader1.JZ_OH_Buyer = orgHeader1.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignee in declaration and two lines and differents consignees", null, wrapper.Consignee);
				invoiceHeader1.JZ_OH_Buyer = orgHeader2.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignee in declaration and two lines and equals consignees (Name)", fullname2, wrapper.Consignee.Name);

				declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader1.PK;
				invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
				invoiceHeader1.JZ_OH_Buyer = orgHeader2.PK;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected when consignee in declaration is different that the consignees in lines is the same (Name)", fullname2, wrapper.Consignee.Name);
			});
		}

		public void TestAdditionalActors()
		{
			CombineAssertions(() =>
			{
				var referenceAA = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceAA.CFR_Code = "AA";
				referenceAA.CFR_Reference = "REFAA";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When AdditionalActors with CFR_Code AA", 0, wrapper.AdditionalActors.Count);

				var referenceMF = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceMF.CFR_Code = SupplyChainActorRoleList.Codes.MF;
				referenceMF.CFR_Reference = "REFMF";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When AdditionalActors with CFR_Code MF", 1, wrapper.AdditionalActors.Count);

				var referenceCS = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceCS.CFR_Code = SupplyChainActorRoleList.Codes.CS;
				referenceCS.CFR_Reference = "REFCS";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When AdditionalActors with CFR_Code MF and CS", 2, wrapper.AdditionalActors.Count);

				var referenceFW = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceFW.CFR_Code = SupplyChainActorRoleList.Codes.FW;
				referenceFW.CFR_Reference = "REFFW";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When AdditionalActors with CFR_Code MF, CS and FW", 3, wrapper.AdditionalActors.Count);

				var referenceWH = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceWH.CFR_Code = SupplyChainActorRoleList.Codes.WH;
				referenceWH.CFR_Reference = "REFWH";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When AdditionalActors with CFR_Code MF, CS, FW and WH", 4, wrapper.AdditionalActors.Count);

				var invoiceLineReferenceCS = invoiceLine.CusSupplyChainActorReferences.AddNew();
				invoiceLineReferenceCS.CFR_Code = "CS";
				invoiceLineReferenceCS.CFR_Reference = "REFCS";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("If add in invoiceLine AdditionalActors with CFR_Code included, in SendMessageWrapper no send any", 0, wrapper.AdditionalActors.Count);
			});
		}

		public void TestAdditionalInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalInfoCollection), invoiceHeader.AdditionalInfos.GetType());

				var addInfo1 = declaration.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = "AA";
				addInfo1.CSI_Code = "AAA";
				addInfo1.CSI_ReferenceNumber = "AAAAAAA";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When AdditionalInfo in declaration with subtype AA", 0, wrapper.AdditionalInfo.Count);

				var addInfo2 = declaration.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
				addInfo2.CSI_Code = "AAA";
				addInfo2.CSI_ReferenceNumber = "AAAAAAA";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When one AdditionalInfo in declaration with subtype " + AdditionalDocList.Codes.AdditionalInformation, 1, wrapper.AdditionalInfo.Count);

				var addInfo3 = entryInstruction.AdditionalInfos.AddNew();
				addInfo3.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
				addInfo3.CSI_Code = "BBB";
				addInfo3.CSI_ReferenceNumber = "BBBBBBB";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When two AdditionalInfos, one in declaration and one in entryInstruction, with subtype " + AdditionalDocList.Codes.AdditionalInformation, 2, wrapper.AdditionalInfo.Count);

				var addInfo4 = entryInstruction.AdditionalInfos.AddNew();
				addInfo4.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
				addInfo4.CSI_Code = "CCC";
				addInfo4.CSI_ReferenceNumber = "CCCCCCCC";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When three AdditionalInfos, one in declaration and two in entryInstruction, with subtype " + AdditionalDocList.Codes.AdditionalInformation, 3, wrapper.AdditionalInfo.Count);

				addInfo4.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("When one AdditionalInfo from entryInstruction has subtype " + AdditionalDocList.Codes.TransportDocuments, 2, wrapper.AdditionalInfo.Count);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);

				var entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, wrapper.Lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		public void TestItineraryCountries()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty itineraryCountries", 0, wrapper.ItineraryCountries.Count);

				var trans1 = declaration.Transports.AddNew();
				var trans2 = declaration.Transports.AddNew();
				var trans3 = declaration.Transports.AddNew();
				var trans4 = declaration.Transports.AddNew();
				var trans5 = declaration.Transports.AddNew();
				var trans6 = declaration.Transports.AddNew();
				trans1.JW_ETA = ZDate.Today.AddDays(1);
				trans2.JW_ETA = ZDate.Today.AddDays(2);
				trans3.JW_ETA = ZDate.Today.AddDays(3);
				trans4.JW_ETA = ZDate.Today.AddDays(4);
				trans5.JW_ETA = ZDate.Today.AddDays(5);
				trans6.JW_ETA = ZDate.Today.AddDays(6);
				trans1.JW_RL_NKLoadPort = "AUSYD";
				trans1.JW_RL_NKDiscPort = "INBOM";
				trans2.JW_RL_NKLoadPort = "INBOM";
				trans2.JW_RL_NKDiscPort = "TRIST";
				trans3.JW_RL_NKLoadPort = "TRIST";
				trans3.JW_RL_NKDiscPort = "DEHAM";
				trans4.JW_RL_NKLoadPort = "DEHAM";
				trans4.JW_RL_NKDiscPort = "GBLBA";
				trans5.JW_RL_NKLoadPort = "GBLBA";
				trans5.JW_RL_NKDiscPort = "ESBCN";
				trans6.JW_RL_NKLoadPort = "ESBCN";
				trans6.JW_RL_NKDiscPort = "FRPAR";
				declaration.JE_RL_NKOrigin = "ESMAD";
				declaration.JE_RL_NKFinalDestination = "FRPAR";

				var expectedCountryListForEXS = new ZString[] { "ES", "AU", "IN", "TR", "DE", "GB", "ES", "FR" };

				entryInstruction.IncludeRoutingSecurityData = false;

				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty itineraryCountries when IncludeRoutingSecurityData is false", 0, wrapper.ItineraryCountries.Count);

				entryInstruction.IncludeRoutingSecurityData = true;
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				wrapper = GetWrapper(entryHeader, Certificate);
				var itineraryCountries = wrapper.ItineraryCountries;
				AssertArrayEqualsByElements("Expected filled itineraryCountries when IncludeRoutingSecurityData is true", CountriesOfRouting, itineraryCountries.ToArray());
				AssertSame("Cached itineraryCountries", wrapper.ItineraryCountries, itineraryCountries);

				entryInstruction.IncludeRoutingSecurityData = false;

				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty itineraryCountries when IncludeRoutingSecurityData is false", 0, wrapper.ItineraryCountries.Count);

				entryInstruction.IncludeRoutingSecurityData = true;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertArrayEqualsByElements("Expected empty itineraryCountries when IncludeRoutingSecurityData is true but is not EXS Declaration", CountriesOfRouting, wrapper.ItineraryCountries.ToArray());

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertArrayEqualsByElements("Expected empty itineraryCountries when IncludeRoutingSecurityData is true but is EXS Declaration(origin and destination)", expectedCountryListForEXS, wrapper.ItineraryCountries.ToArray());
			});
		}

		public void TestCustomsOffice()
		{
			declaration.JE_CustomsOffice = "AAAA";
			AssertEquals("Expected CustomsOffice", "AAAA", wrapper.CustomsOffice);
		}

		public void TestNullLodgingPerson()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.LodgingPerson.ToString());
		}

		public void TestLodgingPerson()
		{
			CombineAssertions(() =>
			{
				var orgAddressDeclarant = createOrgAddress("declarant", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
				declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;

				declaration.JE_DeclarantType = "1";
				AssertNull("Expected null lodgingPerson when only declarant is declared but rep type is not 3", wrapper.LodgingPerson);

				declaration.JE_DeclarantType = "3";
				wrapper = GetWrapper(entryHeader, Certificate);
				var lodgingPerson = wrapper.LodgingPerson;

				AssertNotNull("Expected filled lodgingPerson when only declarant is declared and rep type is 3", lodgingPerson);
				AssertSame("Cached lodgingPerson", wrapper.LodgingPerson, lodgingPerson);
				AssertEquals("Expected lodgingPerson's name to be declarant's", "declarant", lodgingPerson.Name);

				var orgHeaderCarrier = createOrgHeader("carrier", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
				declaration.JE_OH_ShippingLine = orgHeaderCarrier.PK;

				declaration.JE_DeclarantType = "4";
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertNull("Expected null lodgingPerson when declarant and carrier are declared but rep type is not 1, 2 or 3", wrapper.LodgingPerson);

				declaration.JE_DeclarantType = "2";
				wrapper = GetWrapper(entryHeader, Certificate);
				lodgingPerson = wrapper.LodgingPerson;
				AssertNotNull("Expected filled lodgingPerson when declarant and carrier are declared and rep type is 1 or 2", lodgingPerson);
				AssertEquals("Expected lodgingPerson's name to be carrier's", "carrier", lodgingPerson.Name);
			});
		}

		public void TestLodgingPerson_EmailAddress_Empty()
		{
			var orgAddressDeclarant = createOrgAddress("declarant", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			declaration.JE_DeclarantType = "3";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty lodgingPerson's email when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.LodgingPerson.EmailAddress);
			}
		}

		public void TestLodgingPerson_EmailAddress_MailboxEmailAddress()
		{
			var orgAddressDeclarant = createOrgAddress("declarant", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			declaration.JE_DeclarantType = "3";

			const string mailboxEmail = "mail2.mail@mail.com";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled lodgingPerson's email with mailbox email address when filled and clearance email recipient is empty", "mail2.mail@mail.com", wrapper.LodgingPerson.EmailAddress);
			}
		}

		public void TestLodgingPerson_EmailAddress_CustomsClearanceEmailRecipient()
		{
			var orgAddressDeclarant = createOrgAddress("declarant", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			declaration.JE_DeclarantType = "3";

			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled lodgingPerson's email with clearance email recipient when filled", "mail1.mail@mail.com", wrapper.LodgingPerson.EmailAddress);
			}
		}

		public void TestNullRepresentative()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				var orgAddress = createOrgAddress("representative", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
				declaration.JE_OA_DeclarantAddress = orgAddress.PK;

				var representative = wrapper.Representative;

				AssertNotNull("Expected filled Representative", representative);
				AssertSame("Cached Representative", wrapper.Representative, representative);
			});
		}

		public void TestNullCarrier()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Carrier.ToString());
		}

		public void TestCarrier()
		{
			CombineAssertions(() =>
			{
				var orgHeader = createOrgHeader("carrier", "NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
				declaration.JE_OH_ShippingLine = orgHeader.PK;

				var carrier = wrapper.Carrier;

				AssertNotNull("Expected filled Carrier", carrier);
				AssertSame("Cached Carrier", wrapper.Carrier, carrier);
			});
		}

		public void TestSeals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Seals", 0, wrapper.Seals.Count);

				foreach (var seal in ContainerSeals)
				{
					var ctnNumber = "CTN1" + seal;
					var container = declaration.CusContainers.AddNew();
					container.CO_ContainerNumber = ctnNumber;
					container.CO_Seal = seal;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(ctnNumber).IsForInvoiceLine = true;
				}

				wrapper = GetWrapper(entryHeader, Certificate);
				var sealCodes = wrapper.Seals;

				AssertArrayEqualsByElements("Expected filled Seals", ContainerSeals, sealCodes.ToArray());
				AssertSame("Cached Seals", wrapper.Seals, sealCodes);
			});
		}

		public void TestSealsOnlyContainersWithSecondSeals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Seals", 0, wrapper.Seals.Count);

				foreach (var seal in SecondContainerSeals)
				{
					var ctnNumber = "CTN1" + seal;
					var container = declaration.CusContainers.AddNew();
					container.CO_ContainerNumber = ctnNumber;
					container.CO_SecondSeal = seal;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(ctnNumber).IsForInvoiceLine = true;
				}

				wrapper = GetWrapper(entryHeader, Certificate);
				var sealCodes = wrapper.Seals;

				AssertArrayEqualsByElements("Expected filled Seals", SecondContainerSeals, sealCodes.ToArray());
				AssertSame("Cached Seals", wrapper.Seals, sealCodes);
			});
		}

		public void TestSealsOnlyContainersWithAdditionalSeals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Seals", 0, wrapper.Seals.Count);

				foreach (var seal in SecondContainerSeals)
				{
					var ctnNumber = "CTN1" + seal;
					var container = declaration.CusContainers.AddNew();
					container.CO_ContainerNumber = ctnNumber;
					container.AdditionalSeals.AddNew().BK_SealNumber = seal;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(ctnNumber).IsForInvoiceLine = true;
				}

				wrapper = GetWrapper(entryHeader, Certificate);
				var sealCodes = wrapper.Seals;

				AssertArrayEqualsByElements("Expected filled Seals", SecondContainerSeals, sealCodes.ToArray());
				AssertSame("Cached Seals", wrapper.Seals, sealCodes);
			});
		}

		public void TestSealsOnlyEquipmentsWithSeals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Seals", 0, wrapper.Seals.Count);

				var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
				foreach (var seal in ContainerSeals)
				{
					var equNumber = "EQUIP1" + seal;
					var equipment = declaration.Equipments.AddNew();
					equipment.CEQ_IdentificationNumber = equNumber;
					equipment.Seals.AddNew().BK_SealNumber = seal;
					var pack1 = packingGroups.Packages.AddNew();
					pack1.CW_PackQty = 1;
					pack1.CW_ContainerNoOrEquipmentNo = equipment.CEQ_IdentificationNumber;
					invoiceLine.PackagesPivot.AddPivotFor(pack1);
				}

				wrapper = GetWrapper(entryHeader, Certificate);
				var sealCodes = wrapper.Seals;

				AssertArrayEqualsByElements("Expected filled Seals", ContainerSeals, sealCodes.ToArray());
				AssertSame("Cached Seals", wrapper.Seals, sealCodes);
			});
		}

		public void TestSeals_FromInvoiceLineAndEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();

			var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CTN1";
			container1.CO_Seal = "S01";
			container1.CO_SecondSeal = "S21";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "A01";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "A02";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CTN2";
			container2.CO_Seal = "S02";
			container2.CO_SecondSeal = "S22";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "A03";

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CTN3";
			container3.CO_Seal = "S03";
			container3.CO_SecondSeal = "S23";
			container3.AdditionalSeals.AddNew().BK_SealNumber = "A04";

			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CTN4";
			container4.CO_Seal = ZString.Empty;
			container4.CO_SecondSeal = ZString.Empty;
			container4.AdditionalSeals.AddNew().BK_SealNumber = "";

			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN1").IsForInvoiceLine = true;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN2").IsForInvoiceLine = true;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN3").IsForInvoiceLine = true;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CTN4").IsForInvoiceLine = true;

			var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "E01";
			var pack1 = packingGroups.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment2.Seals.AddNew().BK_SealNumber = "E02";
			equipment2.Seals.AddNew().BK_SealNumber = "E03";
			var pack2 = packingGroups.Packages.AddNew();
			pack2.CW_PackQty = 1;
			pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "";
			var pack3 = packingGroups.Packages.AddNew();
			pack3.CW_PackQty = 1;
			pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(pack1);
			invoiceLine2.PackagesPivot.AddPivotFor(pack2);
			invoiceLine2.PackagesPivot.AddPivotFor(pack3);

			CombineAssertions(() =>
			{
				var wrapper1 = GetWrapper(entryHeader1, Certificate);
				AssertArrayEqualsByElements("Expected filled Seals for first entryHeader", new ZString[] { "S01", "S02", "S21", "S22", "A01", "A02", "A03", "E01" }, wrapper1.Seals.ToArray());

				var wrapper2 = GetWrapper(entryHeader2, Certificate);
				AssertArrayEqualsByElements("Expected filled Seals for second entryHeader", new ZString[] { "S03", "S23", "A04", "E02", "E03" }, wrapper2.Seals.ToArray());
			});
		}

		public OrgHeader createOrgHeader(ZString name, ZString nif, ZString email, ZString address)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode;
			orgHeader.OH_FullName = name;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, nif);
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var orgAddress = orgHeader.MainAddress;

			orgAddress.OA_Email = email;
			orgAddress.Address1 = address;
			return orgHeader;
		}

		public OrgAddress createOrgAddress(ZString name, ZString nif, ZString email, ZString address)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode;
			orgHeader.OH_FullName = name;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, nif);
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var orgAddress = orgHeader.MainAddress;

			orgAddress.OA_Email = email;
			orgAddress.Address1 = address;
			return orgAddress;
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
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		EXSSendMessageWrapper wrapper;

		protected EXSSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new EXSSendMessageWrapper(cusEntryHeader, certificateData);

		protected override EXSSendMessageWrapper GetProvider() => wrapper;
	}
}

