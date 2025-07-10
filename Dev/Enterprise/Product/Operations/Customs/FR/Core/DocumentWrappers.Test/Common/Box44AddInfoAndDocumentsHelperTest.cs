using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.DocumentWrappers.Common.Testing;

[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.France)]
sealed class Box44AddInfoAndDocumentsHelperTest : TestCaseWithFactory
{
	public void TestGetBox44AddInfoAndDocuments()
	{
		SetUpRefData();
		var entryLineForTest = GetEntryLineForTest(291.937, "005");

		CombineAssertions("Taxation specifiques should show when both Third Quantity and Third Quantity Unit have been captured.", () =>
		{
			var box44Helper = new Box44AddInfoAndDocumentsHelper();
			var result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest, false);
			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120
Taxations spécifiques : quantité : 291.94 - code mesurage : 005".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, result);

			entryLineForTest.CL_LineNumber = 2;
			expected = @"Mention(s) Spéciale(s): GEN13-PLENTY OF COATS; PAL01";
			result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest, false);
			AssertContains("entryLine.Box44Contents for non-first pages", expected, result);
		});

		var entryLineForTest2 = GetEntryLineForTest(0, "005");

		CombineAssertions("Taxation specifiques should not show when Third Quantity has not been captured.", () =>
		{
			var box44Helper = new Box44AddInfoAndDocumentsHelper();
			var result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest2, false);
			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, result);
		});

		var entryLineForTest3 = GetEntryLineForTest(291.94, "");

		CombineAssertions("Taxation specifiques should not show when Third Quantity has not been captured", () =>
		{
			var box44Helper = new Box44AddInfoAndDocumentsHelper();
			var result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest3, false);
			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, result);
		});

		var entryLineForTest4 = GetEntryLineForTest(291.913, "005");

		CombineAssertions("Taxation specifiques should not show when Third Quantity has not been captured", () =>
		{
			var box44Helper = new Box44AddInfoAndDocumentsHelper();
			var result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest4, false);
			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120
Taxations spécifiques : quantité : 291.91 - code mesurage : 005".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, result);
		});

		CusEntryLine GetEntryLineForTest(double thirdQuantity, string thirdQuantityUnit)
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "1071F61";
			invoiceLine.JI_CustomsThirdQuantity = thirdQuantity;
			invoiceLine.JI_CustomsThirdUnitQty = thirdQuantityUnit;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			declaration.JE_OH_Importer = importer.PK;

			var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;

			var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
			fiscalReferenceOrganisation.OH_Code = "FISCALREP";

			var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
			fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
			fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
			fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
			fiscalReferenceOrganisationAddress.OA_PostCode = "333";
			fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR3";
			fiscalReference.CFR_Reference = "FR33562024100133";
			fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;

			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var cusAuthorisationHeader = warehouseAddress.Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			cusAuthorisationHeader.CPH_OH_PermitHolder = ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;
			cusAuthorisationHeader.CPH_Number = "00001099";

			var suppDocHeader = invoiceHeader.SupportingDocuments.AddNew();
			suppDocHeader.CSI_Code = "HDR1";
			suppDocHeader.CSI_ReferenceNumber = "BILLY";
			suppDocHeader.CSI_SubType = "X";
			suppDocHeader.CSI_Quantity = 99;
			suppDocHeader.CSI_Description = "COZ WE WANT TO";

			var addInfoHeader = invoiceHeader.AdditionalInfos.AddNew();
			addInfoHeader.CSI_Code = "HIT99";
			addInfoHeader.CSI_Description = "RANDOM SONG TITLE";

			var suppDocGroup = declaration.SupportingDocuments.AddNew();
			suppDocGroup.CSI_Code = "HNY1";
			suppDocGroup.CSI_ReferenceNumber = "PIPER";
			suppDocGroup.CSI_SubType = "2";
			suppDocGroup.CSI_Quantity = 22;
			suppDocGroup.CSI_Description = "HONEY TO THE BEE";
			suppDocGroup.CSI_IsDTP = true;

			var addInfoGroup = declaration.AdditionalInfos.AddNew();
			addInfoGroup.CSI_Code = "BOB21";
			addInfoGroup.CSI_Description = "MADE SENSE AT THE TIME";

			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "GEN13";
			addInfo1.CSI_Description = "PLENTY OF COATS";

			var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "PAL01";

			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V911";

			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V910";

			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);

			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			document2.CSI_IsDTP = true;

			return entryLine;
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HDR1", "COZ WE WANT TO", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HNY1", "HONEY TO THE BEE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { addInfCodeType }, "HIT99", "RANDOM SONG TITLE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9100", "I HAVE NO CLUE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9120", "9120 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();
		}
	}

	public void TestGetBox44AddInfoAndDocuments_ShouldHaveEconomicRegime()
	{
		SetUpRefData();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var authHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
		var entryLineForTest = GetEntryLineForTest();
		var box44Helper = new Box44AddInfoAndDocumentsHelper();
		var result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest, true);
		CombineAssertions(() =>
		{
			var expectedIMP = @"
Régime Economique :
° Autorisation Économique (entrée) : TST_ATH_001	° Pays d’Autorisation : GB
° Montant garanti : 19.90
° Délai d’apurement : 9
Nature du perfectionnement, de la transformation ou de l’utilisation des marchandises : N1
Description technique des marchandises et des produits compensateurs ou transformés et les moyens de les identifier : CN Code 123456
Codes relatifs aux conditions économiques conformément à l’annexe 70 : C1
Bureau d’apurement : O1
Lieu de perfectionnement, de transformation ou d’utilisation : L1
Formalités de transfert proposées : T1 Description".TrimStart(new[] { '\r', '\n' });
			AssertEquals("IMP Box44AddInfoAndDocuments", expectedIMP, result);

			declaration.JE_MessageType = "EXP";
			result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest, true);
			var expectedEXP = @"
Régime Economique :
° Autorisation Économique (sortie) : TST_ATH_001	° Pays d’Autorisation : GB".TrimStart(new[] { '\r', '\n' });
			AssertEquals("IMP Box44AddInfoAndDocuments", expectedEXP, result);

			declaration.JE_MessageType = "IMP";
			var rules = authHeader.CusAuthorisationRules.Where(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.STO);
			rules.DeleteAll();
			result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest, true);
			expectedIMP = @"
Régime Economique :
° Autorisation Économique (entrée) : TST_ATH_001	° Pays d’Autorisation : GB
° Montant garanti : 19.90
Nature du perfectionnement, de la transformation ou de l’utilisation des marchandises : N1
Description technique des marchandises et des produits compensateurs ou transformés et les moyens de les identifier : CN Code 123456
Codes relatifs aux conditions économiques conformément à l’annexe 70 : C1
Bureau d’apurement : O1
Lieu de perfectionnement, de transformation ou d’utilisation : L1
Formalités de transfert proposées : T1 Description".TrimStart(new[] { '\r', '\n' });
			AssertEquals("IMP Box44AddInfoAndDocuments", expectedIMP, result);
		});

		result = box44Helper.GetBox44AddInfoAndDocuments(entryLineForTest, false);
		AssertEquals(ZString.Empty, result);

		CusEntryLine GetEntryLineForTest()
		{
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";

			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "1234";
			authHeader.CPH_PermitDescription = "CN Code 123456";
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_IsActive = true;

			var autRule = authHeader.CusAuthorisationRules.AddNew();
			autRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
			autRule.CPR_ValueFrom = "TST_ATH_001";

			var usage = entryInstruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "1234";
			usage.AGC_OH_Owner = customer.PK;

			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.NAT, "N1");
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.CON, "C1");
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.OFC, "O1").CPR_Description = "Square";
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, Enterprise.Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "L1").CPR_Description = "Hidden";
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.TRA, "T1").CPR_Description = "T1 Description";
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.INF, "I1");
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "9");
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
			DocSADHLineTest.CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
			entryLine.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 12); // Duty: 12.46m
			entryLine.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29); // VAT: 29.12m
			entryLine.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";  // ParaFiscal: 43.34m

			return entryLine;
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Description, euGrouping);
			Factory.Save();
			var euDtyRateType = helper.CreateCusRateType(euGrouping.ZZZ_DataGrouping, Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.AUTDC, MapDirectionList.Codes.BTH, "Authorisation document code", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "OPO", "C019", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "IPO", "C601", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "TEA", "C516", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CWP", "C517", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW1", "C518", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "CW2", "C519", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			helper.CreateCusMap(RefCusMapTypeList.Codes.AUTDC, "EUS", "N990", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.France);
			Factory.Save();
		}
	}
}
