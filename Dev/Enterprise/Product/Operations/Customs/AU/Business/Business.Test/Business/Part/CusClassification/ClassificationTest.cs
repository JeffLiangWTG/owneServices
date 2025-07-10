using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(Classification))]
	sealed class ClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDutyRateForCurrentCountry()
		{
			DutyRateHelper.CreateTestTariffRatePeriodSnapshot(Factory);
			Factory.Save();

			Classification classification = Factory.New<Classification>();
			classification.CC_ClassificationType = Classification.ClassificationType.EXP;
			classification.CC_TariffNum = "4201.00.00 01";
			AssertEquals("classification.DutyRateForCurrentCountry", "", classification.DutyRateForCurrentCountry);

			classification.CC_ClassificationType = Classification.ClassificationType.IMP;
			AssertEquals("classification.DutyRateForCurrentCountry", "5.00000", classification.DutyRateForCurrentCountry);

			classification.CC_TariffNum = "0701.10.00 01";
			AssertEquals("classification.DutyRateForCurrentCountry", "0.00000", classification.DutyRateForCurrentCountry);

			classification.CC_TariffNum = "";
			AssertEquals("classification.DutyRateForCurrentCountry", "", classification.DutyRateForCurrentCountry);
		}

		public void TestGenerateAndRefreshQuestions()
		{
			ZDateTime dutyDate = new ZDateTime(2005, 1, 1);

			CMRCommunityProtectionProfile profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;

			CMRCommunityProtectionRisk risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;

			CMRLodgementQuestion question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			CMRCommunityProtectionProfile profile2 = CMRCommunityProtectionProfile.New(Factory);
			profile2.CP_TariffClassificationNumberfield = "00000000";
			profile2.CP_CommunityProtectionRiskIdentifier = 402;

			CMRCommunityProtectionRisk risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 402;
			risk2.CK_StartDate = dutyDate;
			risk2.CK_LodgementQuestionIdentifier = 5001;

			CMRLodgementQuestion question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 5001;
			question2.CQ_LodgementQuestionStartDate = dutyDate;

			Classification classification = Factory.New<Classification>();
			classification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			classification.CC_LookupCode = "LookupCode";
			classification.CC_TariffNum = "0000.00.00";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals("pre-cpndition", 0, classification.Questions.Count);
			classification.GenerateQuestion();
			AssertEquals("2 questions generated", 2, classification.Questions.Count);
			classification.Questions[1].Delete();
			classification.Questions[0].ON_AnswerCode = "Y";
			AssertEquals("pre-cpndition", 1, classification.Questions.Count);
			classification.GenerateQuestion();
			AssertEquals("Subsequent generate should not change anything", 1, classification.Questions.Count);
			AssertEquals("Subsequent generate should not change anything", "Y", classification.Questions[0].ON_AnswerCode);
			classification.RefreshQuestions();
			AssertEquals("Refresh should re-generate", 2, classification.Questions.Count);
			AssertEquals("Refresh now maintains existing valid questions", "Y", classification.Questions[0].ON_AnswerCode);
			AssertEquals("Refresh should re-generate", "", classification.Questions[1].ON_AnswerCode);
		}

		public void TestIsRiskCalculatedFromTariff()
		{
			Classification classification = Factory.New<Classification>();
			AssertEquals("IsRiskCalculatedFromTariff", true, ((ICPQALineAttachee)classification).IsRiskCalculatedFromTariff);
		}

		public void TestIsRiskHistorySupported()
		{
			Classification classification = Factory.New<Classification>();
			AssertEquals("IsRiskHistorySupported", true, ((ICPQALineAttachee)classification).IsRiskHistorySupported);
		}

		public void TestICPQAAttacheeHolder()
		{
			Classification classification = Factory.New<Classification>();
			AssertEquals("Headers", 0, ((ICPQAAttacheeHolder)classification).Headers.Length);
			AssertEquals("Lines.Length", 1, ((ICPQAAttacheeHolder)classification).Lines.Length);
			AssertEquals("Lines", classification, ((ICPQAAttacheeHolder)classification).Lines[0]);
			AssertNull("CachedQuestions", ((ICPQAAttacheeHolder)classification).CachedQuestions);
		}

		public void TestICPQALineAttachee()
		{
			Classification classification = Factory.New<Classification>();
			classification.CC_TariffNum = "0000.00.00 00";
			classification.AddInfo.ZA_ORG = "NZ";

			CPQuestionKeys key = ((ICPQALineAttachee)classification).CPQuestionKey;
			AssertEquals("Key.TariffNumber", "00000000", key.TariffNumber);
			AssertEquals("Key.StatCode", "00", key.StatCode);
			AssertEquals("Key.OriginCode", "NZ", key.OriginCode);
			AssertEquals("SourcesToDefault", 0, ((ICPQALineAttachee)classification).SourcesToDefault.Length);
			AssertNotNull("DefaultUniqueQuestions", ((ICPQALineAttachee)classification).DefaultUniqueQuestions);
			AssertEquals("Key.HasValidNatureOrModeOfTransport", false, key.HasValidOriginOrNatureOrModeOfTransport);
			AssertEquals("TableCode", "CC", ((ICPQALineAttachee)classification).TableCode);
		}

		public void TestICPQAAttachee()
		{
			Classification classification = Factory.New<Classification>();
			AssertEquals("ICPQAAttachee.FKColumnInCusEntryCPDecTable", CusEntryCPDecSchema.ON_ParentID, ((ICPQAAttachee)classification).FKColumnInCusEntryCPDecTable);
			AssertNotNull("ICPWAAttachee.QUestions", classification.Questions);
			AssertEquals("Questions is registered", true, classification.IsRegisteredEditableChildObject(classification.Questions));
			AssertEquals("Selection date", ZDateTime.Today, ((ICPQAAttachee)classification).SelectionDate);
		}

		public void TestValidationClassAttached()
		{
			Classification classification = Factory.New<Classification>();
			AssertEquals(typeof(CusClassificationValidation), classification.Validation.GetType());
		}

		public void TestMandatoryField()
		{
			@class.CC_Description = "";
			@class.Validation.ValidateCC_Description();
			AssertHasErrorContaining(@class.CC_DescriptionInfo, MandatoryValidation.MustBeEntered);

			@class.CC_Description = "DESCRIPTION";
			AssertNoErrorContaining(@class.CC_DescriptionInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestDescriptionFromTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "34060000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
				, description: "CANDLES, TAPERS AND THE LIKE, WHETHER OR NOT COLOURED, PERFUMED OR DECORATED"
				, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				@class.CC_Description = "";
				@class.CC_TariffNum = "3406.00.00";
				AssertEquals("CANDLES, TAPERS AND THE LIKE, WHETHER OR NOT COLOURED, PERFUMED OR DECORATED", @class.CC_Description);
			}
		}

		public void TestDescriptionFromTariffDescription_AHECC()
		{
			var ahecc = Factory.New<AUCAHECC>();
			ahecc.UA_AHECC = "3406.00.00";
			ahecc.UA_LongDescription = "CANDLES, TAPERS AND THE LIKE, WHETHER OR NOT COLOURED, PERFUMED OR DECORATED";

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				@class.CC_Description = "";
				@class.CC_TariffNum = "3406.00.00";
				AssertEquals("CANDLES, TAPERS AND THE LIKE, WHETHER OR NOT COLOURED, PERFUMED OR DECORATED", @class.CC_Description);
			}
		}

		public void TestEmptyLookupCodeError()
		{
			@class.CC_ClassificationType = Classification.ClassificationType.EXP;
			@class.CC_LookupCode = ZString.Empty;
			Assert("Empty look up code is an error", @class.CC_LookupCodeInfo.HasErrors());

			@class.CC_LookupCode = "XXX";
			Assert("No more error", !@class.CC_LookupCodeInfo.HasErrors());
		}

		public void TestDuplicateLookupCodeAndClassTypeError()
		{
			@class.CC_ClassificationType = Classification.ClassificationType.EXP;
			@class.CC_LookupCode = "KKK";
			@class.CC_Description = "TEST";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Classification newClass = newFactory.New<Classification>();
			newClass.CC_ClassificationType = Classification.ClassificationType.EXP;
			newClass.CC_LookupCode = "KKK";
			Assert("Look up code is duplicate", newClass.CC_LookupCodeInfo.HasErrors());

			newClass.CC_LookupCode = "RRR";
			Assert("No more duplicate", !newClass.CC_LookupCodeInfo.HasErrors());
		}

		public void TestValidateTariffNumExist()
		{
			@class.CC_TariffNum = "Invalid";
			AssertHasMessageError("Doesn't exist", @class.CC_TariffNumInfo, "This tariff number does not exist.");
		}

		public void TestClassificationAddInfo()
		{
			var importTariff = Factory.LoadTop1<AUCClass>(new ZQuery());
			Classification importClass = Factory.New<Classification>();
			importClass.CC_AddInfo = "ORG=USA";
			importClass.CC_TariffNum = importTariff.UJ_Code;

			AssertEquals("AddInfo in Classification", "ORG=USA", importClass.AddInfo.AddInfoLine);
		}

		public void TestInstrumentTypeOnClassification()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.InstrumentType = "MD1";
			AssertEquals("InstrumentType", "MD1", importClass.InstrumentType);

			importClass.InstrumentType = "BL";
			AssertEquals("InstrumentType", "BL", importClass.InstrumentType);

			importClass.InstrumentType = "";
			AssertEquals("InstrumentType", "", importClass.InstrumentType);
		}

		public void TestValidateInstrumentType()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.InstrumentType = "TC";
			Assert("This is a valid code", !importClass.InstrumentTypeInfo.HasNotifications());

			importClass.InstrumentType = "XX";
			Assert("Invalid Code", importClass.InstrumentTypeInfo.HasNotifications());
		}

		public void TestInstrumentCodeOnClassification()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.InstrumentCode = "7256555";
			AssertEquals("Instrument Code", "7256555", importClass.InstrumentCode);

			importClass.InstrumentCode = "";
			AssertEquals("Instrument Code", "", importClass.InstrumentCode);
		}

		public void TestEffectiveAddInfo()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.TreatmentCode = "ABC";
			AssertEquals(importClass.AddInfo, importClass.EffectiveAddInfo);
			AssertEquals(importClass.AddInfo.ZA_TreatmentCode_Hidden, importClass.EffectiveAddInfo.ZA_TreatmentCode_Hidden);
			AssertEquals("Treatment Code", "ABC", importClass.EffectiveAddInfo.ZA_TreatmentCode_Hidden);
		}

		public void TestTreatmentCodeOnClassification()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.TreatmentCode = "117";
			AssertEquals("Treatment Code", "117", importClass.TreatmentCode);

			importClass.TreatmentCode = "ABC";
			AssertEquals("Treatment Code", "ABC", importClass.TreatmentCode);

			importClass.TreatmentCode = "";
			AssertEquals("Treatment Code", "", importClass.TreatmentCode);
		}

		public void TestValidateTreatmentCode()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClass.AddInfo.ZA_ORG = "TH"; //Preference : S
			importClass.TreatmentCode = "507";//valid
			Assert("This is valid for this country", !importClass.TreatmentCodeInfo.HasNotifications());

			importClass.TreatmentCode = "520"; //invalid for this country
			Assert("This is invalid for this country", importClass.TreatmentCodeInfo.HasNotifications());
		}

		public void TestSetSecondUQFromTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203003115", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				@class.CC_ClassificationType = Classification.ClassificationType.IMP;
				@class.CC_TariffNum = "2203.00.31 15";
				AssertEquals("Second UQ", "L", @class.AddInfo.ZA_UQ2);
			}
		}

		public void TestSetSecondUQFromTariff_AUCClass()
		{
			var importTariff = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2203.00.31 15");
			if (importTariff == null)
			{
				importTariff = Factory.New<AUCClass>();
				importTariff.UJ_Code = "2203.00.31 15";
				importTariff.UJ_UQ1 = "LA";
				importTariff.UJ_UQ2 = "L";
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				@class.CC_ClassificationType = Classification.ClassificationType.IMP;
				@class.CC_TariffNum = "2203.00.31 15";
				AssertEquals("Second UQ", "L", @class.AddInfo.ZA_UQ2);
			}
		}

		public void TestDocManagerCodeForImport()
		{
			Classification importClass = Factory.New<Classification>();
			importClass.CC_ClassificationType = Classification.ClassificationType.IMP;
			AssertEquals("Code should be IMC. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "IMC", ((IDocManagerSupport)importClass).DocManagerInfo.DocManagerCode);
		}

		public void TestDocManagerCodeForExport()
		{
			AssertEquals("Code should be EXC. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "EXC", ((IDocManagerSupport)@class).DocManagerInfo.DocManagerCode);
		}

		public void TestDutyPercentage()
		{
			CMRTariffRatePeriodSnapshot result = Factory.New<CMRTariffRatePeriodSnapshot>();
			result.TT_TariffClassificationNumber = "00000000";
			result.TT_PreferenceSchemeType = "XX";
			result.TT_StartDate = ZDateTime.Today.AddDays(-1);
			result.TT_EndDate = ZDateTime.Today;
			result.TT_RateNumber = "001";
			result.TT_CustomsValueRate = 1m;//100 Duty
			result.TT_CalculationType = Constants.DutyCalcTypes.Calc;

			CMRTreatmentRatePeriodSnapshot treatment = Factory.New<CMRTreatmentRatePeriodSnapshot>();
			treatment.TP_Code = "000";
			treatment.TP_PreferenceSchemeType = "XX";
			treatment.TP_StartDate = ZDateTime.Today.AddDays(-1);
			treatment.TP_EndDate = ZDateTime.Today;
			treatment.TP_RateNumber = "001";
			treatment.TP_CustomsValueRate = 0.5m;
			treatment.TP_CalculationType = Constants.DutyCalcTypes.Calc;

			Factory.Save();

			Classification importClassification = Factory.New<Classification>();
			importClassification.CC_TariffNum = "00000000";
			importClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClassification.AddInfo.ZA_ORG = "US";
			importClassification.AddInfo.ZA_PST = "XX";
			importClassification.AddInfo.ZA_RNO = "001";
			importClassification.AddInfo.ZA_TreatmentCode_Hidden = "000";

			AssertEquals("DutyRate", 0.5m, importClassification.ImportDutyPercentage);
		}

		public void TestNew()
		{
			AssertNotNull(Classification.New(Factory));
		}

		public void TestCPQuestionsDelete()
		{
			var tariff = Factory.New<Classification>();
			Assert("Pre-condition", !tariff.IsImport);

			var decQuestion1 = tariff.Questions.AddNew();
			var decQuestion2 = tariff.Questions.AddNew();
			Assert(!decQuestion1.IsDeleted);
			Assert(!decQuestion2.IsDeleted);

			tariff.Delete();
			Assert("Pre-condition", tariff.IsDeleted);
			Assert(!decQuestion1.IsDeleted);
			Assert(!decQuestion2.IsDeleted);

			var tariff2 = Factory.New<Classification>();
			tariff2.CC_ClassificationType = Classification.ClassificationType.IMP;
			Assert("Pre-condition", tariff2.IsImport);

			var decQuestion3 = tariff2.Questions.AddNew();
			var decQuestion4 = tariff2.Questions.AddNew();
			Assert(!decQuestion3.IsDeleted);
			Assert(!decQuestion4.IsDeleted);

			tariff2.Delete();
			Assert("Pre-condition", tariff2.IsDeleted);
			Assert(decQuestion3.IsDeleted);
			Assert(decQuestion4.IsDeleted);
		}

		#region Implementation
		readonly string lookupCode = "LookupCode";
		readonly string tariff = "0000.00.00";
		Classification @class;

		protected override void SetUp()
		{
			base.SetUp();

			@class = Factory.New<Classification>();
			@class.CC_ClassificationType = JobDeclaration.ClassificationType.EXP;
			@class.CC_LookupCode = lookupCode;
			@class.CC_TariffNum = tariff;
			@class.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();
		}

		#endregion
	}
}
