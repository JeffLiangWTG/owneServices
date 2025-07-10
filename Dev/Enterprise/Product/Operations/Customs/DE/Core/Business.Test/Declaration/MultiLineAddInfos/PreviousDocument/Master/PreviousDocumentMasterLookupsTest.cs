using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class PreviousDocumentMasterLookupsTest : TestCaseWithFactory
	{
		public void TestTypeList_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var actualCodes = lookups.ProcedureList.GetAllCodes();
			var expectedCodes = new[]
			{
				"199",
				"200",
				"444T1",
				"444T2",
				"444TF",
				"447T1",
				"447T2",
				"447TF",
				"A",
				"AE",
				"AT-AV",
				"AT-ZL",
				"ATA",
				"ATNEU",
				"AV",
				"ENST2L",
				"ESUMA",
				"FREIZ",
				"FV",
				"GB",
				"MAN",
				"MO",
				"OESUMA",
				"OHNE",
				"POST",
				"POUS",
				"PUEB",
				"T-",
				"T1",
				"T1CF",
				"T1DF",
				"T1IC",
				"T1IE",
				"T1IF",
				"T2",
				"T2AN",
				"T2CF",
				"T2DF",
				"T2F",
				"T2IC",
				"T2IE",
				"T2IF",
				"T2L",
				"T2LF",
				"T2M",
				"T2SM",
				"T5",
				"TIR",
				"TRPPVW",
				"V",
				"VER321",
				"VO",
				"VV",
				"Z",
				"ZL",
			};
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		public void TestTypeList_Export()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("AT-AV, AT-ZL", lookups.ProcedureList.CodesAsString);
		}

		public void TestCustomsOfficeList()
		{
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_OA_SellerAddress = subcontractorAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_SupplierAddress = supplierAddress.PK;

			var dateInFuture = ZDate.Today.AddDays(4);
			var dateInPast = ZDate.Today.AddDays(-4);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BankCode");

			var zzd_Perfect = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1", "Valid", dateInPast, dateInFuture);
			var zzd_InPast = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2", "Invalid", dateInPast, dateInPast.AddDays(2));
			var zzd_InFuture = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3", "Invalid", dateInFuture, dateInFuture.AddDays(2));
			var zzd_WrongType = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "4", "Invalid", dateInFuture, dateInFuture.AddDays(2));
			var zzd_NoAttribute = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5", "Invalid", dateInPast, dateInFuture);
			var zzd_FalseValue = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "6", "Invalid", dateInPast, dateInFuture);
			var zzd_InEu = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "7", "Invalid", dateInPast, dateInFuture);
			var zzd_AlsoPerfect = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8", "Valid", dateInPast, dateInFuture);

			var zze_Perfect = helper.CreateCusCodeListAttribute(zzd_Perfect.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			var zze_InPast = helper.CreateCusCodeListAttribute(zzd_InPast.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			var zze_InFuture = helper.CreateCusCodeListAttribute(zzd_InFuture.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			var zze_WrongType = helper.CreateCusCodeListAttribute(zzd_WrongType.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			var zze_FalseValue = helper.CreateCusCodeListAttribute(zzd_FalseValue.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "False");
			var zze_InEu = helper.CreateCusCodeListAttribute(zzd_InEu.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");
			var zze_AlsoPerfect = helper.CreateCusCodeListAttribute(zzd_AlsoPerfect.PK, RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "True");

			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			var customsOfficeList = lookups.CustomsOfficeList;
			customsOfficeList.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "1", "8" }, customsOfficeList.Select(x => x.ZZD_Code));
			AssertSame("Should be cached", customsOfficeList, master.Lookups.CustomsOfficeList);
		}

		public void TestAuthorizationNumberList_ParentProviderIsEntryInstruction()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var fromWarehouse = Factory.New<OrgHeader>();
			fromWarehouse.OH_Code = "EDIBRNMAZ";
			var fromWarehouseAddress = fromWarehouse.MainAddress;

			var sellerOrg = Factory.New<OrgHeader>();
			sellerOrg.OH_Code = "EDISELLER";
			var sellerAddress = sellerOrg.MainAddress;

			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER2");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER3");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER4");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER5");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER6");
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER7");
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER8");
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER9");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER10");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER11");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER12");
			sellerAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER13");
			sellerAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER14");
			sellerAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER15");

			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			declaration.JE_OA_SellerAddress = sellerAddress.PK;
			entryInstruction.CEI_OA_Warehouse = fromWarehouseAddress.PK;

			AssertEquals("NUMBER1, NUMBER2, NUMBER3, NUMBER4, NUMBER5, NUMBER6, NUMBER7, NUMBER8, NUMBER9", lookups.AuthorizationNumberList.CodesAsString);
		}

		public void TestAuthorizationNumberList_ParentProviderIsNotEntryInstruction()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			master = invoiceLine.PreviousDocumentMaster;
			lookups = new PreviousDocumentMasterLookups(master);

			var fromWarehouse = Factory.New<OrgHeader>();
			fromWarehouse.OH_Code = "EDIBRNMAZ";
			var fromWarehouseAddress = fromWarehouse.MainAddress;

			var sellerOrg = Factory.New<OrgHeader>();
			sellerOrg.OH_Code = "EDISELLER";
			var sellerAddress = sellerOrg.MainAddress;

			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER2");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER3");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER4");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER5");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER6");
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER7");
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER8");
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER9");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER10");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER11");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER12");
			sellerAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER13");
			sellerAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER14");
			sellerAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER15");

			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			declaration.JE_OA_SellerAddress = sellerAddress.PK;
			entryInstruction.CEI_OA_Warehouse = fromWarehouseAddress.PK;

			AssertEquals("NUMBER1, NUMBER10, NUMBER11, NUMBER12, NUMBER13, NUMBER14, NUMBER15, NUMBER2, NUMBER3, NUMBER4, NUMBER5, NUMBER6", lookups.AuthorizationNumberList.CodesAsString);
		}

		public void TestAuthorizationNumberList_Cached()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var authorizationNumberList = lookups.AuthorizationNumberList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", ZString.Empty, authorizationNumberList.CodesAsString);
				AssertSame("Cached", authorizationNumberList, lookups.AuthorizationNumberList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";

			representative = Factory.New<OrgHeader>();
			representative.OH_Code = "EDIBRNFRA";

			subcontractor = Factory.New<OrgHeader>();
			subcontractor.OH_Code = "EDIBRNFXX";

			supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "EDIBRNFYY";

			declaration = Factory.New<JobDeclaration>();

			declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.OA_Address1 = "Address 1";

			representativeAddress = representative.Addresses.AddNew();
			representativeAddress.OA_Address1 = "Address 1";

			subcontractorAddress = subcontractor.Addresses.AddNew();
			subcontractorAddress.OA_Address1 = "Address 1";

			supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_Address1 = "Address 1";

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			master = entryInstruction.PreviousDocumentMaster;
			lookups = new PreviousDocumentMasterLookups(master);
		}

		JobDeclaration declaration;
		OrgHeader declarant;
		OrgAddress declarantAddress;
		OrgHeader representative;
		OrgAddress representativeAddress;
		OrgHeader subcontractor;
		OrgAddress subcontractorAddress;
		OrgHeader supplier;
		OrgAddress supplierAddress;
		CusEntryInstruction entryInstruction;
		PreviousDocumentMaster master;
		PreviousDocumentMasterLookups lookups;
	}
}
