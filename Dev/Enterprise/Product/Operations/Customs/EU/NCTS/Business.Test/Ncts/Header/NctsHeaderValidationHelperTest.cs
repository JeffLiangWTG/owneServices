using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderValidationHelperTest : TestCaseWithFactory
	{
		public void TestCheckConditionC901_TIRMovement()
		{
			CombineAssertions(() =>
			{
				var goodsItem = GetFirstTIRGoodsItem();
				goodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("No Supporting Doc", goodsItem, DocumentTypeRowErrorMessage);
				var supportingDoc = goodsItem.SupportingDocuments.AddNew();
				supportingDoc.CSI_Code = NctsHeaderValidationHelper.TirCarnetDocumentCode;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("Supporting Doc", goodsItem, DocumentTypeRowErrorMessage);
			});
		}

		public void TestCheckConditionC901_LineNo()
		{
			CombineAssertions(() =>
			{
				var goodsItem = GetFirstTIRGoodsItem();
				goodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("Line 1", goodsItem, DocumentTypeRowErrorMessage);
				goodsItem.BY_LineNo = 2;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("Line 2", goodsItem, DocumentTypeRowErrorMessage);
			});
		}

		public void TestHasOneDocumentType952_NonTIRMovement()
		{
			CombineAssertions(() =>
			{
				var goodsItem = GetFirstTIRGoodsItem();
				goodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("TIR Movement", goodsItem, DocumentTypeRowErrorMessage);
				goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("T1 Movement", goodsItem, DocumentTypeRowErrorMessage);
			});
		}

		public void TestCheckMandatoryArrivalDestinationTrader() => CombineAssertions(() =>
		{
			const string message = "[TR0074] The Destination Trader with its EORI number is mandatory. Please select an organization with an EORI number";

			var arrival = Factory.New<NctsHeader>();
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (arrival.DestinationTrader.SuspendValidationTesting())
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					arrival.DestinationTrader.E2_AddressOverride = false;
					arrival.DestinationTrader.OrganisationPKInfo.ClearAllNotifications();
					NctsHeaderValidationHelper.CheckMandatoryArrivalDestinationTrader(arrival, arrival.DestinationTrader.OrganisationPKInfo);
					AssertHasMessageError("TR0074=active When Destination Trader is empty there should be an error", arrival.DestinationTrader.OrganisationPKInfo, message);
					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					arrival.DestinationTrader.OrganisationPKInfo.ClearAllNotifications();
					NctsHeaderValidationHelper.CheckMandatoryArrivalDestinationTrader(arrival, arrival.DestinationTrader.OrganisationPKInfo);
					AssertNoMessageError("TR0074=inactive When Destination Trader is empty there should not be an error", arrival.DestinationTrader.OrganisationPKInfo, message);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					arrival.DestinationTrader.E2_AddressOverride = true;
					arrival.DestinationTrader.OrganisationPKInfo.ClearAllNotifications();
					NctsHeaderValidationHelper.CheckMandatoryArrivalDestinationTrader(arrival, arrival.DestinationTrader.OrganisationPKInfo);
					AssertHasMessageErrorContaining("TR0074=active When Destination Trader EORI is not valid there should be an error", arrival.DestinationTrader.OrganisationPKInfo, message);
					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					arrival.DestinationTrader.OrganisationPKInfo.ClearAllNotifications();
					NctsHeaderValidationHelper.CheckMandatoryArrivalDestinationTrader(arrival, arrival.DestinationTrader.OrganisationPKInfo);
					AssertNoMessageErrorContaining("TR0074=inactive When Destination Trader EORI is not valid there should not be an error", arrival.DestinationTrader.OrganisationPKInfo, message);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					var orgWithoutEori = Factory.NewWithValidTestData<OrgHeader>();
					arrival.DestinationTrader.OrganisationPK = orgWithoutEori.PK;
					AssertHasMessageErrorContaining("TR0074=active ", arrival.DestinationTrader.OrganisationPKInfo, message);
					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					arrival.DestinationTrader.OrganisationPKInfo.ClearAllNotifications();
					NctsHeaderValidationHelper.CheckMandatoryArrivalDestinationTrader(arrival, arrival.DestinationTrader.OrganisationPKInfo);
					AssertNoMessageErrorContaining("TR0074=active ", arrival.DestinationTrader.OrganisationPKInfo, message);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0074Active));
					var orgWithEori = Factory.NewWithValidTestData<OrgHeader>();
					orgWithEori.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("EOR", "1232141", Core.Constants.CountryCodes.Germany);
					arrival.DestinationTrader.OrganisationPK = orgWithEori.PK;
					arrival.DestinationTrader.OrganisationPKInfo.ClearAllNotifications();
					NctsHeaderValidationHelper.CheckMandatoryArrivalDestinationTrader(arrival, arrival.DestinationTrader.OrganisationPKInfo);
					AssertNoMessageError(arrival.DestinationTrader.OrganisationPKInfo, message);
				}
			}
		});

		public void TestCheckCustomsOffices_NCTS4()
		{
			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			departure.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;

			CombineAssertions(() =>
			{
				AssertHasMessageError("DES", departure.MovementHeader.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of destination with purpose DES.");
				AssertHasMessageError("DEP", departure.MovementHeader.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of departure with purpose DEP.");

				var officeCodeDES = departure.CustomsOfficesForDeparture.AddNew();
				officeCodeDES.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
				officeCodeDES.CY_Data = "AAA";
				departure.MovementHeader.Validation.ValidateBM_InBondEntryType();

				AssertNoMessageError("DES OfficeOfDestination", departure.MovementHeader.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of destination with purpose DES.");
				AssertHasMessageError("DEP OfficeOfDestination", departure.MovementHeader.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of departure with purpose DEP.");

				var officeCodeDEP = departure.CustomsOfficesForDeparture.AddNew();
				officeCodeDEP.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
				officeCodeDEP.CY_Data = "BBB";

				AssertNoMessageError("DES OfficeOfDeparture", departure.MovementHeader.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of destination with purpose DES.");
				AssertNoMessageError("DEP OfficeOfDeparture", departure.MovementHeader.BM_InBondEntryTypeInfo, "The declaration requires an office of type NCTS Office of departure with purpose DEP.");
			});
		}

		public void TestCheckCustomsOffices_TR0006()
		{
			const string messageError = "[TR0006] The Declaration requires a Customs Office with Purpose 'DES' (Destination Office).";

			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeader = departure.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0006Active));

				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;

				CombineAssertions(() =>
				{
					AssertHasMessageError("No DES Customs Office", movementHeader.BM_InBondEntryTypeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0006Active));
					ValidateBM_InBondEntryType();
					AssertNoMessageError("Rule TR0006 disabled", movementHeader.BM_InBondEntryTypeInfo, messageError);
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0006Active));

					var officeCodeDES = movementHeader.CustomsOfficesForDeparture.AddNew();
					officeCodeDES.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
					officeCodeDES.CY_Data = "AAA";
					ValidateBM_InBondEntryType();
					AssertNoMessageError("With DES Customs Office", movementHeader.BM_InBondEntryTypeInfo, messageError);
				});

				void ValidateBM_InBondEntryType()
				{
					Factory.ClearCachedValue<IEnumerable<CustomsOfficeRequirement>>(string.Join(".", "NctsMovementHeaderCustomsOfficeRequirementHelper.OtherRequirements", departure.BH_HeaderType, movementHeader.BM_InBondEntryType, departure.BH_ApplicationCode));
					movementHeader.Validation.ValidateBM_InBondEntryType();
				}
			}
		}

		public void TestCheckCustomsOffices_TR0007()
		{
			const string messageError = "[TR0007] The Declaration requires a Customs Office with Purpose 'DEP' (Departure Office).";

			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeader = departure.MovementHeader;
			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0007Active));

				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;

				CombineAssertions(() =>
				{
					AssertHasMessageError("No DEP Customs Office", movementHeader.BM_InBondEntryTypeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0007Active));
					ValidateBM_InBondEntryType();
					AssertNoMessageError("Rule TR0007 disabled", departure.MovementHeader.BM_InBondEntryTypeInfo, messageError);
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0007Active));

					var officeCodeDEP = movementHeader.CustomsOfficesForDeparture.AddNew();
					officeCodeDEP.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
					officeCodeDEP.CY_Data = "BBB";
					ValidateBM_InBondEntryType();
					AssertNoMessageError("With DEP Customs Office", movementHeader.BM_InBondEntryTypeInfo, messageError);
				});

				void ValidateBM_InBondEntryType()
				{
					Factory.ClearCachedValue<IEnumerable<CustomsOfficeRequirement>>(string.Join(".", "NctsMovementHeaderCustomsOfficeRequirementHelper.OtherRequirements", departure.BH_HeaderType, movementHeader.BM_InBondEntryType, departure.BH_ApplicationCode));
					movementHeader.Validation.ValidateBM_InBondEntryType();
				}
			}
		}

		public void TestCheckConditionC001()
		{
			var messageError = "Consignee Trader is required for goods destined for NCTS contracting parties.(C001)";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var movementHeader = nctsHeader.MovementHeader;
			var consignee = nctsHeader.Consignee;
			var organisation = Factory.New<OrgHeader>();
			var propertyInfo = nctsHeader.Consignee.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				consignee.Validation.ValidateAll();
				AssertNoMessageError("Empty BM_RL_NKDestinationPort", propertyInfo, messageError);

				movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
				consignee.Validation.ValidateAll();
				AssertHasMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

				movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
				consignee.Validation.ValidateAll();
				AssertHasMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
				consignee.Validation.ValidateAll();
				AssertNoMessageError("C001 is for NCTS4", propertyInfo, messageError);
			});
		}

		public void TestCheckRepresentative_R0850_1()
		{
			const string messageError = "[R0850-1] EORI-Number is required but Representative has no EORI-Number captured in Registration Numbers / Codes.";
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0850_1Active));

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
				var representative = nctsHeader.MovementHeader.Representative;
				var propertyInfo = representative.OrganisationPKInfo;

				CombineAssertions(() =>
				{
					representative.Validation.ValidateOrganisationPK();
					AssertNoMessageError("No Representative captured", propertyInfo, messageError);

					representative.OrganisationPK = orgHeader.PK;
					AssertNoMessageError("Has EORI-Number in Registration Numbers / Codes", propertyInfo, messageError);

					representative.OrganisationPK = orgHeader1.PK;
					AssertHasMessageError("No EORI-Number in Registration Numbers / Codes", propertyInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0850_1Active));
					representative.Validation.ValidateOrganisationPK();
					AssertNoMessageError("RuleR0850_1 isn't active", propertyInfo, messageError);
				});
			}
		}

		public void TestGetDuplicateMRNQuery()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_GB = branch.PK;
			nctsHeader.ArrivalMrnFromUser = "12345";
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			testNctsHeader.BH_GB = branch.PK;
			testNctsHeader.ArrivalMrnFromUser = "12345";
			Factory.Save();

			AssertEquals("There is a Header in same country which has same ArrivalMrnFromUser", 1, Factory.Load<NctsHeader>(NctsHeaderValidationHelper.GetDuplicateMRNQuery(nctsHeader)).Length);
			AssertEquals(testNctsHeader, Factory.LoadTop1<NctsHeader>(NctsHeaderValidationHelper.GetDuplicateMRNQuery(nctsHeader)));
		}

		public void TestIsConditionRP16()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
					AssertEquals("No CusAuthorizationUsages get an ACR code", false, NctsHeaderValidationHelper.IsConditionRP16(nctsHeader));

					var movementHeader = nctsHeader.MovementHeader;
					var cusAuthorizationUsage = movementHeader.CusAuthorizationUsages.AddNew();
					cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					AssertEquals("Declaration type is empty", false, NctsHeaderValidationHelper.IsConditionRP16(nctsHeader));

					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
					AssertEquals("Declaration type does not equal to TIR and got an ACR code CusAuthorizationUsage", true, NctsHeaderValidationHelper.IsConditionRP16(nctsHeader));

					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
					AssertEquals("Declaration type equal to TIR", false, NctsHeaderValidationHelper.IsConditionRP16(nctsHeader));

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleRP16Active));
					AssertEquals("Rule PR16 is inactive", false, NctsHeaderValidationHelper.IsConditionRP16(nctsHeader));
				}
			});
		}

		public void TestCheckCustomsOffices_CustomsOfficesForDepartureChangedAfterCustomsOfficesLoaded()
		{
			var departure = Factory.New<NctsHeader>();
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departure.MovementHeader;

			movementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;

			var officeCodeDEP = movementHeader.CustomsOfficesForDeparture.AddNew();
			officeCodeDEP.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
			officeCodeDEP.CY_Data = "BBB";

			var officeCodeDES = movementHeader.CustomsOfficesForDeparture.AddNew();
			officeCodeDES.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDestination;
			officeCodeDES.CY_Data = "AAA";
			Factory.Save();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleTR0006Active)))
			{
				var factory2 = new BusinessObjectFactory();
				var departure2 = factory2.Load<NctsHeader>(departure.PK);
				var movementHeader2 = departure2.MovementHeader;
				movementHeader2.Validation.ValidateBM_InBondEntryType();
				AssertNoMessageError(movementHeader2.BM_InBondEntryTypeInfo, "[TR0006] The Declaration requires a Customs Office with Purpose 'DES' (Destination Office).");

				movementHeader2.CustomsOfficesForDeparture.RemoveAndDeleteAll();
				movementHeader2.Validation.ValidateBM_InBondEntryType();
				AssertHasMessageError(movementHeader2.BM_InBondEntryTypeInfo, "[TR0006] The Declaration requires a Customs Office with Purpose 'DES' (Destination Office).");
			}
		}

		public void TestCheckConditionR0520_Principal()
		{
			var messageError = "This field may not be amended to a value that is different to what was originally declared to Customs.";
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, suffix: "", traderTin: "123456789012");
			Factory.Save();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageError(departure.Principal.OrganisationPKInfo, messageError);

				departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				var newOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				departure.Principal.OrganisationPK = newOrgPK;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageError(departure.Principal.OrganisationPKInfo, messageError);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				departure.MovementHeader.BM_CustomsStatus = string.Empty;
				Factory.Save();

				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageError(departure.Principal.OrganisationPKInfo, messageError);

				departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				newOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				departure.Principal.OrganisationPK = newOrgPK;
				departure.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageError(departure.Principal.OrganisationPKInfo, messageError);
			}
		}

		public void TestCheckConditionR0520_CustomsOfficesForDeparture()
		{
			var messageError = "This field may not be amended to a value that is different to what was originally declared to Customs.";
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);

			var departureMovement = departure.MovementHeader;
			var officeCodeDEP = departureMovement.CustomsOfficesForDeparture.AddNew();
			officeCodeDEP.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDeparture;
			officeCodeDEP.CY_Data = "DAA";
			Factory.Save();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				officeCodeDEP.Validation.ValidateCY_Data();
				AssertNoMessageError(officeCodeDEP.CY_DataInfo, messageError);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				officeCodeDEP.CY_Data = "DBB";
				officeCodeDEP.Validation.ValidateCY_Data();
				AssertHasMessageError(officeCodeDEP.CY_DataInfo, messageError);

				Factory.Save();

				officeCodeDEP.Validation.ValidateCY_Code();
				AssertNoMessageError(officeCodeDEP.CY_CodeInfo, messageError);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				officeCodeDEP.CY_Code = "XX";
				officeCodeDEP.Validation.ValidateCY_Code();
				AssertHasMessageError(officeCodeDEP.CY_CodeInfo, messageError);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				departureMovement.BM_CustomsStatus = string.Empty;
				Factory.Save();

				officeCodeDEP.Validation.ValidateCY_Data();
				AssertNoMessageError(officeCodeDEP.CY_DataInfo, messageError);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				officeCodeDEP.CY_Data = "CCC";
				officeCodeDEP.Validation.ValidateCY_Data();
				AssertNoMessageError(officeCodeDEP.CY_DataInfo, messageError);

				Factory.Save();

				officeCodeDEP.Validation.ValidateCY_Code();
				AssertNoMessageError(officeCodeDEP.CY_CodeInfo, messageError);

				departureMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				officeCodeDEP.CY_Code = "YY";
				officeCodeDEP.Validation.ValidateCY_Code();
				AssertNoMessageError(officeCodeDEP.CY_CodeInfo, messageError);
			}
		}

		public void TestCheckConditionR0520_Representative()
		{
			var messageError = "This field may not be amended to a value that is different to what was originally declared to Customs.";
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departure.MovementHeader;
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", movementHeader.Representative, suffix: "", traderTin: "123456789012");
			Factory.Save();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.Representative.Validation.ValidateOrganisationPK();
				AssertNoMessageError(movementHeader.Representative.OrganisationPKInfo, messageError);

				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				var newOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				movementHeader.Representative.OrganisationPK = newOrgPK;
				movementHeader.Representative.Validation.ValidateOrganisationPK();
				AssertHasMessageError(movementHeader.Representative.OrganisationPKInfo, messageError);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.BM_CustomsStatus = string.Empty;
				Factory.Save();

				movementHeader.Representative.Validation.ValidateOrganisationPK();
				AssertNoMessageError(movementHeader.Representative.OrganisationPKInfo, messageError);

				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				newOrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				movementHeader.Representative.OrganisationPK = newOrgPK;
				movementHeader.Representative.Validation.ValidateOrganisationPK();
				AssertNoMessageError(movementHeader.Representative.OrganisationPKInfo, messageError);
			}
		}

		public void TestCheckConditionR0520_EntryType()
		{
			var messageError = "This field may not be amended to a value that is different to what was originally declared to Customs.";
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departure.MovementHeader;
			movementHeader.BM_InBondEntryType = "AB";
			Factory.Save();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.Validation.ValidateBM_InBondEntryType();
				AssertNoMessageError(movementHeader.BM_InBondEntryTypeInfo, messageError);

				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				movementHeader.BM_InBondEntryType = "CD";
				movementHeader.Validation.ValidateBM_InBondEntryType();
				AssertHasMessageError(movementHeader.BM_InBondEntryTypeInfo, messageError);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.BM_CustomsStatus = string.Empty;
				Factory.Save();

				movementHeader.Validation.ValidateBM_InBondEntryType();
				AssertNoMessageError(movementHeader.BM_InBondEntryTypeInfo, messageError);

				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				movementHeader.BM_InBondEntryType = "EF";
				movementHeader.Validation.ValidateBM_InBondEntryType();
				AssertNoMessageError(movementHeader.BM_InBondEntryTypeInfo, messageError);
			}
		}

		public void TestCheckConditionR0520_AdditionalDeclarationType()
		{
			var messageError = "This field may not be amended to a value that is different to what was originally declared to Customs.";
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departure.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
				AssertNoMessageError(movementHeader.BM_AdditionalDeclarationTypeInfo, messageError);

				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
				AssertHasMessageError(movementHeader.BM_AdditionalDeclarationTypeInfo, messageError);

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0520Active));

				movementHeader.BM_CustomsStatus = string.Empty;
				Factory.Save();

				movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
				AssertNoMessageError(movementHeader.BM_AdditionalDeclarationTypeInfo, messageError);

				movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				movementHeader.Validation.ValidateBM_AdditionalDeclarationType();
				AssertNoMessageError(movementHeader.BM_AdditionalDeclarationTypeInfo, messageError);
			}
		}

		public void TestCheckConditionR350_RuleIsActive()
		{
			using (ValidationDeciderTestHelper.ActiveForDepartureMovementHeaderPhase5(Factory, v => v.IsRuleR0350Active))
			{
				AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => { });
			}
		}

		public void TestCheckConditionR350_RuleIsNotActive()
		{
			using (ValidationDeciderTestHelper.InactiveForDepartureMovementHeaderPhase5(Factory, r => r.IsRuleR0350Active))
			{
				AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: false, h => { });
			}
		}

		public void TestCheckConditionR350_DepartureMovement()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.BH_HeaderType = NctsMovementType.Codes.Departure);
		}

		public void TestCheckConditionR350_NotPhase5()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: false, h => h.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4);
		}

		public void TestCheckConditionR350_ForPhase5()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5);
		}

		public void TestCheckConditionR350_ReducedDatasetIndicatorUnchecked()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: false, h => h.MovementHeader.BM_ReducedDatasetIndicator = false);
		}

		public void TestCheckConditionR350_ReducedDatasetIndicatorChecked()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.MovementHeader.BM_ReducedDatasetIndicator = true);
		}

		public void TestCheckConditionR350_CodeTRD()
		{
			const string requiredCode = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset;
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: false, h => h.MovementHeader.CusAuthorizationUsages.First().AGC_Code = requiredCode);
		}

		public void TestCheckConditionR350_NoCodeTRD()
		{
			const string anyOtherCode = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.MovementHeader.CusAuthorizationUsages.First().AGC_Code = anyOtherCode);
		}

		public void TestCheckConditionR350_SiblingAuthorsationWithCodeTRD()
		{
			const string requiredCode = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset;
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: false, h => h.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = requiredCode);
		}

		public void TestCheckConditionR350_SiblingAuthorsationWithoutCodeTRD()
		{
			const string anyOtherCode = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = anyOtherCode);
		}

		public void TestCheckConditionR350_InlandTransportModeNotSeaRailAir()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: false, h => h.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport);
		}

		public void TestCheckConditionR350_InlandTransportModeSea()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport);
		}

		public void TestCheckConditionR350_InlandTransportModeRail()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport);
		}

		public void TestCheckConditionR350_InlandTransportModeAir()
		{
			AssertBM_ReducedDatasetIndicatorInfo(errorIsExpected: true, h => h.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport);
		}

		public void TestValidateConsignorAddressPostcode_RuleE1102_1()
		{
			const string expectedWarningMessage = "Consignor Address Postcode is longer than 9 characters, it will be truncated in the message";

			var (organizationPostcodeLengthMoreThan9, organizationPostcodeLengthLessThan10) = SetupOrganizationsForRuleE1102_1();
			var nctsHeader = GetNctsHeader();

			var consignorAddressInfo = nctsHeader.Consignor.E2_OA_AddressInfo;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active)))
			{
				using (SetTransitionPeriod(true))
				{
					nctsHeader.Consignor.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertHasWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode length is greater than 9", consignorAddressInfo, expectedWarningMessage);

					nctsHeader.Consignor.OrganisationPK = organizationPostcodeLengthLessThan10.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode less than 10", consignorAddressInfo, expectedWarningMessage);

					nctsHeader.Consignor.OrganisationPK = ZGuid.Empty;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on is Empty", consignorAddressInfo, expectedWarningMessage);
				}

				using (SetTransitionPeriod(false))
				{
					nctsHeader.Consignor.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is OFF and Postcode length is greater than 9", consignorAddressInfo, expectedWarningMessage);
				}
			}
			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active), value: false))
			using (SetTransitionPeriod(true))
			{
				nctsHeader.Consignor.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
				AssertNoWarningContaining("When RuleE1102_1 disabled and Transit period is on and Postcode length is greater than 9", consignorAddressInfo, expectedWarningMessage);
			}
		}

		public void TestValidateConsigneeAddressPostcode_RuleE1102_1()
		{
			const string expectedWarningMessage = "Consignee Address Postcode is longer than 9 characters, it will be truncated in the message";

			var (organizationPostcodeLengthMoreThan9, organizationPostcodeLengthLessThan10) = SetupOrganizationsForRuleE1102_1();
			var nctsHeader = GetNctsHeader();
			var consigneeAddressInfo = nctsHeader.Consignee.E2_OA_AddressInfo;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active)))
			{
				using (SetTransitionPeriod(true))
				{
					nctsHeader.Consignee.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertHasWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode length is greater than 9", consigneeAddressInfo, expectedWarningMessage);

					nctsHeader.Consignee.OrganisationPK = organizationPostcodeLengthLessThan10.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode less than 10", consigneeAddressInfo, expectedWarningMessage);

					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on is Empty", consigneeAddressInfo, expectedWarningMessage);
				}

				using (SetTransitionPeriod(false))
				{
					nctsHeader.Consignee.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is OFF and Postcode length is greater than 9", consigneeAddressInfo, expectedWarningMessage);
				}
			}
			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active), value: false))
			using (SetTransitionPeriod(true))
			{
				nctsHeader.Consignee.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
				AssertNoWarningContaining("When RuleE1102_1 disabled and Transit period is on and Postcode length is greater than 9", consigneeAddressInfo, expectedWarningMessage);
			}
		}

		public void TestValidatePrincipalAddressPostcode_RuleE1102_1()
		{
			const string expectedWarningMessage = "Principal Address Postcode is longer than 9 characters, it will be truncated in the message";

			var (organizationPostcodeLengthMoreThan9, organizationPostcodeLengthLessThan10) = SetupOrganizationsForRuleE1102_1();
			var nctsHeader = GetNctsHeader();
			var principalAddressInfo = nctsHeader.Principal.E2_OA_AddressInfo;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active)))
			{
				using (SetTransitionPeriod(true))
				{
					nctsHeader.Principal.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertHasWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode length is greater than 9", principalAddressInfo, expectedWarningMessage);

					nctsHeader.Principal.OrganisationPK = organizationPostcodeLengthLessThan10.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on and Postcode less than 10", principalAddressInfo, expectedWarningMessage);

					nctsHeader.Principal.OrganisationPK = ZGuid.Empty;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is on is Empty", principalAddressInfo, expectedWarningMessage);
				}

				using (SetTransitionPeriod(false))
				{
					nctsHeader.Principal.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
					AssertNoWarningContaining("When RuleE1102_1 active and Transit period is OFF and Postcode length is greater than 9", principalAddressInfo, expectedWarningMessage);
				}
			}
			using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102_1Active), value: false))
			using (SetTransitionPeriod(true))
			{
				nctsHeader.Principal.OrganisationPK = organizationPostcodeLengthMoreThan9.PK;
				AssertNoWarningContaining("When RuleE1102_1 disabled and Transit period is on and Postcode length is greater than 9", principalAddressInfo, expectedWarningMessage);
			}
		}

		IDisposable SetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

		(OrgHeader organizationPostcodeLengthMoreThan9, OrgHeader organizationPostcodeLengthLessThan10) SetupOrganizationsForRuleE1102_1()
		{
			var organizationPostcodeLengthMoreThan9 = Factory.New<OrgHeader>();
			organizationPostcodeLengthMoreThan9.MainAddress.Postcode = "1234567890";

			var organizationPostcodeLengthLessThan10 = Factory.New<OrgHeader>();
			organizationPostcodeLengthLessThan10.MainAddress.Postcode = "123456789";
			return (organizationPostcodeLengthMoreThan9, organizationPostcodeLengthLessThan10);
		}

		public void TestCheckConditionC050()
		{
			const string expectedErrorWhenTirDeclaration = "[C050] Please enter a Principal trader with an EORI or TIR or enter full address details";
			const string expectedErrorForNonTirDeclaration = "[C050] Please enter a Principal trader with an EORI or enter full address details";

			var header = GetNctsHeader();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertValidationRule("No ValidationDecider (NCTS4)", true);

			header = GetNctsHeader();
			AssertNotNull("Pre-condition: With ValidationDecider", header.ValidationDecider);

			using (var testContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				header = GetNctsHeader();
				testContext.ClearCachedValidationDecider(header);
				testContext.DisableRule(x => x.IsRuleC0050Active);
				AssertValidationRule("C0050 not active", false);
				testContext.EnableRule(x => x.IsRuleC0050Active);
				AssertValidationRule("C0050 active", true);
			}

			void AssertValidationRule(string assertionMessage, bool isRuleActive)
			{
				header.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
				CombineAssertions($"{assertionMessage}: When DeclarationType = TIR", () =>
				{
					var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD1", traderTin: "");
					header.Principal.OrganisationPK = orgAddress.Header.PK;
					AssertMessageError("When EORI or TIR number is not present", isRuleActive, expectedErrorWhenTirDeclaration);

					var orgAddressWithTir = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD2", traderTir: "TIR1233222", traderTin: "");
					header.Principal.OrganisationPK = orgAddressWithTir.Header.PK;
					AssertMessageError("When TIR number is present", false, expectedErrorWhenTirDeclaration);

					var orgAddressWithEori = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD2", traderTin: "TIN2832947234");
					header.Principal.OrganisationPK = orgAddressWithEori.Header.PK;
					AssertMessageError("When EORI number is present", false, expectedErrorWhenTirDeclaration);
				});

				header.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				CombineAssertions($"{assertionMessage}: When DeclarationType is not a TIR Declaration", () =>
				{
					var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD1", traderTin: "");
					header.Principal.OrganisationPK = orgAddress.Header.PK;
					AssertMessageError("When EORI number is not present", isRuleActive, expectedErrorForNonTirDeclaration);

					var orgAddressWithEori = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD2", traderTin: "TIN2832947234");
					header.Principal.OrganisationPK = orgAddressWithEori.Header.PK;
					AssertMessageError("When EORI number is present", false, expectedErrorForNonTirDeclaration);
				});
			}

			void AssertMessageError(string assertionMessage, bool errorExpected, string errorMessage)
			{
				if (errorExpected)
				{
					AssertHasMessageErrorContaining(assertionMessage, header.Principal.OrganisationPKInfo, errorMessage);
				}
				else
				{
					AssertNoMessageErrorContaining(assertionMessage, header.Principal.OrganisationPKInfo, errorMessage);
				}
			}
		}

		public void TestCheckConditionC236()
		{
			const string expectedErrorWhenTirDeclaration = "[C236] Principal EORI or TIR is required if guarantees are used";
			var header = GetNctsHeader();
			header.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.TirDeclaration;
			NCTSTestHelper.CreateGuaranteeForTest(header, "1", "092398234923423794", "", "122", "OTHER");

			CombineAssertions("When DeclarationType = TIR", () =>
			{
				var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD1", traderTin: "");
				header.Principal.OrganisationPK = orgAddress.Header.PK;
				AssertHasMessageErrorContaining("When EORI or TIR number is not present", header.Principal.OrganisationPKInfo, expectedErrorWhenTirDeclaration);

				var orgAddressWithTir = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD2", traderTir: "TIR1233222", traderTin: "");
				header.Principal.OrganisationPK = orgAddressWithTir.Header.PK;
				AssertNoMessageErrorContaining("When TIR number is present", header.Principal.OrganisationPKInfo, expectedErrorWhenTirDeclaration);

				var orgAddressWithEori = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD2", traderTin: "TIN2832947234");
				header.Principal.OrganisationPK = orgAddressWithEori.Header.PK;
				AssertNoMessageErrorContaining("When EORI number is present", header.Principal.OrganisationPKInfo, expectedErrorWhenTirDeclaration);
			});

			const string expectedErrorForNonTirDeclaration = "[C236] Principal EORI is required if guarantees are used";
			header.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;

			CombineAssertions("When DeclarationType is not a TIR Declaration", () =>
			{
				var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD1", traderTin: "");
				header.Principal.OrganisationPK = orgAddress.Header.PK;
				AssertHasMessageErrorContaining("When EORI number is not present", header.Principal.OrganisationPKInfo, expectedErrorForNonTirDeclaration);

				var orgAddressWithEori = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "TD2", traderTin: "TIN2832947234");
				header.Principal.OrganisationPK = orgAddressWithEori.Header.PK;
				AssertNoMessageErrorContaining("When EORI number is present", header.Principal.OrganisationPKInfo, expectedErrorForNonTirDeclaration);
			});
		}

		void AssertBM_ReducedDatasetIndicatorInfo(bool errorIsExpected, Action<NctsHeader> setHeaderProperty)
		{
			const string errorMessage = "[R0350, R0352] An Authorization for Code=TRD is required when Reduced Data Set Indicator is Yes and Inland M.O.T. is one of these - 1(Sea Transport) or 2(Rail Transport) or 4(Air Transport).";
			var nctsHeader = GetNctsHeader();
			var movementHeader = nctsHeader.MovementHeader;
			setHeaderProperty(nctsHeader);
			movementHeader.Validation.ValidateBM_ReducedDatasetIndicator();
			if (errorIsExpected)
			{
				Assertion.AssertCollectionContains($"Expected notifications would contain {errorMessage}",
					errorMessage, movementHeader.BM_ReducedDatasetIndicatorInfo.Notifications.Select(e => e.Message));
			}
			else
			{
				Assertion.AssertCollectionNotContains($"Expected notifications would not contain {errorMessage}",
					errorMessage, movementHeader.BM_ReducedDatasetIndicatorInfo.Notifications.Select(e => e.Message));
			}
		}

		public void TestIsRuleActive()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, NctsHeaderValidationHelper.IsRuleActive((NctsHeader)null, _ => true));
				AssertEquals(false, NctsHeaderValidationHelper.IsRuleActive((NctsCommonMovementHeader)null, _ => true));
				var nctsHeader = GetNctsHeader();
				AssertEquals(true, NctsHeaderValidationHelper.IsRuleActive(nctsHeader, _ => true));
				AssertEquals(true, NctsHeaderValidationHelper.IsRuleActive(nctsHeader.MovementHeader, _ => true));
			});
		}

		public void TestCheckMandatoryArrivalMrn_Phase5() => CombineAssertions(() =>
		{
			const string message = "[TR0073] You need to supply a movement reference number (MRN).";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0073Active));
				RunTest("TR0073 inactive", false, ZString.Empty);

				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0073Active));
				RunTest("TR0073 active", true, ZString.Empty);

				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0073Active));
				RunTest("MRN not empty", false, "MRN123");
			}

			void RunTest(string assertionMessage, bool expectError, ZString mrn)
			{
				nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = mrn;
				nctsHeader.DestinationTrader.Validation.ValidateAll();
				if (expectError)
				{
					AssertHasMessageError(assertionMessage, nctsHeader.DestinationTrader.OrganisationPKInfo, message);
				}
				else
				{
					AssertNoMessageError(assertionMessage, nctsHeader.DestinationTrader.OrganisationPKInfo, message);
				}
			}
		});

		public void TestCheckMandatoryArrivalOffice_Phase5() => CombineAssertions(() =>
		{
			const string message = "[TR0075] You need to supply a destination customs office for Arrival that is supported.";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

			var arrivalMovement = nctsHeader.ArrivalMovementHeader;

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0075Active));
				RunTest("TR0075 inactive", false, ZString.Empty);

				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0075Active));
				RunTest("TR0075 active", true, ZString.Empty);

				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0075Active));
				RunTest("Office code not empty", false, "XX123456");
			}

			void RunTest(string assertionMessage, bool expectError, ZString destinationCustomsOfficeCodeForArrival)
			{
				arrivalMovement.DestinationCustomsOfficeCodeForArrival = destinationCustomsOfficeCodeForArrival;
				nctsHeader.DestinationTrader.Validation.ValidateAll();
				if (expectError)
				{
					AssertHasMessageError(assertionMessage, nctsHeader.DestinationTrader.OrganisationPKInfo, message);
				}
				else
				{
					AssertNoMessageError(assertionMessage, nctsHeader.DestinationTrader.OrganisationPKInfo, message);
				}
			}
		});

		public void TestIsInternalCommunityTransitProcedureType() => CombineAssertions(() =>
		{
			var internalCommunityTransitProcedureTypes = new[]
			{
				NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure,
				NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories,
			};

			var notInternalCommunityTransitProcedureTypes = new[]
			{
				NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure,
				NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4,
				NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5,
				NctsTypeOfDeclaration.Codes.T2PlusSanMarino,
				NctsTypeOfDeclaration.Codes.TirDeclaration,
				NctsTypeOfDeclaration.Codes.SGI,
				NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland,
			};

			foreach (var type in internalCommunityTransitProcedureTypes)
			{
				AssertEquals(type, true, NctsHeaderValidationHelper.IsInternalCommunityTransitProcedureType(type));
			}

			foreach (var type in notInternalCommunityTransitProcedureTypes)
			{
				AssertEquals(type, false, NctsHeaderValidationHelper.IsInternalCommunityTransitProcedureType(type));
			}
		});

		public void TestRuleTR0087() => CombineAssertions(() =>
		{
			var messageError = "[TR0087] " + MandatoryValidation.YouHaveNotEnteredMessage("Principal");
			var header = GetNctsHeader();
			var orgAddress = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "");

			using (var testContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory, typeof(IRuleTR0087Decider)))
			{
				_ = testContext.ClearCachedValidationDecider(header);
				testContext.EnableRuleDecider<IRuleTR0087Decider>(x => x.IsActive);

				header.Principal.E2_AddressOverride = ZBool.True;
				header.Principal.Validation.ValidateOrganisationPK();
				AssertNoMessageError("TR0087 active, E2_AddressOverride=true", header.Principal.OrganisationPKInfo, messageError);

				header.Principal.E2_AddressOverride = ZBool.False;
				header.Principal.Validation.ValidateOrganisationPK();
				AssertHasMessageError("TR0087 active, E2_AddressOverride=false", header.Principal.OrganisationPKInfo, messageError);

				header.Principal.OrganisationPK = orgAddress.Header.PK;
				AssertNoMessageError("TR0087 active, E2_AddressOverride=true", header.Principal.OrganisationPKInfo, messageError);

				testContext.DisableRuleDecider<IRuleTR0087Decider>(x => x.IsActive);
				header.Principal.OrganisationPK = ZGuid.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(header.Principal.OrganisationPKInfo, messageError, "TR0087 inactive");
			}
		});

		public void TestCheckNoOrMultipleGuaranteesErrorOnPrincipal()
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilySetUseGuaranteeGridValidation(Factory, true))
			{
				var principal = CreateOrgHeader("PRINCIPAL");
				var guaranteeHeader1 = CreateGuaranteeHeader("19860101", EUGuaranteeTypeList.Codes.COD);
				Factory.Save();

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
				nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

				AssertHasMessageError("No Guarantee found", nctsHeader.Principal.OrganisationPKInfo, "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee.");

				var refresher = nctsHeader.GuaranteeRefresher;
				guaranteeHeader1.CPH_OH_PermitHolder = principal.PK;

				refresher.PopulateGuaranteeWithFallbacks();
				AssertEquals("One Guarantee found", "19860101", nctsHeader.MovementHeader.Guarantees.Single().PW_BondNumber);
				AssertNoErrorContaining("One Guarantee found, No error on Principal", nctsHeader.Principal.OrganisationPKInfo, "There is no guarantee found for Principal, Declarant or Organization Proxy. Please select a guarantee.");

				var guaranteeHeader2 = CreateGuaranteeHeader("19860102", EUGuaranteeTypeList.Codes.COD);
				guaranteeHeader2.CPH_OH_PermitHolder = principal.PK;
				refresher.PopulateGuaranteeWithFallbacks();
				nctsHeader.Principal.Validation.ValidateOrganisationPK();

				AssertHasMessageErrorContaining("Multiple Guarantees found", nctsHeader.Principal.OrganisationPKInfo, "There are more than 1 guarantee found for Principal, Declarant or Organization Proxy,");
				AssertContainsExactElementsInAnyOrder(new ZString[] { "19860101", "19860102" }, nctsHeader.GuaranteeRefresher.totalGuarantees);
			}

			OrgHeader CreateOrgHeader(ZString code)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = code;
				const string regNo1 = "12345";
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, regNo1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				return orgHeader;
			}

			CusGuaranteeHeader CreateGuaranteeHeader(ZString guaranteeNumber, ZString guaranteeType)
			{
				var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				guaranteeHeader.CPH_Number = guaranteeNumber;
				guaranteeHeader.CPH_Type = guaranteeType;
				guaranteeHeader.CPH_SubType = "1";

				var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
				rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
				rule.CPR_ValueFrom = "#1";

				return guaranteeHeader;
			}
		}

		NctsHeader GetNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			nctsHeader.MovementHeader.BM_ReducedDatasetIndicator = true;

			const string codeToRaiseError = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
			var authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = codeToRaiseError;

			return nctsHeader;
		}

		NctsDepartureCargoDesc GetFirstTIRGoodsItem()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			return header.Bills.AddNew().GoodsItems.AddNew();
		}

		const string DocumentTypeRowErrorMessage = "A TIR declaration requires one Supporting Document of type 952 (TIR Carnet) on the first goods item(C901)";
	}
}
