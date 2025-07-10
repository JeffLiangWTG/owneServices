using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class NCTSTestHelper : TestCaseWithFactory
	{
		public static void AssertJobDocAddress(JobDocAddress jda, DocAddressType expectedType, string suffix = "0", string countryCode = "GB", string traderTin = "012345678900", string traderTir = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", expectedType, jda.DocAddressType);
				AssertEquals("OrgHeader.OH_FullName", "Oscorp Industries" + suffix, jda.Address.Header.OH_FullName);
				AssertEquals("JobDocAddress.E2_Address1", "Street and No" + suffix, jda.E2_Address1);
				AssertEquals("JobDocAddress.E2_City", "Milton Keynes" + suffix, jda.E2_City);
				AssertEquals("JobDocAddress.E2_Postcode", "MK16 XX" + suffix, jda.E2_Postcode);
				AssertEquals("OrgAddress.OA_RL_NKRelatedPortCode", countryCode + "XX" + suffix, jda.Address.OA_RL_NKRelatedPortCode);
				AssertEquals("OrgHeader EORI", countryCode + traderTin + suffix, EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(jda.Organisation, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
				if (!string.IsNullOrEmpty(traderTir))
				{
					AssertEquals("OrgHeader TIR", traderTir, EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(jda.Organisation, OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers));
				}
			});
		}

		public static void SetupC0009ForEuAndCtCountries(BusinessObjectFactory factory)
		{
			SetupC0009ForCountries(factory, factory.GetEuropeanUnionAndCtCountries().ToArray());
		}

		public static void SetupC0009ForCountries(BusinessObjectFactory factory, params string[] countries)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009 Desc");
			foreach (var country in countries)
			{
				helper.CreateNewOrGetExistingDataGrouping(country, parent: eun);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, country, country, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			factory.Save();
		}

		public static void SetMrnForTest(NctsHeader header, ZString mrn)
		{
			CusEntryNumber.New(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.Company.GC_RN_NKCountryCode).CE_EntryNum = mrn;
		}

		public static NctsDepartureHeaderContainer[] SetupContainersAndSealsForTest(NctsHeader departure)
		{
			return new[]
			{
				AddContainerAndSealsForTest(departure, "CONTAINER1", "SEAL1", "SEAL2", "SEAL3", "SEAL4"),
				AddContainerAndSealsForTest(departure, "CONTAINER2", "SEAL3", "", "", ""),
				AddContainerAndSealsForTest(departure, "CONTAINER3", "", "", "", "")
			};
		}

		public static NctsDepartureHeaderContainer AddContainerAndSealsForTest(NctsHeader departure, ZString containerNumber, ZString seal1, ZString seal2, ZString seal3, ZString seal4)
		{
			var container1 = departure.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = containerNumber;
			container1.BC_Seal1 = seal1;
			container1.BC_Seal2 = seal2;

			if (!seal3.IsEmpty)
			{
				var additionalSeal1 = container1.AdditionalSeals.AddNew();
				additionalSeal1.BK_SealNumber = seal3;
				additionalSeal1.BK_SequenceNumber = 3;
				additionalSeal1.BK_UnloadingState = "NEW";
			}

			if (!seal4.IsEmpty)
			{
				var additionalSeal2 = container1.AdditionalSeals.AddNew();
				additionalSeal2.BK_SealNumber = seal4;
				additionalSeal2.BK_SequenceNumber = 4;
				additionalSeal2.BK_UnloadingState = "NEW";
			}

			return container1;
		}

		public static void CreateEventsAndIncidents(NctsHeader arrival)
		{
			var enRouteEvent1 = arrival.EnRouteTransshipments.AddNew();
			enRouteEvent1.BN_EventPlace = "PLACE1";
			var transhipmentContainer1 = enRouteEvent1.Containers.AddNew();
			transhipmentContainer1.BC_ContainerNum = "A";
			transhipmentContainer1.BC_Seal1 = "A1";
			transhipmentContainer1.BC_Seal2 = "A2";
			var transhipmentContainer2 = enRouteEvent1.Containers.AddNew();
			transhipmentContainer2.BC_ContainerNum = "B";
			transhipmentContainer2.BC_Seal1 = "B1";
			transhipmentContainer2.BC_Seal2 = "B2";

			var enRouteIncident1 = arrival.EnRouteIncidents.AddNew();
			enRouteIncident1.BN_EventPlace = "PLACE1";
			var incidentContainer1 = enRouteIncident1.IncidentContainers.AddNew();
			incidentContainer1.BC_ContainerNum = "C";
			incidentContainer1.BC_Seal1 = "C1";
			incidentContainer1.BC_Seal2 = "C2";
			var incidentContainer2 = enRouteIncident1.IncidentContainers.AddNew();
			incidentContainer2.BC_ContainerNum = "D";
			incidentContainer2.BC_Seal1 = "D1";
			incidentContainer2.BC_Seal2 = "D2";

			var enRouteEvent2 = arrival.EnRouteTransshipments.AddNew();
			enRouteEvent2.BN_EventPlace = "PLACE2";
			var transhipmentContainer3 = enRouteEvent2.Containers.AddNew();
			transhipmentContainer3.BC_ContainerNum = "E";
			transhipmentContainer3.BC_Seal1 = "E1";
			transhipmentContainer3.BC_Seal2 = "E2";
			var transhipmentContainer4 = enRouteEvent2.Containers.AddNew();
			transhipmentContainer4.BC_ContainerNum = "F";
			transhipmentContainer4.BC_Seal1 = "F1";
			transhipmentContainer4.BC_Seal2 = "F2";

			var enRouteIncident2 = arrival.EnRouteIncidents.AddNew();
			enRouteIncident2.BN_EventPlace = "PLACE2";
			var incidentContainer3 = enRouteIncident2.IncidentContainers.AddNew();
			incidentContainer3.BC_ContainerNum = "G";
			incidentContainer3.BC_Seal1 = "G1";
			incidentContainer3.BC_Seal2 = "G2";
			var incidentContainer4 = enRouteIncident2.IncidentContainers.AddNew();
			incidentContainer4.BC_ContainerNum = "H";
			incidentContainer4.BC_Seal1 = "H1";
			incidentContainer4.BC_Seal2 = "H2";
		}

		public static NctsContainer AddContainerForTest(NctsArrivalAndUnloadingCargoDesc item, ZString containerNumber, ZString seal1, ZString seal2)
		{
			var container = item.Containers.AddNew();
			container.ContainerNumber = containerNumber;
			container.BC_Seal1 = seal1;
			container.BC_Seal2 = seal2;
			return container;
		}

		public static GlbBranch MakeDoverBranchForTest(BusinessObjectFactory factory)
		{
			var ukCompany = factory.New<GlbCompany>();
			ukCompany.GC_Code = "OSC";
			ukCompany.GC_Name = "OsCorp Industries Ltd.";
			ukCompany.GC_RN_NKCountryCode = "GB";
			var ukBranch1 = ukCompany.Branches.AddNew();
			ukBranch1.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			ukBranch1.GB_Code = "DVR";
			ukBranch1.GB_City = "Dover";
			factory.Save();
			return ukBranch1;
		}

		public static void SetupGoodsItemsForTest(NctsHeader departure, bool addContainerPivot = true)
		{
			if (addContainerPivot)
			{
				SetupContainersAndSealsForTest(departure);
			}

			var goodsItem1 = departure.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 10;
			goodsItem1.BY_NetWeight = 1;
			goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem1.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;

			if (addContainerPivot)
			{
				SetContainerPivotForTest(goodsItem1, 0);
				SetContainerPivotForTest(goodsItem1, 1);
				SetContainerPivotForTest(goodsItem1, 2);
			}

			SetupPackageForTest(goodsItem1, "MARK1", "BX", 10L);
			SetupPackageForTest(goodsItem1, "MARK2", "CT", 20L);

			var goodsItem2 = departure.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_GrossWeight = 20;
			goodsItem2.BY_NetWeight = 2;
			goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem2.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;

			if (addContainerPivot)
			{
				SetContainerPivotForTest(goodsItem2, 1);
			}

			var goodsItem3 = departure.MovementHeader.GoodsItems.AddNew();
			goodsItem3.BY_GrossWeight = 30;
			goodsItem3.BY_NetWeight = 3;
			goodsItem3.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem3.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;

			SetupPackageForTest(goodsItem3, "MARK3", "BX", 30L);
			SetupPackageForTest(goodsItem3, "MARK4", "CT", 40L);
		}

		public static void SetupPackageForTest(NctsCommonCargoDesc goodsItem, ZString marksAndNumbers, ZString packageType, ZLong packageCount)
		{
			var package = goodsItem.Packages.AddNew();
			package.B5_MarksAndNumbers = marksAndNumbers;
			package.B5_UnitType = packageType;
			package.B5_UnitCount = packageCount;
		}

		public static void SetContainerPivotForTest(NctsDepartureCargoDesc goodsItem, int containerIndex)
		{
			goodsItem.ContainersPivots[containerIndex].Container.BC_Mode = "CNT";
			goodsItem.ContainersPivots[containerIndex].ContainerSelected = true;
		}

		public static NctsEuOfficeCode CreateCustomsOfficeForTest(NctsHeader nctsHeader, string officePurpose, string officeCode, ZDateTime arrivalTime, bool clearOffices = false)
		{
			NctsEuOfficeCode office = null;
			if (clearOffices)
			{
				nctsHeader.CustomsOffices.RemoveAndDeleteAll();
			}
			else
			{
				if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination && nctsHeader.IsDepartureMovement)
				{
					office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				}
				else if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture && nctsHeader.IsDepartureMovement)
				{
					office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				}
			}

			if (office == null)
			{
				office = nctsHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}

			office.CY_Data = officeCode;
			office.CY_Date = arrivalTime;

			var dataGroupingCode = officeCode.Substring(0, 2);
			var factory = nctsHeader.Factory;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurpose });
			nctsHeader.Factory.Save();
			return office;
		}

		public static NctsEuOfficeCode CreateCustomsOfficeForTest(NctsCommonMovementHeader movementHeader, string officePurpose, string officeCode, ZDateTime arrivalTime, bool clearOffices = false)
		{
			NctsEuOfficeCode office = null;
			if (clearOffices)
			{
				movementHeader.CustomsOffices.RemoveAndDeleteAll();
			}
			else
			{
				if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination)
				{
					office = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				}
				else if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)
				{
					office = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				}
			}

			if (office == null)
			{
				office = movementHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}

			office.CY_Data = officeCode;
			office.CY_Date = arrivalTime;

			var dataGroupingCode = officeCode.Substring(0, 2);
			var factory = movementHeader.Factory;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurpose });
			factory.Save();
			return office;
		}

		public static void AddItineraryCountryForTest(NctsHeader nctsHeader, string country)
		{
			var itineraryCountry = nctsHeader.Itinerary.AddNew();
			itineraryCountry.CountryCode = country;
		}

		public static void ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(BusinessObjectFactory factory, JobDocAddress jobDocAddress, bool hasMessageError)
		{
			const string messageError = "[C0505] You have not entered a Post Code.";
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "PC1", jobDocAddress, ZString.Empty, traderTin: ZString.Empty, postCode: ZString.Empty, countryCode: Core.Constants.CountryCodes.Poland);

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0505Active));
				jobDocAddress.Address.Country.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(AssertionMessage(), jobDocAddress.OrganisationPKInfo, messageError);

				jobDocAddress.Address.Country.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
				jobDocAddress.Validation.ValidateOrganisationPK();
				DecideWhetherContainingMessageError(AssertionMessage(), jobDocAddress.OrganisationPKInfo, messageError, hasMessageError);

				jobDocAddress.Address.OA_PostCode = "2123";
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(AssertionMessage(), jobDocAddress.OrganisationPKInfo, messageError);

				jobDocAddress.Address.OA_PostCode = ZString.Empty;
				jobDocAddress.Validation.ValidateOrganisationPK();
				DecideWhetherContainingMessageError(AssertionMessage(), jobDocAddress.OrganisationPKInfo, messageError, hasMessageError);

				jobDocAddress.E2_AddressOverride = ZBool.True;
				jobDocAddress.E2_Postcode = "2123";
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(AssertionMessage(), jobDocAddress.OrganisationPKInfo, messageError);

				jobDocAddress.E2_Postcode = ZString.Empty;
				jobDocAddress.Validation.ValidateOrganisationPK();
				DecideWhetherContainingMessageError(AssertionMessage(), jobDocAddress.OrganisationPKInfo, messageError, hasMessageError);
			}

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(factory))
			{
				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0505Active));
				jobDocAddress.Address.Country.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("C0505 inactive", jobDocAddress.OrganisationPKInfo, messageError);
			}

			string AssertionMessage() => $"RN_PostcodeValidationRule={jobDocAddress.Address.Country.RN_PostcodeValidationRule} OA_PostCode={jobDocAddress.Address.OA_PostCode} E2_AddressOverride={jobDocAddress.E2_AddressOverride} E2_Postcode={jobDocAddress.E2_Postcode}";
		}

		static void DecideWhetherContainingMessageError(string assertionMessage, ZPropertyInfo info, ZString message, bool hasMessageError)
		{
			if (hasMessageError)
			{
				AssertHasMessageErrorContaining(assertionMessage, info, message);
			}
			else
			{
				AssertNoMessageErrorContaining(assertionMessage, info, message);
			}
		}

		public static OrgHeader CreateJobDocAddressForTest(BusinessObjectFactory factory, string traderId, JobDocAddress jda, string suffix = "0", string traderName = "Oscorp Industries", string address1 = "Street and No",
			string postCode = "MK16 XX", string city = "Milton Keynes", string relatedPortCode = "GBXX", string countryCode = "GB", string traderTin = "012345678900", string traderTir = "", string phoneNumber = "+441234567890", Action<OrgHeader> configureOrgHeaderAction = null,
			string contactName = "", string contactPhone = "", string contactEmail = "", string contactAllocation = "")
		{
			var orgAddress = CreateOrgAddressForTest(factory, traderId, suffix, traderName, address1, postCode, city, relatedPortCode, countryCode, traderTin, traderTir, phoneNumber, configureOrgHeaderAction, contactName, contactPhone, contactEmail, contactAllocation);
			jda.E2_OA_Address = orgAddress.PK;
			return orgAddress.Header;
		}

		public static OrgAddress CreateOrgAddressForTest(BusinessObjectFactory factory, string traderId, string suffix = "0", string traderName = "Oscorp Industries", string address1 = "Street and No",
			string postCode = "MK16 XX", string city = "Milton Keynes", string relatedPortCode = "GBXX", string countryCode = "GB", string traderTin = "012345678900", string traderTir = "", string phoneNumber = "+441234567890", Action<OrgHeader> configureOrgHeaderAction = null,
			string contactName = "", string contactPhone = "", string contactEmail = "", string contactAllocation = "")
		{
			var traderOrgHeader = CreateOrgHeaderForTest(factory, traderName + suffix, traderId, relatedPortCode);
			configureOrgHeaderAction?.Invoke(traderOrgHeader);
			if (traderTin != ZString.Empty)
			{
				CreateEoriForTest(traderOrgHeader, traderTin + suffix, countryCode);
			}
			if (traderTir != ZString.Empty)
			{
				traderOrgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, traderTir, countryCode);
			}
			if (contactName != ZString.Empty)
			{
				CreateContactForTest(traderOrgHeader, contactName, contactPhone, contactEmail, contactAllocation);
			}
			var address = traderOrgHeader.Addresses.AddNew();
			address.OA_Address1 = address1 + suffix;
			address.OA_City = city + suffix;
			address.OA_PostCode = postCode + suffix;
			address.OA_RL_NKRelatedPortCode = relatedPortCode + suffix;
			address.OA_RN_NKCountryCode = countryCode;
			address.OA_Phone = phoneNumber;
			return address;
		}

		public static OrgHeader CreateOrgHeaderForTest(BusinessObjectFactory factory, string traderName = "Oscorp Industries", string traderCode = "OSC", string relatedPortCode = "GBXXX")
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = traderCode;
			orgHeader.OH_FullName = traderName;
			orgHeader.OH_RL_NKClosestPort = relatedPortCode;
			return orgHeader;
		}

		public static void CreateContactForTest(OrgHeader orgHeader, string contactName, string contactPhoneNumber, string contactEmail, string allocation)
		{
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = contactPhoneNumber;
			contact.OC_Email = contactEmail;
			contact.Allocations.AddNew().PC_Type = allocation;
		}

		public static void CreateEoriForTest(OrgHeader traderOrgHeader, string traderTin, string countryCode)
		{
			traderOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, traderTin, countryCode);
		}

		public static LinkedCusAuthorisationRule CreateLinkedAuthorisationRuleForTest(CusAuthorisationRule authorisationRule, ZString ruleCode, ZString valueFrom)
		{
			var linkedCusAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			linkedCusAuthorisationRule.CPR_RuleCode = ruleCode;
			linkedCusAuthorisationRule.CPR_ValueFrom = valueFrom;
			return linkedCusAuthorisationRule;
		}

		public static OrgAddress SetupCarrierForTest(NctsHeader departure, string eoriNumber = "GB954131533000", string eoriCountry = Core.Constants.CountryCodes.UnitedKingdom, string contactName = "", string contactPhone = "", string contactEmail = "", string contactAllocation = "")
		{
			var carrier = departure.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "CIREQU");
			var carrierAddress = carrier.MainAddress;
			carrierAddress.CompanyName = "CARRIER1";
			carrierAddress.Address1 = "CARRIER STREET";
			carrierAddress.City = "CITY";
			carrierAddress.Postcode = "CMK1";
			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, eoriCountry);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "GBR/ABC1234", Core.Constants.CountryCodes.UnitedKingdom);
			if (contactName != ZString.Empty)
			{
				CreateContactForTest(carrier, contactName, contactPhone, contactEmail, contactAllocation);
			}
			departure.BH_OH_Carrier = carrier.PK;
			departure.Factory.Save();

			return carrierAddress;
		}

		public static NctsGuarantee[] SetupGuaranteesForTest(NctsHeader departure)
		{
			return new[]
			{
				CreateGuaranteeForTest(departure, "9", "12346789", "AAAAAAAAAA", "ABCD", "FR"),
				CreateGuaranteeForTest(departure, "1", "987654321", "BBBBB", "WXYZ", "BE")
			};
		}

		public static NctsGuarantee CreateGuaranteeForTest(NctsHeader header, ZString guaranteeType, ZString guaranteeReferenceNumber, ZString otherGuaranteeReference, ZString accessCode, ZString validaityLimitationOther)
		{
			var guarantee = header.IsPhase5Departure ? header.MovementHeader.Guarantees.AddNew() : header.Guarantees.AddNew();
			guarantee.PW_BondType = guaranteeType;
			guarantee.PW_BondNumber = guaranteeReferenceNumber;
			guarantee.PW_BondNumber2 = otherGuaranteeReference;
			guarantee.PW_Password = accessCode;
			guarantee.PW_ValidityLimitation = validaityLimitationOther;
			return guarantee;
		}

		public static void AssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertCaptionsAndFullDescription(resourceStringData, expectedCaption, expectedMediumCaption, expectedShortCaption, string.Empty);
		}

		public static void AssertCaptions(ZPropertyInfo propertyInfo, string multipleResourceKey, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			AssertCaptionsAndFullDescription(propertyInfo, multipleResourceKey, expectedCaption, expectedMediumCaption, expectedShortCaption, string.Empty);
		}

		public static void AssertCaptions(Type type, string propertyName, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(type, propertyName);
			AssertCaptionsAndFullDescription(resourceStringData, expectedCaption, expectedMediumCaption, expectedShortCaption, string.Empty);
		}

		public static void AssertCaptions(Type type, string propertyName, string multipleResourceKey, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(type, propertyName, string.IsNullOrEmpty(multipleResourceKey) ? null : new[] { multipleResourceKey });
			AssertCaptionsAndFullDescription(resourceStringData, expectedCaption, expectedMediumCaption, expectedShortCaption, string.Empty);
		}

		public static void AssertCaptionsAndFullDescription(ZPropertyInfo propertyInfo, string multipleResourceKey, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, multipleResourceKey);
			AssertCaptionsAndFullDescription(resourceStringData, expectedCaption, expectedMediumCaption, expectedShortCaption, expectedFullDescription);
		}

		public static void AssertCaptionsAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertCaptionsAndFullDescription(resourceStringData, expectedCaption, expectedMediumCaption, expectedShortCaption, expectedFullDescription);
		}

		static void AssertCaptionsAndFullDescription(ResourceStringData resourceStringData, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", expectedCaption, resourceStringData.Caption);
				AssertEquals("MediumCaption", expectedMediumCaption, resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
				AssertEquals("FullDescription", expectedFullDescription, resourceStringData.FullDescription);
			});
		}

		public const string TestTariffCode = "0304798000";
		public static void SetUpTariff(BusinessObjectFactory factory, string countrycode = Core.Constants.CountryCodes.Latvia, string tariffTypeCode = Universal.Constants.TariffTypes.Import)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countrycode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeCode);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "LIVE ANIMALS", compositeKey: "01", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "03", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", compositeKey: "01.03", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "0304", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Fish fillets and other fish meat (whether or not minced), fresh, chilled or frozen", compositeKey: "01.03..04", nomenclatureGroupType: "CN");

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "8001100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tin, not alloyed", compositeKey: "15.80..01.1");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "80", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "TIN AND ARTICLES THEREOF", compositeKey: "15.80", nomenclatureGroupType: "CN");
			var rateType1 = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var preference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
			var rateCode2 = helper.LoadOrCreateNewCusRateCode(factory, "SEC", rateType2.PK);

			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "ADD");
			var tradeGroup2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Latvia, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);

			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preference2.PK);
			var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate4 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preference1.PK);

			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ABCD");
			helper.CreateCusApplicability(rate4.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.05m, countrycode);
			helper.CreateTaxOrFee("ORD", 0.2m, countrycode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "ORD");

			factory.Save();
		}

		public static void SetUpTariff(BusinessObjectFactory factory, Dictionary<string, string> tariffCodeRateFormula)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			factory.Save();

			helper.CreateTaxOrFee("RID", 0.05m, Core.Constants.CountryCodes.Latvia);
			helper.CreateTaxOrFee("ORD", 0.2m, Core.Constants.CountryCodes.Latvia);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");

			var rateType = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode = helper.CreateCusRateCode(factory, "A00", rateType.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			foreach (var item in tariffCodeRateFormula)
			{
				var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, item.Key, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
				var rate = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, item.Value, preference.PK);
				helper.CreateCusApplicability(rate.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "RID");
				helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "ORD");
			}

			factory.Save();
		}

		public static CusGuaranteeHeader SetupGuarantee(OrgHeader org, string countryCode = "LV")
		{
			SetupC0009ForCountries(org.Factory, countryCode);
			var guaranteeHeader = org.Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = countryCode;
			guaranteeHeader.CPH_Number = "1234";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(20);
			guaranteeHeader.CPH_QtyValIndicator = Customs.Business.PermitQtyValIndicatorList.Codes.VAL;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_ApplicationCode = "GUA";
			guaranteeHeader.CPH_UnitOfMeasure = "EUR";

			return guaranteeHeader;
		}

		public static RefExchangeRate CreateExchangeRate(BusinessObjectFactory factory,  ZDecimal sellRate, ZString exCurrency)
		{
			var exchangeRate = factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = exCurrency;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate.RE_StartDate = ZDateTime.Today;
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate.RE_SellRate = sellRate;
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			return exchangeRate;
		}

		internal static void AssertIsReadOnlyWhenAttributeIsMissing(BusinessObjectFactory factory, string levelAttribute, ISupportingDocumentReadOnlyConditions provider, NctsSupportingDocument supportingDocument, Func<ISupportingDocumentReadOnlyConditions, bool> readOnly, string attributeName, bool readOnlyWhenCodeNotInList = true)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3)
				= CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, levelAttribute, factory, attributeName);

			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals($"TypeCode has attribute '{attributeName}' = 'Y'", false, readOnly(provider));

				supportingDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals($"TypeCode has attribute '{attributeName}' = 'N'", false, readOnly(provider));

				supportingDocument.CSI_Code = refCusCodeList3.ZZD_Code;
				AssertEquals($"TypeCode doesn't have attribute '{attributeName}'", true, readOnly(provider));

				supportingDocument.CSI_Code = ZString.Empty;
				AssertEquals("TypeCode is empty", true, readOnly(provider));

				supportingDocument.CSI_Code = "AAAA";
				AssertEquals("TypeCode is not in the list", readOnlyWhenCodeNotInList, readOnly(provider));
			});
		}

		internal static void AssertUnloadingRemarksReadOnlyForLockedDeclaration(NctsArrivalMovementHeader arrivalMovementHeader, Func<object, bool> readOnly, object parentObject)
		{
			arrivalMovementHeader.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalMovementHeader.Header.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovementHeader.Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, readOnly(parentObject));

				arrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				arrivalMovementHeader.Factory.Save();
				AssertEquals("Editable, even if declaration is accepted", false, readOnly(parentObject));

				var declarationConfig = new DeclarationLockConfig() { DeclarationType = EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks, };
				var tabInfo = declarationConfig.TabInfos.AddNew();
				tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.NctsArrivalUnloadingRemarks;
				var eventInfo = declarationConfig.EventInfos.AddNew();
				eventInfo.EventReference = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				eventInfo.EventType = Events.CustomsEntryStatusCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;

				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DeclarationLockConfigCollection() { declarationConfig }))
				{
					AssertEquals("Declaration Lock Edit set in declaration => Accepted, no lock record", false, readOnly(parentObject));

					arrivalMovementHeader.Header.LockFile("Lock file");
					AssertEquals("Declaration Lock Edit set in declaration => Locked", true, readOnly(parentObject));

					arrivalMovementHeader.Header.UnlockFile("Unlock file");
					AssertEquals("Declaration Lock Edit set in declaration => Unlocked", false, readOnly(parentObject));
				}
			});
		}

		internal static void AssertArrivalNotificationReadOnlyForLockedDeclaration(ZPropertyInfo propertyInfo, NctsHeader header)
		{
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;

			CombineAssertions(() =>
			{
				AssertEquals("By default, the property should not be disabled", false, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = "";
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, no correct CES-record", false, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, CES-record with reference 'UnloadingPermissionGrantedUAP'", true, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, CES-record with reference 'UnloadingRemarks'", true, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, CES-record with reference 'ClosedFullRelease'", true, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, CES-record with reference 'ClosedPartialRelease'", true, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, CES-record with reference 'DiscrepancyResolutionNoRelease'", true, propertyInfo.ReadOnly);

				header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease;
				header.Factory.Save();
				AssertEquals("Declaration Lock Edit has not been set in declaration, CES-record with reference 'DiscrepancyResolutionPartialRelease'", true, propertyInfo.ReadOnly);

				var configs = CustomsDataRegistry.Instance.DeclarationLockForEdit.Value;
				var config = configs.AddNew();
				config.DeclarationType = EUJobMessageTypeList.Codes.NctsArrivalNotification;

				var eventInfo = config.EventInfos.AddNew();
				eventInfo.EventType = AutoEvents.CancelledCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;

				eventInfo = config.EventInfos.AddNew();
				eventInfo.EventReference = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
				eventInfo.EventType = AutoEvents.CustomsEntryStatusCode;
				eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;

				var tabToBecomeLocked = config.TabInfos.AddNew();
				tabToBecomeLocked.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.NctsArrivalNotification;

				using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs))
				{
					AssertNotNull(CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(EUJobMessageTypeList.Codes.NctsArrivalNotification));

					header.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
					header.Factory.Save();
					AssertEquals("Declaration Lock Edit set in declaration, CES-event available but no lock record.", false, propertyInfo.ReadOnly);

					header.LockFile("Lock file");
					AssertEquals("Declaration Lock Edit set in declaration, CES-event available and declaration is locked", true, propertyInfo.ReadOnly);

					header.UnlockFile("Unlock file");
					AssertEquals("Declaration Lock Edit set in declaration, CES-event available and declaration is unlocked", false, propertyInfo.ReadOnly);
				}
			});
		}

		internal static void AssertCheckPW_BondAmount_PermitRuleNotFound(BusinessObjectFactory factory, string phase)
		{
			SetupC0009ForEuAndCtCountries(factory);

			var organization = factory.New<OrgHeader>();
			organization.OH_Code = "ARGO";
			factory.Save();

			var nctsHeader = factory.New<NctsHeaderForTest>();
			nctsHeader.BH_JobReference = "ARG001001";
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = phase;
			nctsHeader.BH_CustomsProfile = "1234";

			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT000000");

			SetUpTariff(factory);

			var guarantee = movementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "1234";
			guarantee.PW_SuretyCode = "ZER";

			var permitHeader = NCTSTestHelper.SetupGuarantee(organization);

			var permitRule = permitHeader.CusGuaranteeRules.AddNew();
			permitRule.CPR_RuleCode = "INV";
			permitRule.CPR_ValueFrom = "ZZZ";

			factory.Save();

			var goodsItem = nctsHeader.IsPhase5 ? nctsHeader.Bills.AddNew().GoodsItems.AddNew() : movementHeader.GoodsItems.AddNew();
			goodsItem.BY_HarmonisedTariff = TestTariffCode;
			goodsItem.BY_MonetaryValue = 100;

			var warningMessage = "A Liability Applicable Percentage is not found for the Guarantee selected in its Rules Tab, so the full amount has been calculated";

			CombineAssertions("Warning message is generated if LAP permit rule is not found", () =>
			{
				guarantee.Validation.ValidatePW_BondAmount();
				AssertHasWarning(guarantee.PW_BondAmountInfo, warningMessage);

				permitRule.CPR_RuleCode = "LAP";
				permitRule.CPR_ValueFrom = "HAL";

				movementHeader.Guarantees.RemoveAndDeleteAll();
				guarantee = movementHeader.Guarantees.AddNew();
				guarantee.PW_BondNumber = "1234";
				guarantee.PW_SuretyCode = "ZER";

				guarantee.Validation.ValidatePW_BondAmount();
				AssertNoWarning(guarantee.PW_BondAmountInfo, warningMessage);
			});
		}

		internal static void AssertCargoDescITariffDescriptionSynchronizerSupporterMembers<T>(T cargoDesc)
			where T : NctsCommonCargoDesc, ITariffDescriptionSynchronizerSupporter
		{
			var supporter = (ITariffDescriptionSynchronizerSupporter)cargoDesc;

			CombineAssertions(() =>
			{
				cargoDesc.BY_Description = "ABC";
				AssertEquals(nameof(supporter.CurrentTariffDescription), "ABC", supporter.CurrentTariffDescription);

				supporter.CurrentTariffDescription = "DEF";
				AssertEquals(nameof(cargoDesc.BY_Description), "DEF", cargoDesc.BY_Description);

				cargoDesc.BY_HarmonisedTariff = NCTSTestHelper.TestTariffCode;
				AssertEquals($"When valid BY_HarmonisedTariff is entered, {supporter.OfficialCustomsTariffDescription}", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES Fish fillets and other fish meat (whether or not minced), fresh, chilled or frozen Other", supporter.OfficialCustomsTariffDescription);

				var universalTariffDescription = cargoDesc.UniversalTariff.FullTariffDescription(cargoDesc.ValuationDate, includeSectionHeadings: false, includeChapterHeading: false, useTariffPreferredLanguage: true).Left(cargoDesc.BY_DescriptionInfo.MaxLength);
				AssertNotNull(cargoDesc.UniversalTariff);
				AssertEquals("OfficialCustomsTariffDescription is same with UniversalTariff when valid", universalTariffDescription, supporter.OfficialCustomsTariffDescription);

				cargoDesc.BY_HarmonisedTariff = "";
				AssertEquals($"When an empty BY_HarmonisedTariff is entered, {supporter.OfficialCustomsTariffDescription}", "", supporter.OfficialCustomsTariffDescription);

				cargoDesc.BY_HarmonisedTariff = "1234567890";
				AssertEquals($"When an invalid BY_HarmonisedTariff is entered, {supporter.OfficialCustomsTariffDescription}", "", supporter.OfficialCustomsTariffDescription);
			});
		}

		internal static void AssertCargoDescSetting_BY_HarmonisedTariff_Synchronizes_BY_Description<T>(T cargoDesc)
			where T : NctsCommonCargoDesc
		{
			CombineAssertions(() =>
			{
				cargoDesc.BY_HarmonisedTariff = TestTariffCode;
				AssertEquals("When setting a valid BY_HarmonisedTariff, BY_Description", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES Fish fillets and other fish meat (whether or not minced), fresh, chilled or frozen Other", cargoDesc.BY_Description);

				cargoDesc.BY_HarmonisedTariff = "8001100000";
				AssertEquals("When switching to another valid BY_HarmonisedTariff, BY_Description", "TIN AND ARTICLES THEREOF Tin, not alloyed", cargoDesc.BY_Description);

				cargoDesc.BY_HarmonisedTariff = "";
				AssertEquals("When setting an empty BY_HarmonisedTariff, BY_Description", "", cargoDesc.BY_Description);

				cargoDesc.BY_HarmonisedTariff = "1234567890";
				AssertEquals("When setting an invalid BY_HarmonisedTariff, BY_Description", "", cargoDesc.BY_Description);
			});
		}

		public static void AssertYesNoListsAreTranslatable(BusinessObject bizObj) => CombineAssertions(() =>
		{
			foreach (var property in bizObj.GetType().GetProperties().Where(p => typeof(ZPropertyInfo).IsAssignableFrom(p.PropertyType)))
			{
				var propertyInfo = (ZPropertyInfo)property.GetValue(bizObj);
				var listDataSource = MetaData.GetListDataSource(bizObj, propertyInfo.PropertyDescriptor);
				if (listDataSource != null && listDataSource.GetType().Name.EndsWith("YesNoList"))
				{
					Assert($"{propertyInfo.Name} [List({listDataSource.GetType().FullName})]: YesNoList should be translatable", !(listDataSource is UntranslatableCodeDescriptionPairList));
				}
			}
		});

		internal static void SetUpTariffAndAntidumpingAndCountervailing(BusinessObjectFactory factory)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", euGrouping);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "LV", ensureDataGroupingExists: false);
			factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, tariffType.PK, NCTSTestHelper.TestTariffCode, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Desc");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Latvia, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);

			var rateTypeDuties = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, "DTY");
			var rateCodeDuties = helper.CreateCusRateCode(factory, "A00", rateTypeDuties.PK);
			var rateDuties = helper.CreateRefCusRate(cusTariff.PK, rateCodeDuties.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12");
			helper.CreateCusApplicability(rateDuties.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("ORD", 0.2m, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingVATApplicability(cusTariff, Core.Constants.CountryCodes.Latvia, "ORD");

			var rateTypeAntidumping = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.AntiDumping, ensureDataGroupingExists: false);
			var rateCodeAntidumping = helper.CreateCusRateCode(factory, "RC1", rateTypeAntidumping.PK);
			var testRateAntidumping1 = helper.CreateRate(cusTariff, rateCodeAntidumping.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");
			var testRateAntidumping2 = helper.CreateRate(cusTariff, rateCodeAntidumping.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");

			var rateTypeCountervailing = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.Countervailing, ensureDataGroupingExists: false);
			var rateCodeCountervailing = helper.CreateCusRateCode(factory, "RC2", rateTypeCountervailing.PK);
			var testRateCountervailing1 = helper.CreateRate(cusTariff, rateCodeCountervailing.PK, ZDateTime.MinSmallDateTimeValue.AddDays(1), ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");
			var testRateCountervailing2 = helper.CreateRate(cusTariff, rateCodeCountervailing.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "25.3 * [FLAT]");

			helper.CreateCusApplicability(testRateAntidumping1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRateAntidumping2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

			helper.CreateCusApplicability(testRateCountervailing1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRateCountervailing2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC03");

			SetupCusCodeForGetAdditionalCodes(helper, factory);
		}

		internal static void SetupCusCodeForGetAdditionalCodes(Universal.Testing.UniversalReferenceTestDataHelper helper, BusinessObjectFactory factory)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var nextMonth = ZDateTime.Today.AddMonths(1);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC01", "EU AC01", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC03", "EU AC03", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);

			factory.Save();
		}

		internal static void SetUpTariffCustomRateFormula(BusinessObjectFactory factory, ZString countryCode, ZString rateUOM1, ZString rateUOM2, bool shouldAddCU1UnitMeasureType = false, string rateUOM3 = "")
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			factory.Save();

			var tariff = helper.CreateTariff(countryCode, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(factory, "A00", rateType1.PK);

			var rate1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateRateUOM(rate1.PK, rateUOM1);
			if (!rateUOM2.IsEmpty)
			{
				helper.CreateRateUOM(rate1.PK, rateUOM2);
			}
			if (!rateUOM3.IsNullOrEmpty())
			{
				helper.CreateRateUOM(rate1.PK, rateUOM3);
			}
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTaxOrFee("ORD", 0.21m, countryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ORD");

			if (shouldAddCU1UnitMeasureType)
			{
				helper.CreateTariffUOM(tariff, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			}

			factory.Save();
		}

		internal static void SetUpTariffSecondUnit(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9111100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(factory, "A00", rateType1.PK);
			var reference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var reference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rateType2 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
			var rateCode2 = helper.CreateCusRateCode(factory, "SEC", rateType2.PK);

			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "MIN(MAX(0.500 * [NAR], VFD * 0.027), VFD * 0.046)", reference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference2.PK);
			var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference1.PK);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.05m, Core.Constants.CountryCodes.Latvia);
			helper.CreateTaxOrFee("ORD", 0.2m, Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "ORD");

			factory.Save();
		}

		internal static void SetUpTariffAllUnits(BusinessObjectFactory factory, ZString countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9111200000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(factory, "A00", rateType1.PK);
			var reference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.5000 * [LTR] + 0.500 * [NAR] + 0.500 * [KGM] + 0.500 * [DTN]", reference1.PK);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("ORD", 0.21m, countryCode);
			helper.CreateTaxOrFee("BRR", 0.06m, countryCode);
			helper.CreateTaxOrFee("BRP", 0.12m, countryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ORD");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "BRP");

			factory.Save();
		}

		internal static ZString[] SetupTransportIdsForOwnPropulsionToTestR0473()
		{
			return new ZString[]
			{
				NctsTransportTypeOfIdList.Codes._10,
				NctsTransportTypeOfIdList.Codes._20,
				NctsTransportTypeOfIdList.Codes._21,
				NctsTransportTypeOfIdList.Codes._30,
				NctsTransportTypeOfIdList.Codes._31,
				NctsTransportTypeOfIdList.Codes._40,
				NctsTransportTypeOfIdList.Codes._41,
				NctsTransportTypeOfIdList.Codes._80,
				NctsTransportTypeOfIdList.Codes._99
			};
		}

		internal static ZString[] SetupTransportIdsForFixedTransportInstallationsToTestR0473()
		{
			return new ZString[]
			{
				NctsTransportTypeOfIdList.Codes._10,
				NctsTransportTypeOfIdList.Codes._20,
				NctsTransportTypeOfIdList.Codes._21,
				NctsTransportTypeOfIdList.Codes._30,
				NctsTransportTypeOfIdList.Codes._31,
				NctsTransportTypeOfIdList.Codes._40,
				NctsTransportTypeOfIdList.Codes._41,
				NctsTransportTypeOfIdList.Codes._80,
				NctsTransportTypeOfIdList.Codes._99
			};
		}

		internal static void AssertCheckBY_RN_NKCountryOfOriginIsValid(NctsCommonCargoDesc goodsItem)
		{
			CombineAssertions(() =>
			{
				goodsItem.Validation.ValidateBY_RN_NKCountryOfOrigin();
				AssertNoMessageError(goodsItem.BY_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
				goodsItem.BY_RN_NKCountryOfOrigin = "X#";
				AssertHasMessageError(goodsItem.BY_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
				goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoMessageError(goodsItem.BY_RN_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		internal static void TestCheckBY_CustomsSecondUnitQtyIsValid(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			factory.Save();

			CombineAssertions(() =>
			{
				goodsItem.Validation.ValidateBY_CustomsSecondUnitQty();
				AssertNoMessageError(goodsItem.BY_CustomsSecondUnitQtyInfo, ListValidation.InvalidCodeMessageError);
				goodsItem.BY_CustomsSecondUnitQty = "X#";
				AssertHasMessageError(goodsItem.BY_CustomsSecondUnitQtyInfo, ListValidation.InvalidCodeMessageError);
				goodsItem.BY_CustomsSecondUnitQty = "DEF";
				AssertNoMessageError(goodsItem.BY_CustomsSecondUnitQtyInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		internal static void AssertCheckBY_CustomsSecondQuantity_RuleTR0084(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem)
		{
			var messageError = "[TR0084] A unit for the Supplementary Quantity has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity";

			CombineAssertions(() =>
			{
				if (goodsItem is NctsArrivalCargoDesc)
				{
					AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, goodsItem.Header);
					using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleActive();
					}

					using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleInactive();
					}
				}
				else
				{
					using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(factory, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleActive();
					}

					using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(factory, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleInactive();
					}
				}
			});

			void AssertRuleActive()
			{
				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				goodsItem.BY_CustomsSecondUnitQty = ZString.Empty;
				goodsItem.Validation.ValidateBY_CustomsSecondQuantity();
				AssertNoMessageErrorContaining("Phase 4 and empty UnitQty", goodsItem.BY_CustomsSecondQuantityInfo, messageError);

				goodsItem.BY_CustomsSecondUnitQty = "KG";
				goodsItem.Validation.ValidateBY_CustomsSecondQuantity();
				AssertNoMessageErrorContaining("Phase 4 and no empty UnitQty", goodsItem.BY_CustomsSecondQuantityInfo, messageError);

				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItem.Validation.ValidateBY_CustomsSecondQuantity();
				AssertHasMessageErrorContaining("Phase 5 and no empty UnitQty", goodsItem.BY_CustomsSecondQuantityInfo, messageError);

				goodsItem.BY_CustomsSecondQuantity = 2;
				AssertNoMessageErrorContaining("Phase 5 and no empty UnitQty no empty Qty", goodsItem.BY_CustomsSecondQuantityInfo, messageError);

				goodsItem.BY_CustomsSecondUnitQty = ZString.Empty;
				goodsItem.BY_CustomsSecondQuantity = 0;
				AssertNoMessageErrorContaining("Phase 5 and empty UnitQty", goodsItem.BY_CustomsSecondQuantityInfo, messageError);
			}

			void AssertRuleInactive()
			{
				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItem.BY_CustomsSecondUnitQty = "KG";
				goodsItem.BY_CustomsSecondQuantity = 0;
				goodsItem.Validation.ValidateBY_CustomsSecondQuantity();
				AssertNoMessageErrorContaining("InactivateValidation - Phase 5 and no empty UnitQty", goodsItem.BY_CustomsSecondQuantityInfo, messageError);
			}
		}

		internal static void AssertCheckBY_CustomsThirdQuantity_RuleTR0084(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem)
		{
			var messageError = "[TR0084] A unit for the [31] Third Qty has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity";

			CombineAssertions(() =>
			{
				if (goodsItem is NctsArrivalCargoDesc)
				{
					AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, goodsItem.Header);
					using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleActive();
					}

					using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleInactive();
					}
				}
				else
				{
					using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(factory, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleActive();
					}

					using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(factory, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleInactive();
					}
				}
			});

			void AssertRuleActive()
			{
				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				goodsItem.BY_CustomsThirdUnitQty = ZString.Empty;
				goodsItem.Validation.ValidateBY_CustomsThirdQuantity();
				AssertNoMessageErrorContaining("Phase 4 and empty UnitQty", goodsItem.BY_CustomsThirdQuantityInfo, messageError);

				goodsItem.BY_CustomsThirdUnitQty = "KG";
				goodsItem.Validation.ValidateBY_CustomsThirdQuantity();
				AssertNoMessageErrorContaining("Phase 4 and no empty UnitQty", goodsItem.BY_CustomsThirdQuantityInfo, messageError);

				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItem.Validation.ValidateBY_CustomsThirdQuantity();
				AssertHasMessageErrorContaining("Phase 5 and no empty UnitQty", goodsItem.BY_CustomsThirdQuantityInfo, messageError);

				goodsItem.BY_CustomsThirdQuantity = 2;
				AssertNoMessageErrorContaining("Phase 5 and no empty UnitQty no empty Qty", goodsItem.BY_CustomsThirdQuantityInfo, messageError);

				goodsItem.BY_CustomsThirdUnitQty = ZString.Empty;
				goodsItem.BY_CustomsThirdQuantity = 0;
				AssertNoMessageErrorContaining("Phase 5 and empty UnitQty", goodsItem.BY_CustomsThirdQuantityInfo, messageError);
			}

			void AssertRuleInactive()
			{
				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItem.BY_CustomsThirdUnitQty = "KG";
				goodsItem.BY_CustomsThirdQuantity = 0;
				goodsItem.Validation.ValidateBY_CustomsThirdQuantity();
				AssertNoMessageErrorContaining("InactivateValidation - Phase 5 and no empty UnitQty", goodsItem.BY_CustomsThirdQuantityInfo, messageError);
			}
		}

		internal static void AssertCheckBY_CustomsFourthQuantity_RuleTR0084(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem)
		{
			var messageError = "[TR0084] A unit for the Fourth Quantity has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity";

			CombineAssertions(() =>
			{
				if (goodsItem is NctsArrivalCargoDesc)
				{
					AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, goodsItem.Header);
					using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleActive();
					}

					using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleInactive();
					}
				}
				else
				{
					using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(factory, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleActive();
					}

					using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(factory, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
					{
						AssertRuleInactive();
					}
				}
			});

			void AssertRuleActive()
			{
				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				goodsItem.BY_CustomsFourthUnitQty = ZString.Empty;
				goodsItem.Validation.ValidateBY_CustomsFourthQuantity();
				AssertNoMessageErrorContaining("Phase 4 and empty UnitQty", goodsItem.BY_CustomsFourthQuantityInfo, messageError);

				goodsItem.BY_CustomsFourthUnitQty = "KG";
				goodsItem.Validation.ValidateBY_CustomsFourthQuantity();
				AssertNoMessageErrorContaining("Phase 4 and no empty UnitQty", goodsItem.BY_CustomsFourthQuantityInfo, messageError);

				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItem.Validation.ValidateBY_CustomsFourthQuantity();
				AssertHasMessageErrorContaining("Phase 5 and no empty UnitQty", goodsItem.BY_CustomsFourthQuantityInfo, messageError);

				goodsItem.BY_CustomsFourthQuantity = 2;
				AssertNoMessageErrorContaining("Phase 5 and no empty UnitQty no empty Qty", goodsItem.BY_CustomsFourthQuantityInfo, messageError);

				goodsItem.BY_CustomsFourthUnitQty = ZString.Empty;
				goodsItem.BY_CustomsFourthQuantity = 0;
				AssertNoMessageErrorContaining("Phase 5 and empty UnitQty", goodsItem.BY_CustomsFourthQuantityInfo, messageError);
			}

			void AssertRuleInactive()
			{
				goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				goodsItem.BY_CustomsFourthUnitQty = "KG";
				goodsItem.BY_CustomsFourthQuantity = 0;
				goodsItem.Validation.ValidateBY_CustomsFourthQuantity();
				AssertNoMessageErrorContaining("InactivateValidation - Phase 5 and no empty UnitQty", goodsItem.BY_CustomsFourthQuantityInfo, messageError);
			}
		}

		internal static void AssertCheckBY_RN_NKCountryOfOriginNR0058<T>(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem, ValidationRuleConfiguration validationRuleConfiguration, NctsValidationDeciderTestContext<T> deciderTestContextArg = null)
			where T : class, INctsCargoDescValidationDecider
		{
			var deciderTestContext = deciderTestContextArg ?? new CargoDescValidationDeciderTestContext<T>(factory);
			AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, goodsItem.Header);

			CombineAssertions(() =>
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.DisableRule(c => c.IsRuleNR0058Active);
				goodsItem.Validation.ValidateBY_RN_NKCountryOfOrigin();
				AssertNoWarning("Origin Country rule disabled", goodsItem.BY_RN_NKCountryOfOriginInfo, validationRuleConfiguration.Messages.NR0058Message);

				deciderTestContext.EnableRule(c => c.IsRuleNR0058Active);
				goodsItem.BY_RN_NKCountryOfOrigin = "";
				AssertHasWarning("Origin Country not specified", goodsItem.BY_RN_NKCountryOfOriginInfo, validationRuleConfiguration.Messages.NR0058Message);

				goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
				AssertNoWarning("Origin Country specified on goods item level", goodsItem.BY_RN_NKCountryOfOriginInfo, validationRuleConfiguration.Messages.NR0058Message);
			});
		}

		internal static void AssertCheckBY_MonetaryValueNR0059<T>(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem, ValidationRuleConfiguration validationRuleConfiguration, NctsValidationDeciderTestContext<T> deciderTestContextArg = null)
			where T : class, INctsCargoDescValidationDecider
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var deciderTestContext = deciderTestContextArg ?? new CargoDescValidationDeciderTestContext<T>(factory);
				AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, goodsItem.Header);

				CombineAssertions(() =>
				{
					deciderTestContext.ClearCachedValidationDecider(goodsItem);

					deciderTestContext.DisableRule(c => c.IsRuleNR0059Active);
					goodsItem.Validation.ValidateBY_MonetaryValue();
					AssertNoWarning("Monetary value rule disabled", goodsItem.BY_MonetaryValueInfo, validationRuleConfiguration.Messages.NR0059Message);

					deciderTestContext.EnableRule(c => c.IsRuleNR0059Active);
					goodsItem.Validation.ValidateBY_MonetaryValue();
					AssertHasWarning("When Monetary Value is empty", goodsItem.BY_MonetaryValueInfo, validationRuleConfiguration.Messages.NR0059Message);

					goodsItem.BY_MonetaryValue = 10;
					AssertNoWarning("When Monetary Value is not empty there is no message error", goodsItem.BY_MonetaryValueInfo, validationRuleConfiguration.Messages.NR0059Message);
				});
			}
		}

		internal static void AssertCheckBY_SupplementsNR0060<T>(BusinessObjectFactory factory, NctsCommonCargoDesc goodsItem, ValidationRuleConfiguration validationRuleConfiguration, NctsValidationDeciderTestContext<T> deciderTestContextArg = null)
			where T : class, INctsCargoDescValidationDecider
		{
			var deciderTestContext = deciderTestContextArg ?? new CargoDescValidationDeciderTestContext<T>(factory);
			AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, goodsItem.Header);

			CombineAssertions(() =>
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.DisableRule(c => c.IsRuleNR0060Active);
				goodsItem.Validation.ValidateBY_Supplements();
				AssertNoMessageError("Rule disabled", goodsItem.BY_SupplementsInfo, validationRuleConfiguration.Messages.NR0060Message);

				deciderTestContext.EnableRule(c => c.IsRuleNR0060Active);
				goodsItem.Validation.ValidateBY_Supplements();
				AssertNoMessageError("Rule enabled and code list empty", goodsItem.BY_SupplementsInfo, validationRuleConfiguration.Messages.NR0060Message);

				SetupTariffAndRateForAdditionalSupplementaryCodes(factory);

				var code = new BaseSupplementaryCode.Loader(factory).LoadOrCreate(goodsItem, 1);
				code.CY_Code = "SUP";
				goodsItem.BY_HarmonisedTariff = "1234512345";
				goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
				AssertEquals("Code list not empty", 1, SupplementaryCodeHelper.GetCodeList(goodsItem).Count);
				goodsItem.Validation.ValidateBY_Supplements();
				AssertHasMessageError("Rule enabled and code list not empty", goodsItem.BY_SupplementsInfo, validationRuleConfiguration.Messages.NR0060Message);
			});
		}

		internal static void AssertCheckBY_Supplements_HasChildValidationNotification(NctsCommonCargoDesc goodsItem)
		{
			const string expectedErrorMessage = "There are errors within 'Additional Codes', please click on 'Additional codes...' to view the error information";

			var additionalSupplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode1.CY_Code = "TEST";
			goodsItem.Validation.ValidateBY_Supplements();
			CombineAssertions(() =>
			{
				AssertNoMessageError("When AdditionalSupplementaryCodes doesn't contains error.", goodsItem.BY_SupplementsInfo, expectedErrorMessage);

				var additionalSupplementaryCode2 = goodsItem.AdditionalSupplementaryCodes.AddNew();
				additionalSupplementaryCode2.CY_Code = "123";
				goodsItem.Validation.ValidateBY_Supplements();
				AssertHasMessageError("When AdditionalSupplementaryCodes contains error.", goodsItem.BY_SupplementsInfo, expectedErrorMessage);
			});
		}

		internal static void SetupTariffAndRateForAdditionalSupplementaryCodes(BusinessObjectFactory factory)
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountry, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountry, "IMP");
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(factory, "A00", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("100", "100", currentCountry);

			var cusTariff = testHelper.CreateTariff(currentCountry, hsnTariffType.PK, "1234512345", date1, date4, "dummy Description 0");

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "additionalcode", "ordernumber");

			testHelper.CreateCusCodeType("ADDCD", "Additional Codes");
			testHelper.CreateCusCodeList(currentCountry, "ADDCD", "additionalcode", "Additional Code 1 Descriptions", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			factory.Save();
		}

		#region NctsArrivalMovementHeaderForTest

		public class NctsArrivalMovementHeaderForLiabilityTest : NctsArrivalMovementHeader
		{
			public NctsArrivalMovementHeaderForLiabilityTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZBool ShouldGuaranteeForArrivalBeVisibleCore => true;
		}

		public static void AddNctsArrivalMovementHeaderForLiabilityTestToHeader(BusinessObjectFactory factory, NctsHeader header)
		{
			var arrivalMovementHeader = factory.New<NctsArrivalMovementHeaderForLiabilityTest>();

			header.MovementHeaders.RemoveAll(match => true);
			header.MovementHeaders.Add(arrivalMovementHeader);
		}

		#endregion

		#region NctsArrivalCargoDescForTest

		public class NctsArrivalCargoDescForTest : NctsArrivalCargoDesc
		{
			public NctsArrivalCargoDescForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString TariffTypeCore => Customs.Universal.Constants.TariffTypes.Import;
		}

		internal static NctsArrivalCargoDescForTest GetArrivalCargoDescTest(BusinessObjectFactory factory)
		{
			var arrivalCargoDesc = factory.New<NctsArrivalCargoDescForTest>();
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			AddNctsArrivalMovementHeaderForLiabilityTestToHeader(factory, header);
			var bill = header.Bills.AddNew();
			arrivalCargoDesc.BY_ParentTableCode = bill.TablePrefix;
			arrivalCargoDesc.BY_ParentID = bill.PK;
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			return arrivalCargoDesc;
		}

		#endregion
	}
}
