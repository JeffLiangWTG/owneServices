using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.MessagesWrappers.Send;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class CusProcedureWrapperTest : TestCaseWithFactory
	{
		public void TestBranchCusBrokerageCode()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "NJG";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "1274BC4E");
			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "CBR_DEC", Core.Constants.CountryCodes.France);

			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "TestMatchAddress";
			declarantAddress.Address1 = "TestMatchAddress";

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_Code = "FR0";
			representative.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "CBR_REP", Core.Constants.CountryCodes.France);
			var representativeAddress = representative.Addresses.AddNew();
			representativeAddress.AddressCode = "TestMatchAddress";
			representativeAddress.Address1 = "TestMatchAddress";

			cusEntryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			cusEntryHeader.Declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			cusEntryHeader.Declaration.JE_OA_Representative = representativeAddress.PK;
			cusEntryHeader.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			cusEntryHeader.Declaration.JE_MessageType = ZString.Empty;
			cusEntryHeader.Declaration.JE_CustomsGuaranteeNumber = "DGUA";
			Factory.Save();

			AssertEquals("CBR retrieved from declarant if exists.", "FRCBR_DEC", cusProcedureExpWrapper.BranchCusBrokerageCode);

			declarant.CustomsCodes.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("CBR retrieved from representative if not configured for declarant.", "FRCBR_REP", cusProcedureExpWrapper.BranchCusBrokerageCode);
		}

		public void TestAgreementOwnerEORI()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Factory.Load<RefCountry>(Core.Constants.CountryGuids.France), "335620241");
			importer.SetCustomsCode(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, Factory.Load<RefCountry>(Core.Constants.CountryGuids.France), "00133");
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_OH = importer.PK;
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TESTACC";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals(importer, declaration.DeltaAccountOrgHeader);

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			AssertEquals("No error should show when the importer has EORI Code.", "FR33562024100133", messageWrapper.CusProcedure.AgreementOwnerEORI);
		}

		public void TestItinerary()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			Transport trans1 = declaration.Transports.AddNew();
			trans1.JW_RL_NKLoadPort = "TRIST";
			trans1.JW_RL_NKDiscPort = "TRIST";

			Transport trans2 = declaration.Transports.AddNew();
			trans2.JW_RL_NKLoadPort = "FRMRS";
			trans2.JW_RL_NKDiscPort = "FRMRS";

			Transport trans3 = declaration.Transports.AddNew();
			trans3.JW_RL_NKDiscPort = "INBOM";
			trans3.JW_RL_NKLoadPort = "INBOM";

			Transport trans4 = declaration.Transports.AddNew();
			trans4.JW_RL_NKDiscPort = "GBLON";
			trans4.JW_RL_NKLoadPort = "GBLON";

			declaration.JE_RL_NKOrigin = "FRPAR";
			declaration.JE_GoodsDestination = "GB";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);

			AssertContainsExactElementsInAnyOrder(entryHeader.CountriesOfRouting, new string[] { "FR", "IN", "GB" });
			AssertEquals("FR", messageWrapper.CusProcedure.DepartureState);
			AssertEquals("GB", messageWrapper.CusProcedure.DestinationState);
			AssertContainsExactElementsInAnyOrder(messageWrapper.CusProcedure.Itinerary, new string[] { "IN" });
		}

		public void TestRepTaxOrganisation()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var fiscalParty = Factory.NewWithValidTestData<OrgHeader>();
			fiscalParty.OH_Code = "FISCAL";
			fiscalParty.MainAddress.Address1 = "FiscalAddress";
			fiscalParty.MainAddress.OA_Code = "FiscalAddress";

			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference.CFR_Reference = "Fiscal_Ref";
			fiscalReference.CFR_OA_Owner = fiscalParty.MainAddress.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			declaration.SetupSupplier(supplier);

			var supplierAddInfo = EUOrgImpAddInfo.Get(supplier, Core.Constants.CountryCodes.France);
			supplierAddInfo.ZO_UseFr3FiscalRepresentation = false;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			declaration.SetupImporter(importer);

			var importerAddInfo = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
			importerAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("Tax Representative should be the Fiscal, because declaration is Import and the importer choose to use FR3.", "FiscalAddress", messageWrapper.CusProcedure.RepTaxOrganisation.Address);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertNull("Tax Representative should be null, because declaration is Export and the supplier choose not to use FR3", messageWrapper.CusProcedure.RepTaxOrganisation);
		}

		public void TestErrorMessageWhenImporterSRTNotConfigured()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			var orgNumber = messageWrapper.CusProcedure.Importers.First().OrganisationNumber;

			AssertEquals("An error should show when the importer misses SRT Code.", 0, errorCollector.ErrorCount);
		}

		public void TestMultipleSuppliers()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_GoodsOrigin = "QR";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.SetupSupplier(supplier);

			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader1.JZ_OH_Supplier = invoiceSupplier.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceHeader2.JZ_OH_Supplier = invoiceSupplier.PK;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine3_2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceHeader3.JZ_OH_Supplier = ZGuid.Empty;

			var invoiceHeader4 = declaration.Invoices.AddNew();
			var invoiceLine4 = invoiceHeader4.InvoiceLines.AddNew();
			invoiceHeader4.JZ_OH_Supplier = supplier.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_CL = entryLine1.PK;
			invoiceLine2_2.JI_CL = entryLine2.PK;
			invoiceLine3_1.JI_CL = entryLine1.PK;
			invoiceLine3_2.JI_CL = entryLine2.PK;

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("Import type of declaration - G1 ProcedureWrapper Suppliers property collects both invoice header and declaration suppliers and ensures all suppliers are unique.", 2, procedureWrapper.Suppliers.Count());

			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Import type of declaration - G1 ProcedureWrapper Suppliers property collects both invoice header and declaration suppliers when E2_AddressOverride is true and ensures all suppliers are unique.", 2, procedureWrapper.Suppliers.Count());

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			AssertEquals("Import type of declaration - G2 ProcedureWrapper Suppliers property collects only the declaration supplier.", 1, procedureWrapper.Suppliers.Count());

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("Export type of declaration - G1 ProcedureWrapper Suppliers property collects only the declaration supplier.", 1, procedureWrapper.Suppliers.Count());

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			AssertEquals("Export type of declaration - G2 ProcedureWrapper Suppliers property collects only the declaration supplier.", 1, procedureWrapper.Suppliers.Count());
		}

		public void TestMultipleImporters()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_GoodsDestination = "QR";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.SetupImporter(importer);

			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader1.JZ_OH_Buyer = invoiceImporter.PK;
			invoiceHeader1.JZ_OA_BuyerAddress = invoiceImporter.MainAddress.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceHeader2.JZ_OH_Buyer = invoiceImporter.PK;
			invoiceHeader2.JZ_OA_BuyerAddress = invoiceImporter.MainAddress.PK;

			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine3_2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceHeader3.JZ_OH_Buyer = ZGuid.Empty;
			invoiceHeader3.JZ_OA_BuyerAddress = ZGuid.Empty;

			var invoiceHeader4 = declaration.Invoices.AddNew();
			var invoiceLine4 = invoiceHeader4.InvoiceLines.AddNew();
			invoiceHeader4.JZ_OH_Buyer = importer.PK;
			invoiceHeader4.JZ_OA_BuyerAddress = importer.MainAddress.PK;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_CL = entryLine1.PK;
			invoiceLine2_2.JI_CL = entryLine2.PK;
			invoiceLine3_1.JI_CL = entryLine1.PK;
			invoiceLine3_2.JI_CL = entryLine2.PK;

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("Export type of declaration - G1 ProcedureWrapper Importers property collects both invoice header and declaration importers and ensures all importers are unique.", 2, procedureWrapper.Importers.Count());

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals("Export type of declaration - G1 ProcedureWrapper Importers property collects both invoice header and declaration importers when E2_AddressOverride is true and ensures all importers are unique.", 2, procedureWrapper.Importers.Count());

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			AssertEquals("Export type of declaration - G2 ProcedureWrapper Importers property collects only the declaration importer.", 1, procedureWrapper.Importers.Count());

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("Import type of declaration - G1 ProcedureWrapper Importers property collects only the declaration importer.", 1, procedureWrapper.Importers.Count());

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			AssertEquals("Import type of declaration - G2 ProcedureWrapper Importers property collects only the declaration importer.", 1, procedureWrapper.Importers.Count());
		}

		public void TestPackageCount()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var package1 = (Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 3;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals(10m, entryHeader.CustomsPackageCount);

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;

			var messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;
			AssertEquals(10, procedureWrapper.PackageCount);
		}

		public void TestEntryGoodsPriceSum()
		{
			AssertEquals(50m, cusProcedureExpWrapper.EntryGoodsPriceSum);
			AssertEquals("EUR", cusProcedureExpWrapper.EntryGoodsPriceCurrency);
		}

		public void TestState()
		{
			CreateNewOrGetExistingRefUNLOCO(WrapperTestHelper.ExportTransportOrigin);
			CreateNewOrGetExistingRefUNLOCO(WrapperTestHelper.ExportTransportDestination);
			AssertEquals("GB", cusProcedureExpWrapper.ArrivalState);
			AssertEquals("FR", cusProcedureExpWrapper.DepartureState);
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
				result.RL_RN_NKCountryCode = code.Left(2);
				Factory.Save();
			}
			return result;
		}

		public void TestVariousOperationCreditNumber()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "NJG";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "1274BC4E");

			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "TestMatchAddress";
			declarantAddress.Address1 = "TestMatchAddress";

			cusEntryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			cusEntryHeader.Declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			cusEntryHeader.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			cusEntryHeader.Declaration.JE_MessageType = ZString.Empty;
			cusEntryHeader.Declaration.JE_CustomsGuaranteeNumber = "DGUA";

			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			Factory.Save();

			AssertEquals("REFA", cusProcedureExpWrapper.VariousOperationCreditNumber);
		}

		public void TestTransportCostsAndInsurance()
		{
			var declaration = Factory.NewWithValidTestData<Declaration.JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			#region OFT

			var new_ThirdCountry_OFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_OFT_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_OFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_OFT_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_OFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_OFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_OFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_OFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_OFT_Included_Charge.J7_IsDutiable = false;
			new_EU_OFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_OFT_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_OFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_OFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_OFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_OFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_OFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_OFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_OFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_OFT_Included_Charge.J7_IsDutiable = false;
			new_Domestic_OFT_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_OFT_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_OFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_OFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 1.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_OFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_OFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_OFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_OFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region ONS

			var new_ThirdCountry_ONS_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ONS_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ONS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ONS_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ONS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_ONS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ONS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_ONS_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ONS_Included_Charge.J7_IsDutiable = false;
			new_EU_ONS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ONS_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_ONS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_ONS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ONS_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_ONS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ONS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_ONS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_ONS_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ONS_Included_Charge.J7_IsDutiable = false;
			new_Domestic_ONS_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ONS_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ONS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_ONS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 2.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ONS_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_ONS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ONS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ONS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region AFT

			var new_ThirdCountry_AFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_AFT_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_AFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_AFT_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_AFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_AFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_AFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_AFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_AFT_Included_Charge.J7_IsDutiable = false;
			new_EU_AFT_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_AFT_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_AFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_AFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_AFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_AFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_AFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_AFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_AFT_Included_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_AFT_Included_Charge.J7_IsDutiable = false;
			new_Domestic_AFT_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_AFT_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_AFT_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_AFT_NotIncluded_Charge = invoiceHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 3.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_AFT_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_AFT_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_AFT_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_AFT_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region ANS

			var new_ThirdCountry_ANS_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ANS_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ANS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ANS_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ANS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_ANS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_ANS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_ANS_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ANS_Included_Charge.J7_IsDutiable = false;
			new_EU_ANS_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ANS_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_ANS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_ANS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_ANS_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_ANS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_ANS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_ANS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_ANS_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ANS_Included_Charge.J7_IsDutiable = false;
			new_Domestic_ANS_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ANS_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ANS_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_ANS_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 4.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_ANS_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_ANS_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_ANS_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_ANS_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CEI

			var new_ThirdCountry_CEI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CEI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CEI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEI_Included_Charge.J7_IsDutiable = false;
			new_EU_CEI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CEI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CEI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CEI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CEI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CEI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CEI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CEI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, 5.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CEI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CNI

			var new_ThirdCountry_CNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNI_Included_Charge.J7_IsDutiable = false;
			new_EU_CNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CNI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, 6.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CEE

			var new_ThirdCountry_CEE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CEE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CEE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CEE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEE_Included_Charge.J7_IsDutiable = false;
			new_EU_CEE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CEE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CEE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CEE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CEE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CEE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CEE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CEE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CEE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CEE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, 7.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CEE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CEE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CEE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CEE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region CNE

			var new_ThirdCountry_CNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_CNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_CNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_CNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNE_Included_Charge.J7_IsDutiable = false;
			new_EU_CNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_CNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_CNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_CNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_CNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_CNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_CNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_CNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_CNE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_CNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, 8.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_CNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_CNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_CNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_CNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FRI

			var new_ThirdCountry_FRI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FRI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FRI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRI_Included_Charge.J7_IsDutiable = false;
			new_EU_FRI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FRI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FRI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FRI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FRI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FRI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FRI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FRI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, 9.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FRI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FNI

			var new_ThirdCountry_FNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNI_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNI_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNI_Included_Charge.J7_IsDutiable = false;
			new_EU_FNI_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNI_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FNI_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNI_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FNI_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNI_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNI_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FNI_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, 10.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNI_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FNI_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNI_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNI_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FRE

			var new_ThirdCountry_FRE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FRE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FRE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FRE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRE_Included_Charge.J7_IsDutiable = false;
			new_EU_FRE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FRE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FRE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FRE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FRE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FRE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FRE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FRE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FRE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FRE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, 11.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FRE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FRE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FRE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FRE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region FNE

			var new_ThirdCountry_FNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.1m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNE_Included_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNE_Included_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_ThirdCountry_FNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.2m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsDutiable = true;
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_ThirdCountry_FNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_EU_FNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.3m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNE_Included_Charge.J7_IsDutiable = false;
			new_EU_FNE_Included_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNE_Included_Charge.J7_IsGSTApplicable = true;
			new_EU_FNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_EU_FNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.4m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_EU_FNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_EU_FNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = true;
			new_EU_FNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_EU_FNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			var new_Domestic_FNE_Included_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.5m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNE_Included_Charge.J7_IsDutiable = false;
			new_Domestic_FNE_Included_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNE_Included_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNE_Included_Charge.J7_IsNotIncludedInInvoice = false;

			var new_Domestic_FNE_NotIncluded_Charge = invoiceHeader.Charges.AddNew(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, 12.6m, Enterprise.Core.Constants.CurrencyCodes.France);
			new_Domestic_FNE_NotIncluded_Charge.J7_IsDutiable = false;
			new_Domestic_FNE_NotIncluded_Charge.J7_IsStatisticalValueApplicable = false;
			new_Domestic_FNE_NotIncluded_Charge.J7_IsGSTApplicable = true;
			new_Domestic_FNE_NotIncluded_Charge.J7_IsNotIncludedInInvoice = true;

			foreach (var charge in invoiceHeader.Charges)
			{
				charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			}

			#endregion

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_CL = cusEntryLine.PK;

			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.ZG_AgreedPlaceCode = "3";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_AirRouteType = "2";

			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;

			AssertEquals(4.2m, procedureWrapper.ThirdCountryTransportCosts.Costs.Amount);
			AssertEquals(7.2m, procedureWrapper.ThirdCountryTransportCosts.Insurance.Amount);
			AssertEquals(10.2m, procedureWrapper.ThirdCountryAirCosts.Costs.Amount);
			AssertEquals(13.2m, procedureWrapper.ThirdCountryAirCosts.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsInInvoice.Costs.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsInInvoice.Insurance.Amount);
			AssertEquals(22.2m, procedureWrapper.EUTransportCostsNotInInvoice.Costs.Amount);
			AssertEquals(25.2m, procedureWrapper.EUTransportCostsNotInInvoice.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.FRAirCosts.Costs.Amount);
			AssertEquals(0m, procedureWrapper.FRAirCosts.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.FRTransportCostsInInvoice.Costs.Amount);
			AssertEquals(0m, procedureWrapper.FRTransportCostsInInvoice.Insurance.Amount);
			AssertEquals(34.2m, procedureWrapper.FRTransportCostsNotInInvoice.Costs.Amount);
			AssertEquals(37.2m, procedureWrapper.FRTransportCostsNotInInvoice.Insurance.Amount);

			declaration.JE_ShipmentIncoTerm = "DDP";
			declaration.ZG_AgreedPlaceCode = "1";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_AirRouteType = "";

			Factory.Save();

			messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);
			procedureWrapper = messageWrapper.CusProcedure;

			AssertEquals(0m, procedureWrapper.ThirdCountryTransportCosts.Costs.Amount);
			AssertEquals(0m, procedureWrapper.ThirdCountryTransportCosts.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.ThirdCountryAirCosts.Costs.Amount);
			AssertEquals(0m, procedureWrapper.ThirdCountryAirCosts.Insurance.Amount);
			AssertEquals(15.9m, procedureWrapper.EUTransportCostsInInvoice.Costs.Amount);
			AssertEquals(18.9m, procedureWrapper.EUTransportCostsInInvoice.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsNotInInvoice.Costs.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsNotInInvoice.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.FRAirCosts.Costs.Amount);
			AssertEquals(0m, procedureWrapper.FRAirCosts.Insurance.Amount);
			AssertEquals(27.9m, procedureWrapper.FRTransportCostsInInvoice.Costs.Amount);
			AssertEquals(30.9m, procedureWrapper.FRTransportCostsInInvoice.Insurance.Amount);
			AssertEquals(34.2m, procedureWrapper.FRTransportCostsNotInInvoice.Costs.Amount);
			AssertEquals(37.2m, procedureWrapper.FRTransportCostsNotInInvoice.Insurance.Amount);

			declaration.JE_ShipmentIncoTerm = "FOB";
			declaration.ZG_AgreedPlaceCode = "3";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_AirRouteType = "5";

			Factory.Save();

			messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);
			procedureWrapper = messageWrapper.CusProcedure;
			Factory.Save();

			AssertEquals(1.2m, procedureWrapper.ThirdCountryTransportCosts.Costs.Amount);
			AssertEquals(2.2m, procedureWrapper.ThirdCountryTransportCosts.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.ThirdCountryAirCosts.Costs.Amount);
			AssertEquals(0m, procedureWrapper.ThirdCountryAirCosts.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsInInvoice.Costs.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsInInvoice.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsNotInInvoice.Costs.Amount);
			AssertEquals(0m, procedureWrapper.EUTransportCostsNotInInvoice.Insurance.Amount);
			AssertEquals(7.0m, procedureWrapper.FRAirCosts.Costs.Amount);
			AssertEquals(9.0m, procedureWrapper.FRAirCosts.Insurance.Amount);
			AssertEquals(0m, procedureWrapper.FRTransportCostsInInvoice.Costs.Amount);
			AssertEquals(0m, procedureWrapper.FRTransportCostsInInvoice.Insurance.Amount);
			AssertEquals(34.2m, procedureWrapper.FRTransportCostsNotInInvoice.Costs.Amount);
			AssertEquals(37.2m, procedureWrapper.FRTransportCostsNotInInvoice.Insurance.Amount);
		}

		public void TestOtherCharges()
		{
			var declaration = Factory.NewWithValidTestData<Declaration.JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_CL = cusEntryLine.PK;

			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, true);
			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, false, false, true);

			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 10m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, false, false, true);
			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.AdjustmentCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, true, true, false);

			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.InterestCharge, 100m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, true);
			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.InterestCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, false);

			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, 1000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 10000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 100000m, Enterprise.Core.Constants.CurrencyCodes.France, false, false, true, true, true);
			CreateInvoiceCharge(invoiceHeader, UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, 0.1m, Enterprise.Core.Constants.CurrencyCodes.France, true, true, false, false, false);

			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;

			AssertEquals("Commission", 1.1m, procedureWrapper.Commission.Amount);
			AssertEquals("VATBaseCosts", 10m, procedureWrapper.VATBaseCosts.Amount);
			AssertEquals("Interest", 100m, procedureWrapper.Interest.Amount);
			AssertEquals("OtherAddedCosts", 111000m, procedureWrapper.OthAddedCosts.Amount);
		}

		void CreateInvoiceCharge(JobComInvoiceHeader invoice, ZString chargeCode, ZDecimal amount, ZString currency, bool isIncludedInInvoice, bool isIncludedInInvoiceLine, bool isDutiable, bool isStatable, bool isVatable)
		{
			var charge = invoice.Charges.AddNew(chargeCode, amount, currency);
			charge.J7_ChargeType = chargeCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsIncludedInITOT = isIncludedInInvoice;
			charge.J7_IsNotIncludedInInvoice = !isIncludedInInvoiceLine;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsStatisticalValueApplicable = isStatable;
			charge.J7_IsGSTApplicable = isVatable;
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
		}

		public void TestExportPropertiesExist()
		{
			AssertEquals(3, cusProcedureExpWrapper.Itinerary.Count());
			AssertEquals("", cusProcedureExpWrapper.CommercialReference);
			AssertEquals(SpecificCircumstanceIndicator.Codes.RoadModeOfTransport, cusProcedureExpWrapper.SpecificCircumstanceIndicator);
			AssertEquals(WrapperTestHelper.ExportTransportOrigin, cusProcedureExpWrapper.OrigineState);
			AssertEquals(WrapperTestHelper.ExportTransportDestination, cusProcedureExpWrapper.DestinationState);
		}

		public void TestProcedureType()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "0001";
			declaration.JE_OH_Importer = importer.PK;

			var importerAddress = importer.Addresses.AddNew();
			importerAddress.AddressCode = "TestMatchAddress";
			importerAddress.Address1 = "TestMatchAddress";

			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DG1", ZString.Empty, ZString.Empty, "AB03FCC5");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DG2", ZString.Empty, ZString.Empty, "AB03FCC5");

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
			Factory.Save();

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_CustomsProfile = "DG1";

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendExpMessageWrapper(messageObject, errorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;
			CombineAssertions(() =>
			{
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				declaration.JE_CustomsProfile = "DG1";
				AssertEquals("G1 DeltaMode", "C", procedureWrapper.ProcedureType);

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				declaration.JE_CustomsProfile = "DG2";
				AssertEquals("G2 DeltaMode", ZString.Empty, cusProcedureExpWrapper.ProcedureType);
			});
		}

		public void TestDELTACAgreementNumber()
		{
			AssertEquals("", cusProcedureExpWrapper.DeltaGAuthorisationNumber);
			errorCollector.WipeErrors();
			var declaration = cusEntryHeader.Declaration;
			declaration.JE_CustomsProfile = "TEST1";
			AssertEquals("TEST1", cusProcedureExpWrapper.DeltaGAuthorisationNumber);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			declaration.JE_CustomsProfile = "";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", cusProcedureExpWrapper.DeltaGAuthorisationNumber);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("", cusProcedureExpWrapper.DeltaGAuthorisationNumber);
			AssertEquals(0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
		}

		public void TestImporterEORINumber()
		{
			AssertEquals(ZString.Empty, cusProcedureExpWrapper.ImporterEORINumber);

			var declaration = cusEntryHeader.Declaration;
			var importer = declaration.Importer;
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "34430738400570", Core.Constants.CountryCodes.France);
			AssertEquals("FR34430738400570", cusProcedureExpWrapper.ImporterEORINumber);
		}

		public void TestEntrySubStyle()
		{
			cusEntryHeader.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			cusEntryHeader.EntryInstruction.CEI_SubStyle = "Y";

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			var cusProcedureWrapper = messageWrapper.CusProcedure;
			AssertEquals("Y", cusProcedureWrapper.EntryStyleCode);

			messageObject.MessageType = EntryActionCodeList.Codes.MAP;
			var messageWrapper2 = new DCSendImpMessageWrapper(messageObject, errorCollector);
			var cusProcedureWrapper2 = messageWrapper2.CusProcedure;
			AssertEquals("Y", cusProcedureWrapper2.EntryStyleCode);

			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			var messageWrapper3 = new DCSendImpMessageWrapper(messageObject, errorCollector);
			var cusProcedureWrapper3 = messageWrapper3.CusProcedure;
			AssertEquals("Y", cusProcedureWrapper3.EntryStyleCode);
		}

		public void TestGetGuaranteeMode()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "NJG";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "1274BC4E");

			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "TestMatchAddress";
			declarantAddress.Address1 = "TestMatchAddress";

			cusEntryHeader.Declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			cusEntryHeader.Declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			cusEntryHeader.Declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			cusEntryHeader.Declaration.JE_MessageType = ZString.Empty;
			cusEntryHeader.Declaration.JE_CustomsGuaranteeNumber = "DGUA";

			var guarantee = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			var rule = guarantee.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.MOD;

			Factory.Save();

			AssertEquals("declaration.CustomsGuarantee.GuaranteeModeCode", "C", cusProcedureExpWrapper.GuaranteeMode);
		}

		public void TestImportersAndSupplier()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			supplier.MainAddress.OA_RN_NKCountryCode = "FR";

			var testedSuppAddress = supplier.Addresses.AddNew();
			testedSuppAddress.Address1 = "addressAdded";
			testedSuppAddress.OA_Code = "addressAdded";
			declaration.SetupSupplier(supplier);

			declaration.SupplierDocumentaryAddress.E2_OA_Address = testedSuppAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			importer.MainAddress.OA_RN_NKCountryCode = "FR";

			var testedImpAddress = importer.Addresses.AddNew();
			testedImpAddress.Address1 = "ImpaddressAdded";
			testedImpAddress.OA_Code = "ImpaddressAdded";

			declaration.SetupImporter(importer);
			declaration.ImporterDocumentaryAddress.E2_OA_Address = testedImpAddress.PK;

			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			AssertEquals("ImpaddressAdded", messageWrapper.CusProcedure.Importers.ElementAt(0).Address);
			AssertEquals("FR", messageWrapper.CusProcedure.Importers.ElementAt(0).CountryCode);

			AssertEquals("addressAdded", messageWrapper.CusProcedure.Suppliers.ElementAt(0).Address);
			AssertEquals("FR", messageWrapper.CusProcedure.Suppliers.ElementAt(0).CountryCode);

			declaration.JE_GoodsOrigin = "IT";
			declaration.JE_GoodsDestination = "QA";
			messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			AssertEquals("JE_GoodsOrigin is not a non-standard country code.", "FR", messageWrapper.CusProcedure.Suppliers.ElementAt(0).CountryCode);
			AssertEquals("JE_GoodsDestination is not a non-standard country code.", "FR", messageWrapper.CusProcedure.Importers.ElementAt(0).CountryCode);

			declaration.JE_GoodsOrigin = "QR";
			declaration.JE_GoodsDestination = "QP";
			messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			AssertEquals("If JE_GoodsOrigin is a non-standard country code, the countryCode of the supplier from the main tab should be equal to JE_GoodsOrigin.", "QR", messageWrapper.CusProcedure.Suppliers.ElementAt(0).CountryCode);
			AssertEquals("If JE_GoodsDestination is a non-standard country code, the countryCode of the importer from the main tab should be equal to JE_GoodsDestination.", "QP", messageWrapper.CusProcedure.Importers.ElementAt(0).CountryCode);
		}

		public void TestImportersAndSupplierWhenSupplierDocumentaryAddressOrImporterDocumentaryAddressIsOverrided()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			supplier.MainAddress.OA_RN_NKCountryCode = "FR";

			var testedSuppAddress = supplier.Addresses.AddNew();
			testedSuppAddress.Address1 = "addressAdded";
			testedSuppAddress.OA_Code = "addressAdded";
			testedSuppAddress.OA_RN_NKCountryCode = "FR";
			declaration.SetupSupplier(supplier);

			declaration.SupplierDocumentaryAddress.E2_OA_Address = testedSuppAddress.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			importer.MainAddress.OA_RN_NKCountryCode = "FR";

			var testedImpAddress = importer.Addresses.AddNew();
			testedImpAddress.Address1 = "ImpaddressAdded";
			testedImpAddress.OA_Code = "ImpaddressAdded";
			testedImpAddress.OA_RN_NKCountryCode = "FR";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = testedImpAddress.PK;

			declaration.SetupImporter(importer);

			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			AssertEquals("ImporterAddress", messageWrapper.CusProcedure.Importers.ElementAt(0).Address);

			AssertEquals("addressAdded", messageWrapper.CusProcedure.Suppliers.ElementAt(0).Address);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;

			declaration.SupplierDocumentaryAddress.E2_Address1 = "supplier overrided";
			declaration.ImporterDocumentaryAddress.E2_Address1 = "Importer overrided";

			Factory.Save();

			messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			AssertEquals("Importer overrided", messageWrapper.CusProcedure.Importers.ElementAt(0).Address);
			AssertEquals("FR", messageWrapper.CusProcedure.Importers.ElementAt(0).CountryCode);

			AssertEquals("supplier overrided", messageWrapper.CusProcedure.Suppliers.ElementAt(0).Address);
			AssertEquals("FR", messageWrapper.CusProcedure.Suppliers.ElementAt(0).CountryCode);

			declaration.JE_GoodsOrigin = "IT";
			declaration.JE_GoodsDestination = "QA";
			messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			AssertEquals("JE_GoodsOrigin is not a non-standard country code.", "FR", messageWrapper.CusProcedure.Suppliers.ElementAt(0).CountryCode);
			AssertEquals("JE_GoodsDestination is not a non-standard country code.", "FR", messageWrapper.CusProcedure.Importers.ElementAt(0).CountryCode);

			declaration.JE_GoodsOrigin = "QR";
			declaration.JE_GoodsDestination = "QP";
			messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			AssertEquals("If JE_GoodsOrigin is a non-standard country code, the countryCode of the supplier from the main tab should be equal to JE_GoodsOrigin.", "QR", messageWrapper.CusProcedure.Suppliers.ElementAt(0).CountryCode);
			AssertEquals("If JE_GoodsDestination is a non-standard country code, the countryCode of the importer from the main tab should be equal to JE_GoodsDestination.", "QP", messageWrapper.CusProcedure.Importers.ElementAt(0).CountryCode);
		}

		public void TestValuationBypassCode()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			AssertEquals("Bypass code have to be empty", string.Empty, messageWrapper.CusProcedure.ValuationBypassCode);

			entryInstruction.ZG_BypassCode = "A";
			AssertEquals("Bypass code have to be equal to entry instruction's one", "A", messageWrapper.CusProcedure.ValuationBypassCode);

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertEquals("Bypass code have to be empty", string.Empty, messageWrapper.CusProcedure.ValuationBypassCode);
		}

		public void TestValuationBypassReason()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			Factory.Save();

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);

			AssertEquals("Bypass reason have to be empty", string.Empty, messageWrapper.CusProcedure.ValuationBypassReason);

			entryInstruction.ZG_BypassReason = "AAAA";
			AssertEquals("Bypass reason have to be equal to entry instruction's one", "AAAA", messageWrapper.CusProcedure.ValuationBypassReason);

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertEquals("Bypass reason have to be empty", string.Empty, messageWrapper.CusProcedure.ValuationBypassReason);
		}

		protected override void SetUp()
		{
			base.SetUp();

			errorCollector = new ErrorCollector();
			cusEntryHeader = new WrapperTestHelper().CreateTestCusEntryHeader(false);

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;

			var messageWrapper = new DCSendImpMessageWrapper(messageObject, errorCollector);
			cusProcedureExpWrapper = messageWrapper.CusProcedure;
		}

		Declaration.CusEntryHeader cusEntryHeader;
		ICusProcedure cusProcedureExpWrapper;
		ErrorCollector errorCollector;
	}
}
