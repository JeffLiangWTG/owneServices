using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMaxCountValidation()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var collection = header.Bills;
			var bill = collection.AddNew();
			AssertNull(collection.MaxCountValidator.Notification);
		}

		public void TestCheckDestinationCustomsOfficeCodeForDeparture()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.MovementHeader.BM_GONumber = "A3";

			var declarantAuthorisation = declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit).WithNumber("DECLARANT_AUT").WithCountry(header.CountryCode);
			declarantAuthorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
			declarantAuthorisation.CPH_OA_AppliesTo = consignor.MainAddress.PK;
			var authorisationRule = declarantAuthorisation.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.OFC;
			authorisationRule.CPR_ValueFrom = "FR002222";

			header.Consignor.E2_OA_Address = consignor.MainAddress.PK;
			header.Declarant.E2_OA_Address = declarant.MainAddress.PK;
			CombineAssertions("Prerequisites", () =>
			{
				Assert(!header.IsPhase5);
				Assert(header.MovementHeader.IsSimplifiedNctsProcedure);
				AssertEquals("FR002222", header.DepartureCustomsOfficeCode);
				AssertEquals("FR002222", header.ConsignorACRAuthorisationOFCRuleValue);
			});

			var info = header.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).CY_DataInfo;

			AssertNoMessageError("No message error is expected when captured Departure Customs Office code matches Declarant ACR authorisation OFC rule value.", info, "This Customs office doesn't match the office set in Declarant authorization matching Consignor address.");

			var officeOfDeparture = NctsEuOfficeCode.Load<NctsEuOfficeCode>(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			officeOfDeparture.CY_Data = "FR001111";
			AssertHasMessageError("A message error is expected when Declarant ACR authorisation OFC rule exists but doesn't match captured Departure Customs Office code.", info, "This Customs office doesn't match the office set in Declarant authorization matching Consignor address.");

			var consignorWithoutAuthorisationOffice = Factory.NewWithValidTestData<OrgHeader>();
			header.Consignor.E2_OA_Address = consignorWithoutAuthorisationOffice.MainAddress.PK;
			officeOfDeparture.CY_Data = "FR001112";
			AssertHasMessageError("A message error is expected when Declarant ACR authorisation OFC rule doesn't exist.", info, "No Declarant ACR authorization matching Consignor address with OFC rule could be found.");

			header.Consignor.E2_OA_Address = consignor.MainAddress.PK;
			AssertNoMessageError("No message error is expected when Declarant has ACR authorisation with OFC rule matching consignor adress.", info, "No Declarant ACR authorization matching Consignor address with OFC rule could be found.");
		}

		public void TestCheckBH_MessageStatus()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms;
				log.SL_SE_NKEvent = Events.FrenchCustomsMessageStatus.Code;
				log.SL_EventTime = ZDateTime.Now;
				var errorContextItem = log.SourceInfoItems.AddNew();
				errorContextItem.Key = "Error";
				errorContextItem.Data = "Error Description";
			}

			var log2 = header.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_Reference = CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms;
				log2.SL_SE_NKEvent = Events.FrenchCustomsMessageStatus.Code;
				log2.SL_EventTime = ZDateTime.Now.AddDays(-1);
				var errorContextItem = log2.SourceInfoItems.AddNew();
				errorContextItem.Key = "Error";
				errorContextItem.Data = "Error Description 2";
			}

			AssertNoWarning("No warning to expect when BH_MessageStatus is not REJ.", header.BH_MessageStatusInfo, "Error Description");

			header.BH_MessageStatus = EDIMessageStatusList.Codes.Rejected;
			AssertHasWarning("Warning should match most recent event.", header.BH_MessageStatusInfo, "Error Description");
		}

		public void TestCheckDestinationCustomsOfficeCodeForArrival()
		{
			var error1 = "This Customs office doesn't match the office set in Declarant ACE authorization matching Destination Trader address.";
			var error2 = "No Declarant ACE authorization matching Destination Trader address with OFC rule could be found.";

			CombineAssertions(() =>
			{
				AssertMessageError("No MessageError is expected when MovementType is Departure.", isMovementTypeArrival: false);
				AssertMessageError("No MessageError is expected for Phase5 NCTSHeader.", isPhase4: false);
				AssertMessageError("No MessageError is expected when IsSimplifiedNctsProcedure is false.", isSimplified: false);
				AssertMessageError("No MessageError is expected when Declarant ACE authorization with OFC rule exists and matches with DestinationCustomsOfficeCodeForArrival.");
				AssertMessageError("MessageError is expected when Declarant ACE authorization exist with OFC rule but does not matches with DestinationCustomsOfficeCodeForArrival", isActualValueMatchesWithCalculatedValue: false, errorExists: true, messageError: error1);
				AssertMessageError("MessageError is expected when no authorization exist with OFC rule", isAuthorizationWithOFCRuleExists: false, errorExists: true, messageError: error2);
			});

			void AssertMessageError(string comment, bool isMovementTypeArrival = true, bool isPhase4 = true, bool isSimplified = true, bool isAuthorizationWithOFCRuleExists = true, bool isActualValueMatchesWithCalculatedValue = true, bool errorExists = false, string messageError = "")
			{
				var header = Factory.New<NctsHeader>();

				header.SetMovementType(isMovementTypeArrival ? NctsMovementType.Codes.Arrival : NctsMovementType.Codes.Departure);
				header.BH_ApplicationCode = isPhase4 ? CusInBondApplicationCodeList.Codes.NCTS4 : CusInBondApplicationCodeList.Codes.NCTS5;

				if (isMovementTypeArrival)
				{
					header.ArrivalMovementHeader.IsSimplifiedNctsProcedure = isSimplified;
				}
				else
				{
					header.MovementHeader.IsSimplifiedNctsProcedure = isSimplified;
				}

				var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
				destinationTrader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsOfficeForTransit, "FR001111", Enterprise.Core.Constants.CountryCodes.France);
				header.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

				var declarant = Factory.NewWithValidTestData<OrgHeader>();

				var address = header.DestinationTrader.Address;
				var authorisation = declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit).WithNumber("DECLARANT_AUT").WithCountry(header.CountryCode);
				authorisation.CPH_OA_AppliesTo = address.PK;
				authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);

				var declarationAuthorisationRule = authorisation.CusAuthorisationRules.AddNew();
				declarationAuthorisationRule.CPR_RuleCode = isAuthorizationWithOFCRuleExists ? CusAuthorisationRuleTypeList.Codes.OFC : CusAuthorisationRuleTypeList.Codes.PCD;
				declarationAuthorisationRule.CPR_ValueFrom = "FR003333";

				header.Declarant.E2_OA_Address = declarant.MainAddress.PK;

				header.DestinationCustomsOfficeCodeForArrival = isActualValueMatchesWithCalculatedValue ? header.DestinationCustomsOfficeCodeForArrival : "FR123456";

				header.Validation.ValidateDestinationCustomsOfficeCodeForArrival();

				if (errorExists)
				{
					AssertHasMessageErrorContaining(comment, header.DestinationCustomsOfficeCodeForArrivalInfo, messageError);
				}
				else
				{
					AssertNoMessageErrorContaining(comment, header.DestinationCustomsOfficeCodeForArrivalInfo, error1);
					AssertNoMessageErrorContaining(comment, header.DestinationCustomsOfficeCodeForArrivalInfo, error2);
				}
			}
		}

		public void TestCheckDestinationCustomsOfficeCodeForArrivalCore()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "";
			AssertNoMessageErrorContaining(nctsHeader.DestinationCustomsOfficeCodeForArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "";
			AssertHasMessageErrorContaining(nctsHeader.DestinationCustomsOfficeCodeForArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ABC";
			AssertNoMessageErrorContaining(nctsHeader.DestinationCustomsOfficeCodeForArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "";
			AssertHasMessageErrorContaining(nctsHeader.DestinationCustomsOfficeCodeForArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ABC";
			AssertNoMessageErrorContaining(nctsHeader.DestinationCustomsOfficeCodeForArrivalInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckArrivalMrnFromUser()
		{
			AssertFormatForArrivalMovement(NctsMovementType.Codes.Arrival);
			AssertFormatForArrivalMovement(NctsMovementType.Codes.DepartureAndArrival);

			var headerForDeparture = Factory.New<NctsHeader>();
			headerForDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			headerForDeparture.ArrivalMrnFromUser = ZString.Empty;
			AssertNoMessageErrorContaining("ArrivalMrnFromUser is not mandatory for departure.", headerForDeparture.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);

			void AssertFormatForArrivalMovement(string nctsMovementType)
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(nctsMovementType);
				CombineAssertions(() =>
				{
					header.ArrivalMrnFromUser = ZString.Empty;
					AssertHasMessageErrorContaining("Message error should be added when ArrivalMrnFromUser is empty.", header.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);

					header.ArrivalMrnFromUser = "111111111111111111";
					AssertHasMessageError("Message error should be added when ArrivalMrnFromUser is not entered to the format.", header.ArrivalMrnFromUserInfo, "You have not entered a valid MRN number. The valid format should be 2 digits + 2 letters of country code + 14 letters/digits.");

					header.ArrivalMrnFromUser = "11XX1234567890ABCD";
					AssertHasMessageError("Message error should be added when ArrivalMrnFromUser is not entered to the format(XX is not a country code).", header.ArrivalMrnFromUserInfo, "You have not entered a valid MRN number. The valid format should be 2 digits + 2 letters of country code + 14 letters/digits.");

					header.ArrivalMrnFromUser = "11FR1234567890ABCD";
					AssertNoMessageErrors("No message error should be added when ArrivalMrnFromUser is valid in expected format.", header.ArrivalMrnFromUserInfo);
				});
			}
		}

		public void TestCheckBH_OH_CarrierIsLinkedToBH_FTZMove()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.BH_FTZMove = false;
			header.BH_OH_Carrier = ZGuid.Empty;
			AssertNoMessageErrorContaining(header.BH_OH_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_FTZMove = true;
			header.BH_OH_Carrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.BH_OH_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_OH_Carrier = org.PK;
			AssertNoMessageErrorContaining(header.BH_OH_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBH_OH_CarrierMustHaveanEORI()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			header.BH_FTZMove = false;
			header.BH_OH_Carrier = org.PK;
			AssertNoMessageErrorContaining(header.BH_OH_CarrierInfo, "Your Security Carrier has no EORI Code.");

			header.BH_FTZMove = true;
			header.BH_OH_Carrier = org.PK;
			AssertHasMessageErrorContaining(header.BH_OH_CarrierInfo, "Your Security Carrier has no EORI Code.");

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "12345645";

			header.BH_OH_Carrier = org.PK;
			AssertNoMessageErrorContaining(header.BH_OH_CarrierInfo, "Your Security Carrier has no EORI Code.");
		}

		public void TestCheckBH_RL_NKImportLoadPort_MustBeFiled()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.PortOfDispatch = "FRPAR";
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, "Either Dispatch Country in Goods tab or Port of Dispatch in Declaration must be filled.");
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, "Dispatch Country must be filled in Goods tab or Port of Dispatch must be filled in Declaration tab but not both.");

			header.PortOfDispatch = "";
			AssertHasMessageError(header.BH_RL_NKImportLoadPortInfo, "Either Dispatch Country in Goods tab or Port of Dispatch in Declaration must be filled.");
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, "Dispatch Country must be filled in Goods tab or Port of Dispatch must be filled in Declaration tab but not both.");

			var goodItem = header.MovementHeader.GoodsItems.AddNew();
			goodItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
			header.Validation.ValidateBH_RL_NKImportLoadPort();
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, "Either Dispatch Country in Goods tab or Port of Dispatch in Declaration must be filled.");
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, "Dispatch Country must be filled in Goods tab or Port of Dispatch must be filled in Declaration tab but not both.");

			header.PortOfDispatch = "FRPAR";
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, "Either Dispatch Country in Goods tab or Port of Dispatch in Declaration must be filled.");
			AssertHasMessageError(header.BH_RL_NKImportLoadPortInfo, "Dispatch Country must be filled in Goods tab or Port of Dispatch must be filled in Declaration tab but not both.");
		}

		public void TestCheckNoOrMultipleGuaranteesErrorOnPrincipalAndDeclarant()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var companyOrgProxy = CreateOrgHeader("COMPANYPROXY");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var nctsConfigurationMock = SetupNctsConfigurationMock();
			var nctsConfiguration = new KeyObjectHandleDictionaryObject { { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, nctsConfigurationMock.Object } };

			using (ObjectFactory.Substitute("NCTS.NctsConfiguration", nctsConfiguration))
			{
				nctsHeader.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
				nctsHeader.Principal.Validation.ValidateOrganisationPK();
				nctsHeader.Declarant.Validation.ValidateOrganisationPK();

				AssertHasMessageError("No Guarantee found, error on Principal", nctsHeader.Principal.OrganisationPKInfo, "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee.");
				AssertHasMessageError("No Guarantee found, error on Declarant", nctsHeader.Declarant.OrganisationPKInfo, "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee.");

				var guaranteeHeader1 = CreateGuaranteeHeader("19860101", EUGuaranteeTypeList.Codes.COD);
				var refresher = nctsHeader.GuaranteeRefresher;
				guaranteeHeader1.CPH_OH_PermitHolder = companyOrgProxy.PK;
				refresher.PopulateGuaranteeWithFallbacks();

				AssertEquals("One Guarantee found", "19860101", nctsHeader.GetEffectiveGuarantees().Single().PW_BondNumber);
				AssertNoErrorContaining("One Guarantee found, No error on Principal", nctsHeader.Principal.OrganisationPKInfo, "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee.");
				AssertNoErrorContaining("One Guarantee found, No error on Declarant", nctsHeader.Declarant.OrganisationPKInfo, "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee.");

				var guaranteeHeader2 = CreateGuaranteeHeader("19860102", EUGuaranteeTypeList.Codes.COD);
				guaranteeHeader2.CPH_OH_PermitHolder = companyOrgProxy.PK;
				refresher.PopulateGuaranteeWithFallbacks();
				nctsHeader.Principal.Validation.ValidateOrganisationPK();
				nctsHeader.Declarant.Validation.ValidateOrganisationPK();

				AssertHasMessageErrorContaining("Multiple Guarantees found", nctsHeader.Principal.OrganisationPKInfo, "There are more than 1 guarantee found for Principal, Declarant or Organization Proxy");
				AssertHasMessageErrorContaining("Multiple Guarantees found", nctsHeader.Declarant.OrganisationPKInfo, "There are more than 1 guarantee found for Principal, Declarant or Organization Proxy");
			}

			AssertNoErrorContaining("UseGuaranteeGridValidationCore disabled", nctsHeader.Declarant.OrganisationPKInfo, "There are more than 1 guarantee found for Principal, Declarant or Organization Proxy");

			Mock<NctsConfiguration> SetupNctsConfigurationMock()
			{
				var nctsConfigurationMock = new Mock<NctsConfiguration>() { CallBase = true };
				nctsConfigurationMock.Protected().Setup<ZBool>("UseDeclarantFallBackCore").Returns(true);
				nctsConfigurationMock.Protected().Setup<ZBool>("UseBranchOrgProxyFallBackCore").Returns(true);
				nctsConfigurationMock.Protected().Setup<ZBool>("UseCompanyOrgProxyFallBackCore").Returns(true);
				nctsConfigurationMock.Protected().Setup<ZBool>("UseGuaranteeGridValidationCore").Returns(true);
				nctsConfigurationMock.Protected().Setup<ZBool>("ClearExistingGuaranteesConfigurationCore").Returns(true);
				return nctsConfigurationMock;
			}
		}

		OrgHeader CreateOrgHeader(ZString code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = code;
			const string regNo1 = "12345";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, regNo1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return orgHeader;
		}

		CusGuaranteeHeader CreateGuaranteeHeader(ZString guaranteeNumber, ZString guaranteeType, string guaranteeSubType = "1")
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeNumber;
			guaranteeHeader.CPH_Type = guaranteeType;
			guaranteeHeader.CPH_SubType = guaranteeSubType;
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#1";
			return guaranteeHeader;
		}
	}
}
