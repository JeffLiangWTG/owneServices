using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.Universal.Constants;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class DeclarationTestHelper : MasterFilesTestHelper
	{
		public DeclarationTestHelper()
		{
		}

		public static void RunAssertionsInPhase5TransitionPeriod(VoidParameterlessDelegate assertions, bool combineAssertions = true)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				if (combineAssertions)
				{
					CombineAssertions(assertions);
				}
				else
				{
					assertions.Invoke();
				}
			}
		}

		public static void RunAssertionsOutsidePhase5TransitionPeriod(VoidParameterlessDelegate assertions, bool combineAssertions = true)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				if (combineAssertions)
				{
					CombineAssertions(assertions);
				}
				else
				{
					assertions.Invoke();
				}
			}
		}

		public static IGlbBranch CreateGbCompanyAndBranchAndSave(BusinessObjectFactory factory, string code = "DUK")
		{
			var orgProxy = factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = code;
			var danielCompany = factory.NewWithValidTestData<GlbCompany>();
			danielCompany.GC_RN_NKCountryCode = "GB";
			danielCompany.GC_Code = code;
			danielCompany.GC_OH_OrgProxy = orgProxy.PK;
			var branch = danielCompany.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_RL_NKHomePort = "GBLHR";
			branch.GB_BranchName = "Daniel";  // For StmPrintJobs to have a prefix
			branch.GB_OH_OrgProxy = orgProxy.PK;
			factory.Save();
			return branch;
		}

		public static void CreateAgentAndShedBadgesAndCreds()
		{
			var badgeFelixstowe = new BadgeCodeSetting();
			badgeFelixstowe.BadgeCode = "FEY";
			badgeFelixstowe.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;
			var lxaAgentBadge = new BadgeCodeSetting();
			lxaAgentBadge.BadgeCode = "LXA";
			lxaAgentBadge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			var caxShed = new BadgeCodeSetting();
			caxShed.BadgeCode = "CAX";
			caxShed.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			var badges = new BadgeCodeSettingCollection();
			badges.Add(lxaAgentBadge);
			badges.Add(caxShed);
			badges.Add(badgeFelixstowe);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credentials = new CredentialsSettingCollection();
			var lxaCredential = new CredentialsSetting();
			lxaCredential.BadgeCode = lxaAgentBadge.BadgeCode;
			lxaCredential.Company = lxaAgentBadge.BadgeCode;
			lxaCredential.Printer = "CUKFFW98000LXA";
			var caxCredential = new CredentialsSetting();
			caxCredential.BadgeCode = caxShed.BadgeCode;
			caxCredential.Printer = "CUKAIR98LHRCAX";
			caxCredential.Company = caxShed.BadgeCode;
			var feyCredential = new CredentialsSetting();
			feyCredential.BadgeCode = badgeFelixstowe.BadgeCode;
			feyCredential.Printer = "XXX";
			feyCredential.Username = "X";
			feyCredential.Password = "X";
			feyCredential.Company = badgeFelixstowe.BadgeCode;
			credentials.Add(caxCredential);
			credentials.Add(lxaCredential);
			credentials.Add(feyCredential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
		}
		public DeclarationTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		BusinessObjectFactory factory;

		public OrgHeader MakeOrganisation1()
		{
			return GetNewOrganisation("", "AGI", "Agnes", "Main St", "", "Kisvarda", "NA", "1234", "HUBUD", "12345", "654987", "987654321000");
		}
		public OrgHeader MakeOrganisation2()
		{
			return GetNewOrganisation("", "DAN", "Daniel", "High St", "New Bradwell", "Milton Keynes", "Bucks", "MK1", "GBMIK", "12345", "654987", "696969699000");
		}

		public string GetStringOfMaxSizePlusOneToTrim(int maxSize) => new string('A', maxSize) + "Z";

		public CusEntryHeader CreateBasicEntry(string declarationType, string messageType, bool addFiscalReferences = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JobComInvoiceLines.AddNew();
				declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
				var instruction1 = Factory.New<Declaration.CusEntryInstruction>();
				instruction1.CEI_JE = declaration.PK;
				if (!declaration.CustomsEntryInstructions.Any())
				{
					declaration.CustomsEntryInstructions.Add(instruction1);
				}
				var invoice1 = declaration.Invoices[0];
				invoice1.JZ_RX_NKInvoice_Currency = "GBP";
				invoice1.JZ_InvoiceAmount = 1234.56d;
				var invoiceLine1 = invoice1.InvoiceLines[0];
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction1.PK;
				var entryLine1 = entryHeader.AllEntryLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				declaration.JE_MessageType = messageType;
				declaration.JE_DeclarationType = declarationType;

				if (addFiscalReferences)
				{
					var frL1 = declaration.InvoiceLines[0].FiscalReferences.AddNew();
					frL1.CFR_Code = "FR1";
					frL1.CFR_Reference = "GB11111111";

					var frL2 = entryHeader.Declaration.InvoiceLines[0].FiscalReferences.AddNew();
					frL2.CFR_Code = "FR2";
					frL2.CFR_Reference = "GB22222222";
				}

				entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
				entryHeader.EntryInstruction.CEI_Style = declarationType;
				Factory.Save();
				return entryHeader;
			}
		}

		public JobDeclaration CreateImportAirDeclaration(bool isImportWithTwoInvoices = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = EntryStyleListImport.Codes.ImportNormal;
			cei.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;
			declaration.JE_RS_NKServiceLevel = "STD";
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_DateAtFinalDestination = ZDateTime.Today;
			declaration.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;

			declaration.JE_OH_Supplier = OSParty.PK;
			OSParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789123");

			declaration.JE_OH_Importer = LocalParty.PK;
			LocalParty.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, declaration.Branch.Country, "346413367");
			LocalParty.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, declaration.Branch.Country, "493948289000");

			declaration.JE_RL_NKOrigin = "AUSYD";

			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;

			SetupDeclaration(declaration);
			var invoiceHeader = SetupInvoiceHeader(declaration);
			SetupInvoiceLine1(invoiceHeader);
			if (isImportWithTwoInvoices)
			{
				SetupInvoiceLine2(invoiceHeader);
			}

			return declaration;
		}

		public JobDeclaration CreateExportAirDeclaration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateCountryMappings(Core.Constants.CountryCodes.UnitedKingdom, helper);
			helper.CreatePreferenceForCountryAndGrouping("100", "Normal Third Country Tariff Duty (Including Ceilings)", "GB", "EUN");
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "C601", "Test C601", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_DeclarationType = "EFD";
				declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
				declaration.JE_EntrySubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD;

				declaration.JE_OH_Supplier = LocalParty.PK;

				LocalParty.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, declaration.Branch.Country, "346413367");

				declaration.JE_OH_Importer = OSParty.PK;
				OSParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789123");

				declaration.JE_RL_NKOrigin = "GBLHR";

				SetupDeclaration(declaration);
				var invoiceHeader = SetupInvoiceHeader(declaration);
				SetupInvoiceLine1(invoiceHeader);
				SetupInvoiceLine2(invoiceHeader);
				return declaration;
			}
		}

		void SetupDeclaration(JobDeclaration declaration)
		{
			declaration.JE_UCR = "8GB123456789000-B00001216"; // TODO: refactor to use a special DUCR Field. (possibly not needed?)
			declaration.SubLocation = "ERT";
			declaration.JE_LocationOfGoods = "LHR";
			declaration.JE_MergeBy = "NON";
			declaration.ZG_ManualCalc = true;
			declaration.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
			declaration.JE_OA_DeclarantAddress = Declarant.MainAddress.PK;

			declaration.JE_OH_ShippingLine = ShippingLine.PK;
			declaration.Branch.GB_OH_OrgProxy = Declarant.PK;

			declaration.JE_RL_NKFinalDestination = "GBLHR";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_DateAtFinalDestination = ZDateTime.Today;

			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalWeight = 100m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_GoodsDescription = "3 HOLE";
			declaration.JE_MasterBill = "08108051202";
			declaration.JE_HouseBill = "HOME0003";
			declaration.JE_VoyageFlightNo = "QF253";
			declaration.JE_RN_NKTransportNationality = "AU";

			var cw1 = declaration.Bills[1].PackingGroups[0].Packages.AddNew();
			cw1.CW_PackQty = 10;
			cw1.CW_PackType = "BX";
			cw1.CW_MarksAndNos = "AS ABOVE";

			var cw2 = declaration.Bills[1].PackingGroups[0].Packages.AddNew();
			cw2.CW_PackQty = 1;
			cw2.CW_PackType = "PK";
			cw2.CW_MarksAndNos = "AWB 176 50455495";
		}

		public void SetupInvoiceLine2(JobComInvoiceHeader invoiceHeader)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_OA_SupervisingOffice = SupervisingOffice.MainAddress.PK;
				invoiceLine2.JI_Tariff = "9401901090";
				invoiceLine2.JI_Procedure = "5100000";
				invoiceLine2.JI_CustomsUnitQty = "KGM";
				invoiceLine2.JI_Description = "Aircraft Seat Parts";
				invoiceLine2.JI_NetWeight = 3m;
				invoiceLine2.JI_NetWeightUQ = "KG";
				invoiceLine2.JI_LinePrice = 2600m;
				invoiceLine2.JI_CountryOfOrigin = "AE";
				invoiceLine2.ZG_ValueAdjustmentCode = "B";
				invoiceLine2.JI_PrimaryPreference = "102";
				var document2 = invoiceLine2.SupportingDocuments.AddNew();
				document2.CSI_Code = "C601";
				document2.CSI_ReferenceNumber = "IP/0903/190/09";
				document2.CSI_Actions = "";
				document2.CSI_Availability = "";
				document2.CSI_Status = "JE";

				var pack2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[2];
				pack2.IsLinked = true;
				pack2.PackQty = 1;

				EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument2 = invoiceLine2.PreviousDocuments.AddNew();
				previousDocument2.CSI_Code = "380";
				previousDocument2.CSI_ReferenceNumber = "F86031";
				previousDocument2.CSI_SubType = "Z";

				var tax2 = invoiceLine2.Taxes.AddNew().Data;
				tax2.G4_Type = "A00";
				tax2.G4_RateOverride = "PDY";
				tax2.G4_RateDuty = "F";
			}
		}

		void SetupInvoiceLine1(JobComInvoiceHeader invoiceHeader)
		{
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "4902100000";
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_Procedure = "4000000";
			invoiceLine1.JI_Description = "PARTS OF RAT";
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.ZG_StatisticalValueManualOverride = true;
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine1.ZG_ValueAdjustmentCode = ValuationAdjustmentCodeList.Codes.CifInvoicePriceValueOrSimplifiedProcedureValueSpv;
			invoiceLine1.JI_PrimaryPreference = "100";
			var pack1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1];
			pack1.IsLinked = true;
			pack1.PackQty = 10;

			EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "703";
			previousDocument1.CSI_SubType = "Z";
			previousDocument1.CSI_ReferenceNumber = "122123";

			var tax1 = invoiceLine1.Taxes.AddNew().Data;
			tax1.G4_Type = "B00";
			tax1.G4_RateDuty = "Z";
		}

		JobComInvoiceHeader SetupInvoiceHeader(JobDeclaration declaration)
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "MYINVOICE";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "GBP";
			return invoiceHeader;
		}

		public override OrgHeader CreateShippingLine()
		{
			return GetNewOrganisation("", "SHPMRSHPLN", "MR Shipping Line", "Shipping Line Address 1", "Shipping Line Address 2", "Sydney", "STATE", "POSTCODE", "AUSYD", "", "", "");
		}

		public override OrgHeader CreateConsignor()
		{
			return GetNewOrganisation("", "OSPEMIAIRL", "Emirates Airlines", "Emirates Engineering Building", "Airport Road", "Sydney", "STATE", "POSTCODE", "AUSYD", "971 4 2245678", "971 4 2244144", "");
		}

		public override OrgHeader CreateConsignee()
		{
			return GetNewOrganisation("BRI012", "LOCPREAIRI", "Premium Aircraft Interiors UK Ltd", "Heath Tecna", "Watchmoor Point", "Camberley", "Surrey", "GU15 3AQ", "GBLHR", "01276 707777", "", "346413367");
		}

		public OrgHeader Declarant
		{
			get { return declarant ?? (declarant = GetNewOrganisation("", "DECSTRSHPC", "Strategic Shipping Co Ltd", "Unit 6 McKay Trading Estate", "Blackthorne Road", "Colnbrook", "GBCBR", "SL3 0AH", "GBLHR", "", "", "584361816")); }
		}
		OrgHeader declarant;

		public OrgHeader SupervisingOffice
		{
			get { return supervisingOffice ?? (supervisingOffice = GetNewOrganisation("", "SPOCUSTOMS", "H.M. Revenue & Customs", "67 Station Road", "", "Redhill  Surrey", "GBREH", "RH1 1QU", "GB", "", "", "")); }
		}
		OrgHeader supervisingOffice;

		public OrgHeader LocalParty
		{
			get { return Consignee; }
		}

		public OrgHeader OSParty
		{
			get { return Consignor; }
		}

		public OrgHeader GetNewOrganisation(ZString gemsCode, ZString shortCode, ZString name, ZString address1, ZString address2, ZString town, ZString state, ZString postCode, ZString portCode, ZString telephone, ZString fax, ZString turnCode, string countryCode = Core.Constants.CountryCodes.UnitedKingdom)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.OH_RL_NKClosestPort = portCode;
			if (!shortCode.IsEmpty)
			{
				result.OH_Code = shortCode;
			}
			result.MainAddress.OA_Address1 = address1;
			result.MainAddress.OA_Address2 = address2;
			result.MainAddress.OA_City = town;
			result.MainAddress.OA_State = state;
			result.MainAddress.OA_PostCode = postCode;
			result.MainAddress.OA_Phone = telephone;
			result.MainAddress.OA_Fax = fax;

			if (!turnCode.IsEmpty)
			{
				result.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, turnCode, countryCode);
			}
			if (!gemsCode.IsEmpty)
			{
				result.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.UnitedKingdomCodeTypes.GemsCustomerCode, gemsCode, countryCode);
			}
			return result;
		}

		public void CreateRefDataForPermits(string grouping)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(grouping, "Test grouping", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document");
			var doc9001I = helper.CreateCusCodeList(grouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var doc9001E = helper.CreateCusCodeList(grouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PERMIT", "PERMIT", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, grouping);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PERMIT", "PERMIT", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, grouping);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, grouping);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, grouping);
			doc9001I.Attributes.AddNew("PERMIT", "PERMIT");
			doc9001E.Attributes.AddNew("PERMIT", "PERMIT");
			doc9001I.Attributes.AddNew("Level", "ITEM");
			doc9001E.Attributes.AddNew("Level", "ITEM");
			Factory.Save();
		}

		public BaseCusPermitHeader SetupPermits(OrgHeader org, ZString permitIndicator, bool useEndDate = true, string permitNumber = "12345")
		{
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			permitHeader.CPH_OH_PermitHolder = org.PK;
			permitHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			permitHeader.CPH_Number = permitNumber;
			permitHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			permitHeader.CPH_EndDate = useEndDate ? ZDate.Today.AddYears(20) : ZDate.Empty;
			permitHeader.CPH_QtyValIndicator = permitIndicator;
			permitHeader.CPH_UnitOfMeasure = "KGM";
			permitHeader.CPH_Type = "9001";
			permitHeader.CPH_ApplicationCode = "PER";
			Factory.Save();

			var openingBalanceTransaction = permitHeader.CusPermitLineTransactions.AddNew();

			if (permitIndicator == PermitQtyValIndicatorList.Codes.BTH || permitIndicator == PermitQtyValIndicatorList.Codes.QTY)
			{
				openingBalanceTransaction.CPL_TranQty = 1000;
			}

			if (permitIndicator == PermitQtyValIndicatorList.Codes.BTH || permitIndicator == PermitQtyValIndicatorList.Codes.VAL)
			{
				openingBalanceTransaction.CPL_TranValue = 10000;
			}
			openingBalanceTransaction.CPL_Reference = "Opening Balance";
			openingBalanceTransaction.CPL_TransactionDate = ZDateTime.Today;
			openingBalanceTransaction.CPL_TransactionType = "OBL";
			openingBalanceTransaction.CPL_TransactionCategory = "VAL";
			openingBalanceTransaction.CPL_TransactionStatus = "CON";

			Factory.Save();

			return permitHeader;
		}

		public void SetupTransaction(BaseCusPermitHeader permitHeader, ZString reference, ZString messageNum)
		{
			var permitLineTransaction = permitHeader.CusPermitLineTransactions.AddNew();
			permitLineTransaction.CPL_TranQty = 0;
			permitLineTransaction.CPL_TranValue = 100;
			permitLineTransaction.CPL_Reference = reference;
			permitLineTransaction.CPL_TransactionDate = ZDateTime.Today;
			permitLineTransaction.CPL_TransactionType = "TRA";
			permitLineTransaction.CPL_TransactionCategory = "CUM";
			permitLineTransaction.CPL_AppId = messageNum;
			permitLineTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;

			Factory.Save();
		}

		public void CreateCountryMappings(ZString dataGroupingCode, UniversalReferenceTestDataHelper helper = null)
		{
			if (helper == null)
			{
				helper = new UniversalReferenceTestDataHelper(Factory);
			}
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", isReadonly: false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, dataGroupingCode);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.Monaco, Core.Constants.CountryCodes.France,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, dataGroupingCode);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.SvalbardAndJanMayen, Core.Constants.CountryCodes.Norway,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, dataGroupingCode);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return factory ?? (factory = base.NewFactory());
		}
	}
}
