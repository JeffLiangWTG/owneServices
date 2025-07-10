using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			CusClassPartPivot parent = Factory.New<CusClassPartPivot>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckOverlaps()
		{
			var part = Factory.New<OrgSupplierPart>();
			var relOrg1 = part.RelatedOrganisations.AddNew();

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = "HTI";
			pivot1.CI_TariffNum = "1234567890";
			pivot1.CI_DateStart = ZDateTime.Now;
			pivot1.CI_DateEnd = ZDateTime.Now.AddDays(10);
			pivot1.CI_OH = relOrg1.OU_OH;

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";
			pivot2.CI_TariffNum = "1234567890";
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(20);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(30);
			pivot2.CI_OH = relOrg1.OU_OH;
			AssertNoErrorContaining(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);

			pivot2.CI_DateStart = ZDateTime.Now.AddDays(5);
			pivot2.CI_DateEnd = ZDateTime.Now.AddDays(18);
			pivot2.CI_TariffNum = "1234567899";
			pivot2.CI_ChildType = "HTI";
			pivot1.CI_TariffNum = "1234567899";
			pivot2.Validation.ValidateAll();
			AssertNoErrorContaining(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = "HTI";
			pivot3.CI_TariffNum = "1234567890";
			pivot3.CI_DateStart = ZDateTime.Now.AddDays(5);
			pivot3.CI_DateEnd = ZDateTime.Now.AddDays(18);
			pivot3.CI_TariffNum = "1234567899";
			pivot3.CI_ChildType = "HTI";
			pivot3.Validation.ValidateAll();
			AssertHasErrorContaining(pivot3.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
		}

		public void TestAttributesRequirement()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DZ1234ZD";
			var part = Factory.New<OrgSupplierPart>();
			var partRelate = part.RelatedOrganisations.AddOwner(org);
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.Validation.ValidateAll();
			string message = "must be specified as it's specified on another";
			AssertNoRowErrorContaining(pivot2, message);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.Validation.ValidateAll();
			AssertNoRowErrorContaining(pivot2, message);

			pivot1.Attributes1.AddNew();
			pivot2.Validation.ValidateAll();
			string attrib1Message = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 1", ClassificationTypeList.Codes.HTI, "NONE");
			AssertContains(message, attrib1Message);
			string attrib2Message = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 2", ClassificationTypeList.Codes.HTI, "NONE");
			AssertContains(message, attrib2Message);
			string attrib3Message = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 3", ClassificationTypeList.Codes.HTI, "NONE");
			string attrib1MessageWitRelated = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 1", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib1MessageWitRelated);
			string attrib2MessageWitRelated = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 2", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib2MessageWitRelated);
			string attrib3MessageWitRelated = CusClassPartPivotValidation.GetAttributeIsRequiredMessage("Attribute 3", ClassificationTypeList.Codes.HTI, "DZ1234ZD");
			AssertContains(message, attrib3MessageWitRelated);
			AssertHasRowErrorContaining(pivot2, message);
			AssertHasRowError(pivot2, attrib1Message);
			AssertNoRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertNoRowError(pivot2, attrib1MessageWitRelated);
			AssertNoRowError(pivot2, attrib2MessageWitRelated);
			AssertNoRowError(pivot2, attrib3MessageWitRelated);

			pivot1.Attributes2.AddNew();
			pivot2.Validation.ValidateAll();
			AssertHasRowError(pivot2, attrib1Message);
			AssertHasRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertNoRowError(pivot2, attrib1MessageWitRelated);
			AssertNoRowError(pivot2, attrib2MessageWitRelated);
			AssertNoRowError(pivot2, attrib3MessageWitRelated);

			pivot1.Attributes3.AddNew();
			pivot2.Validation.ValidateAll();
			AssertHasRowError(pivot2, attrib1Message);
			AssertHasRowError(pivot2, attrib2Message);
			AssertHasRowError(pivot2, attrib3Message);
			AssertNoRowError(pivot2, attrib1MessageWitRelated);
			AssertNoRowError(pivot2, attrib2MessageWitRelated);
			AssertNoRowError(pivot2, attrib3MessageWitRelated);

			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Validation.ValidateAll();
			AssertNoRowErrorContaining(pivot2, message);

			pivot1.CI_OH = partRelate.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.Validation.ValidateAll();
			AssertNoRowError(pivot2, attrib1Message);
			AssertNoRowError(pivot2, attrib2Message);
			AssertNoRowError(pivot2, attrib3Message);
			AssertHasRowError(pivot2, attrib1MessageWitRelated);
			AssertHasRowError(pivot2, attrib2MessageWitRelated);
			AssertHasRowError(pivot2, attrib3MessageWitRelated);
		}

		delegate CusAttributeFilterCollection GetAttributesDelegate(CusClassPartPivot pivot);

		public void TestCheckCI_ChildType()
		{
			AssertChildType(x => x.Attributes1);
			AssertChildType(x => x.Attributes2);
			AssertChildType(x => x.Attributes3);
		}

		void AssertChildType(GetAttributesDelegate getAttributes)
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			CusClassPartPivot pivot1 = part.PivotsForBinding.AddNew();
			var relOrg1 = part.RelatedOrganisations.AddNew();
			var relOrg2 = part.RelatedOrganisations.AddNew();
			pivot1.CI_ChildType = "XXX";
			AssertHasErrorContaining(pivot1.CI_ChildTypeInfo, ListValidation.InvalidCodeError);

			pivot1.CI_ChildType = ZString.Empty;
			AssertHasError(pivot1.CI_ChildTypeInfo, CusClassPartPivotValidation.ClassificationTypeIsMandatory);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot1.CI_ChildTypeInfo, CusClassPartPivotValidation.ClassificationTypeIsMandatory);
			AssertNoErrorContaining(pivot1.CI_ChildTypeInfo, ListValidation.InvalidCodeError);

			CusClassPartPivot pivot2 = part.PivotsForBinding.AddNew();
			pivot1.CI_OH = relOrg1.OU_OH;
			pivot2.CI_OH = relOrg1.OU_OH;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = relOrg2.OU_OH;
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot2.CI_OH = relOrg1.OU_OH;
			getAttributes(pivot2).AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertHasError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = relOrg1.OU_OH;
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot2.Validation.DuplicateAttributeForHTIWithoutAttributes);
			AssertNoError(pivot2.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForNoneHTI);
		}

		public void TestDuplicateHTIWithAttributes()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "IMP1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "IMP2";
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			var relOrg1 = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);
			var relOrg2 = part.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, Enterprise.MasterFiles.Business.OrgPartRelation.RelationshipTypes.Owner);

			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			var attrib1 = pivot1.Attributes1.AddNew();
			var attrib2 = pivot2.Attributes1.AddNew();
			string attribute1Name = nameof(CusAttributeFilter.AttributeFilterName.AT1);
			string attribute2Name = nameof(CusAttributeFilter.AttributeFilterName.AT2);
			string attribute3Name = nameof(CusAttributeFilter.AttributeFilterName.AT3);
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "NO VALUE", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute2Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "A", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute3Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "NO VALUE", "A", "IMP1"));

			var attrib3 = pivot1.Attributes2.AddNew();
			var attrib4 = pivot2.Attributes2.AddNew();
			attrib3.BG_AttributeValue1 = "C";
			attrib4.BG_AttributeValue1 = "C";
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "C", "NO VALUE", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute3Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "C", "A", "IMP1"));

			attrib3.BG_AttributeName = attribute3Name;
			attrib4.BG_AttributeName = attribute3Name;
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "NO VALUE", "C", "IMP1"));
			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute2Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("NO VALUE", "A", "C", "IMP1"));

			var attrib5 = pivot1.Attributes3.AddNew();
			var attrib6 = pivot2.Attributes3.AddNew();
			attrib5.BG_AttributeValue1 = "D";
			attrib6.BG_AttributeValue1 = "D";
			attrib5.BG_AttributeName = attribute2Name;
			attrib6.BG_AttributeName = attribute2Name;

			AssertDuplicateHTIWithAttributes(relOrg1.OU_OH, relOrg2.OU_OH, attribute1Name, pivot1, pivot2, attrib1, attrib2, CusClassPartPivotValidation.DuplicateAttributeForHTIWithAttributes("A", "D", "C", "IMP1"));
		}

		void AssertDuplicateHTIWithAttributes(ZGuid relOrg1PK, ZGuid relOrg2PK, ZString attributeName, CusClassPartPivot pivot1, CusClassPartPivot pivot2, CusAttributeFilter attrib1, CusAttributeFilter attrib2, string messageError)
		{
			pivot1.CI_OH = relOrg1PK;
			pivot2.CI_OH = relOrg1PK;
			attrib1.BG_AttributeName = attributeName;
			attrib2.BG_AttributeName = attributeName;
			attrib1.BG_AttributeValue1 = "A";
			attrib2.BG_AttributeValue1 = "A";
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, messageError);

			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, messageError);

			attrib1.BG_AttributeValue1 = "B";
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, messageError);

			attrib1.BG_AttributeValue1 = "A";
			pivot1.CI_OH = relOrg2PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertNoError(pivot2.CI_ChildTypeInfo, messageError);

			pivot1.CI_OH = relOrg1PK;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot2.CI_ChildTypeInfo, messageError);
		}

		public void TestCheckSIMADutiesRequired()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
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
			pivot.DutiesAndTaxes.DeleteAll();
			pivot.Validation.ValidateCI_TariffNum();
			AssertHasWarningContaining(pivot.CI_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");

			var tax = pivot.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			pivot.Validation.ValidateCI_TariffNum();
			AssertNoWarningContaining(pivot.CI_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");

			pivot.DutiesAndTaxes.DeleteAll();
			pivot.OnRefreshSIMAMeasureEvent = null;
			pivot.CI_TariffNum = "0123456789";
			pivot.DutiesAndTaxes.DeleteAll();
			pivot.Validation.ValidateCI_TariffNum();
			AssertNoWarningContaining(pivot.CI_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");

			pivot.OnRefreshSIMAMeasureEvent = delegate
			{
				return pivot.SIMAMeasures.OfType<SIMADumpingNumber>().FirstOrDefault(x => x.CA_DumpingNumber == "AD1408");
			};
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_TariffNum = "0123456789";
			pivot.DutiesAndTaxes.DeleteAll();
			pivot.Validation.ValidateCI_TariffNum();
			AssertHasWarningContaining(pivot.CI_TariffNumInfo, "Classification Requires SIMA – please add SIMA Code under the SIMA tab > Duties and Taxes");
		}

		public void TestCheckCCA_SIMADumpingDesc()
		{
			#region Universal Tariff Setup

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
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

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1234567890";
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertNoMessageError(pivot.CCA_SIMADumpingDescInfo, CusClassPartPivotValidation.SimaMeasureIsMissingMessage);
			pivot.CCA_SIMADumpingNumInfo.ClearValue();
			pivot.Validation.ValidateCCA_SIMADumpingDesc();
			AssertHasMessageError(pivot.CCA_SIMADumpingDescInfo, CusClassPartPivotValidation.SimaMeasureIsMissingMessage);
		}

		#region TestCheckCI_CC

		public void TestCheckCI_CC()
		{
			CusClassPartPivot pivot = Part.PivotsForBinding.AddNew();
			pivot.Validation.ValidateCI_CC();
			AssertHasError(pivot.CI_CCInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(pivot.CI_CCInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = ZGuid.Empty;
			AssertHasError(pivot.CI_CCInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_TariffNum = "00000000";
			AssertNoError(pivot.CI_CCInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
		}

		#endregion

		#region TestCheckCI_TariffNum

		public void TestCheckCI_TariffNum()
		{
			var cacClassHeader = Factory.New<CACClassHeader>();
			cacClassHeader.ZA_AreaCode = "AAA";
			cacClassHeader.ZA_ClassificationNumber = "1234567890";
			cacClassHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			cacClassHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);

			var pivot = Part.PivotsForBinding.AddNew();
			pivot.Validation.ValidateCI_TariffNum();
			AssertHasError(pivot.CI_TariffNumInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = Factory.New<CusClassification>().PK;
			AssertNoError(pivot.CI_TariffNumInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = ZGuid.Empty;
			AssertHasError(pivot.CI_TariffNumInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_TariffNum = "00000000";
			AssertNoError(pivot.CI_TariffNumInfo, CusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			AssertHasMessageErrors(pivot.CI_TariffNumInfo);
			pivot.CI_TariffNum = "1234567890";
			AssertNoMessageErrors(pivot.CI_TariffNumInfo);
			pivot.CI_TariffNum = "1234567800";
			AssertHasMessageErrorContaining(pivot.CI_TariffNumInfo, ZString.Format("Classification was not found or is not valid for {0}.", ZDateTime.Today.ToShortDateString()));
		}

		#endregion

		#region TestCheckDateStart

		public void TestCheckDateStart()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1200000";

			AssertNoErrorContaining(pivot.CI_DateStartInfo, CusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(10);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(20);
			AssertHasErrorContaining(pivot.CI_DateStartInfo, CusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(5);
			AssertNoErrorContaining(pivot.CI_DateStartInfo, CusClassPartPivotValidation.StartDateMustBeforeEndDate);
		}

		#endregion

		#region TestCheckDateEnd

		public void TestCheckDateEnd()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1200000";

			AssertNoErrorContaining(pivot.CI_DateEndInfo, CusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateStart = ZDateTime.Now.AddDays(20);
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(10);
			AssertHasErrorContaining(pivot.CI_DateEndInfo, CusClassPartPivotValidation.StartDateMustBeforeEndDate);
			pivot.CI_DateEnd = ZDateTime.Now.AddDays(30);
			AssertNoErrorContaining(pivot.CI_DateEndInfo, CusClassPartPivotValidation.StartDateMustBeforeEndDate);
		}

		#endregion

		#region Implementation
		OrgSupplierPart Part
		{
			get { return part ?? (part = Factory.New<OrgSupplierPart>()); }
		}
		OrgSupplierPart part;
		#endregion
	}
}
