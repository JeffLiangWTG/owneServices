using System;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(CodeLookupField))]
	sealed class CodeLookupFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestCodeValidation()
		{
			CodeLookupField testField = new CodeLookupField(Factory);
			testField.DisplayName = "zzz";
			testField.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Unloco));

			Globals.IsWeb = false;
			testField.ZValue = ZString.Empty;
			testField.ValidateZValue();
			Assert(!testField.ZValueInfo.HasError("Invalid Selection"));

			testField.ZValue = "TEST";
			testField.ValidateZValue();
			Assert(!testField.ZValueInfo.HasError("Invalid Selection"));

			testField.ZValue = "USORD";
			testField.ValidateZValue();
			Assert(!testField.ZValueInfo.HasError("Invalid Selection"));

			Globals.IsWeb = true;
			testField.ZValue = ZString.Empty;
			testField.ValidateZValue();
			Assert(!testField.ZValueInfo.HasError("Invalid Selection"));

			testField.ZValue = "TEST";
			testField.ValidateZValue();
			Assert(testField.ZValueInfo.HasError("Invalid Selection"));

			testField.ZValue = "USORD";
			testField.ValidateZValue();
			Assert(!testField.ZValueInfo.HasError("Invalid Selection"));
		}

		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			CodeLookupField field1 = new CodeLookupField(Factory);
			field1.DisplayName = "zzz";
			field1.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			CodeLookupField field2 = new CodeLookupField(Factory);
			field2.DisplayName = "zzz";
			field2.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			CodeLookupField field3 = new CodeLookupField(Factory);
			field3.DisplayName = "zzz";
			field3.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode));

			AssertEquals(true, field1.IsCompatibleWith(field2));
			AssertEquals(false, field1.IsCompatibleWith(field3));
		}

		public void TestJsonConverter()
		{
			var field = new CodeLookupField(new BusinessObjectFactory());
			field.Value = "ValueData";
			field.FieldName = "FieldName";
			field.DisplayName = "Json Test";
			field.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<CodeLookupField>(result);

			AssertEquals("ValueData", deserialisedField.Value);
			AssertEquals("FieldName", deserialisedField.FieldName);
			AssertEquals("Json Test", deserialisedField.DisplayName);
			AssertEquals(CollectionProviderTypeCodeDescriptionList.Codes.Creditor, CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(deserialisedField.CollectionProvider.GetType()));
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);
		}

		public void TestSql()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			field.FieldName = "zzz";
			field.Value = "splaty";
			Match whereClauseMatch = Regex.Match(field.WhereClause(), @"(@p[0-9]+) IN \(zzz\)");
			Assert("Where clause should be of the form <@p1234 IN (zzz)>, but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", field.Value, field.SqlParameters()[0].Value);
			AssertEquals("Correct Parameter type", DbType.AnsiString, field.SqlParameters()[0].DbType);
		}

		public void TestSqlMultifield()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			field.FieldName = "xxx, yyy, zzz";
			field.Value = "splaty";
			Match whereClauseMatch = Regex.Match(field.WhereClause(), @"(@p[0-9]+) IN \(xxx, yyy, zzz\)");
			Assert("Where clause should be of the form <@p1234 IN (xxx, yyy, zzz)>, but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", field.Value, field.SqlParameters()[0].Value);
			AssertEquals("Correct Parameter type", DbType.AnsiString, field.SqlParameters()[0].DbType);
		}

		public void TestValueAsString()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			field.Value = "value1";
			AssertEquals("ValueAsString", "value1", field.ValueAsStringForSerialisation);
			field.ValueAsStringForSerialisation = "value2";

			AssertEquals("Value", "value2", field.Value);
			AssertEquals("HasChanges should be true", true, field.HasChanges);
			AssertEquals("HasSerialisableValueChanged should be true", true, field.HasSerialisableValueChanged);
		}

		public void TestZValue()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			field.Value = "value1";
			AssertEquals("ZValue", "value1", field.ZValue);
			field.ZValue = "value2";

			AssertEquals("Value", "value2", field.Value);
			AssertEquals("HasChanges should be true", true, field.HasChanges);
			AssertEquals("HasSerialisableValueChanged should be true", true, field.HasSerialisableValueChanged);
		}

		public void TestHasSerialisableValueChangedWhenChangingValue()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			AssertEquals("Has not changed", false, field.HasSerialisableValueChanged);
			field.Value = field.Value;
			AssertEquals("Has not changed", false, field.HasSerialisableValueChanged);
			field.Value = "splaty";
			AssertEquals("Has changed", true, field.HasSerialisableValueChanged);
		}

		public void TestHasChanges()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			field.Value = "splaty";
			AssertEquals("HasChanges", true, field.HasChanges);

			field.HasChanges = false;
			field.Value = "splaty";
			AssertEquals("HasChanges", false, field.HasChanges);
		}

		public void TestIsEmpty()
		{
			Globals.IsWeb = true;
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			AssertEquals(field.IsEmpty, true);

			field.ZValue = "splaty";
			AssertEquals(field.IsEmpty, false);

			field.ZValue = "";
			AssertEquals(field.IsEmpty, true);
		}

		public void TestSetBindToAndModuleID()
		{
			CodeLookupField field = new CodeLookupField(Factory);
			AssertNull("BindToList should be null by default", field.BindToList);
			AssertNull("ModuleID should be null by default", field.ModuleID);

			OrgHeaderCollection orgs = new OrgHeaderCollection(Factory);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Vessel);
			field.SetCollectionProvider(provider);
			AssertEquals("BindToList should now be the orgs passed in", typeof(RefVesselCollection), field.BindToList.GetType());
			AssertEquals("ModuleID should now be set", ModuleIDs.RefVessel, field.ModuleID);
		}

		public override void TestSafeCopyValuesFrom()
		{
			CodeLookupField source = new CodeLookupField(Factory);
			source.Value = "this should be copied";
			CodeLookupField destination = new CodeLookupField(Factory);
			((IFilter)destination).SafeCopyValuesFrom(source);
			AssertEquals(source.Value, destination.Value);
		}

		public override void TestClearValues()
		{
			CodeLookupField field = new CodeLookupField(Factory);
			field.Value = "some value";
			((IFilter)field).ClearValues();
			AssertEquals(String.Empty, field.Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			CodeLookupField field = new CodeLookupField(new BusinessObjectFactory());
			field.Value = "splaty";
			ArrayList results = new ArrayList();
			results.Add(field);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get
			{
				return 1;
			}
		}
	}
}
