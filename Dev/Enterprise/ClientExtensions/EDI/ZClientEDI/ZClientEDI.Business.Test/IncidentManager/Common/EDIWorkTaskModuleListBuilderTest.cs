using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class EDIWorkTaskModuleListBuilderTest : TestCaseWithFactory
	{
		public void TestBuild_WithCr8ModuleListType_WithoutProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();
			var actualModules = builder.Build(ModuleListType.Cr8, "CW1", ZString.Empty);
			var expectedCodes = new string[] {
					Cr8ModuleList.Codes.CarbonEnvironmentalCompliance,
					Cr8ModuleList.Codes.CarrierSupplierTariffRateUploadMapping,
					Cr8ModuleList.Codes.GeneralAccountingReportingRegulatory
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Cr8, "AAA", ZString.Empty);
			expectedCodes = new string[] {
					Cr8ModuleList.Codes.DangerousGoodsManagement,
					Cr8ModuleList.Codes.VatComplianceIssue,
					Cr8ModuleList.Codes.OtherComplianceIssue
				};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithCr8ModuleListType_WithProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();
			var actualModules = builder.Build(ModuleListType.Cr8, "CW1", ProductAreaList.Codes.ARC);
			var expectedCodes = new string[] {
					Cr8ModuleList.Codes.CarbonEnvironmentalCompliance
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Cr8, "AAA", ProductAreaList.Codes.CUS);
			expectedCodes = new string[] {
					Cr8ModuleList.Codes.VatComplianceIssue
				};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithCr9ModuleListType_WithoutProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();
			var actualModules = builder.Build(ModuleListType.Cr9, "CW1", ZString.Empty);
			var expectedCodes = new string[] {
					Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization,
					Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization,
					Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Cr9, "AAA", ZString.Empty);
			expectedCodes = new string[] {
					Cr9ModuleList.Codes.WisecloudOtherRequests,
					Cr9ModuleList.Codes.SelfHostedSqlInfrastructureUpdateAssistance,
					Cr9ModuleList.Codes.AccountingDataTakeOn
				};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithCr9ModuleListType_WithProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();
			var actualModules = builder.Build(ModuleListType.Cr9, "CW1", ProductAreaList.Codes.ARC);
			var expectedCodes = new string[] {
					Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization,
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Cr9, "AAA", ProductAreaList.Codes.CUS);
			expectedCodes = new string[] {
					Cr9ModuleList.Codes.SelfHostedSqlInfrastructureUpdateAssistance
				};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithAllModuleListType_WithoutProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();
			var actualModules = builder.Build(ModuleListType.Unspecified, "CW1", ZString.Empty);
			var expectedCodes = new string[] {
					Cr8ModuleList.Codes.CarbonEnvironmentalCompliance,
					Cr8ModuleList.Codes.CarrierSupplierTariffRateUploadMapping,
					Cr8ModuleList.Codes.GeneralAccountingReportingRegulatory,
					Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization,
					Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization,
					Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Unspecified, "AAA", ZString.Empty);
			expectedCodes = new string[] {
					"AA1",
					"AA2"
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Unspecified, "CCC", ZString.Empty);
			expectedCodes = new string[] {
					"CC1",
					Cr8ModuleList.Codes.ReferenceDataBaseData
				};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithAllModuleListType_WithProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();

			var actualModules = builder.Build(ModuleListType.Unspecified, "CW1", ProductAreaList.Codes.ARC);
			var expectedCodes = new string[] {
					Cr8ModuleList.Codes.CarbonEnvironmentalCompliance,
					Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization
				};
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Unspecified, "AAA", ProductAreaList.Codes.CUS);
			expectedCodes = Array.Empty<string>();
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.Unspecified, "AAA", "");
			expectedCodes = new string[] {
					"AA1",
					"AA2"
				};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithStandardModuleListType_WithoutProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();

			var actualModules = builder.Build(ModuleListType.MenuSection, "CW1", ZString.Empty);
			var expectedCodes = Array.Empty<string>();
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.MenuSection, "AAA", ZString.Empty);
			expectedCodes = new string[]
			{
				"AA1",
				"AA2"
			};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_WithStandardModuleListType_WithProductArea()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();

			var actualModules = builder.Build(ModuleListType.MenuSection, "CW1", ProductAreaList.Codes.ARC);
			var expectedCodes = Array.Empty<string>();
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.MenuSection, "AAA", ProductAreaList.Codes.CUS);
			expectedCodes = Array.Empty<string>();
			AssertModuleCodes(expectedCodes, actualModules);

			actualModules = builder.Build(ModuleListType.MenuSection, "AAA", "");
			expectedCodes = new string[]
			{
				"AA1",
				"AA2"
			};
			AssertModuleCodes(expectedCodes, actualModules);
		}

		public void TestBuild_DoesNotThrow()
		{
			var builder = new ModuleListBuilder();

			var actualModules = builder.Build(ModuleListType.MenuSection, "ZZZ", "");
			var expectedCodes = Array.Empty<string>();
			AssertNoExceptionThrown(() => { AssertModuleCodes(expectedCodes, actualModules); });
		}

		public void TestBuild_IncludeProductNameInModuleDescription()
		{
			SetupProductAreaLicenceModuleMappings();

			var builder = new ModuleListBuilder();
			var actualModules = builder.Build(ModuleListType.Unspecified, "", "");
			var expectedCodes = new string[] {
				"AA1", "AA2",
				"BB1",
				"CC1",
				Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, Cr8ModuleList.Codes.CarrierSupplierTariffRateUploadMapping , Cr8ModuleList.Codes.GeneralAccountingReportingRegulatory,
				Cr8ModuleList.Codes.ReferenceDataBaseData,
				Cr8ModuleList.Codes.DangerousGoodsManagement, Cr8ModuleList.Codes.VatComplianceIssue, Cr8ModuleList.Codes.OtherComplianceIssue,
				Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization, Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization, Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor,
				Cr9ModuleList.Codes.WisecloudOtherRequests, Cr9ModuleList.Codes.SelfHostedSqlInfrastructureUpdateAssistance, Cr9ModuleList.Codes.AccountingDataTakeOn
			};
			AssertModuleCodes(expectedCodes, actualModules);

			AssertEquals("[AAA] AA1", actualModules.GetDescriptionFromCode("AA1"));
			AssertEquals("[AAA] AA2", actualModules.GetDescriptionFromCode("AA2"));
			AssertEquals("[BBB] BB1", actualModules.GetDescriptionFromCode("BB1"));
			AssertEquals("[CCC] CC1", actualModules.GetDescriptionFromCode("CC1"));

			AssertEquals($"[CW1 test] {Cr8ModuleList.Descriptions.CarbonEnvironmentalCompliance}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.CarbonEnvironmentalCompliance));
			AssertEquals($"[CW1 test] {Cr8ModuleList.Descriptions.CarrierSupplierTariffRateUploadMapping}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.CarrierSupplierTariffRateUploadMapping));
			AssertEquals($"[CW1 test] {Cr8ModuleList.Descriptions.GeneralAccountingReportingRegulatory}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.GeneralAccountingReportingRegulatory));
			AssertEquals($"[CCC] {Cr8ModuleList.Descriptions.ReferenceDataBaseData}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.ReferenceDataBaseData));
			AssertEquals($"[AAA] {Cr8ModuleList.Descriptions.DangerousGoodsManagement}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.DangerousGoodsManagement));
			AssertEquals($"[AAA] {Cr8ModuleList.Descriptions.VatComplianceIssue}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.VatComplianceIssue));
			AssertEquals($"[AAA] {Cr8ModuleList.Descriptions.OtherComplianceIssue}", actualModules.GetDescriptionFromCode(Cr8ModuleList.Codes.OtherComplianceIssue));

			AssertEquals($"[CW1 test] {Cr9ModuleList.Descriptions.ConsultingAssistanceOnDocbuilderCustomization}", actualModules.GetDescriptionFromCode(Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization));
			AssertEquals($"[CW1 test] {Cr9ModuleList.Descriptions.ConsultingAssistanceOnReportCustomization}", actualModules.GetDescriptionFromCode(Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization));
			AssertEquals($"[CW1 test] {Cr9ModuleList.Descriptions.ConsultingAssistanceOnEadaptor}", actualModules.GetDescriptionFromCode(Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor));
			AssertEquals($"[AAA] {Cr9ModuleList.Descriptions.WisecloudOtherRequests}", actualModules.GetDescriptionFromCode(Cr9ModuleList.Codes.WisecloudOtherRequests));
			AssertEquals($"[AAA] {Cr9ModuleList.Descriptions.SelfHostedSqlInfrastructureUpdateAssistance}", actualModules.GetDescriptionFromCode(Cr9ModuleList.Codes.SelfHostedSqlInfrastructureUpdateAssistance));
			AssertEquals($"[AAA] {Cr9ModuleList.Descriptions.AccountingDataTakeOn}", actualModules.GetDescriptionFromCode(Cr9ModuleList.Codes.AccountingDataTakeOn));
		}

		public void TestBuild_NoProductName_EnabledAndInternalModuleOptions()
		{
			SetupProductAreaLicenceModuleMappings();
			var builder = new ModuleListBuilder();
			var internalEnabledModule1 = new CodeDescriptionPair("AA1", "[AAA] AA1");
			var externalEnabledModule2 = new CodeDescriptionPair("AA2", "[AAA] AA2");
			var externalDisabledModule3 = new CodeDescriptionPair("BB1", "[BBB] BB1");
			var internalDisabledModule4 = new CodeDescriptionPair("CC1", "[CCC] CC1");

			var excludeInternalTExcludeDisabledF = builder.Build(ModuleListType.Unspecified, "", "", true, false);
			AssertCollectionNotContains(internalEnabledModule1, excludeInternalTExcludeDisabledF);
			AssertCollectionNotContains(internalDisabledModule4, excludeInternalTExcludeDisabledF);
			AssertCollectionContains(externalDisabledModule3, excludeInternalTExcludeDisabledF);
			AssertCollectionContains(externalEnabledModule2, excludeInternalTExcludeDisabledF);

			var excludeInternalFExcludeDisabledF = builder.Build(ModuleListType.Unspecified, "", "", false, false);
			AssertCollectionContains(internalEnabledModule1, excludeInternalFExcludeDisabledF);
			AssertCollectionContains(externalEnabledModule2, excludeInternalFExcludeDisabledF);
			AssertCollectionContains(internalDisabledModule4, excludeInternalFExcludeDisabledF);
			AssertCollectionContains(externalDisabledModule3, excludeInternalFExcludeDisabledF);

			var excludeInternalFExcludeDisabledT = builder.Build(ModuleListType.Unspecified, "", "", false, true);
			AssertCollectionNotContains(internalDisabledModule4, excludeInternalFExcludeDisabledT);
			AssertCollectionNotContains(externalDisabledModule3, excludeInternalFExcludeDisabledT);
			AssertCollectionContains(internalEnabledModule1, excludeInternalFExcludeDisabledT);
			AssertCollectionContains(externalEnabledModule2, excludeInternalFExcludeDisabledT);

			var excludeInternalTExcludeDisabledT = builder.Build(ModuleListType.Unspecified, "", "", true, true);
			AssertCollectionNotContains(internalEnabledModule1, excludeInternalTExcludeDisabledT);
			AssertCollectionNotContains(internalDisabledModule4, excludeInternalTExcludeDisabledT);
			AssertCollectionNotContains(externalDisabledModule3, excludeInternalTExcludeDisabledT);
			AssertCollectionContains(externalEnabledModule2, excludeInternalTExcludeDisabledT);
		}

		#region Implementation

		void SetupProductAreaLicenceModuleMappings()
		{
			var productsAndModules = new SystemProductCollection();
			var productAAAModules = productsAndModules.AddNew("AAA", (NoResString)"AAA", true);
			var moduleAA1 = productAAAModules.ModuleMappings.AddNew("AA1", (NoResString)"AA1", "", true, true, true); // isInternal
			var moduleAA2 = productAAAModules.ModuleMappings.AddNew("AA2", (NoResString)"AA2", "", true, false, true); // external, enabled

			var productBBBModules = productsAndModules.AddNew("BBB", (NoResString)"BBB", true);
			var moduleBB1 = productBBBModules.ModuleMappings.AddNew("BB1", (NoResString)"BB1", "", true, false, false); // disabled, external

			var productCCCModules = productsAndModules.AddNew("CCC", (NoResString)"CCC", true);
			var moduleCC1 = productCCCModules.ModuleMappings.AddNew("CC1", (NoResString)"CC1", "", true, true, false); // disabled, internal

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productsAndModules);

			var cr8Collection = new SystemProductCollection();
			var productEntCr8 = cr8Collection.AddNew("CW1", "CW1 test", true);
			productEntCr8.ModuleMappings.AddNew(Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, Cr8ModuleList.Descriptions.CarbonEnvironmentalCompliance, ProductAreaList.Codes.ARC, true);
			productEntCr8.ModuleMappings.AddNew(Cr8ModuleList.Codes.CarrierSupplierTariffRateUploadMapping, Cr8ModuleList.Descriptions.CarrierSupplierTariffRateUploadMapping, ProductAreaList.Codes.CUS, true);
			productEntCr8.ModuleMappings.AddNew(Cr8ModuleList.Codes.GeneralAccountingReportingRegulatory, Cr8ModuleList.Descriptions.GeneralAccountingReportingRegulatory, ZString.Empty, true);

			cr8Collection.AddNew("CCC", "CCC", true).ModuleMappings.AddNew(Cr8ModuleList.Codes.ReferenceDataBaseData, Cr8ModuleList.Descriptions.ReferenceDataBaseData, ZString.Empty, true);

			var prodAcr8 = cr8Collection.AddNew("AAA", "AAA", true);
			prodAcr8.ModuleMappings.AddNew(Cr8ModuleList.Codes.DangerousGoodsManagement, Cr8ModuleList.Descriptions.DangerousGoodsManagement, ProductAreaList.Codes.ARC, true);
			prodAcr8.ModuleMappings.AddNew(Cr8ModuleList.Codes.VatComplianceIssue, Cr8ModuleList.Descriptions.VatComplianceIssue, ProductAreaList.Codes.CUS, true);
			prodAcr8.ModuleMappings.AddNew(Cr8ModuleList.Codes.OtherComplianceIssue, Cr8ModuleList.Descriptions.OtherComplianceIssue, ZString.Empty, true);

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Collection);

			var cr9Collection = new SystemProductCollection();

			var productEntCr9 = cr9Collection.AddNew("CW1", "CW1 test", true);
			productEntCr9.ModuleMappings.AddNew(Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization, Cr9ModuleList.Descriptions.ConsultingAssistanceOnDocbuilderCustomization, ProductAreaList.Codes.ARC, true);
			productEntCr9.ModuleMappings.AddNew(Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization, Cr9ModuleList.Descriptions.ConsultingAssistanceOnReportCustomization, ProductAreaList.Codes.CUS, true);
			productEntCr9.ModuleMappings.AddNew(Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor, Cr9ModuleList.Descriptions.ConsultingAssistanceOnEadaptor, ZString.Empty, true);

			var productAAA = cr9Collection.AddNew("AAA", "AAA", true);
			productAAA.ModuleMappings.AddNew(Cr9ModuleList.Codes.WisecloudOtherRequests, Cr9ModuleList.Descriptions.WisecloudOtherRequests, ProductAreaList.Codes.ARC, true);
			productAAA.ModuleMappings.AddNew(Cr9ModuleList.Codes.SelfHostedSqlInfrastructureUpdateAssistance, Cr9ModuleList.Descriptions.SelfHostedSqlInfrastructureUpdateAssistance, ProductAreaList.Codes.CUS, true);
			productAAA.ModuleMappings.AddNew(Cr9ModuleList.Codes.AccountingDataTakeOn, Cr9ModuleList.Descriptions.AccountingDataTakeOn, ZString.Empty, true);

			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Collection);
		}

		void AssertModuleCodes(IEnumerable<string> expectedCodes, CodeDescriptionPairList actualModules)
		{
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualModules.Cast<ICodeDescription>().Select(module => module.Code).Where(x => expectedCodes.Contains(x)));
		}

		#endregion
	}
}