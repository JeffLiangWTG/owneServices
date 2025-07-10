using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ProcessManagement.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccPlaceOfSupplyHelperTest : TestCaseWithFactory
	{
		#region PlaceOfSupplyTaxRegistrationCode()

		public void TestPlaceOfSupplyTaxRegistrationCode_ReturnsEmpty_WhenNullOrg()
		{
			var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(null);
			AssertEquals(ZString.Empty, regCode);
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_ReturnsUnregistered_WhenOrgIsLocalAndHasNoVATRegistration()
		{
			CreatePlaceOfSupplyTestObjects();
			var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(UnregisteredLocalOrg, GlbCompany.CurrentCompany);
			AssertEquals("NON", regCode);
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_ReturnsForeignOrganization_WhenOrgIsForeignAndHasNoVATRegistration()
		{
			CreatePlaceOfSupplyTestObjects();
			var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(UnregisteredForeignOrg, GlbCompany.CurrentCompany);
			AssertEquals("FRO", regCode);
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_ReturnsLocalCustomer_WhenOrgIsLocalAndHasVATRegistration()
		{
			CreatePlaceOfSupplyTestObjects();
			var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(RegisteredLocalOrg, GlbCompany.CurrentCompany);
			AssertEquals("LOC", regCode);
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_ReturnsLocalCustomer_WhenOrgIsForeignAndHasVATRegistration()
		{
			CreatePlaceOfSupplyTestObjects();
			var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(RegisteredForeignOrg, GlbCompany.CurrentCompany);
			AssertEquals("LOC", regCode);
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_ReturnsLocalCustomer_WhenUsingDifferentCompany()
		{
			CreatePlaceOfSupplyTestObjects();
			RegisteredLocalOrg.OH_RL_NKClosestPort = "NZAKL";
			RegisteredLocalOrg.CustomsCodes[0].OK_RN_NKCodeCountry = "NZ";
			RegisteredLocalOrg.CustomsCodes[0].OK_CodeType = "GST";

			var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(RegisteredLocalOrg, NZCompany);
			AssertEquals("LOC", regCode);
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_WhenOrgIsLocalExcludingOrgCusCode()
		{
			CreatePlaceOfSupplyTestObjects();
			AssertPlaceOfSupplyTaxRegistrationCode_IncludeOrgCusCode(RegisteredLocalOrg, false, "NON");
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_WhenOrgIsLocalIncludingOrgCusCode()
		{
			CreatePlaceOfSupplyTestObjects();
			AssertPlaceOfSupplyTaxRegistrationCode_IncludeOrgCusCode(RegisteredLocalOrg, true, "LOC");
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_WhenOrgIsForeignExcludingOrgCusCode()
		{
			CreatePlaceOfSupplyTestObjects();
			AssertPlaceOfSupplyTaxRegistrationCode_IncludeOrgCusCode(RegisteredForeignOrg, false, "FRO");
		}

		public void TestPlaceOfSupplyTaxRegistrationCode_WhenOrgIsForeignIncludingOrgCusCode()
		{
			CreatePlaceOfSupplyTestObjects();
			AssertPlaceOfSupplyTaxRegistrationCode_IncludeOrgCusCode(RegisteredForeignOrg, true, "LOC");
		}

		void AssertPlaceOfSupplyTaxRegistrationCode_IncludeOrgCusCode(OrgHeader organisation, bool includeOrgCusCodeForTaxRegsistration, string expectedRegCode)
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockOrgCusCodePredicateProvider = new Mock<IOrgCusCodePredicateProvider>();

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				mockICountryComplianceFactory.Setup(x => x.GetIOrgCusCodePredicateProvider(It.IsAny<ZString>())).Returns(mockOrgCusCodePredicateProvider.Object);
				mockOrgCusCodePredicateProvider.Setup(x => x.IncludeForPlaceOfSupplyTaxRegsistration(It.IsAny<OrgCusCode>())).Returns(includeOrgCusCodeForTaxRegsistration);

				var regCode = AccPlaceOfSupplyHelper.PlaceOfSupplyTaxRegistrationCode(organisation, GlbCompany.CurrentCompany);
				AssertEquals(expectedRegCode, regCode);
			}
		}

		#endregion

		#region LookupPlaceOfSupplyRule()

		// See Excel Spreadsheet "FPOS Rule Test Cases.xlsx" in WI00339552 eDocs for an easier to read table.

		public void TestLookupPlaceOfSupplyRule_MatchingOnChargeCodeLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 2: EDI|GRP|BAF|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|NON|BNE|", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 3: EDI|GRP|BAF|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|NON||", actualRule.LookupKey);

				// Row 4: EDI|GRP|BAF|SHP|REV|IMP|FRO|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|||", actualRule.LookupKey);

				// Row 5: EDI|GRP|BAF|SHP|REV|EXP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ExportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||ALL|ALL|||", actualRule.LookupKey);

				// Row 6: EDI|GRP|BAF|SHP|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_MatchingOnChargeCodeLevel_WithCatchAllRuleForChargeCode()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForChargeCodeLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 7: EDI|GRP|BAF|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|CatchAllForChargeCode
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_MatchingOnGroupLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 8: EDI|GRP|FRT|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV||IMP|ALL|NON|BNE|", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 9: EDI|GRP|FRT|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV||IMP|ALL|NON||", actualRule.LookupKey);

				// Row 10: EDI|GRP|FRT|SHP|REV|IMP|FRO|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV||IMP|ALL|||", actualRule.LookupKey);

				// Row 11: EDI|GRP|FRT|SHP|REV|EXP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ExportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV||ALL|ALL|||", actualRule.LookupKey);

				// Row 12: EDI|GRP|FRT|SHP|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_MatchingOnGroupLevel_WithCatchAllRuleForGroup()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForGroupLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 13: EDI|GRP|FRT|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|CatchAllForChargeCode
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_MatchingOnCompanyLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 14: EDI|GRP2|OLAB|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV||IMP|ALL|NON|BNE|", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 15: EDI|GRP2|OLAB|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV||IMP|ALL|NON||", actualRule.LookupKey);

				// Row 16: EDI|GRP2|OLAB|SHP|REV|IMP|FRO|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV||IMP|ALL|||", actualRule.LookupKey);

				// Row 17: EDI|GRP2|OLAB|SHP|REV|EXP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ExportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV||ALL|ALL|||", actualRule.LookupKey);

				// Row 18: EDI|GRP2|OLAB|SHP|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|ALL||ALL|ALL|||", actualRule.LookupKey);

				// Row 19: EDI|GRP2|OLAB|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNull(actualRule);
			}
		}

		public void TestLookupPlaceOfSupplyRule_MatchingOnCompanyLevel_WithCatchAllRuleForCompany()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForCompanyLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 20: EDI|GRP2|OLAB|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|CatchAllForChargeCode
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_MatchingOnDifferentCompany_ReturnsNull()
		{
			CreatePlaceOfSupplyTestObjects();
			var indiaChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			indiaChargeCode.AC_Code = "BAFIN";
			indiaChargeCode.AC_GC = INCompany.PK;
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			// Row 21: DIN||BAFIN|FCN|COS|IMP|NON||BranchRegistryDisabled|NoCatchAll
			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(indiaChargeCode, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertNull(actualRule);
		}

		public void TestLookupPlaceOfSupplyRule_ForOtherJobType_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			// Row 22: DIN|GRP|BAF|GCN|COS|IMP|NON||BranchRegistryDisabled|NoCatchAll
			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, GatewayConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertNull(actualRule);
		}

		public void TestLookupPlaceOfSupplyRule_ForOtherJobType_WithChargeCodeCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForChargeCodeLevel: true);
			Factory.Save();

			// Row 23: DIN|GRP|BAF|GCN|COS|IMP|NON||BranchRegistryDisabled|NoCatchAll
			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, GatewayConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertEquals("EDI|ChargeCode:BAF|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
		}

		public void TestLookupPlaceOfSupplyRule_ForOtherJobType_WithGroupCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForGroupLevel: true);
			Factory.Save();

			// Row 24: DIN|GRP|BAF|GCN|COS|IMP|NON||BranchRegistryDisabled|NoCatchAll
			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, GatewayConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
		}

		public void TestLookupPlaceOfSupplyRule_ForOtherJobType_WithCompanyCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForCompanyLevel: true);
			Factory.Save();

			// Row 25: DIN|GRP|BAF|GCN|COS|IMP|NON||BranchRegistryDisabled|NoCatchAll
			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, GatewayConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
		}

		public void TestLookupPlaceOfSupplyRule_ForJobNotImplementingIJobInvoicingPlugIn_WithCompanyCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet(includeCatchAllRuleForCompanyLevel: true);
			Factory.Save();

			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ConsolidatedTransportBookingAsPlugin, UnregisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertNull("An unsupported Consol that does not implement IJobInvoicingPlugIn always returns null", actualRule);
		}

		public void TestLookupPlaceOfSupplyRule_MatchingWithoutBranch()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 26 EDI|GRP|BAF|SHP|REV|IMP|NON||BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: ZString.Empty);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|NON||", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 27: EDI|GRP|BAF|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: ZString.Empty);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|NON||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnChargeCodeLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|REV||ALL|ALL|NON|BNE|", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|REV||ALL|ALL|NON||", actualRule.LookupKey);

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|REV||ALL|ALL|||", actualRule.LookupKey);

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNull(actualRule);
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnChargeCodeLevel_WithCatchAllRuleForChargeCode()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob(includeCatchAllRuleForChargeCodeLevel: true);
			Factory.Save();

			var dummyTracer = GetDummyTracer();
			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);

				var actualTraceMsg = string.Join("", dummyTracer.Traces);

				#region Expected Trace

				const string expectedTrace = @"
TRACED: Charge Code
DETAILS:
{
  ""companyCode"": ""EDI"",
  ""chargeCode"": ""BAF"",
  ""chargeCodeDescription"": ""Bunker Adjustment Factor""
}

TRACED: Charge Code Group
DETAILS:
{
  ""companyCode"": ""EDI"",
  ""chargeCodeGroup"": ""GRP"",
  ""chargeCodeGroupDescription"": ""GRP Description""
}

TRACED: POS Configuration selection parameters
DETAILS:
{
  ""groupCode"": ""GRP"",
  ""jobType"": ""ALL"",
  ""chargeType"": ""COS"",
  ""transportMode"": """",
  ""incoTerms"": """",
  ""direction"": ""ALL"",
  ""taxRegistration"": ""NON"",
  ""supplyType"": ""LOC"",
  ""branchCode"": """"
}

TRACED: POS Configurations for Charge Code 'BAF'
DETAILS:
[
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": ""NON"",
    ""supplyType"": """",
    ""branchCode"": ""BNE"",
    ""levelCode"": ""BAF"",
    ""levelName"": ""Charge Code"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": ""NON"",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""BAF"",
    ""levelName"": ""Charge Code"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": """",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""BAF"",
    ""levelName"": ""Charge Code"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""ALL"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": """",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""BAF"",
    ""levelName"": ""Charge Code"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": ""NON"",
    ""supplyType"": """",
    ""branchCode"": ""BNE"",
    ""levelCode"": ""GRP"",
    ""levelName"": ""Charge Code Group"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": ""NON"",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""GRP"",
    ""levelName"": ""Charge Code Group"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": """",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""GRP"",
    ""levelName"": ""Charge Code Group"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": ""NON"",
    ""supplyType"": """",
    ""branchCode"": ""BNE"",
    ""levelCode"": ""EDI"",
    ""levelName"": ""Company"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": ""NON"",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""EDI"",
    ""levelName"": ""Company"",
    ""posRule"": ""BIL""
  },
  {
    ""jobType"": ""ALL"",
    ""chargeType"": ""REV"",
    ""transportMode"": ""ALL"",
    ""incoTerms"": """",
    ""direction"": ""ALL"",
    ""taxRegistration"": """",
    ""supplyType"": """",
    ""branchCode"": """",
    ""levelCode"": ""EDI"",
    ""levelName"": ""Company"",
    ""posRule"": ""BIL""
  }
]

TRACED: Best matching POS Configuration
DETAILS:
{
  ""jobType"": ""ALL"",
  ""chargeType"": ""ALL"",
  ""transportMode"": ""ALL"",
  ""incoTerms"": """",
  ""direction"": ""ALL"",
  ""taxRegistration"": """",
  ""supplyType"": """",
  ""branchCode"": """",
  ""levelCode"": ""BAF"",
  ""levelName"": ""Charge Code"",
  ""posRule"": ""BIL""
}"
				;

				#endregion

				AssertMultilineASCIIEquals(expectedTrace, actualTraceMsg);
				dummyTracer.Traces.Clear();

				#region ExpectedSecondRunTraceWithoutAlreadyLoadedAndCachedChargeCodePOSConfigurations

				const string expectedSecondRunTraceWithoutAlreadyLoadedAndCachedChargeCodePOSConfigurations = @"
TRACED: Charge Code
DETAILS:
{
  ""companyCode"": ""EDI"",
  ""chargeCode"": ""BAF"",
  ""chargeCodeDescription"": ""Bunker Adjustment Factor""
}

TRACED: Charge Code Group
DETAILS:
{
  ""companyCode"": ""EDI"",
  ""chargeCodeGroup"": ""GRP"",
  ""chargeCodeGroupDescription"": ""GRP Description""
}

TRACED: POS Configuration selection parameters
DETAILS:
{
  ""groupCode"": ""GRP"",
  ""jobType"": ""ALL"",
  ""chargeType"": ""COS"",
  ""transportMode"": """",
  ""incoTerms"": """",
  ""direction"": ""ALL"",
  ""taxRegistration"": ""NON"",
  ""supplyType"": ""LOC"",
  ""branchCode"": """"
}

TRACED: Best matching POS Configuration
DETAILS:
{
  ""jobType"": ""ALL"",
  ""chargeType"": ""ALL"",
  ""transportMode"": ""ALL"",
  ""incoTerms"": """",
  ""direction"": ""ALL"",
  ""taxRegistration"": """",
  ""supplyType"": """",
  ""branchCode"": """",
  ""levelCode"": ""BAF"",
  ""levelName"": ""Charge Code"",
  ""posRule"": ""BIL""
}";

				#endregion

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertMultilineASCIIEquals(expectedSecondRunTraceWithoutAlreadyLoadedAndCachedChargeCodePOSConfigurations, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnGroupLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|REV||ALL|ALL|NON|BNE|", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|REV||ALL|ALL|NON||", actualRule.LookupKey);

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|REV||ALL|ALL|||", actualRule.LookupKey);

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNull(actualRule);
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnGroupLevel_WithCatchAllRuleForGroup()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob(includeCatchAllRuleForGroupLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnCompanyLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|ALL|REV||ALL|ALL|NON|BNE|", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|ALL|REV||ALL|ALL|NON||", actualRule.LookupKey);

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|ALL|REV||ALL|ALL|||", actualRule.LookupKey);

				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNull(actualRule);
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnCompanyLevel_WithCatchAllRuleForCompany()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob(includeCatchAllRuleForCompanyLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingOnDifferentCompany_ReturnsNull()
		{
			CreatePlaceOfSupplyTestObjects();
			var indiaChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			indiaChargeCode.AC_Code = "BAFIN";
			indiaChargeCode.AC_GC = INCompany.PK;
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob();
			Factory.Save();

			var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(indiaChargeCode, CostSell.Cost, UnregisteredLocalOrg, supplyType: ZString.Empty);
			AssertNull(actualRule);
		}

		public void TestLookupPlaceOfSupplyRuleNoJob_MatchingWithoutBranch()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSetForNoJob();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Revenue, UnregisteredLocalOrg, supplyType: ZString.Empty);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|REV||ALL|ALL|NON||", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, CostSell.Revenue, UnregisteredLocalOrg, supplyType: ZString.Empty);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|REV||ALL|ALL|NON||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_IsCached_ForLifetimeOfFactory()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// EDI|GRP|BAF|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var loadCountBefore = Factory.DatabaseLoadCount;
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				var loadCountAfter = Factory.DatabaseLoadCount;
				var dbLoads = loadCountAfter - loadCountBefore;
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|NON||", actualRule.LookupKey);
				AssertGreaterThan("On first lookup there should be at least one DB load", dbLoads, 0);

				loadCountBefore = Factory.DatabaseLoadCount;
				var actualRule2 = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				loadCountAfter = Factory.DatabaseLoadCount;
				dbLoads = loadCountAfter - loadCountBefore;
				AssertNotNull(actualRule2);
				AssertEquals(actualRule, actualRule2);
				AssertEquals("Caching by factory should prevent DB Loads for the same charge code", 0, dbLoads);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccChargeCodeSchema.AC_Code, "BAF");
				var chargeCodeBAFInNewFactory = newFactory.LoadTop1<AccChargeCode>(query);

				loadCountBefore = newFactory.DatabaseLoadCount;
				var actualRuleFromNewFactory = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(chargeCodeBAFInNewFactory, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				loadCountAfter = newFactory.DatabaseLoadCount;
				dbLoads = loadCountAfter - loadCountBefore;
				AssertNotNull(actualRuleFromNewFactory);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV||IMP|ALL|NON||", actualRuleFromNewFactory.LookupKey);
				AssertEquals("Same rule should be loaded when using different factories", actualRule.PK, actualRuleFromNewFactory.PK);
				AssertGreaterThan("On first lookup in new factory there should be at least one DB load", dbLoads, 0);
			}
		}

		#region Extended Matching Criterias

		public void TestLookupPlaceOfSupplyRule_ExtendedMatchingOnChargeCodeLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationExtendedRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 2: EDI|GRP|BAF|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV|FOB|IMP|SEA|NON|BNE|LOC", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 3: EDI|GRP|BAF|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV|FOB|IMP|SEA|NON||", actualRule.LookupKey);

				// Row 4: EDI|GRP|BAF|SHP|REV|IMP|FRO|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV|FOB|IMP|SEA|||", actualRule.LookupKey);

				// Row 5: EDI|GRP|BAF|SHP|REV|EXP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ExportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|REV|FOB|ALL|ALL|||", actualRule.LookupKey);

				// Row 6: EDI|GRP|BAF|SHP|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ImportShipmentAsPlugin, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|SHP|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_ExtendedMatchingOnChargeCodeLevel_WithCatchAllRuleForChargeCode()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationExtendedRuleSet(includeCatchAllRuleForChargeCodeLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 7: EDI|GRP|BAF|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|CatchAllForChargeCode
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeBAF, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCode:BAF|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_ExtendedMatchingOnGroupLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationExtendedRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 8: EDI|GRP|FRT|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV|FOB|IMP|SEA|NON|BNE|LOC", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 9: EDI|GRP|FRT|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV|FOB|IMP|SEA|NON||", actualRule.LookupKey);

				// Row 10: EDI|GRP|FRT|SHP|REV|IMP|FRO|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV|FOB|IMP|SEA|||", actualRule.LookupKey);

				// Row 11: EDI|GRP|FRT|SHP|REV|EXP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ExportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|REV|FOB|ALL|ALL|||", actualRule.LookupKey);

				// Row 12: EDI|GRP|FRT|SHP|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ImportShipmentAsPlugin, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|SHP|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_ExtendedMatchingOnGroupLevel_WithCatchAllRuleForGroup()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationExtendedRuleSet(includeCatchAllRuleForGroupLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 13: EDI|GRP|FRT|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|CatchAllForChargeCode
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeFRT, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|ChargeCodeGroup:GRP|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		public void TestLookupPlaceOfSupplyRule_ExtendedMatchingOnCompanyLevel_WithoutCatchAllRule()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationExtendedRuleSet();
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				// Row 14: EDI|GRP2|OLAB|SHP|REV|IMP|NON|BNE|BranchRegistryEnabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV|FOB|IMP|SEA|NON|BNE|LOC", actualRule.LookupKey);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 15: EDI|GRP2|OLAB|SHP|REV|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV|FOB|IMP|SEA|NON||", actualRule.LookupKey);

				// Row 16: EDI|GRP2|OLAB|SHP|REV|IMP|FRO|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Revenue, UnregisteredForeignOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV|FOB|IMP|SEA|||", actualRule.LookupKey);

				// Row 17: EDI|GRP2|OLAB|SHP|REV|EXP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ExportShipmentAsPlugin, CostSell.Revenue, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|REV|FOB|ALL|ALL|||", actualRule.LookupKey);

				// Row 18: EDI|GRP2|OLAB|SHP|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ImportShipmentAsPlugin, CostSell.Cost, UnregisteredLocalOrg, supplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|SHP|ALL||ALL|ALL|||", actualRule.LookupKey);

				// Row 19: EDI|GRP2|OLAB|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|NoCatchAll
				actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNull(actualRule);
			}
		}

		public void TestLookupPlaceOfSupplyRule_ExtendedMatchingOnCompanyLevel_WithCatchAllRuleForCompany()
		{
			CreatePlaceOfSupplyTestObjects();
			CreatePlaceOfSupplyConfigurationExtendedRuleSet(includeCatchAllRuleForCompanyLevel: true);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				// Row 20: EDI|GRP2|OLAB|FCN|COS|IMP|NON|BNE|BranchRegistryDisabled|CatchAllForChargeCode
				var actualRule = AccPlaceOfSupplyHelper.LookupPlaceOfSupplyRule(ChargeCodeOLAB, ForwardingConsolAsPlugin, UnregisteredLocalOrg, costSupplyType: SupplyTypeClassificationCodes.LOC, branch: GlbBranch.CurrentBranch);
				AssertNotNull(actualRule);
				AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||", actualRule.LookupKey);
			}
		}

		#endregion

		#region Helpers

		void CreatePlaceOfSupplyConfigurationRuleSet(
			bool includeCatchAllRuleForCompanyLevel = false,
			bool includeCatchAllRuleForGroupLevel = false,
			bool includeCatchAllRuleForChargeCodeLevel = false)
		{
			AssertEquals("Precondition: current country is Australia", "AU", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			// See Excel Spreadsheet "FPOS Rule Test Cases.xlsx" in WI00339552 eDocs for an easier to read table.

			CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", branch: GlbBranch.CurrentBranch);
			CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON");
			CreateConfigurationRule("SHP", "REV", "IMP");
			CreateConfigurationRule("SHP", "REV", "ALL");
			CreateConfigurationRule("SHP", "ALL", "ALL");
			if (includeCatchAllRuleForCompanyLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL");
			}

			CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", branch: GlbBranch.CurrentBranch, group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "IMP", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "ALL", group: GroupGRP);
			CreateConfigurationRule("SHP", "ALL", "ALL", group: GroupGRP);
			if (includeCatchAllRuleForGroupLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL", group: GroupGRP);
			}

			CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", branch: GlbBranch.CurrentBranch, chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "IMP", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "ALL", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "ALL", "ALL", chargeCode: ChargeCodeBAF);
			if (includeCatchAllRuleForChargeCodeLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL", chargeCode: ChargeCodeBAF);
			}

			CreateConfigurationRule("SHP", "ALL", "ALL", company: NZCompany);
		}

		void CreatePlaceOfSupplyConfigurationExtendedRuleSet(
			bool includeCatchAllRuleForCompanyLevel = false,
			bool includeCatchAllRuleForGroupLevel = false,
			bool includeCatchAllRuleForChargeCodeLevel = false)
		{
			AssertEquals("Precondition: current country is Australia", "AU", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			// See Excel Spreadsheet "FPOS Rule Test Cases.xlsx" in WI00339552 eDocs for an easier to read table.

			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branch: GlbBranch.CurrentBranch, supplyType: "LOC");
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branch: GlbBranch.CurrentBranch);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON");
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA");
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB");
			CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB");
			CreateConfigurationRule("SHP", "REV", "ALL");
			CreateConfigurationRule("SHP", "ALL", "ALL");
			if (includeCatchAllRuleForCompanyLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL");
			}

			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branch: GlbBranch.CurrentBranch, supplyType: "LOC", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branch: GlbBranch.CurrentBranch, group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB", group: GroupGRP);
			CreateConfigurationRule("SHP", "REV", "ALL", group: GroupGRP);
			CreateConfigurationRule("SHP", "ALL", "ALL", group: GroupGRP);
			if (includeCatchAllRuleForGroupLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL", group: GroupGRP);
			}

			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branch: GlbBranch.CurrentBranch, supplyType: "LOC", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branch: GlbBranch.CurrentBranch, chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "REV", "ALL", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("SHP", "ALL", "ALL", chargeCode: ChargeCodeBAF);
			if (includeCatchAllRuleForChargeCodeLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL", chargeCode: ChargeCodeBAF);
			}

			CreateConfigurationRule("SHP", "ALL", "ALL", company: NZCompany);
		}

		void CreatePlaceOfSupplyConfigurationRuleSetForNoJob(
			bool includeCatchAllRuleForCompanyLevel = false,
			bool includeCatchAllRuleForGroupLevel = false,
			bool includeCatchAllRuleForChargeCodeLevel = false)
		{
			AssertEquals("Precondition: current country is Australia", "AU", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			// See Excel Spreadsheet "FPOS Rule Test Cases.xlsx" in WI00339552 eDocs for an easier to read table.

			CreateConfigurationRule("ALL", "REV", "ALL", taxReg: "NON", branch: GlbBranch.CurrentBranch);
			CreateConfigurationRule("ALL", "REV", "ALL", taxReg: "NON");
			CreateConfigurationRule("ALL", "REV", "ALL");
			if (includeCatchAllRuleForCompanyLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL");
			}

			CreateConfigurationRule("ALL", "REV", "ALL", taxReg: "NON", branch: GlbBranch.CurrentBranch, group: GroupGRP);
			CreateConfigurationRule("ALL", "REV", "ALL", taxReg: "NON", group: GroupGRP);
			CreateConfigurationRule("ALL", "REV", "ALL", group: GroupGRP);
			if (includeCatchAllRuleForGroupLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL", group: GroupGRP);
			}

			CreateConfigurationRule("ALL", "REV", "ALL", taxReg: "NON", branch: GlbBranch.CurrentBranch, chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("ALL", "REV", "ALL", taxReg: "NON", chargeCode: ChargeCodeBAF);
			CreateConfigurationRule("ALL", "REV", "ALL", chargeCode: ChargeCodeBAF);
			if (includeCatchAllRuleForChargeCodeLevel)
			{
				CreateConfigurationRule("ALL", "ALL", "ALL", chargeCode: ChargeCodeBAF);
			}

			CreateConfigurationRule("ALL", "ALL", "ALL", company: NZCompany);
		}

		#endregion

		#endregion

		#region GetPlaceOfSupplyFromILocation()

		public void TestGetPlaceOfSupplyFromILocation_ReturnsEmpty_WhenNullLocation()
		{
			var dummyTracer = GetDummyTracer();
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(null, null);

				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);

				const string expectedTrace = @"
TRACED: Get POS from Location - Empty Location
DETAILS:
{""posTypeDescription"":"""",""posType"":"""",""posCode"":""""}";

				AssertEquals(expectedTrace, dummyTracer.Traces[0]);
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsEmpty_WhenEmptyLocation()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
				ChargeCodeFRT = Factory.LoadTop1<AccChargeCode>(query);

				UnregisteredLocalOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Revenue, UnregisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(location, null);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsEmpty_WhenRegistryDisabled()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly())
			{
				Assert("Precondition: Place Of Supply is disabled.", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var location = CreateAUBNELocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(location, GlbCompany.CurrentCompany);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsState_WhenStateSetInLocationAndRegistryEnabled()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				var location = CreateAUBNELocation();

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(location, GlbCompany.CurrentCompany);
				AssertEquals(PlaceOfSupplyTypes.State.Code, posType);
				AssertEquals("QLD", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsEmpty_WhenDifferentCountry()
		{
			var indiaCompany = CreateCompany(CountryCodes.India);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.State.Code))
			{
				var location = CreateAUBNELocation();

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(location, indiaCompany);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsCountry_WhenRegistryEnabled()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.Country.Code))
			{
				var location = CreateAUBNELocation();

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(location, GlbCompany.CurrentCompany);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("AU", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsCountry_WhenDifferentCompany()
		{
			var indiaCompany = CreateCompany(CountryCodes.India);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.Country.Code))
			{
				var australiaLocation = CreateAUBNELocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(australiaLocation, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("AU", posCode);

				var usaLocation = CreateUSELPLocation();
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(usaLocation, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("US", posCode);

				var indiaLocation = CreateINPLSLocation();
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(indiaLocation, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("IN", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsTaxZone_WhenRegistryEnabled()
		{
			var canadaCompany = CreateCompany(CountryCodes.Canada);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(canadaCompany, PlaceOfSupplyTypes.TaxZone.Code))
			{
				var britishColumbiaLocation = CreateCAHLCLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, canadaCompany);
				AssertEquals(PlaceOfSupplyTypes.TaxZone.Code, posType);
				AssertEquals("BCTZ", posCode);

				var quebecLocation = CreateCABTVLocation();
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(quebecLocation, canadaCompany);
				AssertEquals(PlaceOfSupplyTypes.TaxZone.Code, posType);
				AssertEquals("QUBC", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsTaxZone_WhenDifferentCompany()
		{
			var indiaCompany = CreateCompany(CountryCodes.India);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.TaxZone.Code))
			{
				var britishColumbiaLocation = CreateCAHLCLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.TaxZone.Code, posType);
				AssertEquals("BCTZ", posCode);

				var quebecLocation = CreateCABTVLocation();
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(quebecLocation, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.TaxZone.Code, posType);
				AssertEquals("QUBC", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsEmpty_WhenNoZoneForLocation()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var indiaLocation = CreateINPLSLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(indiaLocation, GlbCompany.CurrentCompany);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsFirstTaxZone_WhenMany()
		{
			var canadaCompany = CreateCompany(CountryCodes.Canada);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(canadaCompany, PlaceOfSupplyTypes.TaxZone.Code))
			{
				var multiZoneLocation = CreateCAESSLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(multiZoneLocation, canadaCompany);
				AssertEquals(PlaceOfSupplyTypes.TaxZone.Code, posType);
				AssertEquals("ONTZ", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsPredefinedRule_WhenRegistryEnabled()
		{
			var indiaCompany = CreateCompany(CountryCodes.India);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				var britishColumbiaLocation = CreateCAHLCLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, posType);
				AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsEmpty_WhenPredefinedRuleCountryIsSame()
		{
			var indiaCompany = CreateCompany(CountryCodes.India);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				var indiaLocation = CreateINPLSLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(indiaLocation, indiaCompany);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsOtherTerritories_WhenLocationIsOtherTerritoriesLocation()
		{
			var company = CreateCompany(CountryCodes.Australia);
			var dummyTracer = GetDummyTracer();
			AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(company, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				var otherTerritories = new LocationRule(company, PlaceOfSupplyListProvider.Codes.OtherTerritories);
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(otherTerritories, company);
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, posType);
				AssertEquals(PlaceOfSupplyListProvider.Codes.OtherTerritories, posCode);

				const string expectedTrace = @"
TRACED: Get POS from Location
DETAILS:
{""posTypeDescription"":""Predefined Rule in CW1"",""posType"":""RUL"",""posCode"":""OTR""}";
				AssertEquals(expectedTrace, dummyTracer.Traces[0]);
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_ReturnsState_WhenLocationIsAMadeUpStateCalledOtherTerritories()
		{
			var otherTerritoriesState = Factory.NewWithValidTestData<RefCountryStates>();
			otherTerritoriesState.RW_Code = "OTR";
			otherTerritoriesState.RW_RN_NKCountryCode = CountryCodes.India;
			Factory.Save();

			var indiaCompany = CreateCompany(CountryCodes.India);
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, PlaceOfSupplyTypes.State.Code))
			{
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(otherTerritoriesState, indiaCompany);
				AssertEquals(PlaceOfSupplyTypes.State.Code, posType);
				AssertEquals("OTR", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_FallBackOrder_Types()
		{
			var canadaCompany = CreateCompany(CountryCodes.Canada);
			var britishColumbiaLocation = CreateCAHLCLocation();

			var allTypes = new string[] { PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.TaxZone.Code, PlaceOfSupplyTypes.Country.Code, PlaceOfSupplyTypes.PredefinedRule.Code };
			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(canadaCompany, allTypes))
				{
					AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

					var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, canadaCompany);
					AssertEquals(PlaceOfSupplyTypes.State.Code, posType);
					AssertEquals("BC", posCode);

					const string expectedTrace = @"
TRACED: Get POS from Location
DETAILS:
{""posTypeDescription"":""State"",""posType"":""STA"",""posCode"":""BC""}";
					AssertEquals(expectedTrace, dummyTracer.Traces[0]);
					dummyTracer.Traces.Clear();
				}

				var typesCountryZonePredefined = new string[] { PlaceOfSupplyTypes.TaxZone.Code, PlaceOfSupplyTypes.Country.Code, PlaceOfSupplyTypes.PredefinedRule.Code };
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(canadaCompany, typesCountryZonePredefined))
				{
					var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, canadaCompany);
					AssertEquals(PlaceOfSupplyTypes.TaxZone.Code, posType);
					AssertEquals("BCTZ", posCode);

					const string expectedTrace = @"
TRACED: Get POS from Location
DETAILS:
{""posTypeDescription"":""Tax Zone"",""posType"":""TZN"",""posCode"":""BCTZ""}";
					AssertEquals(expectedTrace, dummyTracer.Traces[0]);
					dummyTracer.Traces.Clear();
				}

				var typesZonePredefined = new string[] { PlaceOfSupplyTypes.Country.Code, PlaceOfSupplyTypes.PredefinedRule.Code };
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(canadaCompany, typesZonePredefined))
				{
					var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, canadaCompany);
					AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
					AssertEquals("CA", posCode);

					const string expectedTrace = @"
TRACED: Get POS from Location
DETAILS:
{""posTypeDescription"":""Country/Region"",""posType"":""CON"",""posCode"":""CA""}";
					AssertEquals(expectedTrace, dummyTracer.Traces[0]);
					dummyTracer.Traces.Clear();
				}

				var indiaCompany = CreateCompany(CountryCodes.India);
				var typesPredefined = new string[] { PlaceOfSupplyTypes.PredefinedRule.Code };
				using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, typesPredefined))
				{
					var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, indiaCompany);
					AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, posType);
					AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, posCode);

					const string expectedTrace = @"
TRACED: Get POS from Location
DETAILS:
{""posTypeDescription"":""Predefined Rule in CW1"",""posType"":""RUL"",""posCode"":""ALX""}";
					AssertEquals(expectedTrace, dummyTracer.Traces[0]);
					dummyTracer.Traces.Clear();
				}
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_FallBackOrder_StateAndCountry()
		{
			var locationMissingState = CreateOrgAddressAsLocation(state: "");
			var britishColumbiaLocation = CreateCAHLCLocation();

			var allPlaceOfSupplyTypes = new string[] { PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.Country.Code };
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(GlbCompany.CurrentCompany, allPlaceOfSupplyTypes))
			{
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(locationMissingState, GlbCompany.CurrentCompany);
				AssertEquals("When State is missing location should fallback to country", PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("When State is missing location should fallback to country", "AU", posCode);

				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocation, GlbCompany.CurrentCompany);
				AssertEquals("When Login Company Country does not match Location Country, should fallback to country", PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("When Login Company Country does not match Location Country, should fallback to country", "CA", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_FallBackOrder_StateAndTaxZone()
		{
			var canadaCompany = CreateCompany(CountryCodes.Canada);
			var typesStateZone = new string[] { PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.TaxZone.Code };
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(canadaCompany, typesStateZone))
			{
				var britishColumbiaLocationMissingState = CreateOrgAddressAsLocation(unloco: "CAHLC", state: "");
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(britishColumbiaLocationMissingState, canadaCompany);
				AssertEquals("When State is missing location should fallback to Tax Zone", PlaceOfSupplyTypes.TaxZone.Code, posType);
				AssertEquals("When State is missing location should fallback to Tax Zone", "BCTZ", posCode);

				var brisbaneLocationMissingState = CreateOrgAddressAsLocation(unloco: "AUBNE", state: "");
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(brisbaneLocationMissingState, canadaCompany);
				AssertNullOrEmpty("When State is different country and no applicable Tax Zone, should report no place of supply", posType);
				AssertNullOrEmpty("When State is different country and no applicable Tax Zone, should report no place of supply", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromILocation_FallBackOrder_StateAndPredefinedRule()
		{
			var indiaCompany = CreateCompany(CountryCodes.India);
			var typesStatePredefined = new string[] { PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code };
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(indiaCompany, typesStatePredefined))
			{
				var indiaLocation = CreateINPLSLocation();
				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(indiaLocation, indiaCompany);
				AssertEquals("State should match", PlaceOfSupplyTypes.State.Code, posType);
				AssertEquals("State should match", "AP", posCode);

				var australiaLocation = CreateAUBNELocation();
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(australiaLocation, indiaCompany);
				AssertEquals("When State is different country, should fallback to Predefined Rule", PlaceOfSupplyTypes.PredefinedRule.Code, posType);
				AssertEquals("When State is different country, should fallback to Predefined Rule", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, posCode);

				var indiaLocationMissingState = CreateOrgAddressAsLocation(unloco: "INPLS", state: "");
				(posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromILocation(indiaLocationMissingState, indiaCompany);
				AssertNullOrEmpty("When State is is missing and country matches Login Company, should report no place of supply", posType);
				AssertNullOrEmpty("When State is is missing and country matches Login Company, should report no place of supply", posCode);
			}
		}

		#endregion

		#region GetILocationFromPOSConfiguration()

		#region IJobInvoicing

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnBlankByRegistry_WhenNoMatchingRule()
		{
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();
			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var locationAP = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertEquals(null, locationAP);

				const string expectedTrace = @"
TRACED: Registry - Default POS rule when no matching rule
DETAILS:
{
  ""companyCode"": ""EDI"",
  ""defaultPOSRegistryRule"": ""BLN""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));

				var locationAR = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertEquals(null, locationAR);
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsOrgMainAddress_WhenNoMatchingRule_ForAR()
		{
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsBranchOrgProxyMainAddress_WhenNoMatchingRule_ForAP()
		{
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsOrgMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAR()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsBranchOrgProxyMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAP()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsOtherBranchOrgProxyMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAP()
		{
			var sydneyBranch = GlbCompany.CurrentCompany.Branches.Cast<GlbBranch>().First(b => b.GB_Code == "SYD");
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydneyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CreatePlaceOfSupplyTestObjects();
				CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
				sydneyBranch.GB_OH_OrgProxy = UnregisteredLocalOrg.PK;
				Factory.Save();
			}
			AssertNotEquals("Precondition: Org Proxy address for Sydney Branch is different to default branch", sydneyBranch.OrgProxy.MainAddress.AddressFullFormatted, GlbBranch.CurrentBranch.OrgProxy.MainAddress.AddressFullFormatted);

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, sydneyBranch);
			AssertSame(sydneyBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsBranchOrgProxyMainAddress_WhenSupplierLocationRuleIsMatched_ForAR()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatched_ForAP()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsConsignorPickupDocAddress_WhenPickupLocationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.GetConsignorPickupDocAddress, location);
			AssertEquals("QLD", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsPickupAgentPickupAddress_WhenPickupAgentRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAgentAddr = pickupAgent.Addresses.AddNewMainAddress().WithLocation(unloco: "AUHBA", state: "TAS", capability: OrgAddressType.Pickup.Code);
			ImportShipment.PickupAgentDocumentaryAddress.E2_OA_Address = pickupAgentAddr.PK;

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupAgent);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: "LOC", GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.PickupAgentDocumentaryAddress.Organisation.Addresses.GetAddressWithMainAddressFallback("AUHBA", OrgAddressType.Pickup), location);
			AssertEquals("AUHBA", location.UNLOCO.Code);
			AssertEquals("TAS", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsDeliveryAgentDeliveryAddress_WhenDeliveryAgentRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.Addresses.AddNewMainAddress().WithLocation(unloco: "AUDRW", state: "NT", capability: OrgAddressType.Delivery.Code);
			ImportShipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryAgent);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: "LOC", GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.DeliveryAgent.Addresses.GetAddressWithMainAddressFallback("AUDRW", OrgAddressType.Delivery), location);
			AssertEquals("AUDRW", location.UNLOCO.Code);
			AssertEquals("NT", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsDepartureCFSDocAddress_WhenPickupCFSRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			ImportShipment.GetDepartureCFSDocAddress.Address1 = "3 GetDepartureCFSDocAddress St";
			ImportShipment.GetDepartureCFSDocAddress.Postcode = "3001";
			ImportShipment.GetDepartureCFSDocAddress.State = "VIC";
			ImportShipment.GetDepartureCFSDocAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupCFS);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.GetDepartureCFSDocAddress, location);
			AssertEquals("VIC", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsArrivalCFSDocAddress_WhenDestinationCFSRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			ImportShipment.GetArrivalCFSDocAddress.Address1 = "4 GetArrivalCFSDocAddress St";
			ImportShipment.GetArrivalCFSDocAddress.Postcode = "5001";
			ImportShipment.GetArrivalCFSDocAddress.State = "SA";
			ImportShipment.GetArrivalCFSDocAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryCFS);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.GetArrivalCFSDocAddress, location);
			AssertEquals("SA", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsConsigneeDeliveryDocAddress_WhenDeliveryLocationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			ImportShipment.ConsigneeDeliveryAddress.Address1 = "6 ConsigneeDeliveryAddress St";
			ImportShipment.ConsigneeDeliveryAddress.Postcode = "6001";
			ImportShipment.ConsigneeDeliveryAddress.State = "WA";
			ImportShipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.ConsigneeDeliveryAddress, location);
			AssertEquals("WA", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsConsolLoadPortUNLOCO_WhenConsolPortOfLoadingRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolPortOfLoading);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ConsolShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ForwardingConsol.LoadPort, location);
			AssertEquals("NZWLG", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsOriginUNLOCO_WhenOriginRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.Origin);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.Origin, location);
			AssertEquals("CYZYY", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsDestinationUNLOCO_WhenDestinationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.Destination);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ImportShipment.Destination, location);
			AssertEquals("AUSYD", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsPickupTransitWarehouseAddress_WhenPickupTransitWarehouseRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			var transitWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			transitWarehouse.Addresses.AddNewMainAddress();
			var receivingDepotAddr = transitWarehouse.Addresses.AddNew().WithLocation(unloco: "AULST", state: "TAS", capability: OrgAddressType.PickupAndDelivery.Code);
			receivingDepotAddr.Address1 = "Pickup Warehouse";
			ImportShipment.JS_OA_ExportReceivingDepot = receivingDepotAddr.PK;

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupTransitWarehouse);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame((ImportShipment as ITransitWarehouseInstructionSupporter).PickupTransitWarehouse, location);
			AssertEquals("AULST", location.UNLOCO.Code);
			AssertEquals("Pickup Warehouse", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsDeliveryTransitWarehouseAddress_WhenDeliveryTransitWarehouseRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			var transitWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			transitWarehouse.Addresses.AddNewMainAddress();
			var releaseDepotAddr = transitWarehouse.Addresses.AddNew().WithLocation(unloco: "AUALH", state: "WA", capability: OrgAddressType.PickupAndDelivery.Code);
			releaseDepotAddr.Address1 = "Delivery Warehouse";
			ImportShipment.JS_OA_ImportReleaseDepot = releaseDepotAddr.PK;

			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryTransitWarehouse);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame((ImportShipment as ITransitWarehouseInstructionSupporter).DeliveryTransitWarehouse, location);
			AssertEquals("AUALH", location.UNLOCO.Code);
			AssertEquals("Delivery Warehouse", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsTransportLoadPort_WhenFirstPortOfLoadingInCompanyCountryRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ExportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			Assert("location is RefUNLOCO", location is RefUNLOCO);
			AssertEquals("AUBNE", location.UNLOCO.Code);

			location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ConsolShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertEquals("location is not RefUNLOCO", false, location is RefUNLOCO);
			AssertSame("Fallback to BillToParty", RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsTransportDischargePort_WhenLastPortOfDischargeInCompanyCountryRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ExportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			Assert("location is RefUNLOCO", location is RefUNLOCO);
			AssertEquals("AUPER", location.UNLOCO.Code);

			location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ConsolShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertEquals("location is not RefUNLOCO", false, location is RefUNLOCO);
			AssertSame("Fallback to BillToParty", RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsConsolDischargePortUNLOCO_WhenConsolPortOfDischargeRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolPortOfDischarge);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ConsolShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(ForwardingConsol.DischargePort, location);
			AssertEquals("USELP", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_HasFallbackToBillToPartyLocation_WhenMatchedRuleAddressIsMissing()
		{
			CreatePlaceOfSupplyTestObjects();
			var config = CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ExportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);

			config.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.PickupCFS;
			location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ExportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);

			config.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.DeliveryCFS;
			location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ExportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);

			config.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.DeliveryLocation;
			location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ExportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);

			// Note: Consol cases (ConsolPortOfLoading & ConsolPortOfDischarge) are not here - a Consol missing [Load|Discharge]Location is... well... very unlikely.
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_HasFallbackToBillToPartyLocation_WhenConsolRulesMatchedAndShipmentOnMultipleConsols()
		{
			CreatePlaceOfSupplyTestObjects();
			var config = CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolPortOfLoading);
			var anotherConsol = new TestObjectCreator(Factory).CreateConsol(consolNum: "C00FBK");
			anotherConsol.Shipments.Add(ConsolShipment);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ConsolShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);

			config.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.ConsolPortOfDischarge;
			location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ConsolShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_HasFallbackToBlank_WhenMatchingRuleButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreateConfigurationRule("SHP", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupLocation);
			var newShipment = new TestObjectCreator(Factory).CreateShipment("S0005");
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(newShipment, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			Assert(location.CityTown == null && location.Country == null && location.State == null && location.UNLOCO == null);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobInvoicing_ReturnsBranchOrgProxyMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		#endregion IJobInvoicing

		#region IJobCosting

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnBlankByRegistry_WhenNoMatchingRule()
		{
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();
			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
				AssertEquals(null, location);

				const string expectedTrace = @"
TRACED: Get Location from IJobCostingPlugIn
DETAILS:
{
  ""isActive"": false,
  ""posRule"": """"
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsBranchOrgProxyMainAddress_WhenNoMatchingRule()
		{
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsConsolLoadPortUNLOCO_WhenConsolPortOfLoadingRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolPortOfLoading);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.LoadPort, location);
			AssertEquals("NZWLG", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsConsolDischargePortUNLOCO_WhenConsolPortOfDischargeRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolPortOfDischarge);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.DischargePort, location);
			AssertEquals("USELP", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsSendingForwarderAddress_WhenConsolSendingAgentRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolSendingAgent);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.SendingForwarderAddress, location);
			AssertEquals("AULST", location.UNLOCO.Code);
			AssertEquals("Sending Agent", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsReceivingForwarderAddress_WhenConsolReceivingAgentRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.ConsolReceivingAgent);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.ReceivingForwarderAddress, location);
			AssertEquals("AUBTD", location.UNLOCO.Code);
			AssertEquals("Receiving Agent", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsPackDepotAddress_WhenDepartureCFSRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.DepartureCFS);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.PackDepotAddress, location);
			AssertEquals("AUALH", location.UNLOCO.Code);
			AssertEquals("Pack Depot", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsUnpackDepotAddress_WhenArrivalCFSRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.ArrivalCFS);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.UnpackDepotAddress, location);
			AssertEquals("AUAVA", location.UNLOCO.Code);
			AssertEquals("Unpack Depot", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsDepartureCTOAddress_WhenDepartureCTORuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.DepartureCTO);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.DepartureCTOAddress, location);
			AssertEquals("AUACO", location.UNLOCO.Code);
			AssertEquals("Departure CTO", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsArrivalCTOAddress_WhenArrivalCTORuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.ArrivalCTO);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(ForwardingConsol.ArrivalCTOAddress, location);
			AssertEquals("AUABA", location.UNLOCO.Code);
			AssertEquals("Arrival CTO", (location as OrgAddress)?.Address1);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsBranchOrgProxyMainAddress_WhenBillToPartyLocationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsCreditorMainAddress_WhenSupplierLocationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_HasFallbackToBlank_WhenMatchingRuleButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreateConfigurationRule("FCN", "ALL", "ALL", rule: AccPOSRuleList.Codes.OtherTerritories);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);
			AssertEquals(null, location);
		}

		public void TestGetILocationFromPOSConfiguration_IJobCosting_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			Factory.Save();
			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);
				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, RegisteredLocalOrg, costSupplyType: ZString.Empty);

				AssertSame(RegisteredLocalOrg.MainAddress, location);

				const string expectedTrace = @"
TRACED: Get Location from IJobCostingPlugIn - Fallback when could not get ILocation to Creditor address
DETAILS:
{
  ""code"": """",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""NSW"",
  ""posRule"": ""SUP""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		#endregion IJobCosting

		#region JobDeclaration

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsPortOfLoadingUNLOCO_WhenPortOfLoadingRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.PortOfLoading);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.PortOfLoading, location);
			AssertEquals("NZWLG", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsPortOfArrivalUNLOCO_WhenPortOfDischargeRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.PortOfDischarge);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.PortOfArrival, location);
			AssertEquals("AUBNE", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsPortOfFirstArrivalUNLOCO_WhenPortOfFirstArrivalIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.PortOfFirstArrival);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.PortOfFirstArrival, location);
			AssertEquals("NZDUD", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsOriginUNLOCO_WhenPortOfOriginIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.PortOfOrigin);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.Origin, location);
			AssertEquals("NZMON", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsFinalDestinationUNLOCO_WhenFinalDestinationIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.FinalDestination);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.FinalDestination, location);
			AssertEquals("AUBWU", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsTransportLoadPort_WhenFirstPortOfLoadingInCompanyCountryRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.FirstPortOfLoadingInCompanyCountry);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			Assert("location is RefUNLOCO", location is RefUNLOCO);
			AssertEquals("AUSYD", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsTransportDischargePort_WhenLastPortOfDischargeInCompanyCountryRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.LastPortOfDischargeInCompanyCountry);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			Assert("location is RefUNLOCO", location is RefUNLOCO);
			AssertEquals("AUBNE", location.UNLOCO.Code);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsSupplierPickupAddress_WhenPickupLocationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.SupplierPickupAddress, location);
			AssertEquals("Supplier Pickup Address", (location as JobDocAddress).Address1);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsImporterDeliveryAddress_WhenDeliveryLocationRuleIsMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(Declaration.ImporterDeliveryAddress, location);
			AssertEquals("Importer Delivery Address", (location as JobDocAddress).Address1);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsBranchOrgProxyMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAP()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsOrgMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAR()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();
			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertSame(RegisteredLocalOrg.MainAddress, location);

				const string expectedTrace = @"
TRACED: Get Location from IJobInvoicingPlugIn - Fallback when could not get ILocation to Debtor address
DETAILS:
{
  ""code"": """",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""NSW"",
  ""posRule"": ""BIL""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatched_ForAP()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsBranchOrgProxyMainAddress_WhenSupplierLocationRuleIsMatched_ForAR()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnBlankByRegistry_WhenNoMatchingRule()
		{
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertEquals(null, location);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_HasFallbackToBlank_WhenMatchingRuleButMissingLocation()
		{
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryLocation);
			var newDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(newDeclaration, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			Assert(location.CityTown == null && location.Country == null && location.State == null && location.UNLOCO == null);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_JobDeclaration_ReturnsBranchOrgProxyMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("BRK", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);

			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(DeclarationAsPlugIn, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		#endregion JobDeclaration

		#region NoJob

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsOrgMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAR()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();
			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertSame(RegisteredLocalOrg.MainAddress, location);

				const string expectedTrace = @"
TRACED: Get Location for Charge Code and Supply Type
DETAILS:
{
  ""code"": """",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""NSW"",
  ""posRule"": ""BIL""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnBlankByRegistryBlankValue_WhenNoMatchingRule()
		{
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertEquals(null, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsBranchOrgProxyMainAddressByRegistryDefaultValue_WhenNoMatchingRule_ForAP()
		{
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsOrgMainAddressByRegistryDefaultValue_WhenNoMatchingRule_ForAR()
		{
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsBranchOrgProxyMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAP()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsOtherBranchOrgProxyMainAddress_WhenBillToPartyLocationRuleIsMatched_ForAP()
		{
			var sydneyBranch = GlbCompany.CurrentCompany.Branches.Cast<GlbBranch>().First(b => b.GB_Code == "SYD");
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydneyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CreatePlaceOfSupplyTestObjects();
				CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
				sydneyBranch.GB_OH_OrgProxy = UnregisteredLocalOrg.PK;
				Factory.Save();
			}
			AssertNotEquals("Precondition: Org Proxy address for Sydney Branch is different to default branch", sydneyBranch.OrgProxy.MainAddress.AddressFullFormatted, GlbBranch.CurrentBranch.OrgProxy.MainAddress.AddressFullFormatted);

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, sydneyBranch);
			AssertSame(sydneyBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsBranchOrgProxyMainAddress_WhenSupplierLocationRuleIsMatched_ForAR()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatched_ForAP()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsOrgMainAddress_WhenMatchingRuleButMissingLocation_ForDefaultValue()
		{
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.Value);
			CreatePlaceOfSupplyTestObjects();
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsBranchOrgProxyMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(GlbBranch.CurrentBranch.OrgProxy.MainAddress, location);
		}

		public void TestGetILocationFromPOSConfiguration_NoJob_ReturnsOrgMainAddress_WhenSupplierLocationRuleIsMatchedButMissingLocation()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(ChargeCodeFRT, CostSell.Cost, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(RegisteredLocalOrg.MainAddress, location);
		}

		#endregion NoJob

		#region IConfirmAddressParent

		public void TestGetILocationFromPOSConfiguration_IConfirmAddressParent_ReturnsDepartureCTODocAddress_WhenPickupCTORuleIsMatchedAndDepartureCTOIsvalid()
		{
			CreatePlaceOfSupply_QuotedBookingTestObject();
			QuotedBooking.Mode = Constants.RateMode.COU;
			var addressParent = (IConfirmAddressParent)QuotedBooking;
			addressParent.GetDepartureCTODocAddress.Address1 = "GetDepartureCTODocAddress St";
			addressParent.GetDepartureCTODocAddress.Postcode = "1111";
			addressParent.GetDepartureCTODocAddress.State = "VIC";
			addressParent.GetDepartureCTODocAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("QSH", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupCTO);
			Factory.Save();

			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(QuotedBookingAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertSame(addressParent.GetDepartureCTODocAddress, location);
				AssertEquals("VIC", location.State.RW_Code);

				const string expectedTrace = @"
TRACED: Get Location from IConfirmAddressParent
DETAILS:
{
  ""code"": """",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""VIC"",
  ""posRule"": ""PCT""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_IConfirmAddressParent_ReturnsDepartureCFSDocAddress_WhenPickupCTORuleIsMatchedButDepartureCTOIsInvalid()
		{
			CreatePlaceOfSupply_QuotedBookingTestObject();

			var addressParent = (IConfirmAddressParent)QuotedBooking;
			addressParent.GetDepartureCFSDocAddress.Address1 = "GetDepartureCFSDocAddress St";
			addressParent.GetDepartureCFSDocAddress.Postcode = "1111";
			addressParent.GetDepartureCFSDocAddress.State = "VIC";
			addressParent.GetDepartureCFSDocAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("QSH", "ALL", "ALL", rule: AccPOSRuleList.Codes.PickupCTO);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(QuotedBookingAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(addressParent.GetDepartureCFSDocAddress, location);
			AssertEquals("VIC", location.State.RW_Code);
		}

		public void TestGetILocationFromPOSConfiguration_IConfirmAddressParent_ReturnsArrivalCTODocAddress_WhenDeliveryCTORuleIsMatchedAndArrivalCTOIsValid()
		{
			CreatePlaceOfSupply_QuotedBookingTestObject();
			QuotedBooking.Mode = Constants.RateMode.COU;
			var addressParent = (IConfirmAddressParent)QuotedBooking;
			addressParent.GetArrivalCTODocAddress.Address1 = "GetDepartureCFSDocAddress St";
			addressParent.GetArrivalCTODocAddress.Postcode = "2222";
			addressParent.GetArrivalCTODocAddress.State = "NSW";
			addressParent.GetArrivalCTODocAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("QSH", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryCTO);
			Factory.Save();

			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(QuotedBookingAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertSame(addressParent.GetArrivalCTODocAddress, location);
				AssertEquals("NSW", location.State.RW_Code);

				const string expectedTrace = @"
TRACED: Get Location from IConfirmAddressParent
DETAILS:
{
  ""code"": """",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""NSW"",
  ""posRule"": ""DCT""
}";

				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_IConfirmAddressParent_ReturnsArrivalCFSDocAddress_WhenDeliveryCTORuleMatchedButArrivalCTOIsInValid()
		{
			CreatePlaceOfSupply_QuotedBookingTestObject();
			var addressParent = (IConfirmAddressParent)QuotedBooking;
			addressParent.GetArrivalCFSDocAddress.Address1 = "GetArrivalCTODocAddress St";
			addressParent.GetArrivalCFSDocAddress.Postcode = "2222";
			addressParent.GetArrivalCFSDocAddress.State = "NSW";
			addressParent.GetArrivalCFSDocAddress.E2_RN_NKCountryCode = "AU";

			CreateConfigurationRule("QSH", "ALL", "ALL", rule: AccPOSRuleList.Codes.DeliveryCTO);
			Factory.Save();

			var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(QuotedBookingAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
			AssertSame(addressParent.GetArrivalCFSDocAddress, location);
			AssertEquals("NSW", location.State.RW_Code);
		}

		#endregion

		#region ICO2eLegBasedSupporter

		public void TestGetILocationFromPOSConfiguration_ICO2eLegBasedSupporter_ReturnsLoadPortUNLOCO_WhenLoadRuleIsMatched()
		{
			CreatePlaceOfSupply_QuotedBookingTestObject();
			CreateConfigurationRule("QSH", "ALL", "ALL", rule: AccPOSRuleList.Codes.Load);
			Factory.Save();

			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(QuotedBookingAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				Assert("location is RefUNLOCO", location is RefUNLOCO);
				AssertEquals("AUSYD", location.UNLOCO.Code);

				const string expectedTrace = @"
TRACED: Get Location from ICO2eLegBasedSupporter
DETAILS:
{
  ""code"": ""AUSYD"",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""NSW"",
  ""posRule"": ""LOA""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		public void TestGetILocationFromPOSConfiguration_ICO2eLegBasedSupporter_ReturnsDischargePortUNLOCO_WhenDischargeRuleIsMatched()
		{
			CreatePlaceOfSupply_QuotedBookingTestObject();
			CreateConfigurationRule("QSH", "ALL", "ALL", rule: AccPOSRuleList.Codes.Discharge);
			Factory.Save();

			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(QuotedBookingAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				Assert("location is RefUNLOCO", location is RefUNLOCO);
				AssertEquals("AUBNE", location.UNLOCO.Code);

				const string expectedTrace = @"
TRACED: Get Location from ICO2eLegBasedSupporter
DETAILS:
{
  ""code"": ""AUBNE"",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""QLD"",
  ""posRule"": ""DIS""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		#endregion

		#region ILocation

		public void TestGetILocationFromPOSConfiguration_ILocation_ReturnsCountryRegionPortUNLOCO_WhenCountryRegionPortRuleIsMatched()
		{
			CreatePlaceOfSupply_WorkItemTestObject();
			CreateConfigurationRule("WKI", "ALL", "ALL", rule: AccPOSRuleList.Codes.CountryRegionPort);

			Factory.Save();

			var dummyTracer = GetDummyTracer();

			using (ObjectFactory.Substitute<ITracer>(dummyTracer))
			{
				AssertEquals("Precondition: no trace messages", 0, dummyTracer.Traces.Count);

				var location = AccPlaceOfSupplyHelper.GetILocationFromPOSConfiguration(WorkItemAsPlugin, ChargeCodeFRT, CostSell.Revenue, RegisteredLocalOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertEquals("Code", "AUSYD", location.Code);
				AssertEquals("Description", "Sydney", location.Description);
				AssertEquals("Conuntry", "AU", location.Country.Code);

				const string expectedTrace = @"
TRACED: Get Location from ILocation
DETAILS:
{
  ""code"": ""AUSYD"",
  ""isActive"": true,
  ""countryCode"": ""AU"",
  ""stateCode"": ""NSW"",
  ""posRule"": ""CRP""
}";
				AssertContains(expectedTrace, string.Join("", dummyTracer.Traces));
				dummyTracer.Traces.Clear();
			}
		}

		#endregion

		#endregion

		#region GetPlaceOfSupplyFromConfiguration()

		public void TestGetPlaceOfSupplyFromConfiguration_IJobInvoicing_ReturnsEmpty_WhenPlaceOfSupplyIsDisabled()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly())
			{
				Assert("Precondition: Place of Supply is disabled", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, TexasOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_IJobCosting_ReturnsEmpty_WhenPlaceOfSupplyIsDisabled()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly())
			{
				Assert("Precondition: Place of Supply is disabled", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, TexasOrg, costSupplyType: ZString.Empty);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_NoJob_ReturnsEmpty_WhenPlaceOfSupplyIsDisabled()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly())
			{
				Assert("Precondition: Place of Supply is disabled", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ChargeCodeFRT, CostSell.Revenue, TexasOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertNullOrEmpty(posType);
				AssertNullOrEmpty(posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_IJobInvoicing_ReturnsUS_WhenCountryMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.Country.Code))
			{
				Assert("Precondition: Place of Supply is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Revenue, TexasOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("US", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_IJobInvoicing_ReturnsNSW_WhenStateMatched_ForSydneyBranch()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				Assert("Precondition: Place of Supply is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var sydneyBranch = GlbCompany.CurrentCompany.Branches.Cast<GlbBranch>().First(b => b.GB_Code == "SYD");
				sydneyBranch.GB_OH_OrgProxy = UnregisteredLocalOrg.PK;

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ImportShipmentAsPlugin, ChargeCodeFRT, CostSell.Cost, TexasOrg, supplyType: ZString.Empty, sydneyBranch);
				AssertEquals(PlaceOfSupplyTypes.State.Code, posType);
				AssertEquals("NSW", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_IJobCosting_ReturnsUS_WhenCountryMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.Country.Code))
			{
				Assert("Precondition: Place of Supply is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ForwardingConsolAsPlugin, ChargeCodeFRT, TexasOrg, costSupplyType: ZString.Empty);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("US", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_IJobCosting_ReturnsAU_WhenConsolDoesNotImplementIJobInvoicing()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.SupplierLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.Country.Code))
			{
				Assert("Precondition: Place of Supply is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ConsolidatedTransportBookingAsPlugin, ChargeCodeFRT, TexasOrg, costSupplyType: ZString.Empty);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("When a consol does not implement IJobInvoicing, the ALL rules should not match and BillToPartyLocation should be used as a fallback", "AU", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_NoJob_ReturnsUS_WhenCountryMatched()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.Country.Code))
			{
				Assert("Precondition: Place of Supply is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ChargeCodeFRT, CostSell.Revenue, TexasOrg, supplyType: ZString.Empty, GlbBranch.CurrentBranch);
				AssertEquals(PlaceOfSupplyTypes.Country.Code, posType);
				AssertEquals("US", posCode);
			}
		}

		public void TestGetPlaceOfSupplyFromConfiguration_NoJob_ReturnsNSW_WhenCountryMatched_ForSydneyBranch()
		{
			CreatePlaceOfSupplyTestObjects();
			CreateConfigurationRule("ALL", "ALL", "ALL", rule: AccPOSRuleList.Codes.BillToPartyLocation);
			Factory.Save();

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				Assert("Precondition: Place of Supply is enabled", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());

				var sydneyBranch = GlbCompany.CurrentCompany.Branches.Cast<GlbBranch>().First(b => b.GB_Code == "SYD");
				sydneyBranch.GB_OH_OrgProxy = UnregisteredLocalOrg.PK;

				var (posType, posCode) = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ChargeCodeFRT, CostSell.Cost, TexasOrg, supplyType: ZString.Empty, sydneyBranch);
				AssertEquals(PlaceOfSupplyTypes.State.Code, posType);
				AssertEquals("NSW", posCode);
			}
		}

		#endregion

		#region Tracing

		DummyTracer GetDummyTracer() => new DummyTracer(AccountingTraceSourceCodes.FPOS);

		#endregion

		#region Implementation

		void CreatePlaceOfSupplyTestObjects()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			NZCompany = CreateCompany(CountryCodes.NewZealand);
			INCompany = CreateCompany(CountryCodes.India);

			ImportShipment = testObjectCreator.CreateShipment("S0001", origin: "CYZYY", destination: "AUSYD");
			ImportShipment.ConsignorDocumentaryAddress.Address1 = "1 ConsignorDocumentaryAddress St";
			ImportShipment.ConsignorDocumentaryAddress.Postcode = "2001";
			ImportShipment.ConsignorDocumentaryAddress.State = "NSW";
			ImportShipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			ImportShipment.ConsignorPickupAddress.Address1 = "2 ConsignorPickupAddress St";
			ImportShipment.ConsignorPickupAddress.Postcode = "4001";
			ImportShipment.ConsignorPickupAddress.State = "QLD";
			ImportShipment.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";
			ImportShipment.ConsigneeDocumentaryAddress.Address1 = "5 ConsigneeDocumentaryAddress St";
			ImportShipment.ConsigneeDocumentaryAddress.Postcode = "0820";
			ImportShipment.ConsigneeDocumentaryAddress.State = "NT";
			ImportShipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";

			ExportShipment = testObjectCreator.CreateShipment("S0002", origin: "AUSYD", destination: "CYZYY");
			var shpTransport1 = ExportShipment.Transports.AddNew();
			shpTransport1.JW_LegOrder = 1;
			shpTransport1.JW_RL_NKLoadPort = "NZWLG";
			shpTransport1.JW_RL_NKDiscPort = "AUBNE";
			var shpTransport2 = ExportShipment.Transports.AddNew();
			shpTransport2.JW_LegOrder = 2;
			shpTransport2.JW_RL_NKLoadPort = "AUBNE";
			shpTransport2.JW_RL_NKDiscPort = "AUSYD";
			var shpTransport3 = ExportShipment.Transports.AddNew();
			shpTransport3.JW_LegOrder = 3;
			shpTransport3.JW_RL_NKLoadPort = "AUSYD";
			shpTransport3.JW_RL_NKDiscPort = "AUPER";
			var shpTransport4 = ExportShipment.Transports.AddNew();
			shpTransport4.JW_LegOrder = 4;
			shpTransport4.JW_RL_NKLoadPort = "AUPER";
			shpTransport4.JW_RL_NKDiscPort = "CYZYY";

			ForwardingConsol = testObjectCreator.CreateConsol(consolNum: "C0001", origin: "NZWLG", destination: "USELP");
			ForwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			var conTransport1 = ForwardingConsol.Transports.AddNew();
			conTransport1.JW_LegOrder = 1;
			conTransport1.JW_RL_NKLoadPort = "NZWLG";
			conTransport1.JW_RL_NKDiscPort = "AUBNE";
			var conTransport2 = ForwardingConsol.Transports.AddNew();
			conTransport2.JW_LegOrder = 2;
			conTransport2.JW_RL_NKLoadPort = "AUBNE";
			conTransport2.JW_RL_NKDiscPort = "AUSYD";
			var conTransport3 = ForwardingConsol.Transports.AddNew();
			conTransport3.JW_LegOrder = 3;
			conTransport3.JW_RL_NKLoadPort = "AUSYD";
			conTransport3.JW_RL_NKDiscPort = "USELP";

			ConsolShipment = testObjectCreator.CreateShipment("S0003", origin: "NZWLG", destination: "CYZYY");
			ForwardingConsol.Shipments.Add(ConsolShipment);

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.Addresses.AddNewMainAddress();
			var sendingAgentAddr = sendingAgent.Addresses.AddNew().WithLocation(unloco: "AULST", state: "TAS", capability: OrgAddressType.PickupAndDelivery.Code);
			sendingAgentAddr.Address1 = "Sending Agent";
			ForwardingConsol.JK_OA_SendingForwarderAddress = sendingAgentAddr.PK; // Using an address other than main office address
			var pickupDepotAddr = sendingAgent.Addresses.AddNew().WithLocation(unloco: "AUALH", state: "WA", capability: OrgAddressType.Pickup.Code);
			pickupDepotAddr.Address1 = "Pack Depot";
			ForwardingConsol.JK_OA_PackDepotAddress = pickupDepotAddr.PK; // Using an address other than main office address
			var departureCTOAddr = sendingAgent.Addresses.AddNew().WithLocation(unloco: "AUACO", state: "WA", capability: OrgAddressType.Pickup.Code);
			departureCTOAddr.Address1 = "Departure CTO";
			ForwardingConsol.JK_OA_DepartureCTOAddress = departureCTOAddr.PK; // Using an address other than main office address

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.Addresses.AddNewMainAddress();
			var receivingAgentAddr = receivingAgent.Addresses.AddNew().WithLocation(unloco: "AUBTD", state: "NT", capability: OrgAddressType.PickupAndDelivery.Code);
			receivingAgentAddr.Address1 = "Receiving Agent";
			ForwardingConsol.JK_OA_ReceivingForwarderAddress = receivingAgentAddr.PK; // Using an address other than main office address
			var unpackDepotAddr = receivingAgent.Addresses.AddNew().WithLocation(unloco: "AUAVA", state: "QLD", capability: OrgAddressType.Delivery.Code);
			unpackDepotAddr.Address1 = "Unpack Depot";
			ForwardingConsol.JK_OA_UnpackDepotAddress = unpackDepotAddr.PK; // Using an address other than main office address
			var arrivalCTOAddr = receivingAgent.Addresses.AddNew().WithLocation(unloco: "AUABA", state: "QLD", capability: OrgAddressType.Delivery.Code);
			arrivalCTOAddr.Address1 = "Arrival CTO";
			ForwardingConsol.JK_OA_ArrivalCTOAddress = arrivalCTOAddr.PK; // Using an address other than main office address

			GatewayConsol = testObjectCreator.CreateGatewayConsol(consolNum: "C0002", origin: "AUSYD", destination: "CYZYY", receivingGatewayCompany: GlbCompany.CurrentCompany);

			ConsolidatedTransportBooking = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			ConsolidatedTransportBooking[DtbBookingConsolidationSchema.KB_JobType] = TransportCommon.Shared.TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			Declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Declaration.SupplierPickupAddress.FillWithValidTestData(); // Need to do it to prevent OrFallbackOnEmpty recognising it as empty
			Declaration.SupplierPickupAddress.Address1 = "Supplier Pickup Address";
			Declaration.ImporterDeliveryAddress.FillWithValidTestData(); // Need to do it to prevent OrFallbackOnEmpty recognising it as empty
			Declaration.ImporterDeliveryAddress.Address1 = "Importer Delivery Address";

			var brkTransport1 = Declaration.Transports.AddNew();
			brkTransport1.JW_LegOrder = 1;
			brkTransport1.JW_RL_NKLoadPort = "NZWLG";
			brkTransport1.JW_RL_NKDiscPort = "NZDUD";
			var brkTransport2 = Declaration.Transports.AddNew();
			brkTransport2.JW_LegOrder = 2;
			brkTransport2.JW_RL_NKLoadPort = "NZDUD";
			brkTransport2.JW_RL_NKDiscPort = "AUSYD";
			var brkTransport3 = Declaration.Transports.AddNew();
			brkTransport3.JW_LegOrder = 3;
			brkTransport3.JW_RL_NKLoadPort = "AUSYD";
			brkTransport3.JW_RL_NKDiscPort = "AUBNE";
			var brkTransport4 = Declaration.Transports.AddNew();
			brkTransport4.JW_LegOrder = 4;
			brkTransport4.JW_RL_NKLoadPort = "AUBNE";
			brkTransport4.JW_RL_NKDiscPort = "CYZYY";
			// Have to set some ports  after adding Transports as it is getting reset by the Transports setup
			Declaration.JE_RL_NKPortOfLoading = "NZWLG";
			Declaration.JE_RL_NKOrigin = "NZMON";
			Declaration.JE_RL_NKFinalDestination = "AUBWU";
			Declaration.JE_RL_NKPortOfArrival = "AUBNE";
			Declaration.JE_RL_NKPortOfFirstArrival = "NZDUD";

			UnregisteredLocalOrg = Factory.NewWithValidTestData<OrgHeader>();
			UnregisteredLocalOrg.OH_RL_NKClosestPort = "AUSYD";
			UnregisteredLocalOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "AUSYD", state: "NSW");

			UnregisteredForeignOrg = Factory.NewWithValidTestData<OrgHeader>();
			UnregisteredForeignOrg.OH_RL_NKClosestPort = "CYZYY";
			var vatCode = UnregisteredForeignOrg.CustomsCodes.AddNew();
			vatCode.OK_CodeType = "TIN";
			vatCode.OK_RN_NKCodeCountry = "CY";
			vatCode.OK_CustomsRegNo = "112234345";

			RegisteredLocalOrg = Factory.NewWithValidTestData<OrgHeader>();
			RegisteredLocalOrg.OH_RL_NKClosestPort = "AUSYD";
			RegisteredLocalOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "AUSYD", state: "NSW");
			vatCode = RegisteredLocalOrg.CustomsCodes.AddNew();
			vatCode.OK_CodeType = "ABN";
			vatCode.OK_RN_NKCodeCountry = "AU";
			vatCode.OK_CustomsRegNo = "123456789";

			RegisteredForeignOrg = Factory.NewWithValidTestData<OrgHeader>();
			RegisteredForeignOrg.OH_RL_NKClosestPort = "CYZYY";
			vatCode = RegisteredForeignOrg.CustomsCodes.AddNew();
			vatCode.OK_CodeType = "ABN";
			vatCode.OK_RN_NKCodeCountry = "AU";
			vatCode.OK_CustomsRegNo = "987654321";

			BritishColumbiaOrg = Factory.NewWithValidTestData<OrgHeader>();
			BritishColumbiaOrg.OH_RL_NKClosestPort = "CAHLC";
			BritishColumbiaOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "CAHLC", state: "BC");

			IndiaOrg = Factory.NewWithValidTestData<OrgHeader>();
			IndiaOrg.OH_RL_NKClosestPort = "INPLS";
			IndiaOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "INPLS", state: "AP");

			TexasOrg = Factory.NewWithValidTestData<OrgHeader>();
			TexasOrg.OH_RL_NKClosestPort = "USELP";
			TexasOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "USELP", state: "TX");

			NowhereOrg = Factory.New<OrgHeader>();
			NowhereOrg.OH_Code = "NWORG";
			NowhereOrg.OH_FullName = "Nowhere Inc.";

			GroupGRP = Factory.New<AccPOSChargeCodeGroup>();
			GroupGRP.GRO_Code = "GRP";
			GroupGRP.GRO_Description = "GRP Description";
			GroupGRP2 = Factory.New<AccPOSChargeCodeGroup>();
			GroupGRP2.GRO_Code = "GRP2";
			GroupGRP2.GRO_Description = "GRP2 Description";

			ChargeCodeBAF = CreateChargeCode("BAF");
			var pivot = GroupGRP.ChargeCodePivots.AddNew();
			pivot.GRP_GroupType = "POS";
			pivot.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot.GRP_MemberID = ChargeCodeBAF.PK;

			ChargeCodeFRT = CreateChargeCode("FRT");
			var pivot2 = GroupGRP.ChargeCodePivots.AddNew();
			pivot2.GRP_GroupType = "POS";
			pivot2.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot2.GRP_MemberID = ChargeCodeFRT.PK;

			ChargeCodeOLAB = CreateChargeCode("OLAB");
			var pivot3 = GroupGRP2.ChargeCodePivots.AddNew();
			pivot3.GRP_GroupType = "POS";
			pivot3.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot3.GRP_MemberID = ChargeCodeOLAB.PK;
		}

		AccChargeCode CreateChargeCode(ZString code)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, code);
			return Factory.LoadTop1<AccChargeCode>(query);
		}

		void CreatePlaceOfSupply_QuotedBookingTestObject()
		{
			RegisteredLocalOrg = Factory.NewWithValidTestData<OrgHeader>();
			RegisteredLocalOrg.OH_RL_NKClosestPort = "AUSYD";
			RegisteredLocalOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "AUSYD", state: "NSW");
			ChargeCodeFRT = CreateChargeCode("FRT");
			QuotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			QuotedBooking.Mode = Constants.RateMode.LSE;
			QuotedBooking.Origin = "AUSYD";
			QuotedBooking.Destination = "AUBNE";
			QuotedBooking.LoadPort = "AUSYD";
			QuotedBooking.DischargePort = "AUBNE";
			QuotedBooking.Booking.ConsignorPickupAddress.Address1 = "2 ConsignorPickupAddress St";
			QuotedBooking.Booking.ConsignorPickupAddress.Postcode = "4001";
			QuotedBooking.Booking.ConsignorPickupAddress.State = "QLD";
			QuotedBooking.Booking.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";
		}

		void CreatePlaceOfSupply_WorkItemTestObject()
		{
			RegisteredLocalOrg = Factory.NewWithValidTestData<OrgHeader>();
			RegisteredLocalOrg.OH_RL_NKClosestPort = "AUSYD";
			RegisteredLocalOrg.Addresses.AddNewMainAddress().WithLocation(unloco: "AUSYD", state: "NSW");
			ChargeCodeFRT = CreateChargeCode("FRT");
			WorkItem = Factory.New<WorkItem>();
			WorkItem.WKI_PortOrCountry = "AUSYD";
		}

		GlbCompany NZCompany;
		GlbCompany INCompany;
		OrgHeader UnregisteredLocalOrg;
		OrgHeader UnregisteredForeignOrg;
		OrgHeader RegisteredLocalOrg;
		OrgHeader RegisteredForeignOrg;
		OrgHeader BritishColumbiaOrg;
		OrgHeader IndiaOrg;
		OrgHeader TexasOrg;
		OrgHeader NowhereOrg;
		AccChargeCode ChargeCodeBAF;
		AccChargeCode ChargeCodeFRT;
		AccChargeCode ChargeCodeOLAB;
		AccPOSChargeCodeGroup GroupGRP;
		AccPOSChargeCodeGroup GroupGRP2;
		ForwardingShipment ImportShipment;
		IJobInvoicingPlugIn ImportShipmentAsPlugin => ImportShipment;
		ForwardingShipment ExportShipment;
		IJobInvoicingPlugIn ExportShipmentAsPlugin => ExportShipment;
		ForwardingShipment ConsolShipment;
		IJobInvoicingPlugIn ConsolShipmentAsPlugin => ConsolShipment;
		ForwardingConsol ForwardingConsol;
		IJobCostingPlugIn ForwardingConsolAsPlugin => ForwardingConsol;
		ForwardingConsol GatewayConsol;
		IJobCostingPlugIn GatewayConsolAsPlugin => GatewayConsol;
		DtbBookingConsolidation ConsolidatedTransportBooking;
		IJobCostingPlugIn ConsolidatedTransportBookingAsPlugin => ConsolidatedTransportBooking;
		IJobInvoicingPlugIn DeclarationAsPlugIn => Declaration;
		BaseJobDeclaration Declaration;
		QuotedBooking QuotedBooking;
		IJobInvoicingPlugIn QuotedBookingAsPlugin => QuotedBooking;
		IJobInvoicingPlugIn WorkItemAsPlugin => WorkItem;
		WorkItem WorkItem;

		ILocation CreateAUBNELocation() => CreateOrgAddressAsLocation(unloco: "AUBNE", state: "QLD");
		ILocation CreateUSELPLocation() => CreateOrgAddressAsLocation(unloco: "USELP", state: "TX");
		ILocation CreateINPLSLocation() => CreateOrgAddressAsLocation(unloco: "INPLS", state: "AP");
		ILocation CreateCAHLCLocation() => CreateOrgAddressAsLocation(unloco: "CAHLC", state: "BC");    // BCTZ tax zone
		ILocation CreateCABTVLocation() => CreateOrgAddressAsLocation(unloco: "CABTV", state: "AP");    // QUBC tax zone
		ILocation CreateCAESSLocation() => CreateOrgAddressAsLocation(unloco: "CAESS", state: "AP");    // HSTX and ONTZ tax zone (former is inactive)

		ILocation CreateOrgAddressAsLocation(string unloco = "AUBNE", string state = "QLD")
			=> Factory.NewWithValidTestData<OrgAddress>().WithLocation(unloco: unloco, state: state);

		GlbCompany CreateCompany(ZString countryCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "D" + countryCode;
			company.GC_RN_NKCountryCode = countryCode;
			return company;
		}

		AccPOSConfiguration CreateConfigurationRule(ZString jobType, ZString chargeType, ZString direction, string transportMode = "ALL", string incoTerm = "", string taxReg = "", string supplyType = "", AccPOSChargeCodeGroup group = null, AccChargeCode chargeCode = null, GlbBranch branch = null, GlbCompany company = null, string rule = "BIL")
		{
			var config = Factory.New<AccPOSConfiguration>();
			config.PSC_GC = (company ?? Env.CurrentCompany).PK;
			if (group != null)
			{
				config.PSC_ParentId = group.PK;
				config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			}
			else if (chargeCode != null)
			{
				config.PSC_ParentId = chargeCode.PK;
				config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			}
			config.PSC_JobType = jobType;
			config.PSC_ChargeType = chargeType;
			config.PSC_IncoTerm = incoTerm;
			config.PSC_ServiceDirection = direction;
			config.PSC_TransportMode = transportMode;
			config.PSC_TaxRegistrationType = taxReg;
			config.PSC_NK_Branch = branch?.GB_Code ?? ZString.Empty;
			config.PSC_SupplyType = supplyType;
			config.PSC_PlaceOfSupplyRule = rule ?? "BIL";
			return config;
		}

		#endregion

	}

	internal static class AccPlaceOfSupplyAddressHelper_ForTest
	{
		internal static OrgAddress WithLocation(this OrgAddress address, string unloco = "AUBNE", string state = "QLD", string capability = "OFC")
		{
			address.OA_RN_NKCountryCode = unloco.Substring(0, 2);
			address.State = state;
			address.OA_RL_NKRelatedPortCode = unloco;
			address.AddressCapability.GetAddressCapabilityOnCode(capability).Enabled = true;
			return address;
		}
	}
}
