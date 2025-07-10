using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	public static class GuidedDecisionMakingTestHelper
	{
		public static GuidedDecisionMakingBasic CreateGuidedDecisionMakingBasicForTest(BusinessObjectFactory factory, bool isImport, bool isExport)
		{
			var gDMBasicSource = new Mock<IGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.DataGrouping).Returns("DG1");
			gDMBasicSource.Setup(x => x.ParentDataGrouping).Returns("PDG");
			gDMBasicSource.Setup(x => x.UserLanguage).Returns("EN");
			gDMBasicSource.Setup(x => x.DutyRateTypeCode).Returns("OTH");
			gDMBasicSource.Setup(x => x.AllowQuickAdditionalCodeScreen).Returns(true);
			gDMBasicSource.Setup(x => x.AllowQuickConditionScreen).Returns(true);
			gDMBasicSource.Setup(x => x.EffectiveDate).Returns(ZDate.BrettsBirthday);
			gDMBasicSource.Setup(x => x.TariffCode).Returns("1111111111");
			gDMBasicSource.Setup(x => x.CountryCode).Returns("FR");
			gDMBasicSource.Setup(x => x.CountryOfOrigin).Returns("FR");
			gDMBasicSource.Setup(x => x.Preference).Returns("P1");
			gDMBasicSource.Setup(x => x.QuotaOrderNumber).Returns("Number1");
			gDMBasicSource.Setup(x => x.CustomsFirstQuantity).Returns(100m);
			gDMBasicSource.Setup(x => x.CustomsSecondQuantity).Returns(200m);
			gDMBasicSource.Setup(x => x.CustomsSecondUnitQty).Returns("LPA");
			gDMBasicSource.Setup(x => x.CustomsThirdQuantity).Returns(300m);
			gDMBasicSource.Setup(x => x.CustomsThirdUnitQty).Returns("HLT");
			gDMBasicSource.Setup(x => x.TariffType).Returns("DEF");
			gDMBasicSource.Setup(x => x.IsImport).Returns(isImport);
			gDMBasicSource.Setup(x => x.IsExport).Returns(isExport);
			gDMBasicSource.Setup(x => x.SupplementaryCodes).Returns(new List<ZString>() { "ADD1" });
			gDMBasicSource.Setup(x => x.SupportingAndAdditionalDocuments).Returns(Enumerable.Empty<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>());

			return new GuidedDecisionMakingBasic(gDMBasicSource.Object, factory);
		}

		public static void SetupConditons(BusinessObjectFactory factory)
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var rateType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "RAT1", "Test Rate Condition Type");
			var vatType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.VAT, "VAT1", "Test Vat Condition Type");
			var ctrlType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR1", "Test Ctrl Condition Type");
			var classType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, "CAS2", "Test Class Condition Type");
			var preference = helper.CreatePreferenceForCountry("P1", "TestPreference", dataGrouping);
			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, "RC1", dutyRateType.PK);

			var condition1ValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1ValueType2 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "SupportingDocumentNoReferenceNumber");
			var condition1ValueType3 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.PresentationOfSupportingDoc, "PresentationOfSupportingDoc");
			var condition1ValueType4 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.Formula, "Formula,", true);
			var condition1ValueType5 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Enterprise.Customs.Universal.Constants.ConditionValueType.Information, "Information");

			var condition1 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, rateType.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition1.PK, "R111");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition1.PK, "C111");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType3.PK, condition1.PK, "P111");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition1.PK, "R12");

			var condition2 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C2", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition2.PK, "R222");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType4.PK, condition2.PK, "[KGM] <= 10.000");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition2.PK, "R12");

			var condition3 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, classType.PK, tariff.PK, "C3", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition3.PK, "C333");

			var condition4 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C4", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition4.PK, "C444");

			var condition5 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C5", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition5.PK, "C555");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType5.PK, condition5.PK, "INF1");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType5.PK, condition5.PK, "INF2");

			var condition6 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C6", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType3.PK, condition6.PK, "C666");

			var condition7 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, vatType.PK, tariff.PK, "C7", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition7.PK, "C777");

			helper.CreateCusApplicability(condition4, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add1", "");

			helper.CreateCusApplicability(condition7, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add2", "");
			factory.Save();

			var testRate1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", preferencePk: preference.PK);
			helper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "add1", "");

			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "C111", "C111 desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "R111", "R111 desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "C777", "C777 desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}
	}
}
