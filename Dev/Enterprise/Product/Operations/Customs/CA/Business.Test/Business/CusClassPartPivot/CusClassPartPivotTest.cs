using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CusClassPartPivotTest : TestCaseWithFactory
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
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "1234567890";
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(1, importPivot.SIMAMeasures.Count);
			AssertEquals("AD1407", importPivot.SIMAMeasures[0].CA_DumpingNumber);
		}

		public void TestCCA_RN_NKSourceCountryAndState()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;

			Pivot.CCA_RN_NKSource = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			Assert(!Pivot.CCA_StateOfSourceInfo.ReadOnly);
			Pivot.CCA_StateOfSource = USStatesList.Codes.Alaska;

			Pivot.CCA_RN_NKSource = Enterprise.Core.Constants.CountryCodes.Canada;
			Assert(Pivot.CCA_StateOfSourceInfo.ReadOnly);
			AssertEquals(ZString.Empty, Pivot.CCA_StateOfSource);

			int i = 0;
			Pivot.CCA_StateOfSourceInfo.ValueChanged += delegate
			{ i++; };
			Pivot.CCA_RN_NKSource = Enterprise.Core.Constants.CountryCodes.Denmark;
			Assert("No duplicate setter called", 0 == i);

			Pivot.CCA_RN_NKSource = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			Pivot.CCA_StateOfSource = "AL";
			Pivot.CCA_RN_NKSource = Enterprise.Core.Constants.CountryCodes.Canada;
			Assert("Setter called", i > 1);
			Pivot.CCA_StateOfSourceInfo.ValueChanged -= delegate
			{ i++; };
		}

		public void TestDefaultValues()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Pivot.CI_RN_NKCountry);
			AssertEquals("Pivot Type-HTI", ClassificationTypeList.Codes.HTI, Pivot.CI_ChildType);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var pivot2 = Factory.New<CusClassPartPivot>();
			AssertEquals("Pivot Type-HTI", ClassificationTypeList.Codes.HTI, pivot2.CI_ChildType);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseCusClassPartPivot to include a decider for this class", Factory.New(typeof(BaseCusClassPartPivot)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestCI_ChildType()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_CNSCIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_DFOIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_ECCCIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_GACIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_HCIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_NRCanIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_PHACIndicator = YesNoList.Codes.Yes;
			Pivot.CCA_TCIndicator = YesNoList.Codes.Yes;

			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("CCA_CFIAIndicator", ZString.Empty, Pivot.CCA_CFIAIndicator);
			AssertEquals("CCA_CNSCIndicator", ZString.Empty, Pivot.CCA_CNSCIndicator);
			AssertEquals("CCA_DFOIndicator", ZString.Empty, Pivot.CCA_CNSCIndicator);
			AssertEquals("CCA_ECCCIndicator", ZString.Empty, Pivot.CCA_DFOIndicator);
			AssertEquals("CCA_GACIndicator", ZString.Empty, Pivot.CCA_ECCCIndicator);
			AssertEquals("CCA_HCIndicator", ZString.Empty, Pivot.CCA_GACIndicator);
			AssertEquals("CCA_NRCanIndicator", ZString.Empty, Pivot.CCA_HCIndicator);
			AssertEquals("CCA_PHACIndicator", ZString.Empty, Pivot.CCA_NRCanIndicator);
			AssertEquals("CCA_TCIndicator", ZString.Empty, Pivot.CCA_TCIndicator);
		}

		public void TestCI_ChildTypeDescription()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("ChildTypeDescription", ClassificationTypeList.Descriptions.HTI, Pivot.CI_ChildTypeDescription);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("ChildTypeDescription", ClassificationTypeList.Descriptions.HTE, Pivot.CI_ChildTypeDescription);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals("ChildTypeDescription", ClassificationTypeList.Descriptions.SHB, Pivot.CI_ChildTypeDescription);
		}

		[TestDate(2010, 2, 23)]
		[ExpectNoExceptions]
		public void TestCI_TariffNumTariffInfo()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			TariffPropertyInfoTest.AssertTariffInfo(Pivot.CI_TariffNumTariffInfo, TariffType.Export, ZDateTime.Now, string.Empty);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			TariffPropertyInfoTest.AssertTariffInfo(Pivot.CI_TariffNumTariffInfo, TariffType.Import, ZDateTime.Now, string.Empty);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			TariffPropertyInfoTest.AssertTariffInfo(Pivot.CI_TariffNumTariffInfo, TariffType.Import, ZDateTime.Now, string.Empty);
		}

		public void TestReadOnlys()
		{
			Assert("CI_LastAuditedDate_ReadOnly", Pivot.CI_LastAuditedDateInfo.ReadOnly);
			Pivot.CI_TariffNum = ZString.Empty;
			Pivot.CI_CC = ZGuid.Empty;
			Assert("CI_TariffNum_ReadOnly", !Pivot.CI_TariffNumInfo.ReadOnly);
			Assert("CI_CC_ReadOnly", !Pivot.CI_CCInfo.ReadOnly);
			Pivot.CI_TariffNum = "1";
			Assert("CI_TariffNum_ReadOnly", !Pivot.CI_TariffNumInfo.ReadOnly);
			Assert("CI_CC_ReadOnly", Pivot.CI_CCInfo.ReadOnly);
			Pivot.CI_TariffNum = ZString.Empty;
			Pivot.CI_CC = Factory.New<CusClassification>().PK;
			Assert("CI_TariffNum_ReadOnly", Pivot.CI_TariffNumInfo.ReadOnly);
			Assert("CI_CC_ReadOnly", !Pivot.CI_CCInfo.ReadOnly);
		}

		public void TestLastAuditedUserFullName()
		{
			Pivot.CI_LastAuditedUser = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Last Audited User Full Name", GlbStaff.CurrentUser.GS_FullName + "(" + GlbStaff.CurrentUser.GS_Code + ")", Pivot.LastAuditedUserFullName);
		}

		public void TestLogWhenTariffNumberChanged()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "00000000";
			var product = Factory.New<OrgSupplierPart>();
			Pivot.CI_OP = product.PK;
			Pivot.CI_TariffNum = "1111.11.11";
			AssertEquals("11111111", Pivot.TariffNumber);
			Pivot.CI_CC = classification.PK;
			Pivot.CI_TariffNum = ZString.Empty;
			AssertEquals("00000000", Pivot.TariffNumber);

			Pivot.CI_TariffNum = "1234567890";
			Pivot.CI_TariffNum = "1234566666";
			AssertContainsLog(Pivot.Logs, "Product Class. Tariff # '1234.56.78 90' changed to '1234.56.66 66'", true);
			AssertContainsLog(Pivot.Part.Logs, "Product Class. Tariff # '1234.56.78 90' changed to '1234.56.66 66'", false);
		}

		public void TestCCProperties()
		{
			Pivot.CCA_99TariffCode = "9901";
			Pivot.CCA_ValueForDutyCode = "13";
			Pivot.CCA_AuthorityNumber = "AUTH1";
			Pivot.CCA_TRSNumber = "TRS1";
			Pivot.CCA_TreatmentCode = "11";

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_TariffNum = "1234567890";
			classification.CC_Description = "1234567890";
			classification.CC_LookupCode = "1234567890";
			classification.CCA_99TariffCode = "9902";
			classification.CCA_ValueForDutyCode = "14";
			classification.CCA_AuthorityNumber = "AUTH2";
			classification.CCA_TRSNumber = "TRS2";
			classification.CCA_TreatmentCode = "10";

			var caClassification = classification.Details;
			caClassification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_RN_NKSource = "US";
			caClassification.CCA_StateOfSource = "AL";
			caClassification.CCA_GSTStatusCode = "48";
			caClassification.CCA_ETRateCode = "E01";
			caClassification.CCA_ETExemption = "85";
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;

			AssertEquals("CI_CC_FormattedTariff", ZString.Empty, Pivot.CI_CC_FormattedTariff);
			AssertEquals("CI_CC_CA_99TariffCode", ZString.Empty, Pivot.CI_CC_CA_99TariffCode);
			AssertEquals("CI_CC_CA_ValueForDutyCode", ZString.Empty, Pivot.CI_CC_CA_ValueForDutyCode);
			AssertEquals("CI_CCCA_TreatmentCode", ZString.Empty, Pivot.CI_CC_CA_TreatmentCode);
			AssertEquals("CI_CC_CA_AuthorityNumber", ZString.Empty, Pivot.CI_CC_CA_AuthorityNumber);
			AssertEquals("CI_CC_CA_TRSNumber", ZString.Empty, Pivot.CI_CC_CA_TRSNumber);
			AssertEquals("CI_CC_CA_TreatmentCode", ZString.Empty, Pivot.CI_CC_CA_TreatmentCode);
			AssertEquals("CI_CC_CA_GSTStatusCode", ZString.Empty, Pivot.CI_CC_CA_GSTStatusCode);
			AssertEquals("CI_CC_CA_ETRateCode", ZString.Empty, Pivot.CI_CC_CA_ETRateCode);
			AssertEquals("CI_CC_CA_ETExemption", ZString.Empty, Pivot.CI_CC_CA_ETExemption);
			Pivot.CI_CC = classification.PK;
			AssertEquals("CI_CC_FormattedTariff", "1234.56.78 90", Pivot.CI_CC_FormattedTariff);
			AssertEquals("CI_CC_CA_99TariffCode", "9902", Pivot.CI_CC_CA_99TariffCode);
			AssertEquals("CI_CC_CA_ValueForDutyCode", "14", Pivot.CI_CC_CA_ValueForDutyCode);
			AssertEquals("CI_CC_CA_AuthorityNumber", "AUTH2", Pivot.CI_CC_CA_AuthorityNumber);
			AssertEquals("CI_CC_CA_TRSNumber", "TRS2", Pivot.CI_CC_CA_TRSNumber);
			AssertEquals("CI_CC_CA_TreatmentCode", "10", Pivot.CI_CC_CA_TreatmentCode);
			AssertEquals("CI_CC_CA_GSTStatusCode", "48", Pivot.CI_CC_CA_GSTStatusCode);
			AssertEquals("CI_CC_CA_ETRateCode", "E01", Pivot.CI_CC_CA_ETRateCode);
			AssertEquals("CI_CC_CA_ETExemption", "85", Pivot.CI_CC_CA_ETExemption);
			var classificationNew = Factory.New<CusClassification>();
			classificationNew.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classificationNew.CC_TariffNum = "1234566666";
			classificationNew.CCA_99TariffCode = "9999";
			classificationNew.CCA_ValueForDutyCode = "14";
			classificationNew.CCA_AuthorityNumber = "AUTH2";
			classificationNew.CCA_TRSNumber = "TRS2";
			classificationNew.CCA_TreatmentCode = "10";
			Pivot.CI_CC = classificationNew.PK;

			AssertContainsLog(Pivot.Logs, "Classification Lookup Class. Tariff # '1234.56.78 90' changed to '1234.56.66 66'", true);
			AssertContainsLog(Pivot.Logs, "Classification Lookup Tariff Code '9902' changed to '9999'", true);
		}

		public void TestPGADataShouldFromClassificationWhenClassificationIsNotNULL()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_LastAuditedUser = "USR";
			pivot.CI_LastAuditedDate = ZDateTime.BrettsBirthday;
			pivot.CCA_RN_NKSource = "US";
			pivot.CCA_StateOfSource = "TX";
			Factory.Save();

			pivot.CCA_HCIndicator = YesNoList.Codes.Yes;
			pivot.CCA_PHACIndicator = YesNoList.Codes.Yes;
			pivot.CCA_NRCanIndicator = YesNoList.Codes.Yes;
			pivot.CCA_DFOIndicator = YesNoList.Codes.Yes;
			pivot.CCA_GACIndicator = YesNoList.Codes.Yes;
			pivot.CCA_ECCCIndicator = YesNoList.Codes.Yes;
			pivot.CCA_CNSCIndicator = YesNoList.Codes.Yes;
			pivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			AssertEquals("should from pivot", pivot.HCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.HC).PGAHeader);
			AssertEquals("should from pivot", pivot.PHACPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.PHAC).PGAHeader);
			AssertEquals("should from pivot", pivot.NRCanPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.NRCan).PGAHeader);
			AssertEquals("should from pivot", pivot.DFOPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.DFO).PGAHeader);
			AssertEquals("should from pivot", pivot.GACPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.GAC).PGAHeader);
			AssertEquals("should from pivot", pivot.ECCCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.ECCC).PGAHeader);
			AssertEquals("should from pivot", pivot.CNSCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.CNSC).PGAHeader);
			AssertEquals("should from pivot", pivot.TCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.TC).PGAHeader);
			AssertEquals("should from pivot", pivot.CFIAPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.CFIA).PGAHeader);
			Assert("should not readonly", !pivot.PGARequirements.ReadOnly);

			var caClassification = Factory.New<CusCAClassification>();
			caClassification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_RN_NKSource = "US";
			caClassification.CCA_StateOfSource = "AL";

			var classification = Factory.New<CusClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;
			classification.CC_TariffNum = "P";
			classification.CCA_HCIndicator = YesNoList.Codes.Yes;
			classification.CCA_PHACIndicator = YesNoList.Codes.Yes;
			classification.CCA_NRCanIndicator = YesNoList.Codes.Yes;
			classification.CCA_DFOIndicator = YesNoList.Codes.Yes;
			classification.CCA_GACIndicator = YesNoList.Codes.Yes;
			classification.CCA_ECCCIndicator = YesNoList.Codes.Yes;
			classification.CCA_CNSCIndicator = YesNoList.Codes.Yes;
			classification.CCA_TCIndicator = YesNoList.Codes.Yes;
			classification.CCA_CFIAIndicator = YesNoList.Codes.Yes;

			pivot.CI_CC = classification.PK;

			AssertEquals("should from classification", classification.HCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.HC).PGAHeader);
			AssertEquals("should from classification", classification.PHACPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.PHAC).PGAHeader);
			AssertEquals("should from classification", classification.NRCanPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.NRCan).PGAHeader);
			AssertEquals("should from classification", classification.DFOPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.DFO).PGAHeader);
			AssertEquals("should from classification", classification.GACPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.GAC).PGAHeader);
			AssertEquals("should from classification", classification.ECCCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.ECCC).PGAHeader);
			AssertEquals("should from classification", classification.CNSCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.CNSC).PGAHeader);
			AssertEquals("should from classification", classification.TCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.TC).PGAHeader);
			AssertEquals("should from classification", classification.CFIAPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.CFIA).PGAHeader);

			Assert("should readonly", pivot.PGARequirements.ReadOnly);
			Assert("should readonly", classification.CFIAPGAHeader.ReadOnly);
			Assert("should readonly", classification.CNSCPGAHeader.ReadOnly);
			Assert("should readonly", classification.DFOPGAHeader.ReadOnly);
			Assert("should readonly", classification.ECCCPGAHeader.ReadOnly);
			Assert("should readonly", classification.GACPGAHeader.ReadOnly);
			Assert("should readonly", classification.HCPGAHeader.ReadOnly);
			Assert("should readonly", classification.NRCanPGAHeader.ReadOnly);
			Assert("should readonly", classification.PHACPGAHeader.ReadOnly);
			Assert("should readonly", classification.TCPGAHeader.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).JI_BrandNameInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).JI_ModelInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).OA_ManufacturerInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).RN_NKCountryOfOriginInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).RN_NKCountryOfSourceInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).RW_NKCountryOfSourceStateInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).RW_NKCountryOfSourceStateInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).RW_NKOriginStateInfo.ReadOnly);
			Assert("should readonly", ((IHasPGARequirements)classification).TariffInfo.ReadOnly);

			Assert(!pivot.PGARequirements.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
			caClassification = Factory.New<CusCAClassification>();
			caClassification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_RN_NKSource = "US";
			caClassification.CCA_StateOfSource = "AL";

			classification = Factory.New<CusClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;
			classification.CC_TariffNum = "P";
			classification.CCA_HCIndicator = YesNoList.Codes.Yes;
			classification.HCPGAHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			pivot.CI_CC = classification.PK;
			Assert(pivot.PGARequirements.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));

			pivot.CI_CC = ZGuid.Empty;
			AssertEquals("should from pivot", pivot.HCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.HC).PGAHeader);
			AssertEquals("should from pivot", pivot.PHACPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.PHAC).PGAHeader);
			AssertEquals("should from pivot", pivot.NRCanPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.NRCan).PGAHeader);
			AssertEquals("should from pivot", pivot.DFOPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.DFO).PGAHeader);
			AssertEquals("should from pivot", pivot.GACPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.GAC).PGAHeader);
			AssertEquals("should from pivot", pivot.ECCCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.ECCC).PGAHeader);
			AssertEquals("should from pivot", pivot.CNSCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.CNSC).PGAHeader);
			AssertEquals("should from pivot", pivot.TCPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.TC).PGAHeader);
			AssertEquals("should from pivot", pivot.CFIAPGAHeader, pivot.PGARequirements.PGARequirement(PGACodes.Codes.CFIA).PGAHeader);
			Assert("should not readonly", !pivot.PGARequirements.ReadOnly);
			Assert(!pivot.PGARequirements.HasPGAProgramCodesDeclared(PGACodes.Codes.HC));
		}

		public void TestTreatmentCode()
		{
			Pivot.CCA_TreatmentCode = "3";
			Pivot.CCA_TreatmentCode = "10";
			AssertEquals("10", Pivot.CCA_TreatmentCode);
			AssertContainsLog(Pivot.Logs, "TT '3' changed to '10'", true);
			AssertContainsLog(Pivot.Part.Logs, "TT '3' changed to '10'", false);
		}

		void AssertContainsLog(Logs logs, string expected, bool result)
		{
			var hasLog = logs.GetAllLogs().ToList().Exists(o => expected == (o as StmALog).SL_Reference);
			AssertEquals(result, hasLog);
		}

		public void TestCCA_RN_NKOriginForExport()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert("CCA_ProvinceOfOrigin_ReadOnly", !Pivot.CCA_ProvinceOfOriginInfo.ReadOnly);
			Pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			Assert("CCA_ProvinceOfOrigin_ReadOnly", !Pivot.CCA_ProvinceOfOriginInfo.ReadOnly);
			Pivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			AssertEquals(CanadianProvinceList.Codes.Alberta, Pivot.CCA_ProvinceOfOrigin);
			Pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(CanadianProvinceList.Codes.Alberta, Pivot.CCA_ProvinceOfOrigin);
			Assert("CCA_ProvinceOfOrigin_ReadOnly", !Pivot.CCA_ProvinceOfOriginInfo.ReadOnly);
		}

		public void TestCCA_RN_NKOriginForImport()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("CCA_ProvinceOfOrigin_ReadOnly", Pivot.CCA_ProvinceOfOriginInfo.ReadOnly);
			Pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			Assert("CCA_ProvinceOfOrigin_ReadOnly", !Pivot.CCA_ProvinceOfOriginInfo.ReadOnly);
			Pivot.CCA_ProvinceOfOrigin = USStatesList.Codes.Alaska;
			AssertEquals(USStatesList.Codes.Alaska, Pivot.CCA_ProvinceOfOrigin);
			Pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(ZString.Empty, Pivot.CCA_ProvinceOfOrigin);
			Assert("CCA_ProvinceOfOrigin_ReadOnly", Pivot.CCA_ProvinceOfOriginInfo.ReadOnly);
		}

		public void TestCCA_RN_NKCFIAOrigin()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("CCA_CFIAUSStateOfOrigin_ReadOnly", Pivot.CCA_CFIAUSStateOfOriginInfo.ReadOnly);
			Pivot.CCA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			Assert("CCA_CFIAUSStateOfOrigin_ReadOnly", !Pivot.CCA_CFIAUSStateOfOriginInfo.ReadOnly);
			Pivot.CCA_CFIAUSStateOfOrigin = USStatesList.Codes.Alaska;
			AssertEquals(USStatesList.Codes.Alaska, Pivot.CCA_CFIAUSStateOfOrigin);
			Pivot.CCA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(ZString.Empty, Pivot.CCA_CFIAUSStateOfOrigin);
			Assert("CCA_CFIAUSStateOfOrigin_ReadOnly", Pivot.CCA_CFIAUSStateOfOriginInfo.ReadOnly);
		}

		public void TestCusCodeData()
		{
			AssertEquals("CFIARegistrationNumbers", typeof(CFIARegistrationNumberCollection), Pivot.CFIARegistrationNumbers.GetType());
		}

		public void TestIsHTI()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("Is HTI", Pivot.IsImportClassification);
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert("Is Not HTI", !Pivot.IsImportClassification);
		}

		public void TestChangeOfValueIsLoggedAndClearsAuditEntry()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_LastAuditedUser = "USR";
			pivot.CI_LastAuditedDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			AssertEquals("USR", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.BrettsBirthday, pivot.CI_LastAuditedDate);
			AssertEquals(false, pivot.Details.HasChanges);

			var timeOfChange = ZDateTime.UtcNow;

			pivot.CCA_AirsCode = "XXX";
			AssertEquals(true, pivot.Details.HasChanges);
			AssertEquals("", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.Empty, pivot.CI_LastAuditedDate);

			Factory.Save();
			var logEntry = pivot.Logs.MostRecentLog;
			AssertEquals(Events.EditedARecord, logEntry.Event);
			AssertLessThanOrEqualTo(timeOfChange, logEntry.SL_PostedTimeUtc);
		}

		public void TestCCA_OA_Manufacturer()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			org.Addresses.AddNew().AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			Pivot.CCA_OA_Manufacturer_ZAddress.OrgPK = org.PK;
			AssertEquals(org.MainAddress.PK, Pivot.CCA_OA_Manufacturer);
			AssertEquals(2, Pivot.CCA_OA_Manufacturer_ZAddress.OrgAddress_List.Count);
		}

		public void TestJI_BrandName()
		{
			Pivot.Part.OP_Brand = "TEST";
			AssertEquals("TEST", ((IHasPGARequirements)Pivot).JI_BrandName);
		}

		public void TestJI_Model()
		{
			Pivot.Part.OP_Model = "TEST";
			AssertEquals("TEST", ((IHasPGARequirements)Pivot).JI_Model);
		}

		public void TestIHasPGARequirementsTariffInfo()
		{
			AssertEquals(Pivot.CI_TariffNumInfo, ((IHasPGARequirements)Pivot).TariffInfo);
		}

		public void TestPGARecordsAreDeletedWithPivot()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_OP = part.PK;

			pivot.CCA_DFOIndicator = YesNoList.Codes.Yes;
			AssertEquals("(pre-condition) must have a non-null PGA header", false, pivot.DFOPGAHeader?.IsNull ?? true);
			pivot.DFOPGAHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pivot.DFOPGAHeader.CA_GenusOrSpecies = "TEST-XYZ";
			Factory.Save();

			var pivotPK = pivot.PK;
			var dfoHeaderPK = pivot.DFOPGAHeader.PK;

			var factory2 = NewFactory();
			AssertNotNull("(sanity check) pivot was saved", factory2.Load<CusClassPartPivot>(pivotPK));
			AssertEquals("(sanity check) PGA header was saved", "TEST-XYZ", factory2.Load<DFOPGAHeader>(dfoHeaderPK)?.CA_GenusOrSpecies);

			pivot.Delete();
			Factory.Save();

			var factory3 = NewFactory();
			AssertNull("(sanity check) pivot was deleted", factory3.Load<CusClassPartPivot>(pivotPK));
			AssertNull("PGA header must be deleted", factory3.Load<DFOPGAHeader>(dfoHeaderPK));
		}

		public void TestDefaultPGAIndicatorsWhenTariffChanges()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "3824700308", "TPR");
			AssertEquals(ZString.Empty, Pivot.CCA_TCIndicator);
			Pivot.CI_TariffNum = "3824700308";
			AssertEquals("Y", Pivot.CCA_TCIndicator);
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

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1234567890";
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(ZString.Empty, pivot.CCA_SIMADumpingNumber);
			AssertEquals(1, pivot.DutiesAndTaxes.Count);
			AssertEquals(DutyAndTaxTypes.Codes.SUR, pivot.DutiesAndTaxes[0].C1_TaxType);

			var dutyRateForCurrentCountry = pivot.DutyRateForCurrentCountry;
			AssertEquals(1, pivot.DutiesAndTaxes.Count);

			var taxRateForCurrentCountry = pivot.TaxRateForCurrentCountry;
			AssertEquals(1, pivot.DutiesAndTaxes.Count);

			pivot.DutiesAndTaxes.DeleteAll();
			pivot.OnRefreshSIMAMeasureEvent = null;
			pivot.CI_TariffNum = "0123456789";
			AssertEquals(ZString.Empty, pivot.CCA_SIMADumpingNumber);
			AssertEquals(0, pivot.DutiesAndTaxes.Count);

			pivot.OnRefreshSIMAMeasureEvent = delegate
			{
				return pivot.SIMAMeasures.OfType<SIMADumpingNumber>().FirstOrDefault(x => x.CA_DumpingNumber == "AD1408");
			};
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_TariffNum = "0123456789";
			AssertEquals("AD1408", pivot.CCA_SIMADumpingNumber);
			AssertEquals(1, pivot.DutiesAndTaxes.Count);
			AssertEquals(DutyAndTaxTypes.Codes.CVD, pivot.DutiesAndTaxes[0].C1_TaxType);

			pivot.DutiesAndTaxes.DeleteAll();
			pivot.OnRefreshSIMAMeasureEvent = null;
			pivot.CI_TariffNum = "0123456789";
			((ISupportDataImporting)pivot).IsImportingData = true;
			AssertEquals("AD1408", pivot.CCA_SIMADumpingNumber);
		}

		public void TestSIMADataWhenClassificationIsNotNull()
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
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, description: "SIMA Des", relatedTariffCode: "1234567890");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			classification.CC_TariffNum = "1234567890";
			AssertEquals("AD1407", classification.CCA_SIMADumpingNumber);
			AssertEquals(3, classification.DutiesAndTaxes.Count);
			foreach (var dutyAndTax in classification.DutiesAndTaxes)
			{
				dutyAndTax.C1_ExemptCode = SIMACodes.Codes.C31;
			}
			Factory.Save();

			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			AssertEquals(ZString.Empty, pivot.CCA_SIMADumpingNumber);
			AssertEquals(ZString.Empty, pivot.CCA_SIMADumpingDesc);
			AssertEquals(0, pivot.DutiesAndTaxes.Count);
			AssertEquals(0, pivot.DutiesAndTaxesForCC.Count);

			pivot.CI_CC = classification.PK;
			AssertEquals("SIMA Des", pivot.CCA_SIMADumpingDesc);
			AssertEquals("AD1407", pivot.CCA_SIMADumpingNumber);
			AssertEquals(0, pivot.DutiesAndTaxes.Count);
			AssertEquals(3, pivot.DutiesAndTaxesForCC.Count);
			var surTaxInInvoiceLine = pivot.DutiesAndTaxesForCC.Cast<DutyAndTax>().First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInInvoiceLine, SIMACodes.Codes.C31, false, 10m, "", 0m, "", 0m, "");
			var addTaxInvoiceLine = pivot.DutiesAndTaxesForCC.Cast<DutyAndTax>().First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInvoiceLine, SIMACodes.Codes.C31, false, 100.5m, "NMB", 0m, "", 0m, "");
			var cvdTaxInvoiceLine = pivot.DutiesAndTaxesForCC.Cast<DutyAndTax>().First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInvoiceLine, SIMACodes.Codes.C31, false, 200m, "KGM", 0m, "", 0m, "");
			Assert(pivot.DutiesAndTaxesForCC.ReadOnly);
			AssertEquals(classification.PK, pivot.DutiesAndTaxesForCC[0].B7_ParentID);

			pivot.CI_CC = ZGuid.Empty;
			AssertEquals(ZString.Empty, pivot.CCA_SIMADumpingNumber);
			AssertEquals(ZString.Empty, pivot.CCA_SIMADumpingDesc);
			AssertEquals(0, pivot.DutiesAndTaxes.Count);
			AssertEquals(0, pivot.DutiesAndTaxesForCC.Count);
		}

		void AssertDutyProperties(DutyAndTax dutyAndTax, ZString exemptCode, ZBool isOverride, ZDecimal rate, ZString uom, ZDecimal normalValue, ZString normalCurrency, ZDecimal foreignRate, ZString foreignCurrency)
		{
			AssertEquals(exemptCode, dutyAndTax.C1_ExemptCode);
			AssertEquals(isOverride, dutyAndTax.C1_Override);
			AssertEquals(rate, dutyAndTax.C1_Rate);
			AssertEquals(uom, dutyAndTax.C1_UnitOfMeasure);
			AssertEquals(normalValue, dutyAndTax.C1_NormalValuePerUnit);
			AssertEquals(normalCurrency, dutyAndTax.C1_NormalValueCurrency);
			AssertEquals(foreignRate, dutyAndTax.C1_ForeignRate);
			AssertEquals(foreignCurrency, dutyAndTax.C1_ForeignCurrency);
		}

		public void TestCusCAClassificationNotCreateWhenPivotIsDeleted()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			Factory.Save();
			_ = pivot.Details;
			pivot.Delete();
			_ = pivot.Details.CCA_99TariffCode;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestDefaultUniversalTariffProperties()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("Using Universal Tariff", false, typeof(CusClassPartPivot).GetProperty("UseUniversalTariff", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pivot));
		}

		public void TestNoExceptionWhenAccessJI_ModelWithOutPart()
		{
			var pivot = Factory.New<CusClassPartPivot>() as IHasPGARequirements;
			AssertNoExceptionThrown(() =>
			{
				var model = pivot.JI_Model;
			});
			AssertNoExceptionThrown(() =>
			{
				pivot.JI_Model = "TEST";
			});
		}

		public void TestNoExceptionWhenAccessJI_BrandNameWithOutPart()
		{
			var pivot = Factory.New<CusClassPartPivot>() as IHasPGARequirements;
			AssertNoExceptionThrown(() =>
			{
				var brand = pivot.JI_BrandName;
			});
			AssertNoExceptionThrown(() =>
			{
				pivot.JI_BrandName = "TEST";
			});
		}

		public void TestAMMVReadOnly()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<CusClassPartPivot>();
				AssertEquals(false, pivot.CCA_AMMVPercentageInfo.ReadOnly);
				AssertEquals(false, pivot.CCA_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, pivot.CCA_AMMVPerUnitCurrencyInfo.ReadOnly);
				pivot.CCA_AMMVPerUnit = 10m;
				AssertEquals(true, pivot.CCA_AMMVPercentageInfo.ReadOnly);
				pivot.CCA_AMMVPerUnit = 0m;
				AssertEquals(false, pivot.CCA_AMMVPercentageInfo.ReadOnly);
				pivot.CCA_AMMVPercentage = 10m;
				AssertEquals(true, pivot.CCA_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(true, pivot.CCA_AMMVPerUnitCurrencyInfo.ReadOnly);
				pivot.CCA_AMMVPercentage = 0m;
				AssertEquals(false, pivot.CCA_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, pivot.CCA_AMMVPerUnitCurrencyInfo.ReadOnly);
				pivot.Details.CCA_AMMVPercentage = 10m;
				pivot.Details.CCA_AMMVPerUnit = 10m;
				AssertEquals(false, pivot.CCA_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, pivot.CCA_AMMVPercentageInfo.ReadOnly);
				AssertEquals(false, pivot.CCA_AMMVPerUnitCurrencyInfo.ReadOnly);
			});
		}

		public void TestOnCCA_AMMVPercentageChangedCCA_AMMVPerUnitIsClearedOutAndRefresh()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.Details.CCA_AMMVPercentage = 10m;
				pivot.Details.CCA_AMMVPerUnit = 10m;
				var valueChangedCount = 0;
				(pivot as System.ComponentModel.IBindingList).ListChanged += (sender, e) => valueChangedCount++;
				pivot.CCA_AMMVPercentage = ZDecimal.Zero;
				AssertEquals(1, valueChangedCount);
				AssertEquals(10m, pivot.CCA_AMMVPerUnit);

				pivot.CCA_AMMVPercentage = 20m;
				AssertEquals(2, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CCA_AMMVPerUnit);

				pivot.CCA_AMMVPercentage = 20m;
				AssertEquals("Should not trigger change if value hasn't changed", 2, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CCA_AMMVPerUnit);

				pivot.CCA_AMMVPercentage = 30m;
				AssertEquals(3, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CCA_AMMVPerUnit);
			});
		}

		public void TestOnCCA_AMMVPercentageChangedCD_AMMVPerUnitCurrencyIsEmpty()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CCA_AMMVPerUnitCurrency = "USD";
			pivot.CCA_AMMVPerUnit = 5m;
			AssertEquals("USD", pivot.CCA_AMMVPerUnitCurrency);
			AssertEquals(5m, pivot.CCA_AMMVPerUnit);

			pivot.CCA_AMMVPercentage = 10m;
			AssertEquals(10m, pivot.CCA_AMMVPercentage);
			AssertEquals(0m, pivot.CCA_AMMVPerUnit);
			AssertEquals(ZString.Empty, pivot.CCA_AMMVPerUnitCurrency);
		}

		public void TestOnCCA_AMMVPerUnitChangedCCA_AMMVPercentageIsClearedOutAndRefresh()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.Details.CCA_AMMVPerUnit = 10m;
				pivot.Details.CCA_AMMVPercentage = 10m;
				var valueChangedCount = 0;
				(pivot as System.ComponentModel.IBindingList).ListChanged += (sender, e) => valueChangedCount++;
				pivot.CCA_AMMVPerUnit = ZDecimal.Zero;
				AssertEquals(1, valueChangedCount);
				AssertEquals(10m, pivot.CCA_AMMVPercentage);

				pivot.CCA_AMMVPerUnit = 20m;
				AssertEquals(2, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CCA_AMMVPercentage);

				pivot.CCA_AMMVPerUnit = 20m;
				AssertEquals("Should not trigger change if value hasn't changed", 2, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CCA_AMMVPercentage);

				pivot.CCA_AMMVPerUnit = 30m;
				AssertEquals(3, valueChangedCount);
				AssertEquals(ZDecimal.Zero, pivot.CCA_AMMVPercentage);
			});
		}
		#region Implementation
		protected virtual Type GetExpectedBusinessObjectType()
		{
			return typeof(CusClassPartPivot);
		}

		CusClassPartPivot Pivot
		{
			get
			{
				if (pivotCache != null)
				{
					return pivotCache;
				}
				OrgSupplierPart part = Factory.New<OrgSupplierPart>();
				pivotCache = Factory.New<CusClassPartPivot>();
				pivotCache.CI_OP = part.PK;
				return pivotCache;
			}
		}
		CusClassPartPivot pivotCache;
		#endregion
	}
}
