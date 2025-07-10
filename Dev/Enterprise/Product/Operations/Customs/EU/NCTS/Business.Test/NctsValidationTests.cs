using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeader))]
	public class NctsDepartureValidationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSafetyAndSecurityValidation()
		{
			departure.Principal.Validation.ValidateOrganisationPK();
			departure.BH_FTZMove = false;
			departure.Principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C191");

			departure.BH_FTZMove = true;
			departure.MovementHeader.Validation.ValidateBM_RL_NKForeignDestPort();
			AssertHasMessageErrorContaining(departure.MovementHeader.BM_RL_NKForeignDestPortInfo, "C191");

			SetPlaceOfLoading(departure.MovementHeader, "GBLHR");
			departure.MovementHeader.Validation.ValidateBM_RL_NKForeignDestPort();
			AssertNoMessageErrorContaining(departure.MovementHeader.BM_RL_NKForeignDestPortInfo, "C191");
		}

		#region Rules and Condition for Departure and Arrival Header

		public void TestConditionC001()
		{
			AssertNoMessageErrorContaining(departure.Consignee.OrganisationPKInfo, "C001");
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Australia, false);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Andorra, true);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.SanMarino, true);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.UnitedKingdom, true);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Serbia, true);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Macedonia, true);

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", departure.Consignee, "", relatedPortCode: "", countryCode: "");
			departure.Consignee.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.Consignee.OrganisationPKInfo, "C001");

			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Australia, false);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Andorra, false);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.SanMarino, false);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.UnitedKingdom, false);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Serbia, false);
			SetDestinationCountryAndAssert(Core.Constants.CountryCodes.Macedonia, false);
		}

		void SetDestinationCountryAndAssert(ZString countryCode, ZBool assertTrue)
		{
			departure.MovementHeader.BM_RL_NKDestinationPort = countryCode;
			departure.Consignee.Validation.ValidateOrganisationPK();
			if (assertTrue)
			{
				AssertHasMessageErrorContaining(departure.Consignee.OrganisationPKInfo, "C001");
			}
			else
			{
				AssertNoMessageErrorContaining(departure.Consignee.OrganisationPKInfo, "C001");
			}
		}

		public void TestConditionC030()
		{
			var departureOffice = SetDepartureOffice(departure, "GB123456");
			AssertNoMessageErrorContaining(departureOffice.OfficeCodeInfo, "C030");

			var destinationOffice = SetDestinationOffice(departure, "SM123456");
			departureOffice.ValidateOfficeCode();
			destinationOffice.ValidateOfficeCode();
			AssertHasMessageErrorContaining(departureOffice.OfficeCodeInfo, "C030");

			var transitOffice = AddTransitOffice(departure, "SM4444");
			departureOffice.ValidateOfficeCode();
			transitOffice.ValidateOfficeCode();
			destinationOffice.ValidateOfficeCode();
			AssertNoMessageErrorContaining(departureOffice.OfficeCodeInfo, "C030");
		}

		public void TestConditionC045_Phase4()
		{
			var item1 = departure.MovementHeader.GoodsItems.AddNew();
			var item2 = departure.MovementHeader.GoodsItems.AddNew();
			AssertNoMessageErrorContaining(item1.BY_TypeInfo, "Declaration type at header level must be T-");
			AssertEquals("", departure.MovementHeader.BM_InBondEntryType);
			item1.BY_Type = "T1";
			AssertHasMessageErrorContaining(item1.BY_TypeInfo, "Declaration type at header level must be T-");
			item2.BY_Type = "T2";
			AssertHasMessageErrorContaining(item2.BY_TypeInfo, "Declaration type at header level must be T-");
			AssertEquals("T-", departure.MovementHeader.BM_InBondEntryType);
			item1.Validation.ValidateBY_Type();
			item2.Validation.ValidateBY_Type();
			AssertNoMessageErrorContaining(item1.BY_TypeInfo, "Declaration type at header level must be T-");
			AssertNoMessageErrorContaining(item2.BY_TypeInfo, "Declaration type at header level must be T-");

			departure.MovementHeader.BM_InBondEntryType = "T1";
			item1.Validation.ValidateBY_Type();
			AssertHasMessageErrorContaining(item1.BY_TypeInfo, "Declaration type at header level must be T-");
			item1.BY_Type = "";
			item2.BY_Type = "";
			item1.Validation.ValidateBY_Type();
			AssertNoMessageErrorContaining(item1.BY_TypeInfo, "Declaration type at header level must be T-");
		}

		public void TestLineDeclarationTypeTyeListInvalidCode()
		{
			var goodsItem = departure.MovementHeader.GoodsItems.AddNew();
			AssertNoMessageErrorContaining(goodsItem.BY_TypeInfo, "The code you have selected is not in the list");
			goodsItem.BY_Type = "NAF";
			AssertHasMessageErrorContaining(goodsItem.BY_TypeInfo, "The code you have selected is not in the list");
			goodsItem.BY_Type = "T2F";
			AssertNoMessageErrorContaining(goodsItem.BY_TypeInfo, "The code you have selected is not in the list");
		}

		public void TestConditionC050()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, "", "OSCORP", "F", "U", "C", "GBKK", "GB", "");
			departure.Principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C050");

			orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, "", "OSCORP", "", "", "", "GBKK", "GB", "");
			departure.Principal.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C050");

			NCTSTestHelper.CreateEoriForTest(orgHeader, "EORI1234", "GB");
			departure.Principal.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C050");
		}

		public void TestConditionC111()
		{
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0111Active));

				var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, "1", traderTin: "");
				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C111");

				departure.MovementHeader.IsSimplifiedNctsProcedure = true;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C111");

				departure.MovementHeader.IsSimplifiedNctsProcedure = false;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C111");

				departure.MovementHeader.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C111");

				SetDepartureOffice(departure, "GB123456");
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C111");

				NCTSTestHelper.CreateEoriForTest(orgHeader, "EORI1234", "GB");
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C111");

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0111Active));

				departure.MovementHeader.IsSimplifiedNctsProcedure = true;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Rule inactive", departure.Principal.OrganisationPKInfo, "C111");
			}
		}

		public void TestConditionC191()
		{
			AssertNoMessageErrorContaining(departure.MovementHeader.BM_RL_NKForeignDestPortInfo, "C191");

			departure.BH_FTZMove = true;
			departure.MovementHeader.Validation.ValidateBM_RL_NKForeignDestPort();
			AssertHasMessageErrorContaining(departure.MovementHeader.BM_RL_NKForeignDestPortInfo, "C191");

			SetPlaceOfLoading(departure.MovementHeader, "GBLHR");
			departure.MovementHeader.Validation.ValidateBM_RL_NKForeignDestPort();
			AssertNoMessageErrorContaining(departure.MovementHeader.BM_RL_NKForeignDestPortInfo, "C191");
		}

		protected virtual void SetPlaceOfLoading(NctsDepartureMovementHeader movementHeader, ZString value) => movementHeader.BM_RL_NKForeignDestPort = value;

		public void TestConditionC236()
		{
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0236Active));

				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C236");

				var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, "", "OSCORP", "F", "U", "C", "GBKK", "GB", "");
				var guarantee = departure.Guarantees.AddNew();
				guarantee.PW_BondNumber = "ISA";

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0236Active));

				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C236");

				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0236Active));

				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C236");

				NCTSTestHelper.CreateEoriForTest(orgHeader, "EORI1234", "GB");
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(departure.Principal.OrganisationPKInfo, "C236");
			}
		}

		public void TestConditionC572()
		{
			departure.BH_FTZMove = true;
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "GL", departure.SecurityConsignor, "1", traderTin: "");
			departure.SecurityConsignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C572");

			departure.MovementHeader.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
			departure.SecurityConsignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C572");

			SetDepartureOffice(departure, "GB123456");
			departure.SecurityConsignor.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C572");

			NCTSTestHelper.CreateEoriForTest(orgHeader, "EORI1234", "GB");
			departure.SecurityConsignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C572");
		}

		public void TestConditionR904()
		{
			SetDepartureOffice(departure, "SM123456");

			var destinationOffice = SetDestinationOffice(departure, "AU123456");
			AssertHasMessageErrorContaining(destinationOffice.OfficeCodeInfo, "R904");

			destinationOffice.SetValue("DE123456");
			destinationOffice.ValidateOfficeCode();
			AssertNoMessageErrorContaining(destinationOffice.OfficeCodeInfo, "R904");
		}

		public void TestConditionR905()
		{
			var departureOffice = SetDepartureOffice(departure, "GB123456");
			var destinationOffice = SetDestinationOffice(departure, "AD123456");
			AssertNoMessageErrorContaining(destinationOffice.OfficeCodeInfo, "R905");
			departureOffice.SetValue("RS123456");
			destinationOffice.ValidateOfficeCode();
			AssertHasMessageErrorContaining(destinationOffice.OfficeCodeInfo, "R905");
		}

		public void TestConditionR906()
		{
			SetDestinationOffice(departure, "AD123456");

			var transitOffice = AddTransitOffice(departure, "FR123456");
			AssertHasMessageErrorContaining("When destination office is Andorra but transit office is not", transitOffice.OfficeCodeInfo, "R906");

			transitOffice.SetValue("AD123456");
			transitOffice.ValidateOfficeCode();
			AssertNoMessageErrorContaining("When both destination and transit offices are Andorra", transitOffice.OfficeCodeInfo, "R906");
		}

		public void TestConditionR907()
		{
			SetDestinationOffice(departure, "SM123456");
			var transitOffice = AddTransitOffice(departure, "AU123456");
			AssertHasMessageErrorContaining(transitOffice.OfficeCodeInfo, "R907");
			transitOffice.SetValue("IT123456");
			AssertNoMessageErrorContaining(transitOffice.OfficeCodeInfo, "R907");
		}

		public void TestConditionR908()
		{
			SetDepartureOffice(departure, "NO123456");
			var transitOffice = AddTransitOffice(departure, "FR123456");
			AssertNoMessageErrorContaining(transitOffice.OfficeCodeInfo, "R908");
			transitOffice.SetValue("AD");
			transitOffice.ValidateOfficeCode();
			AssertHasMessageErrorContaining(transitOffice.OfficeCodeInfo, "R908");
		}

		public void TestConditionR910()
		{
			var departureOffice = SetDepartureOffice(departure, "AD123456");
			var transitOffice = AddTransitOffice(departure, "AU123456");
			departureOffice.SetValue("SM123456");
			AssertHasMessageErrorContaining(transitOffice.OfficeCodeInfo, "R910");
			transitOffice.SetValue("IT123456");
			AssertNoMessageErrorContaining(transitOffice.OfficeCodeInfo, "R910");
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			return departure;
		}

		protected virtual void SaveAndReloadNctsHeaderInNewFactory()
		{
			Factory.Save();
			departure = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = Factory.New<NctsHeader>();
			result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			result.SetMovementType(NctsMovementType.Codes.Departure);
			result.MovementHeader.CustomsOffices.AddNew();
			return result;
		}

		protected virtual string NctsDeclarationTypeListCountryCode => Core.Constants.CountryCodes.Latvia;

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList(NctsDeclarationTypeListCountryCode);
			Factory.Save();

			departure = GetNewBusinessObject() as NctsHeader;
		}

		protected NctsHeader departure;

		protected virtual CustomsOfficeTestItem SetDepartureOffice(NctsHeader nctsHeader, ZString officeCode)
		{
			var departureCustomsOffice = NCTSTestHelper.CreateCustomsOfficeForTest(departure, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, officeCode, ZDateTime.Empty);
			return new CustomsOfficeTestItem(departureCustomsOffice);
		}

		protected virtual CustomsOfficeTestItem SetDestinationOffice(NctsHeader nctsHeader, ZString officeCode)
		{
			var destinationCustomsOffice = NCTSTestHelper.CreateCustomsOfficeForTest(departure, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, officeCode, ZDateTime.Empty);
			return new CustomsOfficeTestItem(destinationCustomsOffice);
		}

		protected virtual CustomsOfficeTestItem AddTransitOffice(NctsHeader nctsHeader, ZString officeCode)
		{
			var destinationCustomsOffice = NCTSTestHelper.CreateCustomsOfficeForTest(departure, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, officeCode, ZDateTime.Empty);
			return new CustomsOfficeTestItem(destinationCustomsOffice);
		}

		protected class CustomsOfficeTestItem
		{
			public CustomsOfficeTestItem(EuOfficeCode officeCode)
				: this(officeCode, officeCode.CY_DataInfo, officeCode.Validation.ValidateCY_Data)
			{
			}

			public CustomsOfficeTestItem(ICustomsOffice customsOffice, ZPropertyInfo officeCodeInfo, Action validateOfficeCodeAction)
			{
				CustomsOffice = customsOffice;
				OfficeCodeInfo = officeCodeInfo;
				this.validateOfficeCodeAction = validateOfficeCodeAction;
			}

			readonly Action validateOfficeCodeAction;

			public ICustomsOffice CustomsOffice { get; }
			public ZPropertyInfo OfficeCodeInfo { get; }

			public void ValidateOfficeCode() => validateOfficeCodeAction();

			public void SetValue(ZString value) => OfficeCodeInfo.SetValueFromString(value);
		}
	}

	[TestedType(typeof(NctsHeader))]
	sealed class NctsDepartureValidation_DoNotInheritTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConditionC547()
		{
			var departureMovementHeader = departure.MovementHeader;
			var g1 = departure.Bills.AddNew().GoodsItems.AddNew();
			departure.BH_FTZMove = true;
			departure.MovementHeader.BM_TypeOfSecurity = "BTH";
			departureMovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageErrorContaining(departureMovementHeader.BM_InBondEntryTypeInfo, "C547");
			var sd1 = g1.SupportingDocuments.AddNew();
			sd1.CSI_Code = NctsHeaderValidationHelper.TirCarnetDocumentCode;
			departureMovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageErrorContaining(departureMovementHeader.BM_InBondEntryTypeInfo, "C547");
		}

		public void TestSecurityConsignorValidationAtHeaderLevel()
		{
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.BH_FTZMove = true;
			AssertHasMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C187");
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "GL", departure.SecurityConsignor, "1", traderTin: "");
			departure.SecurityConsignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C187");
		}

		public void TestSecurityConsignorValidationAtGoodsItemLevel()
		{
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.BH_FTZMove = true;
			AssertHasMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C187");
			var goodsItem = departure.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_Description = "xxx";
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CON", goodsItem.SecurityConsignor, "1", traderTin: "");
			departure.SecurityConsignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(departure.SecurityConsignor.OrganisationPKInfo, "C187");
		}

		public void TestConditionC0505DeparturePhase5_Principal()
		{
			const bool c0505Applied = true;

			CombineAssertions(() =>
			{
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, departure.Principal, !c0505Applied);

				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, departure.Principal, c0505Applied);
			});
		}

		public void TestConditionC0505DeparturePhase5_Consignor()
		{
			const bool c0505Applied = true;

			CombineAssertions(() =>
			{
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, departure.Consignor, !c0505Applied);

				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, departure.Consignor, c0505Applied);
			});
		}

		public void TestConditionC0505DeparturePhase5_Consignee()
		{
			const bool c0505Applied = true;

			CombineAssertions(() =>
			{
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, departure.Consignee, !c0505Applied);

				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, departure.Consignee, c0505Applied);
			});
		}

		public void TestPrincipalOrganizationForTIROrEORNumber()
		{
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			const string expectedErrorMessage = "[C0904] TIR Holder Identification Number or EORI Number is required for Principal Organization.";
			var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, "TESTID", countryCode: "IT", relatedPortCode: "ITXX", traderTin: ZString.Empty, traderTir: ZString.Empty);
			departure.Principal.E2_OA_Address = orgAddress.PK;

			departure.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0904Active));

				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("BM_InBondEntryType=TIR, ORG=Having No TIR Holder Id. Number or EORI Number", departure.Principal.OrganisationPKInfo, expectedErrorMessage);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0904Active));
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoError("Rule C0904 is disabled", departure.Principal.OrganisationPKInfo, expectedErrorMessage);
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0904Active));

				orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR123222", "ES");
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("BM_InBondEntryType=TIR, ORG=Having No TIR Holder Id. Number, EORI Number for a different Country", departure.Principal.OrganisationPKInfo, expectedErrorMessage);

				orgAddress.CustomsCodes.DeleteAll();
				orgAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIR12322", "FR");
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("BM_InBondEntryType=TIR, ORG=Having TIR Holder Id. Number for a different country, No EORI Number", departure.Principal.OrganisationPKInfo, expectedErrorMessage);

				var orgAddressWithTir = NCTSTestHelper.CreateOrgAddressForTest(Factory, "T12343", traderTir: "TIR12343");
				departure.Principal.E2_OA_Address = orgAddressWithTir.PK;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("BM_InBondEntryType=TIR, ORG=With TIR Number, No Error.", departure.Principal.OrganisationPKInfo, expectedErrorMessage);

				var orgAddressWithEori = NCTSTestHelper.CreateOrgAddressForTest(Factory, "E12332", traderTin: "EORI1234");
				departure.Principal.E2_OA_Address = orgAddressWithEori.PK;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("BM_InBondEntryType=TIR, ORG=With EORI Number, No Error.", departure.Principal.OrganisationPKInfo, expectedErrorMessage);

				departure.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
				departure.Principal.E2_OA_Address = orgAddress.PK;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("BM_InBondEntryType=T2, No Error.", departure.Principal.OrganisationPKInfo, expectedErrorMessage);
			}
		}

		public void TestRuleTR0087()
		{
			var messageError = "[TR0087] " + MandatoryValidation.YouHaveNotEnteredMessage("Principal");
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var testContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory, typeof(IRuleTR0087Decider)))
			{
				testContext.ClearCachedValidationDecider(departure);
				testContext.EnableRuleDecider<IRuleTR0087Decider>(x => x.IsActive);
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageError("CheckConditionTR0087 invoked", departure.Principal.OrganisationPKInfo, messageError);
			}
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
				&& info.Name != "DestinationCustomsOfficeCodeForArrival")
			{
				base.TestBizObjectField(info);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.Latvia);
			Factory.Save();

			departure = GetNewBusinessObject() as NctsHeader;
		}

		NctsHeader departure;
		protected override BusinessObject GetNewBusinessObject()
		{
			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			return departure;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = Factory.New<NctsHeader>();
			result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			result.SetMovementType(NctsMovementType.Codes.Departure);
			result.MovementHeader.CustomsOffices.AddNew();
			return result;
		}
	}

	[TestedType(typeof(NctsHeader))]
	class NctsArrivalValidationTests : EnterpriseBusinessObjectTestCase
	{
		public void TestArrivalOfficeValidation()
		{
			var departureOffice = NCTSTestHelper.CreateCustomsOfficeForTest(arrival, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "SM12345", ZDateTime.Empty);
			NCTSTestHelper.CreateCustomsOfficeForTest(arrival, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "GB12345", ZDateTime.Empty);
			departureOffice.Validation.ValidateCY_Data();
			AssertNoMessageErrorContaining(departureOffice.CY_DataInfo, "C030");

			arrival.BH_HeaderType = NctsMovementType.Codes.Departure;
			departureOffice.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(departureOffice.CY_DataInfo, "C030");
		}

		#region Rules and Condition for NctsArrival Header

		public void TestConditionC112()
		{
			var trader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CPD", arrival.DestinationTrader, traderTin: "");
			arrival.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(arrival.DestinationTrader.OrganisationPKInfo, "C112");
			arrival.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;
			arrival.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(arrival.DestinationTrader.OrganisationPKInfo, "C112");
			NCTSTestHelper.CreateCustomsOfficeForTest(arrival, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "GB123456", ZDateTime.Empty);
			arrival.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(arrival.DestinationTrader.OrganisationPKInfo, "C112");
			NCTSTestHelper.CreateEoriForTest(trader, "EORI1234", "GB");
			arrival.DestinationTrader.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(arrival.DestinationTrader.OrganisationPKInfo, "C112");
		}

		#endregion

		protected virtual void SaveAndReloadNctsHeaderInNewFactory()
		{
			Factory.Save();
			arrival = new BusinessObjectFactory().Load<NctsHeader>(arrival.PK);
		}

		protected override BusinessObject GetNewBusinessObject() => arrival;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = Factory.New<NctsHeader>();
			result.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			result.SetMovementType(NctsMovementType.Codes.Arrival);
			result.CustomsOffices.AddNew();
			return result;
		}

		public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			Assert("Not tested for Arrivals: Arrivals do not support adding SupportingDocuments to NctsHeader", true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();

			arrival = Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
		}

		protected NctsHeader arrival;
	}
}
