using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(MultipleSelectionLookup))]
	sealed class MultipleSelectionLookupTest : FitlerFieldTestWithClearValues
	{
		public void TestSetValueAsStringForSerialisationWithInvalidStringShouldNotThrowException()
		{
			var pk = Guid.NewGuid().ToString();

			var field = new MultipleSelectionLookup(Factory);
			field.SetCollectionProvider(new OrgHeaderCollectionProvider(Factory));
			AssertNoExceptionThrown(() => field.ValueAsStringForSerialisation = pk);
			Assert(field.BindToList.HasErrors());
			AssertHasRowErrorContaining(field.BindToList[0] as BusinessObject, "No match has been found for this code. Please remove it");

			field = new MultipleSelectionLookup(Factory);
			field.SetCollectionProvider(new LocationCollectionProvider(Factory));
			AssertNoExceptionThrown(() => field.ValueAsStringForSerialisation = pk);
			Assert(field.BindToList.HasErrors());
			AssertHasRowErrorContaining(field.BindToList[0] as BusinessObject, "No match has been found for this code. Please remove it");
		}

		public void TestCodePropertyIsNotSchemaColumn()
		{
			var field = new MultipleSelectionLookup(Factory);
			var provider = new DummyCollectionProvider(Factory);

			field.SetCollectionProvider(provider);
			field.ValueAsStringForSerialisation = "DUM - 99";
			AssertEquals("Collection should not be empty", 1, field.BindToList.Count);

			Assert(((BusinessObject)field.BindToList[0]).HasErrors);
			AssertEquals("No match has been found for this code. Please remove it", ((BusinessObject)field.BindToList[0]).RowErrors.GetFirstMessage());

			var testBO = Factory.New<DummyBusinessObjectCodePropertyIsNotSchemaColumn>();
			testBO.Z0_Code = "DUM";
			testBO.Z0_Number = 10;

			Factory.Save();

			var provider1 = new DummyCollectionProvider(Factory);
			field.SetCollectionProvider(provider1);

			field.ValueAsStringForSerialisation = "DUM - 99";
			AssertEquals("Collection should not be empty", 1, field.BindToList.Count);

			Assert(!((BusinessObject)field.BindToList[0]).HasErrors);
			AssertEquals(testBO.PK, ((BusinessObject)field.BindToList[0]).PK);
		}

		public void TestFieldSpecificsIsCompatibleWithOtherField()
		{
			MultipleSelectionLookup field1 = new MultipleSelectionLookup(Factory);
			field1.DisplayName = "zzz";
			field1.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			MultipleSelectionLookup field2 = new MultipleSelectionLookup(Factory);
			field2.DisplayName = "zzz";
			field2.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Creditor));

			MultipleSelectionLookup field3 = new MultipleSelectionLookup(Factory);
			field3.DisplayName = "zzz";
			field3.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode));

			AssertEquals(true, field1.IsCompatibleWith(field2));
			AssertEquals(false, field1.IsCompatibleWith(field3));
		}

		public void TestCompositeCollection()
		{
			MultipleSelectionLookup field = new MultipleSelectionLookup(Factory);
			field.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Location));
			field.SerialisedByPK = false;
			field.ValueAsStringForSerialisation = "AUSYD, AU, AFOR";
			AssertEquals("Collection should not be empty", 3, field.BindToList.Count);
			var port = field.BindToList.ToArray().OfType<RefUNLOCO>().Single();
			AssertEquals("AUSYD", port.RL_Code);
			var country = field.BindToList.ToArray().OfType<RefCountry>().Single();
			AssertEquals("AU", country.RN_Code);
			var zone = field.BindToList.ToArray().OfType<RefZoneHeader>().Single();
			AssertEquals("AFOR", zone.FZ_Code);
		}

		public void TestClearMultipleValues()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(GetCollectionProviderWithMultipleOrganisation());
			AssertEquals("Collection should not be empty", 3, lookup.BindToList.Count);
			((IFilter)lookup).ClearValues();
			AssertEquals("Collection should be empty", 0, lookup.BindToList.Count);
		}

		public void TestStyles()
		{
			var actualList = typeof(MultipleSelectionLookup.Styles).GetEnumNames();
			AssertArrayEqualsByElements<string>(new string[] { "Grid", "None" }, actualList);
		}

		public void TestRequiredValidation()
		{
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.DisplayName = "TestLookupField";
			lookup.SetCollectionProvider(provider);
			lookup.Validators.Add(new ValidatorPack().RequiredFilter);

			var orgCode = Factory.LoadTop1<OrgHeader>(new ZQuery()).OH_Code;

			lookup.ValueAsStringForSerialisation = orgCode;
			lookup.RunPreSaveValidation();
			AssertNoRowError(lookup, "Should be no row errors because mandatory filter has values");

			((BusinessObjectCollection)lookup.BindToList).RemoveAll();
			lookup.RunPreSaveValidation();
			AssertHasRowError("Should have error", lookup, "'TestLookupField' should have data.");

			lookup.ValueAsStringForSerialisation = orgCode;
			lookup.RunPreSaveValidation();
			AssertNoRowError(lookup, "Should be no row errors (again) because mandatory filter has values");

			lookup = new MultipleSelectionLookup(Factory);
			lookup.DisplayName = "TestLookupField";
			lookup.SetCollectionProvider(provider);
			lookup.Validators.Add(new ValidatorPack().RequiredFilter);

			((BusinessObjectCollection)lookup.BindToList).RemoveAll();
			lookup.RunPreSaveValidation();
			AssertHasRowError("Should have error", lookup, "'TestLookupField' should have data.");

			lookup.ValueAsStringForSerialisation = orgCode;
			lookup.RunPreSaveValidation();
			AssertNoRowError(lookup, "Should be no row errors because mandatory filter has values");

			((BusinessObjectCollection)lookup.BindToList).RemoveAll();
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";

			lookup.MaxAllowableSelections = 1;
			lookup.ValueAsStringForSerialisation = "ABCDEFG, HIJKLMNOP";
			AssertEquals("Lookup should now have 2 orgs in the bindtolist", 2, lookup.BindToList.Count);
			lookup.RunPreSaveValidation();
			AssertHasRowError("Should have error", lookup, "You have chosen 2 items. Please choose up to 1 items or consider other filter criteria.");
			lookup.ValueAsStringForSerialisation = "ABCDEFG";
			lookup.RunPreSaveValidation();
			AssertNoRowError(lookup, "Should be no row errors because you have chosen no more 1 items");
		}

		public void TestSerializationWithNonMatchingCodes()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";

			var bindToCollection = new OrgHeaderCollection(Factory);
			var findboxCollection = new OrgHeaderCollection(Factory);
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			var lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			lookup.ValueAsStringForSerialisation = "SANGO";

			Assert(lookup.BindToList.HasNotifications());
		}

		public void TestJsonConverterWithCodes()
		{
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode);

			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "AAAAA";
			provider.Collection.Add(ref1);

			var ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "BBBBB";
			provider.Collection.Add(ref2);
			Factory.Save();

			var mSLField = new MultipleSelectionLookup(Factory);
			mSLField.SetCollectionProvider(provider);

			mSLField.FieldName = "LookupFieldName";
			AssertJsonConverterWithCodes(2, "AAAAA, BBBBB", "LookupFieldName", false, false);

			mSLField.IsFilterValueExcluded = true;
			AssertJsonConverterWithCodes(2, "AAAAA, BBBBB", "LookupFieldName", true, false);

			mSLField.UseCodesForWhereClause = true;
			AssertJsonConverterWithCodes(2, "AAAAA, BBBBB", "LookupFieldName", true, true);

			void AssertJsonConverterWithCodes(int collectionCount, string valueAsStringForSerialisation, string fieldName, bool isFilterValueExcluded, bool useCodesForWhereClause)
			{
				var result = JsonConverterHelper.Serialize(mSLField);
				var deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);

				AssertEquals(collectionCount, deserialisedField.CollectionProvider.Collection.Count);
				AssertEquals(valueAsStringForSerialisation, deserialisedField.ValueAsStringForSerialisation);
				AssertEquals(fieldName, deserialisedField.FieldName);
				AssertEquals(isFilterValueExcluded, deserialisedField.IsFilterValueExcluded);
				AssertEquals(useCodesForWhereClause, deserialisedField.UseCodesForWhereClause);
			}
		}

		public void TestJsonConverterWithCodes_WithNotMatchingBizoAndLongestPossibleCode()
		{
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode);

			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "AAAAAAAAAA";
			provider.Collection.Add(ref1);

			var ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "BBBBBBBBBB";
			provider.Collection.Add(ref2);
			Factory.Save();

			var mSLField = new MultipleSelectionLookup(Factory);
			mSLField.SetCollectionProvider(provider);

			mSLField.FieldName = "LookupFieldName";

			var result = JsonConverterHelper.Serialize(mSLField);
			var deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);

			AssertEquals(2, deserialisedField.CollectionProvider.Collection.Count);
			AssertEquals("AAAAAAAAAA, BBBBBBBBBB", deserialisedField.ValueAsStringForSerialisation);
			AssertEquals("LookupFieldName", deserialisedField.FieldName);
			AssertEquals(false, deserialisedField.IsFilterValueExcluded);

			mSLField.IsFilterValueExcluded = true;

			result = JsonConverterHelper.Serialize(mSLField);

			ref2.RC_Code = "CC";
			Factory.Save();

			deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);

			AssertEquals(1, deserialisedField.CollectionProvider.Collection.Count);
			AssertEquals("AAAAAAAAAA", deserialisedField.ValueAsStringForSerialisation);
			AssertEquals("LookupFieldName", deserialisedField.FieldName);
			AssertEquals(true, deserialisedField.IsFilterValueExcluded);
		}

		public void TestJsonConverterWithCodes_WithBlankSpaceAtTheFront()
		{
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode);

			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = " AAAAA";
			provider.Collection.Add(ref1);

			var ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = " BBBBB";
			provider.Collection.Add(ref2);
			Factory.Save();

			var mSLField = new MultipleSelectionLookup(Factory);
			mSLField.SetCollectionProvider(provider);

			mSLField.FieldName = "LookupFieldName";
			AssertEquals(" AAAAA,  BBBBB", mSLField.ValueAsStringForSerialisation);

			var result = JsonConverterHelper.Serialize(mSLField);
			var deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);

			AssertEquals(2, deserialisedField.CollectionProvider.Collection.Count);
			AssertEquals(" AAAAA,  BBBBB", deserialisedField.ValueAsStringForSerialisation);
			AssertEquals("LookupFieldName", deserialisedField.FieldName);
			AssertEquals(false, deserialisedField.IsFilterValueExcluded);

			mSLField.IsFilterValueExcluded = true;

			result = JsonConverterHelper.Serialize(mSLField);
			deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);

			AssertEquals(2, deserialisedField.CollectionProvider.Collection.Count);
			AssertEquals(" AAAAA,  BBBBB", deserialisedField.ValueAsStringForSerialisation);
			AssertEquals("LookupFieldName", deserialisedField.FieldName);
			AssertEquals(true, deserialisedField.IsFilterValueExcluded);
		}

		public void TestJsonConverterWithPKs()
		{
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode);

			var ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "AAAAA";
			provider.Collection.Add(ref1);

			var ref2 = Factory.New<RefContainer>();
			ref2.RC_Code = "BBBBB";
			provider.Collection.Add(ref2);
			Factory.Save();

			var oMSLField = new MultipleSelectionLookup(Factory);
			oMSLField.SerialisedByPK = true;
			var columnInfo = new ColumnInfo("Caption");
			columnInfo.Properties.Add("ColumnName", "RC_Code");
			oMSLField.Columns.Add(columnInfo);

			oMSLField.SetCollectionProvider(provider);

			oMSLField.FieldName = "LookupFieldName";

			var result = JsonConverterHelper.Serialize(oMSLField);
			var deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);

			AssertEquals(2, deserialisedField.CollectionProvider.Collection.Count);
			AssertEquals("Should contain PKs", ref1.PK.ToString() + ", " + ref2.PK.ToString(), deserialisedField.ValueAsStringForSerialisation);
			AssertEquals("LookupFieldName", deserialisedField.FieldName);
			AssertEquals("Should contain 1 column", 1, deserialisedField.Columns.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700503 - Pending migration")]
		public void TestNoNullReferenceExceptionThrown()
		{
			var field = new MultipleSelectionLookup(Factory);
			field.FieldName = "LookupFieldName";

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<MultipleSelectionLookup>(result);
			AssertEquals("LookupFieldName", deserialisedField.FieldName);
			AssertEquals("", deserialisedField.ValueAsStringForSerialisation);
		}

		public void TestNonEmptyWhereClause()
		{
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			OrgHeader org1 = Factory.New<OrgHeader>();
			provider.Collection.Add(org1);
			OrgHeader org2 = Factory.New<OrgHeader>();
			provider.Collection.Add(org2);

			OrgHeaderCollection findboxCollection = new OrgHeaderCollection(Factory);

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);
			lookup.FieldName = "OH_PK"; // value is taken from report

			AssertEquals("Precondition: Lookup.IsEmpty", false, lookup.IsEmpty);

			AssertEquals("Filter clause", "OH_PK IN ('" + org1.PK + "','" + org2.PK + "')", lookup.WhereClause());

			lookup.IsFilterValueExcluded = true;
			AssertEquals("Filter clause", "OH_PK NOT IN ('" + org1.PK + "','" + org2.PK + "')", lookup.WhereClause());

			lookup.IsFilterValueExcluded = false;
			provider.Collection.RemoveFromRelationship(org2);
			AssertEquals("Filter clause", "OH_PK IN ('" + org1.PK + "')", lookup.WhereClause());

			lookup.IsFilterValueExcluded = true;
			AssertEquals("Filter clause", "OH_PK NOT IN ('" + org1.PK + "')", lookup.WhereClause());
		}

		public void TestNonEmptyWhereClause_UseCodesForWhereClause()
		{
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "JNC";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "E'GI";

			provider.Collection.Add(org1);
			provider.Collection.Add(org2);

			var lookup = new MultipleSelectionLookup(Factory);
			lookup.UseCodesForWhereClause = true;
			lookup.SetCollectionProvider(provider);
			lookup.FieldName = "OH_CODE";

			AssertEquals("Precondition: Lookup.IsEmpty", false, lookup.IsEmpty);

			AssertEquals("Filter clause", "OH_CODE IN ('JNC','E''GI')", lookup.WhereClause());

			lookup.IsFilterValueExcluded = true;
			AssertEquals("Filter clause", "OH_CODE NOT IN ('JNC','E''GI')", lookup.WhereClause());

			lookup.IsFilterValueExcluded = false;
			provider.Collection.RemoveFromRelationship(org2);
			AssertEquals("Filter clause", "OH_CODE IN ('JNC')", lookup.WhereClause());

			lookup.IsFilterValueExcluded = true;
			AssertEquals("Filter clause", "OH_CODE NOT IN ('JNC')", lookup.WhereClause());
		}

		public void TestGetValueAsString()
		{
			OrgHeaderCollection findboxCollection = new OrgHeaderCollection(Factory);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			provider.Collection.Add(org1);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";
			provider.Collection.Add(org2);

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			AssertEquals("Value as string", "ABCDEFG, HIJKLMNOP", lookup.ValueAsStringForSerialisation.Trim());
		}

		public void TestIsResponsibleForPKs()
		{
			OrgHeaderCollection findboxCollection = new OrgHeaderCollection(Factory);
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			collectionProvider.Collection.Add(org1);

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";
			collectionProvider.Collection.Add(org2);

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(collectionProvider);
			lookup.DisplayName = "TestLookupField";

			int responsibleCount = 0;
			string expectedPks = "'" + org1.PK.ToString() + "','" + org2.PK.ToString() + "'";
			foreach (ValueProvider provider in lookup.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestLookupField.PKs>", Passes.FirstPass))
				{
					AssertEquals("Should return PKs of objects in collection", expectedPks, provider.GetReplacement("<TestLookupField.PKs>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.PKs>!", 1, responsibleCount);
		}

		public void TestIsResponsibleForPKsAsTVP()
		{
			var findboxCollection = new OrgHeaderCollection(Factory);
			var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			var collection = new OrgHeaderCollection(Factory);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			collectionProvider.Collection.Add(org1);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";
			collectionProvider.Collection.Add(org2);

			var lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(collectionProvider);
			lookup.DisplayName = "TestLookupField";

			int responsibleCount = 0;
			foreach (var provider in lookup.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestLookupField.PKsAsTVP>", Passes.FirstPass))
				{
					var result = provider.GetReplacement("<TestLookupField.PKsAsTVP>", new Report(null, null));
					AssertType<ReplacementWithParameterType>(result);
					AssertEquals("ParameterTypeName", "dbo.TVP_uniqueidentifier", ((ReplacementWithParameterType)result).ParameterTypeName);
					var resultMacroValue = ((ReplacementWithParameterType)result).MacroValue;
					AssertType<DataTable>(resultMacroValue);
					var table = (DataTable)resultMacroValue;
					AssertEquals("Columns count", 1, table.Columns.Count);
					var expectedColumnName = "Value";
					AssertEquals("ColumnName", expectedColumnName, table.Columns[0].ColumnName);
					AssertEquals("Rows count", 2, table.Rows.Count);
					AssertEquals("Row value", org1.PK, table.Rows[0][expectedColumnName]);
					AssertEquals("Row value", org2.PK, table.Rows[1][expectedColumnName]);

					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.PKsAsTVP>!", 1, responsibleCount);
		}

		public void TestIsResponsibleForCodesAsTVP()
		{
			var findboxCollection = new OrgHeaderCollection(Factory);
			var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			var collection = new OrgHeaderCollection(Factory);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			collectionProvider.Collection.Add(org1);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";
			collectionProvider.Collection.Add(org2);

			var lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(collectionProvider);
			lookup.DisplayName = "TestLookupField";

			int responsibleCount = 0;
			foreach (var provider in lookup.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestLookupField.CodesAsTVP>", Passes.FirstPass))
				{
					var result = provider.GetReplacement("<TestLookupField.CodesAsTVP>", new Report(null, null));
					AssertType<ReplacementWithParameterType>(result);
					AssertEquals("ParameterTypeName", TVPHelper.TVP_varchar, ((ReplacementWithParameterType)result).ParameterTypeName);
					var resultMacroValue = ((ReplacementWithParameterType)result).MacroValue;
					AssertType<DataTable>(resultMacroValue);
					var table = (DataTable)resultMacroValue;
					AssertEquals("Columns count", 1, table.Columns.Count);
					var expectedColumnName = "Value";
					AssertEquals("ColumnName", expectedColumnName, table.Columns[0].ColumnName);
					AssertEquals("Rows count", 2, table.Rows.Count);
					AssertEquals("Row value", org1.OH_Code, table.Rows[0][expectedColumnName]);
					AssertEquals("Row value", org2.OH_Code, table.Rows[1][expectedColumnName]);

					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.CodesAsTVP>!", 1, responsibleCount);
		}

		public void TestIsResponsibleForIsEmpty()
		{
			var findboxCollection = new OrgHeaderCollection(Factory);
			var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			var collection = new OrgHeaderCollection(Factory);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			collectionProvider.Collection.Add(org1);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";
			collectionProvider.Collection.Add(org2);

			var lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(collectionProvider);
			lookup.DisplayName = "TestLookupField";

			int responsibleCount = 0;
			foreach (var provider in lookup.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<TestLookupField.IsEmpty>", Passes.FirstPass))
				{
					AssertEquals("GetReplacement", false, provider.GetReplacement("<TestLookupField.IsEmpty>", new Report(null, null)));

					collectionProvider.Collection.Clear();
					AssertEquals("GetReplacement", true, provider.GetReplacement("<TestLookupField.IsEmpty>", new Report(null, null)));

					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <TestLookupField.IsEmpty>!", 1, responsibleCount);
		}

		public void TestGetValueAsObject()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(GetCollectionProviderWithSingleOrganisation());

			AssertEquals("string value should have Org code", "ABCDEFG", lookup.ValueAsStringForSerialisation);
			AssertEquals("Should be same as value as string", lookup.ValueAsStringForSerialisation, lookup.ValueAsObject);
		}

		public void TestSetValueAsString()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "HIJKLMNOP";

			OrgHeaderCollection bindToCollection = new OrgHeaderCollection(Factory);
			OrgHeaderCollection findboxCollection = new OrgHeaderCollection(Factory);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);

			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			lookup.ValueAsStringForSerialisation = "ABCDEFG";
			AssertEquals("Lookup should now have 1 org in the bindtolist", 1, lookup.BindToList.Count);
			Assert("Lookup should contain org1", lookup.BindToList.Contains(org1));
			Assert("Lookup should NOT contain org2", !lookup.BindToList.Contains(org2));

			lookup.ValueAsStringForSerialisation = "ABCDEFG, HIJKLMNOP";
			AssertEquals("Lookup should now have 2 orgs in the bindtolist", 2, lookup.BindToList.Count);
			Assert("Lookup should contain org1", lookup.BindToList.Contains(org1));
			Assert("Lookup should contain org2", lookup.BindToList.Contains(org2));
		}

		public void TestSetValueAsStringWithAdditionalFilter()
		{
			AccChargeCode chr1 = Factory.NewWithValidTestData<AccChargeCode>();
			chr1.AC_GC = Env.CurrentCompany.PK;
			chr1.AC_Code = "TEST1";

			AccChargeCode chr2 = Factory.NewWithValidTestData<AccChargeCode>();
			chr2.AC_GC = Env.CurrentCompany.PK;
			chr2.AC_Code = "TEST2";

			AccChargeCode chr3 = Factory.NewWithValidTestData<AccChargeCode>();
			chr3.AC_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
			chr3.AC_Code = "TEST1";

			Factory.Save();

			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ChargeCode);
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			lookup.ValueAsStringForSerialisation = "TEST1";
			AssertEquals("Lookup should now have 1 charge code in the bindtolist", 1, lookup.BindToList.Count);
			Assert("Lookup should contain Chr1", lookup.BindToList.Contains(chr1));

			lookup.ValueAsStringForSerialisation = "TEST1, TEST2";
			AssertEquals("Lookup should now have 2 charge codes in the bindtolist", 2, lookup.BindToList.Count);
			Assert("Lookup should contain Chr1", lookup.BindToList.Contains(chr1));
			Assert("Lookup should contain Chr2", lookup.BindToList.Contains(chr2));
			Assert("Lookup should NOT contain Chr3 as it belongs to another company", !lookup.BindToList.Contains(chr3));
		}

		public void TestSetValueAsStringWithSpaceInsideCode()
		{
			AccChargeCode chr1 = Factory.NewWithValidTestData<AccChargeCode>();
			chr1.AC_GC = Env.CurrentCompany.PK;
			chr1.AC_Code = "AAA";

			AccChargeCode chr2 = Factory.NewWithValidTestData<AccChargeCode>();
			chr2.AC_GC = Env.CurrentCompany.PK;
			chr2.AC_Code = "BBB";

			AccChargeCode chr3 = Factory.NewWithValidTestData<AccChargeCode>();
			chr3.AC_GC = Env.CurrentCompany.PK;
			chr3.AC_Code = "AAA BBB";

			Factory.Save();

			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.ChargeCode);
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(provider);

			lookup.ValueAsStringForSerialisation = "AAA";
			AssertEquals("Lookup should now have 1 charge code in the bindtolist", 1, lookup.BindToList.Count);
			Assert("Lookup should contain Chr1", lookup.BindToList.Contains(chr1));

			lookup.BindToList.Clear();
			lookup.ValueAsStringForSerialisation = "AAA, BBB";
			AssertEquals("Lookup should now have 2 charge codes in the bindtolist", 2, lookup.BindToList.Count);
			Assert("Lookup should contain Chr1", lookup.BindToList.Contains(chr1));
			Assert("Lookup should contain Chr2", lookup.BindToList.Contains(chr2));

			lookup.BindToList.Clear();
			lookup.ValueAsStringForSerialisation = "AAA BBB";
			AssertEquals("Lookup should now have 1 charge codes in the bindtolist", 1, lookup.BindToList.Count);
			Assert("Lookup should contain Chr3", lookup.BindToList.Contains(chr3));
		}

		public void TestSetBindToAndModuleID()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			AssertNull("BindToList should be null by default", lookup.BindToList);
			AssertNull("BindToFindboxList should be null by default", lookup.BindToFindBoxList);
			AssertNull("ModuleID should be null by default", lookup.ModuleID);

			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
			lookup.SetCollectionProvider(provider);

			AssertEquals("BindToList should now be the orgs passed in", typeof(OrgHeaderCollection), lookup.BindToList.GetType());
			AssertEquals("BindToFindboxList should now be the orgs passed in", typeof(OrgHeaderCollection), lookup.BindToFindBoxList.GetType());
			AssertEquals("ModuleID should now be set", ModuleIDs.Organisation, lookup.ModuleID);
		}

		public void TestIsEmpty()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			OrgHeaderCollection orgs1 = new OrgHeaderCollection(Factory);
			OrgHeaderCollection orgs2 = new OrgHeaderCollection(Factory);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
			lookup.SetCollectionProvider(provider);

			AssertEquals("Lookup should be empty - no count of binded collection", true, lookup.IsEmpty);

			lookup.BindToList.Add(Factory.LoadTop1(typeof(OrgHeader), new ZQuery()));
			AssertEquals("Lookup should not be empty any more", false, lookup.IsEmpty);
		}

		public void TestIsHidden()
		{
			var lookup = new MultipleSelectionLookup(Factory);
			lookup.Style = MultipleSelectionLookup.Styles.Grid;

			AssertEquals(false, lookup.IsHidden);

			lookup.Style = MultipleSelectionLookup.Styles.None;

			AssertEquals(true, lookup.IsHidden);
		}

		public void TestSuggestedUserControlType()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			AssertEquals("Pre-condition", MultipleSelectionLookup.Styles.Grid, lookup.Style);
			AssertEquals("type returned - should be MultipleSelectionLookupUserControl when lookup style is Grid", FilterFieldSuggestedUserControlType.MultipleSelectionLookupUserControl, lookup.SuggestedUserControlType);

			lookup.Style = MultipleSelectionLookup.Styles.None;
			AssertEquals("type returned - should be empty control when lookup style is None", FilterFieldSuggestedUserControlType.None, lookup.SuggestedUserControlType);
		}

		public void TestDefaultCurrentCountryToCountryMultipleSelectionLookup()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
				lookup.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Country));
				AssertEquals("Collection should be empty", 0, lookup.BindToList.Count);
				lookup.DefaultCurrentCountryToCountryMultipleSelectionLookup();
				AssertEquals("Collection should not be empty", 1, lookup.BindToList.Count);
				Assert(!((BusinessObject)lookup.BindToList[0]).HasErrors);
				AssertEquals(GlbCompany.CurrentCompany.Country.PK, ((BusinessObject)lookup.BindToList[0]).PK);
			}
		}

		public override void TestSafeCopyValuesFrom()
		{
			MultipleSelectionLookup source = new MultipleSelectionLookup(Factory);
			source.SetCollectionProvider(GetCollectionProviderWithMultipleOrganisation());
			MultipleSelectionLookup destination = new MultipleSelectionLookup(Factory);
			var provider = new DummyCollectionProvider(Factory);
			destination.SetCollectionProvider(provider);

			((IFilter)destination).SafeCopyValuesFrom(source);
			AssertCollectionEquals(source.BindToList, destination.BindToList);
		}

		void AssertCollectionEquals(IBusinessObjectCollection expectedCollection, IBusinessObjectCollection actualCollection)
		{
			Assert("Precondition: There should be at least one element in the collection.", expectedCollection.Count > 0);
			AssertEquals("collection Count should be same", expectedCollection.Count, actualCollection.Count);
			foreach (BusinessObject element in expectedCollection)
			{
				AssertEquals(true, actualCollection.Contains(element));
			}
		}

		public override void TestClearValues()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(GetCollectionProviderWithSingleOrganisation());
			AssertEquals("Collection should not be empty", 1, lookup.BindToList.Count);
			((IFilter)lookup).ClearValues();
			AssertEquals("Collection should be empty", 0, lookup.BindToList.Count);
		}

		public void TestFillFilterData()
		{
			var query = new ZQuery();
			query.MaximumRows = 2;
			var organisations = Factory.Load<OrgHeader>(query);
			var organisation1 = organisations[0];
			var organisation2 = organisations[1];
			var lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation));
			lookup.SerialisedByPK = false;
			lookup.ValueAsStringForSerialisation = $"{organisation1.OH_Code}, {organisation2.OH_Code}";
			var reportFilterData = new ReportFilterData();
			lookup.FillFilterData(reportFilterData);
			AssertEquals("MultipleSelectionLookupFilterCollection should have one MultipleSelectionLookup", 1, reportFilterData.MultipleSelectionLookupFilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new Guid[] { organisation1.PK.ToGuid(), organisation2.PK.ToGuid() }, reportFilterData.MultipleSelectionLookupFilterCollection[0].SelectedValue);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			MultipleSelectionLookup lookup = new MultipleSelectionLookup(Factory);
			lookup.SetCollectionProvider(GetCollectionProviderWithSingleOrganisation());
			ArrayList results = new ArrayList();
			results.Add(lookup);
			return (FilterFieldWithUTSupport[])results.ToArray(typeof(FilterFieldWithUTSupport));
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get
			{
				return 1;
			}
		}

		#region Implementation

		CollectionProvider GetCollectionProviderWithSingleOrganisation()
		{
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			provider.Collection.Add(org1);
			return provider;
		}

		CollectionProvider GetCollectionProviderWithMultipleOrganisation()
		{
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABCDEFG";
			provider.Collection.Add(org1);
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "UVWXYZ";
			provider.Collection.Add(org2);
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "MNOPQR";
			provider.Collection.Add(org3);
			return provider;
		}
		#endregion
	}
}
