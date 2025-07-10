using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.DE.Business.Testing
{
	public static class TestHelper
	{
		public static RefCusTariffType CreateExciseTariffType(UniversalReferenceTestDataHelper helper)
		{
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			tariffType.Factory.Save();
			return tariffType;
		}

		public static void CreateTaxOrFeeWithType(UniversalReferenceTestDataHelper helper, ZString taxOrFeeType, ZString taxOrFeeTypeDescription, ZString taxOrFeeCode, ZDecimal taxOrFeeValue)
		{
			var type = helper.CreateRefCusTaxOrFeeType(taxOrFeeType, taxOrFeeTypeDescription);
			helper.CreateTaxOrFee(taxOrFeeCode, taxOrFeeValue, Core.Constants.CountryCodes.Germany, 0m, 0m, type.ZX0_TaxOrFeeType, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			type.Factory.Save();
		}

		public static void CreateFacilityCodeTypeWithCusCode(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, CustomsConstants.RefCusCodeListTypes.Codes.Facilities, "01", "01 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public static void CreateTSTAuthorizationWithLinkedCustomsOffice(OrgHeader permitHolder)
		{
			var authorisation = permitHolder.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER1");
			var authorisationRule = authorisation.CreateAuthorisationRule(Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, "01");
			authorisationRule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE000001");
		}

		public static void CreateDC40CodeTypeWithCusCode(BusinessObjectFactory factory, ZString levelAttributeValue, ZString additionalAttributeName, ZString code1AdditionalAttributeValue, ZString code2AdditionalAttributeValue)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DC40E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(additionalAttributeName, additionalAttributeName, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, Core.Constants.CountryCodes.Germany);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DE01", "DE01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttributeValue);
			if (!code1AdditionalAttributeValue.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, additionalAttributeName, code1AdditionalAttributeValue);
			}

			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DE02", "DE02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, levelAttributeValue);
			if (!code2AdditionalAttributeValue.IsEmpty)
			{
				helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, additionalAttributeName, code2AdditionalAttributeValue);
			}
			factory.Save();
		}

		public static IDisposable SetTemporaryCurrentUser(this BusinessObjectFactory factory, string title = "", string fullName = "", string workPhone = "", string email = "")
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_Title = title;
			staff.GS_FullName = fullName;
			staff.GS_WorkPhone = workPhone;
			staff.GS_EmailAddress = email;
			return Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		public static OrgHeader CreateDeferralParty(this BusinessObjectFactory factory, ZString accountType, ZString accountNumber)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.AddDefermentAccountNumber(accountType, accountNumber);
			return org;
		}

		public static OrgCusAccount AddDefermentAccountNumber(this OrgHeader org, ZString accountType, ZString accountNumber, string countryCode = "DE")
		{
			var account = org.DefermentAccountNumberCollection.AddNew();
			account.CZ_Code = accountType;
			account.CZ_Account = accountNumber;
			account.CZ_RN_NKCountryCode = countryCode;

			return account;
		}

		public static ZString GetHumanReadableName(this ZPropertyInfo propertyInfo)
		{
			return propertyInfo?.HasHumanReadableName ?? false ? propertyInfo.HumanReadableName.ToString() : @"value";
		}

		public static void ModifyOrgCusCode(this OrgCusCode orgCusCode, ZGuid orgHeaderPK, ZString cusCodeType, ZString countryCode, ZString cusCode, ZGuid premisesAddressPK = default)
		{
			orgCusCode.OK_OH = orgHeaderPK;
			orgCusCode.OK_RN_NKCodeCountry = countryCode;
			orgCusCode.OK_CodeType = cusCodeType;
			orgCusCode.OK_CustomsRegNo = cusCode;
			if (premisesAddressPK != ZGuid.Empty)
			{
				orgCusCode.OK_OA_PremisesAddress = premisesAddressPK;
			}
		}

		public static PreviousDocument GetOnlyPreviousDocument(this Declaration.CusEntryInstruction instruction) => instruction.PreviousDocuments.Cast<PreviousDocument>().Single();

		public static ReimportCountryCode CreateReimportCountryCode(this BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			return entryInstruction.ReimportCountryCodes.AddNew();
		}

		public static IdentificationMeansCode CreateIdentificationMeansCode(this BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			return entryInstruction.IdentificationMeanCodes.AddNew();
		}

		public static ContentInformationType CreateContentInformationType(this BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			return invoiceLine.ContentInformationTypes.AddNew();
		}

		public static Declaration.CusEntryInstruction CreateInwardProcessingInstruction(this BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			return instruction;
		}

		public static void AssertResponseDetails(ResponseMessageDetails responseMessageDetails, ZString embeddedResource, Type xmlObjectType, Type providerType, Type ediMessageType)
		{
			var actualXmlObjectType = responseMessageDetails.XmlObjectType;
			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("Schema", embeddedResource, responseMessageDetails.XsdSchemaEmbeddedResourceName);
				Assertion.AssertEquals("XML Type", xmlObjectType, actualXmlObjectType);
				Assertion.AssertEquals("Provider Type", providerType, responseMessageDetails.ProviderType);
				Assertion.AssertEquals("EDIMessage Type", ediMessageType, responseMessageDetails.EDIMessageType);
				Assertion.AssertNotNull("XSD file is set as an embedded resource", actualXmlObjectType.Assembly.GetManifestResourceInfo(embeddedResource));
			});
		}

		public static void AssertMRNFormatValidated(string assertionMessage, ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(assertionMessage))
			{
				assertionMessage += ": ";
			}

			propertyInfo.SetValueFromString("21DE12345678901234");
			TestCaseWithFactory.AssertNoNotifications($"{assertionMessage}Valid MRN number", propertyInfo);

			propertyInfo.SetValueFromString("DEDE586601055987B7");
			TestCaseWithFactory.AssertHasMessageErrorContaining($"{assertionMessage}MRN number mismatches RegEx", propertyInfo, "Please enter a MRN in the following format with only numbers and upper case letters");

			propertyInfo.SetValueFromString("21DE27364916384835");
			TestCaseWithFactory.AssertHasMessageErrorContaining($"{assertionMessage}Invalid MRN check digit", propertyInfo, "MRN does not have a valid last digit");

			propertyInfo.SetValueFromString("21AB27364916384830");
			TestCaseWithFactory.AssertHasMessageErrorContaining($"{assertionMessage}Invalid MRN country code", propertyInfo, "MRN does not contain a valid country/region code");
		}

		public static void AssertMRNFormatNotValidated(string assertionMessage, ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(assertionMessage))
			{
				assertionMessage += ": ";
			}

			propertyInfo.SetValueFromString("DEDE586601055987B7");
			TestCaseWithFactory.AssertNoMessageErrorContaining($"{assertionMessage}MRN number mismatches RegEx", propertyInfo, "Please enter a MRN in the following format with only numbers and upper case letters");
		}

		public static CusAuthorisationHeader CreateAuthorisationRecord(this OrgHeader orgHeader, ZString type, ZString number, string countryCode = Core.Constants.CountryCodes.Germany)
		{
			var result = orgHeader.Factory.New<CusAuthorisationHeader>();
			result.CPH_OH_PermitHolder = orgHeader.PK;
			result.CPH_Type = type;
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_StartDate = ZDate.Today.AddDays(-1);
			result.CPH_EndDate = ZDate.Today.AddDays(1);
			result.CPH_Number = number;
			return result;
		}

		public static void AssertMRNFormatValidatedOr21CharactersLong(string assertionMessage, ZPropertyInfo propertyInfo)
		{
			AssertMRNFormatValidated(assertionMessage, propertyInfo);

			const string validLengthMessageError = "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).";
			propertyInfo.SetValueFromString("AT1234567890123456789");
			TestCaseWithFactory.AssertNoMessageError($"{assertionMessage} no MRN: valid length of 21 characters", propertyInfo, validLengthMessageError);

			propertyInfo.SetValueFromString("AT123456789012345678");
			TestCaseWithFactory.AssertHasMessageError($"{assertionMessage} no MRN: invalid length of != 21 characters", propertyInfo, validLengthMessageError);
		}

		public static void AssertMRNFormatOr21CharactersLongNotValidated(string assertionMessage, ZPropertyInfo propertyInfo)
		{
			AssertMRNFormatNotValidated(assertionMessage, propertyInfo);

			propertyInfo.SetValueFromString("AT123456789012345678");
			TestCaseWithFactory.AssertNoMessageError($"{assertionMessage} no MRN: invalid length of != 21 characters", propertyInfo, "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).");
		}

		public static CusAuthorisationHeader CreateAuthorisationRecord(this OrgAddress orgAddress, ZString type, ZString number, string countryCode = Core.Constants.CountryCodes.Germany)
		{
			var result = orgAddress.Factory.New<CusAuthorisationHeader>();
			result.CPH_OA_AppliesTo = orgAddress.PK;
			result.CPH_Type = type;
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_StartDate = ZDate.Today.AddDays(-1);
			result.CPH_EndDate = ZDate.Today.AddDays(1);
			result.CPH_Number = number;
			return result;
		}

		public static CusAuthorisationRule CreateAuthorisationRule(this CusAuthorisationHeader authorisation, ZString ruleCode, ZString valueFrom, string ruleDescription = null)
		{
			var result = authorisation.CusAuthorisationRules.AddNew();
			result.CPR_RuleCode = ruleCode;
			result.CPR_ValueFrom = valueFrom;
			if (ruleDescription != null)
			{
				result.CPR_Description = ruleDescription;
			}
			return result;
		}

		public static (CusAuthorisationHeader authorizationHeader, CusAuthorisationRule rule) CreateAuthorisationWithRule(this OrgHeader orgHeader, ZString type, ZString number, ZString ruleCode, ZString valueFrom, string authCountryCode = Core.Constants.CountryCodes.Germany, string ruleDescription = null)
		{
			var authorization = CreateAuthorisationRecord(orgHeader, type, number, authCountryCode);
			var rule = CreateAuthorisationRule(authorization, ruleCode, valueFrom, ruleDescription);
			return (authorization, rule);
		}

		public static LinkedCusAuthorisationRule CreateLinkedAuthorisationRule(this CusAuthorisationRule authorisationRule, ZString ruleCode, ZString valueFrom)
		{
			var result = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			result.CPR_RuleCode = ruleCode;
			result.CPR_ValueFrom = valueFrom;
			return result;
		}

		public static OrgHeader GetOrgHeaderWithEori(this BusinessObjectFactory factory, ZString orgCode, ZString eoriNumber, ZString eoriNumberCountryCode)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			ModifyOrgCusCode(factory.New<OrgCusCode>()
				, orgHeader.PK
				, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori
				, eoriNumberCountryCode
				, eoriNumber);
			return orgHeader;
		}

		public static OrgHeader GetOrgHeaderWithEORIBranch(this BusinessObjectFactory factory, ZString orgCode, ZString eoriBranchNo)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			ModifyOrgCusCode(factory.New<OrgCusCode>()
				, orgHeader.PK
				, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix
				, Core.Constants.CountryCodes.Germany
				, eoriBranchNo
				, orgHeader.MainAddress.PK);
			return orgHeader;
		}

		public static OrgHeader GetOrgHeaderWithEoriNumberAndEORIBranch(this BusinessObjectFactory factory, ZString orgCode, ZString eoriNumber, ZString eoriNumberCountryCode, params ZString[] eoriBranches)
		{
			var orgHeader = GetOrgHeaderWithEori(factory, orgCode, eoriNumber, eoriNumberCountryCode);
			if (eoriBranches.Length <= 1)
			{
				var cusCode = orgHeader.CustomsCodes.AddNew();
				ModifyOrgCusCode(cusCode
					, orgHeader.PK
					, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix
					, Core.Constants.CountryCodes.Germany
					, eoriBranches.FirstOrDefault()
					, orgHeader.MainAddress.PK);
			}
			else
			{
				foreach (var branch in eoriBranches)
				{
					var cusCode = orgHeader.CustomsCodes.AddNew();
					var address = orgHeader.Addresses.AddNew();
					ModifyOrgCusCode(cusCode
						, orgHeader.PK
						, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix
						, Core.Constants.CountryCodes.Germany
						, branch
						, address.PK);
				}
			}
			return orgHeader;
		}

		public static void SetExchangeRates(BusinessObjectFactory factory)
		{
			var usd = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 0.9m, usd);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.IATARate, ZDateTime.Today, ZDateTime.Today, 0.8m, usd);
			var sek = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Sweden);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 10m, sek);
			var jpy = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 150m, jpy);

			void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
			{
				RefExchangeRate result = factory.New<RefExchangeRate>();
				result.RE_GC = company.PK;
				result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				result.RE_StartDate = startDate;
				result.RE_ExpiryDate = endDate;
				result.RE_SellRate = rate;
				result.RE_ExRateType = rateType;
			}
		}

		public static void CreateDEReimportCountryCountryWithCusCode(BusinessObjectFactory factory, ZString code, ZString description)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eUGroup = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eUGroup);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "I0809 CodeList  - Country list (re-import)");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, code, description, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			factory.Save();
		}

		public static void DeleteSingleOrgCusCode(this OrgHeader orgHeader, ZString codeType) => orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType).SingleOrDefault()?.Delete();

		public static void DeleteSingleEORINumber(this OrgHeader orgHeader) => orgHeader.DeleteSingleOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

		public static void DeleteSingleEORIBranch(this OrgHeader orgHeader) => orgHeader.DeleteSingleOrgCusCode(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix);

		public static ZString GetLogbookEORIBranchSuffix(this EDIMessage message) => message.GetSystemDefinedValue<ZString>("LogbookEORIBranchSuffix");

		public static ZString GetLogbookRegistrationNumber(this EDIMessage message) => message.Notes.FindByDescription(LogbookHelper.LogbookRegistrationNumberNoteDescription).SingleOrDefault()?.ST_NoteText ?? ZString.Empty;

		public static ZString[] GetLogbookRegistrationNumbers(this EDIMessage message)
		{
			var result = Array.Empty<ZString>();
			var noteText = message.Notes.FindByDescription(LogbookHelper.LogbookRegistrationNumberNoteDescription).SingleOrDefault()?.ST_NoteText ?? ZString.Empty;
			if (!noteText.IsEmpty)
			{
				result = noteText.Split(LogbookHelper.LogbookRegistrationNumberNoteDescriptionSeperator);
			}
			return result;
		}

		public static ZString GetLogbookLocalReferenceNumber(this EDIMessage message) => message.Notes.FindByDescription(LogbookHelper.LogbookLocalReferenceNumberNoteDescription).SingleOrDefault()?.ST_NoteText ?? ZString.Empty;

		public static ZString GetCustomsEntryStatusEventReference(this Declaration.CusEntryHeader entryHeader) => entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.CustomsEntryStatus).SingleOrDefault()?.SL_Reference ?? ZString.Empty;

		public const string BusinessTestDirectory = @"Enterprise\Product\Operations\Customs\DE\Core\Business.Test\";

		public static void CreateCL010CoutryList(BusinessObjectFactory factory)
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);

			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "European Countries Of Destination");
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Greece, Core.Constants.CountryCodes.Greece, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Italy, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Netherlands, Core.Constants.CountryCodes.Netherlands, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.France, startDate, endDate);
			factory.Save();
		}

		public static void CreateSingleValueConditionForTariff(this UniversalReferenceTestDataHelper helper, ZGuid tariffPK, ZBool isImport, ZBool isExport, ZGuid conditionTypePK, ZString conditionComment, ZGuid conditionValueTypePK, ZString conditionValueValue, ZGuid? preferencePK = null, bool trueMeansStop = false)
		{
			var cond = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.Germany, conditionTypePK, tariffPK, conditionComment, isImport, isExport, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			if (preferencePK.HasValue)
			{
				cond.ZX1_ZZS_Preference = preferencePK.Value;
			}

			cond.ZX1_ConditionValueTrueMeansStop = trueMeansStop;
			cond.Factory.Save();
			if (!trueMeansStop)
			{
				helper.CreateOrGetExistingRefCusConditionValue(conditionValueTypePK, cond.PK, conditionValueValue);
			}
		}
	}
}
