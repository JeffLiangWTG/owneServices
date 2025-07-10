using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMessageType()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.MessageTypeInfo);
			});
		}

		public void TestReleaseRequest()
		{
			CombineAssertions(() =>
			{
				sendingObject.MessageType = NCTS5DeparturePhaseList.Codes.ReleaseRequest;
				ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.ReleaseRequestInfo);
			});
		}

		public void TestCheckRuleTR0020()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEANR100", "ANTWERP PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEZEE100", "ZEEBRUGGE PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "XXX");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEGEN100", "GENT PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "XXX");
			Factory.Save();

			sendingObject.NctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			using (var deciderTestContext = new NctsHeaderMessageSendingObjectValidationDeciderTestContext<INctsHeaderMessageSendingObjectValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(x => x.IsRuleTR0020Active);
				sendingObject.ActualOfficeOfDestination = "BEZEE100";
				AssertNoMessageError("When rule TR0020 is disabled and seleting an invalid office",
					sendingObject.ActualOfficeOfDestinationInfo,
					"Selected Customs Office has not the role DES. Select another one");

				deciderTestContext.EnableRule(x => x.IsRuleTR0020Active);
				CombineAssertions("When rule TR0020 is Enabled", () =>
				{
					sendingObject.ActualOfficeOfDestination = "BEZEE100";
					AssertHasMessageError("When selecting an invalid office", sendingObject.ActualOfficeOfDestinationInfo, "Selected Customs Office has not the role DES. Select another one");
					sendingObject.ActualOfficeOfDestination = "BEANR100";
					AssertNoMessageError("When seleting a correct office", sendingObject.ActualOfficeOfDestinationInfo, "Selected Customs Office has not the role DES. Select another one");
				});
			}
		}

		public void TestCheckRuleTR0021()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var error = "[TR0021] Actual consignee or actual office of destination must be entered if query information is filled in. ";
			using (var deciderTestContext = new NctsHeaderMessageSendingObjectValidationDeciderTestContext<INctsHeaderMessageSendingObjectValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(x => x.IsRuleTR0021Active);
				sendingObject.QueryInformation = "Some query info";
				sendingObject.ActualOfficeOfDestination = "";
				AssertNoMessageError("When rule TR0021 is disabled and query info has been entered but no actual destination office or actual consignee", sendingObject.ActualOfficeOfDestinationInfo, error);

				deciderTestContext.EnableRule(x => x.IsRuleTR0021Active);
				CombineAssertions("When rule TR0021 is Enabled", () =>
				{
					sendingObject.QueryInformation = "Some query info";
					sendingObject.ActualOfficeOfDestination = "";
					AssertHasMessageError("When query info has been entered but no actual destination office or actual consignee", sendingObject.ActualOfficeOfDestinationInfo, error);
					sendingObject.ActualOfficeOfDestination = "BEANR100";
					AssertNoMessageError("When query info and actual destination office are entered but no actual consignee", sendingObject.ActualOfficeOfDestinationInfo, error);
					sendingObject.ActualConsignee.OrganisationPK = orgHeader.PK;
					sendingObject.ActualOfficeOfDestination = ZString.Empty;
					AssertNoMessageError("When query info and actual consignee are entered but no actual departure office", sendingObject.ActualOfficeOfDestinationInfo, error);
				});
			}
		}

		public void TestCheckAdditionalText()
		{
			const string error = "[C0220] You have not entered an Additional Text.";

			using (var deciderTestContext = new NctsHeaderMessageSendingObjectValidationDeciderTestContext<INctsHeaderMessageSendingObjectValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(c => c.IsRuleC0220Active);
				sendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1);
				sendingObject.AdditionalText = ZString.Empty;
				AssertNoMessageError("When Rule C0220 is disabled", sendingObject.AdditionalTextInfo, error);

				deciderTestContext.EnableRule(c => c.IsRuleC0220Active);
				CombineAssertions("When Rule C0220 is enabled", () =>
				{
					sendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1);
					sendingObject.AdditionalText = ZString.Empty;
					AssertHasMessageError(sendingObject.AdditionalTextInfo, error);

					sendingObject.AdditionalText = "hello";
					AssertNoMessageError(sendingObject.AdditionalTextInfo, error);
				});
			}
		}

		public void TestCheckRuleC0315()
		{
			const string error = "[C0315] You have not entered Customs Office Of Destination Actual.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEANR100", "ANTWERP PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEZEE100", "ZEEBRUGGE PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "XXX");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEGEN100", "GENT PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, "XXX");
			Factory.Save();

			sendingObject.NctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var deciderTestContext = new NctsHeaderMessageSendingObjectValidationDeciderTestContext<INctsHeaderMessageSendingObjectValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(x => x.IsRuleC0315Active);
				sendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1);
				sendingObject.ActualOfficeOfDestination = ZString.Empty;
				AssertNoMessageError("When rule C0315 is not active", sendingObject.ActualOfficeOfDestinationInfo, error);

				deciderTestContext.EnableRule(x => x.IsRuleC0315Active);
				CombineAssertions("When rule C0315 is active", () =>
				{
					sendingObject.TC11DeliveryDate = new ZDateTime(2023, 8, 1);
					sendingObject.ActualOfficeOfDestination = ZString.Empty;
					AssertHasMessageError("When selecting an invalid office", sendingObject.ActualOfficeOfDestinationInfo, error);
					sendingObject.TC11DeliveryDate = ZDateTime.Empty;
					sendingObject.Validation.ValidateActualOfficeOfDestination();
					AssertNoMessageError("When seleting a correct office", sendingObject.ActualOfficeOfDestinationInfo, error);
				});
			}
		}

		public void TestCheckCountryCodeSameAsCurrentCompanyForNctsPhase5Header_Departure()
		{
			var messageError = ValidationMessages.CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode;

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var departureCustomsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: true))
			{
				departureCustomsOffice.CY_Data = "LV000001";
				sendingObject.MessageType = "AAA";
				AssertNoError("When country code of the NCTS departure office is same as the job's NCTS country code (login country)", sendingObject.MessageTypeInfo, messageError);

				departureCustomsOffice.CY_Data = "FR000001";
				sendingObject.MessageType = "BBB";
				AssertHasError("When country code of the NCTS departure office is NOT same as the job's NCTS country code (login country)", sendingObject.MessageTypeInfo, messageError);
			}

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: false))
			{
				departureCustomsOffice.CY_Data = "LV000001";
				sendingObject.MessageType = "AAA";
				AssertNoError("When country code of NCTS departure office is NOT required to be same as the job's NCTS country code", sendingObject.MessageTypeInfo, messageError);
				departureCustomsOffice.CY_Data = "FR000001";
				sendingObject.MessageType = "BBB";
				AssertNoError("When country code of NCTS departure office is NOT required to be same as the job's NCTS country code", sendingObject.MessageTypeInfo, messageError);
			}
		}

		public void TestCheckCountryCodeSameAsCurrentCompanyForNctsPhase5Header_Arrival()
		{
			var messageError = ValidationMessages.CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var arrivalCustomsOffice = arrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: true))
			{
				arrivalCustomsOffice.CY_Data = "LV000001";
				sendingObject.MessageType = "AAA";
				AssertNoError("When country code of the NCTS arrival office is same as the job's NCTS country code (login country)", sendingObject.MessageTypeInfo, messageError);

				arrivalCustomsOffice.CY_Data = "FR000001";
				sendingObject.MessageType = "BBB";
				AssertHasError("When country code of the NCTS arrival office is NOT same as the job's NCTS country code (login country)", sendingObject.MessageTypeInfo, messageError);
			}

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: false))
			{
				arrivalCustomsOffice.CY_Data = "LV000001";
				sendingObject.MessageType = "AAA";
				AssertNoError("When country code of NCTS arrival office is NOT required to be same as the job's NCTS country code", sendingObject.MessageTypeInfo, messageError);
				arrivalCustomsOffice.CY_Data = "FR000001";
				sendingObject.MessageType = "BBB";
				AssertNoError("When country code of NCTS arrival office is NOT required to be same as the job's NCTS country code", sendingObject.MessageTypeInfo, messageError);
			}
		}

		public void TestCheckCountryCodeSameAsCurrentCompanyForNctsPhase4Header()
		{
			var messageError = ValidationMessages.CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode;

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.CustomsOffices.RemoveAndDeleteAll();

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var departureCustomsOffice = nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: true))
			{
				departureCustomsOffice.CY_Data = "LV000001";
				sendingObject.MessageType = "AAA";
				AssertNoError("When country code of the NCTS departure office is same as the job's NCTS country code (login country)", sendingObject.MessageTypeInfo, messageError);

				departureCustomsOffice.CY_Data = "FR000001";
				sendingObject.MessageType = "BBB";
				AssertHasError("When country code of the NCTS departure office is NOT same as the job's NCTS country code (login country)", sendingObject.MessageTypeInfo, messageError);
			}

			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany), value: false))
			{
				departureCustomsOffice.CY_Data = "LV000001";
				sendingObject.MessageType = "AAA";
				AssertNoError("When country code of NCTS departure office is NOT required to be same as the job's NCTS country code", sendingObject.MessageTypeInfo, messageError);

				departureCustomsOffice.CY_Data = "FR000001";
				sendingObject.MessageType = "BBB";
				AssertNoError("When country code of NCTS departure office is NOT required to be same as the job's NCTS country code", sendingObject.MessageTypeInfo, messageError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		}
		NctsHeader nctsHeader;
		NctsHeaderMessageSendingObject sendingObject;
	}
}
