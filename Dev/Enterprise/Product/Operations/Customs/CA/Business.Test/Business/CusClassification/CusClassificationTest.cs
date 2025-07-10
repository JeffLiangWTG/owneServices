using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusClassification.Loader))]
	class LoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusClassification.Loader(Factory);
		}
	}

	[TestedType(typeof(CusClassification))]
	public class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		public void TestSIMAMeasures()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			classification.CC_TariffNum = "1234567890";
			AssertEquals(1, classification.SIMAMeasures.Count);
			AssertEquals("AD1407", classification.SIMAMeasures[0].CA_DumpingNumber);
		}

		public void TestPopulateSIMADuties()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var countervailingRelationShip = universalHelper.CreateTariffRelationship(countervailingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var countervailingRate = universalHelper.CreateRate(countervailingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1234567890";
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(ZString.Empty, classification.CCA_SIMADumpingNumber);
			AssertEquals(1, classification.DutiesAndTaxes.Count);
			AssertEquals(DutyAndTaxTypes.Codes.SUR, classification.DutiesAndTaxes[0].C1_TaxType);

			var dutyRateForCurrentCountry = classification.DutyRateForCurrentCountry;
			AssertEquals(1, classification.DutiesAndTaxes.Count);

			classification.DutiesAndTaxes.DeleteAll();
			classification.OnRefreshSIMAMeasureEvent = null;
			classification.CC_TariffNum = "0123456789";
			AssertEquals(ZString.Empty, classification.CCA_SIMADumpingNumber);
			AssertEquals(0, classification.DutiesAndTaxes.Count);

			classification.OnRefreshSIMAMeasureEvent = delegate
			{
				return classification.SIMAMeasures.OfType<SIMADumpingNumber>().FirstOrDefault(x => x.CA_DumpingNumber == "AD1408");
			};
			classification.CC_TariffNum = ZString.Empty;
			classification.CC_TariffNum = "0123456789";
			AssertEquals("AD1408", classification.CCA_SIMADumpingNumber);
			AssertEquals(1, classification.DutiesAndTaxes.Count);
			AssertEquals(DutyAndTaxTypes.Codes.CVD, classification.DutiesAndTaxes[0].C1_TaxType);
		}

		public void TestSettingDescriptionFromTariff()
		{
			Classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			Classification.CC_Description = ZString.Empty;
			Classification.CC_TariffNum = exportTariffCode;
			AssertEquals("Description", exportTariffDescription, Classification.CC_Description);
			Classification.CC_Description = ZString.Empty;
			Classification.CC_TariffNum = customsTariffCode;
			AssertEquals("Description", ZString.Empty, Classification.CC_Description);

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, customsTariffCode, ZDateTime.Today.AddDays(-3), ZDateTime.MaxSmallDateTime, customsTariffDescriptionFromTariffView);
			Factory.Save();

			Classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			Classification.CC_Description = ZString.Empty;
			Classification.CC_TariffNum = exportTariffCode;
			AssertEquals("Description", ZString.Empty, Classification.CC_Description);
			Classification.CC_Description = ZString.Empty;
			Classification.CC_TariffNum = ZString.Empty;

			Classification.Factory.ClearCachedValue<TariffView>(string.Join("_", "LoadMostRecentCachedTariff", "CA", "HSN", customsTariffCode, ZDateTime.Today, null));
			Classification.CC_TariffNum = customsTariffCode;
			AssertEquals("Description from dbo.TariffView", customsTariffDescriptionFromTariffView, Classification.CC_Description);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		[TestDate(2010, 2, 23)]
		[ExpectNoExceptions]
		public void TestCC_FormattedTariffNumTariffInfo()
		{
			Classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			TariffPropertyInfoTest.AssertTariffInfo(Classification.CC_FormattedTariffNumTariffInfo, TariffType.Export, ZDateTime.Now, string.Empty);
			Classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			TariffPropertyInfoTest.AssertTariffInfo(Classification.CC_FormattedTariffNumTariffInfo, TariffType.Import, ZDateTime.Now, string.Empty);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseCusClassification to include a decider for this class", Factory.New(typeof(BaseCusClassification)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestITariffFormatProvider()
		{
			var classification = Factory.New<CusClassification>();
			AssertType<TariffFormatter>("TariffFormatter", ((ITariffFormatProvider)classification).TariffFormatter);
		}

		public void TestCusCAClassificationNotCreateWhenClassificationtIsDeleted()
		{
			var classification = Factory.NewWithValidTestData<CusClassification>();
			Factory.Save();
			_ = classification.Details;
			classification.Delete();
			_ = classification.Details.CCA_99TariffCode;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestCusCAClassificationWrapperProperties()
		{
			var caClassification = Factory.New<CusCAClassification>();
			var manufacturer = Guid.NewGuid();
			caClassification.CCA_OA_Manufacturer = manufacturer;
			caClassification.CCA_RN_NKOrigin = "A";
			caClassification.CCA_ProvinceOfOrigin = "B";
			caClassification.CCA_RN_NKSource = "C";
			caClassification.CCA_StateOfSource = "D";
			caClassification.CCA_Model = "E";
			caClassification.CCA_BrandName = "F";
			caClassification.CCA_CFIAIndicator = "G";
			caClassification.CCA_CNSCIndicator = "H";
			caClassification.CCA_DFOIndicator = "I";
			caClassification.CCA_ECCCIndicator = "J";
			caClassification.CCA_GACIndicator = "K";
			caClassification.CCA_HCIndicator = "L";
			caClassification.CCA_NRCanIndicator = "M";
			caClassification.CCA_PHACIndicator = "N";
			caClassification.CCA_TCIndicator = "O";

			var classification = Factory.New<CusClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;

			AssertEquals(caClassification.CCA_OA_Manufacturer, classification.CCA_OA_Manufacturer);
			AssertEquals(caClassification.CCA_RN_NKOrigin, classification.CCA_RN_NKOrigin);
			AssertEquals(caClassification.CCA_ProvinceOfOrigin, classification.CCA_ProvinceOfOrigin);
			AssertEquals(caClassification.CCA_RN_NKSource, classification.CCA_RN_NKSource);
			AssertEquals(caClassification.CCA_StateOfSource, classification.CCA_StateOfSource);
			AssertEquals(caClassification.CCA_Model, classification.CCA_Model);
			AssertEquals(caClassification.CCA_BrandName, classification.CCA_BrandName);
			AssertEquals(caClassification.CCA_CFIAIndicator, classification.CCA_CFIAIndicator);
			AssertEquals(caClassification.CCA_CNSCIndicator, classification.CCA_CNSCIndicator);
			AssertEquals(caClassification.CCA_DFOIndicator, classification.CCA_DFOIndicator);
			AssertEquals(caClassification.CCA_ECCCIndicator, classification.CCA_ECCCIndicator);
			AssertEquals(caClassification.CCA_GACIndicator, classification.CCA_GACIndicator);
			AssertEquals(caClassification.CCA_HCIndicator, classification.CCA_HCIndicator);
			AssertEquals(caClassification.CCA_NRCanIndicator, classification.CCA_NRCanIndicator);
			AssertEquals(caClassification.CCA_PHACIndicator, classification.CCA_PHACIndicator);
			AssertEquals(caClassification.CCA_TCIndicator, classification.CCA_TCIndicator);
		}

		public void TestIHasPGARequirements()
		{
			var caClassification = Factory.New<CusCAClassification>();
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			caClassification.CCA_OA_Manufacturer = manufacturer;
			caClassification.CCA_RN_NKOrigin = "A";
			caClassification.CCA_ProvinceOfOrigin = "B";
			caClassification.CCA_RN_NKSource = "C";
			caClassification.CCA_StateOfSource = "D";
			caClassification.CCA_Model = "E";
			caClassification.CCA_BrandName = "F";

			var classification = Factory.New<CusClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;
			classification.CC_TariffNum = "P";

			var iHasPGARequirements = classification as IHasPGARequirements;
			AssertEquals(9, iHasPGARequirements.PGARequirements.Count);
			AssertEquals(caClassification.CCA_OA_Manufacturer, iHasPGARequirements.OA_Manufacturer);
			AssertEquals(caClassification.CCA_OA_ManufacturerInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.OA_ManufacturerInfo).InnerInfo);
			AssertEquals(manufacturer, iHasPGARequirements.OA_ManufacturerAddress_ZAddress.AddressFK);
			AssertEquals(typeof(OrgHeaderCollection), iHasPGARequirements.ManufacturersLookup.GetType());
			AssertEquals(caClassification.CCA_RN_NKOrigin, iHasPGARequirements.RN_NKCountryOfOrigin);
			AssertEquals(caClassification.CCA_RN_NKOriginInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.RN_NKCountryOfOriginInfo).InnerInfo);
			AssertEquals(typeof(RefCountryCollection), iHasPGARequirements.CountryOfOriginsLookup.GetType());
			AssertEquals(caClassification.CCA_ProvinceOfOrigin, iHasPGARequirements.RW_NKOriginState);
			AssertEquals(caClassification.CCA_ProvinceOfOriginInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.RW_NKOriginStateInfo).InnerInfo);
			AssertEquals(caClassification.Lookups.StatesOfOrigin, iHasPGARequirements.StateCodeListLookup);
			AssertEquals(caClassification.CCA_RN_NKSource, iHasPGARequirements.RN_NKCountryOfSource);
			AssertEquals(caClassification.CCA_RN_NKSourceInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.RN_NKCountryOfSourceInfo).InnerInfo);
			AssertEquals(typeof(RefCountryCollection), iHasPGARequirements.CountryOfSourceLookup.GetType());
			AssertEquals(caClassification.CCA_StateOfSource, iHasPGARequirements.RW_NKCountryOfSourceState);
			AssertEquals(caClassification.CCA_StateOfSourceInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.RW_NKCountryOfSourceStateInfo).InnerInfo);
			AssertEquals(caClassification.Lookups.CFIAStatesOfOrigin, iHasPGARequirements.CountryOfSourceStateLookup);
			AssertEquals(caClassification.CCA_BrandName, iHasPGARequirements.JI_BrandName);
			AssertEquals(caClassification.CCA_BrandNameInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.JI_BrandNameInfo).InnerInfo);
			AssertEquals(caClassification.CCA_Model, iHasPGARequirements.JI_Model);
			AssertEquals(caClassification.CCA_ModelInfo, ((ZWrappedPropertyInfo)iHasPGARequirements.JI_ModelInfo).InnerInfo);
			AssertEquals(classification.CC_TariffNum, iHasPGARequirements.Tariff);
			AssertEquals(classification.CC_TariffNumInfo, iHasPGARequirements.TariffInfo);
		}

		public void TestIPGARequirementSupporter()
		{
			var caClassification = Factory.New<CusCAClassification>();
			caClassification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_CNSCIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_DFOIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_ECCCIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_GACIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_HCIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_NRCanIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_PHACIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_TCIndicator = YesNoList.Codes.Yes;

			var classification = Factory.New<CusClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;
			classification.CC_ClassificationType = ClassificationType.IMP;
			var iPGARequirementSupporter = classification as IPGARequirementSupporter;

			AssertEquals(classification.Factory, iPGARequirementSupporter.Factory);
			AssertEquals(classification.IsDeleted, iPGARequirementSupporter.IsDeleted);
			AssertEquals(classification.CC_TariffNum, iPGARequirementSupporter.Tariff);
			AssertEquals(ZDate.Today, iPGARequirementSupporter.EffectiveDate);
			AssertEquals(classification.IsHTS, iPGARequirementSupporter.IsPGARequirementEffective);
			AssertEquals(caClassification.CCA_HCIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.HCIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_PHACIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.PHACIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_NRCanIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.NRCanIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_DFOIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.DFOIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_GACIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.GACIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_ECCCIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.ECCCIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_CNSCIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.CNSCIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_TCIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.TCIndInfo).InnerInfo);
			AssertEquals(caClassification.CCA_CFIAIndicatorInfo, ((ZWrappedPropertyInfo)iPGARequirementSupporter.CFIAIndInfo).InnerInfo);
			AssertEquals(classification.CFIAPGAHeader, iPGARequirementSupporter.CFIARequirementProvider);
			AssertEquals(classification.CNSCPGAHeader, iPGARequirementSupporter.CNSCRequirementProvider);
			AssertEquals(classification.DFOPGAHeader, iPGARequirementSupporter.DFORequirementProvider);
			AssertEquals(classification.ECCCPGAHeader, iPGARequirementSupporter.ECCCRequirementProvider);
			AssertEquals(classification.GACPGAHeader, iPGARequirementSupporter.GACRequirementProvider);
			AssertEquals(classification.HCPGAHeader, iPGARequirementSupporter.HCRequirementProvider);
			AssertEquals(classification.NRCanPGAHeader, iPGARequirementSupporter.NRCanRequirementProvider);
			AssertEquals(classification.PHACPGAHeader, iPGARequirementSupporter.PHACRequirementProvider);
			AssertEquals(classification.TCPGAHeader, iPGARequirementSupporter.TCRequirementProvider);
		}

		public void TestSuspendedProperties()
		{
			var classification = Factory.New<CusClassification>();
			classification.SetterSuspender.SuspendSetting(new ZString[] {
				CusClassification.Schema.CCA_OA_Manufacturer,
				CusClassification.Schema.CCA_HCIndicator,
				CusClassification.Schema.CCA_PHACIndicator,
				CusClassification.Schema.CCA_NRCanIndicator,
				CusClassification.Schema.CCA_DFOIndicator,
				CusClassification.Schema.CCA_GACIndicator,
				CusClassification.Schema.CCA_CFIAIndicator,
				CusClassification.Schema.CCA_CNSCIndicator,
				CusClassification.Schema.CCA_ECCCIndicator,
				CusClassification.Schema.CCA_TCIndicator
			});

			var oldGuidValue = classification.CCA_OA_Manufacturer;
			classification.CCA_OA_Manufacturer = Guid.NewGuid();
			AssertEquals(oldGuidValue, classification.CCA_OA_Manufacturer);

			var oldValue = classification.CCA_HCIndicator;
			classification.CCA_HCIndicator = "1";
			AssertEquals(oldValue, classification.CCA_HCIndicator);

			oldValue = classification.CCA_PHACIndicator;
			classification.CCA_PHACIndicator = "1";
			AssertEquals(oldValue, classification.CCA_PHACIndicator);

			oldValue = classification.CCA_NRCanIndicator;
			classification.CCA_NRCanIndicator = "1";
			AssertEquals(oldValue, classification.CCA_NRCanIndicator);

			oldValue = classification.CCA_DFOIndicator;
			classification.CCA_DFOIndicator = "1";
			AssertEquals(oldValue, classification.CCA_DFOIndicator);

			oldValue = classification.CCA_GACIndicator;
			classification.CCA_GACIndicator = "1";
			AssertEquals(oldValue, classification.CCA_GACIndicator);

			oldValue = classification.CCA_CFIAIndicator;
			classification.CCA_CFIAIndicator = "1";
			AssertEquals(oldValue, classification.CCA_CFIAIndicator);

			oldValue = classification.CCA_CNSCIndicator;
			classification.CCA_CNSCIndicator = "1";
			AssertEquals(oldValue, classification.CCA_CNSCIndicator);

			oldValue = classification.CCA_ECCCIndicator;
			classification.CCA_ECCCIndicator = "1";
			AssertEquals(oldValue, classification.CCA_ECCCIndicator);

			oldValue = classification.CCA_TCIndicator;
			classification.CCA_TCIndicator = "1";
			AssertEquals(oldValue, classification.CCA_TCIndicator);
		}

		#region Implementation

		protected new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}

		const string customsTariffCode = "1234567890";
		readonly string customsTariffDescriptionFromTariffView = "CUSTOMS TARIFF DESCRIPTION FROM dbo.TariffView";
		const string exportTariffCode = "12345678";
		const string exportTariffDescription = "EXPORT TARIFF DESCRIPTION";
		const string exportTariffUnits = "KGM";

		protected override void SetUp()
		{
			base.SetUp();
			var cacExportTariff = Factory.New<CACExportTariff>();
			cacExportTariff.CE_Code = exportTariffCode;
			cacExportTariff.CE_Description = exportTariffDescription;
			cacExportTariff.CE_Unit = exportTariffUnits;
			Factory.Save();
		}

		#endregion
	}
}
