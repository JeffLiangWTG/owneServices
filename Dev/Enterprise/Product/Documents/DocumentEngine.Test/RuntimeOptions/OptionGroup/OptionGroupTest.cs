using System;
using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(OptionGroup))]
	sealed class OptionGroupTest : FitlerFieldTestWithClearValues
	{
		protected override void SetUp()
		{
			base.SetUp();
			og = new OptionGroup(new BusinessObjectFactory());
			og.FieldName = "x";
			og.DisplayName = "og";
		}

		public void TestOnChangedEventTriggered()
		{
			var field = new OptionGroup(Factory);
			field.DisplayName = "zzz";
			field.AddOption("Invoice", "INV");
			field.AddOption("Journal", "JNL");
			field.AddOption("NewOne", "NEW");

			field.DescriptionCodePairList[0].Value = ZBool.True;
			Assert("field should has changes", field.HasChanges);
		}

		public void TestGetFieldDescription()
		{
			var field = new OptionGroup(Factory);
			field.DisplayName = "zzz";
			field.AddOption("Invoice", "INV");
			field.AddOption("Journal", "JNL");
			field.AddOption("NewOne", "NEW");

			var list = ((IDescriptionPairListSupportField)field).DescriptionList;
			list.Sort();
			AssertEquals("Invoice;Journal;NewOne", string.Join(";", list));
		}

		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			OptionGroup field1 = new OptionGroup(Factory);
			field1.DisplayName = "zzz";
			field1.AddOption("Invoice", "INV");
			field1.AddOption("Journal", "JNL");
			field1.AddOption("NewOne", "NEW");

			OptionGroup field2 = new OptionGroup(Factory);
			field2.DisplayName = "zzz";
			field2.AddOption("Invoice", "INV");
			field2.AddOption("Journal", "JNL");
			field2.AddOption("NewOne", "NEW");
			field2.DescriptionCodePairList[0].Value = true;
			field2.DescriptionCodePairList[1].Value = false;
			field2.DescriptionCodePairList[2].Value = true;

			OptionGroup field3 = new OptionGroup(Factory);
			field3.DisplayName = "zzz";
			field3.AddOption("Unknown", "XXX");
			field3.DescriptionCodePairList[0].Value = true;

			AssertEquals(true, field1.IsCompatibleWith(field2));
			AssertEquals(false, field1.IsCompatibleWith(field3));
		}

		OptionGroup og;
		public void TestJsonConverter()
		{
			og.AddOption("Invoice", "INV");
			og.AddOption("Journal", "JNL");
			og.AddOption("NewOne", "NEW");
			og.DescriptionCodePairList[0].Value = true;
			og.DescriptionCodePairList[1].Value = false;
			og.DescriptionCodePairList[2].Value = true;

			var result = JsonConverterHelper.Serialize(og);
			var deserialisedField = JsonConverterHelper.Deserialize<OptionGroup>(result);

			AssertEquals("og", deserialisedField.DisplayName);
			AssertEquals("x", deserialisedField.FieldName);
			AssertEquals("Param count", 2, deserialisedField.SqlParameters().Count);
			AssertEquals("Param 1 value", "INV", deserialisedField.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 2 value", "NEW", deserialisedField.SqlParameters()[1].Value.ToString());
			AssertEquals("Invoice", deserialisedField.DescriptionCodePairList[0].Description);
			AssertEquals(true, deserialisedField.DescriptionCodePairList[0].Value);
			AssertEquals("Journal", deserialisedField.DescriptionCodePairList[1].Description);
			AssertEquals(false, deserialisedField.DescriptionCodePairList[1].Value);
			AssertEquals("NewOne", deserialisedField.DescriptionCodePairList[2].Description);
			AssertEquals(true, deserialisedField.DescriptionCodePairList[2].Value);
		}

		public void TestJsonConverter_CaterForBooleansAsBits()
		{
			og.FieldName = "GS_IsActive";
			og.AddOption("Active Staff Only", "Y");
			og.DescriptionCodePairList[0].Value = true;

			var result = JsonConverterHelper.Serialize(og);
			var deserialisedField = JsonConverterHelper.Deserialize<OptionGroup>(result);

			AssertEquals("Param count", 1, deserialisedField.SqlParameters().Count);
			AssertType<Boolean>("Param 1 value should be a bool", deserialisedField.SqlParameters()[0].Value);
			AssertEquals("Param 1 value", true, (bool)deserialisedField.SqlParameters()[0].Value);
			AssertEquals("Active Staff Only", deserialisedField.DescriptionCodePairList[0].Description);
			AssertEquals(true, deserialisedField.DescriptionCodePairList[0].Value);
		}

		public void TestAddAllOptions()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("item1", "item1");
			list.AddPair("item2", "item2");

			AssertEquals("Should have 0 options", 0, og.DescriptionCodePairList.Count);
			og.AddAllOptions(list);
			AssertEquals("Should have 2 options", 2, og.DescriptionCodePairList.Count);
		}

		public void TestAddAllOptionsWithDuplicates_CaseSensitive()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			AssertEquals(0, list.Count);

			list.AddPair("1", "CaseSensitive");
			list.AddPair("2", "CASESENSITIVE");

			AssertEquals("Successfully added", 2, list.Count);
			AssertNoExceptionThrown(() => og.AddAllOptions(list));
		}

		public void TestAddAllOptionsWithDuplicates()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			list.AddPair("AAA", "item");
			list.AddPair("BBB", "item");
			list.AddPair("CCC", "item");
			list.AddPair("DDD", "item (BBB)");
			list.AddPair("EEE", "item (CCC)");
			list.AddPair("FFF", "item (BBB) (DDD)");
			list.AddPair("GGG", "item (BBB) (DDD) (FFF)");
			list.AddPair("HHH", "item (CCC) (EEE)");
			list.AddPair("III", "item 123");
			list.AddPair("III", "item 123 with another description");

			AssertEquals("Should have 0 options", 0, og.DescriptionCodePairList.Count);
			og.AddAllOptions(list);
			AssertEquals("Should have 9 options", 9, og.DescriptionCodePairList.Count);

			AssertEquals("item (AAA)", og.DescriptionCodePairList[0].Description);
			AssertEquals("item (BBB)", og.DescriptionCodePairList[1].Description);
			AssertEquals("item (CCC)", og.DescriptionCodePairList[2].Description);
			AssertEquals("item (BBB) (DDD)", og.DescriptionCodePairList[3].Description);
			AssertEquals("item (CCC) (EEE)", og.DescriptionCodePairList[4].Description);
			AssertEquals("item (BBB) (DDD) (FFF)", og.DescriptionCodePairList[5].Description);
			AssertEquals("item (BBB) (DDD) (FFF) (GGG)", og.DescriptionCodePairList[6].Description);
			AssertEquals("item (CCC) (EEE) (HHH)", og.DescriptionCodePairList[7].Description);
			AssertEquals("item 123", og.DescriptionCodePairList[8].Description);

			var reversedList = new CodeDescriptionPairList();
			for (int i = list.Count; --i >= 0;)
			{
				reversedList.Add(list[i]);
			}

			OptionGroup og2 = new OptionGroup(new BusinessObjectFactory());
			og2.FieldName = "y";
			og2.DisplayName = "og2";
			og2.AddAllOptions(reversedList);
			AssertEquals("Should have 9 options", 9, og2.DescriptionCodePairList.Count);

			AssertEquals("item 123 with another description", og2.DescriptionCodePairList[0].Description);
			AssertEquals("item (CCC) (EEE) (HHH)", og2.DescriptionCodePairList[1].Description);
			AssertEquals("item (BBB) (DDD) (FFF) (GGG)", og2.DescriptionCodePairList[2].Description);
			AssertEquals("item (BBB) (DDD) (FFF)", og2.DescriptionCodePairList[3].Description);
			AssertEquals("item (CCC) (EEE)", og2.DescriptionCodePairList[4].Description);
			AssertEquals("item (BBB) (DDD)", og2.DescriptionCodePairList[5].Description);
			AssertEquals("item (CCC)", og2.DescriptionCodePairList[6].Description);
			AssertEquals("item (BBB)", og2.DescriptionCodePairList[7].Description);
			AssertEquals("item (AAA)", og2.DescriptionCodePairList[8].Description);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestAddAllOptionsMultipleCalls()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("AAA", "item a");
			list.AddPair("BBB", "item b");

			og.AddAllOptions(list);
			og.AddAllOptions(list);
		}

		public void TestDefaultEmpty()
		{
			AssertEquals("Should have 0 options", 0, og.DescriptionCodePairList.Count);
			AssertEquals("Should be empty by default", "", og.WhereClause());
		}

		public void TestWhereClauseOneOption()
		{
			og.AddOption("foo", "bar");
			AssertEquals("Should be no where clause if no options are selected", "", og.WhereClause());
			og.DescriptionCodePairList[0].Value = true;
			Match whereClauseMatch = Regex.Match(og.WhereClause(), @"\(x = (@p[0-9]+)\)");
			Assert("Where clause should be <(x = @p123)>, was: " + og.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param count", 1, og.SqlParameters().Count);
			AssertEquals("Param value", "bar", og.SqlParameters()[0].Value.ToString());
			AssertEquals("Param name", whereClauseMatch.Groups[1].Value, og.SqlParameters()[0].ToString());
		}

		public void TestWhereClauseThreeOptions()
		{
			og.AddOption("foo", "bar");
			og.AddOption("cat", "dog");
			og.AddOption("egg", "tot");
			AssertEquals("Should be no where clause if no options are selected", "", og.WhereClause());
			og.DescriptionCodePairList[0].Value = true;
			og.DescriptionCodePairList[2].Value = true;
			Match whereClauseMatch = Regex.Match(og.WhereClause(), @"\(x = (@p[0-9]+)\) OR \(x = (@p[0-9]+)\)");
			Assert("Where clause should be <(x = @p123) OR (x = @p789)>, was: " + og.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param count", 2, og.SqlParameters().Count);
			AssertEquals("Param 1 value", "bar", og.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 2 value", "tot", og.SqlParameters()[1].Value.ToString());
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, og.SqlParameters()[0].ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, og.SqlParameters()[1].ToString());
		}

		public void TestCommaSeperatedValues()
		{
			og.AddOption("foo", "bar");
			og.AddOption("cat", "dog");
			og.AddOption("egg", "tot");
			AssertEquals("Should be no where clause if no options are selected", "", og.WhereClause());
			og.DescriptionCodePairList[0].Value = true;
			og.DescriptionCodePairList[2].Value = true;
			int providersFound = 0;
			foreach (ValueProvider provider in og.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<og.CommaSeperatedValues>", Passes.FirstPass))
				{
					AssertEquals("\"bar\" , \"tot\"", provider.GetReplacement("<og.CommaSeperatedValues>", new Report(null, null)));
					providersFound++;
				}
			}
			AssertEquals(1, providersFound);
		}

		public override void TestSafeCopyValuesFrom()
		{
			OptionGroup source = new OptionGroup(Factory);
			source.DescriptionCodePairList.Add(new ZBoolDescriptionPair("abc", true));
			OptionGroup destination = new OptionGroup(Factory);
			destination.DescriptionCodePairList.Add(new ZBoolDescriptionPair("abc", false));
			destination.SafeCopyValuesFrom(source);
			AssertEquals("Count", 1, destination.DescriptionCodePairList.Count);
			AssertEquals("Value", true, destination.DescriptionCodePairList["abc"].Value);
		}

		public override void TestClearValues()
		{
			OptionGroup field = new OptionGroup(Factory);
			field.DescriptionCodePairList.Add(new ZBoolDescriptionPair("abc", true));
			field.ClearValues();
			AssertEquals(false, field.DescriptionCodePairList[0].Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			og.AddOption("Invoice", "INV");
			og.AddOption("Journal", "JNL");
			og.DescriptionCodePairList[0].Value = true;
			og.DescriptionCodePairList[1].Value = true;
			ArrayList results = new ArrayList();
			results.Add(og);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get
			{
				return 1;
			}
		}

		#region TestCheckOptionPK

		public void TestCheckOptionPK()
		{
			OptionGroup group1 = new OptionGroup(new BusinessObjectFactory());
			group1.FieldName = "x";
			group1.DisplayName = "Display name for X";
			group1.AddOption("Option #1", "1");
			group1.AddOption("Option #2", "2");
			group1.AddOption("Option #3", "3");
			AssertEquals("Options Count for Group 1", 3, group1.DescriptionCodePairList.Count);
			ZBoolDescriptionPair group1option1 = group1.DescriptionCodePairList[0];
			AssertEquals("Group 1, 'Option #1' DisplayName", "Option #1", group1option1.Description);

			OptionGroup group2 = new OptionGroup(new BusinessObjectFactory());
			group2.FieldName = "Y";
			group2.DisplayName = "Display name for Y";
			group2.AddOption("Option #1", "1");
			group2.AddOption("Option #B", "2");
			AssertEquals("Options Count for Group 2", 2, group2.DescriptionCodePairList.Count);
			ZBoolDescriptionPair group2option1 = group2.DescriptionCodePairList[0];
			AssertEquals("Group 2, 'Option #1' DisplayName", "Option #1", group2option1.Description);

			AssertNotEquals("'Option #1' from Group 1 & 2 should have different PKs", group1option1.PK, group2option1.PK);
		}

		#endregion

		#region TestBindableBooleanItems

		public void TestBindableBooleanItems()
		{
			og.AddOption("Option #1", "1");
			og.AddOption("Option #2", "2");
			og.AddOption("Option #3", "3");

			AssertEquals("Arrays should be of the same size", og.DescriptionCodePairList.Count, og.BindableBooleanItems.Count);
			for (int i = 0; i < og.DescriptionCodePairList.Count; i++)
			{
				AssertEquals("Items should be the same", og.DescriptionCodePairList[i].Description, og.BindableBooleanItems[i].Text);
			}
		}

		#endregion

		public void TestDescriptionToCodeMapper_CaseSensitive()
		{
			og.AddOption("description", "BBB");
			og.AddOption("DESCRIPTION", "AAA");
			AssertEquals("AAA", og.GetCodeFromDescription("DESCRIPTION"));
			AssertEquals("BBB", og.GetCodeFromDescription("description"));
			AssertNull(og.GetCodeFromDescription("DESCRiption"));
		}
	}
}
