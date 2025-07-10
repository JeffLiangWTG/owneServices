using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Test.Utilities.GenericImportExport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	public abstract class ExportWizardTest : ImportExportWizardTest
	{
		public void TestDelimiters()
		{
			var collectionInfo = Helper.CollectionInfo;
			AssertEquals("Delimiters Count", 8, helper.Wizard.Delimiters.Count);
			AssertContains("Delimiters elements", ",, ~, |, space, tab, :, ;, none", helper.Wizard.Delimiters.CodesAsString);
		}

		public void TestFactoryShouldNotBeTheSameAsCollectionFactory()
		{
			var collectionInfo = Helper.CollectionInfo;
			AssertNotEquals("The Wizard should not have the same factory as the Collection as the system should be able to GC the wizard when it's out of scope", Helper.Wizard.Factory, collectionInfo.Factory);
		}

		public void TestExportCollection()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_Description = "DDD";
			child1.Z0_Number = 10;
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_Bool = true;
			child1.Z0_Short = 1;
			child1.Z0_Decimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);
			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_Description = "AAA";
			child2.Z0_Number = 20;
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_Bool = false;
			child2.Z0_Short = 2;
			child2.Z0_Decimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			Helper.Wizard.Mapping[0].Header = "NumHeader";

			var result = Helper.Wizard.ExportCollection(dummy.Collection, Int32.MaxValue);
			AssertEquals(3, result.Count());
			AssertEquals(6, result.ElementAt(0).Length);
			AssertEquals("NumHeader", result.ElementAt(0)[0]);
			AssertEquals("Date", result.ElementAt(0)[5]);
			AssertEquals("20", result.ElementAt(2)[0]);
			AssertEquals("11/10/2007", result.ElementAt(2)[5]);

			Helper.Wizard.IncludeHeaders = false;
			result = Helper.Wizard.ExportCollection(dummy.Collection, Int32.MaxValue);
			AssertEquals(2, result.Count());
			AssertEquals(6, result.ElementAt(0).Length);
			AssertEquals("10", result.ElementAt(0)[0]);
			AssertEquals("10/10/2007", result.ElementAt(0)[5]);
			AssertEquals("20", result.ElementAt(1)[0]);
			AssertEquals("11/10/2007", result.ElementAt(1)[5]);
		}

		public void TestExportCollectionHandlesEmptyDelimiterCorrectlyForCSV()
		{
			Helper.Wizard.IncludeHeaders = false;
			Helper.Wizard.Delimiter = "none";

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_Description = "DDD";
			child1.Z0_Number = 10;
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_Bool = true;
			child1.Z0_Short = 1;
			child1.Z0_AnotherDecimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);

			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_Description = "AAA";
			child2.Z0_Number = 20;
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_Bool = false;
			child2.Z0_Short = 2;
			child2.Z0_AnotherDecimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			string expectedText = "10TTTY11.110/10/2007\r\n20SSSN22.111/10/2007";
			string errorMessage;

			using (TempFile file = TempFile.NewWithExtension("csv"))
			{
				Helper.Wizard.FileName = file.Filename;
				Assert(Helper.Wizard.ExportCollection(dummy.Collection, out errorMessage));
				string actualText = File.ReadAllText(file.Filename);
				AssertEquals("file contents", expectedText + "\r\n", actualText);
			}
		}

		public void TestExportCollectionHandlesNewLinesCorrectlyForCSV()
		{
			Helper.Wizard.IncludeHeaders = false;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_Description = "DDD";
			child1.Z0_Number = 10;
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_Bool = true;
			child1.Z0_Short = 1;
			child1.Z0_AnotherDecimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);

			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_Description = "AAA";
			child2.Z0_Number = 20;
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_Bool = false;
			child2.Z0_Short = 2;
			child2.Z0_AnotherDecimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			string expectedText = "10,TTT,Y,1,1.1,10/10/2007\r\n20,SSS,N,2,2.1,11/10/2007";
			string errorMessage;

			using (TempFile file = TempFile.NewWithExtension("csv"))
			using (Culture.SetTemporarily(Culture.Default))
			{
				Helper.Wizard.FileName = file.Filename;
				Assert(Helper.Wizard.ExportCollection(dummy.Collection, out errorMessage));
				string actualText = File.ReadAllText(file.Filename);
				AssertEquals("AppendExtraNewLineToEndOfFile = Y, CSV Newlines are not handled corredtly", expectedText + "\r\n", actualText);
				AssertEndsWith("AppendExtraNewLineToEndOfFile = Y, There should be a newline at the end of the file", "20,SSS,N,2,2.1,11/10/2007\r\n", actualText);
			}

			helper.Wizard.AppendExtraNewLineToEndOfFile = false;

			using (TempFile file = TempFile.NewWithExtension("csv"))
			using (Culture.SetTemporarily(Culture.Default))
			{
				Helper.Wizard.FileName = file.Filename;
				Assert(Helper.Wizard.ExportCollection(dummy.Collection, out errorMessage));
				string actualText = File.ReadAllText(file.Filename);
				AssertEquals("AppendExtraNewLineToEndOfFile = N, CSV Newlines are not handled corredtly", expectedText, actualText);
				AssertEndsWith("AppendExtraNewLineToEndOfFile = N, There should be no newline at the end of the file", "20,SSS,N,2,2.1,11/10/2007", actualText);
			}
		}

		public void TestExportCollectionHandlesNewLinesCorrectlyForFixedWidth()
		{
			Helper.Wizard.IncludeHeaders = false;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_Description = "DDD";
			child1.Z0_Number = 10;
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_Bool = true;
			child1.Z0_Short = 1;
			child1.Z0_AnotherDecimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);

			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_Description = "AAA";
			child2.Z0_Number = 20;
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_Bool = false;
			child2.Z0_Short = 2;
			child2.Z0_AnotherDecimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			string expectedText = "10TTTY11.110/10/2007\r\n20SSSN22.111/10/2007";

			using (TempFile file = TempFile.NewWithExtension("csv"))
			{
				Helper.Wizard.FileName = file.Filename;
				Helper.Wizard.FixedWidth = true;
				string errorMessage;
				Assert(Helper.Wizard.ExportCollection(dummy.Collection, out errorMessage));
				string actualText = File.ReadAllText(file.Filename);
				AssertEquals("AppendExtraNewLineToEndOfFile = N, Fixed Width Newlines are not handled correctly", expectedText + "\r\n", actualText);
				AssertEndsWith("AppendExtraNewLineToEndOfFile = N, There should be a newline at the end of the file", "20SSSN22.111/10/2007\r\n", actualText);
			}

			helper.Wizard.AppendExtraNewLineToEndOfFile = false;

			using (TempFile file = TempFile.NewWithExtension("csv"))
			{
				Helper.Wizard.FileName = file.Filename;
				Helper.Wizard.FixedWidth = true;
				string errorMessage;
				Assert(Helper.Wizard.ExportCollection(dummy.Collection, out errorMessage));

				string actualText = File.ReadAllText(file.Filename);
				AssertEquals("AppendExtraNewLineToEndOfFile = N, Fixed Width Newlines are not handled correctly", expectedText, actualText);
				AssertEndsWith("AppendExtraNewLineToEndOfFile = N, There should be no newline at the end of the file", "20SSSN22.111/10/2007", actualText);
			}
		}

		public void TestCustomMapping()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "PPP";
			dummy.Z0_Bool = true;

			var pairList = new CustomMapPairListSetting { Name = "custom", Pairs = [new() { Input = "PPP", Output = "PPX" }] };

			var settings = new DataExportWizardSettings
			{
				Delimiter = "|", AppendExtraNewLineToEndOfFile = false, TextQualifier = "&", FileNameExpression = "\"File\" + obj.Date.ToString(\"yyyy-MM-dd\")",
				Mappings = new List<DataExportWizardMappingSetting>(),
				CustomMapLists = [pairList]
			};

			var wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			wizard.SetSettings(settings);
			wizard.Mapping.Add(new ExportWizardMapping(wizard.CollectionInfo.RowTypes.ElementAt(0).Properties.ElementAt(0), wizard.Mapping) { CustomMapList = "custom" });

			var result = wizard.ExportCollection(new BusinessObject[] { dummy }, Int32.MaxValue);

			AssertEquals(2, result.Count());
			AssertEquals(1, result.ElementAt(0).Length);
			AssertEquals(1, result.ElementAt(1).Length);
			AssertEquals("TxtParent", result.ElementAt(0)[0]);
			AssertEquals("PPX", result.ElementAt(1)[0]);
		}

		public void TestMultiTypeExportCollection()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "PPP";
			dummy.Z0_Bool = true;
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_AnotherDecimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);
			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_AnotherDecimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			var result = wizard.ExportCollection(new BusinessObject[] { dummy, child1, child2 }, Int32.MaxValue);

			AssertEquals(4, result.Count());
			AssertEquals(2, result.ElementAt(0).Length);
			AssertEquals(2, result.ElementAt(1).Length);
			AssertEquals(3, result.ElementAt(2).Length);
			AssertEquals(3, result.ElementAt(3).Length);
			AssertEquals("TxtParent", result.ElementAt(0)[0]);
			AssertEquals("Bool", result.ElementAt(0)[1]);
			AssertEquals("PPP", result.ElementAt(1)[0]);
			AssertEquals("Y", result.ElementAt(1)[1]);
			AssertEquals("TTT", result.ElementAt(2)[0]);
			AssertEquals("1.1", result.ElementAt(2)[1]);
			AssertEquals("10/10/2007", result.ElementAt(2)[2]);
		}

		public void TestMultiTypeExportCollectionIgnoreRowType()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "PPP";
			dummy.Z0_Bool = true;
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_Decimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);
			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_Decimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			wizard.IncludeHeaders = false;
			wizard.RowTypeNameFilter = "DummyChildBusinessObject";
			wizard.MappingView.RemoveAndDeleteAll();
			var result = wizard.ExportCollection(new BusinessObject[] { dummy, child1, child2 }, Int32.MaxValue);

			AssertEquals(1, result.Count());
			AssertEquals(2, result.ElementAt(0).Length);
			AssertEquals("PPP", result.ElementAt(0)[0]);
			AssertEquals("Y", result.ElementAt(0)[1]);
		}

		void AssertExportedLinesWithQuoteOption(DummyBusinessObject dummy, string quoteOption, string line1, string line2, string line3, string line4)
		{
			Helper.Wizard.IncludeHeaders = false;
			Helper.Wizard.UseTextQualifier = quoteOption;

			string[] exportedLines;
			using (TempFile file = TempFile.NewWithExtension("csv"))
			{
				Helper.Wizard.FileName = file.Filename;
				string errorMessage;
				Assert(Helper.Wizard.ExportCollection(dummy.Collection, out errorMessage));
				exportedLines = File.ReadAllLines(file.Filename);
			}

			AssertEquals(4, exportedLines.Length);
			AssertEquals(line1, exportedLines[0]);
			AssertEquals(line2, exportedLines[1]);
			AssertEquals(line3, exportedLines[2]);
			AssertEquals(line4, exportedLines[3]);
		}

		public void TestMultiTypeExportCollectionWithUseTextQualifier()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 10;
			child1.Z0_VarCharMax = "TT T";
			child1.Z0_Bool = true;
			child1.Z0_Short = 1;
			child1.Z0_AnotherDecimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);

			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 20;
			child2.Z0_VarCharMax = "S,SS";
			child2.Z0_Bool = false;
			child2.Z0_Short = 2;
			child2.Z0_AnotherDecimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			DummyChildBusinessObject child3 = dummy.Collection.AddNew();
			child3.Z0_Number = 30;
			child3.Z0_VarCharMax = "UUU";
			child3.Z0_Bool = false;
			child3.Z0_Short = 2;
			child3.Z0_AnotherDecimal = 2.1;
			child3.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			DummyChildBusinessObject child4 = dummy.Collection.AddNew();
			child4.Z0_Number = 40;
			child4.Z0_VarCharMax = "Z\"ZZ";
			child4.Z0_Bool = false;
			child4.Z0_Short = 2;
			child4.Z0_AnotherDecimal = 2.1;
			child4.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			AssertExportedLinesWithQuoteOption(dummy, ExportWizard.Constants.AlwaysQuote,
				"\"10\",\"TT T\",\"Y\",\"1\",\"1.1\",\"10/10/2007\"", "\"20\",\"S,SS\",\"N\",\"2\",\"2.1\",\"11/10/2007\"", "\"30\",\"UUU\",\"N\",\"2\",\"2.1\",\"11/10/2007\"", "\"40\",\"Z\"\"ZZ\",\"N\",\"2\",\"2.1\",\"11/10/2007\"");
			AssertExportedLinesWithQuoteOption(dummy, ExportWizard.Constants.QuoteWhenSpaces,
				"10,\"TT T\",Y,1,1.1,10/10/2007", "20,SSS,N,2,2.1,11/10/2007", "30,UUU,N,2,2.1,11/10/2007", "40,Z\"\"ZZ,N,2,2.1,11/10/2007");
			AssertExportedLinesWithQuoteOption(dummy, ExportWizard.Constants.QuoteWhenDelimiter,
				"10,TT T,Y,1,1.1,10/10/2007", "20,\"S,SS\",N,2,2.1,11/10/2007", "30,UUU,N,2,2.1,11/10/2007", "40,Z\"\"ZZ,N,2,2.1,11/10/2007");
			AssertExportedLinesWithQuoteOption(dummy, ExportWizard.Constants.QuoteWhenSpacesOrDelimiter,
				"10,\"TT T\",Y,1,1.1,10/10/2007", "20,\"S,SS\",N,2,2.1,11/10/2007", "30,UUU,N,2,2.1,11/10/2007", "40,Z\"\"ZZ,N,2,2.1,11/10/2007");
			AssertExportedLinesWithQuoteOption(dummy, ExportWizard.Constants.NeverQuote,
				"10,TT T,Y,1,1.1,10/10/2007", "20,SSS,N,2,2.1,11/10/2007", "30,UUU,N,2,2.1,11/10/2007", "40,Z\"ZZ,N,2,2.1,11/10/2007");
		}

		public void TestExportCollectionWithCondition()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 10;
			child1.Z0_VarCharMax = "TTT";
			child1.Z0_Bool = true;
			child1.Z0_Short = 1;
			child1.Z0_AnotherDecimal = 1.1;
			child1.Z0_Date = new ZDateTime(2007, 10, 10);
			DummyChildBusinessObject child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 20;
			child2.Z0_VarCharMax = "SSS";
			child2.Z0_Bool = false;
			child2.Z0_Short = 2;
			child2.Z0_AnotherDecimal = 2.1;
			child2.Z0_Date = new ZDateTime(2007, 10, 11, 17, 30, 0);

			Helper.Wizard.IncludeHeaders = false;
			Helper.Wizard.Mapping[0].ConditionExpr = "Z0_Bool";
			Helper.Wizard.Mapping[1].ConditionExpr = "not Z0_Bool";
			Helper.Wizard.Mapping[2].ConditionExpr = "not Z0_Bool1";

			var result = Helper.Wizard.ExportCollection(dummy.Collection, Int32.MaxValue);
			AssertEquals(2, result.Count());
			AssertEquals(5, result.ElementAt(0).Length);
			AssertEquals(5, result.ElementAt(0).Length);
			AssertEquals("10", result.ElementAt(0)[0]);
			AssertEquals("ERROR: global name 'Z0_Bool1' is not defined", result.ElementAt(0)[1]);
			AssertEquals("1", result.ElementAt(0)[2]);
			AssertEquals("1.1", result.ElementAt(0)[3]);
			AssertEquals("10/10/2007", result.ElementAt(0)[4]);
			AssertEquals("SSS", result.ElementAt(1)[0]);
			AssertEquals("ERROR: global name 'Z0_Bool1' is not defined", result.ElementAt(1)[1]);
			AssertEquals("2", result.ElementAt(1)[2]);
			AssertEquals("2.1", result.ElementAt(1)[3]);
			AssertEquals("11/10/2007", result.ElementAt(1)[4]);
		}

		public void TestHeaderSerialization()
		{
			Helper.Wizard.Mapping[0].Header = "NumHeader";
			DataExportWizardSettings settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			AssertEquals("NumHeader", settings.Mappings[0].Header);

			settings.Mappings[0].Header = "HeaderNum";
			Helper.Wizard.SetSettings(settings);
			AssertEquals("HeaderNum", Helper.Wizard.Mapping[0].Header);
		}

		public void TestLongValuesForCustomMapPair()
		{
			var pair = new CustomMapPair();
			var inputMaxLength = pair.InputInfo.MaxLength;
			var outputMaxLength = pair.OutputInfo.MaxLength;

			var settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			var pairList1 = new CustomMapPairListSetting { Name = "custom1", Pairs =
				[new CustomMapPairSetting() {
					Input = new string('x', inputMaxLength + 1), Output = "PPX"
				}]
			};
			var pairList2 = new CustomMapPairListSetting { Name = "custom2", Pairs =
				[new CustomMapPairSetting() {
					Input = "AAA", Output = new string('y', outputMaxLength + 1)
				}]
			};
			var pairList3 = new CustomMapPairListSetting { Name = "custom3", Pairs =
				[new CustomMapPairSetting() {
					Input = "AAA", Output = "BBB"
				}]
			};

			Assert("Prerequisite:", pairList1.Pairs[0].Input.Length > inputMaxLength);
			Assert("Prerequisite:", pairList2.Pairs[0].Output.Length > outputMaxLength);
			settings.CustomMapLists = [pairList1,pairList2,pairList3];
			Helper.Wizard.SetSettings(settings);
			settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			AssertEquals(inputMaxLength, settings.CustomMapLists[0].Pairs[0].Input.Length);
			AssertEquals(outputMaxLength, settings.CustomMapLists[1].Pairs[0].Output.Length);
			AssertContains("Values for custom mapping \"custom1\" were truncated", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Values for custom mapping \"custom2\" were truncated", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotContains("Values for custom mapping \"custom3\" were truncated", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAlignmentAndWidthSerialization()
		{
			Helper.Wizard.Mapping[0].Width = 10;
			Helper.Wizard.Mapping[0].Alignment = ExportWizardMapping.FieldAlignment.Right;
			DataExportWizardSettings settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			AssertEquals(10, settings.Mappings[0].Width);
			AssertEquals(ExportWizardMapping.FieldAlignment.Right, settings.Mappings[0].Alignment);

			settings.Mappings[0].Width = 20;
			settings.Mappings[0].Alignment = ExportWizardMapping.FieldAlignment.Left;
			Helper.Wizard.SetSettings(settings);
			AssertEquals(20, Helper.Wizard.Mapping[0].Width);
			AssertEquals(ExportWizardMapping.FieldAlignment.Left, Helper.Wizard.Mapping[0].Alignment);
		}

		public void TestConditionExprSerialization()
		{
			Helper.Wizard.Mapping[0].ConditionExpr = "579";
			DataExportWizardSettings settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			AssertEquals("579", settings.Mappings[0].ConditionExpr);

			settings.Mappings[0].ConditionExpr = "468";
			Helper.Wizard.SetSettings(settings);
			AssertEquals("468", Helper.Wizard.Mapping[0].ConditionExpr);
		}

		public void TestMappingSerialization()
		{
			DataExportWizardSettings settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			AssertEquals(DummyBizoSchema.Z0_Number.Name, settings.Mappings[0].MappingName);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, settings.Mappings[1].MappingName);
			AssertEquals(DummyBizoSchema.Z0_Bool.Name, settings.Mappings[2].MappingName);

			Helper.Wizard.MappingView.MoveSelectedElementsUpDown(
				new ExportWizardMapping[] { Helper.Wizard.Mapping[0], Helper.Wizard.Mapping[1] },
				ExportWizardMappingCollectionView.Direction.Down);

			settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			AssertEquals(DummyBizoSchema.Z0_Bool.Name, settings.Mappings[0].MappingName);
			AssertEquals(DummyBizoSchema.Z0_Number.Name, settings.Mappings[1].MappingName);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, settings.Mappings[2].MappingName);
		}

		public void TestMultiTypeMappingSerialization()
		{
			var wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());

			var settings = (DataExportWizardSettings)wizard.GetSettings();
			AssertEquals(5, settings.Mappings.Count);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, settings.Mappings[0].MappingName);
			AssertEquals(DummyBizoSchema.Z0_Bool.Name, settings.Mappings[1].MappingName);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, settings.Mappings[2].MappingName);
			AssertEquals(DummyBizoSchema.Z0_AnotherDecimal.Name, settings.Mappings[3].MappingName);
			AssertEquals(DummyBizoSchema.Z0_Date.Name, settings.Mappings[4].MappingName);
			AssertEquals("DummyBusinessObject", settings.Mappings[0].RowTypeName);
			AssertEquals("DummyBusinessObject", settings.Mappings[1].RowTypeName);
			AssertEquals("DummyChildBusinessObject", settings.Mappings[2].RowTypeName);
			AssertEquals("DummyChildBusinessObject", settings.Mappings[3].RowTypeName);
			AssertEquals("DummyChildBusinessObject", settings.Mappings[4].RowTypeName);

			var newMappings = settings.Mappings;
			settings.Mappings = newMappings.Take(4).Reverse().ToList();

			wizard.SetSettings(settings);
			AssertEquals(4, wizard.Mapping.Count);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, wizard.Mapping[3].MappingName);
			AssertEquals(DummyBizoSchema.Z0_Bool.Name, wizard.Mapping[2].MappingName);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, wizard.Mapping[1].MappingName);
			AssertEquals(DummyBizoSchema.Z0_AnotherDecimal.Name, wizard.Mapping[0].MappingName);
		}

		public void TestSerializationWithExpression()
		{
			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			wizard.Mapping.RemoveAndDeleteAll();
			wizard.Mapping.Add(new ExportWizardMapping(wizard.CollectionInfo.RowTypes.ElementAt(0).Properties.ElementAt(0), wizard.Mapping));
			wizard.Mapping.Add(new ExportWizardMapping(wizard.CollectionInfo.RowTypes.ElementAt(0), wizard.Mapping) { Expression = "26" });
			wizard.Mapping.Add(new ExportWizardMapping(wizard.CollectionInfo.RowTypes.ElementAt(1), wizard.Mapping) { Expression = "", ConditionExpr = "32" });
			wizard.Mapping.Add(new ExportWizardMapping(wizard.CollectionInfo.RowTypes.ElementAt(1), wizard.Mapping) { Expression = "", ConditionExpr = "", CustomMapList = "custom" });

			DataExportWizardSettings settings = (DataExportWizardSettings)wizard.GetSettings();
			AssertEquals(4, settings.Mappings.Count);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, settings.Mappings[0].MappingName);
			AssertEquals("", settings.Mappings[1].MappingName);
			AssertEquals("", settings.Mappings[2].MappingName);
			AssertEquals("", settings.Mappings[3].MappingName);
			AssertEquals("DummyBusinessObject", settings.Mappings[0].RowTypeName);
			AssertEquals("DummyBusinessObject", settings.Mappings[1].RowTypeName);
			AssertEquals("DummyChildBusinessObject", settings.Mappings[2].RowTypeName);
			AssertEquals("DummyChildBusinessObject", settings.Mappings[3].RowTypeName);
			AssertEquals("", settings.Mappings[0].Expression);
			AssertEquals("26", settings.Mappings[1].Expression);
			AssertEquals("", settings.Mappings[2].Expression);
			AssertEquals("", settings.Mappings[3].Expression);
			AssertEquals("", settings.Mappings[1].ConditionExpr);
			AssertEquals("32", settings.Mappings[2].ConditionExpr);
			AssertEquals("", settings.Mappings[3].ConditionExpr);
			AssertEquals("custom", settings.Mappings[3].CustomMapList);

			wizard.Mapping.RemoveAndDeleteAll();
			wizard.SetSettings(settings);
			AssertEquals(4, wizard.Mapping.Count);
			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, wizard.Mapping[0].MappingName);
			AssertEquals("", wizard.Mapping[1].MappingName);
			AssertEquals("", wizard.Mapping[2].MappingName);
			AssertEquals("", wizard.Mapping[3].MappingName);
			AssertEquals("DummyBusinessObject", wizard.Mapping[0].RowType.Name);
			AssertEquals("DummyBusinessObject", wizard.Mapping[1].RowType.Name);
			AssertEquals("DummyChildBusinessObject", wizard.Mapping[2].RowType.Name);
			AssertEquals("DummyChildBusinessObject", wizard.Mapping[3].RowType.Name);
			AssertEquals("", wizard.Mapping[0].Expression);
			AssertEquals("26", wizard.Mapping[1].Expression);
			AssertEquals("", wizard.Mapping[2].Expression);
			AssertEquals("", wizard.Mapping[3].Expression);
			AssertEquals("", wizard.Mapping[1].ConditionExpr);
			AssertEquals("32", wizard.Mapping[2].ConditionExpr);
			AssertEquals("", wizard.Mapping[3].ConditionExpr);
			AssertEquals("custom", wizard.Mapping[3].CustomMapList);
		}

		public void TestRowTypeNameFilter()
		{
			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());

			AssertEquals("DummyBusinessObject", wizard.RowTypeNameFilter);
			AssertEquals(2, wizard.FilteredProperties.Count);

			wizard.RowTypeNameFilter = "DummyChildBusinessObject";
			AssertEquals(3, wizard.FilteredProperties.Count);

			wizard.RowTypeNameFilter = "";
			AssertEquals(0, wizard.FilteredProperties.Count);

			wizard.RowTypeNameFilter = "DummyBusinessObject";
			AssertEquals(2, wizard.FilteredProperties.Count);
		}

		public void TestRowTypeNameFilterHaveLongCharacter()
		{
			var settingsStorageStub = new Mock<ISettingsStorage>();
			settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(Array.Empty<string>());

			var info = new ExportCollectionInfoImpl(Factory, Array.Empty<BusinessObject>())
			{
				{
					ResString.GetMultilingualString("Datei Header", "Datei Header"), typeof(DummyBusinessObject), new ImportPropertyInfoCollection()
					{
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "String" },
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" },
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_Date) { HeaderText = "Date" },
					}
				},
				{
					ResString.GetMultilingualString("Lastschriftverfahren Batch Header", "Lastschriftverfahren Batch Header"), typeof(DummyBusinessObject), new ImportPropertyInfoCollection()
					{
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "String" },
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" },
					}
				}
			};
			ExportWizard wizard = new ExportWizard(info, settingsStorageStub.Object, new FileMapperForTest());

			wizard.RowTypeNameFilter = "Lastschriftverfahren Batch Header";
			Assert(wizard.RowTypeNameFilterInfo.MaxLength >= wizard.RowTypeNameFilter.Length);
			AssertNotEquals(@"The maximum length of 'RowTypeNameFilter' has been exceeded.
 The maximum length of this property is 32 characters, but 33 were entered. New value: Lastschriftverfahren Batch Header. Old value: Datei Header", ErrorReporter.LastMessageReported);

			AssertExceptionThrown(typeof(MaxLengthExceededException), () => { wizard.RowTypeNameFilter = "Lastschriftverfahren Batch Header Test Test"; });
			AssertMultilineASCIIEquals("Error Reporter Message", @"The maximum length of 'RowTypeNameFilter' has been exceeded.
 The maximum length of this property is 40 characters, but 43 were entered. New value: Lastschriftverfahren Batch Header Test Test. Old value: Lastschriftverfahren Batch Header", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestEvaluateStringExpression()
		{
			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());

			string expression = "\"filename-\" + obj.SL_EventTime.ToString(\"yyyy-MM-dd\") + \".csv\"";

			var log = Factory.New<Business.StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2010, 3, 10, 9, 0, 0);
			}

			System.Reflection.MethodInfo methodInfo = wizard.GetType().GetMethod("EvaluateStringExpression", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Func<BusinessObject, string> func = (Func<BusinessObject, string>)methodInfo.MakeGenericMethod(typeof(string)).Invoke(wizard, new object[] { expression });
			string result = func.Invoke(log);

			AssertEquals("filename-2010-03-10.csv", result);
		}

		public void TestSaveCSVWithUTF8Encoding()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			Helper.Wizard.Mapping[0].Header = "测试";
			Helper.Wizard.EncodingType = ExportWizard.Constants.EncodingTypes.UTF8;

			var error = string.Empty;
			Helper.Wizard.FileName = "TEST.CSV";
			Helper.Wizard.FixedWidth = false;

			AssertEquals(Encoding.UTF8, Helper.Wizard.FileExportEncoding);
			AssertEquals(null, Helper.Wizard.CsvExportByteArrayForTest);

			using (Helper.Wizard.CsvStreamForTest = new MemoryStream())
			{
				var result = Helper.Wizard.ExportCollection(dummy.Collection, out error);

				AssertEquals(true, result);
				var expectedBytes = Encoding.UTF8.GetBytes(
@"﻿测试,Txt,Bool,Short,Decimal,Date
,,N,,,
");
				AssertEquals(expectedBytes, Helper.Wizard.CsvExportByteArrayForTest);
			}
		}

		public void TestSaveCSVWithASCIIEncoding()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			Helper.Wizard.Mapping[0].Header = "测试";

			var error = string.Empty;
			Helper.Wizard.FileName = "TEST.CSV";
			Helper.Wizard.FixedWidth = false;

			AssertEquals("By default is ASCII", Encoding.ASCII, Helper.Wizard.FileExportEncoding);
			AssertEquals(null, Helper.Wizard.CsvExportByteArrayForTest);

			using (Helper.Wizard.CsvStreamForTest = new MemoryStream())
			{
				var result = Helper.Wizard.ExportCollection(dummy.Collection, out error);

				AssertEquals(true, result);
				var expectedBytes = Encoding.ASCII.GetBytes(
@"测试,Txt,Bool,Short,Decimal,Date
,,N,,,
");
				AssertEquals(expectedBytes, Helper.Wizard.CsvExportByteArrayForTest);
			}
		}

		public void TestSaveCSVWithUTF8WithoutBOMEncoding()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = dummy.Collection.AddNew();
			Helper.Wizard.Mapping[0].Header = "测试";
			Helper.Wizard.EncodingType = ExportWizard.Constants.EncodingTypes.UTF8WithoutBOM;

			Helper.Wizard.FileName = "TEST.CSV";
			Helper.Wizard.FixedWidth = false;

			AssertNotEquals(new UTF8Encoding(true), helper.Wizard.FileExportEncoding);
			AssertNotEquals(Encoding.UTF8, helper.Wizard.FileExportEncoding);

			AssertEquals(new UTF8Encoding(), helper.Wizard.FileExportEncoding);
			AssertEquals(new UTF8Encoding(false), helper.Wizard.FileExportEncoding);

			using (var stream = new MemoryStream())
			using (Helper.Wizard.CsvStreamForTest = new MemoryStream())
			{
				var result = Helper.Wizard.ExportCollection(dummy.Collection, out string error);

				AssertEquals(true, result);

				var expectedBytes = new UTF8Encoding(false).GetBytes(@"测试,Txt,Bool,Short,Decimal,Date
,,N,,,
");

				AssertEquals(expectedBytes, Helper.Wizard.CsvExportByteArrayForTest);
			}
		}

		public void TestOtherEncodingTypesAreNotSupported()
		{
			AssertExceptionThrown<NotSupportedException>(() => { Helper.Wizard.EncodingType = "WhatEver"; });
		}

		public void TestEncodingTypeShouldBeStoredInTheSettings()
		{
			Helper.Wizard.EncodingType = ExportWizard.Constants.EncodingTypes.ASCII;
			AssertEquals("ASCII", ((DataExportWizardSettings)Helper.Wizard.GetSettings()).EncodingType);

			Helper.Wizard.EncodingType = ExportWizard.Constants.EncodingTypes.UTF8;
			AssertEquals("UTF-8", ((DataExportWizardSettings)Helper.Wizard.GetSettings()).EncodingType);

			var settings = (DataExportWizardSettings)Helper.Wizard.GetSettings();
			settings.EncodingType = ExportWizard.Constants.EncodingTypes.ASCII;
			Helper.Wizard.SetSettings(settings);
			AssertEquals("ASCII", Helper.Wizard.EncodingType);

			settings.EncodingType = ExportWizard.Constants.EncodingTypes.UTF8;
			Helper.Wizard.SetSettings(settings);
			AssertEquals("UTF-8", Helper.Wizard.EncodingType);
		}

		[TestDate(2019, 01, 02)]
		public void TestEvaluateFileNameExpression()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "PPP";
			dummy.Z0_Date = ZDateTime.Now.Date.AddDays(1);

			var settings = new DataExportWizardSettings
			{
				Delimiter = "|", AppendExtraNewLineToEndOfFile = false, TextQualifier = "&", FileNameExpression = "\"File.\" + obj.Z0_Date.ToString(\"yyyy-MM-dd\") + \".txt\"",
				Mappings = new List<DataExportWizardMappingSetting>(),
				CustomMapLists = []
			};

			var wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			wizard.SetSettings(settings);
			var (result, ex) = wizard.EvaluateFileNameExpression();
			AssertNull("Exception", ex);
			AssertEquals("FileNameExpression should be empty until FileNameExpressionObject is set", ZString.Empty, result);

			wizard.FileNameExpressionObject = dummy;

			var basePath = new FileMapperForTest().GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

			(result, ex) = wizard.EvaluateFileNameExpression();
			AssertNull("Exception", ex);
			var expectedFileName = Path.Combine(basePath, "File.2019-01-03.txt");
			AssertEquals("FileNameExpression should use Z0_Date field from object", expectedFileName, result);

			settings.FileNameExpression = "\"File.\" + obj.Z0_VarCharMax + \".txt\"";
			wizard.SetSettings(settings);
			(result, ex) = wizard.EvaluateFileNameExpression();
			AssertNull("Exception", ex);
			expectedFileName = Path.Combine(basePath, "File.PPP.txt");
			AssertEquals("FileNameExpression should use Z0_VarCharMax field from object", expectedFileName, result);
		}

		[TestDate(2019, 01, 02)]
		public void TestEvaluateFileNameExpression_WhenInvalidPythonCode()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "PPP";

			var settings = new DataExportWizardSettings();
			settings.Delimiter = "|";
			settings.AppendExtraNewLineToEndOfFile = false;
			settings.TextQualifier = "&";
			settings.FileNameExpression = "\\TheFolder\\TheFile.\" + obj.Z0_VarCharMax + \".txt\"";
			settings.Mappings = new List<DataExportWizardMappingSetting>();
			settings.CustomMapLists = [];

			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			wizard.SetSettings(settings);
			wizard.FileNameExpressionObject = dummy;

			var (result, ex) = wizard.EvaluateFileNameExpression();
			AssertNotNull("Exception", ex);
			AssertEquals("Exception.Message", "unexpected token '\\'", ex.Message);
			AssertEquals("FileNameExpression should return empty when invalid Python script", ZString.Empty, result);
		}

		[TestDate(2019, 01, 02)]
		public void TestFileNameExpressionValidation_WhenInvalidPythonCode()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "PPP";

			var settings = new DataExportWizardSettings
			{
				Delimiter = "|", AppendExtraNewLineToEndOfFile = false, TextQualifier = "&", FileNameExpression = "\\TheFolder\\TheFile.\" + obj.Z0_VarCharMax + \".txt\"",
				Mappings = new List<DataExportWizardMappingSetting>(),
				CustomMapLists = []
			};

			ExportWizard wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			wizard.SetSettings(settings);
			AssertNoError(wizard.FileNameExpressionInfo, "unexpected token '\\'");

			wizard.FileNameExpressionObject = dummy;
			AssertHasError(wizard.FileNameExpressionInfo, "unexpected token '\\'");
		}

		public void TestMapAsShouldExportedInDescription()
		{
			var wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());

			var xml = @"<DataExportWizardSettings>
					<Mapping><Name>Z0_Date</Name><MapAs>d</MapAs><RowTypeName>DummyChildBusinessObject</RowTypeName><Header>Date</Header></Mapping>
				</DataExportWizardSettings>";
			var wizardSettings = (DataExportWizardSettings)DataExportWizardSettings.FromXml(xml, wizard.SettingsType, true);
			wizard.SetSettings(wizardSettings);

			AssertEquals("Short Date", wizard.Mapping[0].MapAs);
			AssertEquals("d", ((DataExportWizardSettings)wizard.GetSettings()).Mappings[0].MapAs);
		}

		public void TestShowWarningIfMultiLanguageImport()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var wizard = Helper.GetMultiTypeWizard(Array.Empty<BusinessObject>());
			var xml = @"<DataExportWizardSettings>
					<Mapping><RowTypeName>支票付款页眉</RowTypeName></Mapping>
					<Mapping><MapAs>Code</MapAs><RowTypeName>支票付款页眉</RowTypeName></Mapping>
					<Mapping><Name>TEST3</Name><MapAs>Description3</MapAs><CustomMapList>CustomMapping</CustomMapList><Header>Header3</Header><RowTypeName>支票付款页眉</RowTypeName></Mapping>
				</DataExportWizardSettings>";
			var wizardSettings = (DataExportWizardSettings)DataExportWizardSettings.FromXml(xml, wizard.SettingsType, true);

			wizard.SetSettings(wizardSettings);
			AssertEquals("Imported settings are in a different language.To use it, please re-export.", UnitTestUserNotification.Instance.LastMessage.Text);
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

			ExportWizard wizard = new ExportWizard(Helper.CollectionInfo, settingsStorageStub.Object, new FileMapperForTest());
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

			ExportWizard wizard = new ExportWizard(Helper.CollectionInfo, settingsStorageStub.Object, new FileMapperForTest());
			wizard.Setting = "setting";

			wizard.RemoveSettings();

			AssertEquals("Call CheckPermission", true, hasSecurityRightCalled);
			AssertEquals("We show the security error only when user doesn't have security right", expectedIsDialogWasShown, showSecurityErrorCalled);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.Wizard;
		}

		public override ImportExportWizard NewWizard(ISettingsStorage settingsStorage)
		{
			return new ExportWizard(Helper.CollectionInfo, settingsStorage, new FileMapperForTest());
		}

		ExportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ExportWizardTestHelper(Factory);
				}

				return helper;
			}
		}
		ExportWizardTestHelper helper;

		#endregion

		public class ExportWizardTestHelper
		{
			public ExportWizardTestHelper(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public ExportWizard GetMultiTypeWizard(IEnumerable<BusinessObject> businessObjects)
			{
				var settingsStorageStub = new Mock<ISettingsStorage>();
				settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(Array.Empty<string>());

				return new ExportWizard(GetMultiTypeCollectionInfo(Array.Empty<BusinessObject>()), settingsStorageStub.Object, new FileMapperForTest());
			}

			public ExportWizard Wizard
			{
				get
				{
					if (wizard == null)
					{
						var settingsStorageStub = new Mock<ISettingsStorage>();
						settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(Array.Empty<string>());

						wizard = new ExportWizard(CollectionInfo, settingsStorageStub.Object, new FileMapperForTest());
					}

					return wizard;
				}
			}
			ExportWizard wizard;

			public IExportCollectionInfo CollectionInfo
			{
				get
				{
					if (collectionInfo == null)
					{
						var dummy = factory.New<DummyBusinessObject>();
						dummy.Collection.AddNew();
						dummy.Collection.AddNew();
						collectionInfo = new ImportCollectionInfoImpl(dummy.Collection)
					{
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Num" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "Txt" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Short) { HeaderText = "Short" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_AnotherDecimal) { HeaderText = "Decimal" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Date) { HeaderText = "Date" },
					};
					}

					return collectionInfo;
				}
			}
			IExportCollectionInfo collectionInfo;

			public IExportCollectionInfo GetMultiTypeCollectionInfo(IEnumerable<BusinessObject> businessObjects)
			{
				var info = new ExportCollectionInfoImpl(factory, businessObjects)
			{
				{
					ResString.GetMultilingualString("DummyBusinessObject", "DummyBusinessObject"), typeof(DummyBusinessObject), new ImportPropertyInfoCollection()
					{
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "TxtParent" },
						new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBizoSchema.Constants.Z0_Bool) { HeaderText = "Bool" },
					}
				},
				{
					ResString.GetMultilingualString("DummyChildBusinessObject", "DummyChildBusinessObject"), typeof(DummyChildBusinessObject), new ImportPropertyInfoCollection()
					{
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_VarCharMax) { HeaderText = "Txt" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_AnotherDecimal) { HeaderText = "Decimal" },
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Date) { HeaderText = "Date" },
					}
				}
			};

				return info;
			}

			readonly BusinessObjectFactory factory;
		}
	}
}
