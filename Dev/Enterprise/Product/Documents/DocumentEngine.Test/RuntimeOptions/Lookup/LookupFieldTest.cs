using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(LookupField))]
	sealed class LookupFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			LookupField field1 = new LookupField(Factory);
			field1.DisplayName = "zzz";
			field1.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			LookupField field2 = new LookupField(Factory);
			field2.DisplayName = "zzz";
			field2.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			LookupField field3 = new LookupField(Factory);
			field3.DisplayName = "zzz";
			field3.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode));

			AssertEquals(true, field1.IsCompatibleWith(field2));
			AssertEquals(false, field1.IsCompatibleWith(field3));
		}

		public void TestJsonConverter()
		{
			var field = new LookupField(Factory);
			field.FieldName = "zzz";
			field.Value = Guid.NewGuid();
			field.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<LookupField>(result);

			AssertEquals(field.Value, deserialisedField.Value);
			AssertEquals("zzz", deserialisedField.FieldName);
			AssertEquals(CollectionProviderTypeCodeDescriptionList.Codes.Creditor, CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(deserialisedField.CollectionProvider.GetType()));
			AssertEquals(true, deserialisedField.HasSerialisableValueChanged);
		}

		public void TestSql()
		{
			LookupField field = new LookupField(Factory);
			field.FieldName = "zzz";
			field.Value = Guid.NewGuid();
			Match whereClauseMatch = Regex.Match(field.WhereClause(), @"zzz = (@p[0-9]+)");
			Assert("Where clause should be of the form <zzz = @p1234>, but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Should have one parameter", 1, field.SqlParameters().Count);
			AssertEquals("Param 1's name should match the name in the where clause", whereClauseMatch.Groups[1].Value, field.SqlParameters()[0].ToString());
			AssertEquals("Param 1's value should be set", field.Value, field.SqlParameters()[0].Value);
		}

		public void TestValueAsString()
		{
			Guid testPK = Guid.NewGuid();
			Guid testPK2 = Guid.NewGuid();

			LookupField field = new LookupField(new BusinessObjectFactory());
			field.Value = testPK;
			AssertEquals("ValueAsString", testPK.ToString(), field.ValueAsStringForSerialisation);

			field.ValueAsStringForSerialisation = testPK2.ToString();
			AssertEquals("Value", testPK2, field.Value);
		}

		public void TestValueAsString_ForEmptyGuid()
		{
			LookupField field = new LookupField(new BusinessObjectFactory());
			field.ValueAsStringForSerialisation = "";
			AssertEquals(Guid.Empty, field.Value);

			field.ValueAsStringForSerialisation = Guid.Empty.ToString();
			AssertEquals(Guid.Empty, field.Value);
		}

		public void TestHasSerialisableValueChangedWhenChangingValue()
		{
			LookupField field = new LookupField(new BusinessObjectFactory());
			AssertEquals("Has not changed", false, field.HasSerialisableValueChanged);
			field.Value = field.Value;
			AssertEquals("Has not changed", false, field.HasSerialisableValueChanged);
			field.Value = Guid.NewGuid();
			AssertEquals("Has changed", true, field.HasSerialisableValueChanged);
		}

		public void TestHasChanges()
		{
			LookupField field = new LookupField(new BusinessObjectFactory());
			Guid testGuid = Guid.NewGuid();
			field.Value = testGuid;
			AssertEquals("HasChanges", true, field.HasChanges);

			field.HasChanges = false;
			field.Value = testGuid;
			AssertEquals("HasChanges", false, field.HasChanges);
		}

		public void TestOnLookupGuidChanged()
		{
			RefUNLOCO loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			LookupField field = new LookupField(new BusinessObjectFactory());
			((IGuidChangeNotifier)field).LookupGuidChanged += new LookupGuidChanged(LookupFieldTest_OnLookupGuidChanged);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Unloco);
			field.SetCollectionProvider(collectionProvider);
			Guid testGuid = loco.PK.ToGuid();
			LookupFieldGuid = ZGuid.Invalid;
			field.ZValue = testGuid;
			AssertEquals(testGuid, LookupFieldGuid);
		}

		public void TestGetCodeDescriptionForGUID()
		{
			RefUNLOCO loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			LookupField field = new LookupField(new BusinessObjectFactory());
			((IGuidChangeNotifier)field).LookupGuidChanged += new LookupGuidChanged(LookupFieldTest_OnLookupGuidChanged);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Unloco);
			field.SetCollectionProvider(collectionProvider);
			Guid testGuid = loco.PK.ToGuid();
			ICodeDescription pKCodeDescription = field.GetCodeDescriptionForGUID(loco.PK);
			AssertEquals("Code", loco.RL_Code, pKCodeDescription.Code);
			AssertEquals("PK", loco.PK, pKCodeDescription.PK);
			AssertEquals("Description", loco.RL_PortName, pKCodeDescription.Description);
		}

		public void TestErrorWhenInvalid()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			LookupField field = new LookupField(factory);
			RefCountryCollection collection = new RefCountryCollection(factory);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Country);
			field.SetCollectionProvider(collectionProvider);

			RefCountry countryToTest = collection[0];
			field.DisplayName = "TestLookupField";

			field.Value = countryToTest.PK.ToGuid();
			field.ValidateZValue();
			AssertEquals(0, field.ZValueInfo.Notifications.Count());

			field.ZValue = ZGuid.Invalid;
			field.ValidateZValue();
			AssertEquals(1, field.ZValueInfo.Notifications.Count());
			AssertEquals("Enter a valid selection.", field.ZValueInfo.Notifications.First().Message);

			field.ZValue = ZGuid.Empty;
			field.ValidateZValue();
			AssertEquals(0, field.ZValueInfo.Notifications.Count());

			field.ZValue = countryToTest.PK;
			field.ValidateZValue();
			AssertEquals(0, field.ZValueInfo.Notifications.Count());
		}

		public void TestIsResponsibleForCode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			LookupField field = new LookupField(factory);
			RefCountryCollection collection = new RefCountryCollection(factory);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Country);
			field.SetCollectionProvider(collectionProvider);

			RefCountry countryToTest = collection[0];
			field.Value = countryToTest.PK.ToGuid();
			field.DisplayName = "TestLookupField";

			int responsibleCount = 0;
			foreach (ValueProvider provider in field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestLookupField.Code>", Passes.FirstPass))
				{
					AssertEquals(countryToTest.RN_Code, provider.GetReplacement("<TestLookupField.Code>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.Code>!", 1, responsibleCount);
		}

		public void TestIsResponsibleForDescription()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			LookupField field = new LookupField(factory);
			RefCountryCollection collection = new RefCountryCollection(factory);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Country);
			field.SetCollectionProvider(collectionProvider);
			RefCountry countryToTest = collection[0];
			field.Value = countryToTest.PK.ToGuid();
			field.DisplayName = "TestLookupField";

			int responsibleCount = 0;
			foreach (ValueProvider provider in field.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestLookupField.Description>", Passes.FirstPass))
				{
					AssertEquals(countryToTest.RN_Desc, provider.GetReplacement("<TestLookupField.Description>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.Description>!", 1, responsibleCount);
		}

		public void TestSetBindToAndModuleID()
		{
			CodeLookupField field = new CodeLookupField(Factory);
			AssertNull("BindToList should be null by default", field.BindToList);
			AssertNull("ModuleID should be null by default", field.ModuleID);

			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Address);
			field.SetCollectionProvider(collectionProvider);
			AssertEquals("BindToList should now be the OrgAddressCollection passed in", typeof(OrgAddressCollection), field.BindToList.GetType());
			AssertEquals("ModuleID should now be set", ModuleIDs.OrgAddresses, field.ModuleID);
		}

		public override void TestSafeCopyValuesFrom()
		{
			LookupField source = new LookupField(Factory);
			source.Value = Env.CurrentCompany.PK;
			LookupField destination = new LookupField(Factory);
			((IFilter)destination).SafeCopyValuesFrom(source);
			AssertEquals(source.Value, destination.Value);
		}

		public override void TestClearValues()
		{
			LookupField field = new LookupField(Factory);
			field.Value = Env.CurrentCompany.PK;
			((IFilter)field).ClearValues();
			AssertEquals(Guid.Empty, field.Value);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			LookupField field = new LookupField(factory);
			RefCountryCollection collection = new RefCountryCollection(factory);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(factory, CollectionProviderTypeCodeDescriptionList.Codes.Country);
			field.SetCollectionProvider(collectionProvider);

			RefCountry countryToTest = collection[0];
			field.Value = countryToTest.PK.ToGuid();
			field.DisplayName = "TestLookupField";
			ArrayList results = new ArrayList();
			results.Add(field);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases => 1;

		ZGuid LookupFieldGuid;
		void LookupFieldTest_OnLookupGuidChanged(ICodeDescription change)
		{
			LookupFieldGuid = new ZGuid(change.PK);
		}
	}
}
