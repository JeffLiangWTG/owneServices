using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUOrgSupplierPart))]
	sealed class AUOrgSupplierPartTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var part = Factory.New<AUOrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Australia, pivot.CI_RN_NKCountry);
			}
		}

		public void TestEndToEndTest()
		{
			#region Data Setup
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "Canadian Wine";
			var partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "BOT";
			partUnit.OF_ParentPackType = "BOX";
			partUnit.OF_QuantityInParent = 12;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "L";
			partUnit.OF_ParentPackType = "BOT";
			partUnit.OF_QuantityInParent = 120m;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "LA";
			partUnit.OF_ParentPackType = "BOT";
			partUnit.OF_QuantityInParent = 1.20m;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var impTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(impTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			helper.CreateTariffUOM(impTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "T");

			var classification = Factory.New<Classification>();
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			classification.CC_TariffNum = "1234.56.78 90";
			classification.CC_LookupCode = "Wine";
			classification.CC_Description = "Wine";
			classification.CC_ClassificationType = "IMP";

			Factory.Save();

			#endregion
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var importPivot = product.PivotsForBinding.AddNew();
				importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				importPivot.CI_CC = classification.PK;
				importPivot.AddInfo.ZA_ICN = "A";
				importPivot.AddInfo.ZA_GSTE = "FOOD";
				importPivot.AddInfo.ZA_LCTQ = "Y";
				importPivot.AddInfo.ZA_WETQ = "Y";
				importPivot.AddInfo.ZA_PST = "";
				importPivot.AddInfo.ZA_RNO = "002";
				importPivot.AddInfo.ZA_TRN = "004";
				importPivot.AddInfo.ZA_TR2 = "005";

				var lineDutyData = ((IDutyDataFromInvoiceLineProvider)product).GetDutyDataFromInvoiceLine(new ZDateTime(2009, 1, 1), buyer.PK, supplier.PK);

				AssertEquals("12345678", lineDutyData.FirstTariffNumber);
				AssertEquals("90", lineDutyData.StatCode);
				AssertEquals(new ZDateTime(2009, 1, 1), lineDutyData.EffectiveDutyDate);
				AssertEquals("KG", lineDutyData.FirstUQ);
				AssertEquals("T", lineDutyData.SecondUQ);
				AssertEquals("GEN", lineDutyData.Preference);
				AssertEquals(true, lineDutyData.IsGSTExempt);
				AssertEquals("A", lineDutyData.ICN);
				AssertEquals(true, lineDutyData.IsLCTPayable);
				AssertEquals(true, lineDutyData.IsLCTExempt);
				AssertEquals(true, lineDutyData.IsWETExempt);
				AssertEquals("In invoice line this is calculated. in product, it should default back to GEN", "GEN", lineDutyData.Preference);
				AssertEquals("002", lineDutyData.RateNumber);
				AssertEquals("004", lineDutyData.TreatmentRateNumber);
				AssertEquals("005", lineDutyData.SecondTreatmentCode);
			}
		}

		public void TestEndToEndTest_Old()
		{
			#region Data Setup
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "Canadian Wine";
			var partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "BOT";
			partUnit.OF_ParentPackType = "BOX";
			partUnit.OF_QuantityInParent = 12;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "L";
			partUnit.OF_ParentPackType = "BOT";
			partUnit.OF_QuantityInParent = 120m;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "LA";
			partUnit.OF_ParentPackType = "BOT";
			partUnit.OF_QuantityInParent = 1.20m;

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var classification = Factory.New<Classification>();
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			classification.CC_TariffNum = "2203.00.39 26";
			classification.CC_LookupCode = "Wine";
			classification.CC_Description = "Wine";
			classification.CC_ClassificationType = "IMP";

			Factory.Save();

			#endregion

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var importPivot = product.PivotsForBinding.AddNew();
				importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				importPivot.CI_CC = classification.PK;
				importPivot.AddInfo.ZA_ICN = "A";
				importPivot.AddInfo.ZA_GSTE = "FOOD";
				importPivot.AddInfo.ZA_LCTQ = "Y";
				importPivot.AddInfo.ZA_WETQ = "Y";
				importPivot.AddInfo.ZA_PST = "";
				importPivot.AddInfo.ZA_RNO = "002";
				importPivot.AddInfo.ZA_TRN = "004";
				importPivot.AddInfo.ZA_TR2 = "005";

				var lineDutyData = ((IDutyDataFromInvoiceLineProvider)product).GetDutyDataFromInvoiceLine(new ZDateTime(2009, 1, 1), buyer.PK, supplier.PK);

				AssertEquals("22030039", lineDutyData.FirstTariffNumber);
				AssertEquals("26", lineDutyData.StatCode);
				AssertEquals(new ZDateTime(2009, 1, 1), lineDutyData.EffectiveDutyDate);
				AssertEquals("LA", lineDutyData.FirstUQ);
				AssertEquals("L", lineDutyData.SecondUQ);
				AssertEquals("GEN", lineDutyData.Preference);
				AssertEquals(true, lineDutyData.IsGSTExempt);
				AssertEquals("A", lineDutyData.ICN);
				AssertEquals(true, lineDutyData.IsLCTPayable);
				AssertEquals(true, lineDutyData.IsLCTExempt);
				AssertEquals(true, lineDutyData.IsWETExempt);
				AssertEquals("In invoice line this is calculated. in product, it should default back to GEN", "GEN", lineDutyData.Preference);
				AssertEquals("002", lineDutyData.RateNumber);
				AssertEquals("004", lineDutyData.TreatmentRateNumber);
				AssertEquals("005", lineDutyData.SecondTreatmentCode);
			}
		}

		[ExpectNoExceptions]
		public void TestRowNotInTableForDataRefreshBusUpdateInvolved()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var importPivot = part.AddNewImportPivotWithClassification(importClass.PK);
			part.OP_PartNum = "TestTestTestTest";

			var anotherImportClass = Factory.New<Classification>();
			anotherImportClass.CC_LookupCode = "LookupCode2";
			anotherImportClass.CC_ClassificationType = Common.ClassificationType.IMP;
			anotherImportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testDec = newFactory.New<JobDeclaration>();
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_OH_Supplier = supplier;
			invoiceLine.JI_PartNo = part.OP_PartNum;

			importPivot.CI_CC = anotherImportClass.PK;
			Factory.Save();

			importPivot.CI_CC = importClass.PK;
			Factory.Save();
		}

		public void TestDeleteProduct()
		{
			//SetUp
			Assert("PreCondition:Part Relation exists", part.RelatedOrganisations.Count > 0);
			Assert("PreCondition:Import Class pivot is there", part.PivotsForBinding.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI) != null);
			Assert("PreCondition:Export Class pivot is there", part.PivotsForBinding.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE) != null);

			var relation = part.RelatedOrganisations[0];

			var unit = part.PartUnits.AddNew();
			unit.OF_ParentPackType = "KG";
			unit.OF_PackType = "PKG";
			unit.OF_QuantityInParent = 10m;

			var importPivot = part.PivotsForBinding.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTI);
			var exportPivot = part.PivotsForBinding.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE);
			part.Delete();
			Assert("Part is successfully deleted", part.IsDeleted);
			Assert("Part unit is successfully deleted", unit.IsDeleted);
			Assert("Relation is deleted", relation.IsDeleted);
			Assert("Import Pivot is deleted", importPivot.IsDeleted);
			Assert("Export Pivot is deleted", exportPivot.IsDeleted);
		}

		public void TestOP_Cubic()
		{
			var part = Factory.New<AUOrgSupplierPart>();

			AssertEquals("Cubic", 0.0m, part.OP_Cubic);

			part.OP_Width = 10;
			AssertEquals("Cubic", 0.0m, part.OP_Cubic);

			part.OP_Height = 10;
			AssertEquals("Cubic", 0m, part.OP_Cubic);

			part.OP_Depth = 10;
			part.OP_MeasureUQ = "M";
			AssertEquals("Cubic", 1000m, part.OP_Cubic);
		}

		public void TestPartNumUnique()
		{
			var newPart = Factory.New<AUOrgSupplierPart>();
			newPart.OP_PartNum = part.OP_PartNum;
			var relation = newPart.RelatedOrganisations.AddNew();
			relation.OU_OH = part.RelatedOrganisations[0].OU_OH;
			relation.OU_Relationship = part.RelatedOrganisations[0].OU_Relationship;
			newPart.RunPreSaveValidation();
			Assert(newPart.HasErrors);
		}

		public void TestDeletePivot()
		{
			var newPart = Factory.Load<AUOrgSupplierPart>(part.PK);

			var expPivot = newPart.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault(x => x.IsExport);
			while (expPivot != null)
			{
				expPivot.Delete();
				expPivot = newPart.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault(x => x.IsExport);
			}
			Assert("One Pivot Table", newPart.ClassificationsForBinding.Count == 1);
		}

		public void TestLoadClassificationsAfterDelete()
		{
			var newPart = Factory.Load<AUOrgSupplierPart>(part.PK);
			var expPivot = newPart.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault(x => x.IsExport);
			while (expPivot != null)
			{
				expPivot.Delete();
				expPivot = newPart.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault(x => x.IsExport);
			}

			var loadedPart = Factory.Load<AUOrgSupplierPart>(newPart.PK);
			AssertEquals("There should be only one classification", 1, loadedPart.ClassificationsForBinding.Count);
		}

		public void TestImportTreatmentCode()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Pre-condition: ImportTreatmentCode", "", product.ImportTreatmentCode);

			pivot1.CI_CC = Factory.New<BaseCusClassification>().PK;
			pivot1.ClassificationAddInfo.ZA_TreatmentCode_Hidden = "808";
			AssertEquals("ImportTreatmentCode - Should come from classification addinfo", "808", product.ImportTreatmentCode);

			pivot1.AddInfo.ZA_TreatmentCode_Hidden = "901";
			AssertEquals("ImportTreatmentCode", "901", product.ImportTreatmentCode);

			var exportPivot = product.PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = "HTE";
			AssertEquals("ImportTreatmentCode - export class does not affect value", "901", product.ImportTreatmentCode);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.AddInfo.ZA_TreatmentCode_Hidden = "901";
			AssertEquals("ImportTreatmentCode - both import classifications have the same value", "901", product.ImportTreatmentCode);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.AddInfo.ZA_TreatmentCode_Hidden = "715";
			AssertEquals("ImportTreatmentCode - multiple values", "MULTI", product.ImportTreatmentCode);
		}

		public void TestInstrumentType()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Pre-condition: InstrumentType", "", product.InstrumentType);

			pivot1.CI_CC = Factory.New<BaseCusClassification>().PK;
			pivot1.ClassificationAddInfo.ZA_InstrumentType_Hidden = "TX";
			AssertEquals("InstrumentType - Should come from classification addinfo", "TX", product.InstrumentType);

			pivot1.AddInfo.ZA_InstrumentType_Hidden = "AD";
			AssertEquals("InstrumentType", "AD", product.InstrumentType);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.AddInfo.ZA_InstrumentType_Hidden = "AD";
			AssertEquals("InstrumentType - both classifications have same value", "AD", product.InstrumentType);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.AddInfo.ZA_InstrumentType_Hidden = "BE";
			AssertEquals("InstrumentType - multiple values", "MULTI", product.InstrumentType);
		}

		public void TestInstrumentCode()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Pre-condition: InstrumentCode", "", product.InstrumentCode);

			pivot1.CI_CC = Factory.New<BaseCusClassification>().PK;
			pivot1.ClassificationAddInfo.ZA_InstrumentCode_Hidden = "68";
			AssertEquals("InstrumentCode - Should come from classification addinfo", "68", product.InstrumentCode);

			pivot1.AddInfo.ZA_InstrumentCode_Hidden = "15";
			AssertEquals("InstrumentCode", "15", product.InstrumentCode);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.AddInfo.ZA_InstrumentCode_Hidden = "15";
			AssertEquals("InstrumentCode - both classifications have same value", "15", product.InstrumentCode);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.AddInfo.ZA_InstrumentCode_Hidden = "39";
			AssertEquals("InstrumentCode - multiple values", "MULTI", product.InstrumentCode);
		}

		public void TestAddInfo()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.AddInfo.AddInfoLine = "AMB=AAA";
			AssertEquals("AddInfoLine", "AMB=AAA", product.AddInfoLine);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.AddInfo.AddInfoLine = "";
			AssertEquals("AddInfoLine - multiple pivots but single value", "AMB=AAA", product.AddInfoLine);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.AddInfo.AddInfoLine = "AMB=AAA";
			AssertEquals("AddInfoLine - multiple times the same value", "AMB=AAA", product.AddInfoLine);

			var pivot4 = product.PivotsForBinding.AddNew();
			pivot4.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot4.AddInfo.AddInfoLine = "AMB=ABC";
			AssertEquals("AddInfoLine - multiple values", "MULTI", product.AddInfoLine);
		}

		public void TestImportLookupClassification()
		{
			AssertEquals("ImportClassificationLookup", "LookupCode1", part.ImportClassificationLookup);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);
			_ = part.AddNewImportPivotWithClassification(importClass.PK);
			part.OP_PartNum = "TestTestTestTest";

			var anotherImportClass = Factory.New<Classification>();
			anotherImportClass.CC_LookupCode = "LookupCode2";
			anotherImportClass.CC_ClassificationType = Common.ClassificationType.IMP;
			anotherImportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();

			_ = part.AddNewImportPivotWithClassification(anotherImportClass.PK);
			AssertEquals("ImportClassificationLookup - multiple values", "MULTI", part.ImportClassificationLookup);
		}

		public void TestExportLookupClassification()
		{
			AssertEquals("ExportClassificationLookup", "LookupCode1", part.ExportClassificationLookup);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier, OrgPartRelation.RelationshipTypes.Supplier);
			_ = part.AddNewExportPivotWithClassification(exportClass.PK);
			part.OP_PartNum = "TestTestTestTest";

			var anotherExportClass = Factory.New<Classification>();
			anotherExportClass.CC_LookupCode = "LookupCode2";
			anotherExportClass.CC_ClassificationType = Common.ClassificationType.EXP;
			anotherExportClass.CC_TariffNum = "0208.90.00 28";
			Factory.Save();

			_ = part.AddNewExportPivotWithClassification(anotherExportClass.PK);
			AssertEquals("ExportClassificationLookup - multiple values", "MULTI", part.ExportClassificationLookup);
		}

		/// <summary> Error Example
		///		2
		/// KG -> CS
		///		3
		/// CS -> KG
		/// </summary>
		public void TestValidateMoreThanOneConversionFactors()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99990000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var unit1 = Factory.New<OrgPartUnit>();
				unit1.OF_OP = part.PK;
				unit1.OF_PackType = "KG";
				unit1.OF_ParentPackType = "CS";
				unit1.OF_QuantityInParent = 3;

				var unit2 = Factory.New<OrgPartUnit>();
				unit2.OF_OP = part.PK;
				unit2.OF_PackType = "CS";
				unit2.OF_ParentPackType = "KG";
				unit2.OF_QuantityInParent = 2;

				var loadedPart = Factory.Load<AUOrgSupplierPart>(part.PK);
				loadedPart.PartUnits.Load();
				loadedPart.OP_StockKeepingUnit = "CS";
				loadedPart.PivotsForBinding.RemoveAndDeleteAll();

				loadedPart.RunPreSaveValidation();
				Assert("Error Expected", loadedPart.OP_StockKeepingUnitInfo.HasNotifications());
			}
		}

		public void TestValidateMoreThanOneConversionFactors_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var aHECC = Factory.New<AUCAHECC>();
				aHECC.UA_AHECC = ExportTariff;
				aHECC.UA_UQ = "KG";

				var unit1 = Factory.New<OrgPartUnit>();
				unit1.OF_OP = part.PK;
				unit1.OF_PackType = "KG";
				unit1.OF_ParentPackType = "CS";
				unit1.OF_QuantityInParent = 3;

				var unit2 = Factory.New<OrgPartUnit>();
				unit2.OF_OP = part.PK;
				unit2.OF_PackType = "CS";
				unit2.OF_ParentPackType = "KG";
				unit2.OF_QuantityInParent = 2;

				var loadedPart = Factory.Load<AUOrgSupplierPart>(part.PK);
				loadedPart.PartUnits.Load();
				loadedPart.OP_StockKeepingUnit = "CS";
				loadedPart.PivotsForBinding.RemoveAndDeleteAll();

				loadedPart.RunPreSaveValidation();
				Assert("Error Expected", loadedPart.OP_StockKeepingUnitInfo.HasNotifications());
			}
		}

		/// <summary> Error Example
		///		2
		/// KG -> CS
		///		3
		/// KG -> CS
		/// </summary>
		public void TestValidateMoreThanOneConversionFactors1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "99990000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var unit1 = Factory.New<OrgPartUnit>();
				unit1.OF_OP = part.PK;
				unit1.OF_PackType = "KG";
				unit1.OF_ParentPackType = "CS";
				unit1.OF_QuantityInParent = 2;

				var unit2 = Factory.New<OrgPartUnit>();
				unit2.OF_OP = part.PK;
				unit2.OF_PackType = "KG";
				unit2.OF_ParentPackType = "CS";
				unit2.OF_QuantityInParent = 3;

				var loadedPart = Factory.Load<AUOrgSupplierPart>(part.PK);
				loadedPart.PartUnits.Load();
				loadedPart.OP_StockKeepingUnit = "CS";
				loadedPart.PivotsForBinding.RemoveAndDeleteAll();

				loadedPart.RunPreSaveValidation();
				Assert("Error Expected", loadedPart.OP_StockKeepingUnitInfo.HasNotifications());
			}
		}

		public void TestValidateMoreThanOneConversionFactors1_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var aHECC = Factory.New<AUCAHECC>();
				aHECC.UA_AHECC = ExportTariff;
				aHECC.UA_UQ = "KG";

				var unit1 = Factory.New<OrgPartUnit>();
				unit1.OF_OP = part.PK;
				unit1.OF_PackType = "KG";
				unit1.OF_ParentPackType = "CS";
				unit1.OF_QuantityInParent = 2;

				var unit2 = Factory.New<OrgPartUnit>();
				unit2.OF_OP = part.PK;
				unit2.OF_PackType = "KG";
				unit2.OF_ParentPackType = "CS";
				unit2.OF_QuantityInParent = 3;

				var loadedPart = Factory.Load<AUOrgSupplierPart>(part.PK);
				loadedPart.PartUnits.Load();
				loadedPart.OP_StockKeepingUnit = "CS";
				loadedPart.PivotsForBinding.RemoveAndDeleteAll();

				loadedPart.RunPreSaveValidation();
				Assert("Error Expected", loadedPart.OP_StockKeepingUnitInfo.HasNotifications());
			}
		}

		public void TestChangeStockKeepingUnitReValidate()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
				var impTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
				helper.CreateTariffUOM(impTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NO");

				importClass.CC_TariffNum = "1111.11.11 11";

				var newPart = Factory.New<AUOrgSupplierPart>();
				newPart.AddNewImportPivotWithClassification(importClass.PK);

				newPart.OP_StockKeepingUnit = "BOX";
				var warnings = string.Join(", ", newPart.OP_StockKeepingUnitInfo.GetWarnings().Select(x => x.Message));
				AssertContains("Conversion from BOX to NO suggested", "conversion from Stock Keeping UQ 'BOX' to Classification UQ 'NO'", warnings);

				newPart.OP_StockKeepingUnit = "PCE";//As RefPackConversion from PCE to NO is there
				Assert("No conversion necessary", !newPart.OP_StockKeepingUnitInfo.HasWarnings());
			}
		}

		public void TestChangeStockKeepingUnitReValidate_Old()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var importTariff = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				importTariff.SC_TariffClassificationNumber = "00000000";
				importTariff.SC_StatisticalClassificationCode = "00";
				importTariff.SC_QuantityUnit = "NO";
				importTariff.SC_StartDate = ZDateTime.MinSmallDateTimeValue;

				importClass.CC_TariffNum = "0000.00.00 00";

				var newPart = Factory.New<AUOrgSupplierPart>();
				newPart.AddNewImportPivotWithClassification(importClass.PK);

				newPart.OP_StockKeepingUnit = "BOX";
				var warnings = string.Join(", ", newPart.OP_StockKeepingUnitInfo.GetWarnings().Select(x => x.Message));
				AssertContains("Conversion from BOX to NO suggested", "conversion from Stock Keeping UQ 'BOX' to Classification UQ 'NO'", warnings);

				newPart.OP_StockKeepingUnit = "PCE";//As RefPackConversion from PCE to NO is there
				Assert("No conversion necessary", !newPart.OP_StockKeepingUnitInfo.HasWarnings());
			}
		}

		public void TestChangeClassificationDetailReValidate()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
				var impTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
				helper.CreateTariffUOM(impTariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NO");
				var impTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
				helper.CreateTariffUOM(impTariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

				importClass.CC_TariffNum = "1111.11.11 11";
				var newPart = Factory.New<AUOrgSupplierPart>();
				var pivot = newPart.AddNewImportPivotWithClassification(importClass.PK);
				newPart.OP_StockKeepingUnit = "BOX";
				var warnings = string.Join(", ", newPart.OP_StockKeepingUnitInfo.GetWarnings().Select(x => x.Message));
				AssertContains("Conversion from BOX to NO suggested", "conversion from Stock Keeping UQ 'BOX' to Classification UQ 'NO'", warnings);

				var newClass = Factory.New<Classification>();
				newClass.CC_TariffNum = "2222.22.22 22";
				newClass.CC_LookupCode = "NewClass";
				newClass.CC_ClassificationType = Common.ClassificationType.IMP;
				newClass.CC_Description = "Description";
				pivot.CI_CC = newClass.PK;

				newPart.Validation.ValidateOP_StockKeepingUnit();
				warnings = string.Join(", ", newPart.OP_StockKeepingUnitInfo.GetWarnings().Select(x => x.Message));
				AssertContains("Conversion from BOX to KG suggested", "conversion from Stock Keeping UQ 'BOX' to Classification UQ 'KG'", warnings);
			}
		}

		public void TestChangeClassificationDetailReValidate_Old()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var statNO = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				statNO.SC_TariffClassificationNumber = "11111111";
				statNO.SC_StatisticalClassificationCode = "11";
				statNO.SC_QuantityUnit = "NO";
				statNO.SC_StartDate = ZDateTime.MinSmallDateTimeValue;
				var statKG = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				statKG.SC_TariffClassificationNumber = "22222222";
				statKG.SC_StatisticalClassificationCode = "22";
				statKG.SC_QuantityUnit = "KG";
				statKG.SC_StartDate = ZDateTime.MinSmallDateTimeValue;

				var newPart = Factory.New<AUOrgSupplierPart>();
				importClass.CC_TariffNum = "1111.11.11 11";
				var pivot = newPart.AddNewImportPivotWithClassification(importClass.PK);

				newPart.OP_StockKeepingUnit = "BOX";
				var warnings = string.Join(", ", newPart.OP_StockKeepingUnitInfo.GetWarnings().Select(x => x.Message));
				AssertContains("Conversion from BOX to NO suggested", "conversion from Stock Keeping UQ 'BOX' to Classification UQ 'NO'", warnings);

				var newClass = Factory.New<Classification>();
				newClass.CC_TariffNum = "2222.22.22 22";
				newClass.CC_LookupCode = "NewClass";
				newClass.CC_ClassificationType = Common.ClassificationType.IMP;
				newClass.CC_Description = "Description";
				pivot.CI_CC = newClass.PK;

				newPart.Validation.ValidateOP_StockKeepingUnit();
				warnings = string.Join(", ", newPart.OP_StockKeepingUnitInfo.GetWarnings().Select(x => x.Message));
				AssertContains("Conversion from BOX to KG suggested", "conversion from Stock Keeping UQ 'BOX' to Classification UQ 'KG'", warnings);
			}
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var exportClassification = Factory.NewWithValidTestData<Classification>();
			var importClassification = Factory.NewWithValidTestData<Classification>();
			exportClassification.CC_ClassificationType = JobDeclaration.ClassificationType.EXP;
			importClassification.CC_ClassificationType = JobDeclaration.ClassificationType.IMP;
			Factory.Save();

			var product = Factory.New<AUOrgSupplierPart>();
			_ = product.RelatedOrganisations.AddNew(); // from base product
			_ = product.PartUnits.AddNew(); // from this product object
			var exportPivot = product.AddNewExportPivotWithClassification(exportClassification.PK);
			var importPivot = product.AddNewImportPivotWithClassification(importClassification.PK);

			AssertEquals("BusinessObjectsWithRelatedEvents.Length", 4, product.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain ExportPivot.", exportPivot, product.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain ImportPivot.", importPivot, product.BusinessObjectsWithRelatedEvents);
		}

		public void TestRFPProduceType()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Pre-condition: RFPProduceType", "", product.RFPProduceType);

			pivot1.AddInfo.ZA_AQISProduceType_Hidden = "DAI";
			AssertEquals("RFPProduceType", "DAI", product.RFPProduceType);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.AddInfo.ZA_AQISProduceType_Hidden = "DAI";
			AssertEquals("RFPProduceType - both classifications have same value", "DAI", product.RFPProduceType);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot3.AddInfo.ZA_AQISProduceType_Hidden = "GRN";
			AssertEquals("RFPProduceType - multiple values", "MULTI", product.RFPProduceType);
		}

		public void TestRFPProduct()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Pre-condition: RFPProduct", "", product.RFPProduct);

			pivot1.AddInfo.ZA_AQISProduct_Hidden = "AMF";
			AssertEquals("RFPProduct", "AMF", product.RFPProduct);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.AddInfo.ZA_AQISProduct_Hidden = "AMF";
			AssertEquals("RFPProduct - both classifications have same value", "AMF", product.RFPProduct);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot3.AddInfo.ZA_AQISProduct_Hidden = "LWB";
			AssertEquals("RFPProduct - multiple values", "MULTI", product.RFPProduct);
		}

		public void TestRFPSupplementaryCode()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Pre-condition: RFPSupplementaryCode", "", product.RFPSupplementaryCode);

			pivot1.AddInfo.ZA_AQISSupplementaryCode_Hidden = "DM";
			AssertEquals("RFPSupplementaryCode", "DM", product.RFPSupplementaryCode);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.AddInfo.ZA_AQISSupplementaryCode_Hidden = "DM";
			AssertEquals("RFPSupplementaryCode - both classifications have same value", "DM", product.RFPSupplementaryCode);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot3.AddInfo.ZA_AQISSupplementaryCode_Hidden = "GR";
			AssertEquals("RFPSupplementaryCode - multiple values", "MULTI", product.RFPSupplementaryCode);
		}

		public void TestRFPPackType()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Pre-condition: RFPPackType", "", product.RFPPackType);

			pivot1.AddInfo.ZA_AQISPackType_Hidden = "BB";
			AssertEquals("RFPPackType", "BB", product.RFPPackType);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.AddInfo.ZA_AQISPackType_Hidden = "BB";
			AssertEquals("RFPPackType - both classifications have same value", "BB", product.RFPPackType);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot3.AddInfo.ZA_AQISPackType_Hidden = "BG";
			AssertEquals("RFPPackType - multiple values", "MULTI", product.RFPPackType);
		}

		public void TestRFPPreservation()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Pre-condition: RFPPreservation", "", product.RFPPreservation);

			pivot1.AddInfo.ZA_AQISPreservation_Hidden = "C";
			AssertEquals("RFPPreservation", "C", product.RFPPreservation);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.AddInfo.ZA_AQISPreservation_Hidden = "C";
			AssertEquals("RFPPreservation - both classifications have same value", "C", product.RFPPreservation);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot3.AddInfo.ZA_AQISPreservation_Hidden = "X";
			AssertEquals("RFPPreservation - multiple values", "MULTI", product.RFPPreservation);
		}

		public void TestRFPCutCode()
		{
			var product = Factory.New<AUOrgSupplierPart>();
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Pre-condition: RFPCutCode", "", product.RFPCutCode);

			pivot1.AddInfo.ZA_AQISCutCode_Hidden = "55";
			AssertEquals("RFPCutCode", "55", product.RFPCutCode);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.AddInfo.ZA_AQISCutCode_Hidden = "55";
			AssertEquals("RFPCutCode - both classifications have same value", "55", product.RFPCutCode);

			var pivot3 = product.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot3.AddInfo.ZA_AQISCutCode_Hidden = "66";
			AssertEquals("RFPCutCode - multiple values", "MULTI", product.RFPCutCode);
		}

		#region Implementation

		AUOrgSupplierPart part;
		Classification exportClass;
		Classification importClass;
		const string PartNum = "XXX111";
		const string ImportTariff = "00000000";
		const string ExportTariff = "99990000";

		protected override void SetUp()
		{
			base.SetUp();
			importClass = Factory.New<Classification>();
			SetValuesToClassification(importClass, JobDeclaration.ClassificationType.IMP);

			exportClass = Factory.New<Classification>();
			SetValuesToClassification(exportClass, JobDeclaration.ClassificationType.EXP);
			Factory.Save();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "Test Consignor";
			consignor.MainAddress.OA_Address1 = "Test Address 1";
			consignor.OH_RL_NKClosestPort = "AUSYD";

			part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = PartNum;
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = consignor.PK;
			part.OP_StockKeepingUnit = "BO";
			part.AddNewImportPivotWithClassification(importClass.PK).CI_AddInfo = "ORG=AU";
			part.AddNewExportPivotWithClassification(exportClass.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var part = factory.NewWithValidTestData<AUOrgSupplierPart>();
			var supplier = OrgHeader.New(factory);
			supplier.OH_FullName = "Delete test Supplier";
			supplier.MainAddress.OA_Address1 = "Delete Address 1";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var partRelation = part.RelatedOrganisations.AddNew();
			partRelation.OU_OH = supplier.PK;

			return part;
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

		void SetValuesToClassification(Classification @class, string classType)
		{
			SetValuesToClassification(@class, classType, classType.Equals(JobDeclaration.ClassificationType.EXP) ? ExportTariff : ImportTariff);
		}

		#endregion
	}
}
