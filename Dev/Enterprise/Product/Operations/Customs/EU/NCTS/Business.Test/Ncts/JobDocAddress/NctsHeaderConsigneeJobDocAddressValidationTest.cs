using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderConsigneeJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRuleC0001()
		{
			const string messageError = "[C0001] You have not entered Consignee. It is required either on Declaration or House Consignment.";

			using (var ruleTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(c => c.IsRuleC0001Active);
				ruleTestContext.DisableRule(c => c.IsRuleB1823Active);

				var movementHeader = CreateMovementHeader();
				var consignee = movementHeader.Header.Consignee;
				var nctsBill = movementHeader.Header.Bills.AddNew();
				var organisation = Factory.New<OrgHeader>();
				var nctsBillDocAddress = nctsBill.DocAddresses.FindOrCreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				var propertyInfo = consignee.OrganisationPKInfo;

				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When TP off", () =>
					{
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Empty BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						ruleTestContext.DisableRule(c => c.IsRuleC0001Active);
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule C0001 disabled", propertyInfo, messageError);
						ruleTestContext.EnableRule(c => c.IsRuleC0001Active);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);

						nctsBillDocAddress.OrganisationPK = organisation.PK;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("nctsBill consignee is not empty", propertyInfo, messageError);

						nctsBillDocAddress.OrganisationPK = ZGuid.Empty;
						var nctsHeaderDocAddress = movementHeader.Header.DocAddresses.FindOrCreateWithRequirement(movementHeader.Header.ConsigneeJobDocAddressRequirement);
						nctsHeaderDocAddress.OrganisationPK = organisation.PK;
						AssertNoMessageError("nctsHeader consignee is not empty", propertyInfo, messageError);

						nctsHeaderDocAddress.OrganisationPK = ZGuid.Empty;
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Australia;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Not EU and Not CTP BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}

				ruleTestContext.EnableRule(c => c.IsRuleB1823Active);
				using (TemporarilySetTransitionPeriod(true))
				{
					CombineAssertions("When TP on", () =>
					{
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}
			}
		}

		public void TestRuleC0001_NoNctsBill()
		{
			const string messageError = "[C0001] You have not entered Consignee. It is required either on Declaration or House Consignment.";

			using (var ruleTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(c => c.IsRuleC0001Active);
				ruleTestContext.DisableRule(c => c.IsRuleB1823Active);

				var movementHeader = CreateMovementHeader();
				var consignee = movementHeader.Header.Consignee;
				var organisation = Factory.New<OrgHeader>();
				var propertyInfo = consignee.OrganisationPKInfo;

				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When TP off", () =>
					{
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Empty BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						AssertHasMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						ruleTestContext.DisableRule(c => c.IsRuleC0001Active);
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule C0001 disabled", propertyInfo, messageError);
						ruleTestContext.EnableRule(c => c.IsRuleC0001Active);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						AssertHasMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);

						var nctsHeaderDocAddress = movementHeader.Header.DocAddresses.FindOrCreateWithRequirement(movementHeader.Header.ConsigneeJobDocAddressRequirement);
						nctsHeaderDocAddress.OrganisationPK = organisation.PK;
						AssertNoMessageError("nctsHeader consignee is not empty", propertyInfo, messageError);

						nctsHeaderDocAddress.OrganisationPK = ZGuid.Empty;
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Australia;
						AssertNoMessageError("Not EU and Not CTP BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}

				ruleTestContext.EnableRule(c => c.IsRuleB1823Active);
				using (TemporarilySetTransitionPeriod(true))
				{
					CombineAssertions("When TP on", () =>
					{
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}
			}
		}

		public void TestRuleB1823()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			const string messageErrorA = "[B1823] Consignee field must be empty";
			const string messageErrorB = "[B1823] Consignee field must be filled";

			using (var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				context.EnableRule(r => r.IsRuleB1823Active);

				var movementHeader = CreateMovementHeader();
				var consignee = movementHeader.Header.Consignee;
				var organisation = Factory.New<OrgHeader>();
				var propertyInfo = consignee.OrganisationPKInfo;
				movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
				var bill = movementHeader.Header.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();
				goodsItem.Consignee.OrganisationPK = ZGuid.BrettsGuid;
				var additionalInfo = goodsItem.AdditionalInfos.AddNew();

				using (TemporarilySetTransitionPeriod(true))
				{
					CombineAssertions("When TP on", () =>
					{
						consignee.OrganisationPK = ZGuid.BrettsGuid;
						AssertHasMessageError("Both nctsHeader and goodsItem consignee are filled", propertyInfo, messageErrorA);

						consignee.OrganisationPK = ZGuid.Empty;
						AssertNoMessageError("NctsHeader consignee is empty and goodsItem consignee is filled", propertyInfo, messageErrorA);

						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Destination port not in CL0009 list consignee", propertyInfo, messageErrorA);

						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
						goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("Both nctsHeader and goodsItem consignee are empty", propertyInfo, messageErrorB);

						consignee.OrganisationPK = ZGuid.BrettsGuid;
						AssertNoMessageError("NctsHeader consignee is filled and goodsItem consignee is empty", propertyInfo, messageErrorB);

						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;
						movementHeader.BM_TypeOfSecurity = "BTH";
						additionalInfo.CSI_Code = "30600";
						additionalInfo.CSI_SubType = "INF";
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("AdditionalDocument contains code '30600' and subtype 'INF, securityType 'BTH' and header consignee is filled", propertyInfo, messageErrorA);

						movementHeader.BM_TypeOfSecurity = "EXI";
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("AdditionalDocument contains code '30600' and subtype 'INF, securityType 'EXI' and header consignee is filled", propertyInfo, messageErrorA);

						consignee.OrganisationPK = ZGuid.Empty;
						AssertNoMessageError("AdditionalDocument contains code '30600' and subtype 'INF', header consignee is empty", propertyInfo, messageErrorA);
					});
				}

				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When TP off", () =>
					{
						consignee.OrganisationPK = ZGuid.BrettsGuid;
						AssertNoMessageError("AdditionalDocument contains code '30600' and subtype 'INF, securityType 'EXI' and header consignee is filled", propertyInfo, messageErrorA);

						movementHeader.BM_TypeOfSecurity = "BTH";
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("AdditionalDocument contains code '30600' and subtype 'INF, securityType 'BTH' and header consignee is filled", propertyInfo, messageErrorA);

						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
						goodsItem.Consignee.OrganisationPK = ZGuid.BrettsGuid;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Both nctsHeader and goodsItem consignee are filled", propertyInfo, messageErrorA);

						goodsItem.Consignee.OrganisationPK = ZGuid.Empty;
						consignee.OrganisationPK = ZGuid.Empty;
						AssertNoMessageError("Both nctsHeader and goodsItem consignee are empty", propertyInfo, messageErrorB);
					});
				}
			}
		}

		public void TestConsigneeJobDocAddress_WhenAdditionalDocumentType30600_RuleC0001_4()
		{
			const string expectedMessageError = "[C0001-4] Consignee must be empty";

			var movementHeader = CreateMovementHeader();
			var nctsHeader = movementHeader.Header;
			var nctsBill = nctsHeader.Bills.AddNew();
			var address = Factory.New<JobDocAddress>();

			using (var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				_ = context.ClearCachedValidationDecider(nctsHeader);
				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_4Active);

						var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						nctsHeader.Consignee.OrganisationPK = address.PK;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Header has additional documents type 30600, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						var billAdditionalInfo = Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, House consignment has additional documents type 30600, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, House consignment has additional documents type 30600, Header Consignee empty", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);
						nctsBill.AdditionalDocuments.RemoveAndDelete(billAdditionalInfo);

						nctsHeader.Consignee.OrganisationPK = address.PK;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, No Additional documents, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsHeader.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, No Additional documents, Header Consignee empty", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);

						context.DisableRule(r => r.IsRuleC0001_4Active);

						nctsHeader.Consignee.OrganisationPK = address.PK;
						headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, Header has additional documents type 30600, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);

						billAdditionalInfo = Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, House consignment has additional documents type 30600, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);
					});
				}

				using (TemporarilySetTransitionPeriod(true))
				{
					context.EnableRule(r => r.IsRuleC0001_4Active);

					CombineAssertions("During TP", () =>
					{
						nctsHeader.Consignee.OrganisationPK = address.PK;
						var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header has additional documents type 30600, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsHeader.Consignee.OrganisationPK = address.PK;
						var billAdditionalInfo = Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, House consignment has additional documents type 30600, Header Consignee filled", nctsHeader.Consignee.OrganisationPKInfo, expectedMessageError);
					});
				}
			}
		}

		public void TestRuleG0001_1()
		{
			const string messageError = "[G0001-1] Consignee must be empty";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var organisation = Factory.New<OrgHeader>();
			var bill = nctsHeader.Bills.AddNew();
			var consignee = nctsHeader.Consignee;
			var propertyInfo = consignee.OrganisationPKInfo;

			using (var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				_ = context.ClearCachedValidationDecider(nctsHeader);
				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleG0001_1Active);
						consignee.OrganisationPK = organisation.PK;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule G0001-1 is active and consignee is filled", propertyInfo, messageError);

						consignee.OrganisationPK = ZGuid.Empty;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule G0001-1 is active and consignee is empty", propertyInfo, messageError);

						consignee.OrganisationPK = organisation.PK;
						var headerAdditionalInfo = Add30600AdditionalInfo(bill.AdditionalDocuments);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("Rule G0001-1 is active and consignee is filled and HouseConsignment has AdditionalDocument where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);

						context.DisableRule(r => r.IsRuleG0001_1Active);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule G0001-1 is inactive and consignee is filled and HouseConsignment has AdditionalDocument where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);
					});
				}

				using (TemporarilySetTransitionPeriod(true))
				{
					CombineAssertions("During TP", () =>
					{
						var goodsItem = bill.GoodsItems.AddNew();
						context.EnableRule(r => r.IsRuleG0001_1Active);
						consignee.OrganisationPK = organisation.PK;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule G0001-1 is active and consignee is filled", propertyInfo, messageError);

						consignee.OrganisationPK = ZGuid.Empty;
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule G0001-1 is active and consignee is empty", propertyInfo, messageError);

						consignee.OrganisationPK = organisation.PK;
						var headerAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("Rule G0001-1 is active and consignee is filled and House consignments > Goods Items > Additional documents  where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);

						context.DisableRule(r => r.IsRuleG0001_1Active);
						nctsHeader.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule G0001-1 is inactive and consignee is filled and House consignments > Goods Items > Additional documents  where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);
					});
				}
			}
		}

		public void TestCheckConsignee_WithSecurityTypeDestinationCountryAndAdditionalDocuments_RuleC0001_6()
		{
			const string expectedWarning = "[C0001-6] This field will not be written in the message";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			var movementHeader = CreateMovementHeader();
			var consignee = movementHeader.Header.Consignee;
			var orgHeader = Factory.New<OrgHeader>();

			var bill1 = movementHeader.Header.Bills.AddNew();
			var targetPropertyInfo = consignee.OrganisationPKInfo;

			using (var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				_ = context.ClearCachedValidationDecider(movementHeader.Header);
				using (TemporarilySetTransitionPeriod(false))
				{
					context.EnableRule(r => r.IsRuleC0001_6Active);

					CombineAssertions("Rule C0001-6 active, outside TP", () =>
					{
						movementHeader.BM_TypeOfSecurity = "BTH";
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
						Add30600AdditionalInfo(movementHeader.Header.AdditionalDocuments);
						consignee.OrganisationPK = orgHeader.PK;
						AssertHasWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header has AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Australia;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort is in C0009, Header has AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.India;
						movementHeader.BM_TypeOfSecurity = "ENT";
						consignee.Validation.ValidateOrganisationPK();
						AssertNoWarning("When BM_TypeOfSecurity = 'ENT', DestinationPort not in C0009, Header has AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);

						movementHeader.BM_TypeOfSecurity = "BTH";
						consignee.OrganisationPK = ZGuid.Empty;
						AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header has AdditionalDocuments, Header consignee empty", targetPropertyInfo, expectedWarning);

						consignee.OrganisationPK = orgHeader.PK;
						movementHeader.Header.AdditionalDocuments.RemoveAndDeleteAll();
						AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header not contains AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.India;
						movementHeader.BM_TypeOfSecurity = "BTH";
						Add30600AdditionalInfo(bill1.AdditionalDocuments);
						consignee.Validation.ValidateOrganisationPK();
						AssertHasWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Bill1 has AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);

						var bill2 = movementHeader.Header.Bills.AddNew();
						consignee.Validation.ValidateOrganisationPK();
						AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Bill1 has AdditionalDocuments, Bill2 consignee empty with no AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);

						bill2.Consignee.OrganisationPK = orgHeader.PK;
						consignee.Validation.ValidateOrganisationPK();
						Add30600AdditionalInfo(bill2.AdditionalDocuments);

						AssertHasWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Bill1 has AdditionalDocuments, Bill2 consignee filled with AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);
					});

					context.DisableRule(r => r.IsRuleC0001_6Active);

					movementHeader.BM_TypeOfSecurity = "BTH";
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
					Add30600AdditionalInfo(movementHeader.Header.AdditionalDocuments);
					consignee.OrganisationPK = orgHeader.PK;
					consignee.Validation.ValidateOrganisationPK();

					AssertNoWarning("When Rule C0001-6 disabled, BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header has AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);
				}

				using (TemporarilySetTransitionPeriod(true))
				{
					context.EnableRule(r => r.IsRuleC0001_6Active);

					movementHeader.BM_TypeOfSecurity = "BTH";
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
					Add30600AdditionalInfo(movementHeader.Header.AdditionalDocuments);
					consignee.OrganisationPK = orgHeader.PK;
					consignee.Validation.ValidateOrganisationPK();

					AssertNoWarning("During TP, When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header has AdditionalDocuments, Header consignee filled", targetPropertyInfo, expectedWarning);
				}
			}
		}

		public void TestCheckConsignee_WhenHeaderDifferentValueAndAllBillsSameValue_RuleC0001_6()
		{
			var expectedMessage = "[C0001-6] The Consignee Address: Organization declared in the header will be ignored because all the house consignments have the same value, which differs from the value declared in the header.";

			var movementHeader = CreateMovementHeader();
			var consignee = movementHeader.Header.Consignee;
			var bill1 = movementHeader.Header.Bills.AddNew();
			var bill2 = movementHeader.Header.Bills.AddNew();
			movementHeader.BM_TypeOfSecurity = "BTH";

			var address1 = Factory.New<JobDocAddress>();
			var address2 = Factory.New<JobDocAddress>();

			var targetPropertyInfo = consignee.OrganisationPKInfo;

			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				_ = deciderTestContext.ClearCachedValidationDecider(movementHeader.Header);
				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When rule C0001-6 active", () =>
					{
						deciderTestContext.EnableRule(c => c.IsRuleC0001_6Active);

						consignee.OrganisationPK = address1.PK;
						AssertNoWarning("When Header filled and no Bills added", targetPropertyInfo, expectedMessage);

						consignee.OrganisationPK = ZGuid.Empty;
						bill1.Consignee.OrganisationPK = address2.PK;
						bill2.Consignee.OrganisationPK = address2.PK;
						AssertNoWarning("When Header empty and all Bills have same values", targetPropertyInfo, expectedMessage);

						consignee.OrganisationPK = address2.PK;
						AssertNoWarning("When Header filled and all Bills have same values as Header", targetPropertyInfo, expectedMessage);

						consignee.OrganisationPK = address1.PK;
						AssertHasWarning("When Header filled and all Bills have same values but different from Header", targetPropertyInfo, expectedMessage);

						bill1.Consignee.OrganisationPK = address1.PK;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoWarning("When Header filled and all Bills have different values", targetPropertyInfo, expectedMessage);

						bill1.Consignee.OrganisationPK = ZGuid.Empty;
						bill2.Consignee.OrganisationPK = ZGuid.Empty;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoWarning("When Header filled and all Bills empty", targetPropertyInfo, expectedMessage);
					});

					deciderTestContext.DisableRule(c => c.IsRuleC0001_6Active);

					consignee.OrganisationPK = address1.PK;
					bill1.Consignee.OrganisationPK = address2.PK;
					bill2.Consignee.OrganisationPK = address2.PK;
					consignee.Validation.ValidateOrganisationPK();
					AssertNoWarning("When rule C0001-6 disabled, Header filled and all Bills has same values", targetPropertyInfo, expectedMessage);
				}
			}
		}

		public void TestCheckConsignee_WhenHeaderDifferentValueAndBillsHaveDifferentValue_RuleC00001_6()
		{
			var expectedMessage = "[C0001-6] The Consignee Address: Organization declared in the header will be ignored because all house consignments have values, which different from the header one.";

			var movementHeader = CreateMovementHeader();
			var consignee = movementHeader.Header.Consignee;
			var bill1 = movementHeader.Header.Bills.AddNew();
			var bill2 = movementHeader.Header.Bills.AddNew();
			movementHeader.BM_TypeOfSecurity = "BTH";

			var address1 = Factory.New<JobDocAddress>();
			var address2 = Factory.New<JobDocAddress>();
			var address3 = Factory.New<JobDocAddress>();

			var targetPropertyInfo = consignee.OrganisationPKInfo;

			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				_ = deciderTestContext.ClearCachedValidationDecider(movementHeader.Header);
				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When rule C0001-6 active", () =>
					{
						deciderTestContext.EnableRule(c => c.IsRuleC0001_6Active);

						consignee.OrganisationPK = address1.PK;
						AssertNoWarning("When Header filled and no Bills added", targetPropertyInfo, expectedMessage);

						consignee.OrganisationPK = ZGuid.Empty;
						bill1.Consignee.OrganisationPK = address2.PK;
						bill2.Consignee.OrganisationPK = address3.PK;
						AssertNoWarning("When Header empty and all Bills have different values", targetPropertyInfo, expectedMessage);

						consignee.OrganisationPK = address1.PK;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasWarning("When Header filled and all Bills have different values than Header", targetPropertyInfo, expectedMessage);

						bill1.Consignee.OrganisationPK = ZGuid.Empty;
						bill2.Consignee.OrganisationPK = ZGuid.Empty;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoWarning("When Header filled and all Bills empty", targetPropertyInfo, expectedMessage);
					});

					deciderTestContext.DisableRule(c => c.IsRuleC0001_6Active);

					consignee.OrganisationPK = address1.PK;
					bill1.Consignee.OrganisationPK = address2.PK;
					bill2.Consignee.OrganisationPK = address3.PK;
					AssertNoWarning("When rule C0001-6 disabled, Header filled and all Bills have different values than Header", movementHeader.BM_RL_NKDestinationPortInfo, expectedMessage);
				}
			}
		}

		public void TestCheckConsignee_WhenHeaderAndBillEmpty_RuleC0001_6()
		{
			var expectedMessage = "[C0001-6] A Consignee Address: Organization must be declared at header or house level.";

			var movementHeader = CreateMovementHeader();
			var consignee = movementHeader.Header.Consignee;
			var bill1 = movementHeader.Header.Bills.AddNew();
			var bill2 = movementHeader.Header.Bills.AddNew();
			movementHeader.BM_TypeOfSecurity = "BTH";

			var address1 = Factory.New<JobDocAddress>();

			var targetPropertyInfo = consignee.OrganisationPKInfo;

			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				_ = deciderTestContext.ClearCachedValidationDecider(movementHeader.Header);
				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When rule C0001-6 active", () =>
					{
						deciderTestContext.EnableRule(c => c.IsRuleC0001_6Active);

						bill1.Consignee.OrganisationPK = address1.PK;
						bill2.Consignee.OrganisationPK = address1.PK;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header empty and all Bills filled", targetPropertyInfo, expectedMessage);

						bill1.Consignee.OrganisationPK = ZGuid.Empty;
						bill2.Consignee.OrganisationPK = ZGuid.Empty;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("When Header empty and all Bills empty", targetPropertyInfo, expectedMessage);

						consignee.OrganisationPK = address1.PK;
						AssertNoMessageError("When Header filled and all Bills empty", targetPropertyInfo, expectedMessage);
					});

					deciderTestContext.DisableRule(c => c.IsRuleC0001_6Active);

					consignee.OrganisationPK = ZGuid.Empty;
					consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When rule C0001-6 disabled, Header empty and all Bills empty", targetPropertyInfo, expectedMessage);
				}
			}
		}

		AdditionalInfo Add30600AdditionalInfo(ICusSupportingInfoCollection<AdditionalInfo> additionalInfoCollection)
		{
			var additionalInfo = additionalInfoCollection.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

			return additionalInfo;
		}

		NctsDepartureMovementHeader CreateMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader;
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);
	}
}
