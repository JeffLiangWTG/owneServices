using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Test.Utilities.GenericImportExport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.DataMapping.ImportExportWizard;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ImportWizard))]
	sealed class ImportWizardTest : ImportExportWizardTest
	{
		class DummyObjectWithoutInfoProperty : DummyChildBusinessObject
		{
			public DummyObjectWithoutInfoProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString PropertyWithoutInfo { get; set; }
		}

		class DummyNonPersistentBusinessObjectCollectionForTest : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObjectForTest>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObjectForTest();
			}
		}

		class DummyNonPersistentBusinessObjectForTest : DummyNonPersistentBusinessObject
		{
			public bool Code_ReadOnly { get; set; }
		}

		public void TestGetMappedFieldValue_FileColumnIndexOrderContainsValidIndex()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "AAA", "BBB", "CCC", "DDD" } });

			var mapping1 = wizard.Object.Mapping[1]; //Z0_VarCharMax
			mapping1.Delimiter = ";";
			mapping1.AddFileColumnIndex(2);
			mapping1.AddFileColumnIndex(4);
			mapping1.AddFileColumnIndex(0);
			mapping1.AddFileColumnIndex(1);
			mapping1.AddFileColumnIndex(3);
			mapping1.AddFileColumnIndex(5);

			var mapping2 = wizard.Object.Mapping[6]; //Z0_Description
			mapping2.Delimiter = ";";
			mapping2.AddFileColumnIndex(4);

			var collection = new DummyChildBusinessObjectCollection(Factory);
			AssertNoExceptionThrown(() => wizard.Object.ImportIntoCollection(collection, -1));
			AssertEquals("Z0_VarCharMax", mapping1.MappingName);
			AssertEquals("Z0_Description", mapping2.MappingName);
			AssertEquals(1, collection.Count);
			AssertEquals("CCC;AAA;BBB;DDD", collection[0].Z0_VarCharMax);
			AssertEquals("", collection[0].Z0_Description);
		}

		public void TestGetSettings_FileColumnIndexOrder()
		{
			var wizard = new ImportWizard(Helper.CollectionInfo, null, new FileMapperForTest());
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[0].AddFileColumnIndex(1);
			var setting = (DataImportWizardSettings)wizard.GetSettings();

			AssertEquals(2, setting.Mappings[0].FileColumnIndexOrder.Count);
			AssertEquals(0, setting.Mappings[0].FileColumnIndexOrder[0]);
			AssertEquals(1, setting.Mappings[0].FileColumnIndexOrder[1]);
			AssertEquals(SupportedDelimiter.Space, setting.Mappings[0].Delimiter);
		}

		public void TestSetSettings_FileColumnIndexOrder()
		{
			var wizard1 = GetImportWizardForTest(new DataImportWizardMappingSetting() { MappingName = "Z0_VarCharMax", FileColumnIndex = 1 });
			AssertEquals(1, wizard1.Mapping[1].FileColumnIndexOrder.Count);
			AssertEquals(1, wizard1.Mapping[1].FileColumnIndexOrder[0]);

			var wizard2 = GetImportWizardForTest(new DataImportWizardMappingSetting() { MappingName = "Z0_VarCharMax", FileColumnIndex = 1, FileColumnIndexOrder = [2] });
			AssertEquals(1, wizard2.Mapping[1].FileColumnIndexOrder.Count);
			AssertEquals(2, wizard2.Mapping[1].FileColumnIndexOrder[0]);

			var wizard3 = GetImportWizardForTest(new DataImportWizardMappingSetting() { MappingName = "Z0_VarCharMax", FileColumnIndex = 1, FileColumnIndexOrder = [1, 2] });
			AssertEquals(2, wizard3.Mapping[1].FileColumnIndexOrder.Count);
			AssertEquals(1, wizard3.Mapping[1].FileColumnIndexOrder[0]);
			AssertEquals(2, wizard3.Mapping[1].FileColumnIndexOrder[1]);

			ImportWizard GetImportWizardForTest(DataImportWizardMappingSetting mappingSetting)
			{
				var wizard = new ImportWizard(Helper.CollectionInfo, null, new FileMapperForTest());
				var setting = new DataImportWizardSettings { CustomMapLists = [], Mappings = [mappingSetting] };
				wizard.SetSettings(setting);
				return wizard;
			}
		}

		public void TestImportIntoCollection_DynamicReadOnlyProperty()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]> { new[] { "AAA", "Description" } });

			wizard.Object.Mapping[1].AddFileColumnIndex(0); //Z0_VarCharMax
			wizard.Object.Mapping[6].AddFileColumnIndex(1); //Z0_Description

			var collection = new DummyCollection4(Factory);
			wizard.Object.ImportIntoCollection(collection, -1);
			AssertEquals(1, collection.Count);
			AssertEquals("AAA", collection[0].Z0_VarCharMax);
			AssertEquals("Description", collection[0].Z0_Description);
		}

		public void TestImportIntoCollection_MultipleFiledToSaveColumn()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]>() { new string[] { "1", "AAA", "BBB", "CCC" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[1].AddFileColumnIndex(2); //Z0_VarCharMax
			wizard.Object.Mapping[1].AddFileColumnIndex(3); //Z0_VarCharMax

			var collection1 = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.ImportIntoCollection(collection1);
			AssertEquals(1, collection1.Count);
			AssertEquals(1, collection1[0].Z0_Number);
			AssertEquals("AAA BBB CCC", collection1[0].Z0_VarCharMax);

			wizard.Object.Mapping[1].ClearAllFileColumnIndex();
			wizard.Object.Mapping[1].AddFileColumnIndex(2); //Z0_VarCharMax
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[1].AddFileColumnIndex(3); //Z0_VarCharMax

			var collection2 = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.ImportIntoCollection(collection2);
			AssertEquals(1, collection2.Count);
			AssertEquals(1, collection2[0].Z0_Number);
			AssertEquals("BBB AAA CCC", collection2[0].Z0_VarCharMax);

			wizard.Object.Mapping[1].ClearAllFileColumnIndex();
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[1].AddFileColumnIndex(2); //Z0_VarCharMax
			wizard.Object.Mapping[1].AddFileColumnIndex(3); //Z0_VarCharMax
			wizard.Object.Mapping[1].Delimiter = SupportedDelimiter.Newline;

			var collection3 = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.ImportIntoCollection(collection3);
			AssertEquals(1, collection3.Count);
			AssertEquals(1, collection3[0].Z0_Number);
			AssertEquals("AAA\r\nBBB\r\nCCC", collection3[0].Z0_VarCharMax);
		}

		public void TestShouldHasWarningsWithNonPersistentBusinessObjectCollection()
		{
			var testCollection = new DummyNonPersistentBusinessObjectCollectionForTest();
			var collectionInfo = new ImportCollectionInfoImpl(testCollection) { new ImportPropertyInfoImpl<DummyNonPersistentBusinessObjectForTest>("Code") { HeaderText = "Code" }, };
			var settingsStorageStub = new Mock<ISettingsStorage>();
			settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(Array.Empty<string>());

			var wizard = new ImportWizard(collectionInfo, settingsStorageStub.Object, new FileMapperForTest());

			wizard.GenerateReadOnlyWarnings();
			AssertEquals("there should be a warning.", 1, wizard.Mapping.GetWarnings().Count());
			AssertEquals("Warning - record: This field could be read only. Values for this field possibly may not be imported.", wizard.Mapping.GetWarnings().First().Message);
		}

		public void TestMatchForUpdateColumns()
		{
			var testWizard = Helper.Wizard;
			AssertEquals(0, testWizard.MatchForUpdateColumns.Count());
			var importMaping = testWizard.Mapping.OfType<ImportWizardMapping>().First();
			importMaping.AddFileColumnIndex(1);
			importMaping.UpdateExisting = true;
			AssertEquals(1, testWizard.MatchForUpdateColumns.Count());
		}

		public void TestUpdateExisting()
		{
			var newFactory = new BusinessObjectFactory();
			var bizo1 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_VarCharMax = "AAA";
			var bizo2 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo2.Z0_VarCharMax = "BBB";
			var bizo3 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo3.Z0_VarCharMax = "CCC";
			newFactory.Save();

			var count = newFactory.Load<DummyBusinessObject>(
				new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "")).Length;

			var collectionInfo = (ImportCollectionInfoImpl)Helper.CollectionInfo;
			collectionInfo.Add(new ImportPropertyInfoImpl<DummyObjectWithoutInfoProperty>("PropertyWithoutInfo", (BusinessObject bizObj) => new DummyChildBusinessObjectCollection(bizObj.Factory)) { HeaderText = "Invalid" });

			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAA", "Y", "12", "43.3", "2011-3-10", "value" },
				new string[] { "2", "BBB", "N", "13", "43.4", "2011-3-11", "value2" },
				new string[] { "99", "CCC", "Y", "14", "43.5", "2011-3-12", "value3" },
				new string[] { "3", "CCC", "Y", "14", "43.5", "2011-3-12", "value3" }, //testing later values override previous ones successfully
				new string[] { "4", "DDD", "N", "15", "43.6", "2011-3-13", "value4" } }); //matches nothing, don't import
																						  //then set it up the same way for preview's 1, 50
			wizard.Setup(m => m.LoadFile(1, 50, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAA", "Y", "12", "43.3", "2011-3-10", "value" },
				new string[] { "2", "BBB", "N", "13", "43.4", "2011-3-11", "value2" },
				new string[] { "99", "CCC", "Y", "14", "43.5", "2011-3-12", "value3" },
				new string[] { "3", "CCC", "Y", "14", "43.5", "2011-3-12", "value3" }, //testing later values override previous ones successfully
				new string[] { "4", "DDD", "N", "15", "43.6", "2011-3-13", "value4" } }); //matches nothing, don't import

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //Z0_Bool
			wizard.Object.Mapping[3].AddFileColumnIndex(3); //Z0_Short
			wizard.Object.Mapping[4].AddFileColumnIndex(4); //Z0_AnotherDecimal
			wizard.Object.Mapping[5].AddFileColumnIndex(5); //Z0_Date
			wizard.Object.Mapping[6].AddFileColumnIndex(6); //PropertyWithoutInfo

			wizard.Object.Mapping[1].UpdateExisting = true;

			DummyChildBusinessObjectCollection collection1 = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection1.Load(new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, ""));

			wizard.Object.LoadPreview();
			AssertEquals("Preview is correct size", 3, wizard.Object.Preview.Count);

			AssertNoExceptionThrown("PropertyWithoutInfo exception should be ignored", () => wizard.Object.ImportIntoCollection(collection1, -1));
			Factory.Save();
			AssertEquals("correct number of rows imported/updated", 3, collection1.Count);
			AssertContainsExactElementsInAnyOrder("correct Z0_VarCharMax values", collection1.Select(x => x.Z0_VarCharMax), new string[] { "AAA", "BBB", "CCC" });
			AssertEquals("No new rows", count, new BusinessObjectFactory().Load<DummyBusinessObject>(
				new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "")).Length);
			AssertEquals("PK of AAA was reused and number was updated", 1, collection1.Where(x => x.PK == bizo1.PK).First().Z0_Number);
			AssertEquals("PK of BBB was reused and number was updated", 2, collection1.Where(x => x.PK == bizo2.PK).First().Z0_Number);
			AssertEquals("PK of CCC was reused and number was updated", 3, collection1.Where(x => x.PK == bizo3.PK).First().Z0_Number);
		}

		public void TestUpdateExisting_2()
		{
			var newFactory = new BusinessObjectFactory();
			var bizo1 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_VarCharMax = "AAA";
			bizo1.Z0_Short = 123;
			var bizo2 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo2.Z0_VarCharMax = "AAA";
			bizo2.Z0_Short = 123;
			var bizo3 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo3.Z0_VarCharMax = "AAA";
			bizo3.Z0_Short = 234;
			var bizo4 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo4.Z0_VarCharMax = "BBB";
			bizo4.Z0_Short = 234;
			var bizo5 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			bizo5.Z0_VarCharMax = "CCC";
			bizo5.Z0_Short = 234;
			newFactory.Save();

			var count = newFactory.Load<DummyBusinessObject>(
				new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "")).Length;

			var collectionInfo = (ImportCollectionInfoImpl)Helper.CollectionInfo;
			collectionInfo.Add(new ImportPropertyInfoImpl<DummyObjectWithoutInfoProperty>("PropertyWithoutInfo", (BusinessObject bizObj) => new DummyChildBusinessObjectCollection(bizObj.Factory)) { HeaderText = "Invalid" });

			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAA", "Y", "123", "43.3", "2011-3-10", "value" }, //matches two rows
				new string[] { "2", "BBB", "N", "234", "43.4", "2011-3-11", "value2" }, //matches one row
				new string[] { "3", "CCC", "Y", "345", "43.5", "2011-3-12", "value3" }, //matches no rows
				});

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //Z0_Bool
			wizard.Object.Mapping[3].AddFileColumnIndex(3); //Z0_Short
			wizard.Object.Mapping[4].AddFileColumnIndex(4); //Z0_AnotherDecimal
			wizard.Object.Mapping[5].AddFileColumnIndex(5); //Z0_Date
			wizard.Object.Mapping[6].AddFileColumnIndex(6); //PropertyWithoutInfo

			wizard.Object.Mapping[1].UpdateExisting = true;
			wizard.Object.Mapping[3].UpdateExisting = true;

			DummyChildBusinessObjectCollection collection1 = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection1.Load(new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, ""));
			AssertNoExceptionThrown("PropertyWithoutInfo exception should be ignored", () => wizard.Object.ImportIntoCollection(collection1, -1));
			Factory.Save();
			AssertEquals("correct number of rows imported/updated", 5, collection1.Count);
			bizo1.Reload();
			AssertEquals("updated due to AAA/123 match", 1, bizo1.Z0_Number);
			bizo2.Reload();
			AssertEquals("updated due to AAA/123 match", 1, bizo2.Z0_Number);
			bizo3.Reload();
			AssertEquals("no matches", 0, bizo3.Z0_Number);
			bizo4.Reload();
			AssertEquals("updated due to BBB/234 match", 2, bizo4.Z0_Number);
			bizo5.Reload();
			AssertEquals("no matches", 0, bizo5.Z0_Number);
			AssertContainsExactElementsInAnyOrder("correct Z0_VarCharMax values", collection1.Select(x => x.Z0_VarCharMax), new string[] { "AAA", "AAA", "AAA", "BBB", "CCC" });
			AssertContainsExactElementsInAnyOrder("correct Z0_VarCharMax values", collection1.Select(x => x.Z0_Short), new short[] { 123, 123, 234, 234, 234 });
			AssertContainsExactElementsInAnyOrder("correct Z0_Number values", collection1.Select(x => x.Z0_Number), new int[] { 0, 0, 1, 1, 2 });
			AssertEquals("No new rows", count, new BusinessObjectFactory().Load<DummyBusinessObject>(
				new ZQuery(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "")).Length);
		}

		public void TestImportCustomizedFeildsIntoCollection()
		{
			var collection = new CustomDummyChildBusinessObjectCollection(Factory);
			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<CustomDummyChildBusinessObject>("__customFieldString__prop__ZString") { HeaderText = "Txt" },
				new ImportPropertyInfoImpl<CustomDummyChildBusinessObject>("__customFieldDecimal__prop__ZDecimal") { HeaderText = "Decimal" },
				new ImportPropertyInfoImpl<CustomDummyChildBusinessObject>("__customFieldDatetime__prop__ZDateTime") { HeaderText = "Date" },
				new ImportPropertyInfoImpl<CustomDummyChildBusinessObject>("__customFieldInteger__prop__ZInt") { HeaderText = "Num" },
				new ImportPropertyInfoImpl<CustomDummyChildBusinessObject>("__customFieldBoolean__prop__ZBool") { HeaderText = "Bool" }
					};
			var wizard = new Mock<ImportWizard>(collectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
			 .Returns(new List<string[]>() { new string[] { "STRING1", "0.123", "2019-07-12", "10", "Y" }, new string[] { "", "", "", "", "" }, new string[] { "STRING2", "99.999", "2019-07-13", "11", "N" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //__customFieldString__prop__ZString
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //__customFieldDecimal__prop__ZDecimal
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //__customFieldDatetime__prop__ZDateTime
			wizard.Object.Mapping[3].AddFileColumnIndex(3); //__customFieldInteger__prop__ZInt
			wizard.Object.Mapping[4].AddFileColumnIndex(4); //__customFieldBoolean__prop__ZBool

			wizard.Object.ImportIntoCollection(collection, -1);
			AssertEquals(3, collection.Count);

			AssertCustomBusinessObjectResult(collection[0], "STRING1", 0.12m, new ZDateTime(2019, 07, 12), 10, true); //0.12m not 0.123m due to the 2 decimal places
			AssertCustomBusinessObjectResult(collection[1], ZString.Empty, ZDecimal.Zero, ZDateTime.Empty, ZInt.Zero, ZBool.False);
			AssertCustomBusinessObjectResult(collection[2], "STRING2", 100m, new ZDateTime(2019, 07, 13), 11, ZBool.False); //100m not 99.999m due to the 2 decimal palces
		}

		void AssertCustomBusinessObjectResult(ICustomFieldProvider cusObject, ZString stringValue, ZDecimal decimalValue, ZDateTime datetimeValue, ZInt intValue, ZBool boolValue)
		{
			var customBusinessObject = cusObject.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
			AssertEquals(stringValue, customBusinessObject["__customFieldString__prop__ZString"]);
			AssertEquals(decimalValue, customBusinessObject["__customFieldDecimal__prop__ZDecimal"]);
			AssertEquals(datetimeValue, customBusinessObject["__customFieldDatetime__prop__ZDateTime"]);
			AssertEquals(intValue, customBusinessObject["__customFieldInteger__prop__ZInt"]);
			AssertEquals(boolValue, customBusinessObject["__customFieldBoolean__prop__ZBool"]);
		}

		public void TestValidateMappingBeforeLoadPreview()
		{
			var testWizard = Helper.Wizard;
			Assert(!testWizard.Mapping.HasErrors());
			testWizard.LoadPreview();
			Assert(testWizard.Mapping.HasErrors());
			AssertEquals("Error - MappedField: Please map Z0_Guid before importing", testWizard.Mapping.GetErrors().First().Message);
		}

		public void TestLoadPreview_WithTruncatedFile()
		{
			var testWizard = Helper.Wizard;
			var xlsContents =
				new object[,] {
					{ "Text", "Number", "Date" },
					{ "Test1", 100, new DateTime(2009, 2, 1).ToOADate() },
					{ "Test2", 100.50, new DateTime(2008, 3, 1).ToOADate() },
					{ "", "", "" },
				};
			var xlsContentFormats =
				new string[,] {
					{ null, null, "d/mm/yyyy" },
					{ null, null, "d/mm/yyyy" },
					{ null, null, "d/mm/yyyy h:mm" },
					{ null, null, "d/mm/yyyy h:mm" },
				};

			try
			{
				using (TempFile file = CreateTestXls(xlsContents, xlsContentFormats))
				{
					testWizard.FileName = file.Filename;

					Stream wizardStream = testWizard.GetFileStream(true);
					wizardStream.SetLength(wizardStream.Length / 2);
					testWizard.LoadPreview();
				}
			}
			catch (Exception ex)
			{
				ExceptionReporter.Instance.ReportException("TestLoadPreview_WithTruncatedFile", ex);
				AssertEquals("The user should've been notified of the corruption exception", ExcelInterfaceExceptionBase.FileCorruptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDelimiters()
		{
			var collectionInfo = Helper.CollectionInfo;
			AssertEquals("Delimiters Count", 7, helper.Wizard.Delimiters.Count);
			AssertContains("Delimiters elements", ",, ~, |, space, tab, :, ;", helper.Wizard.Delimiters.CodesAsString);
		}

		public void TestFactoryShouldNotBeTheSameAsCollectionFactory()
		{
			var collectionInfo = Helper.CollectionInfo;
			AssertNotEquals("The Wizard should not have the same factory as the Collection as the system should be able to GC the wizard when it's out of scope", Helper.Wizard.Factory, collectionInfo.Collection.Factory);
		}

		public void TestGetFileStream()
		{
			ImportWizard wizard = Helper.Wizard;

			string tmpFilePath = Temp.GetTempFileName();

			try
			{
				wizard.FileName = tmpFilePath;
				Stream stream1 = wizard.CurrentFileStream;
				Stream stream2 = wizard.GetFileStream(true);
				Stream stream3 = wizard.GetFileStream(false);

				AssertNotEquals(stream1, stream2);
				AssertEquals(stream2, stream3);
			}
			finally
			{
				File.Delete(tmpFilePath);
			}
		}

		public void TestFactoryIsSetBasedOnValidateAndSave()
		{
			IImportCollectionInfo importInfo = new ValidateAndSaveImportCollectionInfo<DummyBusinessObjectWithForeignKey>();
			AssertEquals(true, importInfo.ValidateAndSave);
			var wizard = new ImportWizard(importInfo, null, new FileMapperForTest());
			AssertNotNull(wizard.Factory);
			AssertEquals(importInfo.Collection.Factory, wizard.Factory);

			importInfo = new ImportCollectionInfoImpl(importInfo.Collection);
			AssertEquals(false, importInfo.ValidateAndSave);
			wizard = new ImportWizard(importInfo, null, new FileMapperForTest());
			AssertNull(wizard.Factory);
		}

		public void TestImportIntoCollection_HandlingOfInvalidColumns()
		{
			var collectionInfo = (ImportCollectionInfoImpl)Helper.CollectionInfo;
			collectionInfo.Add(new ImportPropertyInfoImpl<DummyObjectWithoutInfoProperty>("PropertyWithoutInfo", (BusinessObject bizObj) => new DummyChildBusinessObjectCollection(bizObj.Factory)) { HeaderText = "Invalid" });

			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAA", "Y", "12", "43.3", "2011-3-10", "value" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //Z0_Bool
			wizard.Object.Mapping[3].AddFileColumnIndex(3); //Z0_Short
			wizard.Object.Mapping[4].AddFileColumnIndex(4); //Z0_AnotherDecimal
			wizard.Object.Mapping[5].AddFileColumnIndex(5); //Z0_Date
			wizard.Object.Mapping[6].AddFileColumnIndex(6); //PropertyWithoutInfo

			DummyChildBusinessObjectCollection collection1 = new DummyChildBusinessObjectCollection(Factory);
			AssertNoExceptionThrown("PropertyWithoutInfo exception should be ignored", () => wizard.Object.ImportIntoCollection(collection1, -1));
			AssertEquals(1, collection1.Count);
			var dummy = collection1[0];
			AssertEquals(1, dummy.Z0_Number);
			AssertEquals("AAA", dummy.Z0_VarCharMax);
			AssertEquals(ZBool.True, dummy.Z0_Bool);
			AssertEquals((short)12, dummy.Z0_Short);
			AssertEquals(43.3m, dummy.Z0_AnotherDecimal);
			AssertEquals(new ZDate(2011, 3, 10), dummy.Z0_Date);

			AssertEquals(false, collection1.IsValidationSuspended);
			AssertEquals(false, collection1.Factory.IsValidationSuspended);
			AssertEquals(false, dummy.IsValidationSuspended);

			SupportDataImportingBizObjCollection collection2 = new SupportDataImportingBizObjCollection(Factory);
			wizard.Object.ImportIntoCollection(collection2, -1);
			AssertEquals(1, collection2.Count);
			var dummy2 = collection2[0];
			AssertEquals(1, dummy2.Z0_Number);
			AssertEquals("AAA", dummy2.Z0_VarCharMax);
			AssertEquals(ZBool.True, dummy2.Z0_Bool);
			AssertEquals((short)12, dummy2.Z0_Short);
			AssertEquals(43.3m, dummy2.Z0_AnotherDecimal);
			AssertEquals(new ZDate(2011, 3, 10), dummy2.Z0_Date);
			Assert(dummy2.IsImportingDataWasSet);
			Assert(!dummy2.IsImportingData);

			AssertEquals(false, collection2.IsValidationSuspended);
			AssertEquals(false, collection2.Factory.IsValidationSuspended);
			AssertEquals(false, dummy2.IsValidationSuspended);
		}

		public void TestImportIntoCollection_ThrowWarningWhenNotWithinSqlPrecisionAndScale()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAA", "Y", "12", "1123456789345678" } }); //Z0_AnotherDecimal = 1,123,456,789,345,678

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //Z0_Bool
			wizard.Object.Mapping[3].AddFileColumnIndex(3); //Z0_Short
			wizard.Object.Mapping[4].AddFileColumnIndex(4); //Z0_AnotherDecimal has a max value of 999,999,999,999,999

			DummyChildBusinessObjectCollection collection = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.Mapping.Add(new ImportWizardMapping(wizard.Object.CollectionInfo.Properties.ElementAt(0), wizard.Object.Mapping) { Expression = "32" });
			wizard.Object.ImportIntoCollection(collection, -1);
			AssertEquals(1, collection.Count);

			var dummy = collection[0];
			AssertEquals(999999999999999m, dummy.Z0_AnotherDecimal);
			AssertEquals("there should be a warning.", 1, wizard.Object.Mapping[4].Notifications.Count());
			AssertEquals("Warning - record: Attempted to insert '1123456789345678' into Field [CargoWise.Schema.SchemaDecimalColumn] which has a maximum numeric value of '999999999999999'. Field was truncated to the max value.", wizard.Object.Mapping[4].Notifications.First().Message);
		}

		public void TestImportIntoCollectionThrowAnExceptionWhenThereIsPythonErrorMessage()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAA" }, new string[] { "2", "BBB" }, new string[] { "3", "CCC" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax

			var collection = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.Mapping.Add(new ImportWizardMapping(wizard.Object.CollectionInfo.Properties.ElementAt(0), wizard.Object.Mapping) { Expression = "32" });
			wizard.Object.ImportIntoCollection(collection, -1);
			AssertEquals(3, collection.Count);
			AssertEquals("Error - Expression: The expression you have defined for Z0_Number has the following Error: expected str, got int." + System.Environment.NewLine +
"Please refer to online documentation for IronPython or consult with an IT professional with an understanding of the IronPython programming language."
, wizard.Object.Mapping[13].Notifications.First().Message);
		}

		public void TestImportIntoCollection()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "AAΓB" }, new string[] { "2", "BBB" }, new string[] { "3", "CCC" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[1].WesternCharactersOnly = true;

			DummyChildBusinessObjectCollection collection1 = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.ImportIntoCollection(collection1, -1);
			AssertEquals(3, collection1.Count);
			AssertEquals(1, collection1[0].Z0_Number);
			AssertEquals(2, collection1[1].Z0_Number);
			AssertEquals(3, collection1[2].Z0_Number);
			AssertEquals("AAB", collection1[0].Z0_VarCharMax);
			AssertEquals("BBB", collection1[1].Z0_VarCharMax);
			AssertEquals("CCC", collection1[2].Z0_VarCharMax);

			AssertEquals(false, collection1.IsValidationSuspended);
			AssertEquals(false, collection1.Factory.IsValidationSuspended);
			AssertEquals(false, collection1[0].IsValidationSuspended);
			AssertEquals(false, collection1[1].IsValidationSuspended);
			AssertEquals(false, collection1[2].IsValidationSuspended);

			SupportDataImportingBizObjCollection collection2 = new SupportDataImportingBizObjCollection(Factory);
			wizard.Object.Mapping[1].WesternCharactersOnly = false;
			wizard.Object.ImportIntoCollection(collection2, -1);
			AssertEquals(3, collection2.Count);
			AssertEquals(1, collection2[0].Z0_Number);
			AssertEquals(2, collection2[1].Z0_Number);
			AssertEquals(3, collection2[2].Z0_Number);
			AssertEquals("AAΓB", collection2[0].Z0_VarCharMax);
			AssertEquals("BBB", collection2[1].Z0_VarCharMax);
			AssertEquals("CCC", collection2[2].Z0_VarCharMax);
			Assert(collection2[0].IsImportingDataWasSet);
			Assert(collection2[1].IsImportingDataWasSet);
			Assert(collection2[2].IsImportingDataWasSet);
			Assert(!collection2[0].IsImportingData);
			Assert(!collection2[1].IsImportingData);
			Assert(!collection2[2].IsImportingData);

			AssertEquals(false, collection2.IsValidationSuspended);
			AssertEquals(false, collection2.Factory.IsValidationSuspended);
			AssertEquals(false, collection2[0].IsValidationSuspended);
			AssertEquals(false, collection2[1].IsValidationSuspended);
			AssertEquals(false, collection2[2].IsValidationSuspended);
		}

		public void TestImportIntoCollection_ZGuid_ImportPropertyInfoImpl()
		{
			var importCollectionInfo = new ValidateAndSaveImportCollectionInfo<DummyBaseBusinessObject>();
			importCollectionInfo.Add(DummyBizoSchema.Constants.Z0_Guid);
			var wizard = new Mock<ImportWizard>(importCollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(new List<string[]>() { new string[] { "XXX" } });
			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Guid

			var collection = new DummyChildBusinessObjectCollection(Factory);
			AssertNoExceptionThrown(() => wizard.Object.ImportIntoCollection(collection));

			AssertEquals(1, collection.Count);
			AssertEquals(ZGuid.Empty, collection[0].Z0_Guid);
		}

		public void TestImportIntoCollection_ZDecimal()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "Europe Number", "3566,55" }, new string[] { "2", "US Number", "405.67" }, new string[] { "3", "No decimal place", "700" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[4].AddFileColumnIndex(2); //Z0_AnotherDecimal

			DummyChildBusinessObjectCollection collection = new DummyChildBusinessObjectCollection(Factory);
			wizard.Object.ImportIntoCollection(collection, -1);
			AssertEquals(3, collection.Count);
			AssertEquals(356655M, collection[0].Z0_AnotherDecimal);
			AssertEquals(405.67M, collection[1].Z0_AnotherDecimal);
			AssertEquals(700M, collection[2].Z0_AnotherDecimal);
		}

		public void TestImportIntoCollection_ZDecimal_UseCurrentCountryNumberFormatting()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Object.UseCurrentCountryNumberFormatting = true;

			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "1", "Europe Number", "3566,55" }, new string[] { "2", "US Number", "405.67" }, new string[] { "3", "No decimal place", "700" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[4].AddFileColumnIndex(2); //Z0_AnotherDecimal

			using (StaticCurrentFetcher.Instance.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Netherlands))
			{
				AssertEquals("Current Number format", ",", Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator);

				var collection = new DummyChildBusinessObjectCollection(Factory);
				wizard.Object.ImportIntoCollection(collection, -1);
				AssertEquals(3, collection.Count);
				AssertEquals(3566.55M, collection[0].Z0_AnotherDecimal);
				AssertEquals(40567M, collection[1].Z0_AnotherDecimal);
				AssertEquals(700M, collection[2].Z0_AnotherDecimal);
			}
		}

		public void TestImportProgressDisplay()
		{
			var mock = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			mock.Setup(m => m.LoadFile(1, -1, false))
				.Returns(new List<string[]>
				{
					new string[] { "1", "AAA" }, new string[] { "2", "BBB" }, new string[] { "3", "CCC" }
				});

			var onProgressArgs = new List<Tuple<int, string>>();
			mock.Setup(m => m.OnProgressChanged(It.IsAny<int>(), It.IsAny<string>()))
				.Callback((int percentComplete, string status) =>
				{
					onProgressArgs.Add(new Tuple<int, string>(percentComplete, status));
				});

			var collection = new DummyWithChildCollection(Factory);
			mock.Object.ImportIntoCollection(collection, -1);

			AssertNoExceptionThrown(
					message: $"{nameof(ImportWizard.OnProgressChanged)} needs to have been called three times.",
					codeToRun: () => mock.Verify(m => m.OnProgressChanged(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(3)));

			CombineAssertions(delegate
			{
				AssertEquals("Progress at 33%", 33, onProgressArgs[0].Item1);
				AssertEquals("Progress at 66%", 66, onProgressArgs[1].Item1);
				AssertEquals("Progress at 100%", 100, onProgressArgs[2].Item1);

				AssertEquals("Status message at 33%", "Importing (1 of 3) ...", onProgressArgs[0].Item2);
				AssertEquals("Status message at 66%", "Importing (2 of 3) ...", onProgressArgs[1].Item2);
				AssertEquals("Status message at 100%", "Importing (3 of 3) ...", onProgressArgs[2].Item2);
			});
		}

		public void TestImportUninitialisedField_Found()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]>() { new string[] { "CASSAL" } });

			wizard.Object.Mapping[0].ClearAllFileColumnIndex();
			wizard.Object.Mapping[1].MappingName = "Child+Z0_VarCharMax";
			wizard.Object.Mapping[1].AddFileColumnIndex(0);

			DummyWithChildCollection collection1 = new DummyWithChildCollection(Factory);
			wizard.Object.ImportIntoCollection(collection1, -1);

			AssertEquals("Invokation count", 1, collection1.InvokationCount);
		}

		public void TestImportUninitialisedField_NotFound()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]>() { new string[] { "CASSAL" } });

			wizard.Object.Mapping[0].ClearAllFileColumnIndex();
			wizard.Object.Mapping[1].MappingName = "123+456";
			wizard.Object.Mapping[1].AddFileColumnIndex(0);

			DummyWithChildCollection collection1 = new DummyWithChildCollection(Factory);
			wizard.Object.ImportIntoCollection(collection1, -1);

			AssertEquals("Invokation count", 0, collection1.InvokationCount);
		}

		public void TestImportUninitialisedField_NoAttributeFound()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]>() { new string[] { "CASSAL" } });

			wizard.Object.Mapping[0].ClearAllFileColumnIndex();
			wizard.Object.Mapping[1].MappingName = "InvokationCount+Z0_VarCharMax";
			wizard.Object.Mapping[1].AddFileColumnIndex(0);

			DummyWithChildCollection collection1 = new DummyWithChildCollection(Factory);
			wizard.Object.ImportIntoCollection(collection1, -1);

			AssertEquals("Invokation count", 0, collection1.InvokationCount);
		}

		public void TestSelectivelyMappedImport()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "A", "BBBB", "C", "DD", "EE.EE", "FFFF-FF-FF" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[2].AddFileColumnIndex(2); //Z0_Bool
			wizard.Object.Mapping[4].AddFileColumnIndex(4); //Z0_AnotherDecimal

			DummyWithDefaultsCollection collection1 = new DummyWithDefaultsCollection(Factory);

			wizard.Object.ImportIntoCollection(collection1, -1);
			AssertEquals(1, collection1.Count);

			AssertEquals(0, collection1[0].Z0_Number); // invalid import value, should be set to type default
			AssertEquals("default", collection1[0].Z0_VarCharMax); // it's not mapped and should not be changed from biz obj initial value
			AssertEquals(false, collection1[0].Z0_Bool); // invalid import value, should be set to type default
			AssertEquals(new ZShort(99), collection1[0].Z0_Short); // it's not mapped and should not be changed from biz obj initial value
			AssertEquals(new ZDecimal(), collection1[0].Z0_AnotherDecimal); // invalid import value, should be set to type default
			AssertEquals(new ZDateTime(1999, 9, 19), collection1[0].Z0_Date); // it's not mapped and should not be changed from biz obj initial value
			AssertEquals(new ZDateTimeOffset(1999, 9, 19, 0, 0, 0, TimeSpan.FromHours(9)), collection1[0].Z0_DateTimeOffset); // it's not mapped from file and should be set to the mapping default value
			AssertEquals(new ZGeography("POINT (-110 44)"), collection1[0].Z0_Geography); // it's mapped from file and blank and should be set to the mapping default value
		}

		public void TestMappingDefaultsImport()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>()))
				.Returns(new List<string[]>() { new string[] { "2222", "file value", "false", "22", "AA.AA", "1999-02-22", "", "" } });

			//Z0_Number
			wizard.Object.Mapping[0].AddFileColumnIndex(0);
			wizard.Object.Mapping[0].DefaultValue = "6666";
			//Z0_VarCharMax
			wizard.Object.Mapping[1].DefaultValue = "mapping default";
			//Z0_Bool
			wizard.Object.Mapping[2].AddFileColumnIndex(2);
			wizard.Object.Mapping[2].DefaultValue = "true";
			//Z0_Short
			wizard.Object.Mapping[3].DefaultValue = "66";
			//Z0_AnotherDecimal
			wizard.Object.Mapping[4].AddFileColumnIndex(4);
			wizard.Object.Mapping[4].DefaultValue = "66.66";
			//Z0_Date
			wizard.Object.Mapping[5].DefaultValue = "1999-06-16";
			//Z0_NVarCharMax
			wizard.Object.Mapping[7].AddFileColumnIndex(6);
			wizard.Object.Mapping[7].DefaultValue = "mapping default";
			//Z0_AnotherNumber
			wizard.Object.Mapping[8].AddFileColumnIndex(7);
			wizard.Object.Mapping[8].DefaultValue = "33";

			DummyWithDefaultsCollection collection1 = new DummyWithDefaultsCollection(Factory);

			wizard.Object.ImportIntoCollection(collection1, -1);
			AssertEquals(1, collection1.Count);

			AssertEquals(2222, collection1[0].Z0_Number); // import value
			AssertEquals("mapping default", collection1[0].Z0_VarCharMax); // it's not mapped from file and should be set to the mapping default value
			AssertEquals(false, collection1[0].Z0_Bool); // import value
			AssertEquals(new ZShort(66), collection1[0].Z0_Short); // it's not mapped from file and should be set to the mapping default value
			AssertEquals(new ZDecimal(), collection1[0].Z0_AnotherDecimal); // invalid import value, should be set to type default
			AssertEquals(new ZDateTime(1999, 6, 16), collection1[0].Z0_Date); // it's not mapped from file and should be set to the mapping default value
			AssertEquals(new ZDateTimeOffset(1999, 9, 19, 0, 0, 0, TimeSpan.FromHours(9)), collection1[0].Z0_DateTimeOffset); // it's not mapped from file and should be set to the mapping default value
			AssertEquals("mapping default", collection1[0].Z0_NVarCharMax); // it's mapped from file and blank and should be set to the mapping default value
			AssertEquals(new ZShort(33), collection1[0].Z0_AnotherNumber); // it's mapped from file and blank and should be set to the mapping default value
			AssertEquals(new ZGeography("POINT (-110 44)"), collection1[0].Z0_Geography); // it's mapped from file and blank and should be set to the mapping default value
		}

		class DummyWithDefaults : DummyBusinessObject
		{
			public DummyWithDefaults(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyWithDefaultsCollection : BusinessObjectCollection<DummyWithDefaults>
		{
			public DummyWithDefaultsCollection(BusinessObjectFactory factory) : base(factory) { }

			protected override BusinessObject AddNewCore()
			{
				DummyWithDefaults newBO = (DummyWithDefaults)base.AddNewCore();

				newBO.Z0_Number = 9999;
				newBO.Z0_VarCharMax = "default";
				newBO.Z0_Bool = true;
				newBO.Z0_Short = new ZShort(99);
				newBO.Z0_Decimal = new ZDecimal(99.99);
				newBO.Z0_Date = new ZDateTime(1999, 9, 19);
				newBO.Z0_DateTimeOffset = new ZDateTimeOffset(1999, 9, 19, 0, 0, 0, TimeSpan.FromHours(9));
				newBO.Z0_Geography = new ZGeography("POINT (-110 44)");

				return newBO;
			}
		}

		class DummyWithChild : DummyChildBusinessObject
		{
			public DummyWithChild(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[ImportInitializationMethodName("CreateChild")]
			public DummyBusinessObject Child { get; private set; }

			public int InvokationCount { get; private set; }

			public void CreateChild()
			{
				InvokationCount++;
			}
		}

		class DummyWithChildCollection : BusinessObjectCollection<DummyWithChild>
		{
			public DummyWithChildCollection(BusinessObjectFactory factory) : base(factory) { }

			public int InvokationCount
			{
				get
				{
					return this.Sum(x => (x as DummyWithChild).InvokationCount);
				}
			}
		}

		public void TestLoadXls()
		{
			using (TempFile xls = CreateTestXls(
				new object[,] {
					{ "Text", "Number", "Date" },
					{ "Test1", 100, new DateTime(2009, 2, 1).ToOADate() },
					{ "Test2", 100.50, new DateTime(2008, 3, 1).ToOADate() },
					{ "", "", "" },
					{ "Test3", 200.40, new DateTime(2007, 4, 1).ToOADate() },
					{ "", "", "" }
				},
				new string[,] {
					{ null, null, "d/mm/yyyy" },
					{ null, null, "d/mm/yyyy" },
					{ null, null, "d/mm/yyyy h:mm" },
					{ null, null, "d/mm/yyyy h:mm" },
					{ null, "0.00_);[Red](0.00)", "h:mm:ss" },
					{ null, "0.00_);[Red](0.00)", "h:mm:ss" }
				})
			)
			{
				Helper.Wizard.FileName = xls.Filename;
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(4, result.Count);

				AssertEquals("Text", result[0][0]);
				AssertEquals("Number", result[0][1]);
				AssertEquals("Date", result[0][2]);

				AssertEquals("Test1", result[1][0]);
				AssertEquals("100", result[1][1]);
				AssertEquals("1/02/2009 12:00:00 AM", result[1][2]);

				AssertEquals("Test2", result[2][0]);
				AssertEquals("100.5", result[2][1]);
				AssertEquals("1/03/2008 12:00:00 AM", result[2][2]);

				AssertEquals("Test3", result[3][0]);
				AssertEquals("200.4", result[3][1]);
				AssertEquals("1/04/2007 12:00:00 AM", result[3][2]);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestLoadXls_DateTimeOffsetVersion()
		{
			using (TempFile xls = CreateTestXls(
				new object[,] {
					{ "Text", "Number", "DateTimeOffset" },
					{ "Test1", 100, new DateTime(2009, 2, 1).ToOADate() },
					{ "Test2", 100.50, new DateTime(2008, 3, 1).ToOADate() },
					{ "", "", "" },
					{ "Test3", 200.40, new DateTime(2007, 4, 1).ToOADate() },
					{ "", "", "" }
				},
				new string[,] {
					{ null, null, "d/mm/yyyy" },
					{ null, null, "d/mm/yyyy" },
					{ null, null, "d/mm/yyyy h:mm" },
					{ null, null, "d/mm/yyyy h:mm" },
					{ null, "0.00_);[Red](0.00)", "h:mm:ss" },
					{ null, "0.00_);[Red](0.00)", "h:mm:ss" }
				})
			)
			{
				Helper.Wizard.FileName = xls.Filename;
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(4, result.Count);

				AssertEquals("Text", result[0][0]);
				AssertEquals("Number", result[0][1]);
				AssertEquals("DateTimeOffset", result[0][2]);

				AssertEquals("Test1", result[1][0]);
				AssertEquals("100", result[1][1]);
				AssertEquals("1/02/2009 12:00:00 AM", result[1][2]);

				AssertEquals("Test2", result[2][0]);
				AssertEquals("100.5", result[2][1]);
				AssertEquals("1/03/2008 12:00:00 AM", result[2][2]);

				AssertEquals("Test3", result[3][0]);
				AssertEquals("200.4", result[3][1]);
				AssertEquals("1/04/2007 12:00:00 AM", result[3][2]);
			}
		}

		public void TestLoadXls_GeographyVersion()
		{
			using (TempFile xls = CreateTestXls(
				new object[,] {
					{ "Text", "Number", "Geography" },
					{ "Test1", 100, "POINT (-121 47)" },
					{ "Test2", 100.50, "POINT (-121 48)" },
					{ "", "", "" },
					{ "Test3", 200.40, "POINT (-121 49)" },
					{ "", "", "" }
				},
				new string[,] {
					{ null, null, null },
					{ null, null, null },
					{ null, null, null },
					{ null, null, null },
					{ null, "0.00_);[Red](0.00)", null },
					{ null, "0.00_);[Red](0.00)", null }
				})
			)
			{
				Helper.Wizard.FileName = xls.Filename;
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(4, result.Count);

				AssertEquals("Text", result[0][0]);
				AssertEquals("Number", result[0][1]);
				AssertEquals("Geography", result[0][2]);

				AssertEquals("Test1", result[1][0]);
				AssertEquals("100", result[1][1]);
				AssertEquals("POINT (-121 47)", result[1][2]);

				AssertEquals("Test2", result[2][0]);
				AssertEquals("100.5", result[2][1]);
				AssertEquals("POINT (-121 48)", result[2][2]);

				AssertEquals("Test3", result[3][0]);
				AssertEquals("200.4", result[3][1]);
				AssertEquals("POINT (-121 49)", result[3][2]);
			}
		}

		public void TestLoadXlsWithVeryBigAndSmallNumbers()
		{
			using (TempFile xls = CreateTestXls(
				new object[,] {
					{ 1.0 },
					{ 0.000003 },
					{ 3000000000 }
				},
				new string[,] {
					{ null },
					{ null },
					{ null }
				})
			)
			{
				Helper.Wizard.FileName = xls.Filename;
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(3, result.Count);

				AssertEquals("1", result[0][0]);
				AssertEquals("0.000003", result[1][0]);
				AssertEquals("3000000000", result[2][0]);

				AssertEquals(1M, ZDecimal.Parse(result[0][0]));
				AssertEquals(0.000003M, ZDecimal.Parse(result[1][0]));
				AssertEquals(3000000000M, ZDecimal.Parse(result[2][0]));
			}
		}

		public void TestLoadCsv_WithTextQualifier()
		{
			using (TempFile tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, @"""This is a """"text"""" column"",""and another column""");

				Helper.Wizard.FileName = tempFile.Filename;
				Helper.Wizard.TextQualifier = "\"";
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(1, result.Count);
				AssertEquals(@"This is a ""text"" column", result[0][0]);
				AssertEquals(@"and another column", result[0][1]);
			}
		}

		public void TestLoadCsv_EmbeddedLineBreaksOK()
		{
			using (TempFile testFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFile.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.WriteLine("\"col1\",\"col2\",\"col3\"");
					sw.WriteLine("\"col1,row1" + System.Environment.NewLine + "col1,row2\",\"col2\",\"col3\"");
					sw.WriteLine("\"col1,row1" + '\n' + "col1,row2\",\"col2\",\"col3\"");
					sw.WriteLine("\"col1\",\"col2\",\"col3\"");
					sw.Flush();
				}

				Helper.Wizard.FileName = testFile.Filename;
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(5, result.Count);

				AssertEquals("TEST1", result[0][0]);
				AssertEquals("TEST2", result[0][1]);
				AssertEquals("TEST3", result[0][2]);

				AssertEquals("col1,row1" + System.Environment.NewLine + "col1,row2", result[2][0]);
				AssertEquals("col2", result[2][1]);
				AssertEquals("col3", result[2][2]);

				AssertEquals("col1,row1" + System.Environment.NewLine + "col1,row2", result[3][0]);
				AssertEquals("col2", result[3][1]);
				AssertEquals("col3", result[3][2]);
			}
		}

		public void TestLoadCsv_FileWithEmptyRows()
		{
			using (TempFile testFile = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFile.Filename))
				{
					sw.WriteLine("TEST1,TEST2,TEST3");
					sw.WriteLine("\"col1\",\"col2\",\"col3\"");
					sw.WriteLine("col1,col2,col3");
					sw.WriteLine(",,");
					sw.WriteLine("\"\",\"\",\"\"");
					sw.WriteLine("\"col1\",\"col2\",\"col3\"");
					sw.WriteLine("col1,col2,col3");
					sw.Flush();
				}

				Helper.Wizard.FileName = testFile.Filename;
				List<string[]> result = Helper.Wizard.LoadFile(0, int.MaxValue);

				AssertEquals(5, result.Count);

				AssertEquals("TEST1", result[0][0]);
				AssertEquals("TEST2", result[0][1]);
				AssertEquals("TEST3", result[0][2]);

				for (int i = 1; i < result.Count; i++)
				{
					AssertEquals("col1", result[i][0]);
					AssertEquals("col2", result[i][1]);
					AssertEquals("col3", result[i][2]);
				}
			}
		}

		#region TestImportIntoCollectionReadOnly

		public void TestImportIntoCollectionReadOnly()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]> { new[] { "1", "AAA" }, new[] { "2", "BBB" }, new[] { "3", "CCC" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax

			DummyCollection1 collection1 = new DummyCollection1(Factory);
			wizard.Object.ImportIntoCollection(collection1, -1);
			AssertEquals(3, collection1.Count);
			AssertEquals(1, collection1[0].Z0_Number);
			AssertEquals(2, collection1[1].Z0_Number);
			AssertEquals(3, collection1[2].Z0_Number);
			AssertEquals("", collection1[0].Z0_VarCharMax);
			AssertEquals("", collection1[1].Z0_VarCharMax);
			AssertEquals("", collection1[2].Z0_VarCharMax);

			DummyCollection2 collection2 = new DummyCollection2(Factory);
			wizard.Object.ImportIntoCollection(collection2, -1);
			AssertEquals(3, collection2.Count);
			AssertEquals(1, collection2[0].Z0_Number);
			AssertEquals(2, collection2[1].Z0_Number);
			AssertEquals(3, collection2[2].Z0_Number);
			AssertEquals("AAA", collection2[0].Z0_VarCharMax);
			AssertEquals("", collection2[1].Z0_VarCharMax);
			AssertEquals("CCC", collection2[2].Z0_VarCharMax);

			DummyCollection3 collection3 = new DummyCollection3(Factory);
			wizard.Object.ImportIntoCollection(collection3, -1);
			AssertEquals(3, collection3.Count);
			AssertEquals(1, collection3[0].Z0_Number);
			AssertEquals(2, collection3[1].Z0_Number);
			AssertEquals(3, collection3[2].Z0_Number);
			AssertEquals("AAA", collection3[0].Z0_VarCharMax);
			AssertEquals("BBB", collection3[1].Z0_VarCharMax);
			AssertEquals("", collection3[2].Z0_VarCharMax);
		}

		public class DummyBizo1 : DummyChildBusinessObject
		{
			public DummyBizo1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[ReadOnly(true)]
			public override ZString Z0_VarCharMax
			{
				get { return base.Z0_VarCharMax; }
				set { base.Z0_VarCharMax = value; }
			}
		}

		public class DummyCollection1 : BusinessObjectCollection<DummyBizo1>
		{
			public DummyCollection1(BusinessObjectFactory factory) : base(factory) { }
		}

		public class DummyBizo2 : DummyChildBusinessObject
		{
			public DummyBizo2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public class DummyCollection2 : BusinessObjectCollection<DummyBizo2>
		{
			public DummyCollection2(BusinessObjectFactory factory) : base(factory) { }

			protected override void OnAdded(BusinessObject bizOAdded)
			{
				base.OnAdded(bizOAdded);
				if (Count == 2)
				{
					((DummyBizo2)bizOAdded).Z0_VarCharMax_ReadOnly = true;
				}
			}
		}

		public class DummyBizo3 : DummyChildBusinessObject
		{
			public DummyBizo3(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[ReadOnlyMember(nameof(PropertyShouldBeReadOnly))]
			public override ZString Z0_VarCharMax
			{
				get { return base.Z0_VarCharMax; }
				set { base.Z0_VarCharMax = value; }
			}

			public virtual bool PropertyShouldBeReadOnly
			{
				get;
				set;
			}
		}

		class DummyBizo4 : DummyBizo3
		{
			public DummyBizo4(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool PropertyShouldBeReadOnly => Z0_Description.IsEmpty || Z0_Description == "Default";
		}

		public class DummyCollection3 : BusinessObjectCollection<DummyBizo3>
		{
			public DummyCollection3(BusinessObjectFactory factory) : base(factory) { }

			protected override void OnAdded(BusinessObject bizOAdded)
			{
				base.OnAdded(bizOAdded);
				if (Count == 3)
				{
					((DummyBizo3)bizOAdded).PropertyShouldBeReadOnly = true;
				}
			}
		}

		class DummyCollection4 : BusinessObjectCollection<DummyBizo4>
		{
			public DummyCollection4(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion

		public void TestSettingMaxLength()
		{
			ImportWizard wizard = Helper.Wizard;
			AssertEquals(StmModuleFilterSchema.S9_FilterName.MaxLength, wizard.SettingInfo.MaxLength);
		}

		public void TestSaveSettings_WhenUserDoesntHaveSecurityRight_ShouldShowError()
		{
			AssertSaveSettings_SecurityRight(userHasSecurityRight: false, expectedIsDialogWasShown: true);
		}

		public void TestSaveSettings_WhenUserHasSecurityRight_ShouldNotShowError()
		{
			AssertSaveSettings_SecurityRight(userHasSecurityRight: true, expectedIsDialogWasShown: false);
		}

		void AssertSaveSettings_SecurityRight(bool userHasSecurityRight, bool expectedIsDialogWasShown)
		{
			var settingsStorageStub = new Mock<ISettingsStorage>();
			var hasSecurityRightCalled = false;
			var showSecurityErrorCalled = false;
			settingsStorageStub.Setup(m => m.HasSecurityRight()).Returns(userHasSecurityRight).Callback(() => hasSecurityRightCalled = true);
			settingsStorageStub.Setup(m => m.ShowSecurityError()).Callback(() => showSecurityErrorCalled = true);

			ImportWizard wizard = new ImportWizard(Helper.CollectionInfo, settingsStorageStub.Object, new FileMapperForTest());
			wizard.Setting = "setting";
			wizard.SaveSettings();

			AssertEquals("Call CheckPermission", true, hasSecurityRightCalled);
			AssertEquals("We show the security error only when user doesn't have security right", expectedIsDialogWasShown, showSecurityErrorCalled);
		}

		public void TestRemoveSettings_WhenUserDoesntHaveSecurityRight_ShouldShowError()
		{
			AssertRemoveSettings_SecurityRight(userHasSecurityRight: false, expectedIsDialogWasShown: true);
		}

		public void TestRemoveSettings_WhenUserHasSecurityRight_ShouldNotShowError()
		{
			AssertRemoveSettings_SecurityRight(userHasSecurityRight: true, expectedIsDialogWasShown: false);
		}

		void AssertRemoveSettings_SecurityRight(bool userHasSecurityRight, bool expectedIsDialogWasShown)
		{
			var settingsStorageStub = new Mock<ISettingsStorage>();
			var hasSecurityRightCalled = false;
			var showSecurityErrorCalled = false;
			settingsStorageStub.Setup(m => m.HasSecurityRight()).Returns(userHasSecurityRight).Callback(() => hasSecurityRightCalled = true);
			settingsStorageStub.Setup(m => m.ShowSecurityError()).Callback(() => showSecurityErrorCalled = true);
			settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(new[] { "setting" });

			ImportWizard wizard = new ImportWizard(Helper.CollectionInfo, settingsStorageStub.Object, new FileMapperForTest());
			wizard.Setting = "setting";

			wizard.RemoveSettings();

			AssertEquals("Call CheckPermission", true, hasSecurityRightCalled);
			AssertEquals("We show the security error only when user doesn't have security right", expectedIsDialogWasShown, showSecurityErrorCalled);
		}

		class SupportDataImportingBizObj : DummyChildBusinessObject, ISupportDataImporting
		{
			public SupportDataImportingBizObj(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsImportingData
			{
				get { return isImportingData; }
				set
				{
					isImportingData = value;
					if (isImportingData)
					{
						IsImportingDataWasSet = true;
					}
				}
			}

			bool isImportingData;
			public bool IsImportingDataWasSet;
		}

		class SupportDataImportingBizObjCollection : BusinessObjectCollection<SupportDataImportingBizObj>
		{
			public SupportDataImportingBizObjCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#region Implementation

		TempFile CreateTestXls(object[,] values, string[,] formats)
		{
			using (IExcelInterface excelDoc = ExcelInterfaceFactory.New())
			{
				excelDoc.NewExcelFile(1);
				for (int i = 0; i < values.GetLength(0); i++)
				{
					for (int j = 0; j < values.GetLength(1); j++)
					{
						excelDoc.WorkSheets[0][i, j] = values[i, j];
						if (formats != null && i < formats.GetLength(0) && j < formats.GetLength(1) && !string.IsNullOrEmpty(formats[i, j]))
						{
							CellFormat cellFormat = new CellFormat();
							cellFormat.FormatPattern = formats[i, j];
							excelDoc.WorkSheets[0].SetCellFormat(i, j, cellFormat);
						}
					}
				}
				TempFile result = TempFile.NewWithExtension("xls");
				excelDoc.SaveToFile(result.Filename);

				return result;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.Wizard;
		}

		public override ImportExportWizard NewWizard(ISettingsStorage settingsStorage)
		{
			return new ImportWizard(Helper.CollectionInfo, settingsStorage, new FileMapperForTest());
		}

		ImportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ImportWizardTestHelper(Factory);
				}

				return helper;
			}
		}

		ImportWizardTestHelper helper;

		#endregion

		#region ISetterSuspenderSupporter

		class TestSetterSuspenderSupporter : DummyChildBusinessObject, ISetterSuspenderSupporter
		{
			public TestSetterSuspenderSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			IEnumerable<string> ISetterSuspenderSupporter.SupportedFields
			{
				get
				{
					yield return "Z0_Number";
				}
			}

			SetterSuspender fsuspender;
			public SetterSuspender SetterSuspender => fsuspender ?? (fsuspender = new SetterSuspender());

			public override ZInt Z0_Number
			{
				get => base.Z0_Number;
				set
				{
					if (value != base.Z0_Number && !SetterSuspender.IsSetterSuspended(nameof(Z0_Number)))
					{
						base.Z0_Number = value;
					}
				}
			}

			public override ZString Z0_VarCharMax
			{
				get => base.Z0_VarCharMax;
				set
				{
					base.Z0_VarCharMax = value;
					Z0_Number = 9999;
				}
			}

			public override ZString Z0_Description
			{
				get => base.Z0_Description;
				set
				{
					Z0_Number = 9000;
					base.Z0_Description = value;
				}
			}
		}

		class TestSetterSuspenderSupporterCollection : BusinessObjectCollection<TestSetterSuspenderSupporter>
		{
			public TestSetterSuspenderSupporterCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		public void TestISetterSuspenderSupporter()
		{
			var wizard = new Mock<ImportWizard>(Helper.CollectionInfo, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(1, -1, It.IsAny<bool>())).Returns(new List<string[]>() { new string[] { "1", "VarCharMax", "Description text" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0); //Z0_Number
			wizard.Object.Mapping[1].AddFileColumnIndex(1); //Z0_VarCharMax
			wizard.Object.Mapping[6].AddFileColumnIndex(2); //Z0_Description

			var collection1 = new TestSetterSuspenderSupporterCollection(Factory);
			wizard.Object.ImportIntoCollection(collection1, -1);
			var testItem = collection1[0];

			AssertEquals("Z0_Number should have been set, and should not have been overridden by other property setters i.e. ResumeSettingAndAddToSuspenderIfMissing should worked.", 1, testItem.Z0_Number);
			AssertEquals("Z0_VarCharMax should have been set.", "VarCharMax", testItem.Z0_VarCharMax);
			AssertEquals("Z0_Description should have been set.", "Description text", testItem.Z0_Description);
			Assert("Z0_Number should be writable.", !testItem.SetterSuspender.IsSetterSuspended("Z0_Number"));
			Assert("Z0_VarCharMax should be writable.", !testItem.SetterSuspender.IsSetterSuspended("Z0_VarCharMax"));
			Assert("Z0_Description should be writable.", !testItem.SetterSuspender.IsSetterSuspended("Z0_Description"));
		}

		#endregion
	}
}
