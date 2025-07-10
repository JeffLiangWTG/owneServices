using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE513MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE513MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("EntryHeader missing", () => new IE513MessageProvider(null));

				var entryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown<ArgumentException>("Declaration missing", () => new IE513MessageProvider(entryHeader));

				var declaration = Factory.New<JobDeclaration>();
				entryHeader.CH_JE = declaration.PK;
				AssertExceptionThrown<ArgumentException>("Instruction missing", () => new IE513MessageProvider(entryHeader));

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				AssertNoExceptionThrown("No missing", () => new IE513MessageProvider(entryHeader));
			});
		}

		#region IIE513Header Members
		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestPresentationOffice()
		{
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE000001");
			AssertEquals("PresentationOffice", "IE000001", Provider.PresentationOffice);
		}

		public void TestExportOffice()
		{
			declaration.JE_CustomsOffice = "Export22";
			AssertEquals("ExportOffice", "Export22", Provider.ExportOffice);
		}

		public void TestExitOffice()
		{
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "Exit222");
			AssertEquals("ExitOffice", "Exit222", Provider.ExitOffice);
		}

		public void TestSupervisingOffice()
		{
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "SUPER123");
			AssertEquals("SupervisingOffice", "SUPER123", Provider.SupervisingOffice);
		}

		public void TestSecurity()
		{
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			entryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			AssertEquals("Security: 2 when B1.", Constants.ExportOperationSecurity.Security, Provider.Security);
			entryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.C1;
			AssertEquals("Security: 2 when C1.", Constants.ExportOperationSecurity.Security, Provider.Security);
		}

		public void TestSecurity_Empty()
		{
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;
			AssertEquals(
				@"Security: Empty when JE_EntryStyle CO, via C0265, WI00543891",
				string.Empty,
				Provider.Security
			);
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;

			entryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			AssertEquals("Security: Empty when B3.", Constants.ExportOperationSecurity.NonSecurity, Provider.Security);
			entryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.B4;
			AssertEquals("Security: Empty when B4.", Constants.ExportOperationSecurity.NonSecurity, Provider.Security);
		}

		public void TestExporter()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "TestExporter";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = exporter.MainAddress.PK;
			AssertEquals("Exporter", "TestExporter", Provider.Exporter.Name);
		}

		public void TestDeclarant()
		{
			var declarantHeader = Factory.New<OrgHeader>();
			declarantHeader.OH_FullName = "TestDeclarantHeader";
			var declarant = Factory.New<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			declarant.OA_OH = declarantHeader.PK;
			AssertEquals("Declarant", "TestDeclarantHeader", Provider.Declarant.Name);
		}

		public void TestRepresentative()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TestRepresentativeHeader";
			var address = Factory.New<OrgAddress>();
			address.OA_CompanyNameOverride = "TestRepresentativeHeader Override";
			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			address.OA_OH = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			CombineAssertions("Representantive", () =>
			{
				var representative = Provider.Representative;
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Contact.Name", "BOB THE BUILDER", representative.Contact.Name);
			});
		}

		public void TestAuthorisations()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var auth1 = instruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;
			auth1.AGC_Number = "AUTH001";
			var auth2 = instruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsValue;
			auth2.AGC_Number = "AUTH002";

			var provider = GetProvider();

			CombineAssertions("AUTHORISATION", () =>
			{
				AssertEquals("Should have 2 items.", 2, provider.Authorisations.Count);
				Assert("Should have an Auth: CUACP AUTH001", provider.Authorisations.Any(auth => auth.IdentificationType == "CUACP" && auth.UCR == "AUTH001" && string.IsNullOrEmpty(auth.AuthorisatonHolder)));
				Assert("Should have an Auth: CUCVA AUTH002", provider.Authorisations.Any(auth => auth.IdentificationType == "CUCVA" && auth.UCR == "AUTH002" && string.IsNullOrEmpty(auth.AuthorisatonHolder)));
			});
		}

		public void TestDeferredPayment()
		{
			AssertNull("DeferredPayment not in use", Provider.DeferredPayment);
		}

		public void TestGoodsShipment()
		{
			AssertSame("GoodsShipment", Provider, Provider.GoodsShipment);
		}

		public void TestIsInTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				AssertEquals("Precondition", true, declaration.IsTransitionPeriodAES30);
				AssertEquals(true, Provider.IsInTransitionPeriod);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				AssertEquals("Precondition", false, declaration.IsTransitionPeriodAES30);
				AssertEquals(false, Provider.IsInTransitionPeriod);
			}
		}

		#endregion

		#region IIE513ExportOperation Members
		public void TestDeclarationType()
		{
			declaration.JE_EntryStyle = "AB";
			AssertEquals("DeclarationType", "AB", Provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			entryInstruction.CEI_SubStyle = "AB";
			AssertEquals("AdditionalDeclarationType", "AB", Provider.AdditionalDeclarationType);
		}

		public void TestPresentationDateTime()
		{
			declaration.ZG_PresentationStartDate = new ZDateTime(2022, 05, 30);
			AssertEquals("PresentationDateTime", new DateTime(2022, 05, 30, 0, 0, 0, DateTimeKind.Unspecified), Provider.PresentationDateTime);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			declaration.ZG_SpecificCircumstanceIndicator = "A20";
			AssertEquals("SpecificCircumstanceIndicator", "A20", Provider.SpecificCircumstanceIndicator);
		}

		public void TestInvoiceAmount()
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine.JI_LinePrice = 150m;
			AssertEquals("InvoiceAmount", 150m, Provider.InvoiceAmount);
		}

		public void TestInvoiceCurrency()
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine.JI_LinePrice = 150m;
			AssertEquals("InvoiceCurrency", Core.Constants.CurrencyCodes.Australia, Provider.InvoiceCurrency);
		}

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN2343234242");
			AssertEquals("MRN", "MRN2343234242", Provider.MRN);
		}
		#endregion

		#region IE513GoodsShipment Members
		public void TestTransactionNature()
		{
			invoice.JZ_ValuationCode = "A";
			AssertEquals("TransactionNature", "A", Provider.TransactionNature);
		}

		public void TestAdditionalSupplyChainActor()
		{
			var ref1 = entryInstruction.CusSupplyChainActorReferences.AddNew();
			var item1 = Provider.AdditionalSupplyChainActor.FirstOrDefault();
			AssertNotNull("Should have an AdditionalSupplyChainActor", item1);
		}

		public void TestDeliveryTerms()
		{
			entryInstruction.CEI_SubStyle = "A";
			invoice.JZ_IncoTerm = "FOB";
			AssertEquals("DeliveryTerms", "FOB", GetProvider().DeliveryTerms.IncotermCode);

			entryInstruction.CEI_SubStyle = "B";
			AssertEquals("DeliveryTerms (not included)", null, GetProvider().DeliveryTerms);
		}

		public void TestPreviousDocuments()
		{
			var previousDocument1 = invoice.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "C1";
			var previousDocument2 = invoice.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "C2";

			CombineAssertions(() =>
			{
				var previousDocuments = Provider.PreviousDocuments.ToList();
				AssertEquals("Count", 2, previousDocuments.Count);
				AssertEquals("Previous Document 1", "C1", previousDocuments[0].Type);
				AssertEquals("Previous Document 2", "C2", previousDocuments[1].Type);
			});
		}

		public void TestPreviousDocuments_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var previousDocument1 = invoice.PreviousDocuments.AddNew();
				previousDocument1.CSI_Code = "C1";
				var previousDocument2 = invoice.PreviousDocuments.AddNew();
				previousDocument2.CSI_Code = "C2";

				var provider = new IE513MessageProvider(entryHeader);
				AssertEquals("Count", 0, provider.PreviousDocuments.Count);
			}
		}

		public void TestSupportingDocuments()
		{
			var supportingDocument1 = invoice.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "C1";
			supportingDocument1.CSI_ItemNumber = 1;
			var supportingDocument2 = invoice.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "C2";
			supportingDocument2.CSI_ItemNumber = 2;

			var supportingDocuments = Provider.SupportingDocuments.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, supportingDocuments.Count);
				AssertEquals("Supporting Document 1", "C1", supportingDocuments[0].Type);
				AssertEquals("Supporting Document 1", "1", supportingDocuments[0].LineNumber);
				AssertEquals("Supporting Document 2", "C2", supportingDocuments[1].Type);
				AssertEquals("Supporting Document 2", "2", supportingDocuments[1].LineNumber);
			});
		}

		public void TestSupportingDocuments_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var supportingDocument1 = invoice.SupportingDocuments.AddNew();
				supportingDocument1.CSI_Code = "C1";
				supportingDocument1.CSI_ItemNumber = 1;
				var supportingDocument2 = invoice.SupportingDocuments.AddNew();
				supportingDocument2.CSI_Code = "C2";
				supportingDocument2.CSI_ItemNumber = 2;
				var provider = new IE513MessageProvider(entryHeader);
				AssertEquals("Count", 0, provider.SupportingDocuments.Count);
			}
		}

		public void TestAdditionalInformations()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, new[] { ("9001", "9001 DES"), ("9002", "9002 DES") });

			var additionalInfo1 = invoice.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "9001";
			additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var additionalInfo2 = invoice.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "9002";
			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			CombineAssertions(() =>
			{
				var additionalInformations = Provider.AdditionalInformations.ToList();
				AssertEquals("Count", 2, additionalInformations.Count);
				AssertEquals("AdditionalInfo 1", "9001", additionalInformations[0].Code);
				AssertEquals("AdditionalInfo 2", "9002", additionalInformations[1].Code);
			});
		}

		public void TestAdditionalInformations_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, new[] { ("9001", "9001 DES"), ("9002", "9002 DES") });

				var additionalInfo1 = invoice.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "9001";
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				var additionalInfo2 = invoice.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "9002";
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

				var provider = new IE513MessageProvider(entryHeader);
				AssertEquals("Count", 0, provider.AdditionalInformations.Count);
			}
		}

		public void TestAdditionalReferences()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, codes: new[] { "9001", "9002" });

			var additionalInfo1 = invoice.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "9001";
			additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo1.CSI_ReferenceNumber = "9001001";
			var additionalInfo2 = invoice.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "9002";
			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo2.CSI_ReferenceNumber = "9002001";

			CombineAssertions(() =>
			{
				var additionalReferences = Provider.AdditionalReferences.ToList();
				AssertEquals("Count", 2, additionalReferences.Count);
				AssertEquals("AdditionalRef 1", "9001", additionalReferences[0].Type);
				AssertEquals("AdditionalRef 2", "9002", additionalReferences[1].Type);
			});
		}

		public void TestAdditionalReferences_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, codes: new[] { "9001", "9002" });

				var additionalInfo1 = invoice.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "9001";
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo1.CSI_ReferenceNumber = "9001001";
				var additionalInfo2 = invoice.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "9002";
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo2.CSI_ReferenceNumber = "9002001";

				var provider = new IE513MessageProvider(entryHeader);
				AssertEquals("Count", 0, provider.AdditionalReferences.Count);
			}
		}

		public void TestAdditionalReferences_InstructionSubStyleYorZ()
		{
			var additionalInfo1 = invoice.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "1D23";
			additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo1.CSI_ReferenceNumber = "202210121000";
			var additionalInfo2 = invoice.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "9002";
			additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo2.CSI_ReferenceNumber = "9002001";

			var provider = GetProvider();
			var additionalReferences = provider.AdditionalReferences.ToList();
			AssertEquals("Count", 2, additionalReferences.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;
			provider = GetProvider();
			additionalReferences = provider.AdditionalReferences.ToList();
			AssertEquals("Count", 1, additionalReferences.Count);
			AssertEquals("AdditionalRef", "9002", additionalReferences[0].Type);
		}

		public void TestConsignment()
		{
			var provider = GetProvider();
			entryInstruction.CEI_SubStyle = "A";
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				var consignment = provider.Consignment;
				AssertEquals("Consignment.InlandTransportMode", "1", consignment.InlandTransportMode);
				AssertEquals("Consignment.BorderModeOfTransport", "4", consignment.BorderModeOfTransport);
			});

			entryInstruction.CEI_SubStyle = "B";
			provider = GetProvider();
			CombineAssertions(() =>
			{
				var consignment = provider.Consignment;
				AssertEquals("Consignment.InlandTransportMode (not included)", string.Empty, consignment.InlandTransportMode);
				AssertEquals("Consignment.BorderModeOfTransport (not included)", string.Empty, consignment.BorderModeOfTransport);
			});
		}

		public void TestWarehouse_Null()
		{
			AssertNull("Warehouse", Provider.Warehouse);
		}

		public void TestWarehouse_B3()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			SetWarehouseData();
			AssertWarehouse(Provider.Warehouse, "R", "TOID01234");
		}

		public void TestWarehouse_Non_B3()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			SetWarehouseData();
			AssertWarehouse(Provider.Warehouse, "U", "FROMID4567");
		}

		void SetWarehouseData()
		{
			var fromWarehouseOwner = Factory.NewWithValidTestData<OrgHeader>();
			var fromWarehouseOwnerAddress = fromWarehouseOwner.MainAddress;
			fromWarehouseOwnerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FROMID4567", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryInstruction.CEI_OA_Warehouse = fromWarehouseOwnerAddress.PK;

			var fromWarehouseUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			fromWarehouseUsage.AGC_Code = "CWP";
			fromWarehouseUsage.AGC_Number = "FROMID1234";
			fromWarehouseUsage.AGC_OH_Owner = fromWarehouseOwner.PK;

			var toWarehouseOwner = Factory.NewWithValidTestData<OrgHeader>();
			var toWarehouseOwnerAddress = toWarehouseOwner.MainAddress;
			toWarehouseOwnerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "TOID01234", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryInstruction.CEI_OA_Warehouse2 = toWarehouseOwnerAddress.PK;

			var toWarehouseUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			toWarehouseUsage.AGC_Code = "CW1";
			toWarehouseUsage.AGC_Number = "TOID56789";
			toWarehouseUsage.AGC_OH_Owner = toWarehouseOwner.PK;
		}

		void AssertWarehouse(IWarehouse warehouse, ZString type, ZString id)
		{
			AssertEquals("Type", type, warehouse.Type);
			AssertEquals("Identifier", id, warehouse.Identifier);
		}

		public void TestGoodsItems()
		{
			AssertType<IE513And515CommonGoodsItemProvider>("GoodsItems should have valid items.", Provider.GoodsItems.Single());
		}
		#endregion

		protected override IE513MessageProvider GetProvider() => new IE513MessageProvider(entryHeader);

		protected override void SetUp()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "ACP", "CUACP", startDate, endDate, "EUN");
			helper.CreateCusMap("EUNAU", "CVA", "CUCVA", startDate, endDate, "EUN");
			Factory.Save();

			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
