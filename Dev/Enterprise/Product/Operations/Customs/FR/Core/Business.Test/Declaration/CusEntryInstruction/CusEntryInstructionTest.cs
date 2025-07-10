using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	partial class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestHasSimplifiedAuthorisationAdditionalInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			Assert(!entryInstruction.HasSimplifiedAuthorisationAdditionalInfo);

			var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100;
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			Assert(entryInstruction.HasSimplifiedAuthorisationAdditionalInfo);

			additionalInfo.CSI_Code = ZString.Empty;
			Assert(!entryInstruction.HasSimplifiedAuthorisationAdditionalInfo);

			additionalInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100;
			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			Assert(!entryInstruction.HasSimplifiedAuthorisationAdditionalInfo);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)instruction;

			AssertEquals("AdditionalInfo type should be the FR one.", typeof(AdditionalInfo), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("SupportingDocument type should be the FR one.", typeof(SupportingDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			AssertEquals("PreviousDocument type should be the FR one.", typeof(PreviousDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		public void TestAdditionalInfosValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>("IsUCC6 IMP", new InstructionConfiguration().GetAdditionalInfoValidationDecider(entryInstruction));
			}
		}

		public void TestCEI_DateForDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = "11";

			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 3000m, declaration.LocalCurrencyCode);
			charge.J7_IsIncludedInITOT = false;

			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 4000m;
			line1.JI_CEI = entryInstruction.PK;
			line1.JI_Procedure = line1.EntryInstruction.CEI_Style + "00";

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "JPY";
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 2000m;
			line2.JI_CEI = entryInstruction.PK;

			SetExchangeRate(Factory, "USD", 4m, new ZDateTime(2023, 1, 21));
			SetExchangeRate(Factory, "JPY", 3m, new ZDateTime(2023, 1, 21));
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 1, 21);
			AssertEquals("According to the current exchange rate between USD and JPY, GroupCharge of invoice1 and invoice2 should be apportioned in a ratio of (4000/4):(2000/3)", 1800m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("According to the current exchange rate between USD and JPY, GroupCharge of invoice1 and invoice2 should be apportioned in a ratio of (4000/4):(2000/3)", 1200m, invoice2.GroupCharges[0].J7_Amount);

			SetExchangeRate(Factory, "USD", 6m, new ZDateTime(2023, 2, 21));
			SetExchangeRate(Factory, "JPY", 2m, new ZDateTime(2023, 2, 21));
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 2, 21);
			AssertEquals("Exchange rate has been updated after resetting the date of duty, GroupCharge of invoice1 and invoice2 should be apportioned in a ratio of (4000/6):(2000/2)", 1200m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Exchange rate has been updated after resetting the date of duty, GroupCharge of invoice1 and invoice2 should be apportioned in a ratio of (4000/6):(2000/2)", 1800m, invoice2.GroupCharges[0].J7_Amount);
		}

		void SetExchangeRate(BusinessObjectFactory factory, string currencyCode, decimal rate, ZDateTime date)
		{
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			var exchangeRate = usdCurrency.ExchangeRates.FirstOrDefault(x => x.RE_RX_NKExCurrency == usdCurrency.Code && x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate && x.RE_GC == GlbCompany.CurrentCompany.PK && x.RE_StartDate <= ZDateTime.Today && x.RE_ExpiryDate >= ZDateTime.Today);
			if (exchangeRate == null)
			{
				exchangeRate = usdCurrency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = currencyCode;
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = date.AddMonths(-1);
				exchangeRate.RE_ExpiryDate = date.AddMonths(1);
			}

			exchangeRate.RE_SellRate = rate;
		}

		public void TestSpecificRegimeUsageAndRelatedAuthorisationHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TESTACC";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = "21P";

			AssertNull(instruction.SpecificRegimeAuthorisationUsage);
			AssertNull(instruction.SpecificRegimeAuthorisationUsage?.RelatedAuthorisationHeader);

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = "OPO";

			AssertNotNull(instruction.SpecificRegimeAuthorisationUsage);
			AssertEquals(usage.PK, instruction.SpecificRegimeAuthorisationUsage.PK);
			AssertNull(instruction.SpecificRegimeAuthorisationUsage?.RelatedAuthorisationHeader);

			usage.AGC_OH_Owner = importer.PK;
			usage.AGC_Number = "12345678";

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345678";
			authorisationHeader.CPH_IsSingleUse = true;
			authorisationHeader.CPH_Type = "OPO";
			authorisationHeader.CPH_OH_PermitHolder = importer.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";

			AssertNotNull(instruction.SpecificRegimeAuthorisationUsage.RelatedAuthorisationHeader);
			AssertEquals(authorisationHeader.PK, instruction.SpecificRegimeAuthorisationUsage.RelatedAuthorisationHeader.PK);
		}

		public void TestGetCusAuthorizationUsages()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertType<CusAuthorizationUsageEntryInstructionCollection>(entryInstruction.CusAuthorizationUsages);
		}

		public void TestFromWarehouseAuthorisation()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			var authorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("TestAuth");
			AssertEquals(authorisation, entryInstruction.FromWarehouseAuthorisation);

			var authHeader = GenerateCusAuthorisationHeader(entryInstruction);
			AssertEquals("FromWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2 or from CEI_OA_Warehouse authorisation CWP/CW1/CW2.", authorisation, entryInstruction.FromWarehouseAuthorisation);

			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			AssertEquals("FromWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2.", authHeader, entryInstruction.FromWarehouseAuthorisation);
		}

		public void TestToWarehouseAuthorisation()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			var authorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("TestAuth");
			AssertEquals(authorisation, entryInstruction.ToWarehouseAuthorisation);

			var authHeader = GenerateCusAuthorisationHeader(entryInstruction);
			AssertEquals("ToWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2 and then from CEI_OA_Warehouse2 authorisation CWP/CW1/CW2.", authorisation, entryInstruction.ToWarehouseAuthorisation);

			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			AssertEquals("ToWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2.", authHeader, entryInstruction.ToWarehouseAuthorisation);
		}

		CusAuthorisationHeader GenerateCusAuthorisationHeader(CusEntryInstruction entryInstruction)
		{
			var authHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_Number = "TestUsage";

			var authorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorisationUsage.AGC_CPH_Authorization = authHeader.PK;

			return authHeader;
		}

		public void TestFromWarehouseCode()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			var authorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("TestAuth");
			AssertEquals("TestAuth", entryInstruction.FromWarehouseCode);

			var authHeader = GenerateCusAuthorisationHeader(entryInstruction);
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			AssertEquals("FromWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2.", "TestUsage", entryInstruction.FromWarehouseCode);
		}

		public void TestToWarehouseCode()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			var authorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("TestAuth");
			AssertEquals("TestAuth", entryInstruction.ToWarehouseCode);

			var authHeader = GenerateCusAuthorisationHeader(entryInstruction);
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			AssertEquals("ToWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2.", "TestUsage", entryInstruction.ToWarehouseCode);
		}

		public void TestFromWarehouseType()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			var authorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("TestAuth");
			AssertEquals(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, entryInstruction.FromWarehouseType);

			var authHeader = GenerateCusAuthorisationHeader(entryInstruction);
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			AssertEquals("FromWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2.", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, entryInstruction.FromWarehouseType);
		}

		public void TestToWarehouseType()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			var authorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("TestAuth");
			AssertEquals(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, entryInstruction.ToWarehouseType);

			var authHeader = GenerateCusAuthorisationHeader(entryInstruction);
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			AssertEquals("ToWarehouseAuthorisation should come from in priority from CusAuthorizationUsages CWP/CW1/CW2.", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, entryInstruction.ToWarehouseType);
		}

		public void TestValidation()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertType<CusEntryInstructionValidation>("Standalone entryInstruction validation type should be CusEntryInstructionValidation.", entryInstruction.Validation);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Prerequisite.", true, declaration.IsDeltaG);
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<DeltaGCusEntryInstructionValidation>("DeltaG entryInstruction validation type should be DeltaGCusEntryInstructionValidation.", entryInstruction.Validation);

			var ucc6Declaration = Factory.New<JobDeclaration>();
			ucc6Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Prerequisite.", true, ucc6Declaration.IsUCC6);
			var ucc6EntryInstruction = ucc6Declaration.CustomsEntryInstructions.AddNew();
			AssertType<CusEntryInstructionValidation>("DeltaIE entryInstruction validation type should be CusEntryInstructionValidation.", ucc6EntryInstruction.Validation);

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			AssertEquals("Prerequisite.", false, otherDeclaration.IsUCC6 || otherDeclaration.IsDeltaG);
			entryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
			AssertType<DeltaGCusEntryInstructionValidation>("entryInstruction validation type should be DeltaGCusEntryInstructionValidation by default.", entryInstruction.Validation);
		}

		public void TestGetNewLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<CusEntryInstructionLookups>(entryInstruction.Lookups);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIECusEntryInstructionLookups>(entryInstruction.Lookups);

			entryInstruction = Factory.New<CusEntryInstruction>();
			AssertType<CusEntryInstructionLookups>(entryInstruction.Lookups);
		}

		public void TestJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			AssertType<JobDeclaration>(instruction.JobDeclaration);
		}

		public void TestLodgeSubstyle()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			AssertType<JobDeclaration>(instruction.JobDeclaration);
			AssertNotNull(instruction.JobDeclaration);

			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			instruction.CEI_SubStyle = "D";
			Assert(instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "A";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "Y";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "Z";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "F";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "C";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			dec.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			instruction.CEI_SubStyle = "F";
			Assert(instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "C";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "Y";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "Z";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "D";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);

			instruction.CEI_SubStyle = "A";
			Assert(!instruction.IsPrelodgedSubstyle);
			Assert(!instruction.IsLodgedSubstyle);
			Assert(!instruction.IsNeitherPrelodgedNorLodged);
		}

		public void TestIsValidSubStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;

			declaration.JE_DeltaMode = "G1";

			foreach (var subStyle in new[] { "D", "A", "Z" })
			{
				entryInstruction.CEI_SubStyle = subStyle;
				AssertEquals(true, entryInstruction.IsValidSubStyle);
			}

			foreach (var subStyle in new[] { "", "XX", "F", "C", "Y" })
			{
				entryInstruction.CEI_SubStyle = subStyle;
				AssertEquals(false, entryInstruction.IsValidSubStyle);
			}

			declaration.JE_DeltaMode = "G2";

			foreach (var subStyle in new[] { "F", "C", "Y", "Z" })
			{
				entryInstruction.CEI_SubStyle = subStyle;
				AssertEquals(true, entryInstruction.IsValidSubStyle);
			}

			foreach (var subStyle in new[] { "", "XX", "D", "A" })
			{
				entryInstruction.CEI_SubStyle = subStyle;
				AssertEquals(false, entryInstruction.IsValidSubStyle);
			}
		}

		public void TestGetComplementarySubstyle()
		{
			AssertEquals("D", CusEntryInstruction.GetComplementarySubstyle("A"));
			AssertEquals("A", CusEntryInstruction.GetComplementarySubstyle("D"));
			AssertEquals("F", CusEntryInstruction.GetComplementarySubstyle("C"));
			AssertEquals("C", CusEntryInstruction.GetComplementarySubstyle("F"));
			AssertEquals("Y", CusEntryInstruction.GetComplementarySubstyle("Y"));
		}

		public void TestSpecificRegimeAuthorisation()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			AssertType<CusEntryHeader>(instruction.EntryHeader);
			AssertNull(instruction.SpecificRegimeAuthorisation);

			AssertEquals("SpecificRegimeNumber", ZString.Empty, instruction.SpecificRegimeNumber);
			AssertEquals("SpecificRegimeDescription", ZString.Empty, instruction.SpecificRegimeDescription);
			AssertEquals("SpecificRegimeCountryCode", ZString.Empty, instruction.SpecificRegimeCountryCode);
			AssertEquals("SpecificRegimeNature", ZString.Empty, instruction.SpecificRegimeNature);
			AssertEquals("SpecificRegimeCondition", ZString.Empty, instruction.SpecificRegimeCondition);
			AssertEquals("SpecificRegimeOffice", ZString.Empty, instruction.SpecificRegimeOffice);
			AssertEquals("SpecificRegimeLocation", ZString.Empty, instruction.SpecificRegimeLocation);
			AssertEquals("SpecificRegimeProcedure", ZString.Empty, instruction.SpecificRegimeProcedure);
			AssertEquals("SpecificRegimeInformation", ZString.Empty, instruction.SpecificRegimeInformation);

			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "";
			usage.AGC_OH_Owner = ZGuid.Empty;
			AssertNull("Authorisation Header has no Permit Holder", instruction.SpecificRegimeAuthorisation.PermitHolder);
			AssertEquals("Authorisation Header has no rules", 0, instruction.SpecificRegimeAuthorisation.CusAuthorisationRules.Count);

			AssertEquals("SpecificRegimeNumber", ZString.Empty, instruction.SpecificRegimeNumber);
			AssertEquals("SpecificRegimeDescription", ZString.Empty, instruction.SpecificRegimeDescription);
			AssertEquals("SpecificRegimeCountryCode", ZString.Empty, instruction.SpecificRegimeCountryCode);
			AssertEquals("SpecificRegimeNature", ZString.Empty, instruction.SpecificRegimeNature);
			AssertEquals("SpecificRegimeCondition", ZString.Empty, instruction.SpecificRegimeCondition);
			AssertEquals("SpecificRegimeOffice", ZString.Empty, instruction.SpecificRegimeOffice);
			AssertEquals("SpecificRegimeLocation", ZString.Empty, instruction.SpecificRegimeLocation);
			AssertEquals("SpecificRegimeProcedure", ZString.Empty, instruction.SpecificRegimeProcedure);
			AssertEquals("SpecificRegimeInformation", ZString.Empty, instruction.SpecificRegimeInformation);

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			authHeader.CPH_PermitDescription = "CN Code 123456";

			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.NAT, "N1");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.CON, "C1");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.OFC, "O1").CPR_Description = "Square";
			CreateCusAuthorisationRule(authHeader, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "L1").CPR_Description = "Hidden";
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.TRA, "T1").CPR_Description = "T1 Description";
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.INF, "I1");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "9");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");

			authHeader.CPH_IsAdHoc = true;
			AssertEquals("SpecificRegimeNumber", "TST_ATH_001", instruction.SpecificRegimeNumber);
			AssertEquals("SpecificRegimeDescription", "CN Code 123456", instruction.SpecificRegimeDescription);
			AssertEquals("SpecificRegimeCountryCode", "GB", instruction.SpecificRegimeCountryCode);
			AssertEquals("SpecificRegimeNature", "N1", instruction.SpecificRegimeNature);
			AssertEquals("SpecificRegimeCondition", "C1", instruction.SpecificRegimeCondition);
			AssertEquals("SpecificRegimeOffice", "O1", instruction.SpecificRegimeOffice);
			AssertEquals("SpecificRegimeLocation", "L1", instruction.SpecificRegimeLocation);
			AssertEquals("SpecificRegimeProcedure", "T1", instruction.SpecificRegimeProcedure);
			AssertEquals("SpecificRegimeProcedureDescription", "T1 Description", instruction.SpecificRegimeProcedureDescription);
			AssertEquals("SpecificRegimeInformation", "I1", instruction.SpecificRegimeInformation);

			authHeader.CPH_IsAdHoc = false;
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.AUT, "ZZZZZ");
			AssertEquals("SpecificRegimeNumber", "ZZZZZ", instruction.SpecificRegimeNumber);
		}

		public void TestCEI_Procedure()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals("Requested Procedure", DataBoundResourceStrings.GetDataForProperty(entryInstruction.CEI_ProcedureInfo).Caption);
			AssertEquals("Req. Procedure", DataBoundResourceStrings.GetDataForProperty(entryInstruction.CEI_ProcedureInfo).MediumCaption);
			AssertEquals("Procedure", DataBoundResourceStrings.GetDataForProperty(entryInstruction.CEI_ProcedureInfo).ShortCaption);
		}

		public void TestDeleteCusGoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusGoodsLocation = entryInstruction.GoodsLocation;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.EntryInstruction;
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var entryInstruction1 = factory1.Load<CusEntryInstruction>(entryInstruction.PK);
			entryInstruction1.Delete();
			factory1.Save();
			AssertNull("should delete dbo.cusGoodsLocation from DB even we do not access it in factory1", Factory.Load<CusGoodsLocation>(cusGoodsLocation.PK));
		}

		internal static CusAuthorisationRule CreateCusAuthorisationRule(CusAuthorisationHeader authHeader, string code, string value)
		{
			var rule = authHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = code;
			rule.CPR_ValueFrom = value;
			return rule;
		}

		public void TestGuaranteeForEntryInstructionType()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();

			AssertType<GuaranteeForEntryInstruction>("guarantee for entry instruction should be FR.GuaranteeForEntryInstruction", entryInstruction.Guarantees.AddNew());
		}

		public void TestBypassCode_MatchesAddInfo()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.AddInfo.ZG_BypassCode = "A";

			AssertEquals("ZG_BypassCode should be equal to AddInfo.ZG_BypassCode", "A", entryInstruction.ZG_BypassCode);
		}

		public void TestBypassReason_MatchesAddInfo()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.AddInfo.ZG_BypassReason = "AAA";

			AssertEquals("ZG_BypassReason should be equal to AddInfo.ZG_BypassReason", "AAA", entryInstruction.ZG_BypassReason);
		}

		public void TestTransNature_MatchesAddInfo()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.AddInfo.ZG_TransNature = "12";

			AssertEquals("ZG_TransNature should be equal to AddInfo.ZG_TransNature", "12", entryInstruction.ZG_TransNature);
		}

		public void TestIsSimplified()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSimplified should be false if declarationType is not equal to I1 or declarationSubType is not equal to C or F.", false, entryInstruction.IsSimplified);

			entryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSimplified should be false if declarationType is not equal to I1 or declarationSubType is not equal to C or F.", false, entryInstruction.IsSimplified);

			entryInstruction.CEI_SubStyle = "F";
			AssertEquals("IsSimplified should be false if declarationType is not equal to I1 or declarationSubType is not equal to C or F.", false, entryInstruction.IsSimplified);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSimplified should be true if declarationType is I1 and declarationSubType is either C or F.", true, entryInstruction.IsSimplified);

			entryInstruction.CEI_SubStyle = "F";
			AssertEquals("IsSimplified should be true if declarationType is I1 and declarationSubType is either C or F.", true, entryInstruction.IsSimplified);

			entryInstruction.CEI_SubStyle = "D";
			AssertEquals("IsSimplified should be false if declarationType is not equal to I1 or declarationSubType is not equal to C or F.", false, entryInstruction.IsSimplified);
		}

		public void TestIsEntryStyleOutOfInward()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.PermanentExportWithEI;
			var extender = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaG);
			var result = extender.IsEntryInstructionOutOfInward(entryInstruction);
			AssertEquals("DeltaG: IsEntryStyleOutOfInward should be false when CEI_Style is not 31P", false, result);

			entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ReExportOfNonUnionGoodsWithEI;
			result = extender.IsEntryInstructionOutOfInward(entryInstruction);
			AssertEquals("DeltaG: IsEntryStyleOutOfInward should be true when CEI_Style is 31P", true, result);

			result = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaIE).IsEntryInstructionOutOfInward(entryInstruction);
			AssertEquals("DeltaIE: IsEntryStyleOutOfInward should be always false", false, result);
		}
	}
}
