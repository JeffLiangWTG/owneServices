using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProduceType()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(10, quarantineHeader.Lookups.ProduceType.Count);
				Assert(quarantineHeader.Lookups.ProduceType.ContainsCode(EXDOCCommodityCodes.Codes.OtherGoods));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals(9, quarantineHeader.Lookups.ProduceType.Count);
				Assert(!quarantineHeader.Lookups.ProduceType.ContainsCode(EXDOCCommodityCodes.Codes.OtherGoods));
			}
		}

		public void TestTemperatureUnit()
		{
			AssertSame(Factory.GetCachedValue<EXDOCTemperatureUnitCodes>(), quarantineHeader.Lookups.TemperatureUnit);
		}

		public void TestCertificatePrintCode()
		{
			AssertSame(Factory.GetCachedValue<EXDOCCertificatePrintCodes>(), quarantineHeader.Lookups.CertificatePrintCode);
		}

		public void TestProductUseIndicatorList()
		{
			AssertSame(Factory.GetCachedValue<EXDOCProductUseIndicatorCodes>(), quarantineHeader.Lookups.ProductUseIndicatorList);

			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertSame("Additional codes for Nexdocs", Factory.GetCachedValue<NEXDOCProductUseIndicatorCodes>(), quarantineHeader.Lookups.ProductUseIndicatorList);
		}

		public void TestLocation()
		{
			AssertSame(Factory.GetCachedValue<EXDOCCodeOrganisation>(), quarantineHeader.Lookups.Location);
		}

		public void TestComplianceCodes()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertSame(Factory.GetCachedValue<EXDOCComplianceStatusCodes>(), quarantineHeader.Lookups.ComplianceCodes);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var cachedCode = Factory.GetCachedValue(string.Concat("QuarantineExDocHeaderLookupsComplianceCodes"), () =>
				{
					var result = new CodeDescriptionPairList();

					result.AddPair(EXDOCComplianceStatusCodes.Codes.Completed, EXDOCComplianceStatusCodes.Descriptions.Completed);
					result.AddPair(EXDOCComplianceStatusCodes.Codes.Final, EXDOCComplianceStatusCodes.Descriptions.Final);
					result.AddPair(EXDOCComplianceStatusCodes.Codes.CertificateReady, EXDOCComplianceStatusCodes.Descriptions.CertificateReady);
					result.AddPair(EXDOCComplianceStatusCodes.Codes.Initial, EXDOCComplianceStatusCodes.Descriptions.Initial);
					result.AddPair(EXDOCComplianceStatusCodes.Codes.Order, EXDOCComplianceStatusCodes.Descriptions.Order);

					return result;
				});

				AssertSame(cachedCode, quarantineHeader.Lookups.ComplianceCodes);
			}
		}

		public void TestTrueAndCompleteIndicatorList()
		{
			AssertSame(Factory.GetCachedValue<EXDOCYesNoEmpty>(), quarantineHeader.Lookups.TrueAndCompleteIndicatorList);
		}

		public void TestImportedProduct()
		{
			AssertSame(Factory.GetCachedValue<EXDOCYesNoEmpty>(), quarantineHeader.Lookups.ImportedProduct);
		}

		public void TestEXDOCAverageAgeOfAnimalsList()
		{
			AssertSame(Factory.GetCachedValue<EXDOCAverageAgeOfAnimalsCodes>(), quarantineHeader.Lookups.EXDOCAverageAgeOfAnimalsList);
		}

		public void TestEXDOCTransitLocationTypeList()
		{
			AssertSame(Factory.GetCachedValue<EXDOCTransitLocationTypeCodes>(), quarantineHeader.Lookups.EXDOCTransitLocationTypeList);
		}

		public void TestEDIUser()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateRegCode(org1, "NEI");
			CreateRegCode(org2, "EEU");
			CreateRegCode(org3, "NEI", "EEU");

			Factory.Save();
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var ediUsers = quarantineHeader.Lookups.EDIUser;
				ediUsers.Load();
				AssertContainsExactElementsInAnyOrder(new[] { org2, org3 }, ediUsers);
				AssertEquals("Organisation does not contain a registration number / code for: (EEU) EXDOC Edi User", ediUsers.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.NewWithValidTestData<OrgHeader>()));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var ediUsers = quarantineHeader.Lookups.EDIUser;
				ediUsers.Load();
				AssertContainsExactElementsInAnyOrder(new[] { org1, org3 }, ediUsers);
				AssertEquals("Organisation does not contain a registration number / code for: (NEI) NEXDOCS External ID", ediUsers.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.NewWithValidTestData<OrgHeader>()));
			}
		}

		public void TestExporterNumber()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateRegCode(org1, "NEN");
			CreateRegCode(org2, "EEN");
			CreateRegCode(org3, "NEN", "EEN");

			Factory.Save();
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var exporterNumber = quarantineHeader.Lookups.ExporterNumber;
				exporterNumber.Load();
				AssertContainsExactElementsInAnyOrder(new[] { org2, org3 }, exporterNumber);
				AssertEquals("Organisation does not contain a registration number / code for: (EEN) EXDOC Exporter Number", exporterNumber.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.NewWithValidTestData<OrgHeader>()));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var exporterNumber = quarantineHeader.Lookups.ExporterNumber;
				exporterNumber.Load();
				AssertContainsExactElementsInAnyOrder(new[] { org1, org3 }, exporterNumber);
				AssertEquals("Organisation does not contain a registration number / code for: (NEN) NEXDOCS Exporter Number", exporterNumber.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.NewWithValidTestData<OrgHeader>()));
			}
		}

		public void TestEstablishment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			CreateRegCode(org1, "ESN");
			CreateRegCode(org2, "NSN");

			Factory.Save();

			var est = quarantineHeader.Lookups.Establishment;
			est.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1 }, est);
			AssertEquals("Organisation does not contain a registration number / code for: (ESN) EXDOC Establishment Number", est.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.NewWithValidTestData<OrgHeader>()));
		}

		public void TestTypes()
		{
			AssertType<EXDOCApprovedCertifierCollection>(quarantineHeader.Lookups.EXDOCApprovedCertifiers);
			AssertType<EXDOCAverageAgeOfAnimalsCodes>(quarantineHeader.Lookups.EXDOCAverageAgeOfAnimalsList);
			AssertType<EXDOCTransitLocationTypeCodes>(quarantineHeader.Lookups.EXDOCTransitLocationTypeList);
		}

		public void TestDeclarationOfCompliance()
		{
			AssertEquals(", NO, YES", quarantineHeader.Lookups.DeclarationOfCompliance.CodesAsString);
			AssertSame(Factory.GetCachedValue<EXDOCYesNoEmpty>(), quarantineHeader.Lookups.DeclarationOfCompliance);
		}

		public void TestLocationWithAqisPlace()
		{
			AssertSame(Factory.GetCachedValue("QuarantineExDocHeader_LocationWithAqisPlace", quarantineHeader.Lookups.GetLocationWithAqisPlace), quarantineHeader.Lookups.LocationWithAqisPlace);
		}

		public void TestAqisPlaces()
		{
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var message = "AqisPlaces";
			var filterBusinessObjectDefaults = quarantineHeader.Lookups.AqisPlaces.FilterBusinessObjectDefaults;
			AssertCollectionContains(message, new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)QuarantineExDocHeaderLookups.NPRTR), filterBusinessObjectDefaults);
			AssertCollectionContains(message, new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.CountryCodes.Australia), filterBusinessObjectDefaults);
			AssertCollectionContains(message, new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDateTime.Today), filterBusinessObjectDefaults);
			AssertCollectionContains(message, new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)QuarantineExDocHeaderLookups.CommodityTypeCode), filterBusinessObjectDefaults);
			AssertCollectionContains(message, new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", quarantineHeader.Lookups.GetNexdocAttributeValue()), filterBusinessObjectDefaults);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
		}
		QuarantineExDocHeader quarantineHeader;

		void CreateRegCode(OrgHeader org, params string[] codes)
		{
			foreach (var code in codes)
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				cusCode.OK_CodeType = code;
				cusCode.OK_CustomsRegNo = "1234";
			}
		}
	}
}
