using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Customs.NL.MessageDefinitions.DS;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.NL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction;
using InvoiceHeaderActiveCollection = Enterprise.Customs.NL.Business.Declaration.InvoiceHeaderActiveCollection;

namespace Enterprise.Customs.NL.Business.Testing;

public static class WrapperTestHelper
{
	public static CusEntryHeader GetEmptyEntryDataForTest(BusinessObjectFactory factory)
	{
		CusEntryHeader entryHeader;

		var declaration = factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.Declarant.Header.Contacts.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_CEI = entryInstruction.PK;

		factory.Save();
		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);

		factory.Save();

		entryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();

		return entryHeader;
	}

	[UseSnapshotProtection]
	public static CusEntryHeader GetEntryHeaderForTest(BusinessObjectFactory factory)
	{
		CusEntryHeader entryHeader;
		CreateCusCodeLists(factory);

		var orgHeaderBuyer = CreateOrgHeader(factory, "Buyer Full Name", "BUYER", "321654");
		CreateAddress(orgHeaderBuyer, "BUY", "Buyerweg 1", "Buyerstad", "3333BY");
		var addressBuyer = CreateAddress(orgHeaderBuyer.Addresses, "BUY", "Dorpstaat 10", "Rotterdam", "1079CK");
		var orgHeaderExporter = CreateOrgHeader(factory, "Exporter Full Name", "EXPORTER", "654321");
		var addressExporter = CreateAddress(orgHeaderExporter, "EXP", "Havenweg 3", "Rotterdam", "1079CK");
		CreateContact(orgHeaderExporter, "Exporter Contact Name", "+3164259513", "exporter@mail.nl");
		var orgHeaderSeller = CreateOrgHeader(factory, "Seller Full Name", "SELLER", "123456");
		var addressSeller = CreateAddress(orgHeaderSeller, "SEL", "Rijksweg 102", "Deventer", "7201MG");
		var orgHeaderWarehouse = CreateOrgHeader(factory, "Warehouse Full Name", "WAREHOUSE", "6247645");
		var addressWarehouse = CreateAddress(orgHeaderWarehouse.Addresses, "WHS", "Valreep 13", "Amsterdam", "7201MG");
		var orgHeaderImporter = CreateOrgHeader(factory, "Importer Full Name", "IMPORTER", "22334455");
		orgHeaderImporter.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Tariff;
		var addressImporter = CreateAddress(orgHeaderImporter, "CST", "Importeursweg 37", "IJburg", "2222AB");
		var orgHeaderControllingAgent = CreateOrgHeader(factory, "Controlling Agent Full Name", "AGENT", "43434343");
		var addressControllingAgent = CreateAddress(orgHeaderControllingAgent, "COA", "ControllingAgentstraat 20", "Colmschate", "7420AA");
		var orgHeaderDeclarant = CreateOrgHeader(factory, "Declarant Full Name", "DECLARANT", "56785678");
		var addressDeclarant = CreateAddress(orgHeaderDeclarant, "CST", "Declarantenstraat 30", "Decapolis", "5890DW");
		CreateContact(orgHeaderDeclarant, "Declarant Contact Name", "+31592874125", "declarant@mail.nl");
		var orgHeadercontrollingCustomer = CreateOrgHeader(factory, "Controlling Customer Full Name", "CCustomer", "78907890");
		var orgHeaderGuaranteeOffice1 = CreateOrgHeader(factory, "Guarantee Office 1", "OFF", "321654");
		var orgHeaderGuaranteeOffice2 = CreateOrgHeader(factory, "Guarantee Office 2", "OFF", "456123");
		var addressGuaranteeOffice = CreateAddress(orgHeaderGuaranteeOffice1.Addresses, "OFF", "Dorpstaat 10", "Rotterdam", "1079CK");
		var orgHeaderCarrier = CreateOrgHeader(factory, "Carrier Full Name", "DIR", "53212346");
		var addressCarrier = CreateAddress(orgHeaderCarrier, "CST", "Carrierstraat 30", "Amsterdam", "3434DW");
		var orgHeaderRepresentative = WrapperTestHelper.CreateOrgHeader(factory, "Representative Full Name", "", "78787878");
		var addressRepresentative = CreateAddress(orgHeaderRepresentative, "", "Rijksweg 102", "Deventer", "7201MG");

		CreateAuthorizationHeader(orgHeaderWarehouse);

		var declaration = CreateDeclaration(factory, orgHeaderSeller.PK, addressSeller.PK, orgHeaderExporter.PK, orgHeaderImporter.PK, orgHeaderControllingAgent.PK, addressDeclarant.PK, orgHeadercontrollingCustomer.PK, orgHeaderWarehouse.PK, orgHeaderBuyer.PK, orgHeaderCarrier.PK, addressRepresentative.PK);
		var cw1 = CreateDeclarationPackage(declaration.Bills[1].PackingGroups[0].Packages, 10, "BX", "AS ABOVE");
		var cw2 = CreateDeclarationPackage(declaration.Bills[1].PackingGroups[0].Packages, 1, "PK", "AWB 176 50455495");
		CreatePreviousDocument(declaration.PreviousDocuments, 1, "PRV-357", "IMH", "IMH1");
		CreateCustomsOffices(declaration);
		CreateInlandTransport(declaration, EU.Business.Declaration.ExportInlandTransportTypeList.Codes._10, Core.Constants.CountryCodes.Netherlands);
		var orgHeaderAuthorization = CreateOrgHeader(factory, "Authorization Full Name", "SUPPLIER", "112233");
		var entryInstruction = CreateEntryInstruction(factory, declaration.CustomsEntryInstructions, "Entry Instruction Description", "A", "H1", "H1", orgHeaderAuthorization.PK);

		CreatePreviousDocument(entryInstruction.PreviousDocuments, 1, "EIV-321", "IMA", "IMA1");
		CreatePreviousDocument(entryInstruction.PreviousDocuments, 2, "EIV-987", "IMZ", "IMZ1");

		CreateAddInfo(entryInstruction.AdditionalInfos, "INF", "INF-123", "INF", "AdditionalDescription");
		CreateAddInfo(entryInstruction.AdditionalInfos, "INF", "INF-456", "IN2", "AdditionalDescription2");
		CreateAddInfo(entryInstruction.AdditionalInfos, "REF", "REF-123", "REF");
		CreateAddInfo(entryInstruction.AdditionalInfos, "REF", "REF-456", "RF2");
		CreateAddInfo(entryInstruction.AdditionalInfos, "TRA", "TRA-123", "TRA");
		CreateAddInfo(entryInstruction.AdditionalInfos, "TRA", "TRA-456", "TR2");

		CreateContainer(declaration.CusContainers, "MSCU0051257", "913746", "", "");
		CreateContainer(declaration.CusContainers, "APLU8521458", "85236", "614283", "498562");

		CreateSupportingDocument(declaration.SupportingDocuments, 1, "Reference", "SubmitterRef", new ZDateTime(2021, 09, 24, 15, 23, 37));
		CreateSupportingDocument(declaration.SupportingDocuments, 2, "Reference2", "SubmitterRef2", new ZDateTime(2021, 09, 24, 15, 25, 23));

		var invoice = CreateInvoice(declaration.Invoices, orgHeaderBuyer.PK, addressBuyer.PK, orgHeaderExporter.PK, addressExporter.PK, orgHeaderImporter.PK);

		CreateSupportingDocument(invoice.SupportingDocuments, 1, "REF-123", "REF2-123", new ZDateTime(2021, 12, 31, 23, 59, 59), "REF");
		CreateSupportingDocument(invoice.SupportingDocuments, 1, "REF-789", "REF2-456", new ZDateTime(2021, 12, 25, 21, 23, 49), "RF2");

		var invoiceLine1 = CreateInvoiceLine(factory, invoice.InvoiceLines, 1, entryInstruction.PK, orgHeaderSeller.PK, addressSeller.PK, 7, 6, 2, 15, "3926909790", orgHeaderAuthorization.PK, "6");
		CreateInvoiceLinePackage(invoiceLine1.PackagesPivot, 4, cw1.PK, declaration.PK);

		var invoiceLine2 = CreateInvoiceLine(factory, invoice.InvoiceLines, 2, entryInstruction.PK, orgHeaderSeller.PK, addressSeller.PK, 5, 4, 1, 16, "3926909790", orgHeaderAuthorization.PK, "8");
		CreateInvoiceLinePackage(invoiceLine2.PackagesPivot, 5, cw2.PK, declaration.PK);

		factory.Save();
		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);

		factory.Save();

		entryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
		entryHeader.MovementReferenceNumberSetter("MRN123", new ZDateTime(2021, 09, 24, 15, 21, 00));
		entryHeader.CH_BGMReference = "EH00001";
		entryHeader.CH_EntryStatus = "428";
		entryHeader.CH_ExitDate = new ZDateTime(2023, 08, 03);

		entryHeader.EntryInstruction.GoodsLocation.CGL_Type = "A";
		entryHeader.EntryInstruction.GoodsLocation.CGL_AdditionalIdentifier = "Kerkstraat 1";
		entryHeader.EntryInstruction.GoodsLocation.Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		entryHeader.EntryInstruction.GoodsLocation.Address.E2_Postcode = "1234AB";

		var cusEntryLine = entryHeader.MergedLines.First();
		cusEntryLine.CL_Description = "Test Description";
		cusEntryLine.CL_StatisticalValue = 10.2;
		cusEntryLine.CL_CustomsValue = 20.3;

		CreateFee(cusEntryLine.Fees, "CT1", 1m, "AB", "%", 10, 1);
		CreateFee(cusEntryLine.Fees, "CT2", 2m, "CD", "X", 20, 2);

		var message = entryHeader.Messages.AddNew();
		message.EM_ApplicationCode = "NLC";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_MessageNum = "123";

		CreateGuarantee(declaration, 699, "EUR", "GUARANTEEREF", "4321", "NL004321", "0", "OTH", orgHeaderGuaranteeOffice1);
		CreateGuarantee(declaration, 899, "EUR", "GUARANTEEREF2", "1234", "NL001234", "1", NLConstants.GuaranteeReferenceTypes.Guarantee, orgHeaderGuaranteeOffice2);
		return entryHeader;
	}

	public static void CreateCusCodeLists(BusinessObjectFactory factory)
	{
		var cty = Core.Constants.CountryCodes.Netherlands;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(cty);

		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		var attributeNameValuePairs = new Dictionary<string, string[]>();

		attributeNameValuePairs.Clear();
		attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(cty, new string[] { importCodeType, exportCodeType }, "SPCD1", "SPCD1 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		helper.CreateCusCodeListsForMultipleTypesWithAttributes(cty, new string[] { importCodeType, exportCodeType }, "SPCD2", "SPCD2 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		factory.Save();
	}

	public static OrgHeader CreateOrgHeader(BusinessObjectFactory factory, ZString fullName, ZString code, ZString customsRegNo)
	{
		var orgHeader = factory.New<OrgHeader>();
		orgHeader.OH_FullName = fullName;
		orgHeader.OH_Code = code;
		orgHeader.OH_RL_NKClosestPort = "NLRTM";
		if (!customsRegNo.IsEmpty)
		{
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = customsRegNo;
		}

		return orgHeader;
	}

	public static OrgAddress CreateAddress(OrgAddressDependentCollection addresses, ZString code, ZString street, ZString city, ZString postCode, OrgAddressType addresType = null)
	{
		var address = addresses.AddNew();
		address.OA_Code = code;
		address.Address1 = street;
		address.OA_City = city;
		address.OA_PostCode = postCode;
		address.OA_RL_NKRelatedPortCode = "NLRTM";
		address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		if (addresType != null)
		{
			address.AddAddressType(addresType);
		}

		return address;
	}

	public static OrgAddress CreateAddress(OrgHeader party, ZString code, ZString street, ZString city, ZString postCode, OrgAddressType addresType = null)
	{
		party.MainAddress.OA_Code = code;
		party.MainAddress.Address1 = street;
		party.MainAddress.OA_City = city;
		party.MainAddress.OA_PostCode = postCode;
		party.MainAddress.OA_RL_NKRelatedPortCode = "NLRTM";
		party.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		if (addresType != null)
		{
			party.MainAddress.AddAddressType(addresType);
		}

		return party.MainAddress;
	}

	static JobDeclaration CreateDeclaration(BusinessObjectFactory factory, ZGuid sellerPK, ZGuid addressSellerPK, ZGuid exporterPK, ZGuid importerPK, ZGuid controllingAgentPK, ZGuid declarantPK, ZGuid controllingCustomerPK, ZGuid warehousePK, ZGuid buyerPK, ZGuid carrierPK, ZGuid representativeAddressPK)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.UnitedStates;
		declaration.JE_TotalWeight = 1.2;
		declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
		declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
		declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Netherlands;
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		declaration.JE_LocationQualifier = "A";
		declaration.JE_RL_NKFinalDestination = "NLDRE";
		declaration.JE_RL_NKOrigin = "USHOU";
		declaration.JE_PaymentMethod = "A";
		declaration.ZG_Box18TransportID = "12AB34";
		declaration.ZG_Box18TransportType = 10;
		declaration.JE_TransportMeans = "10";
		declaration.JE_SubLocationOfGoods = "Kerkstraat 1";
		declaration.JE_LocationOfGoods = Core.Constants.CountryCodes.Netherlands;
		declaration.JE_LocationOtherInformation = "1234AB";
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_DeclarationReference = "DECREF";
		declaration.JE_MasterBill = "08108051202";
		declaration.JE_HouseBill = "HOME0003";
		declaration.SellerOrgPK = sellerPK;
		declaration.JE_OA_SellerAddress = addressSellerPK;
		declaration.JE_OH_Exporter = exporterPK;
		declaration.JE_EntryStyle = EU.Business.EntryStyleListImport.Codes.ImportNormal;
		declaration.JE_CustomsOffice = "NL55566677";
		declaration.JE_OH_Importer = importerPK;
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
		declaration.JE_OH_ControllingAgent = controllingAgentPK;
		declaration.JE_OA_DeclarantAddress = declarantPK;
		declaration.JE_OH_ControllingCustomer = controllingCustomerPK;
		declaration.JE_GS_NKCusAgent = CreateCusAgent(factory).GS_Code;
		declaration.WarehouseDocAddress.OrganisationPK = warehousePK;
		declaration.JE_OH_Buyer = buyerPK;
		declaration.ConsigneeAddressOrgPK = buyerPK;
		declaration.CarrierEUBorderDocAddress.OrganisationPK = exporterPK;
		declaration.ImporterDocumentaryAddress.OrganisationPK = importerPK;
		//declaration.SupplierDocumentaryAddress.OrganisationPK = exporter.PK;
		declaration.ZG_PresentationStartDate = new ZDateTime(2023, 08, 03);
		declaration.ZG_TypeOfSecurity = "1";
		declaration.ZG_SpecificCircumstanceIndicator = "A";
		declaration.ExporterDocAddress.OrganisationPK = carrierPK;
		declaration.JE_OA_Representative = representativeAddressPK;

		return declaration;
	}

	public static GlbStaff CreateStaff(BusinessObjectFactory factory, string code, string fullname, string emailaddress = null)
	{
		var staff = factory.New<GlbStaff>();
		staff.GS_Code = code;
		staff.GS_FullName = fullname;
		staff.GS_EmailAddress = emailaddress;
		return staff;
	}

	public static void AddEmailaddressToStaff(GlbStaff staff, string emailaddress, string code)
	{
		var emailAddress = staff.EmailAddresses.AddNew();
		emailAddress.GSE_EmailAddress = emailaddress;
		emailAddress.GSE_Type = code;
	}

	static GlbStaff CreateCusAgent(BusinessObjectFactory factory)
	{
		var code = "CUS";
		var cusAgent = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, code)).FirstOrDefault() ?? CreateStaff(factory, "CUS", "CusAgent User", "cusagent@acme.com");
		return cusAgent;
	}

	static BasePackage CreateDeclarationPackage(BasePackageCollection packages, ZInt pack_qty, ZString pack_type, ZString marksAndNos)
	{
		var cw = packages.AddNew();
		cw.CW_PackQty = pack_qty;
		cw.CW_PackType = pack_type;
		cw.CW_MarksAndNos = marksAndNos;

		return cw;
	}

	static void CreateCustomsOffices(JobDeclaration declaration)
	{
		declaration.CustomsOffices.RemoveAndDeleteAll();

		// add other type of office for test
		var customsOffice1 = declaration.CustomsOffices.AddNew();
		customsOffice1.CY_Code = EuOfficeCodesTypes.Codes.EoriRegistrationAuthorities;
		customsOffice1.CY_Data = "NL99999999";

		// add supervising office
		var customsOffice2 = declaration.CustomsOffices.AddNew();
		customsOffice2.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
		customsOffice2.CY_Data = "NL12345678";

		var cusOffice = declaration.CustomsOffices.AddNew();
		cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
		cusOffice.CY_Data = "NL000001";
	}

	static void CreateInlandTransport(JobDeclaration declaration, string id, string registrationNationality)
	{
		declaration.JE_TransportIDInland = id;
		declaration.JE_RN_NKTransportNationalityInland = registrationNationality;
	}

	static void CreatePreviousDocument(EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection previousDocuments, ZShort lineNo, ZString referenceNumber, ZString procedure, ZString code)
	{
		var previousDocument = previousDocuments.AddNew();
		previousDocument.CSI_LineNo = lineNo;
		previousDocument.CSI_ReferenceNumber = referenceNumber;
		previousDocument.CSI_Procedure = procedure;
		previousDocument.CSI_Code = code;
	}

	static CusEntryInstruction CreateEntryInstruction(BusinessObjectFactory factory, ICusEntryInstructionCollection<CusEntryInstruction> entryInstructions, ZString description, ZString subStyle, ZString procedure, string style, ZGuid orgHeaderuthorizationPK)
	{
		var entryInstruction = entryInstructions.AddNew();
		entryInstruction.CEI_Description = description;
		entryInstruction.CEI_SubStyle = subStyle;
		entryInstruction.CEI_Procedure = procedure;
		entryInstruction.CEI_Style = style;
		entryInstruction.ZG_IsHighValueOvrd = true;

		CreateFiscalReference(entryInstruction.FiscalReferences, "FR1", "Fiscal Reference", "FIS");
		CreateFiscalReference(entryInstruction.FiscalReferences, "FR2", "Fiscal Reference 2", "FIS");
		CreateCusSupplyChainActorReference(entryInstruction.CusSupplyChainActorReferences, "FW", "SupplyChainActorReference");
		CreateCusSupplyChainActorReference(entryInstruction.CusSupplyChainActorReferences, "WH", "SupplyChainActorReference2");
		CreateCusAuthorizationUsage(entryInstruction.CusAuthorizationUsages, "OPO", "1234567", orgHeaderuthorizationPK);
		CreateCusAuthorizationUsage(entryInstruction.CusAuthorizationUsages, "AEOC", "7654321", orgHeaderuthorizationPK);

		return entryInstruction;
	}

	static void CreateCusAuthorizationUsage<T>(ICusAuthorizationUsageCollection<CusAuthorizationUsage, T> cusAuthorizationUsages, ZString code, ZString number, ZGuid orgHeaderAuthorizationPK)
		where T : BusinessObject, ICusAuthorizationUsageMaster, ILinkable
	{
		var cusAuthorizationUsage = cusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_Code = code;
		cusAuthorizationUsage.AGC_Number = number;
		cusAuthorizationUsage.AGC_OH_Owner = orgHeaderAuthorizationPK;
	}

	static EU.Business.Declaration.CusSupplyChainActorReference CreateCusSupplyChainActorReference(EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<EU.Business.Declaration.CusSupplyChainActorReference> cusSupplyChainActorReferences, ZString code, ZString reference)
	{
		var cusReference = cusSupplyChainActorReferences.AddNew();
		cusReference.CFR_Code = code;
		cusReference.CFR_Reference = reference;

		return cusReference;
	}

	static void CreateFiscalReference(EU.Business.Declaration.ICusFiscalReferenceCollection<EU.Business.Declaration.CusFiscalReference> fiscalReferences, ZString code, ZString reference, ZString type)
	{
		var fiscReference = fiscalReferences.AddNew();
		fiscReference.CFR_Code = code;
		fiscReference.CFR_Reference = reference;
		fiscReference.CFR_Type = type;
	}

	static JobComInvoiceHeader CreateInvoice(InvoiceHeaderActiveCollection invoices, ZGuid orgHeaderBuyerPK, ZGuid addressBuyerPK, ZGuid orgHeaderExporterPK, ZGuid addressExporterPK, ZGuid importerPK)
	{
		var invoice = invoices.AddNew();
		invoice.JZ_ValuationCode = "11";
		invoice.JZ_InvoiceAmount = 101.20;
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		invoice.JZ_UCR = "Trader reference";
		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
		invoice.JZ_IncoTermPlace = "NLRTM";
		invoice.ZG_AgreedPlaceCode = "NL";
		invoice.JZ_InvoiceNumber = "9478";
		invoice.BuyerOrgPK = orgHeaderBuyerPK;
		invoice.JZ_OA_BuyerAddress = addressBuyerPK;
		invoice.ExporterOrgPK = orgHeaderExporterPK;
		invoice.JZ_OA_ExporterAddress = addressExporterPK;
		invoice.JZ_OH_Buyer = importerPK;
		invoice.ZG_TransportChargesMethodOfPayment = "X";

		CreateInvoiceCharge(invoice.Charges, "OFT", 12.5, "EUR");
		CreateInvoiceCharge(invoice.Charges, "OFT", 13.0, "EUR");
		CreateTransportForInvoiceHeader(invoice);
		return invoice;
	}

	static void CreateTransportForInvoiceHeader(JobComInvoiceHeader header)
	{
		header.JobDeclaration.JE_JS = ZGuid.Empty;
		var transport1 = header.Transports.AddNew();
		transport1.JW_RL_NKDiscPortForBinding = "NL123";
		var transport2 = header.Transports.AddNew();
		transport2.JW_RL_NKDiscPortForBinding = "NL234";
		transport1.JW_ETA = ZDate.Today.AddDays(1);
		transport2.JW_ETA = ZDate.Today.AddDays(2);
	}

	static JobComInvoiceLine CreateInvoiceLine(BusinessObjectFactory factory, JobComInvoiceLineViewCollection invoiceLines, ZShort lineNo, ZGuid entryInstructionPK, ZGuid orgHeaderSellerPK, ZGuid addressSellerPK, ZDecimal weight, ZDecimal netWeight, ZDecimal customsSecondQuantity, ZDecimal linePrice, ZString tarrif, ZGuid orgHeaderAuthorizationPK, string transactionNature)
	{
		var invoiceLine = invoiceLines.AddNew();
		invoiceLine.JI_LineNo = lineNo;
		invoiceLine.JI_CEI = entryInstructionPK;
		invoiceLine.SellerOrgPK = orgHeaderSellerPK;
		invoiceLine.JI_OA_Seller = addressSellerPK;
		invoiceLine.JI_Weight = weight;
		invoiceLine.JI_NetWeight = netWeight;
		invoiceLine.JI_CustomsSecondQuantity = customsSecondQuantity;
		invoiceLine.JI_LinePrice = linePrice;
		invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
		invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
		invoiceLine.JI_ValuationCode = "1";
		invoiceLine.JI_CountryOfOrigin = "AB";
		invoiceLine.ZG_CountryOfSupply = "S1";
		invoiceLine.JI_ConcessionOrder = "ID";
		invoiceLine.JI_PrimaryPreference = "2PR";
		invoiceLine.JI_Procedure = "ADDPABC";
		invoiceLine.JI_Tariff = tarrif;
		invoiceLine.RelatedIndicator = false;
		invoiceLine.RelatedIndicator2 = false;
		invoiceLine.RelatedIndicator3 = false;
		invoiceLine.RelatedIndicator4 = false;
		invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Netherlands;
		invoiceLine.JI_OA_ConsigneeAddress = addressSellerPK;
		invoiceLine.JI_OA_ExporterAddress = addressSellerPK;
		invoiceLine.JI_SupplementaryCode1 = "1500";
		invoiceLine.JI_SupplementaryCode2 = "1501";
		invoiceLine.ZG_TransNature = transactionNature;

		CreateSupportingDocument(invoiceLine.SupportingDocuments, 1, "SUPREF11", "SUPREF21", ZDateTime.BrettsBirthday, "SPCD1", 10, "UNT1", "SUPREF21", "EUR", 15);
		CreateSupportingDocument(invoiceLine.SupportingDocuments, 2, "SUPREF21", "SUPREF22", ZDateTime.BrettsBirthday, "SPCD2", 25, "UNT2", "SUPREF22", "EUR", 35);

		CreateAddInfo(invoiceLine.AdditionalInfos, "TRA", "TRAREF1", "TRACOD1");
		CreateAddInfo(invoiceLine.AdditionalInfos, "TRA", "TRAREF2", "TRACOD2");
		CreateAddInfo(invoiceLine.AdditionalInfos, "REF", "REFREF1", "REFCOD1");
		CreateAddInfo(invoiceLine.AdditionalInfos, "REF", "REFREF2", "REFCOD2");
		CreateAddInfo(invoiceLine.AdditionalInfos, "INF", "", "INFCOD1", "INFDES1");
		CreateAddInfo(invoiceLine.AdditionalInfos, "INF", "", "INFCOD2", "INFDES2");

		CreateCusSupplyChainActorReference(invoiceLine.CusSupplyChainActorReferences, "FW", "REFJI1");
		CreateCusSupplyChainActorReference(invoiceLine.CusSupplyChainActorReferences, "WH", "REFJI2");

		CreatePreviousDocument(invoiceLine.PreviousDocuments, 1, "PRV-321", "IMA", "IMA1");
		CreatePreviousDocument(invoiceLine.PreviousDocuments, 2, "PRV-987", "IMZ", "IMZ1");

		CreateAdditionalProcedure(invoiceLine.AdditionalProcedureCodes, "APC1AAA");
		CreateAdditionalProcedure(invoiceLine.AdditionalProcedureCodes, "APC2AAB");

		CreateSupplementaryCode(invoiceLine.AdditionalSupplementaryCodes, "2500", Core.Constants.CountryCodes.Netherlands);
		CreateSupplementaryCode(invoiceLine.AdditionalSupplementaryCodes, "2501", Core.Constants.CountryCodes.EuropeanUnion);

		CreateInvoiceLineCharge(invoiceLine.Charges, "ABC", 12.5);
		CreateInvoiceLineCharge(invoiceLine.Charges, "CDE", 13.0);

		CreateUndg(factory, invoiceLine.UNDGs, "8232");
		CreateUndg(factory, invoiceLine.UNDGs, "2328");

		CreateCusAuthorizationUsage(invoiceLine.CusAuthorizationUsages, "AEOS", "3456789", orgHeaderAuthorizationPK);
		CreateCusAuthorizationUsage(invoiceLine.CusAuthorizationUsages, "AEOF", "9876543", orgHeaderAuthorizationPK);

		var pack = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		pack.IsLinked = true;
		pack.PackQty = 25;

		return invoiceLine;
	}

	public static void CreateUndg(BusinessObjectFactory factory, UNDGDataItemCollection undgs, string unno)
	{
		var undgSubstance = factory.NewWithValidTestData<UNDGSubstance>();
		undgSubstance.DG_UNNO = unno;
		var undg = undgs.AddNew();
		undg.DI_DG = undgSubstance.PK;
	}

	public static void CreateSupplementaryCode(SupplementaryCodeCollection additionalSupplementaryCodes, ZString code, ZString data)
	{
		var cusSupplementaryCode = additionalSupplementaryCodes.AddNew();
		cusSupplementaryCode.CY_Code = code;
		cusSupplementaryCode.CY_Data = data;
	}

	static void CreateAdditionalProcedure(AdditionalProcedureCodeCollection additionalProcedureCodes, ZString code)
	{
		var additionalProcedure = additionalProcedureCodes.AddNew();
		additionalProcedure.CY_Code = code;
	}

	static void CreateInvoiceCharge(IJobComInvChargeCollection<JobComInvCharge> charges, ZString chargeType, ZDecimal amount, ZString currency)
	{
		var invoiceCharge = charges.AddNew();
		invoiceCharge.J7_ChargeType = chargeType;
		invoiceCharge.J7_Amount = amount;
		invoiceCharge.J7_RX_NKCurrency = currency;
	}

	static void CreateInvoiceLineCharge(IJobComInvChargeCollection<JobComInvCharge> charges, ZString chargeType, ZDecimal amount)
	{
		var invoiceLineCharge = charges.AddNew();
		invoiceLineCharge.J7_ChargeType = chargeType;
		invoiceLineCharge.J7_Amount = amount;
	}

	static void CreateInvoiceLinePackage(InvoiceLinePackagePivotCollection packagesPivot, ZInt numberOfPacks, ZGuid cw1PK, ZGuid declarationPK)
	{
		var packInvoiceLine1 = packagesPivot.AddNew();
		packInvoiceLine1.CHC_CW = cw1PK;
		packInvoiceLine1.CHC_JE = declarationPK;
		packInvoiceLine1.CHC_NumberOfPacks = numberOfPacks;
	}

	public static AdditionalInfo CreateAddInfo(AdditionalInfoCollection additionalInfos, ZString type, ZString referenceNumber, ZString code)
	{
		var addInfo = additionalInfos.AddNew();
		addInfo.CSI_SubType = type;
		addInfo.CSI_ReferenceNumber = referenceNumber;
		addInfo.CSI_Code = code;
		return addInfo;
	}

	static AdditionalInfo CreateAddInfo(AdditionalInfoCollection additionalInfos, ZString type, ZString referenceNumber, ZString code, ZString description)
	{
		var addInfo = CreateAddInfo(additionalInfos, type, referenceNumber, code);
		addInfo.CSI_Description = description;
		return addInfo;
	}

	static void CreateContainer(ICusContainerCollection<EU.Business.Declaration.CusContainer> cusContainers, ZString containerNumber, ZString seal, ZString secondSeal, ZString thirdSeal)
	{
		var container = cusContainers.AddNew();
		container.CO_ContainerNumber = containerNumber;
		container.CO_Seal = seal;
		container.CO_SecondSeal = secondSeal;
		if (!thirdSeal.IsEmpty)
		{
			var seal3 = container.AdditionalSeals.AddNew();
			seal3.BK_SealNumber = thirdSeal;
		}
	}

	static SupportingDocument CreateSupportingDocument(SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString referenceNumber2, ZDateTime dateOfExpiry)
	{
		var supportingDoc = supportingDocuments.AddNew();
		supportingDoc.CSI_ItemNumber = itemNumber;
		supportingDoc.CSI_ReferenceNumber = referenceNumber;
		supportingDoc.CSI_ReferenceNumber2 = referenceNumber2;
		supportingDoc.CSI_AdditionalDescription = referenceNumber2;
		supportingDoc.CSI_DateOfExpiry = dateOfExpiry;
		return supportingDoc;
	}

	static SupportingDocument CreateSupportingDocument(SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString referenceNumber2, ZDateTime dateOfExpiry, ZString code)
	{
		var supportingDoc = CreateSupportingDocument(supportingDocuments, itemNumber, referenceNumber, referenceNumber2, dateOfExpiry);
		supportingDoc.CSI_Code = code;
		return supportingDoc;
	}

	static SupportingDocument CreateSupportingDocument(SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString referenceNumber2, ZDateTime dateOfExpiry, ZString code, ZDecimal quantity, ZString unitOfQuantity, ZString additionalDescription, ZString currency, ZDecimal value)
	{
		var supportingDoc = CreateSupportingDocument(supportingDocuments, itemNumber, referenceNumber, referenceNumber2, dateOfExpiry, code);
		supportingDoc.CSI_Quantity = quantity;
		supportingDoc.CSI_UnitOfQuantity = unitOfQuantity;
		supportingDoc.CSI_AdditionalDescription = additionalDescription;
		supportingDoc.CSI_RX_NKCurrency = currency;
		supportingDoc.CSI_Value = value;
		return supportingDoc;
	}

	static void CreateFee(ICusEntryLineFeeCollection<EU.Business.Declaration.CusEntryLineFee, EU.Business.Declaration.CusEntryLine> fees, ZString feeType, ZDecimal amount, ZString methodOfPayment, ZString methodOfCalculation, ZDecimal baseValue, ZDecimal rate)
	{
		var fee1 = fees.AddOrUpdate(feeType, amount);
		fee1.G4_MethodOfPayment = methodOfPayment;
		fee1.CF_MethodOfCalculation = methodOfCalculation;
		fee1.CF_BaseValue = baseValue;
		fee1.CF_Rate = rate;
	}

	public static void CreateAuthorizationHeader(OrgHeader orgHeader)
	{
		var authorisationHeader = orgHeader.Factory.New<Customs.Business.CusAuthorisationHeader>();
		authorisationHeader.CPH_OH_PermitHolder = orgHeader.PK;
		authorisationHeader.CPH_Number = "12345678901234567890123456789012345";
		authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
		authorisationHeader.CPH_StartDate = ZDate.Today;
	}

	public static void CreateContact(OrgHeader party, string contactName, string contactPhone, string contactMail)
	{
		var contact = party.Contacts.AddNew();
		contact.OC_ContactName = contactName;
		contact.OC_Phone = contactPhone;
		contact.OC_Email = contactMail;
	}

	public static EU.Business.Declaration.GuaranteeForDeclaration CreateGuarantee(JobDeclaration declaration, decimal amount, ZString currency, ZString guaranteeRef, ZString accesscode, ZString guaranteeOffice, ZString bondType, ZString guaranteeType, OrgHeader permitHolder)
	{
		var factory = declaration.Factory;
		var guarantee = factory.New<EU.Business.Declaration.GuaranteeForDeclaration>();
		guarantee.PW_Password = accesscode;
		guarantee.PW_CPH_Guarantee = CreateGuaranteeHeader(factory, 1000000, currency, "6789", "NL006789", permitHolder).PK;
		guarantee.PW_BondType = bondType;
		guarantee.PW_BondAmount = amount;
		guarantee.PW_RX_NKCurrency = currency;
		guarantee.PW_BondNumber = guaranteeRef;
		guarantee.PW_BondFiledPort = guaranteeOffice;
		guarantee.PW_BondNumber2 = guaranteeType;
		declaration.Guarantees.Add(guarantee);

		return guarantee;
	}

	static CusGuaranteeHeader CreateGuaranteeHeader(BusinessObjectFactory factory, decimal amount, ZString currency, ZString accesscode, ZString guaranteeOffice, OrgHeader permitHolder)
	{
		var guaranteeHeader = factory.New<CusGuaranteeHeader>();
		guaranteeHeader.AddTransaction("TRANS1", "CMT-TO-CONF", ZString.Empty, ZString.Empty, amount, 0, Customs.Business.PermitTransactionStatusList.Codes.Pending, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
		guaranteeHeader.CPH_UnitOfMeasure = currency;
		guaranteeHeader.MainAccessCode = accesscode;
		guaranteeHeader.CPH_StartDate = ZDate.Today;
		guaranteeHeader.CPH_OH_PermitHolder = permitHolder.PK;
		guaranteeHeader.CPH_Number = accesscode;
		guaranteeHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
		var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
		guaranteeRule.CPR_RuleCode = NLConstants.RuleCodes.Office;
		guaranteeRule.CPR_ValueFrom = guaranteeOffice;
		return guaranteeHeader;
	}

	public static NLEDIMessage CreateSentMessage(CusEntryHeader entryHeader, ZString messageType, ZString typeCode)
	{
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_MessageSubType = messageType;
		message.EM_Status = EDIMessageStatusList.Codes.Sent;
		message.EM_MessageText = CreateMessageText(messageType);
		return message;

		string CreateMessageText(ZString messageType)
		{
			var myNamespaces = new XmlSerializerNamespaces();
			myNamespaces.Add("xsi", @"http://www.w3.org/2001/XMLSchema-instance");

			if (messageType == ExportSendMessageTypes.Codes.DEC)
			{
				var metaData = new CargoWise.Customs.NL.MessageDefinitions.DMS.Declaration_1p30.MetaData();
				metaData.Declaration = new CargoWise.Customs.NL.MessageDefinitions.DMS.Declaration_1p30.MetaDataDeclaration()
				{
					TypeCode = new DeclarationTypeCodeType()
					{
						Value = typeCode
					}
				};
				return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeWithAdditionalNamespaces(metaData, myNamespaces);
			}
			else
			{
				var metaData = new CargoWise.Customs.NL.MessageDefinitions.DMS.AdditionalMessage_1p30.MetaData();
				metaData.Declaration = new CargoWise.Customs.NL.MessageDefinitions.DMS.AdditionalMessage_1p30.MetaDataDeclaration()
				{
					TypeCode = new DeclarationTypeCodeType()
					{
						Value = typeCode
					}
				};
				return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeWithAdditionalNamespaces(metaData, myNamespaces);
			}
		}
	}

	public static NLEDIMessage CreateReleasedMessage(CusEntryHeader entryHeader, ZString messageSubType, string statusNameCode)
	{
		var message = entryHeader.Factory.New<NLEDIMessage>();
		message.EM_MessageText = $"<MetaData xsi:schemaLocation=\"urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:wco:datamodel:WCO:DMS.Response:1\"><WCOTypeCode>CC429A</WCOTypeCode><Response><Status><NameCode>{statusNameCode}</NameCode></Status></Response><CommunicationMetaData> <ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData></MetaData>";
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_Status = EDIMessageStatusList.Codes.Received;
		message.EM_MessageSubType = messageSubType;
		entryHeader.Messages.Add(message);
		return message;
	}
}
