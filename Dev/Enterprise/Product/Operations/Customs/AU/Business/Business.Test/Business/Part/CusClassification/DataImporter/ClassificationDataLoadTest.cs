using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ClassificationDataLoad))]
	sealed class ClassificationDataLoadTest : MasterFiles.Business.Testing.DataLoadTestCase<ClassificationDataLoad>
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			loader.ImportClassificationData("non-existant file");
		}

		public void TestValidationOfHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("Invalid Header Info");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				AssertEquals(1, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
				AssertEquals("IsFileHeaderValid", false, loader.FileHeaderIsValid);
			}
		}

		public void TestValidationOfContent()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
					sw.WriteLine("This is invalid data.");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
			}
		}

		public void TestImportExcelClassifications()
		{
			ClearClassificationRecordsBeforeTesting();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "64051000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "01011091", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "27076000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");

			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0306130042", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "6205900054", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "6403200030", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054, , , ");
					sw.WriteLine("Lookup with concession,IMP,Lookup Description - code with concession,6205900054,218,TC1,833318902");
					sw.WriteLine("Sports Shoes,IMP,Tennis shoes,6403.20.00 30,,,");
					sw.WriteLine("Shoes,EXP,Leather Tennis shoes,64051000,,,");
					sw.WriteLine("Lookup with no desc1,EXP,,64051000,,,");
					sw.WriteLine("Lookup with no desc2,EXP,,01011091,,,");
					sw.WriteLine("Lookup with no desc3,EXP,,2707.60.00,,,");
					sw.WriteLine("Lookup with no desc4,IMP,,0306130042,,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(Classification), checkFilter);
				AssertEquals("There should have been 8 Classification records created", 8, enterpriseClassificationsCreated.Length);
				AssertEquals(9, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(8, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);

				Classification testClass = LoadClassification("Lookup with concession");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Lookup Description - code with concession", testClass.CC_Description);
				AssertEquals("Tariff Number", "6205.90.00 54", testClass.CC_TariffNum);
				AssertEquals("Class. Type", Classification.ClassificationType.IMP, testClass.CC_ClassificationType);
				AssertContains("Add Info - Treatment", "TreatmentCode_Hidden=218", testClass.CC_AddInfo);
				AssertContains("Add Info - Instrument", "InstrumentType_Hidden=TC1", testClass.CC_AddInfo);
				AssertContains("Add Info - Concession", "InstrumentCode_Hidden=83331890", testClass.CC_AddInfo);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("Shoes");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Leather Tennis shoes", testClass.CC_Description);
				AssertEquals("Tariff Number", "6405.10.00", testClass.CC_TariffNum);
				AssertEquals("Class. Type", Classification.ClassificationType.EXP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("Sports Shoes");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Tennis shoes", testClass.CC_Description);
				AssertEquals("Tariff Number", "6403.20.00 30", testClass.CC_TariffNum);
				AssertEquals("Class. Type", Classification.ClassificationType.IMP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("Lookup with no desc1");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);

				testClass = LoadClassification("Lookup with no desc2");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);
				Assert("Classification Description should not be Other", testClass.CC_Description != "Other");

				testClass = LoadClassification("Lookup with no desc3");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);

				testClass = LoadClassification("Lookup with no desc4");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);
				Assert("Classification Description should not be Other", testClass.CC_Description != "Other");
			}
		}

		public void TestImportExcelClassifications_AHECC()
		{
			ClearClassificationRecordsBeforeTesting();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054, , , ");
					sw.WriteLine("Lookup with concession,IMP,Lookup Description - code with concession,6205900054,218,TC1,833318902");
					sw.WriteLine("Sports Shoes,IMP,Tennis shoes,6403.20.00 30,,,");
					sw.WriteLine("Shoes,EXP,Leather Tennis shoes,64051000,,,");
					sw.WriteLine("Lookup with no desc1,EXP,,64051000,,,");
					sw.WriteLine("Lookup with no desc2,EXP,,01011091,,,");
					sw.WriteLine("Lookup with no desc3,EXP,,2707.60.00,,,");
					sw.WriteLine("Lookup with no desc4,IMP,,0306130042,,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(Classification), checkFilter);
				AssertEquals("There should have been 8 Classification records created", 8, enterpriseClassificationsCreated.Length);
				AssertEquals(9, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(8, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(0, loader.RunCounters.RecsExcluded);
				AssertEquals(2, loader.Log.Count);

				Classification testClass = LoadClassification("Lookup with concession");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Lookup Description - code with concession", testClass.CC_Description);
				AssertEquals("Tariff Number", "6205.90.00 54", testClass.CC_TariffNum);
				AssertEquals("Class. Type", Classification.ClassificationType.IMP, testClass.CC_ClassificationType);
				AssertContains("Add Info - Treatment", "TreatmentCode_Hidden=218", testClass.CC_AddInfo);
				AssertContains("Add Info - Instrument", "InstrumentType_Hidden=TC1", testClass.CC_AddInfo);
				AssertContains("Add Info - Concession", "InstrumentCode_Hidden=83331890", testClass.CC_AddInfo);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("Shoes");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Leather Tennis shoes", testClass.CC_Description);
				AssertEquals("Tariff Number", "6405.10.00", testClass.CC_TariffNum);
				AssertEquals("Class. Type", Classification.ClassificationType.EXP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("Sports Shoes");
				AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, testClass.CC_RN_NKCountryCode);
				AssertEquals("Classification Description", "Tennis shoes", testClass.CC_Description);
				AssertEquals("Tariff Number", "6403.20.00 30", testClass.CC_TariffNum);
				AssertEquals("Class. Type", Classification.ClassificationType.IMP, testClass.CC_ClassificationType);
				Assert("Add Info", testClass.CC_AddInfo.IsEmpty);
				Assert(testClass.CC_IsActive);

				testClass = LoadClassification("Lookup with no desc1");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);

				testClass = LoadClassification("Lookup with no desc2");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);
				Assert("Classification Description should not be Other", testClass.CC_Description != "Other");

				testClass = LoadClassification("Lookup with no desc3");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);

				testClass = LoadClassification("Lookup with no desc4");
				Assert("Classification Description should have been generated", !testClass.CC_Description.IsEmpty);
				Assert("Classification Description should not be Other", testClass.CC_Description != "Other");
			}
		}

		public void TestImportClassificationsWithInconsistentData()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054, , , ");
					sw.WriteLine("Sports Shoes,IMP,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(Classification), checkFilter);
				AssertEquals("There should have only been 1 Classification record created", 1, enterpriseClassificationsCreated.Length);
				AssertEquals(3, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(1, loader.RunCounters.RecsExcluded);
				AssertEquals(3, loader.Log.Count);
			}
		}

		public void TestClassificationIsNotCreatedWhenItAlreadyExists()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054,,,");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054,,,");
					sw.WriteLine("TEST,IMP,Test Lookup,6205900054,,,");
					sw.Flush();
				}

				loader.ImportClassificationData(testFileName.Filename);
				ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
				BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(Classification), checkFilter);
				AssertEquals("There should have only been 1 Classification record created", 1, enterpriseClassificationsCreated.Length);
				AssertEquals(4, loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(1, loader.RunCounters.RecsCreated);
				AssertEquals(0, loader.RunCounters.RecsUpdated);
				AssertEquals(2, loader.RunCounters.RecsExcluded);
				AssertEquals(4, loader.Log.Count);
				Assert(loader.Log.Any(entry => entry == "Row 3: Record Excluded - Classification 'TEST' already exists in CargoWise One"));
			}
		}

		public void TestClassificationIsNotCreatedWhenDataHasError()
		{
			ClearClassificationRecordsBeforeTesting();
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
					sw.WriteLine("TEST,IMP,Test Lookup,AAAA,,,");
					sw.Flush();
				}

				AssertInvalidDataIsNotLoaded(testFileName.Filename);
			}
		}

		void AssertInvalidDataIsNotLoaded(string testFilePath)
		{
			loader.ImportClassificationData(testFilePath);
			ZQuery checkFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.GreaterThan, "");
			BusinessObject[] enterpriseClassificationsCreated = Factory.Load(typeof(Classification), checkFilter);
			AssertEquals("There should have been no Classification records created", 0, enterpriseClassificationsCreated.Length);
			AssertEquals(2, loader.RunCounters.RecordsToImportPlusHeader);
			AssertEquals(0, loader.RunCounters.RecsCreated);
			AssertEquals(0, loader.RunCounters.RecsUpdated);
			AssertEquals(1, loader.RunCounters.RecsExcluded);
			AssertEquals(3, loader.Log.Count);
			Assert(loader.Log.Any(entry => entry == "Row 2: Record Excluded - Tariff values provided are not valid in CargoWise One"));
		}

		public void TestProcessDataForThisLineInconsistentDataMessage()
		{
			var loader2 = new ClassificationDataLoad();
			loader2.RunCounters.CurrentRow = 99;
			loader2.ProcessDataForThisLineInternal(new OCsvLine("Test Data"));
			AssertEquals(1, loader2.Log.Count);
			AssertEquals("Row 99 excluded... data is inconsistent with required format.", loader2.Log[0]);

			var loader3 = new ClassificationDataLoad();
			AssertEquals("Preconditions: No Log Expected", 0, loader3.Log.Count);
			loader3.ProcessDataForThisLineInternal(new OCsvLine("111,EXP,DESC,TARIFF"));
			AssertEquals("Four columns should be revised to seven and no error expected.", false, loader3.Log[0].Contains("is inconsistent with required format."));

			var loader4 = new ClassificationDataLoad();
			AssertEquals("Preconditions: No Log Expected", 0, loader4.Log.Count);
			loader4.ProcessDataForThisLineInternal(new OCsvLine("111,EXP,DESC,TARIFF,Col5,Col6,Col7,Col8"));
			AssertEquals("More than Seven columns is not acceptable and the line would not be revised.", "Row 1 excluded... data is inconsistent with required format.", loader4.Log[0]);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			loader = new ClassificationDataLoad();
		}

		protected override ClassificationDataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		ClassificationDataLoad loader;

		void ClearClassificationRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable("JobComInvoiceLine");
			TestCaseHelper.ClearTable("CusClassPartPivot");
			TestCaseHelper.ClearTable("CusClassification");
		}

		Classification LoadClassification(ZString lookupCode)
		{
			ZQuery filter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			var testClassification = Factory.LoadTop1<Classification>(filter);
			AssertNotNull("Expecting Classification " + lookupCode + " to be found", testClassification);

			return testClassification;
		}

		#endregion
	}
}
