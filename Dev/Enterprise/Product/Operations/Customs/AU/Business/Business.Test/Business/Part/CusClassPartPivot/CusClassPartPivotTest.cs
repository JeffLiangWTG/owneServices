using System;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{ //new registry
		#region AQIS Documents

		public void TestAQISDocuments()
		{
			AssertEquals("No values in collection", 0, Pivot.AQISDocuments.Count);

			AQISDocument document = Pivot.AQISDocuments.AddNew();
			document.Number = "1234";
			document.Type = "Type";
			AssertEquals("One value in the collection", 1, Pivot.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Type/1234", Pivot.AddInfo.ZA_AQISDocuments_Hidden);
		}

		public void TestAQISDocumentsWithOneValueInAddInfo()
		{
			AQISDocument document1 = Pivot.AQISDocuments.AddNew();
			document1.Type = "TT1";
			document1.Number = "Num1";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusClassPartPivot reLoadedPivot = factory2.Load<CusClassPartPivot>(Pivot.PK);
			AssertEquals("One value in the collection", 1, reLoadedPivot.AQISDocuments.Count);

			AQISDocument document = reLoadedPivot.AQISDocuments.AddNew();
			document.Type = "TT2";
			document.Number = "Num2";
			AssertEquals("Two values in the collection", 2, reLoadedPivot.AQISDocuments.Count);

			factory2.Save();
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
		}

		public void TestAQISDocumentsWithMultipleValuesInAddInfo()
		{
			AQISDocument document1 = Pivot.AQISDocuments.AddNew();
			document1.Type = "TT1";
			document1.Number = "Num1";

			AQISDocument document2 = Pivot.AQISDocuments.AddNew();
			document2.Type = "TT2";
			document2.Number = "Num2";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusClassPartPivot reLoadedPivot = factory2.Load<CusClassPartPivot>(Pivot.PK);
			AssertEquals("One value in the collection", 2, reLoadedPivot.AQISDocuments.Count);

			AQISDocument document = reLoadedPivot.AQISDocuments.AddNew();
			document.Type = "TT3";
			document.Number = "Num3";
			AssertEquals("Two values in the collection", 3, reLoadedPivot.AQISDocuments.Count);

			factory2.Save();
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT3/Num3"));
		}

		#endregion

		#region AQIS Premises Id And Processing Types

		public void TestAQISPremisesIdAndProcessingTypes()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			AssertEquals("No values in collection", 0, Pivot.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = Pivot.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem1";
			premisesIdAndProcessingType.ProcessingType = "Process";
			AssertEquals("One value in the collection", 1, Pivot.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Prem1/Process", Pivot.AddInfo.ZA_AQISPremIdProcessType_Hidden);
		}

		public void TestAQISPremisesIdAndProcessingTypesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			AQISPremisesIdAndProcessingType premProc1 = Pivot.AQISPremisesIdAndProcessingTypes.AddNew();
			premProc1.PremisesId = "Prem1";
			premProc1.ProcessingType = "Process1";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusClassPartPivot reLoadedPivot = factory2.Load<CusClassPartPivot>(Pivot.PK);
			AssertEquals("One value in the collection", 1, reLoadedPivot.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = reLoadedPivot.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem2";
			premisesIdAndProcessingType.ProcessingType = "Process2";
			AssertEquals("Two values in the collection", 2, reLoadedPivot.AQISPremisesIdAndProcessingTypes.Count);

			factory2.Save();
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
		}

		public void TestAQISPremisesIdAndProcessingTypesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			AQISPremisesIdAndProcessingType premProc1 = Pivot.AQISPremisesIdAndProcessingTypes.AddNew();
			premProc1.PremisesId = "Prem1";
			premProc1.ProcessingType = "Process1";

			AQISPremisesIdAndProcessingType premProc2 = Pivot.AQISPremisesIdAndProcessingTypes.AddNew();
			premProc2.PremisesId = "Prem2";
			premProc2.ProcessingType = "Process2";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusClassPartPivot reLoadedPivot = factory2.Load<CusClassPartPivot>(Pivot.PK);
			AssertEquals("One value in the collection", 2, reLoadedPivot.AQISPremisesIdAndProcessingTypes.Count);

			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = reLoadedPivot.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem3";
			premisesIdAndProcessingType.ProcessingType = "Process3";
			AssertEquals("Two values in the collection", 3, reLoadedPivot.AQISPremisesIdAndProcessingTypes.Count);

			factory2.Save();
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
			AssertEquals("Add Info Value", true, reLoadedPivot.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem3/Process3"));
		}

		#endregion

		#region AQIS Commodity Codes

		public void TestAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			AssertEquals("No values in collection", 0, Pivot.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = Pivot.AQISCommodityCodes.AddNew();
			commodityCode.Code = "1";
			AssertEquals("One value in the collection", 1, Pivot.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "1", Pivot.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			Pivot.AddInfo.ZA_AQISCommCodes_Hidden = "1";
			AssertEquals("One value in the collection", 1, Pivot.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = Pivot.AQISCommodityCodes.AddNew();
			commodityCode.Code = "2";
			AssertEquals("Two values in the collection", 2, Pivot.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
		}

		public void TestAQISCommodityCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			Pivot.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, Pivot.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = Pivot.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			AssertEquals("Two values in the collection", 3, Pivot.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISCommCodes_Hidden.Contains("3"));
		}

		public void TestReBuildAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			Pivot.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, Pivot.AQISCommodityCodes.Count);

			AQISCommodityCode commodityCode = Pivot.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			Pivot.AddInfo.ReBuildAQISCommodityCodes();
			AssertEquals("Add Info Value", "1,2,3", Pivot.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		#endregion

		#region AQIS Entity Ids

		public void TestAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			AssertEquals("No values in collection", 0, Pivot.AQISEntityIds.Count);

			AQISEntityId entityId = Pivot.AQISEntityIds.AddNew();
			entityId.Code = "Code1";
			AssertEquals("One value in the collection", 1, Pivot.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", Pivot.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			Pivot.AddInfo.ZA_AQISEntityIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, Pivot.AQISEntityIds.Count);

			AQISEntityId entityId = Pivot.AQISEntityIds.AddNew();
			entityId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, Pivot.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
		}

		public void TestAQISEntityIdsWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			Pivot.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, Pivot.AQISEntityIds.Count);

			AQISEntityId entityId = Pivot.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, Pivot.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			Pivot.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, Pivot.AQISEntityIds.Count);

			AQISEntityId entityId = Pivot.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			Pivot.AddInfo.ReBuildAQISEntityIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", Pivot.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		#endregion

		#region AQIS Permit Ids

		public void TestAQISPermitIds()
		{
			AssertEquals("No values in collection", 0, Pivot.AQISPermitIds.Count);

			AQISPermitId permitId = Pivot.AQISPermitIds.AddNew();
			permitId.Code = "Code1";
			AssertEquals("One value in the collection", 1, Pivot.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", Pivot.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAQISPermitIdsWithOneValueInAddInfo()
		{
			Pivot.AddInfo.ZA_AQISPermitIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, Pivot.AQISPermitIds.Count);

			AQISPermitId permitId = Pivot.AQISPermitIds.AddNew();
			permitId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, Pivot.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
		}

		public void TestAQISPermitIdsWithMultipleValuesInAddInfo()
		{
			Pivot.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, Pivot.AQISPermitIds.Count);

			AQISPermitId permitId = Pivot.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, Pivot.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISPermitIds()
		{
			Pivot.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, Pivot.AQISPermitIds.Count);

			AQISPermitId permitId = Pivot.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			Pivot.AddInfo.ReBuildAQISPermitIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", Pivot.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		#endregion

		#region AQIS Producer Codes

		public void TestAQISProducerCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			AssertEquals("No values in collection", 0, Pivot.AQISProducerCodes.Count);

			AQISProducerCode producerCode = Pivot.AQISProducerCodes.AddNew();
			producerCode.Code = "Code1";
			AssertEquals("One value in the collection", 1, Pivot.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", Pivot.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			Pivot.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, Pivot.AQISProducerCodes.Count);

			AQISProducerCode producerCode = Pivot.AQISProducerCodes.AddNew();
			producerCode.Code = "Code2";
			AssertEquals("Two values in the collection", 2, Pivot.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
		}

		public void TestAQISProducerCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			Pivot.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, Pivot.AQISProducerCodes.Count);

			AQISProducerCode producerCode = Pivot.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			AssertEquals("Two values in the collection", 3, Pivot.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, Pivot.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISProcducerCode()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			Pivot.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, Pivot.AQISProducerCodes.Count);

			AQISProducerCode producerCode = Pivot.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			Pivot.AddInfo.ReBuildAQISProducerCodes();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", Pivot.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		#endregion

		public void TestEffectiveAddInfo()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = product.PK.ToString().Replace("-", "");

			var classification = Factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = Classification.ClassificationType.Both;
			classification.AddInfo.ZA_ORG = "US";
			classification.AddInfo.ZA_ODF = 10m;

			var classPartPivot = Factory.New<CusClassPartPivot>();
			classPartPivot.CI_CC = classification.PK;
			classPartPivot.CI_OP = product.PK;

			classPartPivot.AddInfo.ZA_ORG = "AU";

			Factory.Save();

			var pivotInNewFactory = NewFactory().Load<CusClassPartPivot>(classPartPivot.PK);
			var effectiveAddInfo = pivotInNewFactory.EffectiveAddInfo;

			Assert("Should not has any changes.", !pivotInNewFactory.AddInfo.HasChanges);
			Assert("Should not has any changes.", !effectiveAddInfo.HasChanges);
			AssertEquals("Should load all string from Classification.AddInfo and CusClassPartPivot.AddInfo.", "ODF=10*ORG=AU", effectiveAddInfo.ToString());
		}

		public void TestClassification()
		{
			AssertEquals("Classification", Class, Pivot.Classification);
		}

		public void TestLoadPartAndClassificationFromBaseTypesIfCanOccur()
		{
			var pivot = Factory.New<CusClassPartPivot>();

			var basePart = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			pivot.CI_OP = basePart.PK;
			basePart.OP_PartNum = "PartNum";

			var baseClassification = Factory.New<BaseCusClassification>();
			pivot.CI_CC = baseClassification.PK;
			baseClassification.CC_LookupCode = "TestLookup";
			baseClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
			baseClassification.CC_RN_NKCountryCode = "XX"; // force type decider to load the base implementation.
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pivotIOF = otherFactory.Load<CusClassPartPivot>(pivot.PK);

			AssertNoExceptionThrown("Loading OrgSupplierPart does not blow up.", () => _ = pivotIOF.Part);
			AssertType<AUOrgSupplierPart>("Part is AUOrgSupplierPart", pivotIOF.Part);

			AssertNoExceptionThrown("Loading BaseCusClassification does not blow up.", () => _ = pivotIOF.Classification);
			AssertEquals("The Classification exists", "XX", ((BaseCusClassPartPivot)pivotIOF).Classification.CC_RN_NKCountryCode);
			AssertNull("Classification on AU Pivot is unavailable", pivotIOF.Classification);

			baseClassification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			var otherFactory2 = new BusinessObjectFactory();
			var pivotIOF2 = otherFactory2.Load<CusClassPartPivot>(pivot.PK);

			AssertType<AUOrgSupplierPart>("Part is AU OrgSupplierPart", pivotIOF2.Part);
			AssertType<Classification>("Classification is AU Classification", pivotIOF2.Classification);
		}

		public void TestTariffAndStatNumber()
		{
			Class.CC_TariffNum = "Tariff";
			AssertEquals("Tariff and Stat", Class.CC_TariffNum, Pivot.TariffAndStatNumber);
		}

		public void TestTariffFormatter()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Is Export", true, Pivot.IsExport);
			Assert("TariffFormatter is AUExportTariffUniversalFormatter", Pivot.CurrentTariffFormatter is AUExportTariffUniversalFormatter);

			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Not Export", false, Pivot.IsExport);
			Assert("TariffFormatter is AUImportTariffUniversalFormatter", Pivot.CurrentTariffFormatter is AUImportTariffUniversalFormatter);
		}

		public void TestInstrumentTypeList()
		{
			AssertEquals("Pivot.InstrumentTypeList.Count it should have edifice and cmr lists", 7, Pivot.InstrumentTypeList.Count);
		}

		#region ICPQA
		public void TestCPQuestionsDelete()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			var tariff = Factory.New<Classification>();
			var part = Factory.New<AUOrgSupplierPart>();
			partPivot.CI_CC = tariff.PK;
			partPivot.CI_OP = part.PK;
			Assert("Pre-condition", !partPivot.Classification.IsImport);

			var decQuestion1 = partPivot.Questions.AddNew();
			var decQuestion2 = partPivot.Questions.AddNew();
			Assert(!decQuestion1.IsDeleted);
			Assert(!decQuestion2.IsDeleted);

			partPivot.Delete();
			Assert("Pre-condition", partPivot.IsDeleted);
			Assert(!decQuestion1.IsDeleted);
			Assert(!decQuestion2.IsDeleted);

			var partPivot2 = Factory.New<CusClassPartPivot>();
			var tariff2 = Factory.New<Classification>();
			var part2 = Factory.New<AUOrgSupplierPart>();
			partPivot2.CI_CC = tariff2.PK;
			partPivot2.CI_OP = part2.PK;

			tariff2.CC_ClassificationType = Classification.ClassificationType.IMP;
			Assert("Pre-condition", partPivot2.Classification.IsImport);

			var decQuestion3 = partPivot2.Questions.AddNew();
			var decQuestion4 = partPivot2.Questions.AddNew();
			Assert(!decQuestion3.IsDeleted);
			Assert(!decQuestion4.IsDeleted);

			partPivot2.Delete();
			Assert("Pre-condition", partPivot2.IsDeleted);
			Assert(decQuestion3.IsDeleted);
			Assert(decQuestion4.IsDeleted);
		}

		public void TestICPQAAttachee()
		{
			var product = Factory.New<CusClassPartPivot>();
			AssertEquals("FKColumnInCusEntryCPDecTable", ZArchitecture.Schema.CusEntryCPDecSchema.ON_ParentID, ((ICPQAAttachee)product).FKColumnInCusEntryCPDecTable);
			AssertNotNull("Questions", ((ICPQAAttachee)product).Questions);
			AssertEquals("Questions", true, product.IsRegisteredEditableChildObject(product.Questions));
			AssertEquals("SelectionDate", ZDateTime.Today, ((ICPQAAttachee)product).SelectionDate);
		}

		public void TestICPQAAttacheeHolder()
		{
			var product = Factory.New<CusClassPartPivot>();
			AssertEquals("Headers", 0, ((ICPQAAttacheeHolder)product).Headers.Length);
			AssertEquals("Lines", product, ((ICPQAAttacheeHolder)product).Lines[0]);
			AssertEquals("CachedQuestions", null, ((ICPQAAttacheeHolder)product).CachedQuestions);
		}

		public void TestIsRiskCalculatedFromTariff()
		{
			var product = Factory.New<CusClassPartPivot>();
			AssertEquals("IsRiskCalculatedFromTariff", true, ((ICPQALineAttachee)product).IsRiskCalculatedFromTariff);
		}

		public void TestIsRiskHistorySupported()
		{
			var product = Factory.New<CusClassPartPivot>();
			AssertEquals("IsRiskHistorySupported", true, ((ICPQALineAttachee)product).IsRiskHistorySupported);
		}

		public void TestICPQALineAttachee()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Classification importClassification = Factory.New<Classification>();
			importClassification.CC_TariffNum = "0000.00.00 00";
			importClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClassification.AddInfo.ZA_ORG = "US";

			CMRCusEntryCPDec defaultQuestionFromClassification = importClassification.Questions.AddNew();
			defaultQuestionFromClassification.ON_CPDecNum = 400;
			defaultQuestionFromClassification.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);

			pivot.CI_CC = importClassification.PK;

			CPQuestionKeys key = ((ICPQALineAttachee)pivot).CPQuestionKey;

			CombineAssertions(() =>
			{
				AssertEquals("Key.TariffNumber", "00000000", key.TariffNumber);
				AssertEquals("Key.StatCode", "00", key.StatCode);
				AssertEquals("Key.OriginCode", "US", key.OriginCode);
				AssertEquals("Key.Nature", "", key.Nature);
				AssertEquals("Key.ModeOfTransport", "", key.ModeOfTransport);
				AssertEquals("Key.HasValidNatureOrModeOfTransport", false, key.HasValidOriginOrNatureOrModeOfTransport);

				AssertEquals("SourcesToDefault", 1, ((ICPQALineAttachee)pivot).SourcesToDefault.Length);
				AssertNotNull("DefaultUniqueQuestions", ((ICPQALineAttachee)pivot).DefaultUniqueQuestions);
				AssertEquals("TableCode", "CI", ((ICPQALineAttachee)pivot).TableCode);
			});
		}

		public void TestCPQuestionKey()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "0000.00.00 00";
			AssertEquals("Need to regenerate ", true, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);

			var key = ((ICPQALineAttachee)pivot).CPQuestionKey;
			AssertEquals("Key.TariffNumber", "00000000", key.TariffNumber);

			pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;
			pivot.CI_TariffNum = "1100.00.00 00";
			AssertEquals("Need to regenerate ", true, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);
		}

		public void TestCPDecQuestionsAnswersAvailableFromTariff()
		{
			var currentDate = ZDateTime.Today;
			string sQLCommand = "DELETE FROM RefDbCmrAU_CMRLodgementQuestion";
			Db.Connection.ExecuteNonQuery(sQLCommand);// Need to delete data from a table in CMR Refernce files for testing purposes

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;
			org.OH_RL_NKClosestPort = "AUSYD";

			var cPDec = Factory.New<CMRCusEntryCPDec>();
			cPDec.ON_CPDecNum = 400;
			cPDec.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec.ON_AnswerCode = "Y";
			cPDec.ON_ParentID = org.PK;
			cPDec.ON_Permit = "PERMIT";

			var lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 400;
			lodgementQuestion.CQ_LodgementQuestionType = "CPQ";
			lodgementQuestion.CQ_LodgementQuestionText = "Question to be asked";
			lodgementQuestion.CQ_LodgementQuestionStartDate = currentDate.AddDays(-1000);

			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "84716000";
			profile.CP_StatisticalClassificationCodefield = "55";
			profile.CP_CommunityProtectionRiskIdentifier = 500;
			profile.CP_LineNatureTypefield = "N10";

			var risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_Identifier = 500;
			risk.CK_StartDate = currentDate.AddDays(-1000);
			risk.CK_EndDate = currentDate.AddDays(1000);
			risk.CK_PermitApplicationIndicator = false;
			risk.CK_LodgementQuestionIdentifier = 400;
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "8471.60.00 55";

			pivot.GenerateQuestion();
			Assert(pivot.Questions.Count > 0);
		}

		#endregion

		public void TestProblemWithAQISSaving()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var importPivot = Factory.New<CusClassPartPivot>();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_OP = product.PK;
			AQISDocument document = importPivot.AQISDocuments.AddNew();
			document.Number = "1234";
			document.Type = "Type";

			importPivot.AddInfo.ZA_AQISPermitIds_Hidden = "Code1";
			Factory.Save();
			AssertEquals("AddInfo contains document", true, importPivot.AddInfo.ToString().Contains("AQISDocuments_Hidden"));
			AssertEquals("AddInfo contains permit", true, importPivot.AddInfo.ToString().Contains("AQISPermitIds_Hidden"));
		}

		public void TestAddInfo()
		{
			var newPivot = GetTestDataForCPDecQuestions();

			AssertEquals("Initial AddInfo", "", newPivot.CI_AddInfo);
			newPivot.AddInfo.ZA_ORG = "US";
			AssertEquals("Initial AddInfo", "ORG=US", newPivot.CI_AddInfo);
		}

		public void TestAddInfoAggregatesClassificationSelectedForPart()
		{
			var newClass = Factory.New<Classification>();
			var newPart = Factory.New<CusClassPartPivot>();
			newPart.CI_CC = newClass.PK;
			AssertEquals("Precondition : AddInfo", "", newPart.CI_AddInfo);

			newPart.Classification.AddInfo.ZA_ORG = "USA";
			newPart.CI_CC = Class.PK;
			AssertEquals("Precondition : AddInfo", "", newPart.CI_AddInfo);

			newPart.AddInfo.ZA_GSTE = "420A";

			Assert("AddInfo", newPart.CI_AddInfo.IndexOf("ORG=USA") < 0);
			Assert("AddInfo", newPart.CI_AddInfo.IndexOf("GSTE=420A") >= 0);
		}

		public void TestDoNotCopyClassificationAddInfoToProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "43298";
			importer.OH_RL_NKClosestPort = "AUSYD";

			var newPart = Factory.New<AUOrgSupplierPart>();
			newPart.OP_PartNum = "TEST100";
			newPart.RelatedOrganisations.AddNew().OU_OH = importer.PK;

			var importClass = Factory.New<Classification>();
			importClass.CC_LookupCode = "IMPLookup";
			importClass.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClass.AddInfo.ZA_GSTE = "FOOD";
			importClass.AddInfo.ZA_TreatmentCode_Hidden = "110";

			var pivot = newPart.AddNewImportPivotWithClassification(importClass.PK);

			Assert("Classification AddInfo should not be copied to product", pivot.CI_AddInfo.IndexOf("GSTE=FOOD") < 0);
			Assert("Classification AddInfo should not be copied to product", pivot.CI_AddInfo.IndexOf("TreatmentCode_Hidden=110") < 0);
			AssertEquals("GSTE=FOOD", pivot.ClassificationAddInfoString);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TEST100";

			AssertEquals("copied from lookup to invoiceline", "FOOD", invoiceLine.AddInfo.ZA_GSTE);
			AssertEquals("copied from lookup to invoiceline", "110", invoiceLine.AddInfo.ZA_TreatmentCode_Hidden);
		}

		public void TestOrgSupplierPartClearDataAndDeleteChildren()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SDK232KSD";
			var product = Factory.NewWithValidTestData<AUOrgSupplierPart>();
			product.OP_PartNum = "10TEST32";
			var relatedOrg = product.RelatedOrganisations.AddOwner(org);
			Factory.Save();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1";
			var classification = Factory.NewWithValidTestData<Classification>();
			classification.CC_ClassificationType = Classification.ClassificationType.IMP;
			pivot.CI_CC = classification.PK;
			Factory.Save();
			product.ClearDataAndDeleteChildren();
			AssertEquals(0, product.PivotsForBinding.Count);
		}

		public void TestDefaultPivotType()
		{
			AssertEquals(ClassificationTypeList.Codes.HTI, Factory.New<CusClassPartPivot>().CI_ChildType);
		}

		public void TestImportExportClassificationDetails()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var expTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			var expTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "00000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(expTariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TY");
			helper.CreateTariffUOM(expTariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TX");
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var impTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			var impTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(impTariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TA");
			helper.CreateTariffUOM(impTariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TB");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var exportClassification1 = Factory.New<Classification>();
				exportClassification1.CC_TariffNum = "0000.00.01";
				exportClassification1.CC_ClassificationType = Classification.ClassificationType.EXP;

				var exportClassification2 = Factory.New<Classification>();
				exportClassification2.CC_TariffNum = "0000.00.02";
				exportClassification2.CC_ClassificationType = Classification.ClassificationType.EXP;

				var importClassification1 = Factory.New<Classification>();
				importClassification1.CC_TariffNum = "0000.00.00 01";
				importClassification1.CC_ClassificationType = Classification.ClassificationType.IMP;

				var importClassification2 = Factory.New<Classification>();
				importClassification2.CC_TariffNum = "0000.00.00 02";
				importClassification2.CC_ClassificationType = Classification.ClassificationType.IMP;

				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_CC = exportClassification1.PK;
				AssertEquals("TY", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = exportClassification2.PK;
				AssertEquals("TX", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification1.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification2.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_CC = exportClassification1.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = exportClassification2.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification1.PK;
				AssertEquals("TA", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification2.PK;
				AssertEquals("TB", pivot.TariffCustomsUnitQuantity);
			}
		}

		public void TestImportExportClassificationDetails_Old()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var aHECC = Factory.New<AUCAHECC>();
				aHECC.UA_AHECC = "0000.00.01";
				aHECC.UA_UQ = "TY";

				var aHECC2 = Factory.New<AUCAHECC>();
				aHECC2.UA_AHECC = "0000.00.02";
				aHECC2.UA_UQ = "TX";

				var statClassification1 = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				statClassification1.SC_TariffClassificationNumber = "00000000";
				statClassification1.SC_StatisticalClassificationCode = "03";
				statClassification1.SC_QuantityUnit = "TA";
				statClassification1.SC_StartDate = ZDateTime.MinSmallDateTimeValue;

				var statClassification2 = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				statClassification2.SC_TariffClassificationNumber = "00000000";
				statClassification2.SC_StatisticalClassificationCode = "04";
				statClassification2.SC_QuantityUnit = "TB";
				statClassification2.SC_StartDate = ZDateTime.MinSmallDateTimeValue;

				var exportClassification1 = Factory.New<Classification>();
				exportClassification1.CC_TariffNum = "0000.00.01";
				exportClassification1.CC_ClassificationType = Classification.ClassificationType.EXP;

				var exportClassification2 = Factory.New<Classification>();
				exportClassification2.CC_TariffNum = "0000.00.02";
				exportClassification2.CC_ClassificationType = Classification.ClassificationType.EXP;

				var importClassification1 = Factory.New<Classification>();
				importClassification1.CC_TariffNum = "0000.00.00 03";
				importClassification1.CC_ClassificationType = Classification.ClassificationType.IMP;

				var importClassification2 = Factory.New<Classification>();
				importClassification2.CC_TariffNum = "0000.00.00 04";
				importClassification2.CC_ClassificationType = Classification.ClassificationType.IMP;

				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_CC = exportClassification1.PK;
				AssertEquals("TY", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = exportClassification2.PK;
				AssertEquals("TX", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification1.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification2.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_CC = exportClassification1.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = exportClassification2.PK;
				AssertEquals("", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification1.PK;
				AssertEquals("TA", pivot.TariffCustomsUnitQuantity);
				pivot.CI_CC = importClassification2.PK;
				AssertEquals("TB", pivot.TariffCustomsUnitQuantity);
			}
		}

		public void TestNeedToRegenerateQuestionsGetsSet()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Classification importClassification = Factory.New<Classification>();
			importClassification.CC_TariffNum = "0000.00.00 00";
			importClassification.CC_ClassificationType = Classification.ClassificationType.IMP;

			AssertEquals("Need to regenerate ", true, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);
			pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;

			pivot.CI_CC = importClassification.PK;
			AssertEquals("Need to regenerate ", true, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);

			pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;
			pivot.CI_AddInfo = "ORG=AU";
			AssertEquals("Need to regenerate ", true, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);

			pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Need to regenerate ", false, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);

			pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Need to regenerate ", true, pivot.LineAttacheeHolderWrapper.NeedToGenerateQuestions);
		}

		public void TestDefaultUniversalTariffProperties()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("Using Universal Tariff", false, typeof(CusClassPartPivot).GetProperty("UseUniversalTariff", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pivot));
		}

		public void TestDutyRateForCurrentCountry()
		{
			DutyRateHelper.CreateTestTariffRatePeriodSnapshot(Factory);
			DutyRateHelper.CreateTestTariffRatePeriodSnapshot(Factory, Core.Constants.CountryCodes.NewZealand, 1, "CALC");
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "4201.00.00 01";

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Type is HTE, Tariff for 4201.00.00 01", ZString.Empty, pivot.DutyRateForCurrentCountry);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertEquals("Type is HTB, Tariff for 4201.00.00 01", "5.00000", pivot.DutyRateForCurrentCountry);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Type is HTI, Tariff for 4201.00.00 01", "5.00000", pivot.DutyRateForCurrentCountry);

			pivot.AddInfo.AddInfoLine = "PST=NZ";
			AssertEquals("Test for AddInfo", "1.00000", pivot.DutyRateForCurrentCountry);
			pivot.AddInfo.AddInfoLine = ZString.Empty;

			pivot.CI_TariffNum = "0701.10.00 01";
			AssertEquals("Tariff for 0701.10.00 01", "0.00000", pivot.DutyRateForCurrentCountry);

			pivot.CI_TariffNum = "";
			AssertEquals("Empty Tariff", ZString.Empty, pivot.DutyRateForCurrentCountry);

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = Classification.ClassificationType.IMP;
			classification.CC_TariffNum = "4201.00.00 01";
			pivot.CI_CC = classification.PK;

			AssertEquals("Return Tariff from Classification", "5.00000", pivot.DutyRateForCurrentCountry);

			pivot.AddInfo.AddInfoLine = "PST=NZ";
			AssertEquals("Return Tariff from Classification, Test for AddInfo", "1.00000", pivot.DutyRateForCurrentCountry);
		}

		CusClassPartPivot fPivot;
		CusClassPartPivot Pivot
		{
			get
			{
				if (fPivot == null)
				{
					fPivot = Factory.New<CusClassPartPivot>();
					fPivot.CI_CC = Class.PK;
					fPivot.CI_OP = Product.PK;
				}
				return fPivot;
			}
		}

		Classification fClass;
		Classification Class
		{
			get
			{
				if (fClass == null)
				{
					fClass = Factory.New<Classification>();
					fClass.CC_LookupCode = "TestLookup";
					fClass.CC_ClassificationType = Classification.ClassificationType.Both;
				}
				return fClass;
			}
		}

		AUOrgSupplierPart fProduct;
		AUOrgSupplierPart Product
		{
			get
			{
				if (fProduct == null)
				{
					fProduct = Factory.New<AUOrgSupplierPart>();
					fProduct.OP_PartNum = fProduct.PK.ToString().Replace("-", "");
				}
				return fProduct;
			}
		}

		CusClassPartPivot GetTestDataForCPDecQuestions()
		{
			ZTestHelper helper = new ZTestHelper(new BusinessObjectFactory());

			Classification importClassWithQuestions = Factory.New<Classification>();

			SetValuesToClassification(importClassWithQuestions, JobDeclaration.ClassificationType.IMP, helper.TestTariffNumber1);

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsConsignor = true;

			AUOrgSupplierPart newPart = Factory.New<AUOrgSupplierPart>();
			newPart.OP_PartNum = "XXX111";
			OrgPartRelation relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = consignor.PK;
			newPart.OP_StockKeepingUnit = "CT";
			var result = newPart.AddNewImportPivotWithClassification(importClassWithQuestions.PK);

			return result;
		}

		void SetValuesToClassification(Classification @class, string classType, string tariffNumber)
		{
			@class.CC_ClassificationType = classType;
			@class.CC_LookupCode = "LookupCode1";
			@class.CC_TariffNum = tariffNumber;
			@class.CC_Description = "DESCRIPTION";
			@class.CC_IsActive = false;
			@class.CC_AddInfo = "ORG=AU";
			@class.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
