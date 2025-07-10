using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.DataMapping.ImportExportWizard;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ImportWizardMapping))]
	sealed class ImportWizardMappingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInvalidDateTimeMapping()
		{
			var helper = new ImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("0103-11-21,0103-11-22,0103-11-23");
			helper.Wizard.FileName = filename;

			var importWizardMapping1 = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_DateOnly);
			var importWizardMapping2 = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_Date);
			var importWizardMapping3 = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_DateTimeOffset);

			importWizardMapping1.AddFileColumnIndex(0);
			importWizardMapping2.AddFileColumnIndex(1);
			importWizardMapping3.AddFileColumnIndex(2);

			var collection = new DummyChildBusinessObjectCollection(Factory);
			helper.Wizard.ImportIntoCollection(collection);
			AssertEquals(1, collection.Count);

			var dummy = collection[0];
			AssertEquals(ZDateTime.Empty, dummy.Z0_Date);
			AssertEquals(ZDate.Empty, dummy.Z0_DateOnly);
			AssertEquals(ZDateTimeOffset.Empty, dummy.Z0_DateTimeOffset);

			AssertNoErrors(importWizardMapping1.MappedFieldInfo);
			AssertNoErrors(importWizardMapping2.MappedFieldInfo);
			AssertNoErrors(importWizardMapping3.MappedFieldInfo);

			File.Delete(filename);
		}

		public void TestFileColumnIndexOrder()
		{
			try
			{
				Collection[0].AddFileColumnIndex(0);
				AssertEquals("Should have 1 file column", 1, collection[0].FileColumnIndexOrder.Count);
				AssertEquals(0, collection[0].FileColumnIndexOrder[0]);
				AssertNullOrEmpty("Delimiter", collection[0].Delimiter);

				Collection[0].ClearAllFileColumnIndex();
				Collection[0].AddFileColumnIndex(0);
				Collection[0].AddFileColumnIndex(1);
				AssertEquals("Should have 2 file column", 2, collection[0].FileColumnIndexOrder.Count);
				AssertEquals(0, collection[0].FileColumnIndexOrder[0]);
				AssertEquals(1, collection[0].FileColumnIndexOrder[1]);
				AssertEquals("Default Delimiter shoule be space", SupportedDelimiter.Space, collection[0].Delimiter);

				Collection[0].AddFileColumnIndex(1);
				AssertEquals("Should have 2 file column", 2, collection[0].FileColumnIndexOrder.Count);
				AssertEquals(0, collection[0].FileColumnIndexOrder[0]);
				AssertEquals(1, collection[0].FileColumnIndexOrder[1]);
				AssertEquals("Default Delimiter shoule be space", SupportedDelimiter.Space, collection[0].Delimiter);

				Collection[0].AddFileColumnIndex(0);
				AssertEquals("Should have 2 file column", 2, collection[0].FileColumnIndexOrder.Count);
				AssertEquals(1, collection[0].FileColumnIndexOrder[0]);
				AssertEquals(0, collection[0].FileColumnIndexOrder[1]);
				AssertEquals("Default Delimiter shoule be space", SupportedDelimiter.Space, collection[0].Delimiter);

				Collection[0].ClearAllFileColumnIndex();
				AssertEquals("Should have no file column", 0, collection[0].FileColumnIndexOrder.Count);
				AssertNullOrEmpty("Delimiter", collection[0].Delimiter);

				Collection[0].AddFileColumnIndex(0);
				AssertEquals("Should have 1 file column", 1, collection[0].FileColumnIndexOrder.Count);
				AssertEquals(0, collection[0].FileColumnIndexOrder[0]);
				AssertNullOrEmpty("Delimiter", collection[0].Delimiter);
			}
			finally
			{
				Collection[0].ClearAllFileColumnIndex();
			}
		}

		public void TestImportWizardMappingDelimiters()
		{
			AssertEquals("Delimiters Count", 8, Collection[0].Delimiters.Count);
			AssertContains("Delimiters elements", ",, ~, |, space, tab, :, ;, newline", Collection[0].Delimiters.CodesAsString);
		}

		public void TestMappedField()
		{
			AssertEquals("", Collection[0].MappedField);

			Collection[0].AddFileColumnIndex(1);
			AssertEquals("Field[1]", Collection[0].MappedField);
		}

		public void TestMappingValidation()
		{
			var mapping = Helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == "Z0_Guid");
			Assert(mapping.Property.IsMandatory);
			Assert(!Helper.Wizard.HasErrors);
			Assert(!mapping.HasErrors);
			Helper.Wizard.RunPreSaveValidation();
			Assert(Helper.Wizard.HasErrors);
			Assert(mapping.HasErrors);
			AssertHasError(mapping.MappedFieldInfo, "Please map Z0_Guid before importing");

			mapping.AddFileColumnIndex(1);
			Assert(!mapping.HasErrors);
			AssertNoErrors(mapping.MappedFieldInfo);
		}

		string GetTemporyFileForMapping(string input)
		{
			string filename = EnvProxy.Instance.GetTempFileName();
			using (var fs = File.Create(filename))
			{
				using (var sw = new StreamWriter(fs))
				{
					sw.WriteLine(input);
				}
			}

			return filename;
		}

		public void TestValidateMapAs_WhenUsingCalculatedDescriptionProperty()
		{
			var helper = new DummyBusinessObjectWithCalculatedDescriptionPropertyImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("123");
			helper.Wizard.FileName = filename;
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_Guid);
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object's DescriptionProperty is set as a calculated property (instead of a schema column), then no alternate key exists and there should have a validation error.", () =>
			{
				Assert(helper.Wizard.HasErrors);
				Assert(importWizardMapping.HasErrors);
				AssertHasError(importWizardMapping.MapAsInfo, "Description cannot be used for mapping to Guid");
			});

			File.Delete(filename);
		}

		public void TestValidateMapAs_WhenUsingCalculatedDescriptionProperty_AndMappedFromFieldIsEmpty()
		{
			var helper = new DummyBusinessObjectWithCalculatedDescriptionPropertyImportWizardTestHelper(Factory);
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_Code);
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object's DescriptionProperty is set as a calculated property (instead of a schema column), then no alternate key exists and we should have validation errors.", () =>
			{
				Assert(helper.Wizard.HasErrors);
				Assert(importWizardMapping.HasErrors);
				AssertHasError(importWizardMapping.MapAsInfo, "Description cannot be used for mapping to Code");
			});
		}

		public void TestValidateMapAs_WhenUsingCalculatedDescriptionProperty_AndMappedFromFieldIsASchemaColumn()
		{
			var helper = new DummyBusinessObjectWithCalculatedDescriptionPropertyImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("Z0_Code");
			helper.Wizard.FileName = filename;
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_Code);
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object's DescriptionProperty is set as a calculated property (instead of a schema column), and the text in MappedFrom is a valid schema column, there should be a validation error.", () =>
			{
				Assert(helper.Wizard.HasErrors);
				Assert(importWizardMapping.HasErrors);
				AssertHasError(importWizardMapping.MapAsInfo, "Description cannot be used for mapping to Code");
			});

			File.Delete(filename);
		}

		public void TestValidateMapAs_WhenUsingSchemaColumnDescriptionProperty_AndMappedFromFieldIsASchemaColumn()
		{
			var helper = new DummyBusinessObjectWithCodeDescriptionPropertyImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("Z0_Description");
			helper.Wizard.FileName = filename;
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == DummyBizoSchema.Constants.Z0_Guid);
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object's DescriptionProperty is set as a schema column, and the text in MappedFrom is the same schema column, then the mapping should be valid.", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			File.Delete(filename);
		}

		public void TestValidateMapAs_WhenUsingMultilingualProperty()
		{
			var helper = new DummyBusinessObjectWithMultilingualStringPropertyImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("Fishing Cat");
			helper.Wizard.FileName = filename;
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == "Z0_Description");
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object has a multilingual property associated with a schema column, and the value can be mapped to a single row in the database, then the mapping should be valid.", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			File.Delete(filename);
		}

		public void TestValidateMapAs_WhenUsingMultilingualProperty_AndValueDoesntExist()
		{
			var helper = new DummyBusinessObjectWithMultilingualStringPropertyImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("Sleepy Cat");
			helper.Wizard.FileName = filename;
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == "Z0_Description");
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object has a multilingual property associated with a schema column, but there is no existing value in the database, then the mapping should be invalid.", () =>
			{
				Assert(helper.Wizard.HasErrors);
				Assert(importWizardMapping.HasErrors);
				AssertHasError(importWizardMapping.MapAsInfo, "Description cannot be used for mapping to Z0_Desc");
			});

			File.Delete(filename);
		}

		public void TestValidateMapAs_WhenUsingMultilingualProperty_AndDuplicateValuesExist()
		{
			var helper = new DummyBusinessObjectWithMultilingualStringPropertyImportWizardTestHelper(Factory);
			var filename = GetTemporyFileForMapping("Knitting Cat");
			helper.Wizard.FileName = filename;
			var importWizardMapping = helper.Wizard.Mapping.Cast<ImportWizardMapping>().First(x => x.MappingName == "Z0_Description");
			importWizardMapping.AddFileColumnIndex(0);
			importWizardMapping.MapAs = "Code";

			CombineAssertions("Precondition", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			importWizardMapping.MapAs = "Description";

			CombineAssertions("When a business object has a multilingual property associated with a schema column, but there are duplicate values in the database, then the mapping should be valid.", () =>
			{
				Assert(!helper.Wizard.HasErrors);
				Assert(!importWizardMapping.HasErrors);
				AssertNoErrors(importWizardMapping.MapAsInfo);
			});

			File.Delete(filename);
		}

		public void TestProperCaseReadOnly()
		{
			var mapping = new ImportWizardMapping(new ImportPropertyInfoImpl<DummyBusinessObjectWithForeignKey>(DummyBusinessObject.Schema.Z0_Description), null);
			AssertEquals(false, mapping.ProperCaseInfo.ReadOnly);

			var mapping2 = new ImportWizardMapping(new ImportPropertyInfoImpl<DummyBusinessObjectWithForeignKey>(DummyBusinessObject.Schema.Z0_Date), null);
			AssertEquals(true, mapping2.ProperCaseInfo.ReadOnly);

			var mapping3 = new ImportWizardMapping(new ImportPropertyInfoImpl<DummyBusinessObjectWithForeignKey>(DummyBusinessObject.Schema.Z0_Guid), null);
			AssertEquals(true, mapping3.ProperCaseInfo.ReadOnly);
		}

		public void TestValidateProperCase()
		{
			var mapping = Collection[0];
			var mappingProperty = (ImportPropertyInfoImpl)mapping.Property;

			mappingProperty.CharacterCasing = ZCharacterCasing.Normal;
			mapping.ProperCase = false;
			AssertNoErrors(mapping.ProperCaseInfo);

			mappingProperty.CharacterCasing = ZCharacterCasing.Normal;
			mapping.ProperCase = true;
			AssertNoErrors(mapping.ProperCaseInfo);

			mappingProperty.CharacterCasing = ZCharacterCasing.Lower;
			mapping.ProperCase = false;
			AssertNoErrors(mapping.ProperCaseInfo);

			mappingProperty.CharacterCasing = ZCharacterCasing.Lower;
			mapping.ProperCase = true;
			AssertHasError(mapping.ProperCaseInfo, "Will not have an effect as the field is lower case only.");

			mappingProperty.CharacterCasing = ZCharacterCasing.Upper;
			mapping.ProperCase = false;
			AssertNoErrors(mapping.ProperCaseInfo);

			mappingProperty.CharacterCasing = ZCharacterCasing.Upper;
			mapping.ProperCase = true;
			AssertHasError(mapping.ProperCaseInfo, "Will not have an effect as the field is upper case only.");
		}

		public void TestMapping()
		{
			Assert(!Collection[0].IsMapped());
			Assert(Collection[0].Text.StartsWith("Num"));
			AssertEquals(DummyBizoSchema.Constants.Z0_Number, Collection[0].MappingName);
			AssertEquals(typeof(ZInt), Collection[0].PropertyType);

			Assert(!Collection[1].IsMapped());
			Assert(Collection[1].Text.StartsWith("Txt"));
			AssertEquals(DummyBizoSchema.Constants.Z0_VarCharMax, Collection[1].MappingName);
			AssertEquals(typeof(ZString), Collection[1].PropertyType);

			Collection[0].AddFileColumnIndex(1);
			Assert(Collection[0].IsMapped());
			AssertEquals(1, Collection[0].FileColumnIndexOrder.First());
			Assert(!Collection[1].IsMapped());

			Collection[1].AddFileColumnIndex(2);
			Assert(Collection[0].IsMapped());
			AssertEquals(1, Collection[0].FileColumnIndexOrder.First());
			Assert(Collection[1].IsMapped());
			AssertEquals(2, Collection[1].FileColumnIndexOrder.First());

			Collection[1].ClearAllFileColumnIndex();
			Collection[1].AddFileColumnIndex(1);
			Assert(Collection[1].IsMapped());
			AssertEquals(1, Collection[1].FileColumnIndexOrder.First());

			Collection[0].ClearAllFileColumnIndex();
			Collection[0].AddFileColumnIndex(3);
			Assert(Collection[0].IsMapped());
			AssertEquals(3, Collection[0].FileColumnIndexOrder.First());
			Assert(Collection[1].IsMapped());
			AssertEquals(1, Collection[1].FileColumnIndexOrder.First());

			Collection.ClearColumnIndexes();
			Assert(!Collection[0].IsMapped());
			Assert(!Collection[1].IsMapped());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestTryParse()
		{
			AssertTryParse(DummyBizoSchema.Constants.Z0_VarCharMax, "Test", true, (ZString)"Test");

			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "Test", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "2007-1-12", true, new ZDateTime(2007, 1, 12));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "20130408", true, new ZDateTime(2013, 4, 8));

			ZDateTime dt = ZDateTime.DefaultDurationEpoch;
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, ":", true, dt);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, ":1", true, dt.AddMinutes(1));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "1:", true, dt.AddMinutes(60));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "0:1", true, dt.AddMinutes(1));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "1:1", true, dt.AddMinutes(61));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "01:40", true, dt.AddMinutes(100));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "001:40", true, dt.AddMinutes(100));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "024:00", true, dt.AddHours(24));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "-024:00", true, dt.AddHours(-24));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "-0:50", true, dt.AddMinutes(-50));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "-999:59", true, dt.AddHours(-999).AddMinutes(-59));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "999:59", true, dt.AddHours(999).AddMinutes(59));
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "-:50", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "1:040", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "10:-5", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "-0:60", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "1000:00", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "999999999999999999999999999999999999999999999999999999:9999999999999999999999999999999999999999999", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "japan:", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "040404abba:0404040dabba", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "100:00:00", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Date, "10:00:00", true, ZDateTime.Today.AddHours(10));

			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "Test", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-1-12", true, new ZDateTimeOffset(2007, 1, 12, 0, 0, 0, TimeSpan.FromHours(11)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "20130408", true, new ZDateTimeOffset(2013, 4, 8, 0, 0, 0, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28/03/2007 12:13:50 PM -07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50.000 PM -07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50.000 -07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28/03/2007 12:13:50 PM +00:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28/03/2007 12:13:50 PM", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28/03/2007 12:13:50", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28/03/2007 12:13", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28/03/2007", true, new ZDateTimeOffset(2007, 3, 28, 00, 00, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50.000 PM", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50 PM", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28", true, new ZDateTimeOffset(2007, 3, 28, 0, 00, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-07 12:13:50.000 PM", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-07 12:13:50 PM", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-07 12:13:50", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-07 12:13", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-07", true, new ZDateTimeOffset(2007, 3, 28, 00, 00, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-91 12:13:50.000 PM", true, new ZDateTimeOffset(1991, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-91 12:13:50 PM", true, new ZDateTimeOffset(1991, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-91 12:13:50", true, new ZDateTimeOffset(1991, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-91 12:13", true, new ZDateTimeOffset(1991, 3, 28, 12, 13, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "28-MAR-91", true, new ZDateTimeOffset(1991, 3, 28, 00, 00, 00, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28T12:13:50", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28T12:13:50+00:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28T12:13:50-00:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28T12:13:50+07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28T12:13:50-07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(10)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50+00:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50-00:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50+07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50-07:00", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.FromHours(-7)));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28T12:13:50Z", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "2007-03-28 12:13:50Z", true, new ZDateTimeOffset(2007, 3, 28, 12, 13, 50, TimeSpan.Zero));
			AssertTryParse(DummyBizoSchema.Constants.Z0_DateTimeOffset, "11/06/2007", true, new ZDateTimeOffset(2007, 6, 11, 00, 00, 00, TimeSpan.FromHours(10)));

			AssertTryParse(DummyBizoSchema.Constants.Z0_Number, "20002000", true, (ZInt)20002000);

			AssertTryParse(DummyBizoSchema.Constants.Z0_Short, "20002000", false, null);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Short, "2", true, (ZShort)2);

			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "Y", true, ZBool.True);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "N", true, ZBool.False);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "1", true, ZBool.True);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "0", true, ZBool.False);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "T", true, ZBool.True);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "F", true, ZBool.False);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "YES", true, ZBool.True);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "NO", true, ZBool.False);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "TRUE", true, ZBool.True);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Bool, "FALSE", true, ZBool.False);

			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "10.10", true, (ZDecimal)10.10m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "$100", true, (ZDecimal)100m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "$AU 99.95", true, (ZDecimal)99.95m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "1000 UAH", true, (ZDecimal)1000m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "101$SG", true, (ZDecimal)101m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "202.", true, (ZDecimal)202m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, ".55", true, (ZDecimal)0.55m);
			AssertTryParse(DummyBizoSchema.Constants.Z0_AnotherDecimal, "ABCD", false, null);

			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectWithForeignKey>();
			dummy.Z0_Code = "ABC";
			Factory.Save();

			AssertTryParse(DummyBizoSchema.Constants.Z0_Guid, "ABC", true, dummy.PK);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Guid, "DEF", false, ZGuid.Empty);
			AssertTryParse(DummyBizoSchema.Constants.Z0_Guid, "ABC", true, dummy.PK, "GHI");
			AssertTryParse(DummyBizoSchema.Constants.Z0_Guid, "DEF", false, (ZString)"GHI", "GHI");

			AssertTryParse(DummyBizoSchema.Constants.Z0_Description, "description1", true, (ZString)"code1", "Description");
		}

		void AssertTryParse(string mappingName, string value, bool success, IZType expectedResult, string mapAs = "", string defaultValue = null)
		{
			bool found = false;
			foreach (ImportWizardMapping m in Collection)
			{
				if (m.MappingName == mappingName)
				{
					found = true;
					m.AddFileColumnIndex(0);
					if (!string.IsNullOrEmpty(mapAs))
					{
						m.MapAs = mapAs;
					}

					IZType result;
					Exception ex;
					AssertEquals(success, m.TryParse(Factory.New(typeof(DummyBusinessObjectWithForeignKey)), new string[] { value }, out result, out ex, defaultValue));
					if (success)
					{
						AssertEquals(expectedResult, result);
					}
					break;
				}
			}

			Assert(found);
		}

		public void TestTryParse_Casing()
		{
			AssertTryParse_Casing(DummyBizoSchema.Constants.Z0_VarCharMax, ZCharacterCasing.Normal, false, (ZString)"WORD word Word wOrd", "WORD word Word wOrd");
			AssertTryParse_Casing(DummyBizoSchema.Constants.Z0_VarCharMax, ZCharacterCasing.Upper, false, (ZString)"WORD WORD WORD WORD", "WORD word Word wOrd");
			AssertTryParse_Casing(DummyBizoSchema.Constants.Z0_VarCharMax, ZCharacterCasing.Lower, false, (ZString)"word word word word", "WORD word Word wOrd");

			AssertTryParse_Casing(DummyBizoSchema.Constants.Z0_VarCharMax, ZCharacterCasing.Normal, true, (ZString)"Word Word Word Word", "WORD word Word wOrd");
			AssertTryParse_Casing(DummyBizoSchema.Constants.Z0_VarCharMax, ZCharacterCasing.Upper, true, (ZString)"WORD WORD WORD WORD", "WORD word Word wOrd");
			AssertTryParse_Casing(DummyBizoSchema.Constants.Z0_VarCharMax, ZCharacterCasing.Lower, true, (ZString)"word word word word", "WORD word Word wOrd");
		}

		void AssertTryParse_Casing(string mappingName, ZCharacterCasing characterCasing, bool properCase, IZType expectedResult, string value)
		{
			bool found = false;
			foreach (ImportWizardMapping m in Collection)
			{
				m.ProperCase = true;
				if (m.MappingName == mappingName)
				{
					found = true;
					m.AddFileColumnIndex(0);
					m.ProperCase = properCase;
					((ImportPropertyInfoImpl)m.Property).CharacterCasing = characterCasing;
					IZType result;
					Exception ex;
					AssertEquals(true, m.TryParse(Factory.New(typeof(DummyBusinessObjectWithForeignKey)), new string[] { value }, out result, out ex));
					AssertEquals(expectedResult, result);
					break;
				}
			}

			Assert(found);
		}

		public void TestTryParseNonPersistentBusinessObject()
		{
			NonPersistentBusinessObjectImportWizardTestHelper helper = new NonPersistentBusinessObjectImportWizardTestHelper(Factory);
			ImportWizardMappingCollection collection = new ImportWizardMappingCollection(helper.Wizard);
			collection.Load();

			ImportWizardMapping mapping = collection[0];
			mapping.AddFileColumnIndex(0);
			mapping.MapAs = "Description";
			IZType result;
			Exception ex;

			for (int i = 0; i < 3; i++)
			{
				mapping.TryParse(new DummyNonPersistentBusinessObject(), new string[] { "Description" + i.ToString() }, out result, out ex);
				AssertEquals("Code" + i.ToString(), result);
				AssertNull(ex);
			}
		}

		public void TestTryFromGuidDropEditList()
		{
			var mapping = new ImportWizardMapping(new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Guid), null);
			var list = new IsNotBusinessObjectListForTest { "TEST" };

			AssertNoExceptionThrown(() => mapping.FindGuidFromList("TEST", list, "Code"));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			foreach (IImportPropertyInfo property in Helper.CollectionInfo.Properties)
			{
				if (property.MappingName == DummyBizoSchema.Constants.Z0_VarCharMax)
				{
					return new ImportWizardMapping(property, new ImportWizardMappingCollection(Helper.Wizard));
				}
			}

			return null;
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

		ImportWizardMappingCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new ImportWizardMappingCollection(Helper.Wizard);
					collection.Load();
					AssertEquals(13, collection.Count);
				}

				return collection;
			}
		}

		ImportWizardMappingCollection collection;

		#endregion
	}
}
