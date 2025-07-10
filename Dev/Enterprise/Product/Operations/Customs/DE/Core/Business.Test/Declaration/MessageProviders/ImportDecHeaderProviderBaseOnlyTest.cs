using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ImportDecHeaderProvider))]
	sealed class ImportDecHeaderProviderBaseOnlyTest : ImportHeaderProviderAbstractTest<ImportDecHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ImportDecHeaderProviderForTest(null));
		}

		public void TestDeclarationKind()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.B;
			AssertEquals("B", Provider.DeclarationKind);
		}

		public void TestDeclarationType()
		{
			entryInstruction.CEI_Style = ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation;
			AssertEquals(ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation, Provider.DeclarationType);
		}

		public void TestPrematureInputFlag_A()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals(false, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_B()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.B;
			AssertEquals(false, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_C()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.C;
			AssertEquals(false, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_D()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.D;
			AssertEquals(true, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_E()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.E;
			AssertEquals(true, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_F()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.F;
			AssertEquals(true, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_Y()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.Y;
			AssertEquals(false, Provider.PrematureInputFlag);
		}

		public void TestPrematureInputFlag_Z()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.Z;
			AssertEquals(false, Provider.PrematureInputFlag);
		}

		public void TestGoodsItemQuantity()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;

			entryHeader.AllEntryLines.AddNew();
			entryHeader.AllEntryLines.AddNew();
			AssertEquals(2, Provider.GoodsItemQuantity);
		}

		public void TestProcedureAuthorisation()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "ENDUSE1234");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var document = invoiceLine.SupportingDocuments.AddNew();
			document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;
			AssertEquals("ENDUSE1234", Provider.ProcedureAuthorisation);
		}

		public void TestProcedureAuthorisation_NoSupportingDocumentTypesRequiringEndOfUseAuthorisation()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var document = invoiceLine.SupportingDocuments.AddNew();
			document.CSI_Code = "!Z7";
			AssertNull(Provider.ProcedureAuthorisation);
		}

		public void TestCustomsGoodsStatus()
		{
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			AssertEquals(EntryStyleListImport.Codes.ImportNormal, Provider.CustomsGoodsStatus);
		}

		public void TestGoodsLocation()
		{
			declaration.JE_LocationOfGoods = "location";
			AssertEquals("location", Provider.GoodsLocation);
		}

		public void TestDepartureCountry()
		{
			declaration.JE_GoodsOrigin = CountryCodes.Latvia;
			AssertEquals(CountryCodes.Latvia, Provider.DepartureCountry);
		}

		public void TestCurrencyCode()
		{
			AssertEquals(CurrencyCodes.Germany, Provider.CurrencyCode);
		}

		public void TestAdditionalInformation()
		{
			entryInstruction.AdditionalInformation = "additional information";
			AssertEquals("additional information", Provider.AdditionalInformation);
		}

		public void TestRepresentativeRelationshipFlag_SEL()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("0", Provider.RepresentativeRelationshipFlag);
		}

		public void TestRepresentativeRelationshipFlag_DIR()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("1", Provider.RepresentativeRelationshipFlag);
		}

		public void TestRepresentativeRelationshipFlag_IND()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("2", Provider.RepresentativeRelationshipFlag);
		}

		public void TestDeclarationPlace()
		{
			AssertEquals("Brisbane", Provider.DeclarationPlace);
		}

		public void TestDeclarant_NotPopulated()
		{
			var address = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OH_Importer = address.Header.PK;
			declaration.JE_OA_DeclarantAddress = address.PK;
			AssertNull(Provider.Declarant);
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var importerAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				var declarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS1");
				declaration.JE_OH_Importer = importerAddress.Header.PK;
				declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
				AssertNotNull("Populated", Provider.Declarant);
				AssertEquals("Declarant's EORI", "GREOR2", Provider.Declarant.Identification.EoriNumber);
			});
		}

		public void TestRepresentative_NotPopulated()
		{
			var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNull(Provider.Representative);
		}

		public void TestRepresentative()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.JE_OA_Representative = representativeAddress.PK;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertNotNull("Populated", Provider.Representative);
				AssertEquals("Represantative's EORI", "GREOR1", Provider.Representative.Identification.EoriNumber);
			});
		}

		public void TestPrincipal_NotPopulated()
		{
			var principalAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_BuyingAgentAddress = principalAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNull(Provider.Principal);
		}

		public void TestPrincipal()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var principalAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.JE_OA_BuyingAgentAddress = principalAddress.PK;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertNotNull("Populated", Provider.Principal);
				AssertEquals("Principal's EORI", "GREOR1", Provider.Principal.Identification.EoriNumber);
			});
		}

		public void TestContactPerson()
		{
			CombineAssertions(() =>
			{
				using (Factory.SetTemporaryCurrentUser(fullName: "Bob Baumeister"))
				{
					AssertNotNull("Populated", Provider.ContactPerson);
					AssertEquals("Current User's name", "Bob Baumeister", Provider.ContactPerson.PersonName);
				}
			});
		}

		public void TestBorderTransportMeansMode()
		{
			declaration.JE_TransportMode = "OWN";
			AssertEquals("9", Provider.BorderTransportMeansMode);
		}

		public void TestBorderTransportMeansType()
		{
			declaration.ZG_BorderTransportMeans = ImportBorderTransportMeansList.Codes.Other;
			AssertEquals("07", Provider.BorderTransportMeansType);
		}

		public void TestBorderTransportMeansInformation_Other()
		{
			declaration.ZG_BorderTransportMeans = ImportBorderTransportMeansList.Codes.Other;
			declaration.JE_VesselName = "information";
			AssertEquals("information", Provider.BorderTransportMeansInformation);
		}

		public void TestBorderTransportMeansInformation_NotOther()
		{
			declaration.ZG_BorderTransportMeans = ImportBorderTransportMeansList.Codes.Truck;
			declaration.JE_VesselName = "information";
			AssertNull(Provider.BorderTransportMeansInformation);
		}

		public void TestBorderTransportMeansNationality_AIR() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.Air, true);

		public void TestBorderTransportMeansNationality_SEA() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.Sea, true);

		public void TestBorderTransportMeansNationality_MAI() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.Mail, false);

		public void TestBorderTransportMeansNationality_ROA() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.Road, true);

		public void TestBorderTransportMeansNationality_RAI() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.Rail, false);

		public void TestBorderTransportMeansNationality_FIX() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.FixedTransportInstallations, false);

		public void TestBorderTransportMeansNationality_IWT() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.InlandWaterwayTransport, true);

		public void TestBorderTransportMeansNationality_OWN() => AssertBorderTransportMeansNationality(TransportTypeList.Codes.OwnPropulsion, false);

		public void TestPreviousAdministrativeReferenceType()
		{
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			AssertEquals("ATNEU", Provider.PreviousAdministrativeReferenceType);
		}

		public void TestPreviousAdministrativeReferenceNumber()
		{
			var previousDocument = entryInstruction.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.PreviousAdministrativeReferenceNumber);

				previousDocument.CSI_ReferenceNumber = "REF123456";
				AssertEquals("Not empty", "REF123456", Provider.PreviousAdministrativeReferenceNumber);
			});
		}

		public void TestPreviousAdministrativeReferenceNumber_NoPreviousDocuments()
		{
			entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
			AssertNull(Provider.PreviousAdministrativeReferenceNumber);
		}

		public void TestSummaryDeclaration_Null()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not ATNEU", true, Provider.PreviousAdministrativeReferenceType != PreviousProcedureList.Codes._ATNEU);
				AssertNull("NULL", Provider.SummaryDeclaration);
			});
		}

		public void TestSummaryDeclaration()
		{
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			AssertNotNull(Provider.SummaryDeclaration);
		}

		public void TestCustomsWarehouse_Null()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not AT-ZL", true, Provider.PreviousAdministrativeReferenceType != PreviousProcedureList.Codes._ATZL);
				AssertNull("NULL", Provider.CustomsWarehouse);
			});
		}

		public void TestCustomsWarehouse()
		{
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertNotNull(Provider.CustomsWarehouse);
		}

		public void TestInwardProcessing_Null()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not AT-AV", true, Provider.PreviousAdministrativeReferenceType != PreviousProcedureList.Codes._ATAV);
				AssertNull("NULL", Provider.InwardProcessing);
			});
		}

		public void TestInwardProcessing()
		{
			entryInstruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			AssertNotNull("Not NULL", Provider.InwardProcessing);
		}

		public void TestConsignee_Null()
		{
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var consigeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
				declaration.ImporterDocumentaryAddress.E2_OA_Address = consigeeAddress.PK;
				AssertNotNull("Populated", Provider.Consignee);
				AssertEquals("Consignee's EORI", "GREOR1", Provider.Consignee.Identification.EoriNumber);
			});
		}

		public void TestContainerFlag_0()
		{
			AssertEquals("Precondition", 0, Provider.ContainerIdentificationNumbers.Count);
			AssertEquals("N", Provider.ContainerFlag);
		}

		public void TestContainerFlag_1()
		{
			SetupContainersForContainerIdentificationNumbers();
			AssertEquals("J", Provider.ContainerFlag);
		}

		public void TestContainerIdentificationNumbers_FCL() => AssertContainerIdentificationNumbers(ContainerModes.FCL);

		public void TestContainerIdentificationNumbers_LCL() => AssertContainerIdentificationNumbers(ContainerModes.LCL);

		public void TestContainerIdentificationNumbers_ULD() => AssertContainerIdentificationNumbers(ContainerModes.ULD);

		public void TestContainerIdentificationNumbers_CNT() => AssertContainerIdentificationNumbers(ContainerModes.Containerised);

		public void TestContainerIdentificationNumbers_NCT()
		{
			SetupContainersForContainerIdentificationNumbers();
			declaration.JE_ContainerMode = ContainerModes.NonContainerised;

			AssertEquals(0, Provider.ContainerIdentificationNumbers.Count);
		}

		public void TestDeliveryTermsCode()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			AssertEquals("FOB", Provider.DeliveryTermsCode);
		}

		public void TestDeliveryTermsCode_NoInvoice()
		{
			AssertNull(Provider.DeliveryTermsCode);
		}

		public void TestDeliveryTermsDescription()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTerm = IncoTerms.FreeOnBoard;
			AssertNull(Provider.DeliveryTermsDescription);
		}

		public void TestDeliveryTermsDescription_Other()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTerm = IncoTerms.Other;
			invoice.JZ_IncoTermDescription = "OtherDesc1";
			AssertEquals("OtherDesc1", Provider.DeliveryTermsDescription);
		}

		public void TestDeliveryTermsDescription_NullDeliveryTermsCode()
		{
			AssertNull(Provider.DeliveryTermsDescription);
		}

		public void TestDocuments()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			var doc1 = invoice.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N380";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;
			var doc2 = invoice.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N999";
			doc2.CSI_ReferenceNumber = "REF2";
			doc2.CSI_DateOfIssue = ZDate.Today;
			var doc3 = invoice.SupportingDocuments.AddNew();
			doc3.CSI_Code = ZString.Empty;
			doc3.CSI_ReferenceNumber = "should not show";
			doc3.CSI_DateOfIssue = ZDate.Today;
			AssertEquals(2, Provider.Documents.Count);
		}

		public void TestDocuments_MultipleInvoices()
		{
			var invoiceHeader1 = AddInvoiceWithInvoiceLine();
			var doc1 = invoiceHeader1.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N300";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;

			var doc2 = invoiceHeader1.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N301";
			doc2.CSI_ReferenceNumber = "REF2";
			doc2.CSI_DateOfIssue = ZDate.Today;

			var invoiceHeader2 = AddInvoiceWithInvoiceLine();

			var doc3 = invoiceHeader2.SupportingDocuments.AddNew();
			doc3.CSI_Code = "N302";
			doc3.CSI_ReferenceNumber = "REF3";
			doc3.CSI_DateOfIssue = ZDate.Today;

			var doc4 = invoiceHeader2.SupportingDocuments.AddNew();
			doc4.CSI_Code = "N303";
			doc4.CSI_ReferenceNumber = "REF4";
			doc4.CSI_DateOfIssue = ZDate.Today;

			AssertEquals(4, Provider.Documents.Count);
		}

		public void TestDocuments_MultipleNonUniqueInvoices()
		{
			var invoiceHeader1 = AddInvoiceWithInvoiceLine();
			var doc1 = invoiceHeader1.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N300";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;

			var doc2 = invoiceHeader1.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N300";
			doc2.CSI_ReferenceNumber = "REF1";
			doc2.CSI_DateOfIssue = ZDate.Today;

			var invoiceHeader2 = AddInvoiceWithInvoiceLine();

			var doc3 = invoiceHeader2.SupportingDocuments.AddNew();
			doc3.CSI_Code = "N301";
			doc3.CSI_ReferenceNumber = "REF2";
			doc3.CSI_DateOfIssue = ZDate.Today;

			var doc4 = invoiceHeader2.SupportingDocuments.AddNew();
			doc4.CSI_Code = "N301";
			doc4.CSI_ReferenceNumber = "REF2";
			doc4.CSI_DateOfIssue = ZDate.Today;

			AssertEquals(2, Provider.Documents.Count);
		}

		public void TestForeignTradeStatisticsInlandTransportMode()
		{
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
			AssertEquals(TransportCodes.Rail, Provider.ForeignTradeStatisticsInlandTransportMode);
		}

		public void TestForeignTradeStatisticsTotalGrossMassMeasure()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_Weight = 123.45m;
			invoice.JZ_WeightUQ = "KG";
			var invoice2 = AddInvoiceWithInvoiceLine();
			invoice2.JZ_Weight = 123.55m;
			invoice2.JZ_WeightUQ = "KG";
			AssertEquals(247.00m, Provider.ForeignTradeStatisticsTotalGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsTotalGrossMassMeasureOtherUnit()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_Weight = 123450m;
			invoice.JZ_WeightUQ = "G";
			AssertEquals(123.5m, Provider.ForeignTradeStatisticsTotalGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsTotalGrossMassMeasureBelow50G()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_Weight = 0.049m;
			invoice.JZ_WeightUQ = "G";
			AssertEquals(0m, Provider.ForeignTradeStatisticsTotalGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsTotalGrossMassMeasureBothHeaderAndLines()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_Weight = 123.45m;
			invoice.JZ_WeightUQ = "KG";
			var invoice2 = AddInvoiceWithInvoiceLine();
			invoice2.JZ_Weight = 0m;
			var invoiceLine2 = invoice2.InvoiceLines.First() as JobComInvoiceLine;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;

			invoiceLine2.JI_Weight = 50.01m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine3.JI_Weight = 100.00m;
			invoiceLine3.JI_WeightUQ = "KG";

			AssertEquals(273.5m, Provider.ForeignTradeStatisticsTotalGrossMassMeasure);
		}

		public void TestForeignTradeStatisticsTotalGrossMassMeasureBothHeaderAndLinesMixedUnits()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_Weight = 123450m;
			invoice.JZ_WeightUQ = "G";
			var invoice2 = AddInvoiceWithInvoiceLine();
			invoice2.JZ_Weight = 0m;
			var invoiceLine2 = invoice2.InvoiceLines.First() as JobComInvoiceLine;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;

			invoiceLine2.JI_Weight = 0.1m;
			invoiceLine2.JI_WeightUQ = "T";
			invoiceLine3.JI_Weight = 150000m;
			invoiceLine3.JI_WeightUQ = "G";

			AssertEquals(373.5m, Provider.ForeignTradeStatisticsTotalGrossMassMeasure);
		}

		protected override ImportDecHeaderProvider GetProvider() => new ImportDecHeaderProviderForTest(entryHeader);

		void AssertBorderTransportMeansNationality(string transportType, bool required)
		{
			declaration.JE_TransportMode = transportType;
			declaration.JE_RN_NKTransportNationality = CountryCodes.Latvia;
			var expectedValue = required ? CountryCodes.Latvia : null;
			AssertEquals(expectedValue, Provider.BorderTransportMeansNationality);
		}

		void AssertContainerIdentificationNumbers(string containerMode)
		{
			declaration.JE_ContainerMode = containerMode;
			SetupContainersForContainerIdentificationNumbers();

			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.ContainerIdentificationNumbers.Count);
				AssertEquals("CON1", Provider.ContainerIdentificationNumbers.First());
				AssertEquals("CON2", Provider.ContainerIdentificationNumbers.Last());
			});
		}

		void SetupContainersForContainerIdentificationNumbers()
		{
			var cusContainer1 = declaration.CusContainers.AddNew();
			cusContainer1.CO_ContainerNumber = "CON1";
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "CON2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CON3";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddPivotFor(cusContainer1);
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddPivotFor(cusContainer2);
		}
	}

	sealed class ImportDecHeaderProviderForTest : ImportDecHeaderProvider
	{
		public ImportDecHeaderProviderForTest(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}
	}
}
